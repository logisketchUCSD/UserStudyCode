using System;
using System.Collections.Generic;
using System.Text;

using Sketch;

namespace NeighborhoodMap
{
    /// <summary>
    /// Point class that knows what substroke it belongs to.
    /// </summary>
    public class LinkedPoint
    {

        private double x, y;
        private Sketch.Substroke parent;
        private int? index;

        public LinkedPoint(double X, double Y, Sketch.Substroke par)
        {
            this.x = X;
            this.y = Y;
            this.parent = par;
            index = null;
        }

        public LinkedPoint(Substroke par, int index)
        {
            Point p = par.PointsL[index];
            this.x = p.X;
            this.y = p.Y;
            this.parent = par;
            this.index = index;
        }

        /// <summary>
        /// Check if there is a point at this point within dist points of the end of the parent substroke.
        /// </summary>
        /// <param name="dist">the number of points away from the actual endpoint to allow</param>
        /// <returns></returns>
        public bool IsNearEndpoint(int dist)
        {
            for (int i = 0; i < dist; i++)
            {
                if (parent.PointsL[i].X == x && parent.PointsL[i].Y == y)
                    return true;
                if (parent.PointsL[parent.PointsL.Count - (1 + i)].X == x &&
                    parent.PointsL[parent.PointsL.Count - (1 + i)].Y == y)
                    return true;
            }
            return false;
        }

        public void UpdateIndex()
        {
            for (int i = 0; i < parent.PointsL.Count; i++)
            {
                if (parent.PointsL[i].X == x && parent.PointsL[i].Y == y)
                {
                    index = i;
                    return;
                }
            }
            throw new Exception(String.Format("Point ({0},{1}) not found in parent substroke",
                (int)x, (int)y));
        }

        #region GETTERS
        public double X
        {
            get
            {
                return this.x;
            }
        }

        public double Y
        {
            get
            {
                return this.y;
            }
        }

        public Substroke Parent
        {
            get
            {
                return this.parent;
            }
        }

        public Point Point
        {
            get
            {
                if (index == null)
                    UpdateIndex();
                return parent.PointsL[(int)index];
            }
        }
        #endregion

    }

    /// <summary>
    /// Sorter for LinkedPoints
    /// Can sort by X or Y value based on boolean passed in to constructor.
    /// </summary>
    public class LinkedPointSorter : IComparer<LinkedPoint>
    {
        // sort by X if true, Y if false
        bool X;

        public LinkedPointSorter(bool X)
        {
            this.X = X;
        }


        #region IComparer<LinkedPoint> Members

        int IComparer<LinkedPoint>.Compare(LinkedPoint x, LinkedPoint y)
        {
            if (X)
            {
                if (x.X == y.X) return 0;
                return (x.X - y.X > 0) ? 1 : -1;
            }
            else
            {
                if (x.Y == y.Y) return 0;
                return (x.Y - y.Y > 0) ? 1 : -1;
            }
        }

        #endregion
    }
}
