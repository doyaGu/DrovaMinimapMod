using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Owns every dependency on Drova's original map UI and produces stable
    /// visual and coordinate data for a single minimap frame.
    /// </summary>
    internal sealed class NativeMapPresentation
    {
        private readonly DirectMapTextureProvider _visualBinding = new();
        private readonly MapCoordinateTransform _coordinateTransform = new();

        internal NativeMapPresentationState Refresh(MapData mapData, Vector2 playerWorldPosition)
        {
            bool hasNativeVisual = _visualBinding.TryResolve(mapData);
            _coordinateTransform.UpdateFromNativeMap(
                mapData,
                _visualBinding.NativeMap,
                _visualBinding.NativeGraphic);
            return new NativeMapPresentationState(
                mapData.Definition,
                hasNativeVisual,
                _visualBinding.Sprite,
                _visualBinding.Texture,
                _visualBinding.Color,
                _visualBinding.AspectRatio,
                _coordinateTransform.PlayerWorldToVisual(mapData, playerWorldPosition),
                _coordinateTransform.Projection);
        }

        internal void Reset()
        {
            _visualBinding.Reset();
            _coordinateTransform.Reset();
        }
    }

    /// <summary>
    /// Immutable native-map data consumed by one minimap frame.
    /// </summary>
    internal readonly struct NativeMapPresentationState
    {
        private readonly MapDefinition _definition;
        private readonly MapProjection _projection;

        internal NativeMapPresentationState(
            MapDefinition definition,
            bool hasNativeVisual,
            Sprite? sprite,
            Texture? texture,
            Color color,
            float aspectRatio,
            Vector2 playerPosition,
            MapProjection projection)
        {
            _definition = definition;
            HasNativeVisual = hasNativeVisual;
            Sprite = sprite;
            Texture = texture;
            Color = color;
            AspectRatio = aspectRatio;
            PlayerPosition = playerPosition;
            _projection = projection;
        }

        internal bool HasNativeVisual { get; }
        internal Sprite? Sprite { get; }
        internal Texture? Texture { get; }
        internal Color Color { get; }
        internal float AspectRatio { get; }
        internal Vector2 PlayerPosition { get; }

        internal Vector2 WorldToVisual(Vector2 worldPosition)
        {
            return _projection.WorldToVisual(_definition, worldPosition);
        }
    }
}
