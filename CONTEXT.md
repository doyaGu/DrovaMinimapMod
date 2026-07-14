# Drova Minimap Context

## Terms

- **Minimap runtime** — Owns player, GUI-root, and region-event lifecycle; it decides whether a frame should be rendered.
- **Area-lifecycle module** — Owns the scene-created `AreaNameSystem`, publishes its instance lifecycle, and provides a snapshot of entered regions.
- **Player-position map resolver** — Selects an enabled definition that is valid on the active scene and contains the player's feet position. Local maps use smallest bounds as a minimap-only specificity rule; overlapping world variants prefer detailed terrain over rough and supplemental maps. It never reads or changes the full-map tab state.
- **Native map presentation** — The one compatibility module for Drova's original map UI. It initializes the hidden map panel, resolves `MapDefinition -> GUI_Map` through `_definitionToMapGui`, reads terrain artwork, calibrates marker space, and always produces one renderable frame result without opening or changing the full-map UI.
- **Map presentation** — Immutable per-map render input: a map definition, its resolved map surface, and the player/marker coordinate conversion for that surface.
- **Map surface** — The single terrain source a minimap frame can render: original sprite, original texture, or the radar fallback. It carries the artwork's aspect ratio, tint, and marker coordinate calibration as one value.
- **Map coordinate calibration** — Maps a normalized position in the original map's `MapArea` marker rect into the terrain artwork rect that the minimap renders.
- **Region-label resolver** — Maps API region values to Drova's native `AreaNames` keys and resolves them against the active game language.
- **Minimap frame** — Immutable render input. It owns layout derivation, marker eligibility, and world-to-viewport projection for one rendered frame.
- **Minimap view** — Owns the mod-created Unity UI tree and renders one minimap frame without querying game state.
