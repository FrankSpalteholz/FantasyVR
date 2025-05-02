using UnityEngine;

public class GeometryVisibilityToggle : MonoBehaviour
{
    // Die drei Geometrien, die ein- und ausgeblendet werden sollen
    public GameObject[] geometriesToToggle;
    
    // Dauer der Überblendung in Sekunden
    public float fadeDuration = 1.0f;
    
    // Speichert die Materialien für jeden Renderer
    private Material[][] materials;
    
    // Speichert den ursprünglichen Zustand der Transparenz
    private bool[] wasTransparent;
    
    // Speichert die zu ändernden Property-Namen für jeden Shader-Typ
    private string[] propertyNames = { "_Color", "_BaseColor" };
    
    // Überblendungsstatus
    private float currentFade = 0.0f;
    private bool isFading = false;
    private bool targetVisible = false;

    void Start()
    {
        // Initialisiere
        InitializeMaterials();
        
        // Setze Anfangszustand auf unsichtbar
        SetVisibility(false, true); // Sofort (ohne Fade) unsichtbar machen
    }
    
    void InitializeMaterials()
    {
        materials = new Material[geometriesToToggle.Length][];
        wasTransparent = new bool[geometriesToToggle.Length];
        
        for (int i = 0; i < geometriesToToggle.Length; i++)
        {
            Renderer[] renderers = geometriesToToggle[i].GetComponentsInChildren<Renderer>();
            materials[i] = new Material[renderers.Length];
            
            // Prüfe, ob ein Material bereits transparent ist
            bool anyTransparent = false;
            
            for (int j = 0; j < renderers.Length; j++)
            {
                // Erstelle eine einzigartige Materialinstanz für jeden Renderer
                materials[i][j] = new Material(renderers[j].material);
                renderers[j].material = materials[i][j];
                
                // Prüfe, ob das Material bereits Transparenz unterstützt
                if (IsTransparent(materials[i][j]))
                {
                    anyTransparent = true;
                }
                else
                {
                    // Stelle sicher, dass wir ein kompatibles Shader-Setup haben
                    PrepareForTransparency(materials[i][j]);
                }
            }
            
            wasTransparent[i] = anyTransparent;
        }
    }
    
    bool IsTransparent(Material mat)
    {
        if (mat.HasProperty("_Surface")) // URP/HDRP
        {
            return mat.GetFloat("_Surface") > 0.5f;
        }
        else if (mat.HasProperty("_Mode")) // Standard
        {
            return mat.GetFloat("_Mode") > 2.5f;
        }
        
        // Prüfe, ob das Material einen transparenten Shader verwendet
        return mat.shader.name.ToLower().Contains("transparent");
    }
    
    void PrepareForTransparency(Material mat)
    {
        // Für Quest-Anwendungen ist es am besten, direkt einen kompatiblen Shader zu verwenden
        // Versuche zuerst, einen bereits installierten transparenten Shader zu finden
        
        string shaderName = mat.shader.name;
        
        if (shaderName.Contains("Universal Render Pipeline") || shaderName.Contains("URP"))
        {
            // URP Shader
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            }
            
            if (mat.HasProperty("_SrcBlend") && mat.HasProperty("_DstBlend"))
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            
            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetInt("_ZWrite", 0);
            }
            
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        else if (shaderName.Contains("Standard"))
        {
            // Standard Shader
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        else
        {
            // Versuche verschiedene Mobile-freundliche transparente Shader
            Shader transparentShader = Shader.Find("Mobile/Transparent/Diffuse");
            
            if (transparentShader == null)
            {
                transparentShader = Shader.Find("Universal Render Pipeline/Lit");
            }
            
            if (transparentShader == null)
            {
                transparentShader = Shader.Find("Transparent/Diffuse");
            }
            
            if (transparentShader != null)
            {
                mat.shader = transparentShader;
                
                // Wenn wir zu einem URP Shader gewechselt sind, setze die Transparenz
                if (mat.HasProperty("_Surface"))
                {
                    mat.SetFloat("_Surface", 1);
                }
            }
        }
    }
    
    void Update()
    {
        if (isFading)
        {
            float targetValue = targetVisible ? 1.0f : 0.0f;
            currentFade = Mathf.MoveTowards(currentFade, targetValue, Time.deltaTime / fadeDuration);
            
            // Aktualisiere Alpha-Werte
            UpdateAlpha(currentFade);
            
            // Prüfe, ob fertig
            if (Mathf.Approximately(currentFade, targetValue))
            {
                isFading = false;
                
                // Wenn vollständig ausgeblendet, deaktiviere die GameObjects
                if (!targetVisible)
                {
                    foreach (GameObject obj in geometriesToToggle)
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }
    }
    
    void UpdateAlpha(float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            for (int j = 0; j < materials[i].Length; j++)
            {
                Material mat = materials[i][j];
                
                // Versuche verschiedene gängige Farbproperties
                foreach (string propertyName in propertyNames)
                {
                    if (mat.HasProperty(propertyName))
                    {
                        Color color = mat.GetColor(propertyName);
                        color.a = alpha;
                        mat.SetColor(propertyName, color);
                        break;
                    }
                }
            }
        }
    }
    
    // Diese Methode wird vom Toggle aufgerufen
    public void OnToggleValueChanged(bool isVisible)
    {
        SetVisibility(isVisible, false);
    }
    
    // Öffentliche Methode für die Sichtbarkeit
    public void SetVisibility(bool visible, bool immediate = false)
    {
        targetVisible = visible;
        
        if (immediate)
        {
            // Sofortige Änderung ohne Überblendung
            currentFade = visible ? 1.0f : 0.0f;
            UpdateAlpha(currentFade);
            
            // Aktiviere/Deaktiviere die GameObjects
            foreach (GameObject obj in geometriesToToggle)
            {
                obj.SetActive(visible);
            }
            
            isFading = false;
        }
        else
        {
            // Mit Überblendung
            if (visible)
            {
                // Aktiviere die GameObjects
                foreach (GameObject obj in geometriesToToggle)
                {
                    obj.SetActive(true);
                }
            }
            
            isFading = true;
        }
    }
}