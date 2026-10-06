using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Editor.Validation;

namespace Vertigo.Wheel.Tests
{
    public sealed class UiHierarchyValidatorTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _created)
            {
                Object.DestroyImmediate(go);
            }

            _created.Clear();
        }

        [Test]
        public void ValidHierarchy_HasNoIssues()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            Image image = AddImage("ui_image_background", canvas.transform);
            image.raycastTarget = false;
            image.maskable = false;

            Assert.That(UiHierarchyValidator.Validate(canvas), Is.Empty);
        }

        [Test]
        public void CanvasScaler_NotExpand_IsReported()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight);

            AssertHasIssue(canvas, UiRule.CanvasScaler);
        }

        [Test]
        public void Name_NotSnakeCaseWithPrefix_IsReported()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            CreateChild("SpinButton", canvas.transform);

            AssertHasIssue(canvas, UiRule.Naming);
        }

        [Test]
        public void RaycastTarget_OnDecorativeImage_IsReported()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            Image image = AddImage("ui_image_decoration", canvas.transform);
            image.raycastTarget = true;
            image.maskable = false;

            AssertHasIssue(canvas, UiRule.RaycastTarget);
        }

        [Test]
        public void RaycastTarget_OnButtonGraphic_IsAllowed()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            Image image = AddImage("ui_button_spin", canvas.transform);
            image.raycastTarget = true;
            image.maskable = false;
            image.gameObject.AddComponent<Button>().targetGraphic = image;

            Assert.That(UiHierarchyValidator.Validate(canvas).Select(issue => issue.Rule), Has.No.Member(UiRule.RaycastTarget));
        }

        [Test]
        public void Maskable_WithoutMask_IsReported()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            Image image = AddImage("ui_image_decoration", canvas.transform);
            image.raycastTarget = false;
            image.maskable = true;

            AssertHasIssue(canvas, UiRule.Maskable);
        }

        [Test]
        public void Animator_OnCanvasRoot_IsReported()
        {
            GameObject canvas = CreateCanvas(CanvasScaler.ScreenMatchMode.Expand);
            canvas.AddComponent<Animator>();

            AssertHasIssue(canvas, UiRule.AnimatorOnRoot);
        }

        private GameObject CreateCanvas(CanvasScaler.ScreenMatchMode matchMode)
        {
            var go = new GameObject("ui_canvas_test", typeof(RectTransform));
            _created.Add(go);
            go.AddComponent<Canvas>();

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = matchMode;
            return go;
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image AddImage(string name, Transform parent)
        {
            return CreateChild(name, parent).AddComponent<Image>();
        }

        private static void AssertHasIssue(GameObject root, UiRule rule)
        {
            Assert.That(UiHierarchyValidator.Validate(root).Select(issue => issue.Rule), Has.Member(rule));
        }
    }
}
