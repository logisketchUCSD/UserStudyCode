using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using System.IO;
using Featurefy;

namespace GateStudy2
{
    class GateStats
    {

        string[] files;
        private string header;
        private ArcLength arclength;
        private Slope slope;
        private Curvature curve;

        /// <summary>
        /// Constructor for the GateStats class
        /// </summary>
        /// <param name="files">The different types of names of files (the variability
        /// besides the user number)</param>
        /// <param name="header">The header for the .csv file</param>
        public GateStats(string[] files, string header)
        {
            this.files = files;
            this.header = header; 
        }

        /// <summary>
        /// Returns the number of substrokes in a shape 
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns>The number of substrokes in the shape</returns>
        public int numSubStrokes(Sketch.Shape shape)
        {
            return shape.Substrokes.Length;
        }
        
        /// <summary>
        /// Returns the number of users that completed the study
        /// </summary>
        /// <param name="filepath">the folder in which the files are located</param>
        /// <returns>the number of users</returns>
        public int findNumUsers(string filepath)
        {
            for (int i = 1; i < 100; i++)
            {
                if (!File.Exists(filepath + i + "_" + files[0] + ".labeled.xml"))
                    return i - 1;
            }
            return 0;
        }

        /// <summary>
        /// Returns a list of all the points that make up a shape
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns>a list of all the points within the shape</returns>
        public List<Point> findPointsInShape(Sketch.Shape shape)
        {
            List<Substroke> subs = shape.SubstrokesL;
            List<Point> points = new List<Point>();
            foreach (Substroke sub in subs)
            {
                foreach (Point p in sub.Points)
                {
                    points.Add(p);
                }
            }
            return points;
        }

        /// <summary>
        /// Finds all of the Wire objects in the sketch
        /// </summary>
        /// <param name="sketch">The Sketch to look in</param>
        /// <returns>a list of all the wires in the sketch</returns>
        public List<Shape> findWiresInSketch(Sketch.Sketch sketch)
        {
            List<Shape> wires = new List<Shape>();
            foreach (Shape shape in sketch.ShapesL)
            {
                if (shape.XmlAttrs.Type == "Wire")
                    wires.Add(shape);
            }
            return wires;
        }

        /// <summary>
        /// Finds all the Label objects in the sketch
        /// </summary>
        /// <param name="sketch">The sketch to look in</param>
        /// <returns>a list of all the labels in the sketch</returns>
        public List<Shape> findLabelsInSketch(Sketch.Sketch sketch)
        {
            List<Shape> labels = new List<Shape>();
            foreach (Shape shape in sketch.ShapesL)
            {
                if (shape.XmlAttrs.Type == "Label")
                    labels.Add(shape);
            }
            return labels;
        }

        /// <summary>
        /// Finds all the gates in the sketch
        /// </summary>
        /// <param name="sketch">The sketch to look in</param>
        /// <returns>a list of all the gates in the sketch</returns>
        public List<Shape> findGatesInSketch(Sketch.Sketch sketch)
        {
            List<Shape> gates = new List<Shape>();
            foreach (Shape shape in sketch.ShapesL)
            {
                if (shape.XmlAttrs.Type == "AND" || shape.XmlAttrs.Type == "OR" || shape.XmlAttrs.Type == "XOR" || shape.XmlAttrs.Type == "NAND" || shape.XmlAttrs.Type == "NOR" || shape.XmlAttrs.Type == "NOT")
                    gates.Add(shape);
            }
            return gates;
        }

        /// <summary>
        /// Finds the minimum distance from a wire to the surrounding gates
        /// </summary>
        /// <param name="gates">The list of gates</param>
        /// <param name="wire">The wire</param>
        /// <returns>The minimum distance from the wire to any gate</returns>
        public double minDistWireToGates(List<Shape> gates, Shape wire)
        {
            double minDist = double.PositiveInfinity;
            foreach (Shape gate in gates)
            {
                foreach (Substroke sg in gate.SubstrokesL)
                {
                    foreach (Point pg in sg.PointsL)
                    {
                        foreach (Substroke sw in wire.SubstrokesL)
                        {
                            foreach (Point pw in sw.PointsL)
                            {
                                if (Dist(pg, pw) < minDist)
                                    minDist = Dist(pg, pw);
                            }
                        }
                    }
                }
            }
            return minDist;
        }

