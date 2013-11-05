using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Ink;

using Featurefy;
using Fragmenter;
using SketchPanelLib;
using Sketch;
using Labeler;
using CommandManagement;
using EditMenu;

namespace Labeler
{
    /// <summary>
    /// UI Tool for correcting stroke labels (e.g., for correcting recognition results).
    /// </summary>
    public class LabelerTool
    {
        #region Internals
        // TODO add comments for each member

        protected CommandManager CM;

        protected SketchPanel sketchPanel;

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

        internal bool show_tooltip = true;

        protected Dictionary<Sketch.Stroke, List<int>> strokeToCorners;

        protected Microsoft.Ink.DrawingAttributes fragmentPtAttributes;

        protected Microsoft.Ink.DrawingAttributes thickenedLabelAttributes;

        private List<Substroke> thickenedStrokes;

        private static int LASSO_THRESHOLD = 100;





        private float inkMovedX;

        private float inkMovedY;

        private System.Windows.Forms.Button labelButton;

        private LabelMenu labelMenu;

        private EditMenu.EditMenu editMenu;

        private System.Windows.Forms.ToolTip toolTip;

        private float closeRadius = 1;
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalSketch"></param>
        public LabelerTool(CommandManager CM, DomainInfo domainInfo, SketchPanel SP)
            : base()
        {
            // Set the CommandManager
            this.CM = CM;

            // Set the domainInfo
            this.domainInfo = domainInfo;

            //this.Resize += new EventHandler(LabelerPanel_Resize);

            //oInk.SelectionChanged += new InkOverlaySelectionChangedEventHandler(oInk_SelectionChanged);

            // Hashtables so we can store what fragment points are associated with FeatureStrokes
            this.strokeToCorners = new Dictionary<Sketch.Stroke, List<int>>();

            this.thickenedStrokes = new List<Substroke>();

            this.sketchPanel = SP;

            // Initialize the drawing attributes for thickened labels
            InitThickenedLabelAttributes(120);

            // Label button & menu
            //InitializeLabelMenu();

            // Resize the panel
            //InkResize();

            // Initialize the drawing attributes for fragment points
            InitFragPtAttributes(Color.Red, 25);


        }


        #endregion

        #region Initializers

        private void Initialize()
        {
            //this.Enabled = false;
            //this.sketchPanel.Controls.Clear();

            sketchPanel.InkPicture.DefaultDrawingAttributes.AntiAliased = true;
            sketchPanel.InkPicture.DefaultDrawingAttributes.FitToCurve = true;


            // Another layer of ink overlaying the current
            // Used when drawing the fragment points
            this.overlayInk = new InkOverlay(sketchPanel.InkPicture);

            this.selectionMoving = false;

            this.selectionResizing = false;


            this.mouseDown = false;

            #region sketchInkOldCode
            // Move center the ink's origin to the top-left corner
            //Rectangle bb = sketchInk.Ink.GetBoundingBox(BoundingBoxMode.PointsOnly);
            //Rectangle bb = sketchPanel.InkPicture.Ink.GetBoundingBox(BoundingBoxMode.PointsOnly);
            //this.inkMovedX = (int)(-bb.X + 300);
            //this.inkMovedY = (int)(-bb.Y + 300);
            //this.sketchInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
            //sketchPanel.InkPicture.Renderer.Move(this.inkMovedX, this.inkMovedY);
            //this.sketchInk.Enabled = true;
            //sketchPanel.InkPicture.Enabled = true;

            // Give the panel the mInk component
            //this.Controls.Add(sketchInk);
            //this.overlayInk = new InkOverlay(this.sketchInk);
            //this.overlayInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
            //this.sketchInk.SelectionMoving += new InkOverlaySelectionMovingEventHandler(sketchInk_SelectionMoving);
            //this.sketchInk.SelectionMoved += new InkOverlaySelectionMovedEventHandler(sketchInk_SelectionMoved);
            //this.sketchInk.SelectionResizing += new InkOverlaySelectionResizingEventHandler(sketchInk_SelectionResizing);
            //this.sketchInk.SelectionResized += new InkOverlaySelectionResizedEventHandler(sketchInk_SelectionResized);
            //this.sketchInk.MouseDown += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseDown);
            //this.sketchInk.MouseMove += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseMove);
            //this.sketchInk.MouseUp += new System.Windows.Forms.MouseEventHandler(sketchInk_MouseUp);

            //this.sketchInk.SelectionChanging += new InkOverlaySelectionChangingEventHandler(sketchInk_SelectionChanging);
            //this.sketchInk.SelectionChanged += new InkOverlaySelectionChangedEventHandler(sketchInk_SelectionChanged);

            // Handle the ToolTip
            //this.sketchInk.MouseHover += new EventHandler(sketchInk_MouseHover);
            #endregion

            // Initialize the label menu
            InitializeLabelMenu();

            // Update the fragment points
            UpdateFragmentCorners();

            // Update the stroke colors
            UpdateColors();

            // Create the ToolTip to be used in displaying Substroke information
            this.toolTip = new System.Windows.Forms.ToolTip();
            this.toolTip.InitialDelay = 100;
            this.toolTip.ShowAlways = true;

            // Resize the InkPicture
            //InkResize();

            InitFragPtAttributes(Color.Red, (int)(SketchInk.Ink.Strokes.GetBoundingBox().Width * 0.005 + 5));
            closeRadius = (float)(SketchInk.Ink.Strokes.GetBoundingBox().Width * 0.0001);// + 1);

            //this.editMenu = new EditMenu.EditMenu(ref sketchPanel,);
        }


