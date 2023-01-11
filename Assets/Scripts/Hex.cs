using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex
{
    static readonly Hex[] axial_direction_vectors = new Hex[] {
        new Hex(+1, 0), new Hex(+1, -1), new Hex(0, -1),
        new Hex(-1, 0), new Hex(-1, +1), new Hex(0, +1)
    };

    public Hex(float q, float r)
    {
        Q = q;
        R = r;
    }

    #region Public

    public float Q { get; private set; }
    public float R { get; private set; }
    public float S { get => -Q - R; }

    public static float AxialDistance(Hex a, Hex b) => (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.Q + a.R - b.Q - b.R) + Mathf.Abs(a.R - b.R)) / 2f;
    public Hex AxialNeighbor(Hex hex, int direction) => AxialAdd(hex, AxialDirection(direction));

    public Hex[] GetHexInRange(int range) => GetHexInRange(this, range);
    public static Hex[] GetHexInRange(Hex center, int range)
    {
        var results = new List<Hex>();
        for(int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
            {
                results.Add(AxialAdd(center, new(q, r)));
            }
        }
        return results.ToArray();
    }

    public bool IsInRange(Hex other, int range) => Hex.AxialDistance(this, other) <= range;

    public struct RangeInfo
    {
        public Hex center;
        public int range;
    }
    public static Hex[] GetHexIntersection(RangeInfo[] ranges)
    {
        HexArrays hexArrays = GetHexArrays(ranges);
        var MaxQ = Mathf.Min(hexArrays.MinQ.ToArray());
        var MinQ = Mathf.Max(hexArrays.MaxQ.ToArray());
        var MaxR = Mathf.Min(hexArrays.MinR.ToArray());
        var MinR = Mathf.Max(hexArrays.MaxR.ToArray());
        var MaxS = Mathf.Min(hexArrays.MinS.ToArray());
        var MinS = Mathf.Max(hexArrays.MaxS.ToArray());

        var results = new List<Hex>();
        for (float q = MinQ; q <= MaxQ; q++)
        {
            for (float r = Mathf.Max(MinR, -q - MaxS); r <= Mathf.Min(MaxR, -q - MinS); r++)
            {
                results.Add(new(q, r));
            }
        }
        return results.ToArray();
    }

    public Vector2 GetHexCenter(float size)
    {
        var x = size * (3f / 2 * Q);
        var y = size * (Mathf.Sqrt(3) / 2 * Q + Mathf.Sqrt(3) * R);
        return new(x, y);
    }

    public static Hex PointToHex(Vector2 point, float size)
    {
        var q = (2f / 3 * point.x) / size;
        var r = (-1f / 3 * point.x + Mathf.Sqrt(3) / 3 * point.y) / size;
        return Round(new(q, r));
    }

    public Vector2 GetHexCorner(float size, int i)
    {
        Vector2 center = GetHexCenter(size);
        var angle_deg = 60 * i;
        var angle_rad = Mathf.PI / 180 * angle_deg;
        return new(center.x + size * Mathf.Cos(angle_rad), center.y + size * Mathf.Sin(angle_rad));
    }

    public static float GetHexMaxSide(int range, float size)
    {
        int diameter = range * 2 + 1;
        return diameter * (Mathf.Sqrt(3) * size);
    }
    #endregion

    #region Private

    struct HexArrays
    {
        public List<float> MaxQ;
        public List<float> MinQ;
        public List<float> MaxR;
        public List<float> MinR;
        public List<float> MaxS;
        public List<float> MinS;
    }
    static Hex AxialDirection(int direction) => axial_direction_vectors[direction];
    static Hex AxialAdd(Hex hex, Hex vec) => new(hex.Q + vec.Q, hex.R + vec.R);
    static HexArrays GetHexArrays(RangeInfo[] ranges)
    {
        HexArrays hexArrays = new()
        {
            MaxQ = new(),
            MinQ = new(),
            MaxR = new(),
            MinR = new(),
            MaxS = new(),
            MinS = new(),
        };
        foreach (var rangeInfo in ranges)
        {
            hexArrays.MaxQ.Add(rangeInfo.center.Q - rangeInfo.range);
            hexArrays.MinQ.Add(rangeInfo.center.Q + rangeInfo.range);
            hexArrays.MaxR.Add(rangeInfo.center.R - rangeInfo.range);
            hexArrays.MinR.Add(rangeInfo.center.R + rangeInfo.range);
            hexArrays.MaxS.Add(rangeInfo.center.S - rangeInfo.range);
            hexArrays.MinS.Add(rangeInfo.center.S + rangeInfo.range);
        }
        return hexArrays;
    }
    static Hex Round(Hex hex)
    {
        int q = (int)Mathf.Round(hex.Q);
        int r = (int)Mathf.Round(hex.R);
        int s = (int)Mathf.Round(hex.S);

        var q_diff = Mathf.Abs(q - hex.Q);
        var r_diff = Mathf.Abs(r - hex.R);
        var s_diff = Mathf.Abs(s - hex.S);

        if (q_diff > r_diff && q_diff > s_diff) {
            q = -r - s;
        }
        else if (r_diff > s_diff)
        {
            r = -q - s;
        }
        return new(q, r);
    }

    #endregion
}
