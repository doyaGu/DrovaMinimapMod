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
        private readonly HashSet<uint> _knownMarkerIds = [];

        internal MarkerRenderer(RectTransform markerLayer)
        {
            _markerLayer = markerLayer;
        }

        internal void Refresh(in MinimapFrame frame)
        {
            _knownMarkerIds.Clear();

            for (int i = 0; i < frame.MapData.Markers.Count; i++)
            {
                MapMarker marker = frame.MapData.Markers[i];
                if (marker == null)
                {
                    continue;
                }

                uint markerId = marker.MarkerId;
                _knownMarkerIds.Add(markerId);
                if (!frame.TryGetMarkerContentPosition(marker, out Vector2 markerPosition))
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

        internal void Clear()
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

        internal static void ReleaseSharedResources()
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

        private static void ApplyMarkerAppearance(Image image, MapMarker marker)
        {
            Sprite? markerSprite = marker.MarkerType?.GuiMarkerPrefab?.IconNormalSprite;
            image.sprite = markerSprite ?? GetFallbackIconSprite();
            image.color = markerSprite == null ? new Color(0.5f, 0.9f, 1f, 1f) : Color.white;
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
