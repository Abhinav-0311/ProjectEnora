using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Small session checkpoint system for the critical progression beats in the game.
/// It restores the player to the requested checkpoint after a scene reload and
/// reapplies any minimal scene state required for the boss encounter.
/// </summary>
public sealed class CheckpointManager : MonoBehaviour
{
    public const string Level1StartId = "level1_start";
    public const string Level2StartId = "level2_start";
    public const string Level2BossId = "level2_boss";

    private struct CheckpointState
    {
        public bool IsValid;
        public string CheckpointId;
        public string SceneName;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    private static CheckpointManager instance;
    private static CheckpointState activeCheckpoint;

    [Header("Boss Checkpoint")]
    [SerializeField] private float bossCheckpointBackwardOffset = 3.6f;
    [SerializeField] private float bossCheckpointSideOffset = 1.35f;
    [SerializeField] private float bossCheckpointHeightOffset = 0.15f;

    public static bool HasActiveCheckpoint =>
        activeCheckpoint.IsValid && !string.IsNullOrWhiteSpace(activeCheckpoint.SceneName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject("CheckpointManager");
        go.AddComponent<CheckpointManager>();
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
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    public static void EnsureSceneStartCheckpoint(string sceneName)
    {
        if (instance == null || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        if (activeCheckpoint.IsValid && activeCheckpoint.SceneName == sceneName)
        {
            return;
        }

        Transform player = FindPlayerTransform();
        if (player == null)
        {
            return;
        }

        string checkpointId = sceneName == SceneNames.Level2
            ? Level2StartId
            : Level1StartId;

        SetCheckpointInternal(checkpointId, sceneName, player.position, player.rotation, announce: false);
    }

    public static void ActivateBossCheckpoint()
    {
        if (instance == null)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != SceneNames.Level2)
        {
            return;
        }

        Transform cannon = FindBossCannonTransform();
        Transform player = FindPlayerTransform();

        Vector3 spawnPosition;
        Quaternion spawnRotation;
        if (cannon != null)
        {
            Vector3 backward = -cannon.forward * instance.bossCheckpointBackwardOffset;
            Vector3 side = cannon.right * instance.bossCheckpointSideOffset;
            spawnPosition = cannon.position + backward + side + Vector3.up * instance.bossCheckpointHeightOffset;

            Vector3 lookDirection = cannon.position - spawnPosition;
            lookDirection.y = 0f;
            spawnRotation = lookDirection.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : cannon.rotation;
        }
        else if (player != null)
        {
            spawnPosition = player.position;
            spawnRotation = player.rotation;
        }
        else
        {
            return;
        }

        SetCheckpointInternal(Level2BossId, sceneName, spawnPosition, spawnRotation, announce: true);
    }

    public static bool IsBossCheckpointActiveForCurrentScene()
    {
        return activeCheckpoint.IsValid
            && activeCheckpoint.SceneName == SceneManager.GetActiveScene().name
            && activeCheckpoint.CheckpointId == Level2BossId;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplyCheckpointAfterLoad(scene.name));
    }

    private IEnumerator ApplyCheckpointAfterLoad(string sceneName)
    {
        yield return null;
        yield return null;

        if (!activeCheckpoint.IsValid || activeCheckpoint.SceneName != sceneName)
        {
            yield break;
        }

        RestoreSceneState(activeCheckpoint.CheckpointId, sceneName);
        RestorePlayer(activeCheckpoint.Position, activeCheckpoint.Rotation);
    }

    private static void SetCheckpointInternal(
        string checkpointId,
        string sceneName,
        Vector3 position,
        Quaternion rotation,
        bool announce)
    {
        activeCheckpoint = new CheckpointState
        {
            IsValid = true,
            CheckpointId = checkpointId,
            SceneName = sceneName,
            Position = position,
            Rotation = rotation
        };

        if (!announce)
        {
            return;
        }

        if (NarrativeHUD.Instance != null)
        {
            NarrativeHUD.Instance.ShowSubtitle("Checkpoint remembered: Judgment Chamber.", 2.5f);
        }

        NarrativeProgress.AddLog(
            "Checkpoint",
            "The castle marked the path to the final trial. Death will return you to the chamber.");
    }

    private static void RestoreSceneState(string checkpointId, string sceneName)
    {
        if (sceneName != SceneNames.Level2 || checkpointId != Level2BossId)
        {
            return;
        }

        SetGameObjectActive("Door_1", false);
        SetGameObjectActive("Door_1 (1)", false);
        SetGameObjectActive("Global Volume", false);
        SetGameObjectActive("Demon", true);
        SetGameObjectActive("Cannon", true);

        Transform player = FindPlayerTransform();
        if (player != null)
        {
            player.gameObject.SetActive(true);
        }
    }

    private static void RestorePlayer(Vector3 position, Quaternion rotation)
    {
        GameObject playerRoot = PlayerRuntimeUtility.ResolvePlayerRoot();
        if (playerRoot == null)
        {
            return;
        }

        playerRoot.SetActive(true);
        PlayerRuntimeUtility.TeleportPlayer(playerRoot, position, rotation);
        PlayerRuntimeUtility.RestoreAfterExternalControl(playerRoot);
    }

    private static Transform FindPlayerTransform()
    {
        GameObject playerRoot = PlayerRuntimeUtility.ResolvePlayerRoot();
        return playerRoot != null ? playerRoot.transform : null;
    }

    private static Transform FindBossCannonTransform()
    {
        GameObject taggedCannon = GameObject.FindGameObjectWithTag("Cannon");
        if (taggedCannon != null)
        {
            return taggedCannon.transform;
        }

        GameObject namedCannon = FindSceneGameObjectByName("Cannon");
        return namedCannon != null ? namedCannon.transform : null;
    }

    private static void SetGameObjectActive(string objectName, bool active)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        GameObject obj = FindSceneGameObjectByName(objectName);
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    private static GameObject FindSceneGameObjectByName(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        GameObject[] candidates = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < candidates.Length; i++)
        {
            GameObject candidate = candidates[i];
            if (candidate == null
                || candidate.hideFlags != HideFlags.None
                || !candidate.scene.IsValid()
                || candidate.name != objectName)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
