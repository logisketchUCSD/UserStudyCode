using System;
using System.Drawing;
using System.Collections.Generic;

using Microsoft.Ink;

using Featurefy;
using Fragmenter;
using Sketch;
using SketchPanelLib;
using CommandManagement;

namespace Labeler
{
	/// <summary>
	/// Summary description for LabelerPanel.
	/// </summary>
	public class LabelerPanel : SketchPanel
	{
        protected CommandManager CM;

        //protected Sketch.Sketch sketch;

        protected Featurefy.FeatureStroke[] featureStrokes;

        protected Microsoft.Ink.InkOverlay overlayInk;

        protected DomainInfo domainInfo;

		protected Stack<System.Drawing.Point> lassoPoints;

        protected Microsoft.Ink.Strokes strokesClicked;

		private bool selectionMoving;

		private bool selectionResizing;

		private bool mouseDown;

		private bool clicked;

		private int checkStrokeLabeling;

        private bool zoomIn;

		internal bool show_tooltip = true;

        private LabelerTool lTool;

		/// <summary>
		/// Hashtable from Microsoft Strokes to Sketch.Substrokes.
		/// </summary>
		//protected Dictionary<int, Substroke> mIdToSubstroke;

        //protected Dictionary<Guid, Microsoft.Ink.Stroke> substrokeIdToMStroke;

        //protected Dictionary<Sketch.Stroke, List<int>> strokeToCorners;

        protected Microsoft.Ink.DrawingAttributes fragmentPtAttributes;

        //protected Microsoft.Ink.DrawingAttributes thickenedLabelAttributes;

		//private List<Substroke> thickenedStrokes;

		private static int LASSO_THRESHOLD = 100;

		/// <summary>
		/// How much does the zoom button zoom in?
		/// </summary>
		private static float SCALE_FACTOR = 2.0F;

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

		private float inkMovedX;

		private float inkMovedY;
		
		//private System.Windows.Forms.Button labelButton;
		
		//private LabelMenu labelMenu;

		private System.Windows.Forms.ToolTip toolTip;

		private float closeRadius = 1;

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="originalSketch"></param>
		public LabelerPanel(CommandManager CM, DomainInfo domainInfo) :
			base()
		{
            lTool = new LabelerTool(CM, domainInfo, this);
			// Set the CommandManager
			this.CM = CM;

			// Set the domainInfo
			this.domainInfo = domainInfo;
			
			// Initialize the InkOverlay
			this.inkPic = new mInkPicture();
			
			this.Resize += new EventHandler(LabelerPanel_Resize);
			
			//oInk.SelectionChanged += new InkOverlaySelectionChangedEventHandler(oInk_SelectionChanged);

			// Hashtables so we can convert between Microsoft.Ink and our Sketch
			//this.ltool.MIdToSubstroke = new Dictionary<int,Substroke>();
			//this.substrokeIdToMStroke = new Dictionary<Guid,Microsoft.Ink.Stroke>();

			// Hashtables so we can store what fragment points are associated with FeatureStrokes
			//this.strokeToCorners = new Dictionary<Sketch.Stroke, List<int>>();

			//this.thickenedStrokes = new List<Substroke>();

			// Initialize the drawing attributes for thickened labels
			//InitThickenedLabelAttributes(120);
			
			// Label button & menu
			//InitializeLabelMenu();

			// Resize the panel
			InkResize();

			// Initialize the drawing attributes for fragment points
			InitFragPtAttributes(Color.Red, 25);
		}
			

