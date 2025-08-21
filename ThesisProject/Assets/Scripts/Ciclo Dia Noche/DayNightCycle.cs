using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DayNightCycle : MonoBehaviour
{
    [Header("Luz del sol")]
    public Light sun;

    [Header("Duración del ciclo (en segundos)")]
    public float dayDuration = 120f;

    [Header("Colores del sol")]
    public Gradient sunColor;

    [Header("Niebla")]
    public Color nightFogColor = new Color(0.02f, 0.02f, 0.05f, 1f);
    public Color dayFogColor = new Color(0.6f, 0.7f, 0.9f, 1f);
    public float fogDensityNight = 0.08f;
    public float fogDensityDay = 0.0f;

    private float time;

    void Update()
    {
        if (sun == null) return;

        time += Time.deltaTime / dayDuration;
        if (time >= 1f) time = 0f;

        float sunRotation = time * 360f;
        sun.transform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0);

        sun.color = sunColor.Evaluate(time);

        float intensity = Mathf.Clamp01(Mathf.Sin(time * Mathf.PI));
        sun.intensity = intensity;

        // ---- NIEBLA ----
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, intensity);
        RenderSettings.fogDensity = Mathf.Lerp(fogDensityNight, fogDensityDay, intensity);
    }
}