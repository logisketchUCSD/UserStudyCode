using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using ConverterXML;
using Utilities;
using Featurefy;
using System.Drawing;

namespace Test_TAMU_Grouping
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int[]> allGaps = new List<int[]>();
            
            /*allGaps.Add(new int[2] { 50, 100 });
            allGaps.Add(new int[2] { 50, 150 });
            allGaps.Add(new int[2] { 50, 200 });
            allGaps.Add(new int[2] { 50, 250 });
            allGaps.Add(new int[2] { 75, 200 });
            allGaps.Add(new int[2] { 75, 250 });
            allGaps.Add(new int[2] { 75, 300 });
            allGaps.Add(new int[2] { 75, 350 });
            allGaps.Add(new int[2] { 100, 200 });
            allGaps.Add(new int[2] { 100, 250 });
            allGaps.Add(new int[2] { 100, 300 });
            allGaps.Add(new int[2] { 100, 350 });
            allGaps.Add(new int[2] { 100, 400 });
            allGaps.Add(new int[2] { 100, 450 });
            allGaps.Add(new int[2] { 100, 500 });
            allGaps.Add(new int[2] { 100, 550 });
            allGaps.Add(new int[2] { 100, 600 });
            allGaps.Add(new int[2] { 125, 200 });
            allGaps.Add(new int[2] { 125, 250 });
            allGaps.Add(new int[2] { 125, 300 });
            allGaps.Add(new int[2] { 125, 350 });
            allGaps.Add(new int[2] { 125, 400 });
            allGaps.Add(new int[2] { 125, 450 });
            allGaps.Add(new int[2] { 125, 500 });
            allGaps.Add(new int[2] { 125, 550 });
            allGaps.Add(new int[2] { 125, 600 });
            allGaps.Add(new int[2] { 150, 200 });
            allGaps.Add(new int[2] { 150, 250 });
            allGaps.Add(new int[2] { 150, 300 });
            allGaps.Add(new int[2] { 150, 350 });
            allGaps.Add(new int[2] { 150, 400 });
            allGaps.Add(new int[2] { 150, 450 });
            allGaps.Add(new int[2] { 150, 500 });
            allGaps.Add(new int[2] { 150, 550 });
            allGaps.Add(new int[2] { 150, 600 });
            */

            // Family Trees
            allGaps.Add(new int[2] { 150, 200 });

            // Digital Circuits
            //allGaps.Add(new int[2] { 125, 250 });

            int largestGap = 0;
            foreach (int[] arr in allGaps)
                largestGap = Math.Max(arr[1], largestGap);


            double spread = 50.0;

            string filename = "C:/Documents and Settings/eric/My Documents/Trunk/Data/Gate Study Data/LabeledPartsSketches/FT Sketches/scaled";//Flipped_FullCircuits";
            string outfile = "C:/Documents and Settings/eric/My Documents/Research/Current/Desktop/simpleTAMUgroupsFT.txt";//DC.txt";
            System.IO.StreamWriter writer = new System.IO.StreamWriter(outfile);
            writer.WriteLine("Sketch,ShortGap,LongGap,TP,FP,FN,TN"); 
            
            string wekaFile = "C:/Documents and Settings/eric/My Documents/Research/Current/Desktop/entropiesFT.arff";//DC.txt";
            System.IO.StreamWriter wekaWriter = new System.IO.StreamWriter(wekaFile);
            wekaWriter.WriteLine("@RELATION FT_Entropies");
            wekaWriter.WriteLine("@ATTRIBUTE 'Entropy' NUMERIC");
            wekaWriter.WriteLine("@ATTRIBUTE class {Text,NonText}");
            wekaWriter.WriteLine();
            wekaWriter.WriteLine("@DATA");

            string[] sketchFiles = System.IO.Directory.GetFiles(filename, "*.labeled.xml");

            foreach (string file in sketchFiles)
            {
                Sketch.Sketch sketch = new ReadXML(file).Sketch;
                //sketch = General.ReOrderParentShapes(sketch);
                //sketch = General.LinkShapes(sketch);

                Dictionary<int, List<Shape>> allShapes = new Dictionary<int, List<Shape>>();
                Dictionary<int, List<StrokePair>> allPairs = new Dictionary<int, List<StrokePair>>();

                Dictionary<Substroke, List<System.Drawing.Point>> resampled = new Dictionary<Substroke, List<System.Drawing.Point>>();
                Dictionary<Substroke, string> entropy = new Dictionary<Substroke, string>();
                Dictionary<Shape, double> entValue = new Dictionary<Shape, double>();
                SortedList<ulong, Substroke> orderedStrokes = new SortedList<ulong, Substroke>();
                foreach (Substroke stroke in sketch.SubstrokesL)
                {
                    orderedStrokes.Add(stroke.XmlAttrs.Time.Value, stroke);
                    resampled.Add(stroke, GetResampledPoints(stroke, spread));
                    entropy.Add(stroke, GetEntropy(resampled[stroke]));
                }

                // Cases: Correct or Incorrect (due to merge)
                // Count of false negatives: cases where adjacent strokes were part of same parent and no join
                //int countFN = 0;
                Dictionary<int, int> countFN = new Dictionary<int, int>();
                // Count of true negatives: cases where adjacent strokes were NOT part of same parent and no join
                //int countTN = 0;
                Dictionary<int, int> countTN = new Dictionary<int, int>();
                // Count of true positives: cases where adjacent strokes were part of same parent and joined
                //int countTP = 0;
                Dictionary<int, int> countTP = new Dictionary<int, int>();
                // Count of false positives: cases where adjacent strokes were NOT part of same parent and joined
                //int countFP = 0;
                Dictionary<int, int> countFP = new Dictionary<int, int>();

                for (int i = 0; i < allGaps.Count; i++)
                {
                    countFN.Add(i, 0);
                    countTN.Add(i, 0);
                    countFP.Add(i, 0);
                    countTP.Add(i, 0);
                    allShapes.Add(i, new List<Shape>());
                    allPairs.Add(i, new List<StrokePair>());
                }

                List<ulong> keys = new List<ulong>(orderedStrokes.Keys);

                for (int i = 0; i < keys.Count - 1; i++)
                {
                    Substroke stroke1 = orderedStrokes[keys[i]];
                    Substroke stroke2 = orderedStrokes[keys[i + 1]];
                    int gap = (int)(stroke2.PointsL[0].Time - stroke1.PointsL[stroke1.PointsL.Count - 1].Time);
                    bool sameShape = false;
                    if (stroke1.ParentShapes.Count > 0 == stroke2.ParentShapes.Count > 0
                        && stroke1.ParentShapes[0] == stroke2.ParentShapes[0])
                        sameShape = true;

                    if (gap <= largestGap)
                    {
                        bool overlap = DoStrokesOverlap(stroke1, stroke2);

                        for (int n = 0; n < allGaps.Count; n++)
                        {
                            bool join = JoinStrokes(overlap, gap, allGaps[n][0], allGaps[n][1]);

                            if (join)
                                allPairs[n].Add(new StrokePair(stroke1, stroke2));

                            if (join && sameShape)
                                countTP[n]++;
                            else if (join && !sameShape)
                                countFP[n]++;
                            else if (!join && sameShape)
                                countFN[n]++;
                            else if (!join && !sameShape)
                                countTN[n]++;
                            else
                                Console.WriteLine("WTF??");
                        }
                    }
                    else if (sameShape)
                        for (int n = 0; n < allGaps.Count; n++)
                            countFN[n]++;
                    else
                        for (int n = 0; n < allGaps.Count; n++)
                            countTN[n]++;   
                }
                
                for (int n = 0; n < allGaps.Count; n++)
                {
                    List<StrokePair> pairs = allPairs[n];
                    List<Shape> shapes = allShapes[n];
                    List<Substroke> pairedStrokes = new List<Substroke>();

                    foreach (Substroke stroke in sketch.SubstrokesL)
                    {
                        bool found = false;
                        foreach (StrokePair pair in pairs)
                        {
                            if (pair.Includes(stroke))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            shapes.Add(MakeShape(stroke));
                        }
                        else
                        {
                            pairedStrokes.Add(stroke);
                        }
                    }
                    
                    foreach (Substroke stroke in pairedStrokes)
                    {
                        bool found = false;
                        foreach (Shape shape in shapes)
                            if (shape.SubstrokesL.Contains(stroke))
                                found = true;

                        if (found)
                            continue;

                        List<Substroke> strokesInShape = new List<Substroke>();
                        strokesInShape.Add(stroke);

                        bool change = false;
                        while (!change)
                        {
                            change = true;
                            foreach (StrokePair pair in pairs)
                            {
                                if (strokesInShape.Contains(pair.Stroke1) || strokesInShape.Contains(pair.Stroke2))
                                {
                                    if (!strokesInShape.Contains(pair.Stroke1))
                                    {
                                        strokesInShape.Add(pair.Stroke1);
                                        change = false;
                                    }

                                    if (!strokesInShape.Contains(pair.Stroke2))
                                    {
                                        strokesInShape.Add(pair.Stroke2);
                                        change = false;
                                    }
                                }
                            }
                        }

                        shapes.Add(MakeShape(strokesInShape));
                    }

                    // Now have all shapes
                    foreach (Shape shape in shapes)
                    {
                        string str = "";
                        foreach (Substroke stroke in shape.SubstrokesL)
                            str += entropy[stroke];

                        double value = GetEntropyValue(str);
                        double diag = Utilities.Compute.BoundingBoxDiagonalLength(Utilities.Compute.BoundingBox(shape.Substrokes));
                        if (diag == 0.0)
                            diag = 0.1;

                        entValue.Add(shape, value / diag);
                    }

                    foreach (KeyValuePair<Shape, double> kvp in entValue)
                    {
                        double nontext = 1.4;
                        double text = 2.8;
                        

                        foreach (Substroke stroke in kvp.Key.SubstrokesL)
                        {
                            string cls = Utilities.General.GetClass(stroke.FirstLabel);
                            if (cls == "Label")
                                cls = "Text";
                            else
                                cls = "NonText";
                            wekaWriter.WriteLine("{0},{1}", kvp.Value, cls);
                        }

                        /*if (kvp.Value < nontext)
                            wekaWriter.WriteLine("{0},{1}", kvp.Value, cls);
                        else if (kvp.Value > text)
                            wekaWriter.WriteLine("{0},{1}", kvp.Value, cls);*/
                    }
                }


                
                

                // Write results to file
                string nameShort = System.IO.Path.GetFileNameWithoutExtension(file);
                nameShort = nameShort.Replace(".labeled", "");
                writer.Write(nameShort + ",");
                for (int n = 0; n < allGaps.Count; n++)
                {
                    int[] gaps = allGaps[n];
                    
                    writer.Write(gaps[0].ToString() + "," + gaps[1].ToString() + ",");
                    writer.Write(countTP[n].ToString() + "," +
                        countFP[n].ToString() + "," +
                        countFN[n].ToString() + "," +
                        countTN[n].ToString() + ",");
                    
                }
                writer.WriteLine();
                writer.Flush();
            }

            writer.Close();
            wekaWriter.Close();
        }

        private static string GetClass(Shape shape)
        {
            string cls = "None";

            foreach (Substroke stroke in shape.SubstrokesL)
            {
                string current = Utilities.General.GetClass(stroke.FirstLabel);
                if (cls == "None")
                    cls = current;
                else if (cls != current)
                    cls = "Mutt";
            }

            return cls;
        }

        private static Shape MakeShape(Substroke stroke)
        {
            List<Substroke> strokes = new List<Substroke>(new Substroke[1] { stroke });
            return MakeShape(strokes);
        }

        private static Shape MakeShape(List<Substroke> strokes)
        {
            return new Shape(strokes, new XmlStructs.XmlShapeAttrs());
        }

        private static double GetEntropyValue(string p)
        {
            double length = (double)p.Length;
            double k = 1000.0;

            Dictionary<char, int> counts = new Dictionary<char, int>();
            foreach (char c in p)
            {
                if (!counts.ContainsKey(c))
                    counts.Add(c, 0);

                counts[c]++;
            }

            double value = 0.0;
            foreach (char c in p)
            {
                double prob = counts[c] / length;
                double logProb = Math.Log10(prob);
                value += -k * prob * logProb;
            }

            return value;
        }

        private static string GetEntropy(List<System.Drawing.Point> points)
        {
            string entropy = "X";

            for (int i = 1; i < points.Count - 1; i++)
            {
                double a1 = Math.Atan2((double)(points[i].Y - points[i - 1].Y), (double)(points[i].X - points[i - 1].X));
                double a2 = Math.Atan2((double)(points[i + 1].Y - points[i].Y), (double)(points[i + 1].X - points[i].X));

                double angle = Math.Abs(a1 - a2);

                string letter = GetLetter(angle);

                entropy += letter;
            }
            entropy += "X";

            return entropy;
        }

        private static string GetLetter(double angle)
        {
            if (angle < 1.0 * Math.PI / 6.0)
                return "F";
            else if (angle < 2.0 * Math.PI / 6.0)
                return "E";
            else if (angle < 3.0 * Math.PI / 6.0)
                return "D";
            else if (angle < 4.0 * Math.PI / 6.0)
                return "C";
            else if (angle < 5.0 * Math.PI / 6.0)
                return "B";
            else
                return "A";
        }

        private static List<System.Drawing.Point> GetResampledPoints(Substroke stroke, double I)
        {
            List<System.Drawing.Point> points = new List<System.Drawing.Point>(stroke.PointsAsSysPoints);
            List<System.Drawing.Point> newPoints = new List<System.Drawing.Point>();
            double D = 0.0;
            double d = 0.0;
            newPoints.Add(points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                d = Utilities.Compute.EuclideanDistance(points[i - 1], points[i]);

                if (D + d >= I)
                {
                    int X = (int)(points[i - 1].X + ((I - D) / d * (points[i].X - points[i - 1].X)));
                    int Y = (int)(points[i - 1].Y + ((I - D) / d * (points[i].Y - points[i - 1].Y)));
                    System.Drawing.Point q = new System.Drawing.Point(X, Y);
                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0.0;
                }
                else
                    D = D + d;
            }

            return newPoints;
        }

        private static bool JoinStrokes(bool overlap, int gap, int timeGap1, int timeGap2)
        {
            if (overlap && gap <= timeGap2)
                return true;
            else if (overlap)
                return false;
            else if (gap <= timeGap1)
                return true;
            else
                return false;
        }

        private static bool DoStrokesOverlap(Substroke stroke1, Substroke stroke2)
        {
            List<Line> l1 = Featurefy.Compute.getLines(stroke1.PointsL);
            List<Line> l2 = Featurefy.Compute.getLines(stroke2.PointsL);
            RectangleF r1 = Utilities.Compute.BoundingBox(stroke1.Points);
            RectangleF r2 = Utilities.Compute.BoundingBox(stroke2.Points);

            List<Intersection> intersections = Featurefy.Compute.Intersect(stroke1, stroke2, l1, l2, r1, r2, 0f);
            return intersections.Count > 0;
        }
    }
}
