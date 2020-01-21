using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OperationOverlayStyle
{
    skinned,
    compound,
    mesh,
    mri
}

public class OperationOverlay : UIObject
{
    public OperationOverlayStyle currentStyle;
    public HeadOperation head;
    public float alpha;

    public void SetAlpha(float newAlpha)
    {
        alpha = newAlpha;
        head.materialInside.color = new Color(0.7f, 0.7f, 0.7f, alpha);
    }

    public bool IsVisible()
    {
        return  Program.instance.startOverlay.launched
        && Program.instance.patientID.approved
        && Program.instance.planningOverlay.planned
        && Program.instance.biopsyManager.currentPhase != BiopsyPhase.analyzing;
    }


    public override void UpdateInterface()
    {
        if (IsVisible())
        {
            base.UpdateInterface();
            enabled = true;
            if (Program.instance.biopsyManager.biopsyTool.CloseToEntryPoint())
            {
                currentStyle = OperationOverlayStyle.mesh;
            }
            else
            {
                currentStyle = OperationOverlayStyle.skinned;
            }
        }
        else
        {
            enabled = false;
        }

    }

}
