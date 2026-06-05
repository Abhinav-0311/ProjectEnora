using UnityEngine;
 
[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}
 
public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;
 
    public AudioClip GetClipFromName(string trackName)
    {
        AudioClip directClip = FindClipExact(trackName);
        if (directClip != null)
        {
            return directClip;
        }

        string fallbackTrackName = ResolveFallbackTrackName(trackName);
        if (!string.IsNullOrEmpty(fallbackTrackName))
        {
            return FindClipExact(fallbackTrackName);
        }

        return null;
    }

    private AudioClip FindClipExact(string trackName)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.clip;
            }
        }

        return null;
    }

    private static string ResolveFallbackTrackName(string requestedTrackName)
    {
        switch (requestedTrackName)
        {
            case MusicTrackNames.Menu:
            case MusicTrackNames.Lore:
                return "MainMenu";
            case MusicTrackNames.Dungeon:
                return "MainMenu";
            case MusicTrackNames.Castle:
            case MusicTrackNames.Boss:
            case MusicTrackNames.Ending:
                return MusicTrackNames.Game;
            default:
                return string.Empty;
        }
    }
}
