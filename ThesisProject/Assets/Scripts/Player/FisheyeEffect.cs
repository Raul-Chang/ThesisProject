using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FisheyeEffect : MonoBehaviour
{
    [Tooltip("Material using the Hidden/Fisheye shader")]
    public Material fisheyeMaterial;

    [Range(0f, 1f)]
    public float intensity = 0f;

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (fisheyeMaterial != null)
        {
            fisheyeMaterial.SetFloat("_Intensity", intensity);
            Graphics.Blit(src, dest, fisheyeMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
