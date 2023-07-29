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
    