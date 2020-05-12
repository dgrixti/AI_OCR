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
using AI_OCR.ML.NET;

namespace AI_OCR.Controllers
{
    public class AI_OCRV3Controller : BaseAIController
    {
        /// private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        private static string DATASET_FILE_1 = "";
        private static string DATASET_FILE_2 = ""; // cw2DataSet3
        private static string ML_TRAINED_MODEL = "";

        private static List<string> DATASET_FILE_CONTENT_1 = new List<string>();
        private static List<string> DATASET_FILE_CONTENT_2 = new List<string>();


        public AI_OCRV3Controller(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            /// _logger = logger;
            _env = env;

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath; // with wwwroot

            DATASET_FILE_1 = Path.Combine(contentRootPath, "dataset", "cw2DataSet1.csv");
            DATASET_FILE_2 = Path.Combine(contentRootPath, "dataset", "cw2DataSet2.csv");
            ML_TRAINED_MODEL = Path.Combine(contentRootPath, "dataset", "mlTrainedModel.zip"); 
        }

        public IActionResult Index()
        {
            // ML Test
            RunTwoFoldTestML();
             
            MulticlassClassifier.RunTest(DATASET_FILE_1, DATASET_FILE_2, ML_TRAINED_MODEL);

            return View();
        }

        private void RunTwoFoldTestML()
        {
            List<OCRCharacter> charactersTrain = new List<OCRCharacter>();

            List<OCRCharacter> charactersTest = new List<OCRCharacter>();

            // Load the training file points
            loadDataFromFile(charactersTrain, DATASET_FILE_1, DATASET_FILE_CONTENT_1);

            // Load the training file points from dataset 2 as well.
            loadDataFromFile(charactersTest, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

            MulticlassClassifier.RunTest(DATASET_FILE_1, DATASET_FILE_2, ML_TRAINED_MODEL);

            MulticlassClassifier.processML(ML_TRAINED_MODEL, charactersTest);

            /// 2684 / 2810   ///95.516014 % accuracy.
        }

        [HttpPost]
        public IActionResult PredictCharacter(string data)
        {
            OCRCharacter charGuess = createCharacterFromBytes(data);
            int guess = MulticlassClassifier.predictChar(charGuess, ML_TRAINED_MODEL);
            return Json(guess.ToString());
        }
    }
}
