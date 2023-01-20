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

    public enum NumTurns {
        One = 3,
        Two = 2,
        Three = 1
    }
    public Vector2 RotatePoint(NumTurns num)
    {
        bool bigger = Mathf.Abs(X) > Mathf.Abs(Y);
        if (num == NumTurns.One)
        {
            var x = Y;
            var y = X;
            if (bigger) y *= -1;
            return new(x, y);
        } else if (num == NumTurns.Two)
        {
            var x = Y;
            var y = X;
            if (bigger) x *= -1;
            else y *= -1;
            return new(x, y);
        } else if (num == NumTurns.Three)
        {
            var x = Y;
            var y = X;
            if (!bigger) x *= -1;
            return new(x, y);
        }
        return new(0, 0);
    }

    public enum Places
    {
        One = 10,
        Two = 100,
        Three = 1000
    }
    public Tuple<int, int> GetRounded(Places places = Places.Two) => new(Mathf.RoundToInt(X * (int)places), Mathf.RoundToInt(Y * (int)places));

    public bool Equals(Vector2 e)
    {
        if (e is null) return false;
        if (e.GetType() != GetType()) return false;
        var a = GetRounded(Places.Three);
        var b = e.GetRounded(Places.Three);
        return (a.Item1 == b.Item1) && (a.Item2) == b.Item2;
    }

    public override bool Equals(object obj) => Equals(obj as Vector2);

    public static bool operator ==(Vector2 lhs, Vector2 rhs) => Equals(lhs, rhs);

    public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

    public static Vector2 operator +(Vector2 lhs, Vector2 rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y);
    public static Vector2 operator +(Vector2 lhs, float value) => new(lhs.X + value, lhs.Y + value);

    public static Vector2 operator -(Vector2 lhs, Vector2 rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y);
    public static Vector2 operator -(Vector2 lhs, float value) => new(lhs.X - value, lhs.Y - value);

    public static Vector2 operator *(Vector2 lhs, Vector2 rhs) => new(lhs.X * rhs.X, lhs.Y * rhs.Y);
    public static Vector2 operator *(Vector2 lhs, float value) => new(lhs.X * value, lhs.Y * value);

    //public static Vector2 operator /(Vector2 lhs, Vector2 rhs) => new(lhs.X / rhs.X, lhs.Y / rhs.Y);
    public static Vector2 operator /(Vector2 lhs, float value) => new(lhs.X / value, lhs.Y / value);

    public override int GetHashCode() 
    {
        var rounded = GetRounded(Places.Three);
        return HashCode.Combine(rounded.Item1.GetHashCode(), rounded.Item2.GetHashCode());
    }

    public override string ToString() => "("+X+", "+Y+")";

    public static implicit operator UnityEngine.Vector2(Vector2 v) => new(v.X, v.Y);
    public static implicit operator Vector2(UnityEngine.Vector2 v) => new(v.x, v.y);
    public static implicit operator UnityEngine.Vector3(Vector2 v) => new(v.X, 0, v.Y);
    public static implicit operator Vector2(UnityEngine.Vector3 v) => new(v.x, v.z);

    public Vector2 Clone => new(X, Y);
}
