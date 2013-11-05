/* created may 24 07
 * this class is based in philosophy on mike's PPTAuto class
 * it is the one that interacts directly with powerpoint
 * and controls what we want ppt to do.
 */

using System;
using System.Collections.Generic;
using Microsoft.Ink;
using System.Drawing;
using System.Windows.Forms;

namespace Basic
{
    class PPTcontrol
    {

        #region Data Members
        internal PowerPoint.Application pptApp;
        internal PowerPoint.Presentation pptPres;
        private PowerPoint.Slides presSlides;
        private int currentSlideNum;
        internal BasicForm myForm;
        internal ButtonForm myButton;
        private const int SHAPEMARGIN = 8; // the margin of error for clicking on a shape

        //internal int choiceCounter; // counts the number of shapes that a mouseDown falls within
        #endregion


        #region Constructors
        /// <summary>
        /// Constructor that initializes pptApp
        /// </summary>
        /// <param name="inputApp">the current application</param>
        public PPTcontrol(object inputApp)
        {

            pptApp = (PowerPoint.Application)inputApp;

        }//end constructor for pptcontrol
        #endregion


        #region Small Helper Functions
        /// <summary>
        /// initPres initializes the current presentation and slide
        /// </summary>
        /// <returns>always returns true, showing presentation is initialized</returns>
        private bool initPres()
        {
            pptPres = pptApp.ActivePresentation;
            presSlides = pptPres.Slides;
            return true;
        }

        /// <summary>
        /// getCurSlide is a very useful function that returns the current
        /// slide number
        /// </summary>
        /// <returns>current slide index</returns>
        internal int getCurSlide()
        {
            if (!initPres())
            {
                System.Windows.Forms.MessageBox.Show("initPres failed!"); return 0;
            }
            return pptApp.ActiveWindow.Selection.SlideRange.SlideNumber;
        }


        #endregion


        #region Slide Manipulation

        /// <summary>
        /// nextSlide moves to the next ppt slide. If the current slide is the last slide,
        /// there is no effect.
        /// </summary>
        public void nextSlide()
        {

            if (!initPres())
            {
                System.Windows.Forms.MessageBox.Show("initPres failed!");
                return;
            }

            // find current slide
            currentSlideNum = getCurSlide();
            if ((currentSlideNum == presSlides.Count))
            {
                // System.Windows.Forms.MessageBox.Show("Sorry, slide number " + currentSlideNum 
                //                                         + " is the last one.");
            }
            else
            {
                presSlides[currentSlideNum + 1].Select();
            }
        }//end nextSlide


        /// <summary>
        /// prevSlide moves to the previous ppt slide. If the current slide is the first slide,
        /// there is no effect
        /// </summary>
        public void prevSlide()
        {
            if (!initPres())
            {
                System.Windows.Forms.MessageBox.Show("initPres failed!");
                return;
            }

            // find current slide
            currentSlideNum = getCurSlide();

            if ((currentSlideNum == 1))
            {
                //System.Windows.Forms.MessageBox.Show("Previous slide operation failed:\n" +
                //                                        "current slide is the first");
            }
            else
            {
                presSlides[currentSlideNum - 1].Select();
            }
        }//end prevslide

        /// <summary>
        /// newSlide adds a new blank slide after the current slide
        /// </summary>
        public void newSlide()
        {
            if (!initPres())
            {
                System.Windows.Forms.MessageBox.Show("initPres failed!");
                return;
            }
            currentSlideNum = getCurSlide();
            myForm.undoStack.Push("");
            myForm.undoStack.Push("");
            myForm.undoStack.Push(currentSlideNum + 1);
            myForm.undoStack.Push("NewSlide");
            presSlides.Add(currentSlideNum+1, PowerPoint.PpSlideLayout.ppLayoutBlank);
            presSlides[currentSlideNum + 1].Select();
        }// end newslide method

        #endregion


        #region Add Objects

        /// <summary>
        /// addMessage takes in a set or coordinates and a string. It adds the string to the
        /// coordinates on the current slide. The width of the textbox containing the string
        /// is dependent on the length of the text.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="s">a string containing the message to be added</param>
        public PowerPoint.Shape addMessage(int x, int y, string s, Rectangle bound, bool sizeDependent)
        {
            PowerPoint.Slide slide = pptApp.ActivePresentation.Slides[getCurSlide()];
            PowerPoint.Shape textbox;
            int width = s.Length * 17; // approximate an initial width for the text added
            textbox = slide.Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, x, y, width, 30);

