using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Option
{
    public string optionValue;

    public string[] optionEdges;
    
    public Option(string _optionValue)
    {
        optionValue = _optionValue;
        optionEdges = _optionValue.Split('-'); 

    }

}
