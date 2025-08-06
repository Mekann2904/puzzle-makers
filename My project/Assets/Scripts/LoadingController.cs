using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Sliderを使う場合などに必要

public class LoadingController : MonoBehaviour
{
    // ▼▼▼ 以下の行は、もしローディングバーUIを追加する場合に使います ▼▼▼
    // public Slider loadingSlider; 

    private GeminiImageGenerator imageGenerator;

    void Start()
    {
        // LevelManagerの設定に応じて処理を分岐
        if (LevelManager.useAIGeneration)
        {
            // AI生成がオンの場合、画像生成から開始
            StartCoroutine(GenerateImageAndLoadScene());
        }
        else
        {
            // AI生成がオフの場合、デフォルト画像を設定してシーンロードへ
            Texture2D defaultTexture = Resources.Load<Texture2D>("sample"); // デフォルト画像名を指定
            if (defaultTexture != null)
            {
                LevelManager.generatedPuzzleImage = defaultTexture;
                StartCoroutine(LoadSceneAsync());
            }
            else
            {
                Debug.LogError("デフォルト画像が見つかりません。");
                SceneManager.LoadScene("TilesRoot"); // エラー時は直接移動
            }
        }
    }

    // AI画像生成を行ってから、非同期でシーンをロードするコルーチン
    IEnumerator GenerateImageAndLoadScene()
    {
        imageGenerator = gameObject.AddComponent<GeminiImageGenerator>();
        Texture2D generatedTexture = null;

        // AI画像生成（完了するまで待つ）
        yield return imageGenerator.GenerateImage(LevelManager.aiPrompt, (Texture2D result) =>
        {
            generatedTexture = result;
        });

        if (generatedTexture != null)
        {
            // 成功したら結果をLevelManagerに保存
            LevelManager.generatedPuzzleImage = generatedTexture;
        }
        else
        {
            Debug.LogError("AI画像生成に失敗しました。デフォルト画像を使用します。");
            // 失敗したらデフォルト画像を設定
            LevelManager.generatedPuzzleImage = Resources.Load<Texture2D>("sample");
        }

        // 画像の準備ができたので、非同期シーンロードを開始
        yield return StartCoroutine(LoadSceneAsync());
    }

    // 非同期で次のシーンをロードし、準備が完了したら遷移するコルーチン
    IEnumerator LoadSceneAsync()
    {
        Debug.Log("バックグラウンドでTilesRootシーンのロードを開始します...");

        // バックグラウンドで次のシーンの読み込みを開始
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TilesRoot");

        // シーンの準備ができても、すぐには有効化しないようにする
        asyncLoad.allowSceneActivation = false;

        // シーンの準備が90%完了するまで待つ
        // (Unityでは、allowSceneActivation=falseの場合、準備が完了するとprogressが0.9で止まる仕様)
        while (asyncLoad.progress < 0.9f)
        {
            // loadingSlider.value = asyncLoad.progress; // ローディングバーUIを更新
            yield return null; // 1フレーム待つ
        }

        Debug.Log("TilesRootシーンの準備が完了しました。シーンを有効化します。");
        // loadingSlider.value = 1f; // ローディングバーを100%に

        // シーンを有効化して、画面を切り替える
        asyncLoad.allowSceneActivation = true;
    }
}
