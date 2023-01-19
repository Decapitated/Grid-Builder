using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

struct MeshData
{
    public List<Vector3> vertices;
    public List<UnityEngine.Vector2> uv;
    public List<UnityEngine.Vector2> uv2;
    public List<Vector3> normals;
    public List<int> triangles;
}

public class Test2 : MonoBehaviour
{
    // Max vertices 65,000/mesh. Max range = 34 = 64,278 vertices; 35 = 68,058 vertices;
    [Range(0, 34)]
    public int range = 1;
    int oldRange = -1;

    [Range(0.1f, 10f)]
    public float scale;
    float oldScale = -1f;

    Hex Center => new(0f, 0f);

    // Variables for threaded work.
    bool workStarted = false;
    bool workDone = false;
    MeshData meshData;

    void Awake()
    {
        workStarted = true;
        new Thread(Generate).Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(workDone)
        {
            Mesh mesh = new()
            {
                vertices = meshData.vertices.ToArray(),
                uv = meshData.uv.ToArray(),
                normals = meshData.normals.ToArray(),
                triangles = meshData.triangles.ToArray()
            };
            if (meshData.uv2 != null) mesh.uv2 = meshData.uv2.ToArray();
            //mesh.Optimize();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;

            workStarted = false;
            workDone = false;
        }
    }

    void Generate()
    {
        var random = GenerateRandom();
        random = SplitShapes(random);
        meshData = ObjectArrayToMesh(random);
        workDone = true;
    }

    List<object> GenerateRandom()
    {
        List<object> quads = new();
        List<Triangle> usedTriangles = new();
        var rand = new System.Random();
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

    void DrawQuad(Quad quad)
    {
        var points = quad.GetPoints2();
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[3], points[0]);
    }

    void DrawTriangle(Triangle triangle)
    {
        var a = PointToWorld(triangle.A);
        var b = PointToWorld(triangle.B);
        var c = PointToWorld(triangle.C);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, a);
    }

    #endregion

    Vector3 PointToWorld(Vector2 point) => transform.position + (Vector3)point;
}
