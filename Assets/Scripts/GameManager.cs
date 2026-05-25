using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string[] correctSequence = { "Order", "Chaos", "Day", "Night" }; // Correct order
    private string[] currentSequence = new string[4]; // To store the current state
    public Animator winAnimator; // Assign the Animator in the Inspector
    public GameObject LastPuzzle;

    public void UpdatePuzzleState(int puzzleIndex, string element)
    {
        currentSequence[puzzleIndex] = element;

        // Check if all puzzles align with the correct sequence
        if (IsSequenceCorrect())
        {
            Debug.Log("All puzzles are correct! Playing animation...");
            winAnimator.SetTrigger("Open"); // Play the animation
            LastPuzzle.SetActive(true);
            Debug.Log("Last Puzzle Active");
        }
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
}
