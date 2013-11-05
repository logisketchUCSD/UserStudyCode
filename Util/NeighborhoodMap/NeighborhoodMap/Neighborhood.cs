using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using System.Drawing;

using Set;

namespace NeighborhoodMap
{

    /// <summary>
    /// Class for testing NeighborhoodMap generation and playing with it
    /// </summary>
    public static class Neighborhood
    {

        /// <summary>
        /// A quick hacky main function to test this.
        /// Uses hard-coded paths for input and output files.
        /// </summary>
        /// <param name="args">unused</param>
        public static void Main(String[] args)
        {

            String filename = @"c:\documents and settings\christine\my documents\sketch\data\e85\new labeled documents\labeled summer 2007\0268_1.6.1.labeled.xml";
            string outfile = @"c:\documents and settings\christine\my documents\out.xml";

            Sketch.Sketch sketch = new ConverterXML.ReadXML(filename).Sketch;
            sketch.RemoveShapes();
            Console.WriteLine("beginning to find neighborhood graph");
            NeighborhoodMap graph = new NeighborhoodMap(sketch, 100, false, 5);
            Console.WriteLine("done");
            AddDebuggingLines(ref sketch, graph);
            Console.WriteLine("writing xml");
            ConverterXML.MakeXML mx = new ConverterXML.MakeXML(sketch);
            mx.WriteXML(outfile);
            Console.Read();
        }

        /// <summary>
        /// an attempt to find labels based on what didn't have connections in certain directions.
        /// didn't work very well.
        /// </summary>
        /// <param name="sketch"></param>
        /// <param name="graph"></param>
        private static void AddLabelShapes(ref Sketch.Sketch sketch, NeighborhoodMap graph)
        {
            foreach (Substroke s in graph.Substrokes)
            {
                Sketch.Point a = s.PointsL[0];
                Sketch.Point b = s.PointsL[s.PointsL.Count - 1];
                Sketch.Point m = s.PointsL[s.PointsL.Count / 2]; 

                double dir = 0d;
                int x = 0;
                bool found = true;
                if (graph[s].Count > 1)
                {
                    bool N, E, W, S;
                    N = E = W = S = false;

                    foreach (Neighbor n in graph[s])
                    {
                        //dir = direction(m, n.neighbor.PointsL[n.neighbor.PointsL.Count/2]);
                        dir = NeighborhoodMap.direction(m, n.dest);
                        if (dir > -Math.PI/3 && dir < Math.PI/3)
                            E = true;
                        if (dir > Math.PI/6 && dir < 5*Math.PI/6)
                            N = true;
                        if (dir > 2*Math.PI/3 || dir < -2*Math.PI/3)
                            W = true;
                        if (dir > -5 * Math.PI / 6 && dir < Math.PI / 6)
                            S = true;
                    }
                    found = !((N&&S) || (E&&W));
                }
                
                if (found)
                {
                    Shape label = new Shape(new List<Substroke>(new Substroke[] { s }),
                        new XmlStructs.XmlShapeAttrs(true));
                    label.XmlAttrs.Type = "Label";
                    label.XmlAttrs.Name = "shape";
                    label.XmlAttrs.Time = 1;
                    sketch.AddShape(label);
                }
            }
        }

        /// <summary>
        /// Add "strokes" connecting the middle of substrokes adjacent in the graph.
        /// </summary>
        /// <param name="sketch">sketch to modify</param>
        /// <param name="graph">neighborhood graph</param>
        private static void AddDebuggingLines(ref Sketch.Sketch sketch, NeighborhoodMap graph)
        {
            Dictionary<Substroke, Sketch.Point> midpoints = new Dictionary<Substroke, Sketch.Point>();
            foreach (Substroke s in sketch.Substrokes)
            {
                midpoints.Add(s, s.PointsL[s.PointsL.Count / 2]);
            }
            foreach (Substroke s in graph.Substrokes)
            {
                foreach (Neighbor n in graph[s])
                {

                    Substroke sub = new Substroke(new Sketch.Point[] { midpoints[s], midpoints[n.neighbor] },
                        new XmlStructs.XmlShapeAttrs(true));
                    sub.XmlAttrs.Time = 0;
                    sub.XmlAttrs.Name = "substroke";
                    sub.XmlAttrs.Type = "substroke";
                    Stroke str = new Stroke(sub);
                    str.XmlAttrs.Time = 0;
                    str.XmlAttrs.Name = "stroke";
                    str.XmlAttrs.Type = "stroke";
                    Shape xor = new Shape(new List<Substroke>(new Substroke[] { sub }),
                        new XmlStructs.XmlShapeAttrs(true));
                    xor.XmlAttrs.Type = "BUBBLE";
                    xor.XmlAttrs.Name = "Shape";
                    xor.XmlAttrs.Time = 0;
                    sketch.AddStroke(str);
                    sketch.AddShape(xor);
                }
            }
        }

