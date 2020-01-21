using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurgicalPointEntry : SurgicalPoint
{
    public void Update()
    {
        if(Program.instance.interactionManager.currentPhase == Phase.close)
        {
            GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
             GetComponent<MeshRenderer>().enabled = false;
        }
    }

   
}
