using UnityEngine;
using UnityEngine.SceneManagement; // For reloading scene if needed

public class CoffinKey : Interactable
{
    public bool isCorrectKey = false; // Assign TRUE only for the justified coffin
    private Animator coffinAnimator;
    public DoorOpener doorOpener; // Reference to door opener script

    [Header("Narrative")]
    [SerializeField] private AudioClip wrongWhisperClip;

    private bool hasBeenOpened = false; // To prevent double interactions

    void Start()
    {
        coffinAnimator = GetComponent<Animator>();
        if (doorOpener == null)
        {
            doorOpener = FindObjectOfType<DoorOpener>();
        }

        onInteract.AddListener(TriggerCoffin);
    }

    void TriggerCoffin()
    {
        if (hasBeenOpened)
            return; // Don't allow opening twice

        hasBeenOpened = true;

        // Play Coffin Open Animation
        if (coffinAnimator != null)
        {
            coffinAnimator.SetTrigger("CoffinOpen");
        }

        // After small delay, check the key
        Invoke(nameof(CheckKey), 1.0f); // 1 second delay to let animation play
    }

    void CheckKey()
    {
        if (isCorrectKey)
        {
            Debug.Log("Correct Key! Opening Door...");
            if (doorOpener != null)
            {
                doorOpener.OpenDoor();
            }
        }
        else
        {
            Debug.Log("Wrong Key! Restarting Scene...");
            if (wrongWhisperClip != null)
                AudioSource.PlayClipAtPoint(wrongWhisperClip, transform.position, 0.9f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
