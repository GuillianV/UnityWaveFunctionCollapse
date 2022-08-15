using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;








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
    private JoinPath JoinPath;
    private GridChunck [] grid ;
    
    
  
    // Start is called before the first frame update
    void Start()
    {
      
        _random = new System.Random(seed);
        _chunckManager = new ChunckManager(Chuncks.ToArray());
        JoinPath = new JoinPath();
        
        grid = new GridChunck[generateSize.x * generateSize.y];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridChunck(_chunckManager.options.ToArray());
        }

        Draw();
       
      //  Debug.Log(JoinPath);
        
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
                        List<string> rightOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.RIGHT, cellLeft.optionsAvailable).ToList();
                        List<Option> finalOptions = new List<Option>();
                        rightOptions?.ForEach(rightOption =>
                        {
                            foreach (Option option in cell.optionsAvailable)
                            {
                                if (option.optionEdges[ChunckManager.LEFT] == rightOption)
                                {
                                    finalOptions.Add(option);
                                }
                            }
                        });
                        
                        
                        if (cellLeft.collapsed && cellLeft.instancied)
                        {
                           
                            if (edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.RIGHT, cellLeft.optionsAvailable)[0]))
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
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.LEFT, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.optionEdges[ChunckManager.LEFT].Contains(NotallowedLeft))
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
                        List<string> downOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.TOP, cellUp.optionsAvailable).ToList();
                        List<Option> finalOptions = new List<Option>();
                        downOptions?.ForEach(downOption =>
                        {
                            foreach (Option option in cell.optionsAvailable)
                            {
                                if (option.optionEdges[ChunckManager.DOWN] == downOption)
                                {
                                    finalOptions.Add(option);
                                }
                            }
                        });
                        
                        
                        if (cellUp.collapsed && cellUp.instancied)
                        {
                           
                            if (edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.TOP, cellUp.optionsAvailable)[0]))
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
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.DOWN, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.optionEdges[ChunckManager.DOWN].Contains(NotallowedLeft))
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
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.LEFT, cellRight.optionsAvailable).ToList();
                        List<Option> finalOptions = new List<Option>();
                        leftOptions?.ForEach(leftOption =>
                        {
                            foreach (Option option in cell.optionsAvailable)
                            {
                                if (option.optionEdges[ChunckManager.RIGHT] == leftOption)
                                {
                                    finalOptions.Add(option);
                                }
                            }
                        });
                        
                        
                        if (cellRight.collapsed && cellRight.instancied)
                        {
                           
                            if (edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.LEFT, cellRight.optionsAvailable)[0]))
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
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.RIGHT, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.optionEdges[ChunckManager.RIGHT].Contains(NotallowedLeft))
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
                        List<string> upOptions= _chunckManager.GetNextGridEdgeOptions(ChunckManager.DOWN, cellDown.optionsAvailable).ToList();
                        List<Option> finalOptions = new List<Option>();
                        upOptions?.ForEach(leftOption =>
                        {
                            foreach (Option option in cell.optionsAvailable)
                            {
                                if (option.optionEdges[ChunckManager.TOP] == leftOption)
                                {
                                    finalOptions.Add(option);
                                }
                            }
                        });
                        
                        
                        if (cellDown.collapsed && cellDown.instancied)
                        {
                           
                            if (edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.DOWN, cellDown.optionsAvailable)[0]))
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
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.TOP, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => edgeToFollow.Any(t => t == l)).ToList();

                        cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                        {
                            bool isInside = true;
                            NotallowedLeftOption?.ForEach(NotallowedLeft =>
                            {
                                if (optionFull.optionEdges[ChunckManager.TOP].Contains(NotallowedLeft))
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
                    foreach (string optionEdge in cell.optionsAvailable[0].optionEdges)
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
                        Option optionChoosed = chunckChoosed.optionsAvailable[0];
                        for (var optionAvailableIndex = 0; optionAvailableIndex < chunckChoosed.optionsAvailable.Length; optionAvailableIndex++)
                        {
                            string[] optionSplited = chunckChoosed.optionsAvailable[optionAvailableIndex].optionEdges;
                            
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
                        
                        chunckChoosed.optionsAvailable = new Option[]{optionChoosed};

                    }
                    else
                    {
                        int optionIndex = _random.Next(0, chunckChoosed.optionsAvailable.Length);

                        Option option = new Option();
                        if (chunckChoosed.optionsAvailable.Length > 0)
                        {
                            option = chunckChoosed.optionsAvailable[optionIndex];
                        }
                        
                        chunckChoosed.optionsAvailable = new Option[]{option};

                    }
                    
                    string index = cell.optionsAvailable[0].optionValue;

                    ChunckData? chunckFound = _chunckManager.GetChunck(index);
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
