using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために必要

/// <summary>
/// BoardGenを使って画像を分割し、TileをSpriteとしてシーン上に並べて表示するスクリプト。
/// LevelManagerから渡された画像を元にパズルを生成します。
/// </summary>
public class TilesGen : MonoBehaviour
{
    [Header("画像設定")]
    [Tooltip("Resourcesフォルダ内の画像ファイル名（AI生成を使わない、または失敗した場合のデフォルト）")]
    public string imageFilename = "sample";

    [Header("パズル設定")]
    [Tooltip("行数")]
    public int row = 4;
    [Tooltip("列数")]
    public int col = 4;

    private BoardGen mBoardGen = new BoardGen();
    private List<GameObject> mTileObjects = new List<GameObject>();
    private GameObject hintCanvasObject; // パネルとヒントを管理するCanvas用の変数

    void Start()
    {
        // LevelManagerに設定された行数と列数を読み込む
        this.row = LevelManager.selectedRows;
        this.col = LevelManager.selectedCols;

        // LevelManagerにAIが生成した画像があるか確認
        if (LevelManager.generatedPuzzleImage != null)
        {
            // あれば、その画像を使ってパズルを生成
            Debug.Log("生成済みのAI画像を元にパズルを作成します。");
            LoadAndSplitImage(LevelManager.generatedPuzzleImage);
        }
        else
        {
            // なければ（AIオフまたは生成失敗）、デフォルト画像でパズルを生成
            Debug.Log("デフォルト画像を元にパズルを作成します。");
            LoadAndSplitImage();
        }
    }

    void LoadAndSplitImage()
    {
        // デフォルト画像の読み込み
        Texture2D mTextureOriginal = Resources.Load<Texture2D>(imageFilename);
        if (mTextureOriginal == null)
        {
            Debug.LogError($"画像が見つかりません: {imageFilename}。Resourcesフォルダ内にあるか確認してください。");
            return;
        }
        LoadAndSplitImage(mTextureOriginal);
    }

