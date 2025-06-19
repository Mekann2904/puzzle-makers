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
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                int idx = i * col + j;
                Tile tile = mBoardGen.tiles[idx];
                Texture2D finalCut = tile.finalCut;

                // ベース画像左上座標（ピクセル単位）
                // ピースごとのtileWidth/Heightを使うと端のタイルで位置誤差が生じるため
                // 元画像を均等分割した基準サイズで位置を計算する
                float baseTileWidth = (float)mTextureOriginal.width / col;
                float baseTileHeight = (float)mTextureOriginal.height / row;
                float baseX = j * baseTileWidth;
                float baseY = i * baseTileHeight;

                // ワールド座標に変換
                float x = -((float)mTextureOriginal.width / 2f) + baseX + tile.tileWidth / 2f;
                float y = ((float)mTextureOriginal.height / 2f) - baseY - tile.tileHeight / 2f;

                // パディング分だけ中心をずらす
                float padOffsetX = ((float)tile.padL - (float)tile.padR) / 2f;
                float padOffsetY = ((float)tile.padB - (float)tile.padT) / 2f;
                x += padOffsetX;
                y += padOffsetY;

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
            LoadAndSplitImage();
        }
    }
}