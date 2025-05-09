using System.Collections;
using UnityEngine;

public class TeleportRayAutoDisabler : MonoBehaviour
{
    [Header("Teleport-Komponenten")]
    [Tooltip("Der Locomotor aus deiner Szene")]
    [SerializeField] private GameObject locomotor;
    
    [Tooltip("Die TeleportMicrogestureInteractor-Komponente")]
    [SerializeField] private GameObject teleportMicrogestureInteractor;
    
    [Tooltip("Das ArcVisual GameObject unter dem TeleportMicrogestureInteractor")]
    [SerializeField] private GameObject arcVisual;
    
    [Header("Einstellungen")]
    [Tooltip("Kurze Verzögerung nach der Teleportation, bevor der Ray deaktiviert wird")]
    [SerializeField] private float deactivationDelay = 0.1f;
    
    // Speichern des ursprünglichen Status
    private bool wasTeleportInteractorEnabled;
    private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportProvider;
    
    private void Start()
    {
        // Finde die notwendigen Komponenten, falls sie nicht manuell zugewiesen wurden
        if (locomotor == null)
        {
            locomotor = GameObject.Find("Locomotor");
        }
        
        if (teleportMicrogestureInteractor == null)
        {
            // Suche nach dem TeleportMicrogestureInteractor in der Hierarchie
            var rightHand = GameObject.Find("RightHand");
            if (rightHand != null)
            {
                var handInteractors = rightHand.transform.Find("HandInteractorsRight");
                if (handInteractors != null)
                {
                    var microgestureGroup = handInteractors.transform.Find("MicroGesturesLocomotionHandInteractorGroup");
                    if (microgestureGroup != null)
                    {
                        teleportMicrogestureInteractor = microgestureGroup.transform.Find("TeleportMicrogestureInteractor").gameObject;
                    }
                }
            }
        }
        
        if (arcVisual == null && teleportMicrogestureInteractor != null)
        {
            // Suche nach dem ArcVisual unter dem TeleportMicrogestureInteractor
            arcVisual = teleportMicrogestureInteractor.transform.Find("ArcVisual").gameObject;
        }
        
        // Finde den TeleportationProvider im Locomotor
        if (locomotor != null)
        {
            teleportProvider = locomotor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>();
            
            if (teleportProvider != null)
            {
                // Event-Listener für Teleportation-Ereignisse hinzufügen
                teleportProvider.endLocomotion += OnTeleportationEnded;
                Debug.Log("TeleportationProvider gefunden und Event-Listener hinzugefügt.");
            }
            else
            {
                Debug.LogWarning("Kein TeleportationProvider im Locomotor gefunden.");
            }
        }
        else
        {
            Debug.LogError("Locomotor nicht gefunden. Bitte manuell zuweisen.");
        }
        
        // Log der gefundenen Objekte
        Debug.Log("TeleportMicrogestureInteractor gefunden: " + (teleportMicrogestureInteractor != null));
        Debug.Log("ArcVisual gefunden: " + (arcVisual != null));
    }
    
    private void OnDestroy()
    {
        // Event-Listener entfernen, wenn das Objekt zerstört wird
        if (teleportProvider != null)
        {
            teleportProvider.endLocomotion -= OnTeleportationEnded;
        }
    }
    
    // Wird aufgerufen, wenn eine Teleportation endet
    private void OnTeleportationEnded(UnityEngine.XR.Interaction.Toolkit.LocomotionSystem locomotionSystem)
    {
        // Starte Coroutine mit kurzer Verzögerung zur Deaktivierung
        StartCoroutine(DeactivateRaysAfterDelay());
    }
    
    // Coroutine, die nach einer kurzen Verzögerung die Rays deaktiviert
    private IEnumerator DeactivateRaysAfterDelay()
    {
        // Kurze Verzögerung, um sicherzustellen, dass die Teleportation vollständig abgeschlossen ist
        yield return new WaitForSeconds(deactivationDelay);
        
        DeactivateAllRays();
    }
    
    // Deaktiviert alle Teleport-Ray-Komponenten
    public void DeactivateAllRays()
    {
        if (teleportMicrogestureInteractor != null)
        {
            // Speichere den aktuellen Status
            var interactor = teleportMicrogestureInteractor.GetComponent<MonoBehaviour>();
            if (interactor != null)
            {
                wasTeleportInteractorEnabled = interactor.enabled;
                interactor.enabled = false;
                Debug.Log("TeleportMicrogestureInteractor deaktiviert");
            }
        }
        
        // Deaktiviere ArcVisual
        if (arcVisual != null)
        {
            arcVisual.SetActive(false);
            Debug.Log("ArcVisual deaktiviert");
        }
        
        // Suche nach weiteren visuellen Elementen im TeleportMicrogestureInteractor
        if (teleportMicrogestureInteractor != null)
        {
            // Deaktiviere alle Kindelemente, die möglicherweise visuelle Elemente sind
            foreach (Transform child in teleportMicrogestureInteractor.transform)
            {
                if (child.gameObject != null && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    Debug.Log("Visuelles Element deaktiviert: " + child.name);
                }
            }
        }
    }
    
    // Öffentliche Methode, um die Rays manuell zu reaktivieren (falls benötigt)
    public void ReactivateRays()
    {
        if (teleportMicrogestureInteractor != null)
        {
            var interactor = teleportMicrogestureInteractor.GetComponent<MonoBehaviour>();
            if (interactor != null)
            {
                interactor.enabled = wasTeleportInteractorEnabled;
                Debug.Log("TeleportMicrogestureInteractor reaktiviert");
            }
        }
        
        // Reaktiviere ArcVisual nur, wenn der Interactor aktiviert ist
        if (arcVisual != null && wasTeleportInteractorEnabled)
        {
            arcVisual.SetActive(true);
            Debug.Log("ArcVisual reaktiviert");
        }
    }
}