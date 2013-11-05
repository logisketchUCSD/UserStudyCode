using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Sketch;
using Data;
using Microsoft.Ink;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.ComponentModel;
using Domain;

namespace Featurefy
{

    interface ISketchFeature
    {
        void SubstrokesAdded(IEnumerable<FeatureStroke> substrokes);
        void SubstrokesRemoved(IEnumerable<FeatureStroke> substrokes);
    }

    interface ISketchFeature<T> : ISketchFeature
    {
        T GetValue();
    }

    [DebuggerDisplay("{GetValue()}")]
    abstract class SubstrokeSumFeature : ISketchFeature<double>
    {
        
        private double _value = 0;

        protected abstract double valueForSubstroke(FeatureStroke s);

        public double GetValue()
        {
            return _value;
        }

        public void SubstrokesAdded(IEnumerable<FeatureStroke> substrokes)
        {
            foreach (FeatureStroke substroke in substrokes)
                _value += valueForSubstroke(substroke);
        }

        public void SubstrokesRemoved(IEnumerable<FeatureStroke> substrokes)
        {
            foreach (FeatureStroke substroke in substrokes)
                _value -= valueForSubstroke(substroke);
        }

    }

    class TotalArcLength : SubstrokeSumFeature
    {
        protected override double valueForSubstroke(FeatureStroke s)
        {
            return s.Substroke.SpatialLength;
        }
    }

    class TotalFirstLastDistance : SubstrokeSumFeature
    {
        protected override double valueForSubstroke(FeatureStroke s)
        {
            return s.Spatial.DistanceFromFirstToLast;
        }
    }

    class TotalFeatureValue : SubstrokeSumFeature
    {
        string _featureName;
        public TotalFeatureValue(string featureName)
        {
            _featureName = featureName;
        }
        protected override double valueForSubstroke(FeatureStroke s)
        {
            return s.Features[_featureName].Value;
        }
    }

    [DebuggerDisplay("{GetValue()}")]
    class SubstrokeAverageFeature : ISketchFeature<double>
    {
        private Sketch.Sketch _sketch;
        private SubstrokeSumFeature _total;

        public SubstrokeAverageFeature(Sketch.Sketch sketch, SubstrokeSumFeature total)
        {
            _sketch = sketch;
            _total = total;
        }

        public double GetValue()
        {
            return _total.GetValue() / _sketch.Substrokes.Length;
        }

        public void SubstrokesAdded(IEnumerable<FeatureStroke> substrokes) { }
        public void SubstrokesRemoved(IEnumerable<FeatureStroke> substrokes) { }
    }

	/// <summary>
	/// Feature aggregator for complete sketches. Contains interaction and multi-stroke features
	/// </summary>
	public class FeatureSketch
	{

		#region BoundingBox Class

		/// <summary>
		/// Helper class to represent a bounding box
		/// </summary>
		private class BoundingBox
		{
			private double topLeftX = Double.PositiveInfinity;
			private double topLeftY = Double.PositiveInfinity;
			private double bottomRightX = 0;
			private double bottomRightY = 0;

			/// <summary>
			/// Create a new bounding box for the given substrokes
			/// </summary>
			/// <param name="fss">The Featurefied Substrokes</param>
			public BoundingBox(List<FeatureStroke> fss)
			{
				foreach (FeatureStroke fs in fss)
				{
					if (fs.Spatial.UpperLeft.X < topLeftX)
						topLeftX = fs.Spatial.UpperLeft.X;
					if (fs.Spatial.UpperLeft.Y < topLeftY)
						topLeftY = fs.Spatial.UpperLeft.Y;
					if (fs.Spatial.LowerRight.X > bottomRightX)
						bottomRightX = fs.Spatial.LowerRight.X;
					if (fs.Spatial.LowerRight.Y > bottomRightY)
						bottomRightY = fs.Spatial.LowerRight.Y;
				}
			}

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="sketchBox">The rectangleF that contains the bounds of the bounding box</param>
            public BoundingBox(System.Drawing.RectangleF sketchBox)
            {
                topLeftX = sketchBox.X;
                topLeftY = sketchBox.Y;
                bottomRightX = sketchBox.Right;
                bottomRightY = sketchBox.Bottom;
            }

			/// <summary>
			/// Convert this bounding box to a double array
			/// </summary>
			/// <returns>[leftx, topy, rightx, bottomy]</returns>
			public double[] ToArray()
			{
				return new double[] { topLeftX, topLeftY, bottomRightX, bottomRightY };
			}

			/// <summary>
			/// Get the top-left coordinate as [x, y]
			/// </summary>
			public double[] TopLeft
			{
				get { return new double[] { topLeftX, topLeftY }; }
			}

			/// <summary>
			/// Get the bottom-right coordinate as [x, y]
			/// </summary>
			public double[] BottomRight
			{
				get { return new double[] { bottomRightX, bottomRightY }; }
			}

			/// <summary>
			/// BBox Width
			/// </summary>
			public double Width
			{
				get { return bottomRightX - topLeftX; }
			}

			/// <summary>
			/// BBox Height
			/// </summary>
			public double Height
			{
				get { return bottomRightY - topLeftY; }
			}

			/// <summary>
			/// BBox Area
			/// </summary>
			public double Area
			{
				get { return Height * Width; }
			}


            /// <summary>
            /// Convert Boundingbox to Rectange
            /// </summary>
            public System.Drawing.RectangleF Rectangle
            {
                get { return new System.Drawing.RectangleF((float)topLeftX, (float)topLeftY, (float)Width, (float)Height);  }
            }
		
        
        }

		#endregion

		#region Enums

