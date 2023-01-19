using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = System.Random;

struct MeshData
{
    public List<Vector3> vertices;
    public List<UnityEngine.Vector2> uv;
    public List<UnityEngine.Vector2> uv2;
    public List<Vector3> normals;
    public List<int> triangles;
}

public class GridBuilder : MonoBehaviour
{
    // Max vertices 65,000/mesh. Max range = 34 = 64,278 vertices; 35 = 68,058 vertices;
    [Range(0, 34)]
    public int range = 1;
    int oldRange;

    [Range(0.1f, 10f)]
    public float scale;
    float oldScale;

    [Range(0, int.MaxValue)]
    public int seed = 0;
    int oldSeed;

    // Layer mask for raycasts.
    public LayerMask layerMask;

    public Color hoverColor = Color.green;

    Hex Center => new(0f, 0f);
    public Hex HoveredHex { get; private set; }

    // Variables for threaded work.
    bool workStarted = false;
    bool workDone = false;
    MeshData meshData;

    void Awake()
    {
        Generate();
    }

    void Start()
    {
        oldRange = range;
        oldScale = scale;
        oldSeed = seed;
    }

    // Update is called once per frame
    void Update()
    {
        // If values change regenerate.
        if (!workStarted && (oldScale != scale || oldRange != range || oldSeed != seed))
        {
            oldScale = scale;
            oldRange = range;
            oldSeed = seed;
            Generate();
        }
        if (workDone) WorkDone();

        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool changed = false;
        if (Physics.Raycast(rayOrigin, out hitInfo, Mathf.Infinity, layerMask))
        {
            Hex tempHex = Hex.PointToHex(hitInfo.point, scale);
            if (Center.IsInRange(tempHex, range))
            {
                HoveredHex = tempHex;
                changed = true;
            }
        }
        if (!changed)
        {
            HoveredHex = null;
        }
    }


    // If generation is done, assign to mesh.
    void WorkDone()
    {
        Mesh mesh = new()
        {
            vertices = meshData.vertices.ToArray(),
            uv = meshData.uv.ToArray(),
            normals = meshData.normals.ToArray(),
            triangles = meshData.triangles.ToArray()
        };
        if (meshData.uv2 != null) mesh.uv2 = meshData.uv2.ToArray();
        mesh.Optimize();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        meshData = new();
        workStarted = false;
        workDone = false;
    }

    void Generate()
    {
        workStarted = true;
        workDone = false;
        new Thread(ThreadGenerate).Start();
    }

    void ThreadGenerate()
    {
        print("Generating grid...");
        var random = GenerateRandom();
        random = SplitShapes(random);
        meshData = ObjectArrayToMesh(random);
        workDone = true;
        print("Finished generating!");
    }

    List<object> GenerateRandom()
    {
        List<object> quads = new();
        List<Triangle> usedTriangles = new();
        var rand = new Random(seed);
        foreach (var cell in Center.GetHexSpiralInRange(range))
        {
            for (int i = 0; i < 6; i++)
            {
                Triangle curTriangle = cell.GetTriangle(scale, i);
                if (usedTriangles.Contains(curTriangle)) continue;
                Triangle opposite = null;
                int randomNum = rand.Next(0, 3) - 1;
                for (int tries = 0; tries < 3; tries++)
                {
                    randomNum = (randomNum + 1) % 3;
                    Triangle temp = null;
                    if (randomNum == 0)
                    {
                        temp = cell.GetTriangle(scale, (i + 6 - 1) % 6);
                    }
                    else if (randomNum == 1)
                    {
                        var neighbor = cell.GetNeighbor(i);
                        if (Center.IsInRange(neighbor, range))
                            temp = neighbor.GetOppositeTriangle(scale, i);
                    }
                    else if (randomNum == 2)
                    {
                        temp = cell.GetTriangle(scale, (i + 6 + 1) % 6);
                    }
                    if (temp is null || usedTriangles.Contains(temp)) continue;
                    opposite = temp;
                    break;
                }
                if (opposite is null)
                {
                    quads.Add(curTriangle);
                }
                else
                {
                    quads.Add(new Quad(curTriangle, opposite));
                    usedTriangles.Add(curTriangle);
                    usedTriangles.Add(opposite);
                }
            }
        }
        return quads;
    }

