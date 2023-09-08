
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityCNTK;
using CNTK;
using UnityEngine.UI;
using System.IO;
using UnityCNTK.ReinforcementLearning;

public class PoleRunner : MonoBehaviour {

    public Text scoreUI;
    public PoleGameEnviroment environment;
    public string saveDataPath = "PolePendulum.bytes";
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

    protected AutoAverage loss;
    protected AutoAverage episodePointAve;
    protected float episodePoint;

    // Use this for initialization
    void Start()
    {
        var network = new PPONetworkContinuousSimple(2, 1, 4, 64, DeviceDescriptor.GPUDevice(0),0.01f);
        model = new PPOModel(network);
        trainer = new TrainerPPOSimple(model, LearnerDefs.AdamLearner(learningRate), 1,10000, 500);

        //test
        //trainer.RewardDiscountFactor = 0.5f;

        loss = new AutoAverage(iterationForEachTrain);
        episodePointAve = new AutoAverage(episodeToRunForEachTrain);
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        trainer.SetLearningRate(learningRate);