using UnityEngine;

public class Candle : Interactable
{
    public int candleIndex;
    private CandlePuzzle puzzleManager;
    public Light candleLight; // Assign manually in Inspector

    public bool isLit { get; private set; } = false; // Ensure it starts false

    void Start()
    {
        SetPromptAction($"light candle {candleIndex}");
        puzzleManager = FindFirstObjectByType<CandlePuzzle>();

        candleLight = GetComponentInChildren<Light>();
        if (candleLight == null)
        {
            Debug.LogError("No Light found on " + gameObject.name + ". Checking manually...");
            candleLight = transform.Find("Point Light")?.GetComponent<Light>();

            if (candleLight == null)
            {
                Debug.LogError("Still no Light found! Make sure a Point Light is a child of " + gameObject.name);
            }
        }

        if (candleLight != null)
        {
            candleLight.enabled = false; // Start unlit
        }

        // Assign interaction event
        onInteract.RemoveListener(LightCandle);
        onInteract.AddListener(LightCandle);
    }

    public void LightCandle()
    {
        if (isLit)
        {
            if (puzzleManager != null)
            {
                puzzleManager.ShowPuzzleHint("That candle is already burning.");
            }

            return;
        }

        isLit = true;
        if (candleLight != null)
        {
            candleLight.enabled = true; // Enable the light
        }
        else
        {
            Debug.LogError("Candle Light is still missing for " + gameObject.name);
        }

        if (puzzleManager != null)
        {
            puzzleManager.CheckCandleOrder(candleIndex);
        }
    }

    public void ResetCandle()
    {
        isLit = false;
        if (candleLight != null)
            candleLight.enabled = false;
    }
}
