

using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using Newtonsoft.Json.Linq;                 
using Unity.EditorCoroutines.Editor;

public static class ReplicateAPIHandler
{
  
    private const string replicateToken = "r8_WgOZ9U2GRlqO83QidrkoodzebF4aWpf1mOfYZ";
    private const string modelVersion = "d2f53e644a89ff1233e054ab6f36e9f4fe1c0d4752657437d9cfba9ed6f46d03";
    private const string submitUrl = "https://api.replicate.com/v1/predictions";
    private const string saveDir = "Assets/GeneratedModels/";
    private const float pollSeconds = 5f;
    

    public static void GenerateModel(string prompt)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(SubmitJob(prompt));
    }

    
    private static IEnumerator SubmitJob(string prompt)
    {
        JObject body = new JObject
        {
            ["version"] = modelVersion,
            ["input"] = new JObject { ["prompt"] = prompt }
        };

        using (UnityWebRequest req = new UnityWebRequest(submitUrl, "POST"))
        {
            byte[] raw = Encoding.UTF8.GetBytes(body.ToString());
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Token {replicateToken}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Replicate submit error: {req.error}");
                yield break;
            }

            string id = JObject.Parse(req.downloadHandler.text)["id"]?.ToString();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Replicate: id not found!"); yield break;
            }

            Debug.Log($"Replicate job submitted ✔ id={id}");
            EditorCoroutineUtility.StartCoroutineOwnerless(PollJob(id));
        }
    }

    
    private static IEnumerator PollJob(string id)
    {
        string statusUrl = $"{submitUrl}/{id}";

        while (true)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(statusUrl))
            {
                req.SetRequestHeader("Authorization", $"Token {replicateToken}");
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Replicate poll error: {req.error}");
                    yield break;
                }

                JObject json = JObject.Parse(req.downloadHandler.text);
                string status = json["status"]?.ToString();

                if (status == "succeeded")
                {
                    
                    string url = json["output"]?[0]?.ToString();
                    Debug.Log("Replicate job done ✔ downloading model…");
                    EditorCoroutineUtility.StartCoroutineOwnerless(DownloadAndImport(url));
                    yield break;
                }
                if (status == "failed" || status == "canceled")
                {
                    Debug.LogError($"Replicate job ended: {status}");
                    yield break;
                }

                Debug.Log($"Replicate status={status} ▸ wait {pollSeconds}s…");
            }
            yield return new EditorWaitForSeconds(pollSeconds);
        }
    }

    
    private static IEnumerator DownloadAndImport(string url)
    {
        if (string.IsNullOrEmpty(url)) { Debug.LogError("model url empty"); yield break; }
        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

        string localPath = saveDir + Path.GetFileName(url);

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Model download failed: {req.error}"); yield break;
            }
            File.WriteAllBytes(localPath, req.downloadHandler.data);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Model saved ➜ {localPath}");

        yield return ImportGlb(localPath);
    }

    
    private static IEnumerator ImportGlb(string assetPath)
    {
        string full = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));

        var gltf = new GltfImport();
        bool ok = false; bool done = false;

        Task t = Task.Run(async () => { ok = await gltf.Load(full); done = true; });
        while (!done) yield return null;

        if (!ok) { Debug.LogError("glTFast load failed"); yield break; }

        GameObject root = new GameObject(Path.GetFileNameWithoutExtension(assetPath));
        gltf.InstantiateMainScene(root.transform);
        Selection.activeGameObject = root;

        Debug.Log("glTFast import ✔ model in scene");
    }
}

