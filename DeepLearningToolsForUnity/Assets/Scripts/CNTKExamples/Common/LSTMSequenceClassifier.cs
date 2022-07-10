
﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CNTK.CSTrainingExamples
{
    /// <summary>
    /// This class shows how to build a recurrent neural network model from ground up and train the model. 
    /// </summary>
    public class LSTMSequenceClassifier
    {
        /// <summary>
        /// Execution folder is: CNTK/x64/BuildFolder
        /// Data folder is: CNTK/Tests/EndToEndTests/Text/SequenceClassification/Data
        /// </summary>
        public static string DataFolder = "../../Tests/EndToEndTests/Text/SequenceClassification/Data";

        /// <summary>
        /// Build and train a RNN model.
        /// </summary>
        /// <param name="device">CPU or GPU device to train and run the model</param>
        public static void Train(DeviceDescriptor device)
        {
            const int inputDim = 2000;
            const int cellDim = 25;
            const int hiddenDim = 25;
            const int embeddingDim = 50;
            const int numOutputClasses = 5;

            // build the model
            var featuresName = "features";
            var features = Variable.InputVariable(new int[] { inputDim }, DataType.Float, featuresName, null, true /*isSparse*/);
            var labelsName = "labels";
            var labels = Variable.InputVariable(new int[] { numOutputClasses }, DataType.Float, labelsName,
                new List<Axis>() { Axis.DefaultBatchAxis() }, true);

            var classifierOutput = LSTMSequenceClassifierNet(features, numOutputClasses, embeddingDim, hiddenDim, cellDim, device, "classifierOutput");
            Function trainingLoss = CNTKLib.CrossEntropyWithSoftmax(classifierOutput, labels, "lossFunction");
            Function prediction = CNTKLib.ClassificationError(classifierOutput, labels, "classificationError");

            // prepare training data
            IList<StreamConfiguration> streamConfigurations = new StreamConfiguration[]
            { new StreamConfiguration(featuresName, inputDim, true, "x"), new StreamConfiguration(labelsName, numOutputClasses, false, "y") };
            var minibatchSource = MinibatchSource.TextFormatMinibatchSource(
                Path.Combine(DataFolder, "Train.ctf"), streamConfigurations,
                MinibatchSource.InfinitelyRepeat, true);
            var featureStreamInfo = minibatchSource.StreamInfo(featuresName);
            var labelStreamInfo = minibatchSource.StreamInfo(labelsName);

            // prepare for training
            TrainingParameterScheduleDouble learningRatePerSample = new TrainingParameterScheduleDouble(
                0.0005, 1);
            TrainingParameterScheduleDouble momentumTimeConstant = CNTKLib.MomentumAsTimeConstantSchedule(256);
            IList<Learner> parameterLearners = new List<Learner>() {
                Learner.MomentumSGDLearner(classifierOutput.Parameters(), learningRatePerSample, momentumTimeConstant, /*unitGainMomentum = */true)  };
            var trainer = Trainer.CreateTrainer(classifierOutput, trainingLoss, prediction, parameterLearners);

            // train the model
            uint minibatchSize = 200;
            int outputFrequencyInMinibatches = 20;
            int miniBatchCount = 0;
            int numEpochs = 5;
            while (numEpochs > 0)
            {
                var minibatchData = minibatchSource.GetNextMinibatch(minibatchSize, device);

                var arguments = new Dictionary<Variable, MinibatchData>
                {
                    { features, minibatchData[featureStreamInfo] },
                    { labels, minibatchData[labelStreamInfo] }
                };

                trainer.TrainMinibatch(arguments, device);
                TestHelper.PrintTrainingProgress(trainer, miniBatchCount++, outputFrequencyInMinibatches);

                // Because minibatchSource is created with MinibatchSource.InfinitelyRepeat, 
                // batching will not end. Each time minibatchSource completes an sweep (epoch),
                // the last minibatch data will be marked as end of a sweep. We use this flag
                // to count number of epochs.
                if (TestHelper.MiniBatchDataIsSweepEnd(minibatchData.Values))
                {
                    numEpochs--;
                }
            }
        }

        static Function Stabilize<ElementType>(Variable x, DeviceDescriptor device)
        {
            bool isFloatType = typeof(ElementType).Equals(typeof(float));
            Constant f, fInv;
            if (isFloatType)
            {
                f = Constant.Scalar(4.0f, device);
                fInv = Constant.Scalar(f.DataType, 1.0 / 4.0f);
            }
            else
            {
                f = Constant.Scalar(4.0, device);
                fInv = Constant.Scalar(f.DataType, 1.0 / 4.0f);
            }

            var beta = CNTKLib.ElementTimes(
                fInv,
                CNTKLib.Log( 
                    Constant.Scalar(f.DataType, 1.0) +  
                    CNTKLib.Exp(CNTKLib.ElementTimes(f, new Parameter(new NDShape(), f.DataType, 0.99537863 /* 1/f*ln (e^f-1) */, device)))));
            return CNTKLib.ElementTimes(beta, x);
        }

        static Tuple<Function, Function> LSTMPCellWithSelfStabilization<ElementType>( 
            Variable input, Variable prevOutput, Variable prevCellState, DeviceDescriptor device)
        {
            int outputDim = prevOutput.Shape[0];
            int cellDim = prevCellState.Shape[0];

            bool isFloatType = typeof(ElementType).Equals(typeof(float));
            DataType dataType = isFloatType ? DataType.Float : DataType.Double;

            Func<int, Parameter> createBiasParam;
            if (isFloatType)
                createBiasParam = (dim) => new Parameter(new int[] { dim }, 0.01f, device, "");
            else
                createBiasParam = (dim) => new Parameter(new int[] { dim }, 0.01, device, "");

            uint seed2 = 1;
            Func<int, Parameter> createProjectionParam = (oDim) => new Parameter(new int[] { oDim, NDShape.InferredDimension },
                    dataType, CNTKLib.GlorotUniformInitializer(1.0, 1, 0, seed2++), device);

            Func<int, Parameter> createDiagWeightParam = (dim) =>
                new Parameter(new int[] { dim }, dataType, CNTKLib.GlorotUniformInitializer(1.0, 1, 0, seed2++), device);

            Function stabilizedPrevOutput = Stabilize<ElementType>(prevOutput, device);
            Function stabilizedPrevCellState = Stabilize<ElementType>(prevCellState, device);

            Func<Variable> projectInput = () =>
                createBiasParam(cellDim) + (createProjectionParam(cellDim) * input);