		private void InitializePanel(Sketch.Sketch sketch)
		{
			this.Enabled = false;
			this.Controls.Clear();

			Sketch.Substroke[] substrokes = sketch.Substrokes;
			this.inkPic = new mInkPicture();
			this.inkPic.DefaultDrawingAttributes.AntiAliased = true;
			this.inkPic.DefaultDrawingAttributes.FitToCurve = true;
			this.inkPic.EditingMode = InkOverlayEditingMode.Select;
			
            //this.mIdToSubstroke.Clear();
            //this.substrokeIdToMStroke.Clear();

			// Setup the Ink in the InkOverlay
			for (int i = 0; i < substrokes.Length; i++)
			{
				System.Drawing.Point[] pts = new System.Drawing.Point[substrokes[i].Points.Length];
				
				for (int k = 0; k < pts.Length; k++)
				{
					pts[k] = new System.Drawing.Point( (int)(substrokes[i].Points[k].X), 
						(int)(substrokes[i].Points[k].Y) );
				}
			
				this.inkPic.Ink.CreateStroke(pts);

				// Allows us to look up a Sketch.Stroke from its Microsoft counterpart
                //int mId = this.inkPic.Ink.Strokes[this.inkPic.Ink.Strokes.Count - 1].Id;
                //this.mIdToSubstroke.Add(mId, substrokes[i]);
                //this.substrokeIdToMStroke.Add(substrokes[i].XmlAttrs.Id.Value, 
                //    this.inkPic.Ink.Strokes[this.inkPic.Ink.Strokes.Count - 1]);
			}

			// Move center the ink's origin to the top-left corner
			Rectangle bb = this.inkPic.Ink.GetBoundingBox(BoundingBoxMode.PointsOnly);
			this.inkMovedX = (int)(-bb.X + 300);
			this.inkMovedY = (int)(-bb.Y + 300);
			this.inkPic.Renderer.Move(this.inkMovedX, this.inkMovedY);

			this.inkPic.Enabled = true;

			// Give the panel the mInk component
			this.Controls.Add(inkPic);

			// Another layer of ink overlaying the current
			// Used when drawing the fragment points
			this.overlayInk = new InkOverlay(this.inkPic);
			this.overlayInk.Renderer.Move(this.inkMovedX, this.inkMovedY);

			// Adds event handlers to the InkPicture
            //this.inkPic.EditingMode = InkOverlayEditingMode.Select;
			
            //this.selectionMoving = false;
            //this.inkPic.SelectionMoving += new InkOverlaySelectionMovingEventHandler(sketchInk_SelectionMoving);
            //this.inkPic.SelectionMoved += new InkOverlaySelectionMovedEventHandler(sketchInk_SelectionMoved);
			
            //this.selectionResizing = false;
            //this.inkPic.SelectionResizing += new InkOverlaySelectionResizingEventHandler(sketchInk_SelectionResizing);
            //this.inkPic.SelectionResized += new InkOverlaySelectionResizedEventHandler(sketchInk_SelectionResized);
			
            //this.mouseDown = false;
            //this.lassoPoints = new Stack<System.Drawing.Point>();
            //this.clicked = false;
            //this.strokesClicked = null;
            //this.inkPic.MouseDown += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseDown);
            //this.inkPic.MouseMove += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseMove);
            //this.inkPic.MouseUp += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseUp);
			
            //this.inkPic.SelectionChanging += new InkOverlaySelectionChangingEventHandler(sketchInk_SelectionChanging);
            //this.inkPic.SelectionChanged += new InkOverlaySelectionChangedEventHandler(sketchInk_SelectionChanged);
		
			// Handle the ToolTip
			//this.inkPic.MouseHover += new EventHandler(sketchInk_MouseHover);

            this.lTool.SubscribeToPanel(this);
			
			// Initialize the label menu
			//InitializeLabelMenu();
			
			// Update the fragment points
			UpdateFragmentCorners();

			// Update the stroke colors
			UpdateColors();
			
			// Create the ToolTip to be used in displaying Substroke information
			this.toolTip = new System.Windows.Forms.ToolTip();
			this.toolTip.InitialDelay = 100;
			this.toolTip.ShowAlways = true;
			
			// Resize the InkPicture
			InkResize();

			InitFragPtAttributes(Color.Red, (int)(this.inkPic.Ink.Strokes.GetBoundingBox().Width * 0.005 + 5));
			closeRadius = (float)(this.inkPic.Ink.Strokes.GetBoundingBox().Width * 0.0001);// + 1);
        }

        #region LabelMenu
        ///// <summary>
        ///// Initialize the LabelMenu we are using to label strokes.
        ///// </summary>
        //private void InitializeLabelMenu()
        //{
        //    labelButton = new System.Windows.Forms.Button();
        //    labelButton.BackColor = Color.Coral;
        //    labelButton.Size = new Size(50, 20);
        //    labelButton.Text = "LABEL";

        //    labelButton.MouseDown += new System.Windows.Forms.MouseEventHandler(labelButton_MouseDown);
			
        //    labelMenu = new LabelMenu(lTool, this.CM);

        //    Controls.Add(labelButton);
        //    Controls.Add(labelMenu);

        //    labelMenu.InitLabels(domainInfo);

        //    labelButton.Hide();
        //    labelMenu.Hide();

        //    Enabled = true;
        //}


        ///// <summary>
        ///// Initialize the labels we are using for this domain
        ///// </summary>
        ///// <param name="domainInfo">Domain to retrieve labels from</param>
        //public void InitLabels(DomainInfo domainInfo)
        //{
        //    this.domainInfo = domainInfo;
			
        //    if (this.labelMenu != null)
        //        this.labelMenu.InitLabels(domainInfo);

        //    UpdateColors();
        //}
        #endregion

        public void UpdateColors()
		{
			if (Sketch != null)
				UpdateColors(Sketch.SubstrokesL);
		}

		public void UpdateColors(List<Substroke> substrokes)
		{
			if (this.domainInfo != null && this.Sketch != null)
			{
				foreach (Sketch.Substroke substroke in substrokes)
				{
					string[] labels = substroke.Labels;

					int bestColorRank = Int32.MinValue;
					Color bestColor = Color.Black;
					foreach (string label in labels)
					{
						int currColorRank = Int32.MinValue; 
						Color currColor = this.domainInfo.GetColor(label);

						if (this.domainInfo.ColorHierarchy.ContainsKey(currColor))
							currColorRank = (int)this.domainInfo.ColorHierarchy[currColor];
							
						if (currColorRank > bestColorRank)
						{
							bestColor = currColor;
							bestColorRank = currColorRank;
						}
					}

					Color[] colorList = getColorList();
					int index = -1;

					if (checkStrokeLabeling == 1)
					{
						if (substroke.ParentShapes.Count == 0)
						{
							bestColor = Color.Red;
						}
						else if (substroke.ParentShapes.Count == 1)
						{
							bestColor = Color.Yellow;
						}
						else
						{
							bestColor = Color.Green;
						}
					}
					else if (checkStrokeLabeling == 2)
					{
						if (substroke.ParentShapes.Count > 0)
							index = substroke.ParentShapes[0].Index;
						else
							index = 0;

						if (index > 15)
							index = index % 15;

						if (index == -1)
							index = 16;

						bestColor = colorList[index];
					}
					else if (checkStrokeLabeling == 3)
					{
						if (substroke.ParentShapes.Count > 0)
							index = substroke.ParentShapes[0].Index;
						else
							index = 0;


						if (index > 15)
							index = index % 15;

						if (index == -1)
							index = 16;

						bestColor = colorList[index];
					}
					

                    if(this.lTool.SubstrokeIdToMStroke.ContainsKey(substroke.XmlAttrs.Id.Value))
					    this.lTool.getInkStrokeBySId(substroke.XmlAttrs.Id.Value).DrawingAttributes.Color = bestColor;
				}
				this.inkPic.Refresh();
			}
        }

