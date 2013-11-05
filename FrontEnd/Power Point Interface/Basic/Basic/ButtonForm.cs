using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Ink;
using System.Collections.Generic;
using Microsoft.Office.Core;

namespace Basic
{
    /// <summary>
    /// This form is for the small toolbar at the top of the ink - form,
    /// which has a few buttons and a feedback textbox.
    /// </summary>
    public partial class ButtonForm : Form
    {
        internal PPTcontrol pptController; // gain access to pptControl methods
        internal BasicForm myBasicForm; // get access to BasicForm methods
        internal int BOUND = 15;
        internal Label gesture;
        internal bool buttonActive;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputController">Connection to the PowerPoint application</param>
        /// <param name="inputForm"> connection to BasicForm (the ink overlay form)</param>
        internal ButtonForm(PPTcontrol inputController, BasicForm inputForm)
        {
            try
            {

                buttonActive = false;
                InitializeComponent();
                pptController = inputController;
                myBasicForm = inputForm;
                this.ClientSize = new System.Drawing.Size(855, 35);
                this.Location = new System.Drawing.Point(192, 110);
                //this.Activated += new EventHandler(ButtonForm_Activated);
                //this.Deactivate += new EventHandler(ButtonForm_Deactivate);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

        }

        void ButtonForm_Deactivate(object sender, EventArgs e)
        {
            buttonActive = false;
            if (buttonActive == false && myBasicForm.basicActive == false && pptController.pptApp.Active == MsoTriState.msoFalse)
            {
                this.Hide();
                myBasicForm.Hide();
                buttonActive = true;
                myBasicForm.basicActive = true;
            }
        }

        void ButtonForm_Activated(object sender, EventArgs e)
        {
            buttonActive = true;
        }


        #region Button Clicks

        /// <summary>
        /// OverlayOn_Click turns the TransparencyKey of the overlay to Wheat. This
        /// simply turns the overlay back on, since it will no longer be transparent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OverlayOn_Click(object sender, EventArgs e)
        {
            // toggle the overlay -- returns true if it was just toggled on.
            if (myBasicForm.toggleOverlay())
            {
                OverlayOn.Text = "Turn Overlay Off";
                this.OverlayOn.BackColor = SystemColors.Control;
                myBasicForm.Activate();
            }
            else
            {
                OverlayOn.Text = "Turn Overlay On";
                this.OverlayOn.BackColor = Color.Red;
                pptController.pptApp.Activate();
            }
        }

        /// <summary>
        /// GesturesOn_Click enables all gestures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GesturesOn_Click(object sender, EventArgs e)
        {
            try
            {
                if (myBasicForm.basicOverlay.GetGestureStatus(ApplicationGesture.DoubleTap).Equals(true))
                {
                    GesturesOn.Text = "Turn Gestures On";
                    myBasicForm.basicOverlay.SetGestureStatus(ApplicationGesture.AllGestures, false);
                    this.GesturesOn.BackColor = Color.Red;
                    showLabel("Gestures Off");
                }
                else
                {
                    GesturesOn.Text = "Turn Gestures Off";
                    myBasicForm.basicOverlay.SetGestureStatus(ApplicationGesture.AllGestures, true);
                    this.GesturesOn.BackColor = SystemColors.Control;
                    showLabel("Gestures On");
                }
            }
            catch (Exception e1)
            {
                System.Windows.Forms.MessageBox.Show(e1.ToString());
            }
            myBasicForm.Activate();
        }


        internal void showLabel(string toggle)
        {
            gesture = new Label();
            gesture.Text = toggle;
            myBasicForm.Controls.Add(gesture);
            gesture.Location = new Point(10, 10);
            gesture.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            gesture.BackColor = Color.LightCyan;
            gesture.ForeColor = Color.Red;
            gesture.AutoSize = true;
            gesture.BringToFront();
            Timer timer = new Timer();
            timer.Interval = 1500;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            myBasicForm.Controls.Remove(gesture);
            gesture.Dispose();
            Timer t = (Timer)sender;
            t.Stop();
        }

        /// <summary>
        /// provides a help menu of the gestures available and their actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetHelpButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(
                "ALL GESTURES MUST BE DONE IN ONE STROKE \n" +
                "Gestures should not start within a SELECTED shape unless you want to move/resize/rotate the shape \n" +
                "\nCommands:\n"+
                "Copy (selected objects):\tDown-Left anywhere on slide\n"+
                "Cut (selected objects):\tDown-Right anywhere on slide\n"+
                "Paste:\t\t\tLeft-Right with turning point at paste location\n" +
                "Delete Object:\t\tSemi-Circle Right starting on object to delete\n" +
                "Clear Slide:\t\tDouble Curlique from lower left to upper right\n" +
                "Delete last added object:\tCurlique from lower left to upper right\n" +
                "Clear all Inkstroke:\t\tScratchout\n" +
                "Delete last Inkstroke:\tLeft-Down\n"+
                "Move:\t\t\tRight-click-drag\n" +
                "Resize:\t\t\tLeft-click-drag on edge of object\n" +
                "Rotate:\t\t\tLeft-click-drag up or down on center of object\n" +
                "Rotate to 0: \t\tDouble circle to set selected objects to 0 rotation\n" +
                "Send Foward: \t\tUp-down starting on the object of interest\n" +
                "Send Backward: \t\tDown-up starting on the object of interest\n" +


                "\nSelecting:\n"+
                "Select an object:\t\tTap on object\n" +
                "Select All:\t\t\tLeft-Up\n" +
                "Select Multiple:\t\tRight starting on shape to add (Ctrl-Click??)\n" +
                "Lasso: \t\t\tRight-click-drag around the objects of interest \n" +


                "\nText Entry:\n"+
                "Enter Text Mode:\t\tUp-Right\n" +
                "Return to Gesture Mode:\tUp-Right-Long\n" +
                "Recognize Text:\t\tCheck\n" +
                

                "\nText Edit:\n" +
                "Insert Text: \t\tSemicircle-Left at location to insert text\n" +
                "Delete Character: \tUp-Left on character to be deleted\n" +
                "Delete Word: \tUp-Left-Long on word to be deleted\n" +


                "\nInsert Shapes:\n"+
                "Block Arrow:\t\tArrow (up, down, left, or right)\n" +
                "Bent Arrow:\t\tChevron (up, down, left, or right)\n" +
                "Circle:\t\t\tCircle\n" +
                "Line:\t\t\tRight-down at starting point and right-down again at endpoint\n" +
                "Square:\t\t\tSquare\n" +
                "Triangle:\t\t\tTriangle\n" +


                "\nSlide Navigation/Manipulation:\n"+
                "New Slide:\t\tStar\n" +
                "Next Slide:\t\tDown\n" +
                "Previous slide:\t\tUp\n"

                );
            myBasicForm.Activate();
        }


