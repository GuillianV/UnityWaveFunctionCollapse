using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class WaveFunctionCollapse : MonoBehaviour
{
 
   

    public WFCData wfcData;
    
    private int iter = 0;
    private ChunckManager _chunckManager;
    private int pathIdentifier = 0;
    private System.Random _random;
    private JoinPath JoinPath;
    private GridChunck [] grid ;
    
    
  
    // Start is called before the first frame update
    void Start()
    {

        if (!wfcData)
        {
            Debug.LogError("Missing WFCData. Try to create it. Right click on assets, create/WFC/WFCData");
            return;
        }
        
        _random = new System.Random(wfcData.seed);
        _chunckManager = new ChunckManager(wfcData.Chuncks.ToArray());
        JoinPath = new JoinPath();
        
        grid = new GridChunck[wfcData.gridSize.x * wfcData.gridSize.y];
        Debug.Log(grid.Length);
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridChunck(_chunckManager.options.ToArray(), i);
        }

        for (int j = 0; j < wfcData.gridSize.y; j++)
        {
            for (int i = 0; i < wfcData.gridSize.x; i++)
            {
                GridChunck cell = grid[i + j * wfcData.gridSize.x];
                cell.Xpos = i;
                cell.Ypos = j;
            }
        }

        Draw();
       
      //  Debug.Log(JoinPath);
        
    }
    
    
      Stopwatch stopwatch = new Stopwatch();
      private int stiter = 0;
    private int maxIter = 100;
    private long ticks = 0;
 
    
    
    

    public void Draw()
    {
   
        stopwatch.Start();
        
        
        if (iter < wfcData.gridSize.x*wfcData.gridSize.y )
        {
            iter++;
   
        
        for (int j = 0; j < wfcData.gridSize.y; j++) {
            for (int i = 0; i < wfcData.gridSize.x; i++) {
                
                
                GridChunck cell = grid[i + j *  wfcData.gridSize.x];
                if (!cell.collapsed)
                {
                    bool isAlone = true;
                    List<int> availablePath = new List<int>();
              
                    
                    //LEFT
                    if (i > 0)
                    {
                        GridChunck cellLeft = grid[i - 1 + j  * wfcData.gridSize.x];
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
                           
                            if (wfcData.edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.RIGHT, cellLeft.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                availablePath.AddRange( cellLeft.path);
                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();
                    
                    }
                    else if (wfcData.enableWalls && i == 0)
                    {
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.LEFT, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => wfcData.edgeToFollow.Any(t => t == l)).ToList();

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
                        GridChunck cellUp = grid[i + (j - 1) * wfcData.gridSize.x];
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
                           
                            if (wfcData.edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.TOP, cellUp.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                availablePath.AddRange( cellUp.path);
                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();

                    } else if (wfcData.enableWalls && j == 0)
                    {
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.DOWN, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => wfcData.edgeToFollow.Any(t => t == l)).ToList();

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
                    if (i < wfcData.gridSize.x -1 )
                    {
                        GridChunck cellRight = grid[i + 1 + j  * wfcData.gridSize.x];
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
                           
                            if (wfcData.edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.LEFT, cellRight.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                availablePath.AddRange( cellRight.path);
                             
                                
                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();
                        
                    } else if (wfcData.enableWalls && i < wfcData.gridSize.x)
                    {
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.RIGHT, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => wfcData.edgeToFollow.Any(t => t == l)).ToList();

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
                    if (j < wfcData.gridSize.y -1 )
                    {
                        GridChunck cellDown = grid[ i + (j + 1) * wfcData.gridSize.x];
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
                           
                            if (wfcData.edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(ChunckManager.DOWN, cellDown.optionsAvailable)[0]))
                            {
                                isAlone = false;
                                availablePath.AddRange( cellDown.path);

                            }
                        }

                        cell.optionsAvailable = finalOptions.ToArray();
                        
                       
                    }else if (wfcData.enableWalls && j < wfcData.gridSize.y)
                    {
                        List<string> leftOptions = _chunckManager.GetNextGridEdgeOptions(ChunckManager.TOP, cell.optionsAvailable).ToList();
                        List<string> NotallowedLeftOption = leftOptions.Where(l => wfcData.edgeToFollow.Any(t => t == l)).ToList();

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


                    if (!isAlone && wfcData.allChunckLinked)
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

       
                if (chunckChoosed.collapsed && !chunckChoosed.instancied)
                {
                    int pathNumber = 0;
                    if (chunckChoosed.optionsAvailable.Length == 0)
                    {
                        Debug.LogError("Missing chunck to match");
                        
                    }
                    else
                    {
                        
                        foreach (string optionEdge in chunckChoosed.optionsAvailable[0].optionEdges)
                        {
                            if (wfcData.edgeToFollow.Contains(optionEdge))
                            {
                                pathNumber++;
                            }
                                    
                        }
                            
                        //Si la cell n'est pas connecté a une autre via un chemin disponible (edgeToFollow). Elle n'enregistre pas de path a son actif
                        if (chunckChoosed.path.Length == 1 && chunckChoosed.path[0] == 0)
                        {
                            //On vient compter le numbre d'arretes de sorties disponibles et on vien créer une valeur pour inscrire un nouveau chemin.
                                
                            //On attribue a la cell un nouvel identifier et on crée un nouveau chemin independant.
                            chunckChoosed.path = new []{pathIdentifier} ;
                            JoinPath.path.Add(pathIdentifier,pathNumber);
                        }else if (chunckChoosed.path.Length == 1 && chunckChoosed.path[0] != 0)
                        {
                            //Si un seul chemin est disponible, on ajoute le chmin au précedant
                            JoinPath.AddPath(chunckChoosed.path[0],pathNumber);
                        }
                        else if(chunckChoosed.path.Length > 1)
                        {
                            
                            int[] otherPaths = chunckChoosed.path.Where(l => l != chunckChoosed.path[0]).ToArray();
                            
                            //Si plusieurs chemins sont disponible on fusionne les 2
                            chunckChoosed =  JoinPath.MergePath(chunckChoosed);
                            foreach (GridChunck gridChunck in grid)
                            {
                                if (gridChunck.path.Any(p =>  otherPaths.Contains(p)))
                                {
                                    gridChunck.path = new int[]{ chunckChoosed.path[0]};
                                }
                            }

                            
                        }


                        if (chunckChoosed.isLastOutput && wfcData.allChunckLinked)
                        {
                            int oldCount = 0;
                            int count = 0;
                            Option optionChoosed = chunckChoosed.optionsAvailable[0];
                            for (var optionAvailableIndex = 0; optionAvailableIndex < chunckChoosed.optionsAvailable.Length; optionAvailableIndex++)
                            {
                                string[] optionSplited = chunckChoosed.optionsAvailable[optionAvailableIndex].optionEdges;
                                
                                foreach (string edge in wfcData.edgeToFollow)
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
                        
                        string index = chunckChoosed.optionsAvailable[0].optionValue;

                        ChunckData? chunckFound = _chunckManager.GetChunck(index);
                        if (chunckFound != null )
                        {
                            Instantiate(chunckFound.assetsToInstanciate,
                                new Vector3(chunckChoosed.Xpos * wfcData.chuncksSize.x, chunckChoosed.Ypos * wfcData.chuncksSize.y, 0), Quaternion.identity);
                            chunckChoosed.instancied = true;
                            
                        }
                    }
                } 
    

        stiter++;
        stopwatch.Stop();
        if (stiter >= maxIter)
        {
            
            //5*5 150000 t
            
            // 18 000 t
            
            long t = stopwatch.ElapsedTicks / stiter;
            long tiime = stopwatch.ElapsedMilliseconds / maxIter;
            Debug.Log(tiime);
            stopwatch.Reset();
            stiter = 0;
           // return;
        }
        else
        {
            ticks = ticks + stopwatch.ElapsedTicks;
        }
        
       
      
    
        
        Draw();
        }
        
    }
    
    
}