        /// <summary>
        /// Subscribes to SketchPanel.  Subscibe only when tool is selected.
        /// <see cref="SketchPanelLib.SketchPanelListener.SubscribeToPanel()"/>
        /// </summary>
        public void SubscribeToPanel(SketchPanel parentPanel)
        {
            // Hook into SketchPanel Events
            sketchPanel.InkPicture.SelectionChanged += new InkOverlaySelectionChangedEventHandler(InkPicture_SelectionChanged);
            sketchPanel.InkPicture.SelectionMoving += new InkOverlaySelectionMovingEventHandler(InkPicture_SelectionMoving);
            sketchPanel.InkPicture.SelectionMoved += new InkOverlaySelectionMovedEventHandler(InkPicture_SelectionChanged);
            sketchPanel.InkPicture.SelectionResized += new InkOverlaySelectionResizedEventHandler(InkPicture_SelectionChanged);
            sketchPanel.InkPicture.SelectionResizing += new InkOverlaySelectionResizingEventHandler(InkPicture_SelectionResizing);
            sketchPanel.InkPicture.MouseDown += new MouseEventHandler(InkPicture_MouseDown);
            sketchPanel.InkPicture.MouseMove += new MouseEventHandler(InkPicture_MouseMove);
            sketchPanel.InkPicture.MouseUp += new MouseEventHandler(InkPicture_MouseUp);
            // sketchPanel.InkPicture.SelectionChanging += new InkOverlaySelectionChangingEventHandler(InkPicture_SelectionChanging);

            // Create LabelMenu and Button
            Initialize();

            // Put panel in Select Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Select;

            // Add button and menu
            sketchPanel.Controls.Add(this.labelButton);
            sketchPanel.Controls.Add(this.labelMenu);

            // Initially hide the controls
            this.labelButton.Hide();
            this.labelMenu.Hide();
        }

        /// <summary>
        /// Unsubscribes from SketchPanel
        /// <see cref="SketchPanelLib.SketchPanelListener.UnSubscribeToPanel()"/>
        /// </summary>
        public void UnsubscribeFromPanel()
        {
            if (sketchPanel == null)
                return;

            sketchPanel.InkPicture.SelectionChanged -= new InkOverlaySelectionChangedEventHandler(InkPicture_SelectionChanged);
            sketchPanel.InkPicture.SelectionMoving -= new InkOverlaySelectionMovingEventHandler(InkPicture_SelectionMoving);
            sketchPanel.InkPicture.SelectionMoved -= new InkOverlaySelectionMovedEventHandler(InkPicture_SelectionChanged);
            sketchPanel.InkPicture.SelectionResized -= new InkOverlaySelectionResizedEventHandler(InkPicture_SelectionChanged);
            // sketchPanel.InkPicture.SelectionResizing -= new InkOverlaySelectionResizingEventHandler(InkPicture_SelectionResizing);
            sketchPanel.InkPicture.MouseDown -= new MouseEventHandler(InkPicture_MouseDown);
            sketchPanel.InkPicture.MouseMove -= new MouseEventHandler(InkPicture_MouseMove);
            sketchPanel.InkPicture.MouseUp -= new MouseEventHandler(InkPicture_MouseUp);
            // sketchPanel.InkPicture.SelectionChanging -= new InkOverlaySelectionChangingEventHandler(InkPicture_SelectionChanging);


            sketchPanel.Controls.Remove(this.labelButton);
            sketchPanel.Controls.Remove(this.labelMenu);

            this.editMenu.removeMenu();

            // HACK Clear selection
            sketchPanel.InkPicture.Selection.Clear();

            // Put panel in Ink Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Ink;
        }



        #region Normal Initializers

        /// <summary>
        /// Initialize the LabelMenu we are using to label strokes.
        /// </summary>
        private void InitializeLabelMenu()
        {
            labelButton = new System.Windows.Forms.Button();
            labelButton.BackColor = Color.Coral;
            labelButton.FlatStyle = FlatStyle.Flat;
            labelButton.Size = new Size(100, 35);
            labelButton.TextAlign = ContentAlignment.MiddleCenter;
            labelButton.Text = "Change Label";
            
            labelButton.MouseDown += new System.Windows.Forms.MouseEventHandler(labelButton_MouseDown);

            labelMenu = new LabelMenu(this, this.CM);

            sketchPanel.Controls.Add(labelButton);
            sketchPanel.Controls.Add(labelMenu);

            labelMenu.InitLabels(domainInfo);

            labelButton.Hide();
            labelMenu.Hide();

            //Enabled = true;
        }


        /// <summary>
        /// Initialize the labels we are using for this domain
        /// </summary>
        /// <param name="domainInfo">Domain to retrieve labels from</param>
        public void InitLabels(DomainInfo domainInfo)
        {
            this.domainInfo = domainInfo;

            if (this.labelMenu != null)
                this.labelMenu.InitLabels(domainInfo);

            UpdateColors();
        }

        private void InitThickenedLabelAttributes(int thickness)
        {
            this.thickenedLabelAttributes = new Microsoft.Ink.DrawingAttributes();
            this.thickenedLabelAttributes.Width = thickness;
            this.thickenedLabelAttributes.Height = thickness;
        }


