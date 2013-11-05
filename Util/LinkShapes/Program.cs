using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConverterXML;
using Sketch;
using Utilities;

namespace LinkShapes
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = "C:\\Documents and Settings\\eric\\My Documents\\Trunk\\Data\\Gate Study Data\\LabeledPartsSketches\\COPY_and_EQN sketches\\";
            string searchPattern = "*.labeled.xml";
            string[] files = Directory.GetFiles(dir, searchPattern);

            foreach (string filename in files)
            {
                //string filename = dir + "01_EQ1_UCR_T.labeled.xml";
                Sketch.Sketch sketch = new ReadXML(filename).Sketch;
                MakeXML make2 = new MakeXML(sketch);
                make2.WriteXML(filename);

                sketch = General.ReOrderParentShapes(sketch);

                foreach (Substroke stroke in sketch.SubstrokesL)
                    for (int i = 0; i < stroke.ParentShapes.Count - 1; i++)
                        stroke.ParentShapes[i].AddShape(stroke.ParentShapes[i + 1]);

                MakeXML make = new MakeXML(sketch);
                make.WriteXML(filename);
            }
        }
    }
}
