using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunck
{
    public bool collapsed = false;
    public bool instancied = false;
    public int[] path = new []{0};
    public bool isLastOutput = false;
    
    public Option[] optionsAvailable;

    public GridChunck(Option[] _optionsAvailable)
    {
        optionsAvailable = _optionsAvailable;
    }
    
}
