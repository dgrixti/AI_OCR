using AI_OCR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.AI.TSP
{
    public class TSPNearestNeighbour
    {

        private List<City> citiesResult = null;
        private double totalDistanceRoute = 0;
        private List<City> cities = null;

        public TSPNearestNeighbour(List<City> cities)
        {
            this.cities = cities;
        }

        public double GetTotalDistanceRoute()
        {
            return totalDistanceRoute;
        }

        public List<City> GetCitiesResult()
        {
            return citiesResult;
        }

        public string GetCitiesResultToString()
        {
            string sequence = "";

            foreach (City s in citiesResult)
            {
                sequence += s.Id + ", ";
            }

            return sequence;
        }

        /**
         * Main execution method.
         */
        public void Execute()
        {

            totalDistanceRoute = 0;

            // Create an array which will hold he cities route
            citiesResult = new List<City>();

            // Get city with id 1 where to start from.
            City firstCity = cities.Where(c => c.Id == 1).FirstOrDefault();

            City currentCity = firstCity;

            // Add the city to route list
            AddCityToRoutesList(cities, citiesResult, currentCity);

            while (cities.Count > 0)
            {

                // Get closes city
                currentCity = FindClosestCity(cities, currentCity);

                // Add to the total distance travelled.
                totalDistanceRoute += currentCity.GetLastDistanceMeasured();

                // Add the city to route list
                AddCityToRoutesList(cities, citiesResult, currentCity);
            }

            // Last city should compare to the first one for return home.
            City lastCity = citiesResult[citiesResult.Count - 1];

            // Make a copy of the original city for final destinal
            // copy is made to avoid changing vars to the original point/route/city.
            City copyFirstCity = new City(firstCity.Id, firstCity.X, firstCity.Y);
            double dist = copyFirstCity.MeasureDistance(lastCity);

            // Add to the total distance travelled.
            totalDistanceRoute += dist; //lastCity.getLastDistanceMeasured();

            // Add the return to fist city as final travel distance.
            citiesResult.Add(copyFirstCity);
        }

        private void AddCityToRoutesList(List<City> cities, List<City> citiesResult, City currentCity)
        {
            // Add viry to route
            citiesResult.Add(currentCity);
            // Remove from remainign cities to visit list.
            cities.Remove(currentCity);
        }

        /**
         * 
         * Compares a given city towards all the remaining unvisited cities
         * using functional java and sort comparer.
         * 
         * @param cities: List of remaining unvisited cities.
         * @param currentCity: From which to check distance.
         * @return: Closes City 
         */
        private City FindClosestCity(List<City> cities, City currentCity)
        {

            City nearestCity;

            // If only 1 remains, measure to the current city and return
            if (cities.Count == 1)
            {
                nearestCity = cities[0];
                nearestCity.MeasureDistance(currentCity);
            }
            else
            {
                // Find the closest city by comparing them all with 
                // their respective position of the current city.
                nearestCity = 
                    cities.Aggregate((c1, c2) => c1.MeasureDistance(currentCity) < c2.MeasureDistance(currentCity) ? c1 : c2);


                //nearestCity = cities.stream().min((c1, c2)-> {
                //    return c1.measureDistance(currentCity) < c2.measureDistance(currentCity) ? -1
                //        : c1.measureDistance(currentCity) > c2.measureDistance(currentCity) ? 1
                //        : 0;
                //}).get();
            }

            return nearestCity;
        }

    }
}
