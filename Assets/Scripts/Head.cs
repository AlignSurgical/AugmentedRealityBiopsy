using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Head : MonoBehaviour
{
    public Material materialInside;

    public Tumor tumor;

    public virtual bool IsVisible()
    {
        return false;
    }
    
}
