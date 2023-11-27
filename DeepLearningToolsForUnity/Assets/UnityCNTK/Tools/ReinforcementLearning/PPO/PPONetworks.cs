﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using CNTK;
using UnityCNTK.LayerDefinitions;
using MathNet.Numerics.Distributions;

namespace UnityCNTK.ReinforcementLearning
{
    public abstract class PPONetwork
    {
        public abstract int StateSize { get; protected set; }
        public abstract int ActionSize { get; protected set; }

        public abstract bool IsActionContinuous { get; protected set; }
        public abstract Variable InputState { get; protected set; }

        public abstract Variable OutputMean { get; protected set; }
        public abstract Variable OutputVariance { get; protected set; }
        public abstract Variable OutputProbabilities { get; protected set; }
        public abstract Variable OutputValue { get; protected set; }

        //CNTK functions to use directly
        public abstract Function ValueFunction { get; protected set; }
        public abstract Function PolicyFunction { get; protected set; }

        public abstract DeviceDescriptor Device { get; protected set; }

    }


    /// <summary>
    /// PPO network for continuous action space similiar to one of Unity ML's python implementation
    /// https://github.com/Unity-Technologies/ml-agents
    /// </summary>
    public class PPONetworkContinuousSimple : PPONetwork
    {
        public override int StateSize { get; protected set; }
        public override int ActionSize { get; protected set; }

        public override bool IsActionContinuous { get; protected set; } = true;
        public override Variable InputState { get; protected set; }

        //actor outputs
        public override Variable OutputMean { get; protected set; }             //for continuous action
        public override Variable OutputVariance { get; protected set; }         //for continuous action
        public override Variable OutputProbabilities { get; protected set; }    //for discrete action
        //critic output
        public override Variable OutputValue { get; protected set; }
        //CNTK functions to use directly
        public override Function ValueFunction { get; protected set; }
        public override Function PolicyFunction { get; protected set; }

        protected SequentialNetworkDense valueNetwork;
        protected SequentialNetworkDense policyNetwork;

        public override DeviceDescriptor Device { get; protected set; }
        public PPONetworkContinuousSimple(int stateSize, int actionSize, int numLayers, int hiddenSize, DeviceDescriptor device, float initialWeightScale = 0.01f)
        {
            Device = device;
            StateSize = stateSize;
            ActionSize = actionSize;

            //create actor network part
            var inputA = new InputLayerDense(stateSize);
            var outputA = new OutputLayerDense(actionSize, null, OutputLayerDense.LossFunction.None);
            outputA.InitialWeightScale = initialWeightScale;
            valueNetwork = new SequentialNetworkDense(inputA, LayerDefineHelper.DenseLayers(numLayers, hiddenSize, true, NormalizationMethod.None, 0, initialWeightScale, new TanhDef()), outputA, device);
            InputState = inputA.InputVariable;
            OutputMean = outputA.GetOutput