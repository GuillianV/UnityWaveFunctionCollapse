using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class Options
{
    public List<string> options = new List<string>();

    public const int LEFT = 0;
    public const int TOP = 1;
    public const int RIGHT = 2;
    public const int DOWN = 3;
    
    public Options(List<Chunck> chuncks)
    {
     
        chuncks?.ForEach(chunck =>
        {
            string optionValue = string.Format("{0}-{1}-{2}-{3}", chunck.Edges.left.patern, chunck.Edges.top.patern,
                chunck.Edges.right.patern, chunck.Edges.down.patern);

            if (!options.Contains(optionValue))
            {
                options.Add(optionValue);
            }
        });
        
        
    }

    public List<string> GetCellEdgeOptions(int Edge, string[] fullOptions)
    {
        List<string> edgeOption = new List<string>();
        fullOptions.ToList()?.ForEach(cdo =>
        {
            string dopt = cdo.Split('-')[Edge];
            if (!edgeOption.Contains(dopt))
            {
                edgeOption.Add(dopt);
            }
                           
        });
        return edgeOption;
    }

    public string[] GetAllOptions()
    {
        return options.ToArray();
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
  
    public GameObject gridToInstanciate ;

    [Header("Rules")]
    public SquareEdge Edges;

    [CanBeNull]
    public Chunck GetChunck(string index, Options options)
    {
       string valuePair = options.options.FirstOrDefault(kpo => {return kpo == index;});
       string[] edges = valuePair.Split('-');

       Chunck chunckFound = null;
       if (edges.Length == 4)
       {
           if (edges[Options.LEFT] == Edges.left.patern && edges[Options.TOP] == Edges.top.patern && edges[Options.RIGHT] == Edges.right.patern  && edges[Options.DOWN] == Edges.down.patern  )
           {
               chunckFound = this;
           }
           
       }
       return chunckFound;

    }
    
}

public class GridChunck
{
    public bool collapsed = false;
    public bool instancied = false;
    public string[] optionsAvailable;

    public GridChunck(string[] _optionsAvailable)
    {
        optionsAvailable = _optionsAvailable;
    }
    
}


#if UNITY_EDITOR
[CustomEditor(typeof(WaveFunctionCollapse))]
public class RandomScript_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields
 
        WaveFunctionCollapse script = (WaveFunctionCollapse)target;
 
        // draw checkbox for the bool
        script.allPathAvailables = EditorGUILayout.Toggle("Enable Path", script.allPathAvailables);
        if (script.allPathAvailables) // if bool is true, show other fields
        {
        
            var list = serializedObject.FindProperty("edgeToFollow");
            EditorGUILayout.PropertyField(list, new GUIContent("Edges To Follow"), true);
        
        }
    }
}
#endif

public class WaveFunctionCollapse : MonoBehaviour
{
    private int iter = 0;

    [Header("Génération options")]
    public Vector2Int generateSize = new Vector2Int(2,2);
    public Vector2Int chunckGridSize = new Vector2Int(8, 8);
   
  
    [HideInInspector]
    [Tooltip("When the map is genérating, the algorith will try to avoid chunck who cant be accesed")]
    public bool allPathAvailables = true;

    [HideInInspector]
    public List<string> edgeToFollow;

    public List<Chunck> Chuncks = new List<Chunck>();

    
    private Options Options;
    
    private GridChunck [] grid ;
    
    // Start is called before the first frame update
    void Start()
    {
        Options = new Options(Chuncks);
        grid = new GridChunck[generateSize.x * generateSize.y];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridChunck(Options.GetAllOptions());
        }

