using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    public float mouseSpeed = 100f;
    public float mouseMultiplier = 10f;
    public float scrollScale = 0.1f;
    public float clickLength = 0.333f;

    public GridBuilder buildGrid;

    Camera camera;

    void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    // Movement speed in units per second.
    public float moveSpeed = 1.0F;
    public float minCameraY = 0.5f;
    bool moving = false;
    float startTime;
    Vector3 start, end;
    float journeyLength;

    void Update()
    {
        // Change camera man position.
        if (GetMouseButtonClicked(2) && buildGrid.MouseClosestFace is not null)
        {
            start = transform.position;
            end = buildGrid.MouseClosestFace;
            moving = true;
            startTime = Time.time;
            journeyLength = Vector3.Distance(start, end);

        }
        if(moving)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * moveSpeed;
            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;
            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(start, end, fractionOfJourney);
            if (Vector3.Distance(transform.position, end) <= 0.001) moving = false;
        }

        // Moved this value into a locally global position.
        UnityEngine.Vector2 mouseMove = GetMouseMovement();

        // Rotate camera man.
        if (Input.GetMouseButton(1))
        {
            Quaternion backupQuat = transform.localRotation;
            transform.Rotate(mouseMove.x * Time.deltaTime * Vector3.up, Space.World);
            transform.Rotate(-mouseMove.y * Time.deltaTime * Vector3.right, Space.Self);
            // Undo move if it messes up the camera.
            if (camera.transform.position.y < transform.position.y + minCameraY) transform.localRotation = backupQuat;
        }

        // Move camera man.
        if (Input.GetMouseButton(0))
        {
            transform.Translate(-mouseMove.x * Time.deltaTime * Vector3.right, Space.Self);
            transform.Translate(-mouseMove.y * Time.deltaTime * Vector3.up, Space.World);
        }
        // Camera points at camer man.
        camera.transform.LookAt(transform);

        Vector3 backupVec = camera.transform.position;
        // Camera moves towards camera man.
        camera.transform.Translate(Input.mouseScrollDelta.y * scrollScale * Vector3.forward);
        // Undo move if it messes up the camera.
        if (camera.transform.position.y < transform.position.y) camera.transform.position = backupVec;

        float maxDistance = Hex.GetHexMaxSide(buildGrid.range, buildGrid.scale);
        transform.position = new(
                Mathf.Max(-maxDistance, Mathf.Min(maxDistance, transform.position.x)),
                Mathf.Max(0f, transform.position.y),
                Mathf.Max(-maxDistance, Mathf.Min(maxDistance, transform.position.z)));

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

    UnityEngine.Vector2 GetMouseMovement()
    {
        return new UnityEngine.Vector2(
            Input.GetAxis("Mouse X") * mouseSpeed * mouseMultiplier,
            Input.GetAxis("Mouse Y") * mouseSpeed * mouseMultiplier);
    }
}
