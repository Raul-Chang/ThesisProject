using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RedBorderEffect : MonoBehaviour
{
    [Tooltip("Material that uses Hidden/RedBorder shader")]
    public Material redBorderMaterial;

    [Range(0f, 1f)]
    public float intensity = 0f;     // base intensity 0..1 (set by controller)

    [Tooltip("Heartbeat pulse amplitude (0..1). 0.2 => ±20% variation")]
    public float pulseAmount = 0.18f;

    [Tooltip("Heartbeat/pulse speed in Hz")]
    public float pulseSpeed = 1.8f;

    [Tooltip("Enable runtime debug logs")]
    public bool debug = false;

    void Update()
    {
        if (redBorderMaterial == null) return;

        // heartbeat multiplier (centered at 1)
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
        float applied = Mathf.Clamp01(intensity) * pulse;

        // apply to shader
        redBorderMaterial.SetFloat("_Intensity", applied);

        if (debug)
            Debug.Log($"[RedBorderEffect] intensity={intensity:F3}, pulse={pulse:F3}, applied={applied:F3}");
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (redBorderMaterial != null)
            Graphics.Blit(src, dest, redBorderMaterial);
        else
            Graphics.Blit(src, dest);
    }

    // Public API for controller
    public void SetIntensity(float value) => intensity = Mathf.Clamp01(value);
    public void SetPulseSpeed(float hz) => pulseSpeed = Mathf.Max(0f, hz);
    public void SetPulseAmount(float amount) => pulseAmount = Mathf.Clamp01(amount);
}
