// FogEffect.cs
using UnityEngine;

public class FogEffect : MonoBehaviour
{
    public bool setFogMode = true;
    public FogMode fogMode = FogMode.ExponentialSquared;
    public Color fogColor = new Color(0.5f, 0.55f, 0.6f, 1f);

    [Header("Density tuning")]
    public float maxFogDensity = 0.25f;   // was 0.06
    public float lightFogDensity = 0.05f; // was 0.01

    public void SetFog(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        if (intensity <= 0.001f)
        {
            RenderSettings.fog = false;
        }
        else
        {
            RenderSettings.fog = true;
            if (setFogMode) RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = Mathf.Lerp(0f, maxFogDensity, intensity);
            RenderSettings.fogColor = fogColor;
        }
    }

    public void SetFogImmediate(float intensity) => SetFog(intensity);

    public float CurrentFog()
    {
        if (!RenderSettings.fog) return 0f;
        return Mathf.InverseLerp(0f, maxFogDensity, RenderSettings.fogDensity);
    }
}
