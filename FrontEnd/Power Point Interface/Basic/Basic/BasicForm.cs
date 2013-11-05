using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Ink;
using Microsoft.Office.Core;
using System.Reflection;


namespace Basic
{
    /// <summary>
    /// This form primarily contains the InkOverlay used to get the user's ink strokes.
    /// It also contains much of the code for interpreting the strokes and gestures.
    /// Note that it is divided into multiple files: 
    /// BasicForm.Gestures.cs contains the code for handling each gestures
    /// BasicForm.Helpers.cs contains assorted "helper" functions
    /// </summary>
    public partial class BasicForm : Form
    {

      
        #region shared data members (internal)
        
        internal InkOverlay basicOverlay; // enables gesture and ink capturing
        internal PPTcontrol pptController;  // enables access to useful functions in PPTcontrol
        internal ButtonForm buttonForm; // a form with all the buttons

        #endregion

        #region private data members
        // TODO : many of the paired X and Y data members could be replaced with an fPoint
        // containing both members

        private int firstX; // use for line to remember the x coordinate of the first endpoint
        private int firstY; // use for line to remember the y coordinate of the first endpoint
        private bool secondPoint = false; // used for adding a line to check for a second input

        private int X; // the current x position, set by mouseDown
        private int Y; // the current y position, set by mouseDown
        private int moveX; // the current x position, set by mouseMove
        private int moveY; // the current y position, set by mouseMove
        private PowerPoint.ShapeRange cutPasteShapes; // holds the last collection of cut shape
        private bool mouseMove; // to see if mouseMove event should call move methods
        private bool resizeAllowed; // to see if mouseMove event should call resize methods
        private bool gestureOK; // to see if a gesture should be recognized or not
        private bool locked; // to keep the original config of a group of shapes for move and rotate
        private bool leftRightLocked; // keep the original config of a group of shapes for resize left / right
        private bool topBottomLocked; // keep the original config of a group of shapes for resize top / bottom
        private bool rotateAllowed; // to allow the shape(s) to rotate with mouse move
        private bool rotate; // to see if a rotation has been done
        private int prevY; // keep track of the previous Y position
        private int prevX; // keep tract of the previous X position
        private bool lassoAllowed; // allow the stroke to be recognized as a lasso gesture

        private fPoint? lastSelectedPoint; // the last point which someone tapped at to select. The '?' means it's nullable.
        private List<PowerPoint.Shape> shapesAtCurrentPoint;
        private int layeredShapeCounter; // out of the shapes at the current point, which one are we on?
        private bool rightClickClear;
        internal Stack<Object> undoStack;
        internal Stack<Object> redoStack;
        internal int nameCounter; // for variety in object names
        List<ShapeAttributes> cutAllShapes;
        internal bool scratchoutTriggered;
        private float prevXvalue, prevYvalue; // two global variables that are onlyl used in move
        internal float prevResizeX;
        internal float prevResizeY;
        internal PowerPoint.Shape current = null;
        internal List<ListBox> alternateList;
        private List<Button> buttons;
        internal bool basicActive;
        internal bool pptActive;

        
        enum edgeType { top, bottom, left, right } // for PowerPoint Shapes, the four possible edges
        private edgeType lastResize; // to keep track of the side that the last resize was done on

        // "Mode" enum represents the current state of the program.
        // GestureMode = all gestures on
        // TextMode = most gestures off; only those relevant for text entry remain
        // InkMode = no gestures are available
        enum Mode { GestureMode, TextMode, InkMode }; // choices for pen modes
        private Mode currentMode; // keep track of the mode we are in
        private const int MARGIN = 8;
        private const int SMALLMARGIN = 2; // for smaller tolerances. AIE
        /// <summary>
        /// A "floating point" structure -- a point on a coordinate plane with two coordinates as floats.
        /// </summary>
        internal struct fPoint
        {
            public fPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public fPoint(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
            public override string ToString()
            {
                return (String.Format("({0},{1})",x,y));
            }
            public float x;
            public float y;

/// <summary>
/// Equals method for fPoints.
/// Works with nullable fPoints, or something.
/// Hopefully there is still a general .Equals (obj) method that isn't getting completely hidden, but this one
/// will get used for fPoints.
/// </summary>
/// <param name="q"></param>
/// <returns></returns>
            public bool Equals(fPoint q)
            {
                if (this.Equals(null)|| (q.Equals(null))) { return ((q.Equals(null)) && (this.Equals(null))); }
                return ((x == q.x) && (y == q.y));
            }



            /// <summary>
            /// Returns true if the calling point is within margin of the callee point. Mutually null points are near each other regardless of margin;
            /// a non-null point is never near a null.
            /// </summary>
            /// <param name="other">The point to test for nearness</param>
            /// <param name="margin">The margin, in ppt. coordinates</param>
            /// <returns></returns>
            internal bool isNear(fPoint? other, int margin)
            {
                if (this.Equals(null) || (other.Equals(null))) { return ((other.Equals(null)) && (this.Equals(null))); }
                return (((other.Value.x - margin <= x) && (other.Value.x + margin >= x) && (other.Value.y - margin <= y) && (other.Value.y + margin >= y)));
                

            }
        };

        #endregion



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputController">The PowerPoint Controller connected to this BasicForm</param>
        internal BasicForm(PPTcontrol inputController)
        {
            basicActive = true;
            pptActive = false;
            try
            {
                InitializeComponent();
                pptController = inputController;
                pptController.pptApp.ActiveWindow.View.Zoom = 75;
            }
            catch (Exception e)
            {
                debugExceptionMessage("BasicForm's constructor", e);
                return;
            }


            int width = xFormConvert(pptController.slideWidth(), pptController.getZoom());
            int height = yFormConvert(pptController.slideHeight(), pptController.getZoom());
            this.ClientSize = new System.Drawing.Size(width, height);

            

            // Add the InkOverlay and set it up
            basicOverlay = new InkOverlay(Panel.Handle); // attach the InkOverlay to the Panel

            ClearGestures(basicOverlay);
            // Initially, the Overlay should recognize ALL gestures
            basicOverlay.SetGestureStatus(ApplicationGesture.AllGestures, true);
            currentMode = Mode.GestureMode;

            basicOverlay.CollectionMode = CollectionMode.InkAndGesture;

            // Attach event handlers

            basicOverlay.MouseUp += new InkCollectorMouseUpEventHandler(bo_MouseUp);
            basicOverlay.MouseMove += new InkCollectorMouseMoveEventHandler(bo_MouseMove);
            basicOverlay.Gesture += new InkCollectorGestureEventHandler(bo_Gesture);
            basicOverlay.SystemGesture += new InkCollectorSystemGestureEventHandler(bo_SystemGesture);
            basicOverlay.Stroke += new InkCollectorStrokeEventHandler(bo_Stroke);
            basicOverlay.MouseDown += new InkCollectorMouseDownEventHandler(bo_MouseDown);

            pptController.pptApp.PresentationClose += new PowerPoint.EApplication_PresentationCloseEventHandler(pptApp_PresentationClose);

            //this.Deactivate += new EventHandler(BasicForm_Deactivate);
            //this.Activated += new EventHandler(BasicForm_Activated);

            basicOverlay.AutoRedraw = true; // repaints the window when it is invalidated
            basicOverlay.Enabled = true;

            mouseMove = false; // no move functions at first
            resizeAllowed = false; // no resize functions at first
            gestureOK = true; // automatically recognize gestures until told otherwise
            locked = false; // have not gotten an original configuration yet
            leftRightLocked = false;
            topBottomLocked = false;
            rotate = false;
            prevY = -1;
            prevX = -1;
            lassoAllowed = false;
            rightClickClear = false;
            undoStack = new Stack<Object>();
            redoStack = new Stack<Object>();
            nameCounter = 0;
            lastSelectedPoint = null;
            shapesAtCurrentPoint = new List<PowerPoint.Shape>();
            scratchoutTriggered = false;
            alternateList = new List<ListBox>();
            buttons = new List<Button>();
        }

        
        
