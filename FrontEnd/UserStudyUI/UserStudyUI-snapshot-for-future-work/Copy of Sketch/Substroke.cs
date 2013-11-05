/**
 * File: Substroke.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections;

namespace Sketch
{
	/// <summary>
	/// Substroke class.
	/// </summary>
	public class Substroke : IComparable
	{
		#region INTERNALS

		/// <summary>
		/// The points of the Substroke
		/// </summary>
		private ArrayList points;
		
		/// <summary>
		/// This is the parent stroke
		/// </summary>
		private Stroke parentStroke;

		/// <summary>
		/// This is the parent shape
		/// </summary>
		private ArrayList parentShapes;

		/// <summary>
		/// The XML attributes of the Substroke
		/// </summary>
		public XmlStructs.XmlShapeAttrs XmlAttrs;
		
		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Constructor
		/// </summary>
		public Substroke() 
			: this(new ArrayList(), new XmlStructs.XmlShapeAttrs())
		{
			//Calls the main constructor
		}

	
		/// <summary>
		/// Create a Substroke from an existing substroke
		/// </summary>
		/// <param name="substroke">The substroke to clone</param>
		public Substroke(Substroke substroke)
			: this((ArrayList)substroke.points.Clone(), substroke.XmlAttrs.Clone())
		{
			// Calls the main constructor
		}
		
		
		/// <summary>
		/// Creates a Substroke from an array of Points and the XML attributes
		/// </summary>
		/// <param name="pts">Array of Points</param>
		/// <param name="XmlAttrs">The XML attributes of the stroke</param>
		public Substroke(Point[] pts, XmlStructs.XmlShapeAttrs XmlAttrs)
			: this(new ArrayList(pts), XmlAttrs)
		{
			// Calls the main constructor
		}

		
		/// <summary>
		/// Creates a Substroke from an ArrayList of Points and the XML attributes
		/// </summary>
		/// <param name="pts">ArrayList of Points</param>
		/// <param name="XmlAttrs">The XML attributes of the stroke</param>
		public Substroke(ArrayList pts, XmlStructs.XmlShapeAttrs XmlAttrs)
		{
			this.points = new ArrayList();	
			this.parentShapes = new ArrayList();
			this.parentStroke = null;

			this.XmlAttrs = XmlAttrs;
			this.AddPoints(pts);
		}
		
		
		#endregion
		
		#region ADD TO SUBSTROKE
		
		/// <summary>
		/// Adds the Point to the Substroke's Point ArrayList. Uses binary search to find insertion spot
		/// </summary>
		/// <param name="point">Point to add</param>
		public void AddPoint(Point point)
		{
			//if (this.points.Contains(point))
			//	this.points.Remove(point);

			if (!this.points.Contains(point))
			{
				int low = 0;
				int high = this.points.Count - 1;
				int mid;
				while (low <= high)
				{
					mid = (high - low) / 2 + low;
					if ((ulong)point.XmlAttrs.Time < (ulong)((Point)this.points[mid]).XmlAttrs.Time)
						high = mid - 1;
					else
						low = mid + 1;
				}
		
				this.points.Insert(low, point);
			}
		}


		/// <summary>
		/// Adds the Points to the Substroke's Point ArrayList.
		/// </summary>
		/// <param name="points">Points to add</param>
		public void AddPoints(Point[] points)
		{
			int length = points.Length;
			
			for (int i = 0; i < length; i++)
				this.AddPoint(points[i]);
		}

		
		/// <summary>
		/// Adds the Points to the Substroke's Point ArrayList.
		/// </summary>
		/// <param name="points">Points to add</param>
		public void AddPoints(ArrayList points)
		{
			int length = points.Count;
			
			for (int i = 0; i < length; i++)
				this.AddPoint((Point)points[i]);
		}

		#endregion

		#region REMOVE FROM SUBSTROKE

		/// <summary>
		/// Deletes a range of Points from the Substroke.
		/// </summary>
		/// <param name="index">Index of the initial Point to delete</param>
		/// <param name="count">How many to remove</param>
		public void DeleteRange(int index, int count)
		{
			this.points.RemoveRange(index, count);
		}

		#endregion

		#region SPLITTERS
		
		/// <summary>
		/// Splits a Substroke at the given index.
		/// </summary>
		/// <param name="index">Index where the Substroke should be split</param>
		public void SplitAt(int index)
		{
			// If there is nothing to split...
			if (index < 1 || index > this.points.Count - 2)
				return;
			
			Substroke lastHalf = new Substroke();

			// We invalidate these things
			this.XmlAttrs.Height = null;
			this.XmlAttrs.Width  = null;
			this.XmlAttrs.Area   = null;
			this.XmlAttrs.LeftX  = null;
			this.XmlAttrs.TopY   = null;
			this.XmlAttrs.X      = null;
			this.XmlAttrs.Y      = null;

			// Copy the rest of the attributes into the second half
			lastHalf.XmlAttrs = this.XmlAttrs.Clone();
			lastHalf.XmlAttrs.Id = System.Guid.NewGuid();

			int length = points.Count;

			// Add points to the last Half
			lastHalf.AddPoints(points.GetRange(index, length - index)); 

			// Change the start of the last half
			lastHalf.XmlAttrs.Start = ((Point)lastHalf.points[0]).XmlAttrs.Id;

			// Remove all the points in the second half
			this.points.RemoveRange(index, length - index);
			//this.points.RemoveRange(index + 1, length - index - 1);	// Used if we want to include the middle, split point in both Substrokes
			
			// Add the end to the first half
			this.XmlAttrs.End = ((Point)this.points[index - 1]).XmlAttrs.Id;

			// The new first half time is equal to the last point's time in it
			this.XmlAttrs.Time = ((Point)this.points[index - 1]).XmlAttrs.Time;

			// Update the parent info
			lastHalf.ParentStroke = this.parentStroke;
			lastHalf.ParentStroke.AddSubstroke(lastHalf);

			lastHalf.ParentShapes = this.parentShapes;
			
			length = lastHalf.parentShapes.Count;
			for (int i = 0; i < length; ++i)
				if (lastHalf.parentShapes[i] != null)
					((Shape)lastHalf.parentShapes[i]).AddSubstroke(lastHalf);
		}


		/// <summary>
		/// Splits a Substroke at the given indices.
		/// </summary>
		/// <param name="indices">Indices where the Substroke should be split</param>
		public void SplitAt(int[] indices)
		{
			// Due to the way that we insert substrokes into the array, we must step through backwards...
			Array.Sort(indices);
			Array.Reverse(indices);

			int length = indices.Length;
			for (int i = 0; i < length; i++)
				this.SplitAt(indices[i]);
		}

		#endregion

		#region GETTERS & SETTERS
		
		/// <summary>
		/// Get the number of Points in this substroke
		/// </summary>
		public int Length
		{
			get
			{
				return this.points.Count;
			}
		}


		/// <summary>
		/// Gets a Point[] of the points contained in the Substroke.
		/// </summary>
		public Point[] Points
		{
			get
			{
				return (Point[])this.points.ToArray(typeof(Point));
			}
		}


		/// <summary>
		/// Gets an ArrayList of Point of the points contained in the Substroke.
		/// </summary>
		public ArrayList PointsAL
		{
			get
			{
				return this.points;
			}
		}


		/// <summary>
		/// Get or set ParentStroke
		/// </summary>
		public Stroke ParentStroke
		{
			get
			{
				return this.parentStroke;
			}

			set
			{
				this.parentStroke = value;
			}
		}


		/// <summary>
		/// Get or set ParentShape
		/// </summary>
		public ArrayList ParentShapes
		{
			get
			{
				return this.parentShapes;
			}

			set
			{
				this.parentShapes = value;
			}
		}


		/// <summary>
		/// Get the labels associated with a Substroke.
		/// </summary>
		/// <returns></returns>
		public string[] GetLabels()
		{
			ArrayList labels = new ArrayList();

			Shape shape;
			int length = this.ParentShapes.Count;
			for (int i = 0; i < length; ++i)
			{
				shape = (Shape)this.ParentShapes[i];
				
				if (!labels.Contains(shape.XmlAttrs.Name))
					labels.Add(shape.XmlAttrs.Name);
			}
			
			return (string[])labels.ToArray(typeof(string));
		}


		/// <summary>
		/// Get the beliefs associated with the labels
		/// </summary>
		/// <returns></returns>
		public double[] GetBeliefs()
		{
			ArrayList labels = new ArrayList();
			ArrayList beliefs = new ArrayList();

			Shape shape;
			int length = this.ParentShapes.Count;
			for (int i = 0; i < length; ++i)
			{
				shape = (Shape)this.ParentShapes[i];
				
				if (!labels.Contains(shape.XmlAttrs.Name))
				{
					labels.Add(shape.XmlAttrs.Name);
					beliefs.Add(shape.XmlAttrs.Control1);
				}
			}
			
			return (double[])beliefs.ToArray(typeof(double));
		}

		/// <summary>
		/// Get the first label
		/// </summary>
		/// <returns></returns>
		public string GetFirstLabel()
		{
			if (this.parentShapes.Count == 0)
				return "unlabeled";
			else
				return (string)((Shape)this.parentShapes[0]).XmlAttrs.Name;
		}

		/// <summary>
		/// Get the first probability
		/// </summary>
		/// <returns></returns>
		public double GetFirstBelief()
		{
			if(this.parentShapes == null || this.parentShapes.Count == 0)
				return -1.0;
			else
			{
				if(((Shape)this.parentShapes[0]).XmlAttrs.Control1 == null)
					return -1.0;
				else
					return (double)((Shape)this.parentShapes[0]).XmlAttrs.Control1;
			}
		}

		#endregion

		#region OTHER
		
		/// <summary>
		/// Clone this Substroke
		/// </summary>
		/// <returns>The cloned Substroke</returns>
		public Substroke Clone()
		{
			return new Substroke(this);
		}


		/// <summary>
		/// Compare this Substroke to another based on time.
		/// Returns less than 0 if this time is less than the other's.
		/// Returns 0 if this time is equal to the other's.
		/// Returns greater than 0 if this time is greater than the other's.
		/// </summary>
		/// <param name="obj">The other Substroke to compare this one to</param>
		/// <returns>An integer indicating how the Substroke times compare</returns>
		int System.IComparable.CompareTo(Object obj)
		{
			Substroke other = (Substroke)obj;

			ulong thisTime  = (ulong)this.XmlAttrs.Time;
			ulong otherTime = (ulong)other.XmlAttrs.Time;

			return (int)(thisTime - otherTime);
		}
		
		#endregion
	}
}
