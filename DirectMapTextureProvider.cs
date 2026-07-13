using Drova_Modding_API.Access;
using Il2CppDrova;
using Il2CppDrova.GUI;
using Il2CppDrova.MapSystem;
using Il2CppDrova.MapSystem.GUI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace DrovaMinimapMod
{
    internal sealed class DirectMapTextureProvider
    {
        private const float ResolveRetryDelay = 0.25f;
        private const float UnavailabilityWarningDelay = 3f;

        private MapDefinition? _definition;
        private GUI_Map? _nativeMap;
        private RectTransform? _nativeGraphic;
        private float _nextResolveAttempt;
        private float _unresolvedSince = -1f;
        private bool _unavailabilityWarningReported;

        internal Sprite? Sprite { get; private set; }
        internal Texture? Texture { get; private set; }
        internal Color Color { get; private set; } = Color.white;
        internal float AspectRatio { get; private set; } = 1f;
        internal GUI_Map? NativeMap => _nativeMap;
        internal RectTransform? NativeGraphic => _nativeGraphic;

        internal bool TryResolve(MapData mapData)
        {
            if (_definition != mapData.Definition)
            {
                Reset(mapData.Definition);
            }

            if (HasResolvedBinding())
            {
                ClearUnresolvedStatus();
                return true;
            }

            ClearResolvedBinding();

            if (Time.unscaledTime < _nextResolveAttempt)
            {
                return false;
            }

            _nextResolveAttempt = Time.unscaledTime + ResolveRetryDelay;
            GUI_Map? guiMap = FindNativeMap(mapData.Definition);
            if (guiMap == null)
            {
                return ReportUnresolved(mapData.Definition);
            }

            _nativeMap = guiMap;

            if (TryReadMapGraphic(guiMap, mapData.Definition))
            {
                ClearUnresolvedStatus();
                return true;
            }

            return ReportUnresolved(mapData.Definition);
        }

        internal void Reset()
        {
            _definition = null;
            _nextResolveAttempt = 0f;
            _unresolvedSince = -1f;
            _unavailabilityWarningReported = false;
            ClearResolvedBinding();
        }

        private void Reset(MapDefinition definition)
        {
            _definition = definition;
            _nextResolveAttempt = 0f;
            _unresolvedSince = -1f;
            _unavailabilityWarningReported = false;
            ClearResolvedBinding();
        }

        private bool HasResolvedBinding()
        {
            return (Sprite != null || Texture != null)
                && _nativeMap != null
                && _nativeGraphic != null;
        }

        private void ClearResolvedBinding()
        {
            _nativeMap = null;
            _nativeGraphic = null;
            Sprite = null;
            Texture = null;
            Color = Color.white;
            AspectRatio = 1f;
        }

        private static GUI_Map? FindNativeMap(MapDefinition definition)
        {
            GUIGameHandler? guiHandler = ProviderAccess.GetGUIGameHandler();
            GUI_Window_GameMenu? gameMenu = guiHandler?.GetPlayerGamePanel();
            GUI_GameMenu_MapPanel? mapPanel = gameMenu?.GetComponentInChildren<GUI_GameMenu_MapPanel>(true);
            if (mapPanel == null)
            {
                return null;
            }

            GUI_Map? activeMap = mapPanel.ActiveMap;
            if (activeMap != null && activeMap.GetMapDefinition() == definition)
            {
                return activeMap;
            }

            foreach (GUI_Map map in mapPanel.GetComponentsInChildren<GUI_Map>(true))
            {
                if (map != null && map.GetMapDefinition() == definition)
                {
                    return map;
                }
            }

            return null;
        }

        private bool TryReadMapGraphic(GUI_Map guiMap, MapDefinition definition)
        {
            string expectedContainerName = MinimapCompatibility.GetMapContainerName(definition.name);
            Transform? expectedContainer = FindMapContainer(guiMap, expectedContainerName);
            if (expectedContainer == null)
            {
                return false;
            }

            foreach (Image image in expectedContainer.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Image" && image.sprite != null)
                {
                    return TryUseImage(image);
                }
            }

            foreach (RawImage rawImage in expectedContainer.GetComponentsInChildren<RawImage>(true))
            {
                if (rawImage.name == "Image" && rawImage.texture != null)
                {
                    return TryUseRawImage(rawImage);
                }
            }

            return false;
        }

        private bool TryUseImage(Image image)
        {
            if (image.sprite == null)
            {
                return false;
            }

            Sprite = image.sprite;
            _nativeGraphic = image.rectTransform;
            Color = image.color;
            AspectRatio = GetAspectRatio(image.rectTransform, Sprite.texture);
            return true;
        }

        private bool TryUseRawImage(RawImage rawImage)
        {
            if (rawImage.texture == null)
            {
                return false;
            }

            _nativeGraphic = rawImage.rectTransform;
            Texture = rawImage.texture;
            Color = rawImage.color;
            AspectRatio = GetAspectRatio(rawImage.rectTransform, Texture);
            return true;
        }

        private static Transform? FindMapContainer(GUI_Map guiMap, string expectedContainerName)
        {
            Transform? directContainer = guiMap.transform.FindChild($"Panel/{expectedContainerName}");
            if (directContainer != null)
            {
                return directContainer;
            }

            foreach (Transform transform in guiMap.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(transform.name, expectedContainerName, StringComparison.Ordinal))
                {
                    return transform;
                }
            }

            return null;
        }

        private bool ReportUnresolved(MapDefinition definition)
        {
            if (_unresolvedSince < 0f)
            {
                _unresolvedSince = Time.unscaledTime;
            }
            else if (!_unavailabilityWarningReported
                     && Time.unscaledTime - _unresolvedSince >= UnavailabilityWarningDelay)
            {
                MelonLogger.Warning(
                    $"Drova Minimap could not bind the native visual for '{definition.name}'. " +
                    "The minimap is using its radar fallback.");
                _unavailabilityWarningReported = true;
            }

            return false;
        }

        private void ClearUnresolvedStatus()
        {
            _unresolvedSince = -1f;
            _unavailabilityWarningReported = false;
        }

        private static float GetAspectRatio(RectTransform rectTransform, Texture texture)
        {
            if (rectTransform.rect.height > 0f && rectTransform.rect.width > 0f)
            {
                return rectTransform.rect.width / rectTransform.rect.height;
            }

            return texture.height > 0 ? (float)texture.width / texture.height : 1f;
        }

    }
}
