using UnityEngine;

public class MouseAction : MonoBehaviour
{

    private bool isDragging = false;
    private Vector3 offset;
    private SpriteRenderer sr;
    private Collider2D col;
    [HideInInspector]
    public Vector3 correctPosition;
    [HideInInspector]
    public float snapDistance = 0.5f;
    private bool snapped = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        PuzzleManager.RegisterPiece(this);
    }

    void OnMouseDown()
    {
        if (snapped) return; // 既に配置済みならドラッグ不可
        isDragging = true;
        if (sr != null) sr.sortingOrder = 1; // 操作中は前面に
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (sr != null) sr.sortingOrder = 0;
        if (!snapped && Vector3.Distance(transform.position, correctPosition) <= snapDistance)
        {
            transform.position = correctPosition;
            snapped = true;
            if (col != null) col.enabled = false; // スナップ後は掴めないように
        }

        PuzzleManager.CheckCompletion();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z) + offset;
        }
    }
}
