using System.Collections.Generic;
using UnityEngine;

public class KeypadManager : MonoBehaviour
{
    [Header("Sequence Settings")]
    [Tooltip("Enter the correct sequence of button presses")]
    List<int> correctSequence = new List<int>() { 4, 6, 2, 3};
    private List<int> playerInput = new List<int>();
    public Animator doorAnimator; // Assign the Door's Animator in the Inspector
    public GameObject SecondPuzzle;
    private void OnEnable()
    {
        KeypadButton.OnButtonPressed += HandleButtonPress;
    }

    private void OnDisable()
    {
        KeypadButton.OnButtonPressed -= HandleButtonPress;
    }

  private void HandleButtonPress(int value)
{
    playerInput.Add(value);
    Debug.Log($"Button {value} pressed. Current sequence: {string.Join(", ", playerInput)}");

    if (playerInput.Count == correctSequence.Count)
    {
        Debug.Log("Player input matches the required sequence length.");
        
        if (IsSequenceCorrect())
        {
            Debug.Log("Correct sequence entered!");
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
                Debug.Log("Animation trigger 'Open' has been set.");
                SecondPuzzle.SetActive(true);
            }
            else
            {
                Debug.LogError("Door Animator is not assigned!");
            }
        }
        else
        {
            Debug.Log("Incorrect sequence. Resetting.");
        }
        
        playerInput.Clear(); // Reset input after validation
        Debug.Log("Player input cleared.");
    }
}




    private bool IsSequenceCorrect()
{
    for (int i = 0; i < correctSequence.Count; i++)
    {
        if (playerInput[i] != correctSequence[i])
        {
            Debug.Log($"Sequence mismatch at index {i}: Expected {correctSequence[i]}, Got {playerInput[i]}");
            return false;
        }
    }
    Debug.Log("Sequence matched successfully!");
    return true;
}

}
