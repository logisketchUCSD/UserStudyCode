using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using ConverterXML;

namespace RemoveStrokesByClass
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> ClassesToRemove = new List<string>();
            ClassesToRemove.Add("Equation");
            ClassesToRemove.Add("Geometry");
            ClassesToRemove.Add("Other");

            string fileMap = "C:\\Documents and Settings\\eric\\Desktop\\Statics Study\\StaticsMap_v1.txt";
            Dictionary<string, string> map = GetMapping(fileMap);

            string sketchDir = "C:\\Documents and Settings\\eric\\Desktop\\Statics Study\\Labeled_Sketches\\";
            string[] sketches = Directory.GetFiles(sketchDir, "*.labeled.xml");

            string saveDir = "C:\\Documents and Settings\\eric\\Desktop\\Statics Study\\Labeled_Sketches_no_EGO\\";

            foreach (string sketchFile in sketches)
            {
                Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                List<Substroke> toRemove = new List<Substroke>();

                foreach (Substroke stroke in sketch.SubstrokesL)
                {
                    string classification = stroke.FirstLabel;
                    string cls = map[classification];
                    if (ClassesToRemove.Contains(cls))
                        toRemove.Add(stroke);
                }

                foreach (Substroke stroke in toRemove)
                    sketch.RemoveSubstrokeByID(stroke);

                string fileShort = Path.GetFileName(sketchFile);
                string newFile = saveDir + fileShort;

                new MakeXML(sketch).WriteXML(newFile);
            }


        }

        private static Dictionary<string, string> GetMapping(string filename)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            StreamReader reader = new StreamReader(filename);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                if (line.Substring(0, 2) == "//") continue;

                string shapeName = line.Substring(0, line.IndexOf(":"));
                string className = line.Substring(line.IndexOf(":") + 2);

                if (!map.ContainsKey(shapeName))
                    map.Add(shapeName, className);
            }

            reader.Close();

            return map;
        }
    }
}
