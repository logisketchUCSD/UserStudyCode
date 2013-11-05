using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using Sketch;

namespace NeighborhoodMap
{
    public class NeighborList : IEnumerable<Neighbor>
    {
        private Dictionary<Substroke, Neighbor> dict;
        List<Substroke> keys;

        public NeighborList()
        {
            dict = new Dictionary<Substroke, Neighbor>();
            keys = new List<Substroke>();
        }

        /// <summary>
        /// Add a point to this NeighborList.
        /// Automatically creates the Neighbor node if needed, otherwise updates
        /// the average intersection point.
        /// </summary>
        /// <param name="owner">owner of the new point</param>
        /// <param name="ownerP">new point</param>
        /// <param name="myP">point in original substroke that matches this new point</param>
        public void addPoint(Substroke owner, System.Drawing.Point ownerP, System.Drawing.Point myP)
        {
            if (!dict.ContainsKey(owner))
            { 
                dict.Add(owner, new Neighbor());
                keys.Add(owner);
            }
            dict[owner].dest.X += (int)(ownerP.X);
            dict[owner].dest.Y += (int)(ownerP.Y);
            dict[owner].src.X += (int)(myP.X);
            dict[owner].src.Y += (int)(myP.Y);
            dict[owner].neighbor = owner;
            dict[owner].num += 1;
        }

        #region GETTERS AND IEnumerable INTERFACE
        public Neighbor this[Substroke key]
        {
            get { return dict[key]; }
        }

        public Neighbor this[int i]
        {
            get { return dict[keys[i]]; }
        }

        public int Count
        {
            get { return dict.Keys.Count; }
        }

        public bool Contains(Substroke s)
        {
            return (keys.Contains(s));
        }

        public IEnumerator<Neighbor> GetEnumerator()
        {
            foreach (Substroke key in dict.Keys)
            {
                yield return dict[key];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public class Neighbor
    {
        public System.Drawing.Point src;
        public System.Drawing.Point dest;
        public int num;
        public Substroke neighbor;
    }
}
