using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sketch
{
    /// <summary>
    /// Stores a pair of strokes.
    /// </summary>
    public class StrokePair : Data.Pair<Substroke, Substroke>
    {

        /// <summary>
        /// Construct a StrokePair with the two given substrokes.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        public StrokePair(Substroke s1, Substroke s2)
            : base(s1, s2)
        {
        }

        /// <summary>
        /// Determine whether the given substroke is in this pair. Uses
        /// strict equality (==) to find it.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Includes(Substroke s)
        {
            if (s == A)
                return true;
            if (s == B)
                return true;
            return false;
        }

    }
}
