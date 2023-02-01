using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;
using System.Linq;
using UnityCNTK;
using UnityCNTK.Tools.StyleAndTexture;
using UnityCNTK.LayerDefinitions;
using System.Threading;
using System;
using MathNet.Numerics;
using Accord.Math;

public class TestSeqNNTrain2D : MonoBehaviour {

    protected SequentialNetworkDense network;
    public DataPlane2D dataPlane;
    protected TrainerSimpleNN trainer;
    public float lr = 0.00001f;
    public bool training = false;
    protected OutputLayerDef outputLayer;
    // Use this for initialization
    void Start () {
        CreateResnode();

        //TestLayerNormalization();

    }
	

	// Update is called once per frame
	void Update () {
        if (training)
        {
            TrainOnce(50);
        }
    }
    
    

    public void CreateResnode()
    {
        var input = new InputLayerDense(2);

        //outputLayer = new OutputLayerDenseBayesian(1);
        outputLayer = 