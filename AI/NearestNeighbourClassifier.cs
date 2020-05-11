using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AI_OCR
{
    /**
     *
     * @author Darren Grixti
     */
    public class NearestNeighbourClassifier
    {
        public static int processNNAndPredict(List<OCRCharacter> charactersTrain,
             List<OCRCharacter> charactersTest, IDistance distCalc)
        {
            try
            {
                int answer = int.MinValue;
                for (int i = 0; i < charactersTest.Count; i++)
                {
                    answer = processAndPredict(charactersTest[i], charactersTrain, distCalc);
                }
                return answer;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static int processAndPredict(OCRCharacter testChar,
            List<OCRCharacter> charactersTrainList, IDistance distCalc)
        {
            double minDistance = 0;
            int minCharIdentifier = Int32.MinValue;
            for (int i = 0; i < charactersTrainList.Count; i++)
            {
                // Compare distance between vectors array using a distance metric (ex: EuclideanDistance distance).
                double distance = charactersTrainList[i].getDistance(distCalc, testChar);
                // If got a shorter distance than before, make this the current nearest distance.
                if (minDistance == 0 || distance < minDistance)
                {
                    minDistance = distance;
                    minCharIdentifier = charactersTrainList[i].getIdentifier();
                }
            }
            return minCharIdentifier;
        }
    }
}
