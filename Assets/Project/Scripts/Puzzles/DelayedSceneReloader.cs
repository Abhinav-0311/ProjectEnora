using System.Collections;
using UnityEngine;

public class DelayedSceneReloader : MonoBehaviour
{
public void ReloadSceneWithDelay(float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(delay));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneTransitionController.ReloadCurrentScene();
    }
}
