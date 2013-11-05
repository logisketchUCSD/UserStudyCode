/**
 * File: Shape.cs
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
	/// Shape class.
	/// </summary>
	public class Shape
	{
		#region INTERNALS

		/// <summary>
		/// Substrokes
		/// </summary>
		private ArrayList substrokes;

		/// <summary>
		/// Shapes
		/// </summary>
		private ArrayList shapes;

		/// <summary>
		/// Parent shape
		/// </summary>
		private Shape parentShape;

		/// <summary>
		/// Xml attributes of the Shape
		/// </summary>
		public XmlStructs.XmlShapeAttrs XmlAttrs;

		#endregion

		#region CONSTRUCTORS
		
		/// <summary>
		/// Constructor
		/// </summary>
		public Shape() :
			this(new ArrayList(), new ArrayList(), new XmlStructs.XmlShapeAttrs())
		{
			//Calls the main constructor
		}

		/// <summary>
		/// Constructor. Create from Shape.
		/// </summary>
		/// <param name="shape">A Shape.</param>
		public Shape(Shape shape)
			: this((ArrayList)shape.shapes.Clone(), (ArrayList)shape.substrokes.Clone(), shape.XmlAttrs.Clone())
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Construct a Shape with given Shapes, Substrokes, and XML attributes.
		/// </summary>
		/// <param name="shapes">Shapes to add</param>
		/// <param name="substrokes">Substrokes to add</param>
		/// <param name="XmlAttrs">XML attributes of the Shape</param>
		public Shape(Shape[] shapes, Substroke[] substrokes, XmlStructs.XmlShapeAttrs XmlAttrs)
			: this(new ArrayList(shapes), new ArrayList(substrokes), XmlAttrs)
		{
			// Calls the main constructor
		}

	
		/// <summary>
		/// Construct a Shape with given Shapes, Substrokes, and XML attributes.
		/// </summary>
		/// <param name="shapes">Shapes to add</param>
		/// <param name="substrokes">Substrokes to add</param>
		/// <param name="XmlAttrs">XML attributes of the Shape</param>
		public Shape(ArrayList shapes, ArrayList substrokes, XmlStructs.XmlShapeAttrs XmlAttrs)
		{
			this.shapes = new ArrayList();
			this.substrokes = new ArrayList();
			this.parentShape = null;

			this.XmlAttrs = XmlAttrs;
			this.AddShapes(shapes);
			this.AddSubstrokes(substrokes);
		}

		#endregion

		#region ADD TO SHAPE

		#region ADD SUBSTROKE(S)

		/// <summary>
		/// Add a Substroke to this shape
		/// </summary>
		/// <param name="substroke">A Substroke</param>
		public void AddSubstroke(Substroke substroke)
		{
			if (!this.substrokes.Contains(substroke))
			{		
				int low = 0;
				int high = this.substrokes.Count - 1;
				int mid;
				while (low <= high)
				{
					mid = (high - low) / 2 + low;
					if ((ulong)substroke.XmlAttrs.Time < (ulong)((Substroke)this.substrokes[mid]).XmlAttrs.Time)
						high = mid - 1;
					else
						low = mid + 1;
				}
		
				this.substrokes.Insert(low, substroke); 
				
				if (substroke.ParentShapes != null && !substroke.ParentShapes.Contains(this))
					substroke.ParentShapes.Add(this);

				UpdateSpatialAttributes(this);
			}
		}

		
		/// <summary>
		/// Add Substrokes to this shape
		/// </summary>
		/// <param name="substrokes">The Substrokes</param>
		public void AddSubstrokes(Substroke[] substrokes)
		{
			int length = substrokes.Length;
			for (int i = 0; i < length; ++i)
				this.AddSubstroke(substrokes[i]);
		}

		
		/// <summary>
		/// Add Substrokes to this shape
		/// </summary>
		/// <param name="substrokes">The Substrokes</param>
		public void AddSubstrokes(ArrayList substrokes)
		{
			int length = substrokes.Count;
			for (int i = 0; i < length; ++i)
				this.AddSubstroke((Substroke)substrokes[i]);
		}

		#endregion

		#region ADD SHAPE(S)

		/// <summary>
		/// Add a Shape to this Shape
		/// </summary>
		/// <param name="shape">The Shape to add</param>
		public void AddShape(Shape shape)
		{
			if (!this.shapes.Contains(shape))
			{
				int low = 0;
				int high = this.shapes.Count - 1;
				int mid;
				while (low <= high)
				{
					mid = (high - low) / 2 + low;
					if ((ulong)shape.XmlAttrs.Time < (ulong)((Shape)this.shapes[mid]).XmlAttrs.Time)
						high = mid - 1;
					else
						low = mid + 1;
				}
		
				this.shapes.Insert(low, shape);
				shape.ChangeParentShape(this);

				UpdateSpatialAttributes(this);
			}
		}


		/// <summary>
		/// Add Shapes to this Shape
		/// </summary>
		/// <param name="shapes">The Shapes</param>
		public void AddShapes(Shape[] shapes)
		{
			int length = shapes.Length;
			for (int i = 0; i < length; ++i)
				this.AddShape(shapes[i]);
		}


		/// <summary>
		/// Add Shapes to this Shape
		/// </summary>
		/// <param name="shapes">The Shapes</param>
		public void AddShapes(ArrayList shapes)
		{
			int length = shapes.Count;
			for (int i = 0; i < length; ++i)
				this.AddShape((Shape)shapes[i]);
		}


		#endregion

		#endregion

		#region REMOVE FROM SHAPE

		#region REMOVE SUBSTROKE(S)

		/// <summary>
		/// Removes a Substroke from the Shape.
		/// </summary>
		/// <param name="substroke">Substroke to remove</param>
		/// <returns>True iff the Substroke is removed</returns>
		public bool RemoveSubstroke(Substroke substroke)
		{
			if (this.substrokes.Contains(substroke))
			{
				substroke.ParentShapes.Remove(this);
				this.substrokes.Remove(substroke);
				UpdateSpatialAttributes(this);
				
				return true;
			}
			else
			{
				return false;
			}
		}

		
		/// <summary>
		/// Removes an ArrayList of Substrokes from the Shape.
		/// </summary>
		/// <param name="substrokes">Substrokes to remove</param>
		/// <returns>True iff all Substrokes are removed</returns>
		public bool RemoveSubstrokes(ArrayList substrokes)
		{
			bool completelyRemoved = true;

			for (int i = 0; i < substrokes.Count; ++i)
			{
				Substroke currSubstroke = (Substroke)substrokes[i];

				if (!RemoveSubstroke(currSubstroke))
				{
					completelyRemoved = false;
					Console.WriteLine("Substroke " + currSubstroke.XmlAttrs.Id + " not removed!");
				}
			}

			return completelyRemoved;
		}

		#endregion

		#region REMOVE SHAPE(S)

		/// <summary>
		/// Removes a Shape from the Shape.
		/// </summary>
		/// <param name="shape">Shape to remove</param>
		/// <returns>True iff the Shape is removed</returns>
		public bool RemoveShape(Shape shape)
		{
			if (this.shapes.Contains(shape))
			{
				shape.ParentShape = null;
				this.shapes.Remove(shape);
				UpdateSpatialAttributes(this);
				
				return true;
			}
			else
			{
				return false;
			}
		}

		
		/// <summary>
		/// Removes an ArrayList of Shapes from the Shape.
		/// </summary>
		/// <param name="shapes">Shapes to remove</param>
		/// <returns>True iff all Shapes are removed</returns>
		public bool RemoveShapes(ArrayList shapes)
		{
			bool completelyRemoved = true;

			for (int i = 0; i < shapes.Count; ++i)
			{
				Shape currShape = (Shape)shapes[i];

				if (!RemoveShape(currShape))
				{
					completelyRemoved = false;
					Console.WriteLine("Shape " + currShape.XmlAttrs.Id + " not removed!");
				}
			}

			return completelyRemoved;
		}

		#endregion

		#endregion

		#region UPDATE ATTRIBUTES

		/// <summary>
		/// Updates the spatial attributes of the Shape, such as the origin
		/// and width of the shape
		/// </summary>
		private void UpdateSpatialAttributes(Shape shape)
		{
			double minX = Double.PositiveInfinity;
			double maxX = Double.NegativeInfinity;

			double minY = Double.PositiveInfinity;
			double maxY = Double.NegativeInfinity;

			// Cycle through the Substrokes within the Shape
			foreach (Substroke s in shape.Substrokes)
			{
				minX = Math.Min(minX, Convert.ToDouble(s.XmlAttrs.X));
				minY = Math.Min(minY, Convert.ToDouble(s.XmlAttrs.Y));

				maxX = Math.Max(maxX, Convert.ToDouble(s.XmlAttrs.X) + Convert.ToDouble(s.XmlAttrs.Width));
				maxY = Math.Max(maxY, Convert.ToDouble(s.XmlAttrs.Y) + Convert.ToDouble(s.XmlAttrs.Height));
			}

			// Cycle through the other Shapes within the Shape
			foreach (Shape sh in shape.Shapes)
			{
				if (sh.XmlAttrs.X == null || sh.XmlAttrs.Y == null ||
					sh.XmlAttrs.Width == null || sh.XmlAttrs.Height == null)
				{
					// Since sh is an object, it gets updated when this is called
					UpdateSpatialAttributes(sh);
				}

				minX = Math.Min(minX, Convert.ToDouble(sh.XmlAttrs.X));
				minY = Math.Min(minY, Convert.ToDouble(sh.XmlAttrs.Y));

				maxX = Math.Max(maxX, Convert.ToDouble(sh.XmlAttrs.X) + Convert.ToDouble(sh.XmlAttrs.Width));
				maxY = Math.Max(maxY, Convert.ToDouble(sh.XmlAttrs.Y) + Convert.ToDouble(sh.XmlAttrs.Height));
			}

			// Set the origin at the top-left corner of the shape group
			this.XmlAttrs.X = minX;
			this.XmlAttrs.Y = minY;

			// Set the width and height of the shape
			this.XmlAttrs.Width = maxX - minX;
			this.XmlAttrs.Height = maxY - minY;
		}

		#endregion

		#region GETTERS & SETTERS

		/// <summary>
		/// Get Substrokes
		/// </summary>
		public Substroke[] Substrokes
		{
			get
			{
				return (Substroke[])this.substrokes.ToArray(typeof(Substroke));
			}
		}


		/// <summary>
		/// Get Shapes
		/// </summary>
		public Shape[] Shapes
		{
			get
			{
				return (Shape[])this.shapes.ToArray(typeof(Shape));
			}
		}


		/// <summary>
		/// Get or set ParentShape
		/// </summary>
		internal Shape ParentShape
		{
			get
			{
				if (this.parentShape == null)
					return new Shape();
				else
                    return this.parentShape;
			}

			set
			{
				this.parentShape = value;
			}
		}

		
		#endregion

		#region OTHER

		/// <summary>
		/// Change this parent shape
		/// </summary>
		/// <param name="shape"></param>
		public void ChangeParentShape(Shape shape)
		{
			this.parentShape = shape;
		}


		/// <summary>
		/// Compute the Clone of this shape.
		/// </summary>
		/// <returns>The Clone of this shape.</returns>
		public Shape Clone()
		{
			return new Shape(this);
		}

		#endregion
	}
}
