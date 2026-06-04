using UnityEngine;

public class Jump : MonoBehaviour
{
    private Rigidbody playerRigidbody;
    public float jumpStrength = 2;
    [SerializeField, Tooltip("Converts jumpStrength into a sharper takeoff velocity.")]
    private float jumpVelocityMultiplier = 2.25f;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    private bool jumpQueued;


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
        }
    }

    void FixedUpdate()
    {
        if (!jumpQueued)
        {
            return;
        }

        jumpQueued = false;

        if (playerRigidbody == null || (groundCheck != null && !groundCheck.isGrounded))
        {
            return;
        }

        Vector3 velocity = playerRigidbody.linearVelocity;
        velocity.y = Mathf.Max(0f, velocity.y);
        velocity.y = Mathf.Max(jumpStrength, jumpStrength * jumpVelocityMultiplier);
        playerRigidbody.linearVelocity = velocity;
        Jumped?.Invoke();
    }
}
