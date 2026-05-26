using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedPauseMenu : MonoBehaviour
{
    public CanvasGroup pauseMenuUI;
    public float fadeDuration = 0.5f;

    private bool isPaused;
    private bool isAnimating;

    private void Awake()
    {
        HidePauseMenuImmediately();
    }

    private void Update()
    {
        if (GameplayOverlayState.IsGameOver || isAnimating)
        {
            return;
        }

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
        if (pauseMenuUI == null || isPaused)
        {
            yield break;
        }

        isAnimating = true;
        GameplayOverlayState.ShowPauseOverlay();

        pauseMenuUI.gameObject.SetActive(true);
        pauseMenuUI.alpha = 0f;
        pauseMenuUI.blocksRaycasts = true;
        pauseMenuUI.interactable = true;

        isPaused = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseMenuUI.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        pauseMenuUI.alpha = 1f;
        isAnimating = false;
    }

    private IEnumerator FadeOutPauseMenu()
    {
        if (pauseMenuUI == null || !isPaused)
        {
            yield break;
        }

        isAnimating = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            pauseMenuUI.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        pauseMenuUI.alpha = 0f;
        pauseMenuUI.blocksRaycasts = false;
        pauseMenuUI.interactable = false;
        pauseMenuUI.gameObject.SetActive(false);

        isPaused = false;
        isAnimating = false;
        GameplayOverlayState.HidePauseOverlay();
    }

    public void ResumeGame()
    {
        if (!isPaused || isAnimating)
        {
            return;
        }

        StartCoroutine(FadeOutPauseMenu());
    }

    public void LoadMainMenu()
    {
        SceneTransitionController.LoadScene(SceneNames.MainMenu);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private void HidePauseMenuImmediately()
    {
        if (pauseMenuUI == null)
        {
            return;
        }

        pauseMenuUI.alpha = 0f;
        pauseMenuUI.blocksRaycasts = false;
        pauseMenuUI.interactable = false;
        pauseMenuUI.gameObject.SetActive(false);
        isPaused = false;
        isAnimating = false;
    }
}
