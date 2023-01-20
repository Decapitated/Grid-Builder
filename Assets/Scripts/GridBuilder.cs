using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static UnityEngine.Mesh;
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

    public int read = 1;

    // Layer mask for raycasts.
    public LayerMask layerMask;

    public Color hoverColor = Color.green;

    public GameObject facePrefab;
    public Shader faceShader;

    Hex Center => new(0f, 0f);

    public Vector3 MouseHover { get; private set; }
    public Vector2 MouseClosestFace { get; private set; }

    // Variables for threaded work.
    bool workStarted = false;
    bool workDone = false;
    MeshData graphMeshData;
    //MeshData dualGraphMeshData;
    Dictionary<Vector2, object> dualGraph;

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

        Raycast();
    }

    void Raycast()
    {
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayOrigin, out hitInfo, Mathf.Infinity, layerMask))
        {
            MouseHover = hitInfo.point;
            Vector2 hitPoint = MouseHover;
            Vector2 closestFace = null;
            float champDist = float.PositiveInfinity;
            foreach (var pair in dualGraph)
            {
                if (closestFace is null)
                {
                    closestFace = pair.Key;
                    champDist = hitPoint.Distance(closestFace);
                    continue;
                }
                float tempDist = hitPoint.Distance(pair.Key);
                if (tempDist < champDist)
                {
                    champDist = tempDist;
                    closestFace = pair.Key;
                }
            }
            MouseClosestFace = closestFace;
        }
        else if (MouseClosestFace is not null) MouseClosestFace = null;
    }

    #region ThreadWork

    void WorkDone()
    {
        Mesh mesh = MeshDataToMesh(graphMeshData);
        mesh.Optimize();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        graphMeshData = new(); // Garbage collect. may change?
        
        // Clear children.
        foreach(Transform child in transform) Destroy(child.gameObject);
        if(dualGraph is not null)
            foreach(var pair in dualGraph)
            {
                GameObject faceObj = Instantiate(facePrefab, transform, false);
                faceObj.name = "Face";
                faceObj.GetComponent<MeshFilter>().mesh = MeshDataToMesh(ObjectToMesh(pair.Value));
                faceObj.GetComponent<Renderer>().material = new Material(faceShader);
                Face faceScript = faceObj.GetComponent<Face>();
                faceScript.gridBuilder = this;
                faceScript.id = pair.Key;
            }

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
        graphMeshData = ObjectArrayToMesh(random);
        dualGraph = GetDualGraph(random);
        workDone = true;
        print("Finished generating!");
    }

    #endregion

    #region Graph Generation

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
            if(shape is Quad quad) quads.AddRange(quad.Split());
            else if (shape is Triangle triangle) quads.AddRange(triangle.Split());
        }
        return quads;
    }

    Dictionary<Vector2, object> GetDualGraph(List<object> shapes)
    {
        var vertexToShapes = GetVertexToShapes(shapes);

        Dictionary<Vector2, object> dualGraphShapes = new();

        foreach(var pair in vertexToShapes)
        {
            var sharedShapes = pair.Value;
            List<Vector2> centerPoints = new();
            foreach(var shape in sharedShapes)
            {
                centerPoints.Add(GetShapeCenter(shape));
            }
            var shapeCenter = GetCenter(centerPoints);
            if (centerPoints.Count == 3)
            {
                if (shapeCenter != pair.Key) continue;
            }
            dualGraphShapes.Add(shapeCenter, new Polygon(centerPoints, shapeCenter));
        }
        return dualGraphShapes;
    }

    Vector2 GetCenter(List<Vector2> points)
    {
        Vector2 temp = new(0, 0);
        foreach(var point in points)
        {
            temp += point;
        }
        return temp / points.Count;
    }

    Dictionary<Vector2, List<object>> GetVertexToShapes(List<object> shapes)
    {
        var vertexToShapes = new Dictionary<Vector2, List<object>>();
        foreach(var shape in shapes)
        {
            foreach(var vertex in GetShapeVertices(shape))
            {
                if (!vertexToShapes.ContainsKey(vertex))
                {
                    vertexToShapes[vertex] = new();
                }
                vertexToShapes[vertex].Add(shape);
            }
        }
        foreach(var pair in vertexToShapes.ToList())
        {
            if (pair.Value.Count < 3) vertexToShapes.Remove(pair.Key);
        }
        return vertexToShapes;
    }

    Vector2 GetShapeCenter(object shape)
    {
        if (shape is Quad quad) return quad.GetCenter();
        if (shape is Triangle triangle) return triangle.GetCenter();
        return new(1000, -1000);
    }

    List<Vector2> GetShapeVertices(object shape)
    {
        List<Vector2> points = new();
        if (shape is Quad quad) points.AddRange(quad.GetPoints());
        else if (shape is Triangle triangle) points.AddRange(triangle.GetPoints());
        return points;
    }

    #endregion

    #region Mesh

    Mesh MeshDataToMesh(MeshData meshData)
    {
        Mesh mesh = new()
        {
            vertices = meshData.vertices.ToArray(),
            uv = meshData.uv.ToArray(),
            normals = meshData.normals.ToArray(),
            triangles = meshData.triangles.ToArray()
        };
        if (meshData.uv2 is not null) mesh.uv2 = meshData.uv2.ToArray();
        return mesh;
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
            AddShapeToMeshArrays(shape, meshData);
        }

        return meshData;
    }

    MeshData ObjectToMesh(object shape)
    {
        MeshData meshData = new MeshData()
        {
            vertices = new(),
            uv = new(),
            uv2 = new(),
            normals = new(),
            triangles = new()
        };

        AddShapeToMeshArrays(shape, meshData);

        return meshData;
    }

    void AddShapeToMeshArrays(object shape, MeshData meshData)
    {
        if (shape is Polygon polygon)
        {
            foreach (var triangle in polygon.GetTriangles())
            {
                var aIndex = AddPointToMeshArrays(triangle.A, meshData, 0, typeof(Quad));
                var bIndex = AddPointToMeshArrays(triangle.B, meshData, 1, typeof(Quad));
                var cIndex = AddPointToMeshArrays(triangle.C, meshData, 4, typeof(Quad));

                meshData.triangles.Add(aIndex);
                meshData.triangles.Add(bIndex);
                meshData.triangles.Add(cIndex);
            }
        }
        else if (shape is Quad quad)
        {
            var points = quad.GetPoints(); // We dont want the points to be sorted
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
        else if (shape is Triangle triangle)
        {
            var aIndex = AddPointToMeshArrays(triangle.A, meshData, 0, typeof(Triangle));
            var bIndex = AddPointToMeshArrays(triangle.B, meshData, 1, typeof(Triangle));
            var cIndex = AddPointToMeshArrays(triangle.C, meshData, 2, typeof(Triangle));

            meshData.triangles.Add(aIndex);
            meshData.triangles.Add(bIndex);
            meshData.triangles.Add(cIndex);
        }
    }

    readonly Vector2[] uvQuadCoords = new Vector2[]
    {
        new(0, 1), new(1, 1), new(1, 0), new(0, 0), new(0.5f, 0.5f)
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

        //if (HoveredHex is not null) DrawSolidHex(HoveredHex, hoverColor);
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
