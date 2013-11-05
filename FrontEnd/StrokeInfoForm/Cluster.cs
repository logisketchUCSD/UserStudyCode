using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using Featurefy;
using System.IO;
using System.Diagnostics;

namespace Cluster
{
    /// <summary>
    /// Class to package together a number of strokes which belong together
    /// </summary>
    [DebuggerDisplay("StartTime = {startTime}, EndTime = {endTime}, #Strokes = {strokes.Count}")]
    public class Cluster
    {
        private List<Substroke> strokes;
        private double score;
        private Guid id;
        private int classificationNum;
        private ulong startTime;
        private ulong endTime;
        private bool justMerged;
        //private List<Substroke> nearestStrokes;
        //private List<Substroke> nearestWires;
        //private List<Substroke> nearestNonWires;

        #region Constructors
        /// <summary>
        /// Create an empty cluster to put strokes into
        /// </summary>
        public Cluster()
        {
            initializeComponent();
            this.strokes = new List<Substroke>();
            this.startTime = 0;
            this.endTime = 0;
        }

        /// <summary>
        /// Creates a cluster from a single substroke
        /// </summary>
        /// <param name="stroke">Substroke to begin cluster with</param>
        public Cluster(Substroke stroke)
        {
            initializeComponent();
            this.strokes = new List<Substroke>();
            this.strokes.Add(stroke);
            this.startTime = stroke.Points[0].Time;
            this.endTime = stroke.Points[stroke.Points.Length - 1].Time;
        }

        /// <summary>
        /// Creates a cluster of strokes from a list
        /// </summary>
        /// <param name="strokes">List of strokes to add to cluster</param>
        public Cluster(List<Substroke> strokes)
        {
            initializeComponent();
            this.strokes = strokes;
            strokesStartEndTimes(strokes, out this.startTime, out this.endTime);
        }

        /// <summary>
        /// Creates a cluster of strokes from an array
        /// </summary>
        /// <param name="strokes">Array of strokes to add to cluster</param>
        public Cluster(Substroke[] strokes)
        {
            initializeComponent();
            this.strokes = new List<Substroke>(strokes.Length);
            foreach (Substroke stroke in strokes)
                this.strokes.Add(stroke);
            strokesStartEndTimes(new List<Substroke>(strokes), out this.startTime, out this.endTime);
        }


        private void initializeComponent()
        {
            this.id = Guid.NewGuid();
            this.score = 0.0;
            this.classificationNum = -1;
            this.justMerged = false;
            //this.nearestStrokes = new List<Substroke>();
            //this.nearestWires = new List<Substroke>();
            //this.nearestNonWires = new List<Substroke>();
        }
        #endregion


        #region Getters & Setters
        /// <summary>
        /// Gets the number of strokes in the cluster
        /// </summary>
        public int Count
        {
            get { return this.strokes.Count; }
        }

        /// <summary>
        /// Gets the Guid of the cluster
        /// </summary>
        public Guid Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the list of strokes in the cluster
        /// </summary>
        public List<Substroke> Strokes
        {
            get { return this.strokes; }
        }

        /// <summary>
        /// Range of 0.0 to 1.0: how likely these cluster belongs
        /// </summary>
        public double Score
        {
            get { return this.score; }
            set { this.score = value; }
        }

