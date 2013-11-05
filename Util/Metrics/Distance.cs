namespace Metrics
{
    /// <summary>
    /// An Abstract Distance metric to be inherited from
    /// </summary>
    public abstract class Distance<T>
    {
        protected T m_a, m_b;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Distance() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public Distance(T a, T b)
        {
            m_a = a;
            m_b = b;
        }

        /// <summary>
        /// Compute the distance between two objects of type T
        /// </summary>
        /// <returns></returns>
        public abstract double distance();

        /// <summary>
        /// Compute the distance using method 
        /// </summary>
        /// <returns></returns>
        public abstract double distance(int method);    
    }
}
