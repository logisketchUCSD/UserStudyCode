using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace Featurefy
{
    /// <summary>
    /// A class for the end points of strokes.
    /// </summary>
    [Serializable]
    public class EndPoint
    {
        private Substroke m_Stroke;
        private int m_End;
        private Point m_Pt;
        private List<EndPoint> m_AttachedEndpoints;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stroke"></param>
        /// <param name="end"></param>
        public EndPoint(Substroke stroke, int end)
        {
            m_Stroke = stroke;
            m_End = end;
            if (end == 1)
                m_Pt = m_Stroke.Points[0];
            else if (end == 2)
                m_Pt = m_Stroke.Points[m_Stroke.Points.Length - 1];
            else
                m_Pt = new Point();
            m_AttachedEndpoints = new List<EndPoint>();
        }

        /// <summary>
        /// Attatch two endpoints
        /// </summary>
        /// <param name="endPoint">Endpoint to attatch</param>
        public void AddAttachment(EndPoint endPoint)
        {
            m_AttachedEndpoints.Add(endPoint);
        }

        /// <summary>
        /// Get the associated substroke
        /// </summary>
        public Substroke Stroke
        {
            get { return m_Stroke; }
        }

        /// <summary>
        /// Get the associated point
        /// </summary>
        public Point Point
        {
            get { return m_Pt; }
        }

        /// <summary>
        /// Get the endpoints attatched to this one
        /// </summary>
        public List<EndPoint> Attachments
        {
            get { return m_AttachedEndpoints; }
        }

        /// <summary>
        /// Get the end of this endpoint
        /// </summary>
        public int End
        {
            get { return m_End; }
        }
    }
}
