using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vector2 = Shapes.Vector2;
public class Polygon
{
    public Dictionary<Vector2, List<Edge>> VertexToEdges { get; private set; } = new();
    public List<Vector2> Points { get; private set; }
    public Vector2 Center { get; private set; }

    public Polygon(List<Vector2> points)
    {
        Points = points;
        Center = CalcCenter();
        GenerateVertexToEdges();
    }

    Vector2 CalcCenter()
    {
        Vector2 temp = new(0, 0);
        foreach (var point in Points)
        {
            temp += point;
        }
        return temp / Points.Count;
    }

    void GenerateVertexToEdges()
    {
        for(int i = 0; i < Points.Count; i++)
        {
            var a = Points[i];
            var b = Points[(i + 1) % Points.Count];
            var edge = new Edge(a, b);

            if (!VertexToEdges.ContainsKey(a))
                VertexToEdges.Add(a, new());
            if (!VertexToEdges.ContainsKey(b))
                VertexToEdges.Add(b, new());

            VertexToEdges[a].Add(edge);
            VertexToEdges[b].Add(edge);
        }
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

    public List<Edge> GetEdges()
    {
        var edges = new List<Edge>();
        for(int i = 0; i < Points.Count; i++)
        {
            edges.Add(new( Points[i], Points[(i + 1) % Points.Count]));
        }
        return edges;
    }

    
}
