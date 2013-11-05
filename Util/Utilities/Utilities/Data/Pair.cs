using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{

    /// <summary>
    /// Represents an immutable pair of values. As much as possible,
    /// pairs are order-independent. That is, Pair(x,y) should be
    /// equivalent to Pair(y,x).
    /// </summary>
    /// <typeparam name="U">the type of the first value</typeparam>
    /// <typeparam name="V">the type of the second value</typeparam>
    public class Pair<U, V>
    {

        private readonly U _a;
        private readonly V _b;

        /// <summary>
        /// Construct a new pair
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public Pair(U a, V b)
        {
            _a = a;
            _b = b;
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
        /// Two pairs are equal if they share the same items; that is,
        /// if this.A.Equals(other.A) and this.B.Equals(other.B)
        /// or this.A.Equals(other.B) and this.B.Equals(other.A)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Pair<U, V>)
            {
                Pair<U, V> other = (Pair<U, V>)obj;
                return
                    (A.Equals(other.A) && B.Equals(other.B)) ||
                    (A.Equals(other.B) && B.Equals(other.A));
            }
            else if (obj is Pair<V, U>)
            {
                Pair<V, U> other = (Pair<V, U>)obj;
                return
                    (A.Equals(other.A) && B.Equals(other.B)) ||
                    (A.Equals(other.B) && B.Equals(other.A));
            }

            return false;
        }

        /// <summary>
        /// Get a hash code for this pair. Depends on the
        /// hash codes of the constituent objects.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }

    }
}
