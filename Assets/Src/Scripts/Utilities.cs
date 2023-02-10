using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector = UnityEngine.Vector2;

public static class Utilities
{
    // Checks if the button was clicked within the set length of time.
    static Dictionary<int, float> downStartTimes = new();
    public static bool GetMouseButtonClicked(int button, float clickLength = 0.333f)
    {
        float currentTime = Time.fixedTime;
        if (Input.GetMouseButtonDown(button)) downStartTimes[button] = currentTime;
        if (Input.GetMouseButtonUp(button))
        {
            float clickTime = currentTime - downStartTimes[button];
            if (clickTime <= clickLength)
            {
                return true;
            }
        }
        return false;
    }

    public static Vector2 GetMouseMovement() => new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

    public static Ray GetRayOrigin(Camera camera)
    {
        return camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
    }

    public static bool ScreenRaycast(Camera camera, out RaycastHit hitInfo, LayerMask layerMask)
    {
        Ray rayOrigin = GetRayOrigin(camera);
        return Physics.Raycast(rayOrigin, out hitInfo, Mathf.Infinity, layerMask);
    }

    public static bool ScreenSphereCast(float radius, Camera camera, out RaycastHit hitInfo, LayerMask layerMask)
    {
        Ray rayOrigin = GetRayOrigin(camera);
        return Physics.SphereCast(rayOrigin, radius, out hitInfo, Mathf.Infinity, layerMask);
    }
}
