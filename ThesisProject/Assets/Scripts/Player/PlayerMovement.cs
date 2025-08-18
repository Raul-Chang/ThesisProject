using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor in middle of screen
    }

    private void Update()
    {
        HandleMouseLook();
        HandleJump();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyExtraGravity();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 targetVelocity = move * moveSpeed;
        Vector3 velocity = rb.velocity;

        // Keep vertical velocity (gravity/jump)
        targetVelocity.y = velocity.y;

        rb.velocity = targetVelocity;
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Simple grounded check
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}

