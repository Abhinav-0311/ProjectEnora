using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimatedPauseMenu : MonoBehaviour
{
    public CanvasGroup pauseMenuUI;
    public float fadeDuration = 0.5f;

    private bool isPaused;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                StartCoroutine(FadeOutPauseMenu());
            }
            else
            {
                StartCoroutine(FadeInPauseMenu());
            }
        }
    }

    private IEnumerator FadeInPauseMenu()
    {
        if (pauseMenuUI == null)
        {
            yield break;
        }

        pauseMenuUI.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        isPaused = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseMenuUI.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        pauseMenuUI.alpha = 1f;
    }

    private IEnumerator FadeOutPauseMenu()
    {
        if (pauseMenuUI == null)
        {
            yield break;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        isPaused = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseMenuUI.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        pauseMenuUI.alpha = 0f;
        pauseMenuUI.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        StartCoroutine(FadeOutPauseMenu());
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneNames.MainMenu);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
