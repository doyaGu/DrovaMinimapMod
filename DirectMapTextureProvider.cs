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

            if (TryReadLargestGraphic(guiMap))
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
                _nativeGraphic = bestRawImage.rectTransform;
                Texture = bestRawImage.texture;
                Color = bestRawImage.color;
                AspectRatio = GetAspectRatio(bestRawImage.rectTransform, Texture);
                return true;
            }

            if (bestImage == null)
            {
                return false;
            }

            Sprite = bestImage.sprite;
            _nativeGraphic = bestImage.rectTransform;
            Color = bestImage.color;
            AspectRatio = GetAspectRatio(bestImage.rectTransform, Sprite.texture);
            return true;
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
