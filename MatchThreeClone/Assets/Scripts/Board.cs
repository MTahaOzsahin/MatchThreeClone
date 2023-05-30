using System;
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

    private void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece");
        }

        var gamePieceTransform = gamePiece.transform;
        gamePieceTransform.position = new Vector3(x, y, 0f);
        gamePieceTransform.rotation = Quaternion.identity;
        gamePiece.SetCoordinates(x,y);
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
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                }
            }
        }
    }
}
