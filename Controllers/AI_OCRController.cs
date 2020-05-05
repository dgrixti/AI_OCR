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
    public class AI_OCRController : Controller
    {
        /// private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        private static string DATASET_FILE_1 = "";
        private static string DATASET_FILE_2 = ""; // cw2DataSet3

        private static List<string> DATASET_FILE_CONTENT_1 = new List<string>();
        private static List<string> DATASET_FILE_CONTENT_2 = new List<string>();

        private static object lockfile1 = new object(); 

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

                return answer.ToString() ;

            }
            catch (Exception e)
            {
                return "ERROR " + e.Message + " FILE: " + DATASET_FILE_1;
            }
        }
        private static void loadDataFromFile(List<OCRCharacter> cities, String fileName,
            List<string> dataSet)
        {
            try
            {
                // if already full
                if (dataSet != null && dataSet.Count > 0)
                {
                    loadDataFromDataset(cities, dataSet);
                    return;
                }
                else
                {
                    lock (lockfile1)
                    {
                        // Read the file and display it line by line.  
                        System.IO.StreamReader file =
                        new System.IO.StreamReader(fileName);

                        int counter = 0;
                        string line;

                        // Cache data in array so it's loaded from file once.
                        while ((line = file.ReadLine()) != null)
                        {
                            //System.Console.WriteLine(line);

                            // add to memory so that the physical file isnt locked for each thread.
                            dataSet.Add(line);
                            counter++;
                        }

                        file.Close();
                    }

                    loadDataFromDataset(cities, dataSet);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void loadDataFromDataset(List<OCRCharacter> cities, List<string> dataSet)
        {
            foreach (string line in dataSet)
            {
                addCharacter(cities, line, true);
            }
        }

        private static void addCharacter(List<OCRCharacter> characters, String line, bool withAnswer)
        {
            String[] vars = line.Split(",");
            double[] pointsArr = new double[vars.Length - 1];
            for (int i = 0; i < vars.Length - 1; i++)
            {
                pointsArr[i] = Double.Parse(vars[i]);
            }

            // Find the answer from the sequence (training mode) or not (predicting mode).
            if (withAnswer)
            {
                characters.Add(new OCRCharacter(pointsArr, Int32.Parse(vars[vars.Length - 1])));
            }
            else
            {
                characters.Add(new OCRCharacter(pointsArr, -1)); // -1 because we dont know what it is
            }
        }
    }
}
