using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Icosphere : MonoBehaviour
{
    MeshFilter meshFilter;
    Renderer renderer;

    [SerializeField]
    private float sphereSize = 2;

    void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        renderer = GetComponent<Renderer>();
    }

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Generate()
    {
        print("Generating...");
        var meshData = GenerateIcosphere(sphereSize);

        Mesh mesh = new()
        {
            vertices = meshData.vertices.ToArray(),
            uv = meshData.uv.ToArray(),
            normals = meshData.normals.ToArray(),
            triangles = meshData.triangles.ToArray()
        };
        if (meshData.uv2.Count > 0) mesh.uv2 = meshData.uv2.ToArray();

        meshFilter.sharedMesh = mesh;
        print("Finished.");
    }

    struct MeshData
    {
        public List<Vector3> vertices;
        public List<Vector2> uv;
        public List<Vector2> uv2;
        public List<Vector3> normals;
        public List<int> triangles;
    }

    class Triangle
    {
        public Vector3 A, B, C;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    MeshData GenerateIcosphere(float size)
    {
        var meshData = new MeshData()
        {
            vertices = new List<Vector3>(),
            uv = new List<Vector2>(),
            uv2 = new List<Vector2>(),
            normals = new List<Vector3>(),
            triangles = new List<int>()
        };

        var goldenRatio = (1f - Mathf.Sqrt(5f)) / 2f;
        var points = GetIcospherePoints(goldenRatio, size);
        var triangles = GetIcosphereTriangles(points);
    
        foreach(var tri in triangles)
        {
            AddTriangleToMeshArrays(tri, meshData);
        }

        return meshData;
    }

    List<Vector3> GetIcospherePoints(float goldenRatio, float size = 1f) => new()
    {
        new(-size,  goldenRatio * size, 0),
        new( size,  goldenRatio * size, 0),
        new(-size, -goldenRatio * size, 0),
        new( size, -goldenRatio * size, 0),
        new(0, -size,  goldenRatio * size),
        new(0,  size,  goldenRatio * size),
        new(0, -size, -goldenRatio * size),
        new(0,  size, -goldenRatio * size),
        new( goldenRatio * size, 0, -size),
        new( goldenRatio * size, 0,  size),
        new(-goldenRatio * size, 0, -size),
        new(-goldenRatio * size, 0,  size)
    };

    List<Triangle> GetIcosphereTriangles(List<Vector3> points) => new()
    {
        new(points[0], points[11], points[5]),
        new(points[0], points[5], points[1]),
        new(points[0], points[1], points[7]),
        new(points[0], points[7], points[10]),
        new(points[0], points[10], points[11]),

        new(points[1], points[5], points[9]),
        new(points[5], points[11], points[4]),
        new(points[11], points[10], points[2]),
        new(points[10], points[7], points[6]),
        new(points[7], points[1], points[8]),

        new(points[3], points[9], points[4]),
        new(points[3], points[4], points[2]),
        new(points[3], points[2], points[6]),
        new(points[3], points[6], points[8]),
        new(points[3], points[8], points[9]),

        new(points[4], points[9], points[5]),
        new(points[2], points[4], points[11]),
        new(points[6], points[2], points[10]),
        new(points[8], points[6], points[7]),
        new(points[9], points[8], points[1])
    };

    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        Vector3 RoundVec(Vector3 v) => new(
            (float)Math.Round(v.x, 2),
            (float)Math.Round(v.y, 2),
            (float)Math.Round(v.z, 2));

        public bool Equals(Vector3 vec1, Vector3 vec2)
        {
            return (vec1 - vec2).sqrMagnitude < 0.01f;
        }

        public int GetHashCode(Vector3 vec)
        {
            var temp = RoundVec(vec);
            return HashCode.Combine(temp.x, temp.y, temp.z);
        }
    }

    void AddTriangleToMeshArrays(Triangle tri, MeshData meshData)
    {
        meshData.vertices.Add(tri.A);
        var a_index = meshData.vertices.Count - 1;
        meshData.normals.Add(tri.A.normalized);
        meshData.uv.Add(new(0, 0));

        meshData.vertices.Add(tri.B);
        var b_index = meshData.vertices.Count - 1;
        meshData.normals.Add(tri.B.normalized);
        meshData.uv.Add(new(0f, 1f));

        meshData.vertices.Add(tri.C);
        var c_index = meshData.vertices.Count - 1;
        meshData.normals.Add(tri.C.normalized);
        meshData.uv.Add(new(1f, 1f));

        meshData.triangles.Add(a_index);
        meshData.triangles.Add(b_index);
        meshData.triangles.Add(c_index);
    }
}
