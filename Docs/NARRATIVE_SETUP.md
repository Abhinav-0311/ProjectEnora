# Narrative Setup

This project now includes a lightweight narrative framework built around reusable beat presets.

## What Was Added

- `NarrativeBeatId`: named story beats for the prison intro, dungeon rooms, castle rooms, sins, boss reveal, and endings.
- `NarrativeBeatLibrary`: central source of subtitle text, recommended chapter tags, durations, and optional music cues.
- `NarrativeSceneIntro`: automatic one-time-per-session intro playback for `Controls`, `lvl1`, and `lvl2`.
- `StoryBeatTrigger` preset support: you can now assign a beat preset and let the trigger pull the line automatically.

## Recommended Trigger Mapping

Use `StoryBeatTrigger` at these room entrances or interact moments:

- `ForgottenMind` for the first dungeon puzzle room
- `CosmicOrder` for the planet/order room
- `TruthOfPerception` for the color/perception room
- `Loop` when the player realizes they must return to the start
- `LockedPast` for the first castle key room
- `RitualOfLight` when the candle ritual puzzle is solved or entered
- `SinPride`, `SinGreed`, `SinWrath`, `SinEnvy`, `SinLust`, `SinGluttony`, `SinSloth` across the seven sins section
- `SinsReveal` after the seven sins sequence is completed
- `HallOfDeath` when entering the coffin hall
- `HallWrongChoice` on wrong coffin feedback
- `HallCorrectChoice` on the correct coffin reveal
- `DemonTwist1` and `DemonTwist2` at the start of the boss encounter
- `EndingEscape` on the main portal
- `EndingRedemption` on the optional redemption ending

## Scene Intro Behavior

These lines now play automatically:

- `Controls`: prison awakening and judgment voice
- `lvl1`: dungeon arrival
- `lvl2`: castle arrival

The auto intros only play once per scene per play session so they do not repeat on every reload.

## Unity Wiring Tips

- Leave the `subtitle` field blank on `StoryBeatTrigger` if you want the preset line to be used directly.
- Fill `subtitle` manually if you want a custom variation for a specific room while keeping the same beat tag.
- Use `chapterOverride` only when a trigger should advance the story differently than its default preset.
- Add a `Boss` track to `MusicLibrary` if you want the demon reveal beats to crossfade automatically.
