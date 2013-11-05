using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Ink;
using Microsoft.Office.Core;
using System.Reflection;

/* 
 * This file continues the BasicForm class.
 * it contains all the methods for recognized gestures.
 * 
 */
namespace Basic
{

    partial class BasicForm
    {

        #region Recognize

        /// <summary>
        /// Checks to see if there is any stroke on basicOverlay to be recognized. If so,
        /// calls helper function to recognize the text. If not, give feedback and return
        /// </summary>
        internal void recognizeText(bool sizeDependent)
        {
            try
            {
                if (basicOverlay.Ink.Strokes.Count > 1) // the one stroke is from the gesture, doesn't count
                {
                    strokeRec(sizeDependent);
                    setFeedback("Your text has been recognized");
                }
                else
                {
                    setFeedback("There is nothing to be recognized!");
                }
            }
            catch (Exception e)
            {
                debugExceptionMessage("recognizeText", e);
            }
        }// recognizeText
        #endregion


        #region delete

        /// <summary>
        /// Deletes the last object added to the slide
        /// </summary>
        /// <param name="allShapes">A list of all shapes on the slide</param>
        private void deleteLastObject(List<PowerPoint.Shape> allShapes)
        {
            if (allShapes.Count == 0)
            {
                setFeedback("There is no shape to be deleted");
            }
            else
            {
                int rangeLength = allShapes.Count;
                PowerPoint.Shape shape = allShapes[rangeLength - 1]; // find the last added shape to the slide
                if (shape.Type == MsoShapeType.msoTextBox)
                {
                    // for each textbox, get rid of the button / listbox associated with it
                    ListBox listboxToDelete = findListboxFromTextbox(shape.Name);
                    listboxToDelete.Hide();
                    stackDeleteTextbox(shape, listboxToDelete); // push both shape and the listbox onto the stack
                    this.Controls.Remove(listboxToDelete);
                    alternateList.Remove(listboxToDelete);
                    clearButtons();
                }
                else
                    stackDeleteShape(shape);
                shape.Delete(); // if found, then this gesture deletes that shape
                setFeedback("Last added object deleted");
            }
        }//deleteLastObject()


        /// <summary>
        /// delete the object chose
        /// </summary>
        /// <param name="shape">the shape to be deleted</param>
        private void deleteChosenObject(PowerPoint.Shape shape)
        {
            if (shape == null)
            {
                setFeedback("No shape deleted - gesture must START on the shape to be deleted");
                return;
            }
            if (shape.Type == MsoShapeType.msoTextBox)
            {
                // for each textbox, get rid of the button / listbox associated with it
                ListBox listboxToDelete = findListboxFromTextbox(shape.Name);
                listboxToDelete.Hide();
                stackDeleteTextbox(shape, listboxToDelete); // push both shape and the listbox onto the stack
                this.Controls.Remove(listboxToDelete);
                alternateList.Remove(listboxToDelete);
                clearButtons();
            }
            else
                stackDeleteShape(shape);
            shape.Delete();
            setFeedback("Shape deleted");
        }



        /// <summary>
        /// Delete all the objects on the current slide
        /// </summary>
        /// <param name="allShapes">A list of all shapes on the slide</param>
        private void deleteAllObjects(List<PowerPoint.Shape> allShapes)
        {

            if (allShapes == null) // if there is no shape, then break out of here
            {
                setFeedback("There is no shape to be deleted");
            }
            else
            {
                // to prevent accidently deleting ALL shapes, ask for a confirmation first
                DialogResult dr = System.Windows.Forms.MessageBox.Show(
                        "Erase all objects on the slide?",
                        "PowerPoint",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Exclamation);
                if (dr == DialogResult.OK) // only go ahead and delete if OK button is pressed
                {
                    stackDeleteShapes(allShapes);
                    foreach (PowerPoint.Shape currentShape in allShapes)
                        currentShape.Delete();
                    setFeedback("All object on current slide deleted");
                }
            }
        }


