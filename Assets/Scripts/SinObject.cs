using UnityEngine;

public class SinObject : Interactable
{
    private SevenObjectPuzzle puzzleManager;

    void Start()
    {
        puzzleManager = FindFirstObjectByType<SevenObjectPuzzle>();
        onInteract.AddListener(TriggerPuzzle);
    }

    void TriggerPuzzle()
    {
        if (puzzleManager != null)
        {
            Debug.Log("SinObject triggered: " + gameObject.name);
            puzzleManager.InteractWithObject(gameObject);
        }
    }
}
