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
    public float lr = 0.00001