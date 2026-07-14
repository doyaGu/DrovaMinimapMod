using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DrovaMinimapMod
{
    internal sealed class MinimapView
    {
        private const float Margin = 24f;
        private const float LabelHeight = 24f;

        private static Sprite? _solidSprite;
        private static Sprite? _arrowSprite;
        private static Texture2D? _arrowTexture;

        private readonly Transform _guiRoot;
        private readonly GameObject _canvasRoot;
        private readonly RectTransform _root;
        private readonly CanvasGroup _canvasGroup;
        private readonly RectTransform _viewport;
        private readonly RectTransform _mapContent;
        private readonly RectTransform _markerLayer;
        private readonly Image _mapImage;
        private readonly RawImage _mapRawImage;
        private readonly Image _playerArrow;
        private readonly TextMeshProUGUI _regionLabel;
        private readonly MarkerRenderer _markerRenderer;

        private float _nextMarkerRefreshTime;
        private Vector2 _contentSize;
        private int _appliedSize = -1;
        private float _appliedOpacity = -1f;
        private string _appliedRegionLabel = string.Empty;

        internal MinimapView(Transform guiRoot)
        {
            _guiRoot = guiRoot;
            _canvasRoot = CreateCanvasRoot(guiRoot);
            _root = CreateRect("DrovaMinimapRoot", _canvasRoot.transform);
            _canvasGroup = _root.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            Image frame = _root.gameObject.AddComponent<Image>();
            frame.sprite = GetSolidSprite();
            frame.color = new Color(0.03f, 0.04f, 0.05f, 0.9f);
            frame.raycastTarget = false;

            _viewport = CreateRect("Viewport", _root);
            _viewport.gameObject.AddComponent<RectMask2D>();

            _mapContent = CreateRect("MapContent", _viewport);
            _mapContent.anchorMin = Vector2.zero;
            _mapContent.anchorMax = Vector2.zero;
            _mapContent.pivot = Vector2.zero;

            _mapImage = CreateImage("MapSprite", _mapContent, GetSolidSprite());
            Stretch(_mapImage.rectTransform);
            _mapImage.color = new Color(0.08f, 0.12f, 0.13f, 0.95f);
            _mapImage.raycastTarget = false;

            _mapRawImage = CreateRawImage("MapTexture", _mapContent);
            Stretch(_mapRawImage.rectTransform);
            _mapRawImage.raycastTarget = false;
            _mapRawImage.gameObject.SetActive(false);

            _markerLayer = CreateRect("Markers", _mapContent);
            Stretch(_markerLayer);
            _markerRenderer = new MarkerRenderer(_markerLayer);

            _playerArrow = CreateImage("PlayerArrow", _viewport, GetArrowSprite());
            RectTransform arrowRect = _playerArrow.rectTransform;
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(22f, 22f);
            _playerArrow.color = new Color(1f, 0.78f, 0.18f, 1f);
            _playerArrow.raycastTarget = false;

            _regionLabel = CreateText("Region", _root);
            _regionLabel.enableWordWrapping = false;
            _regionLabel.font = TMP_Settings.defaultFontAsset;
            _regionLabel.fontSize = 14f;
            _regionLabel.alignment = TextAlignmentOptions.Center;
            _regionLabel.color = new Color(0.95f, 0.95f, 0.92f, 1f);
            _regionLabel.raycastTarget = false;

            ApplyLayout(MinimapPreferences.Default);
            SetVisible(false);
        }

        internal bool IsAttachedTo(Transform guiRoot)
        {
            return _canvasRoot != null && _canvasRoot && _canvasRoot.transform.parent == guiRoot;
        }

        internal void Render(in MinimapFrame frame)
        {
            ApplyLayout(frame.Preferences);
            _contentSize = frame.ContentSize;
            _mapContent.sizeDelta = _contentSize;
            _markerLayer.sizeDelta = _contentSize;
            _mapContent.anchoredPosition = frame.ContentPosition;
            ApplyMapResource(frame.MapPresentation);

            if (frame.LookDirection.sqrMagnitude > 0.0001f)
            {
                _playerArrow.rectTransform.localEulerAngles = new Vector3(
                    0f,
                    0f,
                    Mathf.Atan2(frame.LookDirection.y, frame.LookDirection.x) * Mathf.Rad2Deg - 90f);
            }

            ApplyRegionLabel(frame.RegionLabel);

            if (Time.unscaledTime >= _nextMarkerRefreshTime)
            {
                _markerRenderer.Refresh(frame);
                _nextMarkerRefreshTime = Time.unscaledTime + 0.25f;
            }
        }

        internal void SetVisible(bool visible)
        {
            if (_root != null && _root && _root.gameObject.activeSelf != visible)
            {
                _root.gameObject.SetActive(visible);
            }
        }

        internal void Dispose()
        {
            _markerRenderer.Clear();
            if (_canvasRoot != null && _canvasRoot)
            {
                UnityEngine.Object.Destroy(_canvasRoot);
            }
        }

        internal static void ReleaseSharedResources()
        {
            DestroyRuntimeResource(_solidSprite);
            DestroyRuntimeResource(_arrowSprite);
            DestroyRuntimeResource(_arrowTexture);
            _solidSprite = null;
            _arrowSprite = null;
            _arrowTexture = null;
            MarkerRenderer.ReleaseSharedResources();
        }

        private void ApplyLayout(MinimapPreferences preferences)
        {
            if (_appliedSize == preferences.Size
                && Mathf.Approximately(_appliedOpacity, preferences.Opacity))
            {
                return;
            }

            _appliedSize = preferences.Size;
            _appliedOpacity = preferences.Opacity;
            Layout(preferences.Size, preferences.Opacity);
        }

        private void Layout(int size, float opacity)
        {
            float rootHeight = size + LabelHeight;
            _root.anchorMin = Vector2.one;
            _root.anchorMax = Vector2.one;
            _root.pivot = Vector2.one;
            _root.sizeDelta = new Vector2(size, rootHeight);
            _root.anchoredPosition = new Vector2(-Margin, -Margin);
            _canvasGroup.alpha = opacity;

            _viewport.anchorMin = new Vector2(0f, 0f);
            _viewport.anchorMax = new Vector2(1f, 1f);
            _viewport.offsetMin = new Vector2(2f, LabelHeight + 2f);
            _viewport.offsetMax = new Vector2(-2f, -2f);

            _regionLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            _regionLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            _regionLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
            _regionLabel.rectTransform.offsetMin = Vector2.zero;
            _regionLabel.rectTransform.offsetMax = new Vector2(0f, LabelHeight);
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            Il2CppReferenceArray<Il2CppSystem.Type> componentTypes = new(1);
            componentTypes[0] = Il2CppType.Of<RectTransform>();
            GameObject gameObject = new(name, componentTypes);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private void ApplyMapResource(MapPresentation mapPresentation)
        {
            MapSurface surface = mapPresentation.Surface;
            switch (surface.Kind)
            {
                case MapSurfaceKind.Sprite:
                    _mapImage.gameObject.SetActive(true);
                    _mapRawImage.gameObject.SetActive(false);
                    _mapImage.sprite = surface.Sprite;
                    _mapImage.color = surface.Color;
                    break;

                case MapSurfaceKind.Texture:
                    _mapImage.gameObject.SetActive(false);
                    _mapRawImage.gameObject.SetActive(true);
                    _mapRawImage.texture = surface.Texture;
                    _mapRawImage.color = surface.Color;
                    break;

                case MapSurfaceKind.Radar:
                    _mapImage.gameObject.SetActive(true);
                    _mapRawImage.gameObject.SetActive(false);
                    _mapImage.sprite = GetSolidSprite();
                    _mapImage.color = new Color(0.08f, 0.12f, 0.13f, 0.95f);
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException(nameof(surface.Kind), surface.Kind, null);
            }
        }

        private void ApplyRegionLabel(string regionLabel)
        {
            if (!string.Equals(_appliedRegionLabel, regionLabel, System.StringComparison.Ordinal))
            {
                _regionLabel.font = FindFontFor(regionLabel);
                _appliedRegionLabel = regionLabel;
            }

            _regionLabel.text = regionLabel;
            _regionLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(regionLabel));
        }

        private TMP_FontAsset FindFontFor(string text)
        {
            TMP_FontAsset currentFont = _regionLabel.font ?? TMP_Settings.defaultFontAsset;
            if (SupportsText(currentFont, text))
            {
                return currentFont;
            }

            foreach (TextMeshProUGUI sourceText in _guiRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                TMP_FontAsset? candidate = sourceText.font;
                if (candidate != null && SupportsText(candidate, text))
                {
                    return candidate;
                }
            }

            foreach (TMP_FontAsset candidate in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (candidate != null && SupportsText(candidate, text))
                {
                    return candidate;
                }
            }

            return currentFont;
        }

        private static bool SupportsText(TMP_FontAsset font, string text)
        {
            foreach (char character in text)
            {
                if (!font.HasCharacter(character, true, false))
                {
                    return false;
                }
            }

            return true;
        }

        private static GameObject CreateCanvasRoot(Transform guiRoot)
        {
            Il2CppReferenceArray<Il2CppSystem.Type> componentTypes = new(2);
            componentTypes[0] = Il2CppType.Of<RectTransform>();
            componentTypes[1] = Il2CppType.Of<Canvas>();
            GameObject canvasRoot = new("DrovaMinimapCanvas", componentTypes);
            canvasRoot.transform.SetParent(guiRoot, false);

            RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            Canvas canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10;

            CanvasScaler canvasScaler = canvasRoot.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            return canvasRoot;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            Image image = rectTransform.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            return image;
        }

        private static RawImage CreateRawImage(string name, Transform parent)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            return rectTransform.gameObject.AddComponent<RawImage>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            rectTransform.gameObject.AddComponent<CanvasRenderer>();
            return rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static Sprite GetSolidSprite()
        {
            if (_solidSprite != null && _solidSprite)
            {
                return _solidSprite;
            }

            Texture2D texture = Texture2D.whiteTexture;
            _solidSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return _solidSprite;
        }

        private static Sprite GetArrowSprite()
        {
            if (_arrowSprite != null && _arrowSprite)
            {
                return _arrowSprite;
            }

            const int textureSize = 16;
            _arrowTexture = new Texture2D(textureSize, textureSize);
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float horizontalDistance = Mathf.Abs(x - ((textureSize - 1) * 0.5f));
                    bool arrowHead = y >= 6 && horizontalDistance <= ((textureSize - 1 - y) * 0.7f);
                    bool arrowBody = y < 7 && horizontalDistance <= 2f;
                    _arrowTexture.SetPixel(x, y, arrowHead || arrowBody ? Color.white : Color.clear);
                }
            }

            _arrowTexture.Apply();
            _arrowSprite = Sprite.Create(
                _arrowTexture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f));
            return _arrowSprite;
        }

        private static void DestroyRuntimeResource(UnityEngine.Object? resource)
        {
            if (resource != null && resource)
            {
                UnityEngine.Object.Destroy(resource);
            }
        }

    }
}
