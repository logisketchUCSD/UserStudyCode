﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using SketchPanelLib;
using Sketch;
using DisplayLabel;
using DisplayManager;
using System.Windows.Ink;

namespace DisplayLabel
{
    /// <summary>
    /// UI Tool for displaying Help highlights for Parse Errors
    /// </summary>
    public class DisplayHelpTool
    {
        #region Internals

        /// <summary>
        /// Panel that holds the display - the InkCanvas
        /// </summary>
        protected SketchPanel sketchPanel;

        /// <summary>
        /// Bool for showing label info
        /// </summary>
        internal bool debug = false;

        /// <summary>
        /// Current list of our tooltips
        /// </summary>
        private Dictionary<Shape, Popup> toolTips;

        /// <summary>
        /// Radius to measure whether a tool tip should be displayed
        /// </summary>
        private const int TOOLTIP_RADIUS = 30;

        /// <summary>
        /// Timer to keep track of how long we have been in the hover space
        /// </summary>
        private System.Windows.Forms.Timer hoverTimer;

        /// <summary>
        /// Indicates that we have been in the hover space long enough to bring up tooltips
        /// </summary>
        private bool hoverTimeUp;

        /// <summary>
        /// Timer interval - time in hover space before tooltips appear
        /// Helps to limit distraction to a user who is drawing or editing
        /// </summary>
        private const int INTERVAL = 800;

        /// <summary>
        /// Makes sure we do not get over or under subscribed.
        /// </summary>
        private bool subscribed = false;

        /// <summary>
        /// For making sure that move doesn't remove all tooltips after a recognize
        /// </summary>
        private bool needsMoveBack = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalSketch"></param>
        public DisplayHelpTool(ref SketchPanel SP)
            : base()
        {
            // Set the sketch panel info and tooltips
            this.toolTips = new Dictionary<Shape, Popup>();
            this.sketchPanel = SP;

            // Timer
            this.hoverTimer = new System.Windows.Forms.Timer();
            this.hoverTimer.Interval = INTERVAL;

        }


        #endregion

        #region Initializers

        /// <summary>
        /// Goes through every parse error in the sketch and creates popup for it
        /// </summary>
        public void MakeHelpBlocks(List<ErrorBoxHelp> errors)
        {
            toolTips.Clear();

            foreach (ErrorBoxHelp error in errors)
            {
                Shape troubleShape = error.thisError.Where;
                // only one tooltip per shape
                if (!toolTips.ContainsKey(troubleShape))
                {

                    Popup newTextHelp = new Popup();
                    TextBlock popupText = new TextBlock();
                    popupText.Text = error.thisError.Explanation;
                    popupText.Background = Brushes.Yellow;
                    popupText.Foreground = Brushes.Black;
                    newTextHelp.Child = popupText;
                    newTextHelp.IsOpen = false;
                    newTextHelp.AllowsTransparency = true;

                    newTextHelp.Visibility = System.Windows.Visibility.Visible;
                    newTextHelp.PlacementTarget = sketchPanel.InkCanvas;
                    newTextHelp.Placement = PlacementMode.RelativePoint;


                    toolTips.Add(troubleShape, newTextHelp);
                }

            }
        }

        /// <summary>
        /// Subscribes to SketchPanel.
        /// <see cref="SketchPanelLib.SketchPanelListener.SubscribeToPanel()"/>
        /// </summary>
        public void SubscribeToPanel(SketchPanel parentPanel, List<ErrorBoxHelp> errors)
        {
            // Update tooltips
            toolTips.Clear();
            MakeHelpBlocks(errors);

            if (subscribed) return;
            subscribed = true;

            // Hook into SketchPanel stylus and stroke events
            sketchPanel.InkCanvas.StylusInAirMove += new StylusEventHandler(InkCanvas_StylusMove);
            sketchPanel.InkCanvas.StylusDown += new StylusDownEventHandler(InkCanvas_StylusDown);
            sketchPanel.InkCanvas.StylusOutOfRange += new StylusEventHandler(InkCanvas_StylusOutOfRange);
            sketchPanel.InkCanvas.Strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(InkCanvas_StrokesChanged);

            hoverTimer.Tick += new EventHandler(hoverTimer_Tick);
        }

        /// <summary>
        /// Unsubscribes from SketchPanel
        /// <see cref="SketchPanelLib.SketchPanelListener.UnSubscribeToPanel()"/>
        /// </summary>
        public void UnsubscribeFromPanel()
        {
            if (sketchPanel == null || !subscribed)
                return;
            subscribed = false;

            // Unsubscribe from stylus and stroke events
            sketchPanel.InkCanvas.StylusInAirMove -= new StylusEventHandler(InkCanvas_StylusMove);
            sketchPanel.InkCanvas.StylusDown -= new StylusDownEventHandler(InkCanvas_StylusDown);
            sketchPanel.InkCanvas.StylusOutOfRange -= new StylusEventHandler(InkCanvas_StylusOutOfRange);
            sketchPanel.InkCanvas.Strokes.StrokesChanged -= new StrokeCollectionChangedEventHandler(InkCanvas_StrokesChanged);

            hoverTimer.Tick -= new EventHandler(hoverTimer_Tick);

            hidePopups();
            toolTips.Clear();
        }

