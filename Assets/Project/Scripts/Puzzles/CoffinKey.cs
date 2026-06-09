using UnityEngine;

public class CoffinKey : Interactable
{
    public event System.Action<CoffinKey> CoffinChosen;
    public event System.Action<CoffinKey> CorrectCoffinChosen;
    public event System.Action<CoffinKey> WrongCoffinChosen;

    public bool isCorrectKey = false; // Assign TRUE only for the justified coffin
    private Animator coffinAnimator;
    public DoorOpener doorOpener; // Reference to door opener script

    [Header("Narrative")]
    [SerializeField] private AudioClip wrongWhisperClip;

    private bool hasBeenOpened = false; // To prevent double interactions

    void Start()
    {
        SetPromptAction("open the coffin");
        coffinAnimator = GetComponent<Animator>();
        if (doorOpener == null)
        {
            doorOpener = FindFirstObjectByType<DoorOpener>();
        }

        onInteract.RemoveListener(TriggerCoffin);
        onInteract.AddListener(TriggerCoffin);
    }

    void TriggerCoffin()
    {
        if (hasBeenOpened)
            return; // Don't allow opening twice

        hasBeenOpened = true;
        CoffinChosen?.Invoke(this);

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
            CorrectCoffinChosen?.Invoke(this);
            if (doorOpener != null)
            {
                doorOpener.OpenDoor();
            }
        }
        else
        {
            WrongCoffinChosen?.Invoke(this);
            if (NarrativeHUD.Instance != null)
            {
                NarrativeHUD.Instance.ShowContextCard("Hall of Death", "The coffin was false. Judgment returns you to the beginning.", 2.4f);
            }

            if (wrongWhisperClip != null)
                AudioSource.PlayClipAtPoint(wrongWhisperClip, transform.position, 0.9f);
            SceneTransitionController.ReloadCurrentScene();
        }
    }
}
