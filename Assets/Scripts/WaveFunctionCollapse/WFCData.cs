using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "WFC/WFCData")]
public class WFCData : ScriptableObject
{
   
    [Header("Génération options")]
    
    [Tooltip("The size of the generated grid")]
    public Vector2Int gridSize = new Vector2Int(2,2);
    [Tooltip("The size of the chunck generated. The size depend of your prefab/grid size")]
    public Vector2Int chuncksSize = new Vector2Int(8, 8);

    [Tooltip("When the map is generating, the algorithm will try to avoid chunck who cant be accesed. It depend on the available chuncks and their edges")]
    public bool allChunckLinked = true;
   
    [Tooltip("The chuncks next to grid size will try to fit and avoid path to void")]
    public bool enableWalls = true;
    
    [Tooltip("Values of colliding edges. precise the same value of chunck edge who need to be collided ")]
    public List<string> edgeToFollow;
    
    [Tooltip("Chunck who will be instancied by the algorithm")]
    public List<ChunckData> Chuncks = new List<ChunckData>();

    [Tooltip("The generation seed. Change the seed to change the generation process")]
    public int seed = 1;
    
}
