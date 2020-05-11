using System;
using System.Collections.Generic;
using System.Text;

namespace AI_OCR
{
    public class EuclideanDistance : IDistance
    {
        // Based on the Euclidean equasion: https://en.wikipedia.org/wiki/Euclidean_distance.
        // Distance = SQRT((x2-x1)^2+(y2-y1)^2))
        public double getDistance(double[] pointsArr1, double[] pointsArr2)
        {
            double total = 0, diff;
            for (int i = 0; i < pointsArr1.Length; i++)
            {
                diff = pointsArr2[i] - pointsArr1[i];
                total += diff * diff;
            }
            return (float)Math.Sqrt(total);
        }
    }
}
