using System;
using System.Collections.Generic;
using System.Text;

using Sketch;

namespace NeighborhoodMap
{
    public class NearestNeighborMap
    {
        private NeighborhoodMap map;


        public NearestNeighborMap(NeighborhoodMap nap)
        {
            map = nap;
        }

        public ICollection<Substroke> Substrokes
        {
            get { return map.Substrokes; }            
        }

        public Neighbor this[Substroke s]
        {
            get { return null; }
        }

    }
}
