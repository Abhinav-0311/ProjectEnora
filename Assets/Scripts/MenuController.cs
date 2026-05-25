using UnityEngine;

public class MenuController : MonoBehaviour
{
    public Canvas menuCanvas; // Reference to the canvas you want to disable.

    void Update()
    {
        // Check for the Enter key to disable the canvas and start the game.
        if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            StartGame();
        }

        // Check for the Escape key to quit the game.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            QuitGame();
        }
    }

    public void StartGame()
    {
        // Disable the canvas when the game starts.
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
        // Stops play mode in the Unity editor and exits the game in builds.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
