using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Portal : MonoBehaviour
{
    public Shader portalShader;
    public Portal otherPortal;

    public RenderTexture Texture { get; private set; }
    public Camera PortalCamera { get; private set; }

    public MeshRenderer Screen { get; private set; }

    public bool oneWay = false;

    void Awake()
    {
        PortalCamera = GetComponentInChildren<Camera>();
        PortalCamera.enabled = false;

        Screen = GetComponent<MeshRenderer>();
        Screen.material = new Material(portalShader);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    void OnDestroy()
    {
        if (Texture != null) Texture.Release();
    }

    [Range(1, 10)]
    public int recursionLimit = 1;


    // All work with this but is being weird.
    void OnRenderObject()
    {
        if (!VisibleFromCamera(otherPortal.Screen, Camera.main))
        {
            /*if (Texture != null)
            {
                Texture.Release();
                Texture = null;
            }*/
            return;
        }

        Screen.enabled = false;

        CreateTexture();

        Matrix4x4 m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * Camera.main.transform.localToWorldMatrix;
        PortalCamera.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        SetNearClipPlane();

        PortalCamera.Render();

        Screen.enabled = true;
    }

    void RecursiveRender()
    {
        if (!VisibleFromCamera(otherPortal.Screen, Camera.main)) return;

        CreateTexture();

        Matrix4x4 localToWorld = Camera.main.transform.localToWorldMatrix;
        Matrix4x4[] matrices = new Matrix4x4[recursionLimit];
        for (int i = 0; i < recursionLimit; i++)
        {
            localToWorld = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * localToWorld;
            matrices[recursionLimit - i - 1] = localToWorld;
        }

        Screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

        for (int i = 0; i < recursionLimit; i++)
        {
            PortalCamera.transform.SetPositionAndRotation(matrices[i].GetColumn(3), matrices[i].rotation);
            SetNearClipPlane();
            PortalCamera.Render();
        }

        Screen.shadowCastingMode = ShadowCastingMode.On;
    }

    void CreateTexture()
    {
        if (Texture == null || Texture.width != UnityEngine.Screen.width || Texture.height != UnityEngine.Screen.height)
        {
            if(Texture != null)
            {
                Texture.Release();
            }
            Texture = new(UnityEngine.Screen.width, UnityEngine.Screen.height, 16);
            PortalCamera.targetTexture = Texture;
            otherPortal.Screen.material.SetTexture("_PortalTexture", Texture);
        }
    }

    bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds);
    }

    void SetNearClipPlane()
    {
        Transform clipPlane = transform;
        int dot = Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - PortalCamera.transform.position));

        var camPos = PortalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        var camNormal = PortalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        var camDist = -Vector3.Dot(camPos, camNormal);
        var clipPlaneCamSpace = new Vector4(camNormal.x, camNormal.y, camNormal.z, camDist);
        PortalCamera.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCamSpace);
    }
}
