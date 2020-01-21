using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiopsyPathPlanning : BiopsyPath
{

    public override void Update()
    {
        base.Update();
        if (head.IsVisible())
        {
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, biopsyPoint.transform.position);
            lineRenderer.SetPosition(1, entryPoint.transform.position);
            lineRenderer.SetPosition(2, biopsyPoint.transform.position + TrajectoryVector()*0.2f);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

}
