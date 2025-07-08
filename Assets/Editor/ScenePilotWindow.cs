using UnityEditor;
using UnityEngine;

public class ScenePilotWindow : EditorWindow
{
    private string prompt = "";
    private Vector2 scrollPos;

    [MenuItem("Tools/Bulatzhan's Assistant")]
    public static void ShowWindow()
    {
        
        GetWindow<ScenePilotWindow>("Bulatzhan's AI");
    }

    

    

    private void OnGUI()
    {
        GUILayout.Label("Bulatzhan's AI Assistant", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        prompt = EditorGUILayout.TextArea(prompt, GUILayout.Height(200));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate Script"))
        {
            Debug.Log("Prompt sent to ChatGPT: " + prompt);
            ChatGPTHandler.SendPrompt(prompt);
        }
        if (GUILayout.Button("Generate 3D Object"))
        {
            Debug.Log("Prompt sent to Meshy AI: " + prompt);

            ReplicateAPIHandler.GenerateModel(prompt);
        }
        
        if (GUILayout.Button("Asset Store"))
        {
            AssetStoreFallback.OpenAssetStoreSearch(prompt);
        }
    }

    
}

