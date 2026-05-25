using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Boss aftermath: portal choice - escape (load scene) or stay (redemption ending, optional).
/// </summary>
public class EndingPortal : MonoBehaviour
{
    public enum EndingKind
    {
        Escape,
        Redemption
    }

    [SerializeField] private string playerTag = "Player";
    [SerializeField] private EndingKind ending = EndingKind.Escape;
    [SerializeField] private string sceneToLoad = SceneNames.MainMenu;

    [TextArea(2, 4)]
    [SerializeField] private string lineOnEnter = "You are free... But freedom does not mean forgiveness.";

    [SerializeField] private float subtitleDuration = 6f;
    [SerializeField] private float delayBeforeLoad = 4f;
    [SerializeField] private bool loadSceneAfterDelay = true;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag(playerTag))
        {
            return;
        }

        used = true;

        if (NarrativeHUD.Instance != null && !string.IsNullOrWhiteSpace(lineOnEnter))
        {
            NarrativeHUD.Instance.ShowSubtitle(lineOnEnter.Trim(), subtitleDuration);
        }

        NarrativeProgress.SetChapter(StoryChapter.Ending);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(MusicTrackNames.Ending);
        }

        if (ending == EndingKind.Redemption && !loadSceneAfterDelay)
        {
            return;
        }

        if (loadSceneAfterDelay && !string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadAfterDelay());
        }
    }

    private IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}