        public virtual void ThickenLabel(Microsoft.Ink.Strokes newSelection)
        {
            foreach (Microsoft.Ink.Stroke mStroke in newSelection)
            {
                List<Shape> labels = sketchPanel.InkSketch.GetSketchSubstrokeByInkId(mStroke.Id).ParentShapes;

                foreach (Sketch.Shape labelShape in labels)
                {
                    Sketch.Substroke[] labelSubstrokes = labelShape.Substrokes;
                    foreach (Sketch.Substroke substroke in labelSubstrokes)
                    {
                        Microsoft.Ink.Stroke toModify = sketchPanel.InkSketch.GetInkStrokeBySubstrokeId(substroke.XmlAttrs.Id.Value);
                        toModify.DrawingAttributes.Width = this.thickenedLabelAttributes.Width;
                        toModify.DrawingAttributes.Height = this.thickenedLabelAttributes.Height;
                        /*
                        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
                            this.thickenedLabelAttributes.Width;
                        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
                            this.thickenedLabelAttributes.Height;
                        */
                        this.thickenedStrokes.Add(substroke);
                    }
                }
            }

            sketchPanel.InkPicture.Invalidate();
        }


        public virtual void UnThickenLabel(Microsoft.Ink.Strokes previousSelection)
        {
            /*foreach (Microsoft.Ink.Stroke mStroke in previousSelection)
            {
                ArrayList labels = (this.mIdToSubstroke[mStroke.Id] as Sketch.Substroke).ParentShapes;

                if (labels.Count == 0)
                {
                    mStroke.DrawingAttributes.Width = this.sketchInk.DefaultDrawingAttributes.Width;
                    mStroke.DrawingAttributes.Height = this.sketchInk.DefaultDrawingAttributes.Height;
                }
				
                foreach (Sketch.Shape labelShape in labels)
                {
                    Sketch.Substroke[] labelSubstrokes = labelShape.Substrokes;
				
                    foreach (Sketch.Substroke substroke in labelSubstrokes)
                    {
                        // IMPORTANT: For some reason we need the following line or our colors do not update
                        // correctly. THIS IS A HACK
                        // It's also broken, since we update all the Strokes in all labels with the color
                        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes =
                            mStroke.DrawingAttributes;

                        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
                            this.sketchInk.DefaultDrawingAttributes.Width;
                        (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
                            this.sketchInk.DefaultDrawingAttributes.Height;
                    }
                }
            }*/

            foreach (Substroke substroke in this.thickenedStrokes)
            {
                Microsoft.Ink.Stroke toModify = sketchPanel.InkSketch.GetInkStrokeBySubstrokeId(substroke.XmlAttrs.Id.Value);
                //toModify.DrawingAttributes.Width = this.sketchInk.DefaultDrawingAttributes.Width;
                //toModify.DrawingAttributes.Height = this.sketchInk.DefaultDrawingAttributes.Height;
                toModify.DrawingAttributes.Width = sketchPanel.InkPicture.DefaultDrawingAttributes.Width;
                toModify.DrawingAttributes.Height = sketchPanel.InkPicture.DefaultDrawingAttributes.Height;

                /*
                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Width = 
                    this.sketchInk.DefaultDrawingAttributes.Width;
                (this.substrokeIdToMStroke[substroke.XmlAttrs.Id] as Microsoft.Ink.Stroke).DrawingAttributes.Height = 
                    this.sketchInk.DefaultDrawingAttributes.Height;
                */
            }

            UpdateColors(thickenedStrokes);
            this.thickenedStrokes.Clear();

            //this.sketchInk.Invalidate();
            sketchPanel.InkPicture.Invalidate();
        }


        /// <summary>
        /// Fragment point properties
        /// </summary>
        /// 
        /// <param name="color">Color of the Fragment Points</param>
        /// <param name="thickness">Point width and height</param>
        private void InitFragPtAttributes(Color color, int thickness)
        {
            this.fragmentPtAttributes = new DrawingAttributes();
            this.fragmentPtAttributes.Color = color;
            this.fragmentPtAttributes.Width = thickness;
            this.fragmentPtAttributes.Height = thickness;
        }

        #endregion


        #endregion

        #region Events

        /// <summary>
        /// Moves labelButton to appropriate location and shows it when necessary.
        /// <see cref="Labeler.LabelerPanel.sketchInk_SelectionChanged()"/>
        /// </summary>
        private void InkPicture_SelectionChanged(object sender, EventArgs e)
        {
            if (sketchPanel.InkPicture.Selection.Count > 0)
            {
                // Calculate labelButton position
                int x, y;
                x = sketchPanel.InkPicture.Selection.GetBoundingBox().X +
                    sketchPanel.InkPicture.Selection.GetBoundingBox().Width;
                y = sketchPanel.InkPicture.Selection.GetBoundingBox().Y +
                    sketchPanel.InkPicture.Selection.GetBoundingBox().Height;

                this.editMenu.displayContextMenu(x, y);

            }
            else
            {
                this.editMenu.removeMenu();
            }
        }