        #region ThickenedLabels
        //private void InitThickenedLabelAttributes(int thickness)
        //{
        //    this.lTool.thickenedLabelAttributes = new Microsoft.Ink.DrawingAttributes();
        //    this.lTool.thickenedLabelAttributes.Width = thickness;
        //    this.lTool.thickenedLabelAttributes.Height = thickness;
        //}
		

        //public virtual void ThickenLabel(Microsoft.Ink.Strokes newSelection)
        //{
        //    foreach (Microsoft.Ink.Stroke mStroke in newSelection)
        //    {
        //        if(!this.mIdToSubstroke.ContainsKey(mStroke.Id))
        //            continue;

        //        List<Shape> labels = this.mIdToSubstroke[mStroke.Id].ParentShapes;

        //        foreach (Sketch.Shape labelShape in labels)
        //        {
        //            Sketch.Substroke[] labelSubstrokes = labelShape.Substrokes;
        //            foreach (Sketch.Substroke substroke in labelSubstrokes)
        //            {
        //                if(!this.substrokeIdToMStroke.ContainsKey(substroke.XmlAttrs.Id.Value))
        //                    continue;

        //                Microsoft.Ink.Stroke toModify = this.substrokeIdToMStroke[substroke.XmlAttrs.Id.Value];
        //                toModify.DrawingAttributes.Width = this.thickenedLabelAttributes.Width;
        //                toModify.DrawingAttributes.Height = this.thickenedLabelAttributes.Height;
        //                /*
        //                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
        //                    this.thickenedLabelAttributes.Width;
        //                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
        //                    this.thickenedLabelAttributes.Height;
        //                */
        //                this.thickenedStrokes.Add(substroke);
        //            }
        //        }
        //    }

        //    this.inkPic.Invalidate();
        //}


        //public virtual void UnThickenLabel(Microsoft.Ink.Strokes previousSelection)
        //{
        //    /*foreach (Microsoft.Ink.Stroke mStroke in previousSelection)
        //    {
        //        ArrayList labels = (this.mIdToSubstroke[mStroke.Id] as Sketch.Substroke).ParentShapes;

        //        if (labels.Count == 0)
        //        {
        //            mStroke.DrawingAttributes.Width = this.inkPic.DefaultDrawingAttributes.Width;
        //            mStroke.DrawingAttributes.Height = this.inkPic.DefaultDrawingAttributes.Height;
        //        }
				
        //        foreach (Sketch.Shape labelShape in labels)
        //        {
        //            Sketch.Substroke[] labelSubstrokes = labelShape.Substrokes;
				
        //            foreach (Sketch.Substroke substroke in labelSubstrokes)
        //            {
        //                // IMPORTANT: For some reason we need the following line or our colors do not update
        //                // correctly. THIS IS A HACK
        //                // It's also broken, since we update all the Strokes in all labels with the color
        //                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes =
        //                    mStroke.DrawingAttributes;

        //                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
        //                    this.inkPic.DefaultDrawingAttributes.Width;
        //                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
        //                    this.inkPic.DefaultDrawingAttributes.Height;
        //            }
        //        }
        //    }*/

        //    foreach (Substroke substroke in this.thickenedStrokes)
        //    {
				
        //        if(!this.substrokeIdToMStroke.ContainsKey(substroke.XmlAttrs.Id.Value))
        //            continue;

        //        Microsoft.Ink.Stroke toModify = this.substrokeIdToMStroke[substroke.XmlAttrs.Id.Value];
        //        toModify.DrawingAttributes.Width = this.inkPic.DefaultDrawingAttributes.Width;
        //        toModify.DrawingAttributes.Height = this.inkPic.DefaultDrawingAttributes.Height;

        //        /*
        //        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
        //            this.inkPic.DefaultDrawingAttributes.Width;
        //        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
        //            this.inkPic.DefaultDrawingAttributes.Height;
        //        */
        //    }

        //    UpdateColors(thickenedStrokes);
        //    this.thickenedStrokes.Clear();
			
        //    this.inkPic.Invalidate();
        //}


		/// <summary>
		/// Fragment point properties
		/// </summary>
		/// <param name="color">Drawing color</param>
        /// <param name="thickness">Point width and height</param>
        #endregion

