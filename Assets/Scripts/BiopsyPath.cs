using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Correctness
{
    off,
    intermediate,
    correct
}

public abstract class BiopsyPath : MonoBehaviour
{
    public BiopsyManager biopsyManager;
    public InteractionManager interactionManager;
    public LineRenderer lineRenderer;
    public SurgicalPointEntry entryPoint;
    public GameObject biopsyPoint;
    public Head head;
    public Material materialNormal;

    public float BiopsyAngle()
    {
        return Vector3.Angle(
            biopsyManager.biopsyTool.ToolVector(),
            TrajectoryVector()
        );
    }

    public Correctness CurrentCorrectness()
    {
        if(BiopsyAngle() < 10f 
        && BiopsyAngle() > 170f)
        {
            return Correctness.correct;
        }
        else if(BiopsyAngle() < 20f 
        && BiopsyAngle() > 160f)
        {
            return Correctness.intermediate;
        }
        else
        {
            return Correctness.off;
        }
    }

    public Vector3 TrajectoryVector()
    {
        return (entryPoint.transform.position - biopsyPoint.transform.position).normalized;
    }

    public virtual void Update()
    {

    }

}
