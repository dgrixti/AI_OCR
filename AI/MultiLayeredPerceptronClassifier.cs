using AI_OCR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AI_OCR
{
    /**
     * All the methods serve to create a custom layered multilayer perceptron
     * or a deep learning neural network.     
     *      
     * Originally programed in Java and now translated to C#.
     * 
     * @author Darren Grixti
     */
    public class MultiLayeredPerceptronClassifier
    {
        public int inputs = 0;
        public int outputs = 0;
        public int hiddenLayers = 0;
        public int hiddenNeurons = 0;

        public float[] inputNeuronsArr;
        public float[] hiddenNeuronsArr;
        public float[] outputNeuronsArr;
        public float[] weightsArr;

        public float meanSqErr = 0.0f;
        public int epochs = 0;
        public float error = 0.0f;

        // Temporary arrays for storing previous weights while calculating network.
        public float[] tempWeightsArr;
        public float[] prevWeightsArr;

        public bool isTrained = false;

        private static MultiLayeredPerceptronClassifier instance = null;
        private static readonly object padlock = new object();

        /* 
         * Restrict to singleton initialisaiton for class
        */
        public static MultiLayeredPerceptronClassifier Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MultiLayeredPerceptronClassifier(64, 10, 3, 52); 
                    }
                    return instance;
                }
            }
        }

        //public static MultiLayeredPerceptronClassifier RESET
        //{
        //    get
        //    {
        //        lock (padlock)
        //        {
        //            instance = new MultiLayeredPerceptronClassifier(64, 10, 3, 52);
        //            instance.isTrained = false;
        //            return instance;
        //        }
        //    }
        //}

        /* 
         * Load a MLP from a saved state from file.
        */
        public static void CreateFromFile(MLPState mstate)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new MultiLayeredPerceptronClassifier(64, 10, 3, 52); 

                    instance.inputNeuronsArr = mstate.inputNeuronsArr;
                    instance.hiddenNeuronsArr = mstate.hiddenNeuronsArr;
                    instance.outputNeuronsArr = mstate.outputNeuronsArr;
                    instance.weightsArr = mstate.weightsArr;

                    instance.tempWeightsArr = mstate.tempWeightsArr;
                    instance.prevWeightsArr = mstate.prevWeightsArr;

                    instance.inputs = mstate.inputs;
                    instance.outputs = mstate.outputs;
                    instance.hiddenLayers = mstate.hiddenLayers;
                    instance.hiddenNeurons = mstate.hiddenNeurons;

                    instance.meanSqErr = mstate.meanSqErr;
                    instance.epochs = mstate.epochs;
                    instance.error = mstate.error;

                    instance.isTrained = true;
                }
            }
        }

        private MultiLayeredPerceptronClassifier() { }
        
        /**
         * Create new multi layer perceptron class from scratch.
         * 
         * @param inputs: How many input neurons.
         * @param outputs: How many output neurons.
         * @param hiddenLayers: How many hidden layers.
         * @param hiddenNeurons: How many hidden neurons.
         */
        private MultiLayeredPerceptronClassifier(int inputs, int outputs, int hiddenLayers, int hiddenNeurons)
        {
            this.inputs = inputs;
            this.outputs = outputs;
            this.hiddenLayers = hiddenLayers;
            this.hiddenNeurons = hiddenNeurons;

            // Create input neurons arr.
            inputNeuronsArr = new float[inputs];

            // Create hidden neurons arr.
            int hiddenNeuronsTotal = hiddenNeurons * hiddenLayers;
            hiddenNeuronsArr = new float[hiddenNeuronsTotal];

            // Create output neurons arr.
            outputNeuronsArr = new float[outputs];

            meanSqErr = 999.0f;
            epochs = 1;
            error = 0.0f;
        }

        /**
         * Given the 64 bit OCR data for binarised image and the 
         * expected outcome value, tests the network for a correct output.
         * 
         * Runs the 64 bit inputs through the network and checks
         * which output it gets.
         * 
         * @param character: OCRCharacter obj with 64 bit numeric inputs 
         *                    representing the OCR data & identifier. 
         * @return good or bad (true or false).
         */
        public bool recallNetwork(OCRCharacter character)
        {
            // Populate the input neurons. //
            int answer = character.getIdentifier();

            int index = recallNetworkGuess(character);

            bool correct = index == answer ? true : false;

            // Output feedback 
            Console.WriteLine("Input data for: " + answer + " / output: "
                    + index + " (" + answer + ")" + " correct: " + correct);

            return correct;
        }

        public int recallNetworkGuess(OCRCharacter character)
        {
            // Insert the 64 bit OCR data for the image as inputs.
            double[] imgData = character.getPoints();
            for (int i = 0; i < imgData.Length; i++)
            {
                inputNeuronsArr[i] = (float)imgData[i];
            }

            // Calculate the network
            calculateNetwork();

            float winner = -1;
            int index = 0;

            // Find the best fitting output
            for (int i = 0; i < outputs; i++)
            {
                if (outputNeuronsArr[i] > winner)
                {
                    winner = outputNeuronsArr[i];
                    index = i;
                }
            }

            return index;
        }

        /**
         * Uses the values specified in the constructor to create 
         * the network with the defined layers and hidden neurons
         * setting each neuron with a random value between -0.5 and 0.5.
         */
        public void createNetwork()
        {
            // Calculate the total number of neurons required in the MLP
            // by multiplying all N layer neurons by number of hidden neurons.
            int neuronsTotalNo =
                    inputs * hiddenNeurons +
                    (hiddenNeurons * hiddenNeurons * (hiddenLayers - 1))
                    + outputs * hiddenNeurons;

            // Create the weights arr for each neuron weight.
            weightsArr = new float[neuronsTotalNo];

            // Set a random number from -0.5 to 0.5 for each neuron.
            for (int i = 0; i < neuronsTotalNo; i++)
            {
                weightsArr[i] = (float)NextFloatNo(-0.5, 0.5);
            }

            Console.WriteLine("Total number of neurons produced: " + neuronsTotalNo);
        }

        public static double NextFloatNo(double minimum, double maximum)
        {
            Random random = new Random();
            double ret = random.NextDouble() * (maximum - minimum) + minimum;
            return ret;
        }

        /**
         * Trains the multilayer perceptron by using the training image data and 
         * the expected output.
         * 
         * Iterates each image 64 bit data and passes through all the layers forward
         * and then checks against expected output the error difference and uses
         * back-propagation in order to fix the weights of the neurons via the calculated
         * delta function of error.
         * 
         * @param learningStep: The rate of learning of the algorithm.
         * @param lmse: Least mean square error required to stop the training.
         * @param learningMomentum: The additional learning rate used at the 
         *  beginning of learning to make learning faster. 
         * @param charactersTrainList: List of image data for OCR and their representation value. 
         * @param maxEpochs: Maximum iterations for training if lmse is not reached.
         * 
         * @return true/false success or not. 
         */
        public bool trainNetwork(float learningStep, float lmse,
                float learningMomentum, List<OCRCharacter> charactersTrainList, int maxEpochs)
        {

            int hiddenNeuronsTotal = hiddenNeurons * hiddenLayers;

            // Define the output and hidden deltas.
            float[] outputDelta = new float[outputs];
            float[] hiddenDelta = new float[hiddenNeuronsTotal];

            // Create temporary array with values from original wights array
            tempWeightsArr = weightsArr.Take(weightsArr.Length).ToArray();

            // Create temporary array with values from original wights array
            // where to store previous weights between one layer propagation and another.
            prevWeightsArr = weightsArr.Take(weightsArr.Length).ToArray();

            int outputExpected = 0;

            while (Math.Abs(meanSqErr - lmse) > 0.0001f)
            {

                // For each epoch reset the mean square error
                meanSqErr = 0.0f;

                //for each file
                foreach (OCRCharacter entry in charactersTrainList)
                {

                    // Determine expected output for this image.
                    outputExpected = entry.getIdentifier();

                    // Insert the 64 bit OCR data for the image as inputs.
                    double[] imgData = entry.getPoints();
                    for (int i = 0; i < imgData.Length; i++)
                    {
                        inputNeuronsArr[i] = (float)imgData[i];
                    }

                    // Run a network calculation.
                    calculateNetwork();

                    // Back-probagation section. 
                    performBackPropagation(outputExpected, outputDelta, hiddenDelta);

                    // Weights modification section. 
                    performWeightsAdjustment(learningStep, learningMomentum, outputDelta, hiddenDelta);

                    // Add to the total mean square error for this epoch.
                    meanSqErr += error / (outputs + 1f);

                    // Reset error to 0 for the next iteration
                    error = 0.0f;
                }

                // Report progress
                // if (epochs % 100 == 0) {
                if (epochs % 10 == 0)
                {
                    Console.WriteLine(epochs + " - " + meanSqErr);
                }

                if (epochs > maxEpochs)
                    break;

                epochs++;
            }

            isTrained = true;

            return true;
        }

        /**
         * Adjusts the weights in each layer of the network to get the final 
         * results/actual output closer to the desired output.
         * 
         */
        private void performWeightsAdjustment(float learningStep, float learningMomentum,
                float[] outputDelta, float[] hiddenDelta)
        {

            // Store previous weights in temp weights arr for later use.
            ///tempWeightsArr = Arrays.copyOf(weightsArr, weightsArr.length);
            tempWeightsArr = weightsArr.Take(weightsArr.Length).ToArray();

            // Adjust hidden neuron to input neurons weights.
            for (int i = 0; i < inputs; i++)
            {
                for (int j = 0; j < hiddenNeurons; j++)
                {
                    weightsArr[inputs * j + i] +=
                            (learningMomentum * (inputToHidden(i, j) - prevInputToHidden(i, j))) +
                                    (learningStep * hiddenDelta[j] * inputNeuronsArr[i]);
                }
            }

            // Hidden to hidden weights, provided more than 1 layer exists
            for (int i = 2; i <= hiddenLayers; i++)
            {
                for (int j = 0; j < hiddenNeurons; j++)
                { // From
                    for (int k = 0; k < hiddenNeurons; k++)
                    { // To
                        weightsArr[inputs * hiddenNeurons + ((i - 2) * hiddenNeurons * hiddenNeurons) + hiddenNeurons * j + k] +=
                                (learningMomentum * (hiddenToHidden(i, j, k) - prevHiddenToHidden(i, j, k))) +
                                        (learningStep * hiddenDelta[(i - 1) * hiddenNeurons + k] * hiddenAt(i - 1, j));
                    }
                }
            }

            // Last hidden layer to output weights
            for (int i = 0; i < outputs; i++)
            {
                for (int j = 0; j < hiddenNeurons; j++)
                {
                    weightsArr[(inputs * hiddenNeurons + (hiddenLayers - 1) * hiddenNeurons * hiddenNeurons + j * outputs + i)] +=
                            (learningMomentum * (hiddenToOutput(j, i) - prevHiddenToOutput(j, i))) +
                                    (learningStep * outputDelta[i] * hiddenAt(hiddenLayers, j));
                }
            }

            ///prevWeightsArr = Arrays.copyOf(tempWeightsArr, tempWeightsArr.length);
            prevWeightsArr = weightsArr.Take(weightsArr.Length).ToArray();
        }

        /**
         * Back-propagation, a procedure to repeatedly adjust the weights so as to 
         * minimize the difference between actual output and desired output.
         * 
         */
        private void performBackPropagation(int outputExpected, float[] outputDelta, float[] hiddenDelta)
        {
            // Perform back-probagation from output layer to hiddden layer.
            for (int i = 0; i < outputs; i++)
            {
                // Calculate the the delta of the output layer and 
                // the accumulated error against expected output.
                if (i != outputExpected)
                {
                    outputDelta[i] = (0.0f - outputNeuronsArr[i]) * dersigmoid(outputNeuronsArr[i]);
                    error += (0.0f - outputNeuronsArr[i]) * (0.0f - outputNeuronsArr[i]);
                }
                else
                {
                    outputDelta[i] = (1.0f - outputNeuronsArr[i]) * dersigmoid(outputNeuronsArr[i]);
                    error += (1.0f - outputNeuronsArr[i]) * (1.0f - outputNeuronsArr[i]);
                }
            }

            // Continued backwards-propagation  now, to get the error of each neuron
            // in every layer and calculate the delta of the last hidden layer first.
            for (int i = 0; i < hiddenNeurons; i++)
            {

                // Reset the values from the previous iteration to 0.
                hiddenDelta[(hiddenLayers - 1) * hiddenNeurons + i] = 0.0f;

                // Add to the delta for each connection with an output neuron
                for (int j = 0; j < outputs; j++)
                {
                    hiddenDelta[(hiddenLayers - 1) * hiddenNeurons + i] += outputDelta[j] * hiddenToOutput(i, j);
                }

                // The derivative here is only because of the
                // delta rule weight adjustment about to follow
                hiddenDelta[(hiddenLayers - 1) * hiddenNeurons + i] *= dersigmoid(hiddenAt(hiddenLayers, i));
            }

            // For each additional hidden layer, if exist
            for (int i = hiddenLayers - 1; i > 0; i--)
            {

                // Add to each neuron's hidden delta
                for (int j = 0; j < hiddenNeurons; j++)
                { // From

                    // Reset the values from the previous iteration to 0.
                    hiddenDelta[(i - 1) * hiddenNeurons + j] = 0.0f;

                    for (int k = 0; k < hiddenNeurons; k++)
                    { // To

                        // For each neuron, multiply the weights by previous hidden layers delta
                        hiddenDelta[(i - 1) * hiddenNeurons + j] += hiddenDelta[i * hiddenNeurons + k] * hiddenToHidden(i + 1, j, k);
                    }

                    // The derivative here is only because of the
                    // delta rule weight adjustment about to follow
                    hiddenDelta[(i - 1) * hiddenNeurons + j] *= dersigmoid(hiddenAt(i, j));
                }
            }
        }

        /**
         * Calculates the values of all neurons for the whole network, 
         * from input to hidden to output layers.
         * 
         * Iterates through all the layers forward, at each step calculating
         * the current value multiplied by the successive value and passing
         * the value final value through the activation function for nomalising the value.
         */
        public void calculateNetwork()
        {

            // Propagate forward from input layer towards the hidden layer.
            for (int hidden = 0; hidden < hiddenNeurons; hidden++)
            {
                // First layer, set values are set to 0 for this stage.
                hiddenNeuronsArr[hidden] = 0.0f;

                for (int input = 0; input < inputs; input++)
                {
                    hiddenNeuronsArr[hidden] += inputNeuronsArr[input] * inputToHidden(input, hidden);
                }

                // Finally pass it through the activation function
                hiddenNeuronsArr[hidden] = sigmoid(hiddenNeuronsArr[hidden]);
            }

            // For each other hidden layers, if exist propagate forward to other hidden layers.
            for (int i = 2; i <= hiddenLayers; i++)
            {

                // For each one of the extra layers calculate their values
                for (int j = 0; j < hiddenNeurons; j++)
                { // To

                    // Set the value from first layer's neuron to 0
                    hiddenNeuronsArr[(i - 1) * hiddenNeurons + j] = 0.0f;

                    for (int k = 0; k < hiddenNeurons; k++)
                    { // From
                        hiddenNeuronsArr[(i - 1) * hiddenNeurons + j] += hiddenNeuronsArr[(i - 2) * hiddenNeurons + k] * hiddenToHidden(i, k, j);
                    }

                    // Finally pass it through the activation function
                    hiddenNeuronsArr[(i - 1) * hiddenNeurons + j] = sigmoid(hiddenNeuronsArr[(i - 1) * hiddenNeurons + j]);
                }
            }

            // Apply from hidden to output neurons
            for (int i = 0; i < outputs; i++)
            {

                // Set the value from first layer's neuron to 0
                outputNeuronsArr[i] = 0.0f;

                for (int j = 0; j < hiddenNeurons; j++)
                {
                    outputNeuronsArr[i] += hiddenNeuronsArr[(hiddenLayers - 1) * hiddenNeurons + j] * hiddenToOutput(j, i);
                }

                // Finally pass it through the activation function
                outputNeuronsArr[i] = sigmoid(outputNeuronsArr[i]);
            }

        }

        // Sigmoid Function 
        private float sigmoid(float value)
        {
            return (float)(1f / (1f + Math.Exp(-value)));
        }

        // Derivative of Sigmoid Function
        private float dersigmoid(float value)
        {
            return (value * (1f - value));
        }

        private float prevInputToHidden(int input, int hidden)
        {
            return prevWeightsArr[inputs * hidden + input];
        }

        private float prevHiddenToHidden(int toLayer, int fromHidden, int toHidden)
        {
            return prevWeightsArr[inputs * hiddenNeurons + ((toLayer - 2) * hiddenNeurons * hiddenNeurons) + hiddenNeurons * fromHidden + toHidden];
        }

        private float prevHiddenToOutput(int hidden, int output)
        {
            return prevWeightsArr[inputs * hiddenNeurons + (hiddenLayers - 1) * hiddenNeurons * hiddenNeurons + hidden * outputs + output];
        }

        private float hiddenAt(int layer, int hidden)
        {
            return hiddenNeuronsArr[(layer - 1) * hiddenNeurons + hidden];
        }

        private float inputToHidden(int input, int hidden)
        {
            return weightsArr[inputs * hidden + input];
        }

        private float hiddenToHidden(int toLayer, int fromHidden, int toHidden)
        {
            return weightsArr[inputs * hiddenNeurons + ((toLayer - 2) * hiddenNeurons * hiddenNeurons) + hiddenNeurons * fromHidden + toHidden];
        }

        private float hiddenToOutput(int hidden, int output)
        {
            return weightsArr[(inputs * hiddenNeurons + (hiddenLayers - 1) * hiddenNeurons * hiddenNeurons + hidden * outputs + output)];
        }
    }
}