        //private void sketchInk_SelectionChanged(object sender, EventArgs e)
        //{
        //    if (this.sketchInk.Selection.Count > 0)
        //    {
        //        int x, y;
        //        x = this.sketchInk.Selection.GetBoundingBox().X + 
        //            this.sketchInk.Selection.GetBoundingBox().Width;
        //        y = this.sketchInk.Selection.GetBoundingBox().Y + 
        //            this.sketchInk.Selection.GetBoundingBox().Height;

        //        System.Drawing.Point bottomRight = new System.Drawing.Point(x, y);
        //        this.sketchInk.Renderer.InkSpaceToPixel(this.sketchInk.CreateGraphics(),
        //            ref bottomRight);

        //        bottomRight.X -= 15 - sketchPanel.AutoScrollPosition.X;
        //        bottomRight.Y -= 2 - sketchPanel.AutoScrollPosition.Y;
        //        this.labelButton.Location = bottomRight;

        //        this.labelButton.Visible = true;
        //        this.labelButton.BringToFront();
        //    }
        //    else
        //    {
        //        this.labelButton.Visible = false;
        //    }
        //}

        //#region InkResize

        //private bool zoomIn;

        ///// <summary>
        ///// How much does the zoom button zoom in?
        ///// </summary>
        //private static float SCALE_FACTOR = 2.0F;

        ///// <summary>
        ///// Previous scaling factor. Used in resizing the Ink and the Form.
        ///// </summary>
        //private float prevScale = 1.0f;

        ///// <summary>
        ///// New scaling factor. Used in resizing the Ink and the Form.
        ///// </summary>
        //private float newScale = 1.0f;

        ///// <summary>
        ///// Total scaling factor. Used in resizing the Ink and the Form.
        ///// </summary>
        //private float totalScale = 1.0f;

        //private void LabelerPanel_Resize(object sender, System.EventArgs e)
        //{
        //    InkResize();
        //}


        //private void InkResize()
        //{
        //    InkResize(1.0);
        //}

        //private void InkResize(double scaleFactor)
        //{
        //    //const double MARGIN = 0.03;

        //    // Actual stroke bounding box (in Ink Space)
        //    Rectangle strokeBoundingBox = sketchInk.Ink.Strokes.GetBoundingBox(BoundingBoxMode.PointsOnly);
        //    int strokeWidth = strokeBoundingBox.Width;
        //    int strokeHeight = strokeBoundingBox.Height;

        //    int inkWidth = sketchPanel.Width;
        //    int inkHeight = sketchPanel.Height;

        //    System.Drawing.Graphics g = sketchPanel.CreateGraphics();

        //    if (strokeWidth != 0
        //        && strokeHeight != 0
        //        && inkWidth != 0 && inkHeight != 0)
        //    {
        //        // Convert the rendering space from Ink Space to Pixels
        //        System.Drawing.Point bottomRight = new System.Drawing.Point((int)(strokeWidth), (int)(strokeHeight));
        //        sketchInk.Renderer.InkSpaceToPixel(g, ref bottomRight);
        //        bottomRight.X += 50;
        //        bottomRight.Y += 50;

        //        System.Drawing.Point topLeft = new System.Drawing.Point(0, 0);
        //        sketchInk.Renderer.InkSpaceToPixel(g, ref topLeft);

        //        System.Drawing.Point scalePt = new System.Drawing.Point(bottomRight.X - topLeft.X,
        //            bottomRight.Y - topLeft.Y);

        //        // Scale the rendered strokes by the width scaling factor
        //        float xScale = (float)inkWidth * (float)scaleFactor / (float)scalePt.X;
        //        float yScale = (float)inkHeight * (float)scaleFactor / (float)scalePt.Y;

        //        float scale = Math.Min(yScale, xScale);

        //        // Scale the stroke rendering by the scaling factor
        //        sketchInk.Renderer.Scale(scale, scale, false);

        //        // Scale the fragment point rendering by the scaling factor
        //        overlayInk.Renderer.Scale(scale, scale, true);

        //        // Move the Renderer's (x,y) origin so that we can see the entire Sketch
        //        float toMoveX = (this.inkMovedX * scale) - this.inkMovedX;
        //        float toMoveY = (this.inkMovedY * scale) - this.inkMovedY;
        //        System.Drawing.Point toMove = new System.Drawing.Point((int)toMoveX + 1, (int)toMoveY + 1);
        //        //sketchInk.Renderer.InkSpaceToPixel(g, ref toMove);
        //        sketchInk.Renderer.Move(toMove.X, toMove.Y);

        //        this.inkMovedX = (this.inkMovedX * scale);
        //        this.inkMovedY = (this.inkMovedY * scale);

        //        Size siz = sketchInk.Size;
        //        siz.Height = (int)(siz.Height * scale);
        //        siz.Width = (int)(siz.Width * scale);
        //        sketchInk.Size = siz;

        //        // Expand the InkPicture to encompass the entire Panel
        //        if (sketchInk.Width < sketchPanel.Width)
        //            sketchInk.Width = sketchPanel.Width;
        //        if (sketchInk.Height < sketchPanel.Height)
        //            sketchInk.Height = sketchPanel.Height;

        //        // Update the scaling factors
        //        totalScale *= scale;
        //        prevScale = totalScale;

        //        if (zoomIn)
        //            closeRadius /= scale;

        //        // Update the user-displayed zoom
        //        //zoomTextBox.Text = totalScale.ToString();
        //        sketchInk.Refresh();
        //    }
        //}
        //#endregion



