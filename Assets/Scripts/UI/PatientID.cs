using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientID : UIObject
{
    public bool approved;

    public AudioSource audioSource;
    public void Approve()
    {
        approved = true;
        audioSource.Play();
    }

    public void Dismiss()
    {

    }

    public bool IsVisible()
    {
        return Program.instance.startOverlay.launched && !approved;
    }

    public override void UpdateInterface()
    {
        if (IsVisible())
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
