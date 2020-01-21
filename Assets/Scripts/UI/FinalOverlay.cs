using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalOverlay : UIObject
{
// Start is called before the first frame update
    public bool IsVisible()
    {
        return Program.instance.biopsyManager.currentPhase == BiopsyPhase.analyzing;
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
