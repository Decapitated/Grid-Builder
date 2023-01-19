using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform Point;
    public Transform ReflectPoint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Point.position = (A.position + B.position + C.position) / 3;
        ReflectPoint.position = ReflectPointAcrossLine(Point.position, A.position, B.position);
    }

    Vector3 ReflectPointAcrossLine(Vector3 P, Vector3 A, Vector3 B)
    {
        Vector3 lineDirection = (B - A).normalized;
        Vector3 projection = Vector3.Dot((P - A), lineDirection) / Vector3.Dot(lineDirection, lineDirection) * lineDirection + A;
        Vector3 reflection = 2 * projection - P;
        return reflection;
    }

    UnityEngine.Vector2 ReflectPointAcrossLine(UnityEngine.Vector2 P, UnityEngine.Vector2 A, UnityEngine.Vector2 B)
    {
        UnityEngine.Vector2 lineDirection = (B - A).normalized;
        UnityEngine.Vector2 projection = UnityEngine.Vector2.Dot((P - A), lineDirection) / UnityEngine.Vector2.Dot(lineDirection, lineDirection) * lineDirection + A;
        UnityEngine.Vector2 reflection = 2 * projection - P;
        return reflection;
    }
}
