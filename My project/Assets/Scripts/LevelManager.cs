using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // LevelManagerのインスタンスをどこからでもアクセスできるようにする
    public static LevelManager Instance;

    // 選択された難易度に応じた行数と列数を保持する
    public static int selectedRows = 4; // デフォルト値
    public static int selectedCols = 4; // デフォルト値
    // AI画像生成に関する設定と結果を保持する(Loadingシーンを挟むため)
    public static bool useAIGeneration = true;
    public static string aiPrompt = "美しい風景画、詳細で鮮やかな色彩";
    public static Texture2D generatedPuzzleImage = null; // 生成された画像をここに格納


    private void Awake()
    {
        // シーンを切り替えて LevelManagerが破棄されないようにする
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 難易度を設定してゲームシーンを開始するメソッド
    /// </summary>
    /// <param name="level">0:初級, 1:中級, 2:上級</param>
    public void StartGameWithDifficulty(int level)
    //難易度設定を変更可能
    {
        switch (level)
        {
            case 0: // 初級
                selectedRows = 3;
                selectedCols = 3;
                break;
            case 1: // 中級
                selectedRows = 3;
                selectedCols = 4;
                break;
            case 2: // 上級
                selectedRows = 10;
                selectedCols = 10;
                break;
        }

        // パズルがあるゲームシーンに遷移（シーン名は適宜変更してください）
        SceneManager.LoadScene("LoadingScene");
    }
}