        private void InitFragPtAttributes(Color color, int thickness)
		{
			this.fragmentPtAttributes = new DrawingAttributes();
			this.fragmentPtAttributes.Color = color;
			this.fragmentPtAttributes.Width = thickness;
			this.fragmentPtAttributes.Height = thickness;
		}
		
		
		public void UpdateFragmentCorners(Dictionary<Sketch.Stroke, List<int>> fragStrokeToCorners)
		{
			// Clear the current InkOverlay
			overlayInk.Ink.DeleteStrokes();

			// Split the Stroke at the new fragment points
			foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in fragStrokeToCorners)
			{
				Sketch.Stroke stroke = entry.Key;
				List<int> corners = entry.Value;
				
				bool notEqualOrNull = false;
                List<int> sCorn;
                bool haveCorners = this.lTool.StrokeToCorners.TryGetValue(stroke, out sCorn);
				if (haveCorners)
				{
					notEqualOrNull = true;
				}
				else if (!haveCorners && corners != null && corners.Count > 0)
				{
					notEqualOrNull = true;
				}
				else if (haveCorners && corners != null
					&& corners.ToArray() != sCorn.ToArray())
				{
					notEqualOrNull = true;
				}

				if (notEqualOrNull)
				{
					if ((corners == null || corners.Count == 0) 
						&& sCorn.Count > 0)
						this.lTool.StrokeToCorners.Remove(stroke);
					else
						this.lTool.StrokeToCorners[stroke] = corners;

                    //// Remove all of the substrokes within our InkPicture and hashtables
                    //foreach (Sketch.Substroke substroke in stroke.Substrokes)
                    //{
                    //    this.lTool.MIdToSubstroke.Remove(this.lTool.SubstrokeIdToMStroke[substroke.XmlAttrs.Id.Value].Id);
                    //    this.inkPic.Ink.DeleteStroke(this.lTool.SubstrokeIdToMStroke[substroke.XmlAttrs.Id.Value]);
                    //    this.lTool.SubstrokeIdToMStroke.Remove(substroke.XmlAttrs.Id.Value);
                    //}

					// Merge the substrokes together
					bool labelSame = stroke.MergeSubstrokes();
					if (!labelSame)
					{
						System.Windows.Forms.DialogResult ok = System.Windows.Forms.MessageBox.Show(
							"Labels removed from Stroke " + stroke.XmlAttrs.Id.ToString() + 
							"\ndue to fragmenting a non-uniformly labeled stroke", 
							"Important", 
							System.Windows.Forms.MessageBoxButtons.OK,
							System.Windows.Forms.MessageBoxIcon.Exclamation);
					}

					// Fragment the substroke at the specified indices
					if (corners != null && corners.Count > 0)
						stroke.SplitStrokeAt( corners.ToArray() );

					// Draw the substrokes in the InkPicture
					foreach (Sketch.Substroke substroke in stroke.Substrokes)
					{
						Sketch.Point[] substrokePts = substroke.Points;
						System.Drawing.Point[] pts = new System.Drawing.Point[substrokePts.Length];

                        int len2 = pts.Length;
                        for (int i = 0; i < len2; ++i)
                        {
                            pts[i] = new System.Drawing.Point((int)(substrokePts[i].X),
                                (int)(substrokePts[i].Y));
                        }
			
						this.inkPic.Ink.CreateStroke(pts);

						// Allows us to look up a Sketch.Stroke from its Microsoft counterpart
						int mId = this.inkPic.Ink.Strokes[this.inkPic.Ink.Strokes.Count - 1].Id;
					}

					// Update the colors associated with the substrokes
					UpdateColors(new List<Substroke>(stroke.Substrokes));
				}
			}

			// Add the InkOverlay points we'll be drawing
			List<System.Drawing.Point> ptsToDraw = new List<System.Drawing.Point>();
			
			Sketch.Stroke[] strokes = this.Sketch.Strokes;
			foreach (Sketch.Stroke stroke in strokes)
			{
				List<int> corners;
                if (this.lTool.StrokeToCorners.TryGetValue(stroke, out corners))
				{
					Sketch.Point[] points = stroke.Points;
					
					foreach (int index in corners)
					{
						ptsToDraw.Add(new System.Drawing.Point((int)points[index].X, (int)points[index].Y));
					}
				}
			}
			
			System.Drawing.Point[] fragPts = ptsToDraw.ToArray();
			
			// Create strokes consisting of one point each.
			// This way we can draw the points in our InkPicture and correctly scale the points
			// accordingly.
			foreach (System.Drawing.Point pt in fragPts)
			{
				System.Drawing.Point[] p = new System.Drawing.Point[1];
				p[0] = pt;

				overlayInk.Ink.CreateStroke(p);
			}

			// Render each point
			foreach (Microsoft.Ink.Stroke s in overlayInk.Ink.Strokes)
			{
				s.DrawingAttributes = this.fragmentPtAttributes;
			}

			// Update the Hashtable
			foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in fragStrokeToCorners)
			{
				Sketch.Stroke key = entry.Key;
				List<int> val = entry.Value;
				
				if (val == null || val.Count == 0)
					this.lTool.StrokeToCorners.Remove(key);
				else 
					this.lTool.StrokeToCorners[key] = new List<int>(val);
			}
			
