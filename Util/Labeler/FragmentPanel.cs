using System;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;

using CommandManagement;

namespace Labeler
{
	/// <summary>
	/// Summary description for FragmentPanel.
	/// </summary>
	public class FragmentPanel : System.Windows.Forms.Panel
	{
		private CommandManager CM;
		
		private Sketch.Stroke[] strokes;
		
		private Microsoft.Ink.InkPicture sketchInk;

		private Microsoft.Ink.InkOverlay overlayInk;

		private Microsoft.Ink.DrawingAttributes fragmentPtAttributes;

		private Dictionary<Sketch.Stroke, List<int>> strokeToCorners;

		private float inkMovedX;

		private float inkMovedY;
		
		/// <summary>
		/// Previous scaling factor. Used in resizing the Ink and the Form.
		/// </summary>
		private float prevScale = 1.0f;
		
		/// <summary>
		/// New scaling factor. Used in resizing the Ink and the Form.
		/// </summary>
		private float newScale = 1.0f;
		
		/// <summary>
		/// Total scaling factor. Used in resizing the Ink and the Form.
		/// </summary>
		private float totalScale = 1.0f;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="originalStrokes">Original strokes to fragment</param>
		/// <param name="CM">CommandManager for the panel</param>
		public FragmentPanel(Sketch.Stroke[] strokes, Dictionary<Sketch.Stroke, List<int>> strokeToCorners, CommandManager CM)
		{
			// Set our CommandManager
			this.CM = CM;
			
			// Initialize the Hashtable we'll use to keep track of out split points
			this.strokeToCorners = strokeToCorners;

			// Set some stylistic properties
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.White;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			
			// Allow the ink to be resized
			this.Resize += new EventHandler(FragmentPanel_Resize);

			// Initialize the InkPicture
			sketchInk = new InkPicture();
			sketchInk.EditingMode = InkOverlayEditingMode.Ink;
			sketchInk.SelectionMoved += new InkOverlaySelectionMovedEventHandler(sketchInk_SelectionMoved);
			
			// Setup the Ink in the InkPicture
			this.strokes = (Sketch.Stroke[])strokes.Clone();
			for (int i = 0; i < strokes.Length; i++)
			{
				System.Drawing.Point[] pts = new System.Drawing.Point[strokes[i].Points.Length];
				for (int k = 0; k < pts.Length; k++)
				{
					pts[k] = new System.Drawing.Point( (int)(strokes[i].Points[k].X), 
						(int)(strokes[i].Points[k].Y) );
				}
			
				sketchInk.Ink.CreateStroke(pts);
			}
			
			sketchInk.DefaultDrawingAttributes.Color = Color.Red;
			sketchInk.Stroke += new InkCollectorStrokeEventHandler(sketchInk_Stroke);
			
			// Give the panel the mInk component
			this.Controls.Add(sketchInk);

			// Another layer of ink overlaying the current
			// Used when drawing the fragment points
			overlayInk = new InkOverlay(sketchInk);

			// Move center the ink's origin to the top-left corner
			this.inkMovedX = -sketchInk.Ink.GetBoundingBox().X;
			this.inkMovedY = -sketchInk.Ink.GetBoundingBox().Y;
			sketchInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
			overlayInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
			
			// Initialize the overlay panel with the fragmented corners
			InitFragPtAttributes(Color.Red, 200);
			InitFragmentCorners();

			// Allow the user to draw where the fragment points should be
			//overlayInk.EditingMode = InkOverlayEditingMode.Ink;
			//overlayInk.Enabled = true;
		}


		/// <summary>
		/// Fragment point properties
		/// </summary>
		/// <param name="color">Drawing color</param>
		/// <param name="thickness">Point width and height</param>
		private void InitFragPtAttributes(Color color, int thickness)
		{
			this.fragmentPtAttributes = new DrawingAttributes();
			this.fragmentPtAttributes.Color = color;
			this.fragmentPtAttributes.Width = thickness;
			this.fragmentPtAttributes.Height = thickness;
		}

		
		/// <summary>
		/// Resize the panel
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void FragmentPanel_Resize(object sender, System.EventArgs e)
		{
			InkResize();
		}


