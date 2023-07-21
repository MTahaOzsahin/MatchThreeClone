using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    private Board board;

    public void Init(int x, int y, Board mBoard)
    {
        xIndex = x;
        yIndex = y;
        board = mBoard;
    }

    private void OnMouseDown()
    {
        if (board != null)
        {  
            board.ClickTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (board != null)
        {
            board.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (board != null)
        {
            board.ReleaseTile();
        }
    }
}
