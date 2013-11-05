using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;

using System.Windows.Ink;
using System.Windows.Media.Imaging;


using Sketch;
using System.Windows.Controls;
using System.Windows.Media;

namespace SketchPanelLib
{
    #region Major Event Handlers

    /// <summary>
    /// Delegate for accepting recognition result received events
    /// from a SketchPanel.
    /// </summary>
    /// <param name="result">The recognition result received</param>
    public delegate void RecognitionResultReceivedHandler(RecognitionResult result);

    /// <summary>
    /// Delegate for handling file load events sent from a SketchPanel.
    /// </summary>
    public delegate void SketchFileLoadedHandler();

    /// <summary>
    /// Delegate for passing on when ink is added, removed, moved, etc.
    /// </summary>
    public delegate void InkChangedEventHandler(bool reColor);

    /// <summary>
    /// Delegate for passing on when on the fly recognition should have something drawn
    /// DrawGate in mainwindow
    /// </summary>
    public delegate void InkChangedOnFlyRecHandler();

    /// <summary>
    /// Delegate for indicating when the panel thinks it has been recognized
    /// </summary>
    /// <param name="recognized"></param>
    public delegate void RecognizeChangedEventHandler(bool recognized);

    #endregion

    /// <summary>
    /// A Panel for sketching recognizeable diagrams.  
    /// </summary>
    public class SketchPanel : System.Windows.Controls.Canvas
    {
        #region Internals

        /// <summary>
        /// Stores the main sketch for this panel
        /// </summary>
        protected InkToSketchWPF.InkCanvasSketch inkSketch;

        /// <summary>
        /// Stores the main circuit for this panel
        /// </summary>
        protected CircuitSimLib.Circuit circuit;

        /// <summary>
        /// The Ink display for this sketch
        /// </summary>
        protected InkCanvas inkCanvas;

        /// <summary>
        /// The border of the panel
        /// </summary>
        protected Border myBorder;

        /// <summary>
        /// True if sketch has been recognized, false if not
        /// </summary>
        private bool recognized = false;

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
        /// Event for drawing on the windown before recognition has happened
        /// </summary>
        public event InkChangedOnFlyRecHandler drawGates;

        /// <summary>
        /// Sketch file loaded event.  This panel publishes to this
        /// event whenever the user loads a sketch file into this panel
        /// from disk.
        /// </summary>
        public event SketchFileLoadedHandler SketchFileLoaded;

        /// <summary>
        /// Panel publishes to this event whenever ink is added or removed
        /// </summary>
        public event InkChangedEventHandler InkChanged;

        /// <summary>
        /// Stores the bounding box for the previous selection
        /// </summary>
        protected System.Windows.Rect oldBB;

        /// <summary>
        /// Keeps track of commands for undo/redo
        /// </summary>
        protected CommandManagement.CommandManager CM;

        /// <summary>
        /// Tells us when the recognize status of the panel has changed
        /// </summary>
        public RecognizeChangedEventHandler RecognizeChanged;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Constructor.  Creates a sketchPanel from scratch.
        /// </summary>
        public SketchPanel(CommandManagement.CommandManager commandManager)
            : this(commandManager, new InkToSketchWPF.InkCanvasSketch(new InkCanvas()))
        {
        }

        /// <summary>
        /// Constructor.  Creates a sketchPanel using the given commandManager and inkCanvasSketch.
        /// </summary>
        public SketchPanel(CommandManagement.CommandManager commandManager, InkToSketchWPF.InkCanvasSketch inkCanvasSketch)
            : base()
        {
            CM = commandManager;
            this.InitPanel(inkCanvasSketch);
        }

        /// <summary>
        /// Initializes the panel using the given InkCanvasSketch.
        /// </summary>
        public virtual void InitPanel(InkToSketchWPF.InkCanvasSketch inkCanvasSketch)
        {
            // Add the inkCanvas to the panel
            this.Children.Clear();
            inkSketch = inkCanvasSketch;
            inkCanvas = inkCanvasSketch.InkCanvas;
            inkSketch.InkCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            inkSketch.InkCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.Children.Add(inkSketch.InkCanvas);
            setDefaultInkPicProps();

            // Hook into ink events
            subscribeToEvents();
            Recognized = false;
        }

