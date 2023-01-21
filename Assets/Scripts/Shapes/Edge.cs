using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : System.IEquatable<Edge>
{
    public Vector2 A { get; private set; }
    public Vector2 B { get; private set; }

    public Edge(Vector2 a, Vector2 b)
    {
        A = a; B = b;
    }

    public bool Equals(Edge e)
    {
        if (e is null) return false;
        if (e.GetType() != GetType()) return false;
        return (A == e.A && B == e.B) || (A == e.B && B == e.A);
    }

    public override bool Equals(object obj) => Equals(obj as Edge);

    public static bool operator ==(Edge lhs, Edge rhs) => object.Equals(lhs, rhs);

    public static bool operator !=(Edge lhs, Edge rhs) => !(lhs == rhs);

    public override int GetHashCode() => (A.GetHashCode() ^ B.GetHashCode()) + (B.GetHashCode() ^ A.GetHashCode());
}