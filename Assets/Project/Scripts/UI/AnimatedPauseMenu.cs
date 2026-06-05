using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimatedPauseMenu : MonoBehaviour
{
    public CanvasGroup pauseMenuUI;
    public float fadeDuration = 0.28f;

    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode controllerPauseKey = KeyCode.JoystickButton7;
    [SerializeField] private KeyCode mainMenuKey = KeyCode.M;
    [SerializeField] private KeyCode quitKey = KeyCode.Q;

    private bool isPaused;
    private bool isAnimating;

    private void Awake()
    {
        EnsurePauseMenuUi();
        HidePauseMenuImmediately();
    }

    private void Update()
    {
        if (!IsGameplayScene() || GameplayOverlayState.IsGameOver || isAnimating)
        {
            return;
        }

        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(controllerPauseKey))
        {
            if (isPaused)
            {
                StartCoroutine(FadeOutPauseMenu());
            }
            else
            {
                StartCoroutine(FadeInPauseMenu());
            }

            return;
        }

        if (!isPaused)
        {
            return;
        }

        if (Input.GetKeyDown(mainMenuKey))
        {
            LoadMainMenu();
        }
        else if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
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
        GameplayOverlayState.PrepareForSceneTransition();
        SceneTransitionController.LoadScene(SceneNames.MainMenu);
    }

    public void QuitGame()
    {
        GameplayOverlayState.PrepareForSceneTransition();
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

    private void EnsurePauseMenuUi()
    {
        if (pauseMenuUI != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1200;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        pauseMenuUI = gameObject.GetComponent<CanvasGroup>();
        if (pauseMenuUI == null)
        {
            pauseMenuUI = gameObject.AddComponent<CanvasGroup>();
        }

        if (transform.childCount == 0)
        {
            BuildRuntimePauseMenu();
        }
    }

    private void BuildRuntimePauseMenu()
    {
        Font displayFont = RuntimeTypography.GetDisplayFont();
        Font bodyFont = RuntimeTypography.GetBodyFont();

        Image background = CreatePanel(
            transform,
            "PauseBackground",
            new Vector2(1920f, 1080f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Color(0.03f, 0.025f, 0.02f, 0.78f));
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image card = CreatePanel(
            background.transform,
            "PauseCard",
            new Vector2(620f, 320f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f),
            new Color(0.08f, 0.06f, 0.045f, 0.74f));

        CreateText(
            card.transform,
            "PauseTitle",
            displayFont,
            34,
            new Vector2(500f, 54f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -44f),
            new Color(0.98f, 0.93f, 0.82f, 1f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold).text = "PAUSED";

        CreateText(
            card.transform,
            "PauseBody",
            bodyFont,
            18,
            new Vector2(520f, 90f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 18f),
            new Color(0.95f, 0.92f, 0.85f, 0.96f),
            TextAnchor.MiddleCenter,
            FontStyle.Normal).text =
            "The trial waits.\nPress Esc to resume.";

        CreateText(
            card.transform,
            "PauseOptions",
            bodyFont,
            16,
            new Vector2(520f, 96f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 34f),
            new Color(0.88f, 0.8f, 0.65f, 0.92f),
            TextAnchor.MiddleCenter,
            FontStyle.Italic).text =
            "M  Main Menu        Q  Quit";
    }

    private static bool IsGameplayScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == SceneNames.Level1 || sceneName == SceneNames.Level2;
    }

    private static Image CreatePanel(
        Transform parent,
        string name,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Color color)
    {
        GameObject panelGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(parent, false);

        RectTransform rect = panelGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panelGo.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(
        Transform parent,
        string name,
        Font font,
        int fontSize,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Color color,
        TextAnchor alignment,
        FontStyle fontStyle)
    {
        GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textGo.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = string.Empty;

        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.effectDistance = new Vector2(1.4f, -1.4f);

        return text;
    }
}
