using UnityEngine;

public class MouseAction : MonoBehaviour
{
    public static int totalPieces = 0;
    public static int snappedPieces = 0;

    private bool isDragging = false;
    private Vector3 offset;
    [HideInInspector]
    public Vector3 correctPosition;
    private const float snapDistance = 0.5f;
    private bool snapped = false;

    void Start()
    {
        totalPieces++;
    }

    void OnMouseDown()
    {
        isDragging = true;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (!snapped && Vector3.Distance(transform.position, correctPosition) < snapDistance)
        {
            transform.position = correctPosition;
            snapped = true;
            snappedPieces++;
            if (snappedPieces == totalPieces)
            {
                Debug.Log("Puzzle Completed!");
                PuzzleCompletionNotifier.NotifyClear();
            }
        }
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
