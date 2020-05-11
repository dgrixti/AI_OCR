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
             OCRCharacter characterTest, IDistance distCalc)
        {
            try
            {
                int answer = int.MinValue;
            
                    answer = processAndPredict(characterTest, charactersTrain, distCalc);
                
                return answer;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void processNN(List<OCRCharacter> charactersTrain,
            List<OCRCharacter> charactersTest, IDistance distCalc)
        {

            try
            {

                int correct = 0; 
                int incorrect = 0; 
   

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < charactersTest.Count; i++)
                {
                    int guess = processAndPredict(charactersTest[i], charactersTrain, distCalc);

                    if (charactersTest[i].getIdentifier() == guess)
                    {
                        correct++;
                        Console.WriteLine(i + ". CORRECT - testing is probabbly " + guess + " / " + charactersTest[i].getIdentifier() + " actual.");
                    }
                    else
                    {
                        incorrect++;
                        Console.WriteLine(i + ". FAIL - testing is probabbly " + guess + " / " + charactersTest[i].getIdentifier() + " actual.");
                    }
                }

                stopwatch.Stop();

                //int total = correct + incorrect;

                long estimatedTime = stopwatch.ElapsedMilliseconds; //System.nanoTime() - startTime;

                // Show accuracy results.
                Console.WriteLine(correct + " / " + charactersTest.Count);

                float accuracy = (float)(correct * 100) / charactersTest.Count;
                Console.WriteLine(accuracy + " % accuracy.");

                // Time elapsed
                Console.WriteLine("Execution time is " + ((float)estimatedTime / 1000000000).ToString("#0.000") + " seconds");

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR " + e.Message);
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