            // allow AutoSize and disableing WordWrap allows the textbox to auto fit to the content
            textbox.TextFrame.WordWrap = Microsoft.Office.Core.MsoTriState.msoFalse;
            textbox.TextFrame.AutoSize = PowerPoint.PpAutoSize.ppAutoSizeShapeToFitText;

            // font and font size are obtained from the default PowerPoint font and font size combo box
            
            

            if (myForm.isBold())
                textbox.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
            else
                textbox.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoFalse;
            if (myForm.isItalicize())
                textbox.TextFrame.TextRange.Font.Italic = Microsoft.Office.Core.MsoTriState.msoTrue;
            else
                textbox.TextFrame.TextRange.Font.Italic = Microsoft.Office.Core.MsoTriState.msoFalse;
            if (myForm.isUnderline())
                textbox.TextFrame.TextRange.Font.Underline = Microsoft.Office.Core.MsoTriState.msoTrue;
            else
                textbox.TextFrame.TextRange.Font.Underline = Microsoft.Office.Core.MsoTriState.msoFalse;
            textbox.TextFrame.TextRange.Text = s;
            textbox.Width = textbox.TextFrame.TextRange.BoundWidth + 10;

            // textbox.TextFrame.TextRange.Font.Size = int.Parse(myForm.pptFontSize());
            string name = myForm.pptFont();
            textbox.TextFrame.TextRange.Font.Name = name;

            if (sizeDependent)
            {
                textbox.TextFrame.TextRange.Font.Size = analyzeSize(s, bound, name);
                textbox.Height = textbox.TextFrame.TextRange.BoundHeight;
                textbox.Width = textbox.TextFrame.TextRange.BoundWidth + 10;
            }
            else
            {
                textbox.TextFrame.TextRange.Font.Size = int.Parse(myForm.pptFontSize());
            }

            return textbox;
        }

