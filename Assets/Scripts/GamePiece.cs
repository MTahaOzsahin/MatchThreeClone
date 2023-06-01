using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    private bool _isMoving;
    private Board _board;

    public void Init(Board board)
    {
        _board = board;
    }
    
    public void SetCoordinates(int x,int y)
    {
        xIndex = x;
        yIndex = y;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     Move(Mathf.RoundToInt(transform.position.x + 1), Mathf.RoundToInt(transform.position.y), 0.3f);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     Move(Mathf.RoundToInt(transform.position.x - 1), Mathf.RoundToInt(transform.position.y), 0.3f);
        // }
    }

    public void Move(int destX, int destY,float timeToMove)
    {
        if (!_isMoving) 
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }

    private IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        var startPosition = transform.position;
        var reachedDestination = false;
        var elapsedTime = 0f;
        _isMoving = true;
        while (!reachedDestination)
        {
            if (Vector3.Distance(transform.position,destination)< 0.01f)
            {
                reachedDestination = true;
                if (_board != null)
                {
                    _board.PlaceGamePiece(this,Mathf.RoundToInt(destination.x),Mathf.RoundToInt(destination.y));
                }
            }

            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp(elapsedTime / timeToMove,0,1);
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
             transform.position = Vector3.Lerp(startPosition, destination, t);
            yield return new WaitForEndOfFrame();
        }

        _isMoving = false;
    }
}
