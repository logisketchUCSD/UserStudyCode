using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sketch;
using SketchPanelLib;
using GateDrawing;

namespace DisplayManager
{
    public class GhostGateFeedback : FeedbackMechanism
    {
        #region Internals

        /// <summary>
        /// The actual drawing
        /// </summary>
        private GateDrawing.GateDrawing gateDrawer;

        /// <summary>
        /// The currently showing ghost gates
        /// </summary>
        private List<GhostGate> currGhosts;

        /// <summary>
        /// The gate which apprears when you hover over the shape
        /// </summary>
        private GhostGate hoverGate;

        /// <summary>
        /// True if shapes should appear when the pen is held over strokes
        /// </summary>
        private bool shapesOnHover = true;

        #endregion

        #region Constructor and Subscription

        /// <summary>
        /// Constructor.  Makes gate drawer, initializes lists, subscribes to supplied panel
        /// </summary>
        /// <param name="parent"></param>
        public GhostGateFeedback(ref SketchPanel parent)
            : base(ref parent)
        {
            // Set up gate drawer
            gateDrawer = new GateDrawing.GateDrawing();
            gateDrawer.RotateGates = true;
            gateDrawer.SnapRotation = true;
            gateDrawer.LockDrawingRatio = true;

            // Initialize our list of ghosts
            this.currGhosts = new List<GhostGate>();

            SubscribeToPanel();
        }

        /// <summary>
        /// Subscribe to events on the provided panel
        /// </summary>
        /// <param name="newParent"></param>
        public override void SubscribeToPanel(ref SketchPanel newParent)
        {
            base.SubscribeToPanel(ref newParent);
            SubscribeToPanel();
        }

        /// <summary>
        /// Subscribe to all events
        /// </summary>
        public void SubscribeToPanel()
        {
            if (subscribed)
                return;
            subscribed = true;

            sketchPanel.StylusDown += new System.Windows.Input.StylusDownEventHandler(sketchPanel_StylusDown);
            if (GatesOnHovering)
                sketchPanel.StylusInAirMove += new System.Windows.Input.StylusEventHandler(sketchPanel_StylusInAirMove);
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public override void UnsubscribeFromPanel()
        {
            if (!subscribed)
                return;
            subscribed = false;

            RemoveAllGates();

            sketchPanel.StylusDown -= new System.Windows.Input.StylusDownEventHandler(sketchPanel_StylusDown);
            if (GatesOnHovering)
                sketchPanel.StylusInAirMove -= new System.Windows.Input.StylusEventHandler(sketchPanel_StylusInAirMove);
        }

        #endregion

        #region Drawing and Removing Gates

        /// <summary>
        /// Draws ghost gates for all gates present on the InkCanvas
        /// </summary>
        public void DrawAllGates()
        {
            DrawFeedback(sketchPanel.Sketch.Shapes);
            sketchPanel.EnableDrawing();
        }

        /// <summary>
        /// Draw feedback for all the given shapes
        /// </summary>
        /// <param name="shapes"></param>
        public void DrawFeedback(IEnumerable<Sketch.Shape> shapes)
        {
            foreach (Sketch.Shape shape in shapes)
                DrawFeedback(shape);
        }

        /// <summary>
        /// Draws "ghost" gate for the supplied shape 
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="pos"></param>
        public void DrawFeedback(Sketch.Shape shape, bool isHoverGate = false)
        {
            if (!Domain.LogicDomain.IsGate(shape.Type))
                return;

            GhostGate ghostGate = new GhostGate(shape, ref sketchPanel, ref gateDrawer);
            ghostGate.SubscribeEvents();
            currGhosts.Add(ghostGate);

            if (isHoverGate)
                hoverGate = ghostGate;

            if (shape.UserLabeled)
                ghostGate.ShowDrawingAdvice();
        }

        /// <summary>
        /// Removes all ghost gates from the canvas
        /// </summary>
        public void RemoveAllGates()
        {
            List<GhostGate> dummyList = new List<GhostGate>(currGhosts);
            foreach (GhostGate ghost in dummyList)
                RemoveGate(ghost);

            currGhosts.Clear();
        }

        /// <summary>
        /// Remove a single gate from the canvas 
        /// </summary>
        /// <param name="gate"></param>
        private void RemoveGate(GhostGate gate)
        {
            if (gate == null) return;

            if (gate == hoverGate)
                hoverGate = null;

            gate.UnSubscribeEvents();
            gate.unDraw();
            currGhosts.Remove(gate);
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Remove all gates when we are doing something else
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sketchPanel_StylusDown(object sender, EventArgs e)
        {
            RemoveAllGates();
        }

        /// <summary>
        /// Show ghost gate when you hover near a shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sketchPanel_StylusInAirMove(object sender, System.Windows.Input.StylusEventArgs e)
        {
            RemoveGate(hoverGate);

            System.Windows.Point point = e.GetPosition(sketchPanel.InkCanvas);
            Shape shape = sketchPanel.Sketch.shapeAtPoint(point.X, point.Y, 50);
            
            if (shape != null)
                DrawFeedback(shape, true);
        }
        #endregion

        #region Getters and Setters

        /// <summary>
        /// Get and set whether or not we are showing gates when the stylus is over the shape.
        /// </summary>
        public bool GatesOnHovering
        {
            get { return shapesOnHover; }
            set
            {
                if (value != shapesOnHover)
                {
                    UnsubscribeFromPanel();
                    shapesOnHover = value;
                    SubscribeToPanel();
                }
            }
        }

        #endregion
    }
}
