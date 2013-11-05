using System;
using System.Collections.Generic;
using System.Text;

namespace PairedList
{
    /// <summary>
    /// Represents and holds a connection between two items
    /// </summary>
    /// <typeparam name="Ta">Item A (must implement IComparable)</typeparam>
    /// <typeparam name="Tb">Item B</typeparam>
    public class Pair<Ta, Tb> : IComparable<Pair<Ta, Tb>>
    {
        #region INTERNALS

        /// <summary>
        /// Item A
        /// </summary>
        private Ta m_a;
        
        /// <summary>
        /// Item B
        /// </summary>
        private Tb m_b;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">Item A (type must implement IComparable)</param>
        /// <param name="b">Item B</param>
        public Pair(Ta a, Tb b)
        {
            m_a = a;
            m_b = b;
        }

        #endregion

        #region COMPARE

        int System.IComparable<Pair<Ta, Tb>>.CompareTo(Pair<Ta, Tb> pair)
        {
            try
            {
                int ret = ((IComparable<Ta>)m_a).CompareTo(pair.m_a);
                return ret;
            }
            catch
            {
                throw new Exception("Type " + typeof(Ta).ToString() + " does not implement IComparable."); 
            }
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// Get Item A
        /// </summary>
        public Ta ItemA
        {
            get
            {
                return m_a;
            }
        }

        /// <summary>
        /// Get Item B
        /// </summary>
        public Tb ItemB
        {
            get
            {
                return m_b;
            }
        }

        public Pair<Tb, Ta> Reverse
        {
            get
            {
                return new Pair<Tb, Ta>(m_b, m_a);
            }
        }

        #endregion
    }
}
