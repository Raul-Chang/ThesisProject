// WeatherController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeatherType { Clear, Cloudy, LightRain, HeavyRain, LightFog, HeavyFog }

public class WeatherController : MonoBehaviour
{
    [Header("Forecast")]
    public int totalDays = 5;
    public float transitionDuration = 2f;

    [Header("Allowed weather (random pool toggles)")]
    public bool allowCloudy = true;
    public bool allowLightRain = true;
    public bool allowHeavyRain = true;
    public bool allowLightFog = true;
    public bool allowHeavyFog = true;

    [Header("Force toggles (testing) - if any true, that weather overrides forecast")]
    public bool forceClear = false;
    public bool forceCloudy = false;
    public bool forceLightRain = false;
    public bool forceHeavyRain = false;
    public bool forceLightFog = false;
    public bool forceHeavyFog = false;

    [Header("Effect references (assign in inspector)")]
    public RainEffect rainEffect;
    public CloudEffect cloudEffect;
    public FogEffect fogEffect;
    public Light mainDirectionalLight; // optional, will dim based on weather

    // runtime
    public WeatherType[] forecast; // index 0 = Monday

    void Awake()
    {
        if (totalDays < 1) totalDays = 5;
        GenerateForecast();
    }

    // Generate forecast array. Monday (index 0) is always Clear per your rule.
    public void GenerateForecast()
    {
        forecast = new WeatherType[totalDays];
        forecast[0] = WeatherType.Clear;

        List<WeatherType> pool = new List<WeatherType>();
        if (allowCloudy) pool.Add(WeatherType.Cloudy);
        if (allowLightRain) pool.Add(WeatherType.LightRain);
        if (allowHeavyRain) pool.Add(WeatherType.HeavyRain);
        if (allowLightFog) pool.Add(WeatherType.LightFog);
        if (allowHeavyFog) pool.Add(WeatherType.HeavyFog);

        for (int i = 1; i < totalDays; i++)
        {
            if (pool.Count == 0) forecast[i] = WeatherType.Clear;
            else forecast[i] = pool[Random.Range(0, pool.Count)];
        }

        // quick log
        string s = "Forecast: ";
        for (int i = 0; i < totalDays; i++) s += $"[{i + 1}] {forecast[i]} ";
        Debug.Log(s);
    }

    // Public call: apply the weather for the given day index (0-based).
    // It respects force toggles (the force toggles override the forecast if set).
    public void ApplyForecastDay(int dayIndex)
    {
        if (forecast == null || forecast.Length == 0) GenerateForecast();
        if (dayIndex < 0 || dayIndex >= forecast.Length) dayIndex = 0;
        WeatherType baseWeather = forecast[dayIndex];

        WeatherType effective = ResolveForcedOrBase(baseWeather);
        ApplyWeather(effective);
    }

    WeatherType ResolveForcedOrBase(WeatherType baseWeather)
    {
        if (forceClear) return WeatherType.Clear;
        if (forceCloudy) return WeatherType.Cloudy;
        if (forceLightRain) return WeatherType.LightRain;
        if (forceHeavyRain) return WeatherType.HeavyRain;
        if (forceLightFog) return WeatherType.LightFog;
        if (forceHeavyFog) return WeatherType.HeavyFog;
        return baseWeather;
    }

    // Map weather to effect intensities and start transition coroutine
    void ApplyWeather(WeatherType w)
    {
        float targetCloud = 0f, targetRain = 0f, targetFog = 0f;
        float targetLightIntensity = mainDirectionalLight != null ? 1f : -1f;

        switch (w)
        {
            case WeatherType.Clear:
                targetCloud = 0f; targetRain = 0f; targetFog = 0f; targetLightIntensity = 1f;
                break;
            case WeatherType.Cloudy:
                targetCloud = 1f; targetRain = 0f; targetFog = 0f; targetLightIntensity = 0.8f;
                break;
            case WeatherType.LightRain:
                targetCloud = 0.8f; targetRain = 0.35f; targetFog = 0f; targetLightIntensity = 0.7f;
                break;
            case WeatherType.HeavyRain:
                targetCloud = 1f; targetRain = 1f; targetFog = 0f; targetLightIntensity = 0.45f;
                break;
            case WeatherType.LightFog:
                targetCloud = 0.5f; targetRain = 0f; targetFog = 0.4f; targetLightIntensity = 0.85f;
                break;
            case WeatherType.HeavyFog:
                targetCloud = 0.7f; targetRain = 0f; targetFog = 1f; targetLightIntensity = 0.6f;
                break;
        }

        StopCoroutineIfRunning();
        StartCoroutine(TransitionToWeather(targetCloud, targetRain, targetFog, targetLightIntensity));
        Debug.Log($"Weather -> {w} (cloud {targetCloud}, rain {targetRain}, fog {targetFog})");
    }

