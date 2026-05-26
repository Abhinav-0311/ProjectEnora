using UnityEngine;
using System.Collections;

public class PortalGameOverTrigger : MonoBehaviour
{
    [Header("Game Over UI")]
    public CanvasGroup gameOverCanvas;  // Assign the CanvasGroup of your Game Over panel
    public float fadeDuration = 2f;     // Duration for fade-in

    private bool hasTriggered = false;

    private void Awake()
    {
        ConfigureHiddenCanvasState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(FadeGameOverUI());
        }
    }

    IEnumerator FadeGameOverUI()
    {
        GameplayOverlayState.ShowGameOverOverlay();

        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.blocksRaycasts = false;
            gameOverCanvas.interactable = false;

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                gameOverCanvas.alpha = t;
                yield return null;
            }

            gameOverCanvas.alpha = 1f;
            gameOverCanvas.blocksRaycasts = true;
            gameOverCanvas.interactable = true;
        }
    }

    private void ConfigureHiddenCanvasState()
    {
        if (gameOverCanvas == null)
        {
            return;
        }

        gameOverCanvas.alpha = 0f;
        gameOverCanvas.blocksRaycasts = false;
        gameOverCanvas.interactable = false;
    }
}
