using System;
using System.Collections.Generic;
using System.Text;

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

public readonly struct NarrativeLogEntry
{
    public NarrativeLogEntry(
        string title,
        string body,
        StoryChapter chapter,
        NarrativeBeatId beatId,
        string sceneName)
    {
        Title = title ?? string.Empty;
        Body = body ?? string.Empty;
        Chapter = chapter;
        BeatId = beatId;
        SceneName = sceneName ?? string.Empty;
    }

    public string Title { get; }
    public string Body { get; }
    public StoryChapter Chapter { get; }
    public NarrativeBeatId BeatId { get; }
    public string SceneName { get; }
}

public static class NarrativeProgress
{
    private static readonly List<NarrativeLogEntry> Entries = new List<NarrativeLogEntry>();

    public static event Action<StoryChapter> ChapterChanged;
    public static event Action<NarrativeBeatId> BeatChanged;
    public static event Action<string, string> ObjectiveChanged;
    public static event Action<NarrativeLogEntry> LogEntryAdded;

    public static StoryChapter CurrentChapter { get; private set; } = StoryChapter.None;
    public static NarrativeBeatId CurrentBeat { get; private set; } = NarrativeBeatId.None;
    public static string CurrentSceneName { get; private set; } = string.Empty;
    public static string CurrentObjectiveTitle { get; private set; } = string.Empty;
    public static string CurrentObjectiveBody { get; private set; } = string.Empty;

    public static IReadOnlyList<NarrativeLogEntry> LogEntries => Entries;

    public static void SetChapter(StoryChapter chapter)
    {
        if (CurrentChapter == chapter)
        {
            return;
        }

        CurrentChapter = chapter;
        ChapterChanged?.Invoke(CurrentChapter);
    }

    public static void SetBeat(NarrativeBeatId beatId)
    {
        if (CurrentBeat == beatId)
        {
            return;
        }

        CurrentBeat = beatId;
        BeatChanged?.Invoke(CurrentBeat);
    }

    public static void SetObjective(string title, string body)
    {
        title = title ?? string.Empty;
        body = body ?? string.Empty;

        if (CurrentObjectiveTitle == title && CurrentObjectiveBody == body)
        {
            return;
        }

        CurrentObjectiveTitle = title;
        CurrentObjectiveBody = body;
        ObjectiveChanged?.Invoke(CurrentObjectiveTitle, CurrentObjectiveBody);
    }

    public static void AddLog(string title, string body)
    {
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;

        if (Entries.Count > 0)
        {
            NarrativeLogEntry latest = Entries[Entries.Count - 1];
            if (latest.Title == safeTitle && latest.Body == safeBody)
            {
                return;
            }
        }

        NarrativeLogEntry entry = new NarrativeLogEntry(
            safeTitle,
            safeBody,
            CurrentChapter,
            CurrentBeat,
            CurrentSceneName);

        Entries.Add(entry);
        LogEntryAdded?.Invoke(entry);
    }

    public static void SetChapterFromSceneName(string sceneName)
    {
        CurrentSceneName = sceneName ?? string.Empty;
        SetChapter(sceneName switch
        {
            SceneNames.MainMenu => StoryChapter.MainMenu,
            SceneNames.Controls => StoryChapter.Intro,
            SceneNames.Level1 => StoryChapter.Dungeon,
            SceneNames.Level2 => StoryChapter.Castle,
            _ => StoryChapter.None
        });
    }

    public static string GetBeatLabel(NarrativeBeatId beatId)
    {
        if (beatId == NarrativeBeatId.None)
        {
            return "Memory";
        }

        StringBuilder builder = new StringBuilder();
        string raw = beatId.ToString();
        for (int i = 0; i < raw.Length; i++)
        {
            char character = raw[i];
            if (i > 0 && char.IsUpper(character) && !char.IsUpper(raw[i - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}
