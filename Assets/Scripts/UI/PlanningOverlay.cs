using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanningOverlay : UIObject
{
    public bool planned;

    public AudioSource audioSource;

    public void Plan()
    {
        planned = true;
        audioSource.Play();
    }
    
    public bool IsVisible()
    {
        return Program.instance.startOverlay.launched
        && Program.instance.patientID.approved
        && !Program.instance.planningOverlay.planned;
    }

    public override void UpdateInterface()
    {
        if(IsVisible()) 
        {
            base.UpdateInterface();
            enabled = true;
        }
        else
        {
            enabled = false;
        }
    }

}
