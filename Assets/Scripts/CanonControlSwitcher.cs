using UnityEngine;

public class CannonControlSwitcher : Interactable
{
    public event System.Action EnteredCannon;
    public event System.Action ExitedCannon;

    public GameObject playerRoot;         // Full player GameObject (body + camera)
    public GameObject cannonRoot;          // Full cannon GameObject (cannon + cannon camera)

    [Header("Fallback Visibility")]
    [SerializeField] private GameObject[] additionalPlayerObjectsToHide;

    private bool controllingCannon = false;
    private Renderer[] cachedPlayerRenderers;
    private Collider[] cachedPlayerColliders;

    private void Start()
    {
        onInteract.AddListener(SwitchToCannon);

        if (playerRoot == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                playerRoot = taggedPlayer;
            }
        }

        CachePlayerPresentation();

        if (cannonRoot != null)
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
        if (controllingCannon) return; // Already controlling cannon

        Debug.Log("Switched to Cannon Control!");

        SetPlayerPresentationVisible(false);

        if (playerRoot != null)
        {
            playerRoot.SetActive(false); // Disable player completely (player + camera)
        }

        if (cannonRoot != null)
        {
            cannonRoot.SetActive(true); // Enable cannon (cannon model + cannon camera)
        }

        controllingCannon = true;
        EnteredCannon?.Invoke();
    }

    void SwitchBackToPlayer()
    {
        Debug.Log("Switched back to Player Control!");

        if (playerRoot != null)
        {
            playerRoot.SetActive(true); // Re-enable player (player + camera)
        }

        SetPlayerPresentationVisible(true);

        if (cannonRoot != null)
        {
            cannonRoot.SetActive(false); // Disable cannon entirely
        }

        controllingCannon = false;
        ExitedCannon?.Invoke();
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
}
