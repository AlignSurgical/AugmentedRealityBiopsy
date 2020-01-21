using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartOverlay : UIObject
{
    public bool launched;

    public AudioSource audioSource;

    public void Launch()
    {
        launched = true;
        audioSource.Play();
    }

    public bool IsVisible()
    {
        return !launched;
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
