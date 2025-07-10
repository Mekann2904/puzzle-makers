using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectLv3Scene : MonoBehaviour
{
public void SwitchScene()
    {
        Debug.Log("ボタンが押されました！");
        SceneManager.LoadScene("TilesRoot", LoadSceneMode.Single);
    }
}