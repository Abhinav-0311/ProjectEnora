using UnityEngine;

public class ResetPuzzleObject : Interactable
{
    private SevenObjectPuzzle puzzleManager;

    void Start()
    {
        puzzleManager = FindObjectOfType<SevenObjectPuzzle>();

        if (puzzleManager == null)
        {
            Debug.LogError("SevenObjectPuzzle Manager not found in the scene!");
        }

        // Connect interaction
        onInteract.AddListener(TriggerReset);
    }

    void TriggerReset()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ResetPuzzleManually();
            Debug.Log("Puzzle manually reset by interaction!");
        }
    }
}
