using AI_OCR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.Helper
{
    public class TSPHelper
    {
        // Taken from: https://www.daniweb.com/programming/software-development/threads/349926/c-string-permutation#post1485682

        public static IList<IList<T>> Permutations<T>(IList<T> list)
        {
            List<IList<T>> perms = new List<IList<T>>();

            if (list.Count == 0)
                return perms; // Empty list.

            int factorial = 1;
            for (int i = 2; i <= list.Count; i++)
                factorial *= i;

            for (int v = 0; v < factorial; v++)
            {
                List<T> s = new List<T>(list);
                int k = v;
                for (int j = 2; j <= list.Count; j++)
                {
                    int other = (k % j);
                    T temp = s[j - 1];
                    s[j - 1] = s[other];
                    s[other] = temp;

                    k = k / j;
                }
                perms.Add(s);
            }

            return perms;
        }

        public static List<City> ParseCitiesFromFile(string filePath)
        {
            List<City> cities = new List<City>();
            string line = null;

            if (File.Exists(filePath))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(filePath);
                    while ((line = file.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        string[] vals = line.Split(' ');
                        cities.Add(new City(Int32.Parse(vals[0]), Double.Parse(vals[1]), Double.Parse(vals[2])));
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }

            return cities;
            ///2432902008176640000
        }
    }
}
