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

    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    public float swapTime = 0.5f;

    private Tile[,] mAllTiles;
    private GamePiece[,] mAllGamePieces;

    private Tile mClickedTile;
    private Tile mTargetTile;

    private bool mPlayerInputEnabled = true;

    private void Start()
    {
        mAllTiles = new Tile[width, height];
        mAllGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupCamera();
        FillBoard(10, 0.5f);
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                tile.name = "Tile (" + i + "," + j + ")";
                mAllTiles[i, j] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                mAllTiles[i, j].Init(i, j, this);
            }
        }
    }

    private void SetupCamera()
    {
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
            float aspectRatio = Screen.width / (float)Screen.height;
            float verticalSize = height / 2f + borderSize;
            float horizontalSize = (width / 2f + borderSize) / aspectRatio;
            Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
        }
    }

    private GameObject GetRandomGamePiece()
    {
        int randomIdx = Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[randomIdx] == null)
        {
            Debug.LogWarning("BOARD:  " + randomIdx + "does not contain a valid GamePiece prefab!");
        }
        return gamePiecePrefabs[randomIdx];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD:  Invalid GamePiece!");
            return;
        }

        var gamePieceTransform = gamePiece.transform;
        gamePieceTransform.position = new Vector3(x, y, 0);
        gamePieceTransform.rotation = Quaternion.identity;

        if (IsWithinBounds(x, y))
        {
            mAllGamePieces[x, y] = gamePiece;
        }

        gamePiece.SetCoordinates(x, y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);

        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);

            if (falseYOffset != 0)
            {
                randomPiece.transform.position = new Vector3(x, y + falseYOffset, 0);
                randomPiece.GetComponent<GamePiece>().Move(x, y, moveTime);
            }
            
            randomPiece.transform.parent = transform;
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    private void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        const int maxIterations = 100;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (mAllGamePieces[i, j] == null)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                    var iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        leftMatches ??= new List<GamePiece>();
        downwardMatches ??= new List<GamePiece>();

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);
    }

    public void ClickTile(Tile tile)
    {
        if (mClickedTile == null)
        {
            mClickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (mClickedTile != null && IsNextTo(tile, mClickedTile))
        {
            mTargetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (mClickedTile != null && mTargetTile != null)
        {
            SwitchTiles(mClickedTile, mTargetTile);
        }

        mClickedTile = null;
        mTargetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (mPlayerInputEnabled)
        {
            GamePiece clickedPiece = mAllGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = mAllGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (targetPiece != null && clickedPiece != null)
            {
                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);
                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                }
            }
        }
    }

    static bool IsNextTo(Tile start, Tile end)
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

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = mAllGamePieces[startX, startY];
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
            var nextX = startX + Mathf.RoundToInt(Mathf.Clamp(searchDirection.x, -1, 1) * i);
            var nextY = startY + Mathf.RoundToInt(Mathf.Clamp(searchDirection.y, -1, 1) * i);

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = mAllGamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        
        return null;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        upwardMatches ??= new List<GamePiece>();
        downwardMatches ??= new List<GamePiece>();
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        rightMatches ??= new List<GamePiece>();
        leftMatches ??= new List<GamePiece>();
        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

        horizMatches ??= new List<GamePiece>();
        vertMatches ??= new List<GamePiece>();
        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }

    private List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    private void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = mAllTiles[x, y].GetComponent<SpriteRenderer>();
        var spriteRendererColor = spriteRenderer.color;
        spriteRenderer.color = new Color(spriteRendererColor.r, spriteRendererColor.g, spriteRendererColor.b, 0);
    }

    private void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer spriteRenderer = mAllTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
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

    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = mAllGamePieces[x, y];

        if (pieceToClear != null)
        {
            mAllGamePieces[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        HighlightTileOff(x, y);
    }

    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    private void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if (mAllGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (mAllGamePieces[column, j] != null)
                    {
                        mAllGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        mAllGamePieces[column, i] = mAllGamePieces[column, j];
                        mAllGamePieces[column, i].SetCoordinates(column, i);

                        if (!movingPieces.Contains(mAllGamePieces[column, i]))
                        {
                            movingPieces.Add(mAllGamePieces[column, i]);
                        }

                        mAllGamePieces[column, j] = null;
                        break;

                    }
                }
            }
        }
        return movingPieces;
    }

    private List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (int column in columnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }

        return movingPieces;
    }

    static List<int> GetColumns(List<GamePiece> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (GamePiece piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }

        return columns;
    }

    private void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        mPlayerInputEnabled = false;
        List<GamePiece> matches = gamePieces;

        do
        {
            // clear and collapse
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            // yield return null;

            //refill
            yield return StartCoroutine(RefillRoutine());
            matches = FindAllMatches();

            yield return new WaitForSeconds(0.5f);
        }
        while (matches.Count != 0);

        mPlayerInputEnabled = true;
    }

    private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        HighlightPieces(gamePieces);
        yield return new WaitForSeconds(0.5f);
        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePieces);

            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePieces);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    private IEnumerator RefillRoutine()
    {
        FillBoard(10, 0.5f);
        yield return null;
    }

    static bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
