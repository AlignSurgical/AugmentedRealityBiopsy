using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Head : MeshObject
{
    public Material materialInside;

    public Tumor tumor;

    public override void SetAlpha(float newAlpha)
    {
        base.SetAlpha(newAlpha);
        materialInside.color = new Color(0.7f, 0.7f, 0.7f, alpha);
    }
}
