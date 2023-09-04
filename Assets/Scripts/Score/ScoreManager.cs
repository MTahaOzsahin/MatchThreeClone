using System.Collections;
using UnityEngine.UI;

// Singleton manager class to keep track of our score
namespace Score
{
	public class ScoreManager : Singleton<ScoreManager> 
	{
		// our current score
		private int mCurrentScore;

		// read-only Property to refer to our current score publicly
		public int CurrentScore => mCurrentScore;

		// used to hold a "counter" show the score increment upward to current score
		private int mCounterValue;

		// amount to increment the counter
		readonly int mIncrement = 5;

		// UI.Text that shows the score
		public Text scoreText;


		public float countTime = 1f;

		// Use this for initialization
		void Start () 
		{
			UpdateScoreText (mCurrentScore);
		}

		// update the UI score Text
		public void UpdateScoreText(int scoreValue)
		{
			if (scoreText != null) 
			{
				scoreText.text = scoreValue.ToString ();
			}
		}

		// add a value to the current score
		public void AddScore(int value)
		{
			mCurrentScore += value;
			StartCoroutine (CountScoreRoutine ());
		}

		// coroutine shows the score counting up the currentScore value
		IEnumerator CountScoreRoutine()
		{
			int iterations = 0;

			// if we are less than the current score (and we haven't taken too long to get there)...
			while (mCounterValue < mCurrentScore && iterations < 100000) 
			{
				mCounterValue += mIncrement;
				UpdateScoreText (mCounterValue);
				iterations++;
				yield return null;
			}

			//... set the counter equal to the currentScore and update the score Text
			mCounterValue = mCurrentScore;
			UpdateScoreText (mCurrentScore);

		}

	}
}
