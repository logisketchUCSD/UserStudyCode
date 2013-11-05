using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Sketch;

namespace ConverterDRS
{
    public class ReadDRS
    {

        private Sketch.Sketch sketch;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="filename">path to filename to load in</param>
        public ReadDRS(string filename)
        {
            sketch = load(filename);
        }

        /// <summary>
        /// load a sketch object from a DRS file
        /// </summary>
        /// <param name="filename">path to DRS formatted sketch</param>
        /// <returns></returns>
        public static Sketch.Sketch load(string filename)
        {
            if (!File.Exists(filename)) 
                throw new Exception(string.Format("cannot load nonexistent file '{0}'.", filename));

            StreamReader sr = new StreamReader(filename);
            int next = 0;
            Sketch.Sketch res = new Sketch.Sketch();
            res.XmlAttrs.Id = Guid.NewGuid();

            while (next >= 0)
            {
                //find beginning of stroke
                while ((next = sr.Read()) >= 0 && next != '{') continue;
                if (next < 0) break; //no more strokes

                List<Point> lp = new List<Point>();
                //find points
                while (next != '}')
                {
                    string x = "", y = "", time = "";
                    //beginning of point
                    while ((next = sr.Read()) >= 0 && next != '(' && next != '}') continue;
                    if (next < 0) throw new Exception("EOF found inside a stroke");
                    if (next == '}') break; //no more points

                    // X coordinate
                    while ((next = sr.Read()) >= 0 && next != ' ') x += (char)next;
                    if (next < 0) throw new Exception("EOF found inside a point");
                    // Y coordinate
                    while ((next = sr.Read()) >= 0 && next != ' ') y += (char)next;
                    if (next < 0) throw new Exception("EOF found inside a point");
                    // timestamp
                    while ((next = sr.Read()) >= 0 && next != ')') time += (char)next;
                    if (next < 0) throw new Exception("EOF found inside a point");

                    Point p = new Point(float.Parse(x), float.Parse(y));
					if (time[0] == '-')
						p.XmlAttrs.Time = UInt64.MaxValue - (UInt64)Math.Abs(Int64.Parse(time));
					else
						p.XmlAttrs.Time = ulong.Parse(time);
                    p.XmlAttrs.Name = "point";
					p.XmlAttrs.Pressure = 127;

                    lp.Add(p);
                }

                Substroke sub = new Substroke(lp, new XmlStructs.XmlShapeAttrs(true));
                sub.XmlAttrs.Id = Guid.NewGuid();
                sub.XmlAttrs.Name = "substroke";
                sub.XmlAttrs.Type = "substroke";
                sub.XmlAttrs.Time = sub.PointsL[sub.PointsL.Count - 1].Time;

                Stroke stroke = new Stroke(sub);
                stroke.XmlAttrs.Id = Guid.NewGuid();
                stroke.XmlAttrs.Name = "stroke";
                stroke.XmlAttrs.Type = "stroke";
                stroke.XmlAttrs.Time = sub.XmlAttrs.Time;

                res.AddStroke(stroke);
            }

            return res;
        }

        /// <summary>
        /// get the sketch loaded in by this class
        /// </summary>
        public Sketch.Sketch Sketch
        {
            get
            {
                return sketch;
            }
        }
    }
}
