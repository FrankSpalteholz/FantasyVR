using UnityEngine;
using Meta.XR.BuildingBlocks;
//using Meta.XR.All; // Wichtiger Namespace für Passthrough

public class VRPassthroughBlender : MonoBehaviour
{
    [Header("Settings")]
    public Camera vrCamera; // Deine Hauptkamera
    public Color vrBackgroundColor = Color.black; // VR Hintergrundfarbe
    public float blendDuration = 1.0f; // Sekunden fürs Blending

    [Header("References")]
    public GameObject passthroughLayer; // Das Objekt, das den Passthrough Layer steuert (z.B. OVRPassthroughLayer)

    private bool isInPassthrough = false;
    private bool isBlending = false;
    private float blendTimer = 0f;

    private Color startColor;
    private Color targetColor;
    private float startAlpha;
    private float targetAlpha;

    private CanvasGroup passthroughAlphaGroup;

    void Start()
    {
        // Wir erstellen eine CanvasGroup für smoothes Alpha-Überblenden
        passthroughAlphaGroup = passthroughLayer.GetComponent<CanvasGroup>();
        if (passthroughAlphaGroup == null)
        {
            passthroughAlphaGroup = passthroughLayer.AddComponent<CanvasGroup>();
        }

        // Startzustand: Kein Passthrough
        passthroughLayer.SetActive(false);
        passthroughAlphaGroup.alpha = 0f;
        vrCamera.clearFlags = CameraClearFlags.SolidColor;
        vrCamera.backgroundColor = vrBackgroundColor;
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
            }
        }
    }

    // Diese Methode kannst du vom Poke-Event aufrufen
    public void TogglePassthrough()
    {
        if (isBlending) return;

        isInPassthrough = !isInPassthrough;
        blendTimer = 0f;
        isBlending = true;

        if (isInPassthrough)
        {
            passthroughLayer.SetActive(true);
            startColor = vrCamera.backgroundColor;
            targetColor = new Color(0,0,0,0); // Transparent-Schwarz oder ganz schwarz

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
