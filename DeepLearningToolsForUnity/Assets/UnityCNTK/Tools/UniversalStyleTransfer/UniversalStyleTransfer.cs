
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;
using UnityCNTK;
using System;
using Accord.Math;
using System.Diagnostics;
using MathNet.Numerics;

namespace UnityCNTK.Tools.StyleAndTexture
{
    public class UniversalStyleTransferModel {


        protected CNTK.DeviceDescriptor device;


        protected Function encoderLayersStyle;
        protected Function encoderLayersContent;

        protected List<Function> decoders;

        protected Vector2Int contentDimension;
        protected Vector2Int styleDimension;

        protected byte[] modelWeights;
        
        /// <summary>
        /// encoder or decoder layer in UST. 1 to 5
        /// </summary>
        public enum PassIndex
        {
            PassOne, PassTwo, PassThree, PassFour, PassFive
        }

        [Serializable]
        public class ParameterSet{
            public bool enabled = true;
            public PassIndex Pass;
            public float BlendFactor { get; set; }
            public float Beta { get; set; }
            public float Eps { get; set; }
            public float IgnoreSingularValue { get; set; }
            public ParameterSet(PassIndex pass, float blendFactor = 0.7f)
            {
                Pass = pass;
                BlendFactor = blendFactor;
                Beta = 0.5f;
                Eps = 0.00001f;
                IgnoreSingularValue = 0.00001f;
            }
        }


        public UniversalStyleTransferModel(DeviceDescriptor device, byte[] modelWithWeights)
        {
            this.device = device;
            modelWeights = modelWithWeights;
        }




        public byte[] TransferStyle(byte[] contentRGB24, byte[] styleRGB24, ParameterSet[] passes = null)