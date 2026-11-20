using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class TemporaryAndroidBuild
{
    const string Request = "Temp/android-pokeball-build.request";
    const string Status = "Builds/Android/pokeball-build-status.txt";
    static TemporaryAndroidBuild() => EditorApplication.update += Run;
    static void Run()
    {
        if (!File.Exists(Request)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        try
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                File.WriteAllText(Status, "Switching to Android.");
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                    throw new Exception("Android platform switch failed.");
                return;
            }
            EditorApplication.update -= Run;
            File.Delete(Request);
            var issues = new HashSet<BuildValidationRule>();
            typeof(BuildValidator).GetMethod("GetCurrentValidationIssues", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { issues, BuildTargetGroup.Android });
            File.WriteAllLines("Builds/Android/pokeball-xr-validation.txt", issues.Count == 0
                ? new[] { "Android XR validation: no issues." }
                : issues.Select(issue => (issue.Error ? "ERROR: " : "WARNING: ") + issue.Message));
            if (issues.Any(issue => issue.Error)) throw new Exception("XR validation failed; see validation report.");
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            AssetDatabase.SaveAssets();
            File.WriteAllText(Status, "Building APK.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/AR_Scene.unity" },
                locationPathName = "Builds/Android/AR-Pokeball-Prototype.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            File.WriteAllText(Status, $"{report.summary.result}: {report.summary.totalErrors} errors, {report.summary.totalWarnings} warnings.");
            File.WriteAllLines("Builds/Android/pokeball-build-messages.txt", report.steps.SelectMany(step => step.messages)
                .Where(message => message.type == LogType.Error || message.type == LogType.Exception || message.type == LogType.Warning)
                .Select(message => message.content));
        }
        catch (Exception e)
        {
            if (File.Exists(Request)) File.Delete(Request);
            File.WriteAllText(Status, "Failed: " + e);
            Debug.LogException(e);
        }
    }
}
