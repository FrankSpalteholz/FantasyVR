using UnityEngine;
using UnityEngine.Video;

public class SimpleVideoParticles : MonoBehaviour
{
    [Header("Video Einstellungen")]
    [SerializeField] private VideoClip videoClip;
    [SerializeField] private bool loopVideo = true;
    [SerializeField] private int skipPixels = 4; // Jeden x-ten Pixel nehmen für Performance
    
    [Header("Partikel Einstellungen")]
    [SerializeField] private float particleSize = 0.01f;
    [SerializeField] private float depthScale = 0.5f; // Wie stark der Tiefeneffekt sein soll
    
    private VideoPlayer videoPlayer;
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    private RenderTexture videoTexture;
    private Texture2D readableTexture;
    private Color32[] videoPixels;
    private int width, height;
    
    void Start()
    {
        // Video Player einrichten
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.clip = videoClip;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.playOnAwake = true;
        
        // Textur erstellen
        width = 1280; // Anpassen an deine Videoauflösung
        height = 720;  // Anpassen an deine Videoauflösung
        videoTexture = new RenderTexture(width, height, 0);
        videoPlayer.targetTexture = videoTexture;
        readableTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // Berechne die Anzahl der Partikel
        int particleCount = (width / skipPixels) * (height / skipPixels);
        
        // Partikel System erstellen
        particleSystem = gameObject.AddComponent<ParticleSystem>();
        var mainModule = particleSystem.main;
        mainModule.startLifetime = Mathf.Infinity;
        mainModule.startSpeed = 0;
        mainModule.startSize = particleSize;
        mainModule.maxParticles = particleCount;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission deaktivieren (wir setzen die Partikel manuell)
        var emission = particleSystem.emission;
        emission.enabled = false;
        
        // Partikel Array initialisieren
        particles = new ParticleSystem.Particle[particleCount];
        
        // Partikel erstellen
        InitializeParticles();
        
        // Starte Video
        videoPlayer.Play();
    }
    
    void InitializeParticles()
    {
        int index = 0;
        
        // Erstelle ein Raster von Partikeln
        for (int y = 0; y < height; y += skipPixels)
        {
            for (int x = 0; x < width; x += skipPixels)
            {
                if (index < particles.Length)
                {
                    // Normalisierte Koordinaten (-0.5 bis 0.5)
                    float nx = (x / (float)width) - 0.5f;
                    float ny = (y / (float)height) - 0.5f;
                    
                    // Position setzen
                    particles[index].position = new Vector3(nx, ny, 0);
                    particles[index].startColor = Color.white;
                    particles[index].startSize = particleSize;
                    index++;
                }
            }
        }
        
        // Partikel dem System hinzufügen
        particleSystem.SetParticles(particles, particles.Length);
    }
    
    void Update()
    {
        // Video-Frame in lesbare Textur kopieren
        RenderTexture.active = videoTexture;
        readableTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = null;
        
        // Pixel-Farben holen
        videoPixels = readableTexture.GetPixels32();
        
        // Partikel vom System holen
        particleSystem.GetParticles(particles);
        
        int index = 0;
        // Aktualisiere die Farbe und Position jedes Partikels
        for (int y = 0; y < height; y += skipPixels)
        {
            for (int x = 0; x < width; x += skipPixels)
            {
                if (index < particles.Length)
                {
                    // Pixel-Index berechnen
                    int pixelIndex = y * width + x;
                    
                    // Sicherstellen, dass der Index gültig ist
                    if (pixelIndex < videoPixels.Length)
                    {
                        // Farbe aus dem Video holen
                        Color32 color = videoPixels[pixelIndex];
                        particles[index].startColor = color;
                        
                        // Graustufen-Wert für Tiefe berechnen
                        float grayscale = (0.299f * color.r + 0.587f * color.g + 0.114f * color.b) / 255f;
                        
                        // Position aktualisieren (Z-Achse für Tiefe)
                        float nx = (x / (float)width) - 0.5f;
                        float ny = (y / (float)height) - 0.5f;
                        particles[index].position = new Vector3(nx, ny, grayscale * depthScale);
                    }
                    
                    index++;
                }
            }
        }
        
        // Aktualisierte Partikel zurück ins System schreiben
        particleSystem.SetParticles(particles, particles.Length);
    }
    
    void OnDestroy()
    {
        // Aufräumen
        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
    }
}