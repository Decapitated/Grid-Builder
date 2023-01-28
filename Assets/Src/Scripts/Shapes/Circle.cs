using UnityEngine;
using Vector2 = Shapes.Vector2;
public class Circle
{
    public Vector2 Center { get; private set; }
    public float Radius { get; private set; }
    public Circle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool IsPointInside(Vector2 point)
    {
        var dx = Center.X - point.X;
        var dy = Center.Y - point.Y;
        return Mathf.Sqrt(dx * dx + dy * dy) <= Radius;
    }
}