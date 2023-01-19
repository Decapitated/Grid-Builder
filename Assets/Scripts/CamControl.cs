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
    bool moving = false;
    float startTime;
    Vector3 start, end;
    float journeyLength;

    void Update()
    {
        // Change camera man position.
        if (GetMouseButtonClicked(2) && buildGrid.HoveredHex is not null)
        {
            start = transform.position;
            Vector2 pos = buildGrid.HoveredHex.GetHexCenter(buildGrid.scale);
            end = new(pos.X, 0, pos.Y);
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
        // Move camera man.
        if (Input.GetMouseButton(0))
        {
            float maxDistance = Hex.GetHexMaxSide(buildGrid.range, buildGrid.scale);
            transform.Translate(-mouseMove.x * Time.deltaTime * Vector3.right, Space.Self);
            transform.Translate(-mouseMove.y * Time.deltaTime * Vector3.up, Space.World);
            transform.position = new(
                Mathf.Max(-maxDistance, Mathf.Min(maxDistance, transform.position.x)),
                Mathf.Max(0f, transform.position.y),
                Mathf.Max(-maxDistance, Mathf.Min(maxDistance, transform.position.z)));
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

    UnityEngine.Vector2 GetMouseMovement()
    {
        return new UnityEngine.Vector2(
            Input.GetAxis("Mouse X") * mouseSpeed * mouseMultiplier,
            Input.GetAxis("Mouse Y") * mouseSpeed * mouseMultiplier);
    }
}
