using UnityEngine;
using UnityEngine.UI;

public class ParanoiaBorderEffect : MonoBehaviour
{
    public Image borderImage; // Assign UI Image overlay
    public float maxAlpha = 0.8f; // max visibility of border
    public float pulseSpeed = 2f; // heartbeat speed
    private ParanoiaController paranoia;

    void Start()
    {
        paranoia = FindObjectOfType<ParanoiaController>();
        if (borderImage != null)
        {
            var c = borderImage.color;
            c.a = 0f;
            borderImage.color = c;
        }
    }

    void Update()
    {
        if (borderImage == null || paranoia == null) return;

        float pNorm = paranoia.paranoiaValue / paranoia.maxParanoia;

        // Base alpha grows with paranoia
        float targetAlpha = maxAlpha * pNorm;

        // Add heartbeat pulse (sin wave between 0.8 and 1.2)
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.2f;
        float finalAlpha = targetAlpha * pulse;

        // Apply to image
        Color c = borderImage.color;
        c.a = Mathf.Clamp01(finalAlpha);
        borderImage.color = c;
    }
}
