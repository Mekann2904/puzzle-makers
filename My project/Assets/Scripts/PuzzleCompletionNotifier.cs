using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PuzzleCompletionNotifier : MonoBehaviour
{
    private static PuzzleCompletionNotifier instance;
    private Text message;

    void Awake()
    {
        instance = this;
        Canvas canvas = new GameObject("ClearCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        DontDestroyOnLoad(canvas);

        message = new GameObject("ClearText").AddComponent<Text>();
        message.transform.SetParent(canvas.transform);
        message.alignment = TextAnchor.MiddleCenter;
        message.fontSize = 48;
        message.text = "CLEAR!";
        message.color = Color.red;
        RectTransform rt = message.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        message.gameObject.SetActive(false);
    }

    private void Show()
    {
        if (message != null)
        {
            message.gameObject.SetActive(true);
            Invoke("GoToClearScene", 3f);
        }
    }

    private void GoToClearScene()
    {
        SceneManager.LoadScene("ClearScene");
    }

    public static void NotifyClear()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("PuzzleCompletionNotifier");
            instance = go.AddComponent<PuzzleCompletionNotifier>();
        }
        instance.Show();
    }
}
