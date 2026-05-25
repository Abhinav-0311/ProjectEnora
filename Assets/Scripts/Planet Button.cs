using UnityEngine;

public class PlanetButton : MonoBehaviour
{
    public string buttonValue;
    [SerializeField] private float interactionDistance = 5f;

    public delegate void ButtonPressEvent(string value);
    public static event ButtonPressEvent OnButtonPressed;

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

        if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out RaycastHit hit, interactionDistance))
        {
            return false;
        }

        return hit.transform == transform || hit.transform.IsChildOf(transform);
    }
}
