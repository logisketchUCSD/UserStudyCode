using System;

namespace Metrics
{
    /// <summary>
    /// Computes the distance between Substrokes
    /// </summary>
    public class SubstrokeDistance : Distance<Sketch.Substroke>
    {
        /// <summary>
        /// Minimum distance
        /// </summary>
        public const int MIN = 0;

        /// <summary>
        /// Maximum distance
        /// </summary>
        public const int MAX = 1;

        /// <summary>
        /// Time distance
        /// </summary>
        public const int TIME = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">The substroke</param>
        /// <param name="b">The substroke</param>
        public SubstrokeDistance(Sketch.Substroke a, Sketch.Substroke b) : base(a, b) { }

        /// <summary>
        /// Compute the min distance between two substrokes
        /// </summary>
        /// <returns>The min distance</returns>
        public override double distance()
        {
            return distance(MIN);
        }

        /// <summary>
        /// Compute the distance between two substrokes
        /// </summary>
        /// <param name="method">The method of distance</param>
        /// <returns>Distance metric</returns>
        public override double distance(int method)
        {
            return distance(method, PointDistance.EUCLIDIAN);
        }

        /// <summary>
        /// Computes distance between two substrokes
        /// </summary>
        /// <param name="substrokeMethod">Substroke distance metric to use</param>
        /// <param name="pointMethod">Point distance metric to use</param>
        /// <returns>distance</returns>
        public double distance(int substrokeMethod, int pointMethod)
        {
            switch (substrokeMethod)
            {
                case MIN:
                    return min(pointMethod);
                case MAX:
                    return max(pointMethod);
                case TIME:
                    return time();
                default:
                    return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Computes the min distance between two substrokes
        /// </summary>
        /// <returns>min distance</returns>
        private double min(int pointMethod)
        {
            double min = double.PositiveInfinity;
            double dist;
            
            Sketch.Point[] pointsA = m_a.Points, pointsB = m_b.Points;
            Sketch.Point pa, pb;

            int i, j, lenA = pointsA.Length, lenB = pointsB.Length;
            for (i = 0; i < lenA; ++i)
            {
                pa = pointsA[i];
                for (j = 0; j < lenB; ++j)
                {
                    pb = pointsB[j];
                    dist = (new PointDistance(pa, pb)).distance(pointMethod);
                    if (dist < min)
                        min = dist;
                }
            }
            return min;
        }

        /// <summary>
        /// Computes the max distance between two substrokes
        /// </summary>
        /// <returns>max distance</returns>
        private double max(int pointMethod)
        {
            double max = double.NegativeInfinity;
            double dist;

            Sketch.Point[] pointsA = m_a.Points, pointsB = m_b.Points;
            Sketch.Point pa, pb;

            int i, j, lenA = pointsA.Length, lenB = pointsB.Length;
            for (i = 0; i < lenA; ++i)
            {
                pa = pointsA[i];
                for (j = 0; j < lenB; ++j)
                {
                    pb = pointsB[j];
                    dist = (new PointDistance(pa, pb)).distance(pointMethod);
                    if (dist > max)
                        max = dist;
                }
            }
            return max;
        }

        /// <summary>
        /// Computes time difference
        /// </summary>
        /// <returns></returns>
        private double time()
        {
            return Math.Abs((double)(m_a.XmlAttrs.Time.Value - m_b.XmlAttrs.Time.Value));
        }
    }
}
