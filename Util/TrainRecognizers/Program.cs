using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CombinationRecognizer;
using SubRecognizer;
using Sketch;
using ConverterXML;
using Utilities.Matrix;
using Domain;
using Recognizers;

namespace TrainRecognizers
{
    class Program
    {

        /// <summary>
        /// Arguments
        ///    0: the directory to find files
        ///    1: a string to filter filenames (e.g., "*.xml")
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;


            // Get the list of files
            Console.WriteLine("Finding sketch files...");
            List<string> allSketches = new List<string>(System.IO.Directory.GetFiles(args[0], args[1]));
            Console.WriteLine("    found " + allSketches.Count + " sketches");


            // Load all the shapes in all the sketches
            Console.WriteLine("Loading full data set...");
            List<Shape> shapeData = GetShapeData(allSketches);
            Console.WriteLine("    found " + shapeData.Count + " gates");


            // Print classes found
            HashSet<ShapeType> types = new HashSet<ShapeType>();
            foreach (Shape shape in shapeData)
            {
                types.Add(shape.Type);
            }
            Console.WriteLine("Found " + types.Count + " types:");
            foreach (ShapeType type in types)
            {
                Console.WriteLine("    " + type);
            }
            
            // Save all the shapes to images in the "sketches" folder
            string outputPath = @"sketches\";
            Console.WriteLine("Saving gates to '" + outputPath + "'...");
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            foreach (Shape shape in shapeData)
            {
                System.Drawing.Bitmap b = ToBitmap.createFromShape(shape, 30, 30, true);
                shape.bitmap = b;
                string filename = String.Format(outputPath + shape.Type + "-{0:x}.png", shape.GetHashCode());
                b.Save(filename);
            }
            Console.WriteLine("    finished saving gates");


            // Train the base recognizers on all the data
            Console.WriteLine("Training recognizers on all data...");

#if false
            Console.WriteLine("    rubine");
            RubineRecognizerUpdateable rubine = new RubineRecognizerUpdateable(shapeData);
            rubine.Save("Rubine.rru");
            rubine.LiteRecognizer.Save("RubineLite.rr");

            Console.WriteLine("    dollar");
            DollarRecognizer dollar = new DollarRecognizer(shapeData);
            dollar.Save("Dollar.dr");
#else
            RubineRecognizerUpdateable rubine = new RubineRecognizerUpdateable();
            rubine.Save("Rubine.rru");
            rubine.LiteRecognizer.Save("RubineLite.rr");

            DollarRecognizer dollar = new DollarRecognizer();
            dollar.Save("Dollar.dr");
#endif

            Console.WriteLine("    zernike");
            ZernikeMomentRecognizerUpdateable zernike = new ZernikeMomentRecognizerUpdateable(shapeData);
            zernike.Save("Zernike.zru");
            zernike.LiteRecognizer.Save("ZernikeLite.zr");

            Console.WriteLine("    image");
            ImageRecognizer image = new ImageRecognizer(shapeData);
            image.Save("Image.ir");

            Console.WriteLine("    finished training recognizers");

            RubineRecognizer fullRubine = rubine.LiteRecognizer;
            DollarRecognizer fullDollar = dollar;
            ZernikeMomentRecognizer fullZernike = zernike.LiteRecognizer;
            ImageRecognizer fullImage = image;


            // Split the data up per-user
            Console.WriteLine("Loading per-user data...");
            Dictionary<string, List<Shape>[]> user2data = GetSketchesPerUser(allSketches);
            Console.WriteLine("    found " + user2data.Count + " users");


            // Foreach user: train each of the recognizers and accumulate training data
            // for the combo recognizer
            List<KeyValuePair<ShapeType, Dictionary<string, object>>> data = new List<KeyValuePair<ShapeType, Dictionary<string, object>>>();
            foreach (KeyValuePair<string, List<Shape>[]> pair in user2data)
            {
                string user = pair.Key;

                ////////////////////////////////////////
                ////////////   Train   /////////////////
                ////////////////////////////////////////

                Console.WriteLine("User: " + user);
                List<Shape> trainingSet = pair.Value[0];

#if false
                Console.WriteLine("    rubine");
                rubine = new RubineRecognizerUpdateable(trainingSet);
                rubine.Save("Rubine" + user + ".rru");
                rubine.LiteRecognizer.Save("RubineLite" + user + ".rr");

                Console.WriteLine("    dollar");
                dollar = new DollarRecognizer(trainingSet);
                dollar.Save("Dollar" + user + ".dr");
#else
                rubine = new RubineRecognizerUpdateable();
                rubine.Save("Rubine" + user + ".rru");
                rubine.LiteRecognizer.Save("RubineLite" + user + ".rr");

                dollar = new DollarRecognizer();
                dollar.Save("Dollar" + user + ".dr");
#endif

                Console.WriteLine("    zernike");
                zernike = new ZernikeMomentRecognizerUpdateable(trainingSet);
                zernike.Save("Zernike" + user + ".zru");
                zernike.LiteRecognizer.Save("ZernikeLite" + user + ".zr");

                Console.WriteLine("    image");
                image = new ImageRecognizer(trainingSet);
                image.Save("Image" + user + ".ir");
                fullImage = image;

                ////////////////////////////////////////
                //////////// Evaluate //////////////////
                ////////////////////////////////////////


                List<Shape> testingSet = pair.Value[1];

                // Create the training data for the combo recognizer
                List<KeyValuePair<ShapeType, Dictionary<string, object>>> comboTrainingData = TrainingDataCombo(testingSet, rubine, dollar, zernike, image);
                foreach (KeyValuePair<ShapeType, Dictionary<string, object>> pair2 in comboTrainingData)
                    data.Add(pair2);
            }

