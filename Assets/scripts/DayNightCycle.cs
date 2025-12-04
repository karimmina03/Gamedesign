using UnityEngine;
using UnityEngine.Rendering; // for RenderSettings

public class DayNightCycle : MonoBehaviour
{
    public Light sun; // assign your Directional Light
    public float dayDuration = 120f; // seconds for a full 24h cycle
    private float rotationSpeed;

    [Header("Lighting Settings")]
    public Color dayAmbientColor = Color.white * 0.8f;
    public Color nightAmbientColor = Color.blue * 0.05f;
    public float nightSkyboxExposure = 0.2f;
    public float daySkyboxExposure = 1.2f;
    
    void Start()
    {
        if (sun == null) sun = FindObjectOfType<Light>();
        rotationSpeed = 360f / dayDuration;
    }

    void Update()
    {
        // Rotate the sun smoothly
        sun.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);

        // Compute day/night blend factor based on sun angle
        float angle = sun.transform.eulerAngles.x;
        float t = Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad) * 0.5f + 0.5f);

        // Sunlight intensity + color
        sun.intensity = Mathf.Lerp(0.05f, 1.2f, t);
        sun.color = Color.Lerp(Color.red * 0.6f, Color.white, t);

        // Ambient lighting color
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, t);

        // Optional: adjust skybox exposure if one exists
        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Exposure"))
        {
            RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(nightSkyboxExposure, daySkyboxExposure, t));
        }

        // To make sure Unity updates the lighting
        DynamicGI.UpdateEnvironment();
    }
}
