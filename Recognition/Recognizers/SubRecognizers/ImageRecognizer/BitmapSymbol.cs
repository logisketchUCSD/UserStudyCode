using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using Utilities;
using Utilities.Matrix;
using System.Diagnostics;
using System.Windows;

namespace SubRecognizer
{
    [Serializable]
    public class BitmapSymbol : ICloneable
    {
        #region Constants

        private int NUM_TOP_POLAR_TO_KEEP = 10;
        private int NUM_RECOGNITIONS_TO_RETURN = 5;
        private double ALLOWED_ROTATION_AMOUNT = 360.0;

        private const int GRID_X_SIZE = 24;
        private const int GRID_Y_SIZE = 24;

        private const double POLAR_WEIGHT_DECAY_RATE = 0.1;
        private const double HAUSDORFF_QUANTILE = 0.94;
        private const double OVERLAP_THRESHOLD = 1.0 / 20.0;

        #endregion

        #region Member Variables

        private string _name;
        private ShapeType _type;
        private RecoResult _results;
        private Guid _id;
        private System.Drawing.Bitmap _bitmap;

        private BitmapPoints _points;

        private List<Coord> _screenCoords;
        private GeneralMatrix _sMesh;
        private GeneralMatrix _sDTM;

        private List<Coord> _polarCoords;
        private GeneralMatrix _pMesh;
        private GeneralMatrix _pDTM;

        #endregion

        #region Constructors

