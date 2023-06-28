using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityCNTK.ReinforcementLearning;
using UnityCNTK;
using CNTK;
using UnityEngine.UI;
using System.IO;

public class Ball3DRunner : MonoBehaviour {
    public Text scoreUI;
    public Ball3DEnviroment environment;
    public string saveDataPath;
    public float learningRate = 0.001f;
    protected PPOModel model;
    protected TrainerPPOSimple trainer;

    public int episodesThisTrain = 0;
    public int trainedCount = 0;

    public int episodeToRunForEachTrain = 30;
    public int iterationForEachTrain = 50;
    public int minibatch = 32;

    [Range(0,100)]
    public float timeScale;
    public bool training = true;

    protect