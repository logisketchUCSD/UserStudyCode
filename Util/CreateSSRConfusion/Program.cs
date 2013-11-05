using System;
using System.Collections.Generic;
using System.Text;
using Utilities;
using Sketch;
using ImageAligner;
using ConverterXML;

namespace CreateSSRConfusion
{
    class Program
    {
        static List<string> leafLabels = new List<string>(new string[] {
            "BackLine", "FrontArc", "BackArc", "TopArc", "BottomArc",
            "Bubble", "TopLine", "BottomLine", "Triangle", 
            "GreaterThan", "TouchUp", "Junk", 
            "LabelBoxOther", "Entire_OR", "Entire_AND" });

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            List<string> allLabels = new List<string>(leafLabels);
            allLabels.Add("Wire");

            Dictionary<string, List<string>[]> user2sketches = GetSketchesPerUser(args[0], args[1]);

            Dictionary<string, List<Substroke>> user2gateStrokes = new Dictionary<string, List<Substroke>>();
            Dictionary<string, List<Substroke>> user2otherStrokes = new Dictionary<string, List<Substroke>>();

            foreach (KeyValuePair<string, List<string>[]> pair in user2sketches)
            {
                string user = pair.Key;
                user2gateStrokes.Add(user, new List<Substroke>());
                user2otherStrokes.Add(user, new List<Substroke>());
                int userNum;
                bool good = int.TryParse(user.Substring(0, 2), out userNum);
                if (good && userNum <= 0)
                    continue;

                List<string> sketchFiles = pair.Value[0];
                List<string> testFiles = pair.Value[1];

                foreach (string sketchFile in sketchFiles)
                {
                    Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                    sketch = General.ReOrderParentShapes(sketch);

                    foreach (Substroke s in sketch.Substrokes)
                    {
                        if (s.Labels.Length > 0)
                        {
                            string label = s.Labels[s.Labels.Length - 1];
                            if (leafLabels.Contains(label))
                                user2gateStrokes[user].Add(s);
                        }
                    }
                }

                foreach (string sketchFile in testFiles)
                {
                    Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                    sketch = General.ReOrderParentShapes(sketch);

                    foreach (Substroke s in sketch.Substrokes)
                    {
                        if (s.Labels.Length > 0)
                        {
                            string label = s.Labels[s.Labels.Length - 1];
                            string root = s.Labels[0];
                            if (label == "Wire" || label == "LabelBoxOther")// || root == "LabelBox")
                                user2otherStrokes[user].Add(s);
                        }
                    }
                }
            }

            Dictionary<string, ConfusionMatrix> user2Confusion = new Dictionary<string, ConfusionMatrix>();
            ConfusionMatrix allConfusion = new ConfusionMatrix(allLabels);

            // Use this to train a rubine recognizer on all substrokes...
            //RubineRecognizerUpdateable rubine = new RubineRecognizerUpdateable();

            foreach (string user in user2gateStrokes.Keys)
            {
                RubineRecognizerUpdateable rubine = new RubineRecognizerUpdateable();
                foreach (Substroke s in user2gateStrokes[user])
                    if (s.Labels.Length > 0)
                        rubine.Add(s.Labels[s.Labels.Length - 1], s.PointsL);

                foreach (KeyValuePair<string, List<Substroke>> kv in user2otherStrokes)
                {
                    if (kv.Key.Substring(0, 2) != user.Substring(0, 2))
                        foreach (Substroke s in kv.Value)
                            if (s.Labels.Length > 0)
                            { }//rubine.Add(s.Labels[s.Labels.Length - 1], s.PointsL);
                }

                bool updated = rubine.updateMatrices();
                if (!updated)
                    continue;

                ConfusionMatrix confusion = new ConfusionMatrix(allLabels);

                foreach (string sketchFile in user2sketches[user][1])
                {
                    Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                    sketch = General.ReOrderParentShapes(sketch);

                    foreach (Substroke s in sketch.Substrokes)
                    {
                        if (s.Labels.Length > 0)
                        {
                            string label = s.Labels[s.Labels.Length - 1];
                            if (leafLabels.Contains(label))
                            {
                                string result = rubine.Recognize(s);
                                confusion.Add(label, result);
                                allConfusion.Add(label, result);
                            }
                        }
                    }
                }
                confusion.IncreaseAllByOne();
                user2Confusion.Add(user, confusion);
            }
            allConfusion.IncreaseAllByOne();

            System.IO.StreamWriter allWriter = new System.IO.StreamWriter("allConfusion.txt");
            allConfusion.Print(allWriter);
            allWriter.Close();
            
            foreach (KeyValuePair<string, ConfusionMatrix> kv in user2Confusion)
            {
                string user = kv.Key;
                System.IO.StreamWriter writer = new System.IO.StreamWriter("Confusion" + user + ".txt");

                kv.Value.Print(writer);

                writer.Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private static Dictionary<string, List<string>[]> GetSketchesPerUser(string dir, string search)
        {
            Dictionary<string, List<string>[]> sketches = new Dictionary<string, List<string>[]>();

            List<string> files = new List<string>(System.IO.Directory.GetFiles(dir, search));

            foreach (string f in files)
            {
                string fShort = System.IO.Path.GetFileName(f);
                string user = fShort.Substring(0, 2);
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

            return sketches;
        }

    }
}