        private void InkPicture_SelectionMoving(object sender, InkOverlaySelectionMovingEventArgs e)
        {
            this.selectionMoving = true;
        }


        ///// Handles the InkOverlay to ensure that displayed strokes have not been moved.
        ///// Code pulled from: http://windowssdk.msdn.microsoft.com/en-us/library/microsoft.ink.inkoverlay.selectionmoved.aspx
        ///// </summary>
        ///// <param name="sender">Reference to the object that raised the event</param>
        ///// <param name="e">Selection moving event</param>
        //private void InkPicture_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
        //{
        //    // Get the selection's bounding box
        //    //System.Drawing.Rectangle newBounds = this.sketchInk.Selection.GetBoundingBox();
        //    System.Drawing.Rectangle newBounds = sketchPanel.InkPicture.Selection.GetBoundingBox();

        //    // Move to back to original spot
        //    sketchPanel.InkPicture.Selection.Move(e.OldSelectionBoundingRect.Left - newBounds.Left,
        //        e.OldSelectionBoundingRect.Top - newBounds.Top);

        //    // Trick to insure that selection handles are updated
        //    this.sketchInk.Selection = this.sketchInk.Selection;
        //    sketchPanel.InkPicture.Selection = sketchPanel.InkPicture.Selection;

        //    // Reset our moving flag
        //    this.selectionMoving = false;
        //}


        private void InkPicture_SelectionResizing(object sender, InkOverlaySelectionResizingEventArgs e)
        {
            this.selectionResizing = true;
        }


        ///// <summary>
        ///// Handles the InkOverlay to ensure that displayed strokes have not been resized.
        ///// </summary>
        ///// <param name="sender">Reference to the object that raised the event</param>
        ///// <param name="e">Selection resizing event</param>
        //private void InkPicture_SelectionResized(object sender, InkOverlaySelectionResizedEventArgs e)
        //{
        //    // Move to back to original spot
        //    sketchPanel.InkPicture.Selection.ScaleToRectangle(e.OldSelectionBoundingRect);

        //    // Trick to insure that selection handles are updated
        //    sketchPanel.InkPicture.Selection = sketchPanel.InkPicture.Selection;

        //    // Reset our resizing flag
        //    this.selectionResizing = false;
        //}


        private void InkPicture_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                this.mouseDown = true;
        }


        private void InkPicture_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Drawing.Point mousePosition = new System.Drawing.Point(e.X, e.Y);

            // Display a ToolTip at the current cursor coordinates
            if (show_tooltip)
                DisplayToolTip(mousePosition);

