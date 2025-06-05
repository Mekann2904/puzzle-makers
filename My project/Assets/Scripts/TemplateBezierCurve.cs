using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パズルピースの「凸部分」の形状を定義するテンプレート
/// </summary>
public static class TemplateBezierCurve
{
    // 1x1の正規化空間で定義されたベジェ曲線の制御点
    public static readonly List<Vector2> templateControlPoints = new List<Vector2>
    {
        // ベース幅を狭め、中央が張り出すジグソー型の形状に調整
        new Vector2(0.00f, 0.00f),
        new Vector2(0.20f, 0.00f),
        new Vector2(0.25f, 0.00f),
        new Vector2(0.35f, 0.60f),
        new Vector2(0.50f, 1.00f),
        new Vector2(0.65f, 0.60f),
        new Vector2(0.75f, 0.00f),
        new Vector2(1.00f, 0.00f)
    };


}

