using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Set;
using Sketch;

namespace NeighborhoodMap
{
    /// <summary>
    /// Object to abstract a neighborhood graph.
    /// Shouldn't be constructed by hand, but created by Neighborhood.generateNeighborhoodMap.
    /// </summary>
    public class NeighborhoodMap
    {

        private const double twopi = Math.PI * 2;

        private Dictionary<Substroke, NeighborList> map;
        private List<LinkedPoint> points;
        private bool directed;
        private int context;

        #region CONSTRUCTORS
        /// <summary>
        /// Main constructor takes all the tuneable arguments
        /// </summary>
        /// <param name="sketch">the sketch to make a graph for</param>
        /// <param name="radius">distance between two strokes still considered a connection</param>
        /// <param name="directed">if the graph should be directed</param>
        /// <param name="context">how many points to use for context</param>
        public NeighborhoodMap(Sketch.Sketch sketch, double radius, bool directed, int context)
        {
            this.directed = directed;
            this.context = context;
            generateNeighborhood(sketch, radius, directed, context);
        }

        /* some extra constructors just in case */
        public NeighborhoodMap(Sketch.Sketch sketch, double radius)
            : this(sketch, radius, false, 0) { }
        public NeighborhoodMap(Sketch.Sketch sketch, double radius, bool directed)
            : this(sketch, radius, directed, 0) { }
        public NeighborhoodMap(Sketch.Sketch sketch, double radius, int context)
            : this(sketch, radius, false, context) { }
        #endregion

