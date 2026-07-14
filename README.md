# Drova Minimap

`Drova Minimap` is a MelonLoader mod that renders a non-interactive minimap from Drova's original map visuals.

## Release status

Current release: **1.0.0**

Validated against the bundled Drova Modding API **0.5.2**. The mod reads Drova's per-definition map UI directly, so verify a new game build with [TESTING.md](TESTING.md) before publishing it as supported.

## Features

- Displays the original art for the most specific enabled map covering the player, with the player centered and oriented by look direction.
- Shows native map and NPC markers only when they approach the minimap viewport.
- Hides during menus, modal windows, HUD-hidden states, and whenever no enabled map is valid for the player's current scene and position.
- Includes settings for enablement, automatic area-map switching, size, zoom, and opacity.
- Uses Drova's localization system with shipped translations for every language directory supported by Drova.

## Requirements

- Drova with MelonLoader installed.
- `Drova_Modding_API.dll` version 0.5.2 installed in the game's `Mods` directory.

## Installation

1. Install MelonLoader and the required Drova Modding API.
2. Copy `DrovaMinimapMod.dll` into Drova's `Mods` directory.
3. Start the game. Configure the minimap in the shared Modding settings page.

## Compatibility and fallback behavior

Map selection mirrors the native player-marker eligibility: a map must be enabled, valid on the active scene, and contain the player's feet position. By default, the minimap chooses the smallest matching local map. Disable **Switch to area maps** to retain the best matching world map instead. When only overlapping world-map variants remain, it prefers `World_Detailed`, then the rough world map, while supplemental overlays such as `World_Shrines` are never selected as terrain. This is a minimap presentation rule; it never reads or changes Drova's saved full-map tab (`ActiveMapGuid`).

The visual is read from the matching original `GUI_Map` through the map panel's definition-to-map dictionary, then from that map's `MapArea`. The hidden map panel is initialized once without opening or changing the full-map UI. Cached artwork is re-read if the original UI replaces its sprite, texture, or tint. If the original visual is unavailable, the mod shows the radar fallback.

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
