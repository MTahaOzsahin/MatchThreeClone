using System.Collections.Generic;
using GamePieces;
using Tile;
using UnityEngine;

namespace BoardScripts
{
    [RequireComponent(typeof(Board))]
    public class BoardTiles : MonoBehaviour
    {
        public Board board;

        private void Awake()
        {
            board = GetComponent<Board>();
        }

        // damage a Breakable Tile
        public void BreakTileAt(int x, int y)
        {
            Tile.Tile tileToBreak = board.allTiles[x, y];

            if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
            {
                // play appropriate particle effect
                if (board.particleManager != null)
                {
                    board.particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y);
                }

                tileToBreak.BreakTile();
            }
        }

        // break Tiles corresponding to a list of gamePieces
        public void BreakTileAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    BreakTileAt(piece.xIndex, piece.yIndex);
                }
            }
        }
    }
}
