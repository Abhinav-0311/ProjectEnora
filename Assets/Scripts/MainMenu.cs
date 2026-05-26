using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OnPlay()
    {
        SceneTransitionController.LoadScene(SceneNames.Controls);
        // Music: "Lore" track key when Controls scene loads (MusicManager + MusicLibrary).
    }
    public void OnQuit()
    {
        print("Quitting Application");
        Application.Quit();
    }
}
