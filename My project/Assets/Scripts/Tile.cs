using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ジグソーパズルの1ピース（タイル）を表現するクラス
/// </summary>
public class Tile
{
    public enum Direction { UP, DOWN, LEFT, RIGHT }
    public enum PosNegType { POS, NEG, NONE }

    public Texture2D finalCut { get; private set; }
     
    // publicプロパティに変更して外部からアクセス可能にする
    public int padL { get; private set; }
    public int padT { get; private set; }
    public int padR { get; private set; }
    public int padB { get; private set; }
    public int tileWidth { get; private set; }
    public int tileHeight { get; private set; }

    private Texture2D mOriginalTexture;
    private PosNegType[] mCurveTypes = new PosNegType[4];
    private int xIndex, yIndexFromBottom;

    // 隣接ピース参照
    public Tile neighborUp;
    public Tile neighborDown;
    public Tile neighborLeft;
    public Tile neighborRight;

    // コンストラクタの引数をBoardGenの呼び出しと一致させる
    public Tile(Texture2D texture, int xIndex, int yIndexFromBottom, int tileWidth, int tileHeight, int padding)
    {
        mOriginalTexture = texture;
        this.xIndex = xIndex;
        this.yIndexFromBottom = yIndexFromBottom;
        this.tileWidth = tileWidth;
        this.tileHeight = tileHeight;
         
        padT = padding; padB = padding; padL = padding; padR = padding;

        // Applyを呼び出す前にfinalCutを初期化
        // SetCurveTypeでパディングが変更される可能性があるため、サイズ計算はApply時に行う
    }

    public void SetCurveType(Direction dir, PosNegType type) {
        mCurveTypes[(int)dir] = type;
        if(type == PosNegType.NONE){
            if(dir == Direction.UP) padT = 0;
            if(dir == Direction.DOWN) padB = 0;
            if(dir == Direction.LEFT) padL = 0;
            if(dir == Direction.RIGHT) padR = 0;
        }
    }
    public PosNegType GetCurveType(Direction dir) => mCurveTypes[(int)dir];

    public void Apply()
    {
        finalCut = new Texture2D(tileWidth + padL + padR, tileHeight + padT + padB, TextureFormat.ARGB32, false);
        var clear = new Color(0, 0, 0, 0);
        // 輪郭点リストを作成（端点重複を避けて連結）
        var up = CreateCurve(Direction.UP, mCurveTypes[(int)Direction.UP]);
        var right = CreateCurve(Direction.RIGHT, mCurveTypes[(int)Direction.RIGHT]);
        var down = CreateCurve(Direction.DOWN, mCurveTypes[(int)Direction.DOWN]);
        var left = CreateCurve(Direction.LEFT, mCurveTypes[(int)Direction.LEFT]);
        List<Vector2> contour = new List<Vector2>();
        contour.AddRange(up);
        contour.AddRange(right.Skip(1));
        contour.AddRange(down.Skip(1));
        contour.AddRange(left.Skip(1));
        for (int x = 0; x < finalCut.width; ++x)
        {
            for (int y = 0; y < finalCut.height; ++y)
            {
                // --- パディング部分の合成 ---
                // 左パディング
                if (x < padL && neighborLeft != null)
                {
                    int srcX = neighborLeft.finalCut.width - padR + x;
                    int srcY = y;
                    if (srcX >= 0 && srcX < neighborLeft.finalCut.width && srcY >= 0 && srcY < neighborLeft.finalCut.height)
                    {
                        finalCut.SetPixel(x, y, neighborLeft.finalCut.GetPixel(srcX, srcY));
                        continue;
                    }
                }
                // 右パディング
                if (x >= padL + tileWidth && neighborRight != null)
                {
                    int srcX = x - (padL + tileWidth) + padL;
                    int srcY = y;
                    if (srcX >= 0 && srcX < neighborRight.finalCut.width && srcY >= 0 && srcY < neighborRight.finalCut.height)
                    {
                        finalCut.SetPixel(x, y, neighborRight.finalCut.GetPixel(srcX, srcY));
                        continue;
                    }
                }
                // 上パディング
                if (y >= padB + tileHeight && neighborUp != null)
                {
                    int srcX = x;
                    int srcY = y - (padB + tileHeight) + padB;
                    if (srcX >= 0 && srcX < neighborUp.finalCut.width && srcY >= 0 && srcY < neighborUp.finalCut.height)
                    {
                        finalCut.SetPixel(x, y, neighborUp.finalCut.GetPixel(srcX, srcY));
                        continue;
                    }
                }
                // 下パディング
                if (y < padB && neighborDown != null)
                {
                    int srcX = x;
                    int srcY = neighborDown.finalCut.height - padT + y;
                    if (srcX >= 0 && srcX < neighborDown.finalCut.width && srcY >= 0 && srcY < neighborDown.finalCut.height)
                    {
                        finalCut.SetPixel(x, y, neighborDown.finalCut.GetPixel(srcX, srcY));
                        continue;
                    }
                }
                // --- 通常処理 ---
                Vector2 p = new Vector2(x, y);
                if (!IsPointInPolygon(p, contour))
                {
                    // アンチエイリアス処理: 輪郭からの距離が近い場合は半透明で描画
                    float minDist = float.MaxValue;
                    foreach (var pt in contour)
                    {
                        float dist = Vector2.Distance(p, pt);
                        if (dist < minDist) minDist = dist;
                    }
                    if (minDist < 1.5f) {
                        // 距離が0ならα=1, 1.5ならα=0
                        float alpha = Mathf.Clamp01(1.5f - minDist) / 1.5f;
                        Color c = clear;
                        c.a = alpha;
                        finalCut.SetPixel(x, y, c);
                    } else {
                        finalCut.SetPixel(x, y, clear);
                    }
                }
                else
                {
                    int px = x - padL + xIndex * this.tileWidth;
                    int py = y - padB + yIndexFromBottom * this.tileHeight;
                    if (px >= 0 && px < mOriginalTexture.width && py >= 0 && py < mOriginalTexture.height)
                        finalCut.SetPixel(x, y, mOriginalTexture.GetPixel(px, py));
                    else
                        finalCut.SetPixel(x, y, clear);
                }
            }
        }
        finalCut.Apply();
    }

