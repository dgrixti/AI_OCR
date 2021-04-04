using AI_OCR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.AI.TSP
{
    public class TSPBasic
    {
        private static AI.TSP.TSPRoute Winner { get; set; }

        public static List<AI.TSP.TSPRoute> TspRoutes { get; set; }

        public static AI.TSP.TSPRoute CalculateTSP(IList<IList<string>> perms, City[] cities)
        {
            TspRoutes = new List<TSPRoute>();

            foreach (IList<string> perm in perms)
            {
                List<City> citiesList = new List<City>();
                foreach (string s in perm)
                {
                    City c = cities[Int32.Parse(s) - 1];
                    citiesList.Add(c);
                }

                AI.TSP.TSPRoute route = new AI.TSP.TSPRoute(citiesList);
                double dist = route.CalculateDistance();

                Console.Write("Added route: ");
                foreach (City city in route.Cities)
                {
                    Console.Write(city.Id.ToString() + ", ");
                }
                Console.Write("  with distance: " + dist.ToString("#.00"));
                Console.WriteLine();

                TspRoutes.Add(route);

                if (Winner == null)
                    Winner = route;
                else
                    Winner = (route.TotalDist < Winner.TotalDist) ? route : Winner;
            }

            return Winner;
        }
    }
}
