using UnityEngine;

public class Jump : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    public float jumpStrength = 2;
    [SerializeField, Tooltip("Converts jumpStrength into a sharper takeoff velocity.")]
    private float jumpVelocityMultiplier = 2.25f;
    [SerializeField, Tooltip("Allows a jump to still register briefly after stepping off an edge.")]
    private float coyoteTime = 0.12f;
    [SerializeField, Tooltip("Buffers jump input briefly so presses just before landing still work.")]
    private float jumpBufferTime = 0.12f;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    private bool jumpQueued;
    private float coyoteTimeRemaining;
    private float jumpBufferRemaining;


    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get rigidbody.
        playerRigidbody = GetComponent<Rigidbody>();

        if (groundCheck == null)
        {
            groundCheck = GetComponentInChildren<GroundCheck>();
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
            jumpBufferRemaining = jumpBufferTime;
        }
    }

    void FixedUpdate()
    {
        if (jumpBufferRemaining > 0f)
        {
            jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.fixedDeltaTime);
        }

        bool isGrounded = groundCheck == null || groundCheck.isGrounded;
        if (isGrounded)
        {
            coyoteTimeRemaining = coyoteTime;
        }
        else
        {
            coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - Time.fixedDeltaTime);
        }

        if (!jumpQueued && jumpBufferRemaining <= 0f)
        {
            return;
        }

        jumpQueued = false;

        if (playerRigidbody == null || coyoteTimeRemaining <= 0f)
        {
            return;
        }

        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.y = Mathf.Max(0f, velocity.y);
        velocity.y = Mathf.Max(jumpStrength, jumpStrength * jumpVelocityMultiplier);
        playerRigidbody.linearVelocity = velocity;
        coyoteTimeRemaining = 0f;
        jumpBufferRemaining = 0f;
        Jumped?.Invoke();
    }
}
