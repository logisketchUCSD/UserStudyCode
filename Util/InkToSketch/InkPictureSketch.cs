using System;
using System.Collections.Generic;
using System.Text;

using Sketch;
using Microsoft.Ink;

namespace msInkToHMCSketch
{

    /// <summary>
    /// Binds a Microsoft InkPicture object and a HMC Sketch object together.
    /// Extends InkSketch to handle the following InkPicture-specific events:
    ///    * Add stroke (more efficient than InkSketch method)
    ///    * Move/Resize Ink via Selection move/resize
    /// </summary>
    public class InkPictureSketch : InkSketch
    {
        #region Internals

        /// <summary>
        /// The InkPicture to wrap
        /// </summary>
        private Microsoft.Ink.InkPicture mInkPicture;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.  Creates an empty Sketch object and
        /// synchronizes Sketch with changes to <tt>inkPic</tt>'s 
        /// Ink.
        /// </summary>
        /// <param name="inkPic">The InkPicture with which to synchronize</param>
        public InkPictureSketch(InkPicture inkPic)
            : base(inkPic.Ink)
        {
            mInkPicture = inkPic;

            mInkPicture.InkEnabled = false;
            mInkPicture.Ink = base.mInk;
            mInkPicture.InkEnabled = true;

            attachToInkPicture();
        }

        #endregion

        #region InkPicture Event Listening Methods

        /// <summary>
        /// <see cref="InkPicture.Clear(0"/>
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            if (mInkPicture == null)
                return; // on init, mInkPicture is null at first.

            mInkPicture.InkEnabled = false;
            mInkPicture.Ink = base.mInk;
            mInkPicture.InkEnabled = true;
        }

        /// <summary>
        /// Enables the synchronization of Ink to Sketch and subscribes to InkPicture events.
        /// </summary>
        protected override void subscribeToInk()
        {
            base.subscribeToInk();

            if (recording)
                return;

            attachToInkPicture();
        }

        /// <summary>
        /// Adds listeners specific to the InkPicture.  Helps subscribeToInk().
        /// </summary>
        private void attachToInkPicture()
        {
            mInkPicture.SelectionMoved += new Microsoft.Ink.InkOverlaySelectionMovedEventHandler(mInkPicture_SelectionMoved);
            mInkPicture.SelectionResized += new Microsoft.Ink.InkOverlaySelectionResizedEventHandler(mInkPicture_SelectionResized);
            // Swap add ink to Stroke listener for performance
            mInk.InkAdded -= new Microsoft.Ink.StrokesEventHandler(base.mInk_InkAdded);
            mInkPicture.Stroke += new Microsoft.Ink.InkCollectorStrokeEventHandler(mInkPicture_Stroke);           
        }        

        /// <summary>
        /// Disables the synchronization of Ink to Sketch and unsubscribes to InkPicture events.
        /// </summary>
        protected override void unsubscribeToInk()
        {
            base.unsubscribeToInk();

            if (!recording)
                return;

            mInkPicture.SelectionMoved -= new Microsoft.Ink.InkOverlaySelectionMovedEventHandler(mInkPicture_SelectionMoved);
            mInkPicture.SelectionResized -= new Microsoft.Ink.InkOverlaySelectionResizedEventHandler(mInkPicture_SelectionResized);
            mInkPicture.Stroke -= new Microsoft.Ink.InkCollectorStrokeEventHandler(mInkPicture_Stroke);
        }

        /// <summary>
        /// Forwards Ink adds to Sketch adds
        /// </summary>
        protected void mInkPicture_Stroke(object sender, Microsoft.Ink.InkCollectorStrokeEventArgs e)
        {
            base.AddInkStroke(e.Stroke);
        }

        /// <summary>
        /// Forwards Ink resize transforms to Sketch transforms
        /// </summary>
        protected void mInkPicture_SelectionResized(object sender, Microsoft.Ink.InkOverlaySelectionResizedEventArgs e)
        {
            foreach (Microsoft.Ink.Stroke iStroke in InkPicture.Selection)
            {
                base.TransformInkStroke(iStroke);
            }
        }

        /// <summary>
        /// Forwards Ink move transforms to Sketch transforms
        /// </summary>
        protected void  mInkPicture_SelectionMoved(object sender, Microsoft.Ink.InkOverlaySelectionMovedEventArgs e)
        {
            foreach (Microsoft.Ink.Stroke iStroke in InkPicture.Selection)
            {
                base.TransformInkStroke(iStroke);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the InkPicture that this InkPictureSketch wraps.
        /// Setting the InkPicture will delete all strokes in this InkPictureSketch's
        /// Sketch.
        /// </summary>
        public Microsoft.Ink.InkPicture InkPicture
        {
            get
            {
                return mInkPicture;
            }

            set
            {
                unsubscribeToInk();
                mInkPicture = value;
                base.LoadInk(value.Ink);
                subscribeToInk();
            }
        }

        #endregion

    }
}
