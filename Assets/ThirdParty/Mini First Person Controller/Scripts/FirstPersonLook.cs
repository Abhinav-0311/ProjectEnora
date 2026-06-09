using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    [SerializeField] Camera attachedCamera;
    public float sensitivity = 2;
    public float smoothing = 1.5f;
    [SerializeField] private float wallSafeNearClipPlane = 0.05f;
    private const float MaxRecommendedNearClipPlane = 0.03f;

    Vector2 velocity;
    Vector2 frameVelocity;


    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
        attachedCamera = GetComponent<Camera>();
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (character == null)
        {
            FirstPersonMovement movement = GetComponentInParent<FirstPersonMovement>();
            if (movement != null)
            {
                character = movement.transform;
            }
        }

        if (attachedCamera == null)
        {
            attachedCamera = GetComponent<Camera>();
        }

        if (attachedCamera != null)
        {
            attachedCamera.nearClipPlane = Mathf.Min(
                attachedCamera.nearClipPlane,
                Mathf.Min(wallSafeNearClipPlane, MaxRecommendedNearClipPlane));
        }
    }

    void Update()
    {
        if (GameplayOverlayState.IsOverlayActive || Cursor.lockState != CursorLockMode.Locked || character == null)
        {
            return;
        }

        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        float smoothingFactor = smoothing <= 0.01f ? 1f : 1f / smoothing;
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, smoothingFactor);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }
}
