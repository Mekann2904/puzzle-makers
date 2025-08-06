using UnityEngine;

public class Rotator : MonoBehaviour
{
    // 1秒あたりの回転速度（インスペクターで調整可能）
    public float spinSpeed = 200f;

    // Updateは毎フレーム呼ばれる
    void Update()
    {
        // Z軸を基準に、オブジェクトを回転させる
        // Time.deltaTimeを掛けることで、フレームレートに依存しない滑らかな回転になる
        transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);
    }
}