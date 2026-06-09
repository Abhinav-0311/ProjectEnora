using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

/// <summary>
/// Shared runtime helpers for moving the player safely between gameplay states.
/// This keeps reloads, checkpoints, and cannon handoffs from leaving stale
/// controller, physics, or overlap state behind.
/// </summary>
public static class PlayerRuntimeUtility
{
    public static GameObject ResolvePlayerRoot(GameObject explicitRoot = null)
    {
        if (explicitRoot != null)
        {
            return explicitRoot;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            Transform root = taggedPlayer.transform.root;
            return root != null ? root.gameObject : taggedPlayer;
        }

        FirstPersonController starterController = Object.FindFirstObjectByType<FirstPersonController>(FindObjectsSortMode.None);
        if (starterController != null)
        {
            return starterController.transform.root.gameObject;
        }

        FirstPersonMovement miniController = Object.FindFirstObjectByType<FirstPersonMovement>(FindObjectsSortMode.None);
        if (miniController != null)
        {
            return miniController.transform.root.gameObject;
        }

        GameObject namedPlayer = GameObject.Find("PlayerCapsule") ?? GameObject.Find("First Person Controller");
        return namedPlayer != null ? namedPlayer.transform.root.gameObject : null;
    }

    public static void PrepareForExternalControl(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return;
        }

        ApplyInputReset(playerRoot, cursorLocked: false);
        SetGameplayComponentsEnabled(playerRoot, enabled: false);
        ResetPhysics(playerRoot);
    }

    public static void RestoreAfterExternalControl(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return;
        }

        ResetPhysics(playerRoot);
        SetGameplayComponentsEnabled(playerRoot, enabled: true);
        ApplyInputReset(playerRoot, cursorLocked: true);

        if (!GameplayOverlayState.IsOverlayActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public static void TeleportPlayer(GameObject playerRoot, Vector3 position, Quaternion rotation)
    {
        if (playerRoot == null)
        {
            return;
        }

        List<Behaviour> disabledBehaviours = new List<Behaviour>(6);
        CaptureAndDisable(playerRoot.GetComponent<FirstPersonController>(), disabledBehaviours);
        CaptureAndDisable(playerRoot.GetComponent<FirstPersonMovement>(), disabledBehaviours);
        CaptureAndDisable(playerRoot.GetComponent<Jump>(), disabledBehaviours);
        CaptureAndDisable(playerRoot.GetComponent<Interactor>(), disabledBehaviours);
        CaptureAndDisable(playerRoot.GetComponentInChildren<FirstPersonLook>(true), disabledBehaviours);
        CaptureAndDisable(playerRoot.GetComponentInChildren<FirstPersonAudio>(true), disabledBehaviours);

        CharacterController characterController = playerRoot.GetComponent<CharacterController>();
        bool restoreCharacterController = characterController != null && characterController.enabled;
        if (restoreCharacterController)
        {
            characterController.enabled = false;
        }

        Rigidbody rigidbody = playerRoot.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        playerRoot.transform.SetPositionAndRotation(position, rotation);
        Physics.SyncTransforms();

        ResolvePlayerOverlap(playerRoot);

        if (rigidbody != null)
        {
            rigidbody.position = playerRoot.transform.position;
            rigidbody.rotation = playerRoot.transform.rotation;
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.WakeUp();
        }

        if (restoreCharacterController)
        {
            characterController.enabled = true;
        }

        RestoreCaptured(disabledBehaviours);
    }

    public static void ResolvePlayerOverlap(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return;
        }

        Transform player = playerRoot.transform;
        if (!TryGetCapsuleSettings(player, out Vector3 localCenter, out float radius, out float height))
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
            if (!IsCapsulePlacementClear(player, candidatePosition, originalRotation, localCenter, radius, height))
            {
                continue;
            }

            player.position = candidatePosition;
            Physics.SyncTransforms();
            return;
        }
    }

    public static void ResetPhysics(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return;
        }

        Rigidbody rigidbody = playerRoot.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            return;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.WakeUp();
    }

    private static void SetGameplayComponentsEnabled(GameObject playerRoot, bool enabled)
    {
        SetBehaviourEnabled(playerRoot.GetComponent<FirstPersonController>(), enabled);
        SetBehaviourEnabled(playerRoot.GetComponent<FirstPersonMovement>(), enabled);
        SetBehaviourEnabled(playerRoot.GetComponent<Jump>(), enabled);
        SetBehaviourEnabled(playerRoot.GetComponent<Interactor>(), enabled);
        SetBehaviourEnabled(playerRoot.GetComponentInChildren<FirstPersonLook>(true), enabled);
        SetBehaviourEnabled(playerRoot.GetComponentInChildren<FirstPersonAudio>(true), enabled);
    }

    private static void ApplyInputReset(GameObject playerRoot, bool cursorLocked)
    {
        StarterAssetsInputs starterInputs = playerRoot.GetComponent<StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.MoveInput(Vector2.zero);
            starterInputs.LookInput(Vector2.zero);
            starterInputs.JumpInput(false);
            starterInputs.SprintInput(false);
            starterInputs.cursorLocked = cursorLocked;
            starterInputs.cursorInputForLook = cursorLocked;
        }
    }

    private static void SetBehaviourEnabled(Behaviour behaviour, bool enabled)
    {
        if (behaviour != null)
        {
            behaviour.enabled = enabled;
        }
    }

    private static void CaptureAndDisable(Behaviour behaviour, List<Behaviour> disabledBehaviours)
    {
        if (behaviour == null || !behaviour.enabled)
        {
            return;
        }

        disabledBehaviours.Add(behaviour);
        behaviour.enabled = false;
    }

    private static void RestoreCaptured(List<Behaviour> disabledBehaviours)
    {
        for (int i = 0; i < disabledBehaviours.Count; i++)
        {
            Behaviour behaviour = disabledBehaviours[i];
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }
    }

    private static bool TryGetCapsuleSettings(
        Transform player,
        out Vector3 localCenter,
        out float radius,
        out float height)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            localCenter = Vector3.Scale(characterController.center, player.lossyScale);
            radius = characterController.radius * Mathf.Max(player.lossyScale.x, player.lossyScale.z) * 0.95f;
            height = Mathf.Max(characterController.height * player.lossyScale.y, radius * 2f + 0.05f);
            return true;
        }

        CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            localCenter = Vector3.Scale(capsuleCollider.center, player.lossyScale);
            radius = capsuleCollider.radius * Mathf.Max(player.lossyScale.x, player.lossyScale.z) * 0.95f;
            height = Mathf.Max(capsuleCollider.height * player.lossyScale.y, radius * 2f + 0.05f);
            return true;
        }

        localCenter = Vector3.zero;
        radius = 0f;
        height = 0f;
        return false;
    }

    private static bool IsCapsulePlacementClear(
        Transform player,
        Vector3 candidatePosition,
        Quaternion candidateRotation,
        Vector3 localCenter,
        float radius,
        float height)
    {
        float halfHeight = Mathf.Max(0f, (height * 0.5f) - radius);
        Vector3 center = candidatePosition + candidateRotation * localCenter;
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
}
