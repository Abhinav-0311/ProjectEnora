using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

/// <summary>
/// Shared gameplay overlay state used by pause and game-over screens.
/// It freezes gameplay, suppresses gameplay HUD, and restores the play state cleanly.
/// </summary>
public static class GameplayOverlayState
{
    private static readonly List<Behaviour> DisabledBehaviours = new List<Behaviour>();

    private static bool overlayActive;
    private static bool gameOverActive;
    private static bool controlsSuppressed;

    public static bool IsOverlayActive => overlayActive;
    public static bool IsGameOver => gameOverActive;

    public static void ShowPauseOverlay()
    {
        if (overlayActive)
        {
            return;
        }

        EnterOverlay(isGameOver: false);
    }

    public static void HidePauseOverlay()
    {
        if (!overlayActive || gameOverActive)
        {
            return;
        }

        ExitOverlay();
    }

    public static void ShowGameOverOverlay()
    {
        if (gameOverActive)
        {
            return;
        }

        if (!overlayActive)
        {
            EnterOverlay(isGameOver: true);
            return;
        }

        gameOverActive = true;
    }

    public static void PrepareForSceneTransition()
    {
        if (!overlayActive && !controlsSuppressed && Time.timeScale == 1f)
        {
            return;
        }

        overlayActive = false;
        gameOverActive = false;

        Time.timeScale = 1f;
        SuppressGameplayControls(false);
        SuppressGameplayUi(false);
    }

    private static void EnterOverlay(bool isGameOver)
    {
        overlayActive = true;
        gameOverActive = isGameOver;

        Time.timeScale = 0f;
        SuppressGameplayControls(true);
        SuppressGameplayUi(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static void ExitOverlay()
    {
        overlayActive = false;
        gameOverActive = false;

        Time.timeScale = 1f;
        SuppressGameplayControls(false);
        SuppressGameplayUi(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void SuppressGameplayUi(bool suppress)
    {
        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.SetGameplayHudVisible(!suppress);
        }

        CrosshairController crosshairController = Object.FindFirstObjectByType<CrosshairController>();
        if (crosshairController != null)
        {
            crosshairController.SetVisible(!suppress);
        }
    }

    private static void SuppressGameplayControls(bool suppress)
    {
        StarterAssetsInputs[] inputs = Object.FindObjectsByType<StarterAssetsInputs>(FindObjectsSortMode.None);
        for (int i = 0; i < inputs.Length; i++)
        {
            StarterAssetsInputs input = inputs[i];
            if (input == null)
            {
                continue;
            }

            input.MoveInput(Vector2.zero);
            input.LookInput(Vector2.zero);
            input.JumpInput(false);
            input.SprintInput(false);
            input.cursorLocked = !suppress;
            input.cursorInputForLook = !suppress;
        }

        if (suppress)
        {
            if (controlsSuppressed)
            {
                return;
            }

            DisabledBehaviours.Clear();
            RegisterAndDisable<FirstPersonController>();
            RegisterAndDisable<FirstPersonMovement>();
            RegisterAndDisable<Jump>();
            RegisterAndDisable<FirstPersonLook>();
            RegisterAndDisable<FirstPersonAudio>();
            RegisterAndDisable<Interactor>();
            RegisterAndDisable<PuzzleRaycastLogic>();
            RegisterAndDisable<PuzzleRaycastRotator>();
            RegisterAndDisable<CannonController>();
            RegisterAndDisable<CannonControlSwitcher>();

            controlsSuppressed = true;
            return;
        }

        for (int i = 0; i < DisabledBehaviours.Count; i++)
        {
            Behaviour behaviour = DisabledBehaviours[i];
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        DisabledBehaviours.Clear();
        controlsSuppressed = false;
    }

    private static void RegisterAndDisable<T>() where T : Behaviour
    {
        T[] behaviours = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            T behaviour = behaviours[i];
            if (behaviour == null || !behaviour.enabled)
            {
                continue;
            }

            DisabledBehaviours.Add(behaviour);
            behaviour.enabled = false;
        }
    }
}
