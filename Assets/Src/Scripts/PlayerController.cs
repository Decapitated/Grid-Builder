using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool firstPerson = true;
    public bool canFly = false;

    public bool IsGrounded { get; private set; } = true;

    public float mouseSpeed = 100f;
    public float mouseMultiplier = 10f;
    public float clickLength = 0.333f;

    public float jumpPower = 5f;
    public float movementSpeed = 10f;
    public float sprintMultiplier = 1.5f;

    CharacterController controller;
    Rigidbody rigidbody;
    public Transform cameraTarget;
    Camera camera;
    Animator animator;
    Renderer renderer;
    float rotationX = 0f;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
        camera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        var velocity = rigidbody.velocity;
        UpdateAnimations();

        if (!IsGrounded) print("Not grounded.");
        if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
            rigidbody.velocity = new(velocity.x, rigidbody.velocity.y, velocity.z);
        }
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

        // Check whether the animation controls the position of the character.
        if (!animator.applyRootMotion) UpdateMovement();

        print(rigidbody.velocity);
    }

    void UpdateMovement()
    {
        var move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward * (!(Input.GetKey(KeyCode.LeftShift)) ? 1f : sprintMultiplier);
        if (Input.GetKey(KeyCode.S)) move += -transform.forward;
        if (Input.GetKey(KeyCode.A)) move += -transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        move *= movementSpeed;
        rigidbody.velocity = new(move.x, rigidbody.velocity.y, move.z);
        //rigidbody.AddForce(movementSpeed * Time.deltaTime * move, ForceMode.Force);
        //controller.Move(movementSpeed * Time.deltaTime * move);
        //rigidbody.MovePosition(transform.position + (movementSpeed * Time.deltaTime * move));
    }

    void UpdateAnimations()
    {
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
        if (IsGrounded && Input.GetKeyDown(KeyCode.Space)) animator.SetTrigger(jumpId);
    }

    void OnCollisionStay(Collision collision)
    {
        var bottom = renderer.bounds.center;
        bottom.y -= renderer.bounds.extents.y;
        float minDist = float.PositiveInfinity;
        float angle = 180f;
        // Find closest point to bottom.
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            var tempDist = Vector3.Distance(contact.point, bottom);
            if(tempDist < minDist)
            {
                minDist = tempDist;
                // Check how close the contact normal is to our up vector.
                angle = Vector3.Angle(transform.up, contact.normal);
            }
        }
        // Check if the angle is too steep.
        if (angle <= 45f) IsGrounded = true;
        else IsGrounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        IsGrounded = false;
    }
}