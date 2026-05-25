using UnityEngine;

public class CannonControlSwitcher : Interactable
{
    public GameObject playerRoot;         // Full player GameObject (body + camera)
    public GameObject cannonRoot;          // Full cannon GameObject (cannon + cannon camera)
    
    private bool controllingCannon = false;

    void Start()
    {
        onInteract.AddListener(SwitchToCannon);

        if (cannonRoot != null)
            cannonRoot.SetActive(false); // Start with cannon hidden
    }

    void Update()
    {
        if (controllingCannon)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
            {
                SwitchBackToPlayer();
            }
        }
    }

    void SwitchToCannon()
    {
        if (controllingCannon) return; // Already controlling cannon

        Debug.Log("Switched to Cannon Control!");

        if (playerRoot != null)
            playerRoot.SetActive(false); // Disable player completely (player + camera)

        if (cannonRoot != null)
            cannonRoot.SetActive(true); // Enable cannon (cannon model + cannon camera)

        controllingCannon = true;
    }

    void SwitchBackToPlayer()
    {
        Debug.Log("Switched back to Player Control!");

        if (playerRoot != null)
            playerRoot.SetActive(true); // Re-enable player (player + camera)

        if (cannonRoot != null)
            cannonRoot.SetActive(false); // Disable cannon entirely

        controllingCannon = false;
    }
}
