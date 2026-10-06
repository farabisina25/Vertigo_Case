using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Editor.SceneBuilding
{
    /// <summary>
    /// Creates UI objects with the project defaults: no raycast target or masking unless asked for,
    /// sliced images for bordered sprites and preserved aspect for everything else.
    /// </summary>
    public static class UiFactory
    {
        private const int UiLayer = 5;

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Sprite sprite, bool maskable = false)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.maskable = maskable;

            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            return image;
        }

        /// <summary>A sprite-less, full screen image that blocks clicks to the UI behind it.</summary>
        public static Image CreateBlocker(string name, Transform parent, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            image.maskable = false;
            Stretch(image.rectTransform);
            return image;
        }

        public static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string text,
            float fontSize,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            bool maskable = false)
        {
            var label = CreateRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.alignment = alignment;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.maskable = maskable;
            return label;
        }

        public static Button CreateButton(string name, Transform parent, Sprite sprite)
        {
            Image image = CreateImage(name, parent, sprite);
            image.raycastTarget = true;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            return button;
        }

        public static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            Place(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        }
    }
}
