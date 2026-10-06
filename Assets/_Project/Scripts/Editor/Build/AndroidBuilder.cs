using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Vertigo.Wheel.Editor.SceneBuilding;

namespace Vertigo.Wheel.Editor.Build
{
    /// <summary>
    /// Applies the release player settings and builds a landscape, IL2CPP ARM64 APK.
    /// </summary>
    public static class AndroidBuilder
    {
        public const string OutputPath = "Builds/Android/VertigoWheel.apk";

        private const string CompanyName = "Sina";
        private const string ProductName = "Vertigo Wheel";
        private const string ApplicationId = "com.farabisina.vertigowheel";
        private const string Version = "1.0.0";
        private const int VersionCode = 1;

        [MenuItem("Vertigo/Build Android APK", priority = 40)]
        public static void BuildFromMenu()
        {
            BuildReport report = Build();
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidBuilder] APK built: {Path.GetFullPath(OutputPath)} ({summary.totalSize / (1024f * 1024f):F1} MB).");
                EditorUtility.RevealInFinder(OutputPath);
            }
            else
            {
                Debug.LogError($"[AndroidBuilder] Build {summary.result} with {summary.totalErrors} error(s).");
            }
        }

        public static BuildReport Build()
        {
            ApplyPlayerSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Builds");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { GameSceneBuilder.ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            return BuildPipeline.BuildPlayer(options);
        }

        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationId);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.renderOutsideSafeArea = false;

            EditorUserBuildSettings.buildAppBundle = false;
            AssetDatabase.SaveAssets();
        }
    }
}
