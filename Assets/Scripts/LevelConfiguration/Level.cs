﻿using UnityEngine;

namespace LevelConfiguration
{
    [CreateAssetMenu(fileName ="LevelConfiguration", menuName = "LevelConfiguration/Level")]
    public class Level : ScriptableObject
    {
        // BoardSetup
        [Header("Board Setup")]
        public int width;
        public int height;

        // array of dot Prefabs
        public GameObject[] gamePiecePrefabs;

        // manually positioned Tiles, placed before Board is filled
        public StartingObject[] startingTiles;

        // manually positioned GamePieces, placed before the Board is filled
        public StartingObject[] startingGamePieces;

        // manually positioned Blockers, placed before Board is filled
        public StartingObject[] startingBlockers;

        [Range(0, 1)]
        public float chanceForCollectible = 0f;

        // GameManager Setup
        [Header("Goals")]
        public CollectionGoal[] collectionGoals;

        // minimum scores used to earn stars
        public int[] scoreGoals = { 1000, 2000, 3000 };

        // number of moves left in this level (replaces GameManager movesLeft)
        public int movesLeft = 30;

        public int timeLeft = 60;

        public LevelCounter levelCounter = LevelCounter.Moves;
    }
}
