using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshObject : MonoBehaviour
{
    public float alpha;

    public virtual bool IsVisible()
    {
        return false;
    }

    public virtual void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
    }

}
