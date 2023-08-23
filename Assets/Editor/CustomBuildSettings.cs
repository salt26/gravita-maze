using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class CustomBuildSettings : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.StandaloneOSX)
        {
            // macOS build
            PlayerSettings.defaultScreenWidth = 810;
            PlayerSettings.defaultScreenHeight = 1440;
        }
        else
        {
            // default build
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
        }
    }
}