        /// <summary>
        /// Deletes the last stroke added to the overlay
        /// </summary>
        /// <param name="undoable">whether to add the deleted stroke
        /// to the undo stack. True = add, false = do not add.</param>
        internal void deleteLastStroke(bool undoable)
        {

            int count = basicOverlay.Ink.Strokes.Count;
            if (count == 1) // that one count IS the leftdown stroke
            {
                setFeedback("No stroke to be deleted");

            }
            else
            {
                setFeedback("Last stroke deleted");
                Ink inkClone = basicOverlay.Ink.Clone(); // use for undo stroke
                basicOverlay.Ink.DeleteStroke(basicOverlay.Ink.Strokes[count - 2]); // delete the stroke before leftdown
                Panel.Invalidate(); // invalidate the window to repaint it

                // if we want this stroke to be undone, add its information to the undo stack
                if (undoable)
                {
                    Strokes last = inkClone.Strokes;
                    int numStroke = last.Count;
                    for (int i = 0; i < numStroke - 2; i++)
                    {
                        last.RemoveAt(0); // remove everything except the last stroke, which is what we want
                    }

                    last.RemoveAt(1); // remove the gesture that triggered this
                    undoStack.Push("");
                    undoStack.Push("");
                    undoStack.Push(last); // should only contain the last stroke now
                    undoStack.Push("DeleteLastStroke");
                }
            }

        }//deleteLastStroke


        /// <summary>
        /// delete all the strokes on the basicOverlay
        /// </summary>
        internal void deleteAllStrokes()
        {
            if (basicOverlay.Ink.Strokes.Count == 1)
            {
                setFeedback("No ink to erase");

            }
            else
            {
                // ask for confirmation -- should this be optional in gestureMode?

                DialogResult dr1 = System.Windows.Forms.MessageBox.Show(
                    "Erase all ink on the overlay?",
                    "PowerPoint",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation);
                if (dr1.Equals(DialogResult.OK)) // only proceed if OK button is pressed
                {
                    // Get some information for undo first before deleting the strokes
                    Ink cloneInk = basicOverlay.Ink.Clone(); // deep copy
                    Strokes cloneStroke = cloneInk.Strokes; // a copy of strokes that will not be deleted
                    undoStack.Push("");
                    undoStack.Push("");
                    undoStack.Push(cloneStroke);
                    undoStack.Push("DeleteAllStrokes");

                    basicOverlay.Ink.DeleteStrokes();
                    Panel.Invalidate(); // repaints the window
                }
                setFeedback("Ink erased from overlay");
            }
        }//deleteAllStrokes()

        #endregion


        #region Copy/Cut/Paste
        // note that these functions are SEPARATE from normal windows clipboard
        // functions -- invoking them will not change the clipboard.

        /// <summary>
        /// Copy all the selected objects
        /// </summary>
        private void copySelection()
        {
            try
            {
                PowerPoint.Selection selection;
                setFeedback("test");
                if (fillCurrentSelection(out selection))
                {
                    cutAllShapes = new List<ShapeAttributes>();
                    // save all currently selected shapes into cutPasteShapes object
                    cutPasteShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                    foreach (PowerPoint.Shape s in cutPasteShapes)
                    {
                        ShapeAttributes sa = new ShapeAttributes(s);
                        cutAllShapes.Add(sa);
                    }
                    setFeedback("All selected objects copied");
                }
                else
                {
                    setFeedback("Selection empty - nothing was copied");
                }
            }
            catch (Exception ex)
            {
                debugExceptionMessage("copySelection", ex);
            }
        }//copySelection()


