using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class KeypadManager : MonoBehaviour
{
    public event System.Action PuzzleStarted;
    public event System.Action PuzzleSolved;
    public event System.Action PuzzleFailed;

    private sealed class KeypadRiddlePreset
    {
        public KeypadRiddlePreset(string title, string riddle, params int[] sequence)
        {
            Title = title;
            Riddle = riddle;
            Sequence = sequence;
        }

        public string Title { get; }
        public string Riddle { get; }
        public int[] Sequence { get; }
    }

    private static readonly KeypadRiddlePreset[] Presets =
    {
        new KeypadRiddlePreset(
            "Tablet of Echoes",
            "Begin with the number that stands as its own square.\n" +
            "Add two to wake the next seal.\n" +
            "Halve the first answer to calm the third.\n" +
            "End one step above the third.",
            4, 6, 2, 3),
        new KeypadRiddlePreset(
            "Tablet of Days",
            "A full week opens the lock.\n" +
            "The holy trinity answers after it.\n" +
            "Then comes the loneliest number.\n" +
            "A single hand closes the prayer.",
            7, 3, 1, 5),
        new KeypadRiddlePreset(
            "Tablet of Seasons",
            "First, count the legs of a spider.\n" +
            "Then name the turning seasons.\n" +
            "Stand alone for the third seal.\n" +
            "Finish with the simplest pair.",
            8, 4, 1, 2),
        new KeypadRiddlePreset(
            "Tablet of Ash",
            "The cat's last life starts the chain.\n" +
            "A single hand follows.\n" +
            "Then kneels the trinity.\n" +
            "One soul remains at the end.",
            9, 5, 3, 1),
        new KeypadRiddlePreset(
            "Tablet of Bone",
            "Count the legs of the beetle first.\n" +
            "Then trace the shape that folds into itself.\n" +
            "The smallest even number on this keypad answers next.\n" +
            "A chair's legs close the lock.",
            6, 8, 2, 4),
        new KeypadRiddlePreset(
            "Tablet of Lanterns",
            "Count the beetle's legs first.\n" +
            "Then let one soul stand alone.\n" +
            "The turning seasons answer next.\n" +
            "Finish where the spider keeps its count.",
            6, 1, 4, 8)
    };

    private static int s_lastPresetIndex = -1;
    private static Font s_builtinFont;

    [Header("Puzzle Flow")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject SecondPuzzle;
    [SerializeField] private bool randomizeSequenceEachPlaythrough = true;

    [Header("Runtime Clue Board")]
    [SerializeField] private string clueBoardObjectName = "Room1ClueBoard";
    [SerializeField] private Color boardTint = new Color(0.13f, 0.1f, 0.07f, 1f);
    [SerializeField] private Color boardAccent = new Color(0.84f, 0.74f, 0.56f, 1f);

    [Header("Runtime Display Offsets")]
    [SerializeField] private Vector3 cluePanelOffset = new Vector3(0f, 0f, 0.018f);
    [SerializeField] private float clueVisibleDistance = 2.6f;
    [SerializeField] private float hudVisibleDistance = 4.5f;
    [SerializeField] private float facingDotThreshold = 0.45f;

    private readonly List<int> playerInput = new List<int>();

    private KeypadRiddlePreset activePreset;
    private Transform keypadRoot;
    private Transform clueBoard;
    private Renderer clueBoardRenderer;
    private readonly List<TextMesh> clueTitleMeshes = new List<TextMesh>();
    private readonly List<TextMesh> clueBodyMeshes = new List<TextMesh>();
    private Text codeText;
    private Text statusText;
    private GameObject clueRoot;
    private GameObject hudRoot;
    private bool hasStarted;
    private bool isSolved;

    private void Awake()
    {
        keypadRoot = transform.parent != null ? transform.parent : transform;
        clueBoard = keypadRoot != null ? keypadRoot.Find(clueBoardObjectName) : null;
        clueBoardRenderer = clueBoard != null ? clueBoard.GetComponent<Renderer>() : null;

        if (SecondPuzzle != null)
        {
            SecondPuzzle.SetActive(false);
        }

        SelectPreset();
        BuildRuntimeDisplays();
        UpdateClueBoard();
        RefreshCodeDisplay();
        SetStatus("Read the tablet. Enter the pattern one digit at a time.");
        RefreshRuntimeVisibility();
    }

    private void OnEnable()
    {
        KeypadButton.OnButtonPressed += HandleButtonPress;
    }

    private void OnDisable()
    {
        KeypadButton.OnButtonPressed -= HandleButtonPress;
    }

    private void Update()
    {
        RefreshRuntimeVisibility();
    }

    private void HandleButtonPress(int value)
    {
        if (isSolved || activePreset == null)
        {
            return;
        }

        if (!hasStarted)
        {
            hasStarted = true;
            PuzzleStarted?.Invoke();
        }

        if (playerInput.Count >= activePreset.Sequence.Length)
        {
            return;
        }

        playerInput.Add(value);
        RefreshCodeDisplay();

        if (playerInput.Count < activePreset.Sequence.Length)
        {
            SetStatus($"Seal {playerInput.Count} of {activePreset.Sequence.Length} recorded.");
            return;
        }

        if (IsSequenceCorrect())
        {
            CompletePuzzle();
        }
        else
        {
            playerInput.Clear();
            RefreshCodeDisplay();
            SetStatus("All four seals rang false. The memory wipes clean.");
            PuzzleFailed?.Invoke();
        }
    }

    private void CompletePuzzle()
    {
        isSolved = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            Debug.LogError("KeypadManager: Door Animator is not assigned.");
        }

        if (SecondPuzzle != null)
        {
            SecondPuzzle.SetActive(true);
        }

        RefreshCodeDisplay(showSolvedState: true);
        SetStatus("The first lock yields. The next chamber awakens.");
        RefreshRuntimeVisibility();
        PuzzleSolved?.Invoke();
    }

    private void SelectPreset()
    {
        if (Presets.Length == 0)
        {
            Debug.LogError("KeypadManager: No keypad presets are configured.");
            return;
        }

        int presetIndex = 0;
        if (randomizeSequenceEachPlaythrough && Presets.Length > 1)
        {
            do
            {
                presetIndex = Random.Range(0, Presets.Length);
            }
            while (presetIndex == s_lastPresetIndex);
        }

        activePreset = Presets[presetIndex];
        s_lastPresetIndex = presetIndex;
    }

    private void BuildRuntimeDisplays()
    {
        BuildClueBoardText();
        BuildKeypadHud();
        RestyleClueBoard();
    }

    private void BuildClueBoardText()
    {
        if (clueBoard == null)
        {
            Debug.LogWarning("KeypadManager: Could not find the clue board for Room 1.");
            return;
        }

        clueTitleMeshes.Clear();
        clueBodyMeshes.Clear();

        CreateClueSide("Front");
    }

    private void BuildKeypadHud()
    {
        GameObject canvasGo = new GameObject("Room1KeypadHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(keypadRoot, false);
        hudRoot = canvasGo;

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 650;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Image panel = CreatePanelImage(
            canvasGo.transform,
            "Panel",
            new Vector2(520f, 150f),
            new Color(0.02f, 0.02f, 0.02f, 0.86f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(32f, 28f));

        codeText = CreateText(
            panel.transform,
            "Code",
            new Vector2(460f, 56f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -20f),
            30,
            boardAccent,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        statusText = CreateText(
            panel.transform,
            "Status",
            new Vector2(460f, 70f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(24f, 18f),
            20,
            new Color(0.92f, 0.92f, 0.92f, 1f),
            TextAnchor.LowerLeft,
            FontStyle.Normal);
    }

    private void RefreshRuntimeVisibility()
    {
        bool showClue = ShouldShowClueBoard();
        if (clueRoot != null && clueRoot.activeSelf != showClue)
        {
            clueRoot.SetActive(showClue);
        }

        bool showHud = ShouldShowHud();
        if (hudRoot != null && hudRoot.activeSelf != showHud)
        {
            hudRoot.SetActive(showHud);
        }
    }

    private void UpdateClueBoard()
    {
        if (activePreset == null)
        {
            return;
        }

        for (int i = 0; i < clueTitleMeshes.Count; i++)
        {
            clueTitleMeshes[i].text = "THE FORGOTTEN MIND";
        }

        string clueText =
            activePreset.Title.ToUpperInvariant() +
            "\n\n" +
            activePreset.Riddle +
            "\n\nPRESS E AT THE KEYPAD.";

        for (int i = 0; i < clueBodyMeshes.Count; i++)
        {
            clueBodyMeshes[i].text = clueText;
        }
    }

    private void RefreshCodeDisplay(bool showSolvedState = false)
    {
        if (codeText == null || activePreset == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("LOCK MEMORY: ");

        for (int i = 0; i < activePreset.Sequence.Length; i++)
        {
            if (i < playerInput.Count)
            {
                builder.Append(playerInput[i]);
            }
            else if (showSolvedState)
            {
                builder.Append(activePreset.Sequence[i]);
            }
            else
            {
                builder.Append("_");
            }

            if (i < activePreset.Sequence.Length - 1)
            {
                builder.Append("  ");
            }
        }

        codeText.text = builder.ToString();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private bool IsSequenceCorrect()
    {
        for (int i = 0; i < activePreset.Sequence.Length; i++)
        {
            if (playerInput[i] != activePreset.Sequence[i])
            {
                return false;
            }
        }

        return true;
    }

    private void RestyleClueBoard()
    {
        if (clueBoard == null)
        {
            return;
        }

        Renderer renderer = clueBoard.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        // Use a runtime material instance so the clue board looks intentional without overwriting the asset.
        Material materialInstance = renderer.material;
        if (materialInstance.HasProperty("_BaseMap"))
        {
            materialInstance.SetTexture("_BaseMap", null);
        }

        if (materialInstance.HasProperty("_MainTex"))
        {
            materialInstance.SetTexture("_MainTex", null);
        }

        if (materialInstance.HasProperty("_BaseColor"))
        {
            materialInstance.SetColor("_BaseColor", boardTint);
        }

        if (materialInstance.HasProperty("_Color"))
        {
            materialInstance.SetColor("_Color", boardTint);
        }
    }

    private void CreateClueSide(string sideName)
    {
        Font font = GetBuiltinFont();
        Vector3 localOffset = new Vector3(cluePanelOffset.x, cluePanelOffset.y, cluePanelOffset.z);
        Vector3 sideOffset = clueBoard.TransformDirection(localOffset);
        Quaternion sideRotation = Quaternion.LookRotation(-clueBoard.forward, clueBoard.up);

        GameObject root = new GameObject("Room1Clue" + sideName);
        root.transform.SetParent(keypadRoot, true);
        root.transform.position = clueBoard.position + sideOffset;
        root.transform.rotation = sideRotation;
        root.transform.localScale = Vector3.one;
        clueRoot = root;

        TextMesh title = CreateWorldTextMesh(
            root.transform,
            "Title",
            font,
            42,
            0.014f,
            boardAccent,
            new Vector3(0f, 0.2f, 0f),
            TextAnchor.MiddleCenter,
            TextAlignment.Center);
        clueTitleMeshes.Add(title);

        TextMesh body = CreateWorldTextMesh(
            root.transform,
            "Body",
            font,
            26,
            0.0105f,
            new Color(0.96f, 0.93f, 0.86f, 1f),
            new Vector3(0f, 0.045f, 0f),
            TextAnchor.UpperCenter,
            TextAlignment.Center);
        clueBodyMeshes.Add(body);
    }

    private bool ShouldShowClueBoard()
    {
        if (isSolved || clueBoard == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 targetPosition = GetBoardTargetPosition();
        Vector3 toBoard = targetPosition - mainCamera.transform.position;
        float distance = toBoard.magnitude;
        if (distance > clueVisibleDistance || distance <= Mathf.Epsilon)
        {
            return false;
        }

        float facingDot = Vector3.Dot(mainCamera.transform.forward, toBoard.normalized);
        return facingDot >= facingDotThreshold;
    }

    private bool ShouldShowHud()
    {
        if (isSolved || clueBoard == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 anchorPosition = keypadRoot != null ? keypadRoot.position : clueBoard.position;
        return Vector3.Distance(mainCamera.transform.position, anchorPosition) <= hudVisibleDistance;
    }

    private Vector3 GetBoardTargetPosition()
    {
        if (clueBoardRenderer != null)
        {
            return clueBoardRenderer.bounds.center;
        }

        return clueBoard.position;
    }

    private static TextMesh CreateWorldTextMesh(
        Transform parent,
        string name,
        Font font,
        int fontSize,
        float characterSize,
        Color color,
        Vector3 localPosition,
        TextAnchor anchor,
        TextAlignment alignment)
    {
        GameObject textGo = new GameObject(name, typeof(TextMesh));
        textGo.transform.SetParent(parent, false);
        textGo.transform.localPosition = localPosition;
        textGo.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textGo.GetComponent<TextMesh>();
        textMesh.font = font;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.anchor = anchor;
        textMesh.alignment = alignment;
        textMesh.color = color;
        textMesh.lineSpacing = 0.9f;
        textMesh.text = string.Empty;

        MeshRenderer renderer = textGo.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = font.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return textMesh;
    }

    private static Image CreatePanelImage(
        Transform parent,
        string name,
        Vector2 size,
        Color color,
        Vector2? anchorMin = null,
        Vector2? anchorMax = null,
        Vector2? anchoredPosition = null)
    {
        GameObject imageGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(parent, false);

        RectTransform rect = imageGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin ?? new Vector2(0.5f, 0.5f);
        rect.anchorMax = anchorMax ?? new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2((rect.anchorMin.x + rect.anchorMax.x) * 0.5f, (rect.anchorMin.y + rect.anchorMax.y) * 0.5f);
        rect.anchoredPosition = anchoredPosition ?? Vector2.zero;
        rect.sizeDelta = size;

        Image image = imageGo.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(
        Transform parent,
        string name,
        Vector2 size,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        int fontSize,
        Color color,
        TextAnchor alignment,
        FontStyle fontStyle)
    {
        GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2((rect.anchorMin.x + rect.anchorMax.x) * 0.5f, (rect.anchorMin.y + rect.anchorMax.y) * 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textGo.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = string.Empty;
        return text;
    }

    private static Font GetBuiltinFont()
    {
        if (s_builtinFont != null)
        {
            return s_builtinFont;
        }

        s_builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (s_builtinFont == null)
        {
            s_builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (s_builtinFont == null)
        {
            s_builtinFont = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }

        return s_builtinFont;
    }
}
