using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Bezier)),
 RequireComponent(typeof(MeshFilter)),
 RequireComponent(typeof(MeshRenderer)),
 ExecuteAlways]
public class RoadGenerator : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float roadWidth = 1f;

    [SerializeField]
    Bezier bezier;
    [SerializeField]
    MeshFilter meshFilter;

    void Start()
    {
    }

    MeshData workMeshData;

    void Update()
    {
        if(workDone)
        {
            print("Finished Generating.");
            var mesh = MeshDataToMesh(workMeshData);

            meshFilter.mesh = mesh;

            WorkStarted = false;
            workDone = false;
        }
    }

    Thread workThread  = null;
    public bool WorkStarted { get; private set; } = false;
    bool workDone = false;

    public void Generate()
    {
        if (WorkStarted) return;
        print("Generating...");
        WorkStarted = true;
        workDone = false;
        workThread = new Thread(GenerateMesh);
        workThread.Start();
    }

    [SerializeField]
    private float maxLength = 1f;
    void GenerateMesh()
    {
        MeshData meshData = new()
        {
            vertices = new(),
            uv = new(),
            normals = new(),
            triangles = new()
        };

        var quads = GetQuads();
        foreach ( var quad in quads )
        {
            var length = (quad[1] - quad[0]).magnitude;
            length /= maxLength;
            var a = AddPointToMeshData(quad[0], new(0, 0), meshData);
            var b = AddPointToMeshData(quad[1], new(0, length), meshData);
            var c = AddPointToMeshData(quad[2], new(1, length), meshData);
            var d = AddPointToMeshData(quad[3], new(1, 0), meshData);

            meshData.triangles.Add(a);
            meshData.triangles.Add(c);
            meshData.triangles.Add(b);

            meshData.triangles.Add(a);
            meshData.triangles.Add(d);
            meshData.triangles.Add(c);
        }

        workMeshData = meshData;

        workDone = true;
    }

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

    int AddPointToMeshData(Vector3 point, Vector2 uv, MeshData meshData)
    {
        meshData.vertices.Add(point);
        meshData.normals.Add(Vector3.up);
        meshData.uv.Add(uv);
        return meshData.vertices.Count - 1;
    }

    List<List<Vector3>> GetQuads()
    {
        var lines = new List<Tuple<Vector3, Vector3>>();

        var points = bezier.GetCurvePoints();

        Vector3 prevPerp = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            var dir = (i < points.Count - 1) ? points[i + 1] - points[i] : points[i] - points[i - 1];
            //var dir = (i == 0) ? points[i + 1] - points[i] : points[i] - points[i - 1];
            var perp = Vector3.Cross(Vector3.up, dir);
            if(i > 0 && i < points.Count - 1) perp = (perp + prevPerp) / 2f;

            prevPerp = perp;
            perp = perp.normalized;
            lines.Add(new(points[i] - perp * (roadWidth / 2f), points[i] + perp * (roadWidth / 2f)));
        }

        var quads = new List<List<Vector3>>();
        for (int i = 0; i < lines.Count - 1; i++)
        {
            var quad = new List<Vector3>();
            var aLine = lines[i];
            var bLine = lines[i + 1];
            quad.AddRange(new Vector3[]
            {
                aLine.Item2, bLine.Item2, bLine.Item1, aLine.Item1
            });

            quads.Add(quad);
        }

        return quads;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(RoadGenerator))]
public class RoadGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var roadGenerator = target as RoadGenerator;

        if (!roadGenerator.WorkStarted && GUILayout.Button("Generate Mesh"))
        {
            roadGenerator.Generate();
        }
    }
}

#endif