        private float analyzeSize(string s, Rectangle bound, string name)
        {
            float height = bound.Height / 14;
            float width = bound.Width / 14;
            TextBox test = new TextBox();
            myForm.Controls.Add(test);
            test.Text = s;
            test.WordWrap = false;
            Size preferred;
            for (int i = 8; i < 200; i++) // 200 is just an arbitrarily large max font size
            {
                test.Font = new System.Drawing.Font(name, (float)i, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                preferred = test.PreferredSize;

                if (preferred.Height > height || preferred.Width > width)
                {
                    if (s.Contains("g") || s.Contains("y") || s.Contains("l") || s.Contains("p") || s.Contains("q")
                        || s.Contains("b") || s.Contains("d") || s.Contains("f") || s.Contains("h") || s.Contains("j"))
                        return i - 7;
                    else
                        return i;
                }
            }
            return (float)12;
        }


        /// <summary>
        /// paste acts just like the real paste. It adds a shape of the specified MsoAutoShapeType at
        /// the specified location with the specified dimension. This is implemented because there
        /// is no built in paste for the Shape object that we are interested in
        /// </summary>
        /// <param name="type">the type of the shape</param>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="width">width of the shape</param>
        /// <param name="height">height of the shape</param>
        /// <returns></returns>
        public PowerPoint.Shape paste(Microsoft.Office.Core.MsoAutoShapeType type, int x, int y, float width, float height, int color, float angle)
        {
            int XCORRECTION = 5;
            PowerPoint.Slide slide = pptApp.ActivePresentation.Slides[getCurSlide()];
            PowerPoint.Shape shape = slide.Shapes.AddShape(type, (float)(x + XCORRECTION), y, (int)width, (int)height);
            shape.Fill.ForeColor.RGB = color;
            shape.Rotation = angle;
            return shape;
        }

        public void addShapeToCurrSlide(Microsoft.Office.Core.MsoAutoShapeType shapeType, float x, float y, float width, float height)
        {
            pptApp.ActivePresentation.Slides[getCurSlide()].
                Shapes.AddShape(
                shapeType, x, y, width, height
                );
        }

        public PowerPoint.Shape addLineToCurrSlide(float x1, float y1, float x2, float y2)
        {
            PowerPoint.Shape line = pptApp.ActivePresentation.Slides[getCurSlide()].Shapes.AddLine(
                x1, y1, x2, y2);
            return line;
        }//addLineCurrSlide

        #endregion


        #region Get Information

        /// <summary>
        /// printInfo prints out the height and width of the ppt slide. This will help us
        /// determine the size of the form upon initialization.
        /// Currently not called by anything.
        /// </summary>
        public void printInfo()
        {
            System.Windows.Forms.MessageBox.Show(slideHeight().ToString());
            System.Windows.Forms.MessageBox.Show(slideWidth().ToString());
        }

        /// <summary>
        /// slideHeight returns the height of the slide
        /// </summary>
        /// <returns>height of the slide</returns>
        public int slideHeight()
        {
            return (int)pptApp.ActivePresentation.PageSetup.SlideHeight;
        }

        /// <summary>
        /// slideWidth returns the width of the slide
        /// </summary>
        /// <returns>width of the slide</returns>
        public int slideWidth()
        {
            return (int)pptApp.ActivePresentation.PageSetup.SlideWidth;
        }


        /// <summary>
        /// allShapes collects all the shapes on the current slide into a ShapeRange object, and
        /// returns that.
        /// </summary>
        /// <returns>a shaperange containing all the shapes currently selected</returns>
        public List<PowerPoint.Shape> allShapes()
        {
            PowerPoint.Slide slide = pptApp.ActivePresentation.Slides[getCurSlide()];
            List<PowerPoint.Shape> shapes = new List<PowerPoint.Shape>();            
            foreach (PowerPoint.Shape s in slide.Shapes)
            {
                shapes.Add(s);
            }
            return shapes;
            /*
            PowerPoint.Slide slide = pptApp.ActivePresentation.Slides[getCurSlide()];
            slide.Shapes.SelectAll(); // built in method that selects all object on the slide
            PowerPoint.Selection selection = pptApp.ActiveWindow.Selection; // need the selection to get shapes

            List<int> ids = new List<int>();
            try
            {
                // go through each shape in selection and save those in range
                PowerPoint.ShapeRange range = selection.HasChildShapeRange ? selection.ChildShapeRange : selection.ShapeRange;
                selection.Unselect(); // unselect the entire selection after this call to return to initial situation
                return range;
            }
            catch (Exception)
            {
                return null;
            }
             * */
        }


        /// <summary>
        /// getZoom gets the zoom percentage of the current slide
        /// </summary>
        /// <returns>the current zoom setting of the presentation</returns>
        public int getZoom()
        {
            int zoom = pptApp.ActiveWindow.View.Zoom;
            return zoom;
        }


        /// <summary>
        /// findShape searches through all the shapes on the current slide and checks to see if the
        /// given x and y value falls within any of the shapes. If so, it returns that shape, if not,
        /// it returns null
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>the shape that the mouse falls on, if any</returns>
        public PowerPoint.Shape findShape(int x, int y)
        {           
            BasicForm.fPoint curPoint = new BasicForm.fPoint(x, y);
          
            List<PowerPoint.Shape> range = allShapes(); // need to check through all shapes on the slide
            // iterate through range from latest to earliest
            for (int i = range.Count - 1; i >= 0; --i)
            {               
                // once we get a hit, return it
                if (pointOnShape(curPoint, range[i]))
                {
                    return range[i];
                }
            }
            
            //didn't find any shapes
            return null;
        }

        /// <summary>
        /// Checks whether the input fPoint is "on" the passed shape, with error SHAPEMARGIN (should we pass in margin?) 
        /// </summary>
        /// <param name="curPoint">a fPoint with the x and y values in PowerPoint coordinates</param>
        /// <param name="shape">the PowerPoint shape. May be rotated (but not yet flipped)/</param>
        /// <returns>true if the point and shape coincide, false else</returns>
        internal bool pointOnShape(BasicForm.fPoint curPoint, PowerPoint.Shape shape)
        {
            BasicForm.fPoint p = BasicForm.rotatePointWithShape(curPoint, shape);
            return (shape.Left - SHAPEMARGIN < p.x && p.x < shape.Width + shape.Left + SHAPEMARGIN
                    && shape.Top - SHAPEMARGIN < p.y && p.y < shape.Top + shape.Height + SHAPEMARGIN);
        }

        #endregion




    }//end class pptcontrol
}
