using LevelGoals;
using UnityEngine;
using UnityEngine.UI;

namespace Score
{
    [RequireComponent(typeof(Slider))]
    public class ScoreMeter : MonoBehaviour
    {
        // reference to slider component
        public Slider slider;

        // array of ScoreStar components (defaults to three stars)
        public ScoreStar[] scoreStars = new ScoreStar[3];
    
        // reference to LevelGoal component
        private LevelGoal mLevelGoal;
    
        // reference to maximum score (largest scoring goal)
        private int mMaxScore;

        void Awake()
        {
            // populate the Slider component
            slider = GetComponent<Slider>();
        }

        // position the ScoreStars automatically
        public void SetupStars(LevelGoal levelGoal)
        {
            // if levelGoal is invalid, return immediately
            if (levelGoal == null)
            {
                Debug.LogWarning("SCOREMETER Invalid level goal!");
                return;
            }

            // cache the LevelGoal component for later
            mLevelGoal = levelGoal;

            // set the maximum score goal
            mMaxScore = mLevelGoal.scoreGoals[mLevelGoal.scoreGoals.Length - 1];

            // get the slider's RectTransform width
            float sliderWidth = slider.GetComponent<RectTransform>().rect.width;

            // avoid divide by zero error
            if (mMaxScore > 0)
            {
                // loop through our scoring goals
                for (int i = 0; i < levelGoal.scoreGoals.Length; i++)
                {
                    // if the corresponding ScoreStar exists...
                    if (scoreStars[i] != null)
                    {
                        // set the x value based on the ratio of the scoring goal over the maximum score
                        float newX = (sliderWidth * levelGoal.scoreGoals[i] / mMaxScore) - (sliderWidth * 0.5f);

                        // move the ScoreStar's RectTransform
                        RectTransform starRectXform = scoreStars[i].GetComponent<RectTransform>();

                        if (starRectXform != null)
                        {
                            starRectXform.anchoredPosition = new Vector2(newX, starRectXform.anchoredPosition.y);
                        }
                    }
                }

            }

        }

        // Update the ScoreMeter 
        public void UpdateScoreMeter(int score, int starCount)
        {
            if (mLevelGoal != null)
            {
                // adjust the slider fill area (cast as floats, otherwise will become zero)
                slider.value = score / (float) mMaxScore;
            }

            // activate each star based on current star count
            for (int i = 0; i < starCount; i++)
            {
                if (scoreStars[i] != null)
                {
                    scoreStars[i].Activate();
                }
            }
        }
    }
}
