
﻿using CNTK;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityCNTK;
using UnityEngine;
using UnityEngine.UI;
using UnityCNTK.ReinforcementLearning;

public class MazeDQLRunner : MonoBehaviour {
    public Text scoreUI;
    public Text stepsUI;
    public Text episodeUI;

    public MazeEnvironment environment;
    public string saveDataPath;
    protected DQLModel model;
    protected DQLModel modelTarget;
    protected TrainerDQLSimple trainer;

    [Header("Training Settings")]
    public int experienceBufferSize = 5000000;
    public int batchSize = 64;
    public float discountFactor = 0.98f;
    public int trainingStepInterval = 10;
    public int stepsBeforeTrain = 100000;
    [Header("Changing RL")]
    public int changeRLSteps = 3000000;
    public float startLearningRate = 0.4f, endLearningRate = 0.05f;
    public float CurrentLearningRate
    {
        get
        {
            return startLearningRate + (endLearningRate - startLearningRate) * Mathf.Clamp01(((float)Steps - stepsBeforeTrain) / changeRLSteps);
        }
    }
    [Header("Random action settings")]
    public float randomChanceStart = 1.0f;
    public float randomChanceEnd = 0.05f;
    public int randomChanceDropSteps = 1000000;

    [Range(0, 100)]
    public float timeScale = 1;
    public bool training = true;


    public int Steps { get { return trainer.Steps; } }
    [Header("Information")]
    public int currentEpisode = 0;