        /// <summary>
        /// acts as a normal undo button and undo the last action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Click(object sender, EventArgs e)
        {
            try
            {
                string action; // keeps track of what action we need to undo 
                if (myBasicForm.undoStack.Count < 4) // each action should push 4 things on the stack
                {
                    myBasicForm.setFeedback("There's no more action to undo");
                    myBasicForm.Activate();
                    return;
                }
                action = (string)myBasicForm.undoStack.Pop(); // first thing on stack should describe action to undo

                switch (action)
                {
                    case "insert": // add a phrase to an existing textbox
                        undoInsertText();
                        myBasicForm.setFeedback("Undo Text Insertion");
                        break;
                    case "delete": // delete a character from an existing textbox
                        undoDeleteText();
                        myBasicForm.setFeedback("Undo Character Deletion");
                        break;
                    case "deleteWord": // delete a whole word from an existing textbox
                        undoDeleteWord();
                        myBasicForm.setFeedback("Undo Word Deletion");
                        break;
                    case "AddObject": // add an object to the current slide
                        undoAddObject();
                        myBasicForm.setFeedback("Undo Shape Addition");
                        break;
                    case "TextRec": // recognize the ink strokes currently on basicOverlay
                        undoTextRec();
                        myBasicForm.setFeedback("Undo Text Recognition");
                        break;
                    case "DeleteObject": // delete the object of interest
                        undoDeleteObject();
                        myBasicForm.setFeedback("Undo Object Deletion");
                        break;
                    case "DeleteTextBox": // delete the textbox of interest
                        undoDeleteTextbox();
                        myBasicForm.setFeedback("Undo Textbox Deletion");
                        break;
                    case "ChangeRotation": // change the rotation of selected objects
                        undoChangeRotation();
                        myBasicForm.setFeedback("Undo Rotation Change");
                        break;
                    case "DeleteAllShapes": // delete all the shapes on the slide
                        undoDeleteAllShapes();
                        myBasicForm.setFeedback("Undo Delete All Objects");
                        break;
                    case "CutAll": // cut all the selected shapes
                        undoCutAll();
                        myBasicForm.setFeedback("Undo Cut All Objects");
                        break;
                    case "ChangeOrder": // send shape either forward or backward
                        undoChangeOrder();
                        myBasicForm.setFeedback("Undo Order Change");
                        break;
                    case "DeleteLastStroke": // delete the last stroke added to the overlay
                        undoDeleteLastStroke();
                        myBasicForm.setFeedback("Undo Stroke Deletion");
                        break;
                    case "PasteAll": // paste all the shapes previous cut / copied
                        undoPasteAll();
                        myBasicForm.setFeedback("Undo Paste All Objects");
                        break;
                    case "DeleteAllStrokes": // delete all the strokes from the overlay
                        undoDeleteAllStrokes();
                        myBasicForm.setFeedback("Undo Delete All Strokes");
                        break;
                    case "NewSlide": // add a new slide to current presentation
                        undoNewSlide();
                        myBasicForm.setFeedback("Undo New Slide");
                        break;
                    case "Move":
                        undoMove();
                        myBasicForm.setFeedback("Undo Move");
                        break;
                    case "SelectedIndexChanged":
                        undoIndexChange();
                        myBasicForm.setFeedback("Undo Text Alternative");
                        break;
                    default:
                        myBasicForm.setFeedback("No Action Recognized");
                        myBasicForm.undoStack.Pop();
                        myBasicForm.undoStack.Pop();
                        myBasicForm.undoStack.Pop();
                        break;
                }
                myBasicForm.Activate(); // give focus back to the main form
                if (myBasicForm.redoStack.Count > 0)
                    myBasicForm.buttonForm.RedoButton.Enabled = true;
                else
                    myBasicForm.buttonForm.RedoButton.Enabled = false;
                if (myBasicForm.undoStack.Count > 0)
                    myBasicForm.buttonForm.UndoButton.Enabled = true;
                else
                    myBasicForm.buttonForm.UndoButton.Enabled = false;
            }
            catch (Exception undo)
            {
                System.Windows.Forms.MessageBox.Show(undo.ToString());
            }
        }


