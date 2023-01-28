using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Vector2 = Shapes.Vector2;

public class DualGraph
{
    public Dictionary<Vector2, object> VertexToPolygon { get; private set; } = new();
    public Dictionary<Edge, List<Vector2>> EdgeToShapesVertex { get; private set; } = new();
    public Dictionary<Vector2, List<object>> VertexToMainShapes { get; private set; }

    public DualGraph(List<object> shapes)
    {
        VertexToMainShapes = GetVertexToShapes(shapes);

        foreach (var pair in VertexToMainShapes)
        {
            var sharedShapes = pair.Value;
            List<Vector2> centerPoints = new();
            foreach (var shape in sharedShapes)
            {
                centerPoints.Add(GetShapeCenter(shape));
            }
            Quad.SortPoints(centerPoints);
            var polygon = new Polygon(centerPoints);
            if (centerPoints.Count == 3)
            {
                if (polygon.Center.GetRounded() != pair.Key) continue;
            }
            VertexToPolygon.Add(polygon.Center, polygon);
            foreach (var edge in polygon.GetEdges())
            {
                if (!EdgeToShapesVertex.ContainsKey(edge))
                    EdgeToShapesVertex.Add(edge, new List<Vector2>());
                EdgeToShapesVertex[edge].Add(polygon.Center);
            }
        }
    }

    public Vector2 GetClosestShape(Vector2 point)
    {
        Vector2 closestFace = null;
        float champDist = float.PositiveInfinity;
        foreach (var pair in VertexToPolygon)
        {
            if (closestFace is null)
            {
                closestFace = pair.Key;
                champDist = point.Distance(closestFace);
                continue;
            }
            float tempDist = point.Distance(pair.Key);
            if (tempDist < champDist)
            {
                champDist = tempDist;
                closestFace = pair.Key;
            }
        }
        return closestFace;
    }

    public bool IsNeighbor(Vector2 a, Vector2 b)
    {
        Polygon polygon = GetPolygon(a);
        var edges = polygon.GetEdges();
        foreach (var edge in edges)
        {
            List<Vector2> neighbors = GetNeighbors(edge);
            if (neighbors.Count > 0)
            {
                foreach (Vector2 neighborVertex in neighbors)
                {
                    if (neighborVertex == b) return true;
                }
            }
        }
        return false;
    }

    public Polygon GetPolygon(Vector2 vertex) => (Polygon)VertexToPolygon[vertex];

    public List<Vector2> GetNeighbors(Edge edge)
    {
        List<Vector2> neighbors;
        if (EdgeToShapesVertex.TryGetValue(edge, out neighbors)) return neighbors;
        return new();
    }

    Dictionary<Vector2, List<object>> GetVertexToShapes(List<object> shapes)
    {
        var vertexToShapes = new Dictionary<Vector2, List<object>>();
        foreach (var shape in shapes)
        {
            foreach (var vertex in GetShapeVertices(shape))
            {
                if (!vertexToShapes.ContainsKey(vertex))
                {
                    vertexToShapes[vertex] = new();
                }
                vertexToShapes[vertex].Add(shape);
            }
        }
        foreach (var pair in vertexToShapes.ToList())
        {
            if (pair.Value.Count < 3) vertexToShapes.Remove(pair.Key);
        }
        return vertexToShapes;
    }

    List<Vector2> GetShapeVertices(object shape)
    {
        List<Vector2> points = new();
        if (shape is Quad quad) points.AddRange(quad.GetPoints());
        else if (shape is Triangle triangle) points.AddRange(triangle.GetPoints());
        return points;
    }

    Vector2 GetShapeCenter(object shape)
    {
        if (shape is Quad quad) return quad.GetCenter();
        if (shape is Triangle triangle) return triangle.GetCenter();
        return new(0, 0);
    }
}
