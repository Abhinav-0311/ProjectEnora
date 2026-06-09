using UnityEngine;

[ExecuteInEditMode]
public class GroundCheck : MonoBehaviour
{
    [Tooltip("Maximum distance from the ground.")]
    public float distanceThreshold = .15f;
    [Tooltip("Radius of the grounding sphere cast.")]
    public float sphereRadius = .18f;
    [Tooltip("Which layers should count as ground.")]
    public LayerMask groundLayers = ~0;

    [Tooltip("Whether this transform is grounded now.")]
    public bool isGrounded = true;
    /// <summary>
    /// Called when the ground is touched again.
    /// </summary>
    public event System.Action Grounded;

    const float OriginOffset = .001f;
    Transform RootTransform => transform.root;
    Vector3 SphereCastOrigin => transform.position + Vector3.up * (sphereRadius + OriginOffset);
    float SphereCastDistance => distanceThreshold + OriginOffset;


    void FixedUpdate()
    {
        // Check if we are grounded now.
        bool isGroundedNow = IsGroundedBySphereCast();

        // Call event if we were in the air and we are now touching the ground.
        if (isGroundedNow && !isGrounded)
        {
            Grounded?.Invoke();
        }

        // Update isGrounded.
        isGrounded = isGroundedNow;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a line in the Editor to show whether we are touching the ground.
        Color gizmoColor = isGrounded ? Color.white : Color.red;
        Debug.DrawLine(SphereCastOrigin, SphereCastOrigin + Vector3.down * SphereCastDistance, gizmoColor);
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(SphereCastOrigin, sphereRadius);
        Gizmos.DrawWireSphere(SphereCastOrigin + Vector3.down * SphereCastDistance, sphereRadius);
    }

    private bool IsGroundedBySphereCast()
    {
        Ray ray = new Ray(SphereCastOrigin, Vector3.down);
        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            sphereRadius,
            SphereCastDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.transform.IsChildOf(RootTransform))
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
