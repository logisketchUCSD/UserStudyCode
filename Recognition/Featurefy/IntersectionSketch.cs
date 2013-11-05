using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Serialization;
using Sketch;
using Utilities.Concurrency;

namespace Featurefy
{

    /// <summary>
    /// The Intersection Sketch class computes and stores features regarding intersections
    /// in a sketch.
    /// </summary>
    [Serializable]
    public class IntersectionSketch : ISerializable
    {

        #region Constants

        private const string NUM_LL_INTERSECTIONS = "Number of 'LL' Intersections";
        private const string NUM_LX_INTERSECTIONS = "Number of 'LX' Intersections";
        private const string NUM_XL_INTERSECTIONS = "Number of 'XL' Intersections";
        private const string NUM_XX_INTERSECTIONS = "Number of 'XX' Intersections";

        #endregion

        #region Member Variables

        /// <summary>
        /// Prints debug statements iff debug == true
        /// </summary>
        private bool debug = false;

        /// <summary>
        /// The sketch that this instance tracks
        /// </summary>
        private Sketch.Sketch m_Sketch;

        /// <summary>
        /// All the substrokes in this sketch
        /// </summary>
        private List<Substroke> m_Strokes;

        /// <summary>
        /// Lookup table for a stroke intersections. This is effectively a 2D array, where
        /// each entry contains a Future (a value that can be computed asynchronously). Each 
        /// future computes an intersection pair for the two indexed strokes.
        /// </summary>
        private Dictionary<Substroke, Dictionary<Substroke, Future<IntersectionPair>>> m_Stroke2Intersections;

        /// <summary>
        /// Bounding boxes for each stroke, 
        /// stored so that they don't need to be recalculated
        /// </summary>
        private Dictionary<Substroke, System.Drawing.RectangleF> m_Boxes;

        /// <summary>
        /// Lines connecting points in each stroke, 
        /// stored so that they don't need to be recalculated
        /// </summary>
        private Dictionary<Substroke, List<Line>> m_Lines;

        private Dictionary<Substroke, double> m_ExtensionLengthsExtreme;

        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sketch"></param>
        /// <param name="extensionLengthsExtreme"></param>
        public IntersectionSketch(Sketch.Sketch sketch, Dictionary<Substroke, double> extensionLengthsExtreme)
        {
            m_Sketch = sketch;

            double avgArcLength = GetAvgArcLength(sketch);

            m_Strokes = new List<Substroke>();
            m_Boxes = new Dictionary<Substroke, System.Drawing.RectangleF>();
            m_Lines = new Dictionary<Substroke, List<Line>>();

            foreach (Substroke s in m_Sketch.Substrokes)
            {
                if (!m_Strokes.Contains(s))
                    m_Strokes.Add(s);

                if (!m_Boxes.ContainsKey(s))
                    m_Boxes.Add(s, Compute.BoundingBox(s.Points));

                if (!m_Lines.ContainsKey(s))
                {
                    double d = Math.Min(avgArcLength, (s.SpatialLength + avgArcLength) / 2);
                    d *= Compute.THRESHOLD;
                    List<Line> lines = Compute.getLines(s.PointsL);
                    Line[] endLines = getEndLines(lines, d);
                    lines.Add(endLines[1]);
                    lines.Insert(0, endLines[0]);
                    m_Lines.Add(s, lines);
                }
            }

            m_ExtensionLengthsExtreme = extensionLengthsExtreme;

            m_Stroke2Intersections = new Dictionary<Substroke, Dictionary<Substroke, Future<IntersectionPair>>>();

            for (int i = 0; i < m_Sketch.Substrokes.Length; i++)
                FindIntersections(m_Sketch.SubstrokesL, i);
        }

