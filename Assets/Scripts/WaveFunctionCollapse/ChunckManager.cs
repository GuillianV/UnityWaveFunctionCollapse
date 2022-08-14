using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;



public class ChunckManager : MonoBehaviour
{
    private ChunckData[] _chunckDatas = new ChunckData[]{};

    public ChunckManager(ChunckData[] datas)
    {
        _chunckDatas = datas;
    }
    
    [CanBeNull]
    public ChunckData GetChunck(string index, Options options)
    {
      
        string valuePair = options.options.FirstOrDefault(kpo => {return kpo == index;});
        if (valuePair == null)
        {
            return null;
        }
        
        string[] edges = valuePair.Split('-');
        ChunckData chunckFound = null;
        
        foreach (ChunckData chunckData in _chunckDatas)
        {
          
            if (edges.Length == 4)
            {
                if (edges[Options.LEFT] == chunckData.Edges.left.patern && edges[Options.TOP] == chunckData.Edges.top.patern && edges[Options.RIGHT] == chunckData.Edges.right.patern  && edges[Options.DOWN] == chunckData.Edges.down.patern  )
                {
                    chunckFound = chunckData;
                }
           
            }
        }
        
        return chunckFound;

    }
    
    
    
}