		/// <summary>
		/// Creates the fragment corners found for the sketch
		/// </summary>
		private void InitFragmentCorners() 
		{
			List<System.Drawing.Point> ptsArray = new List<System.Drawing.Point>();

            int len = this.strokes.Length;
			for (int i = 0; i < len; i++)
			{
				List<int> corners;

                try
                { 
                    corners = this.strokeToCorners[this.strokes[i]]; 
                }
                catch (Exception e)
                {
                    corners = null;
                }

				if (corners != null)
				{
					Sketch.Point[] points = strokes[i].Points;
					
					foreach (int index in corners)
					{
						ptsArray.Add(new System.Drawing.Point((int)points[index].X, (int)points[index].Y));
					}
				}
			}
			
			System.Drawing.Point[] pts = ptsArray.ToArray();
			
			// Create strokes consisting of one point each
			// This is done so that we can draw the points in our InkPicture and correctly scale the points
			// accordingly.
			for (int i = 0; i < pts.Length; i++)
			{
				System.Drawing.Point[] p = new System.Drawing.Point[1];
				p[0] = pts[i];

				overlayInk.Ink.CreateStroke(p);
			}

			// Render each point
			foreach (Microsoft.Ink.Stroke s in overlayInk.Ink.Strokes)
			{
				s.DrawingAttributes = this.fragmentPtAttributes;
			}	
		}
				
		
		/// <summary>
		/// Resize the Ink within the panel
		/// </summary>
		private void InkResize()
		{
			const double MARGIN = 0.03;

			// Actual stroke bounding box (in Ink Space)
			int strokeWidth  = sketchInk.Ink.Strokes.GetBoundingBox().Width;
			int strokeHeight = sketchInk.Ink.Strokes.GetBoundingBox().Height;
			
			int inkWidth  = this.Width - Convert.ToInt32(this.Width * MARGIN);
			int inkHeight = this.Height - Convert.ToInt32(this.Height * MARGIN);
		
			System.Drawing.Graphics g = this.CreateGraphics();

			if (strokeWidth != 0 && strokeHeight != 0 && inkWidth != 0 && inkHeight != 0)
			{
				// If we want to scale by the panel's width
				if (/*this.panelWidthBox.Checked ==*/ true)
				{
					// Convert the rendering space from Ink Space to Pixels
					System.Drawing.Point bottomRight = new System.Drawing.Point(strokeWidth, strokeHeight);
					sketchInk.Renderer.InkSpaceToPixel(g, ref bottomRight); 				
				
					System.Drawing.Point topLeft = new System.Drawing.Point(0, 0);
					sketchInk.Renderer.InkSpaceToPixel(g, ref topLeft); 				
				
					System.Drawing.Point scalePt = new System.Drawing.Point(bottomRight.X - topLeft.X, 
						bottomRight.Y - topLeft.Y);
				
					// Scale the rendered strokes by the width scaling factor
					float xScale = (float)inkWidth / (float)scalePt.X;
					float yScale = (float)inkHeight / (float)scalePt.Y;
		
					float scale = Math.Min(xScale, yScale);

					// Scale the stroke rendering by the scaling factor
					sketchInk.Renderer.Scale(scale, scale, false);

					// Scale the fragment point rendering by the scaling factor
					overlayInk.Renderer.Scale(scale, scale, false);

					// Resize the mInk component to encompass all of the scaled strokes
					System.Drawing.Rectangle scaledBoundingBox = sketchInk.Ink.GetBoundingBox();
					System.Drawing.Point scaledBottomRight = new System.Drawing.Point(scaledBoundingBox.Width,
						scaledBoundingBox.Height);

					sketchInk.Renderer.InkSpaceToPixel(g, ref scaledBottomRight); 

					sketchInk.Size = new Size(scaledBottomRight.X, scaledBottomRight.Y);
				
					// Expand the InkPicture to encompass the entire Panel
					if (sketchInk.Width < this.Width)
						sketchInk.Width = this.Width;
					if (sketchInk.Height < this.Height)
						sketchInk.Height = this.Height;
					
					// Move the Renderers' (x,y) origin so that we can see the entire Sketch
					sketchInk.Renderer.Move((this.inkMovedX * scale) - this.inkMovedX, 
						(this.inkMovedY * scale) - this.inkMovedY);
					overlayInk.Renderer.Move((this.inkMovedX * scale) - this.inkMovedX, 
						(this.inkMovedY * scale) - this.inkMovedY);

					this.inkMovedX = (this.inkMovedX * scale);
					this.inkMovedY = (this.inkMovedY * scale);

					// Update the scaling factors
					totalScale *= scale;
					prevScale = totalScale;

					// Update the user-displayed zoom
					//zoomTextBox.Text = totalScale.ToString();
				}
				else
				{
					if (prevScale != 0.0f)
						sketchInk.Renderer.Scale(newScale / prevScale, newScale / prevScale, false);
					
					this.totalScale = prevScale = newScale;	
				}
			}
		}


		/// Handles the InkOverlay to ensure that displayed strokes have not been moved.
		/// Code pulled from: http://windowssdk.msdn.microsoft.com/en-us/library/microsoft.ink.inkoverlay.selectionmoved.aspx
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void sketchInk_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
		{
			// Get the selection's bounding box
			System.Drawing.Rectangle newBounds = sketchInk.Selection.GetBoundingBox();

			// Move to back to original spot
			sketchInk.Selection.Move(e.OldSelectionBoundingRect.Left - newBounds.Left,
				e.OldSelectionBoundingRect.Top - newBounds.Top);

			// Trick to insure that selection handles are updated
			sketchInk.Selection = sketchInk.Selection;
		}