    void FloodFillInitAndFill()
    {
        int w = finalCut.width;
        int h = finalCut.height;
        bool[,] visited = new bool[w, h];
        List<Vector2> allPts = new List<Vector2>();
        for (int i = 0; i < 4; ++i)
            allPts.AddRange(CreateCurve((Direction)i, mCurveTypes[i]));
        foreach (var pt in allPts)
        {
            int x = Mathf.RoundToInt(pt.x);
            int y = Mathf.RoundToInt(pt.y);
            if (x >= 0 && x < w && y >= 0 && y < h)
                visited[x, y] = true;
        }
        // 中心付近でvisitedがfalseの点を探す
        Vector2Int start = new Vector2Int(w / 2, h / 2);
        if (visited[start.x, start.y]) {
            bool found = false;
            for (int r = 1; r < Mathf.Max(w, h) / 2 && !found; r++) {
                for (int dx = -r; dx <= r && !found; dx++) {
                    for (int dy = -r; dy <= r && !found; dy++) {
                        int sx = w / 2 + dx, sy = h / 2 + dy;
                        if (sx >= 0 && sx < w && sy >= 0 && sy < h && !visited[sx, sy]) {
                            start = new Vector2Int(sx, sy);
                            found = true;
                        }
                    }
                }
            }
            if (!found) return; // 輪郭が閉じていない
        }
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(start);
        visited[start.x, start.y] = true;
        while (stack.Count > 0)
        {
            var v = stack.Pop();
            Fill(v.x, v.y);
            foreach (var d in new[] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down })
            {
                int nx = v.x + d.x, ny = v.y + d.y;
                if (nx >= 0 && nx < w && ny >= 0 && ny < h && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    stack.Push(new Vector2Int(nx, ny));
                }
            }
        }
    }

    void Fill(int x, int y)
    {
        int px = x - padL + xIndex * this.tileWidth;
        int py = y - padB + yIndexFromBottom * this.tileHeight;

        if (px < 0 || px >= mOriginalTexture.width || py < 0 || py >= mOriginalTexture.height)
        {
            finalCut.SetPixel(x, y, Color.clear);
            return;
        }
        finalCut.SetPixel(x, y, mOriginalTexture.GetPixel(px, py));
    }

    public List<Vector2> CreateCurve(Direction dir, PosNegType type)
    {
        // 0-1正規化のベジェ曲線
        List<Vector2> pts = new List<Vector2>(BezierCurve.PointList2(TemplateBezierCurve.templateControlPoints, 0.001f));
        float w = this.tileWidth, h = this.tileHeight;
        float padL = this.padL, padT = this.padT, padR = this.padR, padB = this.padB;

        switch (dir)
        {
            case Direction.UP:
                for (int i = 0; i < pts.Count; i++)
                {
                    float x = pts[i].x * w + padL;
                    float y = pts[i].y * padT + padB + h;
                    if (type == PosNegType.NEG) y = -pts[i].y * padT + padB + h;
                    pts[i] = new Vector2(x, y);
                }
                break;
            case Direction.RIGHT:
                for (int i = 0; i < pts.Count; i++)
                {
                    float x = pts[i].y * padR + padL + w;
                    float y = pts[i].x * h + padB;
                    if (type == PosNegType.NEG) x = -pts[i].y * padR + padL + w;
                    pts[i] = new Vector2(x, y);
                }
                break;
            case Direction.DOWN:
                for (int i = 0; i < pts.Count; i++)
                {
                    float x = (1 - pts[i].x) * w + padL;
                    float y = -pts[i].y * padB + padB;
                    if (type == PosNegType.NEG) y = pts[i].y * padB + padB;
                    pts[i] = new Vector2(x, y);
                }
                break;
            case Direction.LEFT:
                for (int i = 0; i < pts.Count; i++)
                {
                    float x = -pts[i].y * padL + padL;
                    float y = (1 - pts[i].x) * h + padB;
                    if (type == PosNegType.NEG) x = pts[i].y * padL + padL;
                    pts[i] = new Vector2(x, y);
                }
                break;
        }
        return pts;
    }

    // 多角形内判定（射影法）
    bool IsPointInPolygon(Vector2 p, List<Vector2> poly)
    {
        int n = poly.Count;
        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y + 1e-6f) + poly[i].x))
                inside = !inside;
        }
        return inside;
    }
}