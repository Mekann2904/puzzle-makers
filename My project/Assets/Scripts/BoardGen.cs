using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 画像全体を複数のピース（Tile）に分割し、各ピースの生成・管理を行うクラス
/// </summary>
public class BoardGen
{
    public int row = 4;
    public int col = 4;
    public List<Tile> tiles = new List<Tile>();

    public void CreateJigsawTiles(Texture2D baseTexture, int row, int col)
    {
        this.row = row;
        this.col = col;
        tiles.Clear();

        int baseTileWidth = baseTexture.width / col;
        int baseTileHeight = baseTexture.height / row;
        int padding = Mathf.Max(4, Mathf.Min(baseTileWidth, baseTileHeight) / 8);

        Tile.PosNegType[,] verticalCurves = new Tile.PosNegType[row + 1, col];
        Tile.PosNegType[,] horizontalCurves = new Tile.PosNegType[row, col + 1];

        for (int i = 0; i < row + 1; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (i > 0 && i < row)
                    verticalCurves[i, j] = RandomPosNeg();
                else
                    verticalCurves[i, j] = Tile.PosNegType.NONE;
            }
        }
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col + 1; j++)
            {
                if (j > 0 && j < col)
                    horizontalCurves[i, j] = RandomPosNeg();
                else
                    horizontalCurves[i, j] = Tile.PosNegType.NONE;
            }
        }

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                // 端のタイルが半端なサイズにならないように調整
                int tileWidth = (j == col - 1) ? baseTexture.width - baseTileWidth * (col - 1) : baseTileWidth;
                int tileHeight = (i == row - 1) ? baseTexture.height - baseTileHeight * (row - 1) : baseTileHeight;
                 
                int yIndexFromBottom = row - 1 - i;

                // Tileコンストラクタの呼び出しを修正
                Tile tile = new Tile(baseTexture, j, yIndexFromBottom, tileWidth, tileHeight, padding);

                // 上下方向のカーブ指定を修正
                // 行インデックスの扱いを誤っており、外周まで凸凹が生成されていました。
                tile.SetCurveType(Tile.Direction.UP, verticalCurves[i, j]);
                tile.SetCurveType(Tile.Direction.DOWN, InvertCurve(verticalCurves[i + 1, j]));
                tile.SetCurveType(Tile.Direction.LEFT, InvertCurve(horizontalCurves[i, j]));
                tile.SetCurveType(Tile.Direction.RIGHT, horizontalCurves[i, j + 1]);

                tile.Apply();
                tiles.Add(tile);
            }
        }
    }

    private static Tile.PosNegType RandomPosNeg()
    {
        return (Random.value < 0.5f) ? Tile.PosNegType.POS : Tile.PosNegType.NEG;
    }

    private static Tile.PosNegType InvertCurve(Tile.PosNegType type)
    {
        if (type == Tile.PosNegType.POS) return Tile.PosNegType.NEG;
        if (type == Tile.PosNegType.NEG) return Tile.PosNegType.POS;
        return Tile.PosNegType.NONE;
    }
}