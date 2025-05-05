using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimplePrefabToggler : MonoBehaviour
{
    [System.Serializable]
    public class TogglePrefabPair
    {
        public Toggle toggle;
        public GameObject prefab;
        [Tooltip("Should this prefab be visible at start?")]
        public bool activeByDefault = false;
    }
    
    [Header("Toggle-Prefab Pairs")]
    public List<TogglePrefabPair> togglePrefabPairs = new List<TogglePrefabPair>();
    
    void Awake()
    {
        // Wichtig: Setze alle Prefabs auf inaktiv bevor irgendetwas anderes passiert
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.prefab != null)
            {
                // Sicherheitshalber deaktivieren, damit nichts aufblitzt
                pair.prefab.SetActive(false);
            }
        }
    }
    
    void Start()
    {
        // Alle Event-Listener von Toggles entfernen, um während der Einrichtung keine Events auszulösen
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.toggle != null)
            {
                pair.toggle.onValueChanged.RemoveAllListeners();
            }
        }
        
        // Prefabs auf ihre Standardzustände initialisieren
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.prefab != null && pair.toggle != null)
            {
                // KORRIGIERT: Prefab-Zustand entsprechend activeByDefault setzen
                bool shouldBeActive = pair.activeByDefault;
                
                // Debug-Log zur Fehlerbehebung
                Debug.Log($"Setting {pair.prefab.name} to {(shouldBeActive ? "active" : "inactive")} (activeByDefault: {pair.activeByDefault})");
                
                // Prefab-Zustand setzen
                pair.prefab.SetActive(shouldBeActive);
                
                // Toggle-Zustand setzen ohne Events auszulösen
                pair.toggle.SetIsOnWithoutNotify(shouldBeActive);
            }
        }
        
        // Jetzt Event-Listener hinzufügen, nachdem alles eingerichtet ist
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.toggle != null && pair.prefab != null)
            {
                // Lokale Variable verwenden, um Closure-Probleme zu vermeiden
                var localPair = pair;
                pair.toggle.onValueChanged.AddListener((isOn) => 
                {
                    Debug.Log($"Toggle {localPair.toggle.name} changed to {isOn}");
                    localPair.prefab.SetActive(isOn);
                });
            }
        }
    }
    
    // Öffentliche Methode zum Abfragen des Zustands eines Prefabs
    public bool GetPrefabState(string toggleName)
    {
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.toggle != null && pair.toggle.name == toggleName)
            {
                return pair.toggle.isOn;
            }
        }
        return false;
    }
    
    // Öffentliche Methode zum Setzen des Zustands eines Prefabs
    public void SetPrefabState(string toggleName, bool state)
    {
        foreach (var pair in togglePrefabPairs)
        {
            if (pair.toggle != null && pair.toggle.name == toggleName)
            {
                pair.toggle.isOn = state;
                break;
            }
        }
    }
}