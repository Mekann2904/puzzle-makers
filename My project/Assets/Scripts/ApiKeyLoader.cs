using System.IO;
using UnityEngine;

public static class ApiKeyLoader
{
    public static string LoadGeminiApiKey()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "apikey.json");
        if (!File.Exists(path))
        {
            Debug.LogError("APIキーファイルが見つかりません: " + path);
            return null;
        }
        
        try
        {
            string json = File.ReadAllText(path);
            var apiKeyData = JsonUtility.FromJson<ApiKeyData>(json);
            
            if (string.IsNullOrEmpty(apiKeyData.GEMINI_API_KEY) || apiKeyData.GEMINI_API_KEY.Contains("ここに"))
            {
                Debug.LogError("APIキーが設定されていません。apikey.jsonファイルに正しいAPIキーを設定してください。");
                return null;
            }
            
            return apiKeyData.GEMINI_API_KEY;
        }
        catch (System.Exception e)
        {
            Debug.LogError("APIキーの読み込みエラー: " + e.Message);
            return null;
        }
    }
    
    [System.Serializable]
    private class ApiKeyData 
    { 
        public string GEMINI_API_KEY; 
    }
} 