		/// <summary>
		/// Different intersection types
		/// </summary>
		public enum IntersectionType
		{
			/// <summary>
			/// Generic T intersection
			/// </summary>
			T,
			/// <summary>
			/// This stroke is the top of a T
			/// </summary>
			TTop,
			/// <summary>
			/// This stroke is the stem of a T
			/// </summary>
			TStem,
			/// <summary>
			/// This stroke is in an L intersection
			/// </summary>
			L,
			/// <summary>
			/// This stroke is in an X intersection
			/// </summary>
			X
		}

		#endregion

		#region Internals

		/// <summary>
		/// The sketch object we're operating on
		/// </summary>
		private readonly Sketch.Sketch _sketch;

		/// <summary>
		/// Total substroke length in the sketch
		/// </summary>
        private TotalArcLength _totalArcLength;

        /// <summary>
        /// Total first-to-last substroke distance in the sketch
        /// </summary>
        private TotalFirstLastDistance _totalFirstLastDistance;

		/// <summary>
		/// Average first-last Euclidean substroke distance in the sketch
		/// </summary>
        private SubstrokeAverageFeature _averageDistance;

        /// <summary>
        /// Total sum of the speeds of each stroke
        /// </summary>
        private TotalFeatureValue _totalStrokeSpeed;

        /// <summary>
        /// Total sum of the widths of stroke bounding boxes
        /// </summary>
        private TotalFeatureValue _bBoxWidthSum;

        /// <summary>
        /// Total sum of the heights of stroke bounding boxes
        /// </summary>
        private TotalFeatureValue _bBoxHeightSum;

        /// <summary>
        /// Total sum of the areas of stroke bounding boxes
        /// </summary>
        private TotalFeatureValue _bBoxAreaSum;

		/// <summary>
		/// Bounding box of the overall sketch
		/// </summary>
		private BoundingBox _box = null;

        /// <summary>
        /// All intersections for the sketch
        /// </summary>
        private readonly IntersectionSketch _intersectionSketch;

        /// <summary>
        /// All pairwise feature values for the sketch
        /// </summary>
        private readonly PairwiseFeatureSketch _pairwiseFeatureSketch;

        /// <summary>
        /// Ensures that the correct stroke order is used for time gaps between strokes
        /// </summary>
        private SortedList<ulong, Substroke> _orderOfStrokes;

        /// <summary>
        /// Features to use for single-stroke classification
        /// </summary>
        private readonly Dictionary<string, bool> _featureListSingle;

        /// <summary>
        /// Features to use for pair-wise comparisons (grouping)
        /// </summary>
        private readonly Dictionary<string, bool> _featureListPair;

        /// <summary>
        /// Features for each stroke
        /// </summary>
        private Dictionary<Substroke, FeatureStroke> _strokeFeatures;

        // Is this needed? These are the lengths to extend each stroke's end
        private Dictionary<Substroke, double> _extensionLengthsActual;

        private Dictionary<Substroke, double> _extensionLengthsExtreme;

        private List<ClosedPath> _closedPaths;

		#endregion

		#region Constructors

        /// <summary>
        /// Constructor for FeatureSketch which includes an existing XML sketch. The lists 
        /// of features to use for both single-stroke and pairwise (grouping) classifications 
        /// are also required.
        /// </summary>
        /// <param name="sketch">XML sketch to featurize</param>
        /// <param name="featureListSingle">List of Features to use for single-stroke classification.</param>
        /// <param name="featureListPair">List of Features to use for pairwise (grouping) classification.</param>
        public FeatureSketch(Sketch.Sketch sketch, Dictionary<string, bool> featureListSingle, Dictionary<string, bool> featureListPair)
        {
            _sketch = sketch;
            _featureListSingle = featureListSingle;
            _featureListPair = featureListPair;
            _closedPaths = new List<ClosedPath>();
            _extensionLengthsActual = new Dictionary<Substroke, double>();
            _extensionLengthsExtreme = new Dictionary<Substroke, double>();

            if (sketch.Points.Length > 0)
                _box = new BoundingBox(Compute.BoundingBox(sketch.Points));
            else
                _box = new BoundingBox(new System.Drawing.RectangleF());

            _orderOfStrokes = GetOrderOfStrokesFromSketch(_sketch);

            _intersectionSketch = new IntersectionSketch(_sketch, _extensionLengthsExtreme);
            _pairwiseFeatureSketch = new PairwiseFeatureSketch(_sketch, _featureListPair);

            _totalArcLength = new TotalArcLength();
            _totalStrokeSpeed = new TotalFeatureValue("Average Pen Speed");
            _totalFirstLastDistance = new TotalFirstLastDistance();
            _averageDistance = new SubstrokeAverageFeature(_sketch, _totalFirstLastDistance);
            _bBoxWidthSum = new TotalFeatureValue("Bounding Box Width");
            _bBoxHeightSum = new TotalFeatureValue("Bounding Box Height");
            _bBoxAreaSum = new TotalFeatureValue("Bounding Box Area");

            _strokeFeatures = new Dictionary<Substroke, FeatureStroke>(_sketch.Substrokes.Length);
            AddSubstrokes(_sketch.SubstrokesL);

            double avgArcLength = AvgArcLength;
            foreach (Substroke stroke in _sketch.Substrokes)
                FindActualExtensionLength(stroke, avgArcLength);
        }

        /// <summary>
        /// Returns a featuresketch made from the settings given in the filenames returned by
        /// Files.SettingsReader.readSettings().
        /// </summary>
        /// <param name="s">sketch to base it off of</param>
        /// <returns>a fully initialized feature sketch</returns>
        public static FeatureSketch MakeFeatureSketch(Sketch.Sketch s)
        {
            return MakeFeatureSketch(s, Files.SettingsReader.readSettings());
        }

