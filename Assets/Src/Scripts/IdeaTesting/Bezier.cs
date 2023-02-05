using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager.UI;
using System;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Bezier : MonoBehaviour
{
    [SerializeField]
    private List<Vector3> points = new()
    {
        new(-1, 0, 0), new(-1, 0, 1), new(1, 0, 1), new(1, 0, 0)
    };

    [SerializeField, Min(1)]
    private int numSegments = 12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        foreach (var point in points)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(transform.TransformPoint(point), 0.1f);
        }
    }

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
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

        DrawPath();
        DrawCurve();
    }

    public List<Vector3> GetControlPoints() => points;

    public List<Vector3> GetCurvePoints() => BezierCurve.GetPoints(points, numSegments);

    void DrawCurve()
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.blue);

        foreach (var _point in GetCurvePoints())
        {
            var point = transform.TransformPoint(_point);
            GL.Vertex3(point.x, point.y, point.z);
        }

        GL.End();
    }

    void DrawPath()
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(Color.yellow);

        foreach (var controlPoint in points)
        {
            var point = transform.TransformPoint(controlPoint);
            GL.Vertex3(point.x, point.y, point.z);
        }

        GL.End();
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Bezier))]
public class BezierEditor : Editor
{
    void OnEnable()
    {
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Tools.hidden = false;
    }

    public void OnSceneGUI()
    {
        var bezier = target as Bezier;
        var points = bezier.GetControlPoints();
        int index = 0;
        foreach (var point in points)
        {
            EditorGUI.BeginChangeCheck();
            var newPoint = Handles.PositionHandle(bezier.transform.TransformPoint(point), Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezier, "Update point position.");
                newPoint = bezier.transform.InverseTransformPoint(newPoint);
                points[index] = newPoint;
            }
            index++;
        }
        
    }
}

#endif