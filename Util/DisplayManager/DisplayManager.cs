using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using CircuitParser;

namespace DisplayManager
{
    public class DisplayManager
    {
        #region Internals

        /// <summary>
        /// The panel this manager is controlling.
        /// </summary>
        private SketchPanelLib.SketchPanel panel;

        private List<ParseError> lastErrors;

        #region Feedback Mechanisms

        /// <summary>
        /// Feedback mechanism for coloring meshes in circuit panel
        /// </summary>
        private MeshHighlightingFeedback circuitMeshFeedback;

        /// <summary>
        /// Feedback mechanism for highlighting endpoints in circuit panel
        /// </summary>
        private EndPointHighlightFeedback circuitEndpointFeedback;

        /// <summary>
        /// Feedback mechanism for drawing ghost gate images in the circuit panel
        /// </summary>
        private GhostGateFeedback ghostGateFeedback;

        /// <summary>
        /// A list of the current errors (?)
        /// </summary>
        public List<ErrorBoxHelp> currentErrors;

        // Parse Error Feedback isn't currently used. 
        // For those of you who are interested in looking
        // into it or reviving it, you can probably find the
        // file saved with the rest of the DisplayManager,
        // albeit no longer included in the project 
        //                            - Sketchers 2010

        ///// <summary>
        ///// Feedback mechanism for highlighting parse errors
        ///// </summary>
        //private ParseErrorFeedback circuitParseErrorFeedback;

        /// <summary>
        /// Feedback mechanism for coloring strokes in circuit panel
        /// </summary>
        private ColorFeedback circuitColorFeedback;

        /// <summary>
        /// Displays tooltips over strokes
        /// </summary>
        private DisplayLabel.DisplayLabelTool displayLabelTool;

        /// <summary>
        /// Displays help information for highlighting errors
        /// </summary>
        private DisplayLabel.DisplayHelpTool displayHelpTool;

        /// <summary>
        /// Triggers when there is a successful shape-connection change via. endpoint feedback
        /// </summary>
        public EndpointsChangedHandler EndpointsChanged;

        #endregion

        #region Bools

        private bool subscribed = false;

        private bool coloringOn = true;

        private bool endpointsOn = true;

        private bool meshOn = true;

        private bool labelsOn = false;

        private bool labelsOnRecOn = false;

        private bool gatesOnRecOn = true;

        private bool gatesOnLabelOn = true;

        private bool errorHighlightOn = false;

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p"> The panel that this Manager controls</param>
        /// <param name="ld"> The domain for labeling strokes</param>
        public DisplayManager(ref SketchPanelLib.SketchPanel p, string CircuitFilePath, 
            EndPointMovedHandler endPointChange, bool ColorStrokes = true)
        {
            panel = p;

            // Add and configure feedback
            circuitColorFeedback = new ColorFeedback(ref panel, CircuitFilePath);
            circuitMeshFeedback = new MeshHighlightingFeedback(ref panel);
            circuitEndpointFeedback = new EndPointHighlightFeedback(ref panel, endPointChange);
            ghostGateFeedback = new GhostGateFeedback(ref panel);
            currentErrors = new List<ErrorBoxHelp>();
            //circuitParseErrorFeedback = new ParseErrorFeedback(panel);
            displayLabelTool = new DisplayLabel.DisplayLabelTool(ref panel);
            displayHelpTool = new DisplayLabel.DisplayHelpTool(ref panel);
        }

        #endregion

        #region Subscription

        /// <summary>
        /// Unsubscribes and resubscribes all feedbacks.
        /// </summary>
        public void Clear()
        {
            currentErrors.Clear();
            RemoveGhostGates();
            RemoveErrorHighlights();
            UnsubscribeFromPanel();
            SubscribeToPanel();
        }

        /// <summary>
        /// Subscribes Feedback Mechanisms to Panel
        /// </summary>
        public void SubscribeToPanel()
        {
            subscribeTooltips();
            EndPointHighlightOn();
            MeshHighlightOn();
            ColorFeedbackOn();
            //ParseErrorFeedbackOn();
            subscribed = true;
        }
        /// <summary>
        /// Unsubscribes Feedback Mechanisms from Panel
        /// </summary>
        public void UnsubscribeFromPanel()
        {
            unsubscribeTooltips();
            unsubscribeHelptips();
            EndPointHighlightOff();
            MeshHighlightOff();
            ColorFeedbackOff();
            //ParseErrorFeedbackOff();  
            subscribed = false;
        }

