using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Lightlife : MonoBehaviour
{
    [Header("Luz (flicker normal)")]
    public Light lampLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float flickerSpeed = 10f;

    [Header("Apagones (expuesto en Inspector)")]
    public bool blackoutEnabled = true;                 // activar/desactivar apagones aleatorios
    public Vector2 blackoutInterval = new Vector2(8f, 20f); // rango aleatorio entre apagones (seg)
    [Min(0f)] public float blackoutFadeOut = 0.15f;     // tiempo de fade a negro
    [Min(0f)] public float blackoutOffSeconds = 0.30f;  // tiempo apagada completamente
    [Min(0f)] public float blackoutFadeIn = 0.25f;      // tiempo de fade al encender

    [Header("Eventos (opcional)")]
    public UnityEvent onBlackoutStarted; // se dispara al quedar en negro
    public UnityEvent onBlackoutEnded;   // se dispara al volver a encender

    // --- Estado interno ---
    private float targetIntensity;
    private float nextBlackoutTime;
    private float blackoutTimer;
    private float cachedBeforeBlackout;

    private enum State { Normal, FadeOut, Off, FadeIn }
    private State state = State.Normal;

    public float CurrentIntensity => lampLight ? lampLight.intensity : 0f; // lectura pública

    void Reset()
    {
        lampLight = GetComponent<Light>();
    }

    void Start()
    {
        if (lampLight == null) lampLight = GetComponent<Light>();
        if (lampLight == null) { Debug.LogError("[Lightlife] No hay Light asignada."); enabled = false; return; }

        targetIntensity = lampLight.intensity;
        ScheduleNextBlackout();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        switch (state)
        {
            case State.Normal:
                // Flicker normal
                targetIntensity = Random.Range(minIntensity, maxIntensity);
                lampLight.intensity = Mathf.Lerp(lampLight.intensity, targetIntensity, dt * flickerSpeed);

                // ¿toca apagón?
                if (blackoutEnabled && Time.time >= nextBlackoutTime)
                    BeginFadeOut();
                break;

            case State.FadeOut:
                blackoutTimer += dt;
                float tOut = blackoutFadeOut <= 0f ? 1f : Mathf.Clamp01(blackoutTimer / blackoutFadeOut);
                lampLight.intensity = Mathf.Lerp(cachedBeforeBlackout, 0f, tOut);
                if (tOut >= 1f) { blackoutTimer = 0f; state = State.Off; onBlackoutStarted?.Invoke(); }
                break;

            case State.Off:
                blackoutTimer += dt;
                lampLight.intensity = 0f;
                if (blackoutTimer >= blackoutOffSeconds)
                {
                    blackoutTimer = 0f;
                    targetIntensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
                    state = State.FadeIn;
                }
                break;

            case State.FadeIn:
                blackoutTimer += dt;
                float tIn = blackoutFadeIn <= 0f ? 1f : Mathf.Clamp01(blackoutTimer / blackoutFadeIn);
                lampLight.intensity = Mathf.Lerp(0f, targetIntensity, tIn);
                if (tIn >= 1f)
                {
                    blackoutTimer = 0f;
                    state = State.Normal;
                    onBlackoutEnded?.Invoke();
                    ScheduleNextBlackout();
                }
                break;
        }
    }

    void BeginFadeOut()
    {
        cachedBeforeBlackout = Mathf.Max(0f, lampLight.intensity);
        blackoutTimer = 0f;
        state = State.FadeOut;
    }

    void ScheduleNextBlackout()
    {
        float a = Mathf.Min(blackoutInterval.x, blackoutInterval.y);
        float b = Mathf.Max(blackoutInterval.x, blackoutInterval.y);
        if (b <= 0f) { nextBlackoutTime = Mathf.Infinity; return; }
        nextBlackoutTime = Time.time + Random.Range(Mathf.Max(0.01f, a), b);
    }

    void OnValidate()
    {
        if (maxIntensity < minIntensity) maxIntensity = minIntensity;
        if (blackoutInterval.x < 0f) blackoutInterval.x = 0f;
        if (blackoutInterval.y < 0f) blackoutInterval.y = 0f;
    }

    // =================== API PÚBLICA ===================

    [ContextMenu("Apagar ahora (blackout inmediato)")]
    public void TriggerBlackoutNow()
    {
        if (state == State.Normal) BeginFadeOut();
    }

    // Cambia los tiempos de apagado (podés llamarlo desde otro script o UI)
    public void SetBlackoutTimings(float fadeOutSeconds, float offSeconds, float fadeInSeconds)
    {
        blackoutFadeOut     = Mathf.Max(0f, fadeOutSeconds);
        blackoutOffSeconds  = Mathf.Max(0f, offSeconds);
        blackoutFadeIn      = Mathf.Max(0f, fadeInSeconds);
    }

    // Activa/desactiva los apagones aleatorios
    public void SetBlackoutEnabled(bool enabled)
    {
        blackoutEnabled = enabled;
        if (!enabled) nextBlackoutTime = Mathf.Infinity;
        else if (state == State.Normal) ScheduleNextBlackout();
    }

    // Cambia el intervalo aleatorio entre apagones
    public void SetBlackoutInterval(float minSeconds, float maxSeconds)
    {
        blackoutInterval = new Vector2(minSeconds, maxSeconds);
        if (state == State.Normal && blackoutEnabled) ScheduleNextBlackout();
    }

    // Cancela un apagón y deja la luz encendida a cierta intensidad (opcional)
    public void ForceLightOn(float intensity = -1f)
    {
        state = State.Normal;
        blackoutTimer = 0f;
        ScheduleNextBlackout();
        if (intensity >= 0f) lampLight.intensity = intensity;
    }
}
