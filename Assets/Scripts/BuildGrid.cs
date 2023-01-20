using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
    float oldRange = -1;
    public int Range { get { return range - 1; } }

    [Range(1, 100)]
    public int mouseRange = 1;
    public LayerMask layerMask;

    [Range(1, 100)]
    public int subdivisions = 4;
    int oldSubdivisions = -1;
    [Range(4, 100)]
    public int circleSteps = 10;


    public bool showGrid = false;

    public GameObject plane;

    Hex centerHex;
    public Hex HoveredHex { get; private set; }
    BlockType[,,] blocks;

    ThreadStart meshWork;
    Thread meshThread;
    bool meshWorkStarted = false;
    bool meshWorkDone = false;
    MeshData meshWorkData;
    Triangle boundingTriangle;
    List<Quad> GLOBAL_QUADS;

    void Awake()
    {
        centerHex = new(0, 0);
        //blocks = new BlockType[sizeX, sizeY, sizeZ];
        var length = Hex.GetHexMaxSide(Range, size) / 10f;
        meshWork = UpdatePlaneMesh;
    }

    void Update()
    {
        //Vector3 scale = plane.transform.localScale;
        //scale.x = length;
        //scale.z = length;
        //plane.transform.localScale = scale;

        if (!meshWorkStarted && (oldSize != size || oldSubdivisions != subdivisions || oldRange != range))
        {
            oldSize = size;
            oldSubdivisions = subdivisions;
            oldRange = range;
            //UpdatePlaneMesh(Hex.GetHexMaxSide(Range, size));
            meshThread = new(meshWork);
            meshWorkStarted = true;
            meshThread.Start();
        }
        if (meshWorkDone)
        {
            Mesh mesh = new()
            {
                vertices = meshWorkData.vertices.ToArray(),
                uv = meshWorkData.uv.ToArray(),
                normals = meshWorkData.normals.ToArray(),
                triangles = meshWorkData.triangles.ToArray()
            };
            if (meshWorkData.uv2 != null) mesh.uv2 = meshWorkData.uv2.ToArray();
            mesh.Optimize();

            plane.GetComponent<MeshFilter>().mesh = mesh;
            plane.GetComponent<MeshCollider>().sharedMesh = mesh;

            meshWorkStarted = false;
            meshWorkDone = false;
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
        if (!changed)
        {
            HoveredHex = null;
        }
    }

    //void UpdatePlaneMesh(float length)
    void UpdatePlaneMesh()
    {
        GenerateBowyerMesh();
        //Mesh planeMesh = GenerateMesh();
        //Mesh planeMesh = GeneratePlane(length, subdivisions);
        //plane.GetComponent<MeshFilter>().mesh = planeMesh;
        //plane.GetComponent<MeshCollider>().sharedMesh = planeMesh;
        //var planeRenderer = plane.GetComponent<MeshRenderer>();
        //planeRenderer.material.SetFloat("_RippleDensity", length);
        meshWorkDone = true;
    }

    #region Mesh

    Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        float test_2_start = Time.realtimeSinceStartup;
        MeshData data = GetMeshData();
        float test_2_total = Time.realtimeSinceStartup - test_2_start;

        Debug.Log("Time: " + test_2_total);

        mesh.vertices = data.vertices.ToArray();
        mesh.uv = data.uv.ToArray();
        mesh.normals = data.normals.ToArray();
        mesh.triangles = data.triangles.ToArray();

        mesh.Optimize();

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
        //mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    struct MeshData
    {
        public List<Vector3> vertices;
        public List<UnityEngine.Vector2> uv;
        public List<UnityEngine.Vector2> uv2;
        public List<Vector3> normals;
        public List<int> triangles;
    }
    MeshData GetMeshData()
    {
        List<Vector3> vertices = new();
        Dictionary<Vector3, int> vertIndices = new();
        List<UnityEngine.Vector2> uvs = new();
        List<Vector3> normals = new();
        List<int> triangles = new();

        // Draw cell outlines
        List<Hex> cells = centerHex.GetHexSpiralInRange(Range);

        foreach (Hex cell in cells)
        {
            var center = cell.GetHexCenter(size);
            Vector3 tempVert = new(center.X, 0, center.Y);
            vertices.Add(tempVert);
            vertIndices.Add(tempVert, vertices.Count - 1);
            normals.Add(Vector3.up);
            uvs.Add(new(center.X, center.Y));
            var cellCenterIndex = vertices.Count - 1;
            for (int i = 0; i < 6; i++)
            {
                var corner_1 = cell.GetHexCorner(size, i);
                tempVert = new(corner_1.X, 0, corner_1.Y);
                int secondIndex = vertIndices.GetValueOrDefault(tempVert, -1);
                if (secondIndex == -1)
                {
                    vertices.Add(tempVert);
                    vertIndices.Add(tempVert, vertices.Count - 1);
                    normals.Add(Vector3.up);
                    uvs.Add(new(corner_1.X, corner_1.Y));
                    secondIndex = vertices.Count - 1;
                }

                var corner_2 = cell.GetHexCorner(size, (i + 1) % 6);
                tempVert = new(corner_2.X, 0, corner_2.Y);
                int thirdIndex = vertIndices.GetValueOrDefault(tempVert, -1);
                if (thirdIndex == -1)
                {
                    vertices.Add(tempVert);
                    vertIndices.Add(tempVert, vertices.Count - 1);
                    normals.Add(Vector3.up);
                    uvs.Add(new(corner_2.X, corner_2.Y));
                    thirdIndex = vertices.Count - 1;
                }

                triangles.Add(thirdIndex);
                triangles.Add(secondIndex);
                triangles.Add(cellCenterIndex);
            }
        }

        return new()
        {
            vertices = vertices,
            uv = uvs,
            normals = normals,
            triangles = triangles
        };
    }

    void GetTriangles(Hex cell, List<Triangle> triangles)
    {
        var center = cell.GetHexCenter(size);
        for (int i = 0; i < 6; i++)
        {
            var corner_1 = cell.GetHexCorner(size, i);
            var corner_2 = cell.GetHexCorner(size, (i + 1) % 6);
            triangles.Add(new(center, corner_1, corner_2));
        }
    }

    List<Triangle> GetTriangles(Hex cell)
    {
        List<Triangle> triangles = new();
        GetTriangles(cell, triangles);
        return triangles;
    }

    void GetTriangles(List<Hex> cells, List<Triangle> triangles)
    {
        foreach (Hex cell in cells)
        {
            GetTriangles(cell, triangles);
        }
    }

    List<Triangle> GetTriangles(List<Hex> cells)
    {
        List<Triangle> triangles = new();
        GetTriangles(cells, triangles);
        return triangles;
    }

    List<object> GetRandomQuads(List<Hex> cells)
    {
        List<object> quads = new();
        List<Triangle> usedTriangles = new();
        var rand = new System.Random();
        foreach (var cell in cells)
        {
            for (int i = 0; i < 6; i++)
            {
                Triangle curTriangle = cell.GetTriangle(size, i);
                if (usedTriangles.Contains(curTriangle)) continue;
                Triangle opposite = null;
                int randomNum = rand.Next(0, 3) - 1;
                for (int tries = 0; tries < 3; tries++)
                {
                    randomNum = (randomNum + 1) % 3;
                    Triangle temp = null;
                    if (randomNum == 0)
                    {
                        temp = cell.GetTriangle(size, (i + 6 - 1) % 6);
                    }
                    else if (randomNum == 1)
                    {
                        var neighbor = cell.GetNeighbor(i);
                        if (centerHex.IsInRange(neighbor, Range))
                            temp = neighbor.GetTriangle(size, (i + 3) % 6);
                    }
                    else if (randomNum == 2)
                    {
                        temp = cell.GetTriangle(size, (i + 6 + 1) % 6);
                    }
                    if (temp == null || usedTriangles.Contains(temp)) continue;
                    opposite = temp;
                    break;
                }
                if (opposite == null)
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

    List<Quad> SplitQuads(List<object> objects)
    {
        List<Quad> result = new();
        foreach (var obj in objects)
        {
            if (obj.GetType() == typeof(Quad))
                result.AddRange(((Quad)obj).Split());
            else if (obj.GetType() == typeof(Triangle))
                result.AddRange(((Triangle)obj).Split());
        }
        return result;
    }

    void GenerateBowyerMesh()
    {
        Debug.Log("Starting Generation.");
        List<Hex> cells = centerHex.GetHexSpiralInRange(Range);
        //List<Triangle> triangles = GetTriangles(cells);
        //TriangleArrayToMesh(triangles);
        GLOBAL_QUADS = SplitQuads(GetRandomQuads(cells));
        meshWorkData = QuadArrayToMesh(GLOBAL_QUADS);
        //TriangleArrayToMesh(BowyerWatson(GetPoints(NumPoints.Two)));
        Debug.Log("Done.");
    }

    List<Edge> GetUniqueEdges(List<Edge> edges)
    {
        List<Edge> uniqueEdges = new();
        List<Edge> sharedEdges = new();
        int i = 0;
        foreach (var edge in edges)
        {
            i++;
            if (!uniqueEdges.Contains(edge) && !sharedEdges.Contains(edge))
            {
                uniqueEdges.Add(edge);
            }
            else if (uniqueEdges.Contains(edge) && !sharedEdges.Contains(edge))
            {
                if (!uniqueEdges.Remove(edge)) Debug.Log("Failed to remove edge.");
                sharedEdges.Add(edge);
            }
        }
        Debug.Log(i + " edges checked.");
        return uniqueEdges;
    }

    List<Triangle> BowyerWatson(List<Vector2> points)
    {
        List<Triangle> triangles = new();
        Triangle superTriangle = Triangle.GetSuperTriangle(points);
        boundingTriangle = superTriangle;
        triangles.Add(superTriangle);
        foreach (var point in points)
        {
            List<Triangle> badTriangles = new();
            foreach (Triangle triangle in triangles)
            {
                if (triangle.IsPointInCircumCircle(point))
                {
                    badTriangles.Add(triangle);
                    if (triangle.Equals(superTriangle)) Debug.Log("Super Triangle added to Bad Triangles.");
                }
            }

            List<Edge> polygon = new();
            foreach (Triangle triangle in badTriangles)
            {
                if (!triangles.Remove(triangle)) Debug.Log("Failed to remove triangle. (1)");
                foreach (var edge in triangle.GetEdges())
                {
                    polygon.Add(edge);
                }
            }
            List<Edge> uniqueEdges = GetUniqueEdges(polygon);
            Debug.Log(uniqueEdges.Count + " unique edges.");

            foreach (var edge in uniqueEdges)
            {
                triangles.Add(new(edge.A, edge.B, point));
            }
        }
        var temp = triangles.ToArray();
        foreach (var triangle in temp)
        {
            if (triangle.A == superTriangle.A || triangle.A == superTriangle.B || triangle.A == superTriangle.C ||
                triangle.B == superTriangle.A || triangle.B == superTriangle.B || triangle.B == superTriangle.C ||
                triangle.C == superTriangle.A || triangle.C == superTriangle.B || triangle.C == superTriangle.C)
            {
                if (!triangles.Remove(triangle)) Debug.Log("Failed to remove triangle. (2)");
            }
        }
        return triangles;
    }

    void TriangleArrayToMesh(List<Triangle> triangles)
    {
        List<Vector3> vertices = new();
        List<UnityEngine.Vector2> uvs = new();
        List<Vector3> normals = new();
        List<int> m_triangles = new();

        Dictionary<Vector3, int> vertIndices = new();

        foreach (Triangle tri in triangles)
        {
            AddTriangleToMeshArrays(
                tri,
                vertIndices,
                vertices,
                uvs,
                normals,
                m_triangles
            );
        }
        meshWorkData = new()
        {
            vertices = vertices,
            uv = uvs,
            normals = normals,
            triangles = m_triangles
        };
    }

    readonly Vector2[] uvQuadCoords = new Vector2[]
    {
        new(0, 1), new(1, 1), new(1, 0), new(0, 0)
    };
    readonly Vector2[] uvTriangleCoords = new Vector2[]
    {
        new(0.5f, 1f), new(1f, 0.1f), new(0f, 0.1f)
    };
    MeshData QuadArrayToMesh(List<Quad> quads)
    {
        MeshData meshData = new MeshData()
        {
            vertices = new(),
            uv = new(),
            uv2 = new(),
            normals = new(),
            triangles = new()
        };

        foreach (Quad quad in quads)
        {
            var points = quad.GetPoints();
            if (points.Count < 4) Debug.Log("less than 4 points? "+points.Count);
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

        return meshData;
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

    // Returns indexs of vertex.
    int AddPointToMeshArrays(Vector2 point, MeshData meshData, int uv, Type uvType)
    {
        Vector3 position = new(point.X, 0, point.Y);
        meshData.vertices.Add(position);
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

    void AddTriangleToMeshArrays(Triangle tri, Dictionary<Vector3, int> vertIndices, List<Vector3> vertices, List<UnityEngine.Vector2> uvs, List<Vector3> normals, List<int> triangles)
    {
        Vector3 a = new(tri.A.X, 0, tri.A.Y);
        int a_index = vertIndices.GetValueOrDefault(a, -1);
        if (a_index == -1)
        {
            vertices.Add(a);
            a_index = vertices.Count - 1;
            vertIndices.Add(a, a_index);
            normals.Add(Vector3.up);
            uvs.Add(new(tri.A.X, tri.A.Y));
        }

        Vector3 b = new(tri.B.X, 0, tri.B.Y);
        int b_index = vertIndices.GetValueOrDefault(b, -1);
        if (b_index == -1)
        {
            vertices.Add(b);
            b_index = vertices.Count - 1;
            vertIndices.Add(b, b_index);
            normals.Add(Vector3.up);
            uvs.Add(new(tri.B.X, tri.B.Y));
        }

        Vector3 c = new(tri.C.X, 0, tri.C.Y);
        int c_index = vertIndices.GetValueOrDefault(c, -1);
        if (c_index == -1)
        {
            vertices.Add(c);
            c_index = vertices.Count - 1;
            vertIndices.Add(c, c_index);
            normals.Add(Vector3.up);
            uvs.Add(new(tri.C.X, tri.C.Y));
        }

        triangles.Add(a_index);
        triangles.Add(b_index);
        triangles.Add(c_index);
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
    [Range(0, 25)]
    public int testNum = 0;
    public bool drawQuads = false;
    public bool IsSolid = false;
    void OnRenderObject()
    {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        DrawPlane();

        // Draw mouse range
        if (HoveredHex != null)
        {
            List<Hex> intersectedHex = Hex.GetHexIntersection(new Hex.RangeInfo[]
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

        // Draw cell outlines
        /*
        foreach (Hex cell in cells)
        {
            if (showGrid) DrawHex(cell, Color.blue);
            foreach (var quad in GetQuads(GetTriangles(cell)))
            {
                DrawQuad(quad, Color.black);
            }
        }*/
        //DrawHex(centerHex.GetNeighbor(testNum % 6), Color.cyan);
        //DrawTriangle(centerHex.GetTriangle(size, testNum % 6), Color.red);
        if (drawQuads && GLOBAL_QUADS != null)
        {
            foreach (var obj in GLOBAL_QUADS)
            {
                if (obj.GetType() == typeof(Quad))
                {
                    if (IsSolid) DrawSolidQuad((Quad)obj, Color.black);
                    DrawQuad((Quad)obj, Color.yellow);
                }
                /*
                else if (obj.GetType() == typeof(Triangle))
                {
                    if (IsSolid) DrawSolidTriangle((Triangle)obj, Color.white);
                    DrawTriangle((Triangle)obj, Color.yellow);
                }*/
            }
        }
        // Draw cell outline where mouse is.
        if (HoveredHex != null) DrawHex(HoveredHex, Color.red);
    }

    void DrawHex(Hex hex, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);
        for (int i = 0; i < 6; i++)
        {
            Vector2 corner_1 = hex.GetHexCorner(size, i);
            Vector2 corner_2 = (i == 5) ? hex.GetHexCorner(size, 0) : hex.GetHexCorner(size, i + 1);
            corner_1 += new Vector2(transform.position.x, transform.position.z);
            corner_2 += new Vector2(transform.position.x, transform.position.z);
            GL.Vertex3(corner_1.X, transform.position.y, corner_1.Y);
            GL.Vertex3(corner_2.X, transform.position.y, corner_2.Y);
        }
        GL.End();
    }

    void DrawSolidHex(Hex hex, Color color)
    {
        Vector2 corner2D = hex.GetHexCorner(size, 0); ;
        Vector3 corner = transform.position + new Vector3(corner2D.X, 0, corner2D.Y);

        GL.Begin(GL.QUADS);
        GL.Color(color);
        GL.Vertex3(corner.x, corner.y, corner.z);
        for (int i = 5; i >= 0; i--)
        {
            corner2D = hex.GetHexCorner(size, i);
            corner = transform.position + new Vector3(corner2D.X, 0, corner2D.Y);
            GL.Vertex3(corner.x, corner.y, corner.z);
            if (i == 3) GL.Vertex3(corner.x, corner.y, corner.z);
        }

        GL.End();
    }

    void DrawTriangle(Triangle triangle, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);

        foreach (Edge edge in triangle.GetEdges())
        {
            GL.Vertex3(edge.A.X, 0, edge.A.Y);
            GL.Vertex3(edge.B.X, 0, edge.B.Y);
        }

        GL.End();
    }

    void DrawSolidTriangle(Triangle triangle, Color color)
    {
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        GL.Vertex3(triangle.A.X, 0, triangle.A.Y);
        GL.Vertex3(triangle.B.X, 0, triangle.B.Y);
        GL.Vertex3(triangle.C.X, 0, triangle.C.Y);

        GL.End();
    }

    void DrawQuad(Quad quad, Color color)
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(color);

        List<Vector2> points = quad.GetPoints();
        var firstPos = transform.position + new Vector3(points[0].X, 0, points[0].Y);
        foreach (var point in points)
        {
            var pos = transform.position + new Vector3(point.X, 0, point.Y);
            GL.Vertex3(pos.x, pos.y, pos.z);
        }
        GL.Vertex3(firstPos.x, firstPos.y, firstPos.z);

        GL.End();
    }

    void DrawSolidQuad(Quad quad, Color color)
    {
        GL.Begin(GL.QUADS);
        GL.Color(color);

        List<Vector2> points = quad.GetPoints();
        foreach (var point in points)
        {
            var pos = transform.position + new Vector3(point.X, 0, point.Y);
            GL.Vertex3(pos.x, pos.y, pos.z);
        }

        GL.End();
    }

    void DrawCircle(Circle circle, float steps, Color color)
    {
        GL.Begin(GL.LINES);
        GL.Color(color);
        UnityEngine.Vector2 oldPoint = new();
        for (float theta = 0f; theta < 2 * Mathf.PI + ((2 * Mathf.PI) / steps); theta += ((2 * Mathf.PI) / steps))
        {
            UnityEngine.Vector2 current = new UnityEngine.Vector2(circle.Center.X, circle.Center.Y) + new UnityEngine.Vector2(circle.Radius * Mathf.Cos(theta), circle.Radius * Mathf.Sin(theta));

            if (theta > 0f)
            {
                GL.Vertex3(oldPoint.x, 0, oldPoint.y);
                GL.Vertex3(current.x, 0, current.y);
            }
            oldPoint = current;
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