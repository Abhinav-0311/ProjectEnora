using UnityEngine;
using UnityEngine.Serialization;

public class PuzzleRaycastRotator : MonoBehaviour
{
    public Transform puzzle; // Assign the Puzzle GameObject
    private static readonly string[] Elements = { "Night", "Day", "Chaos", "Order" };
    private int currentIndex;
    [FormerlySerializedAs("puzzleManager")]
    public SigilAlignmentPuzzle gameManager; // Reference to the Room 3 sigil puzzle
    public int puzzleIndex; // Unique index for this puzzle (0, 1, 2, 3)
    [SerializeField] private float interactionDistance = 3f;

    private void Start()
    {
        EnsureFaceLabels();
        SyncCurrentStateWithManager(false);
    }

    private void Update()
    {
        if (!InteractionInput.IsInteractPressedThisFrame())
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Transform targetPuzzle = puzzle != null ? puzzle : transform;
        Ray ray = InteractionInput.GetCenteredViewRay(mainCamera);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        if (hit.transform == targetPuzzle || hit.transform.IsChildOf(targetPuzzle))
        {
            RotatePuzzle();
        }
    }

    private void RotatePuzzle()
    {
        Transform targetPuzzle = puzzle != null ? puzzle : transform;

        targetPuzzle.Rotate(0f, 90f, 0f);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(FeedbackSoundNames.Click, 0.76f, 1f);
        }

        SyncCurrentStateWithManager(true);
    }

    private void SyncCurrentStateWithManager(bool isInteraction)
    {
        Transform targetPuzzle = puzzle != null ? puzzle : transform;
        currentIndex = ResolveCurrentIndex(targetPuzzle.localEulerAngles.y);

        if (gameManager == null)
        {
            return;
        }

        if (isInteraction)
        {
            gameManager.UpdatePuzzleState(puzzleIndex, Elements[currentIndex]);
            return;
        }

        gameManager.InitializePuzzleState(puzzleIndex, Elements[currentIndex]);
    }

    private static int ResolveCurrentIndex(float yRotation)
    {
        int snappedQuarterTurn = Mathf.RoundToInt(Mathf.Repeat(yRotation, 360f) / 90f) % Elements.Length;
        return snappedQuarterTurn;
    }

    private void EnsureFaceLabels()
    {
        Transform targetPuzzle = puzzle != null ? puzzle : transform;
        Font font = RuntimeTypography.GetDisplayFont();

        for (int i = 0; i < targetPuzzle.childCount; i++)
        {
            Transform face = targetPuzzle.GetChild(i);
            if (face == null || !IsNamedFace(face.name))
            {
                continue;
            }

            if (targetPuzzle.Find("RuntimeLabel_" + face.name) != null)
            {
                continue;
            }

            Vector3 direction = face.localPosition.sqrMagnitude > 0.0001f
                ? face.localPosition.normalized
                : Vector3.forward;

            GameObject labelGo = new GameObject("RuntimeLabel_" + face.name, typeof(TextMesh));
            labelGo.transform.SetParent(targetPuzzle, false);
            labelGo.transform.localPosition = face.localPosition * 1.18f;
            labelGo.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);

            TextMesh textMesh = labelGo.GetComponent<TextMesh>();
            textMesh.font = font;
            textMesh.fontSize = 28;
            textMesh.characterSize = 0.04f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = new Color(0.98f, 0.93f, 0.84f, 1f);
            textMesh.text = face.name.ToUpperInvariant();

            MeshRenderer renderer = labelGo.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static bool IsNamedFace(string faceName)
    {
        switch (faceName)
        {
            case "Order":
            case "Chaos":
            case "Day":
            case "Night":
                return true;
            default:
                return false;
        }
    }
}
