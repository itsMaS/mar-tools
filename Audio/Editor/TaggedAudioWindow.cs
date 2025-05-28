using MarTools.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Video;

public class TaggedAudioWindow : EditorWindow
{
    // VIDEO RECORDING
    RecorderController recorderController;
    RecorderControllerSettings controllerSettings;
    MovieRecorderSettings movieRecorder;

    int startFrame;
    float startTime;
    bool isRecording = false;

    AudioEventList recordedEvents = new AudioEventList();
    string outputFileBase; // path without extension

    // VIDEO LOADING
    string loadedVideoPath;
    AudioEventList loadedEventList;
    bool videoAndTagsLoaded = false;


    [MenuItem("Tools/Recorder Control")]
    public static void ShowWindow()
    {
        GetWindow<TaggedAudioWindow>("Recorder Control");
    }

    void OnEnable()
    {
        controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        recorderController = new RecorderController(controllerSettings);

        movieRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder.name = "Movie Recorder";
        movieRecorder.Enabled = true;
        movieRecorder.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        movieRecorder.VideoBitRateMode = VideoBitrateMode.High;
        movieRecorder.AudioInputSettings.PreserveAudio = true;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string relativePath = Path.Combine("Recordings", $"recording_{timestamp}");
        outputFileBase = Path.Combine(Directory.GetParent(Application.dataPath).FullName, relativePath);

        movieRecorder.OutputFile = relativePath; // Unity handles extension
        controllerSettings.AddRecorderSettings(movieRecorder);
        controllerSettings.SetRecordModeToManual();
    }

    void OnGUI()
    {
        // VIDEO RECORDING

        if (recorderController.IsRecording())
        {
            if (GUILayout.Button("Stop Recording"))
            {
                recorderController.StopRecording();
                isRecording = false;

                AudioManager.OnAudioEvent -= ListenToAudioEvent;

                WriteMTAFile();
            }
        }
        else
        {
            if (GUILayout.Button("Start Recording"))
            {
                recordedEvents = new AudioEventList();

                recorderController.PrepareRecording();
                recorderController.StartRecording();

                startFrame = Time.frameCount;
                startTime = Time.time;
                isRecording = true;

                AudioManager.OnAudioEvent += ListenToAudioEvent;
            }
        }


        // VIDEO LOADING

        GUILayout.Space(20);
        EditorGUILayout.LabelField("▶️ Playback Loader", EditorStyles.boldLabel);

        if (GUILayout.Button("Load Recorded Video (.mp4)"))
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select Recorded MP4", "Recordings", "mp4");

            if (!string.IsNullOrEmpty(selectedPath))
            {
                string mtaPath = Path.ChangeExtension(selectedPath, ".mta");

                if (!File.Exists(mtaPath))
                {
                    Debug.LogWarning($"No matching .mta file found for video: {mtaPath}");
                    loadedVideoPath = null;
                    loadedEventList = null;
                    videoAndTagsLoaded = false;
                }
                else
                {
                    string json = File.ReadAllText(mtaPath);
                    loadedEventList = JsonUtility.FromJson<AudioEventList>(json);
                    loadedVideoPath = selectedPath;
                    videoAndTagsLoaded = true;

                    Debug.Log($"Loaded video: {loadedVideoPath}\nLoaded tags: {loadedEventList.events.Count} events");
                }
            }
        }

        if (videoAndTagsLoaded)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("▶️ Play Loaded Video"))
            {
                EditorPrefs.SetString("MarTools_Review_VideoPath", loadedVideoPath);
                EditorPrefs.SetString("MarTools_Review_MTAJson", JsonUtility.ToJson(loadedEventList));

                EditorSceneManager.OpenScene("Assets/MarAudio/Runtime/Recorder/VideoPreview.unity");

                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.isPlaying = true;
            }
        }
    }

    private void ListenToAudioEvent(string tag, AudioParameter[] parameters)
    {
        if (!isRecording) return;

        int relativeFrame = Time.frameCount - startFrame;

        recordedEvents.events.Add(new AudioEventData
        {
            eventName = tag,
            frame = relativeFrame
        });

        Debug.Log($"[AudioTag] {tag} @ frame {relativeFrame}");
    }

    private void WriteMTAFile()
    {
        string json = JsonUtility.ToJson(recordedEvents, true);

        string mtaPath = outputFileBase + ".mta";

        Directory.CreateDirectory(Path.GetDirectoryName(mtaPath));
        File.WriteAllText(mtaPath, json);

        Debug.Log($"Audio tag data written to: {mtaPath}");
    }

    void OnPlayModeChanged(PlayModeStateChange state)
    {
        Debug.Log($"Play mode changed {state}");

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            GameObject.FindObjectOfType<RecorderPlayback>().Play(loadedVideoPath);
        }
    }
}
