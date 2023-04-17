﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using CNTK;
using UnityCNTK.LayerDefinitions;
using MathNet.Numerics.Distributions;

namespace UnityCNTK.ReinforcementLearning
{
    public class DQLModel
    {

        public QNetwork Network { get; protected set; }
        public int StateSize { get { return Network.StateSize; } }
        public int ActionSize { get { return Network.ActionSize; } }
        public DeviceDescriptor Device { get { return Network.Device; } }


        //---variables for training
        public Variable InputOldAction { get; protected set; }
        public Variable InputState { get { return Network.InputState; } }
        public Variable InputTargetQ { get; protected set; }
        
        public Variable OutputLoss { get; protected set; }
        public Variable OutputAction { get; protected set; }
        public Variable OutputQs { get { return Network.OutputQs; } }
        public Variable OutputMaxQ { get; protected set; }

        //test
        Variable outputTargetQ;

        public Function CNTKFunction { get; protected set; }

        //public Variable testOutputProb;

        public DQLModel(QNetwork network)
        {
            Network = network;

            InputOldAction = CNTKLib.InputVariable(new int[] { 1 }, DataType.Float);

            InputTargetQ = CNTKLib.InputVariable(new int[] { 1 }, DataType.Float);

            var oneHotOldAction = CNTKLib.OneHotOp(InputOldAction, (uint)ActionSize, false, new Axis(0));
            outputTargetQ = CNTKLib.ReduceSum(CNTKLib.ElementTimes(OutputQs, oneHotOldAction), Axis.AllStaticAxes());
            //OutputLoss = CNTKLib.Square(CNTKLib.Minus(outputTargetQ, InputTargetQ),"Loss");
            OutputLoss = Layers.HuberLoss(outputTargetQ, InputTargetQ, Device);

            OutputAction = CNTKLib.Argmax(OutputQs, new Axis(0));
            OutputMaxQ = CNTKLib.ReduceMax(OutputQs, new Axis(0));

            CNTKFunction = Function.Combine(new List<Variable>() { OutputLoss, OutputAction, OutputMaxQ });
        }

        public byte[] Save()
      