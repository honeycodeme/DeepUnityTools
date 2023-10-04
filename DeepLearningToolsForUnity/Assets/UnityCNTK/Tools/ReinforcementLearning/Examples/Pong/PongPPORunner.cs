using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityCNTK.ReinforcementLearning;
using UnityCNTK;
using CNTK;
using UnityEngine.UI;
using System.IO;

public class PongPPORunner : MonoBehaviour {
    public PongEnvironment environment;
    public string saveDataPath = "PongPPO.bytes";
    public float learningRate = 0.001f;
    protected PPOModel model;
    protected TrainerPPOSimple trainer;

    public int episodesThisTrain = 0;
    public int trainedCount = 0;

    public int episodeToRunForEachTrain = 30;
    public int iterationForEachTrain = 50;
    public int minibatch = 32;
    public bool training = true;
    [Range(0, 100)]
    public float timeScale;

    public int Steps { get { return trainer.Steps; } }
    [Header("info")]
    public int currentEpisode = 0;
    public int leftWin = 0;
    public int rightWin = 0;

    public AutoAverage winningRate50Left = new AutoAverage(50);
    protected AutoAverage loss;
    public AutoAverage episodePointAve;
    protected float episodePoint;

    // Use this for initialization
    void Start()
    {
        var network = new PPONetworkDiscreteSimple(6, 3