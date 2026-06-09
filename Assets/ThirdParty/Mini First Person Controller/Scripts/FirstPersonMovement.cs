using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Grounding")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private float groundAcceleration = 48f;
    [SerializeField] private float groundDeceleration = 64f;
    [SerializeField] private float airAcceleration = 18f;
    [SerializeField, Range(0f, 1f)] private float airControlPercent = 0.5f;
    [SerializeField] private float extraGravityMultiplier = 2.35f;
    [SerializeField] private float groundedStickVelocity = -2f;
    [SerializeField] private float maxFallSpeed = 32f;

    private Rigidbody playerRigidbody;
    private Vector2 moveInput;
    private bool runInput;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();
    void Reset()
    {
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get the rigidbody on this.
        playerRigidbody = GetComponent<Rigidbody>();

        if (groundCheck == null)
        {
            groundCheck = GetComponentInChildren<GroundCheck>();
        }
    }

    void Update()
    {
        runInput = canRun && Input.GetKey(runningKey);
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    void FixedUpdate()
    {
        // Update IsRunning from cached input.
        IsRunning = runInput;

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity = new Vector2(
            moveInput.x * targetMovingSpeed,
            moveInput.y * targetMovingSpeed);

        // Apply movement.
        if (playerRigidbody != null)
        {
            bool isGrounded = groundCheck == null || groundCheck.isGrounded;
            float controlPercent = isGrounded ? 1f : airControlPercent;
            float acceleration = isGrounded
                ? (moveInput.sqrMagnitude > 0.0001f ? groundAcceleration : groundDeceleration)
                : airAcceleration;

            Vector3 desiredWorldVelocity = transform.rotation * new Vector3(targetVelocity.x, 0f, targetVelocity.y);
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredWorldVelocity,
                acceleration * controlPercent * Time.fixedDeltaTime);

            float verticalVelocity = currentVelocity.y;
            if (isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }
            else if (!isGrounded)
            {
                verticalVelocity += Physics.gravity.y * (extraGravityMultiplier - 1f) * Time.fixedDeltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
            }

            playerRigidbody.linearVelocity = new Vector3(
                horizontalVelocity.x,
                verticalVelocity,
                horizontalVelocity.z);
        }
    }
}
