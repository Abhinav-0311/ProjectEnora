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
    [SerializeField] private Text interactionPromptText;
    [SerializeField] private Text journalHintText;
    [SerializeField] private Text journalText;
    [SerializeField] private GameObject contextPanel;
    [SerializeField] private GameObject interactionPromptPanel;
    [SerializeField] private GameObject journalPanel;
    [SerializeField] private float defaultDuration = 5f;
    [SerializeField] private float defaultContextDuration = 5.5f;
    [SerializeField] private KeyCode journalToggleKey = KeyCode.J;
    [SerializeField] private KeyCode controllerJournalToggleKey = KeyCode.JoystickButton6;

    private Coroutine subtitleRoutine;
    private Coroutine contextRoutine;
    private Coroutine journalHintRoutine;
    private bool journalVisible;
    private bool gameplayHudVisible = true;
    private bool contextVisible;
    private string interactionPrompt = string.Empty;

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
            || interactionPromptText == null
            || interactionPromptPanel == null
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

        Font displayFont = RuntimeTypography.GetDisplayFont();
        Font bodyFont = RuntimeTypography.GetBodyFont();

        Image objectivePanel = CreatePanel(
            transform,
            "ObjectivePanel",
            new Vector2(390f, 160f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(28f, -28f),
            new Color(0.05f, 0.04f, 0.03f, 0.48f));

        chapterText = CreateText(
            objectivePanel.transform,
            "Chapter",
            bodyFont,
            14,
            new Vector2(330f, 22f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -16f),
            new Color(0.88f, 0.79f, 0.61f, 0.96f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);

        objectiveTitleText = CreateText(
            objectivePanel.transform,
            "ObjectiveTitle",
            displayFont,
            22,
            new Vector2(340f, 54f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -42f),
            new Color(0.98f, 0.94f, 0.84f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);
        objectiveTitleText.resizeTextForBestFit = true;
        objectiveTitleText.resizeTextMinSize = 15;
        objectiveTitleText.resizeTextMaxSize = 22;

        objectiveBodyText = CreateText(
            objectivePanel.transform,
            "ObjectiveBody",
            bodyFont,
            16,
            new Vector2(340f, 74f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -102f),
            new Color(0.94f, 0.91f, 0.84f, 0.96f),
            TextAnchor.UpperLeft,
            FontStyle.Normal);

        Image subtitlePanel = CreatePanel(
            transform,
            "SubtitlePanel",
            new Vector2(840f, 78f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 28f),
            new Color(0.05f, 0.035f, 0.02f, 0.32f));

        subtitleText = CreateText(
            subtitlePanel.transform,
            "StorySubtitle",
            displayFont,
            19,
            new Vector2(760f, 54f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Color(0.97f, 0.94f, 0.86f, 0.98f),
            TextAnchor.MiddleCenter,
            FontStyle.Italic);
        subtitleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        subtitleText.verticalOverflow = VerticalWrapMode.Truncate;
        subtitlePanel.gameObject.SetActive(false);

        contextPanel = CreatePanel(
            transform,
            "ContextPanel",
            new Vector2(390f, 110f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(28f, 112f),
            new Color(0.05f, 0.04f, 0.03f, 0.46f)).gameObject;

        contextTitleText = CreateText(
            contextPanel.transform,
            "ContextTitle",
            displayFont,
            18,
            new Vector2(340f, 26f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(18f, -14f),
            new Color(0.98f, 0.94f, 0.84f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);

        contextBodyText = CreateText(
            contextPanel.transform,
            "ContextBody",
            bodyFont,
            15,
            new Vector2(340f, 62f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(18f, -42f),
            new Color(0.95f, 0.92f, 0.85f, 0.95f),
            TextAnchor.UpperLeft,
            FontStyle.Normal);
        contextPanel.SetActive(false);

        interactionPromptPanel = CreatePanel(
            transform,
            "InteractionPromptPanel",
            new Vector2(420f, 48f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 118f),
            new Color(0.04f, 0.03f, 0.02f, 0.24f)).gameObject;

        interactionPromptText = CreateText(
            interactionPromptPanel.transform,
            "InteractionPromptText",
            bodyFont,
            16,
            new Vector2(360f, 28f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Color(0.95f, 0.92f, 0.85f, 0.95f),
            TextAnchor.MiddleCenter,
            FontStyle.Italic);
        interactionPromptPanel.SetActive(false);

        journalHintText = CreateText(
            transform,
            "JournalHint",
            bodyFont,
            14,
            new Vector2(250f, 24f),
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-24f, 24f),
            new Color(0.89f, 0.8f, 0.63f, 0.9f),
            TextAnchor.LowerRight,
            FontStyle.Italic);
        journalHintText.text = "Press J to open the memory log";

        journalPanel = CreatePanel(
            transform,
            "JournalPanel",
            new Vector2(470f, 560f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f),
            new Vector2(-26f, 0f),
            new Color(0.04f, 0.03f, 0.02f, 0.78f)).gameObject;

        Text journalHeaderText = CreateText(
            journalPanel.transform,
            "JournalHeader",
            displayFont,
            22,
            new Vector2(410f, 30f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -18f),
            new Color(0.98f, 0.94f, 0.84f, 1f),
            TextAnchor.UpperLeft,
            FontStyle.Bold);
        journalHeaderText.text = "MEMORY LOG";

        journalText = CreateText(
            journalPanel.transform,
            "JournalBody",
            bodyFont,
            16,
            new Vector2(410f, 450f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(22f, -62f),
            new Color(0.95f, 0.92f, 0.85f, 0.95f),
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

        if (contextRoutine != null)
        {
            StopCoroutine(contextRoutine);
            contextRoutine = null;
        }

        if (journalHintRoutine != null)
        {
            StopCoroutine(journalHintRoutine);
            journalHintRoutine = null;
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

        ClearInteractionPrompt();
        HideContextCard();
    }

    public void SetGameplayHudVisible(bool isVisible)
    {
        gameplayHudVisible = isVisible;
        ApplyGameplayHudVisibility();
    }

    public void ShowContextCard(string title, string body, float duration = -1f)
    {
        if (duration < 0f)
        {
            duration = defaultContextDuration;
        }

        if (contextRoutine != null)
        {
            StopCoroutine(contextRoutine);
            contextRoutine = null;
        }

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

        if (duration > 0f)
        {
            contextRoutine = StartCoroutine(HideContextCardAfterDelay(duration));
        }
    }

    public void HideContextCard()
    {
        if (contextRoutine != null)
        {
            StopCoroutine(contextRoutine);
            contextRoutine = null;
        }

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

    public void SetInteractionPrompt(string text)
    {
        string resolvedText = string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
        if (interactionPrompt == resolvedText)
        {
            return;
        }

        interactionPrompt = resolvedText;

        if (interactionPromptText != null)
        {
            interactionPromptText.text = interactionPrompt;
        }

        ApplyGameplayHudVisibility();
    }

    public void ClearInteractionPrompt()
    {
        if (string.IsNullOrEmpty(interactionPrompt))
        {
            return;
        }

        interactionPrompt = string.Empty;
        if (interactionPromptText != null)
        {
            interactionPromptText.text = string.Empty;
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

        RefreshObjectiveLayout();
    }

    private void HandleLogEntryAdded(NarrativeLogEntry entry)
    {
        RefreshJournal();

        if (!journalVisible)
        {
            ShowJournalHintTemporarily("New memory recorded. Press J to read.");
        }
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

    private IEnumerator HideContextCardAfterDelay(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        contextRoutine = null;
        HideContextCard();
    }

    private void ShowJournalHintTemporarily(string message, float duration = 2.5f)
    {
        if (journalHintText == null || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (journalHintRoutine != null)
        {
            StopCoroutine(journalHintRoutine);
        }

        journalHintRoutine = StartCoroutine(JournalHintRoutine(message, duration));
    }

    private IEnumerator JournalHintRoutine(string message, float duration)
    {
        if (journalHintText == null)
        {
            yield break;
        }

        journalHintText.text = message;
        yield return new WaitForSecondsRealtime(duration);

        journalHintText.text = journalVisible
            ? "Press J to close the memory log"
            : "Press J to open the memory log";

        journalHintRoutine = null;
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

        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(gameplayHudVisible && !string.IsNullOrWhiteSpace(interactionPrompt));
        }

        if (journalPanel != null)
        {
            journalPanel.SetActive(gameplayHudVisible && journalVisible);
        }
    }

    private void RefreshObjectiveLayout()
    {
        if (objectiveTitleText == null || objectiveBodyText == null)
        {
            return;
        }

        RectTransform panelRect = objectiveTitleText.transform.parent as RectTransform;
        RectTransform titleRect = objectiveTitleText.rectTransform;
        RectTransform bodyRect = objectiveBodyText.rectTransform;
        if (panelRect == null || titleRect == null || bodyRect == null)
        {
            return;
        }

        float titleHeight = Mathf.Clamp(objectiveTitleText.preferredHeight + 4f, 34f, 68f);
        titleRect.sizeDelta = new Vector2(titleRect.sizeDelta.x, titleHeight);

        float bodyHeight = Mathf.Clamp(objectiveBodyText.preferredHeight + 6f, 46f, 94f);
        bodyRect.anchoredPosition = new Vector2(bodyRect.anchoredPosition.x, -52f - titleHeight);
        bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, bodyHeight);

        float panelHeight = Mathf.Clamp(70f + titleHeight + bodyHeight, 140f, 192f);
        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
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

        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
        shadow.effectDistance = new Vector2(1.2f, -1.2f);

        return text;
    }
}
