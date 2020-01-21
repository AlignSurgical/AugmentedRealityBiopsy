using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferFunctionManager : MonoBehaviour
{
    public TransferFunction transferFunction;

    public Material histogramMaterial;
    public Material colorBarMaterial;

    // private int movingColPointIndex = -1;
    // private int movingAlphaPointIndex = -1;
    // private int selectedColPointIndex = -1;

    public TransferFunction CreateTransferFunction()
    {
        transferFunction = new TransferFunction();
        
        transferFunction.texture = new Texture2D(512, 2, TextureFormat.RGBAFloat, false);
        transferFunction.tfCols = new Color[512 * 2];
        
        transferFunction.AddControlPointColor(0.0f, new Color(0.11f, 0.14f, 0.13f, 1.0f));
        transferFunction.AddControlPointColor(0.2415f, new Color(0.469f, 0.354f, 0.223f, 1.0f));
        transferFunction.AddControlPointColor(0.3253f, new Color(1.0f, 1.0f, 1.0f, 1.0f));

        transferFunction.AddControlPointAlpha(0.0f, 0.0f);
        transferFunction.AddControlPointAlpha(0.1787f, 0.0f);
        transferFunction.AddControlPointAlpha(0.2f, 0.024f);
        transferFunction.AddControlPointAlpha(0.28f, 0.03f);
        transferFunction.AddControlPointAlpha(0.4f, 0.546f);
        transferFunction.AddControlPointAlpha(0.547f, 0.5266f);

        transferFunction.GenerateTexture();
        return transferFunction;
    }

    // Update is called once per frame
    void Update()
    {
        Color oldColour = GUI.color;

        transferFunction.GenerateTexture();

        histogramMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
        histogramMaterial.SetTexture("_HistTex", transferFunction.histogramTexture);

        Texture2D tfTexture = transferFunction.GetTexture();

        colorBarMaterial.SetTexture("_TFTex", transferFunction.GetTexture());

        // // Colour control points
        // for (int iCol = 0; iCol < transferFunction.colourControlPoints.Count; iCol++)
        // {
        //     ControlPointColor colPoint = transferFunction.colourControlPoints[iCol];
        //     if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
        //     {
        //         movingColPointIndex = iCol;
        //         selectedColPointIndex = iCol;
        //     }
        //     else if(movingColPointIndex == iCol)
        //     {
        //         colPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
        //     }
        //     tf.colourControlPoints[iCol] = colPoint;
        // }

        // Alpha control points
        // for (int iAlpha = 0; iAlpha < tf.alphaControlPoints.Count; iAlpha++)
        // {
        //     ControlPointAlpha alphaPoint = tf.alphaControlPoints[iAlpha];
        //     Rect ctrlBox = new Rect(bgRect.x + bgRect.width * alphaPoint.dataValue, bgRect.y + (1.0f - alphaPoint.alphaValue) * bgRect.height, 10, 10);
        //     GUI.color = oldColour;
        //     GUI.skin.box.fontSize = 6;
        //     GUI.Box(ctrlBox, "a");
        //     if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ctrlBox.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
        //     {
        //         movingAlphaPointIndex = iAlpha;
        //     }
        //     else if (movingAlphaPointIndex == iAlpha)
        //     {
        //         alphaPoint.dataValue = Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f);
        //         alphaPoint.alphaValue = Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f);
        //     }
        //     tf.alphaControlPoints[iAlpha] = alphaPoint;
        // }

        // if (Event.current.type == EventType.MouseUp)
        // {
        //     movingColPointIndex = -1;
        //     movingAlphaPointIndex = -1;
        // }

        // if(Event.current.type == EventType.MouseDown && Event.current.button == 1)
        // {
        //     if (bgRect.Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
        //         tf.alphaControlPoints.Add(
        //             tf.CreateControlPointAlpha(
        //                 Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), 
        //                 Mathf.Clamp(1.0f - (Event.current.mousePosition.y - bgRect.y) / bgRect.height, 0.0f, 1.0f)
        //             ));
        //     else
        //         tf.colourControlPoints.Add(tf.CreateControlPointColor(Mathf.Clamp((Event.current.mousePosition.x - bgRect.x) / bgRect.width, 0.0f, 1.0f), Random.ColorHSV()));
        //     selectedColPointIndex = -1;
        // }

        // if(selectedColPointIndex != -1)
        // {
        //     ControlPointColor colPoint = tf.colourControlPoints[selectedColPointIndex];
        //     colPoint.colourValue = EditorGUI.ColorField(new Rect(bgRect.x, bgRect.y + bgRect.height + 50, 100.0f, 40.0f), colPoint.colourValue);
        //     tf.colourControlPoints[selectedColPointIndex] = colPoint;
        // }

        // TEST!!! TODO
        Program.instance.volumeRenderedObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_TFTex", tfTexture);
        Program.instance.volumeRenderedObject.GetComponent<MeshRenderer>().sharedMaterial.DisableKeyword("TF2D_ON");

        GUI.color = oldColour;
    }
}
