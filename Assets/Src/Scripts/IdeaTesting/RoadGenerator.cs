using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter)),
 RequireComponent(typeof(MeshRenderer)),
 RequireComponent(typeof(Bezier)),
 ExecuteInEditMode]
public class RoadGenerator : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float roadWidth = 1f;
    [SerializeField]
    float textureTiling = 1f;
    int textureRepeat = 1;

    [SerializeField]
    Bezier bezier;
    [SerializeField]
    MeshFilter meshFilter;
    [SerializeField]
    bool autoGenerate = false;

    void Start()
    {
    }

    MeshData workMeshData;

    void Update()
    {
        if(autoGenerate && bezier.IsEdited)
        {
            bezier.IsEdited = false;
            Generate();
        }
        if(workDone)
        {
            print("Finished Generating.");
            var mesh = MeshDataToMesh(workMeshData);
            mesh.Optimize();

            meshFilter.sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new(1, textureRepeat);

            WorkStarted = false;
            workDone = false;
        }
    }

    Thread workThread  = null;
    public bool WorkStarted { get; private set; } = false;
    bool workDone = false;

    public void Generate()
    {
        if (WorkStarted || !bezier.IsValid) return;
        ClearMesh();
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
        var index = 0;
        foreach (var quad in quads)
        {
            float lenA = index / (float)quads.Count;
            lenA = 1 - MathF.Abs(2 * lenA - 1);
            float lenB = (index + 1) / (float)quads.Count;
            lenB = 1 - MathF.Abs(2 * lenB - 1);

            var a = AddPointToMeshData(quad[0], new(0, lenA), meshData);
            var b = AddPointToMeshData(quad[1], new(0, lenB), meshData);
            var c = AddPointToMeshData(quad[2], new(1, lenB), meshData);
            var d = AddPointToMeshData(quad[3], new(1, lenA), meshData);

            meshData.triangles.Add(a);
            meshData.triangles.Add(c);
            meshData.triangles.Add(b);

            meshData.triangles.Add(a);
            meshData.triangles.Add(d);
            meshData.triangles.Add(c);
            index++;
        }

        workMeshData = meshData;
        textureRepeat = Mathf.RoundToInt(textureTiling * quads.Count * bezier.Spacing * 0.05f);

        workDone = true;
    }

    public void ClearMesh()
    {
        if(meshFilter.sharedMesh != null) meshFilter.sharedMesh.Clear();
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

        for (int i = 0; i < points.Count; i++)
        {
            var dirToThis = points[i] - points[(i + points.Count - 1) % points.Count];
            var dirToNext = points[(i + 1) % points.Count] - points[i];
            var dir = (dirToThis + dirToNext) / 2f;
            if (i == 0) dir = points[1] - points[0];
            else if (!bezier.IsClosed && i == points.Count - 1) dir = points[^1] - points[^2];
            var perp = Vector3.Cross(Vector3.up, dir);

            perp = perp.normalized;
            lines.Add(new(points[i] - perp * (roadWidth / 2f), points[i] + perp * (roadWidth / 2f)));
        }

        var quads = new List<List<Vector3>>();
        for (int i = 0; i < lines.Count; i++)
        {
            if(i < lines.Count - 1 || bezier.IsClosed)
            {
                var quad = new List<Vector3>();
                var aLine = lines[i];
                var bLine = lines[(i + 1) % lines.Count];
                quad.AddRange(new Vector3[]
                {
                    aLine.Item2, bLine.Item2, bLine.Item1, aLine.Item1
                });
                quads.Add(quad);
            }
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

        if (!roadGenerator.WorkStarted)
        {
            if(GUILayout.Button("Generate Mesh")) roadGenerator.Generate();
            if (GUILayout.Button("Clear Mesh")) roadGenerator.ClearMesh();
        }
    }
}

#endif