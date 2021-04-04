using AI_OCR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.AI.TSP
{
    public class TSPRoute
    {
        private List<City> citiesList = new List<City>();
        private double routeDistance = 0;

        public TSPRoute(List<City> cities)
        {
            this.citiesList = cities;
        }

        public double CalculateDistance()
        {
            double totalDistance = 0;
            City previousCity = null;

            foreach (City city in citiesList)
            {
                if (previousCity == null)
                    previousCity = city;
                else
                    totalDistance += city.MeasureDistance(previousCity);
            }

            // Remember to add the last route from the last point back to home (first city)
            totalDistance += citiesList[citiesList.Count-1].MeasureDistance(citiesList[0]);

            // Add the first city to the last as the last hop to return home
            citiesList.Add(citiesList[0]); 

            routeDistance = totalDistance;

            return totalDistance;
        }

        public double TotalDist
        {
            get { return routeDistance;  }
        }

        public List<City> Cities
        {
            get { return citiesList; }
        }

    }
}
