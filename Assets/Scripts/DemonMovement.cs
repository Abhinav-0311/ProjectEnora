using System.Collections;
using UnityEngine;

public class ArcFiringDemon : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementRange = 5f;
    private Vector3 startingPosition;
    private int direction = 1;

    [Header("Attack Settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float fireballSpeed = 8f;
    [SerializeField] private float fireballLifetime = 5f;
    [SerializeField] private float arcHeight = 2f;
    private float attackTimer;

    [Header("Target Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float minAttackAngle = -30f;
    [SerializeField] private float maxAttackAngle = 30f;
    [SerializeField] private Transform targetOverride;
    [SerializeField] private string primaryTargetTag = "Player";
    [SerializeField] private string secondaryTargetTag = "Cannon";

    private Transform player;

    private void Start()
    {
        startingPosition = transform.position;
        ResolveTarget();
    }

    private void Update()
    {
        ResolveTarget();
        PatrolMovement();
        HandleAttack();
    }

    private void PatrolMovement()
    {
        transform.Translate(Vector3.right * direction * movementSpeed * Time.deltaTime, Space.World);

        if (Mathf.Abs(transform.position.x - startingPosition.x) >= movementRange)
        {
            direction *= -1;
        }
    }

    private void HandleAttack()
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(player.position.x, 0f, player.position.z));

        if (distanceToPlayer > detectionRange || !IsPlayerInAttackAngle())
        {
            return;
        }

        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackCooldown;
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private bool IsPlayerInAttackAngle()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Vector3 flatDirectionToPlayer = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
        if (flatDirectionToPlayer.sqrMagnitude <= Mathf.Epsilon)
        {
            return true;
        }

        float angle = Vector3.SignedAngle(flatForward, flatDirectionToPlayer, Vector3.up);
        return angle >= minAttackAngle && angle <= maxAttackAngle;
    }

    private void Attack()
    {
        if (fireballPrefab == null || firePoint == null)
        {
            return;
        }

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        StartCoroutine(MoveInArc(fireball.transform, player.position));
        Destroy(fireball, fireballLifetime);
    }

    private IEnumerator MoveInArc(Transform fireball, Vector3 targetPosition)
    {
        Vector3 startPosition = fireball.position;
        Vector3 directionToTarget = (targetPosition - startPosition).normalized;
        Vector3 velocity = directionToTarget * fireballSpeed;
        velocity.y += Mathf.Sqrt(2f * arcHeight * Mathf.Abs(Physics.gravity.y));

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
            rb.useGravity = true;
        }

        if (fireball.TryGetComponent<Fireball>(out var fireballScript))
        {
            fireballScript.Initialize(velocity);
        }

        yield return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 startPos = Application.isPlaying ? startingPosition : transform.position;
        Gizmos.DrawLine(startPos + Vector3.left * movementRange, startPos + Vector3.right * movementRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        DrawAngleCone();
    }

    private void DrawAngleCone()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);

        float coneLength = detectionRange;
        Quaternion leftRayRotation = Quaternion.AngleAxis(minAttackAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(maxAttackAngle, Vector3.up);

        Vector3 leftDirection = leftRayRotation * transform.forward * coneLength;
        Vector3 rightDirection = rightRayRotation * transform.forward * coneLength;

        Gizmos.DrawRay(transform.position, leftDirection);
        Gizmos.DrawRay(transform.position, rightDirection);

        int segments = 20;
        Vector3 prevPoint = transform.position + leftDirection;
        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.Lerp(minAttackAngle, maxAttackAngle, (float)i / segments);
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 point = transform.position + rot * transform.forward * coneLength;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private void ResolveTarget()
    {
        if (IsValidTarget(targetOverride))
        {
            player = targetOverride;
            return;
        }

        if (IsValidTarget(player))
        {
            return;
        }

        player = FindTargetByTag(primaryTargetTag);
        if (!IsValidTarget(player))
        {
            player = FindTargetByTag(secondaryTargetTag);
        }
    }

    private static Transform FindTargetByTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return null;
        }

        try
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(tagName);
            return targetObject != null ? targetObject.transform : null;
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private static bool IsValidTarget(Transform target)
    {
        return target != null && target.gameObject.activeInHierarchy;
    }
}