            if (data.Count == 0)
                throw new Exception("no data!");

            List<string> features = new List<string>();
            foreach (KeyValuePair<ShapeType, Dictionary<string, object>> instance in data)
                foreach (string feature in instance.Value.Keys)
                    if (!features.Contains(feature))
                        features.Add(feature);

            Console.WriteLine("Found " + data.Count + " data points and " + features.Count + " features.");

            ComboRecognizer combo = new ComboRecognizer(fullRubine, fullDollar, fullZernike, fullImage);
            combo.TrainCombo(features, data);
            combo.Save("Combo.cru");

            Console.WriteLine("Naive bayes updatable has " + combo.ComboClassifier.Examples.Count + " examples.");
            Console.WriteLine("Naive bayes updatable has " + combo.ComboClassifier.Classifier.Classes.Count + " classes:");
            foreach (ShapeType cls in combo.ComboClassifier.Classifier.Classes)
            {
                Console.WriteLine("    " + cls);
            }

            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();

        }

        /// <summary>
        /// Get a dictionary mapping users -> array of shape lists.
        /// 
        /// The first entry in the array is training set.
        /// 
        /// The second entry is the testing set.
        /// </summary>
        /// <param name="files">the list of files to read</param>
        /// <returns>the mapping described above</returns>
        private static Dictionary<string, List<Shape>[]> GetSketchesPerUser(List<string> files)
        {
            Dictionary<string, List<string>[]> sketches = new Dictionary<string, List<string>[]>();

            foreach (string f in files)
            {
                string fShort = Path.GetFileName(f);
                string user = fShort.Substring(0, fShort.IndexOf('_')); // everything to first underscore
                if (fShort.Contains("_T"))
                    user += "T";
                else if (fShort.Contains("_P"))
                    user += "P";

                if (!sketches.ContainsKey(user))
                {
                    sketches.Add(user, new List<string>[2]);
                    sketches[user][1] = new List<string>();
                    sketches[user][0] = new List<string>();
                }
               
                if (fShort.Contains("EQ") || fShort.Contains("COPY"))
                    sketches[user][1].Add(f);
                else
                    sketches[user][0].Add(f);
            }

            Dictionary<string, List<Shape>[]> result = new Dictionary<string, List<Shape>[]>();

            foreach (KeyValuePair<string, List<string>[]> pair in sketches)
            {
                string user = pair.Key;
                List<string> trainingSet = pair.Value[0];
                List<string> testingSet  = pair.Value[1];

                result.Add(user, new List<Shape>[] { 
                    GetShapeData(trainingSet),
                    GetShapeData(testingSet)
                });

            }

            return result;
        }

        /// <summary>
        /// Construct the training data for the combo recognizer.
        /// </summary>
        /// <param name="testData">the data to test on</param>
        /// <param name="rubine">the rubine recognizer</param>
        /// <param name="dollar">the dollar recognizer</param>
        /// <param name="zernike">the zernike recognizer</param>
        /// <param name="image">the image recognizer</param>
        /// <returns>a list of tuples (s,f) where s is a shape type and f is a set of features</returns>
        private static List<KeyValuePair<ShapeType, Dictionary<string, object>>> TrainingDataCombo(
            List<Shape> testData, 
            RubineRecognizerUpdateable rubine, 
            DollarRecognizer dollar, 
            ZernikeMomentRecognizerUpdateable zernike,
            ImageRecognizer image)
        {
            List<KeyValuePair<ShapeType, Dictionary<string, object>>> data = new List<KeyValuePair<ShapeType, Dictionary<string, object>>>();

            foreach (Shape shape in testData)
            {
                string z = zernike.Recognize(shape.SubstrokesL);

                List<ShapeType> img = image.Recognize(shape.SubstrokesL);

                List<string> r = new List<string>();
                List<string> dAvg = new List<string>();
                List<string> d = new List<string>();

                foreach (Substroke s in shape.SubstrokesL)
                {
                    r.Add(rubine.Recognize(s));
                    dAvg.Add(dollar.RecognizeAverage(s));
                    d.Add(dollar.Recognize(s));
                }

                Dictionary<string, object> features = ComboRecognizer.GetFeatures(shape.SubstrokesL.Count, z, img, r, dAvg, d);
                data.Add(new KeyValuePair<ShapeType, Dictionary<string, object>>(shape.Type, features));
            }

            return data;
        }

        /// <summary>
        /// Extracts all of the labeled shapes from a set of sketches. It returns only
        /// gates.
        /// </summary>
        /// <param name="sketchFiles">the list of filenames of labeled sketches</param>
        /// <returns>a list of labeled shapes</returns>
        private static List<Shape> GetShapeData(List<string> sketchFiles)
        {
            List<Shape> result = new List<Shape>();

            foreach (string file in sketchFiles)
            {
                Sketch.Sketch sketch = new ReadXML(file).Sketch;
                if (sketch == null)
                    continue;

                foreach (Shape shape in sketch.Shapes)
                {
                    if (shape.LowercasedType == "unknown")
                    {
                        Console.WriteLine("    Found unlabled shape in '" + file + "'");
                        continue;
                    }

                    result.Add(shape);
                }
            }

            // Keep only gates
            List<Shape> filteredData = new List<Shape>();
            foreach (Shape shape in result)
                if (shape.Type.Classification == LogicDomain.GATE_CLASS)
                    filteredData.Add(shape);
            result = filteredData;

            return result;
        }

    }
}
