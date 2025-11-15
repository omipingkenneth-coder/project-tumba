using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class PreAndroidBuildCleanup : IPreprocessBuildWithReport
{
    // Set the order to run early in the build pipeline
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Only run this cleanup for Android builds
        if (report.summary.platform == BuildTarget.Android)
        {
            string backupPath = @"D:\TumbangPreso_BackUpThisFolder_ButDontShipItWithYourGame";

            if (Directory.Exists(backupPath))
            {
                try
                {
                    Directory.Delete(backupPath, true);
                    UnityEngine.Debug.Log($"🧹 [PreBuildCleanup] Deleted old IL2CPP backup folder at: {backupPath}");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"⚠️ [PreBuildCleanup] Could not delete backup folder: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"✅ [PreBuildCleanup] No existing backup folder found at: {backupPath}");
            }
        }
    }
}
