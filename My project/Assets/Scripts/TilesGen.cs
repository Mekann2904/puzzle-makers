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

                // タイルの基本位置を左上基準で設定
                go.transform.position = new Vector3(j * worldTileWidth, -i * worldTileHeight, 0);

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
                mTileObjects.Add(go);
            }
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