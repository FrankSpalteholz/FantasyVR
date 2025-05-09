using UnityEngine;
using System.Collections.Generic;
using TMPro;

[ExecuteInEditMode]
public class DynamicVoidGrid : MonoBehaviour
{
    [Header("Grid-Main Settings")]
    public float gridSize = 10f;
    public bool showAtRuntime = true;
    
    [Header("Axes Settings")]
    public bool showAxes = true;
    public float axisThickness = 0.03f;
    public Color xAxisColor = new Color(1f, 0.3f, 0.3f, 1f);
    public Color zAxisColor = new Color(0.3f, 0.4f, 1f, 1f);
    
    [Header("Grid Settings")]
    public bool showGrid = true;
    public float gridLineThickness = 0.01f;
    public Color gridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public bool showHalfMeterMarks = true;
    
    [Header("Mark Settings")]
    public bool showMarks = true;
    public float meterMarkLength = 0.2f;
    public float halfMeterMarkLength = 0.1f;
    public float markThickness = 0.015f;
    public Color markColor = new Color(1f, 1f, 1f, 0.8f);
    
    [Header("Label Settings")]
    public bool showLabels = true;
    public float labelSize = 0.2f;
    public Color labelColor = new Color(1f, 1f, 1f, 0.8f);
    public float labelOffset = 0.1f;
    public Font labelFont;
    
    [Header("Polar Coordinate Settings")]
    public bool showPolarCircles = true;
    public int maxCircleRadius = 4;
    public int circleSegments = 60;
    public float circleThickness = 0.02f;
    public Color circleColor = new Color(0.5f, 0.9f, 0.5f, 0.6f);
    
    // Internal variables
    private List<GameObject> gridObjects = new List<GameObject>();
    private Transform gridParent;
    private Material lineMaterial;
    private Material textMaterial;
    
    void OnEnable()
    {
        // Initialize only in editor or if showAtRuntime is enabled
        if (Application.isPlaying && !showAtRuntime)
            return;
        
        CreateMaterials();
        CreateGridParent();
        RegenerateGrid();
    }
    
    void OnDisable()
    {
        ClearGrid();
    }
    
    void OnValidate()
    {
        // When settings change in the Inspector
        if (enabled)
        {
            ClearGrid();
            CreateMaterials();
            CreateGridParent();
            RegenerateGrid();
        }
    }
    
    void CreateMaterials()
    {
        // Create material for lines
        Shader lineShader = Shader.Find("Unlit/Color");
        if (lineShader == null)
            lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (lineShader == null)
            lineShader = Shader.Find("Hidden/Internal-Colored");
            
        lineMaterial = new Material(lineShader);
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.enableInstancing = false;
        lineMaterial.renderQueue = 3000;
        
        // Create material for text
        textMaterial = new Material(Shader.Find("TextMeshPro/Distance Field"));
        if (textMaterial == null)
            textMaterial = new Material(Shader.Find("GUI/Text Shader"));
    }
    
    void CreateGridParent()
    {
        // Create a parent container for all grid objects
        Transform existingParent = transform.Find("VoidGridElements");
        if (existingParent != null)
        {
            gridParent = existingParent;
        }
        else
        {
            GameObject parentObj = new GameObject("VoidGridElements");
            parentObj.transform.SetParent(transform, false);
            gridParent = parentObj.transform;
        }
    }
    
    void ClearGrid()
    {
        // Delete all existing grid objects
        foreach (GameObject obj in gridObjects)
        {
            if (Application.isEditor)
                DestroyImmediate(obj);
            else
                Destroy(obj);
        }
        
        gridObjects.Clear();
    }
    
