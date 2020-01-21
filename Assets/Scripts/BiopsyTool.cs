using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiopsyTool : MonoBehaviour
{
    public BiopsyManager manager;
    public LineRenderer rod;
    public LineRenderer coloredTip;

    public Vector3 toolTip;

    public float lengthOfTool = 0.2f;

    public GameObject RightIndexFinger()
    {
        return GameObject.Find("Right_PokePointer(Clone)");
    }

    public GameObject ThumbDistalJoint()
    {   
        return GameObject.Find("ThumbDistalJoint Proxy Transform");
    }
    
    public GameObject IndexDistalJoint()
    {
        return GameObject.Find("IndexMiddleJoint Proxy Transform");
    }

    public GameObject Wrist()
    {
        GameObject rightHand = GameObject.Find("Right_HandRight(Clone)");
        if (rightHand)
            return rightHand.transform.Find("Wrist Proxy Transform").gameObject;
        else
        {
            return null;
        }
    }

    public float DistanceFromTumor()
    {
        return Vector3.Distance(
            Program.instance.operationOverlay.head.tumor.transform.position,
            toolTip
        );
    }

    public bool CloseToEntryPoint()
    {
        if (RightIndexFinger()
        && Vector3.Distance(
            manager.entryPoint.transform.position,
            toolTip
        ) < 0.15f
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool VeryCloseToEntryPoint()
    {
        if (RightIndexFinger()
        && Vector3.Distance(
            manager.entryPoint.transform.position,
            toolTip
        ) < 0.05f
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 ToolVector()
    {
        if (RightIndexFinger() 
        && IndexDistalJoint() 
        && ThumbDistalJoint())
        {
            return ToolVector(
                RightIndexFinger().transform.position,
                // IndexFingerKnuckle().transform.position 
                Vector3.Lerp(
                    IndexDistalJoint().transform.position, 
                    ThumbDistalJoint().transform.position, 
                    0.5f
                )
            ).normalized;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 ToolVector(Vector3 frontObject, Vector3 backObject)
    {
        return (frontObject - backObject).normalized;
    }

    public void Update()
    {
        if (RightIndexFinger()
        && Program.instance.interactionManager.currentPhase == Phase.close)
        {
            toolTip = RightIndexFinger().transform.position + ToolVector() * lengthOfTool;

            Vector3 endOfTool = RightIndexFinger().transform.position + -ToolVector() * lengthOfTool;

            rod.positionCount = 2;
            rod.SetPosition(0, toolTip);
            rod.SetPosition(1, endOfTool);

            float remainingDistance = Vector3.Distance(
                Program.instance.operationOverlay.head.tumor.transform.position,
                toolTip
            );

            coloredTip.positionCount = 2;
            coloredTip.SetPosition(0, toolTip);
            coloredTip.SetPosition(
                1,
                toolTip + -ToolVector() * remainingDistance // fakefinger
            );

            this.transform.position = RightIndexFinger().transform.position;
        }
        else
        {
            rod.positionCount = 0;
            coloredTip.positionCount = 0;
            toolTip = Vector3.zero;
        }

    }
}
