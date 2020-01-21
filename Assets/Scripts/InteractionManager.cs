using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Phase
{
    far, // you see nothing
    close, // entrypoint
}


public class InteractionManager : MonoBehaviour
{
    public Phase currentPhase;
    public Camera mainCamera;
    public PatientID patientID;

    public float DistanceFromCamera(Vector3 checkPosition)
    {
        return Vector3.Distance(
            mainCamera.transform.position,
            checkPosition
        );
    }

    public float DistanceFromCamera()
    {
        return Vector3.Distance(
            mainCamera.transform.position,
            Program.instance.operationOverlay.transform.position
        );
    }

    public void Update() // stays here
    {
        if(DistanceFromCamera() > 1.8f)
        {
            Program.instance.startOverlay.launched = false;
            Program.instance.patientID.approved = false;
            Program.instance.planningOverlay.planned = false;  
            Program.instance.biopsyManager.procedureOver = false;
            Program.instance.biopsyManager.currentPhase = BiopsyPhase.collecting;
        }
        else if(DistanceFromCamera() > 1.1f )
        {
            currentPhase = Phase.far;
        }
        else if(DistanceFromCamera() <= 1.1f)
        {
            currentPhase = Phase.close;
        }
    }

}