    void RegenerateGrid()
    {
        // Create the main axes
        if (showAxes)
        {
            CreateAxis(Vector3.right, Vector3.left, gridSize, axisThickness, xAxisColor, "X-Axis");
            CreateAxis(Vector3.forward, Vector3.back, gridSize, axisThickness, zAxisColor, "Z-Axis");
        }
        
        // Create the grid
        if (showGrid)
        {
            // X-lines (parallel to X-axis)
            for (int i = 1; i <= gridSize; i++)
            {
                Color lineColor = gridLineColor;
                float thickness = gridLineThickness;
                
                // Positive Z
                CreateLine(
                    new Vector3(-gridSize, 0, i), 
                    new Vector3(gridSize, 0, i), 
                    thickness, 
                    lineColor, 
                    $"GridLine_Z+{i}"
                );
                
                // Negative Z
                CreateLine(
                    new Vector3(-gridSize, 0, -i), 
                    new Vector3(gridSize, 0, -i), 
                    thickness, 
                    lineColor, 
                    $"GridLine_Z-{i}"
                );
            }
            
            // Z-lines (parallel to Z-axis)
            for (int i = 1; i <= gridSize; i++)
            {
                Color lineColor = gridLineColor;
                float thickness = gridLineThickness;
                
                // Positive X
                CreateLine(
                    new Vector3(i, 0, -gridSize), 
                    new Vector3(i, 0, gridSize), 
                    thickness, 
                    lineColor, 
                    $"GridLine_X+{i}"
                );
                
                // Negative X
                CreateLine(
                    new Vector3(-i, 0, -gridSize), 
                    new Vector3(-i, 0, gridSize), 
                    thickness, 
                    lineColor, 
                    $"GridLine_X-{i}"
                );
            }
        }
        
        // Create marks and labels for meters
        if (showMarks)
        {
            // X-axis marks
            for (int i = -Mathf.FloorToInt(gridSize); i <= Mathf.FloorToInt(gridSize); i++)
            {
                if (i == 0) continue; // Skip origin
                
                // Meter mark on X-axis
                CreateMark(
                    new Vector3(i, 0, 0),
                    Vector3.forward,
                    meterMarkLength,
                    markThickness,
                    markColor,
                    $"XMark_{i}"
                );
                
                // Label for meter mark
                if (showLabels)
                {
                    CreateLabel(
                        new Vector3(i, 0, -labelOffset - meterMarkLength/2),
                        i.ToString(),
                        labelSize,
                        labelColor,
                        $"XLabel_{i}"
                    );
                }
                
                // Half-meter marks
                if (showHalfMeterMarks && i < Mathf.FloorToInt(gridSize))
                {
                    CreateMark(
                        new Vector3(i + 0.5f, 0, 0),
                        Vector3.forward,
                        halfMeterMarkLength,
                        markThickness * 0.8f,
                        markColor,
                        $"XHalfMark_{i}.5"
                    );
                }
            }
            
            // Z-axis marks
            for (int i = -Mathf.FloorToInt(gridSize); i <= Mathf.FloorToInt(gridSize); i++)
            {
                if (i == 0) continue; // Skip origin
                
                // Meter mark on Z-axis
                CreateMark(
                    new Vector3(0, 0, i),
                    Vector3.right,
                    meterMarkLength,
                    markThickness,
                    markColor,
                    $"ZMark_{i}"
                );
                
                // Label for meter mark
                if (showLabels)
                {
                    CreateLabel(
                        new Vector3(-labelOffset - meterMarkLength/2, 0, i),
                        i.ToString(),
                        labelSize,
                        labelColor,
                        $"ZLabel_{i}"
                    );
                }
                
                // Half-meter marks
                if (showHalfMeterMarks && i < Mathf.FloorToInt(gridSize))
                {
                    CreateMark(
                        new Vector3(0, 0, i + 0.5f),
                        Vector3.right,
                        halfMeterMarkLength,
                        markThickness * 0.8f,
                        markColor,
                        $"ZHalfMark_{i}.5"
                    );
                }
            }
        }
        
        // Polar coordinates (unit circles)
        if (showPolarCircles)
        {
            for (int radius = 1; radius <= maxCircleRadius; radius++)
            {
                CreateCircle(
                    Vector3.zero,
                    radius,
                    circleSegments,
                    circleThickness,
                    circleColor,
                    $"PolarCircle_{radius}"
                );
                
                // Label for the radius
                if (showLabels)
                {
                    CreateLabel(
                        new Vector3(radius * 0.7071f, 0, radius * 0.7071f), // Position at 45°
                        radius.ToString() + "m",
                        labelSize,
                        circleColor,
                        $"RadiusLabel_{radius}"
                    );
                }
            }
        }
    }
    
    void CreateAxis(Vector3 direction, Vector3 oppositeDirection, float length, float thickness, Color color, string name)
    {
        // Create an axis in both directions
        CreateLine(direction * length, Vector3.zero, thickness, color, name + "_Positive");
        CreateLine(Vector3.zero, oppositeDirection * length, thickness, color, name + "_Negative");
    }
    