        private void RedoButton_Click(object sender, EventArgs e)
        {
            try
            {
                string action; // keeps track of what action we need to undo 
                if (myBasicForm.redoStack.Count < 4) // each action should push 4 things on the stack
                {
                    myBasicForm.setFeedback("There's no more action to redo");
                    myBasicForm.Activate();
                    return;
                }
                action = (string)myBasicForm.redoStack.Pop(); // first thing on stack should describe action to undo

                switch (action)
                {
                    case "insert": // add a phrase to an existing textbox
                        redoInsertText();
                        myBasicForm.setFeedback("Redo Text Insertion");
                        break;
                    case "delete": // delete a character from an existing textbox
                        redoDeleteText();
                        myBasicForm.setFeedback("Redo Character Deletion");
                        break;
                    case "deleteWord": // delete a whole word from an existing textbox
                        redoDeleteWord();
                        myBasicForm.setFeedback("Redo Word Deletion");
                        break;
                    case "AddObject": // add an object to the current slide
                        redoAddObject();
                        myBasicForm.setFeedback("Redo Shape Addition");
                        break;
                    case "TextRec": // recognize the ink strokes currently on basicOverlay
                        redoTextRec();
                        myBasicForm.setFeedback("Redo Text Recognition");
                        break;
                    case "DeleteObject": // delete the object of interest
                        redoDeleteObject();
                        myBasicForm.setFeedback("Redo Object Deletion");
                        break;
                    case "DeleteTextBox": // delete the textbox of interest
                        redoDeleteTextbox();
                        myBasicForm.setFeedback("Redo Textbox Deletion");
                        break;
                    case "ChangeRotation": // change the rotation of selected objects
                        redoChangeRotation();
                        myBasicForm.setFeedback("Redo Rotation Change");
                        break;
                    case "DeleteAllShapes": // delete all the shapes on the slide
                        redoDeleteAllShapes();
                        myBasicForm.setFeedback("Redo Delete All Objects");
                        break;
                    case "CutAll": // cut all the selected shapes
                        redoCutAll();
                        myBasicForm.setFeedback("Redo Cut All Objects");
                        break;
                    case "ChangeOrder": // send shape either forward or backward
                        redoChangeOrder();
                        myBasicForm.setFeedback("Redo Order Change");
                        break;
                    case "DeleteLastStroke": // delete the last stroke added to the overlay
                        redoDeleteLastStroke();
                        myBasicForm.setFeedback("Redo Stroke Deletion");
                        break;
                    case "PasteAll": // paste all the shapes previous cut / copied
                        redoPasteAll();
                        myBasicForm.setFeedback("Redo Paste All Objects");
                        break;
                    case "DeleteAllStrokes": // delete all the strokes from the overlay
                        redoDeleteAllStrokes();
                        myBasicForm.setFeedback("Redo Delete All Strokes");
                        break;
                    case "NewSlide": // add a new slide to current presentation
                        redoNewSlide();
                        myBasicForm.setFeedback("Redo New Slide");
                        break;
                    case "Move":
                        redoMove();
                        myBasicForm.setFeedback("Redo Move");
                        break;
                    case "SelectedIndexChanged":
                        redoIndexChanged();
                        myBasicForm.setFeedback("Redo Alternative Text");
                        break;
                    default:
                        myBasicForm.setFeedback("No Action Recognized");
                        myBasicForm.redoStack.Pop();
                        myBasicForm.redoStack.Pop();
                        myBasicForm.redoStack.Pop();
                        break;
                }
                myBasicForm.Activate(); // give focus back to the main form
                if (myBasicForm.redoStack.Count > 0)
                    myBasicForm.buttonForm.RedoButton.Enabled = true;
                else
                    myBasicForm.buttonForm.RedoButton.Enabled = false;
                if (myBasicForm.undoStack.Count > 0)
                    myBasicForm.buttonForm.UndoButton.Enabled = true;
                else
                    myBasicForm.buttonForm.UndoButton.Enabled = false;
            }
            catch (Exception undo)
            {
                System.Windows.Forms.MessageBox.Show(undo.ToString());
            }
        }

        
        

        #endregion


        #region Undo Methods

        /// <summary>
        /// undo text insertion - delete the text that was inserted
        /// </summary>
        private void undoInsertText()
        {
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.undoStack.Pop(); // attributes about shape
            int index = (int)myBasicForm.undoStack.Pop(); // index where the new text was added
            string content = (string)myBasicForm.undoStack.Pop(); // message that was added
            PowerPoint.Shape undoShape = shapeFromAttributes(sa); // get the shape that matches all the attributes
            undoShape.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = undoShape.TextFrame.TextRange.Text;
            if (index == oldText.Length - content.Length - 1)
                oldText = oldText.Substring(0, index + 1);
            else
                oldText = oldText.Substring(0, index) + oldText.Substring(index + content.Length);
            undoShape.TextFrame.TextRange.Text = oldText; // modify and reset the text
            undoShape.Width = undoShape.TextFrame.TextRange.BoundWidth + BOUND; // make sure width is still OK

            ShapeAttributes newSa = new ShapeAttributes(undoShape);
            myBasicForm.redoStack.Push(newSa);
            myBasicForm.redoStack.Push(index);
            myBasicForm.redoStack.Push(content);
            myBasicForm.redoStack.Push("insert");
        }

        /// <summary>
        /// undo text(character) deletion - get the deleted character back into its proper position
        /// </summary>
        private void undoDeleteText()
        {
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.undoStack.Pop(); // attributes about shape
            int index = (int)myBasicForm.undoStack.Pop(); // index where character was deleted
            string content = (string)myBasicForm.undoStack.Pop(); // content that was deleted

            PowerPoint.Shape undoShape = shapeFromAttributes(sa); // find the shape with stored attributes
            undoShape.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = undoShape.TextFrame.TextRange.Text;
            oldText = oldText.Insert(index, content);
            undoShape.TextFrame.TextRange.Text = oldText; // reset text
            undoShape.Width = undoShape.TextFrame.TextRange.BoundWidth + BOUND; // make sure width is still valid

            ShapeAttributes newSa = new ShapeAttributes(undoShape);
            myBasicForm.redoStack.Push(newSa);
            myBasicForm.redoStack.Push(index);
            myBasicForm.redoStack.Push(content);
            myBasicForm.redoStack.Push("delete");
        }

