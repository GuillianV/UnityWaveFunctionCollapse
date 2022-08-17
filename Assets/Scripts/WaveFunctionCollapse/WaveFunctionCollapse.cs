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

    private List<Option> MissingOptions = new List<Option>();

    private Queue<KeyValuePair<GridChunck,ChunckData>> _gridChuncksQueue = new Queue<KeyValuePair<GridChunck,ChunckData>>();

    Stopwatch stopwatch = new Stopwatch();
    private int stiter = 0;
    private long ticks = 0;
    
    
    
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

        int k = 0;
        for (int j = 0; j < wfcData.gridSize.y; j++)
        {
            for (int i = 0; i < wfcData.gridSize.x; i++)
            {
                GridChunck cell = new GridChunck(_chunckManager.options.ToArray(), k);
               
                cell.Xpos = i;
                cell.Ypos = j;
                grid[k] = cell;
                k++;
            }
        }

        
          
   
        
        foreach (var gridChunck in grid)
        {
            
            stopwatch.Start();


            Draw();
            
            stiter++;
            stopwatch.Stop();
            if (stiter >= wfcData.gridSize.x * wfcData.gridSize.y )
            {
            
                //5*5 150000 t
            
                // 18 000 t
            
                long t = stopwatch.ElapsedTicks / stiter;
                long tiime = stopwatch.ElapsedMilliseconds / ( wfcData.gridSize.x * wfcData.gridSize.y);
                Debug.Log(tiime);
                stopwatch.Reset();
                stiter = 0;
                // return;
            }
            else
            {
                ticks = ticks + stopwatch.ElapsedTicks;
            }

        }
        
        
    }
    



    public void CheckOptions(GridChunck _gridChunck, bool isFirstTime)
    {
          
                    GridChunck cell = grid[_gridChunck.Xpos + _gridChunck.Ypos *  wfcData.gridSize.x];

        
                    if (!cell.collapsed)
                    {
                        bool isAlone = true;
                        List<int> availablePath = new List<int>();

                      
                        
                        List<GridChunckOption> gridChunckOptions = new List<GridChunckOption>();


                       gridChunckOptions.Add( new GridChunckOption(
                           ( _gridChunck.Xpos - 1 + _gridChunck.Ypos * wfcData.gridSize.x)
                            , _gridChunck.Xpos > 0
                            , wfcData.enableWalls && _gridChunck.Xpos == 0
                            , ChunckManager.RIGHT
                            , ChunckManager.LEFT)); 
                        
                   
                        gridChunckOptions.Add( new GridChunckOption(
                           ( _gridChunck.Xpos + (_gridChunck.Ypos - 1) * wfcData.gridSize.x)
                            , (_gridChunck.Ypos > 0)
                            , (wfcData.enableWalls && _gridChunck.Ypos == 0)
                            , ChunckManager.TOP
                            , ChunckManager.DOWN));

                        
                        gridChunckOptions.Add( new GridChunckOption(
                            (_gridChunck.Xpos + 1 + _gridChunck.Ypos  * wfcData.gridSize.x)
                            , _gridChunck.Xpos < wfcData.gridSize.x -1
                            , wfcData.enableWalls && _gridChunck.Xpos < wfcData.gridSize.x
                            , ChunckManager.LEFT
                            , ChunckManager.RIGHT));


                        gridChunckOptions.Add( new GridChunckOption(
                           ( _gridChunck.Xpos + (_gridChunck.Ypos + 1) * wfcData.gridSize.x)
                            , _gridChunck.Ypos < wfcData.gridSize.y -1
                            , wfcData.enableWalls && _gridChunck.Ypos < wfcData.gridSize.y
                            , ChunckManager.DOWN
                            , ChunckManager.TOP));



                        List<string> optionMissingValue = new List<string>();
                        bool isNextToInstancied = false; 
                        foreach (GridChunckOption chunckOption in gridChunckOptions)
                        {

                            if (chunckOption.expressionValue)
                            {

                                GridChunck nearCell = grid[chunckOption.gridChunckIndex];
                               
                                List<string> nearOptions = _chunckManager.GetNextGridEdgeOptions(chunckOption.primaryEdgeOption, nearCell.optionsAvailable).ToList();
                                List<Option> finalOptions = new List<Option>();
                                nearOptions?.ForEach(nearOption =>
                                {
                                    foreach (Option option in cell.optionsAvailable)
                                    {
                                        if (option.optionEdges[chunckOption.oppositeEdgeOption] == nearOption)
                                        {
                                            finalOptions.Add(option);
                                        }
                                    }
                                });
                          
                                if (nearCell.collapsed)
                                {
                                    isNextToInstancied = true;
                                    
                                    if (nearCell.instancied)
                                    {
                                        if (wfcData.edgeToFollow.Contains(_chunckManager.GetNextGridEdgeOptions(chunckOption.primaryEdgeOption, nearCell.optionsAvailable)[0]))
                                        {
                                            isAlone = false;
                                            availablePath.AddRange( nearCell.path);
                                        }
                                    }
                               
                                   
                                }
                                
                                cell.optionsAvailable = finalOptions.ToArray();

                            }
                            else if (chunckOption.oppositeExpressionValue)
                            {
                                List<string> AllEdgesAvailableOptions = _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption, cell.optionsAvailable).ToList();
                                List<string> NotAllowedOptions = AllEdgesAvailableOptions.Where(edge => wfcData.edgeToFollow.Any(WFCedge => WFCedge == edge)).ToList();
                                cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                                {
                                    bool isInside = true;
                                    NotAllowedOptions?.ForEach(NotallowedLeft =>
                                    {
                                        if (optionFull.optionEdges[chunckOption.oppositeEdgeOption].Contains(NotallowedLeft))
                                        {
                                            isInside = false;
                                        }
                                    });

                                    return isInside;

                                }).ToArray();
                            }
                           
                        }

                        if (!isFirstTime && isNextToInstancied)
                        {
                            foreach (GridChunckOption chunckOption in gridChunckOptions)
                            {
                            
                                if (chunckOption.expressionValue)
                                {

                                    GridChunck nearCell = grid[chunckOption.gridChunckIndex];
                                    nearCell.isAlone = false;
                                }
                            
                            }
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
    

    public void Draw()
    {
   
      
        if (iter < wfcData.gridSize.x*wfcData.gridSize.y )
        {
            iter++;

            if (iter == 1)
            {
                foreach (GridChunck gridChunck in grid)
                {
                    if (gridChunck.instancied == false)
                    {
                        CheckOptions(gridChunck,true);
                    }
                }
            }
            else if (iter == 2)
            {
                foreach (GridChunck gridChunck in grid)
                {
                    if (gridChunck.instancied == false)
                    {
                        CheckOptions(gridChunck,false);
                    }
                }    

            }
            else
            {
                //grid.Where(gridChunck => gridChunck.instancied == false && gridChunck.isAlone == false);
                foreach (GridChunck gridChunck in grid)
                {
                    if (gridChunck.instancied == false && gridChunck.isAlone == false)
                    {
                        CheckOptions(gridChunck,false);
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
                        chunckChoosed.instancied = true;
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
                            _gridChuncksQueue.Enqueue(new KeyValuePair<GridChunck, ChunckData>(chunckChoosed,chunckFound) );
                            
                           // Instantiate(chunckFound.assetsToInstanciate,new Vector3(chunckChoosed.Xpos * wfcData.chuncksSize.x, chunckChoosed.Ypos * wfcData.chuncksSize.y, 0), Quaternion.identity);
                            chunckChoosed.instancied = true;
                            
                        }
                    }
                } 
    
   
       
        }
        
    }

    private void Update() 
    {
       
            
            while (_gridChuncksQueue.Count > 0)
            {
                KeyValuePair<GridChunck, ChunckData> chunckChoosed = _gridChuncksQueue.Dequeue();
                Instantiate(chunckChoosed.Value.assetsToInstanciate,new Vector3(chunckChoosed.Key.Xpos * wfcData.chuncksSize.x, chunckChoosed.Key.Ypos * wfcData.chuncksSize.y, 0), Quaternion.identity);
            }

        
        
    }
}
