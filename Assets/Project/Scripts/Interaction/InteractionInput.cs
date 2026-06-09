using UnityEngine;

/// <summary>
/// Shared gameplay input helpers for interaction-based actions.
/// Keeps keyboard, mouse, and controller prompts aligned with actual behavior.
/// </summary>
public static class InteractionInput
{
    private static readonly Vector3 ScreenCenter = new Vector3(0.5f, 0.5f, 0f);

    public static bool IsInteractPressedThisFrame()
    {
        return Input.GetKeyDown(KeyCode.E)
            || Input.GetKeyDown(KeyCode.JoystickButton2)
            || Input.GetMouseButtonDown(0);
    }

    public static Ray GetCenteredViewRay(Camera camera)
    {
        return camera.ViewportPointToRay(ScreenCenter);
    }
}
