
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;

namespace UnityCNTK.LayerDefinitions
{


    public enum NormalizationMethod { None, LayerNormalization, BatchNormalizatoin }
    /*public enum ActivationFunction
    {
        None,
        Softmax,
        Sigmoid,
        Tanh,
        Relu
    }*/



    public abstract class InputLayerDef
    {
        public bool IsBuilt { get { return InputVariable != null; } }
        public Variable InputVariable { get; protected set; } = null;
        public string Name { get; private set; } = "";

        public Variable Build(string name)
        {
            if (!IsBuilt)
            {
                Name = name;
                InputVariable = BuildNetwork( name);
                return InputVariable;
            }
            else
            {
                Debug.LogError("Input layer is already built.");
                return null;
            }
        }
        protected abstract Variable BuildNetwork(string name);
    }

    public class InputLayerDense : InputLayerDef
    {
        public int Size { get; private set; }
        public InputLayerDense(int size)
        {
            Size = size;
        }

        protected override Variable BuildNetwork(string name)
        {
            InputVariable = CNTKLib.InputVariable(new int[] { Size }, DataType.Float,name);
            return InputVariable;
        }
    }

    public class InputLayerCNTKVar : InputLayerDef
    {
        protected Variable tempInputVar;
        public InputLayerCNTKVar(Variable inputVariable)
        {
            tempInputVar = inputVariable;
        }

        protected override Variable BuildNetwork(string name)
        {
            return tempInputVar;
        }
    }

    public abstract class LayerDef
    {
        //whether the layer of network is built/
        //the parameters are only available for access if it is built
        public bool IsBuilt { get { return CNTKFunction != null; } }
        public Function CNTKFunction { get; private set; } = null;
        public string Name { get; private set; } = "";
        public DeviceDescriptor Device { get; private set; }
        public abstract List<string> ParameterNames { get; protected set; }
        /// <summary>
        /// build the network layer with the input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="device"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Function Build(Variable input, DeviceDescriptor device, string name)
        {
            if (!IsBuilt)
            {
                Device = device;
                Name = name;
                CNTKFunction = BuildNetwork(input, device, name);
                return CNTKFunction;
            }
            else
            {
                Debug.LogError("Hidden layer is already built.");
                return null;
            }
        }

        //get parameter by name
        public IList<float> GetParam(string name)
        {
            Debug.Assert(IsBuilt, "Network is not built yet while trying to get a parameter called: " + name);
            Parameter p = CNTKFunction.FindParameterByName(name);
            if (p == null) return null;

            var v = new Value(p.Value());
            return v.GetDenseData<float>(p)[0];
        }
        //set parameter by name
        public void SetParam(string name, float[] data)
        {
            Debug.Assert(IsBuilt, "Network is not built yet while trying to set a parameter called: " + name);
            Parameter p = CNTKFunction.FindParameterByName(name);
            Debug.Assert(p != null, "Did not find parameter called: " + name);

            p.SetValue(new NDArrayView(p.Shape, data, p.Value().Device));//this will has error if the data size does not fit the parameter shape
        }
        protected abstract Function BuildNetwork(Variable input, DeviceDescriptor device, string name);

        /// <summary>
        /// buildt the network without saving the informations so that we can use one layerdef to build multiple netowork. 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="device"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract Function BuildNew(Variable input, DeviceDescriptor device, string name);
    }


    /// <summary>
    /// A resnet node definition
    /// </summary>
    public class ResNodeDenseDef : LayerDef
    {
        //main parameters
        public NormalizationMethod NormalizationMethod { get { return normalizationMethod; } set { Debug.Assert(!IsBuilt, "Can not change after built."); normalizationMethod = value; } }
        private NormalizationMethod normalizationMethod;
        public int HiddenSize { get { return hiddenSize; } set { Debug.Assert(!IsBuilt, "Can not change after built."); hiddenSize = value; } }
        private int hiddenSize;
        public float DropoutRate { get { return dropoutRate; } set { Debug.Assert(!IsBuilt, "Can not change after built."); dropoutRate = Mathf.Clamp01(value); } }
        private float dropoutRate;

