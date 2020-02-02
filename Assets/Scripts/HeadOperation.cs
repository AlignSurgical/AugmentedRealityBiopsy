using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOperation : Head
{
    public Material materialSkin;
    public OperationOverlay operationOverlay;

    public override bool IsVisible()
    {
        return operationOverlay.IsVisible();
    }

    void Update()
    {
        if(IsVisible())
        {
            if (operationOverlay.currentStyle == OperationOverlayStyle.skinned)
            {
                GetComponent<MeshRenderer>().enabled = true;
                GetComponent<MeshRenderer>().material = materialSkin;

            }
            else if (operationOverlay.currentStyle == OperationOverlayStyle.mesh
            || operationOverlay.currentStyle == OperationOverlayStyle.compound)
            {
                GetComponent<MeshRenderer>().enabled = true;
                GetComponent<MeshRenderer>().material = materialInside;
            }
                else if (operationOverlay.currentStyle == OperationOverlayStyle.mri)
            {
                GetComponent<MeshRenderer>().enabled = false;
            }

            tumor.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            GetComponent<MeshRenderer>().enabled = false;
            
            tumor.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
