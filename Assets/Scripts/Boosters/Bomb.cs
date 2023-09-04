// the various bombs available in the Game

using GamePieces;

namespace Boosters
{
	public enum BombType
	{
		None,
		Column,
		Row,
		Adjacent,
		Color

	}

// the Bomb is just a GamePiece with a BombType exposed
	public class Bomb : GamePiece 
	{
		public BombType bombType;

	}
}