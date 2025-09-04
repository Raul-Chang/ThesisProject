using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DisableFogForMapCamera : MonoBehaviour
{
    private bool previousFog;

    void OnPreRender()
    {
        previousFog = RenderSettings.fog;
        RenderSettings.fog = false;   
    }

    void OnPostRender()
    {
        RenderSettings.fog = previousFog;
    }
}