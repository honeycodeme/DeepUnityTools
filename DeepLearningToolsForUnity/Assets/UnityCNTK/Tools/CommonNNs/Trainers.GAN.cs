using System.Collections;
using System.Collections.Generic;
using CNTK;
using System;
using System.Linq;
using UnityEngine;

namespace UnityCNTK
{
    public class TrainerGAN
    {
        protected Trainer trainerG;
        protected Trainer trainerD;

        protected List<Learner> learnersG;
        protected List<Learner> learnersD;

        protected GAN ganReference;
        
        public DeviceDescriptor Device { get; set; }

        protected DataBuffer dataBuffer;


        public float LastLossGenerator { get { return (float)trainerG.PreviousMinibatchLossAverage(); } }
        public float LastLossDiscriminator { get { return (float)trainerD.PreviousMinibatchLossAverage(); } }

        public float LearningRateGenerator { get; private set; }
        public float LearningRateD