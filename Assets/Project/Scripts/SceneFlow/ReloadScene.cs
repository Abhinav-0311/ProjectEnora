using UnityEngine;

public class ReloadScene : MonoBehaviour
{
    private void Update()
    {
        bool reloadPressed = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton8);
        if (!reloadPressed)
        {
            return;
        }

        if (GameplayOverlayState.IsOverlayActive)
        {
            return;
        }

        if (SceneTransitionController.Instance != null && SceneTransitionController.Instance.IsTransitioning)
        {
            return;
        }

        SceneTransitionController.ReloadCurrentScene();
    }
}
