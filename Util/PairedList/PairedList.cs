using System;
using System.Collections.Generic;

namespace PairedList
{
    /// <summary>
    /// A List that can sort by both types
    /// </summary>
    /// <typeparam name="Ta">Type A (must implement IComparable)</typeparam>
    /// <typeparam name="Tb">Type B (mist implement IComparable)</typeparam>
    public class PairedList<Ta, Tb>
    {
        #region INTERNALS

        /// <summary>
        /// List A
        /// </summary>
        private List<Pair<Ta, Tb>> m_a;

        /// <summary>
        /// List B
        /// </summary>
        private List<Pair<Tb, Ta>> m_b;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PairedList()
            : this(5) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public PairedList(int capacity)
        {
            m_a = new List<Pair<Ta, Tb>>(capacity);
            m_b = new List<Pair<Tb, Ta>>(capacity);
        }

        #endregion

        #region MODIFIERS

        /// <summary>
        /// Add a new pair
        /// </summary>
        /// <param name="a">Item A</param>
        /// <param name="b">Item B</param>
        public void Add(Ta a, Tb b)
        {
            m_a.Add(new Pair<Ta, Tb>(a, b));
            m_b.Add(new Pair<Tb, Ta>(b, a));
        }

        /// <summary>
        /// Sort both by Type A and Type B
        /// </summary>
        public void Sort()
        {
            m_a.Sort();
            m_b.Sort();
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// Get the list that is sortable by Type A
        /// </summary>
        public List<Pair<Ta, Tb>> ListA
        {
            get
            {
                return m_a;
            }
        }

        /// <summary>
        /// Get the list that is sortable by Type B
        /// </summary>
        public List<Pair<Tb, Ta>> ListB
        {
            get
            {
                return m_b;
            }
        }

        #endregion
    }
}
