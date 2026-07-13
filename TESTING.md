# Drova Minimap manual regression checklist

Run this checklist before releasing a new DLL.

1. Start at the title screen. Confirm the log contains no `DrovaMinimap` exception and the minimap's settings are present after opening the shared Modding settings page.
2. Start a save on the main world without opening the original map. Confirm that the minimap immediately shows the correct map area and player position, never another UI background.
3. Walk in several directions, then open the original map. Confirm the player arrow and native markers match it.
4. Teleport, reload a save, and change scenes. Confirm the minimap rebinds and does not freeze, drift, or leave duplicate UI.
5. Enter a cave or another non-main-world map. Confirm the minimap hides. Return to the main world and confirm it returns.
6. Open the mod settings, change every setting, close the menu, restart the game, and confirm every value persists before opening the settings page again.
7. Disable the minimap in settings, then open menus and modal windows. Confirm it hides immediately and returns only when enabled gameplay resumes.
8. Visit an area with several native markers. Confirm off-screen markers do not appear at the edge, then appear normally as the player approaches them.
9. Switch between English, Simplified Chinese, and Traditional Chinese. Reopen the options menu and confirm every minimap label is translated without missing-localization text.
10. Inspect MelonLoader's latest log. Confirm there are no DrovaMinimap exceptions or repeated warnings. A native-map warning is acceptable only when the radar fallback persists for at least three seconds.
