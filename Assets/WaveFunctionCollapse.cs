using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;




public class JoinPath
{
    //Name of path / number ou outputs
    public Dictionary<int, int> path = new Dictionary<int, int>();

    public void AddPath(int _pathId, int availablePath)
    {
        path[_pathId] = (path[_pathId] - 1) + (availablePath - 1);
        
    }
    
    public GridChunck MergePath( GridChunck cell)
    {
        
        foreach (int pathId in cell.path)
        {

            path[pathId] = path[pathId] - 1;
            if (path[pathId] <= 1)
            {
                cell.isLastOutput = true;
            }
            
        }

        cell.path = cell.path.Distinct().ToArray();
        int[] otherPaths = cell.path.Where(l => l != cell.path[0]).ToArray();
        
        foreach (int otherIds in otherPaths)
        {

            cell.path = new int[]{ cell.path[0]};
            path[cell.path[0]] = path[cell.path[0]] + path[otherIds];
            path.Remove(otherIds);
            
        }
        
        
        
        

        return cell;
    }
    
}

public class Options
{
    public List<string> options = new List<string>();

    public const int LEFT = 0;
    public const int TOP = 1;
    public const int RIGHT = 2;
    public const int DOWN = 3;
    
    public Options(List<ChunckData> chuncks)
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
        if (fullOptions.Length > 0 )
        {
            try
            {
                fullOptions?.ToList()?.ForEach(cdo =>
                {
                    string dopt = cdo.Split('-')[Edge];
                    if (!edgeOption.Contains(dopt))
                    {
                        edgeOption.Add(dopt);
                    }
                           
                });
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
          
        }
        else
        {
            
        }
        return edgeOption;
    }

    public string[] GetAllOptions()
    {
        return options.ToArray();
    }

}


[Serializable]

public class GridChunck
{
    public bool collapsed = false;
    public bool instancied = false;
    public int[] path = new []{0};
    public bool isLastOutput = false;
    
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

    public int seed = 1;
    public List<ChunckData> Chuncks = new List<ChunckData>();
    
    
    private ChunckManager _chunckManager;
   private int pathIdentifier = 0;
    private System.Random _random;
    private Options Options;
    private JoinPath JoinPath;
    private GridChunck [] grid ;
    
    
  