        /// <summary>
        /// Hide the popups when the window is minimized
        /// </summary>
        public void hidePopups()
        {
            foreach (Popup pop in toolTips.Values)
            {
                pop.IsOpen = false;
                pop.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

        #region Stylus Events

        /// <summary>
        /// Stops the timer and clears the tool tips when the stylus is on the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (needsMoveBack)
            {
                needsMoveBack = false;
                sketchPanel.InkCanvas.StylusInAirMove += new StylusEventHandler(InkCanvas_StylusMove);
            }
            hoverTimer.Stop();
            hoverTimeUp = false;
            HideAllTooltips();
        }

        /// <summary>
        /// Removes the label if a stroke is erased
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StrokeErased(object sender, StylusEventHandler e)
        {
            HideAllTooltips();
        }

        /// <summary>
        /// Updates position of mouse and calls display tooltips
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StylusMove(object sender, StylusEventArgs e)
        {
            if (hoverTimeUp)
                DisplayHelpTip(e.GetPosition(sketchPanel.InkCanvas));
            else if (!hoverTimer.Enabled)
                hoverTimer.Start();
        }

        /// <summary>
        /// Hides all tooltips and stops the timer when the stylus leaves
        /// the InkCanvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StylusOutOfRange(object sender, StylusEventArgs e)
        {
            //HideAllTooltips();
            hoverTimeUp = false;
            hoverTimer.Stop();
        }

        /// <summary>
        /// Refreshes the tooltips to reflect changes to strokes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            /*foreach (Popup box in toolTips.Values)
            {
                //sketchPanel.InkCanvas.Children.Remove(box);
                box.Visibility = System.Windows.Visibility.Hidden;
                box.IsOpen = false;
            }
            toolTips.Clear();
            MakeTextBlocks();*/
        }

        /// <summary>
        /// Indicates that the stylus has been in the hover space long enough to bring
        /// up tool tips
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimeUp = true;
        }

        #endregion

        #region Display and Hide Help

        /// <summary>
        /// Sets all tool tip visibilities to hidden
        /// </summary>
        private void HideAllTooltips()
        {
            foreach (Popup box in toolTips.Values)
            {
                box.Visibility = System.Windows.Visibility.Hidden;
                box.IsOpen = false;
            }

        }

        /// <summary>
        /// Clears the list of tooltips
        /// </summary>
        public void Clear()
        {
            HideAllTooltips();
            toolTips.Clear();
        }

        /// <summary>
        /// Displays help popup for a given error from the mouse coordinates
        /// </summary>
        /// <param name="coordinates">Mouse coordinates</param>
        private void DisplayHelpTip(System.Windows.Point coordinates)
        {
            // Find the shapes that contain this point and update visibilities
            foreach (Sketch.Shape shape in sketchPanel.Sketch.Shapes)
            {
                // See if we can get the corresponding textblock
                Popup textBlock = null;
                   
                //find a cooresponding popup from a shape if one exists
                if (toolTips.ContainsKey(shape))
                    textBlock = toolTips[shape];

                if (textBlock != null) // If we get here, remake the text blocks and try again.
                {
                    //Hit the highlighted area, not just the shape
                    System.Windows.Rect hitbox = shape.Bounds;
                    hitbox.X -= 10;
                    hitbox.Y -= 10;
                    hitbox.Width += 20;
                    hitbox.Height += 20;
                    //correct error box found
                    if (hitbox.Contains(coordinates))
                    {
                        textBlock.HorizontalOffset = 5; //shape.Points[0].X;// coordinates.X;
                        textBlock.VerticalOffset = 30;// sketchPanel.InkCanvas.ActualHeight - 25;// shape.Points[0].Y;// coordinates.Y;
                        textBlock.Visibility = System.Windows.Visibility.Visible;
                        textBlock.IsOpen = true;
                    }
                    else
                    {
                        textBlock.Visibility = System.Windows.Visibility.Hidden;
                        textBlock.IsOpen = false;
                    }
                }
            }
        }

        #endregion

        #region Getters

        public Sketch.Sketch Sketch
        {
            get
            {
                return sketchPanel.InkSketch.Sketch;
            }
        }

        public InkCanvas InkCanvas
        {
            get
            {
                return sketchPanel.InkCanvas;
            }
        }

        #endregion
    }
}
