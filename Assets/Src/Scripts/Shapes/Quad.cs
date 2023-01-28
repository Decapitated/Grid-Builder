using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Vector2 = Shapes.Vector2;
public class Quad : System.IEquatable<Quad>
{
    public Triangle A { get; private set; }
    public Triangle B { get; private set; }

    public Quad(Triangle a, Triangle b)
    {
        A = a; B = b;
    }

    public List<Triangle> GetTriangles() => new(){ A, B };

    public List<Vector2> GetPoints(bool sort = false)
    {
        List<Vector2> pointList = new();
        foreach (Triangle triangle in GetTriangles())
        {
            var triPoints = triangle.GetPoints();
            foreach (Vector2 point in triPoints)
            {
                if(pointList.Contains(point)) continue;
                pointList.Add(point);
            }
        }
        if (sort) SortPoints(pointList);
        return pointList;
    }

    public Vector2 GetCenter()
    {
        return GetCenter(GetPoints());
    }

    public List<Edge> GetEdges()
    {
        var list = new List<Edge>();
        list.AddRange(A.GetEdges());
        list.AddRange(B.GetEdges());
        return list;
    }

    public bool SharesEdge(Quad other) => GetEdges().Intersect(other.GetEdges()).Count() > 0;

    public static Vector2 GetCenter(List<Vector2> points)
    {
        Vector2 total = new(0, 0);
        foreach (var point in points)
            total += point;
        return total / points.Count;
    }

    public static void SortPoints(List<Vector2> points)
    {
        SortPoints(points, GetCenter(points));
    }

    public static void SortPoints(List<Vector2> points, Vector2 center)
    {
        points.Sort(delegate (Vector2 a, Vector2 b)
        {
            float a1 = center.GetAngle2(a, (Mathf.PI / 2f) + (Mathf.PI / 8f));
            float a2 = center.GetAngle2(b, (Mathf.PI / 2f) + (Mathf.PI / 8f));
            if (a1 > a2)
                return 1;
            if (a1 == a2 && center.Distance(a) < center.Distance(b))
                return 1;
            return -1;
        });
    }

    public List<Quad> Split()
    {
        List<Quad> quads = new();
        List<Vector2> points = GetPoints(true);
        List<Vector2> midPoints = new()
        {
            (points[0] + points[1]) / 2f,
            (points[1] + points[2]) / 2f,
            (points[2] + points[3]) / 2f,
            (points[3] + points[0]) / 2f
        };
        Vector2 center = GetCenter(points);
        quads.Add(new(
            new(points[0], midPoints[0], center),
            new(points[0], center, midPoints[3])));
        quads.Add(new(
            new(midPoints[0], points[1], midPoints[1]),
            new(midPoints[0], midPoints[1], center)));
        quads.Add(new(
            new(center, midPoints[1], points[2]),
            new(center, points[2], midPoints[2])));
        quads.Add(new(
            new(midPoints[3], center, midPoints[2]),
            new(midPoints[3], midPoints[2], points[3])));
        return quads;
    }

    public bool Equals(Quad e)
    {
        if (e is null) return false;
        if (e.GetType() != GetType()) return false;
        return (A.Equals(e.A) && B.Equals(e.B)) || (A.Equals(e.B) && B.Equals(e.A));
    }

    public override bool Equals(object obj) => Equals(obj as Quad);

    public static bool operator ==(Quad lhs, Quad rhs) => object.Equals(lhs, rhs);

    public static bool operator !=(Quad lhs, Quad rhs) => !(lhs == rhs);

    public override int GetHashCode() => HashCode.Combine(A.GetHashCode(), B.GetHashCode());
}