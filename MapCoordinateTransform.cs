using Il2CppDrova.MapSystem;
using Il2CppDrova.MapSystem.GUI;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Uses the same player-position conversion and visual bounds as Drova's
    /// original map marker. Native UI access is limited to this adapter path.
    /// </summary>
    internal sealed class MapCoordinateTransform
    {
        private const float NativeRetryDelay = 0.25f;

        private MapDefinition? _definition;
        private GUI_Map? _nativeMap;
        private GUI_MapPlayerMarker? _nativePlayerMarker;
        private bool _hasNativePlayerPosition;
        private Vector2 _nativePlayerPosition;
        private Vector2 _visualScale = Vector2.one;
        private Vector2 _visualOffset = Vector2.zero;
        private float _nextNativeUpdateAttempt;

        internal MapProjection Projection => new(_visualOffset, _visualScale);

        internal Vector2 PlayerWorldToVisual(MapData mapData, Vector2 worldPosition)
        {
            EnsureDefinition(mapData.Definition);
            return _hasNativePlayerPosition
                ? Projection.ToVisual(_nativePlayerPosition)
                : Projection.WorldToVisual(mapData.Definition, worldPosition);
        }

        internal void UpdateFromNativeMap(
            MapData mapData,
            GUI_Map? nativeMap,
            RectTransform? nativeGraphic)
        {
            EnsureDefinition(mapData.Definition);
            if (Time.unscaledTime < _nextNativeUpdateAttempt)
            {
                return;
            }

            if (nativeMap == null)
            {
                InvalidateNativeBinding();
                return;
            }

            GUI_MapPlayerMarker? playerMarker = GetPlayerMarker(nativeMap);
            if (playerMarker == null)
            {
                _hasNativePlayerPosition = false;
                return;
            }

            try
            {
                if (!playerMarker.TryCalcNormalizedPlayerPos(mapData, mapData.Definition, out Vector2 normalizedPosition))
                {
                    _hasNativePlayerPosition = false;
                    return;
                }

                _nativePlayerPosition = Clamp(normalizedPosition);
                _hasNativePlayerPosition = true;
                UpdateVisualBounds(playerMarker, nativeGraphic);
            }
            catch (Exception)
            {
                _hasNativePlayerPosition = false;
                _nextNativeUpdateAttempt = Time.unscaledTime + NativeRetryDelay;
            }
        }

        private void EnsureDefinition(MapDefinition definition)
        {
            if (_definition == definition)
            {
                return;
            }

            _definition = definition;
            _nativeMap = null;
            _nativePlayerMarker = null;
            _nextNativeUpdateAttempt = 0f;
            _hasNativePlayerPosition = false;
            _visualScale = Vector2.one;
            _visualOffset = Vector2.zero;
        }

        private GUI_MapPlayerMarker? GetPlayerMarker(GUI_Map nativeMap)
        {
            if (_nativeMap != nativeMap || _nativePlayerMarker == null)
            {
                _nativeMap = nativeMap;
                _nativePlayerMarker = nativeMap.GetComponentInChildren<GUI_MapPlayerMarker>(true);
                _visualScale = Vector2.one;
                _visualOffset = Vector2.zero;
            }

            return _nativePlayerMarker;
        }

        private void InvalidateNativeBinding()
        {
            _nativeMap = null;
            _nativePlayerMarker = null;
            _hasNativePlayerPosition = false;
            _visualScale = Vector2.one;
            _visualOffset = Vector2.zero;
        }

        internal void Reset()
        {
            _definition = null;
            _nativeMap = null;
            _nativePlayerMarker = null;
            _hasNativePlayerPosition = false;
            _nativePlayerPosition = Vector2.zero;
            _visualScale = Vector2.one;
            _visualOffset = Vector2.zero;
            _nextNativeUpdateAttempt = 0f;
        }

        private void UpdateVisualBounds(GUI_MapPlayerMarker playerMarker, RectTransform? sourceGraphic)
        {
            RectTransform? worldGuiArea = playerMarker._worldGuiArea;
            if (sourceGraphic == null || worldGuiArea == null
                || sourceGraphic.rect.width <= 0f || sourceGraphic.rect.height <= 0f
                || worldGuiArea.rect.width <= 0f || worldGuiArea.rect.height <= 0f)
            {
                return;
            }

            Vector2 visualMin = ToSourceNormalized(sourceGraphic, worldGuiArea.TransformPoint(
                new Vector3(worldGuiArea.rect.xMin, worldGuiArea.rect.yMin, 0f)));
            Vector2 visualMax = ToSourceNormalized(sourceGraphic, worldGuiArea.TransformPoint(
                new Vector3(worldGuiArea.rect.xMax, worldGuiArea.rect.yMax, 0f)));
            Vector2 scale = visualMax - visualMin;
            if (scale.x is < 0.1f or > 1.5f || scale.y is < 0.1f or > 1.5f
                || Mathf.Abs(visualMin.x) > 0.5f || Mathf.Abs(visualMin.y) > 0.5f)
            {
                return;
            }

            _visualOffset = visualMin;
            _visualScale = scale;
        }

        private static Vector2 ToSourceNormalized(RectTransform sourceGraphic, Vector3 worldPosition)
        {
            Vector3 localPosition = sourceGraphic.InverseTransformPoint(worldPosition);
            Rect sourceRect = sourceGraphic.rect;
            return new Vector2(
                Mathf.InverseLerp(sourceRect.xMin, sourceRect.xMax, localPosition.x),
                Mathf.InverseLerp(sourceRect.yMin, sourceRect.yMax, localPosition.y));
        }

        private static Vector2 Clamp(Vector2 position)
        {
            return new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
        }

    }

    /// <summary>
    /// Value projection captured from the current native map binding.
    /// </summary>
    internal readonly struct MapProjection
    {
        private readonly Vector2 _visualOffset;
        private readonly Vector2 _visualScale;

        internal MapProjection(Vector2 visualOffset, Vector2 visualScale)
        {
            _visualOffset = visualOffset;
            _visualScale = visualScale;
        }

        internal Vector2 WorldToVisual(MapDefinition definition, Vector2 worldPosition)
        {
            return ToVisual(definition.CalcWorldToLocal(worldPosition));
        }

        internal Vector2 ToVisual(Vector2 normalizedPosition)
        {
            return Clamp(new Vector2(
                _visualOffset.x + (normalizedPosition.x * _visualScale.x),
                _visualOffset.y + (normalizedPosition.y * _visualScale.y)));
        }

        private static Vector2 Clamp(Vector2 position)
        {
            return new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
        }
    }
}
