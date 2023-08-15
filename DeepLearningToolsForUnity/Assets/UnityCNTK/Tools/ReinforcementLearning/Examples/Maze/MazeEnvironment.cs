using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCNTK;
using UnityCNTK.ReinforcementLearning;

public class MazeEnvironment: MonoBehaviour,IRLEnvironment
{
   
    public Vector2i mazeDimension;
    public bool regenerateMapOnReset = false;
    public bool randomWallChance = false;
    public float wallChanceOnNonPath = 0.3f;
    public int maxStepAllowed = 20;
    public float failureReward = -100;
    public float maxWinReward = 100;
    public float goToWallReward;
    public float goUpReward = 1;
    public float goCloserReward = 1;
    public float stepCostReward = -0.1f;
    
    private Vector2i startPosition;
    private Vector2i goalPosition;
    private Vector2i currentPlayerPosition;
    public bool Win { get; private set; }
    public float[,] map;
    private Dictionary<int, GameState> savedState;

    [Header("Info")]
    public bool isDone = false;
    public int steps = 0;
    public float lastReward = 0;

    public readonly int WallInt = 0;
    public readonly int PlayerInt = 2;
    public readonly int PathInt = 1;
    public readonly int GoalInt = 3;

    private struct GameState
    {
        public float[,] map;
        public Vector2i startPosition;
        public Vector2i goalPosition;
        public Vector2i currentPlayerPosition;
        public bool win;
    }


    // Use this for initialization
    void Start()
    {
        savedState = new Dictionary<int, GameState>();
        RestartGame(true);
    }
    
    public void Step(params float[][] actions)
    {
        Debug.Assert(actions.Length == 1);
        int action = Mathf.RoundToInt(actions[0][0]);
        lastReward = StepAction(action);

        steps++;
    }
    public float LastReward(int actor = 0) { return las