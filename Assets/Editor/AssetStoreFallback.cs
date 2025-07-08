using UnityEngine;
using UnityEngine.Networking;

public static class AssetStoreFallback
{
    public static void OpenAssetStoreSearch(string prompt)
    {
        string query = UnityWebRequest.EscapeURL(prompt.Trim());
        string url = $"https://assetstore.unity.com/?q={query}&orderBy=1";
        Application.OpenURL(url);
        Debug.Log($"🔍 Searching Unity Asset Store for: {prompt}");
    }
}
