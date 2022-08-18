
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class WFCWindow : EditorWindow
{
    public DropdownField _bakingMode;
    
    public TextField _prefabName;
    public Button _bake;
    public ProgressBar _progressBar;
    private bool isBaking = false;

    private Queue<Action>   _queueMainThread = new Queue<Action>();
    private int progressValue = 0;
    private WFCCore core;

    public ObjectField _wfcData;
    public WFCData wfcData;
    
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
        _bakingMode.choices.Add("Tile Top-Down");
       
        _wfcData= root.Q<ObjectField>("wfc_data");
       
        
        _prefabName = root.Q<TextField>("wfc_prefabGeneratedName");
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
            
            if (_progressBar.value >= _progressBar.highValue)
            {
                isBaking = false;
                _progressBar.value = _progressBar.lowValue;
            }
            
            
        }
        
    }

    private void Bake()
    {
        if (wfcData != null && !isBaking && _progressBar!= null)
        {
            isBaking = true;
            _progressBar.highValue = (wfcData.gridSize.x * wfcData.gridSize.y) -1 ;

            GameObject map = new GameObject("Map");
            isBaking = true;
       
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

}
