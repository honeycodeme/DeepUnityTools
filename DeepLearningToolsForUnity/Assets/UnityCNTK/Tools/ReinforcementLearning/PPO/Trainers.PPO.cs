
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;

namespace UnityCNTK.ReinforcementLearning
{

    public interface IRLEnvironment
    {
        //void Step(float[] action);  //TODO remove this and replace this with void Step(params float[][] actions);
        void Step(params float[][] actions);
        float LastReward(int actor=0);
        float[] CurrentState(int actor = 0);
        int CurrentStep();
        bool IsEnd();
        bool IsResolved();
        void Reset();
    }

    public class TrainerPPOSimple
    {
        
        protected DataBuffer dataBuffer;
        public int DataCountStored { get { return dataBuffer.CurrentCount; } }
        public PPOModel Model { get; protected set; }
       
        public int MaxStepHorizon { get; set; }
        
        public float RewardDiscountFactor { get; set; } = 0.99f;
        public float RewardGAEFactor { get; set; } = 0.95f;
        public float ValueLossWeight { get; set; } = 1f;
        public float EntroyLossWeight { get; set; } = 0.0f;
        public float ClipEpsilon { get; set; } = 0.2f;

        public int NumberOfActor { get; private set; } = 1;
        public int Steps { get; protected set; } = 0;

        protected Dictionary<int, List<float>> statesEpisodeHistory;
        protected Dictionary<int, List<float>> rewardsEpisodeHistory;
        protected Dictionary<int, List<float>> actionsEpisodeHistory;
        protected Dictionary<int, List<float>> actionprobsEpisodeHistory;
        protected Dictionary<int, List<float>> valuesEpisodeHistory;



        public Dictionary<int, float[]> LastState { get; private set; }
        public Dictionary<int, float[]> LastAction { get; protected set; }
        public Dictionary<int, float[]> LastActionProbs { get; protected set; }
        public Dictionary<int, float> LastValue { get; protected set; }


        public float LastLoss { get; protected set; }
        //public float LastEvalLoss { get; protected set; }

        protected Trainer trainer;

        protected List<Learner> learners;

        public TrainerPPOSimple(PPOModel model, LearnerDefs.LearnerDef learner, int numberOfActor = 1, int bufferSize = 2048, int maxStepHorizon = 2048)
        {
            Model = model;
            MaxStepHorizon = maxStepHorizon;
            NumberOfActor = numberOfActor;

            statesEpisodeHistory = new Dictionary<int, List<float>>();
            rewardsEpisodeHistory = new Dictionary<int, List<float>>();
            actionsEpisodeHistory = new Dictionary<int, List<float>>();
            valuesEpisodeHistory = new Dictionary<int, List<float>>();
            actionprobsEpisodeHistory = new Dictionary<int, List<float>>();
            for (int i = 0; i < numberOfActor; ++i)
            {
                statesEpisodeHistory[i] = new List<float>();
                rewardsEpisodeHistory[i] = new List<float>();
                actionsEpisodeHistory[i] = new List<float>();
                valuesEpisodeHistory[i] = new List<float>();
                actionprobsEpisodeHistory[i] = new List<float>();
            }


            LastState = new Dictionary<int, float[]>();
            LastAction = new Dictionary<int, float[]>();
            LastActionProbs = new Dictionary<int, float[]>();
            LastValue = new Dictionary<int, float>();

            dataBuffer = new DataBuffer(bufferSize,
                new DataBuffer.DataInfo("State", DataBuffer.DataType.Float, Model.StateSize),
                new DataBuffer.DataInfo("Action", DataBuffer.DataType.Float, Model.IsActionContinuous?Model.ActionSize:1),
                new DataBuffer.DataInfo("ActionProb", DataBuffer.DataType.Float, Model.IsActionContinuous ? Model.ActionSize : 1),
                new DataBuffer.DataInfo("TargetValue", DataBuffer.DataType.Float, 1),
                new DataBuffer.DataInfo("Advantage", DataBuffer.DataType.Float, 1)
                );

            learners = new List<Learner>();
            List<Parameter> parameters = new List<Parameter>(Model.OutputLoss.ToFunction().Parameters());
            learners.Add(learner.Create(parameters));   

            //test
            trainer = Trainer.CreateTrainer(Model.OutputLoss, Model.OutputLoss, Model.OutputLoss, learners);
        }


