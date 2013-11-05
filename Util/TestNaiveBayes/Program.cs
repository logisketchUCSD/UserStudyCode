using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Utilities.NaiveBayes;

namespace TestNaiveBayes
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Provide training examples and a test set");
                return;
            }

            List<string> features;
            List<KeyValuePair<string, Dictionary<string, object>>> examples = GetExamples(args[0], out features);

            NaiveBayesUpdateable bayes = new NaiveBayesUpdateable(features);
            foreach (KeyValuePair<string, Dictionary<string, object>> example in examples)
                bayes.AddExample(example.Key, example.Value);

            bayes.UpdateClassifier();
            NaiveBayes classifier = bayes.Classifier;

            List<KeyValuePair<string, Dictionary<string, object>>> testCases = GetExamples(args[1], out features);
            foreach (KeyValuePair<string, Dictionary<string, object>> pair in testCases)
            {
                string className = pair.Key;
                Dictionary<string, double> results = classifier.Classify(pair.Value);
                Console.WriteLine();
                Console.WriteLine("Actual Class: " + className);
                foreach (KeyValuePair<string, double> result in results)
                    Console.WriteLine("    " + result.Key + ": " + result.Value.ToString());
            }
        }

        private static List<KeyValuePair<string, Dictionary<string, object>>> GetExamples(string filename, out List<string> features)
        {
            List<KeyValuePair<string, Dictionary<string, object>>> examples = new List<KeyValuePair<string, Dictionary<string, object>>>();
            features = new List<string>();
            StreamReader reader = new StreamReader(filename);

            string line = reader.ReadLine();
            string[] header = line.Split(",".ToCharArray());

            while ((line = reader.ReadLine()) != null && line != "")
            {
                string[] values = line.Split(",".ToCharArray());
                string cls = values[0];

                Dictionary<string, object> example = new Dictionary<string, object>(header.Length);
                for (int i = 1; i < values.Length; i++)
                {
                    int num;
                    if (int.TryParse(values[i], out num))
                        example.Add(header[i], (object)num);
                    else
                        example.Add(header[i], (object)values[i]);

                    if (!features.Contains(header[i]))
                        features.Add(header[i]);
                }

                examples.Add(new KeyValuePair<string,Dictionary<string,object>>(cls, example));
            }

            reader.Close();
            return examples;
        }
    }
}
