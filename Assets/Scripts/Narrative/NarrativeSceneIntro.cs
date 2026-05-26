using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Auto-plays short narrative intro lines the first time key scenes load in a play session.
/// </summary>
public class NarrativeSceneIntro : MonoBehaviour
{
    private static NarrativeSceneIntro instance;
    private static readonly HashSet<string> ShownScenes = new HashSet<string>();

    private Coroutine activeSequence;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        var go = new GameObject("NarrativeSceneIntro");
        go.AddComponent<NarrativeSceneIntro>();
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
        PlaySceneIntro(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneIntro(scene);
    }

    private void PlaySceneIntro(Scene scene)
    {
        if (!NarrativeBeatLibrary.TryGetSceneIntroSequence(scene.name, out var beatIds) || beatIds == null || beatIds.Length == 0)
            return;

        if (ShownScenes.Contains(scene.name))
            return;

        ShownScenes.Add(scene.name);

        if (activeSequence != null)
            StopCoroutine(activeSequence);

        activeSequence = StartCoroutine(PlaySequence(beatIds));
    }

    private IEnumerator PlaySequence(IReadOnlyList<NarrativeBeatId> beatIds)
    {
        while (SceneTransitionController.Instance != null && SceneTransitionController.Instance.IsTransitioning)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.75f);

        for (int i = 0; i < beatIds.Count; i++)
        {
            NarrativeBeatDefinition beat = NarrativeBeatLibrary.Get(beatIds[i]);
            if (beat.BeatId == NarrativeBeatId.None)
                continue;

            NarrativeProgress.SetBeat(beat.BeatId);
            if (beat.Chapter != StoryChapter.None)
                NarrativeProgress.SetChapter(beat.Chapter);

            if (!string.IsNullOrWhiteSpace(beat.Subtitle))
            {
                NarrativeProgress.AddLog(
                    NarrativeProgress.GetBeatLabel(beat.BeatId),
                    beat.Subtitle);
            }

            if (!string.IsNullOrEmpty(beat.MusicTrackName) && MusicManager.Instance != null)
                MusicManager.Instance.PlayMusic(beat.MusicTrackName);

            if (!string.IsNullOrWhiteSpace(beat.Subtitle))
            {
                if (NarrativeHUD.Instance != null)
                    NarrativeHUD.Instance.ShowSubtitle(beat.Subtitle, beat.SubtitleDuration);
                else
                    Debug.Log("[Narrative Intro] " + beat.Subtitle);
            }

            yield return new WaitForSecondsRealtime(beat.SubtitleDuration + 0.8f);
        }

        activeSequence = null;
    }
}
