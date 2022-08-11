using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;


public static class Options
{
    public const int BLANK = 0;
    public const int LEFT = 1;
    public const int TOP = 2;
    public const int RIGHT = 3;
    public const int DOWN = 4;
   
    //Need implement data structure and rules

    public static List<int> GetAllOptions()
    {
        return new List<int>() { BLANK,LEFT, TOP, RIGHT, DOWN };
    }
}

[System.Serializable]
public struct Edge
{
    public string patern;
}
[System.Serializable]
public struct SquareEdge
{
    public Edge left;
    public Edge top;
    public Edge right;
    public Edge down;
}


[System.Serializable]
public class Chunck
{
  
    public GameObject gridToInstanciate;

    [Header("Rules")]
    public SquareEdge Edges;
    public bool left;
    public bool top;
    public bool right;
    public bool down;

    [CanBeNull]
    public Chunck GetChunck(int index)
    {
        Chunck myChunck = null;
        switch (index)
        {
            case Options.BLANK :
                break;
            
            case Options.LEFT:
                if (left)
                {
                    myChunck =this;
                } 
                break;
            
            case Options.TOP:
                if (top)
                {
                    myChunck =this;
                } 
                break;
            
            case Options.RIGHT:
                if (right)
                {
                    myChunck =this;
                } 
                break;
            
            case Options.DOWN:
                if (down)
                {
                    myChunck =this;
                } 
                break;
                
        }

        return myChunck;
    }
    
}

public class GridChunck
{
    public bool collapsed = false;
    public List<int> optionsAvailable = Options.GetAllOptions();
}


public class WaveFunctionCollapse : MonoBehaviour
{

    public Vector2Int generateSize = new Vector2Int(2,2);
    public Vector2Int chunckGridSize = new Vector2Int(8, 8);
    public List<Chunck> Chuncks = new List<Chunck>();

    
    private GridChunck [] grid ;
    
    // Start is called before the first frame update
    void Start()
    {
        grid = new GridChunck[generateSize.x * generateSize.y];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridChunck();
        }

        Draw();
    }


    public void Draw()
    {
   
   

       GridChunck[] gridCopy = grid.OrderBy(gridchunk => { return gridchunk.optionsAvailable.Count; }).ToArray();
       gridCopy = gridCopy.Where(gridChunck => { return gridChunck.optionsAvailable.Count == gridCopy[0].optionsAvailable.Count; }).ToArray();

       GridChunck chunckChoosed = gridCopy[UnityEngine.Random.Range(0, gridCopy.Length)];
       chunckChoosed.collapsed = true;
       int optionIndex = UnityEngine.Random.Range(0, chunckChoosed.optionsAvailable.Count);
       int option = chunckChoosed.optionsAvailable[optionIndex];
       chunckChoosed.optionsAvailable = new List<int>() { option };

       Debug.Log(chunckChoosed);
       
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (cell.collapsed) {
                    int index = cell.optionsAvailable[0];
                    if (index != 0)
                    { 
                        Chunck? chunckFound = Chuncks.FirstOrDefault(c => c.GetChunck(index) != null);
                        Instantiate(chunckFound.gridToInstanciate,
                            new Vector3(i * chunckGridSize.x, j * chunckGridSize.y, 0), Quaternion.identity);

                        
                        
                    }
                    
                  
                    
                } 
            }
        }
    }
    
    
}
