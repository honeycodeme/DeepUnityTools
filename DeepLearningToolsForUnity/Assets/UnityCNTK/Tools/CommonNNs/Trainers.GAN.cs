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
        public float LearningRateDiscriminator { get; private set; }

        protected Dictionary<int,CNTKDictionary> savedLearners;
        protected Dictionary<int, Dictionary<Parameter, NDArrayView>> savedParameters;

        public bool usePredictionInTraining = false;

        public TrainerGAN(GAN gan, LearnerDefs.LearnerDef generatorLearner, LearnerDefs.LearnerDef discriminatorLearner, DeviceDescriptor device, int maxDataBufferCount = 50000)
        {

            Device = device;
            ganReference = gan;

            learnersG = new List<Learner>();
            learnersD = new List<Learner>();

            //trainer for  generator
            Learner learnerG = generatorLearner.Create(gan.GeneratorSequentialModel.Parameters);
            learnersG.Add(learnerG);
            trainerG = Trainer.CreateTrainer(gan.GeneratorOutput, gan.GeneratorLoss, gan.GeneratorLoss, learnersG);
            //trainer for  discriminator
            Learner learnerD = discriminatorLearner.Create(gan.DiscriminatorSequentialModel.Parameters);
            learnersD.Add(learnerD);
            trainerD = Trainer.CreateTrainer(gan.DiscriminatorMerged, gan.DiscriminatorLoss, gan.DiscriminatorLoss, learnersD);

            //create databuffer
            List<DataBuffer.DataInfo> dataInfos = new List<DataBuffer.DataInfo>();

            if(gan.InputConditionSize > 0)
            {
                dataInfos.Add(new DataBuffer.DataInfo("Condition", DataBuffer.DataType.Float, gan.InputConditionSize));
            }
            dataInfos.Add(new DataBuffer.DataInfo("Target", DataBuffer.DataType.Float, gan.OutputSize));
            dataBuffer = new DataBuffer(maxDataBufferCount, dataInfos.ToArray());

            //others
            savedLearners = new Dictionary<int, CNTKDictionary>();
            savedParameters = new Dictionary<int, Dictionary<Parameter, NDArrayView>>();
        }


        public void AddData(float[] inputConditions, float[] inputTargets)
        {
            //I am not checking the data size here because the dataBuffer.AddData will check it for me....tooo 