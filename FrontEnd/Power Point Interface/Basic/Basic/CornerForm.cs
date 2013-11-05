using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;

namespace Basic
{
    /// <summary>
    /// This form is meant to sit in the corner of the screen;
    /// mousing over it will cause the overlay to toggle.
    /// It was one of the tests of how best to handle access to the native 
    /// PowerPoint functions.
    /// </summary>
    public partial class CornerForm : Form
    {
        private InkOverlay corneroverlay;
        private BasicForm basicform;
        internal ButtonForm buttonform;

        /// <summary>
        /// Constructor -- attaches an ink overlay and adds
        /// the mouseMove event handler
        /// </summary>
        /// <param name="inputBasicForm"></param>
        public CornerForm(BasicForm inputBasicForm)
        {
            basicform = inputBasicForm;
            InitializeComponent();
            corneroverlay = new InkOverlay(panel1.Handle);
            corneroverlay.MouseMove += new InkCollectorMouseMoveEventHandler(co_MouseMove);
            corneroverlay.Enabled = true;            

        }
        /// <summary>
        /// on mouseMove, toggle the main ink overlay.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void co_MouseMove(object sender, CancelMouseEventArgs e)
        {
            if (basicform.toggleOverlay())
            {
                buttonform.OverlayOn.Text = "Turn Overlay Off";
                buttonform.OverlayOn.BackColor = SystemColors.Control;
            }
            else
            {
                buttonform.OverlayOn.Text = "Turn Overlay On";
                buttonform.OverlayOn.BackColor = Color.Red;
            }
        }

        /// <summary>
        /// this can probably be safely removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}