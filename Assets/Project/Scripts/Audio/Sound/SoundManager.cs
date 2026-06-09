using UnityEngine;
 
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
 
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
 
    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        PlaySound3D(clip, pos, 1f, 1f);
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos, float volume, float pitch)
    {
        if (clip != null)
        {
            GameObject tempAudio = new GameObject("Temp3DSound");
            tempAudio.transform.position = pos;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            source.spatialBlend = 1f;
            source.minDistance = 1f;
            source.maxDistance = 30f;
            source.Play();

            Destroy(tempAudio, clip.length / Mathf.Max(0.01f, source.pitch) + 0.1f);
        }
    }
 
    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(soundName, pos, 1f, 1f);
    }
 
    public void PlaySound3D(string soundName, Vector3 pos, float volume, float pitch)
    {
        if (sfxLibrary == null)
        {
            return;
        }

        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos, volume, pitch);
    }

    public void PlaySound2D(string soundName)
    {
        PlaySound2D(soundName, 1f, 1f);
    }

    public void PlaySound2D(string soundName, float volume, float pitch)
    {
        if (sfx2DSource == null || sfxLibrary == null)
        {
            return;
        }

        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip == null)
        {
            return;
        }

        float originalPitch = sfx2DSource.pitch;
        sfx2DSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        sfx2DSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        sfx2DSource.pitch = originalPitch;
    }
}
