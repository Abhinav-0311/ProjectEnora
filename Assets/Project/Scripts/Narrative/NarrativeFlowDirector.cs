using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime scene director that ties the existing puzzle flow to narrative beats,
/// objectives, and the memory log without requiring manual scene re-wiring.
/// </summary>
public class NarrativeFlowDirector : MonoBehaviour
{
    private static NarrativeFlowDirector instance;

    private bool hasShownForgottenMind;
    private bool hasShownCosmicOrder;
    private bool hasShownTruthOfPerception;
    private bool hasShownLoop;
    private bool hasShownLockedPast;
    private bool hasShownRitualOfLight;
    private bool hasShownSinsReveal;
    private bool hasShownHallOfDeath;
    private bool hasShownDemonTwist;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject("NarrativeFlowDirector");
        go.AddComponent<NarrativeFlowDirector>();
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BeginSceneConfiguration(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BeginSceneConfiguration(scene.name);
    }

    private void BeginSceneConfiguration(string sceneName)
    {
        StopAllCoroutines();
        ResetSceneFlags();
        HideContextCard();

        if (NarrativeHUD.Instance != null)
        {
            bool isGameplayScene = sceneName == SceneNames.Level1 || sceneName == SceneNames.Level2;
            NarrativeHUD.Instance.SetGameplayHudVisible(isGameplayScene);
        }

        NarrativeProgress.SetChapterFromSceneName(sceneName);
        ApplySceneDefaultObjective(sceneName);
        StartCoroutine(ConfigureSceneDeferred(sceneName));
    }

    private IEnumerator ConfigureSceneDeferred(string sceneName)
    {
        yield return null;
        yield return null;

        bool isGameplayScene = sceneName == SceneNames.Level1 || sceneName == SceneNames.Level2;
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ClearTransientUi();
            NarrativeHUD.Instance.SetGameplayHudVisible(isGameplayScene);
        }

