using UnityEngine;
using System.Collections;

public class PortalGameOverTrigger : MonoBehaviour
{
    [Header("Game Over UI")]
    public CanvasGroup gameOverCanvas;  // Assign the CanvasGroup of your Game Over panel
    public float fadeDuration = 2f;     // Duration for fade-in

    private bool hasTriggered = false;

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
        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.blocksRaycasts = true;
            gameOverCanvas.interactable = true;

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                gameOverCanvas.alpha = t;
                yield return null;
            }

            gameOverCanvas.alpha = 1f;
        }
    }
}
