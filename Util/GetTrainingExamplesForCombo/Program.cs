using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using Featurefy;
using ConverterXML;
using CombinationRecognizer;
using Utilities;

namespace GetTrainingExamplesForCombo
{
    class Program
    {
        static List<string> shapeLabels = new List<string>(new string[] {
            "AND", "OR", "NAND", "NOR", "NOT", "NOTBUBBLE", "XOR", "LabelBox" });

        static void Main(string[] args)
        {
            // Usage
            // args[0]: Directory for shape/stroke recognizers
            // args[1]: Directory for sketches
            // args[2]: Sketch search path (*.labeled.xml)
            // args[3]: Text file to save examples to

            string dirR = args[0];
            string dirS = Path.GetDirectoryName(args[3]);
            List<string> allFiles = new List<string>(Directory.GetFiles(args[1], args[2]));
            List<string> sketchFiles = new List<string>();
            foreach (string file in allFiles)
                if (file.Contains("EQ") || file.Contains("COPY"))
                    sketchFiles.Add(file);

            StreamWriter writer = new StreamWriter(args[3]);

            Dictionary<string, List<string>> user2sketches = new Dictionary<string, List<string>>();
            foreach (string file in sketchFiles)
            {
                string user = GetUser(file);
                if (!user2sketches.ContainsKey(user))
                    user2sketches.Add(user, new List<string>());
                
                user2sketches[user].Add(file);                
            }

            foreach (KeyValuePair<string, List<string>> kv in user2sketches)
            {
                string user = kv.Key;
                System.IO.StreamWriter currentWriter = new System.IO.StreamWriter(dirS + "\\" + user + ".txt");
                ComboRecognizer combo = CreateCombo(user, dirR);
                if (combo == null) continue;

                List<string> sketches = kv.Value;
                foreach (string file in sketches)
                {
                    Sketch.Sketch sketch = new ReadXML(file).Sketch;
                    sketch = ReOrderParentShapes(sketch);
                    FeatureSketch fSketch = new FeatureSketch(ref sketch);
                    List<Shape> shapes = GetParentShapes(sketch);

                    foreach (Shape shape in shapes)
                    {
                        Dictionary<string, object> features = combo.GetIndRecognitionResults(shape);
                        WriteFeatures(shape.Label, features, writer);
                        WriteFeatures(shape.Label, features, currentWriter);

                        List<Shape> derivatives = GetShapeDerivatives(shape, fSketch);
                        foreach (Shape der in derivatives)
                        {
                            string label = GetDerivativeLabel(der);
                            Dictionary<string, object> dFeatures = combo.GetIndRecognitionResults(der);
                            WriteFeatures(label, dFeatures, writer);
                            WriteFeatures(label, dFeatures, currentWriter);
                        }
                    }
                }

                currentWriter.Close();
            }

            writer.Close();
        }

        private static string GetDerivativeLabel(Shape der)
        {
            return "NONE";
        }

        private static List<Shape> GetShapeDerivatives(Shape shape, FeatureSketch fSketch)
        {
            List<Shape> derivatives = new List<Shape>();
            Dictionary<Substroke, List<SubstrokeDistance>> distances = fSketch.PairwiseFeatureSketch.AllDistances;
            SortedList<double, Substroke> closest = GetClosestStrokes(shape, distances);

            

            return derivatives;
        }

        private static SortedList<double, Substroke> GetClosestStrokes(Shape shape, Dictionary<Substroke, List<SubstrokeDistance>> distances)
        {
            SortedList<double, Substroke> closest = new SortedList<double, Substroke>();

            foreach (Substroke s in shape.Substrokes)
            {
                if (!distances.ContainsKey(s)) continue;

                List<SubstrokeDistance> dist = distances[s];
                SortedList<double, Substroke> current = new SortedList<double, Substroke>();
                foreach (SubstrokeDistance d in dist)
                {
                    double min = d.Min;
                    while (current.ContainsKey(min))
                        min += double.MinValue;

                    if (d.StrokeA == s && !current.ContainsValue(d.StrokeB))
                        current.Add(min, d.StrokeB);
                    else if (d.StrokeB == s && !current.ContainsValue(d.StrokeA))
                        current.Add(min, d.StrokeA);
                }

                foreach (KeyValuePair<double, Substroke> kv in current)
                {
                    double min = kv.Key;
                    Substroke stroke = kv.Value;
                    while (closest.ContainsKey(min))
                        min += double.MinValue;

                    if (closest.ContainsValue(stroke))
                    {
                        int index = closest.IndexOfValue(stroke);
                        List<double> keys = new List<double>(closest.Keys);
                        double key = keys[index];
                        if (min < key)
                        {
                            closest.RemoveAt(index);
                            closest.Add(min, stroke);
                        }
                    }
                    else
                        closest.Add(min, stroke);
                }
            }

            return closest;
        }

