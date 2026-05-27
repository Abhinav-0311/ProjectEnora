using UnityEngine;
using UnityEngine.Events;

public class Interactor : MonoBehaviour
{
    public LayerMask InteractableLayermask;
    public CrosshairController crosshairController;

    private UnityEvent onInteract;

    private void Start()
    {
        if (crosshairController == null)
        {
            crosshairController = FindFirstObjectByType<CrosshairController>();
        }
    }

    private void Update()
    {
        Camera mainCamera = Camera.main;
        bool isLookingAtInteractable = false;
        bool pressedInteract = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton2);

        if (mainCamera != null
            && Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, 2f, InteractableLayermask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                isLookingAtInteractable = true;
                onInteract = interactable.onInteract;

                if (pressedInteract)
                {
                    onInteract?.Invoke();
                }
            }
        }

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairState(isLookingAtInteractable);
        }
    }
}
