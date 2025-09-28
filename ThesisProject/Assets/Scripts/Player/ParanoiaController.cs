using UnityEngine;
using UnityEngine.UI;

public class ParanoiaController : MonoBehaviour
{
    [Header("Paranoia Settings")]
    [Range(0f, 100f)] public float paranoiaValue = 0f;
    public float maxParanoia = 100f;
    [Tooltip("How fast paranoia changes per second.")]
    public float changeRate = 5f;

    [Header("UI")]
    public Slider paranoiaBar;
    public Image paranoiaFillImage;
    public Color colorGreen = Color.green;
    public Color colorYellow = Color.yellow;
    public Color colorOrange = new Color(1f, 0.5f, 0f);
    public Color colorRed = Color.red;
    public float colorLerpSpeed = 8f;

    [Header("Global audio (explicit sources)")]
    public AudioSource bgmSource;
    public AudioSource enemySource;
    public AudioSource playerSource;

    [Header("Paranoia stage audios (4 clips)")]
    [Tooltip("Index 0 = activates at 25%, Index 1 = 50%, Index 2 = 75%, Index 3 = 100%")]
    public AudioSource[] paranoiaStageAudios = new AudioSource[4];
    [Tooltip("Maximum paranoia stage volume at full paranoia (should be 1.0).")]
    public float paranoiaStageMaxVolume = 1f;
    public float stageFadeSpeed = 6f;

    [Header("Fisheye effect")]
    public FisheyeEffect fisheyeEffect;
    [Tooltip("Maximum fisheye distortion at full paranoia (0..1).")]
    public float maxFisheyeEffect = 0.35f;
    public float fisheyeLerpSpeed = 4f;

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

        // Prepare stage audios
        foreach (var a in paranoiaStageAudios)
        {
            if (a == null) continue;
            a.loop = true;
            a.playOnAwake = false;
            a.volume = 0.5f; // start at min volume
        }
    }

    void Update()
    {
        // Update paranoia
        paranoiaValue += (inLight ? -changeRate : changeRate) * Time.deltaTime;
        paranoiaValue = Mathf.Clamp(paranoiaValue, 0f, maxParanoia);

        if (paranoiaBar != null) paranoiaBar.value = paranoiaValue;

        UpdateBarColor();
        UpdateGlobalAudio();
        UpdateStageAudios();
        UpdateFisheye();
        UpdatePlayerRunRestriction();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AreaLight")) inLight = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AreaLight")) inLight = false;
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

            float threshold = (i + 1) * 0.25f; // 0.25, 0.5, 0.75, 1.0
            float targetVol = 0.5f; // min volume is now 0.5

            if (pNorm >= threshold)
            {
                // volume builds gradually from 0.5 → 1 across threshold..100%
                float stageProgress = (pNorm - threshold) / (1f - threshold);
                targetVol = Mathf.Lerp(0.5f, paranoiaStageMaxVolume, stageProgress);
            }

            if (targetVol > 0.5f + 0.001f)
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

    // ---------------- Fisheye ----------------
    private void UpdateFisheye()
    {
        if (fisheyeEffect == null) return;

        float pNorm = paranoiaValue / maxParanoia;
        float target = maxFisheyeEffect * pNorm;
        float next = Mathf.Lerp(fisheyeEffect.intensity, target, Time.deltaTime * fisheyeLerpSpeed);
        fisheyeEffect.SetIntensity(next);
    }

    // ---------------- Player run lock ----------------
    private void UpdatePlayerRunRestriction()
    {
        if (playerMovement != null)
        {
            playerMovement.runLockedByParanoia = (paranoiaValue >= 75f);
        }
    }

    // ---------------- Public API ----------------
    public void AddParanoia(float amount)
    {
        paranoiaValue = Mathf.Clamp(paranoiaValue + amount, 0f, maxParanoia);
    }

    public void SetParanoia(float value)
    {
        paranoiaValue = Mathf.Clamp(value, 0f, maxParanoia);
    }

    public void ResetParanoia()
    {
        paranoiaValue = 0f;
        foreach (var a in paranoiaStageAudios)
        {
            if (a == null) continue;
            a.volume = 0.5f;
            if (a.isPlaying) a.Stop();
        }
        if (bgmSource != null) bgmSource.volume = 0.5f;
        if (enemySource != null) enemySource.volume = 0.5f;
        if (playerSource != null) playerSource.volume = 0.5f;
        if (paranoiaFillImage != null) paranoiaFillImage.color = colorGreen;
        if (fisheyeEffect != null) fisheyeEffect.SetIntensity(0f);
    }
}
