using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subtitle / story line overlay. Auto-creates a minimal canvas if none assigned.
/// </summary>
public class NarrativeHUD : MonoBehaviour
{
    public static NarrativeHUD Instance { get; private set; }

    [SerializeField] private Text subtitleText;
    [SerializeField] private float defaultDuration = 5f;

    private Coroutine _subtitleRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null) return;
        var go = new GameObject("NarrativeHUD");
        go.AddComponent<NarrativeHUD>();
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

        if (subtitleText == null)
            BuildDefaultSubtitleUI();
    }

    private void BuildDefaultSubtitleUI()
    {
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        var textGo = new GameObject("StorySubtitle");
        textGo.transform.SetParent(transform, false);
        var rect = textGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.06f);
        rect.anchorMax = new Vector2(0.92f, 0.22f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        subtitleText = textGo.AddComponent<Text>();
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                           ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (subtitleText.font == null)
            subtitleText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        subtitleText.fontSize = 22;
        subtitleText.color = new Color(0.92f, 0.9f, 0.85f, 1f);
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitleText.verticalOverflow = VerticalWrapMode.Truncate;
        subtitleText.text = string.Empty;
        textGo.SetActive(false);
    }

    /// <summary>Show a line at the bottom of the screen. Uses unscaled time (works when paused).</summary>
    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        if (subtitleText == null)
        {
            Debug.Log("[Narrative] " + text);
            return;
        }

        if (_subtitleRoutine != null)
            StopCoroutine(_subtitleRoutine);
        _subtitleRoutine = StartCoroutine(SubtitleRoutine(text, duration));
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(duration);
        subtitleText.text = string.Empty;
        subtitleText.gameObject.SetActive(false);
        _subtitleRoutine = null;
    }
}