        #region GRAPH GENERATION
        /// <summary>
        /// Generate the neighborhood graph.
        /// </summary>
        /// <param name="sketch">the sketch to generate the graph for</param>
        /// <param name="radius">the allowable distance one stroke to another</param>
        /// <param name="directed">if the graph should be directed</param>
        /// <param name="context">how many points of context should be used for direction</param>
        /// <returns>a populated NeighborhoodMap</returns>
        private void generateNeighborhood
            (Sketch.Sketch sketch, double radius, bool directed, int context)
        {
            long maxX = 0;

            // points indexed by Y*maxX+X
            Dictionary<long, List<Substroke>> intPoints = new Dictionary<long, List<Substroke>>();
            map = new Dictionary<Substroke, NeighborList>();
            points = new List<LinkedPoint>();

            /*
             * First we find the maximum X value, which we use to index into
             * the points dictionary.
             */
            foreach (Substroke s in sketch.Substrokes)
            {
                for (int i = 0; i < s.PointsL.Count; i++)
                {
                    Sketch.Point p = s.PointsL[i];
                    if (p.X > maxX) maxX = (long)p.X;
                    points.Add(new LinkedPoint(s, i));
                }
            }
            maxX += 1;

            /*
             * Store the location of each point currently in the sketch
             */
            foreach (Substroke s in sketch.Substrokes)
            {
                foreach (Sketch.Point p in s.PointsL)
                {
                    long key = (long)p.Y * maxX;
                    key += (long)p.X;
                    if (!intPoints.ContainsKey(key)) intPoints.Add(key, new List<Substroke>());
                    intPoints[key].Add(s);
                }
            }

            /*
             * For each substroke, check if any of its points are close to 
             * any other points from the sketch.
             */
            foreach (Substroke s in sketch.Substrokes)
            {
                map.Add(s, new NeighborList());

                for (int index = 0; index < s.PointsL.Count; index++)
                {
                    if (index > 10 && index < s.PointsL.Count - 10) continue;
                    if ((index == 0 || index == s.PointsL.Count - 1) && directed) continue;
                    Sketch.Point p = s.PointsL[index];
                    if (directed && curvature(s, index, 1) - curvature(s, index, 20) > 1) continue;

                    for (int i = 0; i < (int)radius; i++)
                    {
                        for (int j = 0; j < (int)radius && Math.Sqrt(i * i + j * j) < radius; j++)
                        {
                            if (i == 0 & j == 0) continue;
                            /*
                             * if p is the origin, we need to check the point translated by |i| 
                             * in the X-direction and |j| in the Y-direction in each of the 4 
                             * quadrants.
                             */
                            long key1 = (long)(p.Y + j) * maxX + (long)(p.X + i);
                            long key2 = (long)(p.Y + j) * maxX + (long)(p.X - i);
                            long key3 = (long)(p.Y - j) * maxX + (long)(p.X + i);
                            long key4 = (long)(p.Y - j) * maxX + (long)(p.X - i);

                            if (intPoints.ContainsKey(key1))
                            {
                                if (!directed || checkDirection(s, index, context, i, j))
                                {
                                    foreach (Substroke test in intPoints[key1])
                                    {
                                        if (test != s)
                                        {
                                            map[s].addPoint(test,
                                                new System.Drawing.Point((int)(p.X + i), (int)(p.Y + j)),
                                                new System.Drawing.Point((int)(p.X), (int)(p.Y)));
                                        }
                                    }
                                }
                            }
                            if (intPoints.ContainsKey(key2))
                            {
                                if (!directed || checkDirection(s, index, context, -i, j))
                                {
                                    foreach (Substroke test in intPoints[key2])
                                    {
                                        if (test != s)
                                        {
                                            map[s].addPoint(test,
                                                new System.Drawing.Point((int)(p.X - i), (int)(p.Y + j)),
                                                new System.Drawing.Point((int)(p.X), (int)(p.Y)));
                                        }
                                    }
                                }
                            }
                            if (intPoints.ContainsKey(key3))
                            {
                                if (!directed || checkDirection(s, index, context, i, -j))
                                {
                                    foreach (Substroke test in intPoints[key3])
                                    {
                                        if (test != s)
                                        {
                                            map[s].addPoint(test,
                                                new System.Drawing.Point((int)(p.X + i), (int)(p.Y - j)),
                                                new System.Drawing.Point((int)(p.X), (int)(p.Y)));
                                        }
                                    }
                                }
                            }
                            if (intPoints.ContainsKey(key4))
                            {
                                if (!directed || checkDirection(s, index, context, -i, -j))
                                {
                                    foreach (Substroke test in intPoints[key4])
                                    {
                                        if (test != s)
                                        {
                                            map[s].addPoint(test,
                                                new System.Drawing.Point((int)(p.X - i), (int)(p.Y - j)),
                                                new System.Drawing.Point((int)(p.X), (int)(p.Y)));
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }

            /*
             * Once we have constructed the Neighbor objects above,
             * the avg Point has the sum of all the points' X- and Y-values.
             * Now that we know the total, we can divide to find the true average.
             */
            foreach (Substroke s in map.Keys)
            {
                for (int j = 0; j < map[s].Count; j++)
                {
                    map[s][j].src.X /= map[s][j].num;
                    map[s][j].src.Y /= map[s][j].num;

                    map[s][j].dest.X /= map[s][j].num;
                    map[s][j].dest.Y /= map[s][j].num;
                }
            }
        }
        #endregion

        #region UTILITY FUNCTIONS
        /// <summary>
        /// Checks if (p.x+i,p.y+j) is in the same direction as the stroke is headed.
        /// </summary>
        /// <param name="s">the stroke</param>
        /// <param name="ind">the index of the point in the stroke</param>
        /// <param name="context">the number of points to use as context</param>
        /// <param name="i">x translation</param>
        /// <param name="j">y translation</param>
        /// <returns></returns>
        private bool checkDirection(Substroke s, int ind, int context, int i, int j)
        {

            double strokeDir = direction(s, ind, context);
            Sketch.Point p = s.PointsL[ind];

            double connectionDir = direction(p.Y, p.Y + j, p.X, p.X + i);

            double diff = Math.Abs(strokeDir - connectionDir);

            return (diff < Math.PI / 6d /*&& (curvature(s,ind,1)-curvature(s, ind, 20)) < 1*/);

        }

        /// <summary>
        /// Find the direction of a substroke at a certain point.
        /// context = 0 just looks at the preceeding point to index.
        /// </summary>
        /// <param name="s">the substroke</param>
        /// <param name="index">the index of the point, must be greater than 0</param>
        /// <param name="context">the number of points to look away and take the average of</param>
        /// <returns>the direction of s at the given point</returns>
        internal static double direction(Substroke s, int index)
        {
            List<Sketch.Point> p = s.PointsL;
            return direction(p[index], p[index+1]);
        }

        internal static double direction(Sketch.Point p1, Sketch.Point p2)
        {
            return direction(p1.Y, p2.Y, p1.X, p2.X);
        }
        internal static double direction(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return direction(p1.Y, p2.Y, p1.X, p2.X);
        }
        internal static double direction(Sketch.Point p1, System.Drawing.Point p2)
        {
            return direction(p1.Y, p2.Y, p1.X, p2.X);
        }

        /// <summary>
        /// Wrapper around atan2 in case i want to change my units
        /// currently: -PI to PI
        /// </summary>
        /// <param name="y1">Y value of first point</param>
        /// <param name="y2">Y value of second point</param>
        /// <param name="x1">X value of first point</param>
        /// <param name="x2">X value of second point</param>
        /// <returns></returns>
        internal static double direction(double y1, double y2, double x1, double x2)
        {
            double atan = Math.Atan2(y2 - y1, x2 - x1);
            return atan;
        }

        /// <summary>
        /// Find the curvature of the stroke at a certain point
        /// </summary>
        /// <param name="s">the stroke</param>
        /// <param name="n">the index of the point</param>
        /// <param name="k">number of points to take for context, >= 1</param>
        /// <returns></returns>
        internal static double curvature(Substroke s, int n, int k)
        {
            if (k < 1)
                k = 1;

            double sum = 0d, plen = 0d;
            List<double> ds = new List<double>();

            for (int i = n - k; i <= n + k; i++)
            {
                ds.Add(direction(s, i, 0));
            }

            for (int i = 0; i < ds.Count-1; i++)
            {
                sum += phi(ds[i + 1] - ds[i]);
            }

            return sum / (double)(ds.Count - 1d);

        }

        /// <summary>
        /// Shifts a given angle into the range -PI to PI
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static double phi(double angle)
        {
            while (angle > Math.PI) angle -= twopi;
            while (angle < -Math.PI) angle += twopi;
            return angle;
        }

        /// <summary>
        /// Given two angles, find the minimum angle separating them.
        /// </summary>
        /// <param name="a">one angle</param>
        /// <param name="b">another angle</param>
        /// <returns>the difference between the angles in the smallest way possible</returns>
        private double minDist(double a, double b)
        {
            double first = Math.Abs(a - b);
            double second = twopi - _;
            return Math.Min(first,second);
        }
        #endregion

        #region GRAPH OPERATIONS
        public List<List<Substroke>> connectedComponents()
        {
            List<List<Substroke>> res = new List<List<Substroke>>();
            Substroke[] temp = new Substroke[map.Keys.Count];
            map.Keys.CopyTo(temp, 0);
            List<Substroke> keys = new List<Substroke>(temp);

            while (keys.Count > 0)
            {
                List<Substroke> cur = new List<Substroke>();
                Queue<Substroke> bfs = new Queue<Substroke>();
                bfs.Enqueue(keys[0]);
                while (bfs.Count > 0)
                {
                    Substroke s = bfs.Dequeue();
                    cur.Add(s);
                    keys.Remove(s);
                    foreach (Neighbor n in map[s])
                    {
                        if (!cur.Contains(n.neighbor))
                        {
                            bfs.Enqueue(n.neighbor);
                            cur.Add(n.neighbor);

                        }
                    }
                }
                res.Add(cur);
            }
            return res;
        }

        public bool Edge(Substroke a, Substroke b)
        {
            return map[a].Contains(b);
        }
        #endregion

        #region GETTERS
        public ICollection<Substroke> Substrokes
        {
            get { return map.Keys; }
        }

        public NeighborList this[Substroke s]
        {
            get { return (map.ContainsKey(s)) ? map[s] : null; }
            set
            {
                if (map.ContainsKey(s)) map[s] = value;
                else map.Add(s, value);
            }
        }

        public bool IsDirected
        {
            get { return directed; }
        }

        public int Context
        {
            get { return context; }
        }

        public List<LinkedPoint> Points
        {
            get { return points; }
        }

        public List<LinkedPoint> PointsXSorted
        {
            get
            {
                points.Sort(new LinkedPointSorter(true));
                return new List<LinkedPoint>(points);
            }
        }

        public List<LinkedPoint> PointsYSorted
        {
            get
            {
                points.Sort(new LinkedPointSorter(false));
                return new List<LinkedPoint>(points);
            }
        }

        public bool ContainsKey(Substroke s)
        {
            return map.ContainsKey(s);
        }
        #endregion
    }
}
