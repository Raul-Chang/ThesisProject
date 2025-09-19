// RainEffect.cs
using UnityEngine;

public class RainEffect : MonoBehaviour
{
    public ParticleSystem rainParticleSystem;
    public AudioSource rainAudio;

    [Header("Emission tuning")]
    public float maxEmissionRate = 1200f;   // heavy rain
    public float lightEmissionRate = 250f;  // light rain reference
    public float minEmissionRate = 0f;

    void Reset()
    {
        if (rainParticleSystem == null) rainParticleSystem = GetComponentInChildren<ParticleSystem>();
        if (rainAudio == null) rainAudio = GetComponent<AudioSource>();
    }

    public void SetIntensity(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        // inside RainEffect.cs -> SetIntensity
        if (rainParticleSystem != null)
        {
            var emission = rainParticleSystem.emission;
            float rate = Mathf.Lerp(minEmissionRate, maxEmissionRate, intensity);
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);

            if (intensity <= 0.001f)
            {
                rainParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                rainParticleSystem.gameObject.SetActive(false);
            }
            else
            {
                if (!rainParticleSystem.gameObject.activeSelf) rainParticleSystem.gameObject.SetActive(true);
                if (!rainParticleSystem.isPlaying) rainParticleSystem.Play();
            }
        }


        if (rainAudio != null)
        {
            rainAudio.volume = Mathf.Lerp(0f, 1f, intensity);
            if (intensity <= 0.001f) rainAudio.Stop(); else if (!rainAudio.isPlaying) rainAudio.Play();
        }
    }

    public void SetIntensityImmediate(float intensity) { SetIntensity(intensity); }

    public float CurrentIntensity()
    {
        if (rainParticleSystem == null) return 0f;
        var emission = rainParticleSystem.emission;
        // assume constantMax is what we used
        float currentRate = emission.rateOverTime.constantMax;
        return Mathf.InverseLerp(minEmissionRate, maxEmissionRate, currentRate);
    }
}
