using System.Collections;
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