            //if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    if (lassoPoints.Count == 0 || PointDistance(lassoPoints.Peek(), mousePosition) > LASSO_THRESHOLD)
            //    {
            //        lassoPoints.Push(mousePosition);
            //    }
            //}
        }


        private void InkPicture_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Handles hiding our label menu if we clicked outside its bounds
            if (this.labelMenu.Visible)
            {
                System.Drawing.Rectangle labelBoundingBox = new Rectangle(this.labelMenu.Location,
                    this.labelMenu.Size);

                if (labelBoundingBox.Contains(new System.Drawing.Point(e.X, e.Y)) && this.lassoPoints != null)
                {
                    this.lassoPoints.Clear();
                    this.mouseDown = false;
                    return;
                }
                else
                {
                    this.labelMenu.Hide();
                }
            }

            // Clear our current Selection
            // I don't know why this is required, but if we don't our strokesSelected becomes
            // bloated for some odd reason...
            this.sketchPanel.InkPicture.Selection.Clear();

            // The maximum number of points allowed in the lasso for the MouseUp
            // to be considered a click
            //const int LASSOTHRESHOLD = 10;

            //// Cursor point (in ink space coordinates)
            //System.Drawing.Point cursorPt = new System.Drawing.Point(e.X, e.Y);
            //sketchPanel.InkPicture.Renderer.PixelToInkSpace(sketchPanel.InkPicture.CreateGraphics(), ref cursorPt);

            //Microsoft.Ink.Strokes justClicked;

            //if (this.lassoPoints.Count < LASSOTHRESHOLD)
            //{
            //    // We performed a click- select the strokes next to the pointer.
            //    this.clicked = true;

            //    // Get the strokes close to the point
            //    justClicked = sketchPanel.InkPicture.Ink.HitTest(cursorPt, closeRadius);
            //    //Microsoft.Ink.Strokes justClicked = this.sketchInk.Ink.HitTest(cursorPt, closeRadius);
            //}
            //else
            //{
            //    // We made a lasso selection, select the points inside the lasso.
            //    this.clicked = false;
            //    justClicked = sketchPanel.InkPicture.Ink.HitTest(lassoPoints.ToArray(), (float) 1.0);
            //}

            
            //foreach (Microsoft.Ink.Stroke s in justClicked)
            //{
            //    Console.WriteLine(s.Id);
            //}

            //// Initialize the strokesClicked holder
            //if (this.strokesClicked == null)
            //{
            //    this.strokesClicked = justClicked;
            //    sketchPanel.InkPicture.Selection = this.strokesClicked;
            //    //this.sketchInk.Selection = this.strokesClicked;
            //}

            //// Checks to see if we haven't clicked on anything
            //else if (justClicked.Count == 0)
            //{
            //    if (this.clicked)
            //    {
            //        // Radius (in ink space coordiantes)
            //        float farRadius = (float)(2 * closeRadius);

            //        justClicked = sketchPanel.InkPicture.Ink.HitTest(cursorPt, farRadius);

            //        // Clear the strokesClicked holder if we clicked far enough away
            //        // from strokes
            //        if (justClicked.Count == 0)
            //            this.strokesClicked.Clear();

            //        sketchPanel.InkPicture.Selection = this.strokesClicked;
            //        //this.sketchInk.Selection = this.strokesClicked;
            //    }
            //    else
            //    {
            //        // We used a lasso and didn't select anything- clear the selection.
            //        this.strokesClicked.Clear();
            //        sketchPanel.InkPicture.Selection = this.strokesClicked;
            //    }
            //}
            //else
            //{
            //    if(this.clicked)
            //    {
            //        // Add or remove what we just clicked on
            //        foreach (Microsoft.Ink.Stroke s in justClicked)
            //        {
            //            if (this.strokesClicked.Contains(s))
            //                this.strokesClicked.Remove(s);
            //            else
            //                this.strokesClicked.Add(s);
            //        }

            //        sketchPanel.InkPicture.Selection = this.strokesClicked;
            //        //this.sketchInk.Selection = this.strokesClicked;
            //    }
            //    else
            //    {
            //        // We used a lasso. Select what's inside of it.
            //        this.strokesClicked = justClicked;

            //        sketchPanel.InkPicture.Selection = this.strokesClicked;
            //    }
            //}

            //// Trick to make sure the selection gets updated anyway.
            //sketchPanel.InkPicture.Selection = sketchPanel.InkPicture.Selection;
            ////this.sketchInk.Selection = this.sketchInk.Selection;

            //// Clear our lassoPoints holder
            //this.lassoPoints.Clear();
            //this.mouseDown = false;
        }


        //private void InkPicture_SelectionChanging(object sender, InkOverlaySelectionChangingEventArgs e)
        //{
        //    //// Don't do anything if we were moving or resizing the selection
        //    //if (this.mouseDown || this.selectionMoving || this.selectionResizing)
        //    //{
        //    //    UnThickenLabel(sketchPanel.InkPicture.Selection);
        //    //    return;
        //    //}

        //    // If we've clicked outside our selection or not close enough to a stroke
        //    if (e.NewSelection.Count == 0)
        //    {
        //        UnThickenLabel(sketchPanel.InkPicture.Selection);

        //        this.strokesClicked.Clear();
        //    }

        //    // If we've clicked on a stroke or group of closely connected strokes
        //    else if (this.clicked)
        //    {
        //        UnThickenLabel(sketchPanel.InkPicture.Selection);

        //        foreach (Microsoft.Ink.Stroke s in this.strokesClicked)
        //        {
        //            if (!e.NewSelection.Contains(s))
        //                e.NewSelection.Add(s);
        //        }

        //        ThickenLabel(e.NewSelection);
        //    }

        //    // If we've performed a lasso selection
        //    else
        //    {
        //        UnThickenLabel(sketchPanel.InkPicture.Selection);

        //        this.strokesClicked.Clear();
        //        this.strokesClicked.Add(e.NewSelection);

        //        ThickenLabel(this.strokesClicked);
        //    }
        //}


        private void labelButton_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Drawing.Point topLeft = this.labelButton.Location;

            //if (topLeft.X + this.labelMenu.Width > this.sketchInk.Width)
            if (topLeft.X + this.labelMenu.Width > sketchPanel.InkPicture.Width)
                topLeft.X -= (this.labelMenu.Width - this.labelButton.Width);
            //if (topLeft.Y + this.labelMenu.Height > this.sketchInk.Height)
            if (topLeft.Y + this.labelMenu.Height > sketchPanel.InkPicture.Height)
                topLeft.Y -= (this.labelMenu.Height - this.labelButton.Height);

            //this.labelMenu.UpdateLabelMenu(this.sketchInk.Selection);
            this.labelMenu.UpdateLabelMenu(sketchPanel.InkPicture.Selection);

            // Should move the scroll to the top somehow

            this.labelMenu.Location = topLeft;
            this.labelMenu.BringToFront();
            this.labelMenu.Show();

            this.labelMenu.Focus();
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
            //this.sketchInk.Renderer.PixelToInkSpace(sketchPanel.CreateGraphics(), ref mousePt);
            sketchPanel.InkPicture.Renderer.PixelToInkSpace(sketchPanel.CreateGraphics(), ref mousePt);

            // Get the closest Microsoft Stroke (in the InkOverlay)
            float strokePt, distance;

            Microsoft.Ink.Stroke closestMSubstroke = null;
            // This NearestPoint function seems to break often, wrapping in a try/catch loop
            try
            {
                 closestMSubstroke = sketchPanel.InkPicture.Ink.NearestPoint(mousePt, out strokePt, out distance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            //	this.sketchInk.Ink.NearestPoint(mousePt, out strokePt, out distance);

            Substroke closestSubstroke = new Substroke();

            try
            {
                // Get the Sketch's corresponding Substroke
                closestSubstroke = (Substroke) sketchPanel.InkSketch.GetSketchSubstrokeByInkId(closestMSubstroke.Id);
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
                //this.toolTip.SetToolTip(this.sketchInk, toolTipLabel);
                this.toolTip.SetToolTip(sketchPanel.InkPicture, toolTipLabel);
                this.toolTip.Active = true;
            }
            else
            {
                // Don't show the ToolTip if we aren't close enough to any Substroke
                this.toolTip.Active = false;
            }
        }
        #endregion

        #region Getters


        public Microsoft.Ink.DrawingAttributes FragmentPtAttributes
        {
            get { return this.fragmentPtAttributes; }
        }

        public Microsoft.Ink.DrawingAttributes ThickenedLabelAttributes
        {
            get { return this.thickenedLabelAttributes;}
        }


        public List<Substroke> ThickenedStrokes
        {
            get { return this.thickenedStrokes; }
        }

        public Microsoft.Ink.Strokes Selection
        {
            get
            {
                //return this.sketchInk.Selection;
                return sketchPanel.InkPicture.Selection;
            }
            set
            {
                sketchPanel.InkPicture.Selection = value;
            }
        }


        public Sketch.Sketch Sketch
        {
            get
            {
                return sketchPanel.InkSketch.Sketch;
            }
        }

        public Substroke getSubstrokeByMId(int mId)
        {
            return sketchPanel.InkSketch.GetSketchSubstrokeByInkId(mId);
        }

        public Microsoft.Ink.Stroke getInkStrokeBySId(Guid? sId)
        {
            return sketchPanel.InkSketch.GetInkStrokeBySubstrokeId(sId);
        }

        public Dictionary<int, Guid?> MIdtoSubstroke
        {
            get
            {
                return sketchPanel.InkSketch.Ink2sketchStr;
            }
        }

        public Dictionary<Guid?, int> SubstrokeIdToMStroke
        {
            get
            {
                return sketchPanel.InkSketch.SketchStr2ink;
            }
        }


        public Microsoft.Ink.InkPicture SketchInk
        {
            get
            {
                //return this.sketchInk;
                return sketchPanel.InkPicture;
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


        public LabelMenu LabelMenu
        {
            get
            {
                return this.labelMenu;
            }
        }

        public Button LabelButton
        {
            get { return this.labelButton; }
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
            if (sketchPanel.InkSketch.Sketch == null)
                return;
            if (checkStrokeLabeling == 2)
            {
                int n = -1;
                Shape[] shapes = sketchPanel.InkSketch.Sketch.Shapes;
                for (int i = 0; i < shapes.Length; ++i)
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
                Shape[] shapes = sketchPanel.InkSketch.Sketch.Shapes;
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

        #region Zoom
        ///// <summary>
        ///// Change the zoom factor
        ///// <returns>The zoom status of the sketch after this action is applied</returns>
        ///// </summary>
        //public bool changeZoom()
        //{
        //    if (zoomIn)
        //        zoomIn = false;
        //    else
        //        zoomIn = true;
        //    InkResize(zoomIn ? SCALE_FACTOR : 1.0);
        //    return zoomIn;
        //}

        ///// <summary>
        ///// Change the zoom
        ///// </summary>
        ///// <param name="to">In or out?</param>
        //public void changeZoom(bool to)
        //{
        //    zoomIn = to;
        //    InkResize(zoomIn ? SCALE_FACTOR : 1.0);
        //}

        ///// <summary>
        ///// Zoom to a specific percent
        ///// </summary>
        ///// <param name="zoomFactor">The zoom factor</param>
        //public void changeZoom(double zoomFactor)
        //{
        //    if (zoomFactor > 1)
        //        zoomIn = true;
        //    InkResize(zoomFactor);
        //}
        #endregion

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

        #endregion

        #region Updates

        public void UpdateColors()
        {
            if (sketchPanel.InkSketch.Sketch != null)
                UpdateColors(sketchPanel.InkSketch.Sketch.SubstrokesL);
        }

        public void UpdateColors(List<Substroke> substrokes)
        {
            if (this.domainInfo != null && this.sketchPanel.InkSketch.Sketch != null)
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
                    
                    sketchPanel.InkSketch.GetInkStrokeBySubstrokeId(substroke.XmlAttrs.Id.Value).DrawingAttributes.Color = bestColor;
                }
                sketchPanel.InkPicture.Refresh();
            }
        }

        //public void UpdateFragmentCorners(Dictionary<Sketch.Stroke, List<int>> fragStrokeToCorners)
        //{
        //    // Clear the current InkOverlay
        //    overlayInk.Ink.DeleteStrokes();

        //    // Split the Stroke at the new fragment points
        //    foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in fragStrokeToCorners)
        //    {
        //        Sketch.Stroke stroke = entry.Key;
        //        List<int> corners = entry.Value;

        //        bool notEqualOrNull = false;
        //        List<int> sCorn;
        //        bool haveCorners = this.strokeToCorners.TryGetValue(stroke, out sCorn);
        //        if (haveCorners)
        //        {
        //            notEqualOrNull = true;
        //        }
        //        else if (!haveCorners && corners != null && corners.Count > 0)
        //        {
        //            notEqualOrNull = true;
        //        }
        //        else if (haveCorners && corners != null
        //            && corners.ToArray() != sCorn.ToArray())
        //        {
        //            notEqualOrNull = true;
        //        }

        //        if (notEqualOrNull)
        //        {
        //            if ((corners == null || corners.Count == 0)
        //                && sCorn.Count > 0)
        //                this.strokeToCorners.Remove(stroke);
        //            else
        //                this.strokeToCorners[stroke] = corners;

        //            // Remove all of the substrokes within our InkPicture and hashtables
        //            foreach (Sketch.Substroke substroke in stroke.Substrokes)
        //            {
        //                this.mIdToSubstroke.Remove(this.substrokeIdToMStroke[substroke.XmlAttrs.Id.Value].Id);
        //                sketchPanel.InkPicture.Ink.DeleteStroke(this.substrokeIdToMStroke[substroke.XmlAttrs.Id.Value]);
        //                this.substrokeIdToMStroke.Remove(substroke.XmlAttrs.Id.Value);
        //            }

        //            // Merge the substrokes together
        //            bool labelSame = stroke.MergeSubstrokes();
        //            if (!labelSame)
        //            {
        //                System.Windows.Forms.DialogResult ok = System.Windows.Forms.MessageBox.Show(
        //                    "Labels removed from Stroke " + stroke.XmlAttrs.Id.ToString() +
        //                    "\ndue to fragmenting a non-uniformly labeled stroke",
        //                    "Important",
        //                    System.Windows.Forms.MessageBoxButtons.OK,
        //                    System.Windows.Forms.MessageBoxIcon.Exclamation);
        //            }

        //            // Fragment the substroke at the specified indices
        //            if (corners != null && corners.Count > 0)
        //                stroke.SplitStrokeAt(corners.ToArray());

        //            // Draw the substrokes in the InkPicture
        //            foreach (Sketch.Substroke substroke in stroke.Substrokes)
        //            {
        //                Sketch.Point[] substrokePts = substroke.Points;
        //                System.Drawing.Point[] pts = new System.Drawing.Point[substrokePts.Length];

        //                int len2 = pts.Length;
        //                for (int i = 0; i < len2; ++i)
        //                {
        //                    pts[i] = new System.Drawing.Point((int)(substrokePts[i].X),
        //                        (int)(substrokePts[i].Y));
        //                }

        //                // Allows us to look up a Sketch.Stroke from its Microsoft counterpart
        //                int mId = this.SketchInk.Ink.Strokes[this.SketchInk.Ink.Strokes.Count - 1].Id;
        //                this.mIdToSubstroke.Add(mId, substroke);
        //                this.substrokeIdToMStroke.Add((System.Guid)substroke.XmlAttrs.Id,
        //                    sketchPanel.InkPicture.Ink.Strokes[sketchPanel.InkPicture.Ink.Strokes.Count - 1]);
        //                //this.sketchInk.Ink.Strokes[sketchInk.Ink.Strokes.Count - 1]);
        //            }

        //            // Update the colors associated with the substrokes
        //            UpdateColors(new List<Substroke>(stroke.Substrokes));
        //        }
        //    }

        //    // Add the InkOverlay points we'll be drawing
        //    List<System.Drawing.Point> ptsToDraw = new List<System.Drawing.Point>();

        //    Sketch.Stroke[] strokes = this.sketch.Strokes;
        //    foreach (Sketch.Stroke stroke in strokes)
        //    {
        //        List<int> corners;
        //        if (this.strokeToCorners.TryGetValue(stroke, out corners))
        //        {
        //            Sketch.Point[] points = stroke.Points;

        //            foreach (int index in corners)
        //            {
        //                ptsToDraw.Add(new System.Drawing.Point((int)points[index].X, (int)points[index].Y));
        //            }
        //        }
        //    }

        //    System.Drawing.Point[] fragPts = ptsToDraw.ToArray();

        //    // Create strokes consisting of one point each.
        //    // This way we can draw the points in our InkPicture and correctly scale the points
        //    // accordingly.
        //    foreach (System.Drawing.Point pt in fragPts)
        //    {
        //        System.Drawing.Point[] p = new System.Drawing.Point[1];
        //        p[0] = pt;

        //        overlayInk.Ink.CreateStroke(p);
        //    }

        //    // Render each point
        //    foreach (Microsoft.Ink.Stroke s in overlayInk.Ink.Strokes)
        //    {
        //        s.DrawingAttributes = this.fragmentPtAttributes;
        //    }

        //    // Update the Hashtable
        //    foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in fragStrokeToCorners)
        //    {
        //        Sketch.Stroke key = entry.Key;
        //        List<int> val = entry.Value;

        //        if (val == null || val.Count == 0)
        //            this.strokeToCorners.Remove(key);
        //        else
        //            this.strokeToCorners[key] = new List<int>(val);
        //    }

        //    sketchPanel.InkPicture.Invalidate();
        //    sketchPanel.InkPicture.Refresh();
        //    this.SketchInk.Invalidate();
        //    this.SketchInk.Refresh();
        //}


        /// <summary>
        /// OLD AND SHOULD NOT  BE USED!!! it is TODO use code above if better
        /// </summary>
 
        public void UpdateFragmentCorners()
        {

            // Clear the current InkOverlay
            overlayInk.Ink.DeleteStrokes();

            List<System.Drawing.Point> ptsArray = new List<System.Drawing.Point>();

            Sketch.Stroke[] strokes = this.sketchPanel.InkSketch.Sketch.Strokes;
            for (int i = 0; i < strokes.Length; i++)
            {
                List<int> corners;
                if (this.strokeToCorners.TryGetValue(strokes[i], out corners))
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

            sketchPanel.Refresh();

        }


        #endregion

    }
}
