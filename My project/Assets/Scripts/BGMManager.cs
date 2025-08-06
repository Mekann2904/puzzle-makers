using UnityEngine;

//BGMの引き継ぎ機能追加
public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // シーン切り替えでも破棄されない
        }
        else
        {
            Destroy(gameObject);  // すでに存在するなら新しい方を破棄
        }
    }
}
