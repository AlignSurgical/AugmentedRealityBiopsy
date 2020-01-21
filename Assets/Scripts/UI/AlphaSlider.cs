using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

public class AlphaSlider : PinchSlider
{
    
    public void UpdateInterface()
    {
        if (Program.instance.interactionManager.currentPhase == Phase.close)
        {
            enabled = true;
            transform.LookAt(2 * transform.position - Camera.main.transform.position);
        }
        else
        {
            enabled = false;
        }
    }

    public void OnEnable()
    {
        int children = transform.childCount;
        for (int i = 0; i < children; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void OnDisable()
    {
        int children = transform.childCount;
        for (int i = 0; i < children; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}