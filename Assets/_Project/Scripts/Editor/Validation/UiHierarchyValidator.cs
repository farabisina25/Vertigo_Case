using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Vertigo.Wheel.Editor.Validation
{
    /// <summary>
    /// Checks a UI hierarchy against the project UI rules and reports every violation it finds.
    /// </summary>
    public static class UiHierarchyValidator
    {
        public const string BlockerSuffix = "_blocker";
        public const string ValueSuffix = "_value";

        private const string ValueFieldSuffix = "Value";
        private const string ProjectNamespace = "Vertigo.Wheel";
        private const float AspectTolerance = 0.02f;

        private static readonly Regex NamePattern = new Regex("^ui(_[a-z0-9]+)+$");

        [MenuItem("Vertigo/Validate UI Hierarchy", priority = 1)]
        public static void ValidateOpenScenes()
        {
            var issues = new List<UiIssue>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                issues.AddRange(Validate(SceneManager.GetSceneAt(i)));
            }

            if (issues.Count == 0)
            {
                Debug.Log("[UiHierarchyValidator] No UI rule violations found.");
                return;
            }

            foreach (UiIssue issue in issues)
            {
                Debug.LogWarning($"[UiHierarchyValidator] {issue}", issue.Context);
            }

            Debug.LogWarning($"[UiHierarchyValidator] {issues.Count} UI rule violation(s) found.");
        }

        public static List<UiIssue> Validate(Scene scene)
        {
            var issues = new List<UiIssue>();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return issues;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                issues.AddRange(Validate(root));
            }

            return issues;
        }

        public static List<UiIssue> Validate(GameObject root)
        {
            var issues = new List<UiIssue>();

            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (IsRootCanvas(canvas))
                {
                    CheckCanvasScaler(canvas, issues);
                }
            }

            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.GetComponentInParent<Canvas>(true) == null)
                {
                    continue;
                }

                CheckName(rect.gameObject, issues);
                CheckAnimators(rect.gameObject, issues);
            }

            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                CheckRaycastTarget(graphic, issues);
                CheckMaskable(graphic, issues);
            }

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                CheckImage(image, issues);
            }

            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                issues.Add(new UiIssue(UiRule.LegacyText, text, $"{GetPath(text)} uses legacy Text, use TextMeshPro."));
            }

            CheckEditorEvents(root, issues);

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                CheckValueReferences(behaviour, issues);
            }

            return issues;
        }

        private static bool IsRootCanvas(Canvas canvas)
        {
            Transform parent = canvas.transform.parent;
            return parent == null || parent.GetComponentInParent<Canvas>(true) == null;
        }

        private static void CheckCanvasScaler(Canvas canvas, List<UiIssue> issues)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null
                || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.Expand)
            {
                issues.Add(new UiIssue(UiRule.CanvasScaler, canvas,
                    $"{GetPath(canvas)} needs a CanvasScaler with Scale With Screen Size and Expand."));
            }
        }

        private static void CheckName(GameObject go, List<UiIssue> issues)
        {
            if (!NamePattern.IsMatch(go.name))
            {
                issues.Add(new UiIssue(UiRule.Naming, go,
                    $"{GetPath(go.transform)} should be lower snake case starting with 'ui_' (general to specific)."));
            }
        }

        // Animated content must live on a child, never on the object that owns a view or the canvas.
        private static void CheckAnimators(GameObject go, List<UiIssue> issues)
        {
            if (go.GetComponent<Animator>() == null && go.GetComponent<Animation>() == null)
            {
                return;
            }

            if (go.GetComponent<Canvas>() != null || HasProjectBehaviour(go))
            {
                issues.Add(new UiIssue(UiRule.AnimatorOnRoot, go,
                    $"{GetPath(go.transform)} has an animator on a root transform, move it to a child."));
            }
        }

        private static void CheckRaycastTarget(Graphic graphic, List<UiIssue> issues)
        {
            if (!graphic.raycastTarget || NeedsRaycast(graphic))
            {
                return;
            }

            issues.Add(new UiIssue(UiRule.RaycastTarget, graphic,
                $"{GetPath(graphic)} has Raycast Target on but nothing on it takes input."));
        }

        private static bool NeedsRaycast(Graphic graphic)
        {
            if (graphic.name.EndsWith(BlockerSuffix) || graphic.GetComponent<ScrollRect>() != null)
            {
                return true;
            }

            foreach (Selectable selectable in graphic.GetComponents<Selectable>())
            {
                if (selectable.targetGraphic == graphic)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckMaskable(Graphic graphic, List<UiIssue> issues)
        {
            if (!(graphic is MaskableGraphic maskableGraphic))
            {
                return;
            }

            bool insideMask = IsInsideMask(graphic.transform);
            if (maskableGraphic.maskable && !insideMask)
            {
                issues.Add(new UiIssue(UiRule.Maskable, graphic,
                    $"{GetPath(graphic)} is Maskable but no mask is above it."));
            }
            else if (!maskableGraphic.maskable && insideMask)
            {
                issues.Add(new UiIssue(UiRule.Maskable, graphic,
                    $"{GetPath(graphic)} is inside a mask but Maskable is off, it will not be clipped."));
            }
        }

        private static bool IsInsideMask(Transform transform)
        {
            for (Transform current = transform.parent; current != null; current = current.parent)
            {
                if (current.GetComponent<RectMask2D>() != null || current.GetComponent<Mask>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void CheckImage(Image image, List<UiIssue> issues)
        {
            Sprite sprite = image.sprite;
            if (sprite == null)
            {
                return;
            }

            bool hasBorder = sprite.border != Vector4.zero;
            if (hasBorder && image.type == Image.Type.Simple)
            {
                issues.Add(new UiIssue(UiRule.ImageType, image,
                    $"{GetPath(image)} uses bordered sprite '{sprite.name}' as Simple, set it to Sliced."));
                return;
            }

            if (!hasBorder && image.type == Image.Type.Sliced)
            {
                issues.Add(new UiIssue(UiRule.ImageType, image,
                    $"{GetPath(image)} is Sliced but sprite '{sprite.name}' has no border."));
                return;
            }

            if (image.type == Image.Type.Simple && !image.preserveAspect && IsStretched(image.rectTransform.rect.size, sprite.rect.size))
            {
                issues.Add(new UiIssue(UiRule.StretchedImage, image,
                    $"{GetPath(image)} stretches '{sprite.name}', enable Preserve Aspect or match its aspect ratio."));
            }
        }

        private static bool IsStretched(Vector2 rectSize, Vector2 spriteSize)
        {
            if (rectSize.x <= 0f || rectSize.y <= 0f || spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return false;
            }

            float rectAspect = rectSize.x / rectSize.y;
            float spriteAspect = spriteSize.x / spriteSize.y;
            return Mathf.Abs(rectAspect / spriteAspect - 1f) > AspectTolerance;
        }

        // Listeners are added from code; anything serialized in the Inspector is a violation.
        private static void CheckEditorEvents(GameObject root, List<UiIssue> issues)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.onClick.GetPersistentEventCount() > 0)
                {
                    issues.Add(new UiIssue(UiRule.EditorEvents, button,
                        $"{GetPath(button)} has OnClick listeners set in the Inspector."));
                }
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>(true))
            {
                if (toggle.onValueChanged.GetPersistentEventCount() > 0)
                {
                    issues.Add(new UiIssue(UiRule.EditorEvents, toggle,
                        $"{GetPath(toggle)} has OnValueChanged listeners set in the Inspector."));
                }
            }

            foreach (EventTrigger trigger in root.GetComponentsInChildren<EventTrigger>(true))
            {
                issues.Add(new UiIssue(UiRule.EditorEvents, trigger,
                    $"{GetPath(trigger)} uses an EventTrigger, subscribe from code instead."));
            }
        }

        // Fields named "...Value" hold changeable UI, so the referenced object must be named "..._value".
        private static void CheckValueReferences(MonoBehaviour behaviour, List<UiIssue> issues)
        {
            if (behaviour == null || !IsProjectType(behaviour))
            {
                return;
            }

            using (var serializedObject = new SerializedObject(behaviour))
            {
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType != SerializedPropertyType.ObjectReference
                        || !property.name.EndsWith(ValueFieldSuffix)
                        || !(property.objectReferenceValue is Component target))
                    {
                        continue;
                    }

                    if (!target.name.EndsWith(ValueSuffix))
                    {
                        issues.Add(new UiIssue(UiRule.ValueSuffix, target,
                            $"{GetPath(target)} is changed by {behaviour.GetType().Name}.{property.name} and must end with '{ValueSuffix}'."));
                    }
                }
            }
        }

        private static bool HasProjectBehaviour(GameObject go)
        {
            foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
            {
                if (behaviour != null && IsProjectType(behaviour))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsProjectType(Object target)
        {
            string ns = target.GetType().Namespace;
            return ns != null && ns.StartsWith(ProjectNamespace);
        }

        private static string GetPath(Component component)
        {
            return GetPath(component.transform);
        }

        private static string GetPath(Transform transform)
        {
            var builder = new StringBuilder(transform.name);
            for (Transform current = transform.parent; current != null; current = current.parent)
            {
                builder.Insert(0, current.name + "/");
            }

            return builder.ToString();
        }
    }
}
