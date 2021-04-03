using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.Models
{
    [Serializable]
    public class City
    {
        private readonly int id = Int32.MinValue;
        private readonly double x = Double.MinValue;
        private readonly double y = Double.MinValue;

        private double lastDistanceMeasured = 0;

        public City(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        public double X
        {
            get
            {
                return x;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
        }

        public int Id
        {
            get { return id; }
        }

        public void ResetLastDistanceMeasured()
        {
            lastDistanceMeasured = 0;
        }

        public double GetLastDistanceMeasured()
        {
            return lastDistanceMeasured;
        }

        // sqrt(x²+ y²)
        public double MeasureDistance(City city)
        {
            double dist = (double)Math.Sqrt(
                Math.Pow(city.x - this.x, 2) +
                Math.Pow(city.y - this.y, 2));

            this.lastDistanceMeasured = dist;

            return dist;
        }

        public void SetLastDistanceMeasured(double dist)
        {
            lastDistanceMeasured = dist;
        }

    }
}
