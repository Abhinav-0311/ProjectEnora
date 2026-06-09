using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManager : MonoBehaviour
{
    private sealed class PlanetTrialPreset
    {
        public PlanetTrialPreset(string title, string clue, params string[] sequence)
        {
            Title = title;
            Clue = clue;
            Sequence = sequence;
        }

        public string Title { get; }
        public string Clue { get; }
        public string[] Sequence { get; }
    }

    private static readonly PlanetTrialPreset[] Presets =
    {
        new PlanetTrialPreset(
            "Atlas of Exile",
            "Begin where life learned to breathe.\n" +
            "Then reach the outcast world that circles in silence.\n" +
            "End at the ringed judge watching from afar.",
            "Earth", "Pluto", "Saturn"),
        new PlanetTrialPreset(
            "Atlas of Dawn",
            "Wake the swiftest planet first.\n" +
            "Then call the evening star.\n" +
            "Let the world of oceans answer last.",
            "Mercury", "Venus", "Earth"),
        new PlanetTrialPreset(
            "Atlas of Giants",
            "Start with the red omen.\n" +
            "Then the king of storms must answer.\n" +
            "Finish beneath the rings of old judgment.",
            "Mars", "Jupiter", "Saturn"),
        new PlanetTrialPreset(
            "Atlas of Silence",
            "Begin with the forgotten wanderer.\n" +
            "Then wake the blue world furthest from the sun.\n" +
            "End with the tilted exile that rolls through the dark.",
            "Pluto", "Neptune", "Uranus"),
        new PlanetTrialPreset(
            "Atlas of Tempests",
            "First the red war-sign answers.\n" +
            "Then the tilted storm-bearer must turn.\n" +
            "Finish at the ocean world beyond it.",
            "Mars", "Uranus", "Neptune"),
        new PlanetTrialPreset(
            "Atlas of Ashes",
            "Begin with the smallest messenger.\n" +
            "Then seek the world that still remembers water.\n" +
            "End with the planet crowned by storms.",
            "Mercury", "Earth", "Jupiter")
    };

    private static int lastPresetIndex = -1;
    private static Font builtinFont;

    public event System.Action PuzzleStarted;
    public event System.Action PuzzleSolved;
    public event System.Action PuzzleFailed;

    [Header("Puzzle Flow")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject ThirdPuzzle;
    [SerializeField] private bool randomizeSequenceEachPlaythrough = true;

    [Header("Board Styling")]
    [SerializeField] private string clueBoardObjectName = "Room2ClueBoard";
    [SerializeField] private Color boardTint = new Color(0.79f, 0.7f, 0.53f, 1f);
    [SerializeField] private Color accentColor = new Color(0.27f, 0.16f, 0.07f, 1f);
    [SerializeField] private Color titleTextColor = new Color(0.13f, 0.07f, 0.03f, 1f);
    [SerializeField] private Color bodyTextColor = new Color(0.11f, 0.06f, 0.03f, 1f);
    [SerializeField] private Color hudTextColor = new Color(0.96f, 0.93f, 0.86f, 0.98f);
    [SerializeField] private Vector3 boardOffset = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private float clueVisibleDistance = 2.8f;
    [SerializeField] private float hudVisibleDistance = 4.8f;
    [SerializeField] private float facingDotThreshold = 0.45f;

    private readonly List<string> playerInput = new List<string>();
    private readonly List<TextMesh> clueTitleMeshes = new List<TextMesh>();
    private readonly List<TextMesh> clueBodyMeshes = new List<TextMesh>();
    private readonly List<Transform> planetNameplates = new List<Transform>();
    private readonly string[] supportPillarNames = { "Pillar Torch", "Pillar Torch (1)" };

    private PlanetTrialPreset activePreset;
    private Transform clueBoard;
    private Renderer clueBoardRenderer;
    private Text codeText;
    private Text statusText;
    private GameObject clueRoot;
    private GameObject hudRoot;
    private bool hasStarted;
    private bool isSolved;

    private void Awake()
    {
        GameObject boardObject = GameObject.Find(clueBoardObjectName);
        clueBoard = boardObject != null ? boardObject.transform : null;
        clueBoardRenderer = clueBoard != null ? clueBoard.GetComponent<Renderer>() : null;

        if (ThirdPuzzle != null)
        {
            ThirdPuzzle.SetActive(false);
        }

        SelectPreset();
        BuildRuntimeDisplays();
        EnsureSupportColliders();
        RefreshClueBoard();
        RefreshCodeDisplay();
        BuildPlanetNameplates();
        SetStatus("Read the atlas and test the first world.");
        RefreshRuntimeVisibility();
    }

    private void OnEnable()
    {
        PlanetButton.OnButtonPressed += HandleButtonPress;
    }

    private void OnDisable()
    {
        PlanetButton.OnButtonPressed -= HandleButtonPress;
    }

    private void Update()
    {
        RefreshRuntimeVisibility();
        RefreshPlanetNameplates();
    }

    private void HandleButtonPress(string value)
    {
        if (isSolved || activePreset == null || string.IsNullOrWhiteSpace(value))
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

        playerInput.Add(value.Trim());
        RefreshCodeDisplay();

        if (playerInput.Count < activePreset.Sequence.Length)
        {
            SetStatus($"{playerInput.Count} of {activePreset.Sequence.Length} worlds answer.");
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
            SetStatus("The stars reject the order. The sky falls silent.");
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
            Debug.LogError("PlanetManager: Door Animator is not assigned.");
        }

        if (ThirdPuzzle != null)
        {
            ThirdPuzzle.SetActive(true);
        }

        RefreshCodeDisplay(true);
        SetStatus("The heavens align.");
        RefreshRuntimeVisibility();
        PuzzleSolved?.Invoke();
    }

    private void SelectPreset()
    {
        if (Presets.Length == 0)
        {
            Debug.LogError("PlanetManager: No planet presets are configured.");
            return;
        }

        int presetIndex = 0;
        if (randomizeSequenceEachPlaythrough && Presets.Length > 1)
        {
            do
            {
                presetIndex = Random.Range(0, Presets.Length);
            }
            while (presetIndex == lastPresetIndex);
        }

        activePreset = Presets[presetIndex];
        lastPresetIndex = presetIndex;
    }

    private void BuildRuntimeDisplays()
    {
        BuildClueBoardText();
        BuildHud();
        RestyleClueBoard();
    }

    private void BuildClueBoardText()
    {
        if (clueBoard == null)
        {
            Debug.LogWarning("PlanetManager: Could not find the Room 2 clue board.");
            return;
        }

        clueTitleMeshes.Clear();
        clueBodyMeshes.Clear();

        Font titleFont = RuntimeTypography.GetDisplayFont();
        Font bodyFont = RuntimeTypography.GetBodyFont();

        GameObject root = new GameObject("Room2ClueFront");
        root.transform.SetParent(clueBoard.parent, true);
        root.transform.position = clueBoard.position + clueBoard.TransformDirection(GetReadableBoardOffset(boardOffset));
        root.transform.rotation = Quaternion.LookRotation(-clueBoard.forward, clueBoard.up);
        root.transform.localScale = Vector3.one;
        clueRoot = root;

        clueTitleMeshes.Add(CreateWorldTextMesh(
            root.transform,
            "Title",
            titleFont,
            50,
            0.0148f,
            titleTextColor,
            new Vector3(0f, 0.206f, 0f),
            TextAnchor.MiddleCenter,
            TextAlignment.Center,
            FontStyle.Bold));

        clueBodyMeshes.Add(CreateWorldTextMesh(
            root.transform,
            "Body",
            bodyFont,
            31,
            0.0114f,
            bodyTextColor,
            new Vector3(0f, 0.048f, 0f),
            TextAnchor.UpperCenter,
            TextAlignment.Center,
            FontStyle.Bold));
    }

    private static Vector3 GetReadableBoardOffset(Vector3 sourceOffset)
    {
        float zSign = sourceOffset.z >= 0f ? 1f : -1f;
        float liftedZ = Mathf.Max(Mathf.Abs(sourceOffset.z), 0.034f);
        return new Vector3(sourceOffset.x, sourceOffset.y, liftedZ * zSign);
    }

    private void BuildHud()
    {
        GameObject canvasGo = new GameObject("Room2PlanetHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);
        hudRoot = canvasGo;

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 651;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Image panel = CreatePanelImage(
            canvasGo.transform,
            "Panel",
            new Vector2(430f, 128f),
            new Color(0.05f, 0.035f, 0.02f, 0.44f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(28f, 24f));

        CreateText(
            panel.transform,
            "Heading",
            new Vector2(370f, 24f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -14f),
            16,
            hudTextColor,
            TextAnchor.UpperLeft,
            FontStyle.Bold).text = "COSMIC ORDER";

        codeText = CreateText(
            panel.transform,
            "Code",
            new Vector2(370f, 30f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -48f),
            17,
            hudTextColor,
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        statusText = CreateText(
            panel.transform,
            "Status",
            new Vector2(370f, 40f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(20f, 14f),
            13,
            new Color(0.92f, 0.89f, 0.82f, 0.96f),
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

    private void RefreshClueBoard()
    {
        if (activePreset == null)
        {
            return;
        }

        for (int i = 0; i < clueTitleMeshes.Count; i++)
        {
            clueTitleMeshes[i].text = "COSMIC ORDER";
        }

        string clueText =
            activePreset.Title +
            "\n\n" +
            activePreset.Clue +
            "\n\nTrust the atlas, not the sky.";

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
        builder.Append("Input: ");

        for (int i = 0; i < activePreset.Sequence.Length; i++)
        {
            if (i > 0)
            {
                builder.Append("  ->  ");
            }

            if (showSolvedState || i < playerInput.Count)
            {
                string value = showSolvedState && i >= playerInput.Count
                    ? activePreset.Sequence[i]
                    : playerInput[i];
                builder.Append(value);
            }
            else
            {
                builder.Append("_");
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
            if (!string.Equals(playerInput[i], activePreset.Sequence[i], System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
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
        if (isSolved)
        {
            return false;
        }

        if (hasStarted)
        {
            return true;
        }

        if (clueBoard == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        return Vector3.Distance(mainCamera.transform.position, clueBoard.position) <= hudVisibleDistance;
    }

    private Vector3 GetBoardTargetPosition()
    {
        if (clueBoardRenderer != null)
        {
            return clueBoardRenderer.bounds.center;
        }

        return clueBoard.position;
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

    private void BuildPlanetNameplates()
    {
        PlanetButton[] planetButtons = FindObjectsByType<PlanetButton>(FindObjectsSortMode.None);
        Font font = RuntimeTypography.GetBodyFont();
        planetNameplates.Clear();
        HashSet<Transform> labeledButtons = new HashSet<Transform>();

        for (int i = 0; i < planetButtons.Length; i++)
        {
            PlanetButton button = planetButtons[i];
            if (button == null || string.IsNullOrWhiteSpace(button.buttonValue))
            {
                continue;
            }

            PlanetButton rootButton = button.GetComponentInParent<PlanetButton>();
            Transform buttonRoot = rootButton != null ? rootButton.transform : button.transform;
            if (!labeledButtons.Add(buttonRoot))
            {
                continue;
            }

            Transform existingLabel = buttonRoot.Find("RuntimePlanetLabel");
            if (existingLabel != null)
            {
                planetNameplates.Add(existingLabel);
                continue;
            }

            GameObject labelGo = new GameObject("RuntimePlanetLabel", typeof(TextMesh));
            labelGo.transform.SetParent(buttonRoot, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            labelGo.transform.localRotation = Quaternion.identity;

            TextMesh textMesh = labelGo.GetComponent<TextMesh>();
            textMesh.font = font;
            textMesh.fontSize = 28;
            textMesh.characterSize = 0.026f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.95f, 0.91f, 0.82f, 0.98f);
            textMesh.text = button.buttonValue.ToUpperInvariant();

            MeshRenderer renderer = labelGo.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            planetNameplates.Add(labelGo.transform);
        }
    }

    private void RefreshPlanetNameplates()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        for (int i = 0; i < planetNameplates.Count; i++)
        {
            Transform label = planetNameplates[i];
            if (label == null)
            {
                continue;
            }

            Vector3 toCamera = label.position - mainCamera.transform.position;
            if (toCamera.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            label.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
        }
    }

    private void EnsureSupportColliders()
    {
        for (int i = 0; i < supportPillarNames.Length; i++)
        {
            GameObject pillar = GameObject.Find(supportPillarNames[i]);
            if (pillar == null || pillar.GetComponent<BoxCollider>() != null)
            {
                continue;
            }

            if (!TryCalculateBounds(pillar, out Bounds bounds))
            {
                continue;
            }

            BoxCollider collider = pillar.AddComponent<BoxCollider>();
            collider.center = pillar.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;
        }
    }

    private static bool TryCalculateBounds(GameObject target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            bounds = new Bounds();
            return false;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return true;
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
        TextAlignment alignment,
        FontStyle fontStyle)
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
        textMesh.fontStyle = fontStyle;
        textMesh.color = color;
        textMesh.lineSpacing = 0.88f;
        textMesh.text = string.Empty;

        WorldTextMeshUtility.ApplyReadableStyle(textMesh, color);

        return textMesh;
    }

    private static Image CreatePanelImage(
        Transform parent,
        string name,
        Vector2 size,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition)
    {
        GameObject imageGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(parent, false);

        RectTransform rect = imageGo.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
        rect.anchoredPosition = anchoredPosition;
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
        rect.pivot = new Vector2((anchorMin.x + anchorMax.x) * 0.5f, (anchorMin.y + anchorMax.y) * 0.5f);
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

        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(1.1f, -1.1f);

        return text;
    }

    private static Font GetBuiltinFont()
    {
        if (builtinFont == null)
        {
            builtinFont = RuntimeTypography.GetBodyFont();
        }

        return builtinFont;
    }
}
