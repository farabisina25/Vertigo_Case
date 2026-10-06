using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Editor.Capture
{
    /// <summary>
    /// Renders the open UI at the target aspect ratios without touching the Game view, applying the same
    /// Expand scaling the CanvasScaler uses on device. Works in edit mode and in play mode.
    /// </summary>
    public static class UiScreenshotCapture
    {
        public const string OutputFolder = "Screenshots";

        private const int UiLayer = 5;
        private static readonly Vector2 FallbackReference = new Vector2(1920f, 1080f);

        private static readonly (string Label, int Width, int Height)[] Resolutions =
        {
            ("20x9", 2400, 1080),
            ("16x9", 1920, 1080),
            ("4x3", 1440, 1080),
        };

        [MenuItem("Vertigo/Capture UI Screenshots", priority = 20)]
        public static void CaptureFromMenu()
        {
            string prefix = EditorApplication.isPlaying ? "play" : "edit";
            List<string> files = CaptureAll(prefix);
            Debug.Log($"[UiScreenshotCapture] Saved {files.Count} screenshot(s) to {Path.GetFullPath(OutputFolder)}.");
        }

        public static List<string> CaptureAll(string prefix)
        {
            Directory.CreateDirectory(OutputFolder);
            var files = new List<string>();

            List<CanvasState> canvases = CollectRootCanvases();
            if (canvases.Count == 0)
            {
                Debug.LogWarning("[UiScreenshotCapture] No root canvas in the open scenes.");
                return files;
            }

            var cameraObject = new GameObject("capture_camera") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                Camera camera = CreateCamera(cameraObject);
                foreach ((string label, int width, int height) in Resolutions)
                {
                    string path = Path.Combine(OutputFolder, $"{prefix}_{label}_{width}x{height}.png");
                    Render(camera, canvases, width, height, path);
                    files.Add(path);
                }
            }
            finally
            {
                foreach (CanvasState state in canvases)
                {
                    state.Restore();
                }

                Object.DestroyImmediate(cameraObject);
            }

            return files;
        }

        private static Camera CreateCamera(GameObject cameraObject)
        {
            Camera main = Camera.main;
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.cullingMask = 1 << UiLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = main != null ? main.backgroundColor : Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.transform.position = new Vector3(0f, 0f, -100f);
            return camera;
        }

        private static void Render(Camera camera, List<CanvasState> canvases, int width, int height, string path)
        {
            Vector2 canvasSize = Vector2.zero;
            foreach (CanvasState state in canvases)
            {
                canvasSize = state.ApplyWorldSpace(camera, width, height);
            }

            Canvas.ForceUpdateCanvases();

            RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.orthographicSize = canvasSize.y * 0.5f;
                camera.aspect = (float)width / height;
                camera.targetTexture = target;
                camera.Render();

                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Object.DestroyImmediate(texture);
            }
        }

        private static List<CanvasState> CollectRootCanvases()
        {
            var states = new List<CanvasState>();
            foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
            {
                if (canvas.isRootCanvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    states.Add(new CanvasState(canvas));
                }
            }

            return states;
        }

        private sealed class CanvasState
        {
            private readonly Canvas _canvas;
            private readonly CanvasScaler _scaler;
            private readonly bool _scalerEnabled;
            private readonly Camera _worldCamera;
            private readonly RectTransform _rect;
            private readonly Vector3 _position;
            private readonly Vector3 _scale;
            private readonly Vector2 _size;

            public CanvasState(Canvas canvas)
            {
                _canvas = canvas;
                _scaler = canvas.GetComponent<CanvasScaler>();
                _scalerEnabled = _scaler != null && _scaler.enabled;
                _worldCamera = canvas.worldCamera;
                _rect = (RectTransform)canvas.transform;
                _position = _rect.position;
                _scale = _rect.localScale;
                _size = _rect.sizeDelta;
            }

            /// <summary>Lays the canvas out as a world space quad the size Expand would give on that screen.</summary>
            public Vector2 ApplyWorldSpace(Camera camera, int width, int height)
            {
                Vector2 reference = _scaler != null ? _scaler.referenceResolution : FallbackReference;
                float scaleFactor = Mathf.Min(width / reference.x, height / reference.y);
                var size = new Vector2(width / scaleFactor, height / scaleFactor);

                if (_scaler != null)
                {
                    _scaler.enabled = false;
                }

                _canvas.renderMode = RenderMode.WorldSpace;
                _canvas.worldCamera = camera;
                _rect.position = Vector3.zero;
                _rect.localScale = Vector3.one;
                _rect.sizeDelta = size;
                return size;
            }

            public void Restore()
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.worldCamera = _worldCamera;
                _rect.position = _position;
                _rect.localScale = _scale;
                _rect.sizeDelta = _size;

                if (_scaler != null)
                {
                    _scaler.enabled = _scalerEnabled;
                }
            }
        }
    }
}
