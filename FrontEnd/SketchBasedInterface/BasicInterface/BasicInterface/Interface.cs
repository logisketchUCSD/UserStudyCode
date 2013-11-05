// This file primarily contains functionality for the "Basic Interface," that
// is, the interface that does NOT implement crossing selection.

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

namespace BasicInterface
{
    public partial class Interface : Form
    {

        #region data members

        internal double X;      // keeps track of the current cursor position
        internal double Y;      
        internal double moveX;  // keeps track of the cursor position for the move event
        internal double moveY;

        // Creating the controls necessary to initialize the application
        Border myBorder;
        Window basicWindow;
        InkCanvas inkCanvas;
        DrawingAttributes inkDA;
        System.Windows.Controls.Button helpMenu = new System.Windows.Controls.Button();

        UIElement actionMenuShape;
        List<UIElement> copyList;
        bool singleStylusDown = false;       // don't want to do things repeatedly in one stylus down

        #endregion

        #region Global Constants

        double BUTTON_HEIGHT = 20;      // height of a menu button
        double BUTTON_WIDTH = 50;       // width of a menu button
        double BUTTON_HEIGHT2 = 20;     // alternative height
        double BUTTON_WIDTH2 = 35;      // alternative width
        double SPACE_BUTTON_TEXT = 10;
        double LIST_PADDING = 2;        // the padding between different items in a list
        double COLOR_SQUARE_SIDE = 15;  // the size of the squares in the list
        double REC_BORDER = 2;

        #endregion

