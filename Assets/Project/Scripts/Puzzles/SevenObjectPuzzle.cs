using UnityEngine;
using System.Collections;

public class SevenObjectPuzzle : MonoBehaviour
{
    public event System.Action PuzzleStarted;
    public event System.Action<GameObject, int> CorrectStep;
    public event System.Action PuzzleReset;
    public event System.Action PuzzleSolved;

    public GameObject[] interactableObjects; // Assign the 7 objects here
    public int[] correctOrder = { 0, 1, 2, 3, 4, 5, 6 }; // Correct interaction order (indexes)
    private int currentStep = 0;
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private bool[] loweredObjects;

    public GameObject door; // The door to open
    private Animator doorAnimator;
    public string doorOpenTrigger = "Open"; // Trigger to open door

    private Animator[] objectAnimators;
    private bool hasStarted;

    void Start()
    {
        if (door != null)
        {
            doorAnimator = door.GetComponent<Animator>();
        }

        objectAnimators = new Animator[interactableObjects.Length];
        originalPositions = new Vector3[interactableObjects.Length];
        originalScales = new Vector3[interactableObjects.Length];
        loweredObjects = new bool[interactableObjects.Length];

        for (int i = 0; i < interactableObjects.Length; i++)
        {
            loweredObjects[i] = false;

            if (interactableObjects[i] != null)
            {
                objectAnimators[i] = interactableObjects[i].GetComponent<Animator>();
                originalPositions[i] = interactableObjects[i].transform.localPosition;
                originalScales[i] = interactableObjects[i].transform.localScale;
            }
            else
            {
                Debug.LogWarning("Missing object in interactableObjects at index: " + i);
            }
        }
    }

    public void ResetPuzzleManually()
    {
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            loweredObjects[i] = false;

            if (objectAnimators[i] != null)
            {
                objectAnimators[i].Play("Idle"); // Reset animation state to Idle
            }

            if (interactableObjects[i] != null)
            {
                interactableObjects[i].transform.localPosition = originalPositions[i];
                interactableObjects[i].transform.localScale = originalScales[i];
            }
        }

        currentStep = 0;
        PuzzleReset?.Invoke();
    }

    public void InteractWithObject(GameObject obj)
    {
        if (currentStep >= correctOrder.Length)
        {
            return;
        }

        if (!hasStarted)
        {
            hasStarted = true;
            PuzzleStarted?.Invoke();
        }

        int index = System.Array.IndexOf(interactableObjects, obj);

        if (index == -1)
        {
            Debug.LogWarning("Interacted object not part of puzzle!");
            return;
        }

        if (loweredObjects[index])
        {
            return;
        }

        if (index == correctOrder[currentStep])
        {
            // Correct interaction
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Godown"); // Play Godown trigger
            }

            loweredObjects[index] = true;
            currentStep++;
            CorrectStep?.Invoke(obj, currentStep - 1);

            if (currentStep == correctOrder.Length)
            {
                OpenDoor();
                ShowPuzzleHint("Judgment accepts the final relic.");
                PuzzleSolved?.Invoke();
            }
            else
            {
                ShowPuzzleHint($"Relic {currentStep}/{correctOrder.Length} accepted.");
            }
        }
        else
        {
            ShowPuzzleHint("The chamber rejects that order. Start again.");
            StartCoroutine(ResetAnimationsAndPuzzle());
        }
    }

    public void ShowPuzzleHint(string message)
    {
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ShowContextCard("Seven Sins", message, 2.2f);
        }
    }

    IEnumerator ResetAnimationsAndPuzzle()
    {
        // Reset all objects (send them back up)
        foreach (Animator anim in objectAnimators)
        {
            if (anim != null)
            {
                anim.SetTrigger("GoUp"); // NEW: trigger the "GoUp" animation
            }
        }

        yield return new WaitForSeconds(0.5f);
        currentStep = 0;
        for (int i = 0; i < loweredObjects.Length; i++)
        {
            loweredObjects[i] = false;
        }
        PuzzleReset?.Invoke();
    }

    void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorOpenTrigger);
        }
        else if (door != null)
        {
            door.SetActive(false);
        }
    }
}
