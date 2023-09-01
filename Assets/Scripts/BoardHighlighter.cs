using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardHighlighter : MonoBehaviour
{
    public Board board;

    private void Awake()
    {
        board = GetComponent<Board>();
    }

    // turn off the temporary highlight
    void HighlightTileOff(int x, int y)
    {
        if (board == null)
            return;

        if (board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = board.allTiles[x, y].GetComponent<SpriteRenderer>();
            var color = spriteRenderer.color;
            color = new Color(color.r, color.g, color.b, 0);
            spriteRenderer.color = color;
        }
    }

    // temporary method to draw a highlight around a Tile
    void HighlightTileOn(int x, int y, Color col)
    {
        if (board == null)
            return;

        if (board.allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = board.allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = col;
        }
    }
    // highlight all matching tiles at position (x,y) in the Board
    private void HighlightMatchesAt(int x, int y)
    {
        if (board == null)
            return;

        HighlightTileOff(x, y);
        var combinedMatches = board.boardMatcher.FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    // highlight all matching tiles in the Board
    private void HighlightMatches()
    {
        if (board == null)
            return;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                HighlightMatchesAt(i, j);

            }
        }
    }

    // highlight Tiles that correspond to a list of GamePieces
    private void HighlightPieces(List<GamePiece> gamePieces)
    {
        if (board == null)
            return;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

}
