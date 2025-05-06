using UnityEngine;

public class MenuDistanceHandler : MonoBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Maximale Distanz in Metern, bei der das Menü erreichbar bleibt")]
    [SerializeField] private float maxDistance = 3.0f;
    
    [Header("Default Position Settings")]
    [Tooltip("Standard-Positionsversatz vom Kopf (in Metern)")]
    [SerializeField] private Vector3 headPositionOffset = new Vector3(0.3f, -0.25f, 0.4f);
    
    [Header("Repositioning Settings")]
    [Tooltip("Geschwindigkeit, mit der das Menü zur Standardposition zurückkehrt")]
    [SerializeField] private float repositioningSpeed = 5.0f;
    [Tooltip("Verzögerung in Sekunden, bevor die Anfangspositionierung erfolgt")]
    [SerializeField] private float initialPositionDelay = 0.2f;
    
    [Header("References")]
    [Tooltip("Referenz zum Menü-GameObject")]
    [SerializeField] private GameObject menuObject;
    [Tooltip("Referenz zum XR Rig Kamera/Kopf-Transform")]
    [SerializeField] private Transform headTransform;
    
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool isReturningToDefault = false;
    private Rigidbody menuRigidbody;
    private bool initialPositionSet = false;
    private float timeSinceStart = 0f;
    
    private void Awake()
    {
        // Wenn kein Menüobjekt gesetzt ist, verwende dieses GameObject
        if (menuObject == null)
            menuObject = gameObject;
        
        // Versuche Rigidbody zu finden (falls vorhanden)
        menuRigidbody = menuObject.GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        // Finde Kopf-Transform, falls nicht gesetzt
        if (headTransform == null)
        {
            // Versuche erst die Main Camera zu finden
            headTransform = Camera.main?.transform;
            
            // Wenn nicht gefunden, warte damit bis später
            if (headTransform == null)
            {
                Debug.LogWarning("Head Transform nicht gefunden! Wird später erneut versucht.");
            }
        }
        
        // Initialisiere initialPositionSet auf false - wir werden im Update warten
        initialPositionSet = false;
    }
    
    private void Update()
    {
        // Prüfe, ob wir noch die Kopf-Transform finden müssen
        if (headTransform == null)
        {
            headTransform = Camera.main?.transform;
            if (headTransform == null)
                return; // Warte weiter, wenn immer noch nicht gefunden
        }
        
        // Warte mit der ersten Positionierung, bis nach der angegebenen Verzögerung
        if (!initialPositionSet)
        {
            timeSinceStart += Time.deltaTime;
            
            if (timeSinceStart >= initialPositionDelay)
            {
                // Setze initiale Position nach einer kurzen Verzögerung
                SetToDefaultPosition(true);
                initialPositionSet = true;
                
                // Log zur Bestätigung
                Debug.Log("Menü an Anfangsposition gesetzt. Kopfposition: " + headTransform.position);
            }
            return; // Weitere Verarbeitung überspringen, bis die Anfangsposition gesetzt ist
        }
        
        // Berechne Distanz zum Kopf
        float distanceToHead = Vector3.Distance(menuObject.transform.position, headTransform.position);
        
        // Wenn außerhalb der maximalen Distanz und noch nicht zurückkehrend
        if (distanceToHead > maxDistance && !isReturningToDefault)
        {
            isReturningToDefault = true;
        }
        
        // Wenn zurückkehrend, bewege zum Standard
        if (isReturningToDefault)
        {
            ReturnToDefaultPosition();
        }
    }
    
    private void SetToDefaultPosition(bool instant = false)
    {
        if (headTransform == null)
        {
            Debug.LogError("SetToDefaultPosition: headTransform ist null!");
            return;
        }
        
        // Prüfe auf ungültige Kopfposition (NaN oder Infinity)
        if (float.IsNaN(headTransform.position.x) || float.IsInfinity(headTransform.position.x) ||
            float.IsNaN(headTransform.position.y) || float.IsInfinity(headTransform.position.y) ||
            float.IsNaN(headTransform.position.z) || float.IsInfinity(headTransform.position.z))
        {
            Debug.LogError("SetToDefaultPosition: headTransform hat ungültige Position!");
            return;
        }
        
        // Berechne Standardposition relativ zum Kopf
        defaultPosition = headTransform.position + 
                          headTransform.right * headPositionOffset.x + 
                          headTransform.up * headPositionOffset.y + 
                          headTransform.forward * headPositionOffset.z;
                          
        // Berechne Standardrotation (zum Benutzer gedreht)
        Vector3 directionToHead = headTransform.position - defaultPosition;
        defaultRotation = Quaternion.LookRotation(directionToHead, Vector3.up);
        
        if (instant)
        {
            // Setze Position und Rotation sofort
            menuObject.transform.position = defaultPosition;
            menuObject.transform.rotation = defaultRotation;
            
            // Setze Physik-Geschwindigkeiten zurück, falls Rigidbody verwendet wird
            if (menuRigidbody != null)
            {
                menuRigidbody.linearVelocity = Vector3.zero;
                menuRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
    
    private void ReturnToDefaultPosition()
    {
        // Aktualisiere Standardposition basierend auf aktueller Kopfposition
        SetToDefaultPosition();
        
        // Bewege sanft zur Standardposition
        menuObject.transform.position = Vector3.Lerp(menuObject.transform.position, defaultPosition, Time.deltaTime * repositioningSpeed);
        menuObject.transform.rotation = Quaternion.Slerp(menuObject.transform.rotation, defaultRotation, Time.deltaTime * repositioningSpeed);
        
        // Prüfe, ob Standardposition erreicht wurde
        float distanceToDefault = Vector3.Distance(menuObject.transform.position, defaultPosition);
        if (distanceToDefault < 0.01f)
        {
            isReturningToDefault = false;
        }
    }

    // Öffentliche Methode zum manuellen Zurücksetzen der Position
    public void ResetToDefaultPosition()
    {
        if (headTransform != null)
        {
            SetToDefaultPosition(true);
            isReturningToDefault = false;
        }
    }
}