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
        private static Sprite? _fallbackIconSprite;
        private readonly RectTransform _markerLayer;
        private readonly Dictionary<uint, Image> _icons = [];
        private readonly HashSet<uint> _visibleMarkerIds = [];

        public MarkerRenderer(RectTransform markerLayer)
        {
            _markerLayer = markerLayer;
        }

        public void Refresh(
            MapData mapData,
            Vector2 contentSize,
            MinimapSettings settings,
            MapCoordinateTransform coordinateTransform)
        {
            _visibleMarkerIds.Clear();

            for (int i = 0; i < mapData.Markers.Count; i++)
            {
                MapMarker marker = mapData.Markers[i];
                if (marker == null || !ShouldShow(marker, settings))
                {
                    continue;
                }

                uint markerId = marker.MarkerId;
                _visibleMarkerIds.Add(markerId);
                if (!_icons.TryGetValue(markerId, out Image? icon) || icon == null)
                {
                    icon = CreateIcon(marker);
                    _icons[markerId] = icon;
                }

                Vector2 normalizedPosition = coordinateTransform.WorldToVisual(mapData, marker.WorldPos);
                RectTransform iconRect = icon.rectTransform;
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.zero;
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = new Vector2(
                    normalizedPosition.x * contentSize.x,
                    normalizedPosition.y * contentSize.y);
                icon.gameObject.SetActive(true);
            }

            foreach (KeyValuePair<uint, Image> pair in _icons.ToArray())
            {
                if (_visibleMarkerIds.Contains(pair.Key))
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
            _visibleMarkerIds.Clear();
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
            image.sprite = marker.MarkerType?.GuiMarkerPrefab?.IconNormalSprite ?? GetFallbackIconSprite();
            image.color = marker.MarkerType?.GuiMarkerPrefab?.IconNormalSprite == null
                ? new Color(0.5f, 0.9f, 1f, 1f)
                : Color.white;
            image.raycastTarget = false;
            rectTransform.sizeDelta = new Vector2(18f, 18f);
            return image;
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
