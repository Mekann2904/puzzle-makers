using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class UIDrag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    public RectTransform targetArea; // ← ドロップ対象（画像など）をInspectorで設定
    public string sceneToLoad = "Next scene"; // ← 遷移先のシーン名

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (RectOverlaps(rectTransform, targetArea))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // RectTransform同士の重なり判定
    private bool RectOverlaps(RectTransform rect1, RectTransform rect2)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rect2, rect1.position, canvas.worldCamera);
    }
}