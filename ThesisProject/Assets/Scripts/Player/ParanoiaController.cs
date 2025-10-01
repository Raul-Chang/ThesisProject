using UnityEngine;
using UnityEngine.UI;

public class ParanoiaController : MonoBehaviour
{
    [Header("Paranoia Settings")]
    [Range(0f, 100f)] public float paranoiaValue = 0f;
    public float maxParanoia = 100f;
    public float changeRate = 5f;

    [Header("UI")]
    public Slider paranoiaBar;
    public Image paranoiaFillImage;
    public Color colorGreen = Color.green;
    public Color colorYellow = Color.yellow;
    public Color colorOrange = new Color(1f, 0.5f, 0f);
    public Color colorRed = Color.red;
    public float colorLerpSpeed = 8f;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource enemySource;
    public AudioSource playerSource;

    [Header("Paranoia stage audios (4 clips)")]
    public AudioSource[] paranoiaStageAudios = new AudioSource[4];
    public float paranoiaStageMaxVolume = 1f;
    public float stageFadeSpeed = 6f;

    [Header("Fisheye effect")]
    public FisheyeEffect fisheyeEffect;
    public float maxFisheyeEffect = 0.35f;
    public float fisheyeLerpSpeed = 4f;

    [Header("Red border effect (post-effect)")]
    public RedBorderEffect redBorderEffect;

    [Tooltip("Minimum intensity of red border (after 25% paranoia)")]
    public float minBorderIntensity = 0.2f;

    [Tooltip("Maximum intensity of red border (at 100% paranoia)")]
    public float maxBorderIntensity = 0.8f;

    [Tooltip("How quickly border intensity updates")]
    public float borderLerpSpeed = 4f;

    [Header("Heartbeat / pulse speed tuning")]
    public float minPulseSpeed = 0.8f;
    public float maxPulseSpeed = 2.2f;
    public float minPulseAmount = 0.06f;
    public float maxPulseAmount = 0.22f;

    private bool inLight = false;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (paranoiaBar != null)
        {
            paranoiaBar.minValue = 0f;
            paranoiaBar.maxValue = maxParanoia;
            paranoiaBar.value = paranoiaValue;

            if (paranoiaFillImage == null)
            {
                Transform fill = paranoiaBar.transform.Find("Fill Area/Fill");
                if (fill != null) paranoiaFillImage = fill.GetComponent<Image>();
            }
        }

        playerMovement = FindObjectOfType<PlayerMovement>();

        foreach (var a in paranoiaStageAudios)
        {
            if (a == null) continue;
            a.loop = true;
            a.playOnAwake = false;
            a.volume = 0.5f;
        }
    }

    void Update()
    {
        // paranoia increase/decrease
        paranoiaValue += (inLight ? -changeRate : changeRate) * Time.deltaTime;
        paranoiaValue = Mathf.Clamp(paranoiaValue, 0f, maxParanoia);

        if (paranoiaBar != null) paranoiaBar.value = paranoiaValue;

        UpdateBarColor();
        UpdateGlobalAudio();
        UpdateStageAudios();
        UpdateFisheye();
        UpdateRedBorder();
        UpdatePlayerRunRestriction();
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("AreaLight")) inLight = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("AreaLight")) inLight = false; }

    // ---------------- UI ----------------
    private void UpdateBarColor()
    {
        if (paranoiaFillImage == null) return;

        Color targetColor = colorGreen;
        if (paranoiaValue <= 25f) targetColor = colorGreen;
        else if (paranoiaValue <= 50f) targetColor = colorYellow;
        else if (paranoiaValue <= 75f) targetColor = colorOrange;
        else targetColor = colorRed;

        paranoiaFillImage.color = Color.Lerp(paranoiaFillImage.color, targetColor, Time.deltaTime * colorLerpSpeed);
    }

    // ---------------- Audio ----------------
    private void UpdateGlobalAudio()
    {
        float pNorm = paranoiaValue / maxParanoia;
        if (bgmSource != null) bgmSource.volume = Mathf.Lerp(0.5f, 1f, pNorm);
        if (enemySource != null) enemySource.volume = Mathf.Lerp(0.5f, 1f, pNorm);
        if (playerSource != null) playerSource.volume = Mathf.Lerp(0.5f, 1f, pNorm);
    }

    private void UpdateStageAudios()
    {
        float pNorm = paranoiaValue / maxParanoia;

        for (int i = 0; i < paranoiaStageAudios.Length; i++)
        {
            var a = paranoiaStageAudios[i];
            if (a == null) continue;

            float threshold = (i + 1) * 0.25f;
            float targetVol = 0.5f;

            if (pNorm >= threshold)
            {
                float stageProgress = (pNorm - threshold) / (1f - threshold);
                targetVol = Mathf.Lerp(0.5f, paranoiaStageMaxVolume, stageProgress);
            }

            if (targetVol > 0.5f)
            {
                if (!a.isPlaying) a.Play();
                a.volume = Mathf.Lerp(a.volume, targetVol, Time.deltaTime * stageFadeSpeed);
            }
            else
            {
                a.volume = Mathf.Lerp(a.volume, 0.5f, Time.deltaTime * stageFadeSpeed);
                if (a.volume <= 0.51f && a.isPlaying) a.Stop();
            }
        }
    }

    // ---------------- Fisheye ----------------
    private void UpdateFisheye()
    {
        if (fisheyeEffect == null) return;

        float pNorm = paranoiaValue / maxParanoia;
        float target = maxFisheyeEffect * pNorm;
        float next = Mathf.Lerp(fisheyeEffect.intensity, target, Time.deltaTime * fisheyeLerpSpeed);
        fisheyeEffect.SetIntensity(next);
    }

    // ---------------- Red border ----------------
    private void UpdateRedBorder()
    {
        if (redBorderEffect == null) return;

        float pNorm = paranoiaValue / maxParanoia;

        float targetIntensity = 0f;

        // Border only appears after 25% paranoia
        if (pNorm >= 0.25f)
        {
            float adjustedNorm = (pNorm - 0.25f) / 0.75f; // remap 25–100% → 0–1
            targetIntensity = Mathf.Lerp(minBorderIntensity, maxBorderIntensity, adjustedNorm);
        }

        float nextIntensity = Mathf.Lerp(redBorderEffect.intensity, targetIntensity, Time.deltaTime * borderLerpSpeed);
        redBorderEffect.SetIntensity(nextIntensity);

        // Pulse grows with paranoia
        float targetPulseSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, pNorm);
        float targetPulseAmount = Mathf.Lerp(minPulseAmount, maxPulseAmount, pNorm);

        redBorderEffect.SetPulseSpeed(targetPulseSpeed);
        redBorderEffect.SetPulseAmount(targetPulseAmount);
    }

    // ---------------- Player run lock ----------------
    private void UpdatePlayerRunRestriction()
    {
        if (playerMovement != null)
            playerMovement.runLockedByParanoia = (paranoiaValue >= 75f);
    }
}
