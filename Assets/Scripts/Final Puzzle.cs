using UnityEngine;

public class FinalPuzzle : MonoBehaviour
{
    private static Font builtinFont;

    public event System.Action LoopResolved;

    public Animator playerAnimator;
    public Transform playerTransform; // Reference to the player's Transform
    public GameObject triggerlocation; // Trigger location object
    public float triggerDistance = 1.0f; // Distance threshold to activate animation

    [Header("Room 4 Board")]
    [SerializeField] private string clueBoardObjectName = "Riddle 4";
    [SerializeField] private Color boardTint = new Color(0.11f, 0.09f, 0.07f, 1f);
    [SerializeField] private Color accentColor = new Color(0.82f, 0.74f, 0.58f, 1f);
    [SerializeField] private Vector3 boardOffset = new Vector3(0f, 0f, 0.02f);
    [SerializeField] private float clueVisibleDistance = 3f;
    [SerializeField] private float facingDotThreshold = 0.45f;

    private bool hasTriggered;
    private Transform clueBoardTransform;
    private Renderer clueBoardRenderer;
    private GameObject clueRoot;

    private void Awake()
    {
        BuildClueBoard();
    }

    private void Update()
    {
        RefreshClueVisibility();

        if (hasTriggered || playerTransform == null || triggerlocation == null)
        {
            return;
        }

        if (Vector3.Distance(playerTransform.position, triggerlocation.transform.position) <= triggerDistance)
        {
            hasTriggered = true;

            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Open");
            }

            LoopResolved?.Invoke();
            RefreshClueVisibility();
            enabled = false; // Disable the script after triggering to avoid repeated triggers
        }
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

        GameObject root = new GameObject("Room4ClueFront");
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
            "THE LOOP");

        CreateBoardText(
            root.transform,
            "Body",
            26,
            0.0105f,
            new Color(0.96f, 0.93f, 0.86f, 1f),
            new Vector3(0f, 0.04f, 0f),
            TextAnchor.UpperCenter,
            "The dungeon closes only when you return.\n\n" +
            "Walk back to the place where the trial began.\n\n" +
            "Forward is no longer the way out.");

        RefreshClueVisibility();
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
        if (hasTriggered || clueBoardTransform == null)
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

        builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont == null)
        {
            builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (builtinFont == null)
        {
            builtinFont = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }

        return builtinFont;
    }
}
