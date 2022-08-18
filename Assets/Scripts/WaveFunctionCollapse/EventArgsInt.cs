using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventArgsInt : EventArgs
{
    public int value;

    public EventArgsInt(int _value)
    {
        value = _value;
    }
}
