using UnityEngine;

public class PlanetButton : MonoBehaviour
{
    public string buttonValue;
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float interactionRadius = 0.08f;

    public delegate void ButtonPressEvent(string value);
    public static event ButtonPressEvent OnButtonPressed;

    private PlanetButton rootButton;

    private void Awake()
    {
        rootButton = GetComponentInParent<PlanetButton>();
        if (rootButton == null)
        {
            rootButton = this;
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            return;
        }

        if (IsLookTargeted())
        {
            PressButton();
        }
    }

    private void OnMouseDown()
    {
        PressButton();
    }

    private void PressButton()
    {
        OnButtonPressed?.Invoke(buttonValue);
    }

    private bool IsLookTargeted()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        if (!Physics.SphereCast(
                mainCamera.transform.position,
                interactionRadius,
                mainCamera.transform.forward,
                out RaycastHit hit,
                interactionDistance))
        {
            return false;
        }

        PlanetButton hitButton = hit.collider.GetComponentInParent<PlanetButton>();
        return hitButton != null && hitButton == rootButton;
    }
}
