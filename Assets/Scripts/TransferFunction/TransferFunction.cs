using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class TransferFunction
{
    public List<ControlPointColor> colourControlPoints = new List<ControlPointColor>();
    public List<ControlPointAlpha> alphaControlPoints = new List<ControlPointAlpha>();

    public Texture2D histogramTexture = null;

    public  Texture2D texture = null;
    public Color[] tfCols;

    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 2;

    public void AddControlPointColor(float dataValue, Color color)
    {
        ControlPointColor newControlPoint = CreateControlPointColor(
            dataValue, color
        );
        
        colourControlPoints.Add(newControlPoint);
        
    }

    public void AddControlPointAlpha(float dataValue, float alphaValue)
    {
        ControlPointAlpha newControlPoint =  CreateControlPointAlpha(
            dataValue, alphaValue
        );
        alphaControlPoints.Add(newControlPoint);
        Program.instance.transferFunctionUI.histogramUI.CreateAlphaSlider(newControlPoint);
    }

    public ControlPointAlpha CreateControlPointAlpha(float dataValue, float alphaValue)
    {
        ControlPointAlpha newControlPoint = ScriptableObject.CreateInstance("ControlPointAlpha") as ControlPointAlpha;
        newControlPoint.dataValue = dataValue;
        newControlPoint.alphaValue = alphaValue;
        return newControlPoint;
    }

    public ControlPointColor CreateControlPointColor(float dataValue, Color color)
    {
        ControlPointColor newControlPoint = ScriptableObject.CreateInstance("ControlPointColor") as ControlPointColor;
        newControlPoint.dataValue = dataValue;
        newControlPoint.colourValue = color;
        return newControlPoint;
    }

    public Texture2D GetTexture()
    {
        if (texture == null)
            GenerateTexture();

        return texture;
    }

    public void GenerateTexture()
    {
        List<ControlPointColor> cols = new List<ControlPointColor>(colourControlPoints);
        List<ControlPointAlpha> alphas = new List<ControlPointAlpha>(alphaControlPoints);

        // Sort lists of control points
        cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
        alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

        // Add colour points at beginning and end
        if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
            cols.Add(CreateControlPointColor(1.0f, Color.white));
        if(cols[0].dataValue > 0.0f)
            cols.Insert(0, CreateControlPointColor(0.0f, Color.white));

        // Add alpha points at beginning and end
        if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
            alphas.Add(CreateControlPointAlpha(1.0f, 1.0f));
        if (alphas[0].dataValue > 0.0f)
            alphas.Insert(0, CreateControlPointAlpha(0.0f, 0.0f));

        int numColours = cols.Count;
        int numAlphas = alphas.Count;
        int iCurrColour = 0;
        int iCurrAlpha = 0;

        for(int iX = 0; iX < TEXTURE_WIDTH; iX++)
        {
            float t = iX / (float)(TEXTURE_WIDTH - 1);
            while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                iCurrColour++;
            while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                iCurrAlpha++;

            ControlPointColor leftCol = cols[iCurrColour];
            ControlPointColor rightCol = cols[iCurrColour + 1];
            ControlPointAlpha leftAlpha = alphas[iCurrAlpha];
            ControlPointAlpha rightAlpha = alphas[iCurrAlpha + 1];

            float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
            float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

            Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
            pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);

            for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
            {
                tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
            }
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(tfCols);
        texture.Apply();
    }
}

