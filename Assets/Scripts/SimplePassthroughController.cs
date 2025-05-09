using UnityEngine;

public class SimplePassthroughController : MonoBehaviour
{
    [Header("Settings")]
    public float fadeTime = 0.5f;
    
    private OVRPassthroughLayer passthroughLayer;
    private bool isPassthroughActive = true;
    private bool isFading = false;
    
    void Start()
    {
        // Hole die OVRPassthroughLayer-Komponente von diesem GameObject
        passthroughLayer = GetComponent<OVRPassthroughLayer>();
        
        if (passthroughLayer == null)
        {
            Debug.LogError("Keine OVRPassthroughLayer-Komponente auf diesem Objekt gefunden!");
            return;
        }
        
        // Starte mit aktivem Passthrough
        passthroughLayer.textureOpacity = 1f;
        isPassthroughActive = true;
    }
    
    // Diese Methode kannst du auf dem Toggle aufrufen
    public void TogglePassthrough()
    {
        if (passthroughLayer == null || isFading) return;
        
        Debug.Log($"TogglePassthrough aufgerufen - Aktueller Zustand: {isPassthroughActive}");
        
        isPassthroughActive = !isPassthroughActive;
        StartCoroutine(FadePassthrough());
    }
    
    private System.Collections.IEnumerator FadePassthrough()
    {
        isFading = true;
        float startOpacity = passthroughLayer.textureOpacity;
        float targetOpacity = isPassthroughActive ? 1f : 0f;
        float timer = 0f;
        
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeTime;
            passthroughLayer.textureOpacity = Mathf.Lerp(startOpacity, targetOpacity, progress);
            yield return null;
        }
        
        passthroughLayer.textureOpacity = targetOpacity;
        isFading = false;
        
        Debug.Log($"Fade abgeschlossen - Neue Opacity: {passthroughLayer.textureOpacity}");
    }
    
    // Optional: Methoden für direkten Zugriff
    public void ShowPassthrough()
    {
        if (passthroughLayer == null) return;
        isPassthroughActive = true;
        StartCoroutine(FadePassthrough());
    }
    
    public void HidePassthrough()
    {
        if (passthroughLayer == null) return;
        isPassthroughActive = false;
        StartCoroutine(FadePassthrough());
    }
    
    // Hilfreiche Methode für den Toggle-Zustand
    public bool IsPassthroughActive()
    {
        return isPassthroughActive;
    }
}