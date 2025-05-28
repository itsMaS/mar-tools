using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace MarTools.Audio
{
    [RequireComponent(typeof(VideoPlayer))]
    public class RecorderPlayback : MonoBehaviour
    {
        VideoPlayer videoPlayer;
        AudioEventList eventList;

        public void Play(string videoUrl)
        {
            videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.url = videoUrl;

            // Load .mta file
            string mtaPath = Path.ChangeExtension(videoUrl, ".mta");
            if (File.Exists(mtaPath))
            {
                string json = File.ReadAllText(mtaPath);
                eventList = JsonUtility.FromJson<AudioEventList>(json);
                Debug.Log($"[RecorderPlayback] Loaded {eventList.events.Count} tag events.");
            }
            else
            {
                Debug.LogWarning($"[RecorderPlayback] .mta file not found at: {mtaPath}");
            }

            videoPlayer.Play();
        }

        private void Update()
        {
            if(!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();

            var consumed = eventList.ConsumeEvents(videoPlayer.frame);

            foreach (var item in consumed)
            {
                AudioManager.PlayAudioEvent(item.eventName);
            }

        }
    }
}
