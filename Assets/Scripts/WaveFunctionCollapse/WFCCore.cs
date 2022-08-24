using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WFCCore
{
    

    public WFCData wfcData;
    public Queue<KeyValuePair<GridChunck,ChunckData>> _gridChuncksQueue = new Queue<KeyValuePair<GridChunck,ChunckData>>();

    
    
    private int iter = 0;
    private ChunckManager _chunckManager;
    private int pathIdentifier = 0;
    private System.Random _random;
    private JoinPath JoinPath;
    private GridChunck [] grid ;
    private List<Option> MissingOptions = new List<Option>();

    public event EventHandler<EventArgs> OnEndDraw;
   


 

    public void UpdateData(WFCData _wfcData, string option)
    {
        wfcData = _wfcData;
    }
    
    public  void Start()
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


        for (var i = 0; i < grid.Length; i++)
        {
            Draw();
      
        }
        
   
   
        
    }
    



    void  CheckOptions(GridChunck _gridChunck, bool isFirstTime)
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
                            , ChunckManager.LEFT
                            , ChunckManager.RIGHT)); 
                        
                   
                        gridChunckOptions.Add( new GridChunckOption(
                            ( _gridChunck.Xpos + (_gridChunck.Ypos + 1) * wfcData.gridSize.x)
                            , _gridChunck.Ypos < wfcData.gridSize.y -1
                           , wfcData.enableWalls && _gridChunck.Ypos < wfcData.gridSize.y
                            , ChunckManager.TOP
                            , ChunckManager.DOWN));

                        
                        gridChunckOptions.Add( new GridChunckOption(
                            (_gridChunck.Xpos + 1 + _gridChunck.Ypos  * wfcData.gridSize.x)
                            , _gridChunck.Xpos < wfcData.gridSize.x -1
                            , wfcData.enableWalls && _gridChunck.Xpos < wfcData.gridSize.x
                            , ChunckManager.RIGHT
                            , ChunckManager.LEFT));


                        gridChunckOptions.Add( new GridChunckOption(
                            ( _gridChunck.Xpos + (_gridChunck.Ypos - 1) * wfcData.gridSize.x)
                            , (_gridChunck.Ypos > 0)
                           , (wfcData.enableWalls && _gridChunck.Ypos == 0)
                            , ChunckManager.DOWN
                            , ChunckManager.TOP));



                        string[] optionMissingValue = new string[4];
                        bool isNextToInstancied = false; 
                        foreach (GridChunckOption chunckOption in gridChunckOptions)
                        {

                            if (chunckOption.expressionValue)
                            {

                                GridChunck nearCell = grid[chunckOption.gridChunckIndex];
                               
                                List<string> nearOptions = _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption, nearCell.optionsAvailable).ToList();
                                List<Option> finalOptions = new List<Option>();
                                nearOptions?.ForEach(nearOption =>
                                {
                                    foreach (Option option in cell.optionsAvailable)
                                    {
                                        if (option.optionEdges[chunckOption.primaryEdgeOption] == new string(nearOption.Reverse().ToArray()) )
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
                                        List<string> NextGridEdgeOptions =
                                            _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption,
                                                nearCell.optionsAvailable);

                                        if (NextGridEdgeOptions.Count > 0)
                                        {
                                            if (wfcData.edgeToFollow.Contains(  new string( NextGridEdgeOptions[0].Reverse().ToArray())))
                                            {
                                                isAlone = false;
                                                availablePath.AddRange( nearCell.path);
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarning("Missing Edge Options");
                                        }
                                        
                                    }
                               
                                   
                                }


                                if (finalOptions.ToArray().Length == 0)
                                {
                                    if (nearOptions.Count > 0)
                                    {
                                        optionMissingValue[chunckOption.primaryEdgeOption] =  new string(nearOptions[0].Reverse().ToArray());

                                    }
                                    
                                }
                                
                                cell.optionsAvailable = finalOptions.ToArray();

                            }
                            else if (chunckOption.oppositeExpressionValue)
                            {
                                List<string> AllEdgesAvailableOptions = _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption, cell.optionsAvailable).ToList();
                                List<string> NotAllowedOptions = new List<string>();
                               
                                    NotAllowedOptions = AllEdgesAvailableOptions.Where(edge => wfcData.edgeToFollow.Any(WFCedge => WFCedge == edge)).ToList();
                                
                                
                                cell.optionsAvailable = cell.optionsAvailable.Where(optionFull =>
                                {
                                    bool isInside = true;
                                    NotAllowedOptions?.ForEach(NotallowedLeft =>
                                    {
                                        // ?
                                        if (optionFull.optionEdges[chunckOption.primaryEdgeOption].Contains( new string( NotallowedLeft.Reverse().ToArray())))
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
                        
                        if (optionMissingValue.Count(missingOption => missingOption != null) > 0)
                        {
                            foreach (GridChunckOption chunckOption in gridChunckOptions)
                            {

                                string? missingOpt = optionMissingValue[chunckOption.primaryEdgeOption];
                                if (missingOpt == null)
                                {

                                    if (chunckOption.expressionValue)
                                    {
                                        List<string> nextCellCollideOptionEdge = _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption, grid[chunckOption.gridChunckIndex].optionsAvailable).ToList();
                                        if (nextCellCollideOptionEdge.Count > 0)
                                        {
                                            optionMissingValue[chunckOption.primaryEdgeOption] =
                                                new string(nextCellCollideOptionEdge[0].Reverse().ToArray());
                                           

                                        }
                                    }
                                    else
                                    {
                                        List<string> AllEdgesAvailableOptions = _chunckManager.GetNextGridEdgeOptions(chunckOption.oppositeEdgeOption, cell.optionsAvailable).ToList();
                                        List<string> NotAllowedOptions = new List<string>();
                               
                                        NotAllowedOptions = AllEdgesAvailableOptions.Where(edge => wfcData.edgeToFollow.Any(WFCedge => WFCedge == edge)).ToList();

                                        if (NotAllowedOptions.Count > 0)
                                        {
                                            optionMissingValue[chunckOption.primaryEdgeOption] =
                                                new string(NotAllowedOptions[0].Reverse().ToArray());
 
                                        }

                                        
                                      
                                    }
                                    
                                  
                                }
                                    
                            }

                          
                            
                            Debug.LogError(string.Format("Missing chunck type of {0}-{1}-{2}-{3} at position X : {4}, Y : {5}",optionMissingValue[ChunckManager.LEFT],optionMissingValue[ChunckManager.TOP],optionMissingValue[ChunckManager.RIGHT],optionMissingValue[ChunckManager.DOWN], cell.Xpos * wfcData.chuncksSize.x, cell.Ypos * wfcData.chuncksSize.y));
                          
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
                grid.ToList().ForEach(gridChunck =>
                {
                    if (gridChunck.optionsAvailable.Length == 0)
                    {
                        gridChunck.optionsAvailable = _chunckManager.options.ToArray();
                    }
                    
                }) ;
                if (grid.Count(gridChunck => gridChunck.instancied == false && gridChunck.isAlone == false) == 0)
                {
                    foreach (GridChunck gridChunck in grid)
                    {
                        CheckOptions(gridChunck, false);
                    } 
                }
                else
                {
                    foreach (GridChunck gridChunck in grid)
                    {
                        if (gridChunck.instancied == false && gridChunck.isAlone == false)
                        {
                            CheckOptions(gridChunck,false);
                        }
                    } 
                }
                
                

            }
          
              

          
            
    
             

            pathIdentifier++;
            GridChunck[] gridCopy = grid.Where(chunck => { return chunck.collapsed == false;}).OrderBy(gridchunk => { return gridchunk.optionsAvailable.Length ; }).ToArray();
           
            
            //
            gridCopy = gridCopy.Where(gridChunck => { return gridChunck.optionsAvailable.Length == gridCopy[0].optionsAvailable.Length ;
            }).ToArray();

            if (gridCopy.Length == 0)
            {
                return;
            }
            GridChunck chunckChoosed = gridCopy[_random.Next(0, gridCopy.Length)];

            chunckChoosed.collapsed = true;

       
                if (chunckChoosed.collapsed && !chunckChoosed.instancied)
                {
                    int pathNumber = 0;


                    if (chunckChoosed.optionsAvailable.Length > 0)
                    {
                        
                    
                    
                        foreach (string optionEdge in chunckChoosed.optionsAvailable[0].optionEdges)
                        {
                            if (wfcData.edgeToFollow.Contains(optionEdge))
                            {
                                pathNumber++;
                            }
                                    
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


                        if ((chunckChoosed.isLastOutput && wfcData.allChunckLinked || iter == 1) && chunckChoosed.optionsAvailable.Length>0 )
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
                        else
                        {
                            if (chunckChoosed.optionsAvailable.Length == 0 || chunckChoosed.optionsAvailable[0].optionValue == null  )
                            {

                                _gridChuncksQueue.Enqueue(new KeyValuePair<GridChunck, ChunckData>(chunckChoosed,null) );
                            
                                // Instantiate(chunckFound.assetsToInstanciate,new Vector3(chunckChoosed.Xpos * wfcData.chuncksSize.x, chunckChoosed.Ypos * wfcData.chuncksSize.y, 0), Quaternion.identity);
                                chunckChoosed.instancied = true;

                                Debug.LogWarning("Missing chunck to match");
                                chunckChoosed.instancied = true;
                                chunckChoosed.optionsAvailable = _chunckManager.options.ToArray();
                        
                            }
                        }
                    
                } 
    
   
       
        }
        OnEndDraw?.Invoke(this,new EventArgs());
    }

}
