using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class SteamBuildWindow : EditorWindow
{
    private static string steamUsername;
    private static string steamPassword;
    private static int appID;
    private static string buildDirectory;
    private static string steamCmdPath;

    private static void Refresh()
    {
        steamUsername = EditorPrefs.GetString("SteamUsername", "");
        steamPassword = EditorPrefs.GetString("SteamPassword", "");
        buildDirectory = EditorPrefs.GetString("BuildDirectory", "Builds");
        appID = EditorPrefs.GetInt("AppID", 0);
        steamCmdPath = EditorPrefs.GetString("SteamCmdPath", @"C:\SteamCMD\steamcmd.exe");
    }


    [MenuItem("File/Build And Upload to Steam")]
    public static void BuildAndUpload()
    {
        Refresh();
        BuildAndUploadInternal();
    }
    
    [MenuItem("Tools/Steam Build Settings")]
    public static void OpenWindow()
    {
        SteamBuildWindow window = GetWindow<SteamBuildWindow>("Steam Build & Upload");
        window.minSize = new Vector2(400, 300);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        GUILayout.Label("Steam Credentials", EditorStyles.boldLabel);
        steamUsername = EditorGUILayout.TextField("Username", steamUsername);
        steamPassword = EditorGUILayout.PasswordField("Password", steamPassword);
        appID = EditorGUILayout.IntField("AppID", appID);
        if (GUILayout.Button("Save Credentials"))
        {
            EditorPrefs.SetString("SteamUsername", steamUsername);
            EditorPrefs.SetString("SteamPassword", steamPassword);
            EditorPrefs.SetInt("AppID", appID);
            UnityEngine.Debug.Log("Steam credentials saved.");
        }

        GUILayout.Space(10);

        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        buildDirectory = EditorGUILayout.TextField("Build Directory", buildDirectory);
        if (GUILayout.Button("Select Build Directory"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Build Directory", buildDirectory, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                buildDirectory = selectedPath;
                EditorPrefs.SetString("BuildDirectory", buildDirectory);
                UnityEngine.Debug.Log("Build directory set to: " + buildDirectory);
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Steam SDK Settings", EditorStyles.boldLabel);
        steamCmdPath = EditorGUILayout.TextField("SteamCMD Path", steamCmdPath);
        if (GUILayout.Button("Select SteamCMD Executable"))
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select SteamCMD Executable",
                                                              Path.GetDirectoryName(steamCmdPath),
                                                              "exe");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                steamCmdPath = selectedPath;
                EditorPrefs.SetString("SteamCmdPath", steamCmdPath);
                UnityEngine.Debug.Log("SteamCMD path set to: " + steamCmdPath);
            }
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Build & Upload"))
        {
            BuildAndUpload();
        }
    }

    private static void BuildAndUploadInternal()
    {
        // Use the enabled scenes from Build Settings.
        var scenes = EditorBuildSettings.scenes
                        .Where(s => s.enabled)
                        .Select(s => s.path)
                        .ToArray();

        // Use the product name from PlayerSettings for the build file name.
        string productName = PlayerSettings.productName;
        // Build the executable path using the selected build directory.
        string buildPath = Path.Combine(buildDirectory, productName + ".exe");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            UploadToSteam(buildPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.LogError("Build failed.");
        }
    }

    private static void UploadToSteam(string buildPath)
    {
        if (!File.Exists(buildPath))
        {
            UnityEngine.Debug.LogError("Build file does not exist: " + buildPath);
            return;
        }

        // Retrieve saved credentials.
        string username = EditorPrefs.GetString("SteamUsername");
        string password = EditorPrefs.GetString("SteamPassword");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            UnityEngine.Debug.LogError("Steam credentials are missing.");
            return;
        }

        // Generate the VDF file using the structure SteamPipe generated.
        string tempVdfPath = GenerateAppBuildVDF(buildPath);
        if (string.IsNullOrEmpty(tempVdfPath))
        {
            UnityEngine.Debug.LogError("Failed to generate the VDF file.");
            return;
        }

        // Use the selected SteamCMD path.

        string logFilePath = Path.Combine(Path.GetDirectoryName(steamCmdPath), "steamcmd.log");
        string arguments = $"-logfile \"{logFilePath}\" +login {username} {password} +run_app_build \"{tempVdfPath}\" +quit";

        UnityEngine.Debug.Log("Executing: " + steamCmdPath + " " + arguments);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = steamCmdPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                UnityEngine.Debug.Log("SteamCMD Output: " + output);
                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.LogError("SteamCMD Error: " + error);
                }

                if (process.ExitCode == 0)
                {
                    UnityEngine.Debug.Log("Upload completed successfully.");
                }
                else
                {
                    UnityEngine.Debug.LogError("Upload failed with exit code: " + process.ExitCode);
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Exception while uploading: " + ex.Message);
        }
        finally
        {
            // Clean up temporary VDF file.
            if (File.Exists(tempVdfPath))
            {
                //File.Delete(tempVdfPath);
            }
        }
    }

    /// <summary>
    /// Generates a temporary VDF file using a template that mimics the structure produced by SteamPipe.
    /// Replace tokens like {BUILD_DESC}, {BUILD_OUTPUT}, and {CONTENT_ROOT} with actual values.
    /// </summary>
    private static string GenerateAppBuildVDF(string buildPath)
    {
        // Updated VDF template with a FileMapping section.
        string vdfTemplate =
        @" ""appbuild""
{
    ""appid""        ""{APP_ID}""
    ""desc""         ""{BUILD_DESC}""
    ""buildoutput""  ""{BUILD_OUTPUT}""
    ""contentroot""  ""{CONTENT_ROOT}""
    ""setlive""      ""public""
    ""depots""
    {
        ""{DEPOT_ID}""
        {
            ""FileMapping""
            {
                ""LocalPath"" ""*""
                ""DepotPath"" "".""
                ""recursive"" ""1""
            }
        }
    }
}";
        // Calculate paths based on the buildPath.
        string buildOutput = Path.GetDirectoryName(Path.GetFullPath(buildPath));
        // Use the build directory as the content root. Make sure your build output contains all necessary files.
        string contentRoot = buildOutput;
        // Create a build description that includes the product name and current timestamp.
        string buildDesc = $"{PlayerSettings.productName} {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        // Replace tokens in the template.
        string vdfContent = vdfTemplate.Replace("{BUILD_DESC}", buildDesc)
                                       .Replace("{BUILD_OUTPUT}", steamCmdPath)
                                       .Replace("{CONTENT_ROOT}", contentRoot)
                                       .Replace("{DEPOT_ID}", (appID + 1).ToString())
                                       .Replace("{APP_ID}", appID.ToString());

        // Write the modified VDF content to a temporary file.
        string tempVdfPath = Path.Combine(Path.GetTempPath(), "app_build_temp.vdf");
        try
        {
            File.WriteAllText(tempVdfPath, vdfContent);
            UnityEngine.Debug.Log("Generated VDF file at: " + tempVdfPath);
            return tempVdfPath;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error writing VDF file: " + ex.Message);
            return null;
        }
    }

}