        void BasicForm_Activated(object sender, EventArgs e)
        {
            basicActive = true;
        }

        bool first = true;
        void BasicForm_Deactivate(object sender, EventArgs e)
        {
            basicActive = false;
            if (first)
                first = false;
            else
            {
                if (basicActive == false && buttonForm.buttonActive == false && pptController.pptApp.Active == MsoTriState.msoFalse)
                {
                    this.Hide();
                    buttonForm.Hide();
                    basicActive = true;
                    buttonForm.buttonActive = true;
                }
            }
        }
  

        #region Event Handlers

        /// <summary>
        /// when PowerPoint closes, this hides basic form to give PowerPoint focus
        /// </summary>
        /// <param name="Pres"></param>
        void pptApp_PresentationClose(PowerPoint.Presentation Pres)
        {
            this.Hide();
            buttonForm.Hide();
        }

        /// <summary>
        /// bo_Gesture is called when a gesture is recognized. This method gets the current
        /// gesture, obtains its x, y coordinates, and carry out the specified action of
        /// that particular gesture.
        /// TODO : make each case in the switch call a relevant method instead of performing
        /// the work directly in the case
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_Gesture(object sender, InkCollectorGestureEventArgs e)
        {
            PowerPoint.Slide slide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            List<PowerPoint.Shape> totalShapes = pptController.allShapes(); // grabs the objects out of selection
            ApplicationGesture gestureID = e.Gestures[0].Id; // identify the recognized gesture

            // Before trying to recognize the gesture, first check to see if gestureOK is true or not.
            // If it is true, then we are good to go. If it is false, that means the user is moving,
            // rotating, or resizing the shapes, drawing gestures that are actually just paths to
            // whatever the user is doing. Thus, we don't want to recognize that particular gesture
            if (gestureOK == false)
            {
                if (lassoAllowed == true)
                    tryLasso(moveX, moveY);
                gestureOK = true; // the gesture after this one should be recognized
                return;
            }


            // this "shape" is re-used within this method 
            PowerPoint.Shape shape;

            int height = (int)getDimension(1); // 1 represents height
            int width = (int)getDimension(2); // 2 represents width
            
            
            switch (gestureID)
            {
                case ApplicationGesture.ArrowDown: // adds an arrow pointing down
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeDownArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("ArrowDown");
                    break;
                case ApplicationGesture.ArrowLeft: // adds an arrow pointing left
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeLeftArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("ArrowLeft");
                    break;
                case ApplicationGesture.ArrowRight: // adds an arrow pointing right
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeRightArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("ArrowRight");
                    break;
                case ApplicationGesture.ArrowUp:  // adds an arrow pointing up
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeUpArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("ArrowUp");
                    break;
                case ApplicationGesture.Check: // triggers text recognition
                    redoStack.Clear();
                    recognizeText(true);                    
                    break;
                case ApplicationGesture.ChevronDown: // adds a bent arrow pointing down
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCurvedUpArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Bent Arrow Down");
                    break;
                case ApplicationGesture.ChevronLeft: // adds a bent arrow pointing left
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCurvedRightArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Bent Arrow Left");
                    break;
                case ApplicationGesture.ChevronRight: // adds a bent arrow pointing right
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCurvedLeftArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Bent Arrow Right");
                    break;
                case ApplicationGesture.ChevronUp: // adds a bent arrow pointing up
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeCurvedDownArrow, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Bent Arrow Up");
                    break;
                case ApplicationGesture.Circle: // adds a circle at specified location; with sample changed color
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeOval, X, Y, width, height);
                    redoStack.Clear();
                    shape.Fill.ForeColor.RGB = 16777215;
                    stackAddShape(shape);
                    //RGBLong = (B * 65536) + (G * 255) + R
                    //(where R, G and B are Byte values)
                    setFeedback("Circle");
                    break;
                case ApplicationGesture.Curlicue: // delete the last added object
                    redoStack.Clear();
                    deleteLastObject(totalShapes);
                    break;
                case ApplicationGesture.DoubleCircle: // set rotation back to 0
                    // currently it is really easy to accidentally rotate the shape, so instead
                    // of trying to rerotate it, just set it back to original
                    redoStack.Clear();
                    rotateReset();
                    setFeedback("All shapes selected reset to 0 rotation");
                    break;
                case ApplicationGesture.DoubleCurlicue: // delete all objects on the current slide
                    redoStack.Clear();
                    deleteAllObjects(totalShapes);
                    break;
                case ApplicationGesture.DoubleTap:
                    setFeedback("Double Tap");
                    break;
                case ApplicationGesture.Down: // Move to the next slide
                    pptController.nextSlide();
                    setFeedback("Next Slide");
                    break;
                case ApplicationGesture.DownLeft: // Copy all selected objects
                    copySelection();
                    break;
                case ApplicationGesture.DownRight: // Cuts all selected objects
                    redoStack.Clear();
                    cutSelection();
                    break;
                case ApplicationGesture.DownUp:
                    redoStack.Clear();
                    shape = pptController.findShape(X, Y);
                    shape.ZOrder(MsoZOrderCmd.msoSendBackward);
                    stackOrderChange(shape, "down");
                    setFeedback("Shape sent backwards");
                    break;
                case ApplicationGesture.Exclamation:
                    break;
                case ApplicationGesture.Left:
                    break;
                case ApplicationGesture.LeftDown: // delete last stroke added
                    redoStack.Clear();
                    deleteLastStroke(true);
                    break;
                case ApplicationGesture.LeftRight: // paste all cut / copied objects
                    redoStack.Clear();
                    paste();
                    break;
                case ApplicationGesture.LeftUp: // select all
                    setFeedback("All objects selected");
                    slide.Shapes.SelectAll();
                    break;
                case ApplicationGesture.NoGesture:
                    setFeedback("No gesture recognized");
                    break;
                case ApplicationGesture.Right: 
                    // multi-select: add object to selection without clearing current selection
                    multiSelect(totalShapes);
                    break;
                case ApplicationGesture.RightDown: // Add a line by connecting two points
                    redoStack.Clear();
                    addLine();
                    break;
                case ApplicationGesture.RightLeft:
                    break;
                case ApplicationGesture.RightUp:
                    break;
                case ApplicationGesture.Scratchout: // Deletes all ink from the overlay
                    redoStack.Clear();
                    scratchoutTriggered = true;
                    deleteAllStrokes();
                    break;
                case ApplicationGesture.SemiCircleLeft:
                    redoStack.Clear();
                    fPoint p = new fPoint(X, Y);
                    shape = selectNext(p, false);
                    if (shape.Type == MsoShapeType.msoTextBox) // only try shape manipulation if selected is text
                        insertIfTextbox(shape);
                    pptController.pptApp.ActiveWindow.Selection.Unselect();
                    break;
                case ApplicationGesture.SemiCircleRight: // delete the shape the gesture started on
                    redoStack.Clear();
                    shape = pptController.findShape(X, Y);
                    deleteChosenObject(shape);
                    break;
                case ApplicationGesture.Square: // adds a square at the specified location
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeRectangle, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Square");
                    break;
                case ApplicationGesture.Star: // adds a blank slide after the current slide, and go to that slide
                    redoStack.Clear();
                    pptController.newSlide();
                    setFeedback("New slide added");
                    break;
                case ApplicationGesture.Tap: // selects an object
                    shape = selectNext(new fPoint(X,Y),true);
                    break;
                case ApplicationGesture.Triangle: // adds a triangle at the specified location
                    shape = slide.Shapes.AddShape(Microsoft.Office.Core.MsoAutoShapeType.msoShapeIsoscelesTriangle, X, Y, width, height);
                    redoStack.Clear();
                    stackAddShape(shape);
                    setFeedback("Triangle");
                    break;
                case ApplicationGesture.Up: // move to the previous slide
                    pptController.prevSlide();
                    setFeedback("Previous slide");
                    break;
                case ApplicationGesture.UpDown:
                    redoStack.Clear();
                    shape = pptController.findShape(X, Y);
                    stackOrderChange(shape, "up");
                    shape.ZOrder(MsoZOrderCmd.msoBringForward);
                    setFeedback("Shape sent forward");
                    break;
                case ApplicationGesture.UpLeft:
                    redoStack.Clear();
                    shape = selectNext(new fPoint(X,Y),false);
                    if (shape.Type == MsoShapeType.msoTextBox)
                    {
                        deleteCharIfTextbox(shape);
                    }
                    pptController.pptApp.ActiveWindow.Selection.Unselect();
                    break;
                case ApplicationGesture.UpLeftLong:
                    redoStack.Clear();
                    shape = selectNext(new fPoint(X,Y),false);
                    if (shape.Type == MsoShapeType.msoTextBox)
                    {
                        deleteWordIfTextbox(shape);
                    }
                    pptController.pptApp.ActiveWindow.Selection.Unselect();
                    break;
                case ApplicationGesture.UpRight: // sets to textMode - turn most of the gestures off
                    if(currentMode == Mode.GestureMode){ toggleMode(Mode.TextMode);}else{ toggleMode(Mode.GestureMode);}
                    //toggleMode(Mode.TextMode);
                    break;
                case ApplicationGesture.UpRightLong: // turn all gestures back on
                    if (currentMode == Mode.GestureMode) { toggleMode(Mode.TextMode); } else { toggleMode(Mode.GestureMode); }
                    //toggleMode(Mode.GestureMode);
                    break;
                default:
                    break;

            }
            // based on whether there are actions to undo / redo,
            // enable the buttons on ButtonForm for redoing / undoing
            if (redoStack.Count > 0)
                buttonForm.RedoButton.Enabled = true;
            else
                buttonForm.RedoButton.Enabled = false;
            if (undoStack.Count > 0)
                buttonForm.UndoButton.Enabled = true;
            else
                buttonForm.UndoButton.Enabled = false;

        } // end bo_gesture()





