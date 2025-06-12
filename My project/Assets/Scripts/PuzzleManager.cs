using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    private static PuzzleManager instance;
    private readonly List<MouseAction> pieces = new List<MouseAction>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void RegisterPiece(MouseAction piece)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("PuzzleManager");
            instance = go.AddComponent<PuzzleManager>();
        }
        if (!instance.pieces.Contains(piece))
        {
            instance.pieces.Add(piece);
        }
    }

    public static void CheckCompletion()
    {
        if (instance == null) return;
        foreach (var p in instance.pieces)
        {
            if (Vector3.Distance(p.transform.position, p.correctPosition) > p.snapDistance * 0.5f)
            {
                return;
            }
        }
        PuzzleCompletionNotifier.NotifyClear();
    }
}
