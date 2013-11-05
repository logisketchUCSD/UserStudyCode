using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DisplayManager
{
    class GhostGate
    {
        #region Internals

        /// <summary>
        /// Image that has the drawing as a source and is added to the inkcanvas. Also Handles events
        /// </summary>
        private System.Windows.Controls.Image relabelImage;

        /// <summary>
        /// The associated sketch panel
        /// </summary>
        private SketchPanelLib.SketchPanel sketchPanel;
        /// <summary>
        /// Shape that this gate is drawing the computer model of. It is kept around to update orientation
        /// if user specified.
        /// </summary>
        private Sketch.Shape myShape;

        /// <summary>
        /// For user specified orientation, where the user pen downed
        /// </summary>
        private System.Windows.Point startPoint;

        /// <summary>
        /// For user specified orientation, where the user pens up after starting the rotation
        /// </summary>
        private System.Windows.Point endPoint;

        /// <summary>
        /// be absolutely sure not to subscribe if you already have the events
        /// </summary>
        private bool subscribed;

        /// <summary>
        ///  To display popup drawing advice next to relabeled gates
        /// </summary>
        private Popup drawingAdvice;

        #endregion

        #region Constructor
        /// <summary>
        /// The ghost gate to be drawn and added to the Sketch. The ghost gates are tracked in
        /// Edit Menu in currGhosts. It delegates when to draw and undraw these. The Ghosts have
        /// the shape that is associated with it so that it can update Orientation.
        /// 
        /// Also the name Ghost gate is super cool
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="SketchPanel"></param>
        public GhostGate(Sketch.Shape shape, ref SketchPanelLib.SketchPanel SketchPanel, ref GateDrawing.GateDrawing gateDrawer)
        {
            //initialize everything
            startPoint = new System.Windows.Point();
            endPoint = new System.Windows.Point();

            subscribed = false;

            if (!Domain.LogicDomain.IsGate(shape.Type))
                return;
            
            myShape = shape;

            // Make the desired image
            GeometryDrawing ghostGate = gateDrawer.DrawGate(myShape.Type, myShape.Bounds, false, true, myShape.Orientation);

            System.Windows.Media.DrawingImage drawingImage = new System.Windows.Media.DrawingImage(ghostGate);
            relabelImage = new System.Windows.Controls.Image();
            relabelImage.Source = drawingImage;

            sketchPanel = SketchPanel;

            drawingAdvice = createDrawingAdvice(ref gateDrawer);

            //Actual adding of the image
            InkCanvas.SetLeft(relabelImage, myShape.Bounds.Left);
            InkCanvas.SetTop(relabelImage, myShape.Bounds.Top);

            sketchPanel.InkCanvas.Children.Add(relabelImage);
            sketchPanel.InkCanvas.Children.Add(drawingAdvice);
        }
        #endregion

        #region Event Subscription
        /// <summary>
        /// Add the handlers
        /// </summary>
        public void SubscribeEvents()
        {
            if (subscribed)
                return;
            relabelImage.StylusDown += new StylusDownEventHandler(relabelImage_StylusDown);
            relabelImage.StylusUp += new StylusEventHandler(relabelImage_StylusUp);
            subscribed = true;
        }

        /// <summary>
        /// Add the handlers
        /// </summary>
        public void UnSubscribeEvents()
        {
            if (!subscribed)
                return;
            relabelImage.StylusDown -= new System.Windows.Input.StylusDownEventHandler(relabelImage_StylusDown);
            relabelImage.StylusUp -= new StylusEventHandler(relabelImage_StylusUp);
            subscribed = false;
        }
        #endregion

        #region Undraw
        /// <summary>
        /// Undraw the image from the screen
        /// </summary>
        public void unDraw()
        {
            if (GhostGateShowing)
            {
                sketchPanel.InkCanvas.Children.Remove(relabelImage);
                sketchPanel.InkCanvas.Children.Remove(drawingAdvice);
                drawingAdvice.IsOpen = false;
            }
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Handles when you pen down on the image that is being drawn, basically just collects the
        /// point for angle calculations later
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void relabelImage_StylusDown(Object sender, StylusEventArgs e)
        {
            startPoint = e.GetPosition(sketchPanel.InkCanvas);
        }
        private void relabelImage_StylusUp(Object sender, StylusEventArgs e)
        {

        }

        #endregion

        #region Creating Drawing advice

        private Popup createDrawingAdvice(ref GateDrawing.GateDrawing gateDrawer)
        {
            // Make textbox to go with the image
            Popup popup = new Popup();
            TextBlock popupText = new TextBlock();
            popupText.Text = gateDrawer.DrawingAdvice;
            popupText.TextWrapping = System.Windows.TextWrapping.Wrap;
            popupText.Background = Brushes.Pink;
            popupText.Foreground = Brushes.Black;
            popup.Child = popupText;
            popup.IsOpen = false;
            popup.AllowsTransparency = true;
            popup.Visibility = System.Windows.Visibility.Visible;
            popup.PlacementTarget = sketchPanel.InkCanvas;
            popup.Placement = PlacementMode.RelativePoint;
            popup.HorizontalOffset = 20;
            popup.VerticalOffset = 50;
            return popup;
        }

        #endregion

        #region Getters/Setters

        /// <summary>
        ///  Set the popup drawing advice to be open
        /// </summary>
        public void ShowDrawingAdvice()
        {
            drawingAdvice.IsOpen = true;
        }

        /// <summary>
        /// Returns true if this ghost gate's image is currently on the ink canvas
        /// </summary>
        public bool GhostGateShowing
        {
            get
            {
                return sketchPanel.InkCanvas.Children.Contains(relabelImage);
            }
        }

        #endregion
    }
}