        /// <summary>
        /// undo text(word) deletion - get the deleted word back into its proper position
        /// </summary>
        private void undoDeleteWord()
        {
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.undoStack.Pop(); // attributes about shape
            int index = (int)myBasicForm.undoStack.Pop(); // index where word was deleted
            string content = (string)myBasicForm.undoStack.Pop(); // content that was deleted

            PowerPoint.Shape undoShape = shapeFromAttributes(sa); // find shape with stored attributes
            undoShape.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = undoShape.TextFrame.TextRange.Text;
            oldText = oldText.Insert(index, content);
            undoShape.TextFrame.TextRange.Text = oldText; // modify and reset the text
            undoShape.Width = undoShape.TextFrame.TextRange.BoundWidth + BOUND; // make sure width is still valid

            ShapeAttributes newSa = new ShapeAttributes(undoShape);
            myBasicForm.redoStack.Push(newSa);
            myBasicForm.redoStack.Push(index);
            myBasicForm.redoStack.Push(content);
            myBasicForm.redoStack.Push("deleteWord");
        }

        /// <summary>
        /// undo object addition - delete the last object that was added
        /// </summary>
        private void undoAddObject()
        {
            ShapeAttributes toDelete = (ShapeAttributes)myBasicForm.undoStack.Pop(); // attributes about shape
            myBasicForm.undoStack.Pop(); // placeholders
            myBasicForm.undoStack.Pop(); // placeholders

            PowerPoint.Shape toDeleteShape = shapeFromAttributes(toDelete); // find shape with attributes
            
            ShapeAttributes newSa = new ShapeAttributes(toDeleteShape);
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(newSa);
            myBasicForm.redoStack.Push("AddObject");

            toDeleteShape.Delete();
        }

        /// <summary>
        /// undo text recognition - delete all the textbox created from last text recognition trigger
        /// and restore the ink strokes to the overlay
        /// </summary>
        private void undoTextRec()
        {
            myBasicForm.basicOverlay.Ink.DeleteStrokes();
            myBasicForm.Panel.Invalidate();
            Strokes strokeRecognized = (Strokes)myBasicForm.undoStack.Pop(); // grab the strokes to restore
            int length = strokeRecognized.Count;
            List<ShapeAttributes> boxes = (List<ShapeAttributes>)myBasicForm.undoStack.Pop(); // textboxes to delete
            myBasicForm.undoStack.Pop(); // placeholder popped

            myBasicForm.basicOverlay.Ink.AddStrokesAtRectangle(strokeRecognized, strokeRecognized.GetBoundingBox());
            myBasicForm.Panel.Invalidate(); // update the overlay after adding the strokes

            List<ListBox> listboxControls = new List<ListBox>();
            foreach (ShapeAttributes shapeToDelete in boxes)
            {
                PowerPoint.Shape textboxToDelete = shapeFromAttributes(shapeToDelete);
                // for each textbox, get rid of the button / listbox associated with it
                ListBox listboxToDelete = myBasicForm.findListboxFromTextbox(textboxToDelete.Name);
                listboxToDelete.Hide();
                myBasicForm.Controls.Remove(listboxToDelete);
                myBasicForm.alternateList.Remove(listboxToDelete);
                listboxControls.Add(listboxToDelete);
                textboxToDelete.Delete();
                myBasicForm.clearButtons();
            }

            myBasicForm.redoStack.Push(listboxControls);
            myBasicForm.redoStack.Push(boxes);
            myBasicForm.redoStack.Push(strokeRecognized);
            myBasicForm.redoStack.Push("TextRec");
        }

        /// <summary>
        /// undo object deletion - bring the object that was last deleted back
        /// </summary>
        private void undoDeleteObject()
        {
            ShapeAttributes add = (ShapeAttributes)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop(); // placeholders
            myBasicForm.undoStack.Pop(); // placeholders

            PowerPoint.Slide slide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            PowerPoint.Shape shapeAdd = slide.Shapes.AddShape(
                add.getType(), add.getLeft(), add.getTop(), add.getWidth(), add.getHeight());
            shapeAdd.Fill.ForeColor.RGB = add.getColor();
            shapeAdd.Rotation = add.getRotation();

            ShapeAttributes newAdd = new ShapeAttributes(shapeAdd);
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(newAdd);
            myBasicForm.redoStack.Push("DeleteObject");
        }

        /// <summary>
        /// undo textbox deletion - bring back the last deleted textbox
        /// </summary>
        private void undoDeleteTextbox()
        {
            ShapeAttributes textbox = (ShapeAttributes)myBasicForm.undoStack.Pop();
            ListBox lb = (ListBox)myBasicForm.undoStack.Pop(); // placeholders
            myBasicForm.undoStack.Pop(); // placeholders

            PowerPoint.Slide currentSlide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            PowerPoint.Shape addTextbox = currentSlide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal,
                textbox.getLeft(), textbox.getTop(), textbox.getWidth(), textbox.getHeight());
            addTextbox.TextFrame.WordWrap = MsoTriState.msoFalse;
            addTextbox.TextFrame.TextRange.Text = textbox.getText();
            addTextbox.TextFrame.TextRange.Font.Size = textbox.getSize();
            addTextbox.TextFrame.TextRange.Font.Name = textbox.getFont();
            addTextbox.Width = addTextbox.TextFrame.TextRange.BoundWidth + 10;
            addTextbox.Name = textbox.getName();

            myBasicForm.Controls.Add(lb);
            myBasicForm.alternateList.Add(lb);

