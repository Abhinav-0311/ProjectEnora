using UnityEngine;

public class Interactor : MonoBehaviour
{
    public LayerMask InteractableLayermask;
    public CrosshairController crosshairController;
    [SerializeField] private float interactionDistance = 2.4f;

    private Interactable hoveredInteractable;

    private void Start()
    {
        if (crosshairController == null)
        {
            crosshairController = FindFirstObjectByType<CrosshairController>();
        }
    }

    private void OnDisable()
    {
        hoveredInteractable = null;

        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ClearInteractionPrompt();
        }

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairState(false);
        }
    }

    private void Update()
    {
        Camera mainCamera = Camera.main;
        bool isLookingAtInteractable = false;
        bool pressedInteract = InteractionInput.IsInteractPressedThisFrame();
        Interactable currentInteractable = null;

        if (mainCamera != null
            && Physics.Raycast(
                InteractionInput.GetCenteredViewRay(mainCamera),
                out RaycastHit hit,
                interactionDistance,
                InteractableLayermask,
                QueryTriggerInteraction.Ignore))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                isLookingAtInteractable = true;
                currentInteractable = interactable;

                if (pressedInteract)
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlaySound2D(FeedbackSoundNames.Click, 0.78f, 0.98f);
                    }

                    interactable.onInteract?.Invoke();
                }
            }
        }

        if (currentInteractable != hoveredInteractable)
        {
            if (currentInteractable != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound2D(FeedbackSoundNames.Hover, 0.48f, 1f);
            }

            hoveredInteractable = currentInteractable;
        }

        if (NarrativeHUD.Instance != null)
        {
            if (currentInteractable != null)
            {
                NarrativeHUD.Instance.SetInteractionPrompt(currentInteractable.GetPromptText());
            }
            else
            {
                NarrativeHUD.Instance.ClearInteractionPrompt();
            }
        }

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairState(isLookingAtInteractable);
        }
    }
}
