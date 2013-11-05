/* created may 24 07
 * this class is based in philosophy on mike's PPTAuto class
 * it is the one that interacts directly with powerpoint
 * and controls what we want ppt to do.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace PowerpointMinipad
{
    class PPTcontrol
    {
        private PowerPoint.Application pptApp;
        private PowerPoint.Presentation pptPres;
        private PowerPoint.Slides presSlides;
        private int currentSlideNum;

        private int coordTestCount;
        

        public PPTcontrol(object inputApp)
        {
            
            pptApp = (PowerPoint.Application)inputApp;

            coordTestCount = 0;

            

         
        }//end constructor for pptcontrol
            
#region small helper functions
            private bool initPres()
            {
            pptPres = pptApp.ActivePresentation;
            presSlides = pptPres.Slides;
            return true;
            }
            
            private int getCurSlide(){
                if(!initPres()){ System.Windows.Forms.MessageBox.Show("initPres failed!"); return 0 ;}

                return pptApp.ActiveWindow.Selection.SlideRange.SlideNumber;
            }


#endregion


        // goes to the next slide
        public void nextSlide()
        {
            if(!initPres()){ System.Windows.Forms.MessageBox.Show("initPres failed!"); return;}
            
            // find current slide
           currentSlideNum = getCurSlide();

            if ((currentSlideNum == presSlides.Count))
            {
                System.Windows.Forms.MessageBox.Show("Sorry, slide number " + currentSlideNum 
                                                        + "is the last one.");
            }
            else
            {
                presSlides[currentSlideNum + 1].Select();
            }            
        }//end nextSlide


        // goes to the previous slide
        public void prevSlide()
        {
            if(!initPres()){ System.Windows.Forms.MessageBox.Show("initPres failed!"); return;}

            // find current slide
            currentSlideNum = getCurSlide();

            if ((currentSlideNum == 1))
            {
                System.Windows.Forms.MessageBox.Show("Previous slide operation failed:\n" +
                                                        "current slide is the first");
            }
            else
            {
                presSlides[currentSlideNum - 1].Select();
            }
        }//end prevslide

        //adds a (blank) slide at the position immediately after the current one
        public void newSlide()
        {
            if(!initPres()){ System.Windows.Forms.MessageBox.Show("initPres failed!"); return;}
            currentSlideNum = getCurSlide();
           presSlides.Add(currentSlideNum, PowerPoint.PpSlideLayout.ppLayoutBlank);


            }// end newslide method

        //adds a star at appropriate place on slide
        public void addStar(int x, int y)
        {
            int curSlide = getCurSlide();
            presSlides[curSlide].Shapes.AddTextbox(Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal, x, y, 100, 100);
            PowerPoint.TextRange textbox = presSlides[curSlide].Shapes[presSlides[curSlide].Shapes.Count].TextFrame.TextRange;
            textbox.Text = "(" + x + "," + y + ")";
            coordTestCount++;

        }//end addstar

    }//end class pptcontrol
}
