using Il2CppDrova.MapSystem;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.UI;

namespace DrovaMinimapMod
{
    internal sealed class MarkerRenderer
    {
        private const float ViewportPadding = 18f;

        private static Sprite? _fallbackIconSprite;
        private readonly RectTransform _markerLayer;
        private readonly Dictionary<uint, Image> _icons = [];
        private readonly HashSet<uint> _knownMarkerIds = [];

        public MarkerRenderer(RectTransform markerLayer)
        {
            _markerLayer = markerLayer;
        }

        public void Refresh(
            MapData mapData,
            Vector2 contentSize,
            Vector2 contentPosition,
            float viewportSize,
            MinimapSettings settings,
            MapCoordinateTransform coordinateTransform)
        {
            _knownMarkerIds.Clear();

            for (int i = 0; i < mapData.Markers.Count; i++)
            {
                MapMarker marker = mapData.Markers[i];
                if (marker == null || !ShouldShow(marker, settings))
                {
                    continue;
                }

                uint markerId = marker.MarkerId;
                _knownMarkerIds.Add(markerId);
                Vector2 normalizedPosition = coordinateTransform.WorldToVisual(mapData, marker.WorldPos);
                Vector2 markerPosition = new(
                    normalizedPosition.x * contentSize.x,
                    normalizedPosition.y * contentSize.y);
                if (!IsNearViewport(markerPosition, contentPosition, viewportSize))
                {
                    if (_icons.TryGetValue(markerId, out Image? offscreenIcon) && offscreenIcon != null)
                    {
                        offscreenIcon.gameObject.SetActive(false);
                    }

                    continue;
                }

                if (!_icons.TryGetValue(markerId, out Image? icon) || icon == null)
                {
                    icon = CreateIcon(marker);
                    _icons[markerId] = icon;
                }

                ApplyMarkerAppearance(icon, marker);
                RectTransform iconRect = icon.rectTransform;
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.zero;
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = markerPosition;
                icon.gameObject.SetActive(true);
            }

            foreach (KeyValuePair<uint, Image> pair in _icons.ToArray())
            {
                if (_knownMarkerIds.Contains(pair.Key))
                {
                    continue;
                }

                if (pair.Value != null && pair.Value)
                {
                    UnityEngine.Object.Destroy(pair.Value.gameObject);
                }

                _icons.Remove(pair.Key);
            }
        }

        public void Clear()
        {
            foreach (Image image in _icons.Values)
            {
                if (image != null && image)
                {
                    UnityEngine.Object.Destroy(image.gameObject);
                }
            }

            _icons.Clear();
            _knownMarkerIds.Clear();
        }

        public static void ReleaseSharedResources()
        {
            if (_fallbackIconSprite != null && _fallbackIconSprite)
            {
                UnityEngine.Object.Destroy(_fallbackIconSprite);
            }

            _fallbackIconSprite = null;
        }

        private Image CreateIcon(MapMarker marker)
        {
            Il2CppReferenceArray<Il2CppSystem.Type> componentTypes = new(1);
            componentTypes[0] = Il2CppType.Of<RectTransform>();
            GameObject gameObject = new($"Marker_{marker.MarkerId}", componentTypes);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_markerLayer, false);

            Image image = gameObject.AddComponent<Image>();
            ApplyMarkerAppearance(image, marker);
            image.raycastTarget = false;
            rectTransform.sizeDelta = new Vector2(18f, 18f);
            return image;
        }

        private static bool IsNearViewport(Vector2 markerPosition, Vector2 contentPosition, float viewportSize)
        {
            Vector2 viewportPosition = contentPosition + markerPosition;
            return viewportPosition.x >= -ViewportPadding
                && viewportPosition.x <= viewportSize + ViewportPadding
                && viewportPosition.y >= -ViewportPadding
                && viewportPosition.y <= viewportSize + ViewportPadding;
        }

        private static void ApplyMarkerAppearance(Image image, MapMarker marker)
        {
            Sprite? markerSprite = marker.MarkerType?.GuiMarkerPrefab?.IconNormalSprite;
            image.sprite = markerSprite ?? GetFallbackIconSprite();
            image.color = markerSprite == null ? new Color(0.5f, 0.9f, 1f, 1f) : Color.white;
        }

        private static bool ShouldShow(MapMarker marker, MinimapSettings settings)
        {
            return marker.IsNpcMarker ? settings.ShowNpcMarkers : settings.ShowStandardMarkers;
        }

        private static Sprite GetFallbackIconSprite()
        {
            if (_fallbackIconSprite != null && _fallbackIconSprite)
            {
                return _fallbackIconSprite;
            }

            Texture2D texture = Texture2D.whiteTexture;
            _fallbackIconSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return _fallbackIconSprite;
        }
    }
}
