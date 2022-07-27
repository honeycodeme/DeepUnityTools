using System.Collections.Generic;
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
      