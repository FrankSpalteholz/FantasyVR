using UnityEngine;

public class MaterialColorPulse : MonoBehaviour
{
    [Header("Material-Einstellungen")]
    [Tooltip("Das Material, dessen Farbe pulsieren soll")]
    [SerializeField] private Material targetMaterial;

    [Tooltip("Nutze Renderer statt Material-Referenz (überschreibt Material-Referenz)")]
    [SerializeField] private bool useRenderer = false;

    [Tooltip("Shader-Property-Name für die Farbe (meist '_BaseColor' oder '_Color')")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Farb-Einstellungen")]
    [Tooltip("Grundfarbe (Farbton und Sättigung bleiben erhalten)")]
    [SerializeField] private Color baseColor = Color.white;

    [Tooltip("Minimale Helligkeit (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float minBrightness = 0.5f;

    [Tooltip("Maximale Helligkeit (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float maxBrightness = 1.0f;

    [Header("Puls-Einstellungen")]
    [Tooltip("Pulsgeschwindigkeit")]
    [SerializeField] private float pulseSpeed = 1.0f;

    [Tooltip("Smooth-Faktor für den Übergang (1 = Sinus, höhere Werte = steilere Kurve)")]
    [Range(1f, 10f)]
    [SerializeField] private float smoothFactor = 1.0f;

    // Private Variablen
    private Renderer rendererComponent;
    private Color originalColor;
    private float h, s, v; // Farbwerte in HSV

    private void Start()
    {
        // Prüfe, ob wir den Renderer des Objekts verwenden sollen
        if (useRenderer)
        {
            rendererComponent = GetComponent<Renderer>();
            if (rendererComponent != null)
            {
                targetMaterial = rendererComponent.material;
            }
            else
            {
                Debug.LogError("Kein Renderer gefunden, obwohl useRenderer aktiviert ist!");
                return;
            }
        }

        // Prüfe, ob ein Material vorhanden ist
        if (targetMaterial == null)
        {
            Debug.LogError("Kein Material zugewiesen!");
            return;
        }

        // Speichere die aktuelle Farbe, wenn baseColor nicht gesetzt ist
        if (baseColor == Color.white)
        {
            originalColor = targetMaterial.GetColor(colorPropertyName);
            baseColor = originalColor;
        }

        // Konvertiere die Basisfarbe in HSV
        Color.RGBToHSV(baseColor, out h, out s, out v);
    }

    private void Update()
    {
        if (targetMaterial == null)
            return;

        // Berechne den Puls-Faktor mit smoothem Übergang
        float pulse = Mathf.Pow(0.5f * (1.0f + Mathf.Sin(Time.time * pulseSpeed)), smoothFactor);
        
        // Interpoliere zwischen min und max Helligkeit
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, pulse);
        
        // Erstelle neue Farbe mit originalen H und S, aber variierendem V (Helligkeit)
        Color newColor = Color.HSVToRGB(h, s, brightness);
        
        // Setze die neue Farbe im Material
        targetMaterial.SetColor(colorPropertyName, newColor);
    }

    // Optional: Setze die Farbe beim Beenden zurück
    private void OnDestroy()
    {
        if (targetMaterial != null && originalColor != Color.clear)
        {
            targetMaterial.SetColor(colorPropertyName, originalColor);
        }
    }

    // Public-Methode, um die Pulsfarbe auch zur Laufzeit ändern zu können
    public void SetBaseColor(Color newBaseColor)
    {
        baseColor = newBaseColor;
        Color.RGBToHSV(baseColor, out h, out s, out v);
    }
}