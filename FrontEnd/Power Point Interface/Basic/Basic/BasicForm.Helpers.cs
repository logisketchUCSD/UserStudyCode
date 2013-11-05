/// a collection of helper functions
/// within the BasicForm class.

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
    /// </summary>
    public partial class BasicForm : Form
    {
        /// <summary>
        /// Switches the Overlay between visible and mouse-event-enabled (wheat) and invisible (LightCyan)
        /// </summary>
        /// <returns>true if the overlay was toggled on, false if it was toggled off.</returns>
        internal bool toggleOverlay()
        {
            if (TransparencyKey.Equals(Color.LightCyan))
            {
                TransparencyKey = Color.Wheat; // overlay on
                this.Activate();
                return true;
            }
            else
            {
                TransparencyKey = Color.LightCyan;
                basicOverlay.Ink.DeleteStrokes();
                Panel.Invalidate();
                pptController.pptApp.Activate();
                return false;
            }

        }


        #region Support Functions
        /// <summary>
        /// toggleMode changes the overlay mode to the one specified by toMode.
        /// </summary>
        /// <param name="toMode">One of GestureMode, InkMode, or TextMode</param>
        private void toggleMode(Mode toMode)
        {
            /*
            if (currentMode == Mode.GestureMode)
            {
                //this is to fool the compiler into making the warning go away. bwah.
            }
            */
            // change which gestures are available based on the mode being switched to
            switch (toMode)
            {

                case Mode.TextMode:
                    currentMode = Mode.TextMode;
                    setFeedback("Entering text mode -- most gestures unavailable");
                    basicOverlay.SetGestureStatus(ApplicationGesture.AllGestures, false);
                    basicOverlay.SetGestureStatus(ApplicationGesture.UpRightLong, true); // turn on gesture mode
                    basicOverlay.SetGestureStatus(ApplicationGesture.Check, true); // recognize text
                    basicOverlay.SetGestureStatus(ApplicationGesture.Scratchout, true); // erase strokes
                    basicOverlay.SetGestureStatus(ApplicationGesture.SemiCircleRight, true); // delete object
                    basicOverlay.SetGestureStatus(ApplicationGesture.Curlicue, true); // delete last object
                    basicOverlay.SetGestureStatus(ApplicationGesture.LeftDown, true); // delete last stroke
                    basicOverlay.SetGestureStatus(ApplicationGesture.SemiCircleLeft, true); // insert text
                    basicOverlay.SetGestureStatus(ApplicationGesture.UpLeft, true); // delete text
                    basicOverlay.SetGestureStatus(ApplicationGesture.UpLeftLong, true); // delete word
                    buttonForm.showLabel("Text Mode");
                    break;
                case Mode.GestureMode:
                    currentMode = Mode.GestureMode;
                    setFeedback("Gestures on");
                    buttonForm.showLabel("Gestures On");
                    basicOverlay.SetGestureStatus(ApplicationGesture.AllGestures, true);
                    break;
                case Mode.InkMode:
                    break;

            }// switch (toMode)


        }// toggleMode

        /// <summary>
        /// ClearGestures clear all the gestures from the form.
        /// </summary>
        /// <param name="basicoverlay">the inkoverlay that we are interested in</param>
        private void ClearGestures(InkOverlay basicoverlay)
        {
            ApplicationGesture nogesture = ApplicationGesture.NoGesture;
            System.Array gestureIDs = System.Enum.GetValues(nogesture.GetType());
            foreach (ApplicationGesture gestureID in gestureIDs)
            {
                basicoverlay.SetGestureStatus(gestureID, false);
            }
        }


        /// <summary>
        /// setFeedBack sets text to the feedback box so the user knows what their gestures did
        /// </summary>
        /// <param name="s">string containing the message to add to the feedback box</param>
        internal void setFeedback(string s)
        {
            buttonForm.FeedbackBox.Text = s;
        }


        /// <summary>
        /// cutAll "cuts" all the selected shapes on the current slide and store those
        /// shapes in a global ShapeRange object to be accessed later.
        /// </summary>
        /// <param name="selection">the selection of shapes on the current slide</param>
        internal void cutAll(PowerPoint.Selection selection)
        {
            cutAllShapes = new List<ShapeAttributes>();
            selection = pptController.pptApp.ActiveWindow.Selection;
            cutPasteShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
            foreach (PowerPoint.Shape s in cutPasteShapes)
            {
                ShapeAttributes clone = new ShapeAttributes(s);
                cutAllShapes.Add(clone);
                s.Delete();
            }
            selection.Unselect();
            undoStack.Push("");
            undoStack.Push("");
            undoStack.Push(cutAllShapes);
            undoStack.Push("CutAll");
        }

        /// <summary>
        /// pasteAll pastes all the selected shapes onto the current slide
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        internal void pasteAll(int x, int y)
        {
            List<ShapeAttributes> shapeToPaste = new List<ShapeAttributes>();
            float initx = cutAllShapes[0].getLeft(); // keeps track of the x orientation of the shapes
            float inity = cutAllShapes[0].getTop(); // keeps track of the y orientation of the shapes
            foreach (ShapeAttributes sa in cutAllShapes)
            {
                pptController.paste(sa.getType(), (int)(x + sa.getLeft() - initx), (int)(y + sa.getTop() - inity),
                    sa.getWidth(), sa.getHeight(), sa.getColor(), sa.getRotation());
                shapeToPaste.Add(sa);
            }
            undoStack.Push("");
            undoStack.Push("");
            undoStack.Push(shapeToPaste);
            undoStack.Push("PasteAll");
        }

        /// <summary>
        /// moves all the selected shapes according to the mousemove. It is constantly called
        /// as long as both mousemove event and rightDrag event are triggered
        /// </summary>
        /// <param name="x">current x position</param>
        /// <param name="y">current y position</param>
        internal void move(int x, int y)
        {
            try
            {
                PowerPoint.Selection selection;
                if (fillCurrentSelection(out selection))
                {
                    lassoAllowed = false;
                    PowerPoint.ShapeRange totalShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                    if (locked == false)
                    {
                        prevXvalue = x;
                        prevYvalue = y;
                        stackMove(totalShapes);
                        locked = true;
                    }
                    totalShapes.IncrementLeft(x - prevXvalue);
                    totalShapes.IncrementTop(y - prevYvalue);
                    gestureOK = false;
                    prevXvalue = x;
                    prevYvalue = y;
                }
            }
            catch (Exception a)
            {
                setFeedback(a.ToString());
            }
        }


        /// <summary>
        /// tryRotate is called when the drag event and the mousemove event happen together.
        /// It rotates all the shapes selected according to the mouse movement
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        internal void tryRotate(int x, int y)
        {

            try
            {
                PowerPoint.Selection selection;
                if (!fillCurrentSelection(out selection))
                {
                    return;
                }
                PowerPoint.ShapeRange totalShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                float angle = 2F; // 2;
                if (locked == false)
                {
                    double innerX, innerY;
                    foreach (PowerPoint.Shape rotateShape in totalShapes)
                    {
                        innerX = rotateShape.Left + (0.4 * rotateShape.Width);
                        innerY = rotateShape.Top + (0.4 * rotateShape.Height);
                        Rectangle innerRec = new Rectangle((int)innerX, (int)innerY,
                            (int)(0.2 * rotateShape.Width), (int)(0.2 * rotateShape.Height));
                        if (innerRec.Contains(x, y))
                        {
                            rotate = true;
                            locked = true;
                            gestureOK = false;
                            prevY = y;
                            prevX = x;
                            break;
                        }
                    }
                }
                if (rotate)
                {
                    foreach (PowerPoint.Shape rotateShape in totalShapes)
                    {
                        if (y > prevY)
                            rotateShape.IncrementRotation(angle);
                        else
                            rotateShape.IncrementRotation(0 - angle);
                    }
                    resizeAllowed = false;
                    prevY = y;
                }

            }
            catch (Exception e)
            {
                // Code accounts for no selection, so this means something bad happened
                debugExceptionMessage("tryRotate", e);
            }
        }
       
        /*
        /// <summary>
        /// tryRotate is called when the drag event and the mousemove event happen together.
        /// It rotates all the shapes selected according to the mouse movement.
        /// targetShape is the shape whose rotate button we were / are over
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        internal void tryRotate(int x, int y, PowerPoint.Shape targetShape)
        {

            try
            {
                PowerPoint.Selection selection;
                if (!fillCurrentSelection(out selection))
                {
                    return;
                }
                PowerPoint.ShapeRange totalShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                float angle = 2F; // 2;
                if (locked == false)
                {
                    // previously, we went throught the shapes AGAIN to see if we were actually on top of a shape -- but that is already
                    // handled in mouseMove which calls this

                   // double innerX, innerY; // OLD CODE
                   // foreach (PowerPoint.Shape rotateShape in totalShapes)
                   // {
                      //  fPoint mouseloc = new fPoint(x, y);
                        // if the mouse is over that shape's "rotate button" (the green bubble) (or whatever alternative)
                      //  if(onRotateMarker(rotateShape, mouseloc))
                        //{
                             old code checked if the mouse was in the middle of the shape
                        innerX = rotateShape.Left + (0.4 * rotateShape.Width);
                        innerY = rotateShape.Top + (0.4 * rotateShape.Height);
                        Rectangle innerRec = new Rectangle((int)innerX, (int)innerY,
                            (int)(0.2 * rotateShape.Width), (int)(0.2 * rotateShape.Height));
                        if (innerRec.Contains(x, y))
                        {

                            rotate = true;
                            locked = true;
                            gestureOK = false;
                            prevY = y;
                            prevX = x;
                           // break;
                      //  } // if
                    //} //foreach
                }
                if (rotate)
                {
                    foreach (PowerPoint.Shape rotateShape in totalShapes)
                    {
                        if (y > prevY)
                            rotateShape.IncrementRotation(angle);
                        else
                            rotateShape.IncrementRotation(0 - angle);
                    }
                    resizeAllowed = false;
                    prevY = y;
                }

            }
            catch (Exception e)
            {
                // Code accounts for no selection, so this means something bad happened
                debugExceptionMessage("tryRotate", e);
            }
        }
         */


        /// <summary>
        /// debugExceptionMessage is for showing messages easily for exceptions, and also
        /// for easy  hiding of them when not in debug mode.
        /// </summary>
        /// <param name="methodName">a string containing the name of the method throwing the exception</param>
        /// <param name="e">the exception</param>
        void debugExceptionMessage(String methodName, Exception e)
        {
            //setFeedback("BasicPad's " + methodName + " method threw an exception: " + e.ToString());
            MessageBox.Show("BasicPad's " + methodName + " method threw an exception: " + e.ToString());
        }

        /// <summary>
        /// rightClick is only called when a right click is recognized AND when there is a current
        /// selection on the slide.
        /// </summary>
        /// <param name="shapes">a collection of shapes</param>
        internal void rightClick(PowerPoint.ShapeRange shapes)
        {
            PowerPoint.Shape clickedShape = null;
            try
            {
                clickedShape = pptController.findShape(X, Y);
            }
            catch (Exception e)
            {
                debugExceptionMessage("right click", e);
                return;
            }

            // If the click wasn't on any shape, just return
            if (clickedShape == null)
            {
                PowerPoint.Selection selection = pptController.pptApp.ActiveWindow.Selection;
                selection.Unselect();
                return;
            }
            //  Then, if it falls on an unselected shape, select that and return
            //if(shapes.
            bool inRange = false; // first assume the cursor is NOT in range of any of the shapes
            try
            {
                foreach (PowerPoint.Shape shape in shapes)
                {
                    // if the cursor falls within the range of ANY of the shape selected
                    if (clickedShape.Equals(shape)) { inRange = true; }

                }
                if (inRange == false) // if the cursor does not fall within the range of any shapes currently selected
                {
                    // then try to see if it falls on top of a shape that is not currently selected,
                    // if so, select that one
                    PowerPoint.Shape shape = pptController.findShape(X, Y);
                    shape.Select(Microsoft.Office.Core.MsoTriState.msoTrue);
                }
            }
            catch (Exception e)
            {
                // this exception should only be thrown when the right click does NOT fall on top of any
                // shapes, so the select method complains. In that case, this does not need to do anything
                // However, it's now checked for above so we should know when it happens
                debugExceptionMessage("rightclick", e);
            }
        }



        /// <summary>
        /// when shapes are stacked in layers, it's difficult to access the shapes hidden under the
        /// first item. SelectNext enables the user to access those objects with a single tap - it
        /// cycles through all the shapes that fall under this mouseDown.
        /// TODO FIXME: right now, if other gestures -- for example, moving or deleting shapes -- happen between
        /// calls to this method, we may try to access shapes that are no longer under the cursor or perhaps
        /// now nonexistant.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>the Shape to be selected</returns>
        internal PowerPoint.Shape selectNext(fPoint p, bool doSelect)
        {
            // if we tapped on the same point the last time we tapped (with small margin), just go through the list and select the next thing
            //TODO FIXME - don't hardcode the margin in
            if (p.isNear(lastSelectedPoint, 2))
            {
                // if it's empty, clear selection
                if (shapesAtCurrentPoint.Count == 0)
                {
                    if (doSelect) { selectShape(null, true); }
                    return null;
                }
                //then, if we've decremented past the end of layeredShapeCounter, loop around
                if (layeredShapeCounter < 0)
                {
                    layeredShapeCounter = shapesAtCurrentPoint.Count - 1;
                }
                // then, select the shape at the counter and decrement for next time
                if (doSelect) { selectShape(shapesAtCurrentPoint[layeredShapeCounter], false); }
                layeredShapeCounter--;
                return shapesAtCurrentPoint[layeredShapeCounter];
            }
            // remember what point we just tapped at
            lastSelectedPoint = p;
            // Reset the list of shapes at the tapped point
            shapesAtCurrentPoint.Clear();
            // get all the shapes
            List<PowerPoint.Shape> totalShapes = pptController.allShapes();
            // List<PowerPoint.Shape> choices = new List<PowerPoint.Shape>(); // to store possible candidates

            // go through each shape and add it to the list if it's there
            foreach (PowerPoint.Shape currentShape in totalShapes)
            {
                if (pptController.pointOnShape(p, currentShape))
                {
                    shapesAtCurrentPoint.Add(currentShape);
                }
            }
            layeredShapeCounter = shapesAtCurrentPoint.Count - 1;

            // # of Shapes: 0 = clear selection
            //            : 1 = add it to selection
            //            : >1= add (last added) to selection and move counter to previous one
            PowerPoint.Shape shape;
            if (layeredShapeCounter == -1)
            {
                shape = null;
                if (doSelect) { selectShape(null, true); }
                setFeedback("No object selected - please tap on top of a valid object");
            }
            else
            {
                shape = shapesAtCurrentPoint[layeredShapeCounter];
                if (doSelect) { selectShape(shape, false); }
                layeredShapeCounter--; //and decrement the counter
                // DEBUG FEEDBACK FIXME
                // setFeedback("Object selected; w=" + shape.Width.ToString() + " h=" + shape.Height.ToString() + " r=" + shape.Rotation.ToString() +
                // " hf=" + shape.HorizontalFlip.ToString() + " vf=" + shape.VerticalFlip.ToString() + layeredShapeCounter);
            }

            // call up an button to display recognition alternatives of that particular textbox
            // set up such that regardless of input doSelect, a textbox will be selected if clicked
            if (shape.Type == MsoShapeType.msoTextBox)
            {
                displayButton(shape);
            }

            return shape;
        }


        /// <summary>
        /// Selects the passed shape. If clearSelection is true, clears the current selection first, else adds the shape to current selection.
        /// 
        /// </summary>
        /// <param name="shape">Shape to select; if null, will just not select it.</param>
        /// <param name="clearSelection">true = clear the current selection first; false = leave it be</param>
        internal void selectShape(PowerPoint.Shape shape, bool clearSelection)
        {
            if (clearSelection)
            {
                PowerPoint.Selection selection = pptController.pptApp.ActiveWindow.Selection;
                selection.Unselect();
            }
            if (shape != null)
            {
                shape.Select(Microsoft.Office.Core.MsoTriState.msoTrue);
            }
        }//selectShape



        /// <summary>
        /// this acts as lasso select, and select the shapes that falls within the bounding box of the
        /// gesture.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        internal void lasso()
        {
            int numStroke = basicOverlay.Ink.Strokes.Count; // get the gesture that invokes this method
            Rectangle area = basicOverlay.Ink.Strokes[numStroke - 1].GetBoundingBox();

            List<PowerPoint.Shape> lassoShape = new List<PowerPoint.Shape>();
            List<PowerPoint.Shape> total = pptController.allShapes();


            // convert the coordinates of the  bounding box of the lasso into PowerPoint coordinates
            // TODO FIXME: Generalize from 1.2 into whatever factor it actually needs to be
            int left = (int)(1.2 * xConvert((int)(area.Left)));
            int top = (int)(1.2 * yConvert((int)(area.Top)));
            int width = (int)(1.2 * xConvert((int)area.Width)); // increase the default width a bit; default too small
            int height = (int)(1.2 * yConvert((int)area.Height)); // ''
            Rectangle areaConverted = new Rectangle(left, top, width, height);

            // check each shape to see if all four corners fall within areaConverted
            foreach (PowerPoint.Shape s in total)
            {
                if (areaConverted.Contains((int)s.Left, (int)s.Top)
                    && (areaConverted.Contains((int)s.Left, (int)(s.Top + s.Height)))
                    && (areaConverted.Contains((int)(s.Left + s.Width), (int)s.Top))
                    && (areaConverted.Contains((int)(s.Left + s.Width), (int)(s.Top + s.Height))))
                {
                    lassoShape.Add(s); // then that shape is a candidate for lasso
                }
            }
            // select the shapes
            foreach (PowerPoint.Shape s in lassoShape)
            {
                s.Select(MsoTriState.msoFalse); // somehow, msoFalse selects the shapes...
            }
            setFeedback("Lassoed shapes selected");
        } // lasso()


        internal void tryLasso(int x, int y)
        {
            try
            {
                PowerPoint.Shape shapeInCursorRange = pptController.findShape(x, y);
                if (shapeInCursorRange == null)
                {
                    lasso();
                }
                lassoAllowed = false;
            }
            catch (Exception e)
            {
                debugExceptionMessage("tryLasso", e);
            }
        }



        /// <summary>
        /// fillCurrentSelection sets the passed selection as the current selection in powerpoint.
        /// it returns true if this selection is nonempty.
        /// </summary>
        /// <param name="selection"> the variable to be filled with the current selection</param>
        /// <returns>Returns true if the selection is nonempty, false otherwise</returns>
        private bool fillCurrentSelection(out PowerPoint.Selection selection)
        {
            selection = null;
            //try
            // {
            // TODO FIXME:
            // if there is no active window (this happens when the presentation is running)
            // this will throw an exception. Need to figure out how to handle this!
            selection = pptController.pptApp.ActiveWindow.Selection;
            if (selection.Type == PowerPoint.PpSelectionType.ppSelectionNone)
            {
                return false;
            }
            return true;
            //}
            //catch (Exception e)
            //{
            //     debugExceptionMessage("fillCurrentSelection", e);
            //}
            // return false;
        }//fillSelection()



        #endregion

        /// <summary>
        /// Returns true if the point falls on the rotate-marker of the passed shape; otherwise false.
        /// WARNING: Makes excessive use of hardcoded numbers.
        /// </summary>
        /// <param name="potentialShapes"></param>
        /// <returns></returns>
        private bool onRotateMarker(PowerPoint.Shape testShape, fPoint cursor)
        {
            float hDisp = 0; // the horizontal displacement from the center of the shape
            float vDisp = 15; // the height above the top of the bounding box that the center of the marker is.
            float margin = 8; // the margin (as radius, but like a square) around the center of the marker that we will accept hits on.

            // the coords (unrotated) of the marker's center.
            fPoint markCenter = new fPoint(testShape.Left + (testShape.Width / 2) + hDisp, testShape.Top - vDisp);

            // get the coords of the point
            fPoint rotCursor = rotatePointWithShape(cursor, testShape);

            //FIXME TEST DEBUG -- adds a shape to the (unrotated) location of where it thinks the marker is
            //pptController.addShapeToCurrSlide(MsoAutoShapeType.msoShapeRectangle, markCenter.x - margin, markCenter.y - margin, margin * 2, margin * 2);


            // see if they're close
            if ((Math.Abs(rotCursor.x - markCenter.x) <= margin) && (Math.Abs(rotCursor.y - markCenter.y) <= margin))
            {
                return true;
            }
            
            return false;
        }



    }
}
