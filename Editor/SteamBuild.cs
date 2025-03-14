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
    private static string steamAuthCode;

    private static string buildPath => Path.Combine(buildDirectory, productName + ".exe");
    private static string productName => PlayerSettings.productName;

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

        Color c;
        Color A = ColorUtility.TryParseHtmlString("#171a21", out c) ? c : Color.white;
        Color B = ColorUtility.TryParseHtmlString("#66c0f4", out c) ? c : Color.white;
        Color C = ColorUtility.TryParseHtmlString("#1b2838", out c) ? c : Color.white;
        Color D = ColorUtility.TryParseHtmlString("#2a475e", out c) ? c : Color.white;
        Color E = ColorUtility.TryParseHtmlString("#c7d5e0", out c) ? c : Color.white;

        DrawBackground(A);

        GUI.contentColor = E;
        GUI.backgroundColor = B;


        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 40;
        labelStyle.fontStyle = FontStyle.Bold;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 20;
        buttonStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("Build to Steam Settings", labelStyle);
                

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

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("Make sure to authentificate your Steam account if it's using any 2FA, by clicking this button and following prompts on the console (this action needs to be done once on this computer)", MessageType.Info);
        if (GUILayout.Button("Authentificate"))
        {
            Authenticate();
        }
        EditorGUILayout.Space(10);

        GUILayout.BeginHorizontal();
        buildDirectory = EditorGUILayout.TextField("Build Directory", buildDirectory);
        if (GUILayout.Button("...", GUILayout.Width(20)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Build Directory", buildDirectory, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                buildDirectory = selectedPath;
                EditorPrefs.SetString("BuildDirectory", buildDirectory);
                UnityEngine.Debug.Log("Build directory set to: " + buildDirectory);
            }
        }
        GUILayout.EndHorizontal();

        if(String.IsNullOrEmpty(steamCmdPath))
        {
            string link = @"https://developer.valvesoftware.com/wiki/SteamCMD";
            EditorGUILayout.HelpBox($"No SteamCMD path, you can download it from: {link}", MessageType.Error);
        }

        GUILayout.BeginHorizontal();
        steamCmdPath = EditorGUILayout.TextField("SteamCMD Path", steamCmdPath);
        if (GUILayout.Button("...", GUILayout.Width(20)))
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
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build & Upload", buttonStyle))
        {
            BuildAndUpload();
        }
        if (GUILayout.Button("Upload", buttonStyle))
        {
            UploadToSteam();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawBackground(Color color)
    {
        // Get the full window rect
        Rect windowRect = new Rect(0, 0, position.width, position.height);

        // Set GUI color
        EditorGUI.DrawRect(windowRect, color);
    }

    private static void BuildAndUploadInternal()
    {
        // Use the enabled scenes from Build Settings.
        var scenes = EditorBuildSettings.scenes
                        .Where(s => s.enabled)
                        .Select(s => s.path)
                        .ToArray();

        // Use the product name from PlayerSettings for the build file name.
        // Build the executable path using the selected build directory.

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
            UploadToSteam();
        }
        else if (summary.result == BuildResult.Failed)
        {
            UnityEngine.Debug.LogError("Build failed.");
        }
    }

    private static void UploadToSteam()
    {
        Refresh();

        if (!File.Exists(buildPath))
        {
            UnityEngine.Debug.LogError("Build file does not exist: " + buildPath);
            return;
        }

        // Retrieve saved credentials.
        if (string.IsNullOrEmpty(steamUsername) || string.IsNullOrEmpty(steamPassword))
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

        string arguments = $"+login {steamUsername} {steamPassword} +run_app_build \"{tempVdfPath}\" +quit";

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
                    OpenWindow();
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Exception while uploading: " + ex.Message);
            OpenWindow();
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

    public static void Authenticate()
    {
        Process.Start(steamCmdPath, $"+login {steamUsername} {steamPassword}");
    }
}
