using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Immutable render input. It owns per-frame layout and marker projection
    /// so the view only creates and updates UI objects.
    /// </summary>
    internal readonly struct MinimapFrame
    {
        private const float ViewportInset = 4f;
        private const float MarkerViewportPadding = 18f;

        internal MinimapFrame(
            MapData mapData,
            MapPresentation mapPresentation,
            Vector2 lookDirection,
            string regionLabel,
            MinimapPreferences preferences)
        {
            MapData = mapData;
            MapPresentation = mapPresentation;
            LookDirection = lookDirection;
            RegionLabel = regionLabel;
            Preferences = preferences;
            ViewportSize = preferences.Size - ViewportInset;
            ContentSize = CalculateContentSize(ViewportSize, preferences.Zoom, mapPresentation.AspectRatio);
            ContentPosition = new Vector2(
                (ViewportSize * 0.5f) - (mapPresentation.PlayerPosition.x * ContentSize.x),
                (ViewportSize * 0.5f) - (mapPresentation.PlayerPosition.y * ContentSize.y));
        }

        internal MapData MapData { get; }
        internal MapPresentation MapPresentation { get; }
        internal Vector2 LookDirection { get; }
        internal string RegionLabel { get; }
        internal MinimapPreferences Preferences { get; }
        internal float ViewportSize { get; }
        internal Vector2 ContentSize { get; }
        internal Vector2 ContentPosition { get; }

        internal bool TryGetMarkerContentPosition(MapMarker marker, out Vector2 markerPosition)
        {
            Vector2 normalizedPosition = MapPresentation.WorldToVisual(marker.WorldPos);
            markerPosition = new Vector2(
                normalizedPosition.x * ContentSize.x,
                normalizedPosition.y * ContentSize.y);
            Vector2 viewportPosition = ContentPosition + markerPosition;
            return viewportPosition.x >= -MarkerViewportPadding
                && viewportPosition.x <= ViewportSize + MarkerViewportPadding
                && viewportPosition.y >= -MarkerViewportPadding
                && viewportPosition.y <= ViewportSize + MarkerViewportPadding;
        }

        private static Vector2 CalculateContentSize(float viewportSize, float zoom, float aspectRatio)
        {
            float largestDimension = viewportSize * zoom;
            if (aspectRatio >= 1f)
            {
                return new Vector2(largestDimension, largestDimension / aspectRatio);
            }

            return new Vector2(largestDimension * aspectRatio, largestDimension);
        }
    }
}
