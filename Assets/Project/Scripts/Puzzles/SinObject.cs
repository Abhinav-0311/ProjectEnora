using UnityEngine;

public class SinObject : Interactable
{
    private SevenObjectPuzzle puzzleManager;

    void Start()
    {
        SetPromptAction($"offer {BuildRelicName(gameObject.name)}");
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

    private static string BuildRelicName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return "this relic";
        }

        return objectName
            .Replace("Barrel", string.Empty)
            .Replace("(Clone)", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }
}
