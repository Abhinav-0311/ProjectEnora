using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subtitle, objective, and memory-log overlay. Auto-creates a runtime canvas if none exists.
/// </summary>
public class NarrativeHUD : MonoBehaviour
{
    public static NarrativeHUD Instance { get; private set; }

    [SerializeField] private Text subtitleText;
    [SerializeField] private Text chapterText;
    [SerializeField] private Text objectiveTitleText;
    [SerializeField] private Text objectiveBodyText;
    [SerializeField] private Text contextTitleText;
    [SerializeField] private Text contextBodyText;
    [SerializeField] private Text journalHintText;
    [SerializeField] private Text journalText;
    [SerializeField] private GameObject contextPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private float defaultDuration = 5f;
    [SerializeField] private KeyCode journalToggleKey = KeyCode.J;
    [SerializeField] private KeyCode controllerJournalToggleKey = KeyCode.JoystickButton6;

    private Coroutine subtitleRoutine;
    private bool journalVisible;
    private bool gameplayHudVisible = true;
    private bool contextVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject go = new GameObject("NarrativeHUD");
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

        if (subtitleText == null
            || chapterText == null
            || objectiveTitleText == null
            || objectiveBodyText == null
            || contextTitleText == null
            || contextBodyText == null
            || contextPanel == null
            || journalText == null
            || journalPanel == null)
        {
            BuildDefaultUi();
        }

