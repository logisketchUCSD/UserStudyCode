// This is the file that contains the functionality of the "Selection
// Interface," that is, the interface that contains the crossing
// functionality.

// How to use the selection interface as of 12/7/08:
// You can draw circles, triangles, or squares, which are recognized
// as shapes. You can also draw strokes and work with them as if they
// are shapes. Draw a star to recognize text. Hold down the stylus as
// if right-clicking with it to bring up the menu. Hover over the
// canvas briefly to enable selection of whatever is there. Cross over
// the vertical handle of an object to select or deselect it. Hold down
// on a selected shape or stroke to move the selection. Resize or rotate
// with the handles attached to the shapes. Move the stylus off of the
// edge of the canvas while hovering to remove a current selection.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Documents;
using System.Windows.Input;

namespace BasicInterface
{
    public partial class Interface : Form
    {
        #region Global Variables
        UIElement lastStylusOver = new UIElement();                 // Element last hovered over
        List<UIElement> currentSelection = new List<UIElement>();   // selected elements
        List<UIElement> currentHandled = new List<UIElement>();     // all elements corresponding to their handles
        List<UIElement> selectionBoxes = new List<UIElement>();     // boxes marking selection
        List<UIElement> unselectedBoxes = new List<UIElement>();    // boxes marking select mode
        List<UIElement> strokeBoxes = new List<UIElement>();        // boxes marking strokes
        List<Stroke> lockedStrokes = new List<Stroke>();            // strokes that are locked to selection boxes
        List<UIElement> selectHandle = new List<UIElement>();       // hover select handles
        List<UIElement> currentCopyList = new List<UIElement>();    // clipboard for copying 
        List<Stroke> currentStrokeCopyList = new List<Stroke>();    // clipboard for copying strokes
        Timer hoverTime = new Timer();                              // Times when to select
        Timer stylusDownTime = new Timer();                         // Times when to move
        bool shouldSelect;                                          // toggles selection
        bool shouldMove;                                            // toggles move option
        bool allowSelection = true;                                 // toggles selection
        bool singleDown = false;                                    // checks for multiple moves
        bool isMoving = false;                                      // checks to see if things are being moved
        bool firstHover = true;
        bool left_side = false;
        
        // stored data for selection
        Point prevPosition = new Point(-5000, -5000);

        // stored data for rotation
        List<UIElement> rotationCircles = new List<UIElement>();
        List<UIElement> rotationShapes = new List<UIElement>();
        List<UIElement> rotationStrokeCircles = new List<UIElement>();
        List<UIElement> rotationStrokes = new List<UIElement>();
        List<RotateTransform> selectionBoxTransforms = new List<RotateTransform>();
        List<UIElement> selectionBoxTransformShapes = new List<UIElement>();
        bool rotationMode = false;
        bool rotationMode2 = false;
        UIElement leftCurrent = new UIElement();
        Ellipse rotater;
        Ellipse rotater2;
        Point setMiddle = new Point(-5000, -5000);
        Point prev = new Point(-5000, -5000);

        // stored data for resizing
        List<UIElement> resizerRectangles = new List<UIElement>();
        List<UIElement> resizerStrokeRectangles = new List<UIElement>();
        Rectangle resizer;
        Rectangle resizer2;
        bool resizeMode = false;
        bool resizeMode2 = false;

        // data member to hold information that determines whether the selected items should be moved
        double startingX = 0;
        double startingY = 0;
        double moveThreshold = 30;
        double lastMoveX;
        double lastMoveY;
        int moveCount = 0;
        #endregion


        #region Global Constants
        // startup variable data
        int HOVER_TIME_MS = 500;                  // how long to wait before enabling selection
        bool START_TIMER_ON_EMPTY_SPACE = true;   // whether to start the timer when hovering over empty space
        int SELECT_BORDER_THICKNESS = 2;          // border thickness of selection rectangles
        int UNSELECTED_BORDER_THICKNESS = 2;      // border thickness of unselected rectangles during selection
        int SELECT_BORDER_DISTANCE = 3;           // distance from selection rectangles to shapes
        
        int ROTATION_HANDLE_RADIUS = 8;
        SolidColorBrush ROTATION_HANDLE_COLOR = Brushes.LightYellow;
        int ROTATION_HANDLE_BORDER_THICKNESS = 1;
        SolidColorBrush ROTATION_HANDLE_BORDER_COLOR = Brushes.Black;

        int RESIZER_SIZE = 15;
        SolidColorBrush RESIZER_COLOR = Brushes.LightYellow;
        int RESIZER_BORDER_THICKNESS = 1;
        SolidColorBrush RESIZER_BORDER_COLOR = Brushes.Black;

        int HANDLE_BORDER_THICKNESS = 1;
        SolidColorBrush HANDLE_COLOR = Brushes.BurlyWood;
        SolidColorBrush HANDLE_BORDER_COLOR = Brushes.Black;
        int HANDLE_WIDTH = 5;                    // how wide the handle is
        double HANDLE_RELATIVE_HEIGHT = .7;      // how tall handles are relative to their shapes
        int HANDLE_MAX_HEIGHT = 280;             // maximum height of the handle in pixels
        int HANDLE_MIN_HEIGHT = 28;              // minimum height of the handle in pixels

        int DE_HANDLE_BORDER_THICKNESS = 1;
        SolidColorBrush DE_HANDLE_COLOR = Brushes.DarkMagenta;
        SolidColorBrush DE_HANDLE_BORDER_COLOR = Brushes.Black;
        int DE_HANDLE_WIDTH = 5;                 // how wide the deselection handle is
        double DE_HANDLE_RELATIVE_HEIGHT = .3;
        int DE_HANDLE_MAX_HEIGHT = 120;
        int DE_HANDLE_MIN_HEIGHT = 12;

        SolidColorBrush SELECTED_COLOR = Brushes.Black;
        SolidColorBrush UNSELECTED_COLOR = Brushes.LightGray;

        bool USE_CONTEXT_MENU = true;      // whether or not to use the context menu
        bool USE_GESTURE_MENU = true;
        bool USE_STANDARD_MENU = true;      // whether or not to use a standard menu for editing

        bool LINE_SELECTION = true;         // whether or not to have line-crossing-style selection enabled
        #endregion


        #region Event Handlers

        /// <summary>
        /// Initializes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectionButton_Click(object sender, EventArgs e)
        {
            // Initialize the basic components
            basicWindow = new Window();
            basicWindow.Show();
            myBorder = new Border();
            inkCanvas = new InkCanvas();
            inkCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            inkCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            
            inkDA = new DrawingAttributes();

            actionMenuShape = new UIElement();
            copyList = new List<UIElement>();

            basicWindow.Content = myBorder;

            DockPanel thePanel = new DockPanel();
            //StackPanel thePanel = new StackPanel();  // Panel to hold content of the window
            thePanel.VerticalAlignment = VerticalAlignment.Stretch;
            thePanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            # region STANDARD_MENU_SETUP
            // Set up controls for the menu, if desired
  
            if (USE_STANDARD_MENU)
            {
                System.Windows.Controls.ToolBar controlPanel = new System.Windows.Controls.ToolBar();   // A panel to hold the menu
                controlPanel.Background = Brushes.AntiqueWhite;

                System.Windows.Controls.Menu menu = new System.Windows.Controls.Menu();
                System.Windows.Controls.MenuItem edit = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem cut = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem copy = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem paste = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem selectAll = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem deleteAll = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem border = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem color = new System.Windows.Controls.MenuItem();
                System.Windows.Controls.MenuItem delete = new System.Windows.Controls.MenuItem();

                cut.Header = "Cut";
                cut.Name = "Cut";
                copy.Header = "Copy";
                copy.Name = "Copy";
                paste.Header = "Paste";
                selectAll.Header = "Select All";
                deleteAll.Header = "Delete All";
                delete.Header = "Delete";
                color.Header = "Change Color...";
                border.Header = "Change Border...";
                
                // This menu is not fully implemented because only some of the 
                // buttons work... just for the demo.
                delete.Click += new RoutedEventHandler(menu_delete_handler);
                cut.Click += new RoutedEventHandler(menu_cut_handler);
                copy.Click += new RoutedEventHandler(menu_copy_handler);
                selectAll.Click += new RoutedEventHandler(menu_selectAll_handler);
                deleteAll.Click += new RoutedEventHandler(menu_deleteAll_handler);
                
                edit.Header = "Edit";
                edit.Items.Add(delete);
                edit.Items.Add(cut);
                edit.Items.Add(copy);
                edit.Items.Add(paste);
                edit.Items.Add(selectAll);
                edit.Items.Add(deleteAll);
                edit.Items.Add(border);
                edit.Items.Add(color);

                menu.Items.Add(edit);

                controlPanel.Items.Add(menu);
                            
                DockPanel.SetDock(controlPanel, System.Windows.Controls.Dock.Top);

                thePanel.Children.Add(controlPanel);
            }
            #endregion 

            DockPanel.SetDock(inkCanvas, System.Windows.Controls.Dock.Bottom);
            thePanel.Children.Add(inkCanvas);
            
            
            myBorder.Child = thePanel;
            
            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;

            if (USE_GESTURE_MENU)
            {
               inkCanvas.SetEnabledGestures(new ApplicationGesture[] 
                                            { ApplicationGesture.Square,
                                              ApplicationGesture.Circle,
                                              ApplicationGesture.Triangle,
                                              ApplicationGesture.Star,
                                              ApplicationGesture.SemicircleRight});
            }
            else
            {
                inkCanvas.SetEnabledGestures(new ApplicationGesture[] 
                                            { ApplicationGesture.Square,
                                              ApplicationGesture.Circle,
                                              ApplicationGesture.Triangle,
                                              ApplicationGesture.Star});
            }



            inkCanvas.UseCustomCursor = true;

            // Add the event listeners
            basicWindow.StylusLeave += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusLeave2);
         