        /// <summary>
        /// (Re)Sets default InkCanvas properties (e.g. background color, stroke color, etc)
        /// </summary>
        private void setDefaultInkPicProps()
        {
            inkCanvas.Background = SketchPanelConstants.DefaultBackColor;
            inkCanvas.Width = this.Width;
            inkCanvas.Height = this.Height;
            inkCanvas.DefaultDrawingAttributes.Color = SketchPanelConstants.DefaultInkColor;
            EnableDrawing();
        }

        /// <summary>
        /// (Re)Hooks into InkCanvas events
        /// </summary>
        private void subscribeToEvents()
        {
            inkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(inkPic_Stroke);
            inkCanvas.StrokeErasing += new InkCanvasStrokeErasingEventHandler(inkPic_StrokeErasing);
            inkCanvas.SelectionMoved += new EventHandler(inkPic_SelectionMovedResized);
            inkCanvas.SelectionMoving += new InkCanvasSelectionEditingEventHandler(inkPic_SelectionMovingResizing);
            inkCanvas.SelectionResized += new EventHandler(inkPic_SelectionMovedResized);
            inkCanvas.SelectionResizing += new InkCanvasSelectionEditingEventHandler(inkPic_SelectionMovingResizing);
            inkCanvas.SelectionChanged += new EventHandler(inkPic_SelectionChanged);
            SizeChanged += new System.Windows.SizeChangedEventHandler(sketchPanel_SizeChanged);
        }
        #endregion

        #region Ink event handling

        private void sketchPanel_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            inkCanvas.Height = ActualHeight;
            inkCanvas.Width = ActualWidth;
            InvalidateVisual();
        }

