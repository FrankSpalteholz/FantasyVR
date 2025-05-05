using UnityEngine;

public class UIFaceUpright : MonoBehaviour
{
    [Tooltip("Optional: Speziell zu verwendende Kamera, ansonsten wird die Hauptkamera verwendet")]
    public Camera targetCamera;
    
    [Tooltip("Soll das UI zur Kamera oder nur nach oben zeigen?")]
    public bool faceCamera = false;
    
    [Tooltip("Nur Y-Achse stabilisieren (empfohlen für Handheld-UI)")]
    public bool onlyStabilizeYRotation = true;
    
    [Tooltip("Y-Rotation zur Kamera spiegeln")]
    public bool invertYRotation = true;
    
    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }
    
    private void LateUpdate()
    {
        if (faceCamera && targetCamera != null)
        {
            if (invertYRotation)
            {
                // Zum Kamera schauen mit gespiegelter Y-Rotation
                Vector3 directionToCamera = transform.position - targetCamera.transform.position;
                directionToCamera.y = 0; // Ignoriere Höhenunterschied
                
                // Spiegele die Richtung auf der Y-Achse
                directionToCamera = -directionToCamera;
                
                // Berechne die Rotation, die das UI zur gespiegelten Richtung ausrichtet
                Quaternion lookRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
                
                if (onlyStabilizeYRotation)
                {
                    // Behalte nur die Y-Rotation bei
                    Vector3 eulerAngles = lookRotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(0, eulerAngles.y, 0);
                }
                else
                {
                    transform.rotation = lookRotation;
                }
            }
            else
            {
                // Normales Verhalten (ohne Spiegelung)
                transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                                 targetCamera.transform.rotation * Vector3.up);
            }
        }
        else
        {
            // Nur die Aufwärts-Ausrichtung beibehalten
            if (onlyStabilizeYRotation)
            {
                // Extrahiere die aktuelle Rotation in Euler-Winkel
                Vector3 rotation = transform.rotation.eulerAngles;
                
                // Behalte nur die Y-Rotation bei und setze X und Z auf 0
                transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
            else
            {
                // Komplette Aufwärts-Orientierung (in Weltkoordinaten)
                transform.up = Vector3.up;
            }
        }
    }
}