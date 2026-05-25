/// <summary>High-level story position for scripting and optional save/load later.</summary>
public enum StoryChapter
{
    None = 0,
    MainMenu = 1,
    Intro = 2,
    Dungeon = 3,
    Castle = 4,
    Boss = 5,
    Ending = 6
}

public static class NarrativeProgress
{
    public static StoryChapter CurrentChapter { get; private set; } = StoryChapter.None;
    public static NarrativeBeatId CurrentBeat { get; private set; } = NarrativeBeatId.None;
    public static string CurrentSceneName { get; private set; } = string.Empty;

    public static void SetChapter(StoryChapter chapter)
    {
        CurrentChapter = chapter;
    }

    public static void SetBeat(NarrativeBeatId beatId)
    {
        CurrentBeat = beatId;
    }

    public static void SetChapterFromSceneName(string sceneName)
    {
        CurrentSceneName = sceneName ?? string.Empty;
        CurrentChapter = sceneName switch
        {
            SceneNames.MainMenu => StoryChapter.MainMenu,
            SceneNames.Controls => StoryChapter.Intro,
            SceneNames.Level1 => StoryChapter.Dungeon,
            SceneNames.Level2 => StoryChapter.Castle,
            _ => StoryChapter.None
        };
    }
}
