using UnityEngine;

public class VRPassthroughBlender : MonoBehaviour
{
    [Header("Settings")]
    public Camera vrCamera;
    public Color vrBackgroundColor = Color.black;
    public float blendDuration = 1.0f;

    [Header("References")]
    public GameObject passthroughLayer;
    
    [Header("Debug")]
    public bool verboseLogging = true;

    private bool isInPassthrough = true;
    private bool isBlending = false;
    private float blendTimer = 0f;

    private Color startColor;
    private Color targetColor;
    private float startAlpha;
    private float targetAlpha;

    private CanvasGroup passthroughAlphaGroup;
    
    void Awake()
    {
        DebugLog("Awake aufgerufen");
    }

    void OnEnable()
    {
        DebugLog("OnEnable aufgerufen");
        
        // Überprüfen und erstellen der CanvasGroup
        if (passthroughLayer != null)
        {
            passthroughAlphaGroup = passthroughLayer.GetComponent<CanvasGroup>();
            
            if (passthroughAlphaGroup == null)
            {
                DebugLog("Keine CanvasGroup gefunden - erstelle neue");
                passthroughAlphaGroup = passthroughLayer.AddComponent<CanvasGroup>();
            }
            
            passthroughAlphaGroup.blocksRaycasts = false;
            DebugLog($"CanvasGroup initialisiert - Alpha: {passthroughAlphaGroup.alpha}");
        }
        else
        {
            DebugLog("Kein passthroughLayer zugewiesen!", LogLevel.Error);
        }
    }

    void Start()
    {
        DebugLog("Start aufgerufen");
        
        passthroughAlphaGroup.blocksRaycasts = false;

        passthroughLayer.SetActive(true);

        // Direkt mit Passthrough starten
        passthroughAlphaGroup.alpha = 1f;
        vrCamera.clearFlags = CameraClearFlags.SolidColor;
        vrCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent
        
        DebugLog($"Startbedingungen gesetzt: Alpha={passthroughAlphaGroup.alpha}, isInPassthrough={isInPassthrough}");
    }

   // Füge diese Methode hinzu, um den Blending-Status abzufragen
public bool IsCurrentlyBlending()
{
    return isBlending;
}

// Ändere die TogglePassthrough-Methode
public void TogglePassthrough()
{
    if (isBlending)
    {
        DebugLog("TogglePassthrough ignoriert - bereits im Blending-Prozess", LogLevel.Warning);
        return;
    }

    bool oldState = isInPassthrough;
    isInPassthrough = !isInPassthrough;
    
    DebugLog($"VRPassthroughBlender: Zustand wechselt von {oldState} zu {isInPassthrough}");
    
    blendTimer = 0f;
    isBlending = true;

    // Rest der Methode bleibt unverändert...
}

// Ändere die Update-Methode, um sicherzustellen, dass isBlending korrekt zurückgesetzt wird
void Update()
{
    if (isBlending)
    {
        blendTimer += Time.deltaTime;
        float t = Mathf.Clamp01(blendTimer / blendDuration);

        vrCamera.backgroundColor = Color.Lerp(startColor, targetColor, t);
        passthroughAlphaGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
        
        if (verboseLogging && ((t * 10) % 2) < 0.1f)
        {
            DebugLog($"Blending-Fortschritt: {t:F2}, Alpha={passthroughAlphaGroup.alpha:F2}");
        }

        if (t >= 1f)
        {
            isBlending = false;
            DebugLog($"Blending abgeschlossen: isInPassthrough={isInPassthrough}, Alpha={passthroughAlphaGroup.alpha:F2}");
        }
    }
}
    // Öffentliche Methode zur Abfrage des aktuellen Zustands
    public bool IsInPassthrough()
    {
        return isInPassthrough;
    }
    
    // Vereinfachtes Logging mit verschiedenen Levels
    enum LogLevel { Info, Debug, Warning, Error }
    
    private void DebugLog(string message, LogLevel level = LogLevel.Debug)
    {
        if (!verboseLogging && level == LogLevel.Debug) return;
        
        string prefix = "[VRPassthrough] ";
        
        // In die Unity-Konsole loggen
        switch (level)
        {
            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(prefix + message);
                break;
            case LogLevel.Error:
                UnityEngine.Debug.LogError(prefix + message);
                break;
            default:
                UnityEngine.Debug.Log(prefix + message);
                break;
        }
        
        // In das Debug-UI loggen, wenn verfügbar
        if (PerformanceDebugger.Instance != null)
        {
            switch (level)
            {
                case LogLevel.Info:
                    PerformanceDebugger.Info(prefix + message);
                    break;
                case LogLevel.Debug:
                    PerformanceDebugger.Debug(prefix + message);
                    break;
                case LogLevel.Warning:
                    PerformanceDebugger.Warning(prefix + message);
                    break;
                case LogLevel.Error:
                    PerformanceDebugger.Error(prefix + message);
                    break;
            }
        }
    }
}