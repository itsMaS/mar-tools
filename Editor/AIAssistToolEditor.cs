using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Unity.Plastic.Newtonsoft.Json;
using System.IO;

namespace MarTools.GPT
{
    public class AIAssistToolEditor : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, string> _scriptOptions;


        private int _selectedScriptIndex;
        private int _selectedTab;
        private string _gptApiKey;
        private bool _isAuthenticated;

        private static readonly string[] Tabs = { "Custom Inspector Generator", "Other Tools" };

        [MenuItem("MarTools/AI Assist Tool")]
        public static void ShowWindow()
        {
            var window = GetWindow<AIAssistToolEditor>("AI Assist Tool");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            _gptApiKey = EditorPrefs.GetString("MarTools.GPT.ApiKey", string.Empty);
            _isAuthenticated = !string.IsNullOrEmpty(_gptApiKey);
            RefreshMonoBehaviourList();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawAuthentication();

            if (_isAuthenticated)
            {
                DrawTabs();

                EditorGUILayout.BeginVertical("box");

                switch (_selectedTab)
                {
                    case 0:
                        DrawCustomInspectorGenerator();
                        break;
                    case 1:
                        DrawOtherTools();
                        break;
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Please authenticate to access the tools.", MessageType.Warning);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("AI Assist Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("A suite of AI-powered tools to assist with Unity development.", MessageType.Info);
            EditorGUILayout.Space();
        }

        private void DrawAuthentication()
        {
            if (!_isAuthenticated)
            {
                EditorGUILayout.LabelField("Authentication Required", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Enter your GPT API key to access the tools.", MessageType.Info);

                _gptApiKey = EditorGUILayout.PasswordField("API Key", _gptApiKey);

                if (GUILayout.Button("Authenticate", GUILayout.Height(30)))
                {
                    Authenticate();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You are authenticated. API Key is stored locally.", MessageType.Info);
                if (GUILayout.Button("Logout", GUILayout.Height(30)))
                {
                    Logout();
                }
            }
        }

        private void Authenticate()
        {
            if (!string.IsNullOrEmpty(_gptApiKey))
            {
                // Store API key in EditorPrefs for local use only.
                EditorPrefs.SetString("MarTools.GPT.ApiKey", _gptApiKey);
                _isAuthenticated = true;
                Debug.Log("Authentication successful.");
            }
            else
            {
                EditorUtility.DisplayDialog("Authentication Failed", "Please enter a valid API key.", "OK");
            }
        }

        private void Logout()
        {
            EditorPrefs.DeleteKey("MarTools.GPT.ApiKey");
            _gptApiKey = string.Empty;
            _isAuthenticated = false;
            Debug.Log("Logged out successfully.");
        }

        private void DrawTabs()
        {
            _selectedTab = GUILayout.Toolbar(_selectedTab, Tabs, GUILayout.Height(30));
        }

        private void DrawCustomInspectorGenerator()
        {
            EditorGUILayout.LabelField("Custom Inspector Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select a MonoBehaviour script to generate a custom inspector.", MessageType.Info);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_scriptOptions.Count > 0)
            {
                _selectedScriptIndex = EditorGUILayout.Popup("Scripts", _selectedScriptIndex, _scriptOptions.Keys.ToArray());

                if (GUILayout.Button("Generate Custom Inspector", GUILayout.Height(40)))
                {
                    GenerateCustomInspector();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No MonoBehaviour scripts found in the project.", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh Script List", GUILayout.Height(30)))
            {
                RefreshMonoBehaviourList();
            }
        }

        private void DrawOtherTools()
        {
            EditorGUILayout.LabelField("Other Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Additional AI-powered tools will be added here.", MessageType.Info);
            EditorGUILayout.LabelField("Coming Soon!", EditorStyles.wordWrappedLabel);
        }

        private void RefreshMonoBehaviourList()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            _scriptOptions = new Dictionary<string, string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null && script.GetClass()?.IsSubclassOf(typeof(MonoBehaviour)) == true)
                {
                    string namespaceName = script.GetClass().Namespace ?? "Global";
                    string displayName = $"{namespaceName}/{script.name}";
                    _scriptOptions.Add(displayName, path);
                }
            }

            if (_scriptOptions.Count == 0)
            {
                _scriptOptions.Add("No MonoBehaviour scripts found", "");
            }

            _selectedScriptIndex = 0;
        }