        public bool Subscribed
        {
            get
            {
                return subscribed;
            }
        }
        #endregion

        #region Menu Options

        /// <summary>
        /// Displays the grouping of the strokes.
        /// </summary>
        public void displayAdjacency()
        {
            foreach (Sketch.Shape shape1 in panel.Sketch.Shapes)
                foreach (Sketch.Shape shape2 in shape1.ConnectedShapes)
                {
                    System.Windows.Point p1 = new System.Windows.Point((int)shape1.Centroid.X, (int)shape1.Centroid.Y);

                    System.Windows.Point p2 = new System.Windows.Point((int)shape2.Centroid.X, (int)shape2.Centroid.Y);

                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
                    line.X1 = p1.X;
                    line.X2 = p2.X;
                    line.Y1 = p1.Y;
                    line.Y1 = p2.Y;

                    panel.InkCanvas.Children.Add(line);
                    line.Visibility = System.Windows.Visibility.Visible;
                }
          
        }

        /// <summary>
        /// Show all the points in the sketch.
        /// </summary>
        public void showPoints()
        {
            foreach (Sketch.Point point in panel.InkSketch.Sketch.Points)
            {
                System.Windows.Shapes.Rectangle p1 = new System.Windows.Shapes.Rectangle();
                p1.Fill = System.Windows.Media.Brushes.Red;
                System.Windows.Controls.InkCanvas.SetTop(p1, point.Y);
                System.Windows.Controls.InkCanvas.SetBottom(p1, point.Y + 1);
                System.Windows.Controls.InkCanvas.SetLeft(p1, point.X);
                System.Windows.Controls.InkCanvas.SetRight(p1, point.X + 1);
                panel.InkCanvas.Children.Add(p1);
                p1.Visibility = System.Windows.Visibility.Visible;
            }

        }

        public void displayConnections(Dictionary<Sketch.Shape, Dictionary<Sketch.Shape, bool>> shapeconnections)
        {

            foreach(Sketch.Shape shape1 in shapeconnections.Keys)
                foreach (Sketch.Shape shape2 in shapeconnections[shape1].Keys)
                    if (shapeconnections[shape1][shape2])
                    {
                        System.Windows.Point p1 = new System.Windows.Point((int)shape1.Centroid.X, (int)shape1.Centroid.Y);

                        System.Windows.Point p2 = new System.Windows.Point((int)shape2.Centroid.X, (int)shape2.Centroid.Y);

                        System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
                        line.X1 = p1.X;
                        line.X2 = p2.X;
                        line.Y1 = p1.Y;
                        line.Y1 = p2.Y;

                        panel.InkCanvas.Children.Add(line);
                        line.Visibility = System.Windows.Visibility.Visible;
                    }

       
        }

        public void displayValidity(Dictionary<Sketch.Shape, bool> validity)
        {
            foreach (Sketch.Shape shape in panel.InkSketch.Sketch.Shapes)
            {
                System.Windows.Media.Color shapecolor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("Red");
                if (validity[shape])
                    shapecolor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("Green");
                foreach (Sketch.Substroke substroke in shape.SubstrokesL)
                {
                    String mstrokeId = panel.InkSketch.GetInkStrokeBySubstrokeId(substroke.Id);
                    System.Windows.Ink.Stroke mstroke = panel.InkSketch.GetInkStrokeById(mstrokeId);
                    mstroke.DrawingAttributes.Color = shapecolor;
                }
            }
            panel.InkCanvas.InvalidateArrange();
            panel.UpdateLayout();
        }

        #endregion

        #region FeedbackMechanisms

        #region General Methods
        /// <summary>
        /// Alerts the feedback mechanisms on relabeling or regrouping
        /// </summary>
        /// <param name="shapes">The shapes which were most recently changed</param>
        public void AlertFeedback(CircuitParser.CircuitParser circuit, IEnumerable<Sketch.Shape> shapes)
        {
            if (circuit != null)
                lastErrors = circuit.ParseErrors;
            else
                lastErrors = null;

            if (StrokeColoring) ColorStrokesByType();
            if (EndpointHighlighting) circuitEndpointFeedback.Clear();
            if (GatesOnLabel) DrawFeedback(shapes);
            if (ErrorHighlighting && circuit != null) AddErrorHighlights(circuit.ParseErrors);
            if (labelsOn)
            {
                unsubscribeTooltips();
                subscribeTooltips();
            }
        }

