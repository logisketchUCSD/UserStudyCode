/**
 * File: Point.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;

namespace Sketch
{
	/// <summary>
	/// Point class.
	/// </summary>
	public class Point : IComparable
	{
		#region INTERNALS

		/// <summary>
		/// The XML attributes of the Point
		/// </summary>
		public XmlStructs.XmlPointAttrs XmlAttrs;
		
		#endregion

		#region CONSTRUCTORS
		
		/// <summary>
		/// Constructor
		/// </summary>
		public Point()
			: this(new XmlStructs.XmlPointAttrs())
		{
			//Calls the main constructor
		}

		/// <summary>
		/// Construct a new Point class from an existing Point.
		/// </summary>
		/// <param name="point">Existing Point to copy</param>
		public Point(Point point)
			: this(point.XmlAttrs.Clone())
		{
			// Calls the main constructor
		}
		
		
		/// <summary>
		/// Construct a new Point class from XML attributes.
		/// </summary>
		/// <param name="XmlAttrs">The XML attributes of the Point</param>
		public Point(XmlStructs.XmlPointAttrs XmlAttrs)
		{
			this.XmlAttrs = XmlAttrs;
		}
		
		#endregion

		#region GETTERS & SETTERS

		/// <summary>
		/// A getter for the x-coordinate of the Point.
		/// This is created so we can bypass having to go into the XML Attributes,
		/// because (x,y) coordinates are more closely tied to actual sketch and stroke
		/// attributes than the XML format.
		/// </summary>
		public float X
		{
			get
			{
				return (float)this.XmlAttrs.X;
			}
		}

		
		/// <summary>
		/// A getter for the y-coordinate of the Point.
		/// This is created so we can bypass having to go into the XML Attributes,
		/// because (x,y) coordinates are more closely tied to actual sketch and stroke
		/// attributes than the XML format.
		/// </summary>
		public float Y
		{
			get
			{
				return (float)this.XmlAttrs.Y;
			}
		}


		/// <summary>
		/// Get the ulong Time of this Point.
		/// </summary>
		public ulong Time
		{
			get
			{
				return (ulong)this.XmlAttrs.Time;
			}
		}


		/// <summary>
		/// Get the ushort Pressure of this Point
		/// </summary>
		public ushort Pressure
		{
			get
			{
				return (ushort)this.XmlAttrs.Pressure;
			}
		}
		

		#endregion

		#region OTHER

		/// <summary>
		/// Clone this Point.
		/// </summary>
		/// <returns>A Cloned point.</returns>
		public Point Clone()
		{
			return new Point(this);
		}


		/// <summary>
		/// Compare this Point to another based on time.
		/// Returns less than 0 if this time is less than the other's.
		/// Returns 0 if this time is equal to the other's.
		/// Returns greater than 0 if this time is greater than the other's.
		/// </summary>
		/// <param name="obj">The other Point to compare this one to</param>
		/// <returns>An integer indicating how the Point times compare</returns>
		int System.IComparable.CompareTo(object obj)
		{
			Point other = (Point)obj;

			ulong thisTime  = (ulong)this.XmlAttrs.Time;
			ulong otherTime = (ulong)other.XmlAttrs.Time;

			return (int)(thisTime - otherTime);
		}

		#endregion
	}
}
