# Drova Minimap Context

## Terms

- **Minimap runtime** — Owns player, GUI-root, and region-event lifecycle; it decides whether a frame should be rendered.
- **Main-world map resolver** — Selects the enabled, player-containing main-world `MapData`, or produces no map for caves and unrelated scenes.
- **Native map presentation** — The single compatibility adapter for Drova's original map UI. It binds visual data and produces coordinate-correct presentation state.
- **Region-label resolver** — Maps API region values to Drova's native `AreaNames` keys and resolves them against the active game language.
- **Minimap frame** — Immutable render input. It owns layout derivation, marker eligibility, and world-to-viewport projection for one rendered frame.
- **Minimap view** — Owns the mod-created Unity UI tree and renders one minimap frame without querying game state.
