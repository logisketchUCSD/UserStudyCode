/**
 * File: Sketch.cs
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
	/// Sketch class.
	/// </summary>
	public class Sketch
	{
		#region INTERNALS

		/// <summary>
		/// The Strokes in a sketch
		/// </summary>
		private ArrayList strokes;
		
		/// <summary>
		/// The Shapes in a sketch
		/// </summary>
		private ArrayList shapes;

		/// <summary>
		/// The XML attributes of the Sketch
		/// </summary>
		public XmlStructs.XmlSketchAttrs XmlAttrs;

		#endregion

		#region CONSTRUCTORS
		
		/// <summary>
		/// Construct a blank Sketch
		/// </summary>
		public Sketch() : 
			this(new ArrayList(), new ArrayList(), new XmlStructs.XmlSketchAttrs())
		{
			//Calls the main constructor
		}

		/// <summary>
		/// Create a new Sketch from an old one
		/// </summary>
		/// <param name="sketch">The Sketch to clone</param>
		public Sketch(Sketch sketch) : 
			this((ArrayList)sketch.strokes.Clone(), (ArrayList)sketch.shapes.Clone(), sketch.XmlAttrs.Clone())
		{
			//Calls the main constructor
		}


		/// <summary>
		/// Construct a Sketch with given Strokes and XML attributes.
		/// </summary>
		/// <param name="strokes">The Strokes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(Stroke[] strokes, XmlStructs.XmlSketchAttrs XmlAttrs) : 
			this(new ArrayList(strokes), new ArrayList(), XmlAttrs)
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Construct a Sketch with given Shapes and XML attributes.
		/// </summary>
		/// <param name="shapes">The Shapes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(Shape[] shapes, XmlStructs.XmlSketchAttrs XmlAttrs) : 
			this(new ArrayList(), new ArrayList(shapes), XmlAttrs)
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Construct a Sketch with Strokes, Shapes, and XML attributes.
		/// </summary>
		/// <param name="strokes">The Strokes in the Sketch</param>
		/// <param name="shapes">The Shapes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(Stroke[] strokes, Shape[] shapes, XmlStructs.XmlSketchAttrs XmlAttrs) : 
			this(new ArrayList(strokes), new ArrayList(shapes), XmlAttrs)
		{
			// Calls the main constructor
		}
		

		/// <summary>
		/// Construct a Sketch with Strokes, Shapes, and XML attributes.
		/// </summary>
		/// <param name="strokes">The Strokes in the Sketch</param>
		/// <param name="shapes">The Shapes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(ArrayList strokes, ArrayList shapes, XmlStructs.XmlSketchAttrs XmlAttrs)
		{
			this.strokes = new ArrayList();
			this.shapes = new ArrayList();
			this.XmlAttrs = XmlAttrs;
		
			this.AddStrokes(strokes);
			this.AddShapes(shapes);
		}
		
		
		#endregion

		#region ADD TO SKETCH

		#region ADD STROKE(S)

		/// <summary>
		/// Add a Strokes to the Sketch.
		/// </summary>
		/// <param name="stroke">The Stroke</param>
		public void AddStroke(Stroke stroke)
		{
			if (!this.strokes.Contains(stroke))
			{
				int low = 0;
				int high = this.strokes.Count - 1;
				int mid;
				while (low <= high)
				{
					mid = (high - low) / 2 + low;
					if ((ulong)stroke.XmlAttrs.Time < (ulong)((Stroke)this.strokes[mid]).XmlAttrs.Time)
						high = mid - 1;
					else
						low = mid + 1;
				}
			
				this.strokes.Insert(low, stroke);
			}		
		}

		
		/// <summary>
		/// Add Strokes to the Sketch.
		/// </summary>
		/// <param name="strokes">The Strokes</param>
		public void AddStrokes(Stroke[] strokes)
		{
			int length = strokes.Length;
			for (int i = 0; i < length; i++)
				this.AddStroke(strokes[i]);
		}

		
		/// <summary>
		/// Add Shapes to the sketch.
		/// </summary>
		/// <param name="strokes">The Strokes</param>
		public void AddStrokes(ArrayList strokes)
		{
			int length = strokes.Count;
			for (int i = 0; i < length; i++)
				this.AddStroke((Stroke)strokes[i]);
		}
		
	
		#endregion

		#region ADD SHAPE(S)

		/// <summary>
		/// Add a Shape to the sketch.
		/// </summary>
		/// <param name="shape">The Shape</param>
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
			}
		}

		
		/// <summary>
		/// Add Shapes to the sketch.
		/// </summary>
		/// <param name="shapes">The Shapes</param>
		public void AddShapes(Shape[] shapes)
		{
			int length = shapes.Length;
			for (int i = 0; i < length; ++i)
				this.AddShape(shapes[i]);
		}

		
		/// <summary>
		/// Add Shapes to the sketch.
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

		#region REMOVE FROM SKETCH

		#region REMOVE STROKE(S)

		/// <summary>
		/// Removes a Stroke from the Sketch.
		/// </summary>
		/// <param name="stroke">Stroke to remove</param>
		/// <returns>True iff Stroke is removed</returns>
		public bool RemoveStroke(Stroke stroke)
		{
			if (this.strokes.Contains(stroke))
			{
				this.strokes.Remove(stroke);
				return true;
			}
			else
			{
				return false;
			}
		}

		
		/// <summary>
		/// Removes an ArrayList of Strokes from the Sketch.
		/// </summary>
		/// <param name="strokes">Strokes to remove</param>
		/// <returns>True iff all Strokes are removed</returns>
		public bool RemoveStrokes(ArrayList strokes)
		{
			bool completelyRemoved = true;

			for (int i = 0; i < strokes.Count; ++i)
			{
				Stroke currStroke = (Stroke)strokes[i];

				if (!RemoveStroke(currStroke))
				{
					completelyRemoved = false;
					Console.WriteLine("Stroke " + currStroke.XmlAttrs.Id + " not removed!");
				}
			}

			return completelyRemoved;
		}

		#endregion
		
		#region REMOVE SHAPE(S)

		/// <summary>
		/// Remove a Shape from the Sketch.
		/// </summary>
		/// <param name="shape">Shape to remove</param>
		/// <returns>True iff the Shape is removed.</returns>
		public bool RemoveShape(Shape shape)
		{
			if (this.shapes.Contains(shape))
			{
				// Update all the Substroke's parents
				for (int i = 0; i < shape.Substrokes.Length; i++)
				{
					shape.Substrokes[i].ParentShapes.Remove(shape);
				}
				
				// Update any Shape's parent
				for (int i = 0; i < shape.Shapes.Length; i++)
				{
					shape.Shapes[i].ParentShape = null;
				}
				
				this.shapes.Remove(shape);
				return true;
			}
			else
			{
				return false;
			}
		}

		
		/// <summary>
		/// Remove an ArrayList of Shapes from the Sketch.
		/// </summary>
		/// <param name="shapes">Shapes to remove</param>
		/// <returns>True iff all Shapes are removed.</returns>
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

		#region REMOVE SUBSTROKE(S)

		/// <summary>
		/// Removes a Substroke from the Sketch.  Removes parent stroke if
		/// this substroke is the only substroke in the stroke.
		/// </summary>
		/// <param name="substroke">Substroke to remove</param>
		/// <returns>True iff Substroke is removed</returns>
		public bool RemoveSubstroke(Substroke substroke)
		{
			Stroke parentStroke = substroke.ParentStroke;
			
			// Remove this substrokes from parent shapes
			// and remove all parent shapes that contain
			// only this substroke
			object[] parentShapes = substroke.ParentShapes.ToArray();
			for (int i = 0; i < parentShapes.Length; ++i)
			{
				if (parentShapes[i] != null)
				{
					Shape currentShape = (Shape) parentShapes[i];
					if (currentShape.Substrokes.Length == 1)
					{
						RemoveShape(currentShape);
					} 
					else
					{
						currentShape.RemoveSubstroke(substroke);
					}
				}
			}

			// Remove the substroke from the parent stroke,
			// or, if the substroke is its parent stroke's only
			// child, remove the parent stroke.
			if (parentStroke.Substrokes.Length == 1)
			{
				return RemoveStroke(parentStroke);
			}
			else
			{
				return parentStroke.RemoveSubstroke(substroke);
			}
		}
		
		/// <summary>
		/// Remove an ArrayList of Substrokes from the Sketch.
		/// </summary>
		/// <param name="shapes">Substrokes to remove</param>
		/// <returns>True iff all Substrokes are removed.</returns>
		public bool RemoveSubstrokes(ArrayList substrokes)
		{
			bool completelyRemoved = true;

			foreach (Substroke currSubstroke in substrokes)
			{
				if (!RemoveSubstroke(currSubstroke))
				{
					completelyRemoved = false;
					Console.WriteLine("Substroke " + currSubstroke.XmlAttrs.Id + " not removed!");
				}
			}

			return completelyRemoved;
		}

		#endregion

		#endregion
		
		#region ADD LABEL

		/// <summary>
		/// Add the substroke into a new, labeled Shape.
		/// </summary>
		/// <param name="substroke">Substroke to be included in the label</param>
		/// <param name="label">Label's string</param>
		/// <returns>The labeled shape added to the Sketch</returns>
		public Shape AddLabel(Substroke substroke, string label)
		{
			ArrayList tempArrayList = new ArrayList();
			tempArrayList.Add(substroke);

			return AddLabel(tempArrayList, label);
		}

		
		/// <summary>
		/// Add the group of substrokes into a new, labeled Shape.
		/// </summary>
		/// <param name="substrokes">Substrokes to be included in the label</param>
		/// <param name="label">Label's string</param>
		/// <returns>The labeled shape added to the Sketch</returns>
		public Shape AddLabel(ArrayList substrokes, string label)
		{
			double probability = -1;
			return AddLabel(substrokes, label, probability);
		}

		
		/// <summary>
		/// Add the substroke into a new, labeled Shape.
		/// </summary>
		/// <param name="substroke">Substroke to be included in the label</param>
		/// <param name="label">Label's string</param>
		/// <param name="probability">Probability of a label's accuracy</param>
		/// <returns>The labeled shape added to the Sketch</returns>
		public Shape AddLabel(Substroke substroke, string label, double probability)
		{
			ArrayList tempArrayList = new ArrayList();
			tempArrayList.Add(substroke);

			return AddLabel(tempArrayList, label, probability);
		}


		/// <summary>
		/// Add the group of substrokes into a new, labeled Shape.
		/// </summary>
		/// <param name="substrokes">Substrokes to be included in the label</param>
		/// <param name="label">Label's string</param>
		/// <param name="probability">Probability of a label's accuracy</param>
		/// <returns>The labeled shape added to the Sketch</returns>
		public Shape AddLabel(ArrayList substrokes, string label, double probability)
		{
			// Create a new labeled shape with our substrokes
			Shape labeled = new Shape();
			labeled.AddSubstrokes(substrokes);

			ulong bestTime = 0;
			ulong toCompare;

			ulong minTime = ulong.MaxValue;

			object start   = null;
			object end     = null;
			object color   = null;
			object raster  = null;
			object pentip  = null;
			object laysink = null;

			Substroke substroke;

			// Loop through all the substrokes
			int length = substrokes.Count;
			for (int i = 0; i < length; ++i)
			{				
				substroke = (Substroke)substrokes[i];
				toCompare = (ulong)substroke.XmlAttrs.Time;
				
				// If we have the last substroke
				if (toCompare > bestTime)
				{
					bestTime = toCompare;
					end = (System.Guid)substroke.XmlAttrs.Id;
				}
				if (toCompare < minTime) // If we have the first substroke
				{
					minTime = toCompare;
					start   = substroke.XmlAttrs.Id;
					color   = substroke.XmlAttrs.Color;
					raster  = substroke.XmlAttrs.Raster;
					pentip  = substroke.XmlAttrs.PenTip;
					laysink = substroke.XmlAttrs.LaysInk;
				}
			}
			
			// Add the information
			labeled.XmlAttrs.Id		  = System.Guid.NewGuid();
			labeled.XmlAttrs.Type	  = "shape";
			labeled.XmlAttrs.Source	  = "Converter";
			labeled.XmlAttrs.Name	  = label;
			labeled.XmlAttrs.Start    = (System.Guid)start;
			labeled.XmlAttrs.End      = (System.Guid)end;
			labeled.XmlAttrs.Time     = bestTime;
			labeled.XmlAttrs.Color    = color;
			labeled.XmlAttrs.Raster   = raster;
			labeled.XmlAttrs.PenTip   = pentip;
			labeled.XmlAttrs.LaysInk  = laysink;
			labeled.XmlAttrs.Control1 = probability;
		
			// Add the label to the Sketch
			this.AddShape(labeled);

			return labeled;
		}

		#endregion
		
		#region REMOVE LABEL

		/// <summary>
		/// Remove a label from the Sketch.
		/// </summary>
		/// <param name="shape">Labeled Shape to remove from the Sketch</param>
		/// <returns>True iff the labeled Shape is removed</returns>
		public bool RemoveLabel(Shape shape)
		{
			return RemoveShape(shape);
		}

		#endregion

		#region GET BY ID

		/// <summary>
		/// Gets the corresponding Shape by its GUID.
		/// </summary>
		/// <param name="shapeId">GUID for the Shape</param>
		/// <returns>The Shape that has the given GUID</returns>
		public Shape GetShape(System.Guid shapeId)
		{
			Shape[] shapes = this.Shapes;
			
			int length = shapes.Length;
			for (int i = 0; i < length; i++)
			{
				if ((System.Guid)shapes[i].XmlAttrs.Id == shapeId)
					return shapes[i];
			}

			return null;
		}

		
		/// <summary>
		/// Gets the corresponding Stroke by its GUID.
		/// </summary>
		/// <param name="strokeId">GUID for the Stroke</param>
		/// <returns>The Stroke that has the given GUID</returns>
		public Stroke GetStroke(System.Guid strokeId)
		{
			Stroke[] strokes = this.Strokes;
			
			int length = strokes.Length;
			for (int i = 0; i < length; i++)
			{
				if ((System.Guid)strokes[i].XmlAttrs.Id == strokeId)
					return strokes[i];
			}
		
			return null;
		}
		
		
		/// <summary>
		/// Gets the corresponding Substroke by its GUID.
		/// </summary>
		/// <param name="substrokeId">GUID for the Substroke</param>
		/// <returns>The Substroke that has the given GUID</returns>
		public Substroke GetSubstroke(System.Guid substrokeId)
		{
			Substroke[] substrokes = this.Substrokes;

			int length = substrokes.Length;
			for (int i = 0; i < length; i++)
			{
				if ((System.Guid)substrokes[i].XmlAttrs.Id == substrokeId)
					return substrokes[i];
			}

			return null;
		}
	
		#endregion

		#region GETTERS & SETTERS
		
		/// <summary>
		/// Get the Label Strings (adds a "unlabeled" as the last one)
		/// </summary>
		public string[] LabelStrings
		{
			get
			{
				ArrayList labels = new ArrayList();
				int length = this.shapes.Count;
				for (int i = 0; i < length; ++i)
				{
					// If it is not "shape" and it hasn't been added
					if (!((string)((Shape)this.shapes[i]).XmlAttrs.Name).ToLower().Equals("shape") && !labels.Contains(((Shape)this.shapes[i]).XmlAttrs.Name))
						labels.Add(((Shape)this.shapes[i]).XmlAttrs.Name);
				}
				labels.Add("unlabeled");
				return (string[])labels.ToArray(typeof(string));
			}
		}

		
		/// <summary>
		/// Get the Shapes of the Sketch
		/// </summary>
		public Shape[] Shapes
		{
			get
			{
				return (Shape[])this.shapes.ToArray(typeof(Shape));
			}
		}


		/// <summary>
		/// Gets the sorted Strokes (ascending based on time) in a Sketch.
		/// </summary>
		public Stroke[] Strokes
		{
			get
			{
				return (Stroke[])this.strokes.ToArray(typeof(Stroke));
			}
		}


		/// <summary>
		/// Gets the sorted Substrokes (ascending based on time) in a Sketch.
		/// </summary>
		public Substroke[] Substrokes
		{
			get
			{
				ArrayList substrokes = new ArrayList();

				int strokesLength = this.strokes.Count;
				int substrokesLength;
				int i;
				int j;
				Substroke[] sstrokes;

				// Loop through all the strokes
				for(i = 0; i < strokesLength; ++i)
				{
					// Get the substrokes of the stroke
					sstrokes = ((Stroke)this.strokes[i]).Substrokes;
					substrokesLength = sstrokes.Length;
					
					// Add all the substrokes
					for(j = 0; j < substrokesLength; ++j)
						substrokes.Add(sstrokes[j]);
				}

				// Sort the Subtrokes in ascending order first.
				//substrokes.Sort();  //Since Strokes are in order, and Substrokes within Strokes are in order, all Substroke should be in order

				return (Substroke[])substrokes.ToArray(typeof(Substroke));
			}
		}


		/// <summary>
		/// Gets the sorted Points (ascending based on time) in a Sketch.
		/// </summary>
		public Point[] Points
		{
			get
			{
				ArrayList points = new ArrayList();

				int strokeLength = this.strokes.Count;
				int pointLength;
				int i;
				int j;
				Point[] ps;

				// Loop through all the strokes
				for(i = 0; i < strokeLength; ++i)
				{
					// Get all the points of the stroke
					ps = ((Stroke)this.strokes[i]).Points;
					pointLength = ps.Length;

					// Add all of the points
					for(j = 0; j < pointLength; ++j)
						points.Add(ps[j]);
				}

				// Sort the Points in ascending order
				//points.Sort();	//Since Stroke are in order, and Points within Strokes are in order, all Points should be in order

				return (Point[])points.ToArray(typeof(Point));
			}
		}
	
		#endregion

		#region OTHER

		/// <summary>
		/// Compute a clone of this Sketch.
		/// </summary>
		/// <returns>The clone of this Sketch.</returns>
		public Sketch Clone()
		{
			return new Sketch(this);
		}

		/// <summary>
		/// Clear all shapes from the sketch
		/// </summary>
		public void RemoveShapes()
		{
			this.shapes.Clear();
		}
		
		/// <summary>
		/// Gets the correctness summary for this Sketch versus another.
		/// Treats this Sketch as the source
		/// </summary>
		/// <param name="sketch"></param>
		/// <param name="withId"></param>
		/// <returns></returns>
		public string CompareToForCorrectness(Sketch sketch, bool withId)
		{
			if (sketch.Substrokes.Length != this.Substrokes.Length)
				return "Not the same content!";
			else
			{
				// Get the combined labels from both source and tester 
				ArrayList myLabels = new ArrayList(this.LabelStrings);
				ArrayList theirLabels = new ArrayList(sketch.LabelStrings);
				
				ArrayList allLabels = new ArrayList();
				for (int i = 0; i < myLabels.Count; ++i)
					if (!allLabels.Contains(myLabels[i]))
						allLabels.Add(myLabels[i]);
				for (int i = 0; i < theirLabels.Count; ++i)
					if (!allLabels.Contains(theirLabels[i]))
						allLabels.Add(theirLabels[i]);

								
				// Set up our counting variables
				int length = allLabels.Count;
				int correct = 0;
				
				int[] labelCount = new int[length];
				int[] myLess = new int[length];		//Amount of index missed
				int[] myMore = new int[length];		//Amount of index mislabeled as that
				
				string toReturn = "";
				string label1, label2;
				int index;

				for (int i = 0; i < this.Substrokes.Length; ++i)
				{
					label1 = this.Substrokes[i].GetFirstLabel();
					label2 = sketch.Substrokes[i].GetFirstLabel();
					
					index = allLabels.IndexOf(label1);
					++labelCount[index];

					// If the two labels are different
					if (!label1.ToLower().Equals(label2.ToLower()))
					{
						// We missed one
						++myLess[index];
						
						// We mislabeled one
						index = allLabels.IndexOf(label2);
						++myMore[index];

						// If ids are active
						if (withId)
							toReturn += this.Substrokes[i].XmlAttrs.Id.ToString() + "\n";
					}
					else
						++correct;
				}

				// Create the summary string

				toReturn += "Overall: " + correct + " / " + this.Substrokes.Length + " = " + (double)correct / this.Substrokes.Length + "\n";
				
				int less;
				int more;
				int count;
				
				for (int i = 0; i < labelCount.Length; ++i)
				{
					more = myMore[i];
					less = myLess[i];
					count = labelCount[i];

					toReturn += (string)allLabels[i] + ": " + (count - less) + " / " + count + " = ";
					if (count > 0)
						toReturn += (double)(count - less) / count;
					else
						toReturn += "XXX";
					toReturn += ",   mislabeled " + more + "\n";
				}

				return toReturn;
			}
		}

		
		#endregion
	}
}
