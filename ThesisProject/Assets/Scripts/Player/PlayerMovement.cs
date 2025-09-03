using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    private float originalHeight;
    private CapsuleCollider capsule;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private Rigidbody rb;
    private Animator animator;
    private float xRotation = 0f;
    private bool isGrounded;

    private bool isCrouching = false;
    private float currentSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;

        originalHeight = capsule.height;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (Menu.IsPaused) return; // stop controls when paused

        HandleMouseLook();
        HandleJump();
        HandleRun();
        HandleCrouch();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        if (Menu.IsPaused) return; // stop physics movement when paused

        HandleMovement();
        ApplyExtraGravity();
    }


    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 targetPosition = rb.position + move * currentSpeed * Time.fixedDeltaTime;

        rb.MovePosition(targetPosition);
    }

    //private void HandleMovement()
    //{
    //    float moveX = Input.GetAxis("Horizontal");
    //    float moveZ = Input.GetAxis("Vertical");

    //    Vector3 move = transform.right * moveX + transform.forward * moveZ;
    //    Vector3 targetVelocity = move * currentSpeed;
    //    Vector3 velocity = rb.velocity;

    //    targetVelocity.y = velocity.y;

    //    rb.velocity = targetVelocity;
    //}

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void HandleRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            currentSpeed = runSpeed;
        }
        else if (!isCrouching)
        {
            currentSpeed = walkSpeed;
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            currentSpeed = crouchSpeed;
            capsule.height = crouchHeight;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            currentSpeed = walkSpeed;
            capsule.height = originalHeight;
        }
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void HandleAnimation()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed > 0.01f)
        {
            animator.speed = 3f;
        }
        else
        {
            animator.speed = 0f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