        /// <summary>
        /// Removes strokes in an undo-able way.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void inkPic_StrokeErasing(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            CM.ExecuteCommand(new CommandList.StrokeRemoveCmd(ref inkSketch, e.Stroke));
            InkChanged(false);
        }

        /// <summary>
        /// Stroke added to ink canvas
        /// </summary>
        protected void inkPic_Stroke(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Remove it from the InkCanvas to let the command do the work
            InkCanvas.Strokes.Remove(e.Stroke);

            // Don't add the stroke if we think it's a dot
            if (e.Stroke.GetBounds().Width < 2 * e.Stroke.DrawingAttributes.Width &&
                e.Stroke.GetBounds().Height < 2 * e.Stroke.DrawingAttributes.Width)
            {
                InkChanged(false);
                return;
            }

            CM.ExecuteCommand(new CommandList.StrokeAddCmd(ref inkSketch, e.Stroke));

            InkChanged(false);
        }


        // Declare a temporary variable for use in before-event hooks
        CommandManagement.Command c;

        /// <summary>
        /// Creates the command with information from the move/resize event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void inkPic_SelectionMovingResizing(object sender, InkCanvasSelectionEditingEventArgs e)
        {
            // Create the command with arguments from before-the-event hook
            c = new CommandList.MoveResizeCmd(ref inkSketch, e.OldRectangle, e.NewRectangle);
        }

        /// <summary>
        /// Moves/resizes the selected strokes by executing the command. Selects the strokes afterwards.
        /// </summary>
        protected void inkPic_SelectionMovedResized(object sender, EventArgs e)
        {
            CM.ExecuteCommand(c);
            InkChanged(false);
            ((SketchPanelLib.CommandList.MoveResizeCmd)c).selectStoredStrokes();
        }

        protected void inkPic_SelectionChanged(object sender, EventArgs e)
        {
            // Do we need to display any menu?
        }

        #endregion
        
        #region Panel event handling

        /// <summary>
        /// Resizes the InkCanvas whenever this panel is resized.
        /// </summary>
        void SketchPanel_Resize(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (inkCanvas.Width < this.Width)
                inkCanvas.Width = this.Width;

            if (inkCanvas.Height < this.Height)
                inkCanvas.Height = this.Height;
        }
        
        #endregion
        
        #region Standard sketch procedures and transformations (open, save, cut/copy/paste, zoom, and change background)
        
        /// <summary>
        /// Loads a sketch file into the panel.
        /// 
        /// Precondition: given file path is valid and readable.  Does nothing
        /// if file path is null.
        /// </summary>
        /// <param name="filepath">Reads from this file path.</param>
        public void LoadSketch(string filepath)
        {
            if (filepath == null)
                return;

            loadSketch(filepath);

            // See whether the sketch is recognized or not
            recognized = true;
            foreach (Substroke sub in Sketch.Substrokes)
            {
                if (sub.Type == new Domain.ShapeType())
                {
                    recognized = false;
                    break;
                }
            }

            if (SketchFileLoaded != null)
                SketchFileLoaded();
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
            inkSketch.SaveSketch(filepath, this.circuit);
        }

        public void ExportSketch(string filepath)
        {
            ConverterXML.SaveToCirc xmlDocument = new ConverterXML.SaveToCirc(Circuit);
            xmlDocument.WriteToFile(filepath);
        }

        /// <summary>
        /// Magnifies the sketch by a preset amount and notifies listeners.
        /// </summary>
        public void ZoomIn()
        {
            StrokeCollection oldSelection = InkCanvas.GetSelectedStrokes();
            SelectAllStrokes();
            System.Windows.Rect oldBounds = InkCanvas.GetSelectionBounds();
            System.Windows.Rect biggerBounds = new System.Windows.Rect(oldBounds.TopLeft,
                new System.Windows.Size(oldBounds.Width * SketchPanelConstants.ZoomInFactor, oldBounds.Height * SketchPanelConstants.ZoomInFactor));

            CM.ExecuteCommand(new CommandList.MoveResizeCmd(ref inkSketch,
                oldBounds, biggerBounds));

            InkCanvas.Select(oldSelection);
            InkChanged(false);
        }

        /// <summary>
        /// Unmagnifies the sketch by a preset amount and notifies listerners.
        /// </summary> 
        public void ZoomOut(int pixels)
        {
            StrokeCollection oldSelection = InkCanvas.GetSelectedStrokes();
            SelectAllStrokes();
            System.Windows.Rect oldBounds = InkCanvas.GetSelectionBounds();
            System.Windows.Rect smallerBounds = new System.Windows.Rect(oldBounds.TopLeft,
                    new System.Windows.Size(oldBounds.Width * SketchPanelConstants.ZoomOutFactor, oldBounds.Height * SketchPanelConstants.ZoomOutFactor));

            CommandList.MoveResizeCmd resize = new CommandList.MoveResizeCmd(ref inkSketch, oldBounds, smallerBounds);
            CM.ExecuteCommand(resize);

            InkCanvas.Select(oldSelection);
            InkChanged(false);
        }

        /// <summary>
        /// Scales the sketch to fit the current panel dimensions.
        /// </summary>
        public void ZoomToFit()
        {
            /*System.Windows.Rect inkRect = inkPic.Strokes.GetBounds();
            // Scale the sketch to fill the InkImage
            System.Windows.Point rightBottom = new System.Windows.Point(inkRect.Right, inkRect.Bottom);

            rightBottom = inkPic.PointToScreen(rightBottom);

            System.Windows.Point scalePt = new System.Windows.Point(rightBottom.X,
                rightBottom.Y );

            // Scale the rendered strokes by the smallest (x or y) scaling factor
            float xScale = (float)(this.Width - SketchPanelConstants.SketchPaddingScreen) / (float)scalePt.X;
            float yScale = (float)(this.Height - SketchPanelConstants.SketchPaddingScreen) / (float)scalePt.Y;

            float scale = xScale < yScale ? xScale : yScale;*/
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
                ImageBrush brush = new ImageBrush();
                brush.ImageSource =
                new BitmapImage(
                    new Uri(filename, UriKind.Relative)
                );

                this.inkCanvas.Background = brush;
            }
            else
                this.inkCanvas.Background = SketchPanelConstants.DefaultBackColor;

            InvalidateVisual();
        }

        /// <summary>
        /// Copies strokes to the clipboard in InkSerializedFormat.  Copies selected strokes;
        /// if there is no selection, then all Ink is copied.  
        /// </summary>
        public void CopyStrokes()
        {
            CM.ExecuteCommand(new CommandList.CopyCmd(InkSketch));
            // InkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
        }


        /// <summary>
        /// Cuts strokes to the clipboard in InkSerializedFormat.  Cuts selected strokes;
        /// if there is no selection, then all Ink is cut.  
        /// </summary>
        public void CutStrokes()
        {
            CM.ExecuteCommand(new CommandList.CutCmd(InkSketch));
            EnableDrawing();

            InkChanged(false);
        }

        /// <summary>
        /// Undoes the last stroke/action, if possible
        /// Recurses until a valid redo occurs or the redo stack is empty.
        /// </summary>
        public void Undo()
        {
            while (CM.UndoValid)
                if (CM.Undo())
                    break;

            InkChanged(false);
        }

        /// <summary>
        /// Redoes the last stroke/action, if possible
        /// Recurses until a valid redo occurs or the redo stack is empty.
        /// </summary>
        public void Redo()
        {
            while (CM.RedoValid)
                if (CM.Redo())
                    break;

            InkChanged(false);
        }

        /// <summary>
        /// Erases all strokes from the sketch
        /// </summary>
        public void DeleteAllStrokes()
        {
            CM.ExecuteCommand(new CommandList.StrokeRemoveCmd(ref inkSketch, new StrokeCollection(inkSketch.InkCanvas.Strokes)));
            EnableDrawing();

            // When the panel is empty, we cannot say that it has been recognized.
            Recognized = false;

            InkChanged(false);
        }

        /// <summary>
        /// Erases all strokes from the sketch without side effects
        /// </summary>
        public void SimpleDeleteAllStrokes()
        {
            // Don't use the CM to avoid side-effects
            CommandList.StrokeRemoveCmd delete = new CommandList.StrokeRemoveCmd(ref inkSketch, new StrokeCollection(inkSketch.InkCanvas.Strokes));
            delete.Execute();

            // When the panel is empty, we cannot say that it has been recognized.
            Recognized = false;

            InkChanged(false);
        }

        /// <summary>
        /// Selects all strokes in the sketch
        /// </summary>
        public void SelectAllStrokes()
        {
            inkCanvas.Select(inkCanvas.Strokes);
        }

        /// <summary>
        /// Erases all selected strokes from the sketch
        /// </summary>
        public void DeleteStrokes()
        {
            CM.ExecuteCommand(new CommandList.StrokeRemoveCmd(ref inkSketch, inkSketch.InkCanvas.GetSelectedStrokes()));

            // If the panel is now empty, we cannot say that it is recognized
            if (inkCanvas.Strokes.Count == 0)
                Recognized = false;

            InkChanged(false);
        }

        /// <summary>
        /// Pastes strokes from the clipboard
        /// </summary>
        public void PasteStrokes(System.Windows.Point point)
        {
            CM.ExecuteCommand(new CommandList.PasteCmd(InkCanvas, InkSketch, point));
            InkChanged(true);
        }

        /// <summary>
        /// Makes the pen able to draw and erase on the ink canvas
        /// </summary>
        public void EnableDrawing()
        {
            EditingMode = InkCanvasEditingMode.InkAndGesture;
            EditingModeInverted = InkCanvasEditingMode.EraseByStroke;
        }


        /// <summary>
        /// Sets both the editing mode and inverted editing mode to none.
        /// </summary>
        public void DisableDrawing()
        {
            EditingMode = InkCanvasEditingMode.None;
            EditingModeInverted = InkCanvasEditingMode.None;
        }

        #endregion

        #region Recognizer Interface

        /// <summary>
        /// Handles recognition results from a recognizer.  A Recognizer will call
        /// this method whenever a result is ready.
        /// </summary>
        /// <param name="source">The source object (Recogizer) that triggering this event</param>
        /// <param name="result">The recognition result</param>
        public void SetRecognitionResult(object source, RecognitionResult result)
        {
            //if (result.UserTriggered)
            //    this.loadSketch(result.Sketch, false, false);

            if (ResultReceived != null)
                ResultReceived(result);
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
        /// Loads a sketch into this Panel.  
        /// 
        /// Precondition: filepath is not null
        /// </summary>
        /// <param name="newSketch">The Sketch to load</param>
        /// <param name="rescale">True iff the sketch should be resized</param>
        /// <param name="reset">True iff the panel should be reinitialized</param>
        public void loadSketch(string filepath)
        {
            StrokeCollection strokes = new StrokeCollection();
            this.setDefaultInkPicProps();

            // Load the Sketch into the InkPicture

#if DEBUG
            strokes = inkSketch.LoadSketch(filepath);
#else
            try
            {
                strokes = inkSketch.LoadSketch(filepath);
            }
            catch
            {
                // ignore all exceptions!
            }
#endif

            // If the sketch had no strokes, we can stop.
            if (strokes == null || strokes.Count == 0)
            {
                Console.WriteLine("ERR: Given sketch had no strokes.");
                return;
            }
            
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
        public InkToSketchWPF.InkCanvasSketch InkSketch
        {
            get
            {
                return this.inkSketch;
            }
        }

        /// <summary>
        /// Gets this panel's InkCanvas.
        /// </summary>
        public InkCanvas InkCanvas
        {
            get
            {
                return inkSketch.InkCanvas;
            }
        }

        /// <summary>
        /// Gets or sets the recognized status of the panel
        /// </summary>
        public bool Recognized
        {
            get
            {
                return recognized;
            }
            set
            {
                recognized = value;
                if (RecognizeChanged != null)
                    RecognizeChanged(recognized);
            }
        }

        /// <summary>
        /// Gets this panel's InkCircuit.
        /// </summary>
        public CircuitSimLib.Circuit Circuit
        {
            get
            {
                return circuit;
            }
            set
            {
                circuit = value;
            }
        }

        /// <summary>
        /// Gets or sets the editing mode of the InkPicture. 
        /// </summary>
        public InkCanvasEditingMode EditingMode
        {
            get
            {
                return inkCanvas.EditingMode;
            }
            set
            {
                if (inkCanvas.EditingMode.Equals(value))
                    return;

                inkCanvas.EditingMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the inverted editing mode of the InkPicture.
        /// </summary>
        public InkCanvasEditingMode EditingModeInverted
        {
            get
            {
                return inkCanvas.EditingModeInverted;
            }
            set
            {
                if (value == inkCanvas.EditingModeInverted)
                    return;

                inkCanvas.EditingModeInverted = value;

            }
        }


        #endregion
    }

    #region SketchPanel Constants
    /// <summary>
    /// Stores constant parameters for SketchPanels
    /// </summary>
    public class SketchPanelConstants
    {
        /// <summary>
        /// Default background color for sketch panel
        /// </summary>
        public static Brush DefaultBackColor = Brushes.White;

        /// <summary>
        /// Default color for ink strokes
        /// </summary>
        public static Color DefaultInkColor = Colors.Black;

        /// <summary>
        /// Default Zoom-In factor, where 1.0 is nominal.
        /// </summary>
        public const float ZoomInFactor = 1.2F;

        /// <summary>
        /// Default Zoom-Out factor, where 1.0 is nominal.
        /// </summary>
        public const float ZoomOutFactor = 0.8F;
    }
    #endregion
}

