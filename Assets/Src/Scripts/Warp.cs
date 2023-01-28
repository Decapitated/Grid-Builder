using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using UnityEngine;

public class Warp : MonoBehaviour
{
    MeshFilter meshFilter;

    [SerializeField]
    private Transform[] Square = new Transform[4];

    Vector2[] quad;
    MVC mvc;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        quad = new Vector2[]
        {
            new(Square[0].position.x, Square[0].position.z),
            new(Square[1].position.x, Square[1].position.z),
            new(Square[2].position.x, Square[2].position.z),
            new(Square[3].position.x, Square[3].position.z)
        };
        mvc = new MVC(quad, meshFilter.mesh.vertices);
    }

    // Update is called once per frame
    void Update()
    {
        var transformed = mvc.TransformPoints(new Vector2[]
        {
            new(Square[0].position.x, Square[0].position.z),
            new(Square[1].position.x, Square[1].position.z),
            new(Square[2].position.x, Square[2].position.z),
            new(Square[3].position.x, Square[3].position.z)
        });
        meshFilter.mesh.vertices = transformed;
    }
}
