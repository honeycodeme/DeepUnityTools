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

        static public void TrainAndEvaluate(De