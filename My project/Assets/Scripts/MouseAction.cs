using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseAction : MonoBehaviour
{
    RectTransform rectTransform;  // 自分の RectTransform
    GraphicRaycaster raycaster;   // UIのRaycast判定用
    EventSystem eventSystem;      // UIイベント処理用

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        raycaster = FindFirstObjectByType<GraphicRaycaster>();
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;

            // UI上のヒット判定を行う
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == this.gameObject)
                {
                    // Canvas の RectTransform を取得
                    RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

                    // スクリーン座標 → ローカル座標に変換
                    Vector2 localPoint;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, null, out localPoint))
                    {
                        rectTransform.anchoredPosition = localPoint;
                        Debug.Log("Image 移動: " + localPoint);
                    }
                    break;
                }
            }
        }
    }
}