		/// <summary>
		/// Fragment the loaded Stroke at the intersections created by the drawn
		/// Microsoft.Ink.Stroke.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void sketchInk_Stroke(object sender, InkCollectorStrokeEventArgs e)
		{
			// Find the float intersection points
			float[] intersections = e.Stroke.FindIntersections(sketchInk.Ink.Strokes);
			System.Drawing.Point[] pointInter = new System.Drawing.Point[intersections.Length];
		
			// Find the actual points of intersection
			for (int i = 0; i < pointInter.Length; ++i)
				pointInter[i] = LocatePoint(e.Stroke, intersections[i]);
			
			// Remove our drawing stroke from the Ink
			this.sketchInk.Ink.DeleteStroke(e.Stroke);
			
			// Threshold for how far away we can draw from the actual stroke
			const double DISTTHRESHOLD = 40.0;

			// Hashtable mapping new points to FeatureStrokes
			Dictionary<Sketch.Stroke, List<int>> ptsToAdd = new Dictionary<Sketch.Stroke,List<int>>();
			
			// For each point in our intersections we'll find the closest point in 
			// the corresponding FeatureStroke points
			foreach (System.Drawing.Point currPt in pointInter)
			{	
				int bestPointIndex = 0;
				double bestDist = Double.PositiveInfinity;
				Sketch.Stroke bestStroke = null;
					
				// Check all of the FeatureStrokes to see if the Point is close enough
				foreach (Sketch.Stroke stroke in this.strokes)
				{
					Sketch.Point[] pts = stroke.Points;
				
					// Find the closest point for this FeatureStroke
					for (int i = 0; i < pts.Length; i++)
					{	
						double currDist = Euclidean(currPt.X, currPt.Y, pts[i].X, pts[i].Y);
						
						if (currDist < bestDist)
						{
							bestDist = currDist;
							bestPointIndex = i;
							bestStroke = stroke;
						}
					}
				}
					
				// If it's close enough, add it to our temporary Hashtable to keep track of 
				// which point corresponds to which FeatureStroke
				if (bestDist < DISTTHRESHOLD)
				{
					bool alreadyExists = false;
					if (this.strokeToCorners.ContainsKey(bestStroke))
					{
						List<int> existingPts = this.strokeToCorners[bestStroke];
						
						if (existingPts.Contains(bestPointIndex))
							alreadyExists = true;
					}

					if (!alreadyExists)
					{
						if (!ptsToAdd.ContainsKey(bestStroke))
						{
							List<int> newPts = new List<int>();
							newPts.Add(bestPointIndex);
							ptsToAdd.Add(bestStroke, newPts);
						}
						else
						{
							List<int> existingPts = ptsToAdd[bestStroke];
							
							if (!existingPts.Contains(bestPointIndex))
								existingPts.Add(bestPointIndex);
						}
					}
				}
			}
			
			if (ptsToAdd.Count > 0)
			{
				// Hand-fragment the stroke
				CM.ExecuteCommand( new CommandList.HandFragmentCornersCmd(ptsToAdd, 
					this.strokeToCorners, this.overlayInk, this.fragmentPtAttributes) );
			}

			sketchInk.Refresh();
		}


		/// <summary>
		/// Get the Point at a floating point index. 
		/// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tpcsdk10/lonestar/Microsoft.Ink/Classes/stroke/Methods/getpoint.asp.
		/// </summary>
		/// <param name="theStroke">Stroke</param>
		/// <param name="theFIndex">Floating point index</param>
		/// <returns>A point on the stroke</returns>
		private System.Drawing.Point LocatePoint(Microsoft.Ink.Stroke theStroke, float theFIndex)
		{
			System.Drawing.Point ptResult = theStroke.GetPoint((int)theFIndex);
			float theFraction = theFIndex - (int)theFIndex;
			if (theFraction > 0.0f)
			{
				System.Drawing.Point ptDelta = theStroke.GetPoint((int)theFIndex + 1);
				ptResult.X += (int)((ptDelta.X - ptResult.X) * theFraction);
				ptResult.Y += (int)((ptDelta.Y - ptResult.Y) * theFraction);
			}
			return ptResult;
		}

		
		/// <summary>
		/// Euclidean distance function
		/// </summary>
		/// <param name="x1">First point's x</param>
		/// <param name="y1">First point's y</param>
		/// <param name="x2">Second point's x</param>
		/// <param name="y2">Second point's y</param>
		/// <returns>The euclidean distance between the two points</returns>
		private double Euclidean(double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt( ((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)) );
		}
		
		
		/// <summary>
		/// Get the drawn Stroke in Microsoft.Ink format
		/// </summary>
		public Microsoft.Ink.InkPicture SketchInk
		{
			get
			{
				return this.sketchInk;
			}
		}

		
		/// <summary>
		/// Get the drawn Stroke in Microsoft.Ink format
		/// </summary>
		public Microsoft.Ink.InkOverlay OverlayInk
		{
			get
			{
				return this.overlayInk;
			}
		}

		
		public Sketch.Stroke[] Strokes
		{
			get
			{
				return this.strokes;
			}
		}
		

		public Dictionary<Sketch.Stroke, List<int>> StrokeToCorners
		{
			get
			{
				return this.strokeToCorners;
			}
			set
			{
				this.strokeToCorners = value;
			}
		}


		public Microsoft.Ink.DrawingAttributes FragmentPtAttributes
		{
			get
			{
				return this.fragmentPtAttributes;
			}
		}
	}
}
