using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized feedback layer for puzzle progress, interaction cues, and combat payoff.
/// Keeps the playable path feeling responsive without duplicating sound logic in every script.
/// </summary>
public class GameplayFeedbackDirector : MonoBehaviour
{
    private static GameplayFeedbackDirector instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject("GameplayFeedbackDirector");
        go.AddComponent<GameplayFeedbackDirector>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        KeypadButton.OnButtonPressed -= HandleKeypadButtonPressed;
        KeypadButton.OnButtonPressed += HandleKeypadButtonPressed;

        PlanetButton.OnButtonPressed -= HandlePlanetButtonPressed;
        PlanetButton.OnButtonPressed += HandlePlanetButtonPressed;

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        ConfigureScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (instance != this)
        {
            return;
        }

        KeypadButton.OnButtonPressed -= HandleKeypadButtonPressed;
        PlanetButton.OnButtonPressed -= HandlePlanetButtonPressed;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureScene(scene.name);
    }

    private void ConfigureScene(string sceneName)
    {
        KeypadManager keypad = FindFirstObjectByType<KeypadManager>();
        if (keypad != null)
        {
            keypad.PuzzleFailed -= HandlePuzzleFailed;
            keypad.PuzzleSolved -= HandlePuzzleSolved;
            keypad.PuzzleFailed += HandlePuzzleFailed;
            keypad.PuzzleSolved += HandlePuzzleSolved;
        }

        PlanetManager planet = FindFirstObjectByType<PlanetManager>();
        if (planet != null)
        {
            planet.PuzzleFailed -= HandlePuzzleFailed;
            planet.PuzzleSolved -= HandlePuzzleSolved;
            planet.PuzzleFailed += HandlePuzzleFailed;
            planet.PuzzleSolved += HandlePuzzleSolved;
        }

        SigilAlignmentPuzzle sigils = FindFirstObjectByType<SigilAlignmentPuzzle>();
        if (sigils != null)
        {
            sigils.PuzzleSolved -= HandlePuzzleSolved;
            sigils.PuzzleSolved += HandlePuzzleSolved;
        }

        FinalPuzzle loop = FindFirstObjectByType<FinalPuzzle>();
        if (loop != null)
        {
            loop.LoopResolved -= HandleLoopResolved;
            loop.LoopResolved += HandleLoopResolved;
        }

        CandlePuzzle candle = FindFirstObjectByType<CandlePuzzle>();
        if (candle != null)
        {
            candle.StepAdvanced -= HandleCandleStepAdvanced;
            candle.PuzzleReset -= HandlePuzzleFailed;
            candle.PuzzleSolved -= HandlePuzzleSolved;
            candle.StepAdvanced += HandleCandleStepAdvanced;
            candle.PuzzleReset += HandlePuzzleFailed;
            candle.PuzzleSolved += HandlePuzzleSolved;
        }

        SevenObjectPuzzle sins = FindFirstObjectByType<SevenObjectPuzzle>();
        if (sins != null)
        {
            sins.CorrectStep -= HandleSinsStepAdvanced;
            sins.PuzzleReset -= HandlePuzzleFailed;
            sins.PuzzleSolved -= HandlePuzzleSolved;
            sins.CorrectStep += HandleSinsStepAdvanced;
            sins.PuzzleReset += HandlePuzzleFailed;
            sins.PuzzleSolved += HandlePuzzleSolved;
        }

        CoffinKey[] coffins = FindObjectsByType<CoffinKey>(FindObjectsSortMode.None);
        for (int i = 0; i < coffins.Length; i++)
        {
            coffins[i].WrongCoffinChosen -= HandleWrongCoffinChosen;
            coffins[i].CorrectCoffinChosen -= HandleCorrectCoffinChosen;
            coffins[i].WrongCoffinChosen += HandleWrongCoffinChosen;
            coffins[i].CorrectCoffinChosen += HandleCorrectCoffinChosen;
        }

        CannonControlSwitcher cannon = FindFirstObjectByType<CannonControlSwitcher>();
        if (cannon != null)
        {
            cannon.EnteredCannon -= HandleEnteredCannon;
            cannon.ExitedCannon -= HandleExitedCannon;
            cannon.EnteredCannon += HandleEnteredCannon;
            cannon.ExitedCannon += HandleExitedCannon;
        }

        DemonHealth demon = FindFirstObjectByType<DemonHealth>();
        if (demon != null)
        {
            demon.HitTaken -= HandleDemonHitTaken;
            demon.Defeated -= HandleDemonDefeated;
            demon.HitTaken += HandleDemonHitTaken;
            demon.Defeated += HandleDemonDefeated;
        }
    }

    private void HandleKeypadButtonPressed(int value)
    {
        PlayUi(FeedbackSoundNames.Click, 0.8f, Random.Range(0.96f, 1.05f));
    }

    private void HandlePlanetButtonPressed(string value)
    {
        PlayUi(FeedbackSoundNames.Click, 0.8f, Random.Range(0.98f, 1.07f));
    }

    private void HandleCandleStepAdvanced(int step)
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.72f, 0.95f + (step * 0.05f));
    }

    private void HandleSinsStepAdvanced(GameObject obj, int stepIndex)
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.76f, 0.9f + (stepIndex * 0.03f));
    }

    private void HandlePuzzleSolved()
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.95f, 1.04f);
    }

    private void HandlePuzzleFailed()
    {
        PlayUi(FeedbackSoundNames.Click, 0.72f, 0.72f);
    }

    private void HandleLoopResolved()
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.9f, 0.98f);
    }

    private void HandleWrongCoffinChosen(CoffinKey coffin)
    {
        PlayUi(FeedbackSoundNames.Click, 0.82f, 0.68f);
    }

    private void HandleCorrectCoffinChosen(CoffinKey coffin)
    {
        PlayUi(FeedbackSoundNames.Confirm, 1f, 0.92f);
    }

    private void HandleEnteredCannon()
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.86f, 0.88f);
    }

    private void HandleExitedCannon()
    {
        PlayUi(FeedbackSoundNames.Click, 0.7f, 0.94f);
    }

    private void HandleDemonHitTaken(int hitsTaken)
    {
        PlayUi(FeedbackSoundNames.Confirm, 0.95f, 0.88f + (hitsTaken * 0.05f));
    }

    private void HandleDemonDefeated()
    {
        PlayUi(FeedbackSoundNames.Confirm, 1f, 0.8f);
    }

    private static void PlayUi(string soundName, float volume, float pitch)
    {
        if (SoundManager.Instance == null)
        {
            return;
        }

        SoundManager.Instance.PlaySound2D(soundName, volume, pitch);
    }
}
