using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool firstPerson = true;
    public bool canFly = false;

    public float mouseSpeed = 100f;
    public float mouseMultiplier = 10f;
    public float clickLength = 0.333f;

    public float movementSpeed = 10f;

    Rigidbody rigidbody;
    public Transform cameraTarget;
    Camera camera;
    Animator animator;
    float rotationX = 0f;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.sleepThreshold = 0;
        camera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // Moved this value into a locally global position.
        Vector2 mouseMove = Utilities.GetMouseMovement() * (mouseSpeed * mouseMultiplier);

        transform.Rotate(mouseMove.x * Time.deltaTime * Vector3.up, Space.World);
        rotationX += mouseMove.y * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        if (firstPerson)
        {
            camera.transform.localPosition = new(0f, 0f, 0f);
            camera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            camera.transform.localPosition = new(0f, 0f, -5f);
            camera.transform.LookAt(cameraTarget);
        }
        cameraTarget.transform.localRotation = Quaternion.Euler(-rotationX, 0f, 0f);

        if (canFly && rigidbody.useGravity)        rigidbody.useGravity = false;
        else if (!canFly && !rigidbody.useGravity) rigidbody.useGravity = true;

        int moveTypeId = Animator.StringToHash("movementType");
        int walkDirId = Animator.StringToHash("walkDir");
        int jumpId = Animator.StringToHash("jump");

        if (Input.GetKey(KeyCode.W))
        {
            // Posotive multiplier gives forwards walk.
            animator.SetFloat(walkDirId, 1f);
            if (Input.GetKey(KeyCode.LeftShift))
                 animator.SetInteger(moveTypeId, 2);
            else animator.SetInteger(moveTypeId, 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // Negative multiplier gives backwards walk.
            animator.SetFloat(walkDirId, -1f);
            animator.SetInteger(moveTypeId, 1);
        }
        else if (Input.GetKey(KeyCode.A)) animator.SetInteger(moveTypeId, 4);
        else if (Input.GetKey(KeyCode.D)) animator.SetInteger(moveTypeId, 3);
        else animator.SetInteger(moveTypeId, 0);
        if (Input.GetKey(KeyCode.Space)) animator.SetTrigger(jumpId);
        //if (canFly && Input.GetKey(KeyCode.Space)) keyDir += Vector3.up;
        //if (canFly && Input.GetKey(KeyCode.LeftControl)) keyDir += Vector3.down;
        //transform.Translate((Input.GetKey(KeyCode.LeftShift) ? 10f : 1f) * movementSpeed * Time.deltaTime * keyDir, Space.Self);
    }
}
