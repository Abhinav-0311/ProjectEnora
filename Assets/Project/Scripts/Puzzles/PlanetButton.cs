using UnityEngine;

public class PlanetButton : MonoBehaviour
{
    public string buttonValue;
    [SerializeField] private float interactionDistance = 6f;
    [SerializeField] private float interactionRadius = 0.18f;
    [SerializeField, Range(0.7f, 0.999f)] private float directAimThreshold = 0.94f;

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
        if (!InteractionInput.IsInteractPressedThisFrame())
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
        if (!Cursor.visible)
        {
            return;
        }

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

        if (TryGetTargetedButton(mainCamera, out PlanetButton targetedButton))
        {
            return targetedButton == rootButton;
        }

        Vector3 toButton = rootButton.transform.position - mainCamera.transform.position;
        float distance = toButton.magnitude;
        if (distance > interactionDistance || distance <= Mathf.Epsilon)
        {
            return false;
        }

        float facingDot = Vector3.Dot(mainCamera.transform.forward, toButton.normalized);
        if (facingDot < directAimThreshold)
        {
            return false;
        }

        if (Physics.Raycast(
                mainCamera.transform.position,
                toButton.normalized,
                out RaycastHit obstructionHit,
                distance + 0.05f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            PlanetButton obstructionButton = obstructionHit.collider.GetComponentInParent<PlanetButton>();
            return obstructionButton != null && obstructionButton == rootButton;
        }

        return true;
    }

    private bool TryGetTargetedButton(Camera mainCamera, out PlanetButton targetedButton)
    {
        Ray ray = InteractionInput.GetCenteredViewRay(mainCamera);
        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            interactionRadius,
            interactionDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        float bestDistance = float.MaxValue;
        targetedButton = null;

        for (int i = 0; i < hits.Length; i++)
        {
            PlanetButton candidate = hits[i].collider.GetComponentInParent<PlanetButton>();
            if (candidate == null)
            {
                continue;
            }

            if (hits[i].distance < bestDistance)
            {
                bestDistance = hits[i].distance;
                targetedButton = candidate.rootButton != null ? candidate.rootButton : candidate;
            }
        }

        return targetedButton != null;
    }
}
