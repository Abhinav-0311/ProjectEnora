using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyAndFadeGameOver : MonoBehaviour
{
    [Header("Objects to Destroy")]
    public List<GameObject> objectsToDestroy;

    [Header("Game Over UI")]
    public CanvasGroup gameOverCanvas; // Assign the CanvasGroup of your Game Over panel
    public float fadeDuration = 2f;    // Duration for fade-in

    private bool hasTriggered = false;

    private void Awake()
    {
        ConfigureHiddenCanvasState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(HandleGameOver());
        }
    }

    IEnumerator HandleGameOver()
    {
        GameplayOverlayState.ShowGameOverOverlay();

        // Destroy all assigned objects
        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }

        // Start fading in Game Over UI
        if (gameOverCanvas != null)
        {
            float elapsed = 0f;
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.blocksRaycasts = false;
            gameOverCanvas.interactable = false;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                gameOverCanvas.alpha = Mathf.Clamp01(elapsed / fadeDuration);
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
