using UnityEngine;
using UnityEngine.UI;

public class Paranoia : MonoBehaviour
{
    [Header("Paranoia Settings")]
    public float paranoiaValue = 0f;
    public float maxParanoia = 20f;
    public float changeRate = 1f; // points per second

    [Header("UI")]
    public Slider paranoiaBar;

    [Header("Audio")]
    [Range(0f, 1f)] public float minVolume = 0.3f; // quietest
    [Range(0f, 1f)] public float maxVolume = 1f;   // loudest
    private AudioSource[] allAudioSources;

    private bool inLight = false;

    void Start()
    {
        // Setup slider values
        if (paranoiaBar != null)
        {
            paranoiaBar.minValue = 0f;
            paranoiaBar.maxValue = maxParanoia;
            paranoiaBar.value = paranoiaValue;
        }

        // Grab all audio sources in the scene
        allAudioSources = FindObjectsOfType<AudioSource>();
    }

    void Update()
    {
        // Update paranoia value
        if (inLight)
            paranoiaValue -= changeRate * Time.deltaTime;
        else
            paranoiaValue += changeRate * Time.deltaTime;

        paranoiaValue = Mathf.Clamp(paranoiaValue, 0f, maxParanoia);

        // Update UI
        if (paranoiaBar != null)
            paranoiaBar.value = paranoiaValue;

        // Update audio volume
        UpdateAudioVolume();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AreaLight"))
            inLight = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("AreaLight"))
            inLight = false;
    }

    private void UpdateAudioVolume()
    {

        float paranoiaPercent = paranoiaValue / maxParanoia;

        float targetVolume = Mathf.Lerp(minVolume, maxVolume, paranoiaPercent);

        foreach (AudioSource source in allAudioSources)
        {
            if (source != null)
                source.volume = targetVolume;
        }
    }
}
