using UnityEngine;

public class PuzzleRaycastLogic : MonoBehaviour
{
    public Transform puzzle; // Assign the Puzzle GameObject
    private int currentIndex = 0; // Tracks the current element (0 = Chaos, 1 = Day, etc.)
    private string[] elements = { "Night", "Day", "Chaos", "Order" }; // Element names
    public GameManager gameManager; // Reference to the GameManager
    public int puzzleIndex; // Unique index for this puzzle (0, 1, 2, 3)

    void Update()
    {
        // Check for mouse click or controller input
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            // Raycast from the camera to the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits the puzzle
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == puzzle)
                {
                    RotatePuzzle();
                }
            }
        }
    }

    void RotatePuzzle()
    {
        // Rotate the puzzle 90 degrees
        puzzle.Rotate(0, 90, 0);
        currentIndex = (currentIndex + 1) % elements.Length;

        // Update the GameManager with the current element
        gameManager.UpdatePuzzleState(puzzleIndex, elements[currentIndex]);

        Debug.Log($"{puzzle.name}: {elements[currentIndex]} aligned");
    }
}
