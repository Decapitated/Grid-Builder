using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Tunnel : MonoBehaviour
{
    struct Portal
    {
        public Transform transform;
        public Renderer renderer;
        public RenderTexture renderTexture;
        public bool canView;
    }

    [Serializable]
    struct PortalInput
    {
        public Transform transform;
        public bool canView;
    }

    [SerializeField]
    private Shader portalShader;

    [SerializeField]
    private PortalInput[] inputPortals = new PortalInput[2];

    [SerializeField]
    private Camera portalCam;

    Portal A, B;

    void Awake()
    {
        portalCam.enabled = false;
        A = InputToPortal(inputPortals[0]);
        A.renderTexture.Create();

        B = InputToPortal(inputPortals[1]);
        B.renderTexture.Create();
    }
    Portal InputToPortal(PortalInput input) => new()
    {
        transform = input.transform,
        canView = input.canView,
        renderer = input.transform.GetComponent<Renderer>(),
        renderTexture = new(Screen.width, Screen.height, 0)
    };

    void Start()
    {
        A.renderer.material = new Material(portalShader);
        A.renderer.material.SetTexture("_PortalTexture", B.renderTexture);

        B.renderer.material = new Material(portalShader);
        B.renderer.material.SetTexture("_PortalTexture", A.renderTexture);
    }

    void OnDestroy()
    {
        A.renderTexture.Release();
        B.renderTexture.Release();
    }

    void OnRenderObject()
    {

        if (A.canView && VisibleFromCamera(A.renderer, Camera.main))
        {
            RenderCamera(B, A);
        }

        if (B.canView && VisibleFromCamera(B.renderer, Camera.main))
        {
            RenderCamera(A, B);
        }
    }

    void RenderCamera(Portal a, Portal b)
    {
        portalCam.targetTexture = a.renderTexture;
        a.renderer.enabled = false;

        Matrix4x4 m = a.transform.localToWorldMatrix * b.transform.transform.worldToLocalMatrix * Camera.main.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        SetNearClipPlane(a.transform);

        portalCam.Render();

        a.renderer.enabled = true;
    }

    bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds);
    }

    void SetNearClipPlane(Transform portal)
    {
        Transform clipPlane = portal;
        int dot = Math.Sign(Vector3.Dot(clipPlane.forward, portal.position - portalCam.transform.position));

        var camPos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        var camNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        var camDist = -Vector3.Dot(camPos, camNormal);
        var clipPlaneCamSpace = new Vector4(camNormal.x, camNormal.y, camNormal.z, camDist);
        portalCam.projectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCamSpace);
    }
}