        /// <summary>
        /// Add "strokes" connecting the middle of substrokes adjacent in the graph.
        /// ID labels absed on 
        /// </summary>
        /// <param name="sketch">sketch to modify</param>
        /// <param name="graph">neighborhood graph</param>
        private static void AddDebuggingLines2(ref Sketch.Sketch sketch, NeighborhoodMap graph)
        {
            string[] labels = { "AND", "OR", "NOT", "NAND", "NOR", "XOR", "Wire", "BUBBLE", "Label" };
            List<List<Substroke>> components = graph.connectedComponents();

            #region midpoints
            Dictionary<Substroke, Sketch.Point> midpoints = new Dictionary<Substroke, Sketch.Point>();
            foreach (Substroke s in sketch.Substrokes)
            {
                midpoints.Add(s, s.PointsL[s.PointsL.Count / 2]);
            }
            #endregion

            double bbminx, bbmaxx, bbminy, bbmaxy;
            double bbarea = area(graph.Substrokes, out bbminx, out bbmaxx, out bbminy, out bbmaxy);
            double THRESH = Math.Min(bbmaxx-bbminx, bbmaxy-bbminy)/5;

            foreach (List<Substroke> group in components)
            {
                Shape xyz = new Shape(group, new XmlStructs.XmlShapeAttrs(true));
                xyz.XmlAttrs.Name = "Shape";
                xyz.XmlAttrs.Time = 1;

                double minx,maxx,miny,maxy;
                double a = area(group, out minx, out maxx, out miny, out maxy);
                // check if it is an outer group and relatively small
                if ((minx - THRESH <= bbminx || maxx + THRESH >= bbmaxx ||
                     miny - THRESH <= bbminy || maxy + THRESH >= bbmaxy) &&
                     a <= bbarea / 40.0)
                {
                    xyz.XmlAttrs.Type = "Label";
                    Console.Write("label!");
                }
                else
                {
                    xyz.XmlAttrs.Type = "Wire";
                }
                sketch.AddShape(xyz);
            }

            #region old
            /*
            foreach (Substroke s in graph.Substrokes)
            {

                foreach (Neighbor n in graph[s])
                {

                    Substroke sub = new Substroke(new Sketch.Point[] { midpoints[s], midpoints[n.neighbor] },
                        new XmlStructs.XmlShapeAttrs(true));
                    sub.XmlAttrs.Time = 0;
                    sub.XmlAttrs.Name = "substroke";
                    sub.XmlAttrs.Type = "substroke";
                    Stroke str = new Stroke(sub);
                    str.XmlAttrs.Time = 0;
                    str.XmlAttrs.Name = "stroke";
                    str.XmlAttrs.Type = "stroke";
                    Shape xor = new Shape(new List<Substroke>(new Substroke[] { sub }),
                        new XmlStructs.XmlShapeAttrs(true));
                    xor.XmlAttrs.Type = thislabel;
                    xor.XmlAttrs.Name = "Shape";
                    xor.XmlAttrs.Time = 0;
                    sketch.AddStroke(str);
                    sketch.AddShape(xor);
                }
            }
             * */
            #endregion
        }

        /// <summary>
        /// Add "strokes" connecting the middle of substrokes adjacent in the graph.
        /// </summary>
        /// <param name="sketch">sketch to modify</param>
        /// <param name="graph">neighborhood graph</param>
        private static void AddDebuggingCycles(ref Sketch.Sketch sketch, NeighborhoodMap graph)
        {
            Dictionary<Substroke, Sketch.Point> midpoints = new Dictionary<Substroke, Sketch.Point>();
            foreach (Substroke s in sketch.Substrokes)
            {
                midpoints.Add(s, s.PointsL[s.PointsL.Count / 2]);
            }
            foreach (Substroke s in graph.Substrokes)
            {
                foreach (Neighbor n in graph[s])
                {
                    if (!graph.Edge(n.neighbor, s)) continue;
                    Substroke sub = new Substroke(new Sketch.Point[] { midpoints[s], midpoints[n.neighbor] },
                        new XmlStructs.XmlShapeAttrs(true));
                    sub.XmlAttrs.Time = 0;
                    sub.XmlAttrs.Name = "substroke";
                    sub.XmlAttrs.Type = "substroke";
                    Stroke str = new Stroke(sub);
                    str.XmlAttrs.Time = 0;
                    str.XmlAttrs.Name = "stroke";
                    str.XmlAttrs.Type = "stroke";
                    Shape xor = new Shape(new List<Substroke>(new Substroke[] { sub }),
                        new XmlStructs.XmlShapeAttrs(true));
                    xor.XmlAttrs.Type = "BUBBLE";
                    xor.XmlAttrs.Name = "Shape";
                    xor.XmlAttrs.Time = 0;
                    sketch.AddStroke(str);
                    sketch.AddShape(xor);
                }
            }
        }

        /// <summary>
        /// area of bounding box of comp
        /// </summary>
        /// <param name="comp">list of strokes</param>
        /// <returns></returns>
        private static double area(ICollection<Substroke> comp, 
            out double minx, out double maxx, out double miny, out double maxy)
        {
            maxx = maxy = double.MinValue;
            minx = miny = double.MaxValue;

            foreach (Substroke s in comp)
            {
                foreach (Sketch.Point p in s.PointsL)
                {
                    if (p.X < minx) minx = p.X;
                    if (p.X > maxx) maxx = p.X;
                    if (p.Y < miny) miny = p.Y;
                    if (p.Y > maxy) maxy = p.Y;
                }
            }

            return (maxx - minx) * (maxy - miny);
        }

    }


}
