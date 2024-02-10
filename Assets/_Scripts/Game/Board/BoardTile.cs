using UnityEngine;

public class BoardTile : MonoBehaviour
{

    [SerializeField] private SpriteRenderer piece;
    [SerializeField] private SpriteRenderer highlight;
    [SerializeField] private SpriteRenderer moveTrace;
    [SerializeField] private SpriteRenderer trajectory;

    public SpriteRenderer Piece
    {
        get
        {
            return piece;
        }
    }

    public bool Highlight
    {
        set { highlight.enabled = value; }
    }

    public bool Trajectory
    {
        set { trajectory.enabled = value; }
    }

    public bool MoveTrace
    {
        set { moveTrace.enabled = value; }
    }

    public void ResetTile()
    {
        Highlight = false;
        Trajectory = false;
        moveTrace.enabled = false;
    }

}
