using UnityEngine;

public class PuzzleRaycastLogic : MonoBehaviour
{
    public Transform puzzle; // Assign the Puzzle GameObject
    private int currentIndex = 0; // Tracks the current element (0 = Chaos, 1 = Day, etc.)
    private string[] elements = { "Night", "Day", "Chaos", "Order" }; // Element names
    public SigilAlignmentPuzzle puzzleManager; // Reference to the Room 3 sigil puzzle
    public int puzzleIndex; // Unique index for this puzzle (0, 1, 2, 3)
    [SerializeField] private float interactionDistance = 3f;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)
            && !Input.GetKeyDown(KeyCode.E)
            && !Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Transform targetPuzzle = puzzle != null ? puzzle : transform;
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return;
        }

        if (hit.transform == targetPuzzle || hit.transform.IsChildOf(targetPuzzle))
        {
            RotatePuzzle();
        }
    }

    void RotatePuzzle()
    {
        Transform targetPuzzle = puzzle != null ? puzzle : transform;

        // Rotate the puzzle 90 degrees
        targetPuzzle.Rotate(0, 90, 0);
        currentIndex = (currentIndex + 1) % elements.Length;

        // Update the puzzle manager with the current element
        if (puzzleManager != null)
        {
            puzzleManager.UpdatePuzzleState(puzzleIndex, elements[currentIndex]);
        }

        Debug.Log($"{targetPuzzle.name}: {elements[currentIndex]} aligned");
    }
}
