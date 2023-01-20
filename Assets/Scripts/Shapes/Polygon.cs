using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{
    public List<Vector2> Points { get; private set; }
    public Vector2 Center { get; private set; }

    public Polygon(List<Vector2> points, Vector2 center)
    {
        Points = points; Center = center;
    }

    public List<Triangle> GetTriangles()
    {
        Quad.SortPoints(Points, Center);
        List<Triangle> triangles = new();
        for(int i = 0; i < Points.Count; i++)
        {
            var a = Points[i];
            var b = Points[(i + 1) % Points.Count];
            triangles.Add(new(a, b, Center));
        }
        return triangles;
    }
}
