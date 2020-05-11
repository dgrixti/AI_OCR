using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.Controllers
{
    public class BaseAIController : Controller
    {
        protected static object lockfile1 = new object();

        protected static void loadDataFromFile(List<OCRCharacter> cities, String fileName,
                List<string> dataSet)
        {
            try
            {
                // if already full
                if (dataSet != null && dataSet.Count > 0)
                {
                    loadDataFromDataset(cities, dataSet);
                    return;
                }
                else
                {
                    lock (lockfile1)
                    {
                        // Read the file and display it line by line.  
                        System.IO.StreamReader file =
                        new System.IO.StreamReader(fileName);

                        int counter = 0;
                        string line;

                        // Cache data in array so it's loaded from file once.
                        while ((line = file.ReadLine()) != null)
                        {
                            //System.Console.WriteLine(line);

                            // add to memory so that the physical file isnt locked for each thread.
                            dataSet.Add(line);
                            counter++;
                        }

                        file.Close();
                    }

                    loadDataFromDataset(cities, dataSet);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected static void loadDataFromDataset(List<OCRCharacter> cities, List<string> dataSet)
        {
            foreach (string line in dataSet)
            {
                addCharacter(cities, line, true);
            }
        }

        protected static OCRCharacter createCharacterFromBytes(String line)
        {
            String[] vars = line.Split(",");
            double[] pointsArr = new double[vars.Length - 1];
            for (int i = 0; i < vars.Length - 1; i++)
            {
                pointsArr[i] = Double.Parse(vars[i]);
            }

            return new OCRCharacter(pointsArr, -1);
        }


        protected static void addCharacter(List<OCRCharacter> characters, String line, bool withAnswer)
        {
            String[] vars = line.Split(",");
            double[] pointsArr = new double[vars.Length - 1];
            for (int i = 0; i < vars.Length - 1; i++)
            {
                pointsArr[i] = Double.Parse(vars[i]);
            }

            // Find the answer from the sequence (training mode) or not (predicting mode).
            if (withAnswer)
            {
                characters.Add(new OCRCharacter(pointsArr, Int32.Parse(vars[vars.Length - 1])));
            }
            else
            {
                characters.Add(new OCRCharacter(pointsArr, -1)); // -1 because we dont know what it is
            }
        }
    }
}