        /// <summary>
        /// Alerts the feedbacks to a whole-circuit recognition change
        /// </summary>
        /// <param name="recognitionButton">Whether or not this alerting was caused by pressing the recognition button</param>
        public void AlertFeedback(CircuitParser.CircuitParser circuit, bool recognitionButton)
        {
            if (circuit != null)
                lastErrors = circuit.ParseErrors;
            else
                lastErrors = null;

            if (StrokeColoring) ColorStrokesByType();
            if (EndpointHighlighting) circuitEndpointFeedback.Clear();
            if (ErrorHighlighting && circuit != null) AddErrorHighlights(circuit.ParseErrors);
            if (labelsOn)
            {
                unsubscribeTooltips();
                subscribeTooltips();
            }

            if (recognitionButton)
            {
                if (GatesOnRec) DrawAllGates();
                if (RecognitionTooltips) showAllTooltips();
            }
        }

        /// <summary>
        /// Refreshes displayed feedbacks
        /// </summary>
        public void RefreshFeedbacks()
        {
            unsubscribeTooltips();
            RemoveErrorHighlights();
            if (ErrorHighlighting) AddErrorHighlights(lastErrors);
            RemoveGhostGates();
            subscribeTooltips();
        }

        /// <summary>
        /// Removes displayed feedbacks from the screen.
        /// </summary>
        public void RemoveFeedbacks()
        {
            unsubscribeTooltips();
            RemoveErrorHighlights();
            RemoveGhostGates();
        }

        #endregion

        #region Mesh Highlighting
        /// <summary>
        /// Subscribes MeshHighlightFeedback
        /// </summary>
        private void MeshHighlightOn()
        {
            if (!circuitMeshFeedback.Subscribed && meshOn)
                circuitMeshFeedback.SubscribeToPanel(ref panel);
        }

        /// <summary>
        /// Unsubscribes MeshHighlightFeedback
        /// </summary>
        private void MeshHighlightOff()
        {
            if (circuitMeshFeedback.Subscribed)
                circuitMeshFeedback.UnsubscribeFromPanel();
        }
        #endregion

        #region Endpoint Highlighting

        /// <summary>
        /// Subscribes EndPointHighlightFeedback
        /// </summary>
        private void EndPointHighlightOn()
        {
            if (!circuitEndpointFeedback.Subscribed && endpointsOn)
            {
                circuitEndpointFeedback.SubscribeToPanel(ref panel);
                circuitEndpointFeedback.EndpointsChanged += new EndpointsChangedHandler(circuitEndpointFeedback_EndpointChanged);
            }
        }

        /// <summary>
        /// Unsubscribes events without removing endpoint highlighting
        /// </summary>
        public void EndPointHighlightPause()
        {
            circuitEndpointFeedback.PauseEvents();
        }

        /// <summary>
        /// Resubscribes events without re-initializing everything
        /// </summary>
        public void EndpointHighlightUnpause()
        {
            if (endpointsOn) circuitEndpointFeedback.UnpauseEvents();
        }

        /// <summary>
        /// Unsubscribes EndPointHighlightFeedback
        /// </summary>
        private void EndPointHighlightOff()
        {
            if (circuitEndpointFeedback.Subscribed)
            {
                circuitEndpointFeedback.UnsubscribeFromPanel();
                circuitEndpointFeedback.EndpointsChanged -= new EndpointsChangedHandler(circuitEndpointFeedback_EndpointChanged);
            }
        }

        /// <summary>
        /// Triggers endpoints changed event
        /// </summary>
        private void circuitEndpointFeedback_EndpointChanged()
        {
            if (EndpointsChanged != null)
                EndpointsChanged();
        }
        #endregion

        #region Ghost Gates

        /// <summary>
        /// Draws All Ghost Gates
        /// </summary>
        private void DrawAllGates()
        {
            ghostGateFeedback.DrawAllGates();
        }

        private void DrawFeedback(IEnumerable<Sketch.Shape> shapes)
        {
            ghostGateFeedback.DrawFeedback(shapes);
        }

        public void DrawFeedback(Sketch.Shape shape)
        {
            ghostGateFeedback.DrawFeedback(shape);
        }

        /// <summary>
        /// Removes all ghost gates from the screen
        /// </summary>
        private void RemoveGhostGates()
        {
            ghostGateFeedback.RemoveAllGates();
        }

        /// <summary>
        /// Turn ghost gate drawing on
        /// </summary>
        private void GhostGatesOn()
        {
            ghostGateFeedback.SubscribeToPanel();
        }

