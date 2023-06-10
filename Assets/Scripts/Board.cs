using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public int width;
    public int height;

    public int borderSize;

    public GameObject tileNormalPrefab;
    public GameObject[] gamePiecePrefabs;

    private Tile[,] _allTiles;
    private GamePiece[,] _allGamePieces;

    private Tile _clickedTile;
    private Tile _targetTile;

    private void Start()
    {
        _allTiles = new Tile[width, height];
        _allGamePieces = new GamePiece[width, height];
        SetupTiles();
        SetupCamera();
        FillRandom();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tileNormalPrefab, new Vector3(i, j, 0), Quaternion.identity);
                tile.name = "Tile (" + i + "," + j + ")";
                _allTiles[i, j] = tile.GetComponent<Tile>();
                _allTiles[i,j].Init(i,j,this);
                tile.transform.parent = transform;
            }
        }
    }

    private void SetupCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
            var aspectRatio = (float)Screen.width / Screen.height;
            var verticalSize = height / 2f + borderSize;
            var horizontalSize = (width / 2f + borderSize) / aspectRatio;
            Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
        } 
    }

    private GameObject GetRandomGamePiece()
    {
        var randomIndex = Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[randomIndex] == null)
        {
            Debug.LogWarning("BOARD: " + randomIndex + "does not contain a valid GamePiece prefab!");
        }

        return gamePiecePrefabs[randomIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece");
        }

        var gamePieceTransform = gamePiece.transform;
        gamePieceTransform.position = new Vector3(x, y, 0f);
        gamePieceTransform.rotation = Quaternion.identity;
        if (IsWithinBounds(x,y))
        {
            _allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoordinates(x,y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (_clickedTile != tile && IsNextTo(tile,_clickedTile))
        {
            _targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (_clickedTile != null && _targetTile != null)
        {
            SwitchTiles(_clickedTile,_targetTile);
        }
        
        _clickedTile = null;
        _targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTileCoroutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTileCoroutine(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = _allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = _allGamePieces[targetTile.xIndex, targetTile.yIndex];

        if (targetPiece != null && clickedPiece != null)
        {
            clickedPiece.Move(targetPiece.xIndex,targetPiece.yIndex,0.3f);
            targetPiece.Move(clickedPiece.xIndex,clickedPiece.yIndex,0.3f);
        
            yield return new WaitForSeconds(0.3f);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedPiece.xIndex, clickedPiece.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetPiece.xIndex, targetPiece.yIndex);

            if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
            {
                clickedPiece.Move(clickedPiece.xIndex,clickedPiece.yIndex,0.3f);
                targetPiece.Move(targetPiece.xIndex,targetPiece.yIndex,0.3f);
            }
            
            yield return new WaitForSeconds(0.3f);

            HighlightMatchesAt(clickedPiece.xIndex,clickedPiece.yIndex);
            HighlightMatchesAt(targetPiece.xIndex,targetPiece.yIndex);
        }
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if (IsWithinBounds(startX,startY))
        {
            startPiece = _allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            var nextX = startX + Mathf.RoundToInt(Mathf.Clamp(searchDirection.x, -1, 1)) * i;
            var nextY = startY + Mathf.RoundToInt(Mathf.Clamp(searchDirection.y, -1, 1)) * i;

            if (!IsWithinBounds(nextX,nextY))
            {
                break;
            }

            GamePiece nextPiece = _allGamePieces[nextX, nextY];

            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        if (matches.Count >= minLenght)
        {
            return matches;
        }

        return null;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        upwardMatches ??= new List<GamePiece>();

        downwardMatches ??= new List<GamePiece>();

        var combineMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combineMatches.Count >= minLenght) ? combineMatches : null;
    }
    
    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        rightMatches ??= new List<GamePiece>();

        leftMatches ??= new List<GamePiece>();

        var combineMatches = rightMatches.Union(leftMatches).ToList();

        return (combineMatches.Count >= minLenght) ? combineMatches : null;
    }

    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (var piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightTileOff(int x, int y)
    {
        var spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
        var spriteRendererColor = spriteRenderer.color;
        spriteRenderer.color = new Color(spriteRendererColor.r, spriteRendererColor.g, spriteRendererColor.b, 0);
    }

    private void HighlightTileOn(int x, int y, Color col)
    {
         var spriteRenderer = _allTiles[x,y].GetComponent<SpriteRenderer>();
         spriteRenderer.color = col;
    }

    private List<GamePiece> FindMatchesAt(int x, int y,int minLenght = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLenght);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLenght);

        horizMatches ??= new List<GamePiece>();

        vertMatches ??= new List<GamePiece>();

        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }
}
