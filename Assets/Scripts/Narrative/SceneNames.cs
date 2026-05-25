/// <summary>
/// Build scene names (must match .unity filenames exactly, without extension).
/// Narrative flow: Main Menu -> Controls (intro / lore / how to play) -> Level 1 dungeon -> Level 2 castle & boss.
/// </summary>
public static class SceneNames
{
    public const string MainMenu = "Main_Menu";
    public const string Controls = "Controls";
    public const string Level1 = "lvl1";
    public const string Level2 = "lvl2";

    /// <summary>Default music keys for MusicLibrary when a scene loads (MusicManager).</summary>
    public static string DefaultMusicForScene(string unitySceneName)
    {
        switch (unitySceneName)
        {
            case MainMenu: return MusicTrackNames.Menu;
            case Controls: return MusicTrackNames.Lore;
            case Level1: return MusicTrackNames.Dungeon;
            case Level2: return MusicTrackNames.Castle;
            default: return string.Empty;
        }
    }
}
