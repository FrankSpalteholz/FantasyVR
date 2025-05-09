using UnityEngine;
using UnityEngine.Video; // Dieser Namespace enthält die VideoPlayer-Klasse

public class VideoToVfxGraph : MonoBehaviour
{
    [Header("Video Einstellungen")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool loopVideo = true;
    
    [Header("Textur Einstellungen")]
    [SerializeField] private int textureWidth = 720;
    [SerializeField] private int textureHeight = 576;
    
    [Header("VFX Graph Referenzen")]
    [SerializeField] private UnityEngine.VFX.VisualEffect vfxGraph;
    [SerializeField] private string videoTexturePropertyName = "VideoTexture";
    [SerializeField] private string depthTexturePropertyName = "DepthTexture";
    
    // Private Variablen
    private VideoPlayer videoPlayer;
    private RenderTexture videoTexture;
    private RenderTexture depthTexture;
    private Material grayscaleConversionMaterial;
    
    void Awake()
    {
        // VideoPlayer erstellen
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.clip = videoClip;
        
        // Video-Textur erstellen
        videoTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        videoTexture.Create();
        
        // Graustufenmaterial für Tiefeneffekt erstellen
        grayscaleConversionMaterial = new Material(Shader.Find("Hidden/Internal-GrayscaleEffect"));
        
        // Tiefentextur erstellen
        depthTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
        depthTexture.Create();
        
        // Video an die Textur senden
        videoPlayer.targetTexture = videoTexture;
        
        // Prüfen ob VFX Graph zugewiesen ist
        if (vfxGraph != null)
        {
            // Texturen zum VFX Graph hinzufügen
            vfxGraph.SetTexture(videoTexturePropertyName, videoTexture);
            vfxGraph.SetTexture(depthTexturePropertyName, depthTexture);
        }
        else
        {
            Debug.LogWarning("Kein VFX Graph zugewiesen!");
        }
    }
    
    void Update()
    {
        if (videoTexture == null || depthTexture == null || grayscaleConversionMaterial == null)
            return;
            
        // Video zu Graustufen für Fake-Tiefe konvertieren
        Graphics.Blit(videoTexture, depthTexture, grayscaleConversionMaterial);
        
        // Hier könntest du zusätzliche Verarbeitung für die Tiefentextur hinzufügen
        // z.B. Kontrast erhöhen für stärkere Tiefenunterschiede
    }
    
    void OnDestroy()
    {
        // Aufräumen
        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
        
        if (depthTexture != null)
        {
            depthTexture.Release();
            Destroy(depthTexture);
        }
        
        if (grayscaleConversionMaterial != null)
        {
            Destroy(grayscaleConversionMaterial);
        }
    }
    
    // Öffentliche Methode, um Zugriff auf die Texturen zu ermöglichen
    public RenderTexture GetVideoTexture()
    {
        return videoTexture;
    }
    
    public RenderTexture GetDepthTexture()
    {
        return depthTexture;
    }
}