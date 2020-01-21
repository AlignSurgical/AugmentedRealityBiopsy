using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class BiopsyUI : UIObject
{
    public BiopsyManager manager;
    public TextMeshPro description;
    public TextMeshPro valueLabel;

    public override void UpdateInterface()
    {
        if(manager.currentPhase == BiopsyPhase.analyzing)
        {
            enabled = false;
        }
        else if(manager.currentPhase == BiopsyPhase.retracting)
        {
            enabled = true;
            description.enabled = false;
            valueLabel.text = "Biopsy Collected." + System.Environment.NewLine + "Withdraw the needle.";
            
            base.UpdateInterface();
        }
        else if (Program.instance.operationOverlay.IsVisible())
        {
            enabled = true;
            description.enabled = true;
            
            base.UpdateInterface();

            valueLabel.text = Math.Round(
                manager.biopsyTool.DistanceFromTumor(),
                2
            ).ToString() + " cm" 
            + System.Environment.NewLine 
            + manager.biopsyPath.BiopsyAngle();
        }

        // else if(Program.instance.operationOverlay.IsVisible() 
        // && manager.biopsyPath.CurrentCorrectness() != Correctness.correct)
        // {
        //     enabled = true;
        //     description.enabled = false;
        //     valueLabel.text = "Adjust Instrument";
            
        //     base.UpdateInterface();
        // }
        // else if (Program.instance.operationOverlay.IsVisible() 
        // && manager.biopsyPath.CurrentCorrectness() == Correctness.correct)
        // {
        //     enabled = true;
        //     description.enabled = true;
            
        //     base.UpdateInterface();

        //     valueLabel.text = Math.Round(
        //         manager.biopsyTool.DistanceFromTumor(),
        //         2
        //     ).ToString() + " cm" 
        //     + System.Environment.NewLine 
        //     + manager.biopsyPath.BiopsyAngle();
        // }
        else
        {
            enabled = false;
        }
    }
}
