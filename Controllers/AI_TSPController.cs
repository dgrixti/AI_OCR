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

        public IActionResult Index()
        {
           

            List<string> list = new List<string>() { "A", "B", "C", "D" };
            IList<IList<string>> perms = TSPHelper.Permutations(list);
            foreach (IList<string> perm in perms)
            {
                foreach (string s in perm)
                {
                    Console.Write(s);
                }
                Console.WriteLine();
            }

            // BUT THERE IS A LIMIT THAT EVEN COMPUTERS CANT HELP US WITH

            return View();
        }

       
    }
}