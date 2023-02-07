using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierCurve
{
    public static List<Vector3> GetPoints(List<Vector3> _points, int numSegments, bool isClosed = false)
    {
        var points = new List<Vector3>();

        for (int i = 0; i <= (_points.Count - 1) / 3; i++)
        {
            Vector3 a;
            Vector3 b;
            Vector3 c;
            Vector3 d;
            if (i == (_points.Count - 1) / 3)
            {
                if (!isClosed) continue;
                a = _points[i * 3];
                b = a + (a - _points[i * 3 - 1]);
                d = _points[0];
                c = d + (d - _points[1]);
            }
            else
            {
                a = _points[i * 3];
                b = _points[i * 3 + 1];
                c = _points[i * 3 + 2];
                d = _points[i * 3 + 3];
            }

            points.AddRange(GetPointsInCubic(a, b, c, d, numSegments));
        }

        return points;
    }

    public static List<Vector3> GetPointsEvenly(List<Vector3> _points, float spacing, float resolution, bool isClosed = false)
    {
        var evenPoints = new List<Vector3> { _points[0] };
        Vector3 prevPoint = _points[0];
        float distSinceLast = 0;

        for (int i = 0; i <= (_points.Count - 1) / 3; i++)
        {
            Vector3 a, b, c, d;
            if (i == (_points.Count - 1) / 3)
            {
                if (!isClosed) continue;
                a = _points[i * 3];
                b = a + (a - _points[i * 3 - 1]);
                d = _points[0];
                c = d + (d - _points[1]);
            }
            else
            {
                a = _points[i * 3];
                b = _points[i * 3 + 1];
                c = _points[i * 3 + 2];
                d = _points[i * 3 + 3];
            }

            evenPoints.AddRange(GetPointsInCubicEvenly(a, b, c, d, resolution, spacing, ref distSinceLast, ref prevPoint));
        }

        return evenPoints;
    }

    public static List<Vector3> GetPointsInCubicEvenly(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float resolution, float spacing, ref float distSince, ref Vector3 prevPoint)
    {
        var points = new List<Vector3>();
        float netLength = Vector3.Distance(a, b) + Vector3.Distance(b, c) + Vector3.Distance(c, d);
        float estCurveLength = Vector3.Distance(a, d) + netLength / 2f;
        float step = 1f / Mathf.CeilToInt(estCurveLength * resolution);

        for (float t = step; t <= 1f; t += step)
        {
            var point = CubicCurve(a, b, c, d, t);
            distSince += Vector3.Distance(prevPoint, point);
            while (distSince >= spacing)
            {
                float distOver = distSince - spacing;
                Vector3 newPoint = point + (prevPoint - point).normalized * distOver;
                points.Add(newPoint);
                distSince = distOver;
                prevPoint = newPoint;
            }
            prevPoint = point;
        }
        return points;
    }

    public static List<Vector3> GetPointsInCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int numSegments)
    {
        var points = new List<Vector3> { a };
        float step = 1f / numSegments;
        for (float t = step; t <= 1f; t += step)
        {
            points.Add(CubicCurve(a, b, c, d, t));
        }
        return points;
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => Vector3.Lerp(a, b, t);
    public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t) => Lerp(Lerp(a, b, t), Lerp(b, c, t), t);
    public static Vector3 CubicCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) => Lerp(QuadraticCurve(a, b, c, t), QuadraticCurve(b, c, d, t), t);
}