        /// <summary>
        /// cutSelection cuts the current selection.
        /// </summary>
        private void cutSelection()
        {

            try
            {
                setFeedback("All selected objects cut");
                PowerPoint.Selection selection;
                if (fillCurrentSelection(out selection))
                {
                    cutAll(selection);
                }
                else
                {
                    setFeedback("Selection empty - nothing was cut");
                }
            }
            catch (Exception ex)
            {
                debugExceptionMessage("cutSelection", ex);
            }

        }//cutSelection()

        
        /// <summary>
        /// Paste all the shapes on clipboard; that is, the last set of shapes
        /// copied or cut.
        /// </summary>
        internal void paste()
        {
            if (cutPasteShapes.Count == 0)
            {
                setFeedback("There is no shape to be pasted");
            }
            else
            {
                setFeedback("All cut / copied objects pasted");
                pasteAll(X, Y);
            }
        }//end paste()

        #endregion


        #region Misc

        /// <summary>
        /// add the object on which the gesture started to the current selection
        /// </summary>
        /// <param name="allShapes">a list of all shapes on the current slide</param>
        private void multiSelect(List<PowerPoint.Shape> allShapes)
        {
            PowerPoint.Shape shape = pptController.findShape(X, Y);
            if (shape.Equals(null))
            {
                setFeedback("No shape selected - nothing added to selection");
                return;
            }
            PowerPoint.Selection selection;
            try
            {
                // if the selection is empty, add the shape anyways
                if (!fillCurrentSelection(out selection))
                {
                    shape.Select(MsoTriState.msoTrue);
                    setFeedback("Added object to previously empty selection");
                }
                else
                {
                    //else, get the selected shapes. I don't know what's the deal with ChildShapeRange.
                    selection = pptController.pptApp.ActiveWindow.Selection;

                    PowerPoint.ShapeRange shapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                    //allShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;

                    // TODO: FIX: why do we need to reselect everything? Figure out why and replace if possible
                    foreach (PowerPoint.Shape s in shapes)
                    {
                        s.Select(MsoTriState.msoFalse); // why does FALSE actually select the object?!?
                    }

                    shape.Select(MsoTriState.msoFalse);

                    setFeedback("New object added to selection");
                }
            }
            catch (Exception ex)
            {
                debugExceptionMessage("case right, for removing exception", ex);
                shape.Select(MsoTriState.msoFalse);
                setFeedback("Added object to previously empty selection");
                //   setFeedback("No currently selection, please select at least one object first");
            }
        }//multiSelect()

        /// <summary>
        /// add a line to the current slide
        /// </summary>
        private void addLine()
        {
            if (secondPoint == false) // if this is the first endpoint of the line
            {
                firstX = X;
                firstY = Y;
                secondPoint = true;
                setFeedback("Line started -- draw another \"RightDown\" at endpoint");
            }
            else
            {
                int secondX = X;
                int secondY = Y;
                secondPoint = false;
                PowerPoint.Shape line = pptController.addLineToCurrSlide((float)firstX, (float)firstY, (float)secondX, (float)secondY);
                stackAddShape(line);
                setFeedback("Line added");
            }
        }//addLine()


        // sets the rotation of all selected shapes to 0 (unrotated).
        internal void rotateReset()
        {
            PowerPoint.Selection selection;
            PowerPoint.ShapeRange selectedShapes = null;
            if (fillCurrentSelection(out selection))
            {
                selectedShapes = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
            }
            else
            {
                setFeedback("Selection empty - nothing to rotate");
                return;
            }
            List<PowerPoint.Shape> rotated = new List<PowerPoint.Shape>(); // keep track of shapes for undo
            List<float> angle = new List<float>();
            foreach (PowerPoint.Shape s in selectedShapes)
            {
                angle.Add(s.Rotation);
                s.Rotation = 0;
                rotated.Add(s);
            }
            stackChangeRotation(angle, rotated);
        }


        #endregion


        #region Textbox Manipulation