        //some other parameters that can be set as well.
        public float InitialWeightScale { get { return initialWeightScale; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialWeightScale = value; } }
        protected float initialWeightScale = 1f;
        public float InitialNormalizationBias { get { return initialNormalizationBias; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialNormalizationBias = value; } }
        protected float initialNormalizationBias = 0;
        public float InitialNormalizationScale { get { return initialNormalizationScale; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialNormalizationScale = value; } }
        protected float initialNormalizationScale = 1;
        public int BNTimeConst { get { return bnTimeConst; } set { Debug.Assert(!IsBuilt, "Can not change after built."); bnTimeConst = value; } }
        protected int bnTimeConst = 4096;
        public bool BNSpatial { get { return bnSpatial; } set { Debug.Assert(!IsBuilt, "Can not change after built."); bnSpatial = value; } }
        protected bool bnSpatial = false;


        public override List<string> ParameterNames { get; protected set; }

        /// <summary>
        /// Construct a ResNodeHiden data for build a ResNet
        /// </summary>
        /// <param name="size">hidden size</param>
        /// <param name="normalization">Which normalizatoin to use</param>
        /// <param name="dropoutRate">dropout rate</param>
        public ResNodeDenseDef(int size, NormalizationMethod normalization, float dropoutRate = 0)
        {
            HiddenSize = size;
            NormalizationMethod = normalization;
            DropoutRate = dropoutRate;
            ParameterNames = new List<string>();
        }

        public enum ResNodeParamType { Bias_LayerOne, Bias_LayerTwo, Weight_LayerOne, Weight_LayerTwo };
        private string ParamTypeToName(ResNodeParamType type)
        {
            string paramName = "";
            switch (type)
            {
                case ResNodeParamType.Bias_LayerOne:
                    paramName = Name + ".Res1" + Layers.BiasSuffix;
                    break;
                case ResNodeParamType.Bias_LayerTwo:
                    paramName = Name + ".Res2" + Layers.BiasSuffix;
                    break;
                case ResNodeParamType.Weight_LayerOne:
                    paramName = Name + ".Res1" + Layers.WeightSuffix;
                    break;
                case ResNodeParamType.Weight_LayerTwo:
                    paramName = Name + ".Res2" + Layers.WeightSuffix;
                    break;
            }
            return paramName;
        }
        public IList<float> GetParam(ResNodeParamType paramType)
        {
            var paramName = ParamTypeToName(paramType);
            if (paramName == "")
                return null;
            return GetParam(paramName);
        }
        public void SetParam(ResNodeParamType paramType, float[] data)
        {
            var paramName = ParamTypeToName(paramType);
            if (paramName == "")
                return;
            SetParam(paramName, data);
        }
        public override Function BuildNew(Variable input, DeviceDescriptor device, string name)
        {
            Debug.LogError("BuildNew not supported");
            return null;
        }
        protected override Function BuildNetwork(Variable input, DeviceDescriptor device, string name)
        {

            Variable toAdd = input;

            //create the size by adding 0s if the input size is smaller than the hidden size
            if (input.Shape[0] < HiddenSize)
            {
                toAdd = Layers.AddDummy(input, HiddenSize - input.Shape[0], device);
            }
            else if (input.Shape[0] > HiddenSize)
            {
                Debug.LogError("Can not have a hidden size that is smaller than the input size in resnetnode");
            }

            //first layer
            var c1 = UnityCNTK.Layers.Dense(input, HiddenSize, device, true, name + ".Res1", InitialWeightScale);
            if (normalizationMethod == NormalizationMethod.BatchNormalizatoin)
            {
                c1 = Layers.BatchNormalization(c1, InitialNormalizationBias, InitialNormalizationScale, BNTimeConst, BNSpatial, device, name + ".BN");
            }
            else if (normalizationMethod == NormalizationMethod.LayerNormalization)
            {
                c1 = Layers.LayerNormalization(c1, device, InitialNormalizationBias, InitialNormalizationScale, name + ".LN");
            }
            c1 = CNTKLib.ReLU(c1);
            if (DropoutRate > 0)
                c1 = CNTKLib.Dropout(c1, DropoutRate);

            //second layer
            var c2 = UnityCNTK.Layers.Dense(c1, hiddenSize, device, true, name + ".Res2", InitialWeightScale);
            if (normalizationMethod == NormalizationMethod.BatchNormalizatoin)
            {
                c2 = Layers.BatchNormalization(c2, InitialNormalizationBias, InitialNormalizationScale, BNTimeConst, BNSpatial, device, name + ".BN");
            }
            else if (normalizationMethod == NormalizationMethod.LayerNormalization)
            {
                c2 = Layers.LayerNormalization(c2, device, InitialNormalizationBias, InitialNormalizationScale, name + ".LN");
            }

            //add together
            var p = CNTKLib.Plus(c2, toAdd);
            p = CNTKLib.ReLU(p);
            if (DropoutRate > 0)
                p = CNTKLib.Dropout(p, DropoutRate);

            //add parameters to list
            ParameterNames.Add(ParamTypeToName(ResNodeParamType.Bias_LayerOne));
            ParameterNames.Add(ParamTypeToName(ResNodeParamType.Bias_LayerTwo));
            ParameterNames.Add(ParamTypeToName(ResNodeParamType.Weight_LayerOne));
            ParameterNames.Add(ParamTypeToName(ResNodeParamType.Weight_LayerTwo));

            return p;
        }

    };


