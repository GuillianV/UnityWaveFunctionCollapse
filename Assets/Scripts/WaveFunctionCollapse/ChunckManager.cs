using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;


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
        

    }
    
  /*  Stopwatch stopwatch = new Stopwatch();
    private int iter = 0;
    private int maxIter = 50;
    private long ticks = 0;
    
    stopwatch.Start();

        
    stopwatch.Stop();
    iter++;
    if (iter < maxIter)
    {
          
        ticks = ticks = stopwatch.ElapsedTicks;
    }
    else
    {
        //Best ~1150
        Debug.Log(ticks / iter);
            
    }*/

    
    public List<string> GetNextGridEdgeOptions(int Edge, Option[] fullOptions)
    {
        List<string> edgeOptions = new List<string>();
        foreach (Option option in fullOptions)
        {
            string edge = option.optionEdges[Edge];
            if (!edgeOptions.Contains(edge))
            {
                edgeOptions.Add(option.optionEdges[Edge]);
            }
        }
        return edgeOptions;
    }
    
    
    
}