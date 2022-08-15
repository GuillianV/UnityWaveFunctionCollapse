
using UnityEditor;
using UnityEngine;

public class WFCWindow : EditorWindow
{
    
    
    [MenuItem("Window/WFC")]
    public static void ShowWindow()
    {
        GetWindow<WFCWindow>("WFC Window");
    }
    
    
    
    
    
}
