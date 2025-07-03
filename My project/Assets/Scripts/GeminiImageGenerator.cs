using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class GeminiImageGenerator : MonoBehaviour
{
    [Header("画像生成設定")]
    [Tooltip("生成する画像の説明")]
    public string prompt = "美しい風景画";
    
    public IEnumerator GenerateImage(string imagePrompt, System.Action<Texture2D> onComplete)
    {
        Debug.Log("画像生成を開始: " + imagePrompt);
        
        string apiKey = ApiKeyLoader.LoadGeminiApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("APIキーが見つかりません");
            onComplete?.Invoke(null);
            yield break;
        }

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-preview-image-generation:generateContent?key=" + apiKey;
        
        var requestData = new GeminiRequest
        {
            contents = new Content[]
            {
                new Content
                {
                    parts = new Part[]
                    {
                        new Part { text = imagePrompt }
                    }
                }
            },
            generationConfig = new GenerationConfig
            {
                responseModalities = new string[] { "TEXT", "IMAGE" }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("リクエストデータ: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("画像生成失敗: " + request.error);
                Debug.LogError("レスポンス: " + request.downloadHandler.text);
                onComplete?.Invoke(null);
                yield break;
            }

            Debug.Log("レスポンス: " + request.downloadHandler.text);

            string base64Image = ExtractBase64Image(request.downloadHandler.text);
            if (!string.IsNullOrEmpty(base64Image))
            {
                Texture2D texture = Base64ToTexture2D(base64Image);
                onComplete?.Invoke(texture);
            }
            else
            {
                Debug.LogError("base64画像データの抽出に失敗しました");
                onComplete?.Invoke(null);
            }
        }
    }

    private string ExtractBase64Image(string response)
    {
        try
        {
            var json = JsonUtility.FromJson<GeminiResponse>(response);
            if (json.candidates != null && json.candidates.Length > 0)
            {
                var parts = json.candidates[0].content.parts;
                if (parts != null && parts.Length > 0)
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i].inlineData != null && !string.IsNullOrEmpty(parts[i].inlineData.data))
                        {
                            return parts[i].inlineData.data;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("レスポンス解析エラー: " + e.Message);
        }
        return null;
    }

    private Texture2D Base64ToTexture2D(string base64String)
    {
        try
        {
            byte[] imageBytes = System.Convert.FromBase64String(base64String);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            return texture;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Base64からTexture2Dへの変換エラー: " + e.Message);
            return null;
        }
    }

    [System.Serializable]
    private class GeminiRequest
    {
        public Content[] contents;
        public GenerationConfig generationConfig;
    }
    
    [System.Serializable]
    private class Content
    {
        public Part[] parts;
    }
    
    [System.Serializable]
    private class Part
    {
        public string text;
    }
    
    [System.Serializable]
    private class GenerationConfig
    {
        public string[] responseModalities;
    }

    [System.Serializable]
    private class ResponsePart
    {
        public string text;
        public InlineData inlineData;
    }

    [System.Serializable]
    private class ResponseContent
    {
        public ResponsePart[] parts;
    }

    [System.Serializable]
    private class Candidate
    {
        public ResponseContent content;
    }
    
    [System.Serializable]
    private class InlineData
    {
        public string mimeType;
        public string data;
    }

    [System.Serializable]
    private class GeminiResponse
    {
        public Candidate[] candidates;
    }
}