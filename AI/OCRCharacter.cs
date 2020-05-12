using System;
using System.Collections.Generic;
using System.Text;

namespace AI_OCR
{
    public class OCRCharacter
    {
        private double[] pointsArr;
        private int identifier;
        public double distance;

        public OCRCharacter(double[] pointsArr, int identifier)
        {
            this.pointsArr = pointsArr;
            this.identifier = identifier;
        }

        public int getIdentifier()
        {
            return this.identifier;
        }

        public double[] getPoints()
        {
            return this.pointsArr;
        }

        public float[] getPointsFloatArr()
        {
            float[] floatArray = new float[pointsArr.Length];
            for (int i = 0; i < pointsArr.Length; i++)
            {
                floatArray[i] = (float)pointsArr[i];
            }
            return floatArray;
        }

        public double getDistance(IDistance distCalculator, OCRCharacter otherCharacter)
        {
            double dist = distCalculator.getDistance(this.pointsArr, otherCharacter.getPoints());
            distance = dist;
            return dist;
        }

    }
}
