using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Bezier : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private List<Vector3> points = new();
    [SerializeField]
    private bool isClosed = false;
    [SerializeField, Min(1)]
    private int numSegments = 12;
    int prevNumSegments = 12;
    public bool IsValid { get { return points.Count > 0 && (points.Count - 1) % 3 == 0; } }
    public bool IsEdited { get; set; } = false;
    public bool IsClosed { get { return isClosed; } }

    void Update()
    {
        if(numSegments != prevNumSegments)
        {
            IsEdited = true;
            prevNumSegments = numSegments;
        }
    }

    public void AddPoint()
    {
        if(points.Count == 0)
        {
            foreach(int x in Enumerable.Range(0, 4))
                points.Add(new(x, 0));
        }
        else
        {
            var dir  = (points[^1] - points[^2]);
            points.Add(points[^1] + dir);
            points.Add(points[^1] + (dir / 2));
            points.Add(points[^1] + (dir / 2));
        }
    }

    public void RemovePoint()
    {
        if (points.Count != 0) points.RemoveRange(points.Count - 3, 3);
    }

    public void Clear() => points.Clear();

    public List<Vector3> GetControlPoints() => points;

    public List<Vector3> GetCurvePoints() => BezierCurve.GetPoints(points, numSegments, isClosed);
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

        for (int i = 0; i <= (points.Count - 1) / 3; i++)
        {
            if (i == (points.Count - 1) / 3)
            {
                if (!bezier.IsClosed) continue;
                var _a = points[i * 3];
                var _b = _a + (_a - points[i * 3 - 1]);
                var _d = points[0];
                var _c = _d + (_d - points[1]);

                Handles.color = Color.red;
                Handles.DrawLine(_a, _b, 5f);
                Handles.DrawLine(_c, _d, 5f);
                Handles.DrawBezier(_a, _d, _b, _c, Color.blue, null, 5f);

                continue;
            }
            var a = points[i * 3];
            var b = points[i * 3 + 1];
            var c = points[i * 3 + 2];
            var d = points[i * 3 + 3];

            Handles.color = Color.red;
            Handles.DrawLine(a, b, 5f);
            Handles.DrawLine(c, d, 5f);
            Handles.DrawBezier(a, d, b, c, Color.blue, null, 5f);
        }

        int index = 0;
        foreach (var point in points)
        {
            EditorGUI.BeginChangeCheck();
            var worldPoint = bezier.transform.TransformPoint(point);
            //var newPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            Handles.color = (index % 3 == 0) ? Color.green : Color.red;
            float size = (index % 3 == 0) ? 0.25f : 0.2f;
            var newPoint = Handles.FreeMoveHandle(worldPoint, HandleUtility.GetHandleSize(worldPoint) * size, Vector3.one, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                bezier.IsEdited = true;
                Undo.RecordObject(bezier, "Update point position.");
                newPoint = bezier.transform.InverseTransformPoint(newPoint);
                if (index % 3 != 0)
                {
                    if(index != points.Count - 2 && (index % 3) % 2 == 0)
                    {
                        points[index + 2] = points[index + 1] + (points[index + 1] - newPoint);
                    } else if (index != 1 && (index % 3) % 2 != 0)
                    {
                        points[index - 2] = points[index - 1] + (points[index - 1] - newPoint);
                    }
                }
                else
                {
                    if(index != points.Count - 1)
                    {
                        points[index + 1] = newPoint + (points[index + 1] - points[index]);
                    }
                    if (index != 0)
                    {
                        points[index - 1] = newPoint + (points[index - 1] - points[index]);
                    }
                }

                points[index] = newPoint;
            }
            index++;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var bezier = target as Bezier;

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(bezier, "Add point.");
            bezier.AddPoint();
        }

        if (bezier.IsValid && GUILayout.Button("Remove Point"))
        {
            Undo.RecordObject(bezier, "Remove point.");
            bezier.RemovePoint();
        }

        if (GUILayout.Button("Clear Points"))
        {
            Undo.RecordObject(bezier, "Clear points.");
            bezier.Clear();
        }
    }
}

#endif