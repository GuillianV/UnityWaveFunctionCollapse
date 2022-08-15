using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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