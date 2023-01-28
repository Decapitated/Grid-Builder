using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UVector2 = UnityEngine.Vector2;
using Vector2 = Shapes.Vector2;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public int range = 2;
    public float scale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        Vector2 maxSides = Hex.GetMaxSides(range, scale);
        float radius = maxSides.Y / 2f;
        Hex hex = new(0, 0);
        //List<Hex> cells = hex.GetHexSpiralInRange(range);
        var cells = Hex.GetHexInRanges(new Hex.RangeInfo[]
            {
                new(){
                    center = new(0, 0),
                    range = range
                },
                new(){
                    center = new(range + 1, -((range + 1f) * 2f)),
                    range = range
                },
                new(){
                    center = new(((range + 1f) * 2f), -(range + 1)),
                    range = range
                },
                new(){
                    center = new(range + 1, range + 1),
                    range = range
                },
                new(){
                    center = new(-((range + 1f) * 2f), range + 1),
                    range = range
                },
                new(){
                    center = new(-(range + 1), range + 1),
                    range = range
                }
            });
        /*var bounds = GetBounds(cells);
        foreach (var cell in cells)
        {
            for(int i = 0; i < 6; i++)
            {
                var a = cell.GetHexCorner(scale, i);
                var b = cell.GetHexCorner(scale, (i + 1) % 6);
                a = NormalizePoint(new(0f, 0f), a, maxSides.X / 2f, bounds);
                b = NormalizePoint(new(0f, 0f), b, maxSides.X / 2f, bounds);

                var aV = GridToSphereCoord(TransformGridPoint(a, maxSides), maxSides.Y / 2f);
                var bV = GridToSphereCoord(TransformGridPoint(b, maxSides), maxSides.Y / 2f);

                Gizmos.DrawLine(aV, bV);
            }
        }*/
        PrintMinMax((maxSides.Y / 2f) * ((range * 2) + 1));
    }

    // Max range for x per hemisphere = (0, PI)
    // Max range for y per hemisphere = (-Radius, Radius)
    public Vector2 TransformGridPoint(Vector2 point, Vector2 maxSides)
    {
        float x = ((point.X + maxSides.Y) - (maxSides.Y / 2f)) / maxSides.Y;
        return new(
            Mathf.PI * x,
            point.Y);
    }

    public Vector3 HexToSphereCoord(UVector2 gridCoord, float sphereRadius, int hexCount)
    {
        float theta = Mathf.Acos(2 * gridCoord.y / sphereRadius);
        float phi = (2 * Mathf.PI / hexCount) * gridCoord.x + Mathf.PI / hexCount;
        float x = sphereRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
        float y = sphereRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
        float z = sphereRadius * Mathf.Cos(theta);
        return new Vector3(x, y, z);
    }

    public Vector3 GridToSphereCoord(UVector2 gridCoord, float sphereRadius)
    {
        float theta = Mathf.Acos(gridCoord.y / sphereRadius);
        float phi = gridCoord.x;
        float x = sphereRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
        float y = sphereRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
        float z = sphereRadius * Mathf.Cos(theta);
        return new Vector3(x, y, z);
    }

    List<Tuple<Vector2, Vector2>> GetBounds(List<Hex> cells)
    {
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (var cell in cells)
        {
            for (int i = 0; i < 6; i++)
            {
                var point = cell.GetHexCorner(scale, i);
                minY = Mathf.Min(minY, point.Y);
                maxY = Mathf.Max(maxY, point.Y);
            }
        }

        return new()
        {
            new(new Vector2(minY, minY), new Vector2(minY, maxY)),
            new(new Vector2(minY, maxY), new Vector2(maxY, maxY)),
            new(new Vector2(maxY, maxY), new Vector2(maxY, minY)),
            new(new Vector2(maxY, minY), new Vector2(minY, minY))
        };
    }
    
    Vector2 GetIntersection(Tuple<Vector2, Vector2> lineA, Tuple<Vector2, Vector2> lineB)
    {
        Vector2 cross = null;
        
        var d = (lineA.Item2.X - lineA.Item1.X) * (lineB.Item2.Y - lineB.Item1.Y) - (lineA.Item2.Y - lineA.Item1.Y) * (lineB.Item2.X - lineB.Item1.X);
        if (d == 0) return cross;
        var u = ((lineB.Item1.X - lineA.Item1.X) * (lineB.Item2.Y - lineB.Item1.Y) - (lineB.Item1.Y - lineA.Item1.Y) * (lineB.Item2.X - lineB.Item1.X)) / d;
        var v = ((lineB.Item1.X - lineA.Item1.X) * (lineA.Item2.Y - lineA.Item1.Y) - (lineB.Item1.Y - lineA.Item1.Y) * (lineA.Item2.X - lineA.Item1.X)) / d;
        if (u < 0 || u > 1 || v < 0 || v > 1) return cross;
        cross = new Vector2(lineA.Item1.X + u * (lineA.Item2.X - lineA.Item1.X), lineA.Item1.Y + u * (lineA.Item2.Y - lineA.Item1.Y));

        return cross;
    }
    
    Vector2 GetIntersection(Tuple<Vector2, Vector2> line, List<Tuple<Vector2, Vector2>> bounds)
    {
        List<Vector2> intersections = new();
        foreach(var bound in bounds)
        {
            var temp = GetIntersection(bound, line);
            if(temp != null) intersections.Add(temp);
        }
        float minDistance = float.PositiveInfinity;
        Vector2 closest = null;
        foreach(var cross in intersections)
        {
            var tempDist = cross.Distance(line.Item1);
            if(tempDist < minDistance)
            {
                minDistance = tempDist;
                closest = cross;
            }
        }
        return closest;
    }

    Vector2 NormalizePoint(Vector2 center, Vector2 point, float radius, List<Tuple<Vector2, Vector2>> bounds)
    {
        var dir = (UVector2)(point - center);
        var lineEnd = (Vector2)((UVector2)center + dir.normalized) * (radius * 2f);
        var cross = GetIntersection(new(center, lineEnd), bounds);
        float lineScale = Mathf.Min(1f, center.Distance(point) / radius);
        return cross * lineScale;
    }

    void PrintMinMax(float radius)
    {
        List<Hex> cells = new Hex(0, 0).GetHexSpiralInRange(range);
        var bounds = GetBounds(cells);

        var center = new Vector2(0, 0);

        foreach (var cell in cells)
        {
            for (int i = 0; i < 6; i++)
            {
                var point = cell.GetHexCorner(scale, i);
                var b = cell.GetHexCorner(scale, (i + 1) % 6);
                Gizmos.DrawLine(point, b);

                point = NormalizePoint(center, point, radius, bounds);
                Gizmos.color = Color.blue;
                if (point is not null) Gizmos.DrawWireSphere(point, 0.1f);
            }
        }

        Gizmos.color = Color.white;
        DrawLine(bounds[0]);
        DrawLine(bounds[1]);
        DrawLine(bounds[2]);
        DrawLine(bounds[3]);
    }

    void DrawLine(Tuple<Vector2, Vector2> line) => Gizmos.DrawLine(line.Item1, line.Item2);
}
