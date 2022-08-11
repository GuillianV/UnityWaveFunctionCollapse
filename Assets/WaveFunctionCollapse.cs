using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;


public class Options
{
    public Dictionary<int, string> options = new Dictionary<int, string>();

    public Options(List<Chunck> chuncks)
    {
     
        chuncks?.ForEach(chunck =>
        {
            string optionValue = string.Format("{0}-{1}-{2}-{3}", chunck.Edges.left.patern, chunck.Edges.top.patern,
                chunck.Edges.right.patern, chunck.Edges.down.patern);

            if (!options.ContainsValue(optionValue))
            {
                options.Add(options.Count+1,optionValue);
            }
        });
        
        
    }

    public int[] GetAllOptions()
    {
        return options.Keys.ToArray();
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
    public bool left;
    public bool top;
    public bool right;
    public bool down;

    [CanBeNull]
    public Chunck GetChunck(int index, Options options)
    {
       KeyValuePair<int,string> valuePair = options.options.FirstOrDefault(kpo => {return kpo.Key == index;});
       string[] edges = valuePair.Value.Split('-');

       Chunck chunckFound = null;
       if (edges.Length == 4)
       {
           if (edges[0] == Edges.left.patern && edges[1] == Edges.top.patern && edges[2] == Edges.right.patern  && edges[3] == Edges.down.patern  )
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
    public int[] optionsAvailable;

    public GridChunck(int[] _optionsAvailable)
    {
        optionsAvailable = _optionsAvailable;
    }
    
}


public class WaveFunctionCollapse : MonoBehaviour
{

    public Vector2Int generateSize = new Vector2Int(2,2);
    public Vector2Int chunckGridSize = new Vector2Int(8, 8);
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
   
   

       GridChunck[] gridCopy = grid.OrderBy(gridchunk => { return gridchunk.optionsAvailable.Length ; }).ToArray();
       gridCopy = gridCopy.Where(gridChunck => { return gridChunck.optionsAvailable.Length == gridCopy[0].optionsAvailable.Length; }).ToArray();

       GridChunck chunckChoosed = gridCopy[UnityEngine.Random.Range(0, gridCopy.Length)];
       chunckChoosed.collapsed = true;
       int optionIndex = UnityEngine.Random.Range(0, chunckChoosed.optionsAvailable.Length);
       int option = chunckChoosed.optionsAvailable[optionIndex];
       chunckChoosed.optionsAvailable = new int[]{option};

       Debug.Log(chunckChoosed);
       
        for (int j = 0; j < generateSize.y; j++) {
            for (int i = 0; i < generateSize.x; i++) {
                GridChunck cell = grid[i + j * generateSize.y];
                if (cell.collapsed) {
                    int index = cell.optionsAvailable[0];
                   
                        Chunck? chunckFound = Chuncks.FirstOrDefault(c => c.GetChunck(index, Options) != null);
                        Instantiate(chunckFound.gridToInstanciate,
                            new Vector3(i * chunckGridSize.x, j * chunckGridSize.y, 0), Quaternion.identity);

                } 
            }
        }
    }
    
    
}
