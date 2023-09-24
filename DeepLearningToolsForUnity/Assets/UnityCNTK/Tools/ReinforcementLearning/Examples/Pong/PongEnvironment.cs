using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCNTK;
using CNTK;
using UnityCNTK.ReinforcementLearning;


public class PongEnvironment : MonoBehaviour,IRLEnvironment {

    public ControlSource playerLeftControl;
    public ControlSource playerRightControl; 

    public float failureReward = -1;
    public float winReward = 1;
    public float hitBallReward = 0.1f;

    public float racketSpeed = 0.02f;
    public float ballSpeed = 0.01f;
    public float racketWidth = 0.05f;

    public readonly int ActionUp = 2;
    public readonly int ActionDown = 0;
    public readonly int ActionStay = 1;

    public float leftStartX = -1;
    public float rightStartX = 1;
    public Vector2 arenaSize = new Vector2(2.2f, 1.0f);


    [Header("Informations")]
    public float leftHitOrMiss = 0;
    public float rightHitOrMiss = 0;

    public GameState CurrentGameState { get { return currentGameState; } }
    private GameState currentGameState;
    


    protected int step = 0;
    public int framesPerStep = 5;

    public struct GameState
    {
        public Vector2 ballVelocity;
        public Vector2 ballPosition;
        public float leftY;
        public float rightY;
        public int gameWinPlayer;
        public float rewardLastStepLeft;
        public float rewardLastStepRight;
    }

    public enum ControlSource
    {
        FromStep,
        FromPlayerInput,
        SimpleAI
    }

    public int GameWinPlayer
    {
        get
        {
            return currentGameState.gameWinPlayer;
        }
        protected set
        {
            currentGameState.gameWinPlayer = value;
        }
    }



    private void Start()
    {
        Physics.autoSimulation = false;
        Reset();
    }


    public float[] CurrentState(int actor=0)
    {
        float[] result = null;
        if (actor == 0)
        {
            result = new float[] {
                currentGameState.leftY,
                currentGameState.rightY,
                currentGameState.ballPosition.x,
                currentGameState.ballPosition.y,
                currentGameState.ballVelocity.x,
                currentGameState.ballVelocity.y
            };
        }
        else
        {
            result = new float[] {
                currentGameState.rightY,
                currentGameState.leftY,
                -currentGameState.ballPosition.x,
                currentGameState.ballPosition.y,
                -currentGameState.ballVelocity.x,
                currentGameState.ballVelocity.y
            };
        }
        return result;
    }
    /// <summary>
    /// steps from the start of this episode
    /// </summary>
    /// <returns></returns>
    public int CurrentStep()
    {
        return step;
    }

    public bool IsResolved()
    {
        return true;
    }

    //public void Step(float[] actions) { Debug.LogError("Need two actor"); }

    /// <summary>
    /// Actions: 0:down, 1 not move, 2 up
    /// </summary>
    /// <param name="actions"></param>
    public void Step(params float[][] actions)
    {
        //get action from different sources
        int actionLeft;
        if (playerLeftControl == ControlSource.FromPlayerInput) {
            actionLeft = Input.GetButton("Up")?2:(Input.GetButton("Down")?0:1);
        } else if (playerLeftControl == ControlSource.SimpleAI) {
            actionLeft = SimpleAI(0);
        } else {
            actionLeft = (int)actions[0][0]; 
        }

        int actionRight;
        if (playerRightControl == ControlSource.FromPlayerInput)
        {
            actionRight = Input.GetButton("Up") ? 2 : (Input.GetButton("Down") ? 0 : 1);
        }
        else if (playerRightControl == ControlSource.SimpleAI)
        {
            actionRight = SimpleAI(1);
        }
        else
        {
            actionRigh