        /// <summary>
        /// Turn ghost gate drawing off
        /// </summary>
        private void GhostGatesOff()
        {
            ghostGateFeedback.UnsubscribeFromPanel();
        }

        #endregion

        #region Color Feedback
        /// <summary>
        /// Subscribes ColorFeedback
        /// </summary>
        private void ColorFeedbackOn()
        {
            if (coloringOn)
                circuitColorFeedback.SubscribeToPanel(ref panel);
        }

        /// <summary>
        /// Unsubscribes ColorFeedback
        /// </summary>
        private void ColorFeedbackOff()
        {
            circuitColorFeedback.UnsubscribeFromPanel();
        }
        #endregion

        #region Error Highlighting
        /// <summary>
        /// Adds Error highlights to canvas
        /// </summary>
        /// <param name="circuit"></param>
        private void AddErrorHighlights(List<ParseError> errors)
        {
            displayHelpTool.Clear();
            RemoveErrorHighlights();

            if (errors == null) return;

            foreach (ParseError error in errors)
            {
                if (panel.Sketch.ShapesL.Contains(error.Where))
                {
                    ErrorBoxHelp newHelp = new ErrorBoxHelp(error, panel);
                    newHelp.drawBox();
                    currentErrors.Add(newHelp);
                }
            }
            displayHelpTool.MakeHelpBlocks(currentErrors);
        }

        /// <summary>
        /// Removes error highlights from canvas
        /// </summary>
        private void RemoveErrorHighlights()
        {
            foreach (ErrorBoxHelp help in currentErrors)
                help.undrawBox();
            currentErrors.Clear();
        }
        #endregion

        #region DisplayLabel

        /// <summary>
        /// Subscribes Tooltips
        /// </summary>
        private void subscribeTooltips()
        {
            if (!labelsOn && !labelsOnRecOn) return;
            displayLabelTool.SubscribeToPanel(panel);
            subscribeHelptips();
        }

        private void showAllTooltips()
        {
            displayLabelTool.DisplayAllToolTips();
        }

        /// <summary>
        /// Unsubscribes Tooltips
        /// </summary>
        private void unsubscribeTooltips()
        {
            displayLabelTool.UnsubscribeFromPanel();
        }

        #endregion

        #region HelpTips
        private void unsubscribeHelptips()
        {
            displayHelpTool.UnsubscribeFromPanel();
        }

        private void subscribeHelptips()
        {
            displayHelpTool.SubscribeToPanel(panel, currentErrors);
        }
        #endregion

        #region Recognition Step Colorings

        /// <summary>
        /// Displays the classification for each stroke, using default (hard-coded) colors
        /// </summary>
        public void displayClassification()
        {
            foreach (Sketch.Substroke substroke in panel.InkSketch.Sketch.Substrokes)
            {
                String mstrokeId = panel.InkSketch.GetInkStrokeBySubstrokeId(substroke.XmlAttrs.Id);
                System.Windows.Ink.Stroke mstroke = panel.InkSketch.GetInkStrokeById(mstrokeId);

                if (substroke.XmlAttrs.Classification == "Wire")
                    mstroke.DrawingAttributes.Color = System.Windows.Media.Colors.Blue;
                else if (substroke.XmlAttrs.Classification == "Gate")
                    mstroke.DrawingAttributes.Color = System.Windows.Media.Colors.Red;
                else if (substroke.XmlAttrs.Classification == "Text")
                    mstroke.DrawingAttributes.Color = System.Windows.Media.Colors.Yellow;
                else
                    mstroke.DrawingAttributes.Color = System.Windows.Media.Colors.Black;
            }

            panel.InkCanvas.InvalidateVisual();
            panel.InkCanvas.UpdateLayout();
        }

        /// <summary>
        /// Displays the classification of each stroke- wire, shape or label
        /// </summary>
        public void displayClassification(Dictionary<string, System.Windows.Media.Color> colorCode)
        {
            foreach (Sketch.Substroke substroke in panel.InkSketch.Sketch.Substrokes)
            {
                String mstrokeId = panel.InkSketch.GetInkStrokeBySubstrokeId(substroke.Id);
                System.Windows.Ink.Stroke mstroke = panel.InkSketch.GetInkStrokeById(mstrokeId);
                mstroke.DrawingAttributes.Color = colorCode[substroke.Classification];
            }

            panel.InkCanvas.InvalidateVisual();
            panel.UpdateLayout();
        }

