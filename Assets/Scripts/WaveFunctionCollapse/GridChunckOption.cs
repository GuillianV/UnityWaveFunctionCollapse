using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridChunckOption
{
    public int gridChunckIndex;
    public bool expressionValue;
    public bool oppositeExpressionValue;
    public int primaryEdgeOption;
    public int oppositeEdgeOption;

    public GridChunckOption(int _gridChunckIndex, bool _expressionValue, bool _oppositeExpressionValue, int _primaryEdgeOption, int _oppositeEdgeOption )
    {

        gridChunckIndex = _gridChunckIndex;
        expressionValue = _expressionValue;
        oppositeExpressionValue = _oppositeExpressionValue;
        primaryEdgeOption = _primaryEdgeOption;
        oppositeEdgeOption = _oppositeEdgeOption;
    }
}
