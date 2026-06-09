using UnityEngine;

public class CandlePuzzle : MonoBehaviour
{
    public event System.Action PuzzleStarted;
    public event System.Action<int> StepAdvanced;
    public event System.Action PuzzleReset;
    public event System.Action PuzzleSolved;

    public GameObject[] candles;  // Assign in the Inspector
    public int[] correctOrder = { 1, 3, 2, 4 }; // Correct lighting order
    private int currentStep = 0;
    public GameObject door; // The door to open
    public Animator dooranim;

    private bool hasStarted;

    void Start()
    {
        if (door != null)
        {
            dooranim = door.GetComponent<Animator>();
        }
    }

    public void CheckCandleOrder(int candleIndex)
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

        if (candleIndex == correctOrder[currentStep])
        {
            currentStep++;
            StepAdvanced?.Invoke(currentStep);

            if (currentStep == correctOrder.Length)
            {
                OpenDoor("DoorOpen");
                ShowPuzzleHint("The ritual accepts the final flame.");
                PuzzleSolved?.Invoke();
            }
            else
            {
                ShowPuzzleHint($"Flame {currentStep}/{correctOrder.Length} accepted.");
            }
        }
        else
        {
            ShowPuzzleHint("The ritual rejects that flame. Begin again.");
            ResetCandles();
        }
    }

    public void ShowPuzzleHint(string message)
    {
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ShowContextCard("Ritual of Light", message, 2.2f);
        }
    }

    void ResetCandles()
    {
        foreach (GameObject candle in candles)
        {
            Candle candleScript = candle.GetComponent<Candle>();
            if (candleScript != null)
            {
                candleScript.ResetCandle(); // Use ResetCandle() instead of modifying isLit directly
            }
        }
        currentStep = 0;
        PuzzleReset?.Invoke();
    }

    void OpenDoor(string Triggername)
    {
        if (dooranim != null)
        {
            dooranim.SetTrigger(Triggername);
        }
    }
}
