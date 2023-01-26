using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portable : MonoBehaviour
{
    public Vector3 PrevOffset { get; set; }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    public virtual void EnterPortalThreshold()
    {

    }

    public virtual void ExitPortalThreshold()
    {

    }
}