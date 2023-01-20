using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2 : System.IEquatable<Vector2>
{
    public float X { get; private set; }
    public float Y { get; private set; }

    public Vector2(float x, float y)
    {
        this.X = x; this.Y = y;
    }

    public float Distance(Vector2 other) => Mathf.Sqrt(Mathf.Pow(X - other.X, 2) + Mathf.Pow(Y - other.Y, 2));

    public float GetAngle(Vector2 other)
    {
        float angle = Mathf.Atan2(other.X - X, other.Y - Y);
        if (angle <= 0f) angle = 2 * Mathf.PI + angle;
        return angle;
    }

    public float GetAngle2(Vector2 other, float angleShift)
    {
        float angle = Mathf.Atan2(other.X - X, other.Y - Y) - (angleShift);
        if (angle <= 0f) angle = 2 * Mathf.PI + angle;
        return angle;
    }

    public bool Equals(Vector2 e)
    {
        if (e is null) return false;
        if (e.GetType() != GetType()) return false;
        return (Mathf.RoundToInt(X * 100) == Mathf.RoundToInt(e.X * 100)) && (Mathf.RoundToInt(Y * 100) == Mathf.RoundToInt(e.Y * 100));
    }

    public override bool Equals(object obj) => Equals(obj as Vector2);

    public static bool operator ==(Vector2 lhs, Vector2 rhs) => Equals(lhs, rhs);

    public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

    public static Vector2 operator +(Vector2 lhs, Vector2 rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
    public static Vector2 operator +(Vector2 lhs, float value) => new(lhs.X + value, lhs.Y + value);

    public static Vector2 operator *(Vector2 lhs, Vector2 rhs) => new(lhs.X * rhs.X, lhs.Y * rhs.Y);
    public static Vector2 operator *(Vector2 lhs, float value) => new(lhs.X * value, lhs.Y * value);

    //public static Vector2 operator /(Vector2 lhs, Vector2 rhs) => new(lhs.X / rhs.X, lhs.Y / rhs.Y);
    public static Vector2 operator /(Vector2 lhs, float value) => new(lhs.X / value, lhs.Y / value);

    public override int GetHashCode() => HashCode.Combine(Mathf.RoundToInt(X * 100).GetHashCode(), Mathf.RoundToInt(Y * 100).GetHashCode());

    public override string ToString() => "("+Math.Round(X, 2)+", "+Math.Round(Y, 2)+")";

    public static implicit operator UnityEngine.Vector2(Vector2 v) => new(v.X, v.Y);
    public static implicit operator Vector2(UnityEngine.Vector2 v) => new(v.x, v.y);
    public static implicit operator UnityEngine.Vector3(Vector2 v) => new(v.X, 0, v.Y);
    public static implicit operator Vector2(UnityEngine.Vector3 v) => new(v.x, v.z);
}
