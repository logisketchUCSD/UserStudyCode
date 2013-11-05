using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;

namespace PowerpointMinipad
{
    public partial class MiniPadForm : Form
    {
        private InkOverlay minioverlay;
        private PenInputPanel minipenpanel;
        internal PPTcontrol pptController;

        internal MiniPadForm(PPTcontrol inputController)
        {
            InitializeComponent();

            pptController = inputController;


            minioverlay = new InkOverlay(this.Handle);
            
            ClearAppGestures(minioverlay);
            minioverlay.CollectionMode = CollectionMode.InkAndGesture;
            minioverlay.Gesture += new InkCollectorGestureEventHandler(mo_Gesture);
            minioverlay.SystemGesture += new InkCollectorSystemGestureEventHandler(mo_SystemGesture);

            // make it recognize EVERYTHING
            minioverlay.SetGestureStatus(ApplicationGesture.AllGestures, true);

            //list of gestures
            // arrow x4, check, chevronx4 , circle, curlique
            
            minioverlay.Enabled = true;

            minipenpanel = new PenInputPanel(xInput);
            minipenpanel.AutoShow = true;

        }// end miniPadForm constructor

        void mo_Gesture(object sender, InkCollectorGestureEventArgs e)
        {   
            ApplicationGesture gestureID = e.Gestures[0].Id;
            string location = e.Gestures[0].HotPoint.ToString();

            
            System.Windows.Forms.MessageBox.Show("Your gesture was: " + gestureID.ToString() + " at " + location);
            switch (gestureID)
            {
                case ApplicationGesture.ArrowDown:
                    pptController.nextSlide();
                    break;
                case ApplicationGesture.ArrowUp:
                    pptController.prevSlide();
                    break;
                case ApplicationGesture.ChevronDown:
                    pptController.newSlide();
                    break;
                default:
                    break;
            }
        } // end mo_gesture method


        void mo_SystemGesture(object sender, InkCollectorSystemGestureEventArgs e)
        {
        

        }// end method mo_systemgesture


        private void ClearAppGestures(InkOverlay minioverlay)
        {
            ApplicationGesture nogesture = ApplicationGesture.NoGesture;
            System.Array gestureIDs = System.Enum.GetValues(nogesture.GetType());
            foreach (ApplicationGesture gestureID in gestureIDs)
            {
                minioverlay.SetGestureStatus(gestureID, false);
            }
        }

        private void coordButton_Click(object sender, EventArgs e)
        {
            //creates a text box containing "*" at the coordinates entered.
            string xs = xInput.Text;
            string ys = yInput.Text;
            int x = int.Parse(xs);
            int y = int.Parse(ys);
            pptController.addStar(x, y);
            //pptController

        }



    
    
    
    
    
    }//end class miniform
}