using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Quad : System.IEquatable<Quad>
{
    public Triangle A { get; private set; }
    public Triangle B { get; private set; }

    public Quad(Triangle a, Triangle b)
    {
        A = a; B = b;
    }

    public List<Triangle> GetTriangles() => new(){ A, B };

    public List<Vector2> GetPoints()
    {
        List<Vector2> points = new();
        foreach (Triangle triangle in GetTriangles())
        {
            var triPoints = triangle.GetPoints();
            foreach (Vector2 point in triPoints)
            {
                var unique = true;
                foreach (var usedPoint in points)
                {
                    if (point.Equals(usedPoint))
                    {
                        unique = false;
                        break;
                    }
                }
                if (unique)
                {
                    points.Add(point);
                }
            }
        }
        return points;
    }

    public List<Vector2> GetPoints2()
    {
        List<Vector2> points = new();
        foreach (Triangle triangle in GetTriangles())
        {
            var triPoints = triangle.GetPoints();
            foreach (Vector2 point in triPoints)
            {
                var unique = true;
                foreach (var usedPoint in points)
                {
                    if (point.Equals(usedPoint))
                    {
                        unique = false;
                        break;
                    }
                }
                if (unique)
                {
                    points.Add(point);
                }
            }
        }

        SortPoints(points);
        return points;
    }

    void SortPoints(List<Vector2> points)
    {
        Vector2 center = (points[0] + points[1] + points[2] + points[3]) / 4f;
        points.Sort(delegate(Vector2 a, Vector2 b)
        {
            float a1 = center.GetAngle2(a, (Mathf.PI / 2f) + (Mathf.PI / 8f));
            float a2 = center.GetAngle2(b, (Mathf.PI / 2f) + (Mathf.PI / 8f));
            if (a1 > a2)
                return 1;
            if(a1 == a2 && center.Distance(a) < center.Distance(b))
                return 1;
            return -1;
        });
    }

    public List<Quad> Split()
    {
        List<Quad> quads = new();
        List<Vector2> points = GetPoints2();
        List<Vector2> midPoints = new()
        {
            (points[0] + points[1]) / 2f,
            (points[1] + points[2]) / 2f,
            (points[2] + points[3]) / 2f,
            (points[3] + points[0]) / 2f
        };
        Vector2 center = (points[0] + points[1] + points[2] + points[3]) / 4f;
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