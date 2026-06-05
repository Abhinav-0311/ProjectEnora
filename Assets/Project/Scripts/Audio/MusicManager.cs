using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    [Tooltip("If true, each scene load crossfades to DefaultMusicForScene (see SceneNames). Disable for manual-only music.")]
    [SerializeField]
    private bool applyMusicFromSceneName = true;

    private Coroutine crossfadeRoutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!applyMusicFromSceneName || musicLibrary == null || musicSource == null)
            return;

        NarrativeProgress.SetChapterFromSceneName(scene.name);
        var track = SceneNames.DefaultMusicForScene(scene.name);
        if (string.IsNullOrEmpty(track))
            return;

        var clip = musicLibrary.GetClipFromName(track);
        if (clip == null)
        {
            Debug.LogWarning(
                $"MusicManager: add \"{track}\" to MusicLibrary for scene \"{scene.name}\" (narrative auto-music).");
            return;
        }

        PlayMusic(track);
    }
 
    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (musicLibrary == null || musicSource == null)
            return;
        var clip = musicLibrary.GetClipFromName(trackName);
        if (clip == null)
        {
            Debug.LogWarning($"MusicManager: no clip for track \"{trackName}\". Add it to MusicLibrary.");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            musicSource.loop = true;
            return;
        }

        if (crossfadeRoutine != null)
        {
            StopCoroutine(crossfadeRoutine);
        }

        crossfadeRoutine = StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }
 
    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }
 
        musicSource.clip = nextTrack;
        musicSource.loop = true;
        musicSource.Play();
 
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }

        musicSource.volume = 1f;
        crossfadeRoutine = null;
    }
}
