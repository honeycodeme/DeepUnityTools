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

    protected AutoAverage loss;
    protected AutoAverage episodePointAve;
    protected float episodePoint;

    // Use this for initialization
    void Start () {
        PPONetworkContinuousSimple network;
        if (environment.is3D)
        {
            network = new PPONetworkContinuousSimple(8, 2, 2, 32, DeviceDescriptor.CPUDevice, 0.01f);
            model = new PPOModel(network);
            trainer = new TrainerPPOSimple(model, LearnerDefs.AdamLearner(learningRate),1, 10000, 200);
            trainer.ClipE