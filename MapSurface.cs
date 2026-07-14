using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// The single terrain source a minimap frame can render.
    /// </summary>
    internal enum MapSurfaceKind
    {
        Radar,
        Sprite,
        Texture,
    }

    /// <summary>
    /// Rendering-neutral terrain artwork and the marker space aligned to it.
    /// </summary>
    internal readonly struct MapSurface
    {
        private readonly Sprite? _sprite;
        private readonly Texture? _texture;
        private readonly MapCoordinateCalibration _calibration;

        private MapSurface(
            MapSurfaceKind kind,
            Sprite? sprite,
            Texture? texture,
            Color color,
            float aspectRatio,
            MapCoordinateCalibration calibration)
        {
            Kind = kind;
            _sprite = sprite;
            _texture = texture;
            Color = color;
            AspectRatio = aspectRatio;
            _calibration = calibration;
        }

        internal static MapSurface Radar { get; } = new(
            MapSurfaceKind.Radar,
            sprite: null,
            texture: null,
            color: Color.white,
            aspectRatio: 1f,
            calibration: MapCoordinateCalibration.UnitSquare);

        internal MapSurfaceKind Kind { get; }
        internal Color Color { get; }
        internal float AspectRatio { get; }

        internal Sprite Sprite => _sprite
            ?? throw new InvalidOperationException("This map surface does not contain a sprite.");

        internal Texture Texture => _texture
            ?? throw new InvalidOperationException("This map surface does not contain a texture.");

        internal static MapSurface FromSprite(
            Sprite sprite,
            Color color,
            float aspectRatio,
            MapCoordinateCalibration calibration)
        {
            return new MapSurface(MapSurfaceKind.Sprite, sprite, null, color, aspectRatio, calibration);
        }

        internal static MapSurface FromTexture(
            Texture texture,
            Color color,
            float aspectRatio,
            MapCoordinateCalibration calibration)
        {
            return new MapSurface(MapSurfaceKind.Texture, null, texture, color, aspectRatio, calibration);
        }

        internal Vector2 ToVisualPosition(Vector2 normalizedMapPosition)
        {
            return _calibration.ToVisualPosition(normalizedMapPosition);
        }
    }

    /// <summary>
    /// Immutable original-map result consumed by one minimap frame.
    /// </summary>
    internal readonly struct MapPresentation
    {
        private readonly MapDefinition _definition;

        internal MapPresentation(
            MapDefinition definition,
            MapSurface surface,
            Vector2 playerPosition)
        {
            _definition = definition;
            Surface = surface;
            PlayerPosition = WorldToVisual(playerPosition);
        }

        internal MapSurface Surface { get; }
        internal float AspectRatio => Surface.AspectRatio;
        internal Vector2 PlayerPosition { get; }

        internal Vector2 WorldToVisual(Vector2 worldPosition)
        {
            Vector2 mapPosition = _definition.CalcWorldToLocal(worldPosition);
            Vector2 normalizedPosition = new(Mathf.Clamp01(mapPosition.x), Mathf.Clamp01(mapPosition.y));
            return Surface.ToVisualPosition(normalizedPosition);
        }
    }

    /// <summary>
    /// Converts Drova's MapArea marker coordinates into terrain-artwork coordinates.
    /// </summary>
    internal readonly struct MapCoordinateCalibration
    {
        internal static MapCoordinateCalibration UnitSquare { get; } = new(Vector2.zero, Vector2.one);

        private readonly Vector2 _origin;
        private readonly Vector2 _size;

        internal MapCoordinateCalibration(Vector2 origin, Vector2 size)
        {
            _origin = origin;
            _size = size;
        }

        internal Vector2 ToVisualPosition(Vector2 normalizedMapPosition)
        {
            return _origin + Vector2.Scale(normalizedMapPosition, _size);
        }
    }
}
