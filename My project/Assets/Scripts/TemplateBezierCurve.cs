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
        new Vector2(0.00f, 0.00f),
        new Vector2(0.15f, 0.00f),
        new Vector2(0.35f, 0.8f),
        new Vector2(0.50f, 0.8f),
        new Vector2(0.65f, 0.8f),
        new Vector2(0.85f, 0.00f),
        new Vector2(1.00f, 0.00f)
    };
}