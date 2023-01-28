using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    List<Portable> trackedPortables = new();

    public Tunnel tunnel;

    void LateUpdate()
    {
        for(int i = 0; i < trackedPortables.Count; i++)
        {
            var portable = trackedPortables[i];
            var offset = portable.transform.position - transform.position;
            int side = (int)Mathf.Sign(Vector3.Dot(offset, transform.forward));
            int sideOld = (int)Mathf.Sign(Vector3.Dot(portable.PrevOffset, transform.forward));

            if(side != sideOld)
            {
                tunnel.TeleportPortable(this, portable);
                trackedPortables.RemoveAt(i);
                i--;
            }
            else
            {
                portable.PrevOffset = offset;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<Portable>();
        if (obj)
        {
            OnPortableEnter(obj);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<Portable>();
        if (obj)
        {
            OnPortableExit(obj);
        }
    }

    public void OnPortableEnter(Portable portable)
    {
        if(!trackedPortables.Contains(portable))
        {
            portable.EnterPortalThreshold();
            portable.PrevOffset = portable.transform.position - transform.position;
            trackedPortables.Add(portable);
        }
    }

    void OnPortableExit(Portable portable)
    {
        if (trackedPortables.Contains(portable))
        {
            portable.ExitPortalThreshold();
            trackedPortables.Remove(portable);
        }
    }
}
