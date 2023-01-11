using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildGrid : MonoBehaviour
{
    enum BlockType
    {
        None,
        One,
        Two,
        Three
    }

    [Range(0.1f, 10f)]
    public float size = 1f;
    float oldSize = -1;
    [Range(1, 100)]
    public int range = 5;
    int Range { get { return range - 1; } }

    [Range(1, 100)]
    public int mouseRange = 1;
    public LayerMask layerMask;

    [Range(1, 100)]
    public int subdivisions = 4;
    int oldSubdivisions = -1;


    public bool showGrid = false;

    public GameObject plane;

    Hex centerHex;
    public Hex HoveredHex { get; private set; }
    BlockType[,,] blocks;

    void Awake()
    {
        centerHex = new(0, 0);
        //blocks = new BlockType[sizeX, sizeY, sizeZ];
        var length = Hex.GetHexMaxSide(Range, size) / 10f;
        UpdatePlaneMesh(length);
    }

    void Update()
    {
        var length = Hex.GetHexMaxSide(Range, size) / 10f;
        //Vector3 scale = plane.transform.localScale;
        //scale.x = length;
        //scale.z = length;
        //plane.transform.localScale = scale;
        if(oldSize != size || oldSubdivisions != subdivisions)
        {
            oldSize = size;
            oldSubdivisions = subdivisions;
            UpdatePlaneMesh(length*10);
        }

        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool changed = false;
        if (Physics.Raycast(rayOrigin, out hitInfo, Mathf.Infinity, layerMask))
        {
            Hex tempHex = Hex.PointToHex(new(hitInfo.point.x, hitInfo.point.z), size);
            if (centerHex.IsInRange(tempHex, Range))
            {
                HoveredHex = tempHex;
                changed = true;
            }
        }
        if(!changed)
        {
            HoveredHex = null;
        }
    }

    void UpdatePlaneMesh(float length)
    {
        Mesh planeMesh = GeneratePlane(length, subdivisions);
        plane.GetComponent<MeshFilter>().mesh = planeMesh;
        plane.GetComponent<MeshCollider>().sharedMesh = planeMesh;
    }

    #region Mesh

    Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        return mesh;
    }

    Mesh GeneratePlane(float size, int subdivisions)
    {
        return GeneratePlane(size, subdivisions, Vector3.right, Vector3.forward);
    }

    Mesh GeneratePlane(float size, int subdivisions, Vector3 right, Vector3 up)
    {
        Mesh mesh = new Mesh();

        int vertexCount = (subdivisions + 1) * (subdivisions + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[subdivisions * subdivisions * 6];

        float halfSize = size / 2;

        int currentVertex = 0;
        int currentTriangle = 0;
        for (int i = 0; i <= subdivisions; i++)
        {
            for (int j = 0; j <= subdivisions; j++)
            {
                float x = (float)i / subdivisions * size - halfSize;
                float z = (float)j / subdivisions * size - halfSize;
                vertices[currentVertex] = x * right + z * up;
                uvs[currentVertex] = new Vector2((float)i / subdivisions, (float)j / subdivisions);
                currentVertex++;

                if (i < subdivisions && j < subdivisions)
                {
                    int topLeft = i * (subdivisions + 1) + j;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + subdivisions + 1;
                    int bottomRight = bottomLeft + 1;

                    triangles[currentTriangle++] = topLeft;
                    triangles[currentTriangle++] = topRight;
                    triangles[currentTriangle++] = bottomLeft;

                    triangles[currentTriangle++] = bottomLeft;
                    triangles[currentTriangle++] = topRight;
                    triangles[currentTriangle++] = bottomRight;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    #endregion

    #region Drawing

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        DrawPlane();

        // Draw mouse range
        if (HoveredHex != null)
        {
            Hex[] intersectedHex = Hex.GetHexIntersection(new Hex.RangeInfo[]
            {
                new(){
                    center = centerHex,
                    range = Range
                },
                new(){
                    center = HoveredHex,
                    range = mouseRange - 1
                }
            });
            foreach (Hex cell in intersectedHex)
            {
                DrawSolidHex(cell, Color.green);
            }
        }
        if (showGrid)
        {
            // Draw cell outlines
            Hex[] cells = centerHex.GetHexInRange(Range);
            foreach (Hex cell in cells)
            {
                DrawHex(cell, Color.blue);
            }
        }

        // Draw cell outline where mouse is.
        if(HoveredHex != null)
        {
            DrawHex(HoveredHex, Color.red);
        }
    }

    void DrawHex(Hex hex, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);
        for (int i = 0; i < 6; i++)
        {
            Vector2 corner_1 = hex.GetHexCorner(size, i);
            Vector2 corner_2 = (i == 5) ? hex.GetHexCorner(size, 0) : hex.GetHexCorner(size, i+1);
            corner_1 += new Vector2(transform.position.x, transform.position.z);
            corner_2 += new Vector2(transform.position.x, transform.position.z);
            GL.Vertex3(corner_1.x, transform.position.y, corner_1.y);
            GL.Vertex3(corner_2.x, transform.position.y, corner_2.y);
        }
        GL.End();
    }

    void DrawSolidHex(Hex hex, Color color)
    {
        Vector2 corner2D = hex.GetHexCorner(size, 0); ;
        Vector3 corner = transform.position + new Vector3(corner2D.x, 0, corner2D.y);

        GL.Begin(GL.QUADS);
        GL.Color(color);
        GL.Vertex3(corner.x, corner.y, corner.z);
        for (int i = 5; i >= 0; i--)
        {
            corner2D = hex.GetHexCorner(size, i);
            corner = transform.position + new Vector3(corner2D.x, 0, corner2D.y);
            GL.Vertex3(corner.x, corner.y, corner.z);
            if(i == 3) GL.Vertex3(corner.x, corner.y, corner.z);
        }
        
        GL.End();
    }

    void DrawPlane()
    {
        float maxSide = Hex.GetHexMaxSide(Range, size);
        float halfSide = maxSide / 2;

        Vector3 topRight = new(halfSide, 0, halfSide);
        Vector3 bottomRight = new(halfSide, 0, -halfSide);
        Vector3 bottomLeftt = new(-halfSide, 0, -halfSide);
        Vector3 topLeft = new(-halfSide, 0, halfSide);

        topRight += transform.position;
        bottomRight += transform.position;
        bottomLeftt += transform.position;
        topLeft += transform.position;

        GL.Begin(GL.LINES);
        GL.Color(Color.yellow);

        GL.Vertex3(topRight.x, topRight.y, topRight.z);
        GL.Vertex3(bottomRight.x, bottomRight.y, bottomRight.z);

        GL.Vertex3(bottomRight.x, bottomRight.y, bottomRight.z);
        GL.Vertex3(bottomLeftt.x, bottomLeftt.y, bottomLeftt.z);

        GL.Vertex3(bottomLeftt.x, bottomLeftt.y, bottomLeftt.z);
        GL.Vertex3(topLeft.x, topLeft.y, topLeft.z);

        GL.Vertex3(topLeft.x, topLeft.y, topLeft.z);
        GL.Vertex3(topRight.x, topRight.y, topRight.z);

        GL.End();
    }

    #endregion
}