using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunck
{
    public bool collapsed = false;
    public bool instancied = false;
    public int[] path = new []{0};
    public bool isLastOutput = false;

    public bool isAlone = true;
    
    public Option[] optionsAvailable;

    public int gridValue;
    public int Xpos;
    public int Ypos;
    
    public GridChunck(Option[] _optionsAvailable, int _gridValue)
    {
        optionsAvailable = _optionsAvailable;
        gridValue = _gridValue;
    }
    
}