        /// <summary>
        /// a quick converting method to get the coordinate in the textbox control
        /// </summary>
        /// <param name="left">input x coordinate</param>
        /// <returns>adjusted x coordinate</returns>
        private int xTextBoxConvert(float left, float sizeInPoint) 
        {
            // (int)((X - left) * 0.6 ) - 4    // this formula, withouth adjustment, works for a default font of 18
            return (int)((X - left) * 0.6) - 4; 
        }

        private int xTextBoxConvert2(float left, float sizeInPoint, float width)
        {
            // (int)((X - left) * 0.6 ) - 4    // this formula, withouth adjustment, works for a default font of 18
            //System.Windows.Forms.MessageBox.Show(X + "  " + left + "       " + (X - left) + "  " + width);
            return (int)((X - left) * (sizeInPoint / 15));
        }

        /// <summary>
        /// return the index position of the character closest to the mousedown
        /// </summary>
        /// <param name="x">x coordinate of the mousedown, adjusted for textbox coordinate</param>
        /// <param name="box">a textbox equivalent of the text shape added</param>
        /// <param name="mode">mode 1 = insert mode, mode 2 = delete mode</param>
        /// <returns>index of the character closest to mousedown</returns>
        private int getCharIndexFromPosition(int x, TextBox box, int mode)
        {
            int distance = int.MaxValue; // default is large, so we can gradually reduce distance
            int index = -1;
            int tempDistance = int.MaxValue;
            for (int i = 0; i < box.Text.Length; i++)
            {
                // get distance between mousedown X and position of character X
                // y value doesn't matter - the default is always 0
                tempDistance = Math.Abs(x - box.GetPositionFromCharIndex(i).X);
                if (tempDistance < distance) // find min distance
                {
                    index = i;
                    distance = tempDistance;
                }
            }
            // assume that user is more likely to add a phrase in between words than characters
            // so to correct for inaccuracy, check the previous and next index for a space.
            // If found, return that index, if not, return the original index

            if (mode == 1)
            {
                if (index > 0 && box.Text[index - 1] == ' ')
                    return index - 1;
                else if (index < box.Text.Length - 1 && box.Text[index + 1] == ' ')
                    return index + 1;
                else
                    return index;
            }
            else // mode == 2
                return index;
        }

        /// <summary>
        /// insert text into a preexisting textbox object at the mousedown location
        /// </summary>
        /// <param name="box"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        private string insertText(TextBox box, float left)
        {
            try
            {
                int leftInt = xTextBoxConvert(left, box.Font.Size); // adjustment, formula found by repeated testing
                int indexOfChar = getCharIndexFromPosition(leftInt, box, 1); // closest character to mousedown
                if (basicOverlay.Ink.Strokes.Count == 1) // that only stroke is the gesture trigger
                {
                    setFeedback("Space inserted");
                    undoStack.Push(" ");
                    undoStack.Push(indexOfChar);
                    return box.Text.Substring(0, indexOfChar) + " " + box.Text.Substring(indexOfChar);
                }
                else
                {
                    string message = basicOverlay.Ink.Strokes.ToString();
                    message = message.Substring(0, message.Length - 1); // get rid of the recognized semi-circle
                    message.Trim();
                    if (indexOfChar == 0) // if insert at beginning, assume insert a word
                        message = message + " ";
                    if (indexOfChar == box.Text.Length - 1) // if insert at the end
                    {
                        int distance = leftInt - box.GetPositionFromCharIndex(indexOfChar).X;
                        if (distance > 3) // if distance is greater, click occurred at the very end
                        {
                            undoStack.Push(" " + message);
                            undoStack.Push(indexOfChar);
                            return box.Text + " " + message; // so just add message to the end
                        }
                    }
                    setFeedback("New text inserted");
                    // else, just add it in the middle
                    undoStack.Push(" " + message);
                    undoStack.Push(indexOfChar);
                    return box.Text.Substring(0, indexOfChar) + " " + message + box.Text.Substring(indexOfChar);
                }
            }
            catch (Exception e)
            {
                debugExceptionMessage("textboxedit", e);
                return box.Text;
            }
        }