        /// <summary>
        /// Finds the minimum distance from a gate to its surrounding wires
        /// </summary>
        /// <param name="wires">a list of wires</param>
        /// <param name="gate">The gate</param>
        /// <returns>The minimum distance from the gate to any wire</returns>
        public double minDistGateToWires(List<Shape> wires, Shape gate)
        {
            double minDist = double.PositiveInfinity;
            foreach (Shape wire in wires)
            {
                foreach (Substroke sg in gate.SubstrokesL)
                {
                    foreach (Point pg in sg.PointsL)
                    {
                        foreach (Substroke sw in wire.SubstrokesL)
                        {
                            foreach (Point pw in sw.PointsL)
                            {
                                if (Dist(pg, pw) < minDist)
                                    minDist = Dist(pg, pw);
                            }
                        }
                    }
                }
            }
            return minDist;
        }

        
        /// <summary>
        /// Finds the minimum distance from a label to the surrounding wires
        /// </summary>
        /// <param name="wires">The list of wires</param>
        /// <param name="label">The label</param>
        /// <returns>the minimum distance from the label to any wire</returns>
        public double minDistLabelToWires(List<Shape> wires, Shape label)
        {
            double minDist = double.PositiveInfinity;
            foreach (Shape wire in wires)
            {
                foreach(Substroke sw in wire.SubstrokesL)
                {
                    foreach (Point pw in sw.PointsL)
                    {
                        foreach (Substroke sl in label.SubstrokesL)
                        {
                            foreach (Point pl in sl.PointsL)
                            {
                                if (Dist(pl, pw) < minDist)
                                    minDist = Dist(pl, pw);
                            }
                        }
                    }
                }
            }
            return minDist;
        }

        /// <summary>
        /// Finds the distance between two points
        /// </summary>
        /// <param name="pt1">The first point</param>
        /// <param name="pt2">The second point</param>
        /// <returns>the distance between the two points</returns>
        public double Dist(Point pt1, Point pt2)
        {
            double dist = Math.Sqrt((pt1.X - pt2.X) * (pt1.X - pt2.X) + (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y));
            return dist;
        }

        /// <summary>
        /// Returns the time it took to draw the shape
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns>total time it took to draw the shape in ms</returns>
        public ulong findShapeTime(Sketch.Shape shape)
        {
            double minTime = Double.PositiveInfinity;
            double maxTime = Double.NegativeInfinity;
            List<Point> points = findPointsInShape(shape);
            foreach (Point p in points)
            {
                if ((Double)(p.Time) < minTime)
                    minTime = p.Time;
                if ((Double)(p.Time) > maxTime)
                    maxTime = (Double)(p.Time);
            }
            return (ulong)(maxTime - minTime);
        }

        /// <summary>
        /// Returns the time it took to draw the stroke
        /// </summary>
        /// <param name="stroke">The stroke</param>
        /// <returns>the total time it took to draw the stroke</returns>
        public ulong findStrokeTime(Sketch.Stroke stroke)
        {
            double minTime = Double.PositiveInfinity;
            double maxTime = Double.NegativeInfinity;
            Point[] points = stroke.Points;
            for (int i = 0; i < points.Length; i++)
            {
                if ((Double)(points[i].Time) < minTime)
                    minTime = points[i].Time;
                if ((Double)(points[i].Time) > maxTime)
                    maxTime = (Double)(points[i].Time);
            }
            return (ulong)(maxTime - minTime);
        }

        /// <summary>
        /// Returns the time it took to draw the substroke
        /// </summary>
        /// <param name="sub">The substroke</param>
        /// <returns>the total time it took to draw the substroke</returns>
        public ulong findSubStrokeTime(Sketch.Substroke sub)
        {
            double minTime = Double.PositiveInfinity;
            double maxTime = Double.NegativeInfinity;
            List<Point> points = sub.PointsL;
            foreach (Point p in points)
            {
                if ((Double)(p.Time) < minTime)
                    minTime = p.Time;
                if ((Double)(p.Time) > maxTime)
                    maxTime = (Double)(p.Time);
            }
            return (ulong)(maxTime - minTime);
        }