            ShapeAttributes newTextbox = new ShapeAttributes(addTextbox);
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(lb);
            myBasicForm.redoStack.Push(newTextbox);
            myBasicForm.redoStack.Push("DeleteTextBox");
        }

        /// <summary>
        /// undo rotation change - restore the original rotation of all objects affected
        /// </summary>
        private void undoChangeRotation()
        {
            List<ShapeAttributes> shapesAttrToRotate = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            List<float> anglesReset = (List<float>)myBasicForm.undoStack.Pop(); // angle to reset to
            myBasicForm.undoStack.Pop();

            List<PowerPoint.Shape> shapesToRotate = new List<PowerPoint.Shape>();
            PowerPoint.Shape shapeToRotate = null;
            foreach (ShapeAttributes changeRot in shapesAttrToRotate) // find the shape corresponding to each attr
            {
                shapeToRotate = shapeFromAttributes(changeRot);
                shapesToRotate.Add(shapeToRotate);
            }
            for (int i = 0; i < shapesToRotate.Count; i++) // reset all rotations
            {
                shapesToRotate[i].Rotation = anglesReset[i];
            }

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(anglesReset);
            myBasicForm.redoStack.Push(shapesAttrToRotate);
            myBasicForm.redoStack.Push("ChangeRotation");
        }

        /// <summary>
        /// undo delete all shapes - restore all of the shapes deleted by a delete-all command
        /// </summary>
        private void undoDeleteAllShapes()
        {
            List<ShapeAttributes> shapesToDelete = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop(); // placeholders
            myBasicForm.undoStack.Pop(); // placeholders

            PowerPoint.Slide currSlide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            List<ShapeAttributes> newShapes = new List<ShapeAttributes>();
            foreach (ShapeAttributes sa in shapesToDelete) // find corresponding shape first
            {
                if (sa.getText() != null)
                {
                    PowerPoint.Shape addTextbox = currSlide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal,
                        sa.getLeft(), sa.getTop(), sa.getWidth() + BOUND, sa.getHeight());
                    addTextbox.TextFrame.WordWrap = MsoTriState.msoFalse;
                    addTextbox.TextFrame.TextRange.Text = sa.getText();
                    addTextbox.TextFrame.TextRange.Font.Size = sa.getSize();
                    addTextbox.TextFrame.TextRange.Font.Name = sa.getFont();
                    ShapeAttributes newSa = new ShapeAttributes(addTextbox);
                    newShapes.Add(newSa);
                }
                else
                {
                    PowerPoint.Shape shapeToAdd = currSlide.Shapes.AddShape(
                    sa.getType(), sa.getLeft(), sa.getTop(), sa.getWidth(), sa.getHeight());
                    shapeToAdd.Fill.ForeColor.RGB = sa.getColor();
                    shapeToAdd.Rotation = sa.getRotation();
                    ShapeAttributes newSa = new ShapeAttributes(shapeToAdd);
                    newShapes.Add(newSa);
                }

                myBasicForm.redoStack.Push("");
                myBasicForm.redoStack.Push("");
                myBasicForm.redoStack.Push(newShapes);
                myBasicForm.redoStack.Push("DeleteAllShapes");
            }
        }

        /// <summary>
        /// undo cut all shapes - restore all of the shapes previous cut by a cut-all command
        /// </summary>
        private void undoCutAll()
        {
            List<ShapeAttributes> shapesCut = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop(); // placeholders
            myBasicForm.undoStack.Pop(); // placeholders

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(shapesCut);
            myBasicForm.redoStack.Push("CutAll");

            foreach (ShapeAttributes sa in shapesCut)
            {
                PowerPoint.Slide currSlide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
                PowerPoint.Shape shapeToAdd = currSlide.Shapes.AddShape(
                    sa.getType(), sa.getLeft(), sa.getTop(), sa.getWidth(), sa.getHeight());
                shapeToAdd.Fill.ForeColor.RGB = sa.getColor();
                shapeToAdd.Rotation = sa.getRotation();
            }
        }

        /// <summary>
        /// undo last stroke deletion - restore the last deleted shape
        /// </summary>
        private void undoDeleteLastStroke()
        {
            Strokes deleteLastStroke = (Strokes)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(deleteLastStroke);
            myBasicForm.redoStack.Push("DeleteLastStroke");

            myBasicForm.basicOverlay.Ink.AddStrokesAtRectangle(deleteLastStroke, deleteLastStroke.GetBoundingBox());
            myBasicForm.Panel.Invalidate(); // refresh panel to show the new stroke
        }

        /// <summary>
        /// undo order change - resend the shape backwards or forwards
        /// </summary>
        private void undoChangeOrder()
        {
            ShapeAttributes change = (ShapeAttributes)myBasicForm.undoStack.Pop();
            string order = (string)myBasicForm.undoStack.Pop(); // tells whether it's up or down
            myBasicForm.undoStack.Pop();

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(order);
            myBasicForm.redoStack.Push(change);
            myBasicForm.redoStack.Push("ChangeOrder");

            PowerPoint.Shape orderChangeShape = shapeFromAttributes(change);
            if (order.Equals("down"))
                orderChangeShape.ZOrder(MsoZOrderCmd.msoBringForward);
            else
                orderChangeShape.ZOrder(MsoZOrderCmd.msoSendBackward);
        }

        /// <summary>
        /// undo paste all - "delete" all the shapes that was last pasted
        /// </summary>
        private void undoPasteAll()
        {
            List<ShapeAttributes> shapesToPaste = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(shapesToPaste);
            myBasicForm.redoStack.Push("PasteAll");

            foreach (ShapeAttributes sa in shapesToPaste)
            {
                PowerPoint.Shape shape = shapeFromAttributesNoPosition(sa);
                shape.Delete();
            }
        }

        /// <summary>
        /// undo all stroke deletion - restore all the strokes that was previously on the overlay
        /// </summary>
        private void undoDeleteAllStrokes()
        {
            myBasicForm.basicOverlay.Ink.DeleteStrokes();
            myBasicForm.Panel.Invalidate();
            Strokes deleteAllStrokes = (Strokes)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(deleteAllStrokes);
            myBasicForm.redoStack.Push("DeleteAllStrokes");

            if (myBasicForm.scratchoutTriggered == true)
                deleteAllStrokes.RemoveAt(deleteAllStrokes.Count - 1); // don't want to restore the gesture trigger
            myBasicForm.basicOverlay.Ink.AddStrokesAtRectangle(deleteAllStrokes, deleteAllStrokes.GetBoundingBox());
            myBasicForm.Panel.Invalidate();
        }

        /// <summary>
        /// undo new slide - delete the last slide added
        /// </summary>
        private void undoNewSlide()
        {
            int slideIndex = (int)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(slideIndex);
            myBasicForm.redoStack.Push("NewSlide");

            PowerPoint.Slide slideToDelete = pptController.pptPres.Slides[slideIndex];
            slideToDelete.Delete();
        }


        /// <summary>
        /// undo move - move the objects back to where they were
        /// </summary>
        private void undoMove()
        {
            List<ShapeAttributes> newCoord = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            List<ShapeAttributes> oldCoord = (List<ShapeAttributes>)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            for (int i = 0; i < newCoord.Count; i++)
            {
                PowerPoint.Shape shape = shapeFromAttributes(newCoord[i]);
                shape.Left = oldCoord[i].getLeft();
                shape.Top = oldCoord[i].getTop();
            }

            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(newCoord);
            myBasicForm.redoStack.Push(oldCoord);
            myBasicForm.redoStack.Push("Move");
        }


        private void undoIndexChange()
        {
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.undoStack.Pop();
            string original = (string)myBasicForm.undoStack.Pop();
            myBasicForm.undoStack.Pop();

            PowerPoint.Shape textbox = shapeFromAttributes(sa);
            string current = textbox.TextFrame.TextRange.Text; // the text in the textbox right now
            textbox.TextFrame.TextRange.Text = original;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + 10;

            ShapeAttributes newSa = new ShapeAttributes(textbox);
            myBasicForm.redoStack.Push("");
            myBasicForm.redoStack.Push(current);
            myBasicForm.redoStack.Push(newSa);
            myBasicForm.redoStack.Push("SelectedIndexChanged");
        }


        #endregion


        #region Find Shape
        /// <summary>
        /// find the shape on the current slide that corresponds to the attributes stored
        /// </summary>
        /// <param name="toDelete">a ShapeAttributes object that stores the attributes of interest</param>
        /// <returns></returns>
        internal PowerPoint.Shape shapeFromAttributes(ShapeAttributes toDelete)
        {
            List<PowerPoint.Shape> all = pptController.allShapes();
            PowerPoint.Shape toDeleteShape = null;
            foreach (PowerPoint.Shape s in all)
            { // check several attributes
                if (s.Left == toDelete.getLeft() && s.Top == toDelete.getTop()
                    && s.Height == toDelete.getHeight())
                {
                    toDeleteShape = s; // find the one added last that corresponds to the attributes
                }
            }
            return toDeleteShape;
        }

        /// <summary>
        /// find the shape on the current slide that corresponds to the attributes stored, but without
        /// comparing the positions
        /// </summary>
        /// <param name="toDelete"></param>
        /// <returns></returns>
        private PowerPoint.Shape shapeFromAttributesNoPosition(ShapeAttributes toDelete)
        {
            List<PowerPoint.Shape> all = pptController.allShapes();
            PowerPoint.Shape toDeleteShape = null;
            foreach (PowerPoint.Shape s in all)
            { // no position comparison, for the functions that set objects invisible
                if ((s.Width == toDelete.getWidth() || s.Width == toDelete.getWidth() + BOUND)
                    && s.Height == toDelete.getHeight() && s.AutoShapeType.Equals(toDelete.getType())
                    && s.Visible == MsoTriState.msoTrue)
                {
                    toDeleteShape = s;
                }
            }
            return toDeleteShape;
        }

        #endregion


        #region Redo Methods

        /// <summary>
        /// redo action: insert text into an existing textbox
        /// </summary>
        private void redoInsertText()
        {
            string content = (string)myBasicForm.redoStack.Pop(); // the content to add to the textbox
            int index = (int)myBasicForm.redoStack.Pop(); // index to add content to
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes of the textbox

            PowerPoint.Shape textbox = shapeFromAttributes(sa); // find the textbox that matches all the attributes
            textbox.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = textbox.TextFrame.TextRange.Text;
            if (index == oldText.Length - 1) // if text is inserted at the end
                oldText += content;
            else // if text is inserted in the middle or beginning
                oldText = oldText.Substring(0, index) + content + oldText.Substring(index);
            textbox.TextFrame.TextRange.Text = oldText;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + BOUND; // to ensure the text fits

            ShapeAttributes newSa = new ShapeAttributes(textbox);
            myBasicForm.undoStack.Push(content);
            myBasicForm.undoStack.Push(index);
            myBasicForm.undoStack.Push(newSa);
            myBasicForm.undoStack.Push("insert");
        }

        /// <summary>
        /// redo action: delete text from an existing textbox 
        /// </summary>
        private void redoDeleteText()
        {
            string content = (string)myBasicForm.redoStack.Pop(); // content to delete
            int index = (int)myBasicForm.redoStack.Pop(); // index to delete from
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes to look for

            PowerPoint.Shape textbox = shapeFromAttributes(sa);
            textbox.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = textbox.TextFrame.TextRange.Text;
            oldText = oldText.Substring(0, index) + oldText.Substring(index + content.Length);
            textbox.TextFrame.TextRange.Text = oldText;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + BOUND;

            ShapeAttributes newSa = new ShapeAttributes(textbox);
            myBasicForm.undoStack.Push(content);
            myBasicForm.undoStack.Push(index);
            myBasicForm.undoStack.Push(newSa);
            myBasicForm.undoStack.Push("delete");
        }

        /// <summary>
        /// redo action: delete a word from an existing textbox
        /// </summary>
        private void redoDeleteWord()
        {
            string content = (string)myBasicForm.redoStack.Pop(); // content to delete
            int index = (int)myBasicForm.redoStack.Pop(); // index to delete at
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes to look for

            PowerPoint.Shape textbox = shapeFromAttributes(sa);
            textbox.TextFrame.WordWrap = MsoTriState.msoFalse;
            string oldText = textbox.TextFrame.TextRange.Text;
            oldText = oldText.Substring(0, index) + oldText.Substring(index + content.Length);
            textbox.TextFrame.TextRange.Text = oldText;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + BOUND;

            ShapeAttributes newSa = new ShapeAttributes(textbox);
            myBasicForm.undoStack.Push(content);
            myBasicForm.undoStack.Push(index);
            myBasicForm.undoStack.Push(newSa);
            myBasicForm.undoStack.Push("deleteWord");
        }

        /// <summary>
        /// redo action: add an object
        /// </summary>
        private void redoAddObject()
        {
            ShapeAttributes add = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes of the shape to add
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            // sets all the required attributes
            PowerPoint.Slide slide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            PowerPoint.Shape shapeAdd = slide.Shapes.AddShape(
                add.getType(), add.getLeft(), add.getTop(), add.getWidth(), add.getHeight());
            shapeAdd.Fill.ForeColor.RGB = add.getColor();
            shapeAdd.Rotation = add.getRotation();

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(add);
            myBasicForm.undoStack.Push("AddObject");
        }

        /// <summary>
        /// redo action: recognize the ink strokes currently on the overlay
        /// </summary>
        private void redoTextRec()
        {
            Strokes strokeRecognized = (Strokes)myBasicForm.redoStack.Pop(); // the strokes
            List<ShapeAttributes> boxes = (List<ShapeAttributes>)myBasicForm.redoStack.Pop(); // list of attributes
            List<ListBox> listboxes = (List<ListBox>)myBasicForm.redoStack.Pop();

            PowerPoint.Slide currentSlide = pptController.pptApp.ActivePresentation.Slides[pptController.getCurSlide()];
            List<ShapeAttributes> newBoxes = new List<ShapeAttributes>();
            foreach (ShapeAttributes textbox in boxes)
            {
                PowerPoint.Shape addTextbox = currentSlide.Shapes.AddTextbox(MsoTextOrientation.msoTextOrientationHorizontal,
                    textbox.getLeft(), textbox.getTop(), textbox.getWidth() + BOUND, textbox.getHeight());
                addTextbox.TextFrame.WordWrap = MsoTriState.msoFalse;
                addTextbox.TextFrame.TextRange.Text = textbox.getText();
                addTextbox.TextFrame.TextRange.Font.Size = textbox.getSize();
                addTextbox.TextFrame.TextRange.Font.Name = textbox.getFont();
                addTextbox.Width = addTextbox.TextFrame.TextRange.BoundWidth + BOUND;
                addTextbox.Name = textbox.getName();
                ShapeAttributes sa = new ShapeAttributes(addTextbox); // store the new attributes
                newBoxes.Add(sa);
            }

            foreach (ListBox lb in listboxes)
            {
                myBasicForm.Controls.Add(lb);
                myBasicForm.alternateList.Add(lb);
            }

            myBasicForm.basicOverlay.Ink.DeleteStrokes(); // clear the screen
            myBasicForm.Panel.Invalidate();

            myBasicForm.undoStack.Push(listboxes);
            myBasicForm.undoStack.Push(newBoxes);
            myBasicForm.undoStack.Push(strokeRecognized);
            myBasicForm.undoStack.Push("TextRec");
        }

        /// <summary>
        /// redo action: delete an object
        /// </summary>
        private void redoDeleteObject()
        {
            ShapeAttributes deleteObject = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes of the shape to delete
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            PowerPoint.Shape shapeToDelete = shapeFromAttributes(deleteObject);
            shapeToDelete.Delete();

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(deleteObject);
            myBasicForm.undoStack.Push("DeleteObject");
        }

        /// <summary>
        /// redo action: delete a textbox
        /// </summary>
        private void redoDeleteTextbox()
        {
            ShapeAttributes textbox = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attributes of the textbox
            ListBox lb = (ListBox)myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            PowerPoint.Shape textboxToAdd = shapeFromAttributes(textbox); // find the actual shape
            textboxToAdd.Delete();

            lb.Hide();
            myBasicForm.Controls.Remove(lb);
            myBasicForm.alternateList.Remove(lb);
            myBasicForm.clearButtons();

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(lb);
            myBasicForm.undoStack.Push(textbox);
            myBasicForm.undoStack.Push("DeleteTextBox");
        }

        /// <summary>
        /// redo action: change rotation
        /// </summary>
        private void redoChangeRotation()
        {
            List<ShapeAttributes> saRotate = (List<ShapeAttributes>)myBasicForm.redoStack.Pop(); // attributes of involved shapes
            List<float> anglesReset = (List<float>)myBasicForm.redoStack.Pop(); // angles
            myBasicForm.redoStack.Pop();

            foreach (ShapeAttributes sa in saRotate) // select all the shapes involved
            {
                PowerPoint.Shape s = shapeFromAttributes(sa);
                s.Select(MsoTriState.msoFalse);
            }

            myBasicForm.rotateReset(); // also pushes everything back onto the undo stack
            PowerPoint.Selection selection = pptController.pptApp.ActiveWindow.Selection;
            selection.Unselect();
        }

        /// <summary>
        /// redo action: delete all the shapes on the slide
        /// </summary>
        private void redoDeleteAllShapes()
        {
            List<ShapeAttributes> shapesToDelete = (List<ShapeAttributes>)myBasicForm.redoStack.Pop(); // list of attributes for shapes to delete
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            foreach (ShapeAttributes sa in shapesToDelete)
            {
                PowerPoint.Shape s = shapeFromAttributes(sa);
                s.Delete();
            }

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(shapesToDelete);
            myBasicForm.undoStack.Push("DeleteAllShapes");
        }

        /// <summary>
        /// redo action: cut all the shapes on the slide
        /// </summary>
        private void redoCutAll()
        {
            List<ShapeAttributes> shapesCut = (List<ShapeAttributes>)myBasicForm.redoStack.Pop(); // list of attributes for shapes to cut
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            foreach (ShapeAttributes sa in shapesCut)
            {
                PowerPoint.Shape s = shapeFromAttributes(sa);
                s.Delete();
            }

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(shapesCut);
            myBasicForm.undoStack.Push("CutAll");
        }

        /// <summary>
        /// redo action: change the order of the shape
        /// </summary>
        private void redoChangeOrder()
        {
            ShapeAttributes change = (ShapeAttributes)myBasicForm.redoStack.Pop(); // attribute of shape involved
            string order = (string)myBasicForm.redoStack.Pop(); // either down (back) or up (forward)
            myBasicForm.redoStack.Pop();

            PowerPoint.Shape orderChangeShape = shapeFromAttributes(change); // grab the actual shape
            if (order.Equals("down"))
                orderChangeShape.ZOrder(MsoZOrderCmd.msoSendBackward);
            else
                orderChangeShape.ZOrder(MsoZOrderCmd.msoBringForward);

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(order);
            myBasicForm.undoStack.Push(change);
            myBasicForm.undoStack.Push("ChangeOrder");
        }

        /// <summary>
        /// redo action: delete the last stroke added to the overlay
        /// </summary>
        private void redoDeleteLastStroke()
        {
            Strokes lastStroke = (Strokes)myBasicForm.redoStack.Pop(); // the last stroke
            Stroke last = lastStroke[0];
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();
            Stroke lastInBasic = null;

            //myBasicForm.deleteLastStroke();
            foreach (Stroke s in myBasicForm.basicOverlay.Ink.Strokes)
            {
                if (s.GetPoint(1).Equals(last.GetPoint(1)) && s.GetPoint(0).Equals(last.GetPoint(0)))
                    lastInBasic = s; // find the stroke on the overlay
            }

            myBasicForm.basicOverlay.Ink.DeleteStroke(lastInBasic);
            myBasicForm.Panel.Invalidate();
            
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(lastStroke);
            myBasicForm.undoStack.Push("DeleteLastStroke");
            
        }

        /// <summary>
        /// redo action: paste all shapes previously cut / copied
        /// </summary>
        private void redoPasteAll()
        {
            List<ShapeAttributes> shapesToPaste = (List<ShapeAttributes>)myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            myBasicForm.paste(); // this method also pushes stuff onto the undo stack
        }

        /// <summary>
        /// redo action: delete all the strokes
        /// </summary>
        private void redoDeleteAllStrokes()
        {
            Strokes deleteAll = (Strokes)myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            myBasicForm.basicOverlay.Ink.DeleteStrokes();
            myBasicForm.Panel.Invalidate();

            myBasicForm.scratchoutTriggered = false; // next time for undo, don't ignore the last stroke
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(deleteAll);
            myBasicForm.undoStack.Push("DeleteAllStrokes");
        }

        /// <summary>
        /// redo action: add a new slide
        /// </summary>
        private void redoNewSlide()
        {
            int slideIndex = (int)myBasicForm.redoStack.Pop();  // index at which to add the new slide
            myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            pptController.pptPres.Slides.Add(slideIndex, PowerPoint.PpSlideLayout.ppLayoutBlank);

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(slideIndex);
            myBasicForm.undoStack.Push("NewSlide");
        }


        private void redoMove()
        {
            List<ShapeAttributes> newCoord = (List<ShapeAttributes>)myBasicForm.redoStack.Pop();
            List<ShapeAttributes> oldCoord = (List<ShapeAttributes>)myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            for (int i = 0; i < newCoord.Count; i++)
            {
                PowerPoint.Shape shape = shapeFromAttributes(newCoord[i]);
                shape.Left = oldCoord[i].getLeft();
                shape.Top = oldCoord[i].getTop();
            }

            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(newCoord);
            myBasicForm.undoStack.Push(oldCoord);
            myBasicForm.undoStack.Push("Move");
        }


        private void redoIndexChanged()
        {
            ShapeAttributes sa = (ShapeAttributes)myBasicForm.redoStack.Pop();
            string original = (string)myBasicForm.redoStack.Pop();
            myBasicForm.redoStack.Pop();

            PowerPoint.Shape textbox = shapeFromAttributes(sa);
            string current = textbox.TextFrame.TextRange.Text; // the text in the textbox right now
            textbox.TextFrame.TextRange.Text = original;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + 10;

            ShapeAttributes newSa = new ShapeAttributes(textbox);
            myBasicForm.undoStack.Push("");
            myBasicForm.undoStack.Push(current);
            myBasicForm.undoStack.Push(newSa);
            myBasicForm.undoStack.Push("SelectedIndexChanged");
        }


        #endregion



    }
}
