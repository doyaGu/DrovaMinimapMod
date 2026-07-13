# Drova Minimap

`Drova Minimap` is a MelonLoader mod that renders a non-interactive minimap from Drova's original main-world map visual.

## Release status

Current release: **1.0.0**

Validated against the bundled Drova Modding API **0.5.1**. The mod depends on Drova's internal main-world map UI naming, so verify a new game build with [TESTING.md](TESTING.md) before publishing it as supported.

## Features

- Displays the original main-world map art with the player centered and oriented by look direction.
- Shows native map and NPC markers only when they approach the minimap viewport.
- Hides during menus, modal windows, HUD-hidden states, caves, and other non-main-world maps.
- Includes settings for enablement, size, zoom, opacity, regular markers, and NPC markers.
- Uses Drova's localization system; Simplified and Traditional Chinese are translated, with English fallback for other supported languages.

## Requirements

- Drova with MelonLoader installed.
- `Drova_Modding_API.dll` version 0.5.1 installed in the game's `Mods` directory.

## Installation

1. Install MelonLoader and the required Drova Modding API.
2. Copy `DrovaMinimapMod.dll` into Drova's `Mods` directory.
3. Start the game. Configure the minimap in the shared Modding settings page.

## Compatibility and fallback behavior

The minimap is intentionally limited to `MapDefinition_World` and `MapDefinition_World_Detailed`. Caves and other independent map definitions hide the minimap.

The mod binds only the original map node matching the active map definition. If Drova's UI has not created that node yet, the mod uses its radar fallback and retries. A single warning is written only if that fallback persists for three seconds, which normally indicates a game/UI compatibility change.

## Building and releasing

Normal builds do **not** deploy into the game:

```powershell
dotnet build .\DrovaMinimapMod.csproj -c Release
```

Create a distributable ZIP, optionally deploying the resulting DLL to a test game installation:

```powershell
.\scripts\Build-Release.ps1 -GameRoot 'C:\Games\Drova'
.\scripts\Build-Release.ps1 -GameRoot 'C:\Games\Drova' -Deploy
```

The script writes `dist/DrovaMinimapMod-<version>.zip`. Run [TESTING.md](TESTING.md) before distributing it.

## License

No license has been selected yet. Choose one before publishing the source or release archive publicly.
