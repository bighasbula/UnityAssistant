using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using Unity.EditorCoroutines.Editor;

public static class ChatGPTHandler
{
    private const string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string model = "mistralai/devstral-small:free";
    private const string apiKey = "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"; // Store securely!

    public static void SendPrompt(string prompt)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(SendRequestCoroutine(prompt));
    }

    private static IEnumerator SendRequestCoroutine(string prompt)
    {
        string bodyJson = JsonUtility.ToJson(new ChatRequest
        {
            model = model,
            messages = new Message[] {
                new Message { role = "system", content = "You are a Unity C# assistant. Return a complete MonoBehaviour script. No explanation or comments." },
                new Message { role = "user", content = prompt }
            }
        });

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("HTTP-Referer", "https://yourdomain.com"); // OpenRouter requires this!

            yield return request.SendWebRequest();
            Debug.Log("[ChatGPTHandler] RAW JSON:\n" + request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OpenRouter request failed: " + request.error);
            }
            else
            {
                string response = request.downloadHandler.text;
                string code = ChatGPTParser.ExtractCodeFromJson(response);
                SaveScriptToFile(code);
                Debug.Log("[ChatGPTHandler] Generated Code:\n" + code);
            }
        }

    }


    private static void SaveScriptToFile(string code)
    {
       
        string className = ExtractClassName(code);
        if (string.IsNullOrEmpty(className))
        {
            className = "GeneratedScript_" + System.DateTime.Now.Ticks;
            Debug.LogWarning("Could not detect class name. Using fallback: " + className);
        }

        
        string folder = "Assets/Scripts/Generated/";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string filePath = folder + className + ".cs";
        File.WriteAllText(filePath, code);
        AssetDatabase.Refresh();

        Debug.Log("✅ Script saved as: " + filePath);
    }

    private static string ExtractClassName(string code)
    {
        var match = System.Text.RegularExpressions.Regex.Match(code, @"\bclass\s+(\w+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public Message[] messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }
}
