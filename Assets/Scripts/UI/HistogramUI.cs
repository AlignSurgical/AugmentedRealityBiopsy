using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public class HistogramUI : MonoBehaviour
{
    public GameObject alphaSliderPrefab;


    public void CreateAlphaSlider(ControlPointAlpha controlPointAlpha)
    {
        // GameObject sliderGo = GameObject.Instantiate(alphaSliderPrefab) as GameObject;
        // sliderGo.transform.SetParent(transform);
        
        // HistogramAlphaSlider newSlider = sliderGo.GetComponent<HistogramAlphaSlider>();
        
        // newSlider.controlPointAlpha = controlPointAlpha;
    }
}
