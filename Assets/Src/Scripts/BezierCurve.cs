using System;
using System.Collections.Generic;
using UnityEngine;

public static class BezierCurve
{
    public static List<Vector3> GetPoints(List<Vector3> controlPoints, int numSegments)
    {
        var segments = new List<Vector3>();
        float step = GetArcLength(controlPoints) / numSegments;
        float totalLength = 0;

        for (int i = 0; i <= numSegments; i++)
        {
            float t = GetTAtLength(controlPoints, totalLength);
            segments.Add(GetPoint(controlPoints, t));
            totalLength += step;
        }

        return segments;
    }

    // Private method to get the point on the curve at a specific time 't'. Range(0.0, 1.0)
    private static Vector3 GetPoint(List<Vector3> points, float t) => TransformPoint(points, t);

    // Private method to calculate the point on the curve using De Casteljau's algorithm
    private static Vector3 TransformPoint(List<Vector3> points, float t)
    {
        // Create a copy of the points array
        Vector3[] a = points.ToArray();

        int i;
        // Loop to calculate the point on the curve
        for (i = points.Count - 1; i > 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                // Calculate intermediate points using linear interpolation
                a[j] = (1 - t) * a[j] + t * a[j + 1];
            }
        }

        // Return the final calculated point
        return a[0];
    }

    // Private method to calculate the arc length of the curve
    private static float GetArcLength(List<Vector3> points)
    {
        float arcLength = 0;
        float step = 0.01f;
        Vector3 prevPoint = GetPoint(points, 0);

        for (float t = step; t < 1.0f; t += step)
        {
            Vector3 currentPoint = GetPoint(points, t);
            arcLength += (currentPoint - prevPoint).magnitude;
            prevPoint = currentPoint;
        }

        return arcLength;
    }

    // Private method to find the value of 't' at a specific length along the curve
    private static float GetTAtLength(List<Vector3> points, float length)
    {
        float t = 0;
        float step = 0.01f;
        float currentLength = 0;
        Vector3 prevPoint = GetPoint(points, 0);

        while (currentLength < length)
        {
            t += step;
            Vector3 currentPoint = GetPoint(points, t);
            currentLength += (currentPoint - prevPoint).magnitude;
            prevPoint = currentPoint;
        }

        return t;
    }
}