using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Immutable, validated user preferences for one minimap frame.
    /// </summary>
    internal readonly struct MinimapPreferences
    {
        internal const int MinimumSize = 160;
        internal const int MaximumSize = 420;
        internal const int DefaultSize = 240;

        internal const float MinimumZoom = 1f;
        internal const float MaximumZoom = 4f;
        internal const float DefaultZoom = 2f;

        internal const int MinimumOpacityPercentage = 40;
        internal const int MaximumOpacityPercentage = 100;
        internal const int DefaultOpacityPercentage = 85;

        internal static MinimapPreferences Default { get; } = new(
            enabled: true,
            size: DefaultSize,
            zoom: DefaultZoom,
            opacityPercentage: DefaultOpacityPercentage,
            showStandardMarkers: true,
            showNpcMarkers: true);

        internal MinimapPreferences(
            bool enabled,
            int size,
            float zoom,
            int opacityPercentage,
            bool showStandardMarkers,
            bool showNpcMarkers)
        {
            Enabled = enabled;
            Size = Mathf.Clamp(size, MinimumSize, MaximumSize);
            Zoom = Mathf.Clamp(zoom, MinimumZoom, MaximumZoom);
            OpacityPercentage = Mathf.Clamp(
                opacityPercentage,
                MinimumOpacityPercentage,
                MaximumOpacityPercentage);
            ShowStandardMarkers = showStandardMarkers;
            ShowNpcMarkers = showNpcMarkers;
        }

        internal bool Enabled { get; }
        internal int Size { get; }
        internal float Zoom { get; }
        internal int OpacityPercentage { get; }
        internal float Opacity => OpacityPercentage / 100f;
        internal bool ShowStandardMarkers { get; }
        internal bool ShowNpcMarkers { get; }
    }
}
