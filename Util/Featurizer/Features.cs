using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Featurizer
{
    class Features
    {
        Dictionary<string, Dictionary<string, List<KeyValuePair<double[], string>>>> m_Values;
        List<string> m_FeatureNames;
        List<string> m_ClassificationNames;
        string m_Domain;

        public Features(List<string> featureNames, List<string> classificationNames, string domain)
        {
            m_Values = new Dictionary<string, Dictionary<string, List<KeyValuePair<double[], string>>>>();
            m_FeatureNames = featureNames;
            m_ClassificationNames = classificationNames;
            m_Domain = domain;
        }

        public void Add(string userName, string className, double[] features, string expectedClassification)
        {
            if (!m_Values.ContainsKey(userName))
                m_Values.Add(userName, new Dictionary<string, List<KeyValuePair<double[], string>>>());

            Dictionary<string, List<KeyValuePair<double[], string>>> perClass = m_Values[userName];
            if (!perClass.ContainsKey(className))
                perClass.Add(className, new List<KeyValuePair<double[], string>>());

            List<KeyValuePair<double[], string>> values = perClass[className];
            values.Add(new KeyValuePair<double[], string>(features, expectedClassification));
        }

        public List<KeyValuePair<double[], string>> GetValues(string userName, string className)
        {
            if (!m_Values.ContainsKey(userName))
                return new List<KeyValuePair<double[], string>>();

            if (!m_Values[userName].ContainsKey(className))
                return new List<KeyValuePair<double[], string>>();

            return m_Values[userName][className];
        }

        public List<KeyValuePair<double[], string>> GetTestingData(string userName, string className)
        {
            List<KeyValuePair<double[], string>> values = new List<KeyValuePair<double[], string>>();
            
            foreach (KeyValuePair<string, List<KeyValuePair<double[], string>>> kvp2 in m_Values[userName])
            {
                if (kvp2.Key != className)
                    continue;

                foreach (KeyValuePair<double[], string> instance in kvp2.Value)
                    values.Add(instance);
            }
            

            return values;
        }


        public List<KeyValuePair<double[], string>> GetTrainingData(string userName, string className)
        {
            List<KeyValuePair<double[], string>> values = new List<KeyValuePair<double[], string>>();
            foreach (KeyValuePair<string, Dictionary<string, List<KeyValuePair<double[], string>>>> kvp in m_Values)
            {
                if (kvp.Key == userName)
                    continue;

                foreach (KeyValuePair<string, List<KeyValuePair<double[], string>>> kvp2 in kvp.Value)
                {
                    if (kvp2.Key != className)
                        continue;

                    foreach (KeyValuePair<double[], string> instance in kvp2.Value)
                        values.Add(instance);
                }
            }

            return values;
        }

        public void PrintARFF(string filename, string userName, string className, bool includeClassification)
        {
            List<KeyValuePair<double[], string>> values = GetTrainingData(userName, className);

            StreamWriter writer = new StreamWriter(filename);

            writer.WriteLine("@RELATION " + m_Domain);
            foreach (string att in m_FeatureNames)
            {
                string att2 = att.Replace("'", "");
                writer.WriteLine("@ATTRIBUTE '" + att2 + "' NUMERIC");
            }

            if (includeClassification)
            {
                writer.Write("@ATTRIBUTE class {");
                for (int i = 0; i < m_ClassificationNames.Count; i++)
                {
                    if (i < m_ClassificationNames.Count - 1)
                        writer.Write(m_ClassificationNames[i] + ",");
                    else
                        writer.Write(m_ClassificationNames[i]);
                }
                writer.WriteLine("}");
            }
            writer.WriteLine();

            writer.WriteLine("@DATA");

            foreach (KeyValuePair<double[], string> value in values)
            {
                if (includeClassification)
                {
                    foreach (double num in value.Key)
                        writer.Write(num + ",");

                    writer.WriteLine(value.Value);
                }
                else
                {
                    for (int i = 0; i < value.Key.Length; i++)
                    {
                        if (i < value.Key.Length - 1)
                            writer.Write(value.Key[i] + ",");
                        else
                            writer.WriteLine(value.Key[i]);
                    }
                }
            }

            writer.Close();
        }

        public void PrintTestingARFF(string filename, string userName, string className, bool includeClassification)
        {
            List<KeyValuePair<double[], string>> values = GetTestingData(userName, className);

            StreamWriter writer = new StreamWriter(filename);

            writer.WriteLine("@RELATION " + m_Domain);
            foreach (string att in m_FeatureNames)
            {
                string att2 = att.Replace("'", "");
                writer.WriteLine("@ATTRIBUTE '" + att2 + "' NUMERIC");
            }

            if (includeClassification)
            {
                writer.Write("@ATTRIBUTE class {");
                for (int i = 0; i < m_ClassificationNames.Count; i++)
                {
                    if (i < m_ClassificationNames.Count - 1)
                        writer.Write(m_ClassificationNames[i] + ",");
                    else
                        writer.Write(m_ClassificationNames[i]);
                }
                writer.WriteLine("}");
            }
            writer.WriteLine();

            writer.WriteLine("@DATA");

            foreach (KeyValuePair<double[], string> value in values)
            {
                if (includeClassification)
                {
                    foreach (double num in value.Key)
                        writer.Write(num + ",");

                    writer.WriteLine(value.Value);
                }
                else
                {
                    for (int i = 0; i < value.Key.Length; i++)
                    {
                        if (i < value.Key.Length - 1)
                            writer.Write(value.Key[i] + ",");
                        else
                            writer.WriteLine(value.Key[i]);
                    }
                }
            }

            writer.Close();
        }

        public void Print(string filename, string userName, string className)
        {
            List<KeyValuePair<double[], string>> values = GetValues(userName, className);
            int numFeatures = values[0].Key.Length;
            int numClasses = 1;
            if (values[0].Value.Contains("Join"))
                numClasses = 1;
            else
                numClasses = 3;

            StreamWriter writer = new StreamWriter(filename);
            writer.WriteLine(numFeatures.ToString());
            writer.WriteLine(numClasses.ToString());

            foreach (KeyValuePair<double[], string> value in values)
            {
                foreach (double num in value.Key)
                    writer.Write(num + ", ");

                string cls = value.Value;
                if (cls == "Join")
                    writer.WriteLine("1");
                else if (cls == "NoJoin")
                    writer.WriteLine("0");
                else if (cls == "Gate")
                    writer.WriteLine("1, 0, 0");
                else if (cls == "Wire")
                    writer.WriteLine("0, 1, 0");
                else if (cls == "Label")
                    writer.WriteLine("0, 0, 1");
            }

            writer.Close();
        }
    }
}
