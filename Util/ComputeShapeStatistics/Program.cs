using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using ConverterXML;
using Utilities;

namespace ComputeShapeStatistics
{
    class Program
    {
        static Dictionary<string, string> abbr;

        static void Main(string[] args)
        {
            // args:
            //    0 = Directory of sketches
            //    1 = Search pattern
            //    2 = Output file

            if (args.Length != 3)
                return;

            GetAbbreviations();

            string[] files = Directory.GetFiles(args[0], args[1]);

            StreamWriter writer = new StreamWriter(args[2]);

            foreach (KeyValuePair<string, string> kvp in abbr)
                writer.WriteLine(kvp.Key + "," + kvp.Value);

            writer.WriteLine("UserName,Task,ShapeName,NumStrokes,NumTouchUps,StrokeOrder");

            foreach (string file in files)
            {
                Sketch.Sketch sketch = new ReadXML(file).Sketch;
                sketch = General.ReOrderParentShapes(sketch);
                sketch = General.LinkShapes(sketch);

                string sketchFile = Path.GetFileName(file);
                string user = sketchFile.Substring(0, 2);
                string task = "Isolated";
                if (sketchFile.Contains("COPY"))
                    task = "Copy";
                else if (sketchFile.Contains("EQ"))
                    task = "Synthesize";
                

                foreach (Shape shape in sketch.ShapesL)
                {
                    if (shape.ParentShape.SubstrokesL.Count != 0 || !General.IsGate(shape))
                        continue;

                    string order = GetStrokeOrderString(shape);
                    int numTouchups = GetNumTouchups(shape);

                    writer.Write(user + ",");
                    writer.Write(task + ",");
                    writer.Write(shape.Type + ",");
                    writer.Write(shape.SubstrokesL.Count.ToString() + ",");
                    writer.Write(numTouchups.ToString() + ",");
                    writer.Write(order);
                    writer.WriteLine();
                }
            }

            writer.Close();
        }

        private static int GetNumTouchups(Shape shape)
        {
            int count = 0;

            foreach (Substroke stroke in shape.SubstrokesL)
                if (stroke.Labels[stroke.Labels.Length - 1] == "TouchUp")
                    count++;

            return count;
        }

        private static string GetStrokeOrderString(Shape shape)
        {
            SortedList<ulong, string> order = new SortedList<ulong, string>();

            foreach (Substroke stroke in shape.SubstrokesL)
                order.Add(stroke.XmlAttrs.Time.Value, stroke.Labels[stroke.Labels.Length - 1]);

            string combo = "";
            foreach (string label in order.Values)
                combo += abbr[label] + "_";

            combo = combo.Substring(0, combo.Length - 1);

            return combo;
        }

        private static void GetAbbreviations()
        {
            abbr = new Dictionary<string, string>();
            abbr.Add("BackLine", "BL");
            abbr.Add("FrontArc", "FA");
            abbr.Add("BackArc", "BA");
            abbr.Add("TopArc", "TopA");
            abbr.Add("BottomArc", "BotA");
            abbr.Add("Bubble", "Bub");
            abbr.Add("TopLine", "TopL");
            abbr.Add("BottomLine", "BotL");
            abbr.Add("Triangle", "Tri");
            abbr.Add("GreaterThan", "GT");
            abbr.Add("TouchUp", "TU");
            abbr.Add("Junk", "Jn");
            abbr.Add("Not_V", "NV");
            abbr.Add("Not_Hat", "NH");
            abbr.Add("LabelBoxOther", "LBO");
            abbr.Add("Entire_OR", "EnOR");
            abbr.Add("Entire_AND", "EnAND");
        }
    }
}
