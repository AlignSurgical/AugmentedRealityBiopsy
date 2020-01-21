using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPlanning : Head
{
    public PlanningOverlay planningOverlay;


    public override bool IsVisible()
    {
        return planningOverlay.IsVisible();
    }


}
