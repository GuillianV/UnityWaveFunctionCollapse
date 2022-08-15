using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;



public class ChunckManager : MonoBehaviour
{
    public List<ChunckData> _chunckDatas = new List<ChunckData>();
    public List<Option> options = new List<Option>();

    private Dictionary<ChunckData, Option> _chunckOptions = new Dictionary<ChunckData, Option>();


    public const int LEFT = 0;
    public const int TOP = 1;
    public const int RIGHT = 2;
    public const int DOWN = 3;
    
    
    
    public ChunckManager(ChunckData[] datas)
    {
       
        
        foreach (ChunckData chunckData in datas)
        {
            string optionValue = string.Format("{0}-{1}-{2}-{3}", chunckData.Edges.left.patern, chunckData.Edges.top.patern,
                chunckData.Edges.right.patern, chunckData.Edges.down.patern);

            
            _chunckDatas.Add(chunckData);
            Option newOption = new Option(optionValue);
            options.Add(newOption);
            _chunckOptions.Add(chunckData,newOption);
           
        }
       
    }
    
    [CanBeNull]
    public ChunckData GetChunck(string _optionValue)
    {

        return _chunckOptions.FirstOrDefault(_chunckOption => _chunckOption.Value.optionValue == _optionValue).Key;
        
     /*   Options option = options.FirstOrDefault(option => {return option.option == _optionValue;});
        if (string.IsNullOrEmpty(option.option))
        {
            return null;
        }
        
        string[] edges = option.option.Split('-');
        ChunckData chunckFound = null;
        
        foreach (ChunckData chunckData in _chunckDatas)
        {
          
            if (edges.Length == 4)
            {
                if (edges[LEFT] == chunckData.Edges.left.patern && edges[TOP] == chunckData.Edges.top.patern && edges[RIGHT] == chunckData.Edges.right.patern  && edges[DOWN] == chunckData.Edges.down.patern  )
                {
                    chunckFound = chunckData;
                }
           
            }
        }
        
        return chunckFound; */

    }
    
    
    public List<string> GetNextGridEdgeOptions(int Edge, Option[] fullOptions)
    {
        List<string> edgeOption = new List<string>();
        if (fullOptions.Length > 0 )
        {
            
                fullOptions?.ToList()?.ForEach(option =>
                {
                    string dopt = option.optionValue.Split('-')[Edge];
                    if (!edgeOption.Contains(dopt))
                    {
                        edgeOption.Add(dopt);
                    }
                           
                });
         
          
        }
        return edgeOption;
    }
    
    
    
}