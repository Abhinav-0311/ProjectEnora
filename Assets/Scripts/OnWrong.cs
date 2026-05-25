using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnWrong : MonoBehaviour
{
public void ReloadSceneWithDelay(float delay)
    {
        StartCoroutine(LoadSceneAfterDelay(delay));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