        Draw();
      
    }


    public void Draw()
    {
   
      
        if (iter < generateSize.x*generateSize.y )
        {
            iter++;
   
    
       GridChunck[] gridCopy = grid.Where(chunck => { return chunck.collapsed == false;}).OrderBy(gridchunk => { return gridchunk.optionsAvailable.Length ; }).ToArray();
       gridCopy = gridCopy.Where(gridChunck => { return gridChunck.optionsAvailable.Length == gridCopy[0].optionsAvailable.Length;
       }).ToArray();

       GridChunck chunckChoosed = gridCopy[UnityEngine.Random.Range(0, gridCopy.Length)];
       chunckChoosed.collapsed = true;
       int optionIndex = UnityEngine.Random.Range(0, chunckChoosed.optionsAvailable.Length);
       string option = chunckChoosed.optionsAvailable[optionIndex];
       chunckChoosed.optionsAvailable = new String[]{option};

       Debug.Log(chunckChoosed);
       
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (cell.collapsed && !cell.instancied) {
                    string index = cell.optionsAvailable[0];
                   
                        Chunck? chunckFound = Chuncks.FirstOrDefault(c => c.GetChunck(index, Options) != null);
                        if (chunckFound != null )
                        {
                            Instantiate(chunckFound.gridToInstanciate,
                                new Vector3(i * chunckGridSize.x, j * chunckGridSize.y, 0), Quaternion.identity);
                            cell.instancied = true;
                        }
                      
                } 
            }
        }
        
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (!cell.collapsed)
                {
                    
                    //LEFT
                    if (i > 0)
                    {
                        GridChunck cellLeft = grid[i - 1 + j  * generateSize.x];
                        List<string> rightOptions = Options.GetCellEdgeOptions(Options.RIGHT, cellLeft.optionsAvailable);
                        List<string> finalOptions = new List<string>();
                        rightOptions?.ForEach(upo =>
                        {
                            List<string> flist = cell.optionsAvailable.Where(oa =>
                            {
                                return oa.Split('-')[Options.LEFT] == upo;
                            }).ToList();
                            finalOptions.AddRange(flist);
                        });

                        cell.optionsAvailable = finalOptions.ToArray();
                    
                    }/*
                    else if (allPathAvailables && i == 0)
                    {
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.LEFT, cell.optionsAvailable);
                        List<string> finalOptions = new List<string>();

                        //Edge X
                        //LeftOpt 0X
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();
                        

                        
                        cell.optionsAvailable = finalOptions.ToArray();
                    }
                    */
                    
                    //UP
                    if (j > 0)
                    {
                        GridChunck cellUp = grid[i + (j - 1) * generateSize.y];
                        List<string> downOptions = Options.GetCellEdgeOptions(Options.TOP, cellUp.optionsAvailable);
                        List<string> finalOptions = new List<string>();
                        downOptions?.ForEach(upo =>
                        {
                            List<string> flist = cell.optionsAvailable.Where(oa =>
                            {
                                return oa.Split('-')[Options.DOWN] == upo;
                            }).ToList();
                            finalOptions.AddRange(flist);
                        });

                        cell.optionsAvailable = finalOptions.ToArray();

                    }
                    //RIGHT
                    if (i < generateSize.x -1 )
                    {
                        GridChunck cellRight = grid[i + 1 + j  * generateSize.x];
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.LEFT, cellRight.optionsAvailable);
                        List<string> finalOptions = new List<string>();
                        leftOptions?.ForEach(upo =>
                        {
                            List<string> flist = cell.optionsAvailable.Where(oa =>
                            {
                                return oa.Split('-')[Options.RIGHT] == upo;
                            }).ToList();
                            finalOptions.AddRange(flist);
                        });

                        cell.optionsAvailable = finalOptions.ToArray();
                        
                    }
                    //DOWN
                    if (j < generateSize.y -1 )
                    {
                        GridChunck cellDown = grid[i + (j + 1) * generateSize.y];
                        List<string> upOptions = Options.GetCellEdgeOptions(Options.DOWN, cellDown.optionsAvailable);
                        List<string> finalOptions = new List<string>();
                        upOptions?.ForEach(upo =>
                        {
                            List<string> flist = cell.optionsAvailable.Where(oa =>
                            {
                                return oa.Split('-')[Options.TOP] == upo;
                            }).ToList();
                            finalOptions.AddRange(flist);
                        });

                        cell.optionsAvailable = finalOptions.ToArray();
                        
                       
                    }
                    
                  
                } 
            }
        }

       
        Draw();
        }
        
    }
    
    
}
