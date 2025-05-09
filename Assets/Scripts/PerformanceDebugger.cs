using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class PerformanceDebugger : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI memoryText;
    [SerializeField] private TextMeshProUGUI vramTotalText;
    [SerializeField] private TextMeshProUGUI drawCallsText;
    [SerializeField] private TextMeshProUGUI trianglesText;
    [SerializeField] private TextMeshProUGUI verticesText;
    
    [Header("Debug Log UI")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private int maxLogLines = 20;
    [SerializeField] private bool showTimestamps = true;
    [SerializeField] private KeyCode toggleLogKey = KeyCode.F2;
    [SerializeField] private bool logVisibleAtStart = true;
    
    [Header("Settings")]
    [SerializeField] private float fpsUpdateInterval = 0.1f;
    [SerializeField] private float statsUpdateInterval = 0.5f;
    [SerializeField] private int fpsBufferSize = 10;
    
    [Header("Performance Thresholds")]
    // FPS thresholds
    [SerializeField] private float goodFpsThreshold = 72f;
    [SerializeField] private float warningFpsThreshold = 60f;
    [SerializeField] private float criticalFpsThreshold = 45f;
    
    // RAM thresholds adjusted for Quest applications
    [SerializeField] private float maxRamDisplay = 600f;
    [SerializeField] private float warningRamThreshold = 500f;
    [SerializeField] private float criticalRamThreshold = 550f;
    
    // VRAM thresholds
    [SerializeField] private float warningVramThreshold = 800f;
    [SerializeField] private float criticalVramThreshold = 1200f;
    
    // Draw calls thresholds
    [SerializeField] private int warningDrawCallsThreshold = 50;
    [SerializeField] private int criticalDrawCallsThreshold = 100;
    
    // Triangles thresholds
    [SerializeField] private int warningTrianglesThreshold = 150000;
    [SerializeField] private int criticalTrianglesThreshold = 250000;
    
    // Vertices thresholds
    [SerializeField] private int warningVerticesThreshold = 200000;
    [SerializeField] private int criticalVerticesThreshold = 350000;
    
    // Color definitions
    private readonly string goodColor = "#00BFFF"; // Ocean blue
    private readonly string warningColor = "yellow";
    private readonly string badColor = "#FF6347"; // Reddish orange
    
    // Zusätzliche Farben für den Debug-Log
    private readonly string infoColor = "white";
    private readonly string debugColor = "#00BFFF";
    private readonly string errorColor = "#FF6347";
    
    // Private variables for FPS calculation
    private float[] fpsBuffer;
    private int fpsBufferIndex = 0;
    private float fpsTimeLeft;
    private float currentFps;
    
    // Neue Debug-Log-Variablen
    private Queue<string> logQueue = new Queue<string>();
    private bool isLogVisible = true;
    private GameObject logContainer; // Optional: Container für den Log-Bereich
    
    // Singleton-Instanz für einfachen Zugriff
    private static PerformanceDebugger instance;
    public static PerformanceDebugger Instance 
    {
        get { return instance; }
    }
    
    void Awake()
    {
        // Singleton-Pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Finde den Container (optional)
        if (logText != null && logContainer == null)
        {
            logContainer = logText.transform.parent.gameObject;
        }
        
        // Initialen Zustand setzen
        isLogVisible = logVisibleAtStart;
        if (logContainer != null)
        {
            logContainer.SetActive(isLogVisible);
        }
        else if (logText != null)
        {
            logText.gameObject.SetActive(isLogVisible);
        }
    }
    
    void Start()
    {
        // Initialize FPS buffer
        fpsBuffer = new float[fpsBufferSize];
        for (int i = 0; i < fpsBufferSize; i++) fpsBuffer[i] = 0f;
        
        fpsTimeLeft = fpsUpdateInterval;
        
        // Start stats update coroutine
        StartCoroutine(UpdateStats());
        
        // Optional: Starte mit einer Begrüßungsnachricht
        LogInfo("Debug-System initialisiert");
    }
    
    void Update()
    {
        // More accurate FPS calculation using Time.unscaledDeltaTime to catch actual performance
        float deltaTime = Time.unscaledDeltaTime;
        
        // Update FPS more frequently
        fpsTimeLeft -= deltaTime;
        
        // Calculate instantaneous FPS
        float instantFps = 1.0f / deltaTime;
        
        // Add to buffer for smoother display
        fpsBuffer[fpsBufferIndex] = instantFps;
        fpsBufferIndex = (fpsBufferIndex + 1) % fpsBufferSize;
        
        if (fpsTimeLeft <= 0f)
        {
            // Calculate average FPS from buffer
            float sum = 0f;
            foreach (float fps in fpsBuffer)
            {
                sum += fps;
            }
            currentFps = sum / fpsBufferSize;
            
            // Update FPS display with accurate and smoothed value
            UpdateFpsDisplay(currentFps);
            
            fpsTimeLeft = fpsUpdateInterval;
        }
        
        // Toggle-Funktion für Log-Anzeige
        if (Input.GetKeyDown(toggleLogKey))
        {
            ToggleLogVisibility();
        }
    }
    
    // Separate coroutine for updating less time-critical stats
    IEnumerator UpdateStats()
    {
        while (true)
        {
            // Update memory, VRAM, draw calls, triangles, vertices
            UpdateMemoryDisplay();
            UpdateDrawCallsDisplay();
            UpdateGeometryCountDisplay();
            
            // Wait for specified interval
            yield return new WaitForSeconds(statsUpdateInterval);
        }
    }
    
    void UpdateFpsDisplay(float fps)
    {
        if (fpsText != null)
        {
            // Use appropriate color based on performance
            string fpsColor;
            if (fps >= goodFpsThreshold) fpsColor = goodColor;
            else if (fps >= warningFpsThreshold) fpsColor = goodColor;
            else if (fps >= criticalFpsThreshold) fpsColor = warningColor;
            else fpsColor = badColor;
            
            // Display with decimal precision
            fpsText.text = $"FPS: <color={fpsColor}>{fps:F1}</color>";
            
            // Add frame time in milliseconds (important for debugging stutters)
            float frameTimeMs = 1000f / fps;
            fpsText.text += $" ({frameTimeMs:F1} ms)";
        }
    }
    
    void UpdateMemoryDisplay()
    {
        if (memoryText != null)
        {
            // Get memory in MB
            float totalMemory = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            float allocatedMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            
            // Cap the display to the maximum value
            float displayMemory = Mathf.Min(totalMemory, maxRamDisplay);
            
            // Use adjusted thresholds for Quest applications
            string memColor = totalMemory > criticalRamThreshold ? badColor : 
                             (totalMemory > warningRamThreshold ? warningColor : goodColor);
            
            memoryText.text = $"RAM: <color={memColor}>{allocatedMemory:F0}/{displayMemory:F0} MB</color>";
        }
        
        if (vramTotalText != null)
        {
            // VRAM usage
            long textureMemory = 0;
            long meshMemory = 0;
            long materialMemory = 0;
            
            // Get all textures
            Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();
            foreach (Texture tex in allTextures)
            {
                textureMemory += Profiler.GetRuntimeMemorySizeLong(tex);
            }
            
            // Get all meshes
            Mesh[] allMeshes = Resources.FindObjectsOfTypeAll<Mesh>();
            foreach (Mesh mesh in allMeshes)
            {
                meshMemory += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            
            // Get all materials
            Material[] allMaterials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (Material mat in allMaterials)
            {
                materialMemory += Profiler.GetRuntimeMemorySizeLong(mat);
            }
            
            float textureMB = textureMemory / (1024f * 1024f);
            float meshMB = meshMemory / (1024f * 1024f);
            float materialMB = materialMemory / (1024f * 1024f);
            float totalVRAM = textureMB + meshMB + materialMB;
            
            string vramColor = totalVRAM > criticalVramThreshold ? badColor : 
                              (totalVRAM > warningVramThreshold ? warningColor : goodColor);
            
            vramTotalText.text = $"VRAM: <color={vramColor}>{totalVRAM:F0} MB</color>";
        }
    }
    
    void UpdateDrawCallsDisplay()
    {
        if (drawCallsText != null)
        {
            int estimatedDrawCalls = 0;
            
            // Estimate draw calls from visible renderers
            Renderer[] renderers = FindObjectsOfType<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.isVisible)
                {
                    estimatedDrawCalls++;
                    if (renderer.materials.Length > 1)
                    {
                        estimatedDrawCalls += renderer.materials.Length - 1;
                    }
                }
            }
            
            string drawCallColor = estimatedDrawCalls > criticalDrawCallsThreshold ? badColor : 
                                  (estimatedDrawCalls > warningDrawCallsThreshold ? warningColor : goodColor);
            
            drawCallsText.text = $"Draw Calls: <color={drawCallColor}>{estimatedDrawCalls}</color>";
        }
    }
    
    void UpdateGeometryCountDisplay()
    {
        int triangleCount = 0;
        int vertexCount = 0;
        
        // Get visible renderers
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.isVisible)
            {
                // Count triangles and vertices from mesh filters
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    triangleCount += meshFilter.sharedMesh.triangles.Length / 3;
                    vertexCount += meshFilter.sharedMesh.vertexCount;
                }
                
                // Count from skinned mesh renderers
                SkinnedMeshRenderer skinnedMesh = renderer as SkinnedMeshRenderer;
                if (skinnedMesh != null && skinnedMesh.sharedMesh != null)
                {
                    triangleCount += skinnedMesh.sharedMesh.triangles.Length / 3;
                    vertexCount += skinnedMesh.sharedMesh.vertexCount;
                }
            }
        }
        
        if (trianglesText != null)
        {
            string triColor = triangleCount > criticalTrianglesThreshold ? badColor : 
                             (triangleCount > warningTrianglesThreshold ? warningColor : goodColor);
            
            trianglesText.text = $"Triangles: <color={triColor}>{triangleCount:N0}</color>";
        }
        
        if (verticesText != null)
        {
            string vertColor = vertexCount > criticalVerticesThreshold ? badColor : 
                              (vertexCount > warningVerticesThreshold ? warningColor : goodColor);
            
            verticesText.text = $"Vertices: <color={vertColor}>{vertexCount:N0}</color>";
        }
    }
    
    // NEUE METHODEN FÜR DAS DEBUG-LOGGING-SYSTEM
    
    public void ToggleLogVisibility()
    {
        isLogVisible = !isLogVisible;
        
        if (logContainer != null)
        {
            logContainer.SetActive(isLogVisible);
        }
        else if (logText != null)
        {
            logText.gameObject.SetActive(isLogVisible);
        }
    }
    
    public void ClearLogs()
    {
        logQueue.Clear();
        UpdateLogDisplay();
    }
    
    private void UpdateLogDisplay()
    {
        if (logText == null) return;
        
        logText.text = string.Join("\n", logQueue.ToArray());
    }
    
    // Interner Log-Handler
    private void AddLogEntry(string message, string colorCode)
    {
        if (logQueue == null) logQueue = new Queue<string>();
        
        // Zeitstempel hinzufügen, wenn gewünscht
        string timestamp = showTimestamps ? $"[{System.DateTime.Now.ToString("HH:mm:ss")}] " : "";
        
        // Formatierte Log-Nachricht zur Queue hinzufügen
        string formattedLog = $"{timestamp}<color={colorCode}>{message}</color>";
        logQueue.Enqueue(formattedLog);
        
        // Queue-Größe begrenzen
        while (logQueue.Count > maxLogLines)
        {
            logQueue.Dequeue();
        }
        
        UpdateLogDisplay();
    }
    
    // Öffentliche Logging-Methoden
    
    public void LogInfo(string message)
    {
        AddLogEntry(message, infoColor);
    }
    
    public void LogDebug(string message)
    {
        AddLogEntry(message, debugColor);
    }
    
    public void LogWarning(string message)
    {
        AddLogEntry(message, warningColor);
    }
    
    public void LogError(string message)
    {
        AddLogEntry(message, errorColor);
    }
    
    // Statische Methoden für einfachen Zugriff
    
    public static void Info(string message)
    {
        if (instance != null)
            instance.LogInfo(message);
    }
    
    public static void Debug(string message)
    {
        if (instance != null)
            instance.LogDebug(message);
    }
    
    public static void Warning(string message)
    {
        if (instance != null)
            instance.LogWarning(message);
    }
    
    public static void Error(string message)
    {
        if (instance != null)
            instance.LogError(message);
    }
}