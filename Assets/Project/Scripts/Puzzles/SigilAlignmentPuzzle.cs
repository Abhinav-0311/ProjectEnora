using UnityEngine;

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
    [SerializeField] private Color boardTint = new Color(0.79f, 0.7f, 0.53f, 1f);
    [SerializeField] private Color accentColor = new Color(0.27f, 0.16f, 0.07f, 1f);
    [SerializeField] private Vector3 boardOffset = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private float clueVisibleDistance = 3f;
    [SerializeField] private float facingDotThreshold = 0.45f;

    private bool hasStarted;
    private bool isSolved;
    private Transform clueBoardTransform;
    private Renderer clueBoardRenderer;
    private GameObject clueRoot;

    private void Awake()
    {
        AutoBindPuzzleControllers();
        BuildClueBoard();
    }

    private void Update()
    {
        RefreshClueVisibility();
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

        // Check if all puzzles align with the correct sequence
        if (IsSequenceCorrect())
        {
            isSolved = true;
            Debug.Log("All puzzles are correct! Playing animation...");
            if (winAnimator != null)
            {
                winAnimator.SetTrigger("Open"); // Play the animation
            }

            if (LastPuzzle != null)
            {
                LastPuzzle.SetActive(true);
                Debug.Log("Last Puzzle Active");
            }

            RefreshClueVisibility();
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
        root.transform.position = clueBoard.transform.position + clueBoard.transform.TransformDirection(boardOffset);
        root.transform.rotation = Quaternion.LookRotation(-clueBoard.transform.forward, clueBoard.transform.up);
        root.transform.localScale = Vector3.one;
        clueRoot = root;

        CreateBoardText(
            root.transform,
            "Title",
            40,
            0.0135f,
            accentColor,
            new Vector3(0f, 0.19f, 0f),
            TextAnchor.MiddleCenter,
            "TRUTH OF PERCEPTION");

        CreateBoardText(
            root.transform,
            "Body",
            26,
            0.0105f,
            new Color(0.2f, 0.12f, 0.06f, 1f),
            new Vector3(0f, 0.04f, 0f),
            TextAnchor.UpperCenter,
            "Each stone shows a name. The colors lie.\n\n" +
            "First the realm is steadied.\n" +
            "Then it is broken.\n" +
            "Then the sky wakes.\n" +
            "Then darkness seals it.\n\n" +
            "Turn the stones until the four truths stand in that order.");

        RefreshClueVisibility();
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
        return facingDot >= facingDotThreshold;
    }

    private static void CreateBoardText(
        Transform parent,
        string name,
        int fontSize,
        float characterSize,
        Color color,
        Vector3 localPosition,
        TextAnchor anchor,
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
        textMesh.color = color;
        textMesh.lineSpacing = 0.9f;
        textMesh.text = text;

        MeshRenderer renderer = textGo.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = textMesh.font.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
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
