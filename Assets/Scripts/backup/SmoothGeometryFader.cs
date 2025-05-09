using UnityEngine;
using System.Collections;

public class SmoothGeometryFader : MonoBehaviour
{
    // Die Geometrien, die ein- und ausgeblendet werden sollen
    public GameObject[] objectsToFade;
    
    // Dauer der Überblendung in Sekunden
    public float fadeDuration = 0.5f;
    
    // Materialien speichern
    private Renderer[][] renderers;
    private Material[][] materials;
    private Color[][] originalColors;
    
    // Fade-Status
    private bool isFading = false;
    private Coroutine fadeCoroutine = null;
    
    void Awake()
    {
        // Initialisierung der Arrays
        InitializeArrays();
        
        // Setze Anfangszustand (unsichtbar)
        SetVisibilityInstant(false);
    }
    
    void InitializeArrays()
    {
        renderers = new Renderer[objectsToFade.Length][];
        materials = new Material[objectsToFade.Length][];
        originalColors = new Color[objectsToFade.Length][];
        
        for (int i = 0; i < objectsToFade.Length; i++)
        {
            // Hole alle Renderer für jedes Objekt
            renderers[i] = objectsToFade[i].GetComponentsInChildren<Renderer>(true);
            materials[i] = new Material[renderers[i].Length];
            originalColors[i] = new Color[renderers[i].Length];
            
            for (int j = 0; j < renderers[i].Length; j++)
            {
                // Wichtig: Erstelle eine einzigartige Kopie des Materials
                materials[i][j] = new Material(renderers[i][j].material);
                renderers[i][j].material = materials[i][j];
                
                // Speichere die Original-Farbe
                if (materials[i][j].HasProperty("_Color"))
                {
                    originalColors[i][j] = materials[i][j].GetColor("_Color");
                }
                else if (materials[i][j].HasProperty("_BaseColor"))
                {
                    originalColors[i][j] = materials[i][j].GetColor("_BaseColor");
                }
                
                // Materialeigenschaften für Transparenz einrichten
                ConfigureTransparency(materials[i][j]);
            }
        }
    }
    
    void ConfigureTransparency(Material mat)
    {
        // Shader-Einstellungen für Transparenz
        if (mat.HasProperty("_SrcBlend") && mat.HasProperty("_DstBlend"))
        {
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        
        if (mat.HasProperty("_ZWrite"))
        {
            mat.SetInt("_ZWrite", 0);
        }
        
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1); // 1 = Transparent
        }
        
        // Je nach Shader-Typ können verschiedene Keywords benötigt werden
        string shaderName = mat.shader.name.ToLower();
        
        if (shaderName.Contains("standard"))
        {
            mat.SetFloat("_Mode", 3); // Transparent
            mat.EnableKeyword("_ALPHABLEND_ON");
        }
        else if (shaderName.Contains("universal") || shaderName.Contains("urp"))
        {
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        
        // Render Queue erhöhen
        mat.renderQueue = 3000;
    }
    
    // Diese Methode wird vom Toggle aufgerufen
    public void FadeObjects(bool fadeIn)
    {
        // Wenn bereits ein Fade läuft, diesen abbrechen
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Starte die neue Fade-Animation
        fadeCoroutine = StartCoroutine(DoFade(fadeIn));
    }
    
    IEnumerator DoFade(bool fadeIn)
    {
        isFading = true;
        
        // Aktiviere alle Objekte, wenn sie eingeblendet werden sollen
        if (fadeIn)
        {
            foreach (GameObject obj in objectsToFade)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                }
            }
        }
        
        // Bestimme Start- und Zielwert
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        
        // Zeit für Interpolation
        float elapsedTime = 0f;
        
        // Debug.Log($"Starting fade from {startAlpha} to {endAlpha}");
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            // Berechne aktuellen Alpha-Wert (mit Smooth-Interpolation)
            float currentAlpha = Mathf.SmoothStep(startAlpha, endAlpha, normalizedTime);
            
            // Debug.Log($"Fading: {currentAlpha} (at {normalizedTime})");
            
            // Wende neuen Alpha-Wert auf alle Materialien an
            SetAlpha(currentAlpha);
            
            yield return null;
        }
        
        // Stelle sicher, dass der Endzustand exakt erreicht wird
        SetAlpha(endAlpha);
        
        // Wenn ausgeblendet, deaktiviere die Objekte
        if (!fadeIn)
        {
            foreach (GameObject obj in objectsToFade)
            {
                obj.SetActive(false);
            }
        }
        
        isFading = false;
        fadeCoroutine = null;
    }
    
    // Setzt den Alpha-Wert für alle Materialien
    void SetAlpha(float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            for (int j = 0; j < materials[i].Length; j++)
            {
                Material mat = materials[i][j];
                Color origColor = originalColors[i][j];
                
                // Versuche verschiedene Farbproperties
                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", new Color(origColor.r, origColor.g, origColor.b, alpha));
                }
                
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", new Color(origColor.r, origColor.g, origColor.b, alpha));
                }
            }
        }
    }
    
    // Setzt die Sichtbarkeit sofort ohne Animation
    void SetVisibilityInstant(bool visible)
    {
        SetAlpha(visible ? 1f : 0f);
        
        foreach (GameObject obj in objectsToFade)
        {
            obj.SetActive(visible);
        }
    }
}