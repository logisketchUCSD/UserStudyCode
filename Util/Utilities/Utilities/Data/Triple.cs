using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{

    /// <summary>
    /// Represents an immutable triple of values.
    /// </summary>
    /// <typeparam name="U">the type of the first value</typeparam>
    /// <typeparam name="V">the type of the second value</typeparam>
    /// <typeparam name="W">the type of the third value</typeparam>
    public class Triple<U, V, W>
    {

        private U _a;
        private V _b;
        private W _c;

        /// <summary>
        /// Construct a new triple
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public Triple(U a, V b, W c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        /// <summary>
        /// Get the first value
        /// </summary>
        public U A
        {
            get { return _a; }
        }

        /// <summary>
        /// Get the second value
        /// </summary>
        public V B
        {
            get { return _b; }
        }

        /// <summary>
        /// Get the third value
        /// </summary>
        public W C
        {
            get { return _c; }
        }

        /// <summary>
        /// Two triples are equal if this.A.Equals(other.A) and
        /// this.B.Equals(other.B) and this.C.Equals(other.C).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Triple<U, V, W>))
                return false;

            Triple<U, V, W> other = (Triple<U, V, W>)obj;

            return (A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C));
        }

        /// <summary>
        /// Get a hash code for this triple. Depends on the
        /// hash codes of the constituent objects.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        }

    }
}