        SyncWithNarrativeState();
        SetJournalVisible(false);
        ApplyGameplayHudVisibility();
    }

    private void OnEnable()
    {
        NarrativeProgress.ChapterChanged += HandleChapterChanged;
        NarrativeProgress.ObjectiveChanged += HandleObjectiveChanged;
        NarrativeProgress.LogEntryAdded += HandleLogEntryAdded;
    }

    private void OnDisable()
    {
        NarrativeProgress.ChapterChanged -= HandleChapterChanged;
        NarrativeProgress.ObjectiveChanged -= HandleObjectiveChanged;
        NarrativeProgress.LogEntryAdded -= HandleLogEntryAdded;
    }

    private void Update()
    {
        if (!gameplayHudVisible)
        {
            return;
        }

        if (Input.GetKeyDown(journalToggleKey) || Input.GetKeyDown(controllerJournalToggleKey))
        {
            SetJournalVisible(!journalVisible);
        }
    }

    private void BuildDefaultUi()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            gameObject.AddComponent<GraphicRaycaster>();
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }

        Image objectivePanel = CreatePanel(
            transform,
            "ObjectivePanel",
            new Vector2(450f, 170f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(28f, -28f),
            new Color(0.03f, 0.03f, 0.03f, 0.84f));

        chapterText = CreateText(
            objectivePanel.transform,
            "Chapter",
            font,
            18,
            new Vector2(390f, 26f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -18f),
            new Color(0.76f, 0.67f, 0.5f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);

        objectiveTitleText = CreateText(
            objectivePanel.transform,
            "ObjectiveTitle",
            font,
            28,
            new Vector2(390f, 40f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -52f),
            new Color(0.94f, 0.9f, 0.82f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);

        objectiveBodyText = CreateText(
            objectivePanel.transform,
            "ObjectiveBody",
            font,
            20,
            new Vector2(390f, 78f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -94f),
            new Color(0.88f, 0.88f, 0.88f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Normal);

        Image subtitlePanel = CreatePanel(
            transform,
            "SubtitlePanel",
            new Vector2(1180f, 120f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 40f),
            new Color(0.01f, 0.01f, 0.01f, 0.72f));

        subtitleText = CreateText(
            subtitlePanel.transform,
            "StorySubtitle",
            font,
            24,
            new Vector2(1080f, 86f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Color(0.92f, 0.9f, 0.85f, 1f),
            TextAnchor.MiddleCenter,
            FontStyle.Normal);
        subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitleText.verticalOverflow = VerticalWrapMode.Truncate;
        subtitlePanel.gameObject.SetActive(false);

        contextPanel = CreatePanel(
            transform,
            "ContextPanel",
            new Vector2(520f, 150f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(28f, 120f),
            new Color(0.03f, 0.03f, 0.03f, 0.84f)).gameObject;

        contextTitleText = CreateText(
            contextPanel.transform,
            "ContextTitle",
            font,
            22,
            new Vector2(460f, 30f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -18f),
            new Color(0.94f, 0.9f, 0.82f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);

        contextBodyText = CreateText(
            contextPanel.transform,
            "ContextBody",
            font,
            18,
            new Vector2(460f, 84f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -52f),
            new Color(0.88f, 0.88f, 0.88f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Normal);
        contextPanel.SetActive(false);

        journalHintText = CreateText(
            transform,
            "JournalHint",
            font,
            16,
            new Vector2(300f, 28f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-24f, 24f),
            new Color(0.76f, 0.67f, 0.5f, 0.95f),
            TextAnchor.LowerRight,
            FontStyle.Italic);
        journalHintText.text = "Press J to open the memory log";

        journalPanel = CreatePanel(
            transform,
            "JournalPanel",
            new Vector2(560f, 640f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-26f, 0f),
            new Color(0.025f, 0.02f, 0.015f, 0.94f)).gameObject;

        Text journalHeaderText = CreateText(
            journalPanel.transform,
            "JournalHeader",
            font,
            26,
            new Vector2(500f, 32f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(26f, -22f),
            new Color(0.94f, 0.9f, 0.82f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);
        journalHeaderText.text = "MEMORY LOG";

        journalText = CreateText(
            journalPanel.transform,
            "JournalBody",
            font,
            18,
            new Vector2(500f, 520f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(26f, -72f),
            new Color(0.88f, 0.88f, 0.88f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Normal);
        journalText.horizontalOverflow = HorizontalWrapMode.Wrap;
        journalText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    /// <summary>Show a line at the bottom of the screen. Uses unscaled time (works when paused).</summary>
    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (duration < 0f)
        {
            duration = defaultDuration;
        }

        if (subtitleText == null)
        {
            Debug.Log("[Narrative] " + text);
            return;
        }

        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
        }

        subtitleRoutine = StartCoroutine(SubtitleRoutine(text, duration));
    }

    /// <summary>Clears any active subtitle and closes transient narrative panels for scene transitions.</summary>
    public void ClearTransientUi()
    {
        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }

        if (subtitleText != null)
        {
            subtitleText.text = string.Empty;

            if (subtitleText.transform.parent != null)
            {
                subtitleText.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                subtitleText.gameObject.SetActive(false);
            }
        }

        if (journalVisible)
        {
            SetJournalVisible(false);
        }

        HideContextCard();
    }

    public void SetGameplayHudVisible(bool isVisible)
    {
        gameplayHudVisible = isVisible;
        ApplyGameplayHudVisibility();
    }

    public void ShowContextCard(string title, string body)
    {
        contextVisible = true;

        if (contextTitleText != null)
        {
            contextTitleText.text = string.IsNullOrWhiteSpace(title) ? "CASTLE TRIAL" : title.ToUpperInvariant();
        }

        if (contextBodyText != null)
        {
            contextBodyText.text = body ?? string.Empty;
        }

        ApplyGameplayHudVisibility();
    }

    public void HideContextCard()
    {
        contextVisible = false;

        if (contextTitleText != null)
        {
            contextTitleText.text = string.Empty;
        }

        if (contextBodyText != null)
        {
            contextBodyText.text = string.Empty;
        }

        ApplyGameplayHudVisibility();
    }

    private void SyncWithNarrativeState()
    {
        HandleChapterChanged(NarrativeProgress.CurrentChapter);
        HandleObjectiveChanged(NarrativeProgress.CurrentObjectiveTitle, NarrativeProgress.CurrentObjectiveBody);
        RefreshJournal();
    }

    private void HandleChapterChanged(StoryChapter chapter)
    {
        if (chapterText != null)
        {
            chapterText.text = chapter == StoryChapter.None
                ? "UNWRITTEN MEMORY"
                : chapter.ToString().ToUpperInvariant();
        }
    }

    private void HandleObjectiveChanged(string title, string body)
    {
        if (objectiveTitleText != null)
        {
            objectiveTitleText.text = string.IsNullOrWhiteSpace(title) ? "CURRENT TRIAL" : title.ToUpperInvariant();
        }

        if (objectiveBodyText != null)
        {
            objectiveBodyText.text = string.IsNullOrWhiteSpace(body)
                ? "Keep moving. The realm is still judging you."
                : body;
        }
    }

    private void HandleLogEntryAdded(NarrativeLogEntry entry)
    {
        RefreshJournal();
    }

    private void RefreshJournal()
    {
        if (journalText == null)
        {
            return;
        }

        IReadOnlyList<NarrativeLogEntry> entries = NarrativeProgress.LogEntries;
        if (entries.Count == 0)
        {
            journalText.text = "No memories recorded yet.";
            return;
        }

        StringBuilder builder = new StringBuilder();
        int startIndex = Mathf.Max(0, entries.Count - 14);
        for (int i = entries.Count - 1; i >= startIndex; i--)
        {
            NarrativeLogEntry entry = entries[i];
            builder.Append(entry.Title.ToUpperInvariant());
            builder.Append('\n');
            builder.Append(entry.Body);

            if (i > startIndex)
            {
                builder.Append("\n\n");
            }
        }

        journalText.text = builder.ToString();
    }

    private void SetJournalVisible(bool isVisible)
    {
        journalVisible = isVisible;
        if (journalPanel != null)
        {
            journalPanel.SetActive(gameplayHudVisible && journalVisible);
        }

        if (journalHintText != null)
        {
            journalHintText.text = journalVisible
                ? "Press J to close the memory log"
                : "Press J to open the memory log";
        }
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        if (subtitleText == null)
        {
            yield break;
        }

        subtitleText.text = text;
        ApplyGameplayHudVisibility();

        yield return new WaitForSecondsRealtime(duration);

        subtitleText.text = string.Empty;
        ApplyGameplayHudVisibility();

        subtitleRoutine = null;
    }

    private void ApplyGameplayHudVisibility()
    {
        if (objectiveTitleText != null && objectiveTitleText.transform.parent != null)
        {
            objectiveTitleText.transform.parent.gameObject.SetActive(gameplayHudVisible);
        }

        if (subtitleText != null)
        {
            bool showSubtitle = gameplayHudVisible && !string.IsNullOrWhiteSpace(subtitleText.text);

            if (subtitleText.transform.parent != null)
            {
                subtitleText.transform.parent.gameObject.SetActive(showSubtitle);
            }
            else
            {
                subtitleText.gameObject.SetActive(showSubtitle);
            }
        }

        if (journalHintText != null)
        {
            journalHintText.gameObject.SetActive(gameplayHudVisible);
        }

        if (contextPanel != null)
        {
            contextPanel.SetActive(gameplayHudVisible && contextVisible);
        }

        if (journalPanel != null)
        {
            journalPanel.SetActive(gameplayHudVisible && journalVisible);
        }
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
        FontStyle style)
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
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = string.Empty;
        return text;
    }
}
