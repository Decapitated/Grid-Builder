using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour
{
    public GridBuilder gridBuilder;
    public Vector2 id;

    Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if(gridBuilder.MouseClosestFace != null && gridBuilder.MouseClosestFace == id) renderer.sharedMaterial.SetFloat("_Active", 1);
        else renderer.sharedMaterial.SetFloat("_Active", 0);
    }
}
