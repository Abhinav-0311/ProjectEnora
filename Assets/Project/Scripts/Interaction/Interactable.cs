using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private string promptAction = "interact";
    [SerializeField] [TextArea] private string promptOverrideText;

    public UnityEvent onInteract;

    public void SetPromptAction(string action)
    {
        promptAction = action ?? string.Empty;
    }

    public void SetPromptOverride(string text)
    {
        promptOverrideText = text ?? string.Empty;
    }

    public string GetPromptText()
    {
        if (!string.IsNullOrWhiteSpace(promptOverrideText))
        {
            return promptOverrideText.Trim();
        }

        string action = string.IsNullOrWhiteSpace(promptAction)
            ? InferPromptActionFromName()
            : promptAction.Trim();

        return $"Press E or Left Click to {action}.";
    }

    private string InferPromptActionFromName()
    {
        string objectName = gameObject.name.ToLowerInvariant();

        if (objectName.Contains("key"))
        {
            return "take the key";
        }

        if (objectName.Contains("cannon"))
        {
            return "enter the cannon";
        }

        if (objectName.Contains("coffin"))
        {
            return "open the coffin";
        }

        if (objectName.Contains("door"))
        {
            return "open the door";
        }

        if (objectName.Contains("portal"))
        {
            return "enter the portal";
        }

        return "interact";
    }
}
