using UnityEngine;

public class ReloadScene : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton8))
        {
            SceneTransitionController.ReloadCurrentScene();
        }
    }
}
