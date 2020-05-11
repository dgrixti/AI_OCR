using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.AI
{
    /**
     *
     * @author Darren Grixti
     */
    public class KNNClassifier
    {

        private static int correct = 0;
        private static int incorrect = 0;
        private static int total = 0;

        /**
         * Runs the KNN classifier on the given 2-Fold dataset.
         * 
         * @param charactersTrain: Training set array.
         * @param charactersTest: Test set array.
         * @param distCalc: Distance metric to use ex: Euclidean, Manhattan etc.
         * @param kSize : The number of neoighbours to choose for classification vote.
         * @param useDistanceScore: True use scoring based on euclidean distance for 
         *          classification, False uses majority votes.
         * 
         */
        public static void processKNN(List<OCRCharacter> charactersTrain,
                List<OCRCharacter> charactersTest, IDistance distCalc, int kSize,
                bool useDistanceScore)
        {

            try
            {

                correct = 0; incorrect = 0; total = 0;

                //long startTime = DateTime.Now.Millisecond;

                for (int i = 0; i < charactersTest.Count; i++)
                {
                    List<OCRCharacter> nearestNeighbours =
                            findKNearestNeighbors(charactersTrain,
                                    distCalc, charactersTest[i], kSize);
                    int classLabel = classify(nearestNeighbours, useDistanceScore);

                    total++;

                    if (classLabel == charactersTest[i].getIdentifier())
                    {
                        correct++;
                        Console.WriteLine(total + ". testing is probabbly " + classLabel + " / " + charactersTest[i].getIdentifier() + " actual. GOOD");
                    }
                    else
                    {
                        incorrect++;
                        Console.WriteLine(total + ". testing is probabbly " + classLabel + " / " + charactersTest[i].getIdentifier() + " actual. BAD");
                    }
                }

                //long estimatedTime = System.nanoTime() - startTime;

                float accuracy = (float)(correct * 100) / total;

                Console.WriteLine(correct + " / " + charactersTest.Count);
                Console.WriteLine("accuracy " + accuracy + "%");

                // Time taken
                //NumberFormat formatter = new DecimalFormat("#0.000");
                // System.out.println("Execution time is " + formatter.format((float)estimatedTime / 1000000000) + " seconds");

            }
            catch (Exception e)
            {
                // e.printStackTrace();
            }
        }

        private static List<OCRCharacter> findKNearestNeighbors(
                List<OCRCharacter> trainListCharacters,
                IDistance calculator, OCRCharacter currentCharacter, int kSize)
        {
            //12

            //List<cTag> week = new List<cTag>();
            //week.Sort((x, y) =>
            //    DateTime.ParseExact(x.date, "dd.MM.yyyy", CultureInfo.InvariantCulture).CompareTo(
            //    DateTime.ParseExact(y.date, "dd.MM.yyyy", CultureInfo.InvariantCulture))
            //);



            //SortByDistance(trainListCharacters, calculator, currentCharacter);


            trainListCharacters.Sort(delegate (OCRCharacter itemA, OCRCharacter itemB)
            {
                return itemA.getDistance(calculator, currentCharacter).CompareTo(itemB.getDistance(calculator, currentCharacter));
            });

            //    // Find the closes 2 (or K) character neighbours by comparing them all with 
            //    // their respective eucledian distance and getting final K from bottom list.
            //    List <OCRCharacter> nearestNeighbours = trainListCharacters.stream().sorted((c1, c2)-> {
            //        return c1.getDistance(calculator, currentCharacter) < c2.getDistance(calculator, currentCharacter) ? -1
            //            : c1.getDistance(calculator, currentCharacter) > c2.getDistance(calculator, currentCharacter) ? 1
            //            : 0;
            //    }).skip(0).limit(kSize).collect(Collectors.toCollection(ArrayList::new));


            return trainListCharacters;
        }

        ////public static string SortByDistance(List<OCRCharacter> items, IDIstance calculator, OCRCharacter currentCharacter)
        ////{
        ////    items.Sort(delegate (OCRCharacter itemA, OCRCharacter itemB)
        ////    {
        ////        return itemA.getDistance(calculator, currentCharacter).CompareTo(itemB.getDistance(calculator, currentCharacter));
        ////    });

        ////    return null;
        ////}

        /**
         * Given a a list of neighbouring characters, checks which character kind has
         * the most frequent occurrence (majority)
         * @param neighbors
         * @param useDistanceScore: True use scoring based on euclidean distance for 
         *          classification, False uses majority votes.
         * @return charIdentifier: character which has highest majority.
         */
        private static int classify(List<OCRCharacter> neighbors, bool useDistanceScore)
        {

            Dictionary<int, double> charOccurenceList = new Dictionary<int, double>();

            int num = neighbors.Count;

            for (int index = 0; index < num; index++)
            {

                OCRCharacter temp = neighbors[index];
                int key = temp.getIdentifier();

                // If character kind has not been added yet.
                if (!charOccurenceList.ContainsKey(key))
                {
                    // Best practise is to add the total distance of the neighbour char type as score value.

                    if (useDistanceScore)
                    {
                        charOccurenceList.Add(key, 1 / temp.distance);
                    }
                    else
                    {
                        charOccurenceList.Add(key, 1.0);
                    }
                }
                // If already added, add occurence majority vote
                else
                {
                    double value = charOccurenceList[key];
                    // Best practise is to add the total distance of the neighbour char type as score value.

                    if (useDistanceScore)
                    {
                        value += 1 / temp.distance;
                    }
                    else
                    {
                        value += 1.0;
                    }
                    charOccurenceList.Add(key, value);
                }
            }

            // Check which kind of identifiers have the most of the same type
            double majorityScore = 0;
            int charIdentifier = -1;

            // Loop and determine which char identifier scored the highest

            foreach (KeyValuePair<int, double> entry in charOccurenceList)
            {
                int identifier = entry.Key;
                double value = entry.Value;

                if (value > majorityScore)
                {
                    majorityScore = value;
                    charIdentifier = identifier;
                }
            }

            return charIdentifier;
        }

    }
}