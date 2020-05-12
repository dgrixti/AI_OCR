using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AI_OCR.ML.NET
{
    public class MulticlassClassifier
    {
        private static MLContext mlContext = new MLContext();

        public static void RunTest(string filePathTrain, string filePathTest, string fileMLTrainedModel)
        {
            Train(filePathTrain, filePathTest, fileMLTrainedModel);
        }

        public static void Train(string filePathTrain, string filePathTest, string fileMLTrainedModel)
        {
            try
            {
                // Skip the training of the model if model already exists.
                if (System.IO.File.Exists(fileMLTrainedModel))
                {
                    return;
                }

                /**
                 * Copied the following code chunk from the official Github of ML.NET
                 * 
                 * https://github.com/dotnet/machinelearning-samples/blob/master/samples/csharp/getting-started/MulticlassClassification_MNIST/MNIST/Program.cs
                 */

                // STEP 1: Common data loading configuration
                var trainData = mlContext.Data.LoadFromTextFile(path: filePathTrain,
                        columns: new[]
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader: false,
                        separatorChar: ','
                        );


                var testData = mlContext.Data.LoadFromTextFile(path: filePathTest,
                        columns: new[]
                        {
                            new TextLoader.Column(nameof(InputData.PixelValues), DataKind.Single, 0, 63),
                            new TextLoader.Column("Number", DataKind.Single, 64)
                        },
                        hasHeader: false,
                        separatorChar: ','
                        );

                // STEP 2: Common data process configuration with pipeline data transformations
                // Use in-memory cache for small/medium datasets to lower training time. Do NOT use it (remove .AppendCacheCheckpoint()) when handling very large datasets.
                var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "Number", keyOrdinality: ValueToKeyMappingEstimator.KeyOrdinality.ByValue).
                    Append(mlContext.Transforms.Concatenate("Features", nameof(InputData.PixelValues)).AppendCacheCheckpoint(mlContext));

                // STEP 3: Set the training algorithm, then create and config the modelBuilder
                var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer).Append(mlContext.Transforms.Conversion.MapKeyToValue("Number", "Label"));

                // STEP 4: Train the model fitting to the DataSet

                Console.WriteLine("=============== Training the model ===============");
                ITransformer trainedModel = trainingPipeline.Fit(trainData);

                Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
                var predictions = trainedModel.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(data: predictions, labelColumnName: "Number", scoreColumnName: "Score");

                ///Common.ConsoleHelper.PrintMultiClassClassificationMetrics(trainer.ToString(), metrics);

                mlContext.Model.Save(trainedModel, trainData.Schema, fileMLTrainedModel);

                Console.WriteLine("The model is saved to {0}", fileMLTrainedModel);

                /*
                 * End Copy
                 */
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR " + e.Message);
            }
        }

        public static int predictChar(OCRCharacter charTest, string fileMLTrainedModel)
        {
            ITransformer trainedModel = mlContext.Model.Load(fileMLTrainedModel, out var modelInputSchema);
            // Create prediction engine related to the loaded trained model
            var predEngine = mlContext.Model.CreatePredictionEngine<InputData, OutPutData>(trainedModel);

            var resultprediction1 = predEngine.Predict(new InputData() { Number=1, PixelValues = charTest.getPointsFloatArr() } );

            //Console.WriteLine($"Actual: 1     Predicted probability:       zero:  {resultprediction1.Score[0]:0.####}");
            //Console.WriteLine($"                                           One :  {resultprediction1.Score[1]:0.####}");
            //Console.WriteLine($"                                           two:   {resultprediction1.Score[2]:0.####}");
            //Console.WriteLine($"                                           three: {resultprediction1.Score[3]:0.####}");
            //Console.WriteLine($"                                           four:  {resultprediction1.Score[4]:0.####}");
            //Console.WriteLine($"                                           five:  {resultprediction1.Score[5]:0.####}");
            //Console.WriteLine($"                                           six:   {resultprediction1.Score[6]:0.####}");
            //Console.WriteLine($"                                           seven: {resultprediction1.Score[7]:0.####}");
            //Console.WriteLine($"                                           eight: {resultprediction1.Score[8]:0.####}");
            //Console.WriteLine($"                                           nine:  {resultprediction1.Score[9]:0.####}");

            int ans = -1;
            float ansVal = 0;

            // Chose the one cahracter with the highest likelihood score
            for (int i =0; i<10; i++)
            {
                if (resultprediction1.Score[i] > ansVal)
                {
                    ansVal = resultprediction1.Score[i];
                    ans = i;
                }
            }
            return ans;
        }

        public static void processML(string fileMLTrainedModel, List<OCRCharacter> charactersTest)
        {
            try
            {
                int correct = 0;
                int incorrect = 0;


                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < charactersTest.Count; i++)
                {
                    int guess = predictChar(charactersTest[i], fileMLTrainedModel);

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


        class InputData
        {
            [ColumnName("PixelValues")]
            [VectorType(64)]
            public float[] PixelValues;

            [LoadColumn(64)]
            public float Number;
        }

        class OutPutData
        {
            [ColumnName("Score")]
            public float[] Score;
        }
    }
}
