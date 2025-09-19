// CloudEffect.cs
using UnityEngine;

[ExecuteAlways]
public class CloudEffect : MonoBehaviour
{
    [Tooltip("Renderer for the cloud overlay (sprite renderer or mesh renderer).")]
    public Renderer cloudRenderer;
    public Color cloudColor = new Color(0.85f, 0.88f, 0.9f, 1f);
    [Range(0f, 1f)] public float maxAlpha = 0.85f;

    [Header("Visibility by look angle")]
    public Camera playerCamera; // default to Camera.main if null
    [Tooltip("Dot(camera.forward, worldUp) at which clouds start to appear (0 = horizon, 1 = straight up)")]
    public float lookDotMin = 0.05f;
    [Tooltip("Dot value that yields full visibility")]
    public float lookDotMax = 0.7f;
    [Tooltip("How fast alpha interpolates")]
    public float fadeSpeed = 4f;

    float targetAlpha = 0f;
    float currentAlpha = 0f;
    float cloudiness = 0f; // 0..1 global cloud cover (set by WeatherController)

    void Reset()
    {
        if (cloudRenderer == null) cloudRenderer = GetComponent<Renderer>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        float dot = Vector3.Dot(playerCamera.transform.forward.normalized, Vector3.up);
        float lookFactor = Mathf.InverseLerp(lookDotMin, lookDotMax, dot); // 0..1

        targetAlpha = Mathf.Clamp01(cloudiness) * lookFactor * maxAlpha;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        ApplyAlpha(currentAlpha);
    }

    void ApplyAlpha(float alpha)
    {
        if (cloudRenderer == null) return;

        var sr = cloudRenderer as SpriteRenderer;
        Color c = cloudColor;
        c.a = alpha;

        if (sr != null) sr.color = c;
        else
        {
            if (cloudRenderer.sharedMaterial != null && cloudRenderer.sharedMaterial.HasProperty("_Color"))
                cloudRenderer.sharedMaterial.color = c;
        }

        // Optionally toggle active to avoid raycasts/render cost when fully invisible
        bool active = alpha > 0.001f;
        if (cloudRenderer.gameObject.activeSelf != active) cloudRenderer.gameObject.SetActive(active);
    }

    // called by WeatherController
    public void SetCloudiness(float intensity)
    {
        cloudiness = Mathf.Clamp01(intensity);
    }

    public void SetCloudinessImmediate(float intensity)
    {
        cloudiness = Mathf.Clamp01(intensity);
        // force immediate update (without waiting for Update)
        if (playerCamera == null) playerCamera = Camera.main;
        float dot = playerCamera != null ? Vector3.Dot(playerCamera.transform.forward.normalized, Vector3.up) : 1f;
        float lookFactor = Mathf.InverseLerp(lookDotMin, lookDotMax, dot);
        currentAlpha = cloudiness * lookFactor * maxAlpha;
        ApplyAlpha(currentAlpha);
    }

    public float CurrentCloudiness()
    {
        return cloudiness;
    }
}
