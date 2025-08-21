using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Luz del sol")]
    public Light sun;

    [Header("Duración del ciclo (en segundos)")]
    public float dayDuration = 120f; // 2 minutos = 1 ciclo completo

    [Header("Colores del sol")]
    public Gradient sunColor;

    private float time;

    void Update()
    {
        if (sun == null) return;

      
        time += Time.deltaTime / dayDuration;
        if (time >= 1f) time = 0f;

      
        float sunRotation = time * 360f;
        sun.transform.rotation = Quaternion.Euler(sunRotation - 90f, 170f, 0);

     
        sun.color = sunColor.Evaluate(time);

        // Intensidad más baja de noche
        sun.intensity = Mathf.Clamp01(Mathf.Sin(time * Mathf.PI));
    }
}