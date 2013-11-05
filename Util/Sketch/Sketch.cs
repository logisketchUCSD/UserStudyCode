/*
 * File: Sketch.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, Max Pflueger, and James Brown
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006, 2008.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using Domain;
using System;
using System.Collections.Generic;
using Data;
using System.Diagnostics;

namespace Sketch
{
	/// <summary>
	/// Sketch class.
	/// </summary>
	[Serializable]
	public class Sketch
	{
		#region INTERNALS

		/// <summary>
		/// The Strokes in a sketch
		/// </summary>
		private List<Stroke> _strokes;
		
		/// <summary>
		/// The Shapes in a sketch
		/// </summary>
		private List<Shape> _shapes;

		/// <summary>
		/// The XML attributes of the Sketch
		/// </summary>
		private XmlStructs.XmlSketchAttrs _xmlAttributes;

		#endregion

		#region CONSTRUCTORS
		
		/// <summary>
		/// Construct a blank Sketch
		/// </summary>
		public Sketch() : 
			this(new List<Stroke>(), new List<Shape>(), XmlStructs.XmlSketchAttrs.CreateNew())
		{
			//Calls the main constructor
		}

		/// <summary>
		/// Create a new Sketch from an old one (creates a deep-copy)
		/// </summary>
		/// <param name="sketch">The Sketch to clone</param>
		public Sketch(Sketch sketch)
		{

            sketch.CheckConsistency();

			List<Substroke> newSubstrokes = new List<Substroke>(sketch.SubstrokesL.Count);
			List<Shape> newShapes = new List<Shape>(sketch.Shapes.Length);
			List<Stroke> newStrokes = new List<Stroke>(sketch.Strokes.Length);
			foreach (Substroke ss in sketch.SubstrokesL)
			{
				newSubstrokes.Add(ss.CloneConstruct());
			}
			foreach (Shape originalShape in sketch._shapes)
			{
				List<Substroke> subShapes = new List<Substroke>(originalShape.SubstrokesL.Count);
				foreach (Substroke originalSS in originalShape.SubstrokesL)
				{
					Substroke toAdd = null;
					foreach (Substroke newSS in newSubstrokes)
					{
						if (newSS.Equals(originalSS))
						{
							toAdd = newSS;
							break;
						}
					}
                    subShapes.Add(toAdd);
				}
				newShapes.Add(new Shape(subShapes, originalShape.XmlAttrs.Clone()));
			}
			foreach (Stroke originalStroke in sketch._strokes)
			{
				List<Substroke> subStrokes = new List<Substroke>(originalStroke.Substrokes.Length);
				foreach (Substroke originalSS in originalStroke.Substrokes)
				{
					Substroke toAdd = null;
					foreach (Substroke newSS in newSubstrokes)
					{
						if (newSS.Equals(originalSS))
						{
							toAdd = newSS;
							break;
						}
					}
                    subStrokes.Add(toAdd);
				}
				newStrokes.Add(new Stroke(subStrokes, originalStroke.XmlAttrs.Clone()));
			}


			_strokes = new List<Stroke>(sketch._strokes.Count);
			_shapes = new List<Shape>(sketch._shapes.Count);
			foreach (Stroke s in newStrokes)
				AddStroke(s);
			foreach (Shape sp in newShapes)
				AddShape(sp);

			_xmlAttributes =  sketch._xmlAttributes.Clone();

            CheckConsistency();

		}


		/// <summary>
		/// Construct a Sketch with given Strokes and XML attributes.
		/// </summary>
		/// <param name="strokes">The Strokes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(Stroke[] strokes, XmlStructs.XmlSketchAttrs XmlAttrs) : 
			this(new List<Stroke>(strokes), new List<Shape>(), XmlAttrs)
		{
			// Calls the main constructor
		}


		/// <summary>
		/// Construct a Sketch with given Shapes and XML attributes.
		/// </summary>
		/// <param name="shapes">The Shapes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(Shape[] shapes, XmlStructs.XmlSketchAttrs XmlAttrs) : 
			this(new List<Stroke>(), new List<Shape>(shapes), XmlAttrs)
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
			this(new List<Stroke>(strokes), new List<Shape>(shapes), XmlAttrs)
		{
			// Calls the main constructor
		}
		

		/// <summary>
		/// Construct a Sketch with Strokes, Shapes, and XML attributes (main constructor)
		/// </summary>
		/// <param name="strokes">The Strokes in the Sketch</param>
		/// <param name="shapes">The Shapes in the Sketch</param>
		/// <param name="XmlAttrs">XML attributes of the Sketch</param>
		public Sketch(List<Stroke> strokes, List<Shape> shapes, XmlStructs.XmlSketchAttrs XmlAttrs)
		{
			this._strokes = new List<Stroke>();
			this._shapes = new List<Shape>();
			this._xmlAttributes = XmlAttrs;
		
			this.AddStrokes(strokes);
			this.AddShapes(shapes);
		}
		
		
		#endregion

		#region ADD TO SKETCH

		#region ADD STROKE(S)

		/// <summary>
		/// Add a Stroke to the Sketch. If the stroke is already in this sketch, this
        /// method does nothing.
		/// </summary>
		/// <param name="stroke">The Stroke</param>
		public void AddStroke(Stroke stroke)
		{
            if (!this._strokes.Contains(stroke))
            {
                int low = 0;
                int high = this._strokes.Count - 1;
                int mid;
                while (low <= high)
                {
                    mid = (high - low) / 2 + low;
                    if (stroke.XmlAttrs.Time.Value < this._strokes[mid].XmlAttrs.Time.Value)
                        high = mid - 1;
                    else
                        low = mid + 1;
                }

                this._strokes.Insert(low, stroke);
            }
		}

		/// <summary>
		/// Add Strokes to the Sketch.
		/// </summary>
		/// <param name="strokes">The Strokes</param>
		public void AddStrokes(IEnumerable<Stroke> strokes)
		{
			foreach (Stroke stroke in strokes)
				AddStroke(stroke);
		}
		
		#endregion

		#region ADD SHAPE(S)

		/// <summary>
		/// Add a Shape to the sketch. Make sure that the substrokes are already added.
		/// </summary>
		/// <param name="shape">The Shape</param>
		public void AddShape(Shape shape)
		{
			if (!this._shapes.Contains(shape) && shape.Substrokes.Length > 0)
			{
				int low = 0;
				int high = this._shapes.Count - 1;
				int mid;
				while (low <= high)
				{
					mid = (high - low) / 2 + low;
                    if (shape.XmlAttrs.Time < ((Shape)this._shapes[mid]).XmlAttrs.Time)
						high = mid - 1;
					else
						low = mid + 1;
				}
		
				this._shapes.Insert(low, shape);

                Substroke[] substrokes = shape.Substrokes;
                Substroke sub;
                int len = substrokes.Length;
                int i;
                bool canAdd;
                for (i = 0; i < len; ++i)
                {
                    canAdd = true;
                    sub = substrokes[i];
                    Stroke correctSub = null;
                    foreach (Stroke stroke in this._strokes)
                        foreach (Substroke sstroke in stroke.Substrokes)
                            if (sstroke.Equals(sub))
                            {
                                canAdd = false;
                                correctSub = stroke;
                            }

                    if (canAdd)
                    {
                        AddStroke(new Stroke(sub));
                    }
                    else
                    {
                        XmlStructs.XmlShapeAttrs attributes = correctSub.XmlAttrs.Clone();
                        if (correctSub != null)
                            RemoveStroke(correctSub);
                        AddStroke(new Stroke(new List<Substroke>{sub}, attributes));
                    }
                }
			}
		}


        /// <summary>
        /// Add Shapes to the sketch.
        /// </summary>
        /// <param name="shapes">The Shapes</param>
        public void AddShapes(IEnumerable<Shape> shapes)
		{
            foreach (Shape shape in shapes)
                AddShape(shape);
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
		private bool RemoveStroke(Stroke stroke)
		{
			if (_strokes.Contains(stroke))
            {
                _strokes.Remove(stroke);
				return true;
			}
			else
			{
				return false;
			}
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
            CheckConsistency();

            Shape parentShape = substroke.ParentShape;
            if (parentShape != null)
            {
                parentShape.RemoveSubstroke(substroke);
                if (parentShape.IsEmpty)
                    RemoveShape(parentShape);
            }

            Stroke parentStroke = substroke.ParentStroke;

            // Remove the substroke from the parent stroke,
            // or, if the substroke is its parent stroke's only
            // child, remove the parent stroke.
            if (parentStroke != null)
            {
                bool success;
                if (parentStroke.Substrokes.Length == 1)
                    success = RemoveStroke(parentStroke);
                else
                    success = parentStroke.RemoveSubstroke(substroke);

                CheckConsistency();
                return success;
            }
            else
                throw new Exception("Substroke being removed does not have a parent stroke!");
        }

        /// <summary>
        /// Remove an List of Substrokes from the Sketch.
        /// </summary>
        /// <param name="substrokes">Substrokes to remove</param>
        /// <returns>True iff all Substrokes are removed.</returns>
        public bool RemoveSubstrokes(IEnumerable<Substroke> substrokes)
        {
            bool completelyRemoved = true;

            foreach (Substroke currSubstroke in substrokes)
            {
                bool success = RemoveSubstroke(currSubstroke);
                if (!success)
                    completelyRemoved = false;
            }

            return completelyRemoved;
        }

        #endregion
		
		#region REMOVE SHAPE(S)

        /// <summary>
        /// Remove a Shape from the Sketch.
        /// 
        /// This method can fail if "shape" is not already in
        /// this sketch.
        /// </summary>
        /// <param name="shape">Shape to remove</param>
        /// <returns>True iff the Shape is successfully removed.</returns>
        public bool RemoveShape(Shape shape)
		{
			if (_shapes.Contains(shape))
			{
                // Remove this shape from the neighbors lists.
                List<Shape> connected = new List<Shape>(shape.ConnectedShapes);
                foreach (Shape shape2 in connected)
                    disconnectShapes(shape2, shape);

				// Update all the Substrokes's parents
                Substroke[] subs = shape.Substrokes;
                int len = subs.Length;
				for (int i = 0; i < len; ++i)
				{
                    subs[i].ParentShape = null;
				}
				
				_shapes.Remove(shape);

                CheckConsistency();

				return true;
			}
			else
			{
				return false;
			}
		}

		
		/// <summary>
		/// Remove an List of Shapes from the Sketch.
		/// </summary>
		/// <param name="shapes">Shapes to remove</param>
		/// <returns>True iff all Shapes are removed. Some shapes may be removed even if this method returns false.</returns>
		public bool RemoveShapes(IEnumerable<Shape> shapes)
        {
            CheckConsistency();
			bool completelyRemoved = true;

            foreach (Shape currShape in shapes)
			{
				if (!RemoveShape(currShape))
				{
					completelyRemoved = false;
                    Console.WriteLine("Shape " + currShape.Id + " not removed!");
				}
            }
            CheckConsistency();

			return completelyRemoved;
		}

		#endregion

        /// <summary>
        /// Removes all shapes from the sketch.
        /// </summary>
        public void clearShapes()
        {
            // We need to duplicate the list of shapes to avoid
            // modifying the list of shapes while iterating over it.
            RemoveShapes(new List<Shape>(_shapes));
        }

		#endregion

        #region ANALYZE SKETCH CONTENTS
        
        /// <summary>
        /// Finds the minimum distance between two shapes
        /// </summary>
        public double minDistance(Shape shape1, Shape shape2)
        {
            double minDistance = double.PositiveInfinity;

            foreach (Point point1 in shape1.Points)
                foreach (Point point2 in shape2.Points)
                {
                    double distance = point1.distance(point2);
                    if (distance < minDistance)
                        minDistance = distance;
                }
            return minDistance;
        }

        /// <summary>
        /// Finds all the shapes that are close to a given shape, within a threshold distance. 
        /// 
        /// NOTE: this might be a useless function, because I just discovered 
        ///       that Shapes have a data member called connectedShapes - a list
        ///       of all the shapes connected to another shape...
        /// </summary>
        /// <param name="shape"> The shape we're trying to find neighboring shapres for </param> 
        /// <returns> A list of neighboring shapes</returns>
        public List<Shape> neighboringShapes(Shape shape)
        {
          //int THRESHOLD_DISTANCE = 1000;    // NOTE: this value was guestimated / more or less pulled out of the air
                                              // We're probably better off calculating some distance based on the shape's
                                              // size, rather than some static number to account for small or large
                                              // sketches.
            float thresholdDistance = ((float)(shape.XmlAttrs.Height) + (float)(shape.XmlAttrs.Width)) / 2F;
            List<Shape> neighbors = new List<Shape>();

            foreach (Shape candidateShape in _shapes)
            {
                if ((!candidateShape.Equals(shape))&& (!neighbors.Contains(candidateShape)))
                {
                    double distance = minDistance(shape, candidateShape);
                    if (distance < thresholdDistance)
                    {
                        neighbors.Add(candidateShape);
                    }
                }
            }
            return neighbors;
        }


        #endregion

        #region Modify Shapes

        /// <summary>
        /// Make every substroke of the given shape into a new shape. Deletes
        /// the given shape from the sketch and adds the new shapes instead.
        /// 
        /// Postconditions:
        ///    - the type of every new shape is the same as the type of the original
        ///    - the new shapes have no connections
        ///    - every new shape consists of exactly one substroke
        ///    - the new shapes are all part of this sketch
        ///    - the given shape is empty and is not part of this sketch
        /// </summary>
        /// <param name="shape">the shape to explode</param>
        /// <returns>the list of new shapes</returns>
        public List<Shape> ExplodeShape(Shape shape)
        {

            CheckConsistency();

            List<Shape> newShapes = new List<Shape>();
            List<Substroke> substrokes = new List<Substroke>(shape.Substrokes);

            foreach (Substroke substroke in substrokes)
            {
                Shape newShape = BreakOffShape(shape, substroke);
                newShapes.Add(newShape);
            }

            CheckConsistency();

            return newShapes;

        }

        /// <summary>
        /// Merges the two shapes into one shape (merges shape 2 into shape 1).
        /// This method is extremely strange, make sure you understand it!
        /// 
        /// Preconditions: 
        ///    - shape1 and shape2 are both shapes in this sketch
        ///    - shape1 != shape2
        ///    
        /// Postconditions:
        ///    - shape2 is no longer in this sketch
        ///    - shape1 contains all of shape2's substrokes
        ///    - shape2's substrokes no longer list shape2 as a parent
        ///    - all shapes in this sketch that referenced shape2 as a 
        ///      connected shape now reference shape1
        ///    - if shape1 and shape2 were connected, then shape1 is connected 
        ///      to itself if and only if an endpoint of shape1 was connected
        ///      to shape2 or vice-versa.
        /// </summary>
        /// <param name="shape1">The shape to merge into.</param>
        /// <param name="shape2">The shape being merged.</param>
        /// <returns>The resultant merged shape.</returns>
        public Shape mergeShapes(Shape shape1, Shape shape2)
        {
            if (!_shapes.Contains(shape1))
                throw new ArgumentException("The shape " + shape1 + " is not a part of this sketch");
            if (!_shapes.Contains(shape2))
                throw new ArgumentException("The shape " + shape2 + " is not a part of this sketch");
            if (shape1 == shape2)
                throw new ArgumentException("Cannot merge a shape with itself!");

            CheckConsistency();
            
            // Determine whether shape1 will have internal connections when we are finished
            bool shouldConnectShape1ToItself = false;
            foreach (EndPoint endpoint in shape1.Endpoints)
            {
                if (endpoint.ConnectedShape == shape2)
                {
                    endpoint.ConnectedShape = shape1; // make sure this endpoint gets updated
                    shouldConnectShape1ToItself = true;
                    break;
                }
            }
            foreach (EndPoint endpoint in shape2.Endpoints)
            {
                if (endpoint.ConnectedShape == shape1)
                {
                    shouldConnectShape1ToItself = true;
                    break;
                }
            }
            if (AreConnected(shape1, shape1))
                shouldConnectShape1ToItself = true;

            // Save old info
            List<Substroke> substrokes = new List<Substroke>(shape2.SubstrokesL);
            List<Shape> connections = new List<Shape>(shape2.ConnectedShapes);

            // Remove substrokes from shape 2
            shape2.RemoveSubstrokes(substrokes);

            // Add them to shape 1
            shape1.AddSubstrokes(substrokes);

            // Update connections
            foreach (Shape connectedShape in connections)
            {
                // connectedShape's endpoint connections need to be updated
                foreach (EndPoint endpoint in connectedShape.Endpoints)
                {
                    if (endpoint.ConnectedShape == shape2)
                        endpoint.ConnectedShape = shape1;
                }

                // add a connectedShape <-> shape1 connection
                connectShapes(shape1, connectedShape);

                // remove the connectedShape <-> shape2 connection
                disconnectShapes(shape2, connectedShape);
            }

            // Remove shape2
            RemoveShape(shape2);

            // Connect shape1 to itself if we have internal connections
            if (shouldConnectShape1ToItself)
                connectShapes(shape1, shape1);
            else
                disconnectShapes(shape1, shape1);

            CheckConsistency();

            return shape1;
        }

        /// <summary>
        /// Break a single substroke off a shape. See BreakOffShape(Shape, IEnumerable).
        /// </summary>
        /// <param name="parentShape"></param>
        /// <param name="substroke"></param>
        /// <returns></returns>
        public Shape BreakOffShape(Shape parentShape, Substroke substroke)
        {
            return BreakOffShape(parentShape, new Substroke[] { substroke });
        }

        /// <summary>
        /// Removes all the given substrokes from a given (parent) shape,
        /// and forms them into a new (child) shape.
        /// 
        /// Preconditions: 
        ///    - The substrokes are actually inside the parent shape.
        ///    
        /// Postconditions:
        ///    - The type of the new shape is the same as the type of the old shape
        ///    - The new shape is part of this sketch
        ///    - The old shape is part of this sketch only if it is nonempty
        /// </summary>
        /// <param name="parentShape">Shape to remove substrokes from</param>
        /// <param name="childSubstrokes">Substrokes to break off</param>
        /// <returns>The child shape</returns>
        public Shape BreakOffShape(Shape parentShape, IEnumerable<Substroke> childSubstrokes)
        {

            CheckConsistency();

            Shape childShape = new Shape();

            foreach (Substroke substroke in childSubstrokes)
            {
                parentShape.RemoveSubstroke(substroke);
                childShape.AddSubstroke(substroke);
            }

            childShape.Type = parentShape.Type;

            AddShape(childShape);

            if (parentShape.IsEmpty)
                RemoveShape(parentShape);

            CheckConsistency();

            return childShape;
        }

        /// <summary>
        /// Resets the shapes of the sketch. Puts every substroke into its own shape, except
        /// it does not split up shapes for which AlreadyGrouped is true.
        /// 
        /// Connections between shapes are kept if both shapes are AlreadyGrouped. Otherwise
        /// connections are destroyed.
        /// </summary>
        public void resetShapes()
        {
            CheckConsistency();

            // Mark all of the shapes we are keeping
            HashSet<Shape> toKeep = new HashSet<Shape>();
            foreach (Shape shape in _shapes)
            {
                if (shape.AlreadyGrouped)
                    toKeep.Add(shape);
            }

            // Go through each of the shapes we are keeping, and make sure it is only connected 
            // to things we are keeping
            foreach (Shape shape in toKeep)
            {
                List<Shape> connected = new List<Shape>(shape.ConnectedShapes);
                foreach (Shape connectedShape in connected)
                {
                    if (!toKeep.Contains(connectedShape))
                        disconnectShapes(shape, connectedShape);
                }
            }

            _shapes.Clear();
            foreach (Substroke substroke in Substrokes)
            {
                // Add user-specified shapes to the sketch
                if ((substroke.ParentShape != null) &&
                    (substroke.ParentShape.AlreadyGrouped))
                {
                    AddShape(substroke.ParentShape);
                }
                else
                {
                    Shape shape = new Shape();
                    substroke.ParentShape = null;
                    shape.Type = new ShapeType();
                    shape.AddSubstroke(substroke);
                    substroke.Endpoints[0].ConnectedShape = null;
                    substroke.Endpoints[1].ConnectedShape = null;
                    AddShape(shape);
                    shape.Classification = substroke.Classification;
                }
            }
            CheckConsistency();
        }

        /// <summary>
        /// Connect two given shapes together. If the shapes are already
        /// connected, this method does nothing. It is legal to connect
        /// a shape to itself. The order of the arguments does not matter.
        /// </summary>
        public void connectShapes(Shape shape1, Shape shape2)
        {

            if (!AreConnected(shape1, shape2))
            {
                shape1.ConnectedShapes.Add(shape2);
                if (shape1 != shape2)
                    shape2.ConnectedShapes.Add(shape1);
            }

        }

        /// <summary>
        /// Disconnects two given shapes. If there are any endpoint
        /// connections between the shapes, those are removed as well.
        /// If the shapes are not connected, this method does nothing.
        /// </summary>
        public void disconnectShapes(Shape shape1, Shape shape2)
        {

            if (AreConnected(shape1, shape2))
            {
                shape1.disconnectFrom(shape2);
            }

        }

        #endregion

        #region ADD LABEL
		
		/// <summary>
		/// Add the substroke into a new, labeled Shape. If the substroke already has a parent shape,
        /// it is removed from its parent. (If its parent is now empty, its parent is deleted.)
		/// </summary>
        /// <param name="changedShapes">The list of shapes that changed in any way is written to this out parameter (if any shapes were removed from the sketch, they
        /// are not included in this list)</param>
		/// <param name="substroke">Substroke to be included in the new shape</param>
		/// <param name="label">The new shape's type</param>
		/// <param name="probability">Probability of a label's accuracy</param>
        /// <param name="source">A string describing who created this shape</param>
		/// <returns>The new shape</returns>
        public Shape MakeNewShapeFromSubstroke(out List<Shape> changedShapes, Substroke substroke, ShapeType label, double probability = 0, string source = "Sketch.MakeNewShapeFromSubstroke")
		{
			return MakeNewShapeFromSubstrokes(out changedShapes, new Substroke[] { substroke }, label, probability, source);
		}


		/// <summary>
        /// Add the group of substrokes into a new, labeled Shape. If the substrokes already have parent shapes,
        /// they are removed from their parents. (Shapes which become empty as a result are deleted.)
        /// </summary>
        /// <param name="changedShapes">The list of shapes that changed in any way is written to this out parameter (if any shapes were removed from the sketch, they
        /// are not included in this list)</param>
		/// <param name="substrokes">Substrokes to be included in the label</param>
		/// <param name="label">Label's string</param>
        /// <param name="probability">Probability of a label's accuracy</param>
        /// <param name="source">A string describing who created this shape</param>
        /// <returns>The new shape</returns>
        public Shape MakeNewShapeFromSubstrokes(out List<Shape> changedShapes, IEnumerable<Substroke> substrokes, ShapeType label, double probability = 0, string source = "Sketch.MakeNewShapeFromSubstroke")
		{
            HashSet<Shape> modified = new HashSet<Shape>();

            // Create the new labeled Shape
            // Remove substrokes from their parents
            foreach (Substroke sub in substrokes)
            {

                // We want to remove the substroke from its parent shape.
                Shape parentShape = sub.ParentShape;

                if (parentShape != null)
                {

                    // Mark this parent as modified
                    modified.Add(parentShape);

                    // The shape has changed; its connections are no longer valid. 
                    parentShape.ClearConnections();

                    // Remove the substroke from the shape.
                    parentShape.RemoveSubstroke(sub);
                    if (parentShape.IsEmpty)
                    {
                        RemoveShape(parentShape);

                        // Remove the parent from the list of modified shapes
                        if (modified.Contains(parentShape))
                            modified.Remove(parentShape);
                    }

                }
            }

            // Create a new labeled shape with our substrokes
            Shape labeled = new Shape(substrokes);
            labeled.Source = source;
            labeled.setRecognitionResults(label, (float)probability);

			// Add the label to the Sketch
			this.AddShape(labeled);

            CheckConsistency();

            changedShapes = new List<Shape>(modified);

			return labeled;
		}

		#endregion
		
		#region REMOVE LABELS/SHAPES

        /// <summary>
        /// Removes all labels from the current sketch, but retains groups
        /// </summary>
        public void RemoveLabels()
		{
			foreach (Shape shape in _shapes)
                shape.Type = new ShapeType();
		}

		/// <summary>
		/// Removes all labels and groups from the current sketch. 
		/// <returns>A list of removed shapes.</returns>
		/// </summary>
		public List<Shape> RemoveLabelsAndGroups()
		{
			List<Shape> groups = new List<Shape>(_shapes.Count);
			for(int i = 0; i < _shapes.Count; ++i)
			{
				groups.Add(_shapes[i]);
				RemoveShape(_shapes[i]);
			}
			return groups;
		}

		#endregion

		#region GETTERS & SETTERS

        /// <summary>
        /// Get the XML attributes associated with this sketch.
        /// </summary>
        public XmlStructs.XmlSketchAttrs XmlAttrs
        {
            get { return _xmlAttributes; }
            set { _xmlAttributes = value; }
        }

        /// <summary>
        /// Get or set the units used for this sketch.
        /// Example: "pixels"
        /// </summary>
        public string Units
        {
            get { return _xmlAttributes.Units; }
            set { _xmlAttributes.Units = value; }
        }

        /// <summary>
        /// Determine whether there are any strokes in this sketch.
        /// This property is true iff there are no Strokes.
        /// There are no Strokes iff there are no Substrokes.
        /// </summary>
        public bool IsEmpty
        {
            get { return _strokes.Count == 0; }
        }

        /// <summary>
        /// Determine whether two shapes are connected.
        /// </summary>
        /// <param name="shape1"></param>
        /// <param name="shape2"></param>
        /// <returns></returns>
        public bool AreConnected(Shape shape1, Shape shape2)
        {
            bool contains12 = shape1.ConnectedShapes.Contains(shape2);
            bool contains21 = shape2.ConnectedShapes.Contains(shape1);
            if (contains12 != contains21)
                throw new Exception("Shape connections between " + shape1 + " and " + shape2 + " are inconsistent.");
            return contains12;
        }
		
		/// <summary>
		/// Get the Type Strings (adds a "unlabeled" as the last one)
		/// </summary>
		public List<string> LabelStrings
		{
			get
			{
                HashSet<string> labels = new HashSet<string>();
				foreach(Shape shape in _shapes)
				{
					labels.Add(shape.Type.Name);
				}
                if(!labels.Contains("unlabeled"))
				    labels.Add("unlabeled");

                return new List<string>(labels);
			}
		}

        /// <summary>
        /// Get an array of all the strokes in this sketch.
        /// </summary>
        public Stroke[] Strokes
        {
            get { return _strokes.ToArray(); }
        }

        /// <summary>
        /// Get a list of all the strokes in this sketch.
        /// </summary>
        public List<Stroke> StrokesL
        {
            get { return _strokes; }
        }
		
		/// <summary>
		/// Get the Shapes of the Sketch
		/// </summary>
		public Shape[] Shapes
		{
			get { return this._shapes.ToArray(); }
		}

		/// <summary>
		/// Gets a List of the Shapes of the Sketch
		/// </summary>
        public List<Shape> ShapesL
        {
            get { return this._shapes; }
        }

		/// <summary>
		/// Gets the ith shape in the sketch (an Eric addition)
		/// </summary>
		/// <param name="i">Which shape to get</param>
		/// <returns>The ith shape in the sketch</returns>
		public Shape getShape(int i)
		{
			return _shapes[i];
		}

        /// <summary>
        /// Gets the sorted Substrokes (ascending based on time) in a Sketch.
        /// </summary>
        public List<Substroke> SubstrokesL
        {
            get
            {
                int strokesLength = this._strokes.Count;
                int substrokesLength;
                int i;
                int j;
                Substroke[] sstrokes;

                List<Substroke> substrokes = new List<Substroke>(strokesLength); //initial estimate of length

                // Loop through all the strokes
                for (i = 0; i < strokesLength; ++i)
                {
                    // Get the substrokes of the stroke
                    sstrokes = this._strokes[i].Substrokes;
                    substrokesLength = sstrokes.Length;

                    // Add all the substrokes
                    for (j = 0; j < substrokesLength; ++j)
                        substrokes.Add(sstrokes[j]);
                }

                // Sort the Subtrokes in ascending order first.
                substrokes.Sort();

                return substrokes;
            }
        }

		/// <summary>
		/// Gets the sorted Substrokes (ascending based on time) in a Sketch.
		/// </summary>
		public Substroke[] Substrokes
		{
			get { return SubstrokesL.ToArray(); }
		}


		/// <summary>
		/// Gets the sorted Points (ascending based on time) in a Sketch.
		/// </summary>
		public Point[] Points
		{
			get
			{
                List<Point> points = new List<Point>();

				// Loop through all the substrokes
                foreach (Substroke substroke in Substrokes)
                    points.AddRange(substroke.Points);

				// Sort the Points in ascending order, just to be sure.
				points.Sort();
	
				return points.ToArray();
			}
		}
	
		#endregion

        #region Consistency Checking

        /// <summary>
        /// Verify that the sketch is consistent. This means:
        ///    - Every substroke in every shape is also in some stroke in this sketch
        ///    - If shape A is connected to B, then B is also connected to A
        ///    - No shape has duplicate entries in its list of connected shapes
        ///    - If shape A is connected to an endpoint of B, then A is in B's list of connected shapes and vice-versa
        ///    - Every substroke has only one parent shape, and that parent is in the sketch
        ///    - Every stroke has only one parent shape, and that parent is in the sketch
        ///    - Every stroke has only one child substroke
        /// </summary>
        public bool CheckConsistency()
        {

#if DEBUG

            foreach (Shape shape in _shapes)
            {
                if (shape.Substrokes.Length == 0)
                    throw new Exception(shape + " contains no substrokes!");

                foreach (Substroke substroke in shape.Substrokes)
                    if (!_strokes.Exists(delegate(Stroke s) { return s.SubstrokesL.Contains(substroke); }))
                        throw new Exception(shape + " contains substroke " + substroke + " which is not in the list of strokes!");

                foreach (Shape connectedShape in shape.ConnectedShapes)
                {
                    if (!_shapes.Contains(connectedShape))
                        throw new Exception(shape + " is connected to " + connectedShape + " which is not in the sketch!");
                    if (!connectedShape.ConnectedShapes.Contains(shape))
                        throw new Exception(shape + " is connected to " + connectedShape + " but not vice-versa!");
                }

                foreach (EndPoint endpoint in shape.Endpoints)
                    if (endpoint.ConnectedShape != null)
                    {
                        if (!_shapes.Contains(endpoint.ConnectedShape))
                            throw new Exception(shape + " has an endpoint connected to " + endpoint.ConnectedShape + " which is not in the sketch!");
                        if (!shape.ConnectedShapes.Contains(endpoint.ConnectedShape))
                            throw new Exception(shape + " has an endpoint connected to " + endpoint.ConnectedShape + ", but the shape is not connected to it!");
                    }

                if (Data.Utils.containsDuplicates(shape.ConnectedShapes))
                    throw new Exception(shape + " contains duplicate connected shapes!");
            }

            foreach (Substroke substroke in Substrokes)
            {
                if (substroke.ParentShape != null && !_shapes.Contains(substroke.ParentShape))
                    throw new Exception("Substroke " + substroke + " has a parent shape which is not in the sketch!");
            }

            foreach (Stroke stroke in Strokes)
            {
                if (stroke.Substrokes.Length != 1)
                    throw new Exception("Found a Stroke with " + stroke.Substrokes.Length + " substrokes; expected 1!");
            }

            foreach (Shape wire in _shapes)
            {
                if (!Domain.LogicDomain.IsWire(wire.Type))
                    continue;
                foreach (Shape shape in wire.ConnectedShapes)
                {
                    bool found = false;
                    foreach (EndPoint endpoint in wire.Endpoints)
                    {
                        if (endpoint.ConnectedShape == shape)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (Domain.LogicDomain.IsWire(shape.Type))
                    {
                        foreach (EndPoint endpoint in shape.Endpoints)
                        {
                            if (endpoint.ConnectedShape == wire)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                        throw new Exception("The wire " + wire + " is connected to " + shape + " as a connected shape, but not by an endpoint!");
                }
            }

#endif
            return true;
            
        }

        #endregion

        #region OTHER

        /// <summary>
        /// Compare sketches for geometric equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Sketch))
                return false;

            if (obj == this)
                return true;

            Sketch other = (Sketch) obj;

            if (this.Shapes.Length != other.Shapes.Length)
                return false;

            List<Substroke> substrokes1 = new List<Substroke>(SubstrokesL);
            List<Substroke> substrokes2 = new List<Substroke>(other.SubstrokesL);

            if (substrokes1.Count != substrokes2.Count)
                return false;

            substrokes1.Sort();
            substrokes2.Sort();

            for (int i = 0; i < substrokes1.Count; i++)
            {
                if (!substrokes1[i].Equals(substrokes2[i]))
                    return false;
            }

            return true;

        }

        /// <summary>
        /// Get the hash code of this sketch. Does not
        /// change when the sketch is modified.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _xmlAttributes.Id.GetHashCode();
        }

        /// <summary>
        /// Find the closest shape to the given point. If there are no shapes within a given
        /// threshold (in pixels), this function returns null.
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        /// <param name="threshold">the threshold</param>
        /// <param name="exclude">(optional) the shape to exclude, or null if all shapes should be considered</param>
        /// <returns>the closest shape to the given point, or null if none were closer than threshold</returns>
        public Shape shapeAtPoint(double x, double y, double threshold, Shape exclude = null)
        {
            double bestDistance = Double.MaxValue;
            Shape bestShape = null;

            foreach (Shape shape in _shapes)
            {
                if (shape == exclude)
                    continue;

                double distance = shape.minDistanceTo(x, y);
                if (distance < threshold && distance < bestDistance)
                {
                    bestShape = shape;
                    bestDistance = distance;
                }
            }

            return bestShape;
        }

        /// <summary>
        /// Find the closest substroke to the given point. If there are no substrokes within a given
        /// threshold (in pixels), this function returns null.
        /// </summary>
        /// <param name="x">the x-coordinate</param>
        /// <param name="y">the y-coordinate</param>
        /// <param name="threshold">the threshold</param>
        /// <param name="exclude">(optional) the substroke to exclude, or null if all substrokes should be considered</param>
        /// <returns>the closest substroke to the given point, or null if none were closer than threshold</returns>
        public Substroke substrokeAtPoint(double x, double y, double threshold, Substroke exclude = null)
        {
            double bestDistance = Double.MaxValue;
            Substroke bestSub = null;

            foreach (Substroke sub in Substrokes)
            {
                if (sub == exclude)
                    continue;

                double distance = sub.minDistanceTo(x, y);
                if (distance < threshold && distance < bestDistance)
                {
                    bestSub = sub;
                    bestDistance = distance;
                }
            }

            return bestSub;
        }

        /// <summary>
        /// Find the substroke in this sketch that best fits the point cloud given.
        /// Uses the pressure values (expected range [0,1]) to weight the fit.
        /// </summary>
        /// <param name="cloud">point cloud to fit</param>
        /// <returns>ordered list of substrokes from best to worst match paired with the value of the match</returns>
        public List<KeyValuePair<Substroke,double>> bestFitSubstroke(List<Point> cloud)
        {
            List<KeyValuePair<Substroke, double>> values = new List<KeyValuePair<Substroke, double>>();

            List<Substroke> ls = SubstrokesL;

            foreach (Substroke s in ls)
            {
                values.Add(new KeyValuePair<Substroke,double>(s, fitSubstroke(s, cloud)));
            }

            // sort by the quality of the fit from low to high
            values.Sort(delegate(KeyValuePair<Substroke, double> a, KeyValuePair<Substroke, double> b)
            {
                return a.Value.CompareTo(b.Value);
            });


            return values;
        }

        /// <summary>
        /// Square root of sum of squares of distances from points in cloud to points in s.
        /// Distances are weighted by the Pressure of each point in the cloud.
        /// </summary>
        /// <param name="s">a substroke</param>
        /// <param name="cloud">a point cloud</param>
        /// <returns>quality of the fit</returns>
        public static double fitSubstroke(Substroke s, List<Point> cloud)
        {

            double res = 0d;

            foreach (Point p1 in cloud)
            {
                double closest = double.PositiveInfinity;

                foreach (Point p2 in s.PointsL)
                {
                    closest = Math.Min(closest, Math.Pow(p1.distance(p2), 2));
                }

                if (double.IsPositiveInfinity(closest)) throw new Exception("given an empty substroke");
                res += closest*((double)(p1.Pressure)/ushort.MaxValue);
            }

            return Math.Sqrt(res);

        }

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
            List<Shape> shps = new List<Shape>(_shapes);
            RemoveShapes(shps);
		}

        /// <summary>
        /// Returns true iff the given sketch contains a Shape which is Equal
        /// to the parameter, using the Equals function which compares based on substroke GUIDs.
        /// </summary>
        /// <param name="s">The shape to look for</param>
        /// <returns>True iff the parameter is in the current shape</returns>
        public bool containsShape(Shape s)
        {
            foreach (Shape comp in Shapes)
            {
                if (comp.Equals(s)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the shape object in the Sketch that is Equal to the parameter s. 
		/// Returns null if no shape is found.
        /// </summary>
        /// <param name="s">The shape to look for</param>
        /// <returns>The Shape in this Sketch that matches the input, or null</returns>
        public Shape getShape(Shape s)
        {
            foreach (Shape comp in _shapes)
            {
				if (comp.Equals(s))
					return comp;
            }
			return null;
        }

		/// <summary>
		/// Determines whether or not the current sketch is labeled
		/// </summary>
		public bool isLabeled
		{
			get
			{
				if (_shapes.Count == 0)
					return false;

				foreach (Shape shape in _shapes)
				{
					if (shape.Type == new ShapeType() || shape.Type == null)
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Determines whether or not the given sketch is fragmented
		/// </summary>
		public bool isFragmented
		{
			get
			{
				foreach (Stroke stroke in _strokes)
					if (stroke.Substrokes.Length > 1)
						return true;
				return false;
			}
		}

		#endregion
    }
}
