using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SigilAlignmentPuzzle : MonoBehaviour
{
    private static Font builtinFont;

    public event System.Action PuzzleStarted;
    public event System.Action PuzzleSolved;

    public string[] correctSequence = { "Order", "Chaos", "Day", "Night" }; // Correct order
    private string[] currentSequence = new string[4]; // To store the current state
    public Animator winAnimator; // Assign the Animator in the Inspector
    public GameObject LastPuzzle;

    [Header("Room 3 Board")]
    [SerializeField] private string clueBoardObjectName = "Room3ClueBoard";
    [SerializeField] private Color boardTint = new Color(0.86f, 0.76f, 0.55f, 1f);
    [SerializeField] private Color accentColor = new Color(0.27f, 0.16f, 0.07f, 1f);
    [SerializeField] private Color titleTextColor = new Color(0.08f, 0.04f, 0.015f, 1f);
    [SerializeField] private Color bodyTextColor = new Color(0.06f, 0.035f, 0.015f, 1f);
    [SerializeField] private Vector3 boardOffset = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private float clueVisibleDistance = 4f;
    [SerializeField] private float hudVisibleDistance = 5f;
    [SerializeField] private float facingDotThreshold = 0.28f;

    private bool hasStarted;
    private bool isSolved;
    private Transform clueBoardTransform;
    private Renderer clueBoardRenderer;
    private GameObject clueRoot;
    private GameObject hudRoot;
    private Text orderText;
    private Text statusText;

    private void Awake()
    {
        AutoBindPuzzleControllers();
        BuildClueBoard();
        BuildHud();
        RefreshHud();
    }

    private void Update()
    {
        RefreshClueVisibility();
        RefreshHudVisibility();
    }

    public void UpdatePuzzleState(int puzzleIndex, string element)
    {
        if (!IsValidPuzzleIndex(puzzleIndex))
        {
            return;
        }

        if (!hasStarted)
        {
            hasStarted = true;
            PuzzleStarted?.Invoke();
        }

        if (isSolved)
        {
            return;
        }

        currentSequence[puzzleIndex] = element;
        SetStatus($"Stone {puzzleIndex + 1} now reads {element}.");
        RefreshHud();

        // Check if all puzzles align with the correct sequence
        if (IsSequenceCorrect())
        {
            isSolved = true;
            if (winAnimator != null)
            {
                winAnimator.SetTrigger("Open"); // Play the animation
            }

            if (LastPuzzle != null)
            {
                LastPuzzle.SetActive(true);
            }

            RefreshClueVisibility();
            RefreshHud();
            PuzzleSolved?.Invoke();
        }
    }

    public void InitializePuzzleState(int puzzleIndex, string element)
    {
        if (!IsValidPuzzleIndex(puzzleIndex) || isSolved)
        {
            return;
        }

        currentSequence[puzzleIndex] = element;
        RefreshHud();
    }

    private bool IsSequenceCorrect()
    {
        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (currentSequence[i] != correctSequence[i])
                return false;
        }
        return true;
    }

    private bool IsValidPuzzleIndex(int puzzleIndex)
    {
        return puzzleIndex >= 0 && puzzleIndex < currentSequence.Length;
    }

    private void BuildClueBoard()
    {
        GameObject clueBoard = GameObject.Find(clueBoardObjectName);
        if (clueBoard == null)
        {
            return;
        }

        clueBoardTransform = clueBoard.transform;
        clueBoardRenderer = clueBoard.GetComponent<Renderer>();

        Renderer boardRenderer = clueBoardRenderer;
        if (boardRenderer != null)
        {
            Material materialInstance = boardRenderer.material;
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

        GameObject root = new GameObject("Room3ClueFront");
        root.transform.SetParent(clueBoard.transform.parent, true);
        root.transform.position = clueBoard.transform.position + clueBoard.transform.TransformDirection(GetReadableBoardOffset(boardOffset));
        root.transform.rotation = Quaternion.LookRotation(-clueBoard.transform.forward, clueBoard.transform.up);
        root.transform.localScale = Vector3.one;
        clueRoot = root;

        CreateBoardText(
            root.transform,
            "Title",
            50,
            0.0154f,
            titleTextColor,
            new Vector3(0f, 0.208f, 0f),
            TextAnchor.MiddleCenter,
            FontStyle.Bold,
            "TRUTH OF PERCEPTION");

        CreateBoardText(
            root.transform,
            "Body",
            31,
            0.0116f,
            bodyTextColor,
            new Vector3(0f, 0.05f, 0f),
            TextAnchor.UpperCenter,
            FontStyle.Bold,
            "Ignore the colors. Read the words.\n\n" +
            "The correct order is:\n" +
            "ORDER  ->  CHAOS  ->  DAY  ->  NIGHT\n\n" +
            "Turn each stone until those four words face you from left to right.");

        RefreshClueVisibility();
    }

    private void BuildHud()
    {
        GameObject canvasGo = new GameObject("Room3SigilHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGo.transform.SetParent(transform, false);
        hudRoot = canvasGo;

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 652;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Image panel = CreatePanelImage(
            canvasGo.transform,
            "Panel",
            new Vector2(430f, 104f),
            new Color(0.05f, 0.035f, 0.02f, 0.34f),
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
            new Color(0.96f, 0.93f, 0.86f, 0.98f),
            TextAnchor.UpperLeft,
            FontStyle.Bold).text = "TRUTH OF PERCEPTION";

        orderText = CreateText(
            panel.transform,
            "Order",
            new Vector2(370f, 30f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(20f, -48f),
            16,
            new Color(0.96f, 0.93f, 0.86f, 0.98f),
            TextAnchor.MiddleLeft,
            FontStyle.Bold);

        statusText = CreateText(
            panel.transform,
            "Status",
            new Vector2(370f, 32f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(20f, 14f),
            13,
            new Color(0.92f, 0.89f, 0.82f, 0.96f),
            TextAnchor.LowerLeft,
            FontStyle.Normal);

        SetStatus("Turn the stones to match the board.");
    }

    private void AutoBindPuzzleControllers()
    {
        PuzzleRaycastRotator[] rotators = FindObjectsByType<PuzzleRaycastRotator>(FindObjectsSortMode.None);
        System.Array.Sort(rotators, (left, right) => CompareByHorizontalPosition(left, right));
        for (int i = 0; i < rotators.Length; i++)
        {
            if (rotators[i] == null)
            {
                continue;
            }

            rotators[i].gameManager = this;
            rotators[i].puzzleIndex = i;
        }

        PuzzleRaycastLogic[] alternateRotators = FindObjectsByType<PuzzleRaycastLogic>(FindObjectsSortMode.None);
        System.Array.Sort(alternateRotators, (left, right) => CompareByHorizontalPosition(left, right));
        for (int i = 0; i < alternateRotators.Length; i++)
        {
            if (alternateRotators[i] == null)
            {
                continue;
            }

            alternateRotators[i].gameManager = this;
            alternateRotators[i].puzzleIndex = i;
        }
    }

    private static int CompareByHorizontalPosition(MonoBehaviour left, MonoBehaviour right)
    {
        if (left == null && right == null)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        return left.transform.position.x.CompareTo(right.transform.position.x);
    }

    private static Vector3 GetReadableBoardOffset(Vector3 sourceOffset)
    {
        float zSign = sourceOffset.z >= 0f ? 1f : -1f;
        float liftedZ = Mathf.Max(Mathf.Abs(sourceOffset.z), 0.034f);
        return new Vector3(sourceOffset.x, sourceOffset.y, liftedZ * zSign);
    }

    private void RefreshClueVisibility()
    {
        if (clueRoot == null)
        {
            return;
        }

        bool showClue = ShouldShowClueBoard();
        if (clueRoot.activeSelf != showClue)
        {
            clueRoot.SetActive(showClue);
        }
    }

    private void RefreshHud()
    {
        if (orderText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("Order: ");
        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(" -> ");
            }

            builder.Append(string.IsNullOrWhiteSpace(currentSequence[i]) ? "_" : currentSequence[i]);
        }

        orderText.text = builder.ToString();
    }

    private void RefreshHudVisibility()
    {
        if (hudRoot == null)
        {
            return;
        }

        bool showHud = ShouldShowHud();
        if (hudRoot.activeSelf != showHud)
        {
            hudRoot.SetActive(showHud);
        }
    }

    private bool ShouldShowClueBoard()
    {
        if (isSolved || clueBoardTransform == null)
        {
            return false;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        Vector3 targetPosition = clueBoardRenderer != null ? clueBoardRenderer.bounds.center : clueBoardTransform.position;
        Vector3 toBoard = targetPosition - mainCamera.transform.position;
        float distance = toBoard.magnitude;
        if (distance > clueVisibleDistance || distance <= Mathf.Epsilon)
        {
            return false;
        }

        float facingDot = Vector3.Dot(mainCamera.transform.forward, toBoard.normalized);
        return facingDot >= facingDotThreshold
            && WorldTextMeshUtility.HasClearSight(mainCamera, clueBoardTransform, targetPosition);
    }

    private bool ShouldShowHud()
    {
        if (isSolved || clueBoardTransform == null)
        {
            return false;
        }

        if (hasStarted)
        {
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return false;
        }

        return Vector3.Distance(mainCamera.transform.position, clueBoardTransform.position) <= hudVisibleDistance;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private static void CreateBoardText(
        Transform parent,
        string name,
        int fontSize,
        float characterSize,
        Color color,
        Vector3 localPosition,
        TextAnchor anchor,
        FontStyle fontStyle,
        string text)
    {
        GameObject textGo = new GameObject(name, typeof(TextMesh));
        textGo.transform.SetParent(parent, false);
        textGo.transform.localPosition = localPosition;
        textGo.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = textGo.GetComponent<TextMesh>();
        textMesh.font = GetBuiltinFont();
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = fontStyle;
        textMesh.color = color;
        textMesh.lineSpacing = 0.88f;
        textMesh.text = text;

        WorldTextMeshUtility.ApplyReadableStyle(textMesh, color);
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
        rect.pivot = new Vector2((rect.anchorMin.x + rect.anchorMax.x) * 0.5f, (rect.anchorMin.y + rect.anchorMax.y) * 0.5f);
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

        Shadow shadow = textGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(1.1f, -1.1f);

        return text;
    }

    private static Font GetBuiltinFont()
    {
        if (builtinFont != null)
        {
            return builtinFont;
        }

        builtinFont = RuntimeTypography.GetBodyFont();

        return builtinFont;
    }
}
