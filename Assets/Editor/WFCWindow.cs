
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class WFCWindow : EditorWindow
{
    public DropdownField _bakingMode;
    
    public TextField _prefabName;
    public TextField _prefabFolder;
    public Button _bake;
    public ProgressBar _progressBar;
    private bool isBaking = false;

    private Queue<Action>   _queueMainThread = new Queue<Action>();
    private int progressValue = 0;
    private int instanciatingValue = 0;
    private WFCCore core;

    public ObjectField _wfcData;
    public WFCData wfcData;

    private GameObject map;
    
    [MenuItem("Window/WFC")]
    public static void ShowWindow()
    {
        var window = GetWindow<WFCWindow>("WFC Window");
        
        window.minSize = new Vector2(600, 700);
        window.position = new Rect(new Vector2(0, 0), window.minSize);
    }
    
    private void CreateGUI()
    {
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("MainWFC");
        quickToolVisualTree.CloneTree(root);


        _bakingMode = root.Q<DropdownField>("wfc_dropdown");
        _bakingMode.choices.Clear();
        _bakingMode.choices.Add("Square Chunck");
        _bakingMode.index = 0;
        _wfcData= root.Q<ObjectField>("wfc_data");
        string wfcdataPath =  PlayerPrefs.GetString("WFCDATA");
        if (!string.IsNullOrEmpty(wfcdataPath))
        {
            wfcData = (WFCData)AssetDatabase.LoadAssetAtPath(wfcdataPath, typeof(WFCData));
            _wfcData.value = wfcData;
        }
      
        
        _prefabName = root.Q<TextField>("wfc_prefabGeneratedName");
        _prefabFolder = root.Q<TextField>("wfc_prefabGeneratedFolder");
        _bake = root.Q<Button>("wfc_bake");
        _bake.clicked += Bake;
        
        
        _progressBar = root.Q<ProgressBar>("wfc_progressBar");
        
        core = new WFCCore();
        core.OnEndDraw += DrawHandler;
    }
    
    
    

    //Loop
    private void OnGUI()
    {

        if (_wfcData.value != null)
        {
            wfcData = (WFCData) _wfcData.value;
            
            
        }
        
       
    }

    

    private void Update()
    {
        while (_queueMainThread.Count > 0)
        {
            
            _queueMainThread?.Dequeue()?.Invoke();
        }
        
        if (_progressBar != null )
        {
            _progressBar.value = progressValue;

            _progressBar.title = CalculateLoadingBar().ToString();
            
            if (_progressBar.value >= _progressBar.highValue)
            {
                isBaking = false;
                progressValue = 0;
                _progressBar.value = _progressBar.lowValue;
                
            }
            
            
        }

        if (instanciatingValue >= _progressBar.highValue)
        {
                instanciatingValue = 0;
            
                if (!Directory.Exists("Assets/"+_prefabFolder.text.Replace("/",String.Empty)))
                    AssetDatabase.CreateFolder("Assets", _prefabFolder.text);
                string localPath = "Assets/"+_prefabFolder.text.Replace("/",String.Empty)+ "/" + _prefabName.text.Replace(".prefab",String.Empty) + ".prefab";

                // Make sure the file name is unique, in case an existing Prefab has the same name.
                localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

                // Create the new Prefab and log whether Prefab was saved successfully.
                bool prefabSuccess;
                PrefabUtility.SaveAsPrefabAsset(map, localPath, out prefabSuccess);
                if (prefabSuccess == true)
                    Debug.Log("Prefab was saved successfully");
                else
                    Debug.Log("Prefab failed to save" + prefabSuccess);
            
            
        }
        
    }

    private void Bake()
    {
        if (wfcData != null && !isBaking && _progressBar!= null)
        {
            isBaking = true;
            int highValue = (wfcData.gridSize.x * wfcData.gridSize.y) - 1;
            _progressBar.highValue = highValue;
            
            map = new GameObject("Map");

           string path = AssetDatabase.GetAssetPath(wfcData);
           PlayerPrefs.SetString("WFCDATA",path);
   
           
           
            core.UpdateData(wfcData, _bakingMode.value);

            new Thread(() => 
            {
            
                core.Start();
                while (core._gridChuncksQueue.Count > 0)
                {
                    _queueMainThread?.Enqueue(()=>{
 
                        if (core._gridChuncksQueue.Count > 0)
                        {
                        
                            _progressBar.value = _progressBar.value +=1 ;
                            KeyValuePair<GridChunck, ChunckData> chunckChoosed = core._gridChuncksQueue.Dequeue();
                            Instantiate(chunckChoosed.Value.assetsToInstanciate,new Vector3(chunckChoosed.Key.Xpos * wfcData.chuncksSize.x, chunckChoosed.Key.Ypos * wfcData.chuncksSize.y, 0), Quaternion.identity, map.transform);
                            instanciatingValue += 1;
                        }
                    
                   
                    });
                
               
                }

            }).Start();

            
        }

       
        
    }

    public void DrawHandler(object sender, EventArgs eventArgs)
    {
        progressValue += 1;
    }

    public float CalculateLoadingBar()
    {
        return _progressBar.value * 100 / _progressBar.highValue;
    }

}
