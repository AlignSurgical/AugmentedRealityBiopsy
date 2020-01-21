using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiopsyPathOperation : BiopsyPath
{
    public Material materialCorrect;
    public Material materialIntermediate;
    public Material materialFalse;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (head.IsVisible())
        {
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, biopsyPoint.transform.position);
            lineRenderer.SetPosition(1, entryPoint.transform.position);
            lineRenderer.SetPosition(2, biopsyPoint.transform.position + TrajectoryVector()*0.2f);

            if(biopsyManager.biopsyTool.VeryCloseToEntryPoint() && CurrentCorrectness() == Correctness.correct)
            {
                lineRenderer.material = materialCorrect;
            }
            else if(biopsyManager.biopsyTool.VeryCloseToEntryPoint() && CurrentCorrectness() == Correctness.intermediate)
            {
                lineRenderer.material = materialIntermediate;
            }
            else if(biopsyManager.biopsyTool.VeryCloseToEntryPoint() && CurrentCorrectness() == Correctness.off)
            {
                lineRenderer.material = materialFalse;
            }
            else 
            {
                lineRenderer.material = materialNormal;
            }
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }

}