            inkCanvas.Gesture += new InkCanvasGestureEventHandler(inkCanvas_Gesture2);
            inkCanvas.StylusDown += new System.Windows.Input.StylusDownEventHandler(inkCanvas_StylusDown2);
            inkCanvas.MouseDown += new System.Windows.Input.MouseButtonEventHandler(inkCanvas_MouseDown);
            inkCanvas.StylusMove += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusMove2);
            inkCanvas.StylusUp += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusUp2);
            inkCanvas.StylusInAirMove += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusInAirMove);
            inkCanvas.StylusOutOfRange += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusOutOfRange);
            inkCanvas.KeyDown += new System.Windows.Input.KeyEventHandler(inkCanvas_KeyDown2);
            inkCanvas.KeyUp += new System.Windows.Input.KeyEventHandler(inkCanvas_KeyUp2);
            if (USE_CONTEXT_MENU)
            {
                inkCanvas.MouseRightButtonUp += new MouseButtonEventHandler(inkCanvas_MouseRightButtonUp);
            }

            // initialize properties of the ink canvas
            inkCanvas.Background = Brushes.White;

            // initialize properties of the ink
            inkDA.Color = Colors.Black;
            inkDA.Height = 2;
            inkDA.Width = 1;
            inkDA.FitToCurve = false;
            inkCanvas.DefaultDrawingAttributes = inkDA;

            // When to go into select mode
            shouldSelect = false;
            hoverTime.Interval = HOVER_TIME_MS;
            hoverTime.Tick += new EventHandler(hoverTime_Tick);

            shouldMove = false;
            // 50 is a good time - not noticed by the user by allows move to calibrate
            // some random variables. This is probably not ideal, but this works for now.
            stylusDownTime.Interval = 50;
            stylusDownTime.Tick += new EventHandler(stylusDownTime_Tick);

            // Make sure nothing starts off as selected.
            currentSelection.Clear();
            updateSelection();
        }

        void menu_delete_handler(object sender, RoutedEventArgs args)
        {
            delete2();
        }

        void menu_copy_handler(object sender, RoutedEventArgs args)
        {
            copy2(1);
        }

        void menu_cut_handler(object sender, RoutedEventArgs args)
        {
            cut2();
        }

        void menu_paste_handler(object sender, RoutedEventArgs args)
        {
            //doesn't do anything
        }

        void menu_deleteAll_handler(object sender, RoutedEventArgs args)
        {
            deleteEverything();
        }

        void menu_selectAll_handler(object sender, RoutedEventArgs args)
        {
            selectEverything();
        }



        /// <summary>
        /// Event handler for when the right mouse button is released (or the
        /// stylus is removed from the canvas after being held there).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            displayContextMenu();
        }

        /// <summary>
        /// Event handler for when a gesture is recognized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_Gesture2(object sender, InkCanvasGestureEventArgs e)
        {
            // If the user is moving objects around, then don't try to recognize
            // anything
            if (shouldMove || buttonTouchSelect)
                return;

            // recognize the stroke as some gestures, if any
            ReadOnlyCollection<GestureRecognitionResult> gestureResults =
                e.GetGestureRecognitionResults();
            Rect bounds = e.Strokes.GetBounds();

            // Perform various actions depending on the recognized gesture
            switch (gestureResults[0].ApplicationGesture)
            {
                case ApplicationGesture.Square: // Draw a rectangle on the canvas
                    addRectangle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Circle: // Draw a circle on the canvas
                    addCircle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Triangle:   // Draw a triangle on the canvas
                    addTriangle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Star:    // Recognize the strokes as text
                    addText(X, Y);
                    break;
                case ApplicationGesture.SemicircleRight:
                    if (USE_GESTURE_MENU)
                    {
                        displayContextMenu();
                    }
                    break;
                default:    // Do nothing if no desired gesture is recognized
                    break;
            }
        }

        /// <summary>
        /// Event handler for when the stylus leaves the ink canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusUp2(object sender, System.Windows.Input.StylusEventArgs e)
        {
            inkCanvas.Cursor = System.Windows.Input.Cursors.Pen;

            if (rotationMode)
            {
                rotationMode = false;
                setMiddle = new Point(-5000, -5000);
                UIElement polygon = rotationShapes[rotationCircles.IndexOf(rotater)];
                return;
            }
            if (rotationMode2)
            {
                rotationMode2 = false;
                setMiddle = new Point(-5000, -5000);
                prev = new Point(-5000, -5000);
                UIElement polygon = rotationStrokes[rotationStrokeCircles.IndexOf(rotater2)];
                return;
            }
            if (resizeMode)
            {
                resizeMode = false;
                return;
            }
            if (resizeMode2)
            {
                resizeMode2 = false;
                return;
            }

            // If we are in button selection mode, then call button selection to perform
            // the selection upon stylus up, and don't do any of the normal things.
            if (buttonSelect)
                return;

            // Don't quite remember why this is here, but I suspect is to get rid of the
            // dots that sometimes remain after choosing a button / item on the menu.
            if (singleDown)
            {
                singleDown = false;
                deleteLastStroke();
            }

            // Set isMoving to false, since once the Stylus leaves the screen, it's no
            // longer in "moving mode".
            if (isMoving)
                isMoving = false;

            // If the moving timer is enabled at the time of the stylus up, disable it,
            // since we are not going to move anything anymore.
            if (stylusDownTime.Enabled)
            {
                stylusDownTime.Stop();
            }

            // If move is allowed, then disable it now since we no longer needs it, and
            // remove the stroke that remains from the move
            if (shouldMove)
            {
                shouldMove = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
                deleteLastStroke();
            }

            // If button touch select is on, then delete the last stroke at stylus up
            if (buttonTouchSelect)
            {
                deleteLastStroke();
            }

            // Set the current location of the stylus
            Point pos = e.GetPosition(inkCanvas);
            X = pos.X;
            Y = pos.Y;

            // Find the shape, if any, the stylus is over. Perform various actions according
            // to the result.
            UIElement shape = findShape(X, Y);

            // If the current object is an items control, then invoke different menus depending
            // on what the items control it is.
            if (!(shape == null) && shape.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
            {
                ItemsControl list = (ItemsControl)shape;
                Point listPos = e.GetPosition(shape);
                if (list.Background == Brushes.LightGreen)
                    changeTextColor2(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightSkyBlue)
                    changeTextBackground2(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightCyan)
                    changeFontSize2(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.Cornsilk)
                    changeFont2(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightSlateGray)
                    selectChangeMainColor(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.Turquoise)
                    selectChangeShapeBorder(listPos.X, listPos.Y, list);
            }

            // set move count to 0 upon stylus up, so it doesn't eventually overflow
            if(!(shape == null))
                moveCount = 0;
            addStrokeRectangles();
        }

        /// <summary>
        /// Event handler for when the stylus leaves the edge of the ink canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusLeave2(object sender, System.Windows.Input.StylusEventArgs e)
        {
            inkCanvas.Cursor = System.Windows.Input.Cursors.Pen;

            if (rotationMode)
            {
                inkCanvas_StylusUp2(sender, e);
            }

            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;

            // clear all selection
            currentSelection.Clear();
            updateSelection();
            
            // remove all existing buttons / item controls
            removeButton2();
            deleteItemsControl();
        }

        /// <summary>
        /// Event handler for when the stylus touches the ink canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusDown2(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            prevPosition.X = -5000;
            firstHover = true;
            // Get the position of the stylus
            Point pos = e.GetPosition(inkCanvas);
            X = pos.X;
            Y = pos.Y;

            // If we are in button selection mode, then don't do any of the following things
            if (buttonSelect)
                return;

            UIElement currentShape = findShape(X, Y);

            // Check to see if we should begin to rotate a shape
            if (rotationCircles.Contains(currentShape))
            {
                UIElement rotationShape = rotationShapes[rotationCircles.IndexOf(currentShape)];
                if (!currentSelection.Contains(rotationShape))
                {
                    currentSelection.Add(rotationShape);
                    updateSelection();
                    inkCanvas_StylusDown2(sender, e);
                }
                else
                {
                    rotationMode = true;
                    rotater = (Ellipse)currentShape;
                }
                return;
            }

            if (resizerRectangles.Contains(currentShape))
            {
                UIElement resizeShape = rotationShapes[resizerRectangles.IndexOf(currentShape)];
                if (!currentSelection.Contains(resizeShape))
                {
                    currentSelection.Add(resizeShape);
                    updateSelection();
                    inkCanvas_StylusDown2(sender, e);
                }
                else
                {
                    resizeMode = true;
                    resizer = (Rectangle)currentShape;
                }
                return;
            }

            // Check to see if we should begin to rotate a stroke
            if (rotationStrokeCircles.Contains(currentShape))
            {
                UIElement rotationStroke = rotationStrokes[rotationStrokeCircles.IndexOf(currentShape)];
                if (!currentSelection.Contains(rotationStroke))
                {
                    currentSelection.Add(rotationStroke);
                    updateSelection();
                    inkCanvas_StylusDown2(sender, e);
                }
                else
                {
                    rotationMode2 = true;
                    rotater2 = (Ellipse)currentShape;
                }
                return;
            }

            if (resizerStrokeRectangles.Contains(currentShape))
            {
                UIElement resizeStroke = rotationStrokes[resizerStrokeRectangles.IndexOf(currentShape)];
                if (!currentSelection.Contains(resizeStroke))
                {
                    currentSelection.Add(resizeStroke);
                    updateSelection();
                    inkCanvas_StylusDown2(sender, e);
                }
                else
                {
                    resizeMode2 = true;
                    resizer2 = (Rectangle)currentShape;
                }
                return;
            }

            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
            inkCanvas.Cursor = System.Windows.Input.Cursors.Pen;

            // If we have not started the timer for move it, do so, and keeps track of where
            // the stylus started at the beginning for the timer.
            if (stylusDownTime.Enabled == false)
            {
                stylusDownTime.Start();
                startingX = X;
                startingY = Y;
            }

            // Stop the hover timer, since we will not select anything anymore
            hoverTime.Stop();
            if (shouldSelect)
                removeSelectionHandle();
            shouldSelect = false;

            // Remove all the buttons if the stroke started on empty space
            if (currentShape == null)
            {
                removeButton2();
                return;
            }

            // If it's not a button, then remove the button menus
            bool isButton = currentShape.GetType().ToString().Equals("System.Windows.Controls.Button");
            if (!isButton)
                removeButton2();
        }

        /// <summary>
        /// Event handler for when the stylus moves on the Ink Canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusMove2(object sender, System.Windows.Input.StylusEventArgs e)
        {
            // If button touch selection is turned on, then don't do anything else:
            // Just calls buttonTouchSelection to perform that selection
            if (buttonTouchSelect)
            {
                buttonTouchSelection(e);
                return;
            }

            // Similarly with rotation
            if (rotationMode)
            {
                rotateTo(e);
            }
            if (rotationMode2)
            {
                rotateStrokeTo(e);
            }

            // And resizing
            if (resizeMode)
            {
                resizeTo(e);
            }
            if (resizeMode2)
            {
                resizeStrokeTo(e);
            }

            Point pos = e.GetPosition(inkCanvas);
            moveX = pos.X;
            moveY = pos.Y;
            UIElement shape = findShape(moveX, moveY);

            // If the stroke started on empty space, delete all the items control
            // If, however, isMoving is on, then we still want to move the shapes, and
            // not exit out of the method immediately.
            if (shape == null && !isMoving)
            {
                deleteItemsControl();
                return;
            }

            if (shouldMove)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
            }


            // Only move the object at certain intervals when shouldMove is allowed
            if (shouldMove && (moveCount % 3 ==0))
            {
                isMoving = true;
                // If we allowed selection, remove the handles and stop selection
                if (shouldSelect)
                    removeSelectionHandle();
                shouldSelect = false;

                // For every element in the selection, update its location
                foreach (UIElement obj in currentSelection)
                {
                    InkCanvas.SetLeft(obj, InkCanvas.GetLeft(obj) + (moveX - lastMoveX));
                    InkCanvas.SetRight(obj, InkCanvas.GetLeft(obj) + obj.DesiredSize.Width);
                    InkCanvas.SetTop(obj, InkCanvas.GetTop(obj) + (moveY - lastMoveY));
                    InkCanvas.SetBottom(obj, InkCanvas.GetTop(obj) + obj.DesiredSize.Height);
                    if (strokeBoxes.Contains(obj))
                    {
                        StylusPointCollection newPoints = new StylusPointCollection();
                        for (int i = 0; i < lockedStrokes[strokeBoxes.IndexOf(obj)].StylusPoints.Count; i++)
                        {
                            StylusPoint point = lockedStrokes[strokeBoxes.IndexOf(obj)].StylusPoints[i];
                            point.X += (moveX - lastMoveX);
                            point.Y += (moveY - lastMoveY);
                            newPoints.Add(point);   
                        }
                        lockedStrokes[strokeBoxes.IndexOf(obj)].StylusPoints = newPoints;
                    }
                }
                updateSelection();
            }

            // Only reset the move variables if they are multiples of 3.
            if (moveCount % 3 == 0)
            {
                lastMoveX = moveX;
                lastMoveY = moveY;
            }
            ++moveCount;

            // If we got here because isMoving is set, then we don't want to continue on with
            // the rest of the code. We can just terminate it right here.
            if (shape == null)
            {
                deleteItemsControl();
                return;
            }

            // Special actions for the case when we clicked on a button
            if (shape.GetType().ToString().Equals(("System.Windows.Controls.Button")))
            {
                // If it's the first time the stylus touches the screen, perform some actions.
                // Subsequence move events in the same stylus down should NOT be counted.
                // This prevents multiple actions starting for the same stroke.
                if (!singleDown)
                {
                    System.Windows.Controls.Button b = (System.Windows.Controls.Button)shape;
                    String label = b.Content.ToString();
                    displayButtonResult2(label, InkCanvas.GetLeft(shape), InkCanvas.GetTop(shape));
                    singleDown = true;
                }
            }

        }

        /// <summary>
        /// Event handler for when the stylus is out of range of the hover space.
        /// That is, far from the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusOutOfRange(object sender, System.Windows.Input.StylusEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;

            inkCanvas.Cursor = System.Windows.Input.Cursors.Pen;

            // Stop selection
            hoverTime.Stop();
            if (shouldSelect)
                removeSelectionHandle();
            shouldSelect = false;
            firstHover = true;
            prevPosition.X = -5000;
        }

        /// <summary>
        /// Event handler for when the hover timer is up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void hoverTime_Tick(object sender, EventArgs e)
        {
            // If we are not currently selecting, but selection is not being restricted
            // by some other routine, then show the handle and enter selection mode.
            // Otherwise, just enter selection mode.
            if (!shouldSelect && allowSelection)
                showSelectHandle();
            shouldSelect = true;
        }

        /// <summary>
        /// Event handler for when the move timer is up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void stylusDownTime_Tick(object sender, EventArgs e)
        {
            /* Note: This is first in place when the move timer is set for 1 second,
             * to give the user a clear chance to indicate movement. However, i'm not sure
             * how important this is now to our current interface. Maybe this can be
             * modified to improve the move routine. Something definitely worth looking into.
             */

            // Find the differences in distance between the two move points
            double xDifference = startingX - moveX;
            double yDifference = startingY - moveY;
            if (xDifference < 0)
                xDifference *= -1;
            if (yDifference < 0)
                yDifference *= -1;

            // If the move distance is small, that means the user wants to start moving objects
            if (xDifference < moveThreshold && yDifference < moveThreshold)
            {
                if (currentSelection.Count > 0)
                {
                    UIElement current = findShape(X, Y);
                    if (current != null && currentSelection.Contains(current))
                    {
                        shouldMove = true;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for when the stylus moves in the air
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusInAirMove(object sender, System.Windows.Input.StylusEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.None;

            Point pos = e.GetPosition(inkCanvas);
            moveX = pos.X;
            moveY = pos.Y;

            UIElement currentStylusOver = findShape2(moveX, moveY);

            // Pick the cursor
            if (rotationCircles.Contains(currentStylusOver) ||
                rotationStrokeCircles.Contains(currentStylusOver))
            {
                inkCanvas.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else if (resizerRectangles.Contains(currentStylusOver) ||
                resizerStrokeRectangles.Contains(currentStylusOver))
            {
                inkCanvas.Cursor = System.Windows.Input.Cursors.SizeNWSE;
            }
            else if (shouldSelect)
            {
                inkCanvas.Cursor = System.Windows.Input.Cursors.Cross;
            }
            else
            {
                inkCanvas.Cursor = System.Windows.Input.Cursors.Pen;
            }

            // What to do when we are hovering over empty space
            if (currentStylusOver == null && START_TIMER_ON_EMPTY_SPACE)
            {
                lastStylusOver = null;
                if (hoverTime.Enabled == false)
                {
                    hoverTime.Start();
                }
                return;
            }
            else if (currentStylusOver == null)
            {
                lastStylusOver = null;
                firstHover = true;
                return;
            }


            // If the hover timer is not activated, then start it
            if (hoverTime.Enabled == false)
            {
                hoverTime.Start();
            }

            if (LINE_SELECTION)
            {
                selectAt(e);
                return;
            }
        }

        #endregion


        #region Change Properties

        /// <summary>
        /// Change the color of the border of the selected shapes
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void selectChangeShapeBorder(double x, double y, ItemsControl list)
        {
            // Get the color, then change the border of all selected shapes
            Rectangle rec = getColorBox(x, y, list);
            foreach (UIElement obj in currentSelection)
            {
                if (!strokeBoxes.Contains(obj))
                {
                    Shape shape = (Shape)obj;
                    if (!(rec == null))
                        shape.Stroke = rec.Fill;
                    deleteItemsControl();
                    deleteLastStroke();
                }
            }
        }

        /// <summary>
        /// Change the color of the selected shapes
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void selectChangeMainColor(double x, double y, ItemsControl list)
        {
            // Get the color and change to that color for all selected objects
            Rectangle rec = getColorBox(x, y, list);
            
            foreach (UIElement obj in currentSelection)
            {
                // If it's a shape, then change it's color
                if (obj.GetType().ToString().Contains("System.Windows.Shape")
                    && !strokeBoxes.Contains(obj))
                {
                    Shape shape = (Shape)obj;
                    if (!(rec == null))
                        shape.Fill = rec.Fill;
                    deleteItemsControl();
                    deleteLastStroke();
                }
                // If the object is a text, then change the font color
                else if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock text = (TextBlock)obj;
                    text.Foreground = rec.Fill;
                    deleteItemsControl();
                    deleteLastStroke();
                }
                else if (strokeBoxes.Contains(obj)) 
                {
                    Color newColor = (Color)System.Windows.Media.ColorConverter.
                        ConvertFromString(rec.Fill.ToString());
                    lockedStrokes[strokeBoxes.IndexOf(obj)].DrawingAttributes.
                        Color = newColor;
                    deleteItemsControl();
                    deleteLastStroke();
                }
            }
        }

        /// <summary>
        /// Change the font size of selected textblock
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void changeFontSize2(double x, double y, ItemsControl list)
        {
            // Find the selected font, then change the font of all selected textblock to that
            System.Windows.Controls.Button b = getBox(x, y, list);

            List<UIElement> savedSources = new List<UIElement>();
            foreach (UIElement source in currentSelection)
            {
                ((TextBlock)source).FontSize = (int)(b.Content);
                deleteItemsControl();
                deleteLastStroke();
                source.Measure(new Size(1000, 1000));
                InkCanvas.SetRight(source, InkCanvas.GetLeft(source) + source.DesiredSize.Width);
                InkCanvas.SetBottom(source, InkCanvas.GetTop(source) + source.DesiredSize.Height);
                savedSources.Add(source);
            }
            currentSelection = savedSources;
            updateSelection();
        }

        /// <summary>
        /// Change the color fo the background of the selected textblocks.
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void changeTextBackground2(double x, double y, ItemsControl list)
        {
            // Get the color, and set the background of all selected objects
            Rectangle rec = getColorBox(x, y, list);

            foreach (UIElement source in currentSelection)
            {
                if (!(rec == null))
                    ((TextBlock)source).Background = rec.Fill;
                deleteItemsControl();
                deleteLastStroke();
            }
        }

        /// <summary>
        /// Change the color of the selected text
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void changeTextColor2(double x, double y, ItemsControl list)
        {
            // Get the desired color and change the font color of all selected textblocks
            Rectangle rec = getColorBox(x, y, list);
            foreach (UIElement source in currentSelection)
            {
                if (!(rec == null))
                    ((TextBlock)source).Foreground = rec.Fill;
                deleteItemsControl();
                deleteLastStroke();
            }
        }

        /// <summary>
        /// Change the font of all selected textblocks
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The current item control in focus</param>
        private void changeFont2(double x, double y, ItemsControl list)
        {
            // Get the desired font and change the font of all selected textblocks
            System.Windows.Controls.Button b = getBox(x, y, list);
            String fontName = (String)(b.Content);

            List<UIElement> savedSources = new List<UIElement>();
            foreach (UIElement obj in currentSelection)
            {
                TextBlock text = (TextBlock)obj;
                switch (fontName)
                {
                    case "Ari":
                        text.FontFamily = new FontFamily("Arial");
                        break;
                    case "Com":
                        text.FontFamily = new FontFamily("Comic Sans MS");
                        break;
                    case "Cou":
                        text.FontFamily = new FontFamily("Courier New");
                        break;
                    case "Luc":
                        text.FontFamily = new FontFamily("Lucida Grande");
                        break;
                    case "TNR":
                        text.FontFamily = new FontFamily("Times New Roman");
                        break;
                    case "Ver":
                        text.FontFamily = new FontFamily("Verdana");
                        break;
                    default:
                        break;
                }

                // Clear the item control, and resize the text to fit it's new font
                deleteItemsControl();
                deleteLastStroke();
                text.Measure(new Size(1000, 1000));
                InkCanvas.SetRight(text, InkCanvas.GetLeft(text) + text.DesiredSize.Width);
                InkCanvas.SetBottom(text, InkCanvas.GetTop(text) + text.DesiredSize.Height);
                savedSources.Add(obj);
            }
            currentSelection = savedSources;
            updateSelection();
        }

        #endregion


        #region Crossing

        /// <summary>
        /// Returns true if the line passes through the vertical line segment
        /// defined by topy, bottomy, and x.
        /// </summary>
        /// <param name="cross"></param>
        /// <param name="topy"></param>
        /// <param name="bottomy"></param>
        /// <param name="x"></param>
        bool crosses(Line cross, double topy, double bottomy, double x)
        {
            if ((cross.X1 > x && cross.X2 > x) || (cross.X1 < x && cross.X2 < x))
                return false;
            double m = (cross.Y2 - cross.Y1) / (cross.X2 - cross.X1);
            double b = cross.Y2 - m * cross.X2;
            double y = m * x + b;
            if (y >= topy && y <= bottomy)
                return true;
            return false;
        }

        /// <summary>
        /// Selects everything along the line between the last time it was
        /// called and the position contained in e.
        /// </summary>
        /// <param name="e"></param>
        private void selectAt(StylusEventArgs e)
        {
            Point pos = e.GetPosition(inkCanvas);
            moveX = pos.X;
            moveY = pos.Y;

            if (prevPosition.X == -5000)
            {
                prevPosition = pos;
                return;
            }

            Line currLine = new Line();
            currLine.X1 = prevPosition.X;
            currLine.Y1 = prevPosition.Y;
            currLine.X2 = moveX;
            currLine.Y2 = moveY;

            for (int i = 0; i < selectHandle.Count; i++)
            {
                if (crosses(currLine, InkCanvas.GetTop(selectHandle[i]),
                    InkCanvas.GetBottom(selectHandle[i]),
                    (InkCanvas.GetLeft(selectHandle[i]) +
                    InkCanvas.GetRight(selectHandle[i])) / 2))
                {
                    UIElement toChange = currentHandled[i];
                    if (currentSelection.Contains(toChange))
                    {
                        currentSelection.Remove(toChange);
                    }
                    else
                    {
                        currentSelection.Add(toChange);
                    }

                    updateSelection();
                }
            }
            prevPosition.X = moveX;
            prevPosition.Y = moveY;
        }

        private bool shapeInRange2(UIElement shape, double x, double y)
        {
            if (resizerRectangles.Contains(shape) || resizerStrokeRectangles.Contains(shape))
                return shapeInRange(shape, x, y);
            // This if statement checks to see if a rotation circle is being hit
            if (rotationCircles.Contains(shape))
            {
                Point topleft = new Point(InkCanvas.GetLeft(shape), InkCanvas.GetTop(shape));
                Point bottomright = new Point(InkCanvas.GetRight(shape), InkCanvas.GetBottom(shape));
                RotateTransform rt = new RotateTransform(((RotateTransform)(shape.RenderTransform)).Angle, ((RotateTransform)(shape.RenderTransform)).CenterX, ((RotateTransform)(shape.RenderTransform)).CenterY);
                rt.CenterX = (InkCanvas.GetLeft((Shape)(rotationShapes[rotationCircles.IndexOf(shape)])) + InkCanvas.GetRight((Shape)(rotationShapes[rotationCircles.IndexOf(shape)]))) / 2;
                rt.CenterY = (InkCanvas.GetBottom((Shape)(rotationShapes[rotationCircles.IndexOf(shape)])) + InkCanvas.GetTop((Shape)(rotationShapes[rotationCircles.IndexOf(shape)]))) / 2;
                topleft = rt.Transform(topleft);
                bottomright = rt.Transform(bottomright);

                double diameter = Math.Sqrt(Math.Pow(topleft.X - bottomright.X, 2) + Math.Pow(topleft.Y - bottomright.Y, 2));
                Point center = new Point((bottomright.X + topleft.X) / 2, (bottomright.Y + topleft.Y) / 2);

                return ((center.X - diameter / 2) < x && (center.X + diameter / 2) > x && (center.Y - diameter / 2) < y && (center.Y + diameter / 2) > y);
            }
            if (rotationStrokeCircles.Contains(shape))
            {
                Point topleft = new Point(InkCanvas.GetLeft(shape), InkCanvas.GetTop(shape));
                Point bottomright = new Point(InkCanvas.GetRight(shape), InkCanvas.GetBottom(shape));
                RotateTransform rt = new RotateTransform(((RotateTransform)(shape.RenderTransform)).Angle, ((RotateTransform)(shape.RenderTransform)).CenterX, ((RotateTransform)(shape.RenderTransform)).CenterY);
                rt.CenterX = (InkCanvas.GetLeft((Shape)(rotationStrokes[rotationStrokeCircles.IndexOf(shape)])) + InkCanvas.GetRight((Shape)(rotationStrokes[rotationStrokeCircles.IndexOf(shape)]))) / 2;
                rt.CenterY = (InkCanvas.GetBottom((Shape)(rotationStrokes[rotationStrokeCircles.IndexOf(shape)])) + InkCanvas.GetTop((Shape)(rotationStrokes[rotationStrokeCircles.IndexOf(shape)]))) / 2;
                topleft = rt.Transform(topleft);
                bottomright = rt.Transform(bottomright);

                double diameter = Math.Sqrt(Math.Pow(topleft.X - bottomright.X, 2) + Math.Pow(topleft.Y - bottomright.Y, 2));
                Point center = new Point((bottomright.X + topleft.X) / 2, (bottomright.Y + topleft.Y) / 2);

                return ((center.X - diameter / 2) < x && (center.X + diameter / 2) > x && (center.Y - diameter / 2) < y && (center.Y + diameter / 2) > y);
            }
            else
            {
                return shape.IsStylusOver;
            }
        }

        /// <summary>
        /// Returns the topmost UIElement that the stylus is over.
        /// </summary>
        private UIElement findShape2(double x, double y)
        {
            for (int i = inkCanvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement current = inkCanvas.Children[i];
                if (shapeInRange2(current, x, y))
                {
                    return inkCanvas.Children[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the selection handle that is highest at a given x & y 
        /// point. This is not currently in use but may be useful later.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private UIElement findTopHandle(double x, double y)
        {
            for (int i = inkCanvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement current = inkCanvas.Children[i];
                if (shapeInRange(current, x, y) && selectHandle.Contains(current))
                    return inkCanvas.Children[i];
            }
            return null;
        }

        /// <summary>
        /// Returns true if a UIElement is an actual shape, rather than
        /// something functional like a selection handle or a rotation
        /// handle.
        /// </summary>
        /// <param name="current"></param>
        private bool isActualShape(UIElement current)
        {
            if (selectionBoxes.Contains(current) ||
                    unselectedBoxes.Contains(current) ||
                    rotationCircles.Contains(current) ||
                    resizerRectangles.Contains(current) ||
                    rotationStrokeCircles.Contains(current) ||
                    resizerStrokeRectangles.Contains(current) ||
                    selectHandle.Contains(current))
                return false;
            return true;
        }

        #endregion

        
        #region Display Menu

        /// <summary>
        /// When a button in the context-based menu is clicked, display a menu or perform
        /// some actions based on the button.
        /// </summary>
        /// <param name="label">The label of the button</param>
        /// <param name="left">The left coordinate of the calling button</param>
        /// <param name="top">The top coordinate of the calling button</param>
        private void displayButtonResult2(string label, double left, double top)
        {
            switch (label)
            {
                case "Bg Color":    // change background color
                    if (exists(Brushes.LightSkyBlue))
                        return;
                    createColor(left, top, Brushes.LightSkyBlue);
                    break;
                case "Ft Size":     // change font size
                    if (exists(Brushes.LightCyan))
                        return;
                    createSize(left, top, Brushes.LightCyan);
                    break;
                case "Font":        // change font
                    if (exists(Brushes.Cornsilk))
                        return;
                    createFont(left, top, Brushes.Cornsilk);
                    break;
                case "Color":       // change main color
                    if (exists(Brushes.LightSlateGray))
                        return;
                    createColor(left, top, Brushes.LightSlateGray);
                    break;
                case "Border":      // change border color
                    if (exists(Brushes.Turquoise))
                        return;
                    createColor(left, top, Brushes.Turquoise);
                    break;
                case "Copy":        // copy the current selection
                    copy2(1);
                    break;
                case "Paste":       // paste what's on the clipboard
                    paste2(X, Y);
                    break;
                case "Cut":         // cut the current selection
                    cut2();
                    break;
                case "Delete":      // delete the current selection
                    delete2();
                    break;
                case "DelAll":      // delete everything
                    deleteEverything();
                    break;
                case "SelAll":      // select everything
                    selectEverything();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Display the context based menu, which contains a ring of shapes
        /// </summary>
        private void displayContextMenu()
        {
            bool hasShape = false;  // checks to see whether the selection has shapes
            bool hasStroke = false;
            bool hasText = false;   // checks to see whether the selection has text
            allowSelection = false; // don't allow selection when the context menu is displayed

            // based on the elements in the selection, toggle the hasShape and hasText flags
            foreach (UIElement obj in currentSelection)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle") ||
                    obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon") ||
                    obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    if(strokeBoxes.Contains(obj))
                        hasStroke = true;
                    else
                        hasShape = true;
                }
                else if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                    hasText = true;
            }

            // Display various different menus depending on the items in the selection, as
            // denoted by the two flags

            if (hasShape && hasText)
            {
                displayColorChangeButton(moveX, moveY);
                displayCopyButton(moveX, moveY);
                displayCutButton(moveX, moveY);
                displayPasteButton(moveX, moveY);
                displayDeleteButton(moveX, moveY);
                //displaySelectButton(moveX, moveY);
                //displayDeleteAllButton(moveX, moveY);
            }

            else if (hasShape)
            {
                displayColorChangeButton(moveX, moveY);
                displayBorderColorButton(moveX, moveY);
                displayCopyButton(moveX, moveY);
                displayCutButton(moveX, moveY);
                displayPasteButton(moveX, moveY);
                displayDeleteButton(moveX, moveY);
                displaySelectButton(moveX, moveY);
                displayDeleteAllButton(moveX, moveY);
            }

            else if (hasStroke)
            {
                displayColorChangeButton(moveX, moveY);
                displayCopyButton(moveX, moveY);
                displayCutButton(moveX, moveY);
                displayPasteButton(moveX, moveY);
                displayDeleteButton(moveX, moveY);
                displaySelectButton(moveX, moveY);
                displayDeleteAllButton(moveX, moveY);
            }

            else if (hasText)
            {
                displayColorChangeButton(moveX, moveY);
                displayBackgroundButton(moveX, moveY);
                displayFontButton(moveX, moveY);
                displayFontSizeButton(moveX, moveY);
                displayCopyButton(moveX, moveY);
                displayCutButton(moveX, moveY);
                displayPasteButton(moveX, moveY);
                displayDeleteButton(moveX, moveY);
                displaySelectButton(moveX, moveY);
                displayDeleteAllButton(moveX, moveY);
            }

            // If nothing is selected, then only paste is a reasonable thing to do,
            else
            {
                displayPasteButton(moveX, moveY);
                displaySelectButton(moveX, moveY);
                displayDeleteAllButton(moveX, moveY);
            }
        }

        /// <summary>
        /// Display the various font size button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayFontSizeButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button size = new System.Windows.Controls.Button();
            size = setButtonProperties2(x - HALFR - BUTTON_WIDTH2, x - HALFR,
                y - HALFR + BUTTON_HEIGHT2 / 2, y - HALFR + BUTTON_HEIGHT2 * 1.5);
            size.Content = "Ft Size";
        }

        /// <summary>
        /// Display the font button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayFontButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button font = new System.Windows.Controls.Button();
            font = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                y - HALFR, y - HALFR + BUTTON_HEIGHT2);
            font.Content = "Font";
        }

        /// <summary>
        /// Display the background color button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayBackgroundButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button background = new System.Windows.Controls.Button();
            background = setButtonProperties2(x + HALFR, x + HALFR + BUTTON_WIDTH2,
                y - HALFR + BUTTON_HEIGHT2 / 2, y - HALFR + BUTTON_HEIGHT2 * 1.5);
            background.Content = "Bg Color";
        }

        /// <summary>
        /// Display the border color button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayBorderColorButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button border = new System.Windows.Controls.Button();
            border = setButtonProperties2(x + HALFR, x + HALFR + BUTTON_WIDTH2,
                y - HALFR + BUTTON_HEIGHT2 / 2, y - HALFR + BUTTON_HEIGHT2 * 1.5);
            border.Content = "Border";
        }

        /// <summary>
        /// Display the delete button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayDeleteButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button delete = new System.Windows.Controls.Button();
            delete = setButtonProperties2(x + HALFR, x + HALFR + BUTTON_WIDTH2,
                y + HALFR + BUTTON_HEIGHT2 * 1.5, y + HALFR + BUTTON_HEIGHT2 * 2.5);
            delete.Content = "Delete";
        }

        /// <summary>
        /// Display the paste button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayPasteButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button paste = new System.Windows.Controls.Button();
            paste = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                y + HALFR + BUTTON_HEIGHT2 * 2, y + HALFR + BUTTON_HEIGHT2 * 3);
            paste.Content = "Paste";
        }
        
        /// <summary>
        /// Display the select-all button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displaySelectButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button select = new System.Windows.Controls.Button();
            select = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                y + HALFR + BUTTON_HEIGHT2 * 3, y + HALFR + BUTTON_HEIGHT2 * 4);
            select.Content = "SelAll";
        }

        /// <summary>
        /// Display the delete-all button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayDeleteAllButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button deleteAll = new System.Windows.Controls.Button();
            deleteAll = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                y + HALFR + BUTTON_HEIGHT2 * 4, y + HALFR + BUTTON_HEIGHT2 * 5);
            deleteAll.Content = "DelAll";
        }

        /// <summary>
        /// Display the cut button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayCutButton(double x, double y)
        {
            double HALFR = 8;

            System.Windows.Controls.Button cut = new System.Windows.Controls.Button();
            cut = setButtonProperties2(x - HALFR - BUTTON_WIDTH2, x - HALFR,
                y + HALFR + BUTTON_HEIGHT2 * 1.5, y + HALFR + BUTTON_HEIGHT2 * 2.5);
            cut.Content = "Cut";
        }

        /// <summary>
        /// Display the copy button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayCopyButton(double x, double y)
        {
            double RADIUS = 16;

            System.Windows.Controls.Button copy = new System.Windows.Controls.Button();
            copy = setButtonProperties2(x - RADIUS - BUTTON_WIDTH2, x - RADIUS,
                y + BUTTON_HEIGHT2, y + 2 * BUTTON_HEIGHT2);
            copy.Content = "Copy";
        }

        /// <summary>
        /// Display the color change button
        /// </summary>
        /// <param name="x">the x coordinate at which to show the button</param>
        /// <param name="y">the y coordinate at which to show the button</param>
        private void displayColorChangeButton(double x, double y)
        {
            double RADIUS = 16;

            System.Windows.Controls.Button colorChange = new System.Windows.Controls.Button();
            colorChange = setButtonProperties2(x + RADIUS, x + BUTTON_WIDTH2 + RADIUS,
                y + BUTTON_HEIGHT2, y + 2 * BUTTON_HEIGHT2);
            colorChange.Content = "Color";
        }


        #endregion


        #region Selection Management

        /// <summary>
        /// Update the current selection
        /// </summary>
        private void updateSelection()
        {
            if (selectHandle.Count > 0)
            {
                removeSelectionHandle();
                showSelectHandle();
            }

            // First clear out the boxes for the old selection
            while (selectionBoxes.Count != 0)
            {
                inkCanvas.Children.Remove(selectionBoxes[0]);
                selectionBoxes.RemoveAt(0);
            }

            // Then based on the new selection, update the boxes
            foreach (UIElement obj in currentSelection)
            {
                Rectangle rec = updateSelectionBox(obj);
                selectionBoxes.Add(rec);
                if (rotationStrokes.Contains(obj))
                {
                    UIElement newobj = rotationStrokeCircles[rotationStrokes.IndexOf(obj)];
                    inkCanvas.Children.Insert(inkCanvas.Children.IndexOf(newobj), rec);
                }
                else
                {
                    inkCanvas.Children.Insert(inkCanvas.Children.IndexOf(obj), rec);
                }
            }
        }

        /// <summary>
        /// Returns an updated selection box for a given object.
        /// </summary>
        /// <param name="obj"></param>
        private Rectangle updateSelectionBox(UIElement obj)
        {
            Rectangle rec = new Rectangle();
            rec.Height = obj.DesiredSize.Height + SELECT_BORDER_DISTANCE * 2;
            rec.Width = obj.DesiredSize.Width + SELECT_BORDER_DISTANCE * 2;
            rec.Fill = Brushes.Transparent;
            rec.StrokeThickness = SELECT_BORDER_THICKNESS;
            InkCanvas.SetLeft(rec, InkCanvas.GetLeft(obj) - SELECT_BORDER_DISTANCE);
            InkCanvas.SetTop(rec, InkCanvas.GetTop(obj) - SELECT_BORDER_DISTANCE);
            List<double> borderDouble = new List<double>();
            borderDouble.Add(2.0);
            DoubleCollection border = new DoubleCollection(borderDouble);
            rec.StrokeDashArray = border;
            rec.Stroke = SELECTED_COLOR;
            if (selectionBoxTransformShapes.Contains(obj))
            {
                rec.RenderTransform = selectionBoxTransforms[selectionBoxTransformShapes.IndexOf(obj)];
            }
            return rec;
        }

        /// <summary>
        /// Show the hover select handles for all the objects on the application
        /// </summary>
        private void showSelectHandle()
        {
            // If we are in button selection mode, then don't display the selection handles
            if (buttonSelect)
                return;

            List<UIElement> rotationCollection = new List<UIElement>();
            List<UIElement> resizeCollection = new List<UIElement>();

            // For every object, display a small vertical handle
            foreach (UIElement obj in inkCanvas.Children)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Controls.Button"))
                    continue;
                if (selectionBoxes.Contains(obj))
                    continue;

                // first draw deselection style
                if (currentSelection.Contains(obj))
                {
                    UIElement handle = makeDeselectionHandle(obj);
                    selectHandle.Add(handle);
                }
                else
                {
                    UIElement handle = makeSelectionHandle(obj);
                    selectHandle.Add(handle);
                }
                

                Rectangle rec = new Rectangle();
                rec.Height = obj.DesiredSize.Height + SELECT_BORDER_DISTANCE * 2;
                rec.Width = obj.DesiredSize.Width + SELECT_BORDER_DISTANCE * 2;
                rec.Fill = Brushes.Transparent;
                rec.StrokeThickness = UNSELECTED_BORDER_THICKNESS;
                InkCanvas.SetLeft(rec, InkCanvas.GetLeft(obj) - SELECT_BORDER_DISTANCE);
                InkCanvas.SetTop(rec, InkCanvas.GetTop(obj) - SELECT_BORDER_DISTANCE);
                List<double> borderDouble = new List<double>();
                borderDouble.Add(2.0);
                DoubleCollection border = new DoubleCollection(borderDouble);
                rec.StrokeDashArray = border;
                rec.Stroke = UNSELECTED_COLOR;
                if (selectionBoxTransformShapes.Contains(obj))
                {
                    rec.RenderTransform = selectionBoxTransforms[selectionBoxTransformShapes.IndexOf(obj)];
                }
                unselectedBoxes.Add(rec);

                currentHandled.Add(obj);

                if (!strokeBoxes.Contains(obj) && !(obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock")))
                {
                    UIElement rotation = makeRotationHandle(obj);
                    UIElement resize = makeResizer(obj);
                    if (selectionBoxTransformShapes.Contains(obj))
                    {
                        resize = rotateResizer(resize, (Shape)obj);
                    }
                    rotationShapes.Add(obj);
                    rotationCollection.Add(rotation);
                    resizeCollection.Add(resize);
                }
                else if (!(obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock")))
                {
                    UIElement rotation = makeStrokeRotationHandle(new Point(InkCanvas.GetRight(obj), InkCanvas.GetBottom(obj) - ((Shape)(obj)).ActualHeight / 2), obj);
                    UIElement resize = makeStrokeResizer(obj);
                    if (selectionBoxTransformShapes.Contains(obj))
                    {
                        resize = rotateResizer(resize, (Shape)obj);
                    }
                    rotationStrokes.Add(obj);
                    rotationCollection.Add(rotation);
                    resizeCollection.Add(resize);
                }
            }
            int count = 0;
            // We want the rotation handle to be directly above the object it refers to
            foreach (UIElement rotation in rotationCollection)
            {
                while (!isActualShape(inkCanvas.Children[count]))
                {
                    count += 1;
                }
                if (strokeBoxes.Contains(inkCanvas.Children[count]))
                {
                    inkCanvas.Children.Add(rotation);
                    count += 1;
                }
                else
                {
                    count += 1;
                    inkCanvas.Children.Insert(count, rotation);
                }
            }

            count = 0;
            foreach (UIElement rectangle in resizeCollection)
            {
                while (!isActualShape(inkCanvas.Children[count]))
                {
                    count += 1;
                }
                if (strokeBoxes.Contains(inkCanvas.Children[count]))
                {
                    inkCanvas.Children.Add(rectangle);
                    count += 1;
                }
                else
                {
                    count += 1;
                    inkCanvas.Children.Insert(count, rectangle);
                }
            }

            count = 0;
            // We want the handle to be directly above the object it refers to
            foreach (UIElement handle in selectHandle)
            {
                while (!isActualShape(inkCanvas.Children[count]))
                {
                    count += 1;
                }
                if (strokeBoxes.Contains(inkCanvas.Children[count]))
                {
                    inkCanvas.Children.Add(handle);
                    count += 1;
                }
                else
                {
                    count += 1;
                    inkCanvas.Children.Insert(count, handle);
                }
            }

            foreach (UIElement obj in unselectedBoxes)
            {
                inkCanvas.Children.Insert(0, obj);
            }
        }

        /// <summary>
        /// Adds the stroke rectangles, that is, the invisible rectangles that
        /// are put behind strokes so that they can operate like an actual
        /// shape.
        /// </summary>
        private void addStrokeRectangles()
        {
            for (int i = 0; i < strokeBoxes.Count; i++)
            {
                if (currentSelection.Contains(strokeBoxes[i]) == false)
                {
                    inkCanvas.Children.Remove(strokeBoxes[i]);
                    lockedStrokes.RemoveAt(i);
                    strokeBoxes.RemoveAt(i);
                }
            }

            foreach (Stroke stroke in inkCanvas.Strokes)
            {
                if (lockedStrokes.Contains(stroke) == false)
                {
                    Rectangle rec = new Rectangle();
                    rec.Height = stroke.GetBounds().Height;
                    rec.Width = stroke.GetBounds().Width;
                    rec.Fill = Brushes.Transparent;
                    InkCanvas.SetLeft(rec, stroke.GetBounds().Left);
                    InkCanvas.SetTop(rec, stroke.GetBounds().Top);
                    InkCanvas.SetBottom(rec, stroke.GetBounds().Bottom);
                    InkCanvas.SetRight(rec, stroke.GetBounds().Right);

                    strokeBoxes.Add(rec);
                    lockedStrokes.Add(stroke);
                    inkCanvas.Children.Add(rec);
                }
            }
        }

        /// <summary>
        /// Remove the selection handle from the objects
        /// </summary>
        private void removeSelectionHandle()
        {
            // Go through the list and clear out every handle seen
            while (selectHandle.Count != 0)
            {
                inkCanvas.Children.Remove(selectHandle[0]);
                selectHandle.RemoveAt(0);
            }
            while (unselectedBoxes.Count != 0)
            {
                inkCanvas.Children.Remove(unselectedBoxes[0]);
                unselectedBoxes.RemoveAt(0);
                currentHandled.RemoveAt(0);
            }
            while (rotationCircles.Count != 0)
            {
                inkCanvas.Children.Remove(rotationCircles[0]);
                rotationCircles.RemoveAt(0);
                rotationShapes.RemoveAt(0);
            }
            while (resizerRectangles.Count != 0)
            {
                inkCanvas.Children.Remove(resizerRectangles[0]);
                resizerRectangles.RemoveAt(0);
            }
            while (rotationStrokeCircles.Count != 0)
            {
                inkCanvas.Children.Remove(rotationStrokeCircles[0]);
                rotationStrokeCircles.RemoveAt(0);
                rotationStrokes.RemoveAt(0);
            }
            while (resizerStrokeRectangles.Count != 0)
            {
                inkCanvas.Children.Remove(resizerStrokeRectangles[0]);
                resizerStrokeRectangles.RemoveAt(0);
            }
        }

        /// <summary>
        /// Select everything
        /// </summary>
        private void selectEverything()
        {
            removeButton();
            currentSelection.Clear();
            updateSelection();
            foreach (UIElement element in inkCanvas.Children)
            {
                if (!(element.GetType().ToString().Equals(("System.Windows.Controls.Button"))))
                    currentSelection.Add(element);
            }
            updateSelection();
        }

        #endregion


        #region Selection Graphics

        /// <summary>
        /// Returns a formatted selection handle given the object it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeSelectionHandle(UIElement obj)
        {
            double rotationAngle = 0;
            if (selectionBoxTransformShapes.Contains(obj))
            {
                RotateTransform rt = selectionBoxTransforms[selectionBoxTransformShapes.IndexOf(obj)];
                rotationAngle = rt.Angle * (2 * Math.PI) / 360;
            }
            double breakAngle = Math.Atan((InkCanvas.GetRight(obj) - InkCanvas.GetLeft(obj)) / (InkCanvas.GetBottom(obj) - InkCanvas.GetTop(obj)));
            if (rotationAngle < -breakAngle)
            {
                rotationAngle += Math.PI;
            }
            else if (rotationAngle < Math.PI && rotationAngle > (-breakAngle + Math.PI))
            {
                rotationAngle -= Math.PI;
            }
            double horizontalMid = (InkCanvas.GetLeft(obj) + InkCanvas.GetRight(obj)) / 2;
            double verticalMid = (InkCanvas.GetBottom(obj) + InkCanvas.GetTop(obj)) / 2;
            Rectangle handle = new Rectangle();
            handle.Width = HANDLE_WIDTH;
            if (rotationAngle < breakAngle)
                handle.Height = (InkCanvas.GetBottom(obj) - InkCanvas.GetTop(obj)) * HANDLE_RELATIVE_HEIGHT;
            else
            {
                handle.Height = (InkCanvas.GetRight(obj) - InkCanvas.GetLeft(obj)) * HANDLE_RELATIVE_HEIGHT;
                rotationAngle -= Math.PI / 2;
            }
            handle.Height = Math.Abs(handle.Height / Math.Cos(rotationAngle)); if (handle.Height > HANDLE_MAX_HEIGHT)
            {
                handle.Height = HANDLE_MAX_HEIGHT;
            }
            else if (handle.Height < HANDLE_MIN_HEIGHT)
            {
                handle.Height = HANDLE_MIN_HEIGHT;
            }
            handle.Fill = HANDLE_COLOR;
            handle.Measure(new Size(1000, 1000));
            InkCanvas.SetBottom(handle, verticalMid + (handle.Height * .5 / HANDLE_RELATIVE_HEIGHT));
            InkCanvas.SetTop(handle, InkCanvas.GetBottom(handle) - handle.Height);
            InkCanvas.SetLeft(handle, horizontalMid - handle.Width / 2);
            InkCanvas.SetRight(handle, horizontalMid + handle.Width / 2);
            handle.StrokeThickness = HANDLE_BORDER_THICKNESS;
            handle.Stroke = HANDLE_BORDER_COLOR;
            return handle;
        }

        /// <summary>
        /// Returns a formatted deselection handle given the object it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeDeselectionHandle(UIElement obj)
        {
            double rotationAngle = 0;
            if (selectionBoxTransformShapes.Contains(obj))
            {
                RotateTransform rt = selectionBoxTransforms[selectionBoxTransformShapes.IndexOf(obj)];
                rotationAngle = rt.Angle * (2 * Math.PI) / 360;
            }
            double breakAngle = Math.Atan((InkCanvas.GetRight(obj) - InkCanvas.GetLeft(obj)) / (InkCanvas.GetBottom(obj) - InkCanvas.GetTop(obj)));
            if (rotationAngle < -breakAngle)
            {
                rotationAngle += Math.PI;
            }
            else if (rotationAngle < Math.PI && rotationAngle > (-breakAngle + Math.PI))
            {
                rotationAngle -= Math.PI;
            }
            double horizontalMid = (InkCanvas.GetLeft(obj) + InkCanvas.GetRight(obj)) / 2;
            double verticalMid = (InkCanvas.GetBottom(obj) + InkCanvas.GetTop(obj)) / 2;
            Rectangle handle = new Rectangle();
            handle.Width = DE_HANDLE_WIDTH;

            if (rotationAngle < breakAngle)
                handle.Height = (InkCanvas.GetBottom(obj) - InkCanvas.GetTop(obj)) * DE_HANDLE_RELATIVE_HEIGHT;
            else
            {
                handle.Height = (InkCanvas.GetRight(obj) - InkCanvas.GetLeft(obj)) * DE_HANDLE_RELATIVE_HEIGHT;
                rotationAngle -= Math.PI / 2;
            }
            handle.Height = Math.Abs(handle.Height / Math.Cos(rotationAngle));
            if (handle.Height > DE_HANDLE_MAX_HEIGHT)
            {
                handle.Height = DE_HANDLE_MAX_HEIGHT;
            }
            else if (handle.Height < DE_HANDLE_MIN_HEIGHT)
            {
                handle.Height = DE_HANDLE_MIN_HEIGHT;
            }
            handle.Fill = DE_HANDLE_COLOR;
            handle.Measure(new Size(1000, 1000));
            InkCanvas.SetTop(handle, verticalMid - (handle.Height * .5 / DE_HANDLE_RELATIVE_HEIGHT));
            InkCanvas.SetBottom(handle, InkCanvas.GetTop(handle) + handle.Height);
            InkCanvas.SetLeft(handle, horizontalMid - handle.Width / 2);
            InkCanvas.SetRight(handle, horizontalMid + handle.Width / 2);
            handle.StrokeThickness = DE_HANDLE_BORDER_THICKNESS;
            handle.Stroke = DE_HANDLE_BORDER_COLOR;
            return handle;
        }

        /// <summary>
        /// Returns a formatted resizing handle given the object it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeResizer(UIElement obj)
        {
            Rectangle resize = new Rectangle();
            resize.Height = RESIZER_SIZE;
            resize.Width = RESIZER_SIZE;
            resize.Fill = RESIZER_COLOR;
            resize.Stroke = RESIZER_BORDER_COLOR;
            resize.StrokeThickness = RESIZER_BORDER_THICKNESS;
            InkCanvas.SetTop(resize, InkCanvas.GetBottom(obj) - RESIZER_SIZE / 2);
            InkCanvas.SetLeft(resize, InkCanvas.GetRight(obj) - RESIZER_SIZE / 2);
            InkCanvas.SetBottom(resize, InkCanvas.GetBottom(obj) + RESIZER_SIZE / 2);
            InkCanvas.SetRight(resize, InkCanvas.GetRight(obj) + RESIZER_SIZE / 2);
            resizerRectangles.Add(resize);
            return resize;
        }

        /// <summary>
        /// Returns a formatted resizing handle given the stroke rectangle it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeStrokeResizer(UIElement obj)
        {
            Rectangle resize = new Rectangle();
            resize.Height = RESIZER_SIZE;
            resize.Width = RESIZER_SIZE;
            resize.Fill = RESIZER_COLOR;
            resize.Stroke = RESIZER_BORDER_COLOR;
            resize.StrokeThickness = RESIZER_BORDER_THICKNESS;
            InkCanvas.SetTop(resize, InkCanvas.GetBottom(obj) - RESIZER_SIZE / 2);
            InkCanvas.SetLeft(resize, InkCanvas.GetRight(obj) - RESIZER_SIZE / 2);
            InkCanvas.SetBottom(resize, InkCanvas.GetBottom(obj) + RESIZER_SIZE / 2);
            InkCanvas.SetRight(resize, InkCanvas.GetRight(obj) + RESIZER_SIZE / 2);
            resizerStrokeRectangles.Add(resize);
            return resize;
        }

        /// <summary>
        /// Returns a formatted rotation handle given the object it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeRotationHandle(UIElement obj)
        {
            Ellipse circle = new Ellipse();
            circle.Height = ROTATION_HANDLE_RADIUS * 2;
            circle.Width = ROTATION_HANDLE_RADIUS * 2;
            circle.Fill = ROTATION_HANDLE_COLOR;
            circle.Stroke = ROTATION_HANDLE_BORDER_COLOR;
            circle.StrokeThickness = ROTATION_HANDLE_BORDER_THICKNESS;
            InkCanvas.SetTop(circle, InkCanvas.GetBottom(obj) - ((Shape)(obj)).ActualHeight / 2 - ROTATION_HANDLE_RADIUS);
            InkCanvas.SetLeft(circle, InkCanvas.GetRight(obj) - ROTATION_HANDLE_RADIUS + SELECT_BORDER_DISTANCE);
            InkCanvas.SetBottom(circle, InkCanvas.GetBottom(obj) - ((Shape)(obj)).ActualHeight / 2 + ROTATION_HANDLE_RADIUS);
            InkCanvas.SetRight(circle, InkCanvas.GetRight(obj) + ROTATION_HANDLE_RADIUS);
            if (selectionBoxTransformShapes.Contains(obj))
            {
                RotateTransform rt = ((RotateTransform)(obj.RenderTransform));
                circle.RenderTransform = new RotateTransform(rt.Angle, rt.CenterX - ((Shape)obj).ActualWidth + ROTATION_HANDLE_RADIUS - SELECT_BORDER_DISTANCE, rt.CenterY - ((Shape)obj).ActualHeight / 2 + ROTATION_HANDLE_RADIUS);
            }
            else
            {
                circle.RenderTransform = new RotateTransform(0, 0, 0);
            }

            rotationCircles.Add(circle);
            return circle;
        }

        /// <summary>
        /// Returns a formatted rotation handle given the stroke rectangle it goes to.
        /// </summary>
        /// <param name="obj"></param>
        private UIElement makeStrokeRotationHandle(Point location, UIElement obj)
        {
            Ellipse circle = new Ellipse();
            circle.Height = ROTATION_HANDLE_RADIUS * 2;
            circle.Width = ROTATION_HANDLE_RADIUS * 2;
            circle.Fill = ROTATION_HANDLE_COLOR;
            circle.Stroke = ROTATION_HANDLE_BORDER_COLOR;
            circle.StrokeThickness = ROTATION_HANDLE_BORDER_THICKNESS;
            InkCanvas.SetTop(circle, location.Y - ROTATION_HANDLE_RADIUS);
            InkCanvas.SetLeft(circle, location.X - ROTATION_HANDLE_RADIUS);
            InkCanvas.SetBottom(circle, location.Y + ROTATION_HANDLE_RADIUS);
            InkCanvas.SetRight(circle, location.X + ROTATION_HANDLE_RADIUS);
            rotationStrokeCircles.Add(circle);
            if (selectionBoxTransformShapes.Contains(obj))
            {
                RotateTransform rt = ((RotateTransform)(obj.RenderTransform));
                circle.RenderTransform = new RotateTransform(rt.Angle, rt.CenterX - ((Shape)obj).ActualWidth + ROTATION_HANDLE_RADIUS - SELECT_BORDER_DISTANCE, rt.CenterY - ((Shape)obj).ActualHeight / 2 + ROTATION_HANDLE_RADIUS);
            }
            else
            {
                circle.RenderTransform = new RotateTransform(0, 0, 0);
            }
            return circle;
        }

        #endregion
        

        #region Transformations

        /// <summary>
        /// Resizes the current object to the point specified by e.
        /// </summary>
        /// <param name="e"></param>
        void resizeTo(System.Windows.Input.StylusEventArgs e)
        {
            Shape shape = (Shape)(rotationShapes[resizerRectangles.IndexOf(resizer)]);
            UIElement rotater2 = rotationCircles[resizerRectangles.IndexOf(resizer)];
            double oldright = InkCanvas.GetRight(shape);
            double oldbottom = InkCanvas.GetBottom(shape);
            double oldtop = InkCanvas.GetTop(shape);
            double oldleft = InkCanvas.GetLeft(shape);
            double y = e.GetPosition(inkCanvas).Y;
            double x = e.GetPosition(inkCanvas).X;
            if (selectionBoxTransformShapes.Contains(shape))
            {
                ((RotateTransform)shape.RenderTransform).Angle = 0;
                ((RotateTransform)selectionBoxes[currentSelection.IndexOf(shape)].RenderTransform).Angle = 0;
                ((RotateTransform)rotater2.RenderTransform).Angle = 0;
            }
            double height = y - oldtop;
            double width = x - oldleft;
            bool horizflop = false;
            bool vertflop = false;
            if (height < 10)
            {
                vertflop = true;
                height = 10;
            }
            if (width < 10)
            {
                horizflop = true;
                width = 10;
            }
            if (height > 9 && width > 9)
            {
                if (shape.GetType().ToString().Equals("System.Windows.Shapes.Rectangle")
                    || shape.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    ((Shape)shape).Height = height;
                    ((Shape)shape).Width = width;
                }
                else if (shape.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    PointCollection vertex = new PointCollection();
                    vertex.Add(new Point(width / 2, 0));
                    vertex.Add(new Point(0, y - oldtop));
                    vertex.Add(new Point(x - oldleft, y - oldtop));
                    ((Polygon)shape).Points = vertex;
                }

                if (vertflop)
                {
                    InkCanvas.SetTop(shape, y - 10);
                    InkCanvas.SetBottom(shape, y);
                }
                else
                {
                    InkCanvas.SetTop(shape, oldtop);
                    InkCanvas.SetBottom(shape, y);
                }
                if (horizflop)
                {
                    InkCanvas.SetLeft(shape, x - 10);
                    InkCanvas.SetRight(shape, x);
                }
                else
                {
                    InkCanvas.SetLeft(shape, oldleft);
                    InkCanvas.SetRight(shape, x);
                }
                InkCanvas.SetTop(resizer, y - RESIZER_SIZE / 2);
                InkCanvas.SetBottom(resizer, y + RESIZER_SIZE / 2);
                InkCanvas.SetLeft(resizer, x - RESIZER_SIZE / 2);
                InkCanvas.SetRight(resizer, x + RESIZER_SIZE / 2);
                InkCanvas.SetTop(rotater2, (y - height / 2) - ROTATION_HANDLE_RADIUS);
                InkCanvas.SetBottom(rotater2, (y - height / 2) + ROTATION_HANDLE_RADIUS);
                InkCanvas.SetLeft(rotater2, x - ROTATION_HANDLE_RADIUS);
                InkCanvas.SetRight(rotater2, x + ROTATION_HANDLE_RADIUS);
                if (currentSelection.Contains(shape))
                {
                    Shape box = (Shape)(selectionBoxes[currentSelection.IndexOf(shape)]);
                    box.Height = height + SELECT_BORDER_DISTANCE * 2;
                    box.Width = width + SELECT_BORDER_DISTANCE * 2;
                    InkCanvas.SetTop(box, InkCanvas.GetTop(shape) - SELECT_BORDER_DISTANCE);
                    InkCanvas.SetBottom(box, InkCanvas.GetBottom(shape) + SELECT_BORDER_DISTANCE);
                    InkCanvas.SetLeft(box, InkCanvas.GetLeft(shape) - SELECT_BORDER_DISTANCE);
                    InkCanvas.SetRight(box, InkCanvas.GetRight(shape) + SELECT_BORDER_DISTANCE);
                }
                Shape box2 = (Shape)(unselectedBoxes[currentHandled.IndexOf(shape)]);
                box2.Height = height + SELECT_BORDER_DISTANCE * 2;
                box2.Width = width + SELECT_BORDER_DISTANCE * 2;
                InkCanvas.SetTop(box2, InkCanvas.GetTop(shape) - SELECT_BORDER_DISTANCE);
                InkCanvas.SetBottom(box2, InkCanvas.GetBottom(shape) + SELECT_BORDER_DISTANCE);
                InkCanvas.SetLeft(box2, InkCanvas.GetLeft(shape) - SELECT_BORDER_DISTANCE);
                InkCanvas.SetRight(box2, InkCanvas.GetRight(shape) + SELECT_BORDER_DISTANCE);


                if (currentSelection.Contains(shape))
                {
                    int rememberdex = inkCanvas.Children.IndexOf(selectHandle[currentHandled.IndexOf(shape)]);
                    inkCanvas.Children.Remove(selectHandle[currentHandled.IndexOf(shape)]);
                    selectHandle[currentHandled.IndexOf(shape)] = makeDeselectionHandle(shape);
                    inkCanvas.Children.Insert(rememberdex, selectHandle[currentHandled.IndexOf(shape)]);
                }
                else
                {
                    selectHandle[currentHandled.IndexOf(shape)] = makeSelectionHandle(shape);
                }
            }
        }

        /// <summary>
        /// Rotates the current object to the point specified by e.
        /// </summary>
        /// <param name="e"></param>
        void rotateTo(System.Windows.Input.StylusEventArgs e)
        {
            Shape shape = (Shape)(rotationShapes[rotationCircles.IndexOf(rotater)]);

            if (setMiddle.X == -5000)
                setMiddle = new Point(shape.ActualWidth / 2.0, shape.ActualHeight / 2.0);

            Point current = e.GetPosition(inkCanvas);
            current.X -= InkCanvas.GetLeft(shape);
            current.Y -= InkCanvas.GetTop(shape);

            double newTheta = Math.Atan2(current.Y - setMiddle.Y, current.X - setMiddle.X) * 57.2957795;

            RotateTransform rotateTransform1 = new RotateTransform(newTheta, setMiddle.X, setMiddle.Y);
            shape.RenderTransform = rotateTransform1;
            RotateTransform rotateTransform2 = new RotateTransform(rotateTransform1.Angle, rotateTransform1.CenterX + SELECT_BORDER_DISTANCE, rotateTransform1.CenterY + SELECT_BORDER_DISTANCE);
            if (currentSelection.Contains(shape))
            {
                Shape box = (Shape)(selectionBoxes[currentSelection.IndexOf(shape)]);
                box.RenderTransform = rotateTransform2;
            }
            Shape box2 = (Shape)(unselectedBoxes[currentHandled.IndexOf(shape)]);
            box2.RenderTransform = rotateTransform2;
            if (selectionBoxTransformShapes.Contains(shape))
            {
                selectionBoxTransforms.RemoveAt(selectionBoxTransformShapes.IndexOf(shape));
                selectionBoxTransformShapes.Remove(shape);
            }
            selectionBoxTransformShapes.Add(shape);
            selectionBoxTransforms.Add(rotateTransform2);
            RotateTransform rotateTransform3 = new RotateTransform(rotateTransform1.Angle, setMiddle.X - shape.ActualWidth + ROTATION_HANDLE_RADIUS - SELECT_BORDER_DISTANCE, setMiddle.Y - shape.ActualHeight / 2 + ROTATION_HANDLE_RADIUS);

            rotater.RenderTransform = rotateTransform3;

            UIElement resizer2 = resizerRectangles[rotationCircles.IndexOf(rotater)];
            resizer2 = rotateResizer(resizer2, shape);

            if (currentSelection.Contains(shape))
            {
                int rememberdex = inkCanvas.Children.IndexOf(selectHandle[currentHandled.IndexOf(shape)]);
                inkCanvas.Children.Remove(selectHandle[currentHandled.IndexOf(shape)]);
                selectHandle[currentHandled.IndexOf(shape)] = makeDeselectionHandle(shape);
                inkCanvas.Children.Insert(rememberdex, selectHandle[currentHandled.IndexOf(shape)]);
            }
            else
            {
                selectHandle[currentHandled.IndexOf(shape)] = makeSelectionHandle(shape);
            }
        }

        /// <summary>
        /// Rotates the current resizer to the point specified by e.
        /// </summary>
        /// <param name="e"></param>
        private UIElement rotateResizer(UIElement resizer2, Shape shape)
        {
            if (selectionBoxTransformShapes.Contains(shape))
            {
                double TORAD = 0.017453292519943295769236907684886;
                double right = InkCanvas.GetRight(shape);
                double bottom = InkCanvas.GetBottom(shape);
                double top = InkCanvas.GetTop(shape);
                double left = InkCanvas.GetLeft(shape);
                Point middle = new Point((right - left) / 2, (bottom - top) / 2);
                double r = Math.Sqrt(Math.Pow(middle.X, 2) + Math.Pow(middle.Y, 2));
                double theta = ((RotateTransform)(shape.RenderTransform)).Angle - 180 + Math.Atan2(middle.Y, middle.X) / TORAD;
                InkCanvas.SetLeft(resizer2, right - Math.Cos(theta * TORAD) * r - RESIZER_SIZE / 2 - middle.X);
                InkCanvas.SetRight(resizer2, right - Math.Cos(theta * TORAD) * r + RESIZER_SIZE / 2 - middle.X);
                InkCanvas.SetTop(resizer2, bottom - Math.Sin(theta * TORAD) * r - RESIZER_SIZE / 2 - middle.Y);
                InkCanvas.SetBottom(resizer2, bottom - Math.Sin(theta * TORAD) * r + RESIZER_SIZE / 2 - middle.Y);
            }
            return resizer2;
        }

        /// <summary>
        /// Rotates the current stroke to the point specified by e.
        /// </summary>
        /// <param name="e"></param>
        void rotateStrokeTo(System.Windows.Input.StylusEventArgs e)
        {
            Shape shape = (Shape)(rotationStrokes[rotationStrokeCircles.IndexOf(rotater2)]);

            if (setMiddle.X == -5000)
                setMiddle = new Point(shape.ActualWidth / 2.0, shape.ActualHeight / 2.0);

            Point current = e.GetPosition(inkCanvas);
            current.X -= InkCanvas.GetLeft(shape);
            current.Y -= InkCanvas.GetTop(shape);

            if (prev.X == -5000)
            {
                prev.X = current.X;
                prev.Y = current.Y;
            }

            double newTheta = Math.Atan2(current.Y - setMiddle.Y, current.X - setMiddle.X) * 57.2957795;
            double currTheta = Math.Atan2(prev.Y - setMiddle.Y, prev.X - setMiddle.X) * 57.2957795;
            prev.X = current.X;
            prev.Y = current.Y;

            Matrix rotatingMatrix = new Matrix();
            rotatingMatrix.RotateAt(newTheta - currTheta, setMiddle.X + InkCanvas.GetLeft(shape), setMiddle.Y + InkCanvas.GetTop(shape));
            lockedStrokes[strokeBoxes.IndexOf(shape)].Transform(rotatingMatrix, true);

            RotateTransform rotateTransform1 = new RotateTransform(newTheta, setMiddle.X, setMiddle.Y);
            shape.RenderTransform = rotateTransform1;
            RotateTransform rotateTransform2 = new RotateTransform(rotateTransform1.Angle, rotateTransform1.CenterX + SELECT_BORDER_DISTANCE, rotateTransform1.CenterY + SELECT_BORDER_DISTANCE);
            if (currentSelection.Contains(shape))
            {
                Shape box = (Shape)(selectionBoxes[currentSelection.IndexOf(shape)]);
                box.RenderTransform = rotateTransform2;
            }
            Shape box2 = (Shape)(unselectedBoxes[currentHandled.IndexOf(shape)]);
            box2.RenderTransform = rotateTransform2;
            if (selectionBoxTransformShapes.Contains(shape))
            {
                selectionBoxTransforms.RemoveAt(selectionBoxTransformShapes.IndexOf(shape));
                selectionBoxTransformShapes.Remove(shape);
            }
            selectionBoxTransformShapes.Add(shape);
            selectionBoxTransforms.Add(rotateTransform2);
            RotateTransform rotateTransform3 = new RotateTransform(rotateTransform1.Angle, setMiddle.X - shape.ActualWidth + ROTATION_HANDLE_RADIUS - SELECT_BORDER_DISTANCE, setMiddle.Y - shape.ActualHeight / 2 + ROTATION_HANDLE_RADIUS);

            rotater2.RenderTransform = rotateTransform3;

            UIElement resizer2 = resizerStrokeRectangles[rotationStrokeCircles.IndexOf(rotater2)];
            resizer2 = rotateResizer(resizer2, shape);

            if (currentSelection.Contains(shape))
            {
                int rememberdex = inkCanvas.Children.IndexOf(selectHandle[currentHandled.IndexOf(shape)]);
                inkCanvas.Children.Remove(selectHandle[currentHandled.IndexOf(shape)]);
                selectHandle[currentHandled.IndexOf(shape)] = makeDeselectionHandle(shape);
                inkCanvas.Children.Insert(rememberdex, selectHandle[currentHandled.IndexOf(shape)]);
            }
            else
            {
                selectHandle[currentHandled.IndexOf(shape)] = makeSelectionHandle(shape);
            }
        }

        /// <summary>
        /// Resizes the current stroke to the point specified by e.
        /// </summary>
        /// <param name="e"></param>
        void resizeStrokeTo(System.Windows.Input.StylusEventArgs e)
        {
            Shape shape = (Shape)(rotationStrokes[resizerStrokeRectangles.IndexOf(resizer2)]);
            UIElement rotater2 = rotationStrokeCircles[resizerStrokeRectangles.IndexOf(resizer2)];
            double oldright = InkCanvas.GetRight(shape);
            double oldbottom = InkCanvas.GetBottom(shape);
            double oldtop = InkCanvas.GetTop(shape);
            double oldleft = InkCanvas.GetLeft(shape);
            double oldangle = 0;
            double y = e.GetPosition(inkCanvas).Y;
            double x = e.GetPosition(inkCanvas).X;
            if (selectionBoxTransformShapes.Contains(shape))
            {
                oldangle = ((RotateTransform)shape.RenderTransform).Angle;
                ((RotateTransform)shape.RenderTransform).Angle = 0;
                ((RotateTransform)selectionBoxes[currentSelection.IndexOf(shape)].RenderTransform).Angle = 0;
                ((RotateTransform)rotater2.RenderTransform).Angle = 0;
            }
            double height = y - oldtop;
            double width = x - oldleft;
            bool horizflop = false;
            bool vertflop = false;
            if (height < 10)
            {
                vertflop = true;
                height = 10;
            }
            if (width < 10)
            {
                horizflop = true;
                width = 10;
            }
            if (height > 9 && width > 9)
            {
                if (shape.GetType().ToString().Equals("System.Windows.Shapes.Rectangle")
                    || shape.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    ((Shape)shape).Height = height;
                    ((Shape)shape).Width = width;
                }
                else if (shape.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    PointCollection vertex = new PointCollection();
                    vertex.Add(new Point(width / 2, 0));
                    vertex.Add(new Point(0, y - oldtop));
                    vertex.Add(new Point(x - oldleft, y - oldtop));
                    ((Polygon)shape).Points = vertex;
                }

                if (vertflop)
                {
                    InkCanvas.SetTop(shape, y - 10);
                    InkCanvas.SetBottom(shape, y);
                }
                else
                {
                    InkCanvas.SetTop(shape, oldtop);
                    InkCanvas.SetBottom(shape, y);
                }
                if (horizflop)
                {
                    InkCanvas.SetLeft(shape, x - 10);
                    InkCanvas.SetRight(shape, x);
                }
                else
                {
                    InkCanvas.SetLeft(shape, oldleft);
                    InkCanvas.SetRight(shape, x);
                }
                InkCanvas.SetTop(resizer2, y - RESIZER_SIZE / 2);
                InkCanvas.SetBottom(resizer2, y + RESIZER_SIZE / 2);
                InkCanvas.SetLeft(resizer2, x - RESIZER_SIZE / 2);
                InkCanvas.SetRight(resizer2, x + RESIZER_SIZE / 2);
                InkCanvas.SetTop(rotater2, (y - height / 2) - ROTATION_HANDLE_RADIUS);
                InkCanvas.SetBottom(rotater2, (y - height / 2) + ROTATION_HANDLE_RADIUS);
                InkCanvas.SetLeft(rotater2, x - ROTATION_HANDLE_RADIUS);
                InkCanvas.SetRight(rotater2, x + ROTATION_HANDLE_RADIUS);
                if (currentSelection.Contains(shape))
                {
                    Shape box = (Shape)(selectionBoxes[currentSelection.IndexOf(shape)]);
                    box.Height = height + SELECT_BORDER_DISTANCE * 2;
                    box.Width = width + SELECT_BORDER_DISTANCE * 2;
                    InkCanvas.SetTop(box, InkCanvas.GetTop(shape) - SELECT_BORDER_DISTANCE);
                    InkCanvas.SetBottom(box, InkCanvas.GetBottom(shape) + SELECT_BORDER_DISTANCE);
                    InkCanvas.SetLeft(box, InkCanvas.GetLeft(shape) - SELECT_BORDER_DISTANCE);
                    InkCanvas.SetRight(box, InkCanvas.GetRight(shape) + SELECT_BORDER_DISTANCE);
                }
                Shape box2 = (Shape)(unselectedBoxes[currentHandled.IndexOf(shape)]);
                box2.Height = height + SELECT_BORDER_DISTANCE * 2;
                box2.Width = width + SELECT_BORDER_DISTANCE * 2;
                InkCanvas.SetTop(box2, InkCanvas.GetTop(shape) - SELECT_BORDER_DISTANCE);
                InkCanvas.SetBottom(box2, InkCanvas.GetBottom(shape) + SELECT_BORDER_DISTANCE);
                InkCanvas.SetLeft(box2, InkCanvas.GetLeft(shape) - SELECT_BORDER_DISTANCE);
                InkCanvas.SetRight(box2, InkCanvas.GetRight(shape) + SELECT_BORDER_DISTANCE);


                if (currentSelection.Contains(shape))
                {
                    int rememberdex = inkCanvas.Children.IndexOf(selectHandle[currentHandled.IndexOf(shape)]);
                    inkCanvas.Children.Remove(selectHandle[currentHandled.IndexOf(shape)]);
                    selectHandle[currentHandled.IndexOf(shape)] = makeDeselectionHandle(shape);
                    inkCanvas.Children.Insert(rememberdex, selectHandle[currentHandled.IndexOf(shape)]);
                }
                else
                {
                    selectHandle[currentHandled.IndexOf(shape)] = makeSelectionHandle(shape);
                }

                Stroke theStroke = lockedStrokes[strokeBoxes.IndexOf(shape)];
                Matrix rotatingMatrix = new Matrix();
                rotatingMatrix.RotateAt(-oldangle, (oldright - oldleft) / 2 + oldleft, (oldbottom - oldtop) / 2 + oldtop);
                theStroke.Transform(rotatingMatrix, true);
                StylusPointCollection points = theStroke.StylusPoints;
                StylusPointCollection newpoints = new StylusPointCollection();
                for (int i = 0; i < points.Count; i++)
                {
                    StylusPoint toAdd = new StylusPoint();
                    toAdd.X = (points[i].X - oldleft) * (InkCanvas.GetRight(shape) - InkCanvas.GetLeft(shape)) / (oldright - oldleft) + InkCanvas.GetLeft(shape);
                    toAdd.Y = (points[i].Y - oldtop) * (InkCanvas.GetBottom(shape) - InkCanvas.GetTop(shape)) / (oldbottom - oldtop) + InkCanvas.GetTop(shape);
                    toAdd.PressureFactor = points[i].PressureFactor;
                    newpoints.Add(toAdd);
                }
                theStroke.StylusPoints = newpoints;

            }
        }

        #endregion


        #region Actions

        private void delete(double x, double y, Rect deleteBoundary)
        {
            Rect bound = new Rect();

            // Delete all shapes that fall within the given boundary rectangle
            UIElementCollection shapes = inkCanvas.Children;
            for (int i = 0; i < shapes.Count; ++i)
            {
                bound.Location = new Point(InkCanvas.GetLeft(shapes[i]), InkCanvas.GetTop(shapes[i]));
                bound.Height = InkCanvas.GetBottom(shapes[i]) - InkCanvas.GetTop(shapes[i]);
                bound.Width = InkCanvas.GetRight(shapes[i]) - InkCanvas.GetLeft(shapes[i]);
                
                if (bound.IntersectsWith(deleteBoundary))
                {
                    if (currentSelection.Contains(shapes[i]))
                    {
                        currentSelection.Remove(shapes[i]);
                        updateSelection();
                    }
                    inkCanvas.Children.Remove(shapes[i]);
                    --i;
                }
            }

            // Also delete all strokes that fall within the given boundary box
            StrokeCollection strokes = inkCanvas.Strokes;
            for (int i = 0; i < strokes.Count; ++i)
            {
                bound = strokes[i].GetBounds();
                if (bound.IntersectsWith(deleteBoundary))
                {
                    inkCanvas.Strokes.Remove(strokes[i]);
                    --i;
                }
            }
        }

        /// <summary>
        /// Remove all the buttons from the interface
        /// </summary>
        private void removeButton2()
        {
            UIElementCollection children = inkCanvas.Children;
            UIElement child;

            // Go through all the elements of ink canvas and delete all buttons
            for (int i = 0; i < children.Count; i++)
            {
                child = children[i];
                if (child.GetType().ToString().Equals("System.Windows.Controls.Button"))
                {
                    inkCanvas.Children.Remove(child);
                    i--; // current one removed and the next one moved up. Check again.
                }
            }

            // Once the buttons are not there anymore, we can allow the users to perform
            // selection again
            allowSelection = true;
        }

        /// <summary>
        /// Copy the objects in the currentSelection list
        /// </summary>
        /// <param name="print">A value of 1 denotes copy. Any other value denotes cut</param>
        private void copy2(int print)
        {
            // Only do this once per stylus down
            if (!singleDown)
            {
                currentStrokeCopyList.Clear();
                currentCopyList.Clear();

                currentSelection = sortByCanvasOrder(currentSelection);

                // for every object in the current selection, we want to make a deep copy
                // of the object and then store that in a list. Depending on the identity of
                // the object (shape or text), we need to remember different things
                foreach (UIElement obj in currentSelection)
                {
                    if (strokeBoxes.Contains(obj))
                    {
                        Stroke toCopy = lockedStrokes[strokeBoxes.IndexOf(obj)].Clone();
                        currentStrokeCopyList.Add(toCopy);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                    {
                        Rectangle toCopy = new Rectangle();
                        Rectangle oldShape = (Rectangle)obj;
                        toCopy.Height = oldShape.Height;
                        toCopy.Width = oldShape.Width;
                        toCopy.Fill = oldShape.Fill;
                        toCopy.Stroke = oldShape.Stroke;
                        toCopy.StrokeThickness = oldShape.StrokeThickness;
                        toCopy.RadiusX = InkCanvas.GetLeft(obj);
                        toCopy.RadiusY = InkCanvas.GetTop(obj);
                        currentCopyList.Add(toCopy);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                    {
                        Ellipse toCopy = new Ellipse();
                        Ellipse oldShape = (Ellipse)obj;
                        toCopy.Height = oldShape.Height;
                        toCopy.Width = oldShape.Width;
                        toCopy.Fill = oldShape.Fill;
                        toCopy.Stroke = oldShape.Stroke;
                        toCopy.StrokeThickness = oldShape.StrokeThickness;
                        toCopy.MaxHeight = InkCanvas.GetTop(obj) * 1000;
                        toCopy.MaxWidth = InkCanvas.GetLeft(obj) * 1000;
                        currentCopyList.Add(toCopy);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                    {
                        Polygon toCopy = new Polygon();
                        Polygon oldShape = (Polygon)obj;
                        toCopy.Height = obj.DesiredSize.Height;
                        toCopy.Width = obj.DesiredSize.Width;
                        toCopy.Fill = oldShape.Fill;
                        toCopy.Stroke = oldShape.Stroke;
                        toCopy.StrokeThickness = oldShape.StrokeThickness;
                        toCopy.Points = oldShape.Points;
                        toCopy.MaxHeight = InkCanvas.GetTop(obj) * 1000;
                        toCopy.MaxWidth = InkCanvas.GetLeft(obj) * 1000;
                        currentCopyList.Add(toCopy);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                    {
                        TextBlock toCopy = new TextBlock();
                        TextBlock oldShape = (TextBlock)obj;
                        toCopy.Text = oldShape.Text;
                        toCopy.Height = obj.DesiredSize.Height;
                        toCopy.Width = obj.DesiredSize.Width;
                        toCopy.TextWrapping = oldShape.TextWrapping;
                        toCopy.Measure(new Size(1000, 1000));
                        toCopy.FontSize = oldShape.FontSize;
                        toCopy.FontFamily = oldShape.FontFamily;
                        toCopy.Background = oldShape.Background;
                        toCopy.Foreground = oldShape.Foreground;
                        toCopy.MaxHeight = InkCanvas.GetTop(obj) * 1000;
                        toCopy.MaxWidth = InkCanvas.GetLeft(obj) * 1000;
                        currentCopyList.Add(toCopy);
                    }
                }
            }

            // 1 denotes copying, so give some feedback. All other values denote cut,
            // which already has a visual feedback
            if (print == 1)
            {
                //flashFeedBack2("Selection copied");
            }

            // Do do these above steps again until the next stylus down
            singleDown = true;

            // Remove all the buttons
            removeButton2();

            addStrokeRectangles();
        }

        /// <summary>
        /// Cut all the selection
        /// </summary>
        private void cut2()
        {
            // Call copy, telling it that this is a cut command
            copy2(0);

            // Delete all the shapes and remove all the buttons
            delete2();
            removeButton2();
        }

        /// <summary>
        /// Delete the selected objects
        /// </summary>
        private void delete2()
        {
            // remove all the ojbects in the list from the inkcanvas.
            foreach (UIElement obj in currentSelection)
            {
                if (strokeBoxes.Contains(obj))
                {
                    inkCanvas.Strokes.Remove
                        (lockedStrokes[strokeBoxes.IndexOf(obj)]);
                }
                inkCanvas.Children.Remove(obj);
            }

            // update the selection and remove the buttons
            currentSelection.Clear();
            updateSelection();
            addStrokeRectangles();
            removeButton2();
        }

        /// <summary>
        /// Paste things on the clipboard onto the canvas.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void paste2(double x, double y)
        {
            if (!singleDown)
            {
                // In case nothing is saved onto the clipboard yet, tell the user so
                if (currentCopyList.Count == 0 && currentStrokeCopyList.Count == 0)
                {
                    MessageBoxResult dr = System.Windows.MessageBox.Show(
                        "Nothing to paste.",
                        "",
                        MessageBoxButton.OK,
                        MessageBoxImage.None);
                    removeButton2();
                    return;
                }

                double relativeX = findMiddleX();
                double relativeY = findMiddleY();

                // Once something new is pasted, the old selection should disappear
                // and the new selection automatically selected. This seems to be
                // more intuitive.
                currentSelection.Clear();
                updateSelection();

                // Again, depending on the element in the copied list, perform
                // the desired actions to paste them onto the interface
                foreach (UIElement obj in currentCopyList)
                {
                    if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                    {
                        Rectangle toPaste = new Rectangle();
                        Rectangle old = (Rectangle)obj;
                        toPaste.Height = old.Height;
                        toPaste.Width = old.Width;
                        toPaste.Fill = old.Fill;
                        toPaste.Stroke = old.Stroke;
                        toPaste.StrokeThickness = old.StrokeThickness;
                        toPaste.Measure(new Size(1000, 1000));
                        InkCanvas.SetTop(toPaste, y + old.RadiusY - relativeY);
                        InkCanvas.SetLeft(toPaste, x + old.RadiusX - relativeX);
                        InkCanvas.SetBottom(toPaste, y + old.RadiusY - relativeY + toPaste.Height);
                        InkCanvas.SetRight(toPaste, x + old.RadiusX - relativeX + toPaste.Width);
                        inkCanvas.Children.Add(toPaste);
                        currentSelection.Add(toPaste);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                    {
                        Ellipse toPaste = new Ellipse();
                        Ellipse old = (Ellipse)obj;
                        toPaste.Height = old.Height;
                        toPaste.Width = old.Width;
                        toPaste.Fill = old.Fill;
                        toPaste.Stroke = old.Stroke;
                        toPaste.StrokeThickness = old.StrokeThickness;
                        toPaste.Measure(new Size(1000, 1000));
                        InkCanvas.SetTop(toPaste, y + old.MaxHeight/1000- relativeY);
                        InkCanvas.SetLeft(toPaste, x + old.MaxWidth/1000 - relativeX);
                        InkCanvas.SetBottom(toPaste, y + old.MaxHeight/1000 - relativeY + toPaste.Height);
                        InkCanvas.SetRight(toPaste, x + old.MaxWidth/1000 - relativeX + toPaste.Width);
                        inkCanvas.Children.Add(toPaste);
                        currentSelection.Add(toPaste);
                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                    {
                        Polygon toPaste = new Polygon();
                        Polygon old = (Polygon)obj;
                        toPaste.Height = old.Height;
                        toPaste.Width = old.Width;
                        toPaste.Fill = old.Fill;
                        toPaste.Stroke = old.Stroke;
                        toPaste.StrokeThickness = old.StrokeThickness;
                        toPaste.Points = old.Points;
                        toPaste.Measure(new Size(1000, 1000));
                        InkCanvas.SetTop(toPaste, y + old.MaxHeight / 1000 - relativeY);
                        InkCanvas.SetLeft(toPaste, x + old.MaxWidth / 1000 - relativeX);
                        InkCanvas.SetBottom(toPaste, y + old.MaxHeight / 1000 - relativeY + toPaste.Height);
                        InkCanvas.SetRight(toPaste, x + old.MaxWidth / 1000 - relativeX + toPaste.Width);
                        inkCanvas.Children.Add(toPaste);
                        currentSelection.Add(toPaste);

                    }
                    else if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                    {
                        TextBlock toPaste = new TextBlock();
                        TextBlock old = (TextBlock)obj;
                        toPaste.Height = obj.DesiredSize.Height;
                        toPaste.Width = obj.DesiredSize.Width;
                        toPaste.Text = old.Text;
                        toPaste.TextWrapping = old.TextWrapping;
                        toPaste.Measure(new Size(1000, 1000));
                        toPaste.FontSize = old.FontSize;
                        toPaste.FontFamily = old.FontFamily;
                        toPaste.Background = old.Background;
                        toPaste.Foreground = old.Foreground;
                        copyList.Add(toPaste);
                        toPaste.Measure(new Size(1000, 1000));
                        InkCanvas.SetTop(toPaste, y + old.MaxHeight / 1000 - relativeY);
                        InkCanvas.SetLeft(toPaste, x + old.MaxWidth / 1000 - relativeX);
                        InkCanvas.SetBottom(toPaste, y + old.MaxHeight / 1000 - relativeY + toPaste.Height);
                        InkCanvas.SetRight(toPaste, x + old.MaxWidth / 1000 - relativeX + toPaste.Width);
                        inkCanvas.Children.Add(toPaste);
                        currentSelection.Add(toPaste);
                    }
                }

                // Delete the last stroke, remove buttons, and update the selection.
                singleDown = true;
                deleteLastStroke();

                foreach (Stroke stroke in currentStrokeCopyList)
                {

                        Stroke newStroke = stroke.Clone();
                        StylusPointCollection newPoints = new StylusPointCollection();
                        for (int j = 0; j < newStroke.StylusPoints.Count; j++)
                        {
                            StylusPoint point = newStroke.StylusPoints[j];
                            point.X = point.X + x - relativeX;
                            point.Y = point.Y + y - relativeY;
                            newPoints.Add(point);
                        }
                        newStroke.StylusPoints = newPoints;
                        inkCanvas.Strokes.Add(newStroke);
                        Rectangle rec = new Rectangle();
                        rec.Height = newStroke.GetBounds().Height;
                        rec.Width = newStroke.GetBounds().Width;
                        rec.Fill = Brushes.Transparent;
                        InkCanvas.SetLeft(rec, newStroke.GetBounds().Left);
                        InkCanvas.SetTop(rec, newStroke.GetBounds().Top);
                        InkCanvas.SetBottom(rec, newStroke.GetBounds().Bottom);
                        InkCanvas.SetRight(rec, newStroke.GetBounds().Right);

                        strokeBoxes.Add(rec);
                        lockedStrokes.Add(newStroke);
                        inkCanvas.Children.Insert(0, rec);
                        currentSelection.Add(rec);
                }

                removeButton2();
                updateSelection();

                // Also disable selection for now, and remove all the handles. This is to prevent
                // immediate modification of the current, newly pasted selection
                if (shouldSelect)
                    removeSelectionHandle();
                shouldSelect = false;
            }
        }

        /// <summary>
        /// Sort the given list based on the order of the elements on the canvas
        /// </summary>
        /// <param name="currentCopyList">The list to sort</param>
        /// <returns></returns>
        List<UIElement> sortByCanvasOrder(List<UIElement> currentCopyList)
        {
            List<UIElement> sorted = new List<UIElement>();
            UIElementCollection children = inkCanvas.Children;

            // Look through inkcanvas and add the object common both to it and the copy
            // list onto a new list. Return that list as the sorted version of the given list
            foreach (UIElement obj in children)
            {
                if (currentCopyList.Contains(obj))
                    sorted.Add(obj);
            }
            return sorted;
        }

        /// <summary>
        /// Calculate the middle Y value
        /// </summary>
        /// <returns></returns>
        private double findMiddleY()
        {
            double min = 5000;
            double max = 0;

            // each type of object needs to be treated differently
            foreach (UIElement obj in currentCopyList)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle s = (Rectangle)obj;
                    if (s.RadiusY < min)
                        min = s.RadiusY;
                    if (s.RadiusY > max)
                        max = s.RadiusY;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse s = (Ellipse)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                    if (s.MaxHeight / 1000 > max)
                        max = s.MaxHeight / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon s = (Polygon)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                    if (s.MaxHeight / 1000 > max)
                        max = s.MaxHeight / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock s = (TextBlock)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                    if (s.MaxHeight / 1000 > max)
                        max = s.MaxHeight / 1000;
                }
            }

            foreach (Stroke stroke in currentStrokeCopyList)
            {
                if (stroke.GetBounds().Top < min)
                    min = stroke.GetBounds().Top;
                if (stroke.GetBounds().Top > max)
                    max = stroke.GetBounds().Top;
            }

            return (max + min) / 2;
        }
        
        /// <summary>
        /// Find the middle X value
        /// </summary>
        /// <returns></returns>
        private double findMiddleX()
        {
            double min = 5000;
            double max = 0;

            // Need to treat different types of objects differently
            foreach (UIElement obj in currentCopyList)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle s = (Rectangle)obj;
                    if (s.RadiusX < min)
                        min = s.RadiusX;
                    if (s.RadiusX > max)
                        max = s.RadiusX;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse s = (Ellipse)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                    if (s.MaxWidth / 1000 > max)
                        max = s.MaxWidth / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon s = (Polygon)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                    if (s.MaxWidth / 1000 > max)
                        max = s.MaxWidth / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock s = (TextBlock)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                    if (s.MaxWidth / 1000 > max)
                        max = s.MaxWidth / 1000;
                }
            }

            foreach (Stroke stroke in currentStrokeCopyList)
            {
                if (stroke.GetBounds().Left < min)
                    min = stroke.GetBounds().Left;
                if (stroke.GetBounds().Left > max)
                    max = stroke.GetBounds().Left;
            }

            return (max + min) / 2;
        }

        /// <summary>
        /// Find the minimum Y value among the elements
        /// </summary>
        /// <param name="list">List of elements we are acting on</param>
        /// <returns></returns>
        private double findMinY(List<UIElement> list)
        {
            double min = 5000;

            foreach (UIElement obj in list)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle s = (Rectangle)obj;
                    if (s.RadiusY < min)
                        min = s.RadiusY;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse s = (Ellipse)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon s = (Polygon)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock s = (TextBlock)obj;
                    if (s.MaxHeight / 1000 < min)
                        min = s.MaxHeight / 1000;
                }
            }

            return min;
        }

        /// <summary>
        /// Find the max X value among the given objects
        /// </summary>
        /// <param name="list">The list of objects we are acting on</param>
        /// <returns></returns>
        private double findMinX(List<UIElement> list)
        {

            double min = 5000;

            foreach (UIElement obj in list)
            {
                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle s = (Rectangle)obj;
                    if (s.RadiusX < min)
                        min = s.RadiusX;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse s = (Ellipse)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon s = (Polygon)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                }

                if (obj.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock s = (TextBlock)obj;
                    if (s.MaxWidth / 1000 < min)
                        min = s.MaxWidth / 1000;
                }
            }

            return min;
        }

        #endregion


        #region feedBack

        /// <summary>
        /// Flash a message to the user briefly in large font. Intended for testing.
        /// </summary>
        /// <param name="message">The message to print</param>
        private void flashFeedBack2(String message)
        {
            if (singleDown == false)
            {
                System.Windows.Controls.Label feedback = new System.Windows.Controls.Label();
                feedback.Content = message;
                feedback.FontFamily = new FontFamily("Verdana");
                feedback.Foreground = Brushes.Red;
                feedback.FontSize = 24;
                feedback.Opacity = 0.3;
                InkCanvas.SetLeft(feedback, 60);
                InkCanvas.SetTop(feedback, 2);
                inkCanvas.Children.Add(feedback);
                Timer timer = new Timer();
                timer.Interval = 1500;
                timer.Start();
                timer.Tick += new EventHandler(timer_Tick);
                singleDown = true;
            }
        }

        #endregion
    }
}