        private static ComboRecognizer CreateCombo(string user, string dir)
        {
            string dollarStr = dir + "\\Dollar" + user + ".dr";
            bool dollarBool = File.Exists(dollarStr);
            string rubineStr = dir + "\\RubineLite" + user + ".rr";
            bool rubineBool = File.Exists(rubineStr);
            string zernikeStr = dir + "\\ZernikeLite" + user + ".zr";
            bool zernikeBool = File.Exists(zernikeStr);
            string imageStr = dir + "\\Image" + user + ".ir";
            bool imageBool = File.Exists(imageStr);

            if (!(dollarBool && rubineBool && zernikeBool && imageBool))
                return null;

            DollarRecognizer dr = DollarRecognizer.Load(dollarStr);
            RubineRecognizer rr = RubineRecognizer.Load(rubineStr);
            ZernikeMomentRecognizer zr = ZernikeMomentRecognizer.Load(zernikeStr);
            ImageRecognizer ir = ImageRecognizer.Load(imageStr);

            ComboRecognizer combo = new ComboRecognizer(rr, dr, zr, ir);

            return combo;
        }

        private static string GetUser(string file)
        {
            string fShort = System.IO.Path.GetFileName(file);
            string user = fShort.Substring(0, 2);
            if (fShort.Contains("_T"))
                user += "T";
            else if (fShort.Contains("_P"))
                user += "P";

            return user;
        }

        private static void WriteFeatures(string className, Dictionary<string, object> features, System.IO.StreamWriter writer)
        {
            foreach (object value in features.Values)
                writer.Write(value + ",");

            writer.WriteLine(className);
        }

        private static List<Shape> GetParentShapes(Sketch.Sketch sketch)
        {
            List<Shape> shapes = new List<Shape>();

            if (sketch != null)
                foreach (Substroke s in sketch.Substrokes)
                    if (s.ParentShapes.Count > 0 
                        && s.Labels.Length > 0 
                        && !shapes.Contains(s.ParentShapes[0]) 
                        && shapeLabels.Contains(s.ParentShapes[0].Label))
                            shapes.Add(s.ParentShapes[0]);

            return shapes;
        }      

        private static Sketch.Sketch ReOrderParentShapes(Sketch.Sketch sketch)
        {
            List<List<string>> hierarchy = GetHierarchy();

            foreach (Substroke stroke in sketch.Substrokes)
            {
                List<string> labels = new List<string>(stroke.Labels);
                List<Shape> oldParents = new List<Shape>(stroke.ParentShapes);
                List<Shape> parents = new List<Shape>();

                foreach (List<string> order in hierarchy)
                {
                    foreach (string shape in order)
                    {
                        if (labels.Contains(shape))
                        {
                            foreach (Shape s in oldParents)
                            {
                                if (s.Label == shape)
                                {
                                    parents.Add(s);
                                }
                            }
                        }
                    }
                }

                stroke.ParentShapes = parents;
            }

            return sketch;
        }

        private static List<List<string>> GetHierarchy()
        {
            List<List<string>> hierarchy = new List<List<string>>();
            List<string> first = new List<string>();
            first.Add("Wire");
            first.Add("Label");
            first.Add("XNOR");
            first.Add("NOT");
            first.Add("LabelBox");

            List<string> second = new List<string>();
            second.Add("NOR");
            second.Add("NAND");
            second.Add("XOR");

            List<string> third = new List<string>();
            third.Add("AND");
            third.Add("OR");
            third.Add("NOTBUBBLE");

            List<string> fourth = new List<string>(new string[] {
            "BackLine", "FrontArc", "BackArc", "TopArc", "BottomArc",
            "Bubble", "TopLine", "BottomLine", "Triangle", 
            "GreaterThan", "TouchUp", "Junk", "A", "B", "C", "D", 
            "Y", "LabelBoxOther", "Not_V", "Not_Hat", "Entire_OR", "Entire_AND" });

            hierarchy.Add(first);
            hierarchy.Add(second);
            hierarchy.Add(third);
            hierarchy.Add(fourth);

            return hierarchy;
        }

    }
}
