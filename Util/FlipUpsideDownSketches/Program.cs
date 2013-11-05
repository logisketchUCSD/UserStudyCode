using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using ConverterXML;

namespace FlipUpsideDownSketches
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                return;

            string[] files = Directory.GetFiles(args[0], args[1]);

            string dirOut = args[0] + "\\Flipped\\";
            if (!Directory.Exists(dirOut))
                Directory.CreateDirectory(dirOut);

            float minT = 3000.0f;

            foreach (string file in files)
            {
                Sketch.Sketch sketch = new ReadXML(file).Sketch;

                if (file.Contains("_P.labeled"))
                {
                    float max = 0f;
                    foreach (Point pt in sketch.Points)
                        max = Math.Max(max, pt.Y);

                    max += minT;

                    foreach (Point pt in sketch.Points)
                        pt.Y = max - pt.Y;
                }

                MakeXML make = new MakeXML(sketch);
                make.WriteXML(dirOut + Path.GetFileName(file));
            }
        }
    }
}
