using UnityEngine;

public class Tutorial : MonoBehaviour
{
    // You can change this in the Inspector if needed
    public KeyCode startKey = KeyCode.Return; // 'Return' is the Enter key
    public KeyCode controllerStartKey = KeyCode.JoystickButton2;

    private void Update()
    {
        if (Input.GetKeyDown(startKey) || Input.GetKeyDown(controllerStartKey))
        {
            SceneTransitionController.LoadScene(SceneNames.Level1);
        }
    }
}
