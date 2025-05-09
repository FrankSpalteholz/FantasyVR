using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoSyncManager : MonoBehaviour
{
    public VideoPlayer[] videoPlayers = new VideoPlayer[5];
    
    [Header("Playback Options")]
    public bool loopVideos = true;
    public bool stopAtEnd = true;
    public bool syncEveryFrame = false;
    public float syncCheckInterval = 0.5f;
    
    [Header("Advanced Options")]
    public int maxFrameDifference = 2;
    public float timeToLoopEnd = 0.1f;
    
    private int readyCount = 0;
    private bool allPrepared = false;
    private double videoLength = 0;
    private float syncTimer = 0;
    private bool isLooping = false;
    private bool initialized = false;
    
    // OnEnable statt Start verwenden, wird aufgerufen wenn das GameObject aktiviert wird
    void OnEnable()
    {
        // Wenn bereits initialisiert, direkt starten
        if (initialized)
        {
            StartCoroutine(PlayAllSynchronized());
            return;
        }
        
        // Erste Initialisierung
        initialized = true;
        string videoPath = Path.Combine(Application.streamingAssetsPath, "test.mp4");

        readyCount = 0;
        allPrepared = false;
        
        foreach (var vp in videoPlayers)
        {
            vp.playOnAwake = false;
            vp.source = VideoSource.Url;
            vp.url = videoPath;
            vp.isLooping = false;
            vp.Prepare();
            vp.prepareCompleted += OnPrepared;
        }
    }
    
    void OnDisable()
    {
        // Videos stoppen, wenn das GameObject deaktiviert wird
        foreach (var vp in videoPlayers)
        {
            if (vp.isPlaying)
                vp.Stop();
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        readyCount++;
        // VideoLength holen (nur einmal nötig)
        if (videoLength == 0)
            videoLength = vp.length;

        if (readyCount == videoPlayers.Length)
        {
            allPrepared = true;
            StartCoroutine(PlayAllSynchronized());
        }
    }

    IEnumerator PlayAllSynchronized()
    {
        foreach (var vp in videoPlayers)
        {
            vp.time = 0;
        }

        yield return new WaitForEndOfFrame();

        foreach (var vp in videoPlayers)
        {
            vp.Play();
        }
        
        // Nach dem Starten aller Videos, führe sofort eine Synchronisation durch
        yield return new WaitForEndOfFrame();
        SyncVideoFrames();
    }

    void Update()
    {
        if (!allPrepared) return;
        
        // Synchronisations-Check
        if (syncEveryFrame)
        {
            SyncVideoFrames();
        }
        else
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= syncCheckInterval)
            {
                SyncVideoFrames();
                syncTimer = 0;
            }
        }

        // Nur prüfen, wenn kein Loop gerade durchgeführt wird
        if (!isLooping)
        {
            // Prüfe, ob ein Player am Ende angekommen ist
            bool shouldLoop = false;
            bool anyVideoStillPlaying = false;
            
            foreach (var vp in videoPlayers)
            {
                if (vp.isPlaying)
                {
                    anyVideoStillPlaying = true;
                    
                    // Wenn ein Video nahe am Ende ist und wir loopen sollen
                    if (loopVideos && (vp.frame >= (long)vp.frameCount - 5 || vp.time >= videoLength - timeToLoopEnd))
                    {
                        shouldLoop = true;
                        break;
                    }
                }
            }
            
            // Wenn alle Videos gestoppt haben und wir nicht loopen möchten
            if (!anyVideoStillPlaying && !stopAtEnd)
            {
                StartCoroutine(PlayAllSynchronized());
                return;
            }
            
            if (shouldLoop)
            {
                StartCoroutine(RestartLoop());
            }
        }
    }
    
    void SyncVideoFrames()
    {
        // Nur aktive Synchronisation, wenn mehr als ein Video vorhanden ist
        if (videoPlayers.Length <= 1 || !allPrepared) return;
        
        // Verwende ersten Player als Master
        VideoPlayer masterPlayer = videoPlayers[0];
        
        if (!masterPlayer.isPlaying) return;
        
        long masterFrame = masterPlayer.frame;
        
        // Synchronisiere alle anderen Player mit dem Master
        for (int i = 1; i < videoPlayers.Length; i++)
        {
            VideoPlayer slavePlayer = videoPlayers[i];
            
            if (!slavePlayer.isPlaying) continue;
            
            long frameDifference = Mathf.Abs((int)(masterFrame - slavePlayer.frame));
            
            // Nur synchronisieren, wenn der Unterschied zu groß wird
            if (frameDifference > maxFrameDifference)
            {
                slavePlayer.frame = masterFrame;
            }
        }
    }

    IEnumerator RestartLoop()
    {
        isLooping = true;
        
        foreach (var vp in videoPlayers)
        {
            vp.Stop();
        }

        yield return new WaitForEndOfFrame();

        foreach (var vp in videoPlayers)
        {
            vp.time = 0;
        }

        yield return new WaitForEndOfFrame();

        if (loopVideos)
        {
            foreach (var vp in videoPlayers)
            {
                vp.Play();
            }
            
            // Nach dem Neustarten direkt eine Synchronisation durchführen
            yield return new WaitForEndOfFrame();
            SyncVideoFrames();
        }
        
        isLooping = false;
    }
    
    // Öffentliche Methoden zur manuellen Steuerung
    
    public void ForceStartVideos()
    {
        if (allPrepared)
        {
            StartCoroutine(PlayAllSynchronized());
        }
        else
        {
            // Falls Videos noch nicht vorbereitet sind
            StartCoroutine(WaitForPreparationAndPlay());
        }
    }
    
    IEnumerator WaitForPreparationAndPlay()
    {
        // Warte maximal 5 Sekunden auf Vorbereitung
        float timeout = 5.0f;
        float elapsed = 0f;
        
        while (!allPrepared && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (allPrepared)
        {
            StartCoroutine(PlayAllSynchronized());
        }
        else
        {
            Debug.LogWarning("Videos konnten nicht rechtzeitig vorbereitet werden!");
        }
    }
    
    public void StopVideos()
    {
        foreach (var vp in videoPlayers)
        {
            vp.Stop();
        }
    }
    
    // Weitere nützliche öffentliche Methoden
    
    public void ToggleLoop(bool enableLoop)
    {
        loopVideos = enableLoop;
    }
    
    public void ToggleStopAtEnd(bool enableStop)
    {
        stopAtEnd = enableStop;
    }
}