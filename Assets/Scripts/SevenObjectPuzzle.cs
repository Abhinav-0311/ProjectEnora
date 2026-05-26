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

        for (int i = 0; i < interactableObjects.Length; i++)
        {
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
        Debug.Log("Manual Reset Called!");

        for (int i = 0; i < interactableObjects.Length; i++)
        {
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

        Debug.Log("Interacted with object: " + obj.name + " at step " + currentStep);

        if (index == correctOrder[currentStep])
        {
            // Correct interaction
            Animator anim = obj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Godown"); // Play Godown trigger
            }

            currentStep++;
            CorrectStep?.Invoke(obj, currentStep - 1);

            if (currentStep == correctOrder.Length)
            {
                OpenDoor();
                PuzzleSolved?.Invoke();
            }
        }
        else
        {
            Debug.Log("Wrong order! Resetting puzzle.");
            StartCoroutine(ResetAnimationsAndPuzzle());
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
        PuzzleReset?.Invoke();
    }

    void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorOpenTrigger);
            Debug.Log("Door opened with animation!");
        }
        else if (door != null)
        {
            door.SetActive(false);
            Debug.Log("Door deactivated!");
        }
    }
}
