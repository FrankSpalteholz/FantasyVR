using UnityEngine;

public class VRPassthroughBlender : MonoBehaviour
{
    [Header("Settings")]
    public Camera vrCamera;
    public Color vrBackgroundColor = Color.black;
    public float blendDuration = 1.0f;

    [Header("References")]
    public GameObject passthroughLayer;

    private bool isInPassthrough = true; // 👉 Startzustand: Passthrough aktiv
    private bool isBlending = false;
    private float blendTimer = 0f;

    private Color startColor;
    private Color targetColor;
    private float startAlpha;
    private float targetAlpha;

    private CanvasGroup passthroughAlphaGroup;

    void Start()
    {
        passthroughAlphaGroup = passthroughLayer.GetComponent<CanvasGroup>();
        if (passthroughAlphaGroup == null)
            passthroughAlphaGroup = passthroughLayer.AddComponent<CanvasGroup>();

        passthroughAlphaGroup.blocksRaycasts = false;

        passthroughLayer.SetActive(true);

        // Direkt mit Passthrough starten
        passthroughAlphaGroup.alpha = 1f;
        vrCamera.clearFlags = CameraClearFlags.SolidColor;
        vrCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent
    }

    void Update()
    {
        if (isBlending)
        {
            blendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blendTimer / blendDuration);

            vrCamera.backgroundColor = Color.Lerp(startColor, targetColor, t);
            passthroughAlphaGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (t >= 1f)
            {
                isBlending = false;
                // optional: passthroughLayer.SetActive(isInPassthrough);
            }
        }
    }

    public void TogglePassthrough()
    {
        if (isBlending) return;

        isInPassthrough = !isInPassthrough;
        blendTimer = 0f;
        isBlending = true;

        if (isInPassthrough)
        {
            startColor = vrCamera.backgroundColor;
            targetColor = new Color(0, 0, 0, 0);

            startAlpha = 0f;
            targetAlpha = 1f;
        }
        else
        {
            startColor = vrCamera.backgroundColor;
            targetColor = vrBackgroundColor;

            startAlpha = 1f;
            targetAlpha = 0f;
        }
    }
}
