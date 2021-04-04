using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using AI_OCR.Helper;
using Microsoft.Extensions.Logging;
using AI_OCR.Models;
using Newtonsoft.Json;
using AI_OCR.AI.TSP;

namespace AI_OCR.Controllers
{
    public class AI_TSPController : BaseAIController
    {
        private readonly IWebHostEnvironment _env;

        private static string DATASET_FILE_1 = "";


        public AI_TSPController(ILogger<AI_TSPController> logger, IWebHostEnvironment env)
        {
            /// _logger = logger;
            _env = env;

            string contentRootPath = _env.ContentRootPath;
            string webRootPath = _env.WebRootPath; // with wwwroot

            DATASET_FILE_1 = Path.Combine(contentRootPath, "dataset", "test2tsp.txt");
        }

        [HttpPost]
        public IActionResult GetCities(string vers)
        {
            List<City> cities = TSPHelper.ParseCitiesFromFile(DATASET_FILE_1);
            return Json(cities.ToArray());
        }

        [HttpPost]
        public IActionResult GetPermutationsSimple(string vers)
        {
            List<String> result = new List<string>();

            List<string> list = new List<string>() { "1", "2", "3", "4" };
            IList<IList<string>> perms = TSPHelper.Permutations(list);
            foreach (IList<string> perm in perms)
            {
                string sequence = "";

                foreach (string s in perm)
                {
                    Console.Write(s);
                    sequence += s + ", ";
                }

                result.Add(sequence);
                Console.WriteLine();
            }

            return Json(result.ToArray());
        }

        public IActionResult Index()
        {
          
            // BUT THERE IS A LIMIT THAT EVEN COMPUTERS CANT HELP US WITH

            return View();
        }

        [HttpGet]
        public IActionResult FindRouteWithNNSimple()
        {
            // Get cities x,y coordiates from file
            List<City> cities = TSPHelper.ParseCitiesFromFile(DATASET_FILE_1);

            IEnumerable<City> filteredCities = cities.Take<City>(4);

            TSPNearestNeighbour nn = new TSPNearestNeighbour(filteredCities.ToList());
            nn.Execute();


            return Json(new
            {
                distance = nn.GetTotalDistanceRoute(),
                cityOrder = nn.GetCitiesResultToString()
            });

            // return Json(nn.GetCitiesResult());
        }

        [HttpGet]
        public IActionResult FindRouteWithNN()
        {
            // Get cities x,y coordiates from file
            List<City> cities = TSPHelper.ParseCitiesFromFile(DATASET_FILE_1);

            TSPNearestNeighbour nn = new TSPNearestNeighbour(cities);
            nn.Execute();


            return Json(new
            {
                distance = nn.GetTotalDistanceRoute(),
                cityOrder = nn.GetCitiesResultToString()
            });

            // return Json(nn.GetCitiesResult());
        }

        [HttpGet]
        public IActionResult FindRoute()
        {
            // Get cities x,y coordiates from file
            List<City> cities = TSPHelper.ParseCitiesFromFile(DATASET_FILE_1);

            // Take the first 4
            IEnumerable<City> filteredCities = cities.Take<City>(4);

            // Get permunations for 4 cities
            List<string> list = new List<string>() { "1", "2", "3", "4" };
            IList<IList<string>> perms = TSPHelper.Permutations(list);

            TSPRoute winner = TSPBasic.CalculateTSP(perms, filteredCities.ToArray());

            // print the results in console

            string output = "";

            Console.WriteLine();
            Console.WriteLine();
            output += "Winner route is: ";
            foreach (City city in winner.Cities)
            {
                output += city.Id.ToString() + ", ";
            }
            Console.WriteLine(output + "  with distance: " + winner.TotalDist.ToString("#.00"));

            return Json(new
            {
                distance = winner.TotalDist.ToString("#.00"),
                cityOrder = output
            });
        }
    }
}