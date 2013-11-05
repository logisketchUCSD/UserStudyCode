using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Microsoft.Ink;

using Sketch;
using msInkToHMCSketch;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SketchPanelLib
{
    /// <summary>
    /// Delegate for accepting recognition result received events
    /// from a SketchPanel.
    /// </summary>
    /// <param name="result">The recognition result received</param>
    public delegate void RecognitionResultReceivedHandler(RecognitionResult result);

    /// <summary>
    /// Delegate for handling Ink Transformation Events (e.g. move/resize) sent
    /// from a SketchPanel
    /// </summary>
    /// <param name="originalBoundingBox">bounding box of Ink before transformation</param>
    /// <param name="newBoundingBox">bounding box of Ink after transformation</param>
    /// <param name="iStrokes">the Ink strokes transformed</param>
    /// <param name="eventType">the type of tranformation event; indicates how transformVector
    /// is meant to be interpreted.</param>
    public delegate void InkTransformedEventHandler(RectangleF originalBoundingBox, RectangleF newBoundingBox, 
                            Microsoft.Ink.Strokes iStrokes, InkTransformEventType eventType);

    /// <summary>
    /// Type of Ink transformation that caused InkTranformed event.  
    /// </summary>
    public enum InkTransformEventType { 

        /// <summary>
        /// Uniform Renderer scale is applied to all strokes in Ink; 
        /// bounding box height/width change in pixel space but not 
        /// in Ink space.  Height/Width of newBoundingBox reflect
        /// the scale factor used to zoom; X/Y reflect the movement
        /// vector of the strokes (if any); originalBoundingBox 
        /// contains original Ink bounding box of all strokes in Ink.
        /// </summary>
        ZoomTransform,

        /// <summary>
        /// Move and scale is possible on abitrary ink selection;
        /// bounding box height/width (may) change in both Ink
        /// and pixel space.
        /// </summary>
        SelectionToolTransform 
    };

    /// <summary>
    /// Delegate for handling file load events sent from a SketchPanel.
    /// </summary>
    public delegate void SketchFileLoadedHandler();

    /// <summary>
    /// A Panel for sketching recognizeable diagrams.  
    /// </summary>
    public class SketchPanel : System.Windows.Forms.Panel
    {
        #region Internals

        /// <summary>
        /// Stores the main sketch for this panel
        /// </summary>
        protected InkPictureSketch inkSketch;

        /// <summary>
        /// The Ink display for this sketch
        /// </summary>
        protected Labeler.mInkPicture inkPic;

        /// <summary>
        /// Recognition trigger event.  This panel publishes to this event 
        /// whenever the user triggers recognition.
        /// </summary>
        public event RecognitionTriggerEventHandler RecognitionTriggered;

        /// <summary>
        /// Recognition result received event.  This panel publishes to this
        /// event whenever a recognition result is received.  Feedback mechanisms
        /// should subscribe to this event.
        /// </summary>
        public event RecognitionResultReceivedHandler ResultReceived;

        /// <summary>
        /// Ink Transform event.  This panel publishes to this event
        /// after the panel has zoomed in or out, or when 
        /// ink has been moved or resized.  This event
        /// is published after the transformation has occurred
        /// but before the panel has refreshed.
        /// </summary>
        public event InkTransformedEventHandler InkTransformed;

        /// <summary>
        /// Sketch file loaded event.  This panel publishes to this
        /// event whenever the user loads a sketch file into this panel
        /// from disk.
        /// </summary>
        public event SketchFileLoadedHandler SketchFileLoaded;

        /// <summary>
        /// Stores the time at which the user last edited the
        /// conetnts of this panel in DateTime FileTime format.  
        /// </summary>
        protected long lastFocus;


        #endregion


        #region Constructor and Initialization 

        /// <summary>
        /// Constructor.  Creates a new (Sketch)Panel.
        /// </summary>
        public SketchPanel()
            : base()
        {
            // Set basic panel properties
            this.AutoScroll = true;
            this.Resize += new EventHandler(SketchPanel_Resize);

            // Initialize Ink and Sketch
            this.InitPanel();
        }

        /// <summary>
        /// Deletes the current sketch and restores this panel to its original state.
        /// Useful for restarting this panel in order to start over and create a new
        /// sketch.
        /// </summary>
        public virtual void InitPanel()
        {
            // Set double buffering of Panel
            this.SetStyle(ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer, true);

            // Set up InkPicture
            inkPic = new Labeler.mInkPicture();
            this.Controls.Clear();
            this.Controls.Add(this.inkPic);
            setDefaultInkPicProps();

            // Init InkSketch
            inkSketch = new InkPictureSketch(inkPic);

            // Hook into ink events
            attachToInkPicure();

            // Zero the focus parameter
            lastFocus = 0L;
        }

        /// <summary>
        /// (Re)Sets default InkPicture properties (e.g. background color, stroke color, etc)
        /// </summary>
        private void setDefaultInkPicProps()
        {
            inkPic.BackColor = SketchPanelConstants.DefaultBackColor;
            inkPic.Width = this.Width;
            inkPic.Height = this.Height;
            inkPic.DefaultDrawingAttributes.Color = SketchPanelConstants.DefaultInkColor;
            inkPic.EditingMode = InkOverlayEditingMode.Ink;
            inkPic.EraserMode = InkOverlayEraserMode.StrokeErase;
        }

        /// <summary>
        /// (Re)Hooks into InkPicture events
        /// </summary>
        private void attachToInkPicure()
        {
            inkPic.Stroke += new InkCollectorStrokeEventHandler(inkPic_Stroke);
            inkPic.StrokesDeleted += new InkOverlayStrokesDeletedEventHandler(inkPic_StrokesDeleted);
            inkPic.CursorInRange += new InkCollectorCursorInRangeEventHandler(inkPic_CursorInRange);
            inkPic.SelectionMoved += new InkOverlaySelectionMovedEventHandler(inkPic_SelectionMoved);
            inkPic.SelectionResized += new InkOverlaySelectionResizedEventHandler(inkPic_SelectionResized);
            inkPic.SelectionChanged += new InkOverlaySelectionChangedEventHandler(inkPic_SelectionChanged);
        }

        #endregion


        #region Ink event handling

        /// <summary>
        /// Resizes InkPicture when necessary (for scrolling)
        /// </summary>
        protected void inkPic_StrokesDeleted(object sender, EventArgs e)
        {
            this.resizeInkPicture();

            updateFocus();
        }

        /// <summary>
        /// Resizes InkPicture when necessary (for scrolling)
        /// </summary>
        protected void inkPic_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            this.resizeInkPicture();

            updateFocus();
        }

        /// <summary>
        /// Resizes InkPicture when necessary (for scrolling) and notifies subscribers
        /// </summary>
        protected void inkPic_SelectionResized(object sender, InkOverlaySelectionResizedEventArgs e)
        {
            Rectangle oldBB = e.OldSelectionBoundingRect;
            Rectangle newBB = inkPic.Selection.GetBoundingBox();

            InkTransformedEventHandler xformEvent = InkTransformed;
            if (xformEvent != null)
                xformEvent(oldBB, newBB, inkPic.Selection, InkTransformEventType.SelectionToolTransform);

            this.resizeInkPicture();

            updateFocus();
        }

        /// <summary>
        /// Resizes InkPicture when necessary (for scrolling) and notifies subscribers
        /// </summary>
        protected void inkPic_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
        {
            Rectangle oldBB = e.OldSelectionBoundingRect;
            Rectangle newBB = inkPic.Selection.GetBoundingBox();

            InkTransformedEventHandler xformEvent = InkTransformed;
            if (xformEvent != null)
                xformEvent(oldBB, newBB, inkPic.Selection, InkTransformEventType.SelectionToolTransform);

            this.resizeInkPicture();

            updateFocus();
        }

        /// <summary>
        /// Updates the focus time of this panel
        /// </summary>
        protected void inkPic_SelectionChanged(object sender, EventArgs e)
        {
            updateFocus();
        }

        /// <summary>
        /// Updates when this panel was last in focus (e.g. the user
        /// added/erased an Ink stroke or modified a selection).
        /// </summary>
        protected void updateFocus()
        {
            lastFocus = DateTime.Now.ToFileTime();
        }

        /// <summary>
        /// Resizes the InkPicture to the bounding box of the Ink
        /// or the size of this panel (whichever is larger).  Call this
        /// after transforming the ink in a way that might change
        /// the ink's bounding box.
        /// 
        /// TODO: resize such that we catch (allow scrolling for) 
        /// ink with negative coordinate values.
        /// </summary>
        private void resizeInkPicture()
        {
            // Get lower right corner (x,y) of Ink
            Rectangle currentInkBoundingBox = inkPic.Ink.GetBoundingBox();
            System.Drawing.Point lowerRight = new System.Drawing.Point(currentInkBoundingBox.Right, currentInkBoundingBox.Bottom);
            lowerRight.X += SketchPanelConstants.SketchPaddingInk;
            lowerRight.Y += SketchPanelConstants.SketchPaddingInk;
            using (Graphics g = inkPic.CreateGraphics())
            {
                inkPic.Renderer.InkSpaceToPixel(g, ref lowerRight);
            }

            // For now, only make inkPic bigger.
            if (lowerRight.X > Math.Max(inkPic.Width, this.Width))
            {
                inkPic.Width = lowerRight.X;
            }
            //else
            //{
            //    inkPic.Width = this.Width;
            //}

            if (lowerRight.Y > Math.Max(inkPic.Height, this.Height))
            {
                inkPic.Height = lowerRight.Y;
            }
            //else
            //{
            //    inkPic.Height = this.Height;
            //}
        }

        /// <summary>
        /// Switches Ink editing mode to delete when the stylus is inverted.  NOTE: This feature
        /// might not be supported on all Tablets.  
        /// </summary>
        private void inkPic_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
        {
            // Using explicit comparisons to minimize the number of refreshes 
            // that the cursor event causes; only switch modes once and don't 
            // keep pushing into sketch or erase if we're already in that mode.
            if (e.Cursor.Inverted && inkPic.EditingMode == InkOverlayEditingMode.Ink)
            {
                inkPic.EditingMode = InkOverlayEditingMode.Delete;
            }
            else if (!e.Cursor.Inverted && inkPic.EditingMode == InkOverlayEditingMode.Delete)
            {
                inkPic.EditingMode = InkOverlayEditingMode.Ink;
            }
        }

        /// <summary>
        /// Invalidates the bounding box enclosing the given stroke, causing this area 
        /// of the screen to repaint.  Useful for causing a (only) small portion of the InkPicture
        /// to repaint, thus minimizing screen flicker.  
        /// </summary>
        /// <param name="iStroke">The Ink stroke to invalidate</param>
        public void InvalidateStroke(Microsoft.Ink.Stroke iStroke)
        {
            InvalidateStroke(iStroke, 0);
        }

        /// <summary>
        /// <see cref="InvalidateStroke"/>
        /// </summary>
        /// <param name="iStroke">The Ink stroke to invalidate</param>
        /// <param name="padding">Number of pixels to pad invalidation 
        /// rectangle in Ink space pixels</param>
        public void InvalidateStroke(Microsoft.Ink.Stroke iStroke, int padding)
        {
            using (Graphics g = InkPicture.CreateGraphics())
            {
                Rectangle bb = iStroke.GetBoundingBox();
                System.Drawing.Point upperLeft = new System.Drawing.Point(bb.X - padding, bb.Y - padding);
                System.Drawing.Point lowerRight = new System.Drawing.Point(bb.Right + padding, bb.Bottom + padding);
                InkPicture.Renderer.InkSpaceToPixel(g, ref upperLeft);
                InkPicture.Renderer.InkSpaceToPixel(g, ref lowerRight);

                InkPicture.Invalidate(new Rectangle(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y));
            }
        }

        #endregion


        #region Panel event handling

        /// <summary>
        /// Resizes the InkPicture whenever this panel is resized.
        /// </summary>
        void SketchPanel_Resize(object sender, EventArgs e)
        {
            if (inkPic.Width < this.Width)
                inkPic.Width = this.Width;

            if (inkPic.Height < this.Height)
                inkPic.Height = this.Height;

            //this.resizeInkPicture();
        }
        
        #endregion


        #region Standard sketch procedures and transformations (open, save, cut/copy/paste, zoom, and change background)
        
        /// <summary>
        /// Loads a sketch file into the panel and resizes the display to
        /// fill the screen.  Deletes the current contents of the file.
        /// 
        /// Use this method instead of InkSketch.LoadFile() so that 
        /// the display will be refreshed properly.  Loads both Sketch XML
        /// and Journal files.
        /// 
        /// Precondition: given file path is valid and readable.  Does nothing
        /// if file path is null.
        /// </summary>
        /// <param name="filepath">Reads from this file path.</param>
        public void LoadSketch(string filepath)
        {
            if (filepath == null)
                return;

            loadSketch(filepath, false, true);

            SketchFileLoadedHandler loaded = SketchFileLoaded;
            if (loaded != null)
                loaded();

            ZoomToFit();
        }

        /// <summary>
        /// Saves the current sketch to the specified file path in XML format.
        /// Wrapper for InkSketch.SavePanel().  
        /// 
        /// Precondition: given file path is valid and writable.  
        /// </summary>
        /// <param name="filepath">Writes to this file path.</param>
        public void SaveSketch(string filepath)
        {
            inkSketch.SaveSketch(filepath);
        }

        /// <summary>
        /// Magnifies the sketch by a preset amount and notifies listerners.
        /// </summary>
        public void ZoomIn()
        {
            Rectangle oldBB = inkPic.Ink.GetBoundingBox();
            inkPic.Renderer.Scale(SketchPanelConstants.ZoomInFactor, SketchPanelConstants.ZoomInFactor, true);
            
            InkTransformedEventHandler zoomEvent = InkTransformed;
            if (zoomEvent != null)
                zoomEvent(new RectangleF(oldBB.X, oldBB.Y, oldBB.Width, oldBB.Height), 
                    new RectangleF(0.0F, 0.0F, SketchPanelConstants.ZoomInFactor, SketchPanelConstants.ZoomInFactor),
                    inkPic.Ink.Strokes, InkTransformEventType.ZoomTransform);
            
            this.resizeInkPicture();
            inkPic.Refresh();
        }

        /// <summary>
        /// Unmagnifies the sketch by a preset amount and notifies listerners.
        /// </summary> 
        public void ZoomOut()
        {
            Rectangle oldBB = inkPic.Ink.GetBoundingBox();
            inkPic.Renderer.Scale(SketchPanelConstants.ZoomOutFactor, SketchPanelConstants.ZoomOutFactor, true);

            InkTransformedEventHandler zoomEvent = InkTransformed;
            if (zoomEvent != null)
                zoomEvent(new RectangleF(oldBB.X, oldBB.Y, oldBB.Width, oldBB.Height),
                    new RectangleF(0.0F, 0.0F, SketchPanelConstants.ZoomOutFactor, SketchPanelConstants.ZoomOutFactor),
                    inkPic.Ink.Strokes, InkTransformEventType.ZoomTransform);

            resizeInkPicture();
            inkPic.Refresh();
        }

        /// <summary>
        /// Scales the sketch to fit the current panel dimensions.
        /// </summary>
        public void ZoomToFit()
        {
            Rectangle inkRect = inkPic.Ink.GetBoundingBox();

            // Move the Ink's origin to the upper top-left corner and update InkSketch
            float moveX = -1.0F * inkRect.X + SketchPanelConstants.SketchPaddingInk;
            float moveY = -1.0F * inkRect.Y + SketchPanelConstants.SketchPaddingInk;
            inkPic.Ink.Strokes.Move(moveX, moveY);

            foreach (Microsoft.Ink.Stroke iStroke in inkPic.Ink.Strokes)
            {
                inkSketch.TransformInkStroke(iStroke);
            }

            // Scale the sketch to fill the InkImage
            System.Drawing.Point rightBottom = new System.Drawing.Point(inkRect.Right, inkRect.Bottom);
            using (Graphics g = inkPic.CreateGraphics())
            {
                inkPic.Renderer.InkSpaceToPixel(g, ref rightBottom);
            }

            System.Drawing.Point scalePt = new System.Drawing.Point(rightBottom.X - this.Left,
                rightBottom.Y - this.Top);

            // Scale the rendered strokes by the smallest (x or y) scaling factor
            float xScale = (float)(this.Width - SketchPanelConstants.SketchPaddingScreen) / (float)scalePt.X;
            float yScale = (float)(this.Height - SketchPanelConstants.SketchPaddingScreen) / (float)scalePt.Y;

            float scale = xScale < yScale ? xScale : yScale;

            inkPic.Renderer.Scale(scale, scale, true);

            // Notify subscribers
            InkTransformedEventHandler xformEvent = InkTransformed;
            if (xformEvent != null)
                xformEvent(new RectangleF(inkRect.X, inkRect.Y, inkRect.Width, inkRect.Height),
                    new RectangleF(moveX, moveY, scale, scale), 
                    inkPic.Ink.Strokes, InkTransformEventType.ZoomTransform);

            // Fix the InkPicture
            resizeInkPicture();
            inkPic.Refresh();
        }

        /// <summary>
        /// Changes background image of the InkPicture.  Throws 
        /// relevant exceptions if the file load fails.  
        /// <see cref="System.Drawing.Image.FromFile"/>.
        /// </summary>
        /// <param name="filename">The file path of the image file to load</param>
        public void ChangeBackground(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                this.inkPic.BackgroundImage = System.Drawing.Image.FromFile(filename);
                this.inkPic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Tile;
            }
            else
                this.inkPic.BackgroundImage = null;
        }

        /// <summary>
        /// Copies strokes to the clipboard in InkSerializedFormat.  Copies selected strokes;
        /// if there is no selection, then all Ink is copied.  
        /// </summary>
        public void CopyStrokes()
        {
            // If there are no strokes selected, then copy all strokes in this panel
            Microsoft.Ink.Strokes selectedStrokes;
            if (inkPic.Selection.Count != 0)
            {
                selectedStrokes = inkPic.Selection;
            }
            else
            {
                selectedStrokes = inkPic.Ink.Strokes;
            }

            // Cut the strokes
            inkPic.Ink.ClipboardCopy(selectedStrokes, InkClipboardFormats.InkSerializedFormat, InkClipboardModes.Copy);
        }
        

        /// <summary>
        /// Cuts strokes to the clipboard in InkSerializedFormat.  Cuts selected strokes;
        /// if there is no selection, then all Ink is cut.  
        /// </summary>
        public void CutStrokes()
        {
            // If there are no strokes selected, then copy all strokes in this panel
            Microsoft.Ink.Strokes selectedStrokes;
            if (inkPic.Selection.Count != 0)
            {
                selectedStrokes = inkPic.Selection;
            }
            else
            {
                selectedStrokes = inkPic.Ink.Strokes;
            }

            // Cut the strokes
            inkPic.Ink.ClipboardCopy(selectedStrokes, InkClipboardFormats.InkSerializedFormat, InkClipboardModes.Cut);

            // Remove the strokes from the InkSketch
            foreach (Microsoft.Ink.Stroke iStroke in selectedStrokes)
            {
                inkSketch.DeleteInkStroke(iStroke.Id);
            }
        }

        /// <summary>
        /// Pastes strokes from the clipboard.
        /// 
        /// Precondition: inkPic is in Ink mode.  
        /// </summary>
        public void PasteStrokes()
        {
            inkPic.Ink.ClipboardPaste();
        }

        #endregion


        #region Recognizer Inteface

        /// <summary>
        /// Handles recognition results from a recognizer.  A Recognizer will call
        /// this method whenever a result is ready.
        /// </summary>
        /// <param name="source">The source object (Recogizer) that triggering this event</param>
        /// <param name="result">The recognition result</param>
        public void SetRecognitionResult(object source, RecognitionResult result)
        {
            if (result.UserTriggered)
                this.loadSketch(result.Sketch, false, false);

            RecognitionResultReceivedHandler resultReceived = ResultReceived;
            if (resultReceived != null)
                resultReceived(result);
        }

        /// <summary>
        /// Sends the Sketch contained in this panel to the recognizer.
        /// TODO: currently has debug functionality; needs to set userTriggered properly
        /// </summary>
        public void TriggerRecognition()
        {
            RecognitionArgs args = new RecognitionArgs();
            args.UserTriggered = true;
            args.Sketch = this.inkSketch.Sketch;

            RecognitionTriggerEventHandler triggerHandler = RecognitionTriggered;

            if (triggerHandler != null)
            {
                triggerHandler(this, args);
            }
        }

        /// <summary>
        /// Loads a sketch into this Panel.  Possibly rescales the sketch to fit
        /// the current display and/or resets this panel entirely.  This method helps
        /// both display recognition results and new sketch files.  
        /// 
        /// Precondition: newSketch is not null
        /// </summary>
        /// <param name="newSketch">The Skech to load</param>
        /// <param name="rescale">True iff the sketch should be resized</param>
        /// <param name="reset">True iff the panel should be reinitialized</param>
        private void loadSketch(object newSketch, bool rescale, bool reset)
        {
            // Reinitialize the sketch panel if necessary
            if (reset)
            {
                this.setDefaultInkPicProps();
            }

            // Load the Sketch into the InkPicture
            inkPic.Enabled = false;
            if (newSketch is string)
            {
                inkSketch.LoadSketch((string)newSketch);
            }
            else if (newSketch is Sketch.Sketch)
            {
                inkSketch.LoadSketch((Sketch.Sketch)newSketch);
            }
            else if (newSketch == null)
            {
                // We should never get here; fail quietly
                Console.WriteLine("Error in SketchPanel#loadSketch(): "
                    + "given null sketch. Breaking load operation");
                return;
            }

            // Rescale the ink if needed
            if (rescale)
            {
                ZoomToFit();
            }

            inkPic.Enabled = true;

            // Resize the InkPicture
            this.resizeInkPicture();
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the sketch in this panel.
        /// </summary>
        public Sketch.Sketch Sketch
		{
			get
			{
				return inkSketch.Sketch;
			}
		}

        /// <summary>
        /// Gets this panel's InkSketch.
        /// </summary>
        public InkSketch InkSketch
        {
            get
            {
                return this.inkSketch;
            }
        }

        /// <summary>
        /// Gets this panel's InkPicture.
        /// </summary>
        public Microsoft.Ink.InkPicture InkPicture
        {
            get
            {
                return inkPic;
            }
        }

        /// <summary>
        /// Returns the time (in DateTime FileTime format) that the user last 
        /// drew a stroke on this panel.
        /// </summary>
        public long LastFocus
        {
            get
            {
                return lastFocus;
            }
        }

        /// <summary>
        /// Gets or sets the editing mode of the InkPicture.  Automatically disables
        /// the InkPicture while changing editing modes.
        /// </summary>
        public Microsoft.Ink.InkOverlayEditingMode EditingMode
        {
            get
            {
                return inkPic.EditingMode;
            }
            set
            {
                if (value == inkPic.EditingMode)
                    return;

                inkPic.Enabled = false;
                inkPic.EditingMode = value;
                inkPic.Enabled = true;
            }
        }

        #endregion
    }


    /// <summary>
    /// Stores constant parameters for SketchPanels
    /// </summary>
    public class SketchPanelConstants
    {
        /// <summary>
        /// Default units for sketch
        /// </summary>
        public const string SketchUnits = "himetric";

        /// <summary>
        /// Default background color for sketch panel
        /// </summary>
        public static Color DefaultBackColor = Color.White;

        /// <summary>
        /// Default color for ink strokes
        /// </summary>
        public static Color DefaultInkColor = Color.Black;

        /// <summary>
        /// Padding to give to newly opened sketches in
        /// ink space coordinates.
        /// </summary>
        public const int SketchPaddingInk = 250;

        /// <summary>
        /// Padding to give to newly opened sketches in
        /// screen coordinates.
        /// </summary>
        public const int SketchPaddingScreen = 25;

        /// <summary>
        /// Default Zoom-In factor, where 1.0 is nominal.
        /// </summary>
        public const float ZoomInFactor = 1.2F;

        /// <summary>
        /// Default Zoom-Out factor, where 1.0 is nominal.
        /// </summary>
        public const float ZoomOutFactor = 0.8F;
    }
}
