using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileManager : MonoBehaviour
{
    public void SwitchScene()
    {
        Debug.Log("ボタンが押されました！");
        SceneManager.LoadScene("TilesRoot", LoadSceneMode.Single);
    }
}
