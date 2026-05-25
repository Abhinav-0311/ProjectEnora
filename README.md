# ENORA

ENORA is a first-person Unity puzzle game built around dungeon exploration, symbolic rooms, environmental storytelling, and a late-game combat encounter. The current project version includes menu flow, narrative HUD beats, interactable puzzles, cannon combat, and level progression across multiple scenes.

## Project Stack

- Unity 6 `6000.2.7f2`
- Universal Render Pipeline (URP)
- C#
- Unity Input System
- Cinemachine

## Current Scene Flow

The build settings currently use this sequence:

1. `Main_Menu`
2. `Controls`
3. `lvl1`
4. `lvl2`

## Core Features

- First-person exploration and interaction
- Multi-stage puzzle progression
- Candle, sequence, and object-order puzzle logic
- Narrative subtitle triggers and scene-aware music flow
- Cannon control section and boss-style demon encounter
- Pause menu, audio managers, and scene reload/game over flows

## Story Spine

The project now follows a clearer narrative arc: "The Seven Trials of Atonement."

- Level 1 is the dungeon of memory awakening where logic, order, perception, and repetition rebuild the player's understanding of their past.
- Level 2 is the castle of judgment where buried guilt, ritual, the seven sins, death, and the demon confrontation reveal that the prison is a reflection of the player's own choices.
- The ending reframes escape as a moral question instead of a simple victory state.

## Narrative System

- Scene intro lines can now play automatically for the opening, dungeon arrival, and castle arrival.
- `StoryBeatTrigger` supports reusable story presets through `NarrativeBeatId`, so room triggers can pull authored lines without hardcoding text every time.
- `NarrativeDefaultLines` and `NarrativeBeatLibrary` contain the current story copy for the prison intro, puzzle rooms, sin reveals, boss lines, and endings.

## How To Open

1. Clone the repository:

```bash
git clone https://github.com/Abhinav-0311/ProjectEnora.git
```

2. Open the project folder in Unity Hub.
3. Use Unity `6000.2.7f2` if available for the closest match to the current project.
4. Open `Assets/Scenes/Main_Menu.unity`.
5. Press Play in the Unity editor.

## Repository Notes

- Unity-generated folders such as `Library`, `Logs`, `Temp`, `Builds`, and generated solution files are intentionally ignored in Git.
- Large local archives such as `.unitypackage` files and build zips are also ignored so the repository stays focused on the source project.

## Team

- Abhinav Jain
- Shiva Singh
- Priyanshu Jain
- Kush Sharma

## Status

This repository now contains the current local Unity project snapshot plus the earlier GitHub history merged into `main`.
