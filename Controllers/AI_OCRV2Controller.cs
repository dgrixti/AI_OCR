using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AI_OCR.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AI_OCR.Helper;
using Newtonsoft.Json;

namespace AI_OCR.Controllers
{
    public class AI_OCRV2Controller : BaseAIController
    {
        /// private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        private static string DATASET_FILE_1 = "";
        private static string DATASET_FILE_2 = ""; // cw2DataSet3
        private static string MLP_STATE_FILE = "";

        private static List<string> DATASET_FILE_CONTENT_1 = new List<string>();
        private static List<string> DATASET_FILE_CONTENT_2 = new List<string>();

        private static MultiLayeredPerceptronClassifier mlp = null;


        public AI_OCRV2Controller(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            /// _logger = logger;
            _env = env;

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath; // with wwwroot

            DATASET_FILE_1 = Path.Combine(contentRootPath, "dataset", "cw2DataSet1.csv");
            DATASET_FILE_2 = Path.Combine(contentRootPath, "dataset", "cw2DataSet2.csv");
            MLP_STATE_FILE = Path.Combine(contentRootPath, "dataset", "mlpstate.json");
        }

        public IActionResult Index()
        {

            // MLP TEST
            /// RunTwoFoldTestMLP();

            // KNN TEST
            /// RunTwoFoldTestKNN();

            return View();
        }

        private void RunTwoFoldTestKNN()
        {

            List<OCRCharacter> charactersTrain = new List<OCRCharacter>();

            List<OCRCharacter> charactersTest = new List<OCRCharacter>();

            // Load the training file points
            loadDataFromFile(charactersTrain, DATASET_FILE_1, DATASET_FILE_CONTENT_1);

            // Load the training file points from dataset 2 as well.
            loadDataFromFile(charactersTest, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

            bool useDistanceScoring = false;

            KNNClassifier.processKNN(charactersTrain, charactersTest, new EuclideanDistance(), 4, useDistanceScoring);
        }

        private void RunTwoFoldTestMLP()
        {
            float accuracy = runMLP(-1);
        }

        [HttpPost]
        public IActionResult PredictCharacterMLP(string data)
        {
            if (mlp == null)
            {
                runMLP(-1);
            }

            OCRCharacter charGuess = createCharacterFromBytes(data);
            int guess = mlp.recallNetworkGuess(charGuess);
            return Json(guess.ToString());
        }

        [HttpPost]
        public IActionResult PredictCharacterKNN(string data)
        {
            List<OCRCharacter> charactersTrain = new List<OCRCharacter>();

            List<OCRCharacter> charactersTest = new List<OCRCharacter>();

            // Load the training file points
            loadDataFromFile(charactersTrain, DATASET_FILE_1, DATASET_FILE_CONTENT_1);

            // Load the training file points from dataset 2 as well.
            loadDataFromFile(charactersTrain, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

            // Load the training file points from dataset 2 as well.
            ///loadDataFromFile(charactersTest, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

            bool useDistanceScoring = true;

            OCRCharacter charGuess = createCharacterFromBytes(data);
            int guess = KNNClassifier.processKNNAndPredict(charactersTrain, charGuess, new EuclideanDistance(), 4, useDistanceScoring);
            return Json(guess.ToString());
        }

        /**
          *  Multilayer perceptron function.
          * 
          * @param trainDatasetLimit: Apply limit on training dataset for each character 1 - 275 or -1 for MAX.
          * @throws Exception 
          */
        private float runMLP(int trainDatasetLimit)
        {
            List<OCRCharacter> charactersTrain = new List<OCRCharacter>();

            List<OCRCharacter> charactersTest = new List<OCRCharacter>();

            // Load the training file points
            loadDataFromFile(charactersTrain, DATASET_FILE_1, DATASET_FILE_CONTENT_1);

            // Load the training file points from dataset 2 as well.
            loadDataFromFile(charactersTest, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

            int limit = trainDatasetLimit <= 0 ? 280 : trainDatasetLimit;

            List<OCRCharacter> charactersTrainFiltered =
                ResultFilter.Instance.limitResults(charactersTrain, limit);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            
            
            bool saveState = true;

            // Load ready config from file if present.
            if (System.IO.File.Exists(MLP_STATE_FILE))
            {
                loadMLPState();
                saveState = false;
            }

            // Create a static singleton instance
            mlp = MultiLayeredPerceptronClassifier.Instance;
       
            if (!mlp.isTrained)
            {
                mlp.createNetwork();
                Console.WriteLine("Found number of images: " + charactersTrainFiltered.Count + "\n");
                mlp.trainNetwork(0.04f, 0.01f, 0.4f, charactersTrainFiltered, 500);
            }

            int correct = 0;
            foreach (OCRCharacter entry in charactersTest)
            {
                correct += mlp.recallNetwork(entry) ? 1 : 0;
            }

            stopwatch.Stop();
            long estimatedTime = stopwatch.ElapsedMilliseconds; //System.nanoTime() - startTime;

            // Show accuracy results.
            Console.WriteLine(correct + " / " + charactersTest.Count);

            float accuracy = (float)(correct * 100) / charactersTest.Count;
            Console.WriteLine(accuracy + " % accuracy.");

            // Time elapsed
            Console.WriteLine("Execution time is " + ((float)estimatedTime / 1000000000).ToString("#0.000") + " seconds");

            // Save to file, to avoid long load time.
            if (saveState)
            {
                saveMLPState(mlp, accuracy);  
            }
            /// saveMLPState(mlp, accuracy);

            return accuracy;
        }


        private void saveMLPState(MultiLayeredPerceptronClassifier mlp, float accuracy)
        {
            MLPState mstate = new MLPState() 
            {
                inputNeuronsArr =  mlp.inputNeuronsArr,
                hiddenNeuronsArr = mlp.hiddenNeuronsArr,
                outputNeuronsArr = mlp.outputNeuronsArr,
                weightsArr = mlp.weightsArr,
                tempWeightsArr = mlp.tempWeightsArr,
                prevWeightsArr = mlp.prevWeightsArr,
                inputs = mlp.inputs,
                outputs = mlp.outputs,
                hiddenLayers = mlp.hiddenLayers,
                hiddenNeurons = mlp.hiddenNeurons,
                meanSqErr = mlp.meanSqErr,
                epochs = mlp.epochs,
                error = mlp.error
            };
           
            // write the data (overwrites)
            using (var stream = new StreamWriter(MLP_STATE_FILE, append: false)) ///  + "_" + accuracy
            {
                stream.Write(JsonConvert.SerializeObject(mstate));
            }
        }

        private void loadMLPState()
        {
            using (var stream = new StreamReader(MLP_STATE_FILE))
            {
                MLPState mstate = JsonConvert.DeserializeObject<MLPState>(stream.ReadToEnd());

                // Load state from file.
                MultiLayeredPerceptronClassifier.CreateFromFile(mstate);
            }
        }

    }
}
