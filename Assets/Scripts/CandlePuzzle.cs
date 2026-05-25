using UnityEngine;

public class CandlePuzzle : MonoBehaviour
{
    public GameObject[] candles;  // Assign in the Inspector
    public int[] correctOrder = { 1, 3, 2, 4 }; // Correct lighting order
    private int currentStep = 0;
    public GameObject door; // The door to open
    public Animator dooranim;

    void Start()
    {
        dooranim = door.GetComponent<Animator>();
    }

    public void CheckCandleOrder(int candleIndex)
    {
        if (candleIndex == correctOrder[currentStep])
        {
            currentStep++;

            if (currentStep == correctOrder.Length)
            {
                OpenDoor("DoorOpen");
            }
        }
        else
        {
            ResetCandles();
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
    }

    void OpenDoor(string Triggername)
    {
        dooranim.SetTrigger("DoorOpen");
    }
}