    List<object> SplitShapes(List<object> shapes)
    {
        List<object> quads = new();
        foreach (var shape in shapes)
        {
            if(shape.GetType() == typeof(Quad))
                quads.AddRange(((Quad)shape).Split());
            else if (shape.GetType() == typeof(Triangle))
                quads.AddRange(((Triangle)shape).Split());
        }
        return quads;
    }

    MeshData ObjectArrayToMesh(List<object> shapes)
    {
        MeshData meshData = new MeshData()
        {
            vertices = new(),
            uv = new(),
            uv2 = new(),
            normals = new(),
            triangles = new()
        };

        foreach (object shape in shapes)
        {
            if (shape.GetType() == typeof(Quad))
            {
                var quad = (Quad)shape;
                var points = quad.GetPoints();
                var aIndex = AddPointToMeshArrays(points[0], meshData, 0, typeof(Quad));
                var bIndex = AddPointToMeshArrays(points[1], meshData, 1, typeof(Quad));
                var cIndex = AddPointToMeshArrays(points[2], meshData, 2, typeof(Quad));
                var dIndex = AddPointToMeshArrays(points[3], meshData, 3, typeof(Quad));

                meshData.triangles.Add(aIndex);
                meshData.triangles.Add(bIndex);
                meshData.triangles.Add(cIndex);

                meshData.triangles.Add(cIndex);
                meshData.triangles.Add(dIndex);
                meshData.triangles.Add(aIndex);
            }
            else if (shape.GetType() == typeof(Triangle))
            {
                var triangle = (Triangle)shape;
                var aIndex = AddPointToMeshArrays(triangle.A, meshData, 0, typeof(Triangle));
                var bIndex = AddPointToMeshArrays(triangle.B, meshData, 1, typeof(Triangle));
                var cIndex = AddPointToMeshArrays(triangle.C, meshData, 2, typeof(Triangle));

                meshData.triangles.Add(aIndex);
                meshData.triangles.Add(bIndex);
                meshData.triangles.Add(cIndex);
            }
        }

        return meshData;
    }

    readonly Vector2[] uvQuadCoords = new Vector2[]
    {
        new(0, 1), new(1, 1), new(1, 0), new(0, 0)
    };
    readonly Vector2[] uvTriangleCoords = new Vector2[]
    {
        new(0.5f, 1f), new(1f, 0.1f), new(0f, 0.1f)
    };

    // Returns indexs of vertex.
    int AddPointToMeshArrays(Vector2 point, MeshData meshData, int uv, Type uvType)
    {
        meshData.vertices.Add(point);
        meshData.normals.Add(Vector3.up);
        if (uvType == typeof(Quad))
        {
            meshData.uv.Add(uvQuadCoords[uv]);
            meshData.uv2.Add(new(1f, 0f));
        }
        else if (uvType == typeof(Triangle))
        {
            meshData.uv.Add(uvTriangleCoords[uv]);
            meshData.uv2.Add(new(0f, 0f));
        }
        return meshData.vertices.Count - 1;
    }

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

        if (HoveredHex is not null) DrawSolidHex(HoveredHex, hoverColor);
    }

    void DrawSolidHex(Hex hex, Color color)
    {
        Vector2 corner2D = hex.GetHexCorner(scale, 0); ;
        Vector3 corner = transform.position + new Vector3(corner2D.X, 0, corner2D.Y);

        GL.Begin(GL.QUADS);
        GL.Color(color);
        GL.Vertex3(corner.x, corner.y, corner.z);
        for (int i = 5; i >= 0; i--)
        {
            corner2D = hex.GetHexCorner(scale, i);
            corner = transform.position + new Vector3(corner2D.X, 0, corner2D.Y);
            GL.Vertex3(corner.x, corner.y, corner.z);
            if (i == 3) GL.Vertex3(corner.x, corner.y, corner.z);
        }

        GL.End();
    }

    #endregion
}