    Coroutine transitionCoroutine = null;
    void StopCoroutineIfRunning()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    IEnumerator TransitionToWeather(float targetCloud, float targetRain, float targetFog, float targetLightIntensity)
    {
        transitionCoroutine = null;

        float startCloud = cloudEffect != null ? cloudEffect.CurrentCloudiness() : 0f;
        float startRain = rainEffect != null ? rainEffect.CurrentIntensity() : 0f;
        float startFog = fogEffect != null ? fogEffect.CurrentFog() : 0f;
        float startLight = mainDirectionalLight != null ? mainDirectionalLight.intensity : 0f;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, transitionDuration);

        while (t < dur)
        {
            float a = t / dur;
            float c = Mathf.Lerp(startCloud, targetCloud, a);
            float r = Mathf.Lerp(startRain, targetRain, a);
            float f = Mathf.Lerp(startFog, targetFog, a);

            if (cloudEffect != null) cloudEffect.SetCloudiness(c);
            if (rainEffect != null) rainEffect.SetIntensity(r);
            if (fogEffect != null) fogEffect.SetFog(f);
            if (mainDirectionalLight != null && targetLightIntensity >= 0f)
                mainDirectionalLight.intensity = Mathf.Lerp(startLight, targetLightIntensity, a);

            t += Time.deltaTime;
            yield return null;
        }

        if (cloudEffect != null) cloudEffect.SetCloudiness(targetCloud);
        if (rainEffect != null) rainEffect.SetIntensity(targetRain);
        if (fogEffect != null) fogEffect.SetFog(targetFog);
        if (mainDirectionalLight != null && targetLightIntensity >= 0f)
            mainDirectionalLight.intensity = targetLightIntensity;

        transitionCoroutine = null;
        yield break;
    }

    // Public helper to apply immediately without transition (used at scene start if needed)
    public void ApplyWeatherImmediate(WeatherType w)
    {
        float targetCloud = 0f, targetRain = 0f, targetFog = 0f;
        float targetLightIntensity = mainDirectionalLight != null ? 1f : -1f;

        switch (w)
        {
            case WeatherType.Clear:
                targetCloud = 0f; targetRain = 0f; targetFog = 0f; targetLightIntensity = 1f;
                break;
            case WeatherType.Cloudy:
                targetCloud = 1f; targetRain = 0f; targetFog = 0f; targetLightIntensity = 0.8f;
                break;
            case WeatherType.LightRain:
                targetCloud = 0.8f; targetRain = 0.35f; targetFog = 0f; targetLightIntensity = 0.7f;
                break;
            case WeatherType.HeavyRain:
                targetCloud = 1f; targetRain = 1f; targetFog = 0f; targetLightIntensity = 0.45f;
                break;
            case WeatherType.LightFog:
                targetCloud = 0.5f; targetRain = 0f; targetFog = 0.4f; targetLightIntensity = 0.85f;
                break;
            case WeatherType.HeavyFog:
                targetCloud = 0.7f; targetRain = 0f; targetFog = 1f; targetLightIntensity = 0.6f;
                break;
        }

        StopCoroutineIfRunning();
        if (cloudEffect != null) cloudEffect.SetCloudinessImmediate(targetCloud);
        if (rainEffect != null) rainEffect.SetIntensityImmediate(targetRain);
        if (fogEffect != null) fogEffect.SetFogImmediate(targetFog);
        if (mainDirectionalLight != null && targetLightIntensity >= 0f) mainDirectionalLight.intensity = targetLightIntensity;
    }
}