        /// <summary>
        /// Displays the grouping of the strokes.
        /// </summary>
        public void displayGroups()
        {
            Random colorPicker = new Random();
            foreach (Sketch.Shape shape in panel.InkSketch.Sketch.Shapes)
            {
                Byte[] colorValues = new Byte[4];
                colorPicker.NextBytes(colorValues);
                System.Windows.Media.Color shapecolor = System.Windows.Media.Color.FromArgb(colorValues[0], colorValues[1], colorValues[2], colorValues[3]);
                foreach (Sketch.Substroke substroke in shape.SubstrokesL)
                {
                    String mstrokeId = panel.InkSketch.GetInkStrokeBySubstrokeId(substroke.Id);
                    System.Windows.Ink.Stroke mstroke = panel.InkSketch.GetInkStrokeById(mstrokeId);
                    mstroke.DrawingAttributes.Color = shapecolor;
                }
            }
            panel.InkCanvas.InvalidateVisual();
            panel.UpdateLayout();
        }

        public void ColorStrokesByType()
        {
            if (panel.InkSketch.Sketch != null)
                ColorStrokesByType(panel.InkSketch.Sketch.SubstrokesL);
        }

        public void ColorStrokesByType(List<Sketch.Substroke> substrokes)
        {
            if (!coloringOn)
            {
                ClearColors();
                return;
            }

            if (this.panel.InkSketch.Sketch != null)
            {
                foreach (Sketch.Substroke substroke in substrokes)
                {
                    Domain.ShapeType label = substroke.Type;

                    Color color = label.Color;

                    System.Windows.Ink.Stroke stroke = panel.InkSketch.GetInkStrokeBySubstroke(substroke);
                    stroke.DrawingAttributes.Color = color;
                }
                panel.InkCanvas.InvalidateVisual();
                panel.InkCanvas.UpdateLayout();
            }
        }

        public void ClearColors()
        {
            foreach (System.Windows.Ink.Stroke stroke in panel.InkCanvas.Strokes)
            {
                stroke.DrawingAttributes.Color = Colors.Black;
            }
        }

        #endregion

        #endregion

        #region Feedback Mechanisms Active

        /// <summary>
        /// Get and set whether or not we are showing gates when the stylus is over the shape.
        /// </summary>
        public bool GatesOnHovering
        {
            get
            {
                return ghostGateFeedback.GatesOnHovering;
            }
            set
            {
                ghostGateFeedback.GatesOnHovering = value;
            }
        }

        public bool GatesOnRec
        {
            get
            {
                return gatesOnRecOn;
            }
            set
            {
                if (value)
                    GhostGatesOn();
                else if (!GatesOnLabel && !GatesOnHovering)
                    GhostGatesOff();
                gatesOnRecOn = value;
            }
        }

        public bool GatesOnLabel
        {
            get
            {
                return gatesOnLabelOn;
            }
            set
            {
                if (value)
                    GhostGatesOn();
                else if (!GatesOnRec && !GatesOnHovering)
                    GhostGatesOff();
                gatesOnLabelOn = value;
            }
        }

        public bool StrokeColoring
        {
            get
            {
                return coloringOn;
            }
            set
            {
                coloringOn = value;
                if (value)
                    ColorFeedbackOn();
                else
                    ColorFeedbackOff();
                ColorStrokesByType();
            }
        }

        public bool EndpointHighlighting
        {
            get
            {
                return endpointsOn;
            }
            set
            {
                endpointsOn = value;
                if (value)
                    EndPointHighlightOn();
                else
                    EndPointHighlightOff();
            }
        }

        public bool MeshHighlighting
        {
            get
            {
                return meshOn;
            }
            set
            {
                meshOn = value;
                if (value)
                    MeshHighlightOn();
                else
                    MeshHighlightOff();
            }
        }

        public bool StylusTooltips
        {
            get
            {
                return labelsOn;
            }
            set
            {
                labelsOn = value;
                displayLabelTool.StylusTooltips = value;
                if (value)
                    subscribeTooltips();
                else
                    unsubscribeTooltips();
            }
        }

        public bool RecognitionTooltips
        {
            get
            {
                return labelsOnRecOn;
            }
            set
            {
                labelsOnRecOn = value;
            }
        }

        public bool ErrorHighlighting
        {
            get
            {
                return errorHighlightOn;
            }
            set
            {
                if (!value)
                {
                    RemoveErrorHighlights();
                    unsubscribeHelptips();
                }
                else
                {
                    subscribeHelptips();
                    if (lastErrors != null) AddErrorHighlights(lastErrors);
                }
                errorHighlightOn = value;
            }
        }

        #endregion

    }
}