        /// <summary>
        /// bo_SystemGesture is called when a system gesture is recognized.
        /// These tend to be "mouse-like" things - drag, right drag, and tap.
        /// It will fire *once* when it first detects the action is ocurring; and does not
        /// clear the ink.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_SystemGesture(object sender, InkCollectorSystemGestureEventArgs e)
        {
            SystemGesture gesture = e.Id;
            switch (gesture)
            {
                    // right drag is currently used for lassooing or moving objects
                    // so no gestures should be recognized
                case SystemGesture.RightDrag: // move object(s) around on the slide
                    gestureOK = false;
                    mouseMove = true;
                    lassoAllowed = true;
                    break;
                case SystemGesture.Drag: // resizes object(s) selected if mousedown near the edge of shape(s)
                    resizeAllowed = true;
                    rotateAllowed = true;
                    break;
                case SystemGesture.RightTap:
                    selectNext(new fPoint(X, Y), false);
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// bo_MouseDown handles the mousedown event. e contains
        /// the location of the mousedown; thus, when a stroke is begun
        /// this fires, grabbing the starting location of the stroke
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_MouseDown(object sender, CancelMouseEventArgs e)
        {
            try
            {
                // I don't know what this does
                clearButtons();
                foreach (ListBox lb in alternateList)
                {
                    lb.Hide();
                }
            }
            catch (Exception lb)
            {
                debugExceptionMessage("disposing alternates", lb);
            }

            X = e.X;
            Y = e.Y;
            X = xDownConvert(X); // need to convert from ink coordinate to PowerPoint coordinate
            Y = yDownConvert(Y); // need to convert from ink coordinate to PowerPoint coordinate

            // The following code only apply to right clicks. A left click does nothing
            if (e.Button.ToString().Equals("2")) // "2" represents a right click
            {
                try
                {
                    PowerPoint.Selection selection;
                    if (fillCurrentSelection(out selection))
                    {
                        PowerPoint.ShapeRange shapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                        rightClick(shapes); // checks what to do in the case of a rightClick when there is a selection
                    }
                    else
                    {
                        PowerPoint.Shape shape = pptController.findShape(X, Y);
                        if (shape != null)
                            shape.Select(Microsoft.Office.Core.MsoTriState.msoCTrue); // select the shape that the cursor is on
                    }

                }
                catch (Exception ex) // Exception occurs when there is NO current selection 
                {
                    debugExceptionMessage("mousedown for exception removal purposes", ex);

                }
            }
        }

       

        /// <summary>
        /// bo_MouseUp is called when the user releases a mouse button. Currently this method clears
        /// information stored in lists during mouseMove events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_MouseUp(object sender, CancelMouseEventArgs e)
        {
            if (mouseMove == true)
            {
                PowerPoint.Selection selection;
                if (fillCurrentSelection(out selection)) // if there is a current selection
                {
                    // save the selected shapes
                    PowerPoint.ShapeRange all = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                    stackMoveEnd(all);
                }
            }
            gestureOK = true;
            mouseMove = false; // no longer call move methods
            resizeAllowed = false; // no longer call resize methods
            rotateAllowed = false;
            rotate = false;
            locked = false; // has no current initial orientation
            leftRightLocked = false;
            topBottomLocked = false;

            if (rightClickClear == true)
            {
                basicOverlay.Ink.DeleteStrokes();
                Panel.Invalidate();
                rightClickClear = false;
            }
        }

        

        /// <summary>
        /// bo_MouseMove is called when the user moves the mouse / cursor. Everytime this happens,
        /// the current x, y coordinates are stored. Also, if certain conditions are met, appropriate
        /// methods are called to manipulate the shapes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_MouseMove(object sender, CancelMouseEventArgs e)
        {
            moveX = e.X;
            moveY = e.Y;
            moveX = xDownConvert(moveX); // convert coordinates from ink to PowerPoint
            moveY = yDownConvert(moveY);

            try
            {
                if ((rotateAllowed || resizeAllowed) && topBottomLocked == false && leftRightLocked == false )
                {
                    bool proceed = false; // a toggle to determine if mouse move should continue to call
                    PowerPoint.Shape shape = pptController.findShape(X, Y);

                    PowerPoint.Selection selection;
                    PowerPoint.ShapeRange all;
                    if (fillCurrentSelection(out selection)) // if there is a current selection
                    {
                        // save the selected shapes
                        all = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                    }
                    else // if there is no current selection, don't rotate or resize anything
                    {
                        rotateAllowed = false;
                        resizeAllowed = false;
                        return; // don't keep going, or will encounter exception
                    }

                    foreach (PowerPoint.Shape s in all)
                    {
                        if (s.Equals(shape)) // if the shape mouse falls on is selected
                        {
                            proceed = true; // then keep going
                            break;
                        }
                    }
                    if (!proceed) // if not keep going, don't rotate or resize
                    {
                        rotateAllowed = false;
                        resizeAllowed = false;
                    }
                }
            }
            catch (Exception blah)
            {
                setFeedback(blah.ToString());
            }

            if (mouseMove) // if a System RightDrag gesture is recognized
            {
                move(moveX, moveY); // allow the shape(s) to move
                rightClickClear = true;
            }
            if (rotateAllowed)
                tryRotate(moveX, moveY); // allow the shape(s) to rotate
            if (resizeAllowed) // if a System Drag gesture is recognized
                tryResize(moveX, moveY); // allow the shape(s) to resize
  
        }



        /// <summary>
        /// THIS METHOD IS CURRENTLY NEVER CALLED
        /// It was a  test to see when to turn off the overlay -- if
        /// an event is attached to this, it will (I believe) turn off the overlay
        /// when the pen leaves the screen? Or possible when right-click happens.
        /// bo_CursorButtonDown fires when the user's pen touches the screen or push the pen button.
        /// However, it only does useful stuff if the button is pushed: it turns the overlay on / off.
        /// The other trigger, pen touching screen, does nothing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_CursorButtonDown(object sender, InkCollectorCursorButtonDownEventArgs e)
        {
            if (e.Button.Name.Equals("Barrel Switch")) // Right click
            {
                if (this.TransparencyKey.Equals(Color.LightCyan)) // Cyan is transparent, so Overlay is currently off
                {
                    this.TransparencyKey = Color.Wheat; // turn overlay on
                    buttonForm.OverlayOn.Text = "Turn Overlay Off";
                    buttonForm.OverlayOn.BackColor = SystemColors.Control;

                }
                else
                {
                    this.TransparencyKey = Color.LightCyan; // turn overlay off
                    buttonForm.OverlayOn.Text = "Turn Overlay On";
                    basicOverlay.Ink.DeleteStrokes();
                    Panel.Invalidate();
                    buttonForm.OverlayOn.BackColor = Color.Red;

                }
            }
        }


        /// <summary>
        /// bo_Stroke is called when a stroke is recognized. To prevent the stroke from being
        /// deleted immediately, this method forces the thread for sleep for 100 ms before
        /// moving on.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void bo_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            System.Threading.Thread.Sleep(100); // keep the stroke for a while for user to see what he has drawn
            if (basicOverlay.GetGestureStatus(ApplicationGesture.ArrowDown) == true) //ie, if gesture mode is on -- 
                                // TODO : replace with gestureMode test
            {
                // if the gesture mode is ON and a stroke is stored, then the intended gesture is not recognized
                //setFeedback("Not a recognized gesture. Please try again");
                basicOverlay.Ink.DeleteStrokes(); // delete the stroke
                Panel.Invalidate(); // and repaint the window so the wrong strokes don't cluster the overlay
            }
            
        }
        /// <summary>
        /// Gets the height or width of the bounding box of the current strokes
        /// on the overlay.
        /// TODO : replace int param with an enum or possibly a boolean. Please.
        /// </summary>
        /// <param name="type">whether to get height or width. 1 = height, 2 = width</param>
        /// <returns>height or width or the bounding box; -1 if incorrect type was passed.</returns>
        internal float getDimension(int type)
        {
            int numStrokes = basicOverlay.Ink.Strokes.Count;
            Rectangle bound = basicOverlay.Ink.Strokes[numStrokes-1].GetBoundingBox();
            if (type == 1)
                return yConvert(bound.Height);
            else if (type == 2)
                return xConvert(bound.Width);
            else
                return -1;
        }

        #endregion


        #region Conversions
        // assorted functions for converting things.

        /// <summary>
        /// makefPoint converts a pair of integers into a floating-point point.
        /// This is not currently used by anything -- deprecated in favor
        /// of fPoint constructor. TODO : carefully remove
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private fPoint makefPoint(int x, int y)
        {
            fPoint temp;
            temp.x = (float)x;
            temp.y = (float)y;
            return temp;
        }
        
      
        // TODO : replace the following functions with functions taking fPoints
        // and converting both x and y simultaneously

        /// <summary>
        /// xConvert onverts the x coordinate of the HotPoints to the x coordinate of ppt. This way, the input
        /// and the output position will match up
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <returns>converted x coordinate</returns>
        internal int xConvert(int x)
        {
            double DEFAULT_Scale = 31.7; // the best number to divide by to convert the hotpoint, 1280*1024 res
            double DEFAULT_Trans = 3; // the best number to translate by to convert the hotpoint, 1280*1024 res
            // double DEFAULT_Zoom = 90;
            return (int)(x / DEFAULT_Scale + DEFAULT_Trans);
        }


        /// <summary>
        /// yConvert converts the y coordinate of the HotPoints to the y coordinate of ppt. This way, the input
        /// and the output position will match up
        /// </summary>
        /// <param name="y">y coordinate</param>
        /// <returns>converted y coordinate</returns>
        internal int yConvert(int y)
        {
            double DEFAULT_Scale = 31.8; // the best number to divide by to convert the hotpoint, 1280*1024 res
            double DEFAULT_Trans = 3; // // the best number to translate by to convert the hotpoint, 1280*1024 res
            //double DEFAULT_Zoom = 90;
            return (int)(y / DEFAULT_Scale + DEFAULT_Trans);
        }


        /// <summary>
        /// xFormConvert converts the x coordinate of the form to the x coordinate of ppt. This way, the
        /// size of the form will match up with the size of the slide.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <returns>converted x coordinate</returns>
        private int xFormConvert(int x, int zoom)
        {
            double DEFAULT_Trans = 185; // the amount to move the form by initially to fit the overlay, 1280*1024 res
            double DEFAULT_Zoom = 90;
            return (int)(((x + DEFAULT_Trans)) * (zoom / DEFAULT_Zoom));
        }

        /// <summary>
        /// yFormConvert converts the y coordinate of the form to the y coordinate of ppt. This way, the
        /// size of the form will match up with the size of the slide.
        /// </summary>
        /// <param name="y">y coordinate</param>
        /// <returns>converted y coordinate</returns>
        private int yFormConvert(int y, int zoom)
        {
            double DEFAULT_Trans = 140; // the amount to move the form by initially to fit the overlay, 1280*1024 res
            double DEFAULT_Zoom = 90;
            return (int)((y + DEFAULT_Trans) * (zoom / DEFAULT_Zoom));
        }
        /// <summary>
        /// xDownConvert converts the x coordinate of a mouseDown / mouseMove event (ink coordinate) to PowerPoint
        /// coordinate
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <returns>converted x coordinate</returns>
        private int xDownConvert(int x)
        {
            // firstX gets a value only if we are in ink mode and this is the first stroke. This stores the
            // location where the first stroke is done so when the text is recognized, we can place the text
            // at the initial position, i.e, where the user started writing
            if ((basicOverlay.GetGestureStatus(ApplicationGesture.ArrowDown) == false) && (basicOverlay.Ink.Strokes.Count == 1))
                firstX = (int)(x * 1);
            return (int)(x * 1);
            //x*0.84 = setting for 90% zoom
            //x*1 = setting for 75% zoom
        }
        /// <summary>
        /// yDownConvert converts the y coordinate of a mouseDown event / mouseMove event (ink coordinate)
        /// to PowerPoint coordinate
        /// </summary>
        /// <param name="y">y coordinate</param>
        /// <returns>converted y coordinate</returns>
        private int yDownConvert(int y)
        {
            // firstY gets a value only if we are in ink mode and this is the first stroke. This stores the
            // location where the first stroke is done so when the text is recognized, we can place the text
            // at the initial position, i.e, where the user started writing
            if ((basicOverlay.GetGestureStatus(ApplicationGesture.ArrowDown) == false) && (basicOverlay.Ink.Strokes.Count == 1))
                firstY = (int)(y * 1);
            return (int)(y * 1);
        }

        #endregion


        #region Recognition

        /// <summary>
        /// strokeRec is called when a text recognition command is triggered by a
        /// gesture. It prints the message currently on the form onto the current
        /// ppt slide at the location of the gesture. The message is modified to
        /// exclude the gesture stroke. Then all the strokes are deleted and screen
        /// invalidated to repaint the form.
        /// </summary>
        private void strokeRec(bool sizeDependent)
        {
            Stroke s1 = null; // stroke under consideration
            Stroke s2 = null; // next stroke
            int x1, y1, x2, y2; // x, y coordinates of the two strokes
            List<int> subMessage = new List<int>(); // list of indexes at break point
            Strokes collection = basicOverlay.Ink.Strokes;
            for (int i = 0; i < collection.Count-2; i++) // don't recognize the check gesture
            {
                
                s1 = basicOverlay.Ink.Strokes[i];
                s2 = basicOverlay.Ink.Strokes[i+1];
                int lastPoint = s1.GetPoints().Length; // need this index to get the last point
                x1 = xConvert(s1.GetPoint(lastPoint-1).X); // compare last point in this stroke
                y1 = yConvert(s1.GetPoint(lastPoint-1).Y);
                x2 = xConvert(s2.GetPoint(0).X); // to the first point in the next stroke
                y2 = yConvert(s2.GetPoint(0).Y);
                
                if (distanceTooGreat(x1, x2, y1, y2)) // compare against a certain distance threshold
                {
                    subMessage.Add(i+1); // if too far apart, store the index for future access
                }
            }
            subMessage.Add(collection.Count - 1); // need to grab the last set of strokes
            string message;
            int startingX, startingY; // where to add the message
            int prevIndex = 0; // end of previous message index
            PowerPoint.Shape textbox;
            Ink ink = new Ink();
            ink = basicOverlay.Ink.Clone(); // get a deep copy of the ink for the undo stack
            Strokes strokesForUndo = ink.Strokes;
            List<ShapeAttributes> textBoxForUndo = new List<ShapeAttributes>();
            foreach (int s in subMessage)
            {
                Strokes sub = basicOverlay.Ink.Strokes;
                sub.Clear();
                for (int i = prevIndex; i<s; i++)
                {
                    sub.Add(collection[i]); // add each stroke in one group to strokes object
                }
                startingX = (int)(1.15 * xConvert(collection[prevIndex].GetPoint(0).X));
                startingY = (int)(1.15 * yConvert(collection[prevIndex].GetPoint(0).Y));
                message = sub.ToString();
                textbox = pptController.addMessage(startingX, startingY, message, sub.GetBoundingBox(), sizeDependent);
                ShapeAttributes textboxClone = new ShapeAttributes(textbox);
                textBoxForUndo.Add(textboxClone);
                prevIndex = s;

                RecognitionAlternates alternates = getAlternative(sub);
                showAlternative(alternates, startingX, startingY, textbox.Name);

            }

            strokesForUndo.RemoveAt(strokesForUndo.Count - 1);
            undoStack.Push(""); // 1st, placeholder
            undoStack.Push(textBoxForUndo); // 2nd, list of textbox created by this
            undoStack.Push(strokesForUndo); // 3rd, stroke for the recognition
            undoStack.Push("TextRec"); // 4th, name of action

            foreach (ListBox lb in alternateList)
            {
                lb.SelectedIndexChanged += new EventHandler(lb_SelectedIndexChanged);
            }

            basicOverlay.Ink.DeleteStrokes();
            Panel.Invalidate();
        }

        

        /// <summary>
        /// Checks to see if the distance between the two sets of input coordinate exceeds the
        /// distance threhold
        /// </summary>
        /// <param name="x1">x coord of first point</param>
        /// <param name="x2">x coord of second point</param>
        /// <param name="y1">y coord of first point</param>
        /// <param name="y2">y coord of second point</param>
        /// <returns>distance between the two sets of input greater or less then threshold</returns>
        private bool distanceTooGreat(int x1, int x2, int y1, int y2)
        {
            float DISTANCE_THRESHOLD = 80; // a random value right now. can be adjusted later
            int xDiffSq = (x1 - x2) * (x1 - x2);
            int yDiffSq = (y1 - y2) * (y1 - y2);
            if (Math.Sqrt(xDiffSq + yDiffSq) > DISTANCE_THRESHOLD)
                return true;
            else
                return false;
        }

        #endregion



        #region CommandBar Access

        /// <summary>
        /// pptFont accesses the PowerPoint built-in font combobox to get the current font setting.
        /// Any text added to the slide through our form will take on that font.
        /// </summary>
        /// <returns>the current font selection</returns>
        internal string pptFont()
        {
            CommandBars aCommandBars;
            CommandBar aStandardBar;
            aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                            null, pptController.pptApp, null);
            aStandardBar = aCommandBars["Formatting"]; // get to the formatting command bar
            CommandBarComboBox font;
            foreach (CommandBarControl c in aStandardBar.Controls)
            {
                if (c.Caption.Equals("&Font:")) // find the font combo box
                {
                    font = (CommandBarComboBox)c; // to gain access to relavent methods, need to cast it first
                    return font.Text;
                }
            }
            return null;
        }


