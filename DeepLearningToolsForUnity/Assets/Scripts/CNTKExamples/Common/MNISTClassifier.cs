﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CNTK.CSTrainingExamples
{
    /// <summary>
    /// This class shows how to build and train a classifier for handwritting data (MNIST).
    /// For more details, please follow a serial of tutorials below:
    /// https://github.com/Microsoft/CNTK/blob/master/Tutorials/CNTK_103A_MNIST_DataLoader.ipynb
    /// https://github.com/Microsoft/CNTK/blob/master/Tutorials/CNTK_103B_MNIST_LogisticRegression.ipynb
    /// https://github.com/Microsoft/CNTK/blob/master/Tutorials/CNTK_103C_MNIST_MultiLayerPerceptron.ipynb
    /// https://github.com/Microsoft/CNTK/blob/master/Tutorials/CNTK_103D_MNIST_ConvolutionalNeuralNetwork.ipynb
    /// </summary>
    public class MNISTClassifier
    {
        /// <summary>
        /// Execution folder is: CNTK/x64/BuildFolder
        /// Data folder is: CNTK/Tests/EndToEndTests/Image/Data
        /// </summary>
        public static string ImageDataFolder = "../../Tests/EndToEndTests/Image/Data";

        /// <summary>
        /// Train and evaluate a image classifier for MNIST data.
        /// </summary>
        /// <param name="device">CPU or GPU device to run training and evaluation</param>
        /// <param name="useConvolution">option to use convolution network or to use multilayer perceptron</param>
        /// <param name="forceRetrain">whether to override an existing model.
        /// if true, any existing model will be overridden and the new one evaluated. 
        /// if false and there is an existing model, the existing model is evaluated.</param>
        public static void TrainAndEvaluate(DeviceDescriptor device, bool useConvolution, bool forceRetrain)
        {
            var featureStreamName = "features";
            var labelsStreamName = "labels";
            var classifierName = "classifierOutput";
            Function classifierOutput;
            int[] imageDim = useConvolution ? new int[] { 28, 28, 1 } : new int[] { 784 };
            int imageSize = 28 * 28;
            int numClasses = 10;

            IList<StreamConfiguration> streamConfigurations = new StreamConfiguration[]
                { new StreamConfiguration(featureStreamName, imageSize), new StreamConfiguration(labelsStreamName, numClasses) };

            string modelFile = useConvolution ? "MNISTConvolution.model" : "MNISTMLP.model";

            // If a model already exists and not set to force retrain, validate the model and return.
            if (File.Exists(modelFile) && !forceRetrain)
            {
                var minibatchSourceExistModel = MinibatchSource.TextFormatMinibatchSource(
                    Path.Combine(ImageDataFolder, "Test_cntk_text.txt"), streamConfigurations);
                TestHelper.ValidateModelWithMinibatchSource(modelFile, minibatchSourceExistModel,
                                    imageDim, numClasses, featureStreamName, labelsStreamName, classifierName, device);
                return;
            }

            // build the network
            var input = CNTKLib.InputVariable(imageDim, DataType.Float, featureStreamName);
            if (useConvolution)
            {
                var scaledInput = CNTKLib.ElementTimes(Constant.Scalar<float>(0.00390625f, device), input);
                classifierOutput = CreateConvolutionalNeuralNetwork(scaledInput, numClasses, device, classifierName);
            }
            else
            {
                // For MLP, we like to have the middle layer to have certain amount of states.
                int hiddenLayerDim = 200;
                var scaledInput = CNTKLib.ElementTimes(Constant.Scalar<float>(0.00390625f, device), input);
                classifierOutput = CreateMLPClassifier(device, numClasses, hiddenLayerDim, scaledInput, classifierName);
            }

            var labels = CNTKLib.InputVariable(new int[] { numClasses }, DataType.Float, labelsStreamName);
            var trainingLoss = CNTKLib.CrossEntropyWithSoftmax(new Variable(classifierOutput), labels, "lossFunction");
            var prediction = CNTKLib.ClassificationError(new Variable(classifierOutput), labels, "classificationError");

            // prepare training data
            var minibatchSource = MinibatchSource.TextFormatMinibatchSource(
                Path.Combine(ImageDataFolder, "Train_cntk_text.txt"), streamConfigurations, MinibatchSource.InfinitelyRepeat);

            var featureStreamInfo = minibatchSource.StreamInfo(featureStreamName);
            var labelStreamInfo = minibatchSource.StreamInfo(labelsStreamName);

            // set per sample learning rate
            CNTK.TrainingParameterScheduleDouble learningRatePerSample = new CNTK.TrainingParameterScheduleDouble(
                0.003125, 1);

            IList<Learner> parameterLearners = new List<Learner>() { Learner.SGDLearner(classifierOutput.Parameters(), learningRatePerSample) };
            var trainer = Trainer.CreateTrainer(classifierOutput, trainingLoss, prediction, parameterLearners);

            //
            const uint minibatchSize = 64;
            int outputFrequencyInMinibatches = 20, i = 0;
            int epochs = 5;
            while (epochs > 0)
            {
                var minibatchData = minibatchSource.GetNextMinibatch(minibatchSize, device);
                var arguments = new Dictionary<Variable, MinibatchData>
                {
                    { input, minibatchData[featureStreamInfo] },
                    { labels, minibatchData[labelStreamInfo] }
                };

                trainer.TrainMinibatch(arguments, device);
                TestHelper.PrintTrainingProgress(trainer, i++, outputFrequencyInMinibatches);

                // MinibatchSource is created with MinibatchSource.InfinitelyRepeat.
                // Batching will not end. Each time minibatchSource completes an sweep (epoch),
                // the last minibatch data will be marked as end of a sweep. We use this flag
                // to count number of epochs.
                if (TestHelper.MiniBatchDataIsSweepEnd(minibatchData.Values))
                {
                    epochs--;
                }
            }

            // save the trained model
            classifierOutput.Save(modelFile);

            // validate the model
            var minibatchSourceNewModel = MinibatchSource.TextFormatMinibatchSource(
                Path.Combine(ImageDataFolder, "Test_cntk_text.txt"), streamConfigurations, MinibatchSource.FullDat