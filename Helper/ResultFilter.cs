using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.Helper
{
    /**
     *
     * @author Darren
     */
    public class ResultFilter
    {

        private static ResultFilter instance = null;
        private static readonly object padlock = new object();

        private ResultFilter()
        {
        }

        /* 
         * Restrict to singleton initialisaiton for class
        */ 
        public static ResultFilter Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ResultFilter();
                    }
                    return instance;
                }
            }
        }

        public List<OCRCharacter> limitResults(List<OCRCharacter> charactersTrain, int maxTotal)
        {

            List<OCRCharacter> charactersFiltered = new List<OCRCharacter>();

            int max0 = 0, max1 = 0, max2 = 0, max3 = 0,
                    max4 = 0, max5 = 0, max6 = 0,
                    max7 = 0, max8 = 0, max9 = 0;


            foreach (OCRCharacter character in charactersTrain)
            {

                int answer = character.getIdentifier();
                double[] data = character.getPoints();

                switch (answer)
                {
                    case 0:
                        if (max0 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max0++;
                        break;
                    case 1:
                        if (max1 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max1++;
                        break;
                    case 2:
                        if (max2 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max2++;
                        break;
                    case 3:
                        if (max3 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max3++;
                        break;
                    case 4:
                        if (max4 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max4++;
                        break;
                    case 5:
                        if (max5 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max5++;
                        break;
                    case 6:
                        if (max6 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max6++;
                        break;
                    case 7:
                        if (max7 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max7++;
                        break;
                    case 8:
                        if (max8 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max8++;
                        break;
                    case 9:
                        if (max9 <= maxTotal)
                        {
                            charactersFiltered.Add(character);
                        }
                        max9++;
                        break;
                }

            }

            return charactersFiltered;
        }
    }
}

