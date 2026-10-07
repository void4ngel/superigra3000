using UnityEngine;

[ExecuteAlways]
public class SkyboxAnimator : MonoBehaviour
{
    [Tooltip("Skybox Material")]
    public Material skyboxMaterial;

    [Tooltip("Redactor animations")]
    public bool animateInEditor = false;

    void Update()
    {
        if (skyboxMaterial == null) return;
        float t = 0f;
        if (Application.isPlaying || animateInEditor)
            t = Time.time;

        skyboxMaterial.SetFloat("_AnimTime", t);
    }
}