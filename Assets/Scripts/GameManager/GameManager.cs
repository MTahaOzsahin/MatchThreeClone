using System.Collections;
using System.Collections.Generic;
using BoardScripts;
using Collection;
using GamePieces;
using LevelConfiguration;
using LevelGoals;
using Score;
using Sounds;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;

// the GameManager is the master controller for the GamePlay

namespace GameManager
{
    [RequireComponent(typeof(LevelGoal))]
    public class GameManager : Singleton<GameManager>
    {
        public World currentWorld;
        public int currentLevel;

        // reference to the Board
        private Board mBoard;

        // is the player read to play?
        private bool mIsReadyToBegin;

        // is the game over?
        private bool mIsGameOver;

        public bool IsGameOver 
        {
            get => mIsGameOver;
            set => mIsGameOver = value;
        }

        // do we have a winner?
        private bool mIsWinner;

        // are we ready to load/reload a new level?
        private bool mIsReadyToReload;

        // reference to LevelGoal component
        private LevelGoal mLevelGoal;

        private LevelGoalCollected mLevelGoalCollected;

        // public reference to LevelGoalTimed component
        public LevelGoal LevelGoal => mLevelGoal;


        public override void Awake()
        {
            base.Awake();

            // fill in LevelGoal and LevelGoalTimed components
            mLevelGoal = GetComponent<LevelGoal>();
            //m_levelGoalTimed = GetComponent<LevelGoalTimed>();
            mLevelGoalCollected = GetComponent<LevelGoalCollected>();
            // cache a reference to the Board
            mBoard = FindObjectOfType<Board>().GetComponent<Board>();

        }

        // looks at a specific world and reads the data
        void ConfigureLevel(int levelIndex)
        {
            if (currentWorld == null)
            {
                Debug.LogError("GAMEMANAGER SetupLevelData: missing world...");
                return;
            }

            if (levelIndex >= currentWorld.levels.Length)
            {
                Debug.LogError("GAMEMANAGER SetupLevelData: invalid level index...");
                return;
            }

            if (mBoard == null)
            {
                Debug.LogError("GAMEMANAGER SetupLevelData: missing Board...");
                return;
            }

            // reference to the Level ScriptableObject (just for readability)
            Level levelConfig = currentWorld.levels[levelIndex];

            mBoard.width = levelConfig.width;
            mBoard.height = levelConfig.height;
            mBoard.startingTiles = levelConfig.startingTiles;
            mBoard.startingGamePieces = levelConfig.startingGamePieces;
            mBoard.startingBlockers = levelConfig.startingBlockers;
            mBoard.gamePiecePrefabs = levelConfig.gamePiecePrefabs;
            mBoard.chanceForCollectible = levelConfig.chanceForCollectible;

            // we need to create a new Collection Goal array by instantiating the prefabs
            List<CollectionGoal> goals = new List<CollectionGoal>();
            foreach (CollectionGoal g in levelConfig.collectionGoals)
            {
                CollectionGoal instance = Instantiate(g, transform);
                goals.Add(instance);
            }

            // we can only assign the array of instances to the 
            mLevelGoalCollected.collectionGoals = goals.ToArray();
            mLevelGoalCollected.scoreGoals = levelConfig.scoreGoals;
            mLevelGoalCollected.movesLeft = levelConfig.movesLeft;
            mLevelGoalCollected.timeLeft = levelConfig.timeLeft;
            mLevelGoalCollected.levelCounter = levelConfig.levelCounter;
        }