        public BitmapSymbol()
        {
            _name = "";
            _type = new ShapeType();
            _results = new RecoResult();
            _id = Guid.NewGuid();
            _points = new BitmapPoints();
            _screenCoords = new List<Coord>();
            _sMesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);
            _sDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, double.PositiveInfinity);
            _polarCoords = new List<Coord>();
            _pMesh = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);
            _pDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, double.PositiveInfinity);
        }

        public BitmapSymbol(List<Substroke> strokes)
            : this()
        {
            foreach (Substroke sub in strokes)
                _points.AddStroke(sub);
            Process();
        }

        public BitmapSymbol(List<Substroke> strokes, ShapeType type, System.Drawing.Bitmap bitmap)
            : this(strokes)
        {
            _bitmap = bitmap;
            _type = type;
            Process();
        }

        public object Clone()
        {
            BitmapSymbol symbol = (BitmapSymbol)this.MemberwiseClone();
            symbol._results = (RecoResult)this._results.Clone();
            symbol._sMesh = (GeneralMatrix)this._sMesh.Clone();
            symbol._sDTM = (GeneralMatrix)this._sDTM.Clone();
            symbol._pMesh = (GeneralMatrix)this._pMesh.Clone();
            symbol._pDTM = (GeneralMatrix)this._pDTM.Clone();
            symbol._points = (BitmapPoints)this._points.Clone();

            symbol._polarCoords = new List<Coord>();
            foreach (Coord cd in this._polarCoords)
                symbol._polarCoords.Add((Coord)cd.Clone());

            symbol._screenCoords = new List<Coord>();
            foreach (Coord cd in this._screenCoords)
                symbol._screenCoords.Add((Coord)cd.Clone());

            return symbol;
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Calculates the dissimilarity between the calling BitmapSymbol and
        /// BitmapSymbol S in terms of their polar coordinates using the
        /// modified Hausdorff distance (aka the mean distance).
        /// </summary>
        private SymbolRank Polar_Mod_Hausdorff(BitmapSymbol S)
        {
            int lower_rot_index = (int)Math.Floor(ALLOWED_ROTATION_AMOUNT * GRID_X_SIZE / 720.0);
            int upper_rot_index = GRID_X_SIZE - lower_rot_index; //trick

            List<Coord> A = new List<Coord>(_polarCoords);
            List<Coord> B = new List<Coord>(S._polarCoords);

            if (A.Count == 0 || B.Count == 0) return new SymbolRank();

            double minA2B = double.PositiveInfinity;
            double distan = 0.0;
            int besti = 0; // the best translation in theta

#if FALSE
            Console.WriteLine("Your gate: ");
            Console.Write(_pMesh.ToString());

            Console.WriteLine("Your template: ");
            Console.Write(S._pMesh.ToString());
#endif

            // rotations in screen coordinates are the same as translations in polar coords
            // we have polar coords, so translate in X (theta) until you get the best orientation
            for (int i = 0; i < GRID_X_SIZE; i++)
            {
                if (i > lower_rot_index && i < upper_rot_index)
                    continue;

                distan = 0.0;
                // find the distance from each point in A to the nearest point in B using B_DTM
                foreach (Coord a in A)
                {
                    int y_ind = (int)a.Y;
                    int x_ind = (int)a.X - i; // translate by i on the theta (X) axis
                    if (x_ind < 0) x_ind += GRID_X_SIZE; // make sure we're still on the graph

                    //putting less weight on points that have small rel dist - y 
                    double weight = Math.Pow((double)y_ind, POLAR_WEIGHT_DECAY_RATE);
                    distan += S._pDTM.GetElement(y_ind, x_ind) * weight;
                }

                // this is the best orientation if the total distance is the smallest
                if (distan < minA2B)
                {
                    minA2B = distan;
                    besti = i;
                }
            }

            // set the best rotation angle (in radians)
            double bestRotAngle = besti * 2.0 * Math.PI / (double)GRID_X_SIZE;

            // we've already found the best orientation
            // find the distance from each point in B to the nearest point in A using A_DTM
            double minB2A = 0.0;
            foreach (Coord b in B)
            {
                int y_ind = (int)b.Y;
                int x_ind = (int)b.X - besti; // slide B back by besti
                if (x_ind < 0) x_ind += GRID_X_SIZE;
                double weight = Math.Pow((double)y_ind, POLAR_WEIGHT_DECAY_RATE);
                minB2A += _pDTM.GetElement(y_ind, x_ind) * weight;
            }

            minA2B /= (double)A.Count;
            minB2A /= (double)B.Count;

#if FALSE
            Console.WriteLine("Finding best orientation match of your gate and " + S._name);
            Console.WriteLine("The best translation is " + besti + " which is " + bestRotAngle + " radians.");
            Console.WriteLine("A2B distance: " + minA2B + ", B2A distance: " + minB2A);
            string templateName = S._name;
#endif

            return new SymbolRank(Math.Max(minA2B, minB2A), S, bestRotAngle);
        }

        /// <summary>
        /// Calculates the maximum Y value of a list of coordinates.
        /// </summary>
        /// <param name="points">The list of coordinates.</param>
        /// <returns>The largest Y value.</returns>
        private double Ymax(List<Coord> points)
        {
            double max = double.NegativeInfinity;

            foreach (Coord pt in points)
                max = Math.Max(max, pt.Y);

            return max;
        }

        /// <summary>
        /// Calculates the distance from every point in one BitmapSymbol to the closest point
        /// in another.
        /// </summary>
        private static List<double> directedScreenDistance(BitmapSymbol from, BitmapSymbol to)
        {
            List<double> dist = new List<double>();
            foreach (Coord pt in from._screenCoords)
                dist.Add(to._sDTM.GetElement((int)pt.Y, (int)pt.X));
            return dist;
        }

        /// <summary>
        /// Calculates the Hausdorff distance between the calling BitmapSymbol and the 
        /// BitmapSymbol S.
        /// </summary>
        /// <returns>The maximum of the two partial Hausdorff distances.</returns>
        private SymbolRank Partial_Hausdorff(BitmapSymbol S)
        {
            List<double> distancesAB = directedScreenDistance(this, S);
            List<double> distancesBA = directedScreenDistance(S, this);

            distancesAB.Sort();
            distancesBA.Sort();

            double hAB = double.PositiveInfinity;
            double hBA = double.PositiveInfinity;

            if (distancesAB.Count != 0) hAB = distancesAB[(int)Math.Floor(((distancesAB.Count - 1) * HAUSDORFF_QUANTILE))];
            if (distancesBA.Count != 0) hBA = distancesBA[(int)Math.Floor(((distancesBA.Count - 1) * HAUSDORFF_QUANTILE))];

            return new SymbolRank(Math.Max(hAB, hBA), S);
        }

        /// <summary>
        /// Calculates the average directed distance between the calling BitmapSymbol
        /// and the BitmapSymbol S.
        /// </summary>
        /// <returns>The average distance between the two sets of points.</returns>
        private SymbolRank Modified_Hausdorff(BitmapSymbol S)
        {
            List<double> distancesAB = directedScreenDistance(this, S);
            List<double> distancesBA = directedScreenDistance(S, this);

            double AB = 0.0;
            double BA = 0.0;

            foreach (double dist in distancesAB)
                AB += dist;

            foreach (double dist in distancesBA)
                BA += dist;

            AB /= this._screenCoords.Count;
            BA /= S._screenCoords.Count;

            return new SymbolRank(Math.Max(AB, BA), S);
        }

        /// <summary>
        /// Calculates information about the number of black pixels and how they overlap.
        /// </summary>
        /// <param name="A">A BitmapSymbol</param>
        /// <param name="B">Another BitmapSymbol</param>
        /// <param name="A_count">The number of black pixels in A</param>
        /// <param name="B_count">The number of black pixels in B</param>
        /// <param name="black_overlap">The number of black pixels that "overlap" in the two</param>
        /// <param name="white_overlap">The number of white pixels in A that don't "overlap"
        ///     with a black pixel in B</param>
        private static void Black_White(BitmapSymbol A, BitmapSymbol B, 
                out int A_count, out int B_count, 
                out int black_overlap, out int white_overlap)
        {
            // we consider pixels to be overlapping if they are separated
            // by less than a certain fraction of the image's diagonal length
            double E = OVERLAP_THRESHOLD * Math.Sqrt(Math.Pow((double)GRID_X_SIZE, 2.0) + Math.Pow((double)GRID_Y_SIZE, 2.0));

            A_count = B_count = black_overlap = white_overlap = 0; // initialize them all to zero!

            for (int i = 0; i < A._sDTM.ColumnDimension; i++)
            {
                for (int j = 0; j < A._sDTM.RowDimension; j++)
                {
                    if (A._sDTM.GetElement(i, j) == 0.0)
                        A_count++;

                    if (B._sDTM.GetElement(i, j) == 0.0)
                        B_count++;

                    if ((A._sDTM.GetElement(i, j) == 0.0 && B._sDTM.GetElement(i, j) < E))
                        black_overlap++;

                    if (A._sDTM.GetElement(i, j) > 0.0 && B._sDTM.GetElement(i, j) > 0.0)
                        white_overlap++;
                }
            }
        }

        /// <summary>
        /// Calculates the Tanimoto Similarity Coefficient for the calling BitmapSymbol
        /// and the BitmapSymbol S.
        /// </summary>
        private SymbolRank Tanimoto_Distance(BitmapSymbol S)
        {
            int A_count, B_count, black_overlap, white_overlap;

            Black_White(this, S, out A_count, out B_count, out black_overlap, out white_overlap);

            double Tanim = (double)black_overlap / (double)(A_count + B_count - black_overlap);
            double TanimComp = (double)white_overlap / (double)(A_count + B_count - 2 * black_overlap + white_overlap);
            double image_size = (double)_sDTM.ColumnDimension * _sDTM.RowDimension;
            double p = (double)(A_count + B_count) / (2.0 * image_size);
            // this has magic numbers. Sorry. We didn't make them up. See the image recognition paper. (Link at top)
            double alpha = 0.75 - 0.25 * p;
            double distance = 1.0 - (alpha * Tanim + (1.0 - alpha) * TanimComp);

            // return the Tanimoto Similarity Coefficient
            return new SymbolRank(distance, S);
        }

        /// <summary>
        /// Calculates the Yule Coefficient (or the coefficient of colligation) for the
        /// the calling BitmapSymbol and the BitmapSymbol S
        /// </summary>
        private SymbolRank Yule_Distance(BitmapSymbol S)
        {
            int A_count, B_count, black_overlap, white_overlap;

            Black_White(this, S, out A_count, out B_count, out black_overlap, out white_overlap);

            double lonely_A = A_count - black_overlap; // the number of black pixels in A that do not have a match in B
            double lonely_B = B_count - black_overlap;
            double overlapping = (double)(black_overlap * white_overlap);
            double numerator = overlapping - lonely_A * lonely_B;
            double denominator = overlapping + lonely_A * lonely_B;

            return new SymbolRank(numerator / denominator, S);
        }

        #endregion

        #region Getters & Setters

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ShapeType SymbolType
        {
            get { return _type; }
        }

        public Guid ID
        {
            get { return _id; }
        }

        #endregion

        #region Processing

        private void Process()
        {
            _sMesh = _points.QuantizeScreen(GRID_X_SIZE, GRID_Y_SIZE);
            _screenCoords = IndexList(_sMesh);
            ScreenDistanceTransform();

            _pMesh = _points.QuantizePolar(GRID_X_SIZE, GRID_Y_SIZE);
            _polarCoords = IndexList(_pMesh);
            PolarDistanceTransform();
        }

        /// <summary>
        /// Calculates and saves the distance transform map for the screen coordinates.
        /// Uses _sMesh and saves to _sDTM.
        /// A distance transform map is a matrix in which every entry represents the
        /// distance from that point to the nearest point with ink.
        /// </summary>
        private void ScreenDistanceTransform()
        {
            List<Coord> indices = IndexList(_sMesh);

            _sDTM = new GeneralMatrix(GRID_Y_SIZE, GRID_X_SIZE, 0.0);

            for (int i = 0; i < GRID_Y_SIZE; i++)
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    Coord cp = new Coord(j, i);
                    double mindist = double.PositiveInfinity;

                    foreach (Coord pt in indices)
                    {
                        double distan = cp.distanceTo(pt);
                        if (distan < mindist)
                            mindist = distan;
                    }
                    _sDTM.SetElement(i, j, mindist);
                }
        }

        /// <summary>
        /// Calculates and saves the distance transform map for the polar coordinates.
        /// Uses _pMesh and saves to _pDTM.
        /// A distance transform map is a matrix in which every entry represents the
        /// distance from that point to the nearest point with ink.
        /// </summary>
        private void PolarDistanceTransform()
        {
            List<Coord> indices = IndexList(_pMesh);

            for (int i = 0; i < GRID_Y_SIZE; i++)
                for (int j = 0; j < GRID_X_SIZE; j++)
                {
                    Coord cp = new Coord(j, i);

                    double mindist = double.PositiveInfinity;

                    foreach (Coord pt in indices)
                    {
                        // the straight distance between current point and current index
                        double dx = Math.Abs(cp.X - pt.X);
                        double dy = Math.Abs(cp.Y - pt.Y);
                        double straightDist = Math.Sqrt(dx * dx + dy * dy);

                        dx = (double)GRID_X_SIZE - dx;
                        double wrapDist = Math.Sqrt(dx * dx + dy * dy);

                        double distan = Math.Min(straightDist, wrapDist);

                        if (distan < mindist)
                            mindist = distan;
                    }
                    _pDTM.SetElement(i, j, mindist);
                }

#if FALSE
            Console.WriteLine("Your polar distance transform map:");
            Console.Write(_pDTM.ToString())
#endif
        }

        /// <summary>
        /// Gets a list of coordinates from a general matrix.
        /// 
        /// Any point in the general matrix that has a value greater than
        /// zero will correspond to an entry in the list of coords.
        /// </summary>
        /// <param name="mesh">The GeneralMatrix to create the list from.</param>
        /// <returns>The list of Coords representing the matrix.</returns>
        private List<Coord> IndexList(GeneralMatrix mesh)
        {
            List<Coord> indices = new List<Coord>();
            
            // make a list of the coordinates of the black pixels
            for (int row = 0; row < GRID_Y_SIZE; row++)
                for (int col = 0; col < GRID_X_SIZE; col++)
                    if (mesh.GetElement(row, col) > 0.0)
                        indices.Add(new Coord(col, row));

            return indices;
        }

        #endregion

        #region Other Functions

        private void Rotate(double theta)
        {
#if JESSI
            Console.WriteLine("Rotating your BitmapSymbol by " + theta + " radians.");
#endif
            _points.Rotate(theta);

            Process();
        }

        public System.Drawing.Bitmap toBitmap()
        {
            return _bitmap;
        }

        #endregion

        #region Recognition

        public List<SymbolRank> Recognize(List<BitmapSymbol> defns)
        {
            if (defns.Count == 0)
                throw new Exception("The recognizer does not have any templates!");

            if (defns.Count < NUM_TOP_POLAR_TO_KEEP)
                NUM_TOP_POLAR_TO_KEEP = defns.Count;

            polarRecognition(defns);

#if JESSI
            Console.WriteLine("\nThese templates made it through the polar recognition round:");
            foreach (SymbolRank sr in _results.BestN(ResultType.POLAR, NUM_TOP_POLAR_TO_KEEP))
                Console.WriteLine(sr.SymbolName);
#endif

            screenRecognition();
            combineResults();

#if JESSI
            Console.WriteLine("Your templates have now been reordered by screen recognition:");
            foreach (SymbolRank sr in _results.BestN(ResultType.FUSION, NUM_TOP_POLAR_TO_KEEP))
                Console.WriteLine(sr.SymbolName);
#endif

            return _results.BestN(ResultType.FUSION, NUM_RECOGNITIONS_TO_RETURN);
        }

        private void polarRecognition(List<BitmapSymbol> defns)
        {
            foreach (BitmapSymbol bs in defns)
            {
                SymbolRank polar_result = Polar_Mod_Hausdorff(bs);
                _results.Add(ResultType.POLAR, polar_result);
            }
        }

        private void screenRecognition()
        {
            
#if FALSE // useful for debugging rotation
            List<double> rotateBy = new List<double> { Math.PI/4, Math.PI/2, Math.PI };
            foreach (double theta in rotateBy)
            {
                BitmapSymbol debug = (BitmapSymbol)Clone();
                debug.Rotate(theta);
            }
#endif
            List<SymbolRank> topPolar = _results.BestN(ResultType.POLAR, NUM_TOP_POLAR_TO_KEEP);
            foreach (SymbolRank sr in topPolar)
            {

#if JESSI
                Console.WriteLine("Doing screen recognition for template " + sr.SymbolName);
#endif
                // clone the BitmapSymbol so that we can rotate it without losing information
                BitmapSymbol clone = (BitmapSymbol)Clone();
                clone.Rotate(-sr.BestOrientation);

                // calculate the data using the rotated clone, but store the output in this symbol's results.
                _results.Add(ResultType.PARTIAL_HAUSDORFF, clone.Partial_Hausdorff(sr.Symbol));
                _results.Add(ResultType.MOD_HAUSDORFF, clone.Modified_Hausdorff(sr.Symbol));
                _results.Add(ResultType.TANIMOTO, clone.Tanimoto_Distance(sr.Symbol));
                _results.Add(ResultType.YULE, clone.Yule_Distance(sr.Symbol));
            }
        }

        private void combineResults()
        {
            List<SymbolRank> topPolar = _results.BestN(ResultType.POLAR, NUM_TOP_POLAR_TO_KEEP);
            foreach (SymbolRank sr in topPolar)
            {
                double distance = 0.0;
                SymbolRank part_haus = _results.getSR(ResultType.PARTIAL_HAUSDORFF, sr.Symbol);
                distance += _results.Normalize(ResultType.PARTIAL_HAUSDORFF, part_haus);

                SymbolRank mod_haus = _results.getSR(ResultType.MOD_HAUSDORFF, sr.Symbol);
                distance += _results.Normalize(ResultType.MOD_HAUSDORFF, mod_haus);

                SymbolRank tanim = _results.getSR(ResultType.TANIMOTO, sr.Symbol);
                distance += 1 - _results.Normalize(ResultType.TANIMOTO, tanim);

                SymbolRank yule = _results.getSR(ResultType.YULE, sr.Symbol);
                distance += 1 - _results.Normalize(ResultType.YULE, yule);

                _results.Add(ResultType.FUSION, new SymbolRank(distance, sr.Symbol, sr.BestOrientation));
            }
        }

        /// <summary>
        /// uses polar coordinate matching to find the orientation of a BitmapSymbol
        /// that makes it best match the given BitmapSymbol template.
        /// </summary>
        /// <param name="defn"></param>
        /// <returns></returns>
        public double bestOrientation(BitmapSymbol defn)
        {
            if (defn == null) return 0.0;

#if JESSI
            Console.WriteLine("Using template " + defn._name + " to orient shape.");
#endif

            return Polar_Mod_Hausdorff(defn).BestOrientation;
        }

        #endregion
    }
}