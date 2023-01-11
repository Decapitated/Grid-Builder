using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    public Transform target;
    public float mouseSpeed = 100f;
    public float mouseMultiplier = 10f;
    public float scrollScale = 0.1f;
    public float clickLength = 0.333f;

    public BuildGrid buildGrid;
    Camera camera;

    void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Change camera man position.
        if (GetMouseButtonClicked(2) && buildGrid.HoveredHex != null)
        {
            Vector2 pos = buildGrid.HoveredHex.GetHexCenter(buildGrid.size);
            transform.position = new(pos.x, transform.position.y, pos.y);
        }
        // Moved this value into a locally global position.
        Vector2 mouseMove = GetMouseMovement();
        // Move camera man.
        if (Input.GetMouseButton(0))
        {
            transform.Translate(-mouseMove.x * Time.deltaTime * Vector3.right, Space.Self);
            transform.Translate(-mouseMove.y * Time.deltaTime * Vector3.up, Space.World);
        }
        // Rotate camera man.
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(mouseMove.x * Time.deltaTime * Vector3.up, Space.World);
            transform.Rotate(-mouseMove.y * Time.deltaTime * Vector3.right, Space.Self);
        }
        // Camera points at camer man.
        camera.transform.LookAt(transform);
        // Camera moves towards camera man.
        camera.transform.Translate(Input.mouseScrollDelta.y * scrollScale * Vector3.forward);
    }

    // Checks if the button was clicked within the set length of time.
    Dictionary<int, float> downStartTimes = new();
    public bool GetMouseButtonClicked(int button)
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

    Vector2 GetMouseMovement()
    {
        return new Vector2(
            Input.GetAxis("Mouse X") * mouseSpeed * mouseMultiplier,
            Input.GetAxis("Mouse Y") * mouseSpeed * mouseMultiplier);
    }
}