    void CreateLine(Vector3 start, Vector3 end, float thickness, Color color, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(gridParent, false);
        
        // Create mesh for the line
        MeshFilter mf = lineObj.AddComponent<MeshFilter>();
        MeshRenderer mr = lineObj.AddComponent<MeshRenderer>();
        
        // Create a 3D line as a narrow rectangle
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        
        Vector3[] vertices = new Vector3[4];
        vertices[0] = start + perpendicular * thickness / 2;
        vertices[1] = end + perpendicular * thickness / 2;
        vertices[2] = end - perpendicular * thickness / 2;
        vertices[3] = start - perpendicular * thickness / 2;
        
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        mf.mesh = mesh;
        
        // Assign material
        Material mat = new Material(lineMaterial);
        mat.color = color;
        mr.material = mat;
        
        // Add object to list
        gridObjects.Add(lineObj);
    }
    
    void CreateMark(Vector3 position, Vector3 direction, float length, float thickness, Color color, string name)
    {
        Vector3 start = position - direction * (length / 2);
        Vector3 end = position + direction * (length / 2);
        
        CreateLine(start, end, thickness, color, name);
    }
    
    void CreateLabel(Vector3 position, string text, float size, Color color, string name)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(gridParent, false);
        labelObj.transform.position = position;
        
        // Adjust rotation to make text readable
        labelObj.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        labelObj.transform.Rotate(90f, 0f, 0f, Space.Self);
        
        // Add TextMeshPro component if available
        TextMeshPro textMesh = labelObj.AddComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = size * 10;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableAutoSizing = true;
            textMesh.fontSizeMin = size * 5;
            textMesh.fontSizeMax = size * 15;
        }
        else
        {
            // Fallback to regular TextMesh
            TextMesh textMeshBasic = labelObj.AddComponent<TextMesh>();
            textMeshBasic.text = text;
            textMeshBasic.color = color;
            textMeshBasic.fontSize = Mathf.RoundToInt(size * 100);
            textMeshBasic.alignment = TextAlignment.Center;
            textMeshBasic.anchor = TextAnchor.MiddleCenter;
            
            if (labelFont != null)
                textMeshBasic.font = labelFont;
                
            MeshRenderer mr = labelObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = textMaterial;
            }
        }
        
        // Scale to adjust size
        labelObj.transform.localScale = new Vector3(size, size, size);
        
        // Add object to list
        gridObjects.Add(labelObj);
    }
    
    void CreateCircle(Vector3 center, float radius, int segments, float thickness, Color color, string name)
    {
        GameObject circleObj = new GameObject(name);
        circleObj.transform.SetParent(gridParent, false);
        
        MeshFilter mf = circleObj.AddComponent<MeshFilter>();
        MeshRenderer mr = circleObj.AddComponent<MeshRenderer>();
        
        // Create a circle mesh
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];
        Vector2[] uvs = new Vector2[segments * 2];
        
        float angleStep = 2f * Mathf.PI / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float nextAngle = (i + 1) % segments * angleStep;
            
            // Outer and inner points of the ring segment
            Vector3 outerPoint = new Vector3(Mathf.Sin(angle) * (radius + thickness/2), 0, Mathf.Cos(angle) * (radius + thickness/2));
            Vector3 innerPoint = new Vector3(Mathf.Sin(angle) * (radius - thickness/2), 0, Mathf.Cos(angle) * (radius - thickness/2));
            
            vertices[i * 2] = center + outerPoint;
            vertices[i * 2 + 1] = center + innerPoint;
            
            uvs[i * 2] = new Vector2((float)i / segments, 0);
            uvs[i * 2 + 1] = new Vector2((float)i / segments, 1);
            
            // Triangles
            int baseIndex = i * 2;
            int nextBaseIndex = ((i + 1) % segments) * 2;
            
            triangles[i * 6] = baseIndex;
            triangles[i * 6 + 1] = nextBaseIndex;
            triangles[i * 6 + 2] = baseIndex + 1;
            
            triangles[i * 6 + 3] = baseIndex + 1;
            triangles[i * 6 + 4] = nextBaseIndex;
            triangles[i * 6 + 5] = nextBaseIndex + 1;
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        mf.mesh = mesh;
        
        // Assign material
        Material mat = new Material(lineMaterial);
        mat.color = color;
        mr.material = mat;
        
        // Add object to list
        gridObjects.Add(circleObj);
    }
    
    // Update is only used in editor to update the grid if the parent transform changes
    void Update()
    {
        if (!Application.isPlaying || showAtRuntime)
        {
            // Could implement grid animation here
        }
    }
}