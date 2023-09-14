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
    public Vector2 arenaSize = new Vector2