    // Start is called before the first frame update
    void Start()
    {
      
        _random = new System.Random(seed);
        _chunckManager = new ChunckManager(Chuncks.ToArray());
        
        Options = new Options(Chuncks);
        JoinPath = new JoinPath();
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
   
        
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (!cell.collapsed)
                {
                    bool isAlone = true;
                    List<int> availablePath = new List<int>();
              
                    
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
                        
                        
                        if (cellLeft.collapsed && cellLeft.instancied)
                        {
                           
                            if (edgeToFollow.Contains(Options.GetCellEdgeOptions(Options.RIGHT, cellLeft.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                foreach (int cp in cellLeft.path)
                                {
                                    availablePath.Add(cp);
                                }
                                
                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();
                    
                    }
                    else if (allPathAvailables && i == 0)
                    {
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.LEFT, cell.optionsAvailable);
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.Split('-')[Options.LEFT].Contains(NotallowedLeft))
                                {
                                    isInside = false;
                                }
                            });

                            return isInside;

                        }).ToArray();
                       
                    }
                    
                    
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
                        
                        
                        if (cellUp.collapsed && cellUp.instancied)
                        {
                            
                            if (edgeToFollow.Contains(Options.GetCellEdgeOptions(Options.TOP, cellUp.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                foreach (int cp in cellUp.path)
                                {
                                    availablePath.Add(cp);
                                }
                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();

                    } else if (allPathAvailables && j == 0)
                    {
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.DOWN, cell.optionsAvailable);
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.Split('-')[Options.DOWN].Contains(NotallowedLeft))
                                {
                                    isInside = false;
                                }
                            });

                            return isInside;

                        }).ToArray();
                       
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

                        if (cellRight.collapsed && cellRight.instancied)
                        {
                            
                            if (edgeToFollow.Contains(Options.GetCellEdgeOptions(Options.LEFT, cellRight.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                foreach (int cp in cellRight.path)
                                {
                                    availablePath.Add(cp);
                                }
                            }
                        }
                        
                        cell.optionsAvailable = finalOptions.ToArray();
                        
                    } else if (allPathAvailables && i < generateSize.x)
                    {
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.RIGHT, cell.optionsAvailable);
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.Split('-')[Options.RIGHT].Contains(NotallowedLeft))
                                {
                                    isInside = false;
                                }
                            });

                            return isInside;

                        }).ToArray();
                       
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

                        
                        
                        if (cellDown.collapsed && cellDown.instancied)
                        {

                       
                            if (edgeToFollow.Contains(Options.GetCellEdgeOptions(Options.DOWN, cellDown.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                foreach (int cp in cellDown.path)
                                {
                                    availablePath.Add(cp);
                                }
                            }
                          
                        }
                        
                        cell.optionsAvailable = finalOptions.ToArray();
                        
                       
                    }else if (allPathAvailables && j < generateSize.y)
                    {
                        List<string> leftOptions = Options.GetCellEdgeOptions(Options.TOP, cell.optionsAvailable);
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.Split('-')[Options.TOP].Contains(NotallowedLeft))
                                {
                                    isInside = false;
                                }
                            });

                            return isInside;

                        }).ToArray();
                       
                    }


                    if (!isAlone && allPathAvailables)
                    {

                         availablePath.Remove(0);

                        if (availablePath.Count > 0)
                        {  
                            cell.path = availablePath.ToArray();
                            
                        }
                      
                        
                    }
                    
                
                  
                } 
            }
        }

        pathIdentifier++;
        GridChunck[] gridCopy = grid.Where(chunck => { return chunck.collapsed == false;}).OrderBy(gridchunk => { return gridchunk.optionsAvailable.Length ; }).ToArray();
        gridCopy = gridCopy.Where(gridChunck => { return gridChunck.optionsAvailable.Length == gridCopy[0].optionsAvailable.Length;
        }).ToArray();

        GridChunck chunckChoosed = gridCopy[_random.Next(0, gridCopy.Length)];
        chunckChoosed.collapsed = true;
        
        
   
        Debug.Log(chunckChoosed);
       
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (cell.collapsed && !cell.instancied)
                {
                    
                    int pathNumber = 0;
                    foreach (string optionEdge in cell.optionsAvailable[0].Split('-'))
                    {
                        if (edgeToFollow.Contains(optionEdge))
                        {
                            pathNumber++;
                        }
                                
                    }
                        
                    //Si la cell n'est pas connecté a une autre via un chemin disponible (edgeToFollow). Elle n'enregistre pas de path a son actif
                    if (cell.path.Length == 1 && cell.path[0] == 0)
                    {
                        //On vient compter le numbre d'arretes de sorties disponibles et on vien créer une valeur pour inscrire un nouveau chemin.
                            
                        //On attribue a la cell un nouvel identifier et on crée un nouveau chemin independant.
                        cell.path = new []{pathIdentifier} ;
                        JoinPath.path.Add(pathIdentifier,pathNumber);
                    }else if (cell.path.Length == 1 && cell.path[0] != 0)
                    {
                        //Si un seul chemin est disponible, on ajoute le chmin au précedant
                        JoinPath.AddPath(cell.path[0],pathNumber);
                    }
                    else if(cell.path.Length > 1)
                    {
                        
                        int[] otherPaths = cell.path.Where(l => l != cell.path[0]).ToArray();
                        
                        //Si plusieurs chemins sont disponible on fusionne les 2
                        cell =  JoinPath.MergePath(cell);
                        foreach (GridChunck gridChunck in grid)
                        {
                            if (gridChunck.path.Any(p =>  otherPaths.Contains(p)))
                            {
                                gridChunck.path = new int[]{ cell.path[0]};
                            }
                        }

                        
                    }


                    if (cell.isLastOutput)
                    {
                        int oldCount = 0;
                        int count = 0;
                        string optionChoosed = chunckChoosed.optionsAvailable[0];
                        for (var optionAvailableIndex = 0; optionAvailableIndex < chunckChoosed.optionsAvailable.Length; optionAvailableIndex++)
                        {
                            string[] optionSplited = chunckChoosed.optionsAvailable[optionAvailableIndex].Split('-');
                            
                            foreach (string edge in edgeToFollow)
                            {
                                count +=  optionSplited.Where(x => x.Equals(edge)).Count();
                            }

                            if (count > oldCount)
                            {
                                oldCount =  count;
                                count = 0;
                                optionChoosed = chunckChoosed.optionsAvailable[optionAvailableIndex];
                            }
                        }
                        
                        chunckChoosed.optionsAvailable = new String[]{optionChoosed};

                    }
                    else
                    {
                        int optionIndex = _random.Next(0, chunckChoosed.optionsAvailable.Length);
                        string option = chunckChoosed.optionsAvailable.Length > 0 ? chunckChoosed.optionsAvailable[optionIndex] : "0";
                        chunckChoosed.optionsAvailable = new String[]{option};

                    }
                    
                    string index = cell.optionsAvailable[0];

                    ChunckData? chunckFound = _chunckManager.GetChunck(index, Options);
                    if (chunckFound != null )
                    {
                        Instantiate(chunckFound.assetsToInstanciate,
                            new Vector3(i * chunckGridSize.x, j * chunckGridSize.y, 0), Quaternion.identity);
                        cell.instancied = true;
                        
                    }
                      
                } 
            }
        }

        Draw();
        }
        
    }
    
    
}
