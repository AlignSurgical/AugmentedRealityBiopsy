using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum BiopsyPhase
{
    collecting,
    retracting,
    analyzing
}

public class BiopsyManager : MonoBehaviour
{
    public BiopsyPhase currentPhase;
    public BiopsyTool biopsyTool;

    public BiopsyPath biopsyPath;

    public GameObject biopsyPoint;
    public SurgicalPointEntry entryPoint;

    public bool procedureOver;

    public AudioSource audioSource;

    public AudioClip soundBiopsyCollected;
    public AudioClip soundTestimonial;

    public void Update()
    {
        if (currentPhase == BiopsyPhase.collecting
        && Vector3.Distance(
                biopsyPoint.transform.position,
                biopsyTool.toolTip
            ) < 0.02f)
        {
           CollectBiopsy();
        }
        else if(currentPhase == BiopsyPhase.retracting
            && Vector3.Distance(
            biopsyPoint.transform.position,
            biopsyTool.toolTip
        ) > 0.12f)
        {
           AnalyzeBiopsy();
        }

    }
    public void CollectBiopsy()
    {
        currentPhase = BiopsyPhase.retracting;
        audioSource.PlayOneShot(soundBiopsyCollected);
    }

    public void AnalyzeBiopsy()
    {
        currentPhase = BiopsyPhase.analyzing;
        audioSource.PlayOneShot(soundBiopsyCollected);
        audioSource.PlayOneShot(soundTestimonial);
    }
}
