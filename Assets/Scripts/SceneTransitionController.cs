using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Simple persistent full-screen fade used for smoother scene-to-scene transitions.
/// </summary>
public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private Text transitionTitleText;
    [SerializeField] private Text transitionSubtitleText;
    [SerializeField] private float defaultFadeOutDuration = 0.55f;
    [SerializeField] private float defaultFadeInDuration = 0.6f;
    [SerializeField] private float postLoadHoldDuration = 0.12f;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("SceneTransitionController");
        go.AddComponent<SceneTransitionController>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage == null)
        {
            BuildOverlay();
        }

        SetFadeAlpha(0f);
        SetOverlayRaycast(false);
    }

    public static void LoadScene(string sceneName)
    {
        ResolveSceneTransitionPresentation(
            sceneName,
            out string title,
            out string subtitle,
            out float fadeOutDuration,
            out float holdDuration,
            out float fadeInDuration);

        LoadScene(sceneName, title, subtitle, fadeOutDuration, holdDuration, fadeInDuration);
    }

    public static void LoadScene(
        string sceneName,
        string title,
        string subtitle,
        float fadeOutDuration = -1f,
        float holdDuration = -1f,
        float fadeInDuration = -1f)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.StartSceneTransition(sceneName, title, subtitle, fadeOutDuration, holdDuration, fadeInDuration);
    }

    public static void LoadScene(int buildIndex)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(buildIndex);
            return;
        }

        Instance.StartSceneTransition(buildIndex);
    }

    public static void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid())
        {
            return;
        }

        if (Instance == null)
        {
            SceneManager.LoadScene(activeScene.name);
            return;
        }

        Instance.StartSceneTransition(activeScene.name, string.Empty, string.Empty, 0.4f, 0.08f, 0.45f);
    }

    private void StartSceneTransition(
        string sceneName,
        string title,
        string subtitle,
        float fadeOutDuration,
        float holdDuration,
        float fadeInDuration)
    {
        if (isTransitioning || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        StartCoroutine(TransitionRoutine(
            () => SceneManager.LoadSceneAsync(sceneName),
            title,
            subtitle,
            fadeOutDuration,
            holdDuration,
            fadeInDuration));
    }

    private void StartSceneTransition(int buildIndex)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionRoutine(
            () => SceneManager.LoadSceneAsync(buildIndex),
            string.Empty,
            string.Empty,
            defaultFadeOutDuration,
            postLoadHoldDuration,
            defaultFadeInDuration));
    }

    private IEnumerator TransitionRoutine(
        System.Func<AsyncOperation> beginLoad,
        string title,
        string subtitle,
        float fadeOutDuration,
        float holdDuration,
        float fadeInDuration)
    {
        isTransitioning = true;
        SetOverlayRaycast(true);
        HideTransitionCopy();

        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ClearTransientUi();
        }

        yield return Fade(0f, 1f, ResolveDuration(fadeOutDuration, defaultFadeOutDuration));

        GameplayOverlayState.PrepareForSceneTransition();

        AsyncOperation operation = beginLoad();
        if (operation != null)
        {
            while (!operation.isDone)
            {
                yield return null;
            }
        }

        ShowTransitionCopy(title, subtitle);

        float resolvedHoldDuration = ResolveDuration(holdDuration, postLoadHoldDuration);
        if (resolvedHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(resolvedHoldDuration);
        }

        yield return Fade(1f, 0f, ResolveDuration(fadeInDuration, defaultFadeInDuration));

        HideTransitionCopy();
        SetOverlayRaycast(false);
        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null || duration <= 0f)
        {
            SetFadeAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetFadeAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();
        }

        GameObject imageGo = new GameObject("Fade", typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(transform, false);

        RectTransform rect = imageGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeImage = imageGo.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }

        transitionTitleText = CreateText(
            transform,
            "TransitionTitle",
            font,
            42,
            FontStyle.Bold,
            new Vector2(0.5f, 0.54f),
            new Vector2(0.5f, 0.54f),
            new Vector2(0f, 0f),
            new Vector2(1000f, 80f),
            new Color(0.94f, 0.9f, 0.82f, 1f),
            TextAnchor.MiddleCenter);

        transitionSubtitleText = CreateText(
            transform,
            "TransitionSubtitle",
            font,
            22,
            FontStyle.Normal,
            new Vector2(0.5f, 0.48f),
            new Vector2(0.5f, 0.48f),
            new Vector2(0f, 0f),
            new Vector2(1080f, 64f),
            new Color(0.74f, 0.69f, 0.62f, 1f),
            TextAnchor.MiddleCenter);

        HideTransitionCopy();
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }

    private void SetOverlayRaycast(bool enabled)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = enabled;
        }
    }

    private static void ResolveSceneTransitionPresentation(
        string sceneName,
        out string title,
        out string subtitle,
        out float fadeOutDuration,
        out float holdDuration,
        out float fadeInDuration)
    {
        title = string.Empty;
        subtitle = string.Empty;
        fadeOutDuration = -1f;
        holdDuration = -1f;
        fadeInDuration = -1f;

        switch (sceneName)
        {
            case SceneNames.Controls:
                title = "AWAKENING";
                subtitle = "The realm remembers before you do.";
                fadeOutDuration = 0.62f;
                holdDuration = 0.35f;
                fadeInDuration = 0.7f;
                break;
            case SceneNames.Level1:
                title = "DUNGEON";
                subtitle = "Trial I - The Forgotten Mind";
                fadeOutDuration = 0.68f;
                holdDuration = 0.45f;
                fadeInDuration = 0.82f;
                break;
            case SceneNames.Level2:
                title = "CASTLE";
                subtitle = "Trial V - The Locked Past";
                fadeOutDuration = 0.75f;
                holdDuration = 0.6f;
                fadeInDuration = 0.9f;
                break;
        }
    }

    private static float ResolveDuration(float requestedDuration, float fallbackDuration)
    {
        return requestedDuration >= 0f ? requestedDuration : fallbackDuration;
    }

    private void ShowTransitionCopy(string title, string subtitle)
    {
        if (transitionTitleText != null)
        {
            transitionTitleText.text = title ?? string.Empty;
            transitionTitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(transitionTitleText.text));
        }

        if (transitionSubtitleText != null)
        {
            transitionSubtitleText.text = subtitle ?? string.Empty;
            transitionSubtitleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(transitionSubtitleText.text));
        }
    }

    private void HideTransitionCopy()
    {
        if (transitionTitleText != null)
        {
            transitionTitleText.text = string.Empty;
            transitionTitleText.gameObject.SetActive(false);
        }

        if (transitionSubtitleText != null)
        {
            transitionSubtitleText.text = string.Empty;
            transitionSubtitleText.gameObject.SetActive(false);
        }
    }

    private static Text CreateText(
        Transform parent,
        string name,
        Font font,
        int fontSize,
        FontStyle fontStyle,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color,
        TextAnchor alignment)
    {
        GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(parent, false);

        RectTransform rectTransform = textGo.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Text text = textGo.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.text = string.Empty;
        return text;
    }
}
