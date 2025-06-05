using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ベジェ曲線の計算を行う静的ヘルパークラス
/// </summary>
public static class BezierCurve
{
    /// <summary>
    /// 制御点のリストから、ベジェ曲線上の点のリストを生成します。
    /// </summary>
    /// <param name="controlPoints">ベジェ曲線の制御点</param>
    /// <param name="detail">点の細かさ。小さいほど滑らかになります。</param>
    /// <returns>曲線上の点のリスト</returns>
    public static List<Vector2> PointList2(List<Vector2> controlPoints, float detail)
    {
        List<Vector2> pointList = new List<Vector2>();
        for (float t = 0; t <= 1; t += detail)
        {
            pointList.Add(CalculateBezierPoint(t, controlPoints));
        }
        pointList.Add(CalculateBezierPoint(1f, controlPoints)); // 確実に終点を追加
        return pointList;
    }

    /// <summary>
    /// ド・カステリョのアルゴリズムを使用して、指定されたt値におけるベジェ曲線上の点を計算します。
    /// </summary>
    private static Vector2 CalculateBezierPoint(float t, List<Vector2> controlPoints)
    {
        if (controlPoints.Count == 1)
        {
            return controlPoints[0];
        }

        List<Vector2> newPoints = new List<Vector2>();
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector2 p = (1 - t) * controlPoints[i] + t * controlPoints[i + 1];
            newPoints.Add(p);
        }
        return CalculateBezierPoint(t, newPoints);
    }
}