        /// <summary>
        /// Returns a featuresketch made from the settings given in the filenames.
        /// </summary>
        /// <param name="s">sketch to base it off of</param>
        /// <param name="filenames">a dictionary of settings</param>
        /// <returns>a fully initialized feature sketch</returns>
        public static FeatureSketch MakeFeatureSketch(Sketch.Sketch s, Dictionary<string, string> filenames)
        {
            Dictionary<string, bool> classifierFeatures, grouperFeatures;

            bool found;

            string filename;

            found = filenames.TryGetValue("FeaturesSingle", out filename);
            if (found)
                classifierFeatures = ReadFeatureListFile(filename);
            else
            {
                MessageBox.Show("No file containing List of Single Stroke Features to Use specified! (Key = FeaturesSingle)");
                classifierFeatures = new Dictionary<string, bool>();
            }

            found = filenames.TryGetValue("FeaturesGroup", out filename);
            if (found)
                grouperFeatures = ReadFeatureListFile(filename);
            else
            {
                MessageBox.Show("No file containing List of Grouper Features to Use specified! (Key = FeaturesGroup)");
                grouperFeatures = new Dictionary<string, bool>();
            }

            return new FeatureSketch(s, classifierFeatures, grouperFeatures);
        }

		#endregion

        #region Finding Closed Paths

        /// <summary>
        /// Finds all of the closed paths in the sketch.
        /// </summary>
        /// <returns></returns>
        private List<ClosedPath> FindClosedPaths(out bool UseClosedPath)
        {
            pruneFeatureStrokes();

            UseClosedPath = false;
            foreach (KeyValuePair<Substroke, FeatureStroke> pair in _strokeFeatures)
            {


                if (pair.Value.UseClosedPath)
                {
                    UseClosedPath = true;
                    break;
                }
            }

            Dictionary<Substroke, EndPoint[]> endPointConnections = FindEndPointConnections();

            return FindClosedPaths(_strokeFeatures, endPointConnections);
        }

        /// <summary>
        /// Finds all the Endpoint-Endpoint connections in the sketch. 
        /// This is used to find the closed paths.
        /// </summary>
        /// <returns></returns>
        private Dictionary<Substroke, EndPoint[]> FindEndPointConnections()
        {
            Dictionary<Substroke, EndPoint[]> stroke2endpoints = new Dictionary<Substroke, EndPoint[]>(_sketch.Substrokes.Length);
            List<EndPoint> endPointConnections = new List<EndPoint>(_sketch.Substrokes.Length * 2);
            foreach (Substroke s in _sketch.Substrokes)
            {
                EndPoint pt1 = new EndPoint(s, 1);
                EndPoint pt2 = new EndPoint(s, 2);
                endPointConnections.Add(pt1);
                endPointConnections.Add(pt2);
                stroke2endpoints.Add(s, new EndPoint[2] { pt1, pt2 });
            }

            for (int i = 0; i < endPointConnections.Count; i++)
            {
                for (int j = i + 1; j < endPointConnections.Count; j++)
                {
                    Substroke s1 = endPointConnections[i].Stroke;
                    Substroke s2 = endPointConnections[j].Stroke;
                    if (s1 != s2)
                    {
                        double d = 0.0;
                        if (_extensionLengthsActual.ContainsKey(s1) && _extensionLengthsActual.ContainsKey(s2))
                            d = Math.Max(_extensionLengthsActual[s1], _extensionLengthsActual[s2]);
                        double dist = Compute.EuclideanDistance(endPointConnections[i].Point, endPointConnections[j].Point);
                        if (dist < d)
                        {
                            endPointConnections[i].AddAttachment(endPointConnections[j]);
                            endPointConnections[j].AddAttachment(endPointConnections[i]);
                        }
                    }
                }
            }

            return stroke2endpoints;
        }

