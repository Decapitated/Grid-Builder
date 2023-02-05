using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierCurve
{
    public static List<Vector3> GetPoints(List<Vector3> _points, int numSegments, bool isClosed = false)
    {
        var points = new List<Vector3>();
        float step = 1f / numSegments;

        for (int i = 0; i <= (_points.Count - 1) / 3; i++)
        {
            if(i == (_points.Count - 1) / 3)
            {
                if (!isClosed) continue;
                var _a = _points[i * 3];
                var _b = _a + (_a - _points[i * 3 - 1]);
                var _d = _points[0];
                var _c = _d + (_d - _points[1]);

                for (float t = step; t <= 1f; t += step)
                {
                    points.Add(CubicCurve(_a, _b, _c, _d, t));
                }
                continue;
            }
            var a = _points[i * 3];
            var b = _points[i * 3 + 1];
            var c = _points[i * 3 + 2];
            var d = _points[i * 3 + 3];

            for (float t = (i == 0) ? 0f : step; t <= 1f; t += step)
            {
                points.Add(CubicCurve(a, b, c, d, t));
            }
        }

        return points;
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * t;
    public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t) => Lerp(Lerp(a, b, t), Lerp(b, c, t), t);
    public static Vector3 CubicCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) => Lerp(QuadraticCurve(a, b, c, t), QuadraticCurve(b, c, d, t), t);
}