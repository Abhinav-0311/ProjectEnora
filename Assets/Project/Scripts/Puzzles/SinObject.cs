using UnityEngine;

public class SinObject : Interactable
{
    private SevenObjectPuzzle puzzleManager;

    void Start()
    {
        SetPromptAction("offer this relic");
        puzzleManager = FindFirstObjectByType<SevenObjectPuzzle>();
        onInteract.RemoveListener(TriggerPuzzle);
        onInteract.AddListener(TriggerPuzzle);
    }

    void TriggerPuzzle()
    {
        if (puzzleManager != null)
        {
            puzzleManager.InteractWithObject(gameObject);
        }
    }
}
