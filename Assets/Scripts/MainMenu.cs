using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlay()
    {
        SceneManager.LoadScene(SceneNames.Controls);
        // Music: "Lore" track key when Controls scene loads (MusicManager + MusicLibrary).
    }
    public void OnQuit(){
        print("Quitting Application");
        Application.Quit();
    }
}
