using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CannonControllerNew : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 30f; // Left/right rotation speed (degrees per second)

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float launchForce = 500f;
    public float minForce = 100f;
    public float maxForce = 1500f;
    public float forceAdjustSpeed = 300f; // How fast scrollwheel changes force

    [Header("Trajectory Settings")]
    public int trajectoryPoints = 30;
    public float timeBetweenPoints = 0.1f;
    public LayerMask collisionMask; // optional, for raycasting

    private LineRenderer lineRenderer;
    private bool canShoot = true;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryPoints;
    }

    void Update()
    {
        HandleRotation();
        HandleForceAdjust();
        HandleShooting();
        DrawTrajectory();
    }

    void HandleRotation()
    {
        float move = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrows
        transform.Rotate(Vector3.up * move * moveSpeed * Time.deltaTime);
    }

    void HandleForceAdjust()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            launchForce = Mathf.Clamp(launchForce + scroll * forceAdjustSpeed * scroll, minForce, maxForce);
        }
    }

    void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject ball = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.AddForce(firePoint.forward * launchForce, ForceMode.Impulse); // Instant force

        canShoot = false;
        Invoke(nameof(Reload), 1f); // Small cooldown between shots

        Destroy(ball, 10f); // Clean up cannonball
    }

    void Reload()
    {
        canShoot = true;
    }

    void DrawTrajectory()
    {
        Vector3 startPosition = firePoint.position;
        Vector3 startVelocity = firePoint.forward * launchForce / 1f; // mass=1kg assumed

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 point = CalculatePositionAtTime(startPosition, startVelocity, time);
            lineRenderer.SetPosition(i, point);

            if (i > 0)
            {
                if (Physics.Linecast(lineRenderer.GetPosition(i - 1), point, out RaycastHit hit, collisionMask))
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }

    Vector3 CalculatePositionAtTime(Vector3 startPos, Vector3 startVelocity, float time)
    {
        return startPos + startVelocity * time + 0.5f * Physics.gravity * time * time;
    }
}
