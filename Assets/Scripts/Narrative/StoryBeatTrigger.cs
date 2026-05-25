using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Place on a trigger collider. Fires subtitle, optional one-shot voice, music crossfade, and custom events.
/// Use for room intros, wall echoes, and chapter beats.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StoryBeatTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool useTriggerCollider = true;

    [Header("Narrative Preset")]
    [SerializeField] private NarrativeBeatId beatId = NarrativeBeatId.None;
    [SerializeField] private StoryChapter chapterOverride = StoryChapter.None;

    [TextArea(2, 6)]
    [SerializeField] private string subtitle;

    [SerializeField] private float subtitleDuration = 5f;
    [SerializeField] private AudioClip voiceLine;
    [SerializeField] private bool voiceIs2D = true;

    /// <summary>Optional MusicLibrary key (e.g. Dungeon, Boss). Leave empty to keep current track.</summary>
    [SerializeField] private string musicTrackName;

    [SerializeField] private UnityEvent onStoryBeat;

    private bool _fired;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider) return;
        TryFire(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (useTriggerCollider) return;
        TryFire(other.gameObject);
    }

    private void TryFire(GameObject go)
    {
        if (!go.CompareTag(playerTag)) return;
        if (triggerOnce && _fired) return;
        _fired = true;
        Fire();
    }

    /// <summary>Call from UnityEvents or other scripts (e.g. first interact).</summary>
    public void FireFromScript()
    {
        if (triggerOnce && _fired) return;
        _fired = true;
        Fire();
    }

    private void Fire()
    {
        NarrativeBeatDefinition beat = NarrativeBeatLibrary.Get(beatId);
        string resolvedSubtitle = ResolveSubtitle(beat);
        float resolvedSubtitleDuration = ResolveSubtitleDuration(beat);
        string resolvedMusicTrack = ResolveMusicTrack(beat);
        StoryChapter resolvedChapter = ResolveChapter(beat);

        if (beatId != NarrativeBeatId.None)
            NarrativeProgress.SetBeat(beatId);

        if (resolvedChapter != StoryChapter.None)
            NarrativeProgress.SetChapter(resolvedChapter);

        if (!string.IsNullOrWhiteSpace(resolvedSubtitle))
        {
            if (NarrativeHUD.Instance != null)
                NarrativeHUD.Instance.ShowSubtitle(resolvedSubtitle.Trim(), resolvedSubtitleDuration);
            else
                Debug.Log("[Story] " + resolvedSubtitle);
        }

        if (voiceLine != null)
        {
            var pos = voiceIs2D && Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(voiceLine, pos, 1f);
        }
        else
        {
            var src = GetComponent<AudioSource>();
            if (src != null && src.clip != null)
                src.Play();
        }

        if (!string.IsNullOrEmpty(resolvedMusicTrack) && MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(resolvedMusicTrack);

        onStoryBeat?.Invoke();
    }

    private string ResolveSubtitle(NarrativeBeatDefinition beat)
    {
        if (!string.IsNullOrWhiteSpace(subtitle))
            return subtitle.Trim();

        return beat.Subtitle;
    }

    private float ResolveSubtitleDuration(NarrativeBeatDefinition beat)
    {
        if (!string.IsNullOrWhiteSpace(subtitle))
            return subtitleDuration;

        return beat.SubtitleDuration > 0f ? beat.SubtitleDuration : subtitleDuration;
    }

    private string ResolveMusicTrack(NarrativeBeatDefinition beat)
    {
        if (!string.IsNullOrEmpty(musicTrackName))
            return musicTrackName;

        return beat.MusicTrackName;
    }

    private StoryChapter ResolveChapter(NarrativeBeatDefinition beat)
    {
        if (chapterOverride != StoryChapter.None)
            return chapterOverride;

        return beat.Chapter;
    }
}
