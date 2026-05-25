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
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            if (NarrativeHUD.Instance != null)
                NarrativeHUD.Instance.ShowSubtitle(subtitle.Trim(), subtitleDuration);
            else
                Debug.Log("[Story] " + subtitle);
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

        if (!string.IsNullOrEmpty(musicTrackName) && MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(musicTrackName);

        onStoryBeat?.Invoke();
    }
}
