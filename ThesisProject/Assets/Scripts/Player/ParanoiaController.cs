using UnityEngine;
using UnityEngine.UI;

public class ParanoiaController : MonoBehaviour
{
    [Header("Paranoia Settings")]
    public float paranoiaValue = 0f;
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

    [Header("Global audio (explicit sources)")]
    public AudioSource bgmSource;
    public float bgmMinVolume = 0.2f, bgmMaxVolume = 1f;
    public AudioSource enemySource;
    public float enemyMinVolume = 0.1f, enemyMaxVolume = 1f;
    public AudioSource playerSource;
    public float playerMinVolume = 0f, playerMaxVolume = 0.8f;

    [Header("Paranoia stage audios (4 clips)")]
    public AudioSource[] paranoiaStageAudios = new AudioSource[4];
    public float paranoiaStageMaxVolume = 1f;
    public float stageFadeSpeed = 6f;

    [Header("Fisheye effect")]
    public FisheyeEffect fisheyeEffect;
    public float stage1Fisheye = 0.08f;
    public float stage2Fisheye = 0.18f;
    public float stage3Fisheye = 0.32f;
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
            a.volume = 0f;
        }
    }

    void Update()
    {
        // Paranoia increase/decrease
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
        float pNorm = paranoiaValue / (maxParanoia );
        if (bgmSource != null) bgmSource.volume = Mathf.Lerp(bgmMinVolume, bgmMaxVolume, pNorm);
        if (enemySource != null) enemySource.volume = Mathf.Lerp(enemyMinVolume, enemyMaxVolume, pNorm);
        if (playerSource != null) playerSource.volume = Mathf.Lerp(playerMinVolume, playerMaxVolume, pNorm);
    }

    private void UpdateStageAudios()
    {
        float pNorm = paranoiaValue / maxParanoia;

        for (int i = 0; i < paranoiaStageAudios.Length; i++)
        {
            var a = paranoiaStageAudios[i];
            if (a == null) continue;

            float threshold = (i + 1) * 0.25f; // 25, 50, 75, 100
            float targetVol = 0f;

            if (pNorm >= threshold)
            {
                targetVol = Mathf.Lerp(0f, paranoiaStageMaxVolume, pNorm);
            }

            if (targetVol > 0.001f)
            {
                if (!a.isPlaying) a.Play();
                a.volume = Mathf.Lerp(a.volume, targetVol, Time.deltaTime * stageFadeSpeed);
            }
            else
            {
                a.volume = Mathf.Lerp(a.volume, 0f, Time.deltaTime * stageFadeSpeed);
                if (a.volume <= 0.001f && a.isPlaying) a.Stop();
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

        float p = paranoiaValue / maxParanoia;
        float target = 0f;
        if (p >= 0.25f && p < 0.5f) target = stage1Fisheye;
        else if (p >= 0.5f && p < 0.75f) target = stage2Fisheye;
        else if (p >= 0.75f) target = stage3Fisheye;

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
}
