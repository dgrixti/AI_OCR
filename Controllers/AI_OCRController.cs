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

namespace AI_OCR.Controllers
{
    public class AI_OCRController : BaseAIController
    {
        /// private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        private static string DATASET_FILE_1 = "";
        private static string DATASET_FILE_2 = ""; // cw2DataSet3

        private static List<string> DATASET_FILE_CONTENT_1 = new List<string>();
        private static List<string> DATASET_FILE_CONTENT_2 = new List<string>();


        public AI_OCRController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            /// _logger = logger;
            _env = env;

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath; // with wwwroot

            DATASET_FILE_1 = Path.Combine(contentRootPath, "dataset", "cw2DataSet1.csv");
            DATASET_FILE_2 = Path.Combine(contentRootPath, "dataset", "cw2DataSet2.csv");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PredictCharacter(string data)
        {
            string answer = runNearesNeighbour(new EuclideanDistance(), data);
            return Json(answer);
        }

        private static string runNearesNeighbour(IDistance distCalc, string predictTest)
        {
            List<OCRCharacter> charactersTrain = new List<OCRCharacter>();
            List<OCRCharacter> charactersTest = new List<OCRCharacter>();
            addCharacter(charactersTest, predictTest, false);

            try
            {
                // Load the training file points
                loadDataFromFile(charactersTrain, DATASET_FILE_1, DATASET_FILE_CONTENT_1);

                // Load the training file points from dataset 2 as well.
                loadDataFromFile(charactersTrain, DATASET_FILE_2, DATASET_FILE_CONTENT_2);

                // Predict the number
                int answer = NearestNeighbourClassifier.processNNAndPredict(charactersTrain, charactersTest, distCalc);

                return answer.ToString();

            }
            catch (Exception e)
            {
                return "ERROR " + e.Message + " FILE: " + DATASET_FILE_1;
            }
        }
    }
}
