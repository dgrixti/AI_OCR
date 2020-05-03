using System;
using System.Collections.Generic;
using System.Text;

namespace AI_OCR
{
    public interface IDistance
    {
        double getDistance(double[] features1, double[] features2);
    }

}
