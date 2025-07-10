using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectTitle : MonoBehaviour
{
   public void SwitchScene()
    {
        Debug.Log("ボタンが押されました！");
        SceneManager.LoadScene("Titlescene", LoadSceneMode.Single);
    } 
}
