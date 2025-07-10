using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoardGenを使って画像を分割し、TileをSpriteとしてシーン上に並べて表示するスクリプト
/// AI画像生成機能付き
/// </summary>
public class TilesGen : MonoBehaviour
{
    [Header("画像設定")]
    [Tooltip("Resourcesフォルダ内の画像ファイル名（AI生成を使わない場合）")]
    public string imageFilename = "sample";
    [Tooltip("AIで画像を生成するかどうか")]
    public bool useAIGeneration = true;
    [Tooltip("AI生成する画像の説明")]
    public string aiPrompt = "美しい風景画、詳細で鮮やかな色彩";
    
    [Header("パズル設定")]
    [Tooltip("行数")]
    public int row = 4;
    [Tooltip("列数")]
    public int col = 4;

    private BoardGen mBoardGen = new BoardGen();
    private List<GameObject> mTileObjects = new List<GameObject>();
    private GameObject frameObject;
    private GeminiImageGenerator imageGenerator;
    private bool isGenerating = false;

    void Start()
    {
        // GeminiImageGeneratorコンポーネントを追加
        imageGenerator = gameObject.GetComponent<GeminiImageGenerator>();
        if (imageGenerator == null)
        {
            imageGenerator = gameObject.AddComponent<GeminiImageGenerator>();
        }
        
        if (useAIGeneration)
        {
            StartCoroutine(GenerateAndLoadImage());
        }
        else
        {
            LoadAndSplitImage();
        }
    }

    IEnumerator GenerateAndLoadImage()
    {
        if (isGenerating)
        {
            Debug.Log("画像生成中です...");
            yield break;
        }
        
        isGenerating = true;
        Debug.Log("AI画像生成を開始: " + aiPrompt);
        
        yield return imageGenerator.GenerateImage(aiPrompt, (Texture2D generatedTexture) =>
        {
            isGenerating = false;
            if (generatedTexture != null)
            {
                Debug.Log("AI画像生成完了。パズルを作成します。");
                LoadAndSplitImage(generatedTexture);
            }
            else
            {
                Debug.LogError("AI画像生成に失敗しました。デフォルト画像を使用します。");
                LoadAndSplitImage();
            }
        });
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

        // 枠の生成
        if (frameObject != null) Destroy(frameObject);
        frameObject = new GameObject("PuzzleFrame");
        frameObject.transform.parent = this.transform;
        frameObject.transform.position = offset;
        SpriteRenderer frameSr = frameObject.AddComponent<SpriteRenderer>();
        Sprite frameSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        frameSr.sprite = frameSprite;
        frameSr.drawMode = SpriteDrawMode.Sliced;
        frameSr.size = new Vector2(totalWidth + 0.1f, totalHeight + 0.1f);
        frameSr.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        frameSr.sortingOrder = -1;

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

                // ベース画像左上座標（ピクセル単位）
                float baseX = j * baseTileWidthPx;
                float baseY = i * baseTileHeightPx;

                // ワールド座標に変換
                float x = -((float)mTextureOriginal.width / 2f) + baseX + tile.tileWidth / 2f;
                float y = ((float)mTextureOriginal.height / 2f) - baseY - tile.tileHeight / 2f;

                // パディング分だけ中心をずらす
                float padOffsetX = ((float)tile.padL - (float)tile.padR) / 2f;
                float padOffsetY = ((float)tile.padB - (float)tile.padT) / 2f;
                // The pivot of the sprite is at the center of the final
                // bounding box which includes padding. To align the original
                // tile area correctly, move the pivot in the opposite
                // direction of the padding difference.
                x -= padOffsetX;
                y -= padOffsetY;

                x /= pixelsPerUnit;
                y /= pixelsPerUnit;

                Vector3 piecePos = new Vector3(x, y, 0);
                GameObject go = new GameObject($"Tile_{i}_{j}");
                go.transform.parent = this.transform;
                go.transform.position = piecePos;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = (row - i) * col + (col - j); // 上・左ほど手前
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
        float scatterRadius = Mathf.Max(totalWidth, totalHeight) * 0.8f;
        foreach (var obj in mTileObjects)
        {
            Vector2 rnd = Random.insideUnitCircle * scatterRadius; // 画面外に出過ぎないよう内側で散らす
            Vector3 randomOffset = new Vector3(rnd.x, rnd.y, 0);
            obj.transform.position += randomOffset;
        }
    }

    void Update()
    {
        // スペースキーで再生成
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (useAIGeneration)
            {
                StartCoroutine(GenerateAndLoadImage());
            }
            else
            {
                LoadAndSplitImage();
            }
        }
        
        // Gキーでデフォルト画像とAI生成画像を切り替え
        if (Input.GetKeyDown(KeyCode.G))
        {
            useAIGeneration = !useAIGeneration;
            Debug.Log("AI生成モード: " + (useAIGeneration ? "ON" : "OFF"));
            
            if (useAIGeneration)
            {
                StartCoroutine(GenerateAndLoadImage());
            }
            else
            {
                LoadAndSplitImage();
            }
        }
    }
}