using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : System.IEquatable<Triangle>
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }
    public Vector2 C { get; private set; }

    public Vector2 MidA { get => (A + B) / 2f; }
    public Vector2 MidB { get => (B + C) / 2f; }
    public Vector2 MidC { get => (C + A) / 2f; }

    public Circle CircumCircle
    {
        get
        {
            Vector2 midAB = (A + B) / 2;
            Vector2 midAC = (A + C) / 2;

            float slopeAB = -(B.X - A.X) / (B.Y - A.Y);
            float slopeAC = -(C.X - A.X) / (C.Y - A.Y);

            float x = (slopeAB * midAB.X - slopeAC * midAC.X + midAC.Y - midAB.Y) / (slopeAB - slopeAC);
            float y = midAB.Y + slopeAB * (x - midAB.X);

            var a = UnityEngine.Vector2.Distance(A, B);
            var b = UnityEngine.Vector2.Distance(B, C);
            var c = UnityEngine.Vector2.Distance(C, A);
            return new(
                new(x, y),
                (a * b * c) / Mathf.Sqrt((a + b + c) * (b + c - a) * (c + a - b) * (a + b - c))
            );
        }
    }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        A = a; B = b; C = c;
    }

    public static Triangle GetSuperTriangle(List<Vector2> points)
    {
        var minX = Mathf.Infinity;
        var minY = Mathf.Infinity;
        var maxX = -Mathf.Infinity;
        var maxY = -Mathf.Infinity;
        foreach (Vector2 point in points)
        {
            minX = Mathf.Min(minX, point.X);
            minY = Mathf.Min(minY, point.Y);
            maxX = Mathf.Max(maxX, point.X);
            maxY = Mathf.Max(maxY, point.Y);
        }

        var dx = (maxX - minX) * 100;
        var dy = (maxY - minY) * 100;

        var v0 = new Vector2(minX - dx, minY - dy * 3);
        var v1 = new Vector2(minX - dx, maxY + dy);
        var v2 = new Vector2(maxX + dx * 3, maxY + dy);

        return new Triangle(v0, v1, v2);
    }

    public Edge[] GetEdges()
    {
        return new Edge[]{
                new(A, B),
                new(B, C),
                new(C, A)
            };
    }

    public float[] GetAngles()
    {
        float lA = UnityEngine.Vector2.Distance(A, B),
              lB = UnityEngine.Vector2.Distance(B, C),
              lC = UnityEngine.Vector2.Distance(C, A);
        var angles = new float[3];
        angles[0] = Mathf.Acos(
            (Mathf.Sqrt(lB) + Mathf.Sqrt(lC) - Mathf.Sqrt(lA)) / (2 * lB * lC)
        );
        angles[1] = Mathf.Acos(
            (Mathf.Sqrt(lA) + Mathf.Sqrt(lC) - Mathf.Sqrt(lB)) / (2 * lA * lC)
        );
        angles[2] = Mathf.Acos(
            (Mathf.Sqrt(lA) + Mathf.Sqrt(lB) - Mathf.Sqrt(lC)) / (2 * lA * lB)
        );
        return angles;
    }

    public List<Vector2> GetPoints() => new(){ A, B, C };

    public float GetArea() => GetArea(A, B, C);
    public static float GetArea(Vector2 a, Vector2 b, Vector2 c) => Mathf.Abs((a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) / 2f);

    public Vector2 GetCenter() => (A + B + C) / 3f;

    public bool CheckAngles(float upper, float lower)
    {
        foreach (float angle in GetAngles())
        {
            if (angle > upper || angle < lower) return false;
        }
        return true;
    }

    public bool IsPointInside(Vector2 point)
    {
        float area_1 = GetArea(A, B, C);
        float area_2 = GetArea(point, B, C);
        float area_3 = GetArea(A, point, C);
        float area_4 = GetArea(A, B, point);
        return area_1 == (area_2 + area_3 + area_4);
    }

    public bool IsPointInCircumCircle(Vector2 point)
    {
        return CircumCircle.IsPointInside(point);
    }

    public bool SharesEdge(Triangle tri)
    {
        if (Equals(tri)) return true;
        foreach (var edge in GetEdges())
        {
            foreach (var edge2 in tri.GetEdges())
            {
                if (edge.Equals(edge2)) return true;
            }
        }
        return false;
    }

    public Vector2 GetOppositePoint(Edge e)
    {
        if ((e.A == B && e.B == C) || (e.A == C && e.B == B)) return A;
        if ((e.A == A && e.B == C) || (e.A == C && e.B == A)) return B;
        if ((e.A == A && e.B == B) || (e.A == B && e.B == A)) return C;
        return new(0, 0);
    }

    public Triangle GetOppositeTriangle(Edge e)
    {
        Vector2 opposite = GetOppositePoint(e);
        Vector2 reflected = ReflectPointAcrossLine(opposite, e.A, e.B);
        return new(e.A, e.B, reflected);
    }

    Vector2 ReflectPointAcrossLine(UnityEngine.Vector2 P, UnityEngine.Vector2 A, UnityEngine.Vector2 B)
    {
        UnityEngine.Vector2 lineDirection = (B - A).normalized;
        UnityEngine.Vector2 projection = UnityEngine.Vector2.Dot((P - A), lineDirection) / UnityEngine.Vector2.Dot(lineDirection, lineDirection) * lineDirection + A;
        UnityEngine.Vector2 reflection = 2 * projection - P;
        return reflection;
    }

    public List<Quad> Split()
    {
        List<Quad> quads = new();
        Vector2 center = GetCenter();
        quads.Add(new(
            new(A, MidA, center),
            new(A, center, MidC)));
        quads.Add(new(
            new(B, MidB, center),
            new(B, center, MidA)));
        quads.Add(new(
            new(C, MidC, center),
            new(C, center, MidB)));
        return quads;
    }

    public bool Equals(Triangle tri)
    {
        if (tri is null) return false;
        if (tri.GetType() != GetType()) return false;
        var aPoints = GetPoints();
        var bPoints = tri.GetPoints();
        return (aPoints[0] == bPoints[0] && aPoints[1] == bPoints[1] && aPoints[2] == bPoints[2]) ||
               (aPoints[0] == bPoints[0] && aPoints[1] == bPoints[2] && aPoints[2] == bPoints[1]) ||
               (aPoints[0] == bPoints[1] && aPoints[1] == bPoints[0] && aPoints[2] == bPoints[2]) ||
               (aPoints[0] == bPoints[1] && aPoints[1] == bPoints[2] && aPoints[2] == bPoints[0]) ||
               (aPoints[0] == bPoints[2] && aPoints[1] == bPoints[0] && aPoints[2] == bPoints[1]) ||
               (aPoints[0] == bPoints[2] && aPoints[1] == bPoints[1] && aPoints[2] == bPoints[0]);
    }

    public override bool Equals(object obj) => Equals(obj as Triangle);

    public static bool operator ==(Triangle lhs, Triangle rhs) => object.Equals(lhs, rhs);

    public static bool operator !=(Triangle lhs, Triangle rhs) => !(lhs == rhs);

    public override int GetHashCode() => HashCode.Combine(A.GetHashCode(), B.GetHashCode(), C.GetHashCode());

    public override string ToString() => "[ "+A.ToString()+", "+B.ToString()+", "+C.ToString()+" ]";
}