    void LoadAndSplitImage(Texture2D mTextureOriginal)
    {
        // 既存のオブジェクトをクリア
        foreach (var obj in mTileObjects)
        {
            if (obj != null) Destroy(obj);
        }
        mTileObjects.Clear();

        if (mTextureOriginal == null)
        {
            Debug.LogError("画像がnullです。");
            return;
        }

        // パズルピースの生成
        mBoardGen.CreateJigsawTiles(mTextureOriginal, row, col);

        // ワールドでのタイルの基本サイズ
        float pixelsPerUnit = 100f;
        float worldTileWidth = (float)mTextureOriginal.width / col / pixelsPerUnit;
        float worldTileHeight = (float)mTextureOriginal.height / row / pixelsPerUnit;

        // 画像全体の幅・高さ（ワールド単位）
        float totalWidth = worldTileWidth * col;
        float totalHeight = worldTileHeight * row;

        // offsetを画面中央（原点）に揃える
        Vector3 offset = Vector3.zero;

        // --- ▼▼▼ UI Imageを使ったパネルとヒントの生成 ▼▼▼ ---
        // 1. パネルとヒントを配置するための「World Space Canvas」を生成
        if (hintCanvasObject != null) Destroy(hintCanvasObject);
        hintCanvasObject = new GameObject("HintCanvas");
        hintCanvasObject.transform.parent = this.transform;

        Canvas hintCanvas = hintCanvasObject.AddComponent<Canvas>();
        hintCanvas.renderMode = RenderMode.WorldSpace;
        hintCanvasObject.AddComponent<GraphicRaycaster>();

        // 【重要】Canvasが他のSpriteRendererと正しくソートされるように設定
        hintCanvas.overrideSorting = true; // ソート順を上書きする設定を有効に
        hintCanvas.sortingOrder = -10;     // パズルピース(0以上)よりずっと小さい値を設定し、奥に描画

        // CanvasのRectTransformを設定
        RectTransform canvasRT = hintCanvasObject.GetComponent<RectTransform>();
        // Z座標は物理的な位置を決めるが、描画順はsortingOrderが優先される
        canvasRT.position = new Vector3(offset.x, offset.y, 0f); 
        canvasRT.sizeDelta = new Vector2(totalWidth, totalHeight);
        canvasRT.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // 2. Canvasの子要素として「白色パネル」をImageで生成
        GameObject whitePanelGO = new GameObject("WhitePanel");
        whitePanelGO.transform.parent = hintCanvasObject.transform;
        Image panelImage = whitePanelGO.AddComponent<Image>();
        panelImage.color = Color.white;
        RectTransform panelRT = whitePanelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        panelRT.anchoredPosition = Vector2.zero;

        // 3. Canvasの子要素として「半透明ヒント」をImageで生成
        GameObject hintImageGO = new GameObject("HintImage");
        hintImageGO.transform.parent = hintCanvasObject.transform;
        Image hintImage = hintImageGO.AddComponent<Image>();
        hintImage.sprite = Sprite.Create(
            mTextureOriginal,
            new Rect(0, 0, mTextureOriginal.width, mTextureOriginal.height),
            new Vector2(0.5f, 0.5f)
        );
        hintImage.color = new Color(1f, 1f, 1f, 0.5f);
        RectTransform hintRT = hintImageGO.GetComponent<RectTransform>();
        hintRT.anchorMin = Vector2.zero;
        hintRT.anchorMax = Vector2.one;
        hintRT.sizeDelta = Vector2.zero;
        hintRT.anchoredPosition = Vector2.zero;
        
        // --- ▲▲▲ ここまでが修正箇所 ▲▲▲ ---

        // 生成されたタイルをシーンに配置
        int baseTileWidthPx = mTextureOriginal.width / col;
        int baseTileHeightPx = mTextureOriginal.height / row;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int idx = i * col + j;
                Tile tile = mBoardGen.tiles[idx];
                Texture2D finalCut = tile.finalCut;

                float baseX = j * baseTileWidthPx;
                float baseY = i * baseTileHeightPx;

                float x = -((float)mTextureOriginal.width / 2f) + baseX + tile.tileWidth / 2f;
                float y = ((float)mTextureOriginal.height / 2f) - baseY - tile.tileHeight / 2f;

                float padOffsetX = ((float)tile.padL - (float)tile.padR) / 2f;
                float padOffsetY = ((float)tile.padB - (float)tile.padT) / 2f;
                x -= padOffsetX;
                y -= padOffsetY;

                x /= pixelsPerUnit;
                y /= pixelsPerUnit;

                Vector3 piecePos = new Vector3(x, y, 0);
                GameObject go = new GameObject($"Tile_{i}_{j}");
                go.transform.parent = this.transform;
                go.transform.position = piecePos;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = (row - i) * col + (col - j);
                Sprite sprite = Sprite.Create(
                    finalCut,
                    new Rect(0, 0, finalCut.width, finalCut.height),
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit
                );
                sr.sprite = sprite;

                BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
                collider.size = sr.bounds.size;
                MouseAction action = go.AddComponent<MouseAction>();
                action.correctPosition = go.transform.position;
                action.snapDistance = Mathf.Min(tile.tileWidth, tile.tileHeight) / pixelsPerUnit * 0.5f;

                float pieceWidth = (float)finalCut.width / pixelsPerUnit;
                float pieceHeight = (float)finalCut.height / pixelsPerUnit;
                Rect cellRect = new Rect(
                    piecePos.x - pieceWidth / 2f,
                    piecePos.y - pieceHeight / 2f,
                    pieceWidth,
                    pieceHeight
                );
                Vector3 cellCenter = piecePos;
                action.cellRect = cellRect;
                action.cellCenter = cellCenter;

                mTileObjects.Add(go);
            }
        }

        // ランダムにピースを散らす
        Camera mainCamera = Camera.main;
        float camHeight = mainCamera.orthographicSize * 2;
        float camWidth = camHeight * mainCamera.aspect;
        
        float padding = 1.0f;
        float minX = -camWidth / 2 + padding;
        float maxX = camWidth / 2 - padding;
        float minY = -camHeight / 2 + padding;
        float maxY = camHeight / 2 - padding;
        
        foreach (var obj in mTileObjects)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            
            obj.transform.position = new Vector3(randomX, randomY, 0);
        }
    }

    void Update()
    {
        // 必要ありません。
    }
}