        /// <summary>
        /// delete the character in the textbox closest to the mousedown position
        /// </summary>
        /// <param name="box">a textbox representative of the text object</param>
        /// <param name="left">x position</param>
        /// <returns></returns>
        internal string deleteText(TextBox box, float left)
        {
            int leftInt = xTextBoxConvert2(left, box.Font.Size, box.Width); // adjustment, formula found by repeated testing
            int indexOfChar = getCharIndexFromPosition(leftInt, box, 2); // closest character to mousedown
            //int defaultIndex = box.GetCharIndexFromPosition(new Point((X - box.Left)/3, 1));
            //System.Windows.Forms.MessageBox.Show(indexOfChar + "   " + defaultIndex + "   " + box.Width + "   " + ((X - box.Left)/3));
            undoStack.Push(box.Text[indexOfChar].ToString()); 
            undoStack.Push(indexOfChar);
            return box.Text.Remove(indexOfChar, 1);
        }

        /// <summary>
        /// delete the word in the textbox closest to the mousedown position
        /// </summary>
        /// <param name="box">a textbox representative of the text object</param>
        /// <param name="left">x position</param>
        /// <returns></returns>
        internal string deleteWholeWord(TextBox box, float left)
        {
            int leftInt = xTextBoxConvert(left, box.Font.Size); // adjust x coordinate
            int indexOfChar = getCharIndexFromPosition(leftInt, box, 2); // character closest to mousedown
            string message = box.Text;
            int firstSpace=0, secondSpace=message.Length; // find the two spaces
            for (int before = indexOfChar; before >= 0; before--) // first space before character
            {
                if (message[before] == ' ')
                {
                    firstSpace = before;
                    break;
                }
            }
            for (int after = indexOfChar; after < message.Length; after++) // first space after character
            {
                if (message[after] == ' ')
                {
                    secondSpace = after;
                    break;
                }
            }
            undoStack.Push(box.Text.Substring(firstSpace, secondSpace - firstSpace));
            undoStack.Push(firstSpace);
            return box.Text.Remove(firstSpace, secondSpace - firstSpace);
        }


