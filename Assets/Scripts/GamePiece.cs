using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour
{

    public enum MatchValue
    {
        Yellow,
        Blue,
        Magenta,
        Indigo,
        Green,
        Teal,
        Red,
        Cyan,
        Wild
    }

    public MatchValue matchValue;
    
    public int xIndex;
    public int yIndex;

    private bool isMoving;
    private Board board;

    public void Init(Board mBoard)
    {
        board = mBoard;
    }
    
    public void SetCoordinates(int x,int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Move(int destX, int destY,float timeToMove)
    {
        if (!isMoving) 
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        var startPosition = transform.position;
        var reachedDestination = false;
        var elapsedTime = 0f;
        isMoving = true;
        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position,destination)< 0.01f)
            {
                reachedDestination = true;
                if (board != null)
                {
                    board.PlaceGamePiece(this,Mathf.RoundToInt(destination.x),Mathf.RoundToInt(destination.y));
                }
            }

            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp(elapsedTime / timeToMove,0,1);
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
             transform.position = Vector3.Lerp(startPosition, destination, t);
            yield return new WaitForEndOfFrame();
        }

        isMoving = false;
    }
}