        private async void GenerateCustomInspector()
        {
            if (_scriptOptions.Count == 0 || _selectedScriptIndex >= _scriptOptions.Count)
            {
                Debug.LogWarning("No script selected or no scripts available.");
                return;
            }

            string selectedScript = _scriptOptions.Keys.ToArray()[_selectedScriptIndex];
            string scriptName = selectedScript.Split('/').Last();
            string fullScriptPath = _scriptOptions[selectedScript];
            string fullScriptCode = File.ReadAllText(fullScriptPath);

            Debug.Log($"Generating custom inspector for: {selectedScript}");

            try
            {
                string prompt = $"Please generate a custom inspector script for {scriptName} Here is the code for the script:{fullScriptCode} These are some examples on how you should approach creating custom editor for this script:1. the script contains parameters like maxHealth and current health. You would create a health bar that would show these values in the inspector.2. the script parameters are quite messy and maybe some don't need to be displayed once others are. You would make the editor so some parameters are only shown when others are within some ranges or enabled/disabled3. You would make tabs for the parameters for easier readability4. You would add custom handles that would allow user to control some script parameters through scene window interactions5. You would visualize any parameters like range of the enemy in the world through scene viewFeel free to come up with any other things that would help the user use this script.The answer must be THE SCRIPT ONLY (no additional commentary). What you answer back will go straight into the script file.";

                string response = await SendGPTRequest(prompt);

                string inspectorScript = $"{response}";

                // Path to the Editor folder
                string editorFolderPath = Path.Combine(Application.dataPath, "Editor");

                // Ensure the Editor folder exists
                if (!Directory.Exists(editorFolderPath))
                {
                    Directory.CreateDirectory(editorFolderPath);
                }

                // File path to write the script
                string filePath = Path.Combine(editorFolderPath, $"{scriptName}CustomEditor.cs");

                // Write the script content
                File.WriteAllText(filePath, inspectorScript);

                // Refresh the AssetDatabase to show the file in Unity Editor
                AssetDatabase.Refresh();

                Debug.Log("Script written to: " + filePath);

                EditorUtility.DisplayDialog("Custom Inspector Generated", $"Custom inspector for {scriptName} generated successfully!", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating custom inspector: {ex.Message} {ex.StackTrace}");
                EditorUtility.DisplayDialog("Error", "Failed to generate custom inspector. Check the console for details.", "OK");
            }
        }

        private async Task<string> SendGPTRequest(string prompt)
        {
            using (var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_gptApiKey}");
                request.SetRequestHeader("Content-Type", "application/json");

                // Updated request body
                var body = new
                {
                    model = "gpt-4o", // Specify the model
                    messages = new[] { new { role = "user", content = prompt } },
                    max_tokens = 1000,
                    temperature = 1.0,
                    top_p = 1.0,
                    frequency_penalty = 0.0,
                    presence_penalty = 0.0
                };

                string bodyJson = JsonConvert.SerializeObject(body); // Serialize using Newtonsoft.Json
                Debug.Log($"Request Body: {bodyJson}"); // Log the request body for debugging
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"HTTP Error: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}");
                    throw new System.Exception($"HTTP Error: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                Debug.Log($"Response: {request.downloadHandler.text}"); // Log the full response
                var jsonResponse = JsonConvert.DeserializeObject<CompletionResponse>(request.downloadHandler.text);

                if (jsonResponse.choices == null || jsonResponse.choices.Length == 0)
                {
                    throw new System.Exception("API returned no choices.");
                }

                return jsonResponse.choices[0].message.content;
            }
        }

        [System.Serializable]
        class CompletionResponse
        {
            public Choice[] choices { get; set; }

            [System.Serializable]
            public class Choice
            {
                public Message message { get; set; }
            }

            [System.Serializable]
            public class Message
            {
                public string content { get; set; }
            }
        }
    }
}