        /// <summary>
        /// try to insert new text into an existing textbox
        /// </summary>
        /// <param name="shape"></param>
        private void insertIfTextbox(PowerPoint.Shape shape)
        {
            shape.TextFrame.WordWrap = MsoTriState.msoFalse;
            TextBox box = new TextBox(); // need some functions withint TextBox class
            box.Text = shape.TextFrame.TextRange.Text;
            box.Font = new System.Drawing.Font(shape.TextFrame.TextRange.Font.Name, shape.TextFrame.TextRange.Font.Size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            box.Width = (int)shape.TextFrame.TextRange.BoundWidth + 10;
            shape.TextFrame.TextRange.Text = insertText(box, shape.Left);
            shape.Width = shape.TextFrame.TextRange.BoundWidth + buttonForm.BOUND; // adjust width accordingly
            ShapeAttributes clone = new ShapeAttributes(shape);
            undoStack.Push(clone); // keep track of name so for undo's sake
            undoStack.Push("insert");
            setFeedback("Text inserted");
            basicOverlay.Ink.DeleteStrokes();
            Panel.Invalidate();
        }


        /// <summary>
        /// try to delete some character from an existing textbox
        /// </summary>
        /// <param name="shape"></param>
        private void deleteCharIfTextbox(PowerPoint.Shape shape)
        {
            TextBox box = new TextBox();
            box.Text = shape.TextFrame.TextRange.Text;
            box.Font = new System.Drawing.Font(shape.TextFrame.TextRange.Font.Name, shape.TextFrame.TextRange.Font.Size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            box.Width = (int)shape.TextFrame.TextRange.BoundWidth + 10;
            shape.TextFrame.TextRange.Text = deleteText(box, shape.Left);
            shape.Width = shape.TextFrame.TextRange.BoundWidth + 10;
            ShapeAttributes clone = new ShapeAttributes(shape);
            undoStack.Push(clone);
            undoStack.Push("delete");
            setFeedback("Character deleted");
            basicOverlay.Ink.DeleteStrokes();
            Panel.Invalidate();
            if (shape.TextFrame.TextRange.Text.Length == 0)
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show(
                "Textbox is now empty. Do you wish to delete it?",
                "PowerPoint",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation);
                if (dr == DialogResult.OK) // only go ahead and delete if OK button is pressed
                {
                    deleteChosenObject(shape);
                }
            }
        }


        /// <summary>
        /// try to delete some word from an existing textbox
        /// </summary>
        /// <param name="shape"></param>
        private void deleteWordIfTextbox(PowerPoint.Shape shape)
        {
            TextBox box = new TextBox();
            box.Text = shape.TextFrame.TextRange.Text;
            box.Font = new System.Drawing.Font(shape.TextFrame.TextRange.Font.Name, shape.TextFrame.TextRange.Font.Size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            box.Width = (int)shape.TextFrame.TextRange.BoundWidth + 10;
            shape.TextFrame.TextRange.Text = deleteWholeWord(box, shape.Left);
            shape.Width = shape.TextFrame.TextRange.BoundWidth + 10;
            ShapeAttributes clone1 = new ShapeAttributes(shape);
            undoStack.Push(clone1);
            undoStack.Push("deleteWord");
            setFeedback("Word deleted");
            basicOverlay.Ink.DeleteStrokes();
            Panel.Invalidate();
            if (shape.TextFrame.TextRange.Text.Length == 0)
            {
                DialogResult dr = System.Windows.Forms.MessageBox.Show(
                "Textbox is now empty. Do you wish to delete it?",
                "PowerPoint",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Exclamation);
                if (dr == DialogResult.OK) // only go ahead and delete if OK button is pressed
                {
                    deleteChosenObject(shape);
                }
            }
        }


        #endregion


        #region Alternative Recognition Result

        /// <summary>
        /// obtains and returns alternative recognition results
        /// </summary>
        /// <param name="strokes">the strokes to be recognized</param>
        /// <returns>list of alternative</returns>
        private RecognitionAlternates getAlternative(Strokes strokes)
        {
            try
            {
                RecognizerContext context = new RecognizerContext();
                context.Strokes = strokes;
                RecognitionStatus status;
                RecognitionResult result = context.Recognize(out status);
                RecognitionAlternates ra;
                if (RecognitionStatus.NoError == status)
                {
                    ra = result.GetAlternatesFromSelection();
                }
                else
                {
                    ra = null;
                }
                return ra;
            }
            catch (Exception)
            {
                return null;
            }
        }

        
        /// <summary>
        /// display the alternatives of each strokes collection in listboxes, shown below
        /// the recognized textbox.
        /// </summary>
        /// <param name="alternates">list of alternatives</param>
        /// <param name="x">x coordinate of corresponding textbox</param>
        /// <param name="y">y coordinate of corresponding textbox</param>
        private void showAlternative(RecognitionAlternates alternates, int x, int y, string name)
        {
            try
            {
                ListBox list = new ListBox();
                Controls.Add(list);
                // list.BringToFront();

                if (alternates != null)
                {
                    // if I change the location here, I must also change it in findTextbox
                    for (int i = 0; i < alternates.Count; i++)
                    {
                        list.Items.Add(alternates[i]);
                        if (list.Items.Count == 3)
                            list.Size = list.PreferredSize;
                    }

                }
                else
                {
                    list.Items.Add("No alternatives");
                    list.Size = list.PreferredSize;
                }

                list.ScrollAlwaysVisible = true;
                list.IntegralHeight = true;
                list.AllowDrop = false;
                list.Name = name;

                list.Enabled = true;
                list.Hide();
               
                alternateList.Add(list); // keep track of all the listboxes created at one time
            }
            catch (Exception list)
            {
                debugExceptionMessage("list", list);
            }
        }

        /// <summary>
        /// called by the selectedIndexChanged event handler when the user selects an alternative
        /// in any of the listboxes. It then replace the text in the original textbox with the
        /// selected alternative.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            PowerPoint.Shape shapeToChange = findTextboxFromListbox(lb.Name);
            string original = shapeToChange.TextFrame.TextRange.Text;

            shapeToChange.TextFrame.TextRange.Text = lb.SelectedItem.ToString();
            shapeToChange.Width = shapeToChange.TextFrame.TextRange.BoundWidth + 10;
            ShapeAttributes sa = new ShapeAttributes(shapeToChange);
            updateButton(shapeToChange);
            lb.Hide();

            undoStack.Push("");
            undoStack.Push(original);
            undoStack.Push(sa);
            undoStack.Push("SelectedIndexChanged");
        }

        /// <summary>
        /// update the location of the button after the text of the textbox has changed
        /// </summary>
        /// <param name="shapeToChange"></param>
        private void updateButton(PowerPoint.Shape shapeToChange)
        {
            int MARGIN = 20;
            foreach (Button b in buttons)
            {
                if (b.Name == shapeToChange.Name)
                {
                    int x = (int)(shapeToChange.Left + shapeToChange.Width + MARGIN);
                    int y = (int)(shapeToChange.Top + shapeToChange.Height);
                    b.Location = new Point(x, y);
                }
            }
        }


        /// <summary>
        /// find the textbox that corresponds the given listbox. This is now done by a purely
        /// location search
        /// </summary>
        /// <param name="lb"></param>
        /// <returns></returns>
        internal PowerPoint.Shape findTextboxFromListbox(string name)
        {
            List<PowerPoint.Shape> range = pptController.allShapes();
            // the adjustment to Left and Top is dependent on the values set when listbox is created
            foreach (PowerPoint.Shape s in range)
            {
                if (s.Name == name)
                    return s;
            }
            return null;
        }

        internal ListBox findListboxFromTextbox(string name)
        {
            foreach (ListBox lb in alternateList)
            {
                if (lb.Name == name)
                    return lb;
            }
            return null;
        }


        /// <summary>
        /// display a button beside the given shape, giving the user the ability
        /// to call up a list of alternative recognition results
        /// </summary>
        /// <param name="shape"></param>
        private void displayButton(PowerPoint.Shape shape)
        {
            int MARGIN = 20;
            int x = (int)(shape.Left + shape.Width + MARGIN);
            int y = (int)(shape.Top + shape.Height);
            Button dropdown = new Button();
            this.Controls.Add(dropdown);
            dropdown.BringToFront();
            dropdown.Location = new Point(x, y);
            dropdown.Height = 15;
            dropdown.Width = 15;
            dropdown.Text = "v";
            dropdown.Click += new EventHandler(dropdown_Click);
            dropdown.Name = shape.Name;
            buttons.Add(dropdown);
            updateListbox(shape);
        }


        internal void clearButtons()
        {
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
        }


        /// <summary>
        /// occurs when you click on the dropdown button. Toggles the visibility of the
        /// corresponding listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dropdown_Click(object sender, EventArgs e)
        {
            try
            {
                Button dropdown = (Button)sender;
                ListBox lb = findListboxFromButton(dropdown.Left, dropdown.Top, dropdown.Name);
                lb.BringToFront();

                if (lb.Visible == false)
                    lb.Show();
                else
                    lb.Hide();
            }
            catch (Exception blah)
            {
                debugExceptionMessage("drop down click", blah);
            }
        }

        /// <summary>
        /// becaused on information given about a button, find the corresponding listbox.
        /// This is mainly based on position and name
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="name">name of textbox / button</param>
        /// <returns></returns>
        internal ListBox findListboxFromButton(int x, int y, string name)
        {
            List<PowerPoint.Shape> all = pptController.allShapes();
            PowerPoint.Shape textbox = null; // first find the textbox associated with this button
            foreach (PowerPoint.Shape s in all)
            {
                if (s.Name == name) // they should have the same name because button is initialized so
                    textbox = s;
            }

            foreach (ListBox lb in alternateList) // then from the textbox, we can easily find the listbox
            {
                if (textbox.Name == lb.Name)
                    return lb;
            }
            System.Windows.Forms.MessageBox.Show("Should never get here!    " + alternateList.Count);
            return null;
        }


        private void updateListbox(PowerPoint.Shape s)
        {
            ListBox lb = findListboxFromTextbox(s.Name);
            int x = (int)s.Left;
            int y = (int)(s.Top + s.Height);
            lb.Location = new System.Drawing.Point(x + 8, y + 15);
        }



        #endregion



        #region Stack

        private void stackAddShape(PowerPoint.Shape s)
        {
            ShapeAttributes clone = new ShapeAttributes(s);
            undoStack.Push(""); // placeholder
            undoStack.Push(""); // placeholder
            undoStack.Push(clone); // object to be added
            undoStack.Push("AddObject"); // action
        }


        private void stackDeleteShape(PowerPoint.Shape shape)
        {
            ShapeAttributes clone = new ShapeAttributes(shape);
            undoStack.Push("");
            undoStack.Push("");
            undoStack.Push(clone);
            undoStack.Push("DeleteObject");
        }



        private void stackChangeRotation(List<float> angle, List<PowerPoint.Shape> rotated)
        {
            List<ShapeAttributes> clones = new List<ShapeAttributes>();
            ShapeAttributes clone;
            foreach (PowerPoint.Shape s in rotated)
            {
                clone = new ShapeAttributes(s);
                clones.Add(clone);
            }
            undoStack.Push("");
            undoStack.Push(angle);
            undoStack.Push(clones);
            undoStack.Push("ChangeRotation");
        }


        private void stackDeleteShapes(List<PowerPoint.Shape> allShapes)
        {
            List<ShapeAttributes> clones = new List<ShapeAttributes>();
            foreach (PowerPoint.Shape shape in allShapes)
            {
                ShapeAttributes clone = new ShapeAttributes(shape);
                clones.Add(clone);
            }
            undoStack.Push("");
            undoStack.Push("");
            undoStack.Push(clones);
            undoStack.Push("DeleteAllShapes");
        }



        private void stackDeleteTextbox(PowerPoint.Shape shape, ListBox lb)
        {
            ShapeAttributes clone = new ShapeAttributes(shape);
            undoStack.Push("");
            undoStack.Push(lb);
            undoStack.Push(clone);
            undoStack.Push("DeleteTextBox");
        }


        private void stackOrderChange(PowerPoint.Shape shape, string order)
        {
            ShapeAttributes clone = new ShapeAttributes(shape);
            undoStack.Push("");
            undoStack.Push(order);
            undoStack.Push(clone);
            undoStack.Push("ChangeOrder");
        }


        private void stackMove(PowerPoint.ShapeRange totalShapes)
        {
            List<ShapeAttributes> clones = new List<ShapeAttributes>();
            foreach (PowerPoint.Shape s in totalShapes)
            {
                ShapeAttributes sa = new ShapeAttributes(s);
                clones.Add(sa);
            }
            undoStack.Push("");
            undoStack.Push(clones);
        }

        private void stackMoveEnd(PowerPoint.ShapeRange totalShapes)
        {
            List<ShapeAttributes> clones = new List<ShapeAttributes>();
            foreach (PowerPoint.Shape s in totalShapes)
            {
                ShapeAttributes sa = new ShapeAttributes(s);
                clones.Add(sa);
            }
            undoStack.Push(clones);
            undoStack.Push("Move");
        }


        #endregion


    }//class BasicForm
}//namespace Basic
