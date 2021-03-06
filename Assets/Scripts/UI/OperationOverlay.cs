﻿using System.Collections;
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
    public VolumeRenderedObject volumeRenderedObject;
    public MeshObject meshObject;

    public bool IsVisible()
    {
        if(Program.instance.currentMode == ProgramMode.dissection)
            return true;
        else if(Program.instance.currentMode == ProgramMode.biopsy)
            return  Program.instance.startOverlay.launched
            && Program.instance.patientID.approved
            && Program.instance.planningOverlay.planned
            && Program.instance.biopsyManager.currentPhase != BiopsyPhase.analyzing;
        else
            return false;
    }


    public override void UpdateInterface()
    {
        if (IsVisible())
        {
            base.UpdateInterface();
            enabled = true;

            if (Program.instance.currentMode == ProgramMode.biopsy 
            && Program.instance.biopsyManager.biopsyTool.CloseToEntryPoint())
            {
                currentStyle = OperationOverlayStyle.mesh;
            }
            else if(Program.instance.currentMode == ProgramMode.biopsy 
            && !Program.instance.biopsyManager.biopsyTool.CloseToEntryPoint())
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
