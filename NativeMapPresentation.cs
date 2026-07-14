using Drova_Modding_API.Access;
using Il2CppInterop.Runtime;
using Il2CppDrova;
using Il2CppDrova.GUI;
using Il2CppDrova.MapSystem;
using Il2CppDrova.MapSystem.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Resolves Drova's original map UI into one complete minimap presentation.
    /// This is the only place that knows how to initialize the hidden map panel,
    /// locate a native map, select its terrain visual, and calibrate marker space.
    /// </summary>
    internal sealed class NativeMapPresentation
    {
        private const float ResolveRetryDelay = 0.25f;

        private MapDefinition? _definition;
        private NativeMapBinding? _binding;
        private float _nextResolveAttempt;

        internal MapPresentation Resolve(MapData mapData, Vector2 playerWorldPosition)
        {
            return new MapPresentation(
                mapData.Definition,
                ResolveSurface(mapData),
                playerWorldPosition);
        }

        internal void Reset()
        {
            _definition = null;
            _nextResolveAttempt = 0f;
            ClearBinding();
        }

        private MapSurface ResolveSurface(MapData mapData)
        {
            if (_definition != mapData.Definition)
            {
                ResetFor(mapData.Definition);
            }

            NativeMapBinding? cachedBinding = _binding;
            if (cachedBinding != null && cachedBinding.IsAlive)
            {
                return cachedBinding.Surface;
            }

            ClearBinding();
            if (Time.unscaledTime < _nextResolveAttempt)
            {
                return MapSurface.Radar;
            }

            _nextResolveAttempt = Time.unscaledTime + ResolveRetryDelay;
            GUI_Map? nativeMap = FindNativeMap(mapData.Definition);
            if (nativeMap == null)
            {
                return MapSurface.Radar;
            }

            NativeMapBinding? resolvedBinding = ReadBinding(nativeMap);
            if (resolvedBinding == null)
            {
                return MapSurface.Radar;
            }

            _binding = resolvedBinding;
            return resolvedBinding.Surface;
        }

        private void ResetFor(MapDefinition definition)
        {
            _definition = definition;
            _nextResolveAttempt = 0f;
            ClearBinding();
        }

        private void ClearBinding()
        {
            _binding = null;
        }

        private static GUI_Map? FindNativeMap(MapDefinition definition)
        {
            GUIGameHandler? guiHandler = ProviderAccess.GetGUIGameHandler();
            GUI_Window_GameMenu? gameMenu = guiHandler?.GetPlayerGamePanel();
            GUI_GameMenu_MapPanel? mapPanel = FindMapPanel(gameMenu);
            if (mapPanel == null)
            {
                return null;
            }

            if (!EnsureMapPanelInitialized(gameMenu!, mapPanel))
            {
                return null;
            }

            var mapsByDefinition = mapPanel._definitionToMapGui;
            if (mapsByDefinition == null
                || !mapsByDefinition.TryGetValue(definition, out var buttonMapData)
                || buttonMapData == null)
            {
                return null;
            }

            return buttonMapData.Map;
        }

        private static GUI_GameMenu_MapPanel? FindMapPanel(GUI_Window_GameMenu? gameMenu)
        {
            if (gameMenu == null
                || !gameMenu.TryGetPanelWithId((int)GUI_Window_GameMenu.ECategories.Map, out var mapTab)
                || mapTab?.Instance == null)
            {
                return null;
            }

            return mapTab.Instance.TryCast<GUI_GameMenu_MapPanel>();
        }

        private static bool EnsureMapPanelInitialized(
            GUI_Window_GameMenu gameMenu,
            GUI_GameMenu_MapPanel mapPanel)
        {
            if (mapPanel._definitionToMapGui != null)
            {
                return true;
            }

            List<Behaviour> disabledBehaviours = [];
            bool gameMenuWasActive = gameMenu.gameObject.activeSelf;
            bool mapPanelWasActive = mapPanel.gameObject.activeSelf;

            try
            {
                Disable(gameMenu, disabledBehaviours);
                Disable(mapPanel, disabledBehaviours);
                foreach (GUI_Map map in mapPanel.GetComponentsInChildren<GUI_Map>(true))
                {
                    if (map != null)
                    {
                        Disable(map, disabledBehaviours);
                    }
                }

                if (!gameMenuWasActive)
                {
                    gameMenu.gameObject.SetActive(true);
                }

                if (!mapPanelWasActive)
                {
                    mapPanel.gameObject.SetActive(true);
                }
            }
            finally
            {
                if (!mapPanelWasActive && mapPanel.gameObject.activeSelf)
                {
                    mapPanel.gameObject.SetActive(false);
                }

                if (!gameMenuWasActive && gameMenu.gameObject.activeSelf)
                {
                    gameMenu.gameObject.SetActive(false);
                }

                for (int i = disabledBehaviours.Count - 1; i >= 0; i--)
                {
                    Behaviour behaviour = disabledBehaviours[i];
                    if (behaviour != null)
                    {
                        behaviour.enabled = true;
                    }
                }
            }

            return mapPanel._definitionToMapGui != null;
        }

        private static void Disable(Behaviour behaviour, List<Behaviour> disabledBehaviours)
        {
            if (!behaviour.enabled)
            {
                return;
            }

            behaviour.enabled = false;
            disabledBehaviours.Add(behaviour);
        }

        private static NativeMapBinding? ReadBinding(GUI_Map nativeMap)
        {
            RectTransform? mapArea = nativeMap.MapArea;
            if (mapArea == null)
            {
                return null;
            }

            // MapArea is Drova's marker container. Its parent Image or RawImage is
            // the terrain artwork; children are markers and must never be copied.
            Image? parentImage = mapArea.parent?.GetComponent<Image>();
            if (parentImage?.sprite != null)
            {
                return CreateSpriteBinding(nativeMap, parentImage, mapArea);
            }

            Image? mapAreaImage = mapArea.GetComponent<Image>();
            if (mapAreaImage?.sprite != null)
            {
                return CreateSpriteBinding(nativeMap, mapAreaImage, mapArea);
            }

            RawImage? parentRawImage = mapArea.parent?.GetComponent<RawImage>();
            if (parentRawImage?.texture != null)
            {
                return CreateTextureBinding(nativeMap, parentRawImage, mapArea);
            }

            RawImage? mapAreaRawImage = mapArea.GetComponent<RawImage>();
            if (mapAreaRawImage?.texture != null)
            {
                return CreateTextureBinding(nativeMap, mapAreaRawImage, mapArea);
            }

            return null;
        }

        private static bool HasUsableLayout(
            RectTransform visualRectTransform,
            RectTransform mapArea)
        {
            Rect visualRect = visualRectTransform.rect;
            Rect mapAreaRect = mapArea.rect;
            return visualRect.width > 0f
                   && visualRect.height > 0f
                   && mapAreaRect.width > 0f
                   && mapAreaRect.height > 0f;
        }

        private static NativeMapBinding? CreateSpriteBinding(
            GUI_Map nativeMap,
            Image image,
            RectTransform mapArea)
        {
            if (!HasUsableLayout(image.rectTransform, mapArea))
            {
                return null;
            }

            MapSurface surface = MapSurface.FromSprite(
                image.sprite!,
                image.color,
                GetAspectRatio(image.rectTransform, image.sprite.texture),
                CaptureCalibration(image.rectTransform, mapArea));
            return new NativeMapBinding(nativeMap, image, mapArea, surface);
        }

        private static NativeMapBinding? CreateTextureBinding(
            GUI_Map nativeMap,
            RawImage rawImage,
            RectTransform mapArea)
        {
            if (!HasUsableLayout(rawImage.rectTransform, mapArea))
            {
                return null;
            }

            MapSurface surface = MapSurface.FromTexture(
                rawImage.texture!,
                rawImage.color,
                GetAspectRatio(rawImage.rectTransform, rawImage.texture),
                CaptureCalibration(rawImage.rectTransform, mapArea));
            return new NativeMapBinding(nativeMap, rawImage, mapArea, surface);
        }

        private static MapCoordinateCalibration CaptureCalibration(
            RectTransform visualRectTransform,
            RectTransform mapArea)
        {
            Rect visualRect = visualRectTransform.rect;
            Rect mapAreaRect = mapArea.rect;
            if (visualRect.width <= 0f || visualRect.height <= 0f
                || mapAreaRect.width <= 0f || mapAreaRect.height <= 0f)
            {
                return MapCoordinateCalibration.UnitSquare;
            }

            Vector3 areaMinInVisual = visualRectTransform.InverseTransformPoint(
                mapArea.TransformPoint(new Vector3(mapAreaRect.xMin, mapAreaRect.yMin, 0f)));
            Vector3 areaMaxInVisual = visualRectTransform.InverseTransformPoint(
                mapArea.TransformPoint(new Vector3(mapAreaRect.xMax, mapAreaRect.yMax, 0f)));

            Vector2 visualMin = visualRect.min;
            Vector2 origin = new(
                (areaMinInVisual.x - visualMin.x) / visualRect.width,
                (areaMinInVisual.y - visualMin.y) / visualRect.height);
            Vector2 size = new(
                (areaMaxInVisual.x - areaMinInVisual.x) / visualRect.width,
                (areaMaxInVisual.y - areaMinInVisual.y) / visualRect.height);
            return new MapCoordinateCalibration(origin, size);
        }

        private static float GetAspectRatio(RectTransform rectTransform, Texture texture)
        {
            if (rectTransform.rect.height > 0f && rectTransform.rect.width > 0f)
            {
                return rectTransform.rect.width / rectTransform.rect.height;
            }

            return texture.height > 0 ? (float)texture.width / texture.height : 1f;
        }

        private sealed class NativeMapBinding
        {
            private readonly GUI_Map _map;
            private readonly RectTransform _mapArea;
            private readonly Image? _image;
            private readonly RawImage? _rawImage;
            private readonly Sprite? _sprite;
            private readonly Texture? _texture;
            private readonly Color _color;

            internal NativeMapBinding(
                GUI_Map map,
                Image image,
                RectTransform mapArea,
                MapSurface surface)
            {
                _map = map;
                _mapArea = mapArea;
                _image = image;
                _sprite = image.sprite;
                _color = image.color;
                Surface = surface;
            }

            internal NativeMapBinding(
                GUI_Map map,
                RawImage rawImage,
                RectTransform mapArea,
                MapSurface surface)
            {
                _map = map;
                _mapArea = mapArea;
                _rawImage = rawImage;
                _texture = rawImage.texture;
                _color = rawImage.color;
                Surface = surface;
            }

            internal MapSurface Surface { get; }
            internal bool IsAlive => _map
                                     && _mapArea
                                     && _map.MapArea == _mapArea
                                     && IsSourceCurrent();

            private bool IsSourceCurrent()
            {
                if (_image != null)
                {
                    return _image
                           && _image.sprite == _sprite
                           && _image.color == _color;
                }

                return _rawImage != null
                       && _rawImage
                       && _rawImage.texture == _texture
                       && _rawImage.color == _color;
            }
        }
    }
}
