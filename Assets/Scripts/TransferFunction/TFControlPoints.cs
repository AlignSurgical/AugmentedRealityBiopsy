using UnityEngine;

public struct TFColourControlPoint
{
    public float dataValue;
    public Color colourValue;

    public TFColourControlPoint(float dataValue, Color colourValue)
    {
        this.dataValue = dataValue;
        this.colourValue = colourValue;
    }
}