        /// <summary>
        /// pptFontSize accesses the PowerPoint built-in font size combobox to get the current font setting.
        /// Any text added to the slide through our form will take on that font size.
        /// </summary>
        /// <returns>current font size selection</returns>
        internal string pptFontSize()
        {
            string defaultSize = "16";
            CommandBars aCommandBars;
            CommandBar aStandardBar;
            aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                            null, pptController.pptApp, null);
            aStandardBar = aCommandBars["Formatting"];
            CommandBarComboBox fontsize;
            //msocontrolcombobox
            foreach (CommandBarControl c in aStandardBar.Controls)
            {
                if (c.Caption.Equals("&Font Size:"))
                {
                    fontsize = (CommandBarComboBox)c; // to access the methods, need to cast it first
                    return fontsize.Text;
                }
            }
            return defaultSize;
        }

        /// <summary>
        /// isBold accesses the bold control button on command bar. Any text added to the slide
        /// through our form will take on that value.
        /// </summary>
        /// <returns>whether bold is selected</returns>
        internal bool isBold()
        {

            CommandBars aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                            null, pptController.pptApp, null);
            CommandBar aFormattingBar = aCommandBars["Formatting"];
            CommandBarButton bold;
            foreach (CommandBarControl c in aFormattingBar.Controls)
            {
                if (c.Caption.Equals("&Bold")) // msoControlButton
                {
                    bold = (CommandBarButton)c;
                    if (bold.State.ToString().Equals("msoButtonDown"))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// isItalicize accesses the italic control button on command bar. Any text added to the slide
        /// through our form will take on that value
        /// </summary>
        /// <returns>whether italicize is selected</returns>
        internal bool isItalicize()
        {
            CommandBars aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                            null, pptController.pptApp, null);
            CommandBar aFormattingBar = aCommandBars["Formatting"];
            CommandBarButton italicize;
            foreach (CommandBarControl c in aFormattingBar.Controls)
            {
                if (c.Caption.Equals("&Italic"))
                {
                    italicize = (CommandBarButton)c;
                    if (italicize.State.ToString().Equals("msoButtonDown"))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// isUnderline accesses the underline control button on command bar. Any text added to the slide
        /// through our form will take on that value
        /// </summary>
        /// <returns>whether underline is selected</returns>
        internal bool isUnderline()
        {
            CommandBars aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                            null, pptController.pptApp, null);
            CommandBar aFormattingBar = aCommandBars["Formatting"];
            CommandBarButton underline;
            foreach (CommandBarControl c in aFormattingBar.Controls)
            {
                if (c.Caption.Equals("&Underline"))
                {
                    underline = (CommandBarButton)c;
                    if (underline.State.ToString().Equals("msoButtonDown"))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }


        /// <summary>
        /// Invokes PowerPoint's built-in undo button to carry out an undo
        /// Currently never called -- may not work!
        /// </summary>
        internal void undo()
        {
            try
            {
                CommandBars aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                                null, pptController.pptApp, null);
                CommandBar aStandardBar = aCommandBars["Standard"];
                CommandBarControl undoButton = null;
                //msoControlSplitDropdown    &Undo Last
                foreach (CommandBarControl c in aStandardBar.Controls)
                {
                    //System.Windows.Forms.MessageBox.Show(c.Id + "   " + c.Caption);
                    if (c.Id == 128) // 128 is the ID for the undo button
                    {
                        if (c.Enabled)
                        {
                            setFeedback("PowerPoint undo");
                            undoButton = (CommandBarControl)c;
                        }
                        else
                        {
                            setFeedback(c.Caption);
                            return;
                        }
                        break;
                    }
                }
                undoButton.Execute();
            }
            catch (Exception e)
            {
                debugExceptionMessage("undo", e);
            }
        }


        /// <summary>
        /// Invokes PowerPoint's built-in redo button to carry out an redo
        /// currently never called -- may not work!
        /// </summary>
        internal void redo()
        {
            try
            {
                CommandBars aCommandBars = (CommandBars)pptController.pptApp.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty,
                                null, pptController.pptApp, null);
                CommandBar aStandardBar = aCommandBars["Standard"];
                CommandBarControl redoButton = null;
                //msoControlSplitDropdown    &Undo Last
                foreach (CommandBarControl c in aStandardBar.Controls)
                {
                    if (c.Id == 129) // 129 is the ID for the redo button
                    {
                        if (c.Enabled)
                        {
                            setFeedback("PowerPoint redo");
                            redoButton = (CommandBarControl)c;
                        }
                        else
                        {
                            setFeedback(c.Caption);
                            return;
                        }
                        break;
                    }
                }
                redoButton.Execute();
            }
            catch (Exception e)
            {
                debugExceptionMessage("redo", e);
            }
        }



        #endregion


        #region Resize

        /// <summary>
        /// attempts to resize all selected shapes
        /// based on the distance from the pointer to the edge of the "resize target" shape
        /// 
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        void tryResize(int x, int y)
        {
            try
            {
                // This is the point at which the user clicked
                fPoint clickedPoint = new fPoint(x, y);                           
                PowerPoint.Selection selection;
                //if the selection is empty, do nothing
                if ( !fillCurrentSelection(out selection)) { return; }
                //get the selection of shapes
                PowerPoint.ShapeRange shapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                edgeType nearestEdge;
                foreach (PowerPoint.Shape shape in shapes)
                {
                    // once we find a shape that the point is near the edge of, resize everything and jump out of the loop.
                    if (isNearEdge(out nearestEdge, shape, clickedPoint))
                    {
                        resizeShapes(nearestEdge, clickedPoint, shapes);
                        return;
                    }
                   
                }//foreach
            }
            catch (Exception e)
            {
                debugExceptionMessage("tryResize", e);
            }
        }

        /// <summary>
        /// for now, just calls the appropriate resizeEdge method
        /// </summary>
        /// <param name="nearestEdge"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="shapes"></param>
        private void resizeShapes(edgeType nearestEdge, fPoint toPoint, PowerPoint.ShapeRange shapes)
        {
            try
            {
                int x = (int)toPoint.x;
                int y = (int)toPoint.y;
                switch (nearestEdge)
                {
                    case edgeType.top:
                        resizeTop(x, y, shapes);
                        break;
                    case edgeType.left:
                        resizeLeft(x, y, shapes);
                        break;
                    case edgeType.right:
                        resizeRight(x, y, shapes);
                        break;
                    case edgeType.bottom:
                        resizeBottom(x, y, shapes);
                        break;
                }
            }
            catch (Exception resize)
            {
                debugExceptionMessage("resize", resize);
            }
        }
        
        /// <summary>
        /// This method determines whether the passed point is near an edge of the passed shape. If it is, it returns true and sets
        ///  nearestEdge to the nearest edge. Note that it looks at the (possibly rotated) bounding box of the shape as opposed to the shape itself.
        /// </summary>
        /// <param name="nearestEdge">This should be the variable that you wish to have the edge the point is nearest to stored in. If the 
        /// coordinate is not within MARGIN (a constant) of any edge, EdgeType.top is returned.</param>
        /// <param name="inputShape">The shape to be tested for nearness.</param>
        /// <param name="x">The X coordinate of the point.</param>
        /// <param name="y">The Y coordinate of the point.</param>
        /// <returns></returns>
        bool isNearEdge(out edgeType nearestEdge, PowerPoint.Shape inputShape, fPoint testPoint)
            {
            //default-initialize nearestEdge
            nearestEdge = edgeType.top;
            fPoint center = getCenter(inputShape);
            fPoint rotatedPoint = rotatePointWithShape(testPoint, inputShape);

            bool foundEdge = false; // did we actually find an edge it's near?
            List<edgeType> edgeOrder = new List<edgeType>(); // the order to check the edges in
            // based on whichever edge was successfully resized last,
            // check the edges in that order 
            switch(lastResize)
            {
                case edgeType.bottom:
                    edgeOrder.Add(edgeType.bottom);
                    edgeOrder.Add(edgeType.top);
                    edgeOrder.Add(edgeType.left);
                    edgeOrder.Add(edgeType.right);
                    break;
                case edgeType.top:
                    edgeOrder.Add(edgeType.top);
                    edgeOrder.Add(edgeType.bottom);                    
                    edgeOrder.Add(edgeType.left);
                    edgeOrder.Add(edgeType.right);
                    break;
                case edgeType.left:
                    edgeOrder.Add(edgeType.left);
                    edgeOrder.Add(edgeType.right);
                    edgeOrder.Add(edgeType.bottom);
                    edgeOrder.Add(edgeType.top);
                    break;
                case edgeType.right:
                    edgeOrder.Add(edgeType.right);
                    edgeOrder.Add(edgeType.left);
                    edgeOrder.Add(edgeType.bottom);
                    edgeOrder.Add(edgeType.top);  
                    break;
            }//switch(lastResize)
            foundEdge = checkFourEdges(out nearestEdge, edgeOrder, inputShape, rotatedPoint);
            return foundEdge;

            
        }

        /// <summary>
        /// This method actually does the work of checking the edges in the order specified. It returns true if any of them coincide and it sets
        /// nearestEdge to the one which does.
        /// </summary>
        /// <param name="nearestEdge"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <param name="fourth"></param>
        /// <returns></returns>
        private bool checkFourEdges(out edgeType nearestEdge, List<edgeType>checkOrder, PowerPoint.Shape shape, fPoint checkPoint)
        {
            nearestEdge = edgeType.top;
            bool foundMatch = false;

            //check the edges in the order specified by checkOrder
            for (int i = 0; i < 4; ++i)
            {
                // ACTUALLY CHECK THE EDGE HERE and see if it matched
                foundMatch = checkPointNearEdge(checkOrder[i], shape, checkPoint);
                if (foundMatch)
                {
                    nearestEdge = checkOrder[i];
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Actually checks if a point is near an edge. 
        /// </summary>
        /// <param name="edgeType"></param>
        /// <param name="shape"></param>
        /// <param name="checkPoint"></param>
        /// <returns></returns>
        private bool checkPointNearEdge(edgeType edge, PowerPoint.Shape shape, fPoint point)
        {
            int hFlip = shape.HorizontalFlip.Equals(MsoTriState.msoTrue) ? -1 : 1;
            int vFlip = shape.VerticalFlip.Equals(MsoTriState.msoTrue) ? -1 : 1;

            float topY = shape.Top;
            float bottomY = shape.Top + shape.Height * hFlip;
            float leftX = shape.Left;
            float rightX = shape.Left + shape.Width * vFlip;

            switch (edge)
            {    
                case edgeType.top:
                    if ((leftX < point.x) && (rightX > point.x) && ((topY - MARGIN) < point.y) && ((topY + MARGIN) > point.y)) { return true; }
                    break;
                case edgeType.bottom:
                    if ((leftX < point.x) && (rightX > point.x) && ((bottomY - MARGIN) < point.y) && ((bottomY + MARGIN) > point.y)) { return true; }
                    break;
                case edgeType.left:
                    if ((topY < point.y) && (bottomY > point.y) && ((leftX - MARGIN) < point.x) && ((leftX + MARGIN) > point.x)) { return true; }
                    break;
                case edgeType.right:
                    if ((topY < point.y) && (bottomY > point.y) && ((rightX - MARGIN) < point.x) && ((rightX + MARGIN) > point.x)) { return true; }
                    break;
            }
            return false;
        }



        /// <summary>
        /// Given the shape inputShape which has a certain amount r of clockwise of rotation, and a point,
        /// this will return a point which is rotated COUNTERCLOCKWISE by r about the center of the shape.
        /// What this means is that the returned point is in the same location relative to the edges, etc
        /// of the _unrotated_ shape (which doesn't exists except in PowerPoint.Shape 's memory location)
        /// as the original point is to the rotated shape.
        /// 
        /// This is NOT flip-aware yet.
        /// </summary>
        /// <param name="testPoint"></param>
        /// <param name="inputShape"></param>
        /// <returns></returns>
        internal static  fPoint rotatePointWithShape(fPoint testPoint, PowerPoint.Shape inputShape)
        {  
            fPoint center = getCenter(inputShape);
            // set the center as the origin by subtracting it from the point
            
            testPoint.x -= center.x;
            testPoint.y -= center.y;

            // Okay, this is odd.
            // first, the simple part: we have degrees and we need radians.
            // So. InputShape.Rotation is a measure of how many degrees the shape
            // is rotated in a clockwise direction. However, the transform below - as wikipedia says:
            // "(x,y) is rotated counterclockwise by r and we want to know the coordinates (x' y') after
            // the rotation." BUT. Consider. The user sees the shape, which is rotated clockwise by r, and
            // clicks (perhaps) on a (rotated) edge. That click is (x, y). We know where the edges _would be_
            // if the shape were *not* rotated; so we want to know, "if the shape were unrotated and the user
            // clicked on the same place relative to the edge, where would that click have been?"
            // thus, we want to rotate their click COUNTERCLOCKWISE to line up with the imaginary "unrotated"
            // edges; so we can just stick our angle into the transform without making it negative.
            float rot = DegToRad(inputShape.Rotation);
                            
            fPoint rotPoint;
            // apply the transform
            // x' = x cos r - y sin r
            // y' = x sin r + y cos r
            rotPoint.x = (float)(testPoint.x * Math.Cos(rot) - testPoint.y * Math.Sin(rot));
            rotPoint.y = (float)(testPoint.x * Math.Sin(rot) + testPoint.y * Math.Cos(rot));

            //once the point is rotated about the "origin", add the center's displacement back on
            rotPoint.x += center.x;
            rotPoint.y += center.y;

            return rotPoint;
        }

        /// <summary>
        /// Converts the PowerPoint coordinates (clockwise deg) to clockwise radians. Note that this is the opposite
        /// from normal radians / degrees -- the y axis is flipped.
        /// </summary>
        /// <param name="p">The number to convert in degrees</param>
        /// <returns></returns>
        private static float DegToRad(float p)
        {
            // 2 PI / 360 : degrees to radians
            return (float)(p * 2 * Math.PI / 360); 
        }

        /// <summary>
        /// Given a PowerPoint shape, returns the coordinates of the center
        /// </summary>
        /// <param name="inputShape">the shape to find the center of</param>
        /// <returns></returns>
        private static fPoint getCenter(PowerPoint.Shape inputShape)
        {
            int flipY = inputShape.VerticalFlip.Equals(MsoTriState.msoTrue) ? -1 : 1;
            int flipX = inputShape.HorizontalFlip.Equals(MsoTriState.msoTrue) ? -1 : 1;

            fPoint temp;
            temp.y = inputShape.Top + flipY*(inputShape.Height / 2);
            temp.x = inputShape.Left + flipX*(inputShape.Width / 2);

            return temp;
            
        }// end isNearEdge


        /// <summary>
        /// resize the bottom side of all selected shapes
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">selected shapes</param>
        void resizeBottom(int x, int y, PowerPoint.ShapeRange shapes)
        {
            if (Math.Abs(y - prevResizeY) < 4)
                return;
            if (topBottomLocked == false)
            {
                prevResizeY = y;
                topBottomLocked = true;
                gestureOK = false;
                rotateAllowed = false;
                float currentDistance = float.MaxValue;
                foreach (PowerPoint.Shape s in shapes)
                {
                    if (Math.Abs(y - (s.Top + s.Height)) < currentDistance && x > s.Left && x < s.Left + s.Width)
                    {
                        current = s;
                        currentDistance = Math.Abs(y - (s.Top + s.Height));
                    }
                }
            }
            float scale = (float)((y - prevResizeY) / current.Height);
            shapes.ScaleHeight(1 + scale, MsoTriState.msoFalse, MsoScaleFrom.msoScaleFromTopLeft);
            lastResize = edgeType.bottom;    
            prevResizeY = y;
        }


        /// <summary>
        /// resize the top side of the selected shapes
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">selected shapes</param>
        void resizeTop(int x, int y, PowerPoint.ShapeRange shapes)
        {
            if (Math.Abs(y - prevResizeY) < 4)
                return;
            if (topBottomLocked == false)
            {
                prevResizeY = y;
                topBottomLocked = true;
                gestureOK = false;
                rotateAllowed = false;
                float currentDistance = float.MaxValue;
                foreach (PowerPoint.Shape s in shapes)
                {
                    if (Math.Abs(y - s.Top) < currentDistance && x > s.Left && x < s.Left + s.Width)
                    {
                        current = s;
                        currentDistance = Math.Abs(y - s.Top);
                    }
                }
            }
            float scale = (float)((y - prevResizeY) / current.Height);
            shapes.ScaleHeight(1 - scale, MsoTriState.msoFalse, MsoScaleFrom.msoScaleFromBottomRight);
            lastResize = edgeType.top;
            prevResizeY = y;
        }


        /// <summary>
        /// resize the right side of the shapes
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">selected shapes</param>
        void resizeRight(int x, int y, PowerPoint.ShapeRange shapes)
        {
            if (Math.Abs(x - prevResizeX) < 4)
                return;
            if (leftRightLocked == false)
            {
                prevResizeX = x;
                leftRightLocked = true;
                gestureOK = false;
                rotateAllowed = false;
                float currentDistance = float.MaxValue;
                foreach (PowerPoint.Shape s in shapes)
                {
                    if (Math.Abs(x - (s.Left + s.Width)) < currentDistance)
                    {
                        current = s;
                        currentDistance = Math.Abs(x - (s.Left + s.Width));
                    }
                }
            }
            float scale = (float)((x - prevResizeX) / current.Width);
            shapes.ScaleWidth(1 + scale, MsoTriState.msoFalse, MsoScaleFrom.msoScaleFromTopLeft);
            lastResize = edgeType.right;
            prevResizeX = x;
        }


        /// <summary>
        /// resize the left side of the shapes
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">selected shapes</param>
        void resizeLeft(int x, int y, PowerPoint.ShapeRange shapes)
        {
            if (Math.Abs(x - prevResizeX) < 4)
                return;
            if (leftRightLocked == false)
            {
                prevResizeX = x;
                leftRightLocked = true;
                gestureOK = false;
                rotateAllowed = false;
                float currentDistance = float.MaxValue;
                foreach (PowerPoint.Shape s in shapes)
                {
                    if (Math.Abs(x - s.Left) < currentDistance)
                    {
                        current = s;
                        currentDistance = Math.Abs(x - s.Left);
                    }
                }
            }
            float scale = (float)((x - prevResizeX) / current.Width);
            shapes.ScaleWidth(1 - scale, MsoTriState.msoFalse, MsoScaleFrom.msoScaleFromBottomRight);
            lastResize = edgeType.right;
            prevResizeX = x;
        }

        
        /*
         * deprecated versions of the above functions.
         * i believe the difference is that the current versions use a scaleHeight
         * and scaleWidth method built-in , while these use more "homebrewed" resizing.
         * Can probably be safely deleted.
         * 
        /// <summary>
        /// resizes the top side of selected shape(s)
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">collection of shapes currently selected</param>
        void resizeTop(int x, int y, PowerPoint.ShapeRange shapes)
        {
            setFeedback("Top");
            gestureOK = false;
            rotateAllowed = false;
            if (topBottomLocked == false)
            {
                foreach (PowerPoint.Shape shape in shapes)
                {
                    top.Add((int)(y - shape.Top));
                }
                topBottomLocked = true;
            }
            int index = 0;
            foreach (PowerPoint.Shape shape in shapes)
            {
                shape.Height = shape.Height + (shape.Top - y) + top[index];
                if (shape.Height > 2)
                    shape.Top = y - top[index];
                index++;
            }
            lastResize = edgeType.top;
            
        }

        /// <summary>
        /// resize the right side of the shape(s)
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">collection of shapes currently selected</param>
        void resizeRight(int x, int y, PowerPoint.ShapeRange shapes)
        {
            setFeedback("Right");
            gestureOK = false;
            rotateAllowed = false;

            // The first time this method is called within a single drag event,
            // mark each selected shape's ... something... more or less where the left edge is,
            // except plus the first offset. I have changed to just "the left edge" to see if it works better.
            if (leftRightLocked == false)
            {
                foreach (PowerPoint.Shape shape in shapes)
                {
                    right.Add((int)(x - shape.Width));  //(x - shape.Width));
                }
                leftRightLocked = true;
            }
            int index = 0;
            //float tempRotation;
            foreach (PowerPoint.Shape shape in shapes)
            {
                //tempRotation = shape.Rotation;
                //shape.Rotation = 0;
                shape.Width = x - right[index];
                //shape.Rotation = tempRotation;
                index++;
            }
            lastResize = edgeType.right;
            
        }

        /// <summary>
        /// resizes the left side of the shape(s)
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="shapes">collection of shape(s) currently selected</param>
        void resizeLeft(int x, int y, PowerPoint.ShapeRange shapes)
        {
            setFeedback("Left");
            gestureOK = false;
            rotateAllowed = false;
            if (leftRightLocked == false)
            {
                foreach (PowerPoint.Shape shape in shapes)
                {
                    left.Add((int)(x - shape.Left));
                }
                leftRightLocked = true;
            }
            int index = 0;
            foreach (PowerPoint.Shape shape in shapes)
            {
                shape.Width = shape.Width + (shape.Left - x) + left[index];
                if (shape.Width > 2)
                    shape.Left = x - left[index];
                index++;
            }
            lastResize = edgeType.left;
            

        }
         */

        #endregion


    }// public partial class BasicForm : Form
}// namespace Basic
