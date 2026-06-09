using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CannonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Sliding speed
    public Transform firePoint; // The barrel tip

    [Header("Aiming Settings")]
    public float aimSpeed = 30f;
    public float minAimAngle = 0f;
    public float maxAimAngle = 80f;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public float launchForce = 500f;
   public float projectileLifetime = 10f; // NEW: How long before projectile disappears
   public float shootCooldown = 1f;


    [Header("Trajectory Settings")]
    public int trajectoryPoints = 30;
    public float timeStep = 0.1f;
    public LayerMask collisionMask;

    [Header("Control State")]
    [SerializeField] private CannonControlSwitcher controlSwitcher;
    [SerializeField] private bool requireCannonControl = true;

    private LineRenderer lineRenderer;
    private float nextShootTime = 0f;

    private float currentAimAngle = 45f;

    private void Awake()
    {
        if (controlSwitcher == null)
        {
            controlSwitcher = GetComponent<CannonControlSwitcher>();
        }
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = trajectoryPoints;
        ApplyAimRotation();
        SetTrajectoryVisible(CanAcceptInput());
    }

    private void OnEnable()
    {
        nextShootTime = 0f;
        ApplyAimRotation();
        SetTrajectoryVisible(CanAcceptInput());
    }

    void Update()
    {
        if (!CanAcceptInput())
        {
            SetTrajectoryVisible(false);
            return;
        }

        SetTrajectoryVisible(true);
        HandleMovement();
        HandleAiming();
        DrawTrajectory();
        HandleShooting();
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrows
        Vector3 move = Vector3.right * moveInput * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World); // Slide along world X axis
    }

    void HandleAiming()
    {
        if (firePoint == null)
        {
            return;
        }

        float aimInput = Input.GetAxis("Vertical"); // Up/Down Arrows

        if (aimInput != 0f)
        {
            currentAimAngle += aimInput * aimSpeed * Time.deltaTime;
            currentAimAngle = Mathf.Clamp(currentAimAngle, minAimAngle, maxAimAngle);

            ApplyAimRotation();
        }
    }

   void HandleShooting()
{
    if ((Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.JoystickButton0)
            || Input.GetKeyDown(KeyCode.JoystickButton5))
        && Time.time >= nextShootTime)
    {
        Shoot();
        nextShootTime = Time.time + shootCooldown; // Set next allowed shot time
    }
}


    void Shoot()
{
    if (projectilePrefab != null && firePoint != null)
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * launchForce, ForceMode.Impulse);
        }

        // Automatically destroy the projectile after a certain time
        Destroy(projectile, projectileLifetime);
    }
}


    void DrawTrajectory()
    {
        if (firePoint == null || lineRenderer == null)
        {
            return;
        }

        Vector3 startPosition = firePoint.position;
        Vector3 startVelocity = firePoint.forward * launchForce;

        Vector3[] points = new Vector3[trajectoryPoints];
        lineRenderer.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;
            points[i] = CalculatePointAtTime(startPosition, startVelocity, time);

            if (i > 0)
            {
                if (Physics.Linecast(points[i - 1], points[i], out RaycastHit hit, collisionMask))
                {
                    points[i] = hit.point;
                    lineRenderer.positionCount = i + 1;
                    break;
                }
            }
        }

        lineRenderer.SetPositions(points);
    }

    Vector3 CalculatePointAtTime(Vector3 startPos, Vector3 startVelocity, float time)
    {
        return startPos + startVelocity * time + 0.5f * Physics.gravity * time * time;
    }

    private bool CanAcceptInput()
    {
        return !requireCannonControl || controlSwitcher == null || controlSwitcher.IsControllingCannon;
    }

    private void ApplyAimRotation()
    {
        if (firePoint != null)
        {
            firePoint.localRotation = Quaternion.Euler(-currentAimAngle, 0f, 0f);
        }
    }

    private void SetTrajectoryVisible(bool visible)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = visible;
        }
    }
}