        private double GetAvgArcLength(Sketch.Sketch sketch)
        {
            if (sketch.SubstrokesL.Count == 0)
                return 0.0;

            double sum = 0.0;
            foreach (Substroke stroke in sketch.SubstrokesL)
                sum += stroke.SpatialLength;

            return sum / sketch.SubstrokesL.Count;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the given stroke to the intersection sketch
        /// </summary>
        /// <param name="stroke"></param>
        public void AddStroke(Substroke stroke)
        {

            if (m_Strokes.Contains(stroke))
                return;

            if (m_Stroke2Intersections.ContainsKey(stroke))
                return;

            if (m_Boxes.ContainsKey(stroke))
                return;

            if (m_Lines.ContainsKey(stroke))
                return;

            List<Line> lines = Compute.getLines(stroke.PointsL);

            Line[] endLines = null;
            endLines = getEndLines(lines, m_ExtensionLengthsExtreme[stroke]);

            lines.Insert(0, endLines[0]);
            lines.Add(endLines[1]);
            m_Lines.Add(stroke, lines);

            m_Boxes.Add(stroke, Compute.BoundingBox(lines));

            m_Stroke2Intersections.Add(stroke, new Dictionary<Substroke, Future<IntersectionPair>>());
            foreach (Substroke s in m_Strokes)
            {
                Future<IntersectionPair> pair = getPair(stroke, s);
                m_Stroke2Intersections[s].Add(stroke, pair);
                m_Stroke2Intersections[stroke].Add(s, pair);
            }

            m_Strokes.Add(stroke);

        }

        /// <summary>
        /// Removes the given stroke from the intersection sketch.
        /// 
        /// Precondition: the given stroke is already in this sketch.
        /// </summary>
        /// <param name="stroke"></param>
        public void RemoveStroke(Substroke stroke)
        {
            if (!m_Strokes.Contains(stroke))
                throw new ArgumentException("The stroke " + stroke + " is not in this IntersectionSketch!");

            m_Strokes.Remove(stroke);
            m_Stroke2Intersections.Remove(stroke);
            m_Boxes.Remove(stroke);
            m_Lines.Remove(stroke);

            foreach (Substroke s in m_Strokes)
            {
                m_Stroke2Intersections[s].Remove(stroke);
            }
        }

        /// <summary>
        /// Updates the intersection sketch
        /// </summary>
        /// <param name="stroke"></param>
        public void UpdateIntersections(Substroke stroke)
        {
            RemoveStroke(stroke);
            AddStroke(stroke);
        }

        /// <summary>
        /// Gets the end lines of the intersection sketch
        /// </summary>
        /// <param name="a"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private Line[] getEndLines(List<Line> a, double d)
        {
            Line[] lines = new Line[2];
            int limit = 10;

            // New Method Stuff
            System.Drawing.PointF lineStart = new System.Drawing.PointF();
            System.Drawing.PointF lineEnd = new System.Drawing.PointF();
            if (a.Count > 0)
            {
                lineStart = a[0].EndPoint1;
                lineEnd = a[a.Count - 1].EndPoint2;
            }
            // End New Method Stuff

            if (a.Count > limit)
            {
                lines[0] = new Line(a[limit].EndPoint1, a[0].EndPoint1, true);
                lines[1] = new Line(a[a.Count - 1 - limit].EndPoint1, a[a.Count - 1].EndPoint2, true);
            }
            else if (a.Count > 0)
            {
                lines[0] = new Line(a[a.Count - 1].EndPoint2, a[0].EndPoint1, true);
                lines[1] = new Line(a[0].EndPoint1, a[a.Count - 1].EndPoint2, true);
                //return lines;
            }
            else
            {
                lines[0] = new Line(new System.Drawing.PointF(0.0f, 0.0f), new System.Drawing.PointF(0.0f, 0.0f), true);
                lines[1] = new Line(new System.Drawing.PointF(0.0f, 0.0f), new System.Drawing.PointF(0.0f, 0.0f), true);
                return lines;
            }

            lines[0].extend(d);
            lines[1].extend(d);

            // New Method Stuff
            Line line1 = new Line(lines[0].EndPoint2, lineStart, true);
            Line line2 = new Line(lines[1].EndPoint2, lineEnd, true);

            return new Line[2] { line1, line2 };
            // End New Method Stuff

            //return lines;
        }

        private void FindIntersections(List<Substroke> strokes, int indexOfStroke)
        {
            if (indexOfStroke >= strokes.Count)
                return;

            Substroke stroke = strokes[indexOfStroke];
            int size = Math.Max(1, indexOfStroke - 1);
            Dictionary<Substroke, Future<IntersectionPair>> pairs = new Dictionary<Substroke, Future<IntersectionPair>>(size);
            for (int i = 0; i < indexOfStroke; i++)
            {
                Substroke s = strokes[i];
                Future<IntersectionPair> pair = getPair(stroke, s);
                pairs.Add(s, pair);
                lock (m_Stroke2Intersections)
                {
                    if (m_Stroke2Intersections.ContainsKey(s))
                        m_Stroke2Intersections[s].Add(stroke, pair);
                }
            }

            if (!m_Strokes.Contains(stroke))
                m_Strokes.Add(stroke);

            if (!m_Stroke2Intersections.ContainsKey(stroke))
                m_Stroke2Intersections.Add(stroke, pairs);
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get the IntersectionPair objects associated with a pair of substrokes
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private Future<IntersectionPair> getPair(Substroke s1, Substroke s2)
        {
            return new Future<IntersectionPair>(delegate()
            {

                if (!m_Boxes.ContainsKey(s1))
                    throw new Exception("IntersectionSketch.m_Boxes does not contain key s1");

                if (!m_Boxes.ContainsKey(s2))
                    throw new Exception("IntersectionSketch.m_Boxes does not contain key s2");

                if (!m_Lines.ContainsKey(s1))
                    throw new Exception("IntersectionSketch.m_Lines does not contain key s1");

                if (!m_Lines.ContainsKey(s2))
                    throw new Exception("IntersectionSketch.m_Lines does not contain key s2");

                return new IntersectionPair(s1, s2, m_Boxes[s1], m_Boxes[s2], m_Lines[s1], m_Lines[s2]);
            });
        }

        /// <summary>
        /// Get the dictionary of intersections for the given stroke
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns></returns>
        public Dictionary<string, int> GetIntersectionCounts(Substroke stroke)
        {
            if (!m_Stroke2Intersections.ContainsKey(stroke))
                return null;

            double avgArcLength = GetAvgArcLength(m_Sketch);
            double dA = Math.Min(avgArcLength, (stroke.SpatialLength + avgArcLength) / 2) * Compute.THRESHOLD;

            Dictionary<Substroke, Future<IntersectionPair>> intersections = m_Stroke2Intersections[stroke];
            Dictionary<string, int> counts = new Dictionary<string, int>(4);
            counts.Add(NUM_LL_INTERSECTIONS, 0);
            counts.Add(NUM_LX_INTERSECTIONS, 0);
            counts.Add(NUM_XL_INTERSECTIONS, 0);
            counts.Add(NUM_XX_INTERSECTIONS, 0);

            foreach (Future<IntersectionPair> futurePair in intersections.Values)
            {
                IntersectionPair pair = futurePair.Value;

                double[] endDistances = new double[4];
                endDistances[0] = Compute.EuclideanDistance(pair.StrokeA.Points[0], pair.StrokeB.Points[0]);
                endDistances[1] = Compute.EuclideanDistance(pair.StrokeA.Points[0], pair.StrokeB.Points[pair.StrokeB.Points.Length - 1]);
                endDistances[2] = Compute.EuclideanDistance(pair.StrokeA.Points[pair.StrokeA.Points.Length - 1], pair.StrokeB.Points[0]);
                endDistances[3] = Compute.EuclideanDistance(pair.StrokeA.Points[pair.StrokeA.Points.Length - 1], pair.StrokeB.Points[pair.StrokeB.Points.Length - 1]);

                Substroke otherStroke = pair.StrokeA;
                if (stroke == otherStroke)
                    otherStroke = pair.StrokeB;

                double dB = Math.Min(avgArcLength, (otherStroke.SpatialLength + avgArcLength) / 2) * Compute.THRESHOLD;

                double d = Math.Min(dA, dB);

                if (pair.IsEmpty)
                {
                    foreach (double dist in endDistances)
                        if (dist < d)
                            counts[NUM_LL_INTERSECTIONS]++;

                    continue;
                }

                List<string> endIntersections = new List<string>();
                foreach (Intersection intersection in pair.Intersections)
                {
                    float a = intersection.GetIntersectionPoint(stroke);
                    float b = intersection.GetOtherStrokesIntersectionPoint(stroke);

                    if (a == -1.0f || b == -1.0f)
                        continue;
                    
                    bool aL = false;
                    bool bL = false;
                    bool aL1 = false;
                    bool aL2 = false;
                    bool bL1 = false;
                    bool bL2 = false;

                    double d_over_A = dA / stroke.SpatialLength;
                    double d_over_B = dB / otherStroke.SpatialLength;

                    if (a < -d_over_A || a > (1.0 + d_over_A) ||
                        b < -d_over_B || b > (1.0 + d_over_B))
                        continue;

                    if ((a <= d_over_A && a >= -d_over_A))
                        aL1 = true;
                    else if ((a >= (1.0 - d_over_A) && (a <= 1.0 + d_over_A)))
                        aL2 = true;
                    if (aL1 || aL2)
                        aL = true;

                    if ((b <= d_over_B && b >= -d_over_B))
                        bL1 = true;
                    else if ((b >= (1.0 - d_over_B) && (b <= 1.0 + d_over_B)))
                        bL2 = true;
                    if (bL1 || bL2)
                        bL = true;

                    string type;

                    if (aL && bL)
                    {
                        type = "LL";
                        counts[NUM_LL_INTERSECTIONS]++;
                        if (aL1 && bL1)
                            endIntersections.Add("a1b1");
                        else if (aL1 && bL2)
                            endIntersections.Add("a1b2");
                        else if (aL2 && bL1)
                            endIntersections.Add("a2b1");
                        else if (aL2 && bL2)
                            endIntersections.Add("a2b2");
                    }
                    else if (aL)
                    {
                        type = "LX";
                        counts[NUM_LX_INTERSECTIONS]++;
                    }
                    else if (bL)
                    {
                        type = "XL";
                        counts[NUM_XL_INTERSECTIONS]++;
                    }
                    else
                    {
                        type = "XX";
                        counts[NUM_XX_INTERSECTIONS]++;
                    }

                    if (debug)
                        Console.WriteLine("{6}: a = {7}, dA = {0}, aInt = {2}, d/a = {4}, b = {8}, dB = {1}, bInt = {3}, d/b = {5}",
                            dA.ToString("#0"), dB.ToString("#0"),
                            a.ToString("#0.00"), b.ToString("#0.00"),
                            d_over_A.ToString("#0.00"), d_over_B.ToString("#0.00"),
                            type, stroke.SpatialLength.ToString("#0"), otherStroke.SpatialLength.ToString("#0"));
                
                }

                if (endDistances[0] < d && !endIntersections.Contains("a1b1"))
                    counts[NUM_LL_INTERSECTIONS]++;
                if (endDistances[1] < d && !endIntersections.Contains("a1b2"))
                    counts[NUM_LL_INTERSECTIONS]++;
                if (endDistances[2] < d && !endIntersections.Contains("a2b1"))
                    counts[NUM_LL_INTERSECTIONS]++;
                if (endDistances[3] < d && !endIntersections.Contains("a2b2"))
                    counts[NUM_LL_INTERSECTIONS]++;

            }

            return counts;
        }

        /// <summary>
        /// Gets the strokes in the intersection sketch
        /// </summary>
        public List<Substroke> Strokes
        {
            get { return m_Strokes; }
        }

        /// <summary>
        /// Gets the dictionary of substrokes and lines
        /// </summary>
        public Dictionary<Substroke, List<Line>> Lines
        {
            get { return m_Lines; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public IntersectionSketch(SerializationInfo info, StreamingContext context)
        {
            m_Boxes = (Dictionary<Substroke, System.Drawing.RectangleF>)info.GetValue("Boxes", typeof(Dictionary<Substroke, System.Drawing.RectangleF>));
            m_Lines = (Dictionary<Substroke, List<Line>>)info.GetValue("Lines", typeof(Dictionary<Substroke, List<Line>>));
            m_Sketch = (Sketch.Sketch)info.GetValue("Sketch", typeof(Sketch.Sketch));
            m_Stroke2Intersections = (Dictionary<Substroke, Dictionary<Substroke, Future<IntersectionPair>>>)info.GetValue("Stroke2Intersections", typeof(Dictionary<Substroke, Dictionary<Substroke, Future<IntersectionPair>>>));
            m_Strokes = (List<Substroke>)info.GetValue("Strokes", typeof(List<Substroke>));
        }

        /// <summary>
        /// Gets the data of the intersection sketch
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Boxes", m_Boxes);
            info.AddValue("Lines", m_Lines);
            info.AddValue("Sketch", m_Sketch);
            info.AddValue("Stroke2Intersections", m_Stroke2Intersections);
            info.AddValue("Strokes", m_Strokes);
        }

        #endregion

    }
}