        /// <summary>
        /// Search function which starts the process which goes through all of 
        /// the endpoint-endpoint connections to find the closed path.
        /// </summary>
        /// <param name="features">Used to search for single-stroke closed paths.</param>
        /// <param name="endPtConnections">Endpoint-Endpoint connections in the sketch.</param>
        /// <returns></returns>
        private List<ClosedPath> FindClosedPaths(Dictionary<Substroke, FeatureStroke> features, Dictionary<Substroke, EndPoint[]> endPtConnections)
        {
            List<ClosedPath> paths = new List<ClosedPath>();

            foreach (KeyValuePair<Substroke, FeatureStroke> pair in features)
            {
                Substroke stroke = pair.Key;
                FeatureStroke feature = pair.Value;

                // If the stroke is self enclosing, automatically make a closed path of it
                if (feature.Features.ContainsKey("Self Enclosing"))
                {
                    if (feature.Features["Self Enclosing"].Value == 1.0)
                    {
                        ClosedPath path = new ClosedPath(stroke);
                        paths.Add(path);
                    }
                }

                EndPoint endPt1 = endPtConnections[stroke][0];
                EndPoint endPt2 = endPtConnections[stroke][1];
                if (endPt1.Attachments.Count > 0 && endPt2.Attachments.Count > 0)
                {
                    // Search for closed paths starting end 1
                    foreach (EndPoint endPoint in endPt1.Attachments)
                    {
                        //KeyValuePair<Guid, int> pathStartPt = new KeyValuePair<Guid, int>(feature.id, 1);
                        List<Substroke> strokesSoFar = new List<Substroke>();
                        strokesSoFar.Add(stroke);

                        if (ClosedPathSearch(endPt1, endPoint, endPtConnections, ref strokesSoFar))
                        {
                            ClosedPath path = new ClosedPath();
                            foreach (Substroke s in strokesSoFar)
                                path.AddSubtroke(s);

                            paths.Add(path);
                        }
                    }

                    // Search for closed paths starting at end 2
                    foreach (EndPoint endPoint in endPt2.Attachments)
                    {
                        //KeyValuePair<Guid, int> pathStartPt = new KeyValuePair<Guid, int>(feature.id, 1);
                        List<Substroke> strokesSoFar = new List<Substroke>();
                        strokesSoFar.Add(stroke);

                        if (ClosedPathSearch(endPt2, endPoint, endPtConnections, ref strokesSoFar))
                        {
                            ClosedPath path = new ClosedPath();
                            foreach (Substroke s in strokesSoFar)
                                path.AddSubtroke(s);

                            paths.Add(path);
                        }
                    }
                }
            }

            // Remove duplicate paths
            List<ClosedPath> paths2remove = new List<ClosedPath>();
            for (int i = 0; i < paths.Count; i++)
            {
                for (int j = i + 1; j < paths.Count; j++)
                {
                    if (paths[j].Equals(paths[i]))
                        paths2remove.Add(paths[j]);
                }
            }
            foreach (ClosedPath path2remove in paths2remove)
                paths.Remove(path2remove);

            // Set closedShapeConnection = true for each stroke involved
            foreach (ClosedPath path in paths)
            {
                foreach (Substroke s in path.Substrokes)
                {
                    if (_strokeFeatures.ContainsKey(s))
                    {
                        if (_strokeFeatures[s].Features.ContainsKey("Part of a Closed Path"))
                            _strokeFeatures[s].Features["Part of a Closed Path"] = new PartOfClosedPath(true);
                        else
                            _strokeFeatures[s].Features.Add("Part of a Closed Path", new PartOfClosedPath(true));
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Recursive function which searches the endpoint-endpoint connection space to find closed paths.
        /// </summary>
        /// <param name="pathStartPt">Where did the path start?</param>
        /// <param name="nextStroke">What endpoint are we going to now?</param>
        /// <param name="endPtConnections">All Endpoint-Endpoint connections.</param>
        /// <param name="strokesSoFar">Path so far, so that the same stroke is not 
        /// traversed more than once in a single path search.</param>
        /// <returns>Whether a closed path has been found.</returns>
        private bool ClosedPathSearch(EndPoint pathStartPt, EndPoint nextStroke,
            Dictionary<Substroke, EndPoint[]> endPtConnections, ref List<Substroke> strokesSoFar)
        {
            // Base case - have found a closed path
            if (nextStroke.Stroke == pathStartPt.Stroke && nextStroke != pathStartPt)
                return true;

            Substroke stroke = nextStroke.Stroke;
            if (!strokesSoFar.Contains(stroke))
            {
                strokesSoFar.Add(stroke);
                if (nextStroke.End == 2)
                {
                    EndPoint endPt = endPtConnections[stroke][0];
                    // Search for closed paths starting end 1
                    foreach (EndPoint endPoint in endPt.Attachments)
                        return ClosedPathSearch(pathStartPt, endPoint, endPtConnections, ref strokesSoFar);
                }
                else if (nextStroke.End == 1)
                {
                    EndPoint endPt = endPtConnections[stroke][1];
                    // Search for closed paths starting end 2
                    foreach (EndPoint endPoint in endPt.Attachments)
                        return ClosedPathSearch(pathStartPt, endPoint, endPtConnections, ref strokesSoFar);
                }
            }

            return false;
        }

        #endregion

		#region Accessors

        #region General Accessors

        private ISketchFeature[] SketchFeatures
        {
            get 
            {
                return new ISketchFeature[] {
                    _totalArcLength,
                    _totalStrokeSpeed,
                    _totalFirstLastDistance,
                    _averageDistance,
                    _bBoxWidthSum,
                    _bBoxHeightSum,
                    _bBoxAreaSum
                };
            }
        }

        /// <summary>
        /// The number of shapes in this sketch
        /// </summary>
        public int NumShapes
        {
            get { return _sketch.Shapes.Length; }
        }

        /// <summary>
        /// Get a reference to the sketch associated with this object
        /// </summary>
        public Sketch.Sketch Sketch
        {
            get { return _sketch; }
        }

        /// <summary>
        /// List of which features are on/off for single-stroke classification.
        /// </summary>
        public Dictionary<string, bool> FeatureListSingle
        {
            get { return _featureListSingle; }
        }

        /// <summary>
        /// List of which features are on/off for pairwise (grouping) classification.
        /// </summary>
        public Dictionary<string, bool> FeatureListPair
        {
            get { return _featureListPair; }
        }

        /// <summary>
        /// Gets a representation of all pairwise features in the sketch.
        /// </summary>
        public PairwiseFeatureSketch PairwiseFeatureSketch
        {
            get { return _pairwiseFeatureSketch; }
        }

        /// <summary>
        /// Gets a representation of all intersection features in the sketch
        /// </summary>
        public IntersectionSketch IntersectionSketch
        {
            get { return _intersectionSketch; }
        }

        #endregion

        #region Totals and Averages

		/// <summary>
		/// The sum of all of the arclengths of substrokes in this sketch
		/// </summary>
		public double TotalArcLength
		{
			get { return _totalArcLength.GetValue(); }
		}

		/// <summary>
		/// Average Euclidean first-last distance of a substroke in this sketch
		/// </summary>
		private double AverageDistance
		{
			get { return _averageDistance.GetValue(); }
		}

		/// <summary>
		/// The sum of the Euclidean first-last distances of the substrokes in this sketch
		/// </summary>
		public double TotalDistance
		{
			get
			{
                return AverageDistance * _strokeFeatures.Count;
			}
		}

        /// <summary>
        /// The average Arc Length in the sketch.
        /// </summary>
        private double AvgArcLength
        {
            get
            {
                if (_sketch.Substrokes.Length > 0)
                    return TotalArcLength / _sketch.Substrokes.Length;
                else
                    return 0.0;
            }
        }

        /// <summary>
        /// The average Width of a stroke's bounding box in the sketch.
        /// </summary>
        private double AvgBBoxWidth
        {
            get
            {
                if (_sketch.Substrokes.Length > 0)
                    return _bBoxWidthSum.GetValue() / _sketch.Substrokes.Length;
                else
                    return 0.0;
            }
        }

        /// <summary>
        /// The average Height of a stroke's bounding box in the sketch.
        /// </summary>
        private double AvgBBoxHeight
        {
            get
            {
                if (_sketch.Substrokes.Length > 0)
                    return _bBoxHeightSum.GetValue() / _sketch.Substrokes.Length;
                else
                    return 0.0;
            }
        }

        /// <summary>
        /// The average Area of a stroke's bounding box in the sketch.
        /// </summary>
        private double AvgBBoxArea
        {
            get
            {
                if (_sketch.Substrokes.Length > 0)
                    return _bBoxAreaSum.GetValue() / _sketch.Substrokes.Length;
                else
                    return 0.0;
            }
        }

        /// <summary>
        /// The average Pen Speed of a stroke in the sketch.
        /// </summary>
        private double AvgPenSpeed
        {
            get
            {
                if (_sketch.Substrokes.Length > 0)
                    return _totalStrokeSpeed.GetValue() / _sketch.Substrokes.Length;
                else
                    return 0.0;
            }
        }

        #endregion

        #region FeatureStroke Accessors

        /// <summary>
        /// Gets all the Feature strokes for the sketch.
        /// </summary>
        public Dictionary<Substroke, FeatureStroke> StrokeFeatures
        {
            get { return _strokeFeatures; }
        }

        #endregion

        #region Getting Stroke Features

        /// <summary>
        /// Get the feature values for a single substroke
        /// </summary>
        /// <param name="substroke">the substroke</param>
        /// <returns>an array of features</returns>
        public double[] GetValuesSingle(Substroke substroke)
        {
            UpdateNormalizersSingle();
            bool UseClosedPath;
            _closedPaths = FindClosedPaths(out UseClosedPath);

            FeatureStroke featureStroke = _strokeFeatures[substroke];
            featureStroke.CalculateMultiStrokeFeatures(_orderOfStrokes, _box.Rectangle);

            if (UseClosedPath)
            {
                foreach (ClosedPath path in _closedPaths)
                {
                    if (!path.Substrokes.Contains(substroke) && Compute.StrokeInsideBoundingBox(substroke, path.BoundingBox))
                    {
                        Feature value = new InsideClosedPath(true);
                        if (featureStroke.Features.ContainsKey("Inside a Closed Path"))
                            featureStroke.Features["Inside a Closed Path"] = value;
                        else
                            featureStroke.Features.Add("Inside a Closed Path", value);
                        break;
                    }
                }
            }

            if (featureStroke.UseIntersections)
                featureStroke.SetIntersectionFeatures(_intersectionSketch.GetIntersectionCounts(substroke));

            List<double> set = new List<double>();

            foreach (KeyValuePair<string, bool> pair2 in _featureListSingle)
            {
                string featureName = pair2.Key;
                bool useFeature = pair2.Value;

                if (!useFeature)
                    continue;

                if (!featureStroke.Features.ContainsKey(featureName))
                    throw new Exception("Missing feature: " + featureName);

                double value = featureStroke.Features[featureName].NormalizedValue;
                set.Add(value);
            }

            return set.ToArray();
        }

        /// <summary>
        /// Gets the input values for the grouper.
        /// </summary>
        /// <param name="strokeClassifications">Dictionary mapping substrokes to classifications</param>
        /// <returns>All pairwise feature values.</returns>
        public Dictionary<string, Dictionary<FeatureStrokePair, double[]>> GetValuesPairwise(Dictionary<Substroke, string> strokeClassifications)
        {
            Dictionary<string, List<double[]>> class2Values = new Dictionary<string, List<double[]>>();
            Dictionary<string, Dictionary<FeatureStrokePair, double[]>> pair2values = new Dictionary<string, Dictionary<FeatureStrokePair, double[]>>();

            bool useAllPairs = false;
            foreach (KeyValuePair<string, bool> useFeature in _featureListPair)
            {
                if (useFeature.Key.Substring(0, 6) == "Stroke" && useFeature.Value)
                {
                    useAllPairs = true;
                    break;
                }
            }

            if (useAllPairs)
            {
                class2Values.Add("All", new List<double[]>());
                pair2values.Add("All", new Dictionary<FeatureStrokePair, double[]>());
            }

            List<FeatureStrokePair> pairs = _pairwiseFeatureSketch.AllFeaturePairs;
            UpdateNormalizersPair(pairs);
            
            bool UseClosedPath;
            _closedPaths = FindClosedPaths(out UseClosedPath);

            foreach (FeatureStrokePair strokePair in pairs)
            {
                Substroke s1 = strokePair.A;
                Substroke s2 = strokePair.B;
                List<double> set = new List<double>();
                bool closed = false;
                foreach (ClosedPath path in _closedPaths)
                {
                    if (path.Substrokes.Contains(s1) && path.Substrokes.Contains(s2))
                    {
                        closed = true;
                        break;
                    }
                }

                // Strokes are same class
                if (useAllPairs || (strokeClassifications.ContainsKey(s1) && strokeClassifications.ContainsKey(s2) 
                    && strokeClassifications[s1] == strokeClassifications[s2]))
                {
                    string strokeClass1 = strokeClassifications[s1];
                    string strokeClass2 = strokeClassifications[s2];
                    double min1 = _pairwiseFeatureSketch.MinDistanceFromStrokeSameClass(s1);
                    double min2 = _pairwiseFeatureSketch.MinDistanceFromStrokeSameClass(s2);
                    double ratioA = Compute.GetDistanceRatio(strokePair.SubstrokeDistance.Min, min1);
                    double ratioB = Compute.GetDistanceRatio(strokePair.SubstrokeDistance.Min, min2);
                    double ratio1 = Math.Max(ratioA, ratioB);
                    double ratio2 = Math.Min(ratioA, ratioB);

                    // Loop through all features
                    foreach (KeyValuePair<string, bool> featureInfoPair in _featureListPair)
                    {
                        string featureName = featureInfoPair.Key;
                        bool useFeature = featureInfoPair.Value;

                        if (!useFeature)
                            continue;

                        // Special condition for Distance ratios, compute the value here!
                        if (featureName == "Distance Ratio A" || featureName == "Distance Ratio B")
                        {
                            double distance = strokePair.SubstrokeDistance.Min;
                            double value = 0.0;
                            if (featureName == "Distance Ratio A")
                                value = ratio1;
                            else
                                value = ratio2;

                            set.Add(value);
                        }
                        // Another special condition for closed paths
                        else if (featureName == "Part of Same Closed Path")
                        {
                            if (closed)
                                strokePair.Features[featureName] = new SameClosedPath(1.0);

                            set.Add(strokePair.Features[featureName].Value);
                        }
                        // Another special condition for classes
                        else if (featureName == "Stroke 1 Class 1")
                        {
                            if (strokeClass1 == LogicDomain.GATE_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        else if (featureName == "Stroke 1 Class 2")
                        {
                            if (strokeClass1 == LogicDomain.TEXT_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        else if (featureName == "Stroke 1 Class 3")
                        {
                            if (strokeClass1 == LogicDomain.WIRE_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        else if (featureName == "Stroke 2 Class 1")
                        {
                            if (strokeClass2 == LogicDomain.GATE_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        else if (featureName == "Stroke 2 Class 2")
                        {
                            if (strokeClass2 == LogicDomain.TEXT_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        else if (featureName == "Stroke 2 Class 3")
                        {
                            if (strokeClass2 == LogicDomain.WIRE_CLASS)
                                set.Add(1.0);
                            else
                                set.Add(0.0);
                        }
                        // Not a distance ratio, go about getting the value normally
                        else if (strokePair.Features.ContainsKey(featureName))
                        {
                            double value = strokePair.Features[featureName].NormalizedValue;
                            set.Add(value);
                        }
                    }

                    // Add the data from the latest stroke pair to the list
                    // List is separated by class name
                    if (useAllPairs)
                    {
                        class2Values["All"].Add(set.ToArray());
                    }
                    else if (class2Values.ContainsKey(strokeClass1))
                        class2Values[strokeClass1].Add(set.ToArray());
                    else
                    {
                        List<double[]> values = new List<double[]>();
                        values.Add(set.ToArray());
                        class2Values.Add(strokeClass1, values);
                    }

                    // Add the data from the latest stroke pair to the list
                    // List is separated by class name

                    if (useAllPairs)
                    {
                        if (!pair2values["All"].ContainsKey(strokePair))
                            pair2values["All"].Add(strokePair, set.ToArray());
                    }
                    else if (pair2values.ContainsKey(strokeClass1))
                    {
                        if (!pair2values[strokeClass1].ContainsKey(strokePair))
                            pair2values[strokeClass1].Add(strokePair, set.ToArray());
                    }
                    else
                    {
                        Dictionary<FeatureStrokePair, double[]> values = new Dictionary<FeatureStrokePair, double[]>();
                        values.Add(strokePair, set.ToArray());
                        pair2values.Add(strokeClass1, values);
                    }
                }
            }

            return pair2values;
        }

        #endregion

        #endregion

        #region Adding and Removing Strokes

        private void AddSubstrokes(IEnumerable<Substroke> substrokes)
        {
            List<FeatureStroke> newFeatureStrokes = new List<FeatureStroke>();

            foreach (Substroke stroke in substrokes)
            {
                if (_box.Width == 0f && _box.Height == 0f)
                    _box = new BoundingBox(Compute.BoundingBox(stroke.Points));

                if (!Compute.StrokeInsideBoundingBox(stroke, _box.Rectangle))
                    _box = new BoundingBox(Compute.BoundingBox(_sketch.Points));

                FeatureStroke feature = GetFeatureStrokeAndUpdateSums(stroke);
                newFeatureStrokes.Add(feature);

                FindActualExtensionLength(stroke, AvgArcLength);

                if (!_orderOfStrokes.ContainsValue(stroke) && !_orderOfStrokes.ContainsKey(feature.Substroke.XmlAttrs.Time.Value))
                    _orderOfStrokes.Add(feature.Substroke.XmlAttrs.Time.Value, feature.Substroke);

                _intersectionSketch.AddStroke(stroke);
                _pairwiseFeatureSketch.AddStroke(stroke);
            }

            foreach (ISketchFeature sketchFeature in SketchFeatures)
                sketchFeature.SubstrokesAdded(newFeatureStrokes);
        }

        /// <summary>
        /// Adds the given stroke to the feature sketch. This method also
        /// adds the stroke to the underlying sketch.
        /// </summary>
        /// <param name="stroke"></param>
        public void AddStroke(Sketch.Stroke stroke)
        {
            _sketch.AddStroke(stroke);
            AddSubstrokes(stroke.Substrokes);
        }

        private void UpdateExtensionLengths()
        {
            double avgArcLength = AvgArcLength;

            foreach (Substroke s in _sketch.Substrokes)
            {
                double d1 = (s.SpatialLength + avgArcLength) / 2.0;
                double extensionLength = Math.Min(avgArcLength, d1) * Compute.THRESHOLD;

                if (_extensionLengthsActual.ContainsKey(s))
                    _extensionLengthsActual[s] = extensionLength;
                else
                    _extensionLengthsActual.Add(
                        s, extensionLength);

                if (!_extensionLengthsExtreme.ContainsKey(s))
                    _extensionLengthsExtreme.Add(s, Math.Max(avgArcLength, s.SpatialLength));

                if (_extensionLengthsExtreme[s] < extensionLength)
                {
                    _extensionLengthsExtreme[s] = extensionLength;
                    _intersectionSketch.UpdateIntersections(s);
                }

            }
        }

        /// <summary>
        /// Remove the given substrokes from the feature sketch.
        /// </summary>
        /// <param name="substrokes">the substrokes to remove</param>
        public void RemoveSubstrokes(IEnumerable<Substroke> substrokes)
        {
            List<FeatureStroke> oldFeatureStrokes = new List<FeatureStroke>();

            foreach (Substroke stroke in substrokes)
            {
                if (!_sketch.SubstrokesL.Contains(stroke))
                    throw new ArgumentException("The given substroke was not in the underlying sketch");

                if (!_strokeFeatures.ContainsKey(stroke))
                    throw new Exception("The given substroke is in the sketch, but not tracked by this feature sketch");

                if (!_orderOfStrokes.ContainsValue(stroke))
                    throw new Exception("The given substroke is tracked by this feature sketch, but is not sorted");

                _sketch.RemoveSubstroke(stroke);

                if (_sketch.IsEmpty)
                    _box = new BoundingBox(new System.Drawing.RectangleF());
                else
                    _box = new BoundingBox(Compute.BoundingBox(_sketch.Points));

                oldFeatureStrokes.Add(_strokeFeatures[stroke]);
                _strokeFeatures.Remove(stroke);

                int index = _orderOfStrokes.IndexOfValue(stroke);
                _orderOfStrokes.RemoveAt(index);

                if (_extensionLengthsActual.ContainsKey(stroke))
                    _extensionLengthsActual.Remove(stroke);

                if (_extensionLengthsExtreme.ContainsKey(stroke))
                    _extensionLengthsExtreme.Remove(stroke);

                if (_intersectionSketch.Strokes.Contains(stroke))
                    _intersectionSketch.RemoveStroke(stroke);

                if (_pairwiseFeatureSketch.Strokes.Contains(stroke))
                    _pairwiseFeatureSketch.RemoveStroke(stroke);
            }

            foreach (ISketchFeature sketchFeature in SketchFeatures)
                sketchFeature.SubstrokesRemoved(oldFeatureStrokes);
        }

        /// <summary>
        /// Removes the stroke from the feature sketch and all the downstream 
        /// representations and features.
        /// </summary>
        /// <param name="stroke">Stroke to Remove.</param>
        public void RemoveSubstroke(Substroke stroke)
        {
            RemoveSubstrokes(new Substroke[] { stroke });
        }

        #endregion

        #region Other Functions

        /// <summary>
        /// Removes feature strokes which are not in the sketch.
        /// </summary>
        private void pruneFeatureStrokes()
        {
            Dictionary<Substroke, FeatureStroke> result = new Dictionary<Substroke, FeatureStroke>();
            foreach (KeyValuePair<Substroke, FeatureStroke> pair in _strokeFeatures)
            {
                if (Sketch.SubstrokesL.Contains(pair.Key))
                    result.Add(pair.Key, pair.Value);

            }
            _strokeFeatures = result;
        }
        
        private static SortedList<ulong, Substroke> GetOrderOfStrokesFromSketch(Sketch.Sketch sketch)
        {
            SortedList<ulong, Substroke> order = new SortedList<ulong, Substroke>();
            foreach (Substroke stroke in sketch.Substrokes)
            {
                if (!order.ContainsValue(stroke))
                {
                    ulong time = stroke.Time.Value;
                    while (order.ContainsKey(time))
                        time++;

                    order.Add(time, stroke);
                }
            }

            return order;
        }

        private FeatureStroke GetFeatureStrokeAndUpdateSums(Substroke stroke)
        {
            FeatureStroke feature = new FeatureStroke(stroke, _featureListSingle);
            _strokeFeatures.Add(stroke, feature);
            return feature;
        }

        private void FindActualExtensionLength(Substroke stroke, double avgArcLength)
        {
            double d1 = (stroke.SpatialLength + avgArcLength) / 2.0;
            double extensionLength = Math.Min(avgArcLength, d1) * Compute.THRESHOLD;
            if (!_extensionLengthsActual.ContainsKey(stroke))
                _extensionLengthsActual.Add(stroke, extensionLength);

            UpdateExtensionLengths();
        }

        /// <summary>
        /// Function to set the normalizers for feature-stroke-pairs based on sketch size
        /// </summary>
        private void UpdateNormalizersPair(List<FeatureStrokePair> pairs)
        {
            double sketchDiagonal = Math.Sqrt(Math.Pow(_box.Height, 2.0) + Math.Pow(_box.Width, 2.0));

            foreach (KeyValuePair<string, bool> pair in _featureListPair)
            {
                string featureName = pair.Key;
                bool useFeature = pair.Value;
                if (useFeature)
                {
                    foreach (FeatureStrokePair featureStrokePair in pairs)
                    {
                        #region Switch
                        switch (featureName)
                        {
                            case ("Minimum Distance"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Maximum Distance"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Centroid Distance"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Horizontal Overlap"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Vertical Overlap"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Minimum Endpoint to Endpoint Distance"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            case ("Minimum Endpoint to Any Point Distance"):
                                featureStrokePair.Features[featureName].SetNormalizer(sketchDiagonal);
                                break;
                            default:
                                break;
                        }
                        #endregion
                    }
                }
            }
        }

         /// <summary>
        /// Function to set the normalizers for each feature-stroke based on sketch averages
        /// </summary>
        private void UpdateNormalizersSingle()
        {
            double avgArcLength = AvgArcLength;
            double avgWidth = AvgBBoxWidth;
            double avgHeight = AvgBBoxHeight;
            double avgArea = AvgBBoxArea;
            double avgPenSpeed = AvgPenSpeed;
            double avgAreaSQRT = Math.Sqrt(avgArea);

            foreach (KeyValuePair<string, bool> pair in _featureListSingle)
            {
                string featureName = pair.Key;
                bool useFeature = pair.Value;
                if (useFeature)
                {
                    foreach (KeyValuePair<Substroke, FeatureStroke> featureStroke in _strokeFeatures)
                    {
                        #region Switch
                        switch (featureName)
                        {
                            case ("Arc Length"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgArcLength);
                                break;
                            case ("Bounding Box Width"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgWidth);
                                //featureStroke.Value.Features[featureName].SetNormalizer(avgAreaSQRT);
                                break;
                            case ("Bounding Box Height"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgHeight);
                                //featureStroke.Value.Features[featureName].SetNormalizer(avgAreaSQRT); 
                                break;
                            case ("Bounding Box Area"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgArea);
                                break;
                            case ("Average Pen Speed"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgPenSpeed);
                                break;
                            case ("Maximum Pen Speed"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgPenSpeed);
                                break;
                            case ("Minimum Pen Speed"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgPenSpeed);
                                break;
                            case ("Difference Between Maximum and Minimum Pen Speed"):
                                featureStroke.Value.Features[featureName].SetNormalizer(avgPenSpeed);
                                break;
                            case ("Time to Draw Stroke"):
                                //featureStroke.Value.Features[featureName].SetNormalizer(avgTimeDraw);
                                break;
                            case ("Time to Previous Stroke"):
                                //featureStroke.Value.Features[featureName].SetNormalizer(avgTimeDraw);
                                break;
                            case ("Time to Next Stroke"):
                                //featureStroke.Value.Features[featureName].SetNormalizer(avgTimeDraw);
                                break;
                            default:
                                //if (!_strokeFeatures[featureStroke.Key].Features.ContainsKey(featureName))
                                    //_strokeFeatures[featureStroke.Key].Features.Add(featureName, new Feature());

                                break;
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the Sketch, FeatureSketch, IntersectionSketch and PairwiseFeatureSketch are consistent.
        /// </summary>
        /// <returns></returns>
        public bool hasConsistentSubstrokes()
        {
            List<Sketch.Substroke> sketchSubstrokes = Sketch.SubstrokesL;
            List<Sketch.Substroke> intersectionSketchSubstrokes = IntersectionSketch.Strokes;
            List<Sketch.Substroke> pairwiseSketchSubstrokes = PairwiseFeatureSketch.Strokes;
            Dictionary<Sketch.Substroke, Featurefy.FeatureStroke> featureSketch = StrokeFeatures;

            // The four data structures should have the same size.
            bool sameSize = (sketchSubstrokes.Count == intersectionSketchSubstrokes.Count &&
                sketchSubstrokes.Count == pairwiseSketchSubstrokes.Count &&
                sketchSubstrokes.Count == featureSketch.Keys.Count);

            // The four data structures should have the same substrokes.
            bool intersectionHasCorrectStrokes = true;
            bool pairwiseHasCorrectStrokes = true;
            bool featureHasCorrectStrokes = true;
            foreach (Sketch.Substroke substroke in sketchSubstrokes)
            {
                if (!intersectionSketchSubstrokes.Contains(substroke)) intersectionHasCorrectStrokes = false;
                if (!pairwiseSketchSubstrokes.Contains(substroke)) pairwiseHasCorrectStrokes = false;
                if (!featureSketch.ContainsKey(substroke)) featureHasCorrectStrokes = false;
            }

#if DEBUG
            if (!sameSize) throw new Exception("The Sketch, FeatureSketch, IntersectionSketch and PairwiseFeatureSketch are inconsistent");
            if (!intersectionHasCorrectStrokes) throw new Exception("The IntersectionSketch is missing a stroke");
            if (!pairwiseHasCorrectStrokes) throw new Exception("The PairwiseFeatureSketch is missing a stroke");
            if (!featureHasCorrectStrokes) throw new Exception("FeatureSketch is missing a stroke");
#endif

            return (sameSize && intersectionHasCorrectStrokes && pairwiseHasCorrectStrokes && featureHasCorrectStrokes);
        }

        #endregion

        #region Loading Feature List

        /// <summary>
        /// Reads in features from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static Dictionary<string, bool> ReadFeatureListFile(string filename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            Dictionary<string, bool> featureList = new Dictionary<string, bool>();
            string line;

            while ((line = reader.ReadLine()) != null && line != "")
            {
                string key = line.Substring(0, line.IndexOf("=") - 1);
                string value = line.Substring(line.IndexOf("=") + 2);
                if (value == "true")
                    featureList.Add(key, true);
                else if (value == "false")
                    featureList.Add(key, false);
                else
                    return null;
            }

            reader.Close();

            return featureList;
        }

        #endregion

    }
}