        /// <summary>
        /// Step the enviormenet for training.
        /// </summary>
        /// <param name="environment"></param>
        public void Step(IRLEnvironment environment)
        {
            float[][] actions = new float[NumberOfActor][];
            

            float[] statesAll = new float[NumberOfActor * Model.StateSize];
            for (int i = 0; i < NumberOfActor; ++i)
            {
                var states = environment.CurrentState(i).CopyToArray();
                LastState[i] = states;
                Array.Copy(states, 0, statesAll, i * Model.StateSize, Model.StateSize);
            }

            if (Model.IsActionContinuous)
            {
                float[] actionProbs = null;
                float[] tempAction = Model.EvaluateActionContinuous(statesAll, out actionProbs);
                for (int i = 0; i < NumberOfActor; ++i)
                {
                    actions[i] = new float[Model.ActionSize];
                    Array.Copy(tempAction, i* Model.ActionSize, actions[i], 0, Model.ActionSize);
                    LastAction[i] = actions[i];
                    LastActionProbs[i] = new float[Model.ActionSize];
                    Array.Copy(actionProbs, i * Model.ActionSize, LastActionProbs[i], 0, Model.ActionSize);
                }
                
            }
            else
            {
                float[] actionProbs = null;
                int[] tempAction = Model.EvaluateActionDiscrete(statesAll, out actionProbs, true);
                for (int i = 0; i < NumberOfActor; ++i)
                {
                    actions[i] = new float[] { tempAction[i] };
                    LastAction[i] = actions[i];
                    LastActionProbs[i] = new float[] { actionProbs[i] };
                }
            }
            for (int i = 0; i < NumberOfActor; ++i)
            {
                LastValue[i] = Model.EvaluateValue(statesAll)[i];
            }

            environment.Step(actions);
            Steps++;
        }

        /// <summary>
        /// called after step and when the enviorment is resolved. return whether the enviourment should reset
        /// </summary>
        /// <param name="environment"></param>
        public bool Record(IRLEnvironment environment)
        {
            Debug.Assert(environment.IsResolved());
            bool isEnd = environment.IsEnd();

            for (int i = 0; i < NumberOfActor; ++i)
            {
                float reward = environment.LastReward();
                AddHistory(LastState[i], reward, LastAction[i], LastActionProbs[i], LastValue[i], i);
            }

            if (isEnd || environment.CurrentStep() >= MaxStepHorizon) {
                float[] nextValues = new float[NumberOfActor];
                if (!isEnd)
                {
                    nextValues = Model.EvaluateValue(environment.CurrentState());
                }
                else
                {
                    for (int i = 0; i < NumberOfActor; ++i)
                    {
                        nextValues[i] = 0;
                    }
                }

                for (int i = 0; i < NumberOfActor; ++i)
                {
                    ProcessEpisodeHistory(nextValues[i],i);
                }
                
                return true;
            }
            return false;
        }


        public void TrainAllData(int batchsize, int epoch)
        {
            var fetches = new List<Tuple<string, int, string>>();
            fetches.Add(new Tuple<string, int, string>("State", 0, "State"));
            fetches.Add(new Tuple<string, int, string>("Action", 0, "Action"));
            fetches.Add(new Tuple<string, int, string>("ActionProb", 0, "ActionProb"));
            fetches.Add(new Tuple<string, int, string>("TargetValue", 0, "TargetValue"));
            fetches.Add(new Tuple<string, int, string>("Advantage", 0, "Advantage"));

            float lossEpoch = 0;
            float lossMean = 0;
            for (int i = 0; i < epoch; ++i)
            {
                var samples = dataBuffer.SampleBatchesReordered(batchsize, fetches.ToArray());
                float[] states = (float[])samples["State"];
                float[] actions = (float[])samples["Action"];
                float[] actionProbs = (float[])samples["ActionProb"];
                float[] targetValues = (float[])samples["TargetValue"];
                float[] advantages = (float[])samples["Advantage"];

                int batchCount = targetValues.Length / batchsize;
                int actionUnitSize = (Model.IsActionContinuous ? Model.ActionSize : 1);
                for (int j =0;j < batchCount; ++j)
                {
                    TrainBatch(SubArray(states,j*batchsize*Model.StateSize, batchsize * Model.StateSize), 
                        SubArray(actions, j * batchsize * actionUnitSize, batchsize * actionUnitSize),
                        SubArray(actionProbs, j * batchsize * actionUnitSize, batchsize * actionUnitSize),
                        SubArray(targetValues, j * batchsize, batchsize),
                        SubArray(advantages, j * batchsize, batchsize));
                    lossEpoch += LastLoss;
                }
                lossMean += lossEpoch / batchCount;
            }
            LastLoss = lossMean / epoch;
        }

        public void TrainRandomBatch(int batchSize)
        {
            var fetches = new List<Tuple<string, int, string>>();
            fetches.Add(new Tuple<string, int, string>("State", 0, "State"));
            fetches.Add(new Tuple<string, int, string>("Action", 0, "Action"));
            fetches.Add(new Tuple<string, int, string>("ActionProb", 0, "ActionProb"));
            fetches.Add(new Tuple<string, int, string>("TargetValue", 0, "TargetValue"));