        void Start()
        {

            ConfigureLevel(currentLevel);


            if (UIManager.Instance != null)
            {
                // position ScoreStar horizontally
                if (UIManager.Instance.scoreMeter != null)
                {
                    UIManager.Instance.scoreMeter.SetupStars(mLevelGoal);
                }

                // use the Scene name as the Level name
                if (UIManager.Instance.levelNameText != null)
                {
                    // get a reference to the current Scene
                    Scene scene = SceneManager.GetActiveScene();
                    UIManager.Instance.levelNameText.text = scene.name;
                }

                if (mLevelGoalCollected != null)
                {
                    UIManager.Instance.EnableCollectionGoalLayout(true);
                    UIManager.Instance.SetupCollectionGoalLayout(mLevelGoalCollected.collectionGoals);
                }
                else
                {
                    UIManager.Instance.EnableCollectionGoalLayout(false);
                }

                bool useTimer = (mLevelGoal.levelCounter == LevelCounter.Timer);

                UIManager.Instance.EnableTimer(useTimer);
                UIManager.Instance.EnableMovesCounter(!useTimer);
            }

            // update the moves left UI
            mLevelGoal.movesLeft++;
            UpdateMoves();

            // start the main game loop
            StartCoroutine("ExecuteGameLoop");
        }

        // update the Text component that shows our moves left
        public void UpdateMoves()
        {
            // if the LevelGoal is not timed (e.g. LevelGoalScored)...
            if (mLevelGoal.levelCounter == LevelCounter.Moves)
            {
                // decrement a move
                mLevelGoal.movesLeft--;

                // update the UI
                if (UIManager.Instance != null && UIManager.Instance.movesLeftText != null)
                {
                    UIManager.Instance.movesLeftText.text = mLevelGoal.movesLeft.ToString();
                }
            }
        }

        // this is the main coroutine for the Game, that determines are basic beginning/middle/end

        // each stage of the game must complete before we advance to the next stage
        // add as many stages here as necessary

        IEnumerator ExecuteGameLoop()
        {
            yield return StartCoroutine("StartGameRoutine");
            yield return StartCoroutine("PlayGameRoutine");

            // wait for board to refill
            yield return StartCoroutine("WaitForBoardRoutine", 0.5f);

            yield return StartCoroutine("EndGameRoutine");
        }

        // switches ready to begin status to true
        public void BeginGame()
        {
            mIsReadyToBegin = true;

        }

        // coroutine for the level introduction
        IEnumerator StartGameRoutine()
        {
            if (UIManager.Instance != null)
            {
                // show the message window with the level goal
                if (UIManager.Instance.messageWindow != null)
                {
                    UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
                    int maxGoal = mLevelGoal.scoreGoals.Length - 1;
                    UIManager.Instance.messageWindow.ShowScoreMessage(mLevelGoal.scoreGoals[maxGoal]);

                    if (mLevelGoal.levelCounter == LevelCounter.Timer)
                    {
                        UIManager.Instance.messageWindow.ShowTimedGoal(mLevelGoal.timeLeft);
                    }
                    else
                    {
                        UIManager.Instance.messageWindow.ShowMovesGoal(mLevelGoal.movesLeft);
                    }

                    if (mLevelGoalCollected != null)
                    {
                        UIManager.Instance.messageWindow.ShowCollectionGoal();

                        GameObject goalLayout = UIManager.Instance.messageWindow.collectionGoalLayout;

                        if (goalLayout != null)
                        {
                            UIManager.Instance.SetupCollectionGoalLayout(mLevelGoalCollected.collectionGoals, goalLayout, 80);
                        }
                    }
                    else
                    {
                        UIManager.Instance.messageWindow.ShowCollectionGoal(false);
                    }
                }
            }

            // wait until the player is ready
            while (!mIsReadyToBegin)
            {
                yield return null;
            }

            // fade off the ScreenFader
            if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
            {
                UIManager.Instance.screenFader.FadeOff();
            }

            // wait half a second
            yield return new WaitForSeconds(0.5f);

            // setup the Board
            if (mBoard != null)
            {
                mBoard.boardSetup.SetupBoard();
            }
        }

        // coroutine for game play
        IEnumerator PlayGameRoutine()
        {
            // if level is timed, start the timer
            if (mLevelGoal.levelCounter == LevelCounter.Timer)
            {
                mLevelGoal.StartCountdown();
            }
            // while the end game condition is not true, we keep playing
            // just keep waiting one frame and checking for game conditions
            while (!mIsGameOver)
            {

                mIsGameOver = mLevelGoal.IsGameOver();

                mIsWinner = mLevelGoal.IsWinner();

                // wait one frame
                yield return null;
            }
        }

