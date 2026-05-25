public enum NarrativeBeatId
{
    None = 0,
    PrisonAwakening = 1,
    PrisonJudgment = 2,
    PrisonLockedDoor = 3,
    DungeonArrival = 4,
    ForgottenMind = 5,
    CosmicOrder = 6,
    TruthOfPerception = 7,
    Loop = 8,
    CastleArrival = 9,
    LockedPast = 10,
    RitualOfLight = 11,
    SinPride = 12,
    SinGreed = 13,
    SinWrath = 14,
    SinEnvy = 15,
    SinLust = 16,
    SinGluttony = 17,
    SinSloth = 18,
    SinsReveal = 19,
    HallOfDeath = 20,
    HallWrongChoice = 21,
    HallCorrectChoice = 22,
    DemonTwist1 = 23,
    DemonTwist2 = 24,
    BossPhaseWrath = 25,
    BossPhaseGreed = 26,
    EndingEscape = 27,
    EndingRedemption = 28
}

public struct NarrativeBeatDefinition
{
    public NarrativeBeatDefinition(
        NarrativeBeatId beatId,
        StoryChapter chapter,
        string subtitle,
        float subtitleDuration = 5f,
        string musicTrackName = "")
    {
        BeatId = beatId;
        Chapter = chapter;
        Subtitle = subtitle ?? string.Empty;
        SubtitleDuration = subtitleDuration;
        MusicTrackName = musicTrackName ?? string.Empty;
    }

    public NarrativeBeatId BeatId { get; }
    public StoryChapter Chapter { get; }
    public string Subtitle { get; }
    public float SubtitleDuration { get; }
    public string MusicTrackName { get; }
}

public static class NarrativeBeatLibrary
{
    private static readonly NarrativeBeatId[] ControlsIntroSequence =
    {
        NarrativeBeatId.PrisonAwakening,
        NarrativeBeatId.PrisonJudgment,
        NarrativeBeatId.PrisonLockedDoor
    };

    private static readonly NarrativeBeatId[] Level1IntroSequence =
    {
        NarrativeBeatId.DungeonArrival
    };

    private static readonly NarrativeBeatId[] Level2IntroSequence =
    {
        NarrativeBeatId.CastleArrival
    };

    public static NarrativeBeatDefinition Get(NarrativeBeatId beatId)
    {
        switch (beatId)
        {
            case NarrativeBeatId.PrisonAwakening:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Intro,
                    NarrativeDefaultLines.PrisonAwakening,
                    4.5f);
            case NarrativeBeatId.PrisonJudgment:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Intro,
                    NarrativeDefaultLines.PrisonJudgment,
                    6f,
                    MusicTrackNames.Lore);
            case NarrativeBeatId.PrisonLockedDoor:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Intro,
                    NarrativeDefaultLines.PrisonLockedDoor,
                    3.5f);
            case NarrativeBeatId.DungeonArrival:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Dungeon,
                    NarrativeDefaultLines.DungeonArrival,
                    4.5f,
                    MusicTrackNames.Dungeon);
            case NarrativeBeatId.ForgottenMind:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Dungeon,
                    NarrativeDefaultLines.ForgottenMindWall,
                    4.5f);
            case NarrativeBeatId.CosmicOrder:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Dungeon,
                    NarrativeDefaultLines.CosmicOrderWall,
                    4.5f);
            case NarrativeBeatId.TruthOfPerception:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Dungeon,
                    NarrativeDefaultLines.TruthPerceptionWall,
                    4.5f);
            case NarrativeBeatId.Loop:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Dungeon,
                    NarrativeDefaultLines.LoopWall,
                    5f);
            case NarrativeBeatId.CastleArrival:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.CastleArrival,
                    5f,
                    MusicTrackNames.Castle);
            case NarrativeBeatId.LockedPast:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.LockedPast,
                    4.5f);
            case NarrativeBeatId.RitualOfLight:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.RitualOfLight,
                    4.5f);
            case NarrativeBeatId.SinPride:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinPride,
                    4.5f);
            case NarrativeBeatId.SinGreed:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinGreed,
                    4.5f);
            case NarrativeBeatId.SinWrath:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinWrath,
                    4.5f);
            case NarrativeBeatId.SinEnvy:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinEnvy,
                    4.5f);
            case NarrativeBeatId.SinLust:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinLust,
                    4.5f);
            case NarrativeBeatId.SinGluttony:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinGluttony,
                    4.5f);
            case NarrativeBeatId.SinSloth:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinSloth,
                    4.5f);
            case NarrativeBeatId.SinsReveal:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.SinsReveal,
                    5f);
            case NarrativeBeatId.HallOfDeath:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.HallOfDeath,
                    5f);
            case NarrativeBeatId.HallWrongChoice:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.HallWrongChoice,
                    3.5f);
            case NarrativeBeatId.HallCorrectChoice:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Castle,
                    NarrativeDefaultLines.HallCorrectChoice,
                    3.5f);
            case NarrativeBeatId.DemonTwist1:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Boss,
                    NarrativeDefaultLines.DemonTwist1,
                    4.5f,
                    MusicTrackNames.Boss);
            case NarrativeBeatId.DemonTwist2:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Boss,
                    NarrativeDefaultLines.DemonTwist2,
                    4.5f,
                    MusicTrackNames.Boss);
            case NarrativeBeatId.BossPhaseWrath:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Boss,
                    NarrativeDefaultLines.BossPhaseWrath,
                    3.5f,
                    MusicTrackNames.Boss);
            case NarrativeBeatId.BossPhaseGreed:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Boss,
                    NarrativeDefaultLines.BossPhaseGreed,
                    3.5f,
                    MusicTrackNames.Boss);
            case NarrativeBeatId.EndingEscape:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Ending,
                    NarrativeDefaultLines.EndingPortal,
                    6f,
                    MusicTrackNames.Ending);
            case NarrativeBeatId.EndingRedemption:
                return new NarrativeBeatDefinition(
                    beatId,
                    StoryChapter.Ending,
                    NarrativeDefaultLines.EndingRedemption,
                    6f,
                    MusicTrackNames.Ending);
            default:
                return default;
        }
    }

    public static bool TryGetSceneIntroSequence(string sceneName, out NarrativeBeatId[] beatIds)
    {
        switch (sceneName)
        {
            case SceneNames.Controls:
                beatIds = ControlsIntroSequence;
                return true;
            case SceneNames.Level1:
                beatIds = Level1IntroSequence;
                return true;
            case SceneNames.Level2:
                beatIds = Level2IntroSequence;
                return true;
            default:
                beatIds = null;
                return false;
        }
    }
}