        /// <summary>
        /// Returns the average pressure of the points in a stroke
        /// </summary>
        /// <param name="stroke">The stroke</param>
        /// <returns>the average pressure</returns>
        public ushort findAvgPressure(Sketch.Substroke stroke)
        {
            List<Point> points = stroke.PointsL;
            ulong totalPressure = 0;
            foreach (Point p in points)
            {
                totalPressure += p.Pressure;
            }
            return (ushort)(totalPressure / ((ulong)points.Count));
        }

        /// <summary>
        /// Returns whether or not the strokes that make up a shape were
        /// drawn consecutively
        /// </summary>
        /// <param name="sketch">The sketch that contains the shape</param>
        /// <param name="shape">The shape to be checked</param>
        /// <returns>Whether or not the shape was drawn consecutively</returns>
        public bool isConsecutive(Sketch.Sketch sketch, Sketch.Shape shape)
        {
            Substroke[] strokes = shape.Substrokes;
            Substroke sub1 = strokes[0];
            bool inShape = false;
            int shapeIndex = 0;
            for (int i = 0; i < sketch.Substrokes.Length; i++)
            {
                Substroke str = sketch.Substrokes[i];
                if (!inShape && str.Equals(sub1))
                    inShape = true;
                if (shapeIndex == strokes.Length)
                    break;
                if (inShape)
                {
                    sub1 = strokes[shapeIndex];
                    if(!sub1.Equals(str))
                    {
                        return false;
                    }
                    shapeIndex++;
                }
            }
            return true;
        }

        /// <summary>
        /// A getter for the header string
        /// </summary>
        public string Header
        {
            get
            {
                return header;
            }
        }

        /// <summary>
        /// Returns the width of the shape
        /// </summary>
        public float getWidth(Shape shape)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            foreach (Point point in findPointsInShape(shape))
            {
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;
            }
            return maxX - minX;
        }

        /// <summary>
        /// Returns the height of the shape
        /// </summary>
        public float getHeight(Shape shape)
        {
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            foreach (Point point in findPointsInShape(shape))
            {
                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
            }
            return maxY - minY;
        }

        /// <summary>
        /// Returns the number of points in the shape
        /// </summary>
        public int getNumPoints(Shape shape)
        {
            int count = 0;
            foreach (Substroke sub in shape.SubstrokesL)
            {
                count += sub.PointsL.Count;
            }
            return count;
        }

        /// <summary>
        /// Returns the number of points in the substroke
        /// </summary>
        public int getNumPoints(Substroke sub)
        {
            return sub.PointsL.Count;
        }

        /// <summary>
        /// Returns the average curvature of the substroke
        /// </summary>
        public double getAvgCurvature(Substroke sub)
        {
            this.arclength = new ArcLength(sub.Points);
            this.slope = new Slope(sub.Points);
            this.curve = new Curvature(sub.Points, arclength.Profile, slope.TanProfile);
            return curve.AverageCurvature;
        }

        /// <summary>
        /// Returns the maximum curvature of the substroke
        /// </summary>
        public double getMaxCurvature(Substroke sub)
        {
            return curve.MaximumCurvature;
        }

        /// <summary>
        /// Returns the minimum curvature of the substroke
        /// </summary>
        public double getMinCurvature(Substroke sub)
        {
            return curve.MinimumCurvature;
        }

        /// <summary>
        /// Returns the number of strokes in the shape
        /// </summary>
        public int numStrokesPerShape(Shape shape)
        {
            return shape.Substrokes.Length;
        }

        /// <summary>
        /// Returns the time of the first point in the substroke
        /// </summary>
        public ulong getStartTime(Substroke sub)
        {
            double time = double.PositiveInfinity;
            foreach(Point p in sub.PointsL)
            {
                if((Double)(p.Time) < time)
                    time = (Double)(p.Time);
            }
            return (ulong)time;
        }

        /// <summary>
        /// Returns the time of the last point in the substroke
        /// </summary>
        public ulong getEndTime(Substroke sub)
        {
            double time = double.NegativeInfinity;
            foreach(Point p in sub.PointsL)
            {
                if((Double)(p.Time) > time)
                    time = (Double)(p.Time);
            }
            return (ulong)time;
        }


    }
}