        /// <summary>
        /// Get the bounding box around the entire cluster of strokes
        /// </summary>
        public System.Drawing.Rectangle BoundingBox
        {
            get
            {
                int minX = 1000000;
                int minY = 1000000;
                int maxX = 0;
                int maxY = 0;

                foreach (Substroke stroke in this.strokes)
                {
                    if (stroke.XmlAttrs.X < minX) 
                        minX = (int)stroke.XmlAttrs.X;
                    if (stroke.XmlAttrs.X + stroke.XmlAttrs.Width > maxX)
                        maxX = (int)stroke.XmlAttrs.X + (int)stroke.XmlAttrs.Width;
                    if (stroke.XmlAttrs.Y < minY)
                        minY = (int)stroke.XmlAttrs.Y;
                    if (stroke.XmlAttrs.Y + stroke.XmlAttrs.Height > maxY)
                        maxY = (int)stroke.XmlAttrs.Y + (int)stroke.XmlAttrs.Height;
                }
                return new System.Drawing.Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
        }

        /*
        /// <summary>
        /// Gets the list of nearest strokes to the current cluster
        /// </summary>
        public List<Substroke> NearestStrokes
        {
            get { return this.nearestStrokes; }
        }

        /// <summary>
        /// Gets the list of nearest strokes to the current cluster 
        /// which are classified as 'Wires'
        /// </summary>
        public List<Substroke> NearestWires
        {
            get { return this.nearestWires; }
        }

        /// <summary>
        /// Gets the list of nearest strokes to the current cluster
        /// which are classified as 'Non-Wires'
        /// </summary>
        public List<Substroke> NearestNonWires
        {
            get { return this.nearestNonWires; }
        }
         * */

        /// <summary>
        /// Get the classification number of this cluster (for color-coding)
        /// </summary>
        public int ClassificationNum
        {
            get { return this.classificationNum; }
            set { this.classificationNum = value; }
        }

        /// <summary>
        /// Get the starting time of the first stroke in this cluster
        /// </summary>
        public ulong StartTime
        {
            get 
            {
                if (this.startTime == 0)
                {
                    this.startTime = strokes[0].Points[0].Time;
                    foreach (Substroke s in this.strokes)
                        this.startTime = Math.Min(this.startTime, s.Points[0].Time);
                }
                return this.startTime; 
            }
        }

        /// <summary>
        /// Get the ending time of the last stroke in this cluster
        /// </summary>
        public ulong EndTime
        {
            get 
            {
                if (this.endTime == 0)
                {
                    this.endTime = strokes[0].Points[strokes[0].Points.Length - 1].Time;
                    foreach (Substroke s in this.strokes)
                        this.endTime = Math.Max(this.endTime, s.Points[s.Points.Length - 1].Time);
                }
                return this.endTime; 
            }
        }

        /// <summary>
        /// Determine whether a cluster has just been merged
        /// Used mainly for display purposes (thicken strokes)
        /// </summary>
        public bool JustMerged
        {
            get { return this.justMerged; }
        }
        #endregion


        #region Functions
        /// <summary>
        /// Indicates whether two clusters share a stroke in common
        /// </summary>
        /// <param name="C">Cluster to compare 'this' to</param>
        /// <returns>bool indicator of strokes in common</returns>
        public bool strokesOverlap(Cluster C)
        {
            foreach (Substroke stroke in this.strokes)
            {
                foreach (Substroke cStroke in C.strokes)
                {
                    if (stroke.XmlAttrs.Id.Equals(cStroke.XmlAttrs.Id.Value))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Indicates whether two clusters have overlapping areas
        /// </summary>
        /// <param name="C">Cluster to compare 'this' to</param>
        /// <returns>bool indicator of area in common</returns>
        public bool boundingBoxOverlap(Cluster C)
        {
            if (overlap(this.BoundingBox, C.BoundingBox, 0))
                return true;
            return false;
        }

        private bool overlap(System.Drawing.Rectangle a, System.Drawing.Rectangle b, int fudgeFactor)
        {
            if ((a.X >= (b.X - fudgeFactor) && a.X <= (b.X + b.Width + fudgeFactor))
                || ((a.X + a.Width) >= (b.X - fudgeFactor) && (a.X + a.Width) <= (b.X + b.Width + fudgeFactor))
                || ((b.X >= (a.X - fudgeFactor)) && (b.X <= (a.X + a.Width + fudgeFactor))))  // overlap in x
            {
                if ((a.Y >= (b.Y - fudgeFactor) && a.Y <= (b.Y + b.Height + fudgeFactor))
                    || ((a.Y + a.Height) >= (b.Y - fudgeFactor) && (a.Y + a.Height) <= (b.Y + b.Height + fudgeFactor))
                    || (b.Y >= (a.Y - fudgeFactor) && b.Y <= (a.Y + a.Height + fudgeFactor)))  // overlap in y
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determine whether a Cluster contains a certain Substroke
        /// </summary>
        /// <param name="stroke">Substroke to check</param>
        /// <returns>Bool whether cluster contains substroke</returns>
        public bool contains(Substroke stroke)
        {
            foreach (Substroke s in this.strokes)
            {
                if (s.XmlAttrs.Id.Equals(stroke.XmlAttrs.Id.Value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determine whether a Cluster contains a certain Substroke by Guid
        /// </summary>
        /// <param name="ident">Substroke Guid to check</param>
        /// <returns>Bool whether cluster contains Substroke with ID</returns>
        public bool contains(Guid ident)
        {
            foreach (Substroke stroke in this.strokes)
            {
                if (stroke.XmlAttrs.Id.Equals(ident))
                    return true;
            }
            return false;
        }

        private void strokesStartEndTimes(List<Substroke> strokes, out ulong startTime, out ulong endTime)
        {
            startTime = strokes[0].Points[0].Time * 2;
            endTime = 0;
            foreach (Substroke s in strokes)
            {
                startTime = Math.Min(startTime, s.Points[0].Time);
                endTime = Math.Max(endTime, s.Points[s.Points.Length - 1].Time);
            }
        }

        /*
        /// <summary>
        /// Determine whether a cluster's nearest strokes contains a given stroke by Guid
        /// </summary>
        /// <param name="ident">Guid of stroke to check</param>
        /// <returns>Bool whether nearest strokes contains a given stroke</returns>
        public bool nearestContains(Guid ident)
        {
            foreach (Substroke stroke in this.nearestStrokes)
            {
                if (stroke.XmlAttrs.Id.Equals(ident))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Add a stroke to the nearest strokes list by type
        /// </summary>
        /// <param name="stroke">Stroke to add to nearest strokes list</param>
        /// <param name="classifiedType">Type of stroke being added</param>
        public void addNearStroke(Substroke stroke, string classifiedType)
        {
            this.nearestStrokes.Add(stroke);
            if (classifiedType == "Wire")
                this.nearestWires.Add(stroke);
            else if (classifiedType == "Non-Wire")
                this.nearestNonWires.Add(stroke);
        }
         * */

        /// <summary>
        /// Add a stroke to the cluster
        /// </summary>
        /// <param name="stroke">Stroke to add</param>
        public void addStroke(Substroke stroke)
        {
            this.strokes.Add(stroke);
        }

        /// <summary>
        /// Add a list of strokes to the cluster
        /// </summary>
        /// <param name="strokes">List of strokes to add</param>
        public void addStroke(List<Substroke> strokes)
        {
            foreach (Substroke s in strokes)
                this.strokes.Add(s);
        }

        /// <summary>
        /// Merge two clusters into one
        /// </summary>
        /// <param name="c1">First cluster to merge</param>
        /// <param name="c2">Second cluster to merge</param>
        /// <returns>New cluster</returns>
        public static Cluster merge(Cluster c1, Cluster c2)
        {
            Cluster nCluster = new Cluster();

            nCluster.addStroke(c1.Strokes);

            if (c2.Strokes.Count > c1.Strokes.Count)
                nCluster.classificationNum = c2.classificationNum;
            else
                nCluster.classificationNum = c1.classificationNum;

            nCluster.addStroke(c2.Strokes);
            nCluster.justMerged = true;

            return nCluster;
        }
        #endregion
    }

    [DebuggerDisplay("TimeD = {timeDelta}, SpatialD = {spatialDelta}, #Clusters = {clusters.Count}")]
    public class ClusterSet
    {
        private List<Cluster> clusters;
        private double spatialDelta;
        private double timeDelta;
        private Guid[] closestClusters;
        private Guid[] temporalClosestClusters;
        private Guid id;
        private Dictionary<Guid, int> clusterClassifications;

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClusterSet()
        {
            initializeClass();
            this.clusters = new List<Cluster>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cluster">First cluster to place in the ClusterSet</param>
        public ClusterSet(Cluster cluster)
        {
            initializeClass();
            this.clusters = new List<Cluster>();
            this.clusters.Add(cluster);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clusters">Clusters to put into the ClusterSet</param>
        public ClusterSet(List<Cluster> clusters)
        {
            initializeClass();
            this.clusters = clusters;
        }

        /// <summary>
        /// Create a clusterset from classifications
        /// </summary>
        /// <param name="sketch">Sketch with substrokes to populate cluster with</param>
        /// <param name="classifications">Classifications from NN</param>
        /// <param name="type">Type of classification to create clusters from</param>
        public ClusterSet(Sketch.Sketch sketch, Dictionary<Guid, string> classifications, string type)
        {
            initializeClass();
            this.clusters = new List<Cluster>();

            foreach (Substroke s in sketch.Substrokes)
            {
                if (classifications[s.Id] == type)
                    this.clusters.Add(new Cluster(s));
            }
        }

        public void initializeClass()
        {
            this.id = Guid.NewGuid();
            this.spatialDelta = 1000000.0;
            this.timeDelta = 1000000.0;
            this.closestClusters = new Guid[2];
            this.temporalClosestClusters = new Guid[2];
            this.clusterClassifications = new Dictionary<Guid, int>();
        }
        #endregion


        #region Getters & Setters
        /// <summary>
        /// Get the smallest Merge Distance in this ClusterSet
        /// </summary>
        public double Delta
        {
            get { return this.spatialDelta; }
        }

        /// <summary>
        /// Get the Guid of the ClusterSet
        /// </summary>
        public Guid Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Get the List of clusters
        /// </summary>
        public List<Cluster> Clusters
        {
            get { return this.clusters; }
        }

        /// <summary>
        /// Get a specific cluster based on Guid
        /// </summary>
        /// <param name="id">Guid of cluster</param>
        /// <param name="foundCluster">Tells whether a cluster was successfully found</param>
        /// <returns>Cluster associated with ID</returns>
        public Cluster getCluster(Guid id, out bool foundCluster)
        {
            foreach (Cluster c in this.clusters)
            {
                if (c.Id == id)
                {
                    foundCluster = true;
                    return c;
                }
            }
            foundCluster = false;
            return new Cluster();
        }

        private Cluster getCluster(Guid strokeID)
        {
            foreach (Cluster c in this.clusters)
            {
                foreach (Substroke stroke in c.Strokes)
                {
                    if (stroke.Id == strokeID)
                        return c;
                }
            }

            return new Cluster();
        }

        /// <summary>
        /// Get the Guids of the closest 2 clusters
        /// </summary>
        public Guid[] ClosestClusters
        {
            get { return this.closestClusters; }
        }

        /// <summary>
        /// Get the cluster classifications for this clusterset
        /// </summary>
        public Dictionary<Guid, int> getClusterClassifications(Sketch.Sketch sketch)
        {
            if (this.clusterClassifications.Count == 0)
                applyClassifications(sketch);

            return this.clusterClassifications; 
        }

        /// <summary>
        /// Get the shortest time between two clusters in this clusterSet
        /// </summary>
        public double TimeDelta
        {
            get { return this.timeDelta; }
        }

        /// <summary>
        /// Get the Guids of the closest clusters temporally
        /// </summary>
        public Guid[] TemporalClosestClusters
        {
            get { return this.temporalClosestClusters; }
        }
        #endregion


        public void applyClassifications(Sketch.Sketch sketch)
        {
            int classNum = 0;
            foreach (Cluster c in this.clusters)
            {
                if (c.ClassificationNum == -1)
                {
                    classNum++;
                    c.ClassificationNum = classNum;
                }
            }

            foreach (Substroke stroke in sketch.Substrokes)
            {
                bool found = false;
                foreach (Cluster c in this.clusters)
                {
                    foreach (Substroke s in c.Strokes)
                    {
                        if (s.Id == stroke.Id)
                            found = true;
                    }
                }

                if (found)
                {
                    if (!clusterClassifications.ContainsKey(stroke.Id))
                        clusterClassifications.Add(stroke.Id, getCluster(stroke.Id).ClassificationNum);
                }
                else
                {
                    if (!clusterClassifications.ContainsKey(stroke.Id))
                        clusterClassifications.Add(stroke.Id, 0);
                }
            }
        }

        private int getClusterClassNum(Guid SubstrokeID, Guid ClusterID)
        {
            foreach (Cluster c in this.clusters)
            {
                if (c.Id == ClusterID)
                {
                    foreach (Substroke stroke in c.Strokes)
                    {
                        if (stroke.Id == SubstrokeID)
                            return c.ClassificationNum;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Add another cluster to the ClusterSet
        /// </summary>
        /// <param name="cluster">Cluster to add</param>
        public void addCluster(Cluster cluster)
        {
            this.clusters.Add(cluster);
        }

        /// <summary>
        /// Search through all clusters for a minimum distance between any 2 clusters
        /// </summary>
        public void findMinDelta()
        {
            Cluster c1, c2;
            for (int i = 0; i < clusters.Count; i++)
            {
                c1 = clusters[i];
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    c2 = clusters[j];
                    if (c1.Id != c2.Id)
                    {
                        double distance = computeDistance(c1.BoundingBox, c2.BoundingBox);
                        if (distance < this.spatialDelta)
                        {
                            distance = computeDistance(c1, c2);
                            if (distance < this.spatialDelta)
                            {
                                this.spatialDelta = distance;
                                this.closestClusters[0] = c1.Id;
                                this.closestClusters[1] = c2.Id;
                            }
                        }
                    }
                }
            }
        }

        private double computeDistance(System.Drawing.Rectangle rect1, System.Drawing.Rectangle rect2)
        {
            bool overlapHorizontal, overlapVertical;

            // Find if overlap in X
            if ((rect1.X >= rect2.X && rect1.X <= rect2.X + rect2.Width)
                || (rect1.X + rect1.Width >= rect2.X && rect1.X + rect1.Width <= rect2.X + rect2.Width)
                || (rect2.X >= rect1.X && rect2.X <= rect1.X + rect1.Width))
                overlapHorizontal = true;
            else
                overlapHorizontal = false;

            // Find if overlap in Y
            if ((rect1.Y >= rect2.Y && rect1.Y <= rect2.Y + rect2.Height)
                || (rect1.Y + rect1.Height >= rect2.Y && rect1.Y + rect1.Height <= rect2.Y + rect2.Height)
                || (rect2.Y >= rect1.Y && rect2.Y <= rect1.Y + rect1.Height))
                overlapVertical = true;
            else
                overlapVertical = false;

            if (overlapHorizontal && overlapVertical)
                return 0.0;
            else if (overlapHorizontal)
                return Math.Min(rect1.Y - (rect2.Y + rect2.Height), rect2.Y - (rect1.Y + rect1.Height));
            else if (overlapVertical)
                return Math.Min(rect1.X - (rect2.X + rect2.Width), rect2.X - (rect1.X + rect1.Width));
            else
            {
                double distance = 1000000.0;

                distance = Math.Min(distance, euclideanDistance(rect1, 1, rect2, 1));
                distance = Math.Min(distance, euclideanDistance(rect1, 1, rect2, 2));
                distance = Math.Min(distance, euclideanDistance(rect1, 1, rect2, 3));
                distance = Math.Min(distance, euclideanDistance(rect1, 1, rect2, 4));
                distance = Math.Min(distance, euclideanDistance(rect1, 2, rect2, 1));
                distance = Math.Min(distance, euclideanDistance(rect1, 2, rect2, 2));
                distance = Math.Min(distance, euclideanDistance(rect1, 2, rect2, 3));
                distance = Math.Min(distance, euclideanDistance(rect1, 2, rect2, 4));
                distance = Math.Min(distance, euclideanDistance(rect1, 3, rect2, 1));
                distance = Math.Min(distance, euclideanDistance(rect1, 3, rect2, 2));
                distance = Math.Min(distance, euclideanDistance(rect1, 3, rect2, 3));
                distance = Math.Min(distance, euclideanDistance(rect1, 3, rect2, 4));
                distance = Math.Min(distance, euclideanDistance(rect1, 4, rect2, 1));
                distance = Math.Min(distance, euclideanDistance(rect1, 4, rect2, 2));
                distance = Math.Min(distance, euclideanDistance(rect1, 4, rect2, 3));
                distance = Math.Min(distance, euclideanDistance(rect1, 4, rect2, 4));

                return distance;
            }
        }

        private double computeDistance(Cluster c1, Cluster c2)
        {
            double distance = 1000000.0;
            foreach (Substroke stroke1 in c1.Strokes)
            {
                foreach (Substroke stroke2 in c2.Strokes)
                {
                    foreach (Point pt1 in stroke1.Points)
                    {
                        foreach (Point pt2 in stroke2.Points)
                            distance = Math.Min(distance, euclDist(pt1, pt2));
                    }
                }
            }

            return distance;
        }

        private double euclDist(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2.0) + Math.Pow(pt1.Y - pt2.Y, 2.0));
        }

        private double euclideanDistance(System.Drawing.Rectangle rect1, int corner1, System.Drawing.Rectangle rect2, int corner2)
        {
            System.Drawing.PointF pt1, pt2;
            switch (corner1)
            {
                case 1:
                    pt1 = new System.Drawing.PointF((float)(rect1.X), (float)(rect1.Y));
                    break;
                case 2:
                    pt1 = new System.Drawing.PointF((float)(rect1.X + rect1.Width), (float)(rect1.Y));
                    break;
                case 3:
                    pt1 = new System.Drawing.PointF((float)(rect1.X), (float)(rect1.Y + rect1.Height));
                    break;
                case 4:
                    pt1 = new System.Drawing.PointF((float)(rect1.X + rect1.Width), (float)(rect1.Y + rect1.Height));
                    break;
                default:
                    pt1 = new System.Drawing.PointF();
                    break;
            }

            switch (corner2)
            {
                case 1:
                    pt2 = new System.Drawing.PointF((float)(rect2.X), (float)(rect2.Y));
                    break;
                case 2:
                    pt2 = new System.Drawing.PointF((float)(rect2.X + rect2.Width), (float)(rect2.Y));
                    break;
                case 3:
                    pt2 = new System.Drawing.PointF((float)(rect2.X), (float)(rect2.Y + rect2.Height));
                    break;
                case 4:
                    pt2 = new System.Drawing.PointF((float)(rect2.X + rect2.Width), (float)(rect2.Y + rect2.Height));
                    break;
                default:
                    pt2 = new System.Drawing.PointF();
                    break;
            }

            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2.0) + Math.Pow(pt1.Y - pt2.Y, 2.0));
        }

        public void findMinTimeDelta()
        {
            Cluster c1, c2;
            for (int i = 0; i < clusters.Count; i++)
            {
                c1 = clusters[i];
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    c2 = clusters[j];
                    if (c1.Id != c2.Id)
                    {
                        ulong timeGap = computeTimeGap(c1, c2);
                        if (timeGap < this.timeDelta)
                        {
                            this.timeDelta = timeGap;
                            this.temporalClosestClusters[0] = c1.Id;
                            this.temporalClosestClusters[1] = c2.Id;
                        }
                    }
                }
            }
        }

        public List<double> findAllDeltas()
        {
            List<double> values = new List<double>(clusters.Count * clusters.Count / 2);
            Cluster c1, c2;
            for (int i = 0; i < clusters.Count; i++)
            {
                c1 = clusters[i];
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    c2 = clusters[j];
                    if (c1.Id != c2.Id)
                    {
                        values.Add(computeDistance(c1, c2));
                    }
                }
            }

            return values;
        }

        public List<double> findAllTimeDeltas()
        {
            List<double> values = new List<double>(clusters.Count * clusters.Count / 2);
            Cluster c1, c2;
            for (int i = 0; i < clusters.Count; i++)
            {
                c1 = clusters[i];
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    c2 = clusters[j];
                    if (c1.Id != c2.Id)
                    {
                        values.Add(computeTimeGap(c1, c2));
                    }
                }
            }

            return values;
        }

        private ulong computeTimeGap(Cluster c1, Cluster c2)
        {
            if (c1.StartTime <= c2.StartTime)
            {
                if (c1.EndTime <= c2.StartTime)
                    return c2.StartTime - c1.EndTime;
                else
                    return 0;
            }
            else
            {
                if (c2.EndTime <= c1.StartTime)
                    return c1.StartTime - c2.EndTime;
                else
                    return 0;
            }
        }
    }

    [DebuggerDisplay("#ClusterSets = {clusterSets.Count}, Current Cluster = {currentClusterSet}")]
    public class AllClusterSets
    {
        private List<ClusterSet> clusterSets;

        private ClusterSet bestClusterSet;

        private double[] mergeDistances;

        private double[] temporalMergeDistances;

        private double[,] distances;

        private ulong[,] times;

        private double[,] probabilities;

        private double distanceThreshold;
        private ulong timeThreshold;
        private double clusteringThreshold;

        Dictionary<int, Sketch.Substroke> ind2Substroke;

        private double[] clusterWeights;

        private int currentClusterSet;

        public int bestCluster;

        private bool useSpatial;

        public AllClusterSets(ClusterSet cSet, bool spatial)
        {
            this.clusterSets = new List<ClusterSet>();
            this.clusterSets.Add(cSet);
            this.currentClusterSet = -1;
            this.useSpatial = spatial;
            this.distanceThreshold = 250.0;
            this.timeThreshold = 900;
            this.clusteringThreshold = 0.7;
            this.clusterWeights = new double[]{0.5, 0.5};
            this.mergeDistances = new double[cSet.Clusters.Count];
            this.temporalMergeDistances = new double[cSet.Clusters.Count];
            this.ind2Substroke = new Dictionary<int, Substroke>();
            this.bestCluster = 0;
        }

        public AllClusterSets(Sketch.Sketch sketch, Dictionary<Guid, string> strokeClassifications, string type, bool spatial)
        {
            this.useSpatial = spatial;
            ClusterSet cSet = new ClusterSet(sketch, strokeClassifications, "Shape");
            cSet.findMinDelta();
            cSet.findMinTimeDelta();
            cSet.applyClassifications(sketch);
            this.clusterSets = new List<ClusterSet>();
            this.clusterSets.Add(cSet);
            this.currentClusterSet = -1;
            this.mergeDistances = new double[cSet.Clusters.Count];
            this.temporalMergeDistances = new double[cSet.Clusters.Count];
            this.bestCluster = 0;
            this.distanceThreshold = 250.0;
            this.timeThreshold = 900;
            this.clusteringThreshold = 0.7;
            this.clusterWeights = new double[]{0.5, 0.5};
            this.ind2Substroke = new Dictionary<int, Substroke>();
            //findAllClusterSets();
            findBestClusterFromProbs();
        }

        public void findAllClusterSets()
        {
            List<double> spatialDistances = clusterSets[0].findAllDeltas();
            List<double> temporalDistances = clusterSets[0].findAllTimeDeltas();

            double averageDistances = computeAverage(spatialDistances);
            double averageTimes = computeAverage(temporalDistances);

            int current = 0;
            while (clusterSets[current].Clusters.Count > 2)
            {
                current = clusterSets.Count - 1;
                Guid[] closeClusters;
                if (this.useSpatial)
                    closeClusters = clusterSets[current].ClosestClusters;
                else
                    closeClusters = clusterSets[current].TemporalClosestClusters;
                ClusterSet newSet = new ClusterSet();
                bool clusterToMerge = false;
                bool clusterToMergeAdded = false;
                foreach (Cluster c in clusterSets[current].Clusters)
                {
                    clusterToMerge = false;
                    foreach (Guid id in closeClusters)
                    {
                        if (c.Id == id)
                            clusterToMerge = true;
                    }

                    if (!clusterToMerge)
                    {
                        Cluster cNew = new Cluster();
                        cNew.addStroke(c.Strokes);
                        cNew.ClassificationNum = c.ClassificationNum;
                        newSet.addCluster(cNew);
                    }
                    else
                    {
                        if (!clusterToMergeAdded)
                        {
                            bool okay1, okay2;
                            Cluster c1 = clusterSets[current].getCluster(closeClusters[0], out okay1);
                            Cluster c2 = clusterSets[current].getCluster(closeClusters[1], out okay2);
                            if (okay1 && okay2)
                            {
                                clusterToMergeAdded = true;
                                newSet.addCluster(Cluster.merge(c1, c2));
                            }
                        }
                    }
                }

                newSet.findMinDelta();
                newSet.findMinTimeDelta();
                clusterSets.Add(newSet);
            }


            findBestCluster();
        }

        /// <summary>
        /// Compute a 2-D array of the minimum distances between all sets of strokes in a classification
        /// </summary>
        private void findAllDistances()
        {
            List<Substroke> strokes = new List<Substroke>();
            foreach (Cluster c in this.clusterSets[0].Clusters)
                strokes.Add(c.Strokes[0]);

            distances = new double[strokes.Count, strokes.Count];
            for (int i = 0; i < strokes.Count; i++)
            {
                ind2Substroke.Add(i, strokes[i]);
                for (int j = i; j < strokes.Count; j++)
                {
                    if (i == j)
                        distances[i, j] = 0;
                    else
                    {
                        distances[i, j] = computeDistances(strokes[i], strokes[j]);
                        distances[j, i] = distances[i, j];                       
                    }
                }
            }
        }

        /// <summary>
        /// Compute the minimum Euclidean Distance between two strokes
        /// </summary>
        /// <param name="a">First Stroke</param>
        /// <param name="b">Second Stroke</param>
        /// <returns>Minimum Distance</returns>
        private double computeDistances(Substroke a, Substroke b)
        {
            double d = 1000000.0;

            foreach (Point pt1 in a.Points)
            {
                foreach (Point pt2 in b.Points)
                {
                    d = Math.Min(d, euclDist(pt1, pt2));
                }
            }

            return d;
        }

        /// <summary>
        /// Compute the Euclidean Distance between two points
        /// </summary>
        /// <param name="pt1">First Point</param>
        /// <param name="pt2">Second Point</param>
        /// <returns>Distance</returns>
        private double euclDist(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2.0) + Math.Pow(pt1.Y - pt2.Y, 2.0));
        }

        /// <summary>
        /// Compute a 2-D array of the time gaps between all sets of strokes in a classification
        /// </summary>
        private void findAllTimes()
        {
            List<Substroke> strokes = new List<Substroke>();
            foreach (Cluster c in this.clusterSets[0].Clusters)
                strokes.Add(c.Strokes[0]);

            times = new ulong[strokes.Count, strokes.Count];
            for (int i = 0; i < strokes.Count; i++)
            {
                for (int j = i; j < strokes.Count; j++)
                {
                    if (i == j)
                        times[i, j] = 0;
                    else
                    {
                        times[i, j] = computeTimes(strokes[i], strokes[j]);
                        times[j, i] = times[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Compute the time gap between two strokes
        /// </summary>
        /// <param name="a">First stroke</param>
        /// <param name="b">Second stroke</param>
        /// <returns>Time gap</returns>
        private ulong computeTimes(Substroke a, Substroke b)
        {
            ulong t = 1000000000000;

            if (a.Points[0].Time > b.Points[b.Points.Length - 1].Time)
                t = a.Points[0].Time - b.Points[b.Points.Length - 1].Time;
            else if (b.Points[0].Time > a.Points[a.Points.Length - 1].Time)
                t = b.Points[0].Time - a.Points[a.Points.Length - 1].Time;
            else
                t = 0;

            return t;
        }

        /// <summary>
        /// Compute a 2-D array of probabilities of clustering any two given strokes
        /// </summary>
        private void findProbabilitiesClustering()
        {
            int num = this.distances.GetLength(0);
            probabilities = new double[num, num];

            for (int i = 0; i < num; i++)
            {
                for (int j = i; j < num; j++)
                {
                    double Pd = thresholdingFcnDistance(this.distances[i, j]);
                    double Pt = thresholdingFcnTime(this.times[i, j]);


                    if (i == j)
                        probabilities[i, j] = 0;
                    else
                    {
                        probabilities[i, j] = Pd * this.clusterWeights[0] + Pt * this.clusterWeights[1];
                        probabilities[j, i] = probabilities[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Compute a probability that 2 strokes should be clustered together based on distance
        /// </summary>
        /// <param name="d">Distance between the strokes</param>
        /// <returns>Distance probability that strokes belong together</returns>
        private double thresholdingFcnDistance(double d)
        {
            if (d < distanceThreshold)
                return 1.0;
            else
                return 0.0;
        }

        /// <summary>
        /// Compute a probability that 2 strokes should be clustered together based on distance
        /// </summary>
        /// <param name="t">Time gap between the strokes</param>
        /// <returns>Time probability that strokes belong together</returns>
        private double thresholdingFcnTime(ulong t)
        {
            if (t < timeThreshold)
                return 1.0;
            else
                return 0.0;
        }

        /// <summary>
        /// Determine the best clustering based on the stroke-stroke probability matrix
        /// </summary>
        public void findBestClusterFromProbs()
        {
            findAllDistances();
            findAllTimes();
            findProbabilitiesClustering();
            List<int[]> pairs = findPairs();
            List<int> singles = findSingles(pairs);

            List<List<int>> finalClusters = findFinalClusters(pairs, singles);

            List<Cluster> bestClusters = new List<Cluster>();
            foreach (List<int> cluster in finalClusters)
            {
                List<Substroke> strokes = new List<Substroke>(cluster.Count);
                foreach (int i in cluster)
                    strokes.Add(ind2Substroke[i]);

                bestClusters.Add(new Cluster(strokes));
            }

            bestClusterSet = new ClusterSet(bestClusters);
        }

        /// <summary>
        /// Find all single stroke clusters (ones that didn't get grouped)
        /// </summary>
        /// <param name="pairs">list of pairs of substroke indices</param>
        /// <returns>Indices of Single Stroke Clusters</returns>
        private List<int> findSingles(List<int[]> pairs)
        {
            int num = this.distances.GetLength(0);
            List<int> pairValues = new List<int>(pairs.Count * 2);
            int a1, a2;
            bool found1, found2;

            foreach (int[] pair in pairs)
            {
                a1 = pair[0]; a2 = pair[1];
                found1 = false; found2 = false;

                foreach (int b in pairValues)
                {
                    if (a1 == b)
                        found1 = true;
                    if (a2 == b)
                        found2 = true;
                }

                if (!found1)
                    pairValues.Add(a1);
                if (!found2)
                    pairValues.Add(a2);
            }
            pairValues.Sort();

            List<int> singles = new List<int>(num - pairValues.Count);
            for (int i = 0; i < num; i++)
            {
                if (!pairValues.Contains(i))
                    singles.Add(i);
            }

            return singles;
        }

        /// <summary>
        /// Find stroke pairs based on probability matrix
        /// </summary>
        /// <returns>List of pairs</returns>
        private List<int[]> findPairs()
        {
            int num = this.distances.GetLength(0);
            List<int[]> pairs = new List<int[]>();

            for (int i = 0; i < num; i++)
            {
                for (int j = i; j < num; j++)
                {
                    if (this.probabilities[i, j] > clusteringThreshold)
                    {
                        pairs.Add(new int[] { i, j });
                    }
                }
            }

            return pairs;
        }

        /// <summary>
        /// Combines pair lists of strokes to find best clusterings
        /// </summary>
        /// <param name="pairs">List of stroke pairs</param>
        /// <returns>List of indices to make final clusters from</returns>
        private List<List<int>> findFinalClusters(List<int[]> pairs, List<int> singles)
        {
            List<List<int>> finalClusters = new List<List<int>>();
            bool found, found1, found2;
            int a1, a2;

            foreach (int[] pair in pairs)
            {
                found = found1 = found2 = false;
                a1 = pair[0];
                a2 = pair[1];

                foreach (List<int> cluster in finalClusters)
                {
                    foreach (int b in cluster)
                    {
                        if (a1 == b)
                        {
                            found = true;
                            found1 = true;
                        }
                        if (a2 == b)
                        {
                            found = true;
                            found2 = true;
                        }
                    }

                    if (found1 && !found2)
                        cluster.Add(a2);
                    else if (found2 && !found1)
                        cluster.Add(a1);
                }

                if (!found)
                    finalClusters.Add(new List<int>(pair));
            }

            foreach (int a in singles)
            {
                List<int> l = new List<int>(1);
                l.Add(a);
                finalClusters.Add(new List<int>(l));
            }

            return finalClusters;
        }

        private double computeAverage(List<double> values)
        {
            double avg = 0.0;
            for (int i = 0; i < values.Count; i++)
                avg += values[i];

            avg /= (values.Count);
            return avg;
        }

        /// <summary>
        /// Get the "best" Cluster Set which is found by inter-stroke distances and times.
        /// </summary>
        public ClusterSet BestClusterSet
        {
            get { return this.bestClusterSet; }
        }

        public List<ClusterSet> ClusterSets
        {
            get { return this.clusterSets; }
        }

        public int CurrentClusterSet
        {
            get { return this.currentClusterSet; }
        }

        public Dictionary<Guid, int> getClusterClassifications(ClusterSet cSet, Sketch.Sketch sketch)
        {
            foreach (ClusterSet cs in this.clusterSets)
            {
                if (cs.Id == cSet.Id)
                    return cs.getClusterClassifications(sketch);
            }
            if (cSet.Id == bestClusterSet.Id)
                return bestClusterSet.getClusterClassifications(sketch);

            return new Dictionary<Guid,int>();
        }

        public void nextClusterSet()
        {
            this.currentClusterSet++;
            if (this.currentClusterSet >= this.clusterSets.Count)
                this.currentClusterSet = 0;
        }

        public void previousClusterSet()
        {
            this.currentClusterSet--;
            if (this.currentClusterSet < 0)
                this.currentClusterSet = this.clusterSets.Count - 1;
        }

        public double[] MergeDistances
        {
            get
            {
                for (int i = 0; i < this.clusterSets.Count; i++)
                {
                    this.mergeDistances[i] = this.clusterSets[i].Delta;
                }
                return this.mergeDistances;
            }
        }

        public double[] TemporalMergeDistances
        {
            get
            {
                for (int i = 0; i < this.clusterSets.Count; i++)
                {
                    this.temporalMergeDistances[i] = this.clusterSets[i].TimeDelta;
                }
                return this.temporalMergeDistances;
            }
        }

        public void printDistances()
        {
            StreamWriter writer = new StreamWriter("C:\\distances.txt");

            for (int i = 0; i < this.TemporalMergeDistances.Length; i++)
            {
                writer.WriteLine("{0}, {1}, {2}", i, this.MergeDistances[i], this.TemporalMergeDistances[i]);
            }

            writer.Close();
        }

        public static void printDistances(double[] distances, double[] times)
        {
            StreamWriter writer = new StreamWriter("C:\\distances.txt");

            if (distances.Length == times.Length)
            {
                for (int i = 0; i < distances.Length; i++)
                {
                    writer.WriteLine("{0}, {1}, {2}", i, distances[i], times[i]);
                }
            }

            writer.Close();
        }

        public string currentDistances()
        {
            string a, b, c;

            if (currentClusterSet > 1)
                a = "     i-1= " + mergeDistances[currentClusterSet - 2].ToString("#0.00") + ",";
            else
                a = "     i-1= 0,";

            if (currentClusterSet > 0)
                b = "     i= " + mergeDistances[currentClusterSet - 1].ToString("#0.00") + ",";
            else
                b = "     i= 0,";

            if (currentClusterSet < clusterSets.Count)
                c = "     i+1= " + mergeDistances[currentClusterSet].ToString("#0.00");
            else
                c = "     i+1= 0";

            return a + b + c;
        }

        public string currentTimes()
        {
            string a, b, c;

            if (currentClusterSet > 1)
                a = "     i-1= " + TemporalMergeDistances[currentClusterSet - 2].ToString("#0.00") + ",";
            else
                a = "     i-1= 0,";

            if (currentClusterSet > 0)
                b = "     i= " + TemporalMergeDistances[currentClusterSet - 1].ToString("#0.00") + ",";
            else
                b = "     i= 0,";

            if (currentClusterSet < clusterSets.Count)
                c = "     i+1= " + TemporalMergeDistances[currentClusterSet].ToString("#0.00");
            else
                c = "     i+1= 0";

            return a + b + c;
        }

        private void findBestCluster()
        {
            double[] distances;
            if (useSpatial)
                distances = this.mergeDistances;
            else
                distances = this.temporalMergeDistances;
            this.bestCluster = 1;
            double numer, denom;
            if (distances.Length > 3)
            {
                double bestJump, Jump;
                numer = Math.Pow(distances[2] - distances[1], 2.0);
                denom = distances[1] - distances[0];
                if (denom < 0.1) denom = 0.1;
                bestJump = numer / denom;
                for (int i = 2; i < distances.Length - 2; i++)
                {
                    numer = Math.Pow(distances[i + 1] - distances[i], 2.0);
                    denom = distances[i] - distances[i - 1];
                    if (denom < 0.1) denom = 0.1;
                    Jump = numer / denom;

                    if (Jump > bestJump)
                    {
                        bestJump = Jump;
                        bestCluster = i;
                    }
                }
            }
        }

    }

    /*
    public class SketchClusters
    {
        private List<Cluster> clusters;
        private List<Cluster> oneStrokeClusters;
        private List<Cluster> twoStrokeClusters;
        private List<Cluster> threeStrokeClusters;
        private List<Cluster> fourStrokeClusters;
        private List<Cluster> fiveStrokeClusters;
        private List<Cluster> sixStrokeClusters;
        private List<Cluster> sevenStrokeClusters;
        private Sketch.Sketch sketch;
        private List<Substroke> nonWireStrokes;
        private List<Substroke> wireStrokes;
        private List<StrokeFeatures> features;
        private Dictionary<Guid, string> classifiedType;
        private Dictionary<Guid, StrokeFeatures> featureLookup;

        public SketchClusters(Sketch.Sketch sketch, List<StrokeFeatures> features)
        {
            this.sketch = sketch;
            this.features = features;
            this.clusters = new List<Cluster>();
            this.oneStrokeClusters = new List<Cluster>();
            this.twoStrokeClusters = new List<Cluster>();
            this.threeStrokeClusters = new List<Cluster>();
            this.fourStrokeClusters = new List<Cluster>();
            this.fiveStrokeClusters = new List<Cluster>();
            this.sixStrokeClusters = new List<Cluster>();
            this.sevenStrokeClusters = new List<Cluster>();
            this.nonWireStrokes = new List<Substroke>();
            this.wireStrokes = new List<Substroke>();
            this.classifiedType = new Dictionary<Guid, string>(this.features.Count);
            this.featureLookup = new Dictionary<Guid, StrokeFeatures>(this.features.Count);
            foreach (StrokeFeatures feature in this.features)
                featureLookup.Add(feature.Id, feature);
            separateStrokes();
            findOneStrokeClusters();
            findNearestStrokes();
            twoStrokeClustersFromOne();
        }


        private void separateStrokes()
        {
            for (int i = 0; i < this.sketch.Substrokes.Length; i++)
            {
                if (this.features[i].Classification == "Wire")
                {
                    this.classifiedType.Add(this.features[i].Id, "Wire");
                    wireStrokes.Add(this.sketch.Substrokes[i]);
                }
                else if (this.features[i].Classification == "Non-Wire")
                {
                    this.classifiedType.Add(this.features[i].Id, "Non-Wire");
                    nonWireStrokes.Add(this.sketch.Substrokes[i]);
                }
            }
        }

        private void findOneStrokeClusters()
        {
            foreach (Substroke stroke in this.nonWireStrokes)
                this.oneStrokeClusters.Add(new Cluster(stroke));
        }

        private void findNearestStrokes()
        {
            foreach (Cluster cluster in this.oneStrokeClusters)
            {
                StrokeFeatures feature = featureLookup[cluster.Strokes[0].XmlAttrs.Id.Value];
                
                addIntersectingStrokes(feature, cluster);

                if (cluster.NearestStrokes.Count < 10)
                {
                }
            }
        }

        private void addIntersectingStrokes(StrokeFeatures feature, Cluster cluster)
        {
            // Add any L-intersection strokes
            foreach (Guid lId in feature.L_Strokes)
            {
                if (!cluster.nearestContains(lId))
                {
                    if (classifiedType[lId] == "Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(lId), "Wire");
                    else if (classifiedType[lId] == "Non-Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(lId), "Non-Wire");
                }
            }

            // Add any T-Intersection strokes
            foreach (Guid tId in feature.T_Strokes_Teeing)
            {
                if (!cluster.nearestContains(tId))
                {
                    if (classifiedType[tId] == "Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(tId), "Wire");
                    else if (classifiedType[tId] == "Non-Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(tId), "Non-Wire");
                }
            }

            // Add any X-Intersection strokes
            foreach (Guid xId in feature.X_Strokes)
            {
                if (!cluster.nearestContains(xId))
                {
                    if (classifiedType[xId] == "Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(xId), "Wire");
                    else if (classifiedType[xId] == "Non-Wire")
                        cluster.addNearStroke(this.sketch.GetSubstroke(xId), "Non-Wire");
                }
            }
        }

        private void twoStrokeClustersFromOne()
        {
            for (int i = 0; i < this.clusters.Count; i++)
            {
                for (int j = i + 1; j < this.clusters.Count; j++)
                {
                    if (this.clusters[i].boundingBoxOverlap(this.clusters[j]))
                        this.twoStrokeClusters.Add(new Cluster(new Substroke[] { 
                            this.clusters[i].Strokes[0], this.clusters[j].Strokes[0] }));
                }
            }
        }

        private System.Drawing.Rectangle boundingBox(Substroke stroke)
        {
            return new System.Drawing.Rectangle((int)stroke.XmlAttrs.X, (int)stroke.XmlAttrs.Y, 
                (int)stroke.XmlAttrs.Width, (int)stroke.XmlAttrs.Height);
        }
    }
     * */
}