        IEnumerator WaitForBoardRoutine(float delay = 0f)
        {
            if (mLevelGoal.levelCounter == LevelCounter.Timer && UIManager.Instance != null
                                                              && UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.FadeOff();
                UIManager.Instance.timer.paused = true;
            }

            if (mBoard != null)
            {
                // this accounts for the swapTime delay in the Board's SwitchTilesRoutine BEFORE ClearAndRefillRoutine is invoked
                yield return new WaitForSeconds(mBoard.swapTime);

                // wait while the Board is refilling
                while (mBoard.isRefilling)
                {
                    yield return null;
                }
            }

            // extra delay before we go to the EndGameRoutine
            yield return new WaitForSeconds(delay);
        }

        // coroutine for the end of the level
        IEnumerator EndGameRoutine()
        {
            // set ready to reload to false to give the player time to read the screen
            mIsReadyToReload = false;


            // if player beat the level goals, show the win screen and play the win sound
            if (mIsWinner)
            {
                ShowWinScreen();
            } 
            // otherwise, show the lose screen and play the lose sound
            else
            {   
                ShowLoseScreen();
            }

            // wait one second
            yield return new WaitForSeconds(1f);

            // fade the screen 
            if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
            {
                UIManager.Instance.screenFader.FadeOn();
            }  

            // wait until read to reload
            while (!mIsReadyToReload)
            {
                yield return null;
            }

            // reload the scene (you would customize this to go back to the menu or go to the next level
            // but we just reload the same scene in this demo
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		
        }

        void ShowWinScreen()
        {
            if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowWinMessage();
                UIManager.Instance.messageWindow.ShowCollectionGoal(false);

                if (ScoreManager.Instance != null)
                {
                    string scoreStr = "you scored\n" + ScoreManager.Instance.CurrentScore.ToString() + " points!";
                    UIManager.Instance.messageWindow.ShowGoalCaption(scoreStr,0,70);
                }

                if (UIManager.Instance.messageWindow.goalCompleteIcon != null)
                {
                    UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalCompleteIcon);
                }
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
        }

        void ShowLoseScreen()
        {
            if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXFormMover>().MoveOn();
                UIManager.Instance.messageWindow.ShowLoseMessage();
                UIManager.Instance.messageWindow.ShowCollectionGoal(false);

                string caption;
                if (mLevelGoal.levelCounter == LevelCounter.Timer)
                {
                    caption = "Out of time!";
                }
                else
                {
                    caption = "Out of moves!";
                }

                UIManager.Instance.messageWindow.ShowGoalCaption(caption, 0, 70);

                if (UIManager.Instance.messageWindow.goalFailedIcon != null)
                {
                    UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalFailedIcon);
                }

            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLoseSound();
            }
        }

        // use this to acknowledge that the player is ready to reload
        public void ReloadScene()
        {
            mIsReadyToReload = true;
        }

        // score points and play a sound
        public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
        {
            if (piece != null)
            {
                if (ScoreManager.Instance != null)
                {
                    // score points
                    ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);

                    // update the scoreStars in the Level Goal component
                    mLevelGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);

                    if (UIManager.Instance != null && UIManager.Instance.scoreMeter != null)
                    {
                        UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, 
                            mLevelGoal.scoreStars);
                    }
                }

                // play scoring sound clip
                if (SoundManager.Instance != null && piece.clearSound != null)
                {
                    SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
                }
            }
        }

        public void AddTime(int timeValue)
        {
            if (mLevelGoal.levelCounter == LevelCounter.Timer)
            {
                mLevelGoal.AddTime(timeValue);
            }
        }

        public void UpdateCollectionGoals(GamePiece pieceToCheck)
        {
            if (mLevelGoalCollected != null)
            {
                mLevelGoalCollected.UpdateGoals(pieceToCheck);
            }
        }




    }
}
