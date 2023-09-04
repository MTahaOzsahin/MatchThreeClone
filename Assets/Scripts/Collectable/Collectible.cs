
// a Collectible is just a GamePiece with no match value

// it could be either cleared by a Bomb and/or cleared at the bottom of the screen

using GamePieces;

namespace Collectable
{
	public class Collectible : GamePiece 
	{
		public bool clearedByBomb;
		public bool clearedAtBottom = true;


		// Use this for initialization
		void Start () 
		{
			matchValue = MatchValue.None;
		}

	}
}
