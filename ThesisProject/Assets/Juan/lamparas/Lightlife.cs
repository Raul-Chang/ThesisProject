using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light lampLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float speed = 10f;

    private float targetIntensity;

    void Start()
    {
        if (lampLight == null)
            lampLight = GetComponent<Light>();

        targetIntensity = lampLight.intensity;
    }

    void Update()
    {
        // genero un valor aleatorio para intensidad
        targetIntensity = Random.Range(minIntensity, maxIntensity);

        // interpolo suavemente hacia esa intensidad
        lampLight.intensity = Mathf.Lerp(lampLight.intensity, targetIntensity, Time.deltaTime * speed);
    }
}