        switch (sceneName)
        {
            case SceneNames.Level1:
                ConfigureLevel1();
                break;
            case SceneNames.Level2:
                ConfigureLevel2();
                break;
            case SceneNames.Controls:
                NarrativeProgress.SetObjective("Awakening", "Learn the controls, then press Enter to descend into the dungeon.");
                break;
            case SceneNames.MainMenu:
                NarrativeProgress.SetObjective("Main Menu", "Begin the trial when you are ready.");
                break;
        }
    }

    private void ConfigureLevel1()
    {
        GameplayOverlayState.PrepareForSceneTransition();
        CheckpointManager.EnsureSceneStartCheckpoint(SceneNames.Level1);
        EnsurePauseMenuExists();
        NarrativeProgress.SetObjective("Trial I - Forgotten Mind", "Decode the tablet and enter the four memory seals.");
        StartCoroutine(EnsurePlayerReady());

        KeypadManager keypadManager = FindFirstObjectByType<KeypadManager>();
        if (keypadManager != null)
        {
            keypadManager.PuzzleStarted -= HandleKeypadStarted;
            keypadManager.PuzzleSolved -= HandleKeypadSolved;
            keypadManager.PuzzleStarted += HandleKeypadStarted;
            keypadManager.PuzzleSolved += HandleKeypadSolved;
        }

        PlanetManager planetManager = FindFirstObjectByType<PlanetManager>();
        if (planetManager != null)
        {
            planetManager.PuzzleStarted -= HandlePlanetStarted;
            planetManager.PuzzleSolved -= HandlePlanetSolved;
            planetManager.PuzzleStarted += HandlePlanetStarted;
            planetManager.PuzzleSolved += HandlePlanetSolved;
        }

        SigilAlignmentPuzzle sigilPuzzle = FindFirstObjectByType<SigilAlignmentPuzzle>();
        if (sigilPuzzle != null)
        {
            sigilPuzzle.PuzzleStarted -= HandleTruthPuzzleStarted;
            sigilPuzzle.PuzzleSolved -= HandleTruthPuzzleSolved;
            sigilPuzzle.PuzzleStarted += HandleTruthPuzzleStarted;
            sigilPuzzle.PuzzleSolved += HandleTruthPuzzleSolved;
        }

        FinalPuzzle finalPuzzle = FindFirstObjectByType<FinalPuzzle>();
        if (finalPuzzle != null)
        {
            finalPuzzle.LoopResolved -= HandleLoopResolved;
            finalPuzzle.LoopResolved += HandleLoopResolved;
        }
    }

    private void ConfigureLevel2()
    {
        GameplayOverlayState.PrepareForSceneTransition();
        CheckpointManager.EnsureSceneStartCheckpoint(SceneNames.Level2);
        EnsurePauseMenuExists();

        NarrativeProgress.SetObjective("Trial V - The Locked Past", "Search the castle and claim what was buried from you.");
        ShowContextCard(
            "Locked Past",
            "Search the castle for the buried key. The first locked door will not yield until you reclaim it.");
        StartCoroutine(EnsurePlayerReady());

        GameObject keyObject = GameObject.Find("CastleBuriedKey");
        if (keyObject != null && keyObject.TryGetComponent(out Interactable keyInteractable))
        {
            keyInteractable.onInteract.RemoveListener(HandleCastleKeyInteracted);
            keyInteractable.onInteract.AddListener(HandleCastleKeyInteracted);
        }

        CandlePuzzle candlePuzzle = FindFirstObjectByType<CandlePuzzle>();
        if (candlePuzzle != null)
        {
            candlePuzzle.PuzzleStarted -= HandleCandlePuzzleStarted;
            candlePuzzle.StepAdvanced -= HandleCandleStepAdvanced;
            candlePuzzle.PuzzleReset -= HandleCandlePuzzleReset;
            candlePuzzle.PuzzleSolved -= HandleCandlePuzzleSolved;
            candlePuzzle.PuzzleStarted += HandleCandlePuzzleStarted;
            candlePuzzle.StepAdvanced += HandleCandleStepAdvanced;
            candlePuzzle.PuzzleReset += HandleCandlePuzzleReset;
            candlePuzzle.PuzzleSolved += HandleCandlePuzzleSolved;
        }

        SevenObjectPuzzle sinsPuzzle = FindFirstObjectByType<SevenObjectPuzzle>();
        if (sinsPuzzle != null)
        {
            sinsPuzzle.PuzzleStarted -= HandleSinsPuzzleStarted;
            sinsPuzzle.CorrectStep -= HandleSinsCorrectStep;
            sinsPuzzle.PuzzleReset -= HandleSinsPuzzleReset;
            sinsPuzzle.PuzzleSolved -= HandleSinsPuzzleSolved;
            sinsPuzzle.PuzzleStarted += HandleSinsPuzzleStarted;
            sinsPuzzle.CorrectStep += HandleSinsCorrectStep;
            sinsPuzzle.PuzzleReset += HandleSinsPuzzleReset;
            sinsPuzzle.PuzzleSolved += HandleSinsPuzzleSolved;
        }

        CoffinKey[] coffinKeys = FindObjectsByType<CoffinKey>(FindObjectsSortMode.None);
        for (int i = 0; i < coffinKeys.Length; i++)
        {
            coffinKeys[i].CoffinChosen -= HandleCoffinChosen;
            coffinKeys[i].WrongCoffinChosen -= HandleWrongCoffinChosen;
            coffinKeys[i].CorrectCoffinChosen -= HandleCorrectCoffinChosen;
            coffinKeys[i].CoffinChosen += HandleCoffinChosen;
            coffinKeys[i].WrongCoffinChosen += HandleWrongCoffinChosen;
            coffinKeys[i].CorrectCoffinChosen += HandleCorrectCoffinChosen;
        }

        CannonControlSwitcher cannonSwitcher = FindFirstObjectByType<CannonControlSwitcher>();
        if (cannonSwitcher != null)
        {
            cannonSwitcher.EnteredCannon -= HandleEnteredCannon;
            cannonSwitcher.ExitedCannon -= HandleExitedCannon;
            cannonSwitcher.EnteredCannon += HandleEnteredCannon;
            cannonSwitcher.ExitedCannon += HandleExitedCannon;
        }

        DemonHealth demonHealth = FindFirstObjectByType<DemonHealth>();
        if (demonHealth != null)
        {
            demonHealth.HitTaken -= HandleDemonHitTaken;
            demonHealth.Defeated -= HandleDemonDefeated;
            demonHealth.HitTaken += HandleDemonHitTaken;
            demonHealth.Defeated += HandleDemonDefeated;
        }

        EndingPortal[] endingPortals = FindObjectsByType<EndingPortal>(FindObjectsSortMode.None);
        for (int i = 0; i < endingPortals.Length; i++)
        {
            endingPortals[i].PortalEntered -= HandlePortalEntered;
            endingPortals[i].PortalEntered += HandlePortalEntered;
        }

        if (CheckpointManager.IsBossCheckpointActiveForCurrentScene())
        {
            NarrativeProgress.SetObjective("Final Trial - Reflection", "Use the cannon to break the demon's form.");
            ShowContextCard(
                "Judgment Chamber",
                "The chamber remembers your approach. Claim the cannon and break the reflection's form.");
            hasShownLockedPast = true;
            hasShownRitualOfLight = true;
            hasShownSinsReveal = true;
            hasShownHallOfDeath = true;
        }
    }

    private void HandleKeypadStarted()
    {
        if (hasShownForgottenMind)
        {
            return;
        }

        hasShownForgottenMind = true;
        FireBeat(
            NarrativeBeatId.ForgottenMind,
            "Trial I - Forgotten Mind",
            "Decode the tablet and enter the four memory seals.");
    }

    private void HandleKeypadSolved()
    {
        NarrativeProgress.AddLog("First Lock", "The first lock yielded to thought and pattern.");
        NarrativeProgress.SetObjective("Trial II - Cosmic Order", "Study the second tablet and press the planets in the hidden order.");
        StartCoroutine(BindPlanetManagerAfterUnlock());
    }

    private IEnumerator BindPlanetManagerAfterUnlock()
    {
        yield return null;

        PlanetManager planetManager = FindFirstObjectByType<PlanetManager>();
        if (planetManager != null)
        {
            planetManager.PuzzleStarted -= HandlePlanetStarted;
            planetManager.PuzzleSolved -= HandlePlanetSolved;
            planetManager.PuzzleStarted += HandlePlanetStarted;
            planetManager.PuzzleSolved += HandlePlanetSolved;
        }
    }

    private void HandlePlanetStarted()
    {
        if (hasShownCosmicOrder)
        {
            return;
        }

        hasShownCosmicOrder = true;
        FireBeat(
            NarrativeBeatId.CosmicOrder,
            "Trial II - Cosmic Order",
            "Press the planets in the order hidden within the tablet.");
    }

    private void HandlePlanetSolved()
    {
        NarrativeProgress.AddLog("Second Lock", "The heavens answered once the hidden order was obeyed.");
        NarrativeProgress.SetObjective("Trial III - Truth Of Perception", "Rotate the four sigils until order, chaos, day, and night stand correctly.");
    }

    private void HandleTruthPuzzleStarted()
    {
        if (hasShownTruthOfPerception)
        {
            return;
        }

        hasShownTruthOfPerception = true;
        FireBeat(
            NarrativeBeatId.TruthOfPerception,
            "Trial III - Truth Of Perception",
            "Turn the sigils until what you see matches what is true.");
    }

    private void HandleTruthPuzzleSolved()
    {
        if (!hasShownLoop)
        {
            hasShownLoop = true;
            FireBeat(
                NarrativeBeatId.Loop,
                "Trial IV - The Loop",
                "Return to the place where the journey began.");
            return;
        }

        NarrativeProgress.SetObjective("Trial IV - The Loop", "Return to the place where the journey began.");
    }

    private void HandleLoopResolved()
    {
        NarrativeProgress.AddLog("Cycle Broken", "The loop collapsed once you faced the path that birthed it.");
    }

    private void HandleCastleKeyInteracted()
    {
        if (hasShownLockedPast)
        {
            return;
        }

        hasShownLockedPast = true;
        FireBeat(
            NarrativeBeatId.LockedPast,
            "Trial V - The Locked Past",
            "You buried your past, but it still waits behind the next door.");
        NarrativeProgress.AddLog("Buried Key", "The castle yielded the first key as if it had been waiting for your hand.");
        ShowContextCard(
            "Locked Past",
            "The castle keeps what you hid. Recover the key, cross the threshold, and prepare for the ritual chamber.");
    }

    private void HandleCandlePuzzleStarted()
    {
        if (hasShownRitualOfLight)
        {
            return;
        }

        hasShownRitualOfLight = true;
        FireBeat(
            NarrativeBeatId.RitualOfLight,
            "Trial VI - Ritual Of Light",
            "Light the candles in the proper ritual order.");
        ShowContextCard(
            "Ritual of Light",
            "Light the four candles in the hidden order. A false flame will extinguish the ritual.");
    }

    private void HandleCandleStepAdvanced(int step)
    {
        ShowContextCard(
            "Ritual of Light",
            $"Flame {step}/4 accepted. Continue the rite without breaking the sequence.");
    }

    private void HandleCandlePuzzleReset()
    {
        ShowContextCard(
            "Ritual of Light",
            "The ritual collapsed. Read the chamber again and relight the candles from the beginning.");
    }

    private void HandleCandlePuzzleSolved()
    {
        NarrativeProgress.AddLog("Ritual Complete", "The flame answered as if it had been waiting for your hand.");
        NarrativeProgress.SetObjective("Trial VII - Seven Sins", "Judge the seven relics in the proper order.");
        ShowContextCard(
            "Seven Sins",
            "The ritual chamber has opened the path ahead. Enter the next hall and begin the judgment of the seven relics.");
    }

    private void HandleSinsPuzzleStarted()
    {
        NarrativeProgress.SetObjective("Trial VII - Seven Sins", "Judge the seven relics in the proper order.");
        ShowContextCard(
            "Seven Sins",
            "Judge the seven relics in their proper order. Each accepted relic will answer the chamber.");
    }

    private void HandleSinsCorrectStep(GameObject obj, int stepIndex)
    {
        NarrativeBeatId beatId = BeatForSinObject(obj != null ? obj.name : string.Empty);
        if (beatId == NarrativeBeatId.None)
        {
            return;
        }

        FireBeat(beatId);
        int remainingSteps = Mathf.Max(0, 7 - (stepIndex + 1));
        ShowContextCard(
            "Seven Sins",
            remainingSteps > 0
                ? $"Relic {stepIndex + 1}/7 accepted. {remainingSteps} remain before judgment is complete."
                : "The final relic has answered. The chamber is ready to reveal the truth.");
        NarrativeProgress.AddLog(
            NarrativeProgress.GetBeatLabel(beatId),
            $"Sin {stepIndex + 1} has answered: {NarrativeBeatLibrary.Get(beatId).Subtitle}");
    }

    private void HandleSinsPuzzleReset()
    {
        ShowContextCard(
            "Seven Sins",
            "The chamber rejected that order. Begin the judgment again from the first relic.");
    }

    private void HandleSinsPuzzleSolved()
    {
        if (hasShownSinsReveal)
        {
            return;
        }

        hasShownSinsReveal = true;
        FireBeat(
            NarrativeBeatId.SinsReveal,
            "Hall Of Death",
            "The castle has named your sins. Read the death-riddle and choose the coffin bound to the truth.");
        ShowContextCard(
            "Hall of Death",
            "Choose the coffin that matches the riddle. A false choice will send the judgment back to the beginning.");
    }

    private void HandleCoffinChosen(CoffinKey coffin)
    {
        if (hasShownHallOfDeath)
        {
            return;
        }

        hasShownHallOfDeath = true;
        FireBeat(
            NarrativeBeatId.HallOfDeath,
            "Hall Of Death",
            "Choose the coffin that matches the riddle. A false choice will return you to the start.");
    }

    private void HandleWrongCoffinChosen(CoffinKey coffin)
    {
        FireBeat(NarrativeBeatId.HallWrongChoice);
        ShowContextCard(
            "Hall of Death",
            "That coffin was false. The castle rejects you and begins the judgment again.");
    }

    private void HandleCorrectCoffinChosen(CoffinKey coffin)
    {
        CheckpointManager.ActivateBossCheckpoint();
        FireBeat(
            NarrativeBeatId.HallCorrectChoice,
            "Final Trial - Reflection",
            "The path to the cannon stands open. Face the reflection that has been waiting for you.");
        ShowContextCard(
            "Judgment Chamber",
            "The true coffin has answered. Claim the cannon and face the reflection waiting beyond the gate.");
    }

    private void HandleEnteredCannon()
    {
        HideContextCard();

        if (hasShownDemonTwist)
        {
            NarrativeProgress.SetObjective("Final Trial - Reflection", "Use the cannon to break the demon's form.");
            return;
        }

        hasShownDemonTwist = true;
        FireBeat(
            NarrativeBeatId.DemonTwist1,
            "Final Trial - Reflection",
            "Use the cannon to break the demon's form.");
        StartCoroutine(FireDelayedBeat(NarrativeBeatId.DemonTwist2, 2.6f));
    }

    private void HandleExitedCannon()
    {
        NarrativeProgress.AddLog("Retreat", "You left the cannon, but the reflection still waits.");
    }

    private void HandleDemonHitTaken(int hitsTaken)
    {
        if (hitsTaken == 1)
        {
            FireBeat(NarrativeBeatId.BossPhaseWrath);
        }
        else if (hitsTaken == 2)
        {
            FireBeat(NarrativeBeatId.BossPhaseGreed);
        }
    }

    private void HandleDemonDefeated()
    {
        NarrativeProgress.AddLog("Reflection Broken", "The demon fell, but its judgment did not vanish with it.");
        NarrativeProgress.SetObjective("Ending", "Step into the portal, or remain and face what freedom means.");
        ShowContextCard(
            "Ending",
            "A portal has appeared. Step through it to escape, or remain and accept what redemption costs.");
    }

    private void HandlePortalEntered(EndingPortal.EndingKind endingKind)
    {
        NarrativeBeatId beatId = endingKind == EndingPortal.EndingKind.Redemption
            ? NarrativeBeatId.EndingRedemption
            : NarrativeBeatId.EndingEscape;

        NarrativeBeatDefinition beat = NarrativeBeatLibrary.Get(beatId);
        NarrativeProgress.SetBeat(beatId);
        if (beat.Chapter != StoryChapter.None)
        {
            NarrativeProgress.SetChapter(beat.Chapter);
        }

        if (!string.IsNullOrWhiteSpace(beat.Subtitle))
        {
            NarrativeProgress.AddLog(NarrativeProgress.GetBeatLabel(beatId), beat.Subtitle);
        }
    }

    private IEnumerator FireDelayedBeat(NarrativeBeatId beatId, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        FireBeat(beatId);
    }

    private void FireBeat(
        NarrativeBeatId beatId,
        string objectiveTitle = null,
        string objectiveBody = null)
    {
        NarrativeBeatDefinition beat = NarrativeBeatLibrary.Get(beatId);
        if (beat.BeatId == NarrativeBeatId.None)
        {
            return;
        }

        NarrativeProgress.SetBeat(beatId);
        if (beat.Chapter != StoryChapter.None)
        {
            NarrativeProgress.SetChapter(beat.Chapter);
        }

        if (!string.IsNullOrWhiteSpace(beat.Subtitle))
        {
            NarrativeProgress.AddLog(NarrativeProgress.GetBeatLabel(beatId), beat.Subtitle);
            if (NarrativeHUD.Instance != null)
            {
                NarrativeHUD.Instance.ShowSubtitle(beat.Subtitle, beat.SubtitleDuration);
            }
        }

        if (!string.IsNullOrEmpty(beat.MusicTrackName) && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(beat.MusicTrackName);
        }

        if (objectiveTitle != null || objectiveBody != null)
        {
            string resolvedTitle = objectiveTitle ?? NarrativeProgress.CurrentObjectiveTitle;
            string resolvedBody = objectiveBody ?? NarrativeProgress.CurrentObjectiveBody;
            NarrativeProgress.SetObjective(resolvedTitle, resolvedBody);
        }
    }

    private static NarrativeBeatId BeatForSinObject(string objectName)
    {
        switch (objectName)
        {
            case "PrideBarrel":
                return NarrativeBeatId.SinPride;
            case "GreedBarrel":
                return NarrativeBeatId.SinGreed;
            case "WrathBarrel":
                return NarrativeBeatId.SinWrath;
            case "EnvyBarrel":
                return NarrativeBeatId.SinEnvy;
            case "LustBarrel":
                return NarrativeBeatId.SinLust;
            case "GluttonBarrel":
                return NarrativeBeatId.SinGluttony;
            case "SlothBarrel":
                return NarrativeBeatId.SinSloth;
            default:
                return NarrativeBeatId.None;
        }
    }

    private void ResetSceneFlags()
    {
        hasShownForgottenMind = false;
        hasShownCosmicOrder = false;
        hasShownTruthOfPerception = false;
        hasShownLoop = false;
        hasShownLockedPast = false;
        hasShownRitualOfLight = false;
        hasShownSinsReveal = false;
        hasShownHallOfDeath = false;
        hasShownDemonTwist = false;
    }

    private void ApplySceneDefaultObjective(string sceneName)
    {
        switch (sceneName)
        {
            case SceneNames.Level1:
                NarrativeProgress.SetObjective("Trial I - Forgotten Mind", "Decode the tablet and enter the four memory seals.");
                break;
            case SceneNames.Level2:
                NarrativeProgress.SetObjective("Trial V - The Locked Past", "Search the castle and claim what was buried from you.");
                break;
            case SceneNames.Controls:
                NarrativeProgress.SetObjective("Awakening", "Learn the controls, then press Enter to descend into the dungeon.");
                break;
            case SceneNames.MainMenu:
                NarrativeProgress.SetObjective("Main Menu", "Begin the trial when you are ready.");
                break;
        }
    }

    private static void EnsurePauseMenuExists()
    {
        if (FindFirstObjectByType<AnimatedPauseMenu>() != null)
        {
            return;
        }

        GameObject pauseMenu = new GameObject("RuntimePauseMenu");
        pauseMenu.AddComponent<AnimatedPauseMenu>();
    }

    private IEnumerator EnsurePlayerReady()
    {
        yield return null;

        GameplayOverlayState.PrepareForSceneTransition();

        Transform player = FindPlayerTransform();
        if (player == null)
        {
            yield break;
        }

        FirstPersonMovement movement = player.GetComponent<FirstPersonMovement>();
        if (movement != null)
        {
            movement.enabled = true;
        }

        Jump jump = player.GetComponent<Jump>();
        if (jump != null)
        {
            jump.enabled = true;
        }

        Interactor interactor = player.GetComponent<Interactor>();
        if (interactor != null)
        {
            interactor.enabled = true;
        }

        FirstPersonLook look = player.GetComponentInChildren<FirstPersonLook>(true);
        if (look != null)
        {
            look.enabled = true;
        }

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.WakeUp();
        }

        TryResolvePlayerOverlap(player);
    }

    private static Transform FindPlayerTransform()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            return taggedPlayer.transform;
        }

        FirstPersonMovement movement = FindFirstObjectByType<FirstPersonMovement>();
        return movement != null ? movement.transform : null;
    }

    private static void TryResolvePlayerOverlap(Transform player)
    {
        if (player == null)
        {
            return;
        }

        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            return;
        }

        Vector3[] offsets =
        {
            Vector3.zero,
            Vector3.up * 0.2f,
            Vector3.up * 0.6f,
            Vector3.up * 1.1f,
            player.forward * 0.45f + Vector3.up * 0.6f,
            -player.forward * 0.45f + Vector3.up * 0.6f,
            player.right * 0.45f + Vector3.up * 0.6f,
            -player.right * 0.45f + Vector3.up * 0.6f
        };

        Vector3 originalPosition = player.position;
        Quaternion originalRotation = player.rotation;

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 candidatePosition = originalPosition + offsets[i];
            if (IsCapsulePlacementClear(player, capsule, candidatePosition, originalRotation))
            {
                player.position = candidatePosition;
                return;
            }
        }
    }

    private static bool IsCapsulePlacementClear(
        Transform player,
        CapsuleCollider capsule,
        Vector3 candidatePosition,
        Quaternion candidateRotation)
    {
        float radius = capsule.radius * Mathf.Max(player.lossyScale.x, player.lossyScale.z) * 0.95f;
        float scaledHeight = Mathf.Max(capsule.height * player.lossyScale.y, radius * 2f + 0.05f);
        float halfHeight = Mathf.Max(0f, (scaledHeight * 0.5f) - radius);

        Vector3 center = candidatePosition + candidateRotation * Vector3.Scale(capsule.center, player.lossyScale);
        Vector3 top = center + Vector3.up * halfHeight;
        Vector3 bottom = center - Vector3.up * halfHeight;

        Collider[] overlaps = Physics.OverlapCapsule(top, bottom, radius, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider overlap = overlaps[i];
            if (overlap == null || overlap.transform.IsChildOf(player))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private static void ShowContextCard(string title, string body)
    {
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ShowContextCard(title, body);
        }
    }

    private static void HideContextCard()
    {
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.HideContextCard();
        }
    }
}
