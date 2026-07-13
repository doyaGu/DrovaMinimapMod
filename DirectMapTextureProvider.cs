using Drova_Modding_API.Access;
using Il2CppDrova;
using Il2CppDrova.GUI;
using Il2CppDrova.MapSystem;
using Il2CppDrova.MapSystem.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace DrovaMinimapMod
{
    internal sealed class DirectMapTextureProvider
    {
        private MapDefinition? _definition;
        private GUI_Map? _nativeMap;
        private RectTransform? _nativeGraphic;
        private float _nextResolveAttempt;

        public Sprite? Sprite { get; private set; }
        public Texture? Texture { get; private set; }
        public Color Color { get; private set; } = Color.white;
        public float AspectRatio { get; private set; } = 1f;
        public GUI_Map? NativeMap => _nativeMap;
        public RectTransform? NativeGraphic => _nativeGraphic;

        public bool TryResolve(MapData mapData)
        {
            if (_definition != mapData.Definition)
            {
                Reset(mapData.Definition);
            }

            if (HasResolvedBinding())
            {
                return true;
            }

            ClearResolvedBinding();

            if (Time.unscaledTime < _nextResolveAttempt)
            {
                return false;
            }

            _nextResolveAttempt = Time.unscaledTime + 1f;
            GUI_Map? guiMap = FindNativeMap(mapData.Definition);
            if (guiMap == null)
            {
                return false;
            }

            _nativeMap = guiMap;

            if (TryReadMapGraphic(guiMap, mapData.Definition))
            {
                return true;
            }

            return false;
        }

        private void Reset(MapDefinition definition)
        {
            _definition = definition;
            _nextResolveAttempt = 0f;
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
            string expectedContainerName = GetExpectedContainerName(definition);
            Transform? expectedContainer = guiMap.transform.FindChild($"Panel/{expectedContainerName}");
            if (expectedContainer != null)
            {
                Image? image = expectedContainer.FindChild("Image")?.GetComponent<Image>();
                if (image != null && image.sprite != null)
                {
                    return TryUseImage(image);
                }

                RawImage? rawImage = expectedContainer.FindChild("Image")?.GetComponent<RawImage>();
                if (rawImage != null && rawImage.texture != null)
                {
                    return TryUseRawImage(rawImage);
                }
            }

            return TryReadLargestGraphic(guiMap);
        }

        private bool TryReadLargestGraphic(GUI_Map guiMap)
        {
            Image? bestImage = null;
            float bestImageArea = 0f;
            foreach (Image image in guiMap.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite == null)
                {
                    continue;
                }

                float area = image.rectTransform.rect.width * image.rectTransform.rect.height;
                if (area > bestImageArea)
                {
                    bestImageArea = area;
                    bestImage = image;
                }
            }

            RawImage? bestRawImage = null;
            float bestRawImageArea = 0f;
            foreach (RawImage rawImage in guiMap.GetComponentsInChildren<RawImage>(true))
            {
                if (rawImage.texture == null)
                {
                    continue;
                }

                float area = rawImage.rectTransform.rect.width * rawImage.rectTransform.rect.height;
                if (area > bestRawImageArea)
                {
                    bestRawImageArea = area;
                    bestRawImage = rawImage;
                }
            }

            if (bestRawImage != null && bestRawImageArea > bestImageArea)
            {
                return TryUseRawImage(bestRawImage);
            }

            if (bestImage == null)
            {
                return false;
            }

            return TryUseImage(bestImage);
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

        private static string GetExpectedContainerName(MapDefinition definition)
        {
            return definition.name.StartsWith("MapDefinition_", StringComparison.Ordinal)
                ? "Map_" + definition.name["MapDefinition_".Length..]
                : definition.name;
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
