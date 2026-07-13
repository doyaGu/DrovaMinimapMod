using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Immutable, validated user preferences for one minimap frame.
    /// </summary>
    internal readonly struct MinimapPreferences
    {
        internal const int DefaultSize = 240;
        internal const float DefaultZoom = 2f;
        internal const float DefaultOpacity = 0.85f;

        internal static MinimapPreferences Default { get; } = new(
            enabled: true,
            size: DefaultSize,
            zoom: DefaultZoom,
            opacity: DefaultOpacity,
            showStandardMarkers: true,
            showNpcMarkers: true);

        internal MinimapPreferences(
            bool enabled,
            int size,
            float zoom,
            float opacity,
            bool showStandardMarkers,
            bool showNpcMarkers)
        {
            Enabled = enabled;
            Size = Mathf.Clamp(size, 160, 420);
            Zoom = Mathf.Clamp(zoom, 1f, 4f);
            Opacity = Mathf.Clamp(opacity, 0.4f, 1f);
            ShowStandardMarkers = showStandardMarkers;
            ShowNpcMarkers = showNpcMarkers;
        }

        internal bool Enabled { get; }
        internal int Size { get; }
        internal float Zoom { get; }
        internal float Opacity { get; }
        internal bool ShowStandardMarkers { get; }
        internal bool ShowNpcMarkers { get; }
    }
}
