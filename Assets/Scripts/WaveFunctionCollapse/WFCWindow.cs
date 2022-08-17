
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class WFCWindow : EditorWindow
{
    public DropdownField _bakingMode;
    public ObjectField _wfcData;
    public TextField _prefabName;
    public Button _bake;
    public ProgressBar _progressBar;

    public WFCData wfcData;
    private bool isBaking = false;
    
    
    [MenuItem("Window/WFC")]
    public static void ShowWindow()
    {
        var window = GetWindow<WFCWindow>("WFC Window");
        
        window.minSize = new Vector2(600, 700);
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

        wfcData =(WFCData) _wfcData.value;
        
        _prefabName = root.Q<TextField>("wfc_prefabGeneratedName");
        _bake = root.Q<Button>("wfc_bake");
        _bake.clicked += Bake;
        
        
        _progressBar = root.Q<ProgressBar>("wfc_progressBar");

    }

    //Loop
    private void OnGUI()
    {
        if (_bakingMode != null)
        {
            Debug.Log(_bakingMode.value);
        }

        if (_progressBar != null && isBaking)
        {
            if (_progressBar.value < _progressBar.highValue)
            {
                _progressBar.value += 0.1f;
            }
            else
            {
                isBaking = false;
                _progressBar.value = _progressBar.lowValue;
            }
            
            
        }
        
        
    }

    private void Bake()
    {
        isBaking = true;
    }
    
    

}
