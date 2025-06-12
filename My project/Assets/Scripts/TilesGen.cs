using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoardGenを使って画像を分割し、TileをSpriteとしてシーン上に並べて表示するスクリプト
/// </summary>
public class TilesGen : MonoBehaviour
{
    [Tooltip("Resourcesフォルダ内の画像ファイル名")]
    public string imageFilename = "sample";
    [Tooltip("行数")]
    public int row = 4;
    [Tooltip("列数")]
    public int col = 4;

    private BoardGen mBoardGen = new BoardGen();
    private List<GameObject> mTileObjects = new List<GameObject>();
    private GameObject frameObject;

    void Start()
    {
        LoadAndSplitImage();
    }

    void LoadAndSplitImage()
    {
        // 既存のオブジェクトをクリア
        foreach (var obj in mTileObjects)
        {
            if (obj != null) Destroy(obj);
        }
        mTileObjects.Clear();
        MouseAction.totalPieces = 0;
        MouseAction.snappedPieces = 0;

        // 画像の読み込み
        Texture2D mTextureOriginal = Resources.Load<Texture2D>(imageFilename);
        if (mTextureOriginal == null)
        {
            Debug.LogError($"画像が見つかりません: {imageFilename}。Resourcesフォルダ内にあるか確認してください。");
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

        // 枠の生成
        if (frameObject != null) Destroy(frameObject);
        frameObject = new GameObject("PuzzleFrame");
        frameObject.transform.parent = this.transform;
        frameObject.transform.position = Vector3.zero;
        SpriteRenderer frameSr = frameObject.AddComponent<SpriteRenderer>();
        Sprite frameSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        frameSr.sprite = frameSprite;
        frameSr.drawMode = SpriteDrawMode.Sliced;
        frameSr.size = new Vector2(totalWidth + 0.1f, totalHeight + 0.1f);
        frameSr.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        frameSr.sortingOrder = -1;

        // 画像の左上が中央に来るようにオフセット
        Vector3 offset = new Vector3(-totalWidth / 2f + worldTileWidth / 2f, totalHeight / 2f - worldTileHeight / 2f, 0);

        // 生成されたタイルをシーンに配置
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int idx = i * col + j;
                Tile tile = mBoardGen.tiles[idx];
                Texture2D finalCut = tile.finalCut;

                GameObject go = new GameObject($"Tile_{i}_{j}");
                go.transform.parent = this.transform;

                // ピースの配置座標を修正（オフセットを加える）
                go.transform.position = new Vector3(j * worldTileWidth, -i * worldTileHeight, 0) + offset;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                 
                // 【重要】スプライトのピボット（中心点）を、タイルの「左上」に設定します。
                // これにより、GameObjectのTransform位置が、ピース画像の左上端と正確に一致します。
                // 元のコードではピボットが左下になっていたため、位置がずれていました。
                float pivotX = (float)tile.padL / finalCut.width;
                // Yのピボットをテクスチャの下から「ベース画像の高さ + 下パディング」の位置に設定 = ベース画像の左上
                float pivotY = (float)(tile.padB + tile.tileHeight) / finalCut.height;

                Sprite sprite = Sprite.Create(
                    finalCut,
                    new Rect(0, 0, finalCut.width, finalCut.height),
                    new Vector2(pivotX, pivotY),
                    pixelsPerUnit
                );
                sr.sprite = sprite;

                // 追加: Collider2DとMouseActionをアタッチ
                BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
                collider.size = sr.bounds.size;
                MouseAction action = go.AddComponent<MouseAction>();
                action.correctPosition = go.transform.position;
                action.snapDistance = Mathf.Min(worldTileWidth, worldTileHeight) * 0.3f;

                mTileObjects.Add(go);
            }
        }

        // ランダムにピースを散らす
        float scatterRadius = Mathf.Max(totalWidth, totalHeight) * 0.8f;
        foreach (var obj in mTileObjects)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector3 randomOffset = new Vector3(dir.x, dir.y, 0) * scatterRadius;
            obj.transform.position += randomOffset;
        }
    }

    void Update()
    {
        // スペースキーで再生成
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadAndSplitImage();
        }
    }
}