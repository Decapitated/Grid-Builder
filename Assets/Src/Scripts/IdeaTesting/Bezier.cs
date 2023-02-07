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
    [SerializeField, HideInInspector]
    private Dictionary<int, Quaternion> rotations = new();

    public Quaternion GetRotation(int index) => rotations[index];
    public void SetRotation(int index, Quaternion rotation) => rotations[index] = rotation;

    [SerializeField]
    private bool isClosed = false;

    [SerializeField, Min(1f)]
    private float resolution = 1;
    float prevResolution = 1;

    [SerializeField, Min(0.1f)]
    private float spacing = 0.1f;
    public float Spacing { get { return spacing; } }
    float prevSpacing = 0.1f;

    public bool IsValid { get { return points.Count > 0 && (points.Count - 1) % 3 == 0; } }
    public bool IsEdited { get; set; } = false;
    public bool IsClosed { get { return isClosed; } }

    void Update()
    {
        if(resolution != prevResolution)
        {
            IsEdited = true;
            prevResolution = resolution;
        }
        if (spacing != prevSpacing)
        {
            IsEdited = true;
            prevSpacing = spacing;
        }
    }

    public void AddPoint()
    {
        if(points.Count == 0)
        {
            foreach(int x in Enumerable.Range(0, 4))
                points.Add(new(x, 0));
            rotations.Add(0, Quaternion.identity);
        }
        else
        {
            var dir  = (points[^1] - points[^2]);
            points.Add(points[^1] + dir);
            points.Add(points[^1] + (dir / 2));
            points.Add(points[^1] + (dir / 2));
        }
        rotations.Add(points.Count - 1, Quaternion.identity);
    }

    public void RemovePoint()
    {
        if (points.Count != 0) points.RemoveRange(points.Count - 3, 3);
    }

    public void Clear()
    {
        points.Clear();
        rotations.Clear();
    }

    public List<Vector3> GetControlPoints() => points;

    public List<Vector3> GetCurvePoints() => BezierCurve.GetPointsEvenly(points, spacing, resolution, isClosed);
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
        if (points.Count <= 0) return;
        for (int i = 0; i <= (points.Count - 1) / 3; i++)
        {
            Vector3 a, b, c, d;
            if (i == (points.Count - 1) / 3)
            {
                if (!bezier.IsClosed) continue;
                a = points[i * 3];
                b = a + (a - points[i * 3 - 1]);
                d = points[0];
                c = d + (d - points[1]);
            }
            else
            {
                a = points[i * 3];
                b = points[i * 3 + 1];
                c = points[i * 3 + 2];
                d = points[i * 3 + 3];
            }

            a = bezier.transform.TransformPoint(a);
            b = bezier.transform.TransformPoint(b);
            c = bezier.transform.TransformPoint(c);
            d = bezier.transform.TransformPoint(d);

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
            Handles.color = (index % 3 == 0) ? Color.green : Color.red;
            float size = (index % 3 == 0) ? 0.25f : 0.2f;
            Handles.FreeMoveHandle(worldPoint, HandleUtility.GetHandleSize(worldPoint) * 0.2f, Vector3.one, Handles.SphereHandleCap);
            Vector3 newPoint = worldPoint;
            Quaternion newQuaternion = Quaternion.identity;
            if(index % 3 == 0)
            {
                newQuaternion = bezier.GetRotation(index);
                Handles.TransformHandle(ref newPoint, ref newQuaternion);
            }
            else
            {
                newPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            }
            if (EditorGUI.EndChangeCheck())
            {
                bezier.IsEdited = true;
                Undo.RecordObject(bezier, "Update point position.");
                newPoint = bezier.transform.InverseTransformPoint(newPoint);
                if (index % 3 != 0) // If not anchor point. update the opposite control point.
                {
                    if(index != points.Count - 2 && (index % 3) % 2 == 0)
                    {
                        points[index + 2] = points[index + 1] + (points[index + 1] - newPoint);
                    } else if (index != 1 && (index % 3) % 2 != 0)
                    {
                        points[index - 2] = points[index - 1] + (points[index - 1] - newPoint);
                    }
                }
                else // Move control points with anchor point.
                {
                    if(index != points.Count - 1)
                    {
                        points[index + 1] = newPoint + (points[index + 1] - points[index]);
                    }
                    if (index != 0)
                    {
                        points[index - 1] = newPoint + (points[index - 1] - points[index]);
                    }
                    bezier.SetRotation(index, newQuaternion);
                }

                points[index] = newPoint;
            }
            index++;
        }
        if(bezier.IsClosed)
        {
            var _a = points[^1];
            var _b = _a + (_a - points[^2]);

            var _d = points[0];
            var _c = _d + (_d - points[1]);

            Handles.color = Color.red;
            EditorGUI.BeginChangeCheck();
            var worldPoint = bezier.transform.TransformPoint(_b);
            Handles.FreeMoveHandle(worldPoint, HandleUtility.GetHandleSize(worldPoint) * 0.2f, Vector3.one, Handles.SphereHandleCap);
            var newPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                bezier.IsEdited = true;
                Undo.RecordObject(bezier, "Update point position.");
                newPoint = bezier.transform.InverseTransformPoint(newPoint);

                points[^2] = points[^1] + (points[^1] - newPoint);
            }

            EditorGUI.BeginChangeCheck();
            worldPoint = bezier.transform.TransformPoint(_c);
            Handles.FreeMoveHandle(worldPoint, HandleUtility.GetHandleSize(worldPoint) * 0.2f, Vector3.one, Handles.SphereHandleCap);
            newPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                bezier.IsEdited = true;
                Undo.RecordObject(bezier, "Update point position.");
                newPoint = bezier.transform.InverseTransformPoint(newPoint);

                points[1] = points[0] + (points[0] - newPoint);
            }
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