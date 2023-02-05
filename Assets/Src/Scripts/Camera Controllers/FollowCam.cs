using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float smoothTime = 0.333333f;
    Vector3 velocity = Vector3.zero;

    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        SetViewDistance(currentViewIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            currentViewIndex = (currentViewIndex + 1) % ViewDistances.Length;
            SetViewDistance(currentViewIndex);
        }
    }

    [SerializeField]
    private float mouseSpeed = 5f;
    [SerializeField]
    private float mouseMultiplier = 100f;

    float[] ViewDistances = new float[]
    {
        2, 4, 8, 16
    };
    int currentViewIndex = 0;

    public float turnSpeed = 5f;
    void FixedUpdate()
    {
        // Moved this value into a locally global position.
        Vector2 mouseMove = Utilities.GetMouseMovement() * (mouseSpeed * mouseMultiplier);

        //if(Input.GetMouseButton(1))
            

        if(Input.GetKey(KeyCode.A))
            transform.Rotate(-turnSpeed * Time.deltaTime * Vector3.up, Space.World);
        else if (Input.GetKey(KeyCode.D))
            transform.Rotate(turnSpeed * Time.deltaTime * Vector3.up, Space.World);
        //else transform.Rotate(mouseMove.x * Time.deltaTime * Vector3.up, Space.World);
    }

    void SetViewDistance(int viewIndex)
    {
        camera.transform.localPosition = new(0, 0, -ViewDistances[viewIndex]);
    }

    public void MoveCamera(Vector3 target)
    {
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
    }
}
