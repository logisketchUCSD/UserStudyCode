using System;

namespace Metrics
{
    /// <summary>
    /// Computes the distance between points
    /// </summary>
    public class PointDistance : Distance<Sketch.Point>
    {
        /// <summary>
        /// Euclidian distance
        /// </summary>
        public const int EUCLIDIAN = 0;

        /// <summary>
        /// Block distance
        /// </summary>
        public const int BLOCK = 1;

        /// <summary>
        /// Time distance
        /// </summary>
        public const int TIME = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public PointDistance(Sketch.Point a, Sketch.Point b) : base(a, b) { }

        /// <summary>
        /// Computes the euler distance between two points
        /// </summary>
        /// <returns></returns>
        public override double distance()
        {
            return distance(EUCLIDIAN);
        }

        /// <summary>
        /// Computes the distance between two points
        /// </summary>
        /// <param name="method">Method to compute distance</param>
        /// <returns>The distance</returns>
        public override double distance(int method)
        {
            switch (method)
            {
                case EUCLIDIAN:
                    return euler();
                case BLOCK:
                    return block();
                case TIME:
                    return time();
                default:
                    return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// sqrt(x^2 + y^2)
        /// </summary>
        /// <returns></returns>
        private double euler()
        {
            return Math.Sqrt((m_a.X - m_b.X) * (m_a.X - m_b.X) + (m_a.Y - m_b.Y) * (m_a.Y - m_b.Y));
        }

        /// <summary>
        /// abs(x) + abs(y)
        /// </summary>
        /// <returns></returns>
        private double block()
        {
            return Math.Abs(m_a.X - m_b.X) + Math.Abs(m_a.Y - m_b.Y);
        }

        /// <summary>
        /// Time difference
        /// </summary>
        /// <returns></returns>
        private double time()
        {
            return Math.Abs((double)(m_a.Time - m_b.Time));
        }
    }
}