        /// <summary>
        /// initialize everything
        /// </summary>
        internal Interface()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                debugExceptionMessage("Interface Constructor", e);
            }
            
        }

        /// <summary>
        /// shows messages easily for exception, and for hiding them
        /// when we are not in debugging mode
        /// </summary>
        /// <param name="methodName">the method in which exception is thrown</param>
        /// <param name="e">the exception</param>
        public void debugExceptionMessage(String methodName, Exception e)
        {
            System.Windows.Forms.MessageBox.Show(methodName + " threw an exception: " + e.ToString());
        }


        #region Event Handler

        /// <summary>
        /// The form displayed allows the user to open various window
        /// applications via different buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitBasic_Click(object sender, EventArgs e)
        {
            // initialize all the basic components
            basicWindow = new Window();
            basicWindow.Show();
            myBorder = new Border();
            inkCanvas = new InkCanvas();
            inkDA = new DrawingAttributes();
            helpMenu = new System.Windows.Controls.Button();

            actionMenuShape = new UIElement();
            copyList = new List<UIElement>();

            basicWindow.Content = myBorder;
            myBorder.Child = inkCanvas;
            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;

            // set the gestures active on the screen
            inkCanvas.SetEnabledGestures(new ApplicationGesture[] 
                                        { ApplicationGesture.Square,
                                          ApplicationGesture.Circle,
                                          ApplicationGesture.Triangle,
                                          ApplicationGesture.SemicircleRight,
                                          ApplicationGesture.SemicircleLeft,
                                          ApplicationGesture.Check,
                                          ApplicationGesture.Curlicue,
                                          ApplicationGesture.DoubleCurlicue,
                                          ApplicationGesture.ScratchOut,
                                          ApplicationGesture.Star});

            // Add the event listeners important to the application
            inkCanvas.Gesture += new InkCanvasGestureEventHandler(inkCanvas_Gesture);
            inkCanvas.StylusDown += new System.Windows.Input.StylusDownEventHandler(inkCanvas_StylusDown);
            inkCanvas.MouseDown += new System.Windows.Input.MouseButtonEventHandler(inkCanvas_MouseDown);
            inkCanvas.StylusMove += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusMove);
            inkCanvas.StylusUp += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusUp);
            inkCanvas.StylusLeave += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusLeave);
            inkCanvas.KeyDown += new System.Windows.Input.KeyEventHandler(inkCanvas_KeyDown);
            inkCanvas.KeyUp += new System.Windows.Input.KeyEventHandler(inkCanvas_KeyUp);

            // initialize properties of the ink canvas
            inkCanvas.Background = Brushes.White;

            // initialize properties of the ink
            inkDA.Color = Colors.Black;
            inkDA.Height = 2;
            inkDA.Width = 1;
            inkDA.FitToCurve = false;
            inkCanvas.DefaultDrawingAttributes = inkDA;

            placeHelpMenu();
        }

        void inkCanvas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.ToString().Equals("S"))
            {
                buttonSelect = false;
                transferSelection();
                inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
            }
        }

        void inkCanvas_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.ToString().Equals("S"))
            {
                buttonSelect = true;
                currentSelection.Clear();
                updateSelection();
                removeSelectionHandle();
                inkCanvas.EditingMode = InkCanvasEditingMode.Select;
            }
        }

        /// <summary>
        /// Event handler for what happens when the stylus leaves the inkCanvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusLeave(object sender, System.Windows.Input.StylusEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
            repositionObjects();
        }

        /// <summary>
        /// Reposition all the objects on the inkCanvas
        /// </summary>
        private void repositionObjects()
        {
            UIElementCollection shapes = inkCanvas.Children;
            foreach (UIElement shape in shapes)
            {
                InkCanvas.SetRight(shape, InkCanvas.GetLeft(shape) + shape.DesiredSize.Width);
                InkCanvas.SetBottom(shape, InkCanvas.GetTop(shape) + shape.DesiredSize.Height);
            }
        }

        /// <summary>
        /// Place the help menu button on the upper left hand corner of the inkCanvas.
        /// The help menu displays a short list of commands that are often used in the interface.
        /// </summary>
        private void placeHelpMenu()
        {
            helpMenu.Height = 20;
            helpMenu.Width = 60;
            helpMenu.FontSize = 10;
            helpMenu.Content = "Help Menu";
            InkCanvas.SetLeft(helpMenu, 0);
            InkCanvas.SetTop(helpMenu, 0);
            InkCanvas.SetRight(helpMenu, helpMenu.Width);
            InkCanvas.SetBottom(helpMenu, helpMenu.Height);
            inkCanvas.Children.Add(helpMenu);
        }

        /// <summary>
        /// Event handler for StylusMove event on the ink canvas.
        /// Occurs every time the stylus moves within the boundary of the
        /// inkCanvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusMove(object sender, System.Windows.Input.StylusEventArgs e)
        {
            Point pos = e.GetPosition(inkCanvas);
            moveX = pos.X;
            moveY = pos.Y;

            // Actions depend on the shape the stylus is over, if any
            UIElement shape = findShape(moveX, moveY);

            // if starting a stroke on an empty space, then clear any existing item controls
            if (shape == null)
            {
                deleteItemsControl();
                return;
            }

            // if the current shape is a button, then display the appropriate menu based
            // on the content of the button
            if (shape.GetType().ToString().Equals(("System.Windows.Controls.Button")))
            {
                System.Windows.Controls.Button b = (System.Windows.Controls.Button)shape;
                String label = b.Content.ToString();
                displayButtonResult(label, InkCanvas.GetLeft(shape), InkCanvas.GetTop(shape));
            }
        }


        /// <summary>
        /// Event handler for the StylusDown event. Every time the stylus is placed
        /// onto the screen, this event handler is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusDown(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            Point pos = e.GetPosition(inkCanvas);
            X = pos.X;
            Y = pos.Y;

            // Once the stylus hits the screen, the user no longer wishes to perform selection.
            // Thus, pause the timer and disable selection
            hoverTime.Stop();
            shouldSelect = false;

            // If the user starts a button on an empty space, then clear all the existing
            // buttons.
            UIElement currentShape = findShape(X, Y);
            if (currentShape == null)
            {
                removeButton();
                return;
            }

            // If the button clicked is the help button, then display help menu. Otherwise
            // if it's not a button, then clear all the buttons as well.
            bool isButton = currentShape.GetType().ToString().Equals("System.Windows.Controls.Button");
            if (isButton && currentShape.Equals(helpMenu))
                displayHelp();
            if (!isButton)
                removeButton();
        }

        
        /// <summary>
        /// The mouseDown counterpart of StylusDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(inkCanvas);
            X = pos.X;
            Y = pos.Y;
        }

        /// <summary>
        /// Event handler for lifting the stylus from the InkCanvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_StylusUp(object sender, System.Windows.Input.StylusEventArgs e)
        {
            // reset singleStylusDown to false, since the next stylusDown will be the first
            // occurance of that stylus down.
            if (singleStylusDown)
            {
                singleStylusDown = false;
                deleteLastStroke();
            }
            Point pos = e.GetPosition(inkCanvas);
            X = pos.X;
            Y = pos.Y;
            UIElement shape = findShape(X, Y);

            // If the stylus up doesn't occur on any object, just return and do nothing
            if (shape == null)
                return;

            // If the stylus up is on an itemcontrol, then based on the content of the item
            // control, perform the appropriate action
            if (shape.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
            {
                ItemsControl list = (ItemsControl)shape;
                Point listPos = e.GetPosition(shape);
                if (list.Background == Brushes.LightGreen)
                    changeTextColor(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightSkyBlue)
                    changeTextBackground(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightCyan)
                    changeFontSize(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.Cornsilk)
                    changeFont(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.LightSlateGray)
                    changeShapeColor(listPos.X, listPos.Y, list);
                if (list.Background == Brushes.Turquoise)
                    changeShapeBorder(listPos.X, listPos.Y, list);
            }
        }


        /// <summary>
        /// Event handler for gesture events on the inkCanvas. Recognizes
        /// gestures and calls the appropriate method to deal with each
        /// gesture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {
            ReadOnlyCollection<GestureRecognitionResult> gestureResults =
                e.GetGestureRecognitionResults();
            Rect bounds = e.Strokes.GetBounds();

            // Based on the recognized gesture, perform different actions
            switch (gestureResults[0].ApplicationGesture)
            {
                case ApplicationGesture.Square:
                    addRectangle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Circle:
                    addCircle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Triangle:
                    addTriangle(bounds.X, bounds.Y, bounds.Height, bounds.Width);
                    break;
                case ApplicationGesture.Check:
                    addText(X, Y);
                    break;
                case ApplicationGesture.SemicircleRight:
                    displayMenu(X, Y);
                    break;
                case ApplicationGesture.SemicircleLeft:
                    displayActionMenu(moveX, moveY);
                    break;
                case ApplicationGesture.Curlicue:
                    deleteLastStroke();
                    break;
                case ApplicationGesture.ScratchOut:
                    deleteShapeAt(X, Y);
                    break;
                case ApplicationGesture.Star:
                    locateObjects();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// A temporary private method to locate the position of all the objects on
        /// the interface. The purpose is to detect objects who's location on the InkCanvas
        /// is incorrectly set, due to NaN values or others. Not really in use anymore.
        /// </summary>
        private void locateObjects()
        {
            UIElementCollection shapes = inkCanvas.Children;
            List<Rectangle> rectangles = new List<Rectangle>();

            // Place a blue rectangle to mark the position of each object on the canvas.
            foreach (UIElement shape in shapes)
            {
                Rectangle rec = new Rectangle();
                rec.Height = 10;
                rec.Width = 10;
                rec.Fill = Brushes.Blue;
                InkCanvas.SetLeft(rec, InkCanvas.GetLeft(shape));
                InkCanvas.SetTop(rec, InkCanvas.GetTop(shape) + shape.DesiredSize.Height);
                rectangles.Add(rec);
            }

            foreach (Rectangle rec in rectangles)
                inkCanvas.Children.Add(rec);
        }
        #endregion


        #region Stroke Manipulation

        /// <summary>
        /// Delete all the strokes on the ink canvas
        /// </summary>
        private void deleteEverything()
        {
            // Check to make sure the user really wants to delete ALL of the strokes
            MessageBoxResult dr = System.Windows.MessageBox.Show(
                        "Delete all shapes and strokes?",
                        "Confirm",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Exclamation);
            if (dr == MessageBoxResult.OK) // only go ahead and delete if OK button is pressed
            {
                inkCanvas.Strokes.Clear();
                strokeBoxes.Clear();
                lockedStrokes.Clear();

                inkCanvas.Children.Clear();
                currentSelection.Clear();
                currentHandled.Clear();
                selectionBoxes.Clear();
                unselectedBoxes.Clear();
                selectHandle.Clear();
                rotationCircles.Clear();
                rotationShapes.Clear();
                rotationStrokeCircles.Clear();
                rotationStrokes.Clear();
                selectionBoxTransforms.Clear();
                selectionBoxTransformShapes.Clear();
                resizerRectangles.Clear();
                resizerStrokeRectangles.Clear();
            }
        }

        /// <summary>
        /// Delete the last stroke added to the inkcanvas. That is, the last element in the
        /// inkcanvas.strokes collection.
        /// </summary>
        private void deleteLastStroke()
        {
            if (inkCanvas.Strokes.Count == 0)
                return;
            if (lockedStrokes.Contains(inkCanvas.Strokes[inkCanvas.Strokes.Count - 1]))
                return;
            inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);
        }


        #endregion


        #region Change Properties

        /// <summary>
        /// Change the background color of a textblock
        /// </summary>
        /// <param name="x">x coordinate of stylus down</param>
        /// <param name="y">y coordinate of stylus down</param>
        /// <param name="list">The itemlist the stylus down fell on</param>
        private void changeTextBackground(double x, double y, ItemsControl list)
        {
            // Get the color represented by the color box clicked, and then set the background
            // color according to that.
            Rectangle rec = getColorBox(x, y, list);
            TextBlock source = findTextFromItems(list);
            if(!(rec==null))
                source.Background = rec.Fill;
            deleteItemsControl();
            deleteLastStroke();
        }

        /// <summary>
        /// Change the color of the text in the textblock
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        private void changeTextColor(double x, double y, ItemsControl list)
        {
            Rectangle rec = getColorBox(x, y, list);
            TextBlock source = findTextFromItems(list);
            if (!(rec == null))
                source.Foreground = rec.Fill;
            deleteItemsControl();
            deleteLastStroke();
        }

        /// <summary>
        /// Change the font size of the text in the text block
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        private void changeFontSize(double x, double y, ItemsControl list)
        {
            System.Windows.Controls.Button b = getBox(x, y, list);
            TextBlock source = findTextFromItems(list);
            source.FontSize = (int)(b.Content);
            deleteItemsControl();
            deleteLastStroke();
            source.Measure(new Size(1000, 1000));
            InkCanvas.SetRight(source, InkCanvas.GetLeft(source) + source.DesiredSize.Width);
            InkCanvas.SetBottom(source, InkCanvas.GetTop(source) + source.DesiredSize.Height);
        }

        /// <summary>
        /// Change the font of the text in the textblock
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        private void changeFont(double x, double y, ItemsControl list)
        {
            System.Windows.Controls.Button b = getBox(x, y, list);
            TextBlock source = findTextFromItems(list);
            String fontName = (String)(b.Content);
            switch (fontName)
            {
                case "Ari":
                    source.FontFamily = new FontFamily("Arial");
                    break;
                case "Com":
                    source.FontFamily = new FontFamily("Comic Sans MS");
                    break;
                case "Cou":
                    source.FontFamily = new FontFamily("Courier New");
                    break;
                case "Luc":
                    source.FontFamily = new FontFamily("Lucida Grande");
                    break;
                case "TNR":
                    source.FontFamily = new FontFamily("Times New Roman");
                    break;
                case "Ver":
                    source.FontFamily = new FontFamily("Verdana");
                    break;
                default:
                    break;
            }
            deleteItemsControl();
            deleteLastStroke();
            source.Measure(new Size(1000, 1000));
            InkCanvas.SetRight(source, InkCanvas.GetLeft(source) + source.DesiredSize.Width);
            InkCanvas.SetBottom(source, InkCanvas.GetTop(source) + source.DesiredSize.Height);
        }

        /// <summary>
        /// Change the color of the shape
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        private void changeShapeColor(double x, double y, ItemsControl list)
        {
            Rectangle rec = getColorBox(x, y, list);
            Shape source = findShapeFromItems(list);
            if (!(rec == null))
                source.Fill = rec.Fill;
            deleteItemsControl();
            deleteLastStroke();
        }

        /// <summary>
        /// Change the color of the shape border
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        private void changeShapeBorder(double x, double y, ItemsControl list)
        {
            Rectangle rec = getColorBox(x, y, list);
            Shape source = findShapeFromItems(list);
            if (!(rec == null))
                source.Stroke = rec.Fill;
            deleteItemsControl();
            deleteLastStroke();
        }

        /// <summary>
        /// Based on the input, find the box on the item control that the stylus fell onto.
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        /// <returns>Grab the box and returns as a button</returns>
        private System.Windows.Controls.Button getBox(double x, double y, ItemsControl list)
        {
            double height = list.DesiredSize.Height;
            int numItem = list.Items.Count;
            int desiredItem = (int)(y / ((int)height / numItem));
            return (System.Windows.Controls.Button)list.Items[desiredItem];
        }

        /// <summary>
        /// Based on the input, find the color box that the stylus fell onto
        /// </summary>
        /// <param name="x">x coordinate of the stylus</param>
        /// <param name="y">y coordinate of the stylus</param>
        /// <param name="list">The item list that the stylus fell on</param>
        /// <returns>A rectangle representing the color box</returns>
        private Rectangle getColorBox(double x, double y, ItemsControl list)
        {
            double height = list.DesiredSize.Height;
            int numItem = list.Items.Count;
            int desiredItem = (int)(y / ((int)height / numItem));
            Rectangle item = null;
            try
            {
                item = (Rectangle)list.Items[desiredItem];
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                System.Console.WriteLine(e.Message);
            }
            return item;
        }

        /// <summary>
        /// Based on the given items control, find the specific object that it is
        /// acting on.
        /// </summary>
        /// <param name="list">The given items control in focus</param>
        /// <returns>the textblock that the item control should be acting on</returns>
        private TextBlock findTextFromItems(ItemsControl list)
        {
            // FLAW: For simplicity, only checking the horizontal positions,
            // not vertical ones. This simplification should not be a problem,
            // since the position is so exact that it's nearly impossible to get
            // the same position for multiple objects. But should fix this if
            // this assumption turns out to be problematic.
            double top = InkCanvas.GetBottom(list) + BUTTON_HEIGHT + SPACE_BUTTON_TEXT;
            UIElementCollection allChildren = inkCanvas.Children;
            foreach (UIElement child in allChildren)
            {
                // need to take into consideration both of the itemsControl possible
                if (InkCanvas.GetTop(child) == top)
                    return (TextBlock)child;
            }
            return null;
        }

        /// <summary>
        /// Based on the item control, find the shape that it should be acting on
        /// </summary>
        /// <param name="list">The item list in focus that we are interested in</param>
        /// <returns>The shape that the item list should be acting on</returns>
        private Shape findShapeFromItems(ItemsControl list)
        {
            double top = InkCanvas.GetBottom(list) + BUTTON_HEIGHT + SPACE_BUTTON_TEXT;
            UIElementCollection allChildren = inkCanvas.Children;
            foreach (UIElement child in allChildren)
            {
                // need to take into consideration both of the itemsControl possible
                if (InkCanvas.GetTop(child) == top)
                    return (Shape)child;
            }
            return null;
        }


        #endregion


        #region Button Interaction
        /// <summary>
        /// Each time the stylus crosses over a button, depending on the name
        /// of the button, display the appropriate menu for the user to interact
        /// with.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        private void displayButtonResult(string label, double left, double top)
        {
            switch (label)
            {
                case "Txt Color":  // first check to see if the ItemsControl already exists
                    if (exists(Brushes.LightGreen))
                        return;
                    createColor(left, top, Brushes.LightGreen); // if it doesn't exist, create it
                    break;
                case "Bg Color":
                    if (exists(Brushes.LightSkyBlue))
                        return;
                    createColor(left, top, Brushes.LightSkyBlue);
                    break;
                case "Font Size":
                    if (exists(Brushes.LightCyan))
                        return;
                    createSize(left, top, Brushes.LightCyan);
                    break;
                case "Font":
                    if (exists(Brushes.Cornsilk))
                        return;
                    createFont(left, top, Brushes.Cornsilk);
                    break;
                case "Color":
                    if (exists(Brushes.LightSlateGray))
                        return;
                    createColor(left, top, Brushes.LightSlateGray);
                    break;
                case "Border":
                    if (exists(Brushes.Turquoise))
                        return;
                    createColor(left, top, Brushes.Turquoise);
                    break;
                case "Copy":
                    copy(1);
                    break;
                case "Paste":
                    paste(X, Y);
                    break;
                case "Cut":
                    cut();
                    break;
                case "Delete":
                    delete();
                    break;
                case "Select":
                    select();
                    break;
                default:
                    break;
            }
        }

        

        /// <summary>
        /// Specifies the properties of a given button located at the given
        /// positions on the InkCanvas.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        private System.Windows.Controls.Button setButtonProperties(
            double left, double right, double top, double bottom)
        {
            double height = BUTTON_HEIGHT;
            double width = BUTTON_WIDTH;

            System.Windows.Controls.Button b = new System.Windows.Controls.Button();
            b.Height = height;
            b.Width = width;
            b.FontSize = 10;
            InkCanvas.SetLeft(b, left);
            InkCanvas.SetRight(b, left + width);
            InkCanvas.SetTop(b, top - height - SPACE_BUTTON_TEXT);
            InkCanvas.SetBottom(b, top - SPACE_BUTTON_TEXT);
            inkCanvas.Children.Add(b);
            return b;
        }

        private System.Windows.Controls.Button setButtonProperties2(
            double left, double right, double top, double bottom)
        {
            double height = BUTTON_HEIGHT2;
            double width = BUTTON_WIDTH2;

            System.Windows.Controls.Button b = new System.Windows.Controls.Button();
            b.Height = height;
            b.Width = width;
            b.FontSize = 9;
            InkCanvas.SetLeft(b, left);
            InkCanvas.SetRight(b, left + width);
            InkCanvas.SetTop(b, top - height - SPACE_BUTTON_TEXT);
            InkCanvas.SetBottom(b, top - SPACE_BUTTON_TEXT);
            inkCanvas.Children.Add(b);
            return b;
        }


        private void removeButton()
        {
            UIElementCollection children = inkCanvas.Children;
            UIElement child;
            for (int i = 0; i < children.Count; i++)
            {
                child = children[i];
                if (child.GetType().ToString().Equals("System.Windows.Controls.Button"))
                {
                    if (InkCanvas.GetBottom(child) == 20) // do not remove the Help Menu button
                        continue;
                    inkCanvas.Children.Remove(child);
                    i--; // current one removed and the next one moved up. Check again.
                }
            }
        }

        private void displayHelp()
        {
            MessageBoxResult dr = System.Windows.MessageBox.Show(
                        "Gesture List: \n \n" +
                        "Note: These instructions are only applicable for this interface \n" +
                        "which does not implement selection. Please use the selection \n" +
                        "interface to use HoverCross. \n \n" +
                        "Note: If multiple property changes are desirable, please \n" +
                        "do not lift the stylus after pressing the button, and only lift \n" +
                        "it once you made the property change. If only one property change \n" +
                        "is desired, then this is not necessary. \n \n" +
                        "Text Recognition:    Check \n" +
                        "Bring Up Menus:      SemiCircleRight \n" +
                        "Delete Last Stroke:   Curlique (towards upper right) \n" +
                        "Delete All Strokes:    DoubleCurlique (towards upper right) \n" +
                        "Delete shape:            Scratchout on top of the shape \n",
                        "Help Menu",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
        }

        /// <summary>
        /// display a menu over the shape at which this method is called. What
        /// the menu contains is totally dependent on what the calling object is
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void displayMenu(double x, double y)
        {
            UIElement shape = findShape(x, y);
            if (shape == null)
                return;
            String type = shape.ToString();

            double left = InkCanvas.GetLeft(shape);
            double right = InkCanvas.GetRight(shape);
            double top = InkCanvas.GetTop(shape);
            double bottom = InkCanvas.GetBottom(shape);

            if (type.Equals("System.Windows.Shapes.Rectangle") ||
                type.Equals("System.Windows.Shapes.Ellipse") ||
                type.Equals("System.Windows.Shapes.Polygon"))
            {
                System.Windows.Controls.Button color = new System.Windows.Controls.Button();
                color = setButtonProperties(left, right, top, bottom);
                color.Content = "Color";

                System.Windows.Controls.Button border = new System.Windows.Controls.Button();
                border = setButtonProperties(left + BUTTON_WIDTH, right + BUTTON_WIDTH, 
                    top, bottom);
                border.Content = "Border";
            }

            else if (type.Equals("System.Windows.Controls.TextBlock"))
            {
                System.Windows.Controls.Button textColor = new System.Windows.Controls.Button();
                textColor = setButtonProperties(left, right, top, bottom);
                textColor.Content = "Txt Color";

                System.Windows.Controls.Button background = new System.Windows.Controls.Button();
                background = setButtonProperties(left + BUTTON_WIDTH, right + BUTTON_WIDTH,
                    top, bottom);
                background.Content = "Bg Color";

                System.Windows.Controls.Button fontSize = new System.Windows.Controls.Button();
                fontSize = setButtonProperties(left + 2 * BUTTON_WIDTH, right + 2 * BUTTON_WIDTH,
                    top, bottom);
                fontSize.Content = "Font Size";

                System.Windows.Controls.Button font = new System.Windows.Controls.Button();
                font = setButtonProperties(left + 3 * BUTTON_WIDTH, right + 3 * BUTTON_WIDTH,
                    top, bottom);
                font.Content = "Font";
            }


        }


        #endregion

        #region Action Menu

        private void displayActionMenu(double x, double y)
        {
            UIElement shape = findShape(X, Y); // use global stylusDown coordinates
                                               // to locate the object
            
            actionMenuShape = shape;
            // use x, y to determine where to put the buttons
            if (shape == null)
                return;
            String type = shape.ToString();
            double RADIUS = 16;
            double HALFR = 8;

            if (type.Equals("System.Windows.Shapes.Rectangle") ||
                type.Equals("System.Windows.Shapes.Ellipse") ||
                type.Equals("System.Windows.Shapes.Polygon") ||
                type.Equals("System.Windows.Controls.TextBlock"))
            {
                System.Windows.Controls.Button copy = new System.Windows.Controls.Button();
                copy = setButtonProperties2(x + RADIUS, x + BUTTON_WIDTH2 + RADIUS, 
                    y + BUTTON_HEIGHT2, y + 2 * BUTTON_HEIGHT2);
                copy.Content = "Copy";

                System.Windows.Controls.Button cut = new System.Windows.Controls.Button();
                cut = setButtonProperties2(x + HALFR, x + HALFR + BUTTON_WIDTH2,
                    y - HALFR + BUTTON_HEIGHT2 / 2, y - HALFR + BUTTON_HEIGHT2 * 1.5);
                cut.Content = "Cut";

                System.Windows.Controls.Button paste = new System.Windows.Controls.Button();
                paste = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                    y - HALFR, y - HALFR + BUTTON_HEIGHT2);
                paste.Content = "Paste";

                System.Windows.Controls.Button delete = new System.Windows.Controls.Button();
                delete = setButtonProperties2(x - HALFR - BUTTON_WIDTH2, x - HALFR,
                    y - HALFR + BUTTON_HEIGHT2 / 2, y - HALFR + BUTTON_HEIGHT2 * 1.5);
                delete.Content = "Delete";

                System.Windows.Controls.Button select = new System.Windows.Controls.Button();
                select = setButtonProperties2(x - RADIUS - BUTTON_WIDTH2, x - RADIUS,
                    y + BUTTON_HEIGHT2, y + 2 * BUTTON_HEIGHT2);
                select.Content = "Select";

                /*
                System.Windows.Controls.Button move = new System.Windows.Controls.Button();
                move = setButtonProperties2(x - HALFR - BUTTON_WIDTH2, x - HALFR,
                    y + HALFR + BUTTON_HEIGHT2 * 1.5, y + HALFR + BUTTON_HEIGHT2 * 2.5);
                move.Content = "Move";

                System.Windows.Controls.Button rotate = new System.Windows.Controls.Button();
                rotate = setButtonProperties2(x - BUTTON_WIDTH2 / 2, x + BUTTON_WIDTH2 / 2,
                    y + HALFR + BUTTON_HEIGHT2 * 2, y + HALFR + BUTTON_HEIGHT2 * 3);
                rotate.Content = "Rotate";

                System.Windows.Controls.Button resize = new System.Windows.Controls.Button();
                resize = setButtonProperties2(x + HALFR, x + HALFR + BUTTON_WIDTH2,
                    y + HALFR + BUTTON_HEIGHT2 * 1.5, y + HALFR + BUTTON_HEIGHT2 * 2.5);
                resize.Content = "Resize";
                 */
            }
        }



        private void paste(double x, double y)
        {
            if (!singleStylusDown)
            {
                if (copyList.Count == 0)
                {
                    flashFeedBack("Nothing to Paste");
                    return;
                }
                if (copyList[0].GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle toPaste = new Rectangle();
                    Rectangle old = (Rectangle)copyList[0];
                    toPaste.Height = old.Height;
                    toPaste.Width = old.Width;
                    toPaste.Fill = old.Fill;
                    toPaste.Stroke = old.Stroke;
                    toPaste.StrokeThickness = old.StrokeThickness;
                    InkCanvas.SetTop(toPaste, y);
                    InkCanvas.SetLeft(toPaste, x);
                    InkCanvas.SetBottom(toPaste, y + toPaste.Height);
                    InkCanvas.SetRight(toPaste, x + toPaste.Width);
                    inkCanvas.Children.Add(toPaste);
                }
                else if (copyList[0].GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse toPaste = new Ellipse();
                    Ellipse old = (Ellipse)copyList[0];
                    toPaste.Height = old.Height;
                    toPaste.Width = old.Width;
                    toPaste.Fill = old.Fill;
                    toPaste.Stroke = old.Stroke;
                    toPaste.StrokeThickness = old.StrokeThickness;
                    InkCanvas.SetTop(toPaste, y);
                    InkCanvas.SetLeft(toPaste, x);
                    InkCanvas.SetBottom(toPaste, y + toPaste.Height);
                    InkCanvas.SetRight(toPaste, x + toPaste.Width);
                    inkCanvas.Children.Add(toPaste);
                }
                else if (copyList[0].GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon toPaste = new Polygon();
                    Polygon old = (Polygon)copyList[0];
                    toPaste.Height = old.Height;
                    toPaste.Width = old.Width;
                    toPaste.Fill = old.Fill;
                    toPaste.Stroke = old.Stroke;
                    toPaste.StrokeThickness = old.StrokeThickness;
                    toPaste.Points = old.Points;
                    InkCanvas.SetTop(toPaste, y);
                    InkCanvas.SetLeft(toPaste, x);
                    InkCanvas.SetBottom(toPaste, y + toPaste.Height);
                    InkCanvas.SetRight(toPaste, x + toPaste.Width);
                    inkCanvas.Children.Add(toPaste);
                }
                else if (copyList[0].GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock toPaste = new TextBlock();
                    TextBlock old = (TextBlock)actionMenuShape;
                    toPaste.Text = old.Text;
                    toPaste.TextWrapping = old.TextWrapping;
                    toPaste.Measure(new Size(1000, 1000));
                    toPaste.FontSize = old.FontSize;
                    toPaste.FontFamily = old.FontFamily;
                    toPaste.Background = old.Background;
                    toPaste.Foreground = old.Foreground;
                    copyList.Add(toPaste);
                    InkCanvas.SetTop(toPaste, y);
                    InkCanvas.SetLeft(toPaste, x);
                    InkCanvas.SetBottom(toPaste, y + toPaste.DesiredSize.Height);
                    InkCanvas.SetRight(toPaste, x + toPaste.DesiredSize.Width);
                    inkCanvas.Children.Add(toPaste);
                }
                singleStylusDown = true;
                deleteLastStroke();
            }
        }

        /// <summary>
        /// copy the object that invoked the action menu
        /// </summary>
        /// <param name="print">Whether to flash the message or not</param>
        private void copy(int print)
        {
            if (!singleStylusDown)
            {
                copyList.Clear();
                if (actionMenuShape.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
                {
                    Rectangle toCopy = new Rectangle();
                    Rectangle oldShape = (Rectangle)actionMenuShape;
                    toCopy.Height = oldShape.Height;
                    toCopy.Width = oldShape.Width;
                    toCopy.Fill = oldShape.Fill;
                    toCopy.Stroke = oldShape.Stroke;
                    toCopy.StrokeThickness = oldShape.StrokeThickness;
                    copyList.Add(toCopy);
                    if (print == 1)
                        flashFeedBack("Rectangle copied");
                }
                else if (actionMenuShape.GetType().ToString().Equals("System.Windows.Shapes.Ellipse"))
                {
                    Ellipse toCopy = new Ellipse();
                    Ellipse oldShape = (Ellipse)actionMenuShape;
                    toCopy.Height = oldShape.Height;
                    toCopy.Width = oldShape.Width;
                    toCopy.Fill = oldShape.Fill;
                    toCopy.Stroke = oldShape.Stroke;
                    toCopy.StrokeThickness = oldShape.StrokeThickness;
                    copyList.Add(toCopy);
                    if (print == 1)
                        flashFeedBack("Ellipse copied");
                }
                else if (actionMenuShape.GetType().ToString().Equals("System.Windows.Shapes.Polygon"))
                {
                    Polygon toCopy = new Polygon();
                    Polygon oldShape = (Polygon)actionMenuShape;
                    toCopy.Height = oldShape.Height;
                    toCopy.Width = oldShape.Width;
                    toCopy.Fill = oldShape.Fill;
                    toCopy.Stroke = oldShape.Stroke;
                    toCopy.StrokeThickness = oldShape.StrokeThickness;
                    toCopy.Points = oldShape.Points;
                    copyList.Add(toCopy);
                    if (print == 1)
                        flashFeedBack("Triangle copied");
                }
                else if (actionMenuShape.GetType().ToString().Equals("System.Windows.Controls.TextBlock"))
                {
                    TextBlock toCopy = new TextBlock();
                    TextBlock oldShape = (TextBlock)actionMenuShape;
                    toCopy.Text = oldShape.Text;
                    toCopy.TextWrapping = oldShape.TextWrapping;
                    toCopy.Measure(new Size(1000, 1000));
                    toCopy.FontSize = oldShape.FontSize;
                    toCopy.FontFamily = oldShape.FontFamily;
                    toCopy.Background = oldShape.Background;
                    toCopy.Foreground = oldShape.Foreground;
                    copyList.Add(toCopy);
                    if (print == 1) 
                        flashFeedBack("Text copied");
                }
                singleStylusDown = true;
            }
        }

        private void cut()
        {
            copy(0);
            delete();
        }

        private void delete()
        {
            inkCanvas.Children.Remove(actionMenuShape);
        }

        private void select()
        {
            removeButton();
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;
        }

        #endregion

        #region ItemsControl

        private void createColor(double left, double top, SolidColorBrush color)
        {
            if (hasItemsList())
                deleteItemsControl();
            ItemsControl colors1 = new ItemsControl();
            ItemsControl colors2 = new ItemsControl();
            colors1.Items.Add(createColorBox(Brushes.Red));
            colors1.Items.Add(createColorBox(Brushes.Orange));
            colors1.Items.Add(createColorBox(Brushes.Yellow));
            colors1.Items.Add(createColorBox(Brushes.Green));
            colors1.Items.Add(createColorBox(Brushes.Cyan));
            colors2.Items.Add(createColorBox(Brushes.Blue));
            colors2.Items.Add(createColorBox(Brushes.Violet));
            colors2.Items.Add(createColorBox(Brushes.Brown));
            colors2.Items.Add(createColorBox(Brushes.Black));
            colors2.Items.Add(createColorBox(Brushes.White));
            colors1.Measure(new Size(1000, 1000));
            colors2.Measure(new Size(1000, 1000));
            colors1.Padding = new Thickness(LIST_PADDING);
            colors2.Padding = new Thickness(LIST_PADDING);
            colors1.Background = color;
            colors2.Background = color;
            InkCanvas.SetLeft(colors1, left);
            InkCanvas.SetLeft(colors2, left + colors1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(colors1, left + colors1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(colors2, left + 2 * (colors1.DesiredSize.Width) + 2 * LIST_PADDING);
            InkCanvas.SetTop(colors1, top - LIST_PADDING - colors1.DesiredSize.Height);
            InkCanvas.SetTop(colors2, top - LIST_PADDING - colors2.DesiredSize.Height);
            InkCanvas.SetBottom(colors1, top);
            InkCanvas.SetBottom(colors2, top);
            inkCanvas.Children.Add(colors1);
            inkCanvas.Children.Add(colors2);
        }

        private void createSize(double left, double top, SolidColorBrush color)
        {
            if (hasItemsList())
                deleteItemsControl();
            ItemsControl size1 = new ItemsControl();
            ItemsControl size2 = new ItemsControl();
            size1.Items.Add(createSizeBox(8, color));
            size1.Items.Add(createSizeBox(14, color));
            size1.Items.Add(createSizeBox(20, color));
            size1.Items.Add(createSizeBox(36, color));
            size1.Items.Add(createSizeBox(72, color));
            size1.Items.Add(createSizeBox(120, color));
            size1.Items.Add(createSizeBox(200, color));
            size2.Items.Add(createSizeBox(12, color));
            size2.Items.Add(createSizeBox(16, color));
            size2.Items.Add(createSizeBox(24, color));
            size2.Items.Add(createSizeBox(48, color));
            size2.Items.Add(createSizeBox(90, color));
            size2.Items.Add(createSizeBox(150, color));
            size2.Items.Add(createSizeBox(300, color));
            size1.Measure(new Size(1000, 1000));
            size2.Measure(new Size(1000, 1000));
            size1.Padding = new Thickness(LIST_PADDING);
            size2.Padding = new Thickness(LIST_PADDING);
            size1.Background = color;
            size2.Background = color;
            InkCanvas.SetLeft(size1, left);
            InkCanvas.SetLeft(size2, left + size1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(size1, left + size1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(size2, left + 2 * (size1.DesiredSize.Width) + 2 * LIST_PADDING);
            InkCanvas.SetTop(size1, top - LIST_PADDING - size1.DesiredSize.Height);
            InkCanvas.SetTop(size2, top - LIST_PADDING - size2.DesiredSize.Height);
            InkCanvas.SetBottom(size1, top);
            InkCanvas.SetBottom(size2, top);
            inkCanvas.Children.Add(size1);
            inkCanvas.Children.Add(size2);
        }


        private void createFont(double left, double top, SolidColorBrush color)
        {
            if (hasItemsList())
                deleteItemsControl();

            ItemsControl size1 = new ItemsControl();
            ItemsControl size2 = new ItemsControl();
            size1.Items.Add(createTextBox("Ari", color));
            size1.Items.Add(createTextBox("TNR", color));
            size1.Items.Add(createTextBox("Luc", color));
            size2.Items.Add(createTextBox("Com", color));
            size2.Items.Add(createTextBox("Ver", color));
            size2.Items.Add(createTextBox("Cou", color));
            size1.Measure(new Size(1000, 1000));
            size2.Measure(new Size(1000, 1000));
            size1.Padding = new Thickness(LIST_PADDING);
            size2.Padding = new Thickness(LIST_PADDING);
            size1.Background = color;
            size2.Background = color;
            InkCanvas.SetLeft(size1, left);
            InkCanvas.SetLeft(size2, left + size1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(size1, left + size1.DesiredSize.Width + LIST_PADDING);
            InkCanvas.SetRight(size2, left + 2 * (size1.DesiredSize.Width) + 2 * LIST_PADDING);
            InkCanvas.SetTop(size1, top - LIST_PADDING - size1.DesiredSize.Height);
            InkCanvas.SetTop(size2, top - LIST_PADDING - size2.DesiredSize.Height);
            InkCanvas.SetBottom(size1, top);
            InkCanvas.SetBottom(size2, top);
            inkCanvas.Children.Add(size1);
            inkCanvas.Children.Add(size2);
        }



        private bool hasItemsList()
        {
            UIElementCollection children = inkCanvas.Children;
            foreach (UIElement child in children)
            {
                if (child.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
                    return true;
            }
            return false;
        }


        private bool exists(SolidColorBrush color)
        {
            UIElementCollection children = inkCanvas.Children;
            foreach (UIElement child in children)
            {
                if (child.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
                    if (((ItemsControl)child).Background.Equals(color))
                        return true;  // don't create it again if it already exists
            }
            return false;
        }


        private void deleteItemsControl()
        {
            UIElementCollection shapes = inkCanvas.Children;
            UIElement child;
            for (int i = 0; i < inkCanvas.Children.Count; i++)
            {
                child = shapes[i];
                if (child.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
                {
                    inkCanvas.Children.RemoveAt(i + 1);
                    inkCanvas.Children.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// A helper method for adding small colorful rectangles to controls.
        /// Mainly used to indicate color options for a given UIElement or
        /// textblock
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Rectangle createColorBox(SolidColorBrush color)
        {
            Rectangle rec = new Rectangle();
            rec.Height = COLOR_SQUARE_SIDE;
            rec.Width = COLOR_SQUARE_SIDE;
            rec.Fill = color;
            return rec;
        }


        private object createSizeBox(int s, SolidColorBrush color)
        {
            System.Windows.Controls.Button size = new System.Windows.Controls.Button();
            size.Content = s;
            size.Background = color;
            size.Height = COLOR_SQUARE_SIDE*1.5;
            size.Width = COLOR_SQUARE_SIDE*2;
            size.FontSize = 8;
            return size;
        }

        private object createTextBox(string name, SolidColorBrush color)
        {
            System.Windows.Controls.Button font = new System.Windows.Controls.Button();
            font.Content = name;
            font.Background = color;
            font.Height = COLOR_SQUARE_SIDE*1.5;
            font.Width = COLOR_SQUARE_SIDE*2;
            font.FontSize = 8;
            return font;
        }


        #endregion


        #region Add to InkCanvas

        /// <summary>
        /// Add a textbox to the specified x, y coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void addText(double x, double y)
        {
            System.Windows.Controls.TextBlock text = new System.Windows.Controls.TextBlock();

            InkAnalyzer analyzer = new InkAnalyzer();
            if (inkCanvas.Strokes.Count >= 1)
                analyzer.AddStrokes(inkCanvas.Strokes);
            else
                return;
            AnalysisStatus status = analyzer.Analyze();
            if (status.Successful)
            {
                text.Text = analyzer.GetRecognizedString();
                text.TextWrapping = TextWrapping.Wrap;
                text.Measure(new Size(1000, 1000));
                text.FontSize = determineFontSize(text, inkCanvas.Strokes.GetBounds());

                Point start = (Point)inkCanvas.Strokes[0].StylusPoints[0];
                InkCanvas.SetLeft(text, start.X);
                InkCanvas.SetTop(text, start.Y);
                InkCanvas.SetRight(text, start.X + text.DesiredSize.Width);
                InkCanvas.SetBottom(text, start.Y + text.DesiredSize.Height);
                inkCanvas.Children.Add(text);

                currentSelection.Clear();
                updateSelection();
                for (int i = 0; i < strokeBoxes.Count; i++)
                {
                    inkCanvas.Children.Remove(strokeBoxes[i]);
                }
                lockedStrokes.Clear();
                strokeBoxes.Clear();
                inkCanvas.Strokes.Clear();
            }
        }


        private double determineFontSize(TextBlock text, Rect rect)
        {
            for (int i = 10; i < 200; ++i)
            {
                text.FontSize = i;
                text.Measure(new Size(1000, 1000));
                if (text.DesiredSize.Height > rect.Height || text.DesiredSize.Width > rect.Width)
                    return i;
            }

            return 200;
        }

        /// <summary>
        /// Add a rectangle of the given size at the specified location
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        private void addRectangle(double xPos, double yPos, double height, double width)
        {
            Rectangle rec = new Rectangle();
            rec.Height = height;
            rec.Width = width;
            rec.Fill = Brushes.Crimson;
            rec.Stroke = Brushes.Black;
            rec.StrokeThickness = REC_BORDER;
            InkCanvas.SetTop(rec, yPos);
            InkCanvas.SetLeft(rec, xPos);
            InkCanvas.SetBottom(rec, yPos + height);
            InkCanvas.SetRight(rec, xPos + width);
            inkCanvas.Children.Add(rec);
        }


        private void addCircle(double xPos, double yPos, double height, double width)
        {
            Ellipse circle = new Ellipse();
            circle.Height = height;
            circle.Width = width;
            circle.Fill = Brushes.Yellow;
            circle.Stroke = Brushes.Black;
            circle.StrokeThickness = REC_BORDER;
            InkCanvas.SetTop(circle, yPos);
            InkCanvas.SetLeft(circle, xPos);
            InkCanvas.SetBottom(circle, yPos + height);
            InkCanvas.SetRight(circle, xPos + width);
            inkCanvas.Children.Add(circle);
        }


        private void addTriangle(double xPos, double yPos, double height, double width)
        {
            Polygon triangle = new Polygon();
            PointCollection vertex = new PointCollection();
            vertex.Add(new Point(0.5 * width, 0));
            vertex.Add(new Point(0, height));
            vertex.Add(new Point(width, height));
            triangle.Points = vertex;
            triangle.Fill = Brushes.Green;
            triangle.Stroke = Brushes.Black;
            triangle.StrokeThickness = REC_BORDER;
            InkCanvas.SetTop(triangle, yPos);
            InkCanvas.SetLeft(triangle, xPos);
            InkCanvas.SetBottom(triangle, yPos + height);
            InkCanvas.SetRight(triangle, xPos + width);
            inkCanvas.Children.Add(triangle);
        }


        #endregion


        #region Do Stuff At Location

        /// <summary>
        /// select the shape located at the given x, y position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void selectShapeAt(double x, double y)
        {
            UIElementCollection shapes = inkCanvas.Children;
            UIElement shape = findShape(x, y);
            if (shape != null)
                inkCanvas.Select(new UIElement[] { shape });
            inkCanvas.EditingMode = InkCanvasEditingMode.InkAndGesture;
        }

        /// <summary>
        /// Delete the shape at the given x, y position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void deleteShapeAt(double x, double y)
        {
            UIElement shape = findShape(x, y);
            if (shape != null)
            {
                //System.Windows.MessageBox.Show(inkCanvas.Children.Contains(shape).ToString());
                inkCanvas.Children.Remove(shape);
                //System.Windows.MessageBox.Show(inkCanvas.Children.Contains(shape).ToString() +
                //    "   " + inkCanvas.Children.Count.ToString());
            }
        }

        /// <summary>
        /// Find the shape at the given x, y location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private UIElement findShape(double x, double y)
        {
            UIElementCollection shapes = inkCanvas.Children;
            for (int i = shapes.Count - 1; i >= 0; i--)
            {
                if (shapeInRange(shapes[i], x, y))
                {
                    
                    return shapes[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Checks to see if the current shape is within the range of
        /// the Stylus position
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool shapeInRange(UIElement shape, double x, double y)
        {
            double left = InkCanvas.GetLeft(shape);
            double top = InkCanvas.GetTop(shape);
            double right = InkCanvas.GetRight(shape);
            double bottom = InkCanvas.GetBottom(shape);
            return (left < x && right > x && top < y && bottom > y);
        }



        /// <summary>
        /// Given a location x and y, find the button that is located at that
        /// point, if any.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void findButtonAt(double x, double y)
        {
            UIElement shape = findShape(x, y);
            if (shape == null)
                return;
            if (shape.GetType().ToString().Equals("System.Windows.Controls.Button"))
            {
                System.Windows.Controls.Button b = (System.Windows.Controls.Button)shape;
                String label = b.Content.ToString();
                displayButtonResult(label, InkCanvas.GetLeft(shape), InkCanvas.GetTop(shape));
            }
        }


        private void deleteObject(double x, double y)
        {
            UIElement toDelete = findShape(x, y);
            inkCanvas.Children.Remove(toDelete);
        }


        #endregion


        #region Feedback

        private void flashFeedBack(String message)
        {
            if (singleStylusDown == false)
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
                singleStylusDown = true;
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UIElementCollection children = inkCanvas.Children;
            foreach (UIElement child in children)
            {
                if (child.GetType().ToString().Equals("System.Windows.Controls.Label"))
                {
                    inkCanvas.Children.Remove(child);
                    return;
                }
            }
            Timer timer = (Timer)sender;
            timer.Stop();
            timer.Dispose();
        }

        #endregion


    }
}