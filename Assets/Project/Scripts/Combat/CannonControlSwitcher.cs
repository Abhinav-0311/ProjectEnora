using UnityEngine;

public class CannonControlSwitcher : Interactable
{
    public event System.Action EnteredCannon;
    public event System.Action ExitedCannon;

    public bool IsControllingCannon => controllingCannon;

    public GameObject playerRoot;         // Full player GameObject (body + camera)
    public GameObject cannonRoot;          // Full cannon GameObject (cannon + cannon camera)

    [Header("Fallback Visibility")]
    [SerializeField] private GameObject[] additionalPlayerObjectsToHide;

    [Header("Exit Placement")]
    [SerializeField] private Transform playerExitAnchor;
    [SerializeField] private float fallbackExitBackwardOffset = 2.5f;
    [SerializeField] private float fallbackExitSideOffset = 1.35f;
    [SerializeField] private float fallbackExitHeightOffset = 0.15f;

    private bool controllingCannon = false;
    private Renderer[] cachedPlayerRenderers;
    private Collider[] cachedPlayerColliders;
    private Vector3 lastPlayerPosition;
    private Quaternion lastPlayerRotation = Quaternion.identity;

    private void Start()
    {
        SetPromptAction("enter the cannon");
        onInteract.RemoveListener(SwitchToCannon);
        onInteract.AddListener(SwitchToCannon);

        playerRoot = PlayerRuntimeUtility.ResolvePlayerRoot(playerRoot);
        CachePlayerPresentation();

        if (ShouldToggleCannonRoot())
        {
            cannonRoot.SetActive(false); // Start with cannon hidden
        }
    }

    private void Update()
    {
        if (controllingCannon)
        {
            if (Input.GetKeyDown(KeyCode.Q)
                || Input.GetKeyDown(KeyCode.Escape)
                || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                SwitchBackToPlayer();
            }
        }
    }

    void SwitchToCannon()
    {
        if (controllingCannon)
        {
            return;
        }

        playerRoot = PlayerRuntimeUtility.ResolvePlayerRoot(playerRoot);
        if (playerRoot == null)
        {
            return;
        }

        CachePlayerPresentation();
        lastPlayerPosition = playerRoot.transform.position;
        lastPlayerRotation = playerRoot.transform.rotation;

        PlayerRuntimeUtility.PrepareForExternalControl(playerRoot);

        SetPlayerPresentationVisible(false);
        playerRoot.SetActive(false);

        if (ShouldToggleCannonRoot())
        {
            cannonRoot.SetActive(true); // Enable cannon (cannon model + cannon camera)
        }

        controllingCannon = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EnteredCannon?.Invoke();
    }

    void SwitchBackToPlayer()
    {
        if (!controllingCannon)
        {
            return;
        }

        playerRoot = PlayerRuntimeUtility.ResolvePlayerRoot(playerRoot);
        if (playerRoot != null)
        {
            playerRoot.SetActive(true);
            CachePlayerPresentation();
            ResolveExitPose(out Vector3 exitPosition, out Quaternion exitRotation);
            PlayerRuntimeUtility.TeleportPlayer(playerRoot, exitPosition, exitRotation);
            SetPlayerPresentationVisible(true);
            PlayerRuntimeUtility.RestoreAfterExternalControl(playerRoot);
        }

        controllingCannon = false;
        ExitedCannon?.Invoke();

        if (ShouldToggleCannonRoot())
        {
            cannonRoot.SetActive(false);
        }
    }

    private void CachePlayerPresentation()
    {
        cachedPlayerRenderers = playerRoot != null
            ? playerRoot.GetComponentsInChildren<Renderer>(true)
            : System.Array.Empty<Renderer>();

        cachedPlayerColliders = playerRoot != null
            ? playerRoot.GetComponentsInChildren<Collider>(true)
            : System.Array.Empty<Collider>();
    }

    private void SetPlayerPresentationVisible(bool visible)
    {
        for (int i = 0; i < cachedPlayerRenderers.Length; i++)
        {
            if (cachedPlayerRenderers[i] != null)
            {
                cachedPlayerRenderers[i].enabled = visible;
            }
        }

        for (int i = 0; i < cachedPlayerColliders.Length; i++)
        {
            if (cachedPlayerColliders[i] != null)
            {
                cachedPlayerColliders[i].enabled = visible;
            }
        }

        if (additionalPlayerObjectsToHide == null)
        {
            return;
        }

        for (int i = 0; i < additionalPlayerObjectsToHide.Length; i++)
        {
            if (additionalPlayerObjectsToHide[i] != null)
            {
                additionalPlayerObjectsToHide[i].SetActive(visible);
            }
        }
    }

    private void ResolveExitPose(out Vector3 position, out Quaternion rotation)
    {
        if (playerExitAnchor != null)
        {
            position = playerExitAnchor.position;
            rotation = playerExitAnchor.rotation;
            return;
        }

        Transform referenceTransform = cannonRoot != null ? cannonRoot.transform : transform;
        if (referenceTransform != null)
        {
            Vector3 backward = -referenceTransform.forward * fallbackExitBackwardOffset;
            Vector3 side = referenceTransform.right * fallbackExitSideOffset;
            position = referenceTransform.position + backward + side + Vector3.up * fallbackExitHeightOffset;

            Vector3 lookDirection = referenceTransform.position - position;
            lookDirection.y = 0f;
            rotation = lookDirection.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : referenceTransform.rotation;
            return;
        }

        position = lastPlayerPosition;
        rotation = lastPlayerRotation;
    }

    private bool ShouldToggleCannonRoot()
    {
        return cannonRoot != null && cannonRoot != gameObject;
    }
}
