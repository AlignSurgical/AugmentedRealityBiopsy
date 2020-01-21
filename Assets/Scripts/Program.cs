using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Program : MonoBehaviour
{
    public static Program instance;
    public ImportManager importManager;
    
    public InteractionManager interactionManager;
    
    public TransferFunctionManager transferFunctionManager;
    public BiopsyManager biopsyManager;

    public TransferFunctionUI transferFunctionUI;

    public StartOverlay startOverlay;
    public PatientID patientID;
    public BiopsyUI biopsyUI;
    public PlanningOverlay planningOverlay;
    
    public VolumeRenderedObject volumeRenderedObject;
    public OperationOverlay operationOverlay;
    public FinalOverlay finalOverlay;


    public void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        startOverlay.UpdateInterface();
        operationOverlay.UpdateInterface();
        patientID.UpdateInterface();
        biopsyUI.UpdateInterface();
        planningOverlay.UpdateInterface();
        finalOverlay.UpdateInterface();
        // if(alphaSlider)
        //     alphaSlider.UpdateInterface();
    }

}
