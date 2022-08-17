
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class WFCWindow : EditorWindow
{
    
    
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

     
    }
    
    
    
}
