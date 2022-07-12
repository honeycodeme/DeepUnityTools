using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNTK.CSTrainingExamples
{
    /// <summary>
    /// This class shows how the train and evaluate a Logistic Regression classifier.
    /// Data are randomly generated into 2 classes with statistically separable features.
    /// See https://github.com/Microsoft/CNTK/blob/master/Tutorials/CNTK_101_LogisticRegression.ipynb for more details.
    /// </summary>
    public class LogisticRegression
    {
        static int inputDim = 3;
        static int numOutputClasses = 2;

        static public void TrainAndEvaluate(DeviceDescriptor device)
        {
            try
            {

                // build a logistic regression model
                Variable featureVariable, labelVariable;

                featureVariable = Variable.InputVariable(new int[] { inputDim }, DataType.Float);
                labelVariable = Variable.InputVariable(new int[] { numOutputClasses }, DataType.Float);



                var classifierOutput = CreateLinearModel(featureVariable, numOutputClasses, device);

                var loss = CNTKLib.CrossEntropyWithSoftmax(classifierOutput, labelVariable);
                var evalError = CNTKLib.ClassificationError(classifierOutput, labelVariable);
                UnityEngine.Debug.Log("Model built");

                // prepare for training
                CNTK.TrainingParameterScheduleDouble learningRatePerSample = new CNTK.TrainingParameterScheduleDouble(0.02, 1);
                UnityEngine.Debug.Log("Training parameter created");
                IList<Learner> parameterLearners =
                    new List<Learner>() { Learner.SGDLearner(classifierOutput.Parameters(), learningRatePerSample) };
                UnityEngine.Debug.Log("Learners created");
                
                var trainer = Trainer.CreateTrainer(classifierOutput, loss, evalError, parameterLearners);

                int minibatchSize = 64;
                int numMinibatchesToTrain = 1000;
                int updatePerMinibatches = 50;

                UnityEngine.Debug.Log("Trainer created");
                

        