			this.inkPic.Invalidate();
			this.inkPic.Refresh();
		}

			
		/// <summary>
		/// OLD AND NOT USED!!!
		/// </summary>
		public void UpdateFragmentCorners()
		{
            
			// Clear the current InkOverlay
			overlayInk.Ink.DeleteStrokes();

			List<System.Drawing.Point> ptsArray = new List<System.Drawing.Point>();
			
			Sketch.Stroke[] strokes = this.Sketch.Strokes;
			for (int i = 0; i < strokes.Length; i++)
			{
				List<int> corners;
                if(this.lTool.StrokeToCorners.TryGetValue(strokes[i], out corners))
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

			this.Refresh();
            
		}


		private void LabelerPanel_Resize(object sender, System.EventArgs e)
		{
			InkResize();
		}


		private void InkResize()
		{
			InkResize(1.0);
		}

		private void InkResize(double scaleFactor)
		{
			//const double MARGIN = 0.03;

			// Actual stroke bounding box (in Ink Space)
			Rectangle strokeBoundingBox = this.inkPic.Ink.Strokes.GetBoundingBox(BoundingBoxMode.PointsOnly);
			int strokeWidth = strokeBoundingBox.Width;
			int strokeHeight = strokeBoundingBox.Height;

			int inkWidth  = this.Width;
			int inkHeight = this.Height;

			System.Drawing.Graphics g = CreateGraphics();

			if (strokeWidth != 0
				&& strokeHeight != 0
				&& inkWidth != 0 && inkHeight != 0)
			{
				// Convert the rendering space from Ink Space to Pixels
				System.Drawing.Point bottomRight = new System.Drawing.Point((int)(strokeWidth), (int)(strokeHeight));
				this.inkPic.Renderer.InkSpaceToPixel(g, ref bottomRight);
				bottomRight.X += 50;
				bottomRight.Y += 50;
			
				System.Drawing.Point topLeft = new System.Drawing.Point(0, 0);
				this.inkPic.Renderer.InkSpaceToPixel(g, ref topLeft); 				
			
				System.Drawing.Point scalePt = new System.Drawing.Point(bottomRight.X - topLeft.X, 
					bottomRight.Y - topLeft.Y);
			
				// Scale the rendered strokes by the width scaling factor
				float xScale = (float)inkWidth * (float)scaleFactor / (float)scalePt.X;
				float yScale = (float)inkHeight * (float)scaleFactor / (float)scalePt.Y;

				float scale = Math.Min(yScale, xScale);

				// Scale the stroke rendering by the scaling factor
				this.inkPic.Renderer.Scale(scale, scale, false);

				// Scale the fragment point rendering by the scaling factor
				overlayInk.Renderer.Scale(scale, scale, true);
				
				// Move the Renderer's (x,y) origin so that we can see the entire Sketch
				float toMoveX = (this.inkMovedX * scale) - this.inkMovedX;
				float toMoveY = (this.inkMovedY * scale) - this.inkMovedY;
				System.Drawing.Point toMove = new System.Drawing.Point((int)toMoveX + 1, (int)toMoveY + 1);
				//this.inkPic.Renderer.InkSpaceToPixel(g, ref toMove);
				this.inkPic.Renderer.Move(toMove.X, toMove.Y);

				this.inkMovedX = (this.inkMovedX * scale);
				this.inkMovedY = (this.inkMovedY * scale);

				Size siz = this.inkPic.Size;
				siz.Height = (int)(siz.Height* scale);
				siz.Width = (int)(siz.Width * scale);
				this.inkPic.Size = siz;

				// Expand the InkPicture to encompass the entire Panel
				if (this.inkPic.Width < this.Width)
					this.inkPic.Width = this.Width;
				if (this.inkPic.Height < this.Height)
					this.inkPic.Height = this.Height;
				
				// Update the scaling factors
				totalScale *= scale;
				prevScale = totalScale;

				if (zoomIn)
					closeRadius /= scale;

				// Update the user-displayed zoom
				//zoomTextBox.Text = totalScale.ToString();
				this.inkPic.Refresh();
			}
		}


		private void sketchInk_SelectionMoving(object sender, InkOverlaySelectionMovingEventArgs e)
		{
			this.selectionMoving = true;
		}


		/// Handles the InkOverlay to ensure that displayed strokes have not been moved.
		/// Code pulled from: http://windowssdk.msdn.microsoft.com/en-us/library/microsoft.ink.inkoverlay.selectionmoved.aspx
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Selection moving event</param>
		private void sketchInk_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
		{
			// Get the selection's bounding box
			System.Drawing.Rectangle newBounds = this.inkPic.Selection.GetBoundingBox();

			// Move to back to original spot
			this.inkPic.Selection.Move(e.OldSelectionBoundingRect.Left - newBounds.Left,
				e.OldSelectionBoundingRect.Top - newBounds.Top);

			// Trick to insure that selection handles are updated
			this.inkPic.Selection = this.inkPic.Selection;
			
			// Reset our moving flag
			this.selectionMoving = false;
		}

		
		private void sketchInk_SelectionResizing(object sender, InkOverlaySelectionResizingEventArgs e)
		{
			this.selectionResizing = true;
		}
		
		
		/// <summary>
		/// Handles the InkOverlay to ensure that displayed strokes have not been resized.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Selection resizing event</param>
		private void sketchInk_SelectionResized(object sender, InkOverlaySelectionResizedEventArgs e)
		{
			// Move to back to original spot
			this.inkPic.Selection.ScaleToRectangle(e.OldSelectionBoundingRect);

			// Trick to insure that selection handles are updated
			this.inkPic.Selection = this.inkPic.Selection;
			
			// Reset our resizing flag
			this.selectionResizing = false;
		}


		private void sketchInk_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
				this.mouseDown = true;
		}

		
		private void sketchInk_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point mousePosition = new System.Drawing.Point(e.X, e.Y);

			// Display a ToolTip at the current cursor coordinates
			if (show_tooltip)
				DisplayToolTip(mousePosition);

			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if (lassoPoints.Count == 0 || PointDistance(lassoPoints.Peek(), mousePosition) > LASSO_THRESHOLD)
				{
					lassoPoints.Push(mousePosition);
				}
			}
		}

		
		private void sketchInk_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Handles hiding our label menu if we clicked outside its bounds
			if (this.lTool.LabelMenu.Visible)
			{
				System.Drawing.Rectangle labelBoundingBox = new Rectangle(this.lTool.LabelMenu.Location,
					this.lTool.LabelMenu.Size);
				
				if (labelBoundingBox.Contains(new System.Drawing.Point(e.X, e.Y)))
				{
					this.lassoPoints.Clear();
					this.mouseDown = false;
					return;
				}
				else
				{
					this.lTool.LabelMenu.Hide();
				}
			}
			
			// Don't do anything if we were moving or resizing the selection
			if (this.selectionMoving || this.selectionResizing)
			{
				this.lassoPoints.Clear();
				this.mouseDown = false;
				return;
			}

			// Clear our current Selection
			// I don't know why this is required, but if we don't our strokesSelected becomes
			// bloated for some odd reason...
			//this.inkPic.Selection.Clear();
			
			// The maximum number of points allowed in the lasso for the MouseUp
			// to be considered a click
			const int LASSOTHRESHOLD = 10;
			
			if (this.lassoPoints.Count < LASSOTHRESHOLD)
				this.clicked = true;
			else
				this.clicked = false;

			// Clear our lassoPoints holder
			this.lassoPoints.Clear();
			this.mouseDown = false;
			
			// Radius (in ink space coordiantes)
			float farRadius = (float)(2 * closeRadius);

			// Cursor point (in ink space coordinates)
			System.Drawing.Point cursorPt = new System.Drawing.Point(e.X, e.Y);
			this.inkPic.Renderer.PixelToInkSpace(this.inkPic.CreateGraphics(), ref cursorPt);
			
			// Get the strokes close to the point
			Microsoft.Ink.Strokes justClicked = this.inkPic.Ink.HitTest(cursorPt, closeRadius);
			foreach (Microsoft.Ink.Stroke s in justClicked)
			{
				Console.WriteLine(s.Id);
			}
			// Initialize the strokesClicked holder
			if (this.strokesClicked == null)
			{
				this.strokesClicked = justClicked;
				this.inkPic.Selection = this.strokesClicked;
			}
			
			// Checks to see if we haven't clicked on anything
			else if (justClicked.Count == 0 && this.clicked)
			{
				justClicked = this.inkPic.Ink.HitTest(cursorPt, farRadius);
			
				// Clear the strokesClicked holder if we clicked far enough away
				// from strokes
				if (justClicked.Count == 0)
					this.strokesClicked.Clear();

				this.inkPic.Selection = this.strokesClicked;
			}

			// Add or remove what we just clicked on
			else if (this.clicked)
			{
				foreach (Microsoft.Ink.Stroke s in justClicked)
				{
					if (this.strokesClicked.Contains(s))
						this.strokesClicked.Remove(s);
					else
						this.strokesClicked.Add(s);
				}

				this.inkPic.Selection = this.strokesClicked;
			}

			// Trick to ensure we update the selection anyway
			else
			{
				this.inkPic.Selection = this.inkPic.Selection;
			}
		}


		private void sketchInk_SelectionChanging(object sender, InkOverlaySelectionChangingEventArgs e)
		{
			// Don't do anything if we were moving or resizing the selection
			if (this.mouseDown || this.selectionMoving || this.selectionResizing)
			{
				lTool.UnThickenLabel(this.inkPic.Selection);
				return;
			}

			// If we've clicked outside our selection or not close enough to a stroke
			else if (e.NewSelection.Count == 0)
			{
				lTool.UnThickenLabel(this.inkPic.Selection);

				this.strokesClicked.Clear();
			}
			
			// If we've clicked on a stroke or group of closely connected strokes
			else if (this.clicked)
			{
				lTool.UnThickenLabel(this.inkPic.Selection);
			
				foreach (Microsoft.Ink.Stroke s in this.strokesClicked)
				{
					if (!e.NewSelection.Contains(s))
						e.NewSelection.Add(s);
				}

				lTool.ThickenLabel(e.NewSelection);
			}

			// If we've performed a lasso selection
			else
			{
				lTool.UnThickenLabel(this.inkPic.Selection);
				
				this.strokesClicked.Clear();
				this.strokesClicked.Add(e.NewSelection);
				
				lTool.ThickenLabel(this.strokesClicked);
			}
		}


		private void sketchInk_SelectionChanged(object sender, EventArgs e)
		{
			if (this.inkPic.Selection.Count > 0)
			{
				int x, y;
				x = this.inkPic.Selection.GetBoundingBox().X + 
					this.inkPic.Selection.GetBoundingBox().Width;
				y = this.inkPic.Selection.GetBoundingBox().Y + 
					this.inkPic.Selection.GetBoundingBox().Height;

				System.Drawing.Point bottomRight = new System.Drawing.Point(x, y);
				this.inkPic.Renderer.InkSpaceToPixel(this.inkPic.CreateGraphics(),
					ref bottomRight);
				
				bottomRight.X -= 15 - this.AutoScrollPosition.X;
				bottomRight.Y -= 2 - this.AutoScrollPosition.Y;
				this.lTool.LabelButton.Location = bottomRight;

				this.lTool.LabelButton.Visible = true;
				this.lTool.LabelButton.BringToFront();
			}
			else
			{
				this.lTool.LabelButton.Visible = false;
			}
		}
		
		
		private void labelButton_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			System.Drawing.Point topLeft = this.lTool.LabelButton.Location;

			if (topLeft.X + this.lTool.LabelMenu.Width > this.inkPic.Width)
				topLeft.X -= (this.lTool.LabelMenu.Width - this.lTool.LabelButton.Width);
			if (topLeft.Y + this.lTool.LabelMenu.Height > this.inkPic.Height)
				topLeft.Y -= (this.lTool.LabelMenu.Height - this.lTool.LabelButton.Height);

			this.lTool.LabelMenu.UpdateLabelMenu(this.inkPic.Selection);
			
			// Should move the scroll to the top somehow
			
			this.lTool.LabelMenu.Location = topLeft;
			this.lTool.LabelMenu.BringToFront();
			this.lTool.LabelMenu.Show();

			this.lTool.LabelMenu.Focus();
		}
		
		
		/// <summary>
		/// Displays a ToolTip for a Substroke based on where the mouse is located.
		/// </summary>
		/// <param name="coordinates">Mouse coordinates</param>
		private void DisplayToolTip(System.Drawing.Point coordinates)
		{
			if (!show_tooltip)
				return;

			// NOTE: We do this currently since the ToolTip is worthless.
			// In C# 1.1 we can't position it, so it's under the user's hand.
			//if (this.sketchInk == null || true) //~Re-enable for demos
			//	return;

			// Get the current mouse position and convert it into InkSpace
			System.Drawing.Point mousePt = coordinates;
			this.inkPic.Renderer.PixelToInkSpace(this.CreateGraphics(), ref mousePt);

			// Get the closest Microsoft Stroke (in the InkOverlay)
			float strokePt, distance;
			Microsoft.Ink.Stroke closestMSubstroke = 
				this.inkPic.Ink.NearestPoint(mousePt, out strokePt, out distance);

            Substroke closestSubstroke = new Substroke();
			
            try
            {
                // Get the Sketch's corresponding Substroke
                closestSubstroke = (Substroke)this.lTool.getSubstrokeByMId(closestMSubstroke.Id);
            }
            catch
            {
                this.toolTip.Active = false;
            }
			// If the distance to a Substroke is less than some threshold...
			if (distance < 30 && closestSubstroke != null)
			{
				// Create the ToolTip string with and Id and Labels
				string toolTipLabel = "Id: " + closestSubstroke.XmlAttrs.Id.ToString() + "\nLabels: ";
				string[] labels = closestSubstroke.Labels;
				for (int i = 0; i < labels.Length; i++)
				{
					toolTipLabel += labels[i];
				
					if (i < labels.Length - 1)
						toolTipLabel += ", ";        
				}

				toolTipLabel += "\nProbabilities: " + closestSubstroke.GetFirstBelief();
				
				// Show the ToolTip
				this.toolTip.SetToolTip(this.inkPic, toolTipLabel);
				this.toolTip.Active = true;
			}
			else
			{
				// Don't show the ToolTip if we aren't close enough to any Substroke
				this.toolTip.Active = false;
			}
		}



        public LabelerTool LTool
        {
            get { return this.lTool; }
        }

		public Microsoft.Ink.Strokes Selection
		{
			get
			{
				return this.inkPic.Selection;
			}
			set
			{
				this.inkPic.Selection = value;
			}
		}


        public Sketch.Sketch Sketch
        {
            get
            {
                return base.Sketch;
            }
            set
            {
                this.inkSketch.Sketch = value;
                InitializePanel(this.inkSketch.Sketch);
            }
        }



        //public Dictionary<Sketch.Stroke, List<int>> StrokeToCorners
        //{
        //    get
        //    {
        //        return this.strokeToCorners;
        //    }
        //    set
        //    {
        //        this.strokeToCorners = value;
        //    }
        //}


        //public LabelMenu LabelMenu
        //{
        //    get
        //    {
        //        return this.labelMenu;
        //    }
        //}

        /// <summary>
        /// HACK for inheritance.  Do not use.
        /// <see cref="http://blogs.msdn.com/jmanning/archive/2005/09/21/472456.aspx"/>
        /// </summary>
        public LabelerPanel()
        {
        }

		/// <summary>
		/// Change labeling on the stroke for modal displaying
		/// </summary>
		/// <param name="k">The label mode
		/// <example>k=2 => gate labeling</example>
		/// <example>k=3 => non-gate labeling</example></param>
		public void changeStrokeLabeling(int k)
		{
			checkStrokeLabeling = k;
			if (Sketch == null)
				return;
			if (checkStrokeLabeling == 2)
			{
				int n = -1;
				Shape[] shapes = Sketch.Shapes;
				for(int i = 0; i < shapes.Length; ++i)
				{
					if (shapes[i].IsGate)
					{
						++n;
						shapes[i].Index = n;
					}
					else
					{
						shapes[i].Index = -1;
					}
				}
			}
			else if (checkStrokeLabeling == 3)
			{
				int n = -1;
				Shape[] shapes = Sketch.Shapes;
				for (int i = 0; i < shapes.Length; ++i)
				{
					if (!shapes[i].IsGate)
					{
						++n;
						shapes[i].Index = n;
					}
					else
					{
						shapes[i].Index = -1;
					}
				}
			}
  
			UpdateColors();
		}

		/// <summary>
		/// Change the zoom factor
		/// <returns>The zoom status of the sketch after this action is applied</returns>
		/// </summary>
		public bool changeZoom()
		{
			if (zoomIn)
				zoomIn = false;
			else
				zoomIn = true;
			InkResize(zoomIn ? SCALE_FACTOR : 1.0);
			return zoomIn;
		}

		/// <summary>
		/// Change the zoom
		/// </summary>
		/// <param name="to">In or out?</param>
		public void changeZoom(bool to)
		{
			zoomIn = to;
			InkResize(zoomIn ? SCALE_FACTOR : 1.0);
		}

		/// <summary>
		/// Zoom to a specific percent
		/// </summary>
		/// <param name="zoomFactor">The zoom factor</param>
		public void changeZoom(double zoomFactor)
		{
			if (zoomFactor > 1)
				zoomIn = true;
			InkResize(zoomFactor);
		}
        
		/// <summary>
		/// Gets a static list of colors
		/// </summary>
		/// <returns>17 colors</returns>
		public Color[] getColorList()
        {
            Color[] colorList = new Color[17];

            colorList[0] = Color.Black;
            colorList[1] = Color.Brown;
            colorList[2] = Color.Green;
            colorList[3] = Color.Cyan;
            colorList[4] = Color.Purple;
            colorList[5] = Color.Pink;
            colorList[6] = Color.Blue;
            colorList[7] = Color.DarkCyan;
            colorList[8] = Color.Silver;
            colorList[9] = Color.Red;
            colorList[10] = Color.Teal;
            colorList[11] = Color.RoyalBlue;
            colorList[12] = Color.PaleGreen;
            colorList[13] = Color.PaleVioletRed;
            colorList[14] = Color.YellowGreen;
            colorList[15] = Color.Plum;
            colorList[16] = Color.Yellow;

            return colorList;
        }

		protected static double PointDistance(System.Drawing.Point P1, System.Drawing.Point P2)
		{
			return Math.Sqrt(Math.Pow(P1.X - P2.X, 2) + Math.Pow(P1.Y - P2.Y, 2));
		}


        /* Eric's stuff which is no longer used in the labeler
         * Functionality for stroke classification and clustering can
         * be found in the project "Classifier" in the Util directory.
        
        public List<double> wireClassifications;
        public Dictionary<Guid, int> clusterClassifications;
         * 
         * 
        /// <summary>
		/// Toggles the labeling mode switch value
		/// <list type="bullet">
		/// <listheader>Modes</listheader>
		/// <item>0 - Normal labeling mode</item>
		/// <item>1 - Multiple labeling check mode (shows shapes with multiple labels)</item>
		/// <item>2 - Gate Shapes separated (shows all unique gate shapes)</item>
		/// <item>3 - Non-Gate Shapes separated (shows all unique non-gate shapes)</item>
		/// </list>
		/// </summary>
		public void changeSwitchValue()
		{
			checkStrokeLabeling++;

			if (checkStrokeLabeling >= 3)
	            checkStrokeLabeling = 0;

	        changeStrokeLabeling(checkStrokeLabeling);
	    }
         * 
        public void resetcheckStrokeLabeling() { checkStrokeLabeling = 0; }
         * 
         * 
        private int findSubstrokeIndex(Guid id)
		{
			for(int i =0 ; i < sketch.SubstrokesL.Count; ++i)
			{
				if(sketch.SubstrokesL[i].XmlAttrs.Id.Value.Equals(id))
					return i;
			}
			return 0;
		}
         * 
        /// <summary>
        /// Select and thicken the strokes that are in the most recently merged cluster
        /// </summary>
        /// <param name="substrokes">List of substrokes in the cluster to thicken</param>
        public void thickenCluster(List<Substroke> substrokes)
        {
            Microsoft.Ink.Strokes strokes = this.Selection;
            strokes.Clear();
            
            foreach (Substroke s in substrokes)
                strokes.Add(substrokeIdToMStroke[s.Id]);

            this.sketchInk.Selection = strokes;
        }
         * 
         * 
         * Taken from the UpdateColors(List<Substroke> substrokes) function
        else if (checkStrokeLabeling == 4)
					{
						double factor = 0.0;
						int i = findSubstrokeIndex(substroke.XmlAttrs.Id.Value);
						if (wireClassifications[i] >= 0.5 + factor)
							index = 1;
						else if (wireClassifications[i] < 0.5 - factor)
							index = 2;
						else if (wireClassifications[i] >= 0.5)
							index = 3;
						else
							index = 5;

						bestColor = colorList[index];
					}
                    else if (checkStrokeLabeling == 5)
                    {
                        index = clusterClassifications[substroke.XmlAttrs.Id.Value];

                        while (index >= colorList.Length)
                            index = index - colorList.Length + 1;

                        bestColor = colorList[index];
                    }
         * */
    }


}
