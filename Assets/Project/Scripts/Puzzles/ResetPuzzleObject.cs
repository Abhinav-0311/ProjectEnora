using UnityEngine;

public class ResetPuzzleObject : Interactable
{
    private SevenObjectPuzzle puzzleManager;

    void Start()
    {
        SetPromptAction("reset the relic order");
        puzzleManager = FindFirstObjectByType<SevenObjectPuzzle>();

        if (puzzleManager == null)
        {
            Debug.LogError("SevenObjectPuzzle Manager not found in the scene!");
        }

        // Connect interaction
        onInteract.RemoveListener(TriggerReset);
        onInteract.AddListener(TriggerReset);
    }

    void TriggerReset()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ResetPuzzleManually();
            puzzleManager.ShowPuzzleHint("The relic order has been reset.");
        }
    }
}
