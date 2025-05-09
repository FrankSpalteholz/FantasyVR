using UnityEngine;
using UnityEngine.UI;

public class PassthroughToggleHandler : MonoBehaviour
{
    public Toggle passthroughToggle;
    public VRPassthroughBlender passthroughBlender;
    
    [Header("Debug")]
    public bool verboseLogging = true;
    private int clickCounter = 0;
    
    private bool isInitialized = false;
    private bool isUpdatingToggle = false;
    
    void Start()
    {
        if (passthroughToggle != null && passthroughBlender != null)
        {
            // Entferne alle Listener
            passthroughToggle.onValueChanged.RemoveAllListeners();
            
            // Button-Komponente prüfen
            Button toggleButton = passthroughToggle.GetComponent<Button>();
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveAllListeners();
                toggleButton.onClick.AddListener(OnButtonDirectClick);
                DebugLog("Direkter Button-Listener hinzugefügt");
            }
            
            // Toggle-Event hinzufügen
            passthroughToggle.onValueChanged.AddListener(OnToggleValueChanged);
            
            isInitialized = true;
            
            // Synchronisiere mit aktuellem Zustand
            Invoke("InitialSync", 0.1f);
        }
    }
    
    void InitialSync()
    {
        if (!isInitialized) return;
        
        bool currentPassthroughState = passthroughBlender.IsInPassthrough();
        
        // Einfache Logik: Toggle an = Passthrough an, Toggle aus = VR aktiv
        isUpdatingToggle = true;
        passthroughToggle.SetIsOnWithoutNotify(currentPassthroughState);
        isUpdatingToggle = false;
        
        DebugLog($"Initiale Synchronisation: Toggle={currentPassthroughState}, Passthrough={currentPassthroughState}");
    }
    
    void OnButtonDirectClick()
    {
        clickCounter++;
        DebugLog($"*** DIREKTER BUTTON-KLICK #{clickCounter} ERKANNT ***");
        
        if (passthroughBlender == null) return;
        
        // Einfache Logik: Umschalten
        passthroughBlender.TogglePassthrough();
        
        // UI nach kurzer Verzögerung aktualisieren
        Invoke("SyncUIWithPassthrough", 0.1f);
    }
    
    void OnToggleValueChanged(bool isOn)
    {
        // Verhindere Rekursion
        if (isUpdatingToggle) 
        {
            DebugLog("Toggle-Event ignoriert (während Update)");
            return;
        }
        
        DebugLog($"Toggle-ValueChanged-Event: isOn={isOn}");
        
        if (passthroughBlender != null)
        {
            bool currentPassthroughState = passthroughBlender.IsInPassthrough();
            
            DebugLog($"Toggle ändern: Toggle={isOn}, aktuelle Passthrough={currentPassthroughState}");
            
            // Falls unterschiedlich, Passthrough umschalten
            if (currentPassthroughState != isOn)
            {
                DebugLog("Passthrough-Zustand wird umgeschaltet");
                passthroughBlender.TogglePassthrough();
            }
        }
    }
    
    void SyncUIWithPassthrough()
    {
        if (!isInitialized || passthroughBlender == null || passthroughToggle == null) return;
        
        bool currentPassthroughState = passthroughBlender.IsInPassthrough();
        
        if (passthroughToggle.isOn != currentPassthroughState)
        {
            isUpdatingToggle = true;
            passthroughToggle.SetIsOnWithoutNotify(currentPassthroughState);
            isUpdatingToggle = false;
            DebugLog($"UI aktualisiert: Toggle auf {currentPassthroughState} gesetzt");
        }
    }
    
    // Vereinfachtes Logging
    enum LogLevel { Info, Debug, Warning, Error }
    
    private void DebugLog(string message, LogLevel level = LogLevel.Debug)
    {
        if (!verboseLogging && level == LogLevel.Debug) return;
        
        string prefix = "[PassthroughToggler] ";
        
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