    //a normal dense layer definition
    public class LayerDenseDef : LayerDef
    {
        //main parameters
        public NormalizationMethod NormalizationMethod { get { return normalizationMethod; } set { Debug.Assert(!IsBuilt, "Can not change after built."); normalizationMethod = value; } }
        private NormalizationMethod normalizationMethod;
        public int HiddenSize { get { return hiddenSize; } set { Debug.Assert(!IsBuilt, "Can not change after built."); hiddenSize = value; } }
        private int hiddenSize;
        public float DropoutRate { get { return dropoutRate; } set { Debug.Assert(!IsBuilt, "Can not change after built."); dropoutRate = Mathf.Clamp01(value); } }
        private float dropoutRate;
        public bool HasBias { get { return hasBias; } set { Debug.Assert(!IsBuilt, "Can not change after built."); hasBias = value; } }
        private bool hasBias;
        public LayerDef Activation { get { return activation; } set { Debug.Assert(!IsBuilt, "Can not change after built."); activation = value ; } }
        private LayerDef activation;

        //some other parameters that can be set as well.
        public float InitialWeightScale { get { return initialWeightScale; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialWeightScale = value; } }
        protected float initialWeightScale = 0.02f;
        public float InitialNormalizationBias { get { return initialNormalizationBias; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialNormalizationBias = value; } }
        protected float initialNormalizationBias = 0;
        public float InitialNormalizationScale { get { return initialNormalizationScale; } set { Debug.Assert(!IsBuilt, "Can not change after built."); initialNormalizationScale = value; } }
        protected float initialNormalizationScale = 1;
        public int BNTimeConst { get { return bnTimeConst; } set { Debug.Assert(!IsBuilt, "Can not change after built."); bnTimeConst = value; } }
        protected int bnTimeConst = 4096;
        public bool BNSpatial { get { return bnSpatial; } set { Debug.Assert(!IsBuilt, "Can not change after built."); bnSpatial = value; } }
        protected bool bnSpatial = false;


        public override List<string> ParameterNames { get; protected set; }

        /// <summary>
        /// Construct a dense layer data for build
        /// </summary>
        /// <param name="size">hidden size</param>
        /// <param name="normalization">Which normalizatoin to use</param>
        /// <param name="dropoutRate">dropout rate</param>
        public LayerDenseDef(int size, NormalizationMethod normalization, LayerDef activationDef, float dropoutRate = 0)
        {
            HiddenSize = size;
            NormalizationMethod = normalization;
            DropoutRate = dropoutRate;
            ParameterNames = new List<string>();
            Activation = activationDef;
        }

        public enum DenseParamType { Bias, Weight };
        private string ParamTypeToName(DenseParamType type)
        {
            string paramName = "";
            switch (type)
            {
                case DenseParamType.Bias:
                    paramName = Name + ".Dense" + Layers.BiasSuffix;
                    break;
                case DenseParamType.Weight:
                    paramName = Name + ".Dense" + Layers.WeightSuffix;
                    break;
            }
            return paramName;
        }
        public IList<float> GetParam(DenseParamType paramType)
        {
            var paramName = ParamTypeToName(paramType);
            if (paramName == "")
                return null;
            return GetParam(paramName);
        }
        public void SetParam(DenseParamType paramType, float[] data)