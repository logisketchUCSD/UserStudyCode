﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CircuitSimLib;
using Domain;
using GateDrawing;

namespace SimulationManager
{

    public partial class CleanCircuit : InkCanvas
    {

        #region Internals

        /// <summary>
        /// Set to true to turn on print statements.
        /// </summary>
        private bool debug = false;

        /// <summary>
        /// Our DrawingGroup - hold all elements of drawing
        /// </summary>
        private DrawingGroup DrawingGroup;

        /// <summary>
        /// Lists of gates currently displayed based on shape names and drawings
        /// </summary>
        private Dictionary<string, GeometryDrawing> gatesDrawn;

        /// <summary>
        /// Dictionary of elements
        /// </summary>
        private Dictionary<string, CircuitElement> circuitElements;

        /// <summary>
        /// Dictionary of gates/labels and their corresponding mesh indexes
        /// </summary>
        private Dictionary<string, List<int>> meshDict;

        /// <summary>
        /// List of meshes associated with an index
        /// </summary>
        private Dictionary<int, List<GeometryDrawing>> meshesDrawn;

        /// <summary>
        /// Dictionary of our Inputs and drawings
        /// </summary>
        private Dictionary<string, GeometryDrawing> labels;

        /// <summary>
        /// Draws cleanCircuit gates
        /// </summary>
        private GateDrawing.GateDrawing gateDrawer;

        #endregion

        #region Graph Values

        private const double wireBuffer = 2.0;
        private const double RegularLineThickness = 1.5;
        private const double HighlightLineThickness = 3;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for clean window
        /// </summary>
        /// <param name="sketchpanel"></param>
        /// <param name="eventHandlingOn">Allow the stylus event handler to work if simulating the clean circuit only</param>
        public CleanCircuit(ref SketchPanelLib.SketchPanel sketchpanel, bool eventHandlingOn) : base()
        {
            // Make gate drawer, set some parameters
            gateDrawer = new GateDrawing.GateDrawing();
            gateDrawer.LockDrawingRatio = true;
            gateDrawer.RotateGates = true;
            gateDrawer.SnapRotation = true;

            if (eventHandlingOn)
                this.StylusInAirMove += new StylusEventHandler(ParentStylusInAirMove);
            this.SizeChanged +=new SizeChangedEventHandler(CleanCircuit_SizeChanged);

            circuitElements = sketchpanel.Circuit.CircuitElementGraph;

            // Initialize DrawingGroup and our members that keep track of what we've drawn
            DrawingGroup = new DrawingGroup();
            gatesDrawn = new Dictionary<string, GeometryDrawing>();
            labels = new Dictionary<string, GeometryDrawing>();
            meshDict = new Dictionary<string, List<int>>();
            meshesDrawn = new Dictionary<int, List<GeometryDrawing>>();

            Draw();
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draws all circuit elements and wires
        /// </summary>
        private void Draw()
        {
            DrawingGroup.Children.Clear();

            // Draw our elements and wires
            DrawElements();
            DrawWires();

            Draw(DrawingGroup);
        }

        /// <summary>
        /// Clears the canvas, draws everything in the supplied drawing group
        /// </summary>
        /// <param name="group"></param>
        private void Draw(DrawingGroup group)
        {
            this.Children.Clear();
            foreach (Drawing shape in group.Children)
            {
                DrawingImage di = new DrawingImage(shape);
                Image image = new Image();
                image.Source = di;
                InkCanvas.SetLeft(image, shape.Bounds.Left);
                InkCanvas.SetTop(image, shape.Bounds.Top);
                this.Children.Add(image);
            }
        }

        #region Drawers

        /// <summary>
        /// Draws each circuit element
        /// </summary>
        /// <param name="list"></param>
        /// <param name="generation"></param>
        public void DrawElements()
        {
            if (circuitElements == null)
                return;

            gatesDrawn.Clear();
            labels.Clear();

            //Go through each element and draw appropriate images
            foreach (CircuitElement e in circuitElements.Values)
            {
                if (e.Type == "Gate")
                    DrawGate((Gate)e);
                else if (e.Type == "Input" || e.Type == "Output")
                    DrawLabel(e, e.Type == "Input");
            }
            if (debug)
            {
                Console.WriteLine("Keys are ");
                foreach (string item in labels.Keys)
                    Console.WriteLine(item);
                foreach (string item in gatesDrawn.Keys)
                    Console.WriteLine(item);
            }
        }

        /// <summary>
        /// Creates an image of the gate at the given point using our drawings in Gate Images
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="point"></param>
        private void DrawGate(Gate gate)
        {
            if (gatesDrawn.ContainsKey(gate.Name))
                DrawingGroup.Children.Remove(gatesDrawn[gate.Name]);

            GeometryDrawing drawnGate = gateDrawer.DrawGate(gate.GateType, gate.Bounds, true, false, gate.Orientation);

            // Add to image and to our list of drawn gates
            if (drawnGate != null)
            {
                DrawingGroup.Children.Add(drawnGate);
                gatesDrawn[gate.Name] = drawnGate;
                gate.CleanBounds = drawnGate.Bounds;
            }
        }

        /// <summary>
        /// Creates a text label for an input/output that can be modified
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="point"></param>
        private void DrawLabel(CircuitElement elem, bool input)
        {
            if (labels.ContainsKey(elem.Name))
                DrawingGroup.Children.Remove(labels[elem.Name]);

            FormattedText text = new FormattedText(elem.Name, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                                FlowDirection.LeftToRight, new Typeface("Verdana"), elem.Bounds.Height, Brushes.Salmon);
            Geometry textGeometry = text.BuildGeometry(elem.Bounds.TopLeft);
            GeometryDrawing drawing = new GeometryDrawing(Brushes.Salmon, new Pen(), textGeometry);
            
            labels[elem.Name] = drawing;
            elem.CleanBounds = drawing.Bounds;
            DrawingGroup.Children.Add(drawing);
        }


        /// <summary>
        /// Draws the lines that connect circuit elements. 
        /// Feel free to change how we determine wire paths, it's pretty lame right now..
        /// </summary>
        private void DrawWires()
        {
            ClearWires();

            // Hack:: Dictionary to keep track of number of inputs already added to a gate
            Dictionary<string, int> numInputsAdded = new Dictionary<string, int>();

            // Go through each element and connect to each output element.
            foreach(CircuitElement elem in circuitElements.Values)
            {
                // List to keep track of lines drawn
                List<GeometryDrawing> linesDrawn = new List<GeometryDrawing>();

                // If we have no parents to connect to, return
                if (elem.Outputs.Count == 0)
                    continue;
                
                // Set Pen
                Pen pen = GetPen(elem);

                // Go through all the parents to connect
                foreach (int index in elem.Outputs.Keys)
                {
                    // Find start elem and location
                    Point start = elem.OutPoint(index, true);

                    foreach (CircuitElement parent in elem.Outputs[index])
                    {
                        // Find endpoint
                        Point end = parent.InPoint(ref numInputsAdded, true);
                        Point mid1 = new Point(start.X + (end.X - start.X) / 2, start.Y);
                        Point mid2 = end;

                        // Zig-zag once
                        mid1 = ClearPathwayHoriz(start, mid1);
                        linesDrawn.Add(CreateLine(start, mid1, pen));

                        // Zig-zag until we reach the end
                        while (mid1 != end)
                        {
                            mid2 = ClearPathwayVert(mid1, end);
                            linesDrawn.Add(CreateLine(mid1, mid2, pen));

                            mid1 = ClearPathwayHoriz(mid2, end);
                            linesDrawn.Add(CreateLine(mid2, mid1, pen));

                            if (mid1 == mid2) // We're looping, let's stop this.
                            {
                                linesDrawn.Add(CreateLine(mid1, end, pen));
                                break;
                            }
                        }
                    }
                }
                // Update our meshes with the lines drawn
                UpdateMeshes(linesDrawn, elem);
            }
        }

        
        #endregion

        #region Helpers for Drawers

        /// <summary>
        /// Creates a LineGeometry and adds it to the drawinggroup
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private GeometryDrawing CreateLine(Point start, Point end, Pen pen)
        {
            LineGeometry line = new LineGeometry(start, end);
            GeometryDrawing geometry = new GeometryDrawing(Brushes.Black, pen, line);
            DrawingGroup.Children.Add(geometry);
            return geometry;
        }

        /// <summary>
        /// Sets the pen color (based on circuit value) and thickness
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        private Pen GetPen(CircuitElement elem, int index = 0)
        {
            Pen pen = new Pen();
            pen.Thickness = RegularLineThickness;
            int value = elem.Output[index];

            if (value == 0)
                pen.Brush = Brushes.MidnightBlue;
            else
                pen.Brush = Brushes.SkyBlue;
            return pen;
        }

        /// <summary>
        /// Returns the point at which a purely horizontal path traveling from start to end must end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private Point ClearPathwayHoriz(Point start, Point end)
        {
            bool goingRight = true;
            double startX = start.X;
            double endX = end.X;
            if (end.X < start.X)
            {
                startX = end.X;
                endX = start.X;
                goingRight = false;
            }

            foreach (Rect location in circuitElemLocations.Values)
            {
                if (location.X > startX && location.X < endX && start.Y > location.Y && start.Y < location.Y + location.Height)
                {
                    Point point = new Point();
                    point.Y = start.Y;
                    if (goingRight)
                        point.X = location.Left - wireBuffer;
                    else
                        point.X = location.Right + wireBuffer;
                    return point;
                }
            }
            return new Point(end.X,start.Y);
        }

        /// <summary>
        /// Returns the point at which a purely vertical path traveling from start to end must end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private Point ClearPathwayVert(Point start, Point end)
        {
            bool goingDown = true;
            double startY = start.Y;
            double endY = end.Y;
            if (end.Y < start.Y)
            {
                startY = end.Y;
                endY = start.Y;
                goingDown = false;
            }

            foreach (Rect location in circuitElemLocations.Values)
            {
                if (location.Y > startY && location.Y < endY && start.X > location.X && start.X < location.X + location.Width)
                {
                    Point point = new Point();
                    point.X = start.X;
                    if (goingDown)
                        point.Y = location.Top - wireBuffer;
                    else
                        point.Y = location.Bottom + wireBuffer;
                    return point;
                }
            }
            return new Point(start.X, end.Y);
        }

        /// <summary>
        /// A dictionary of label names and the point at which their associated popup should appear
        /// </summary>
        /// <param name="canvasOffset"></param>
        /// <returns></returns>
        public Dictionary<string, Point> LabelLocations
        {
            get
            {
                Dictionary<string, Point> labelLocations = new Dictionary<string, Point>();

                // Magic numbers! They come from CircuitValue popups, which assumes that inputs and outputs 
                // want a left->right orientation and adjusts accordingly
                int popupBuffer = 3;
                int popupSize = 25;

                foreach (string key in labels.Keys)
                {
                    // Create a new point with canvas offset
                    string Type = circuitElements[key].Type;
                    Rect bounds = labels[key].Bounds;

                    // Pick a point based on type and orientation
                    // Untested on Angles other than 0 (for labels don't get orientations other than 0 yet), so feel free to alter
                    Point point = new Point();
                    if (Type == "Input")
                    {
                        point = bounds.TopLeft;
                        if (circuitElements[key].Angle == 90)
                        {
                            point.X -= popupBuffer;
                            point.Y -= popupBuffer + popupSize;
                        }
                        else if (circuitElements[key].Angle == 180)
                        {
                            point = bounds.TopRight;
                            point.X += 2*popupBuffer + popupSize;
                        }
                        else if (circuitElements[key].Angle == 270)
                        {
                            point = bounds.BottomLeft;
                            point.X += popupBuffer;
                            point.Y += popupBuffer;
                        }
                    }
                    else if (Type == "Output")
                    {
                        point = bounds.TopRight;
                        if (circuitElements[key].Angle == 90)
                        {
                            point = bounds.BottomLeft;
                            point.X -= popupBuffer + popupSize;
                            point.Y += popupBuffer;
                        }
                        else if (circuitElements[key].Angle == 180)
                        {
                            point = bounds.TopLeft;
                            point.X -= 2*popupBuffer + popupSize;
                        }
                        else if (circuitElements[key].Angle == 270)
                        {
                            point = bounds.TopLeft;
                            point.X += popupBuffer;
                            point.Y -= popupBuffer + popupSize;
                        }
                    }
                    labelLocations.Add(key, point);
                }
                return labelLocations;
            }
        }

        /// <summary>
        /// A dictionary of shape names and the bounds of their associated drawings
        /// </summary>
        private Dictionary<string, Rect> circuitElemLocations
        {
            get
            {
                Dictionary<string, Rect> temp = new Dictionary<string, Rect>();

                foreach (string label in labels.Keys)
                    temp[label] = labels[label].Bounds;
                foreach (string gate in gatesDrawn.Keys)
                    temp[gate] = gatesDrawn[gate].Bounds;

                return temp;
            }
        }

        #endregion

        #region Mesh Updates

        /// <summary>
        /// Updates our meshes drawn and our meshDictionary
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="child"></param>
        private void UpdateMeshes(List<GeometryDrawing> mesh, CircuitElement elem)
        {
            // NotBubble Case
            if (elem.Type == "Gate" && ((Gate)elem).GateType == LogicDomain.NOTBUBBLE)
                elem = elem.Outputs[0][0];

            // ID number of this mesh
            int index = meshesDrawn.Keys.Count;

            // Update our parent meshes 
            foreach (List<CircuitElement> list in elem.Outputs.Values)

            {
                foreach (CircuitElement parent in list)
                {
                    string ParentName = parent.Name;
                    if (parent.Type == "Gate" && ((Gate)parent).GateType == LogicDomain.NOTBUBBLE && parent.Outputs.Count > 0)
                        ParentName = parent.Outputs[0][0].Name;

                    if (meshDict.ContainsKey(ParentName))
                        meshDict[ParentName].Add(index);
                    else
                    {
                        List<int> newMeshList = new List<int>();
                        newMeshList.Add(index);
                        meshDict[ParentName] = newMeshList;
                    }
                }
            }

            // Update our elem meshes
            if (meshDict.ContainsKey(elem.Name))
                meshDict[elem.Name].Add(index);
            else
            {
                List<int> newMeshList = new List<int>();
                newMeshList.Add(index);
                meshDict[elem.Name] = newMeshList;
            }

            // Add to meshes drawn
            meshesDrawn[index] = mesh;
        }

        #endregion

        #endregion

        #region Updates from Toggle/TruthTable inputs

        /// <summary>
        /// Removes the old wires from the sketch
        /// </summary>
        private void ClearWires()
        {
            foreach (List<GeometryDrawing> listLines in meshesDrawn.Values)
                foreach (GeometryDrawing line in listLines)
                    if (DrawingGroup.Children.Contains(line))
                        DrawingGroup.Children.Remove(line);

            meshDict.Clear();
            meshesDrawn.Clear();
        }

        /// <summary>
        /// Removes old wires and updates new wires (new colors)
        /// </summary>
        public void UpdateWires()
        {
            DrawWires();
            Draw(DrawingGroup);
        }

        /// <summary>
        /// Updates the TextGeometries to display the new Names
        /// </summary>
        /// <param name="oldNames"></param>
        /// <param name="newNames"></param>
        public void UpdateLabel(Dictionary<string,string> newNameDict)
        {
            // Update our geometries
            foreach (string key in newNameDict.Keys)
            {
                DrawingGroup.Children.Remove(labels[key]);

                // Update the meshDict
                List<int> value;
                meshDict.TryGetValue(key, out value);
                meshDict.Remove(key);
                meshDict.Add(newNameDict[key], value);

                if (debug) Console.WriteLine("Key removed is " + key);

                CircuitElement elem = circuitElements[key];
                
                bool input = (elem.Type == "Input");

                DrawLabel(elem, input);
                labels.Remove(key);
            }
            // Update Image
            Draw(DrawingGroup);
        }

        #endregion

        #region Mesh Highlighting

        /// <summary>
        /// Event for mesh highlighting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentStylusInAirMove(object sender, StylusEventArgs e)
        {
            // Our stylus position
            Point stylPt = e.GetPosition(this);

            // Unhighlight everything
            foreach (List<GeometryDrawing> mesh in meshesDrawn.Values)
                foreach (GeometryDrawing line in mesh)
                    line.Pen.Thickness = RegularLineThickness;
            foreach (GeometryDrawing gate in gatesDrawn.Values)
                gate.Pen.Thickness = RegularLineThickness;
            foreach (GeometryDrawing label in labels.Values)
                label.Pen.Thickness = RegularLineThickness;

            // What do we want to highlight?
            List<int> meshesHighlighted = new List<int>();
            List<String> elementsHighlighted = new List<string>();

            // The stylus is only at one place, so if we find an element under it we shouldn't look any more.
            bool pointFound = false;

            // Find everything that should be highlighted
            // Try not to search any more than you have to!

            // Look at the gates first...
            foreach (string gate in gatesDrawn.Keys)
                if (gatesDrawn[gate].Bounds.Contains(stylPt))
                {
                    foreach (int mesh in meshDict[gate])
                        if (!meshesHighlighted.Contains(mesh))
                            meshesHighlighted.Add(mesh);

                    elementsHighlighted.Add(gate);

                    pointFound = true;

                    break;
                }
            
            // If we haven't found the point, look in the labels
            if (!pointFound)
                foreach (string label in labels.Keys)
                    if (labels[label].Bounds.Contains(stylPt))
                    {
                        foreach (int mesh in meshDict[label])
                            if (!meshesHighlighted.Contains(mesh))
                                meshesHighlighted.Add(mesh);

                        elementsHighlighted.Add(label);

                        pointFound = true;

                        break;
                    }

            // If we still haven't found the point, look in the wires
            if (!pointFound)
                foreach (int mesh in meshesDrawn.Keys)
                {
                    foreach (GeometryDrawing line in meshesDrawn[mesh])
                    {
                        Rect bounds = new Rect(new Point(line.Bounds.X - 10, line.Bounds.Y - 10),
                            new Size(line.Bounds.Width + 20, line.Bounds.Height + 20));
                        if (bounds.Contains(stylPt))
                        {
                            pointFound = true;
                            meshesHighlighted.Add(mesh);

                            foreach (string elem in meshDict.Keys)
                                if (meshDict[elem].Contains(mesh))
                                    elementsHighlighted.Add(elem);

                            break;
                        }
                    }
                }

            // Now highlight everything that we found
            if (pointFound)
            {
                HighlightElements(elementsHighlighted);
                HighlightMeshes(meshesHighlighted);
            }
        }

        /// <summary>
        /// Give all the indicated meshes the pen thickness of WireHighlightThickness
        /// </summary>
        /// <param name="meshes"></param>
        private void HighlightMeshes(List<int> meshes)
        {
            foreach (int mesh in meshes)
                foreach (GeometryDrawing line in meshesDrawn[mesh])
                    line.Pen.Thickness = HighlightLineThickness;
        }

        /// <summary>
        /// Give all the indicated elements the pen thickness of WireHighlightThickness
        /// </summary>
        /// <param name="elements"></param>
        private void HighlightElements(List<string> elements)
        {
            foreach (string elementName in elements)
            {
                if (gatesDrawn.ContainsKey(elementName))
                    gatesDrawn[elementName].Pen.Thickness = HighlightLineThickness;
                else if (labels.ContainsKey(elementName))
                    labels[elementName].Pen.Thickness = HighlightLineThickness;
            }
        }

        #endregion

        private void CleanCircuit_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        #region Unused Code

        /// <summary>
        /// Special case for drawing NotBubbles, not currently used
        /// </summary>
        /// <param name="point"></param>
        /// <param name="numInputs"></param>
        private void DrawNotBubble(Point point, int numInputs, double gateHeight)
        {
            if (numInputs <= 1)
                numInputs = 2;
            double diameter = Math.Min(gateHeight / numInputs, 5.0);//NotBubbleHeight);
            Rect bounds = new Rect(point.X, point.Y, diameter, diameter);
            gateDrawer.DrawGate(LogicDomain.NOTBUBBLE, bounds, true, false);
        }

        /// <summary>
        /// Returns the image for the supplied gate (not currently used)
        /// </summary>
        /// <param name="gate"></param>
        /// <param name="point"></param>
        private ImageDrawing GetGateImage(Gate gate, System.Windows.Point point)
        {
            // Create our image
            ImageDrawing gateImage = new ImageDrawing();
            string uriString = "Gate Images\\";
            uriString += gate.GateType;
            uriString += ".gif";
            gateImage.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + uriString));
            //gateImage.Rect = new Rect(point, new System.Windows.Size(GateWidth, GateHeight));

            return gateImage;
        }

        /// <summary>
        /// Calculates how many elements have been added to the generation, not currently used
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="genInputs"></param>
        /// <returns></returns>
        private int GetGenerationNumber(int generation, ref Dictionary<int, int> genInputs)
        {
            int GenNumber = 1;
            if (genInputs.ContainsKey(generation))
            {
                GenNumber = genInputs[generation] + 1;
                genInputs[generation] += 1;
            }
            else
                genInputs.Add(generation, 1);
            return GenNumber;
        }

        /// <summary>
        /// Traverses back to the inputs to see how many gates precede this gate (worst case), not currently used
        /// </summary>
        /// <param name="gate"></param>
        /// <returns></returns>
        private int FindGateGeneration(CircuitElement gate, string origGate)
        {
            int maxGateGen = 0;
            /*
            foreach (CircuitElement elem in gate.Inputs.Keys)
            {
                // Check to make sure we have not entered a loop
                if (elem.Name == origGate)
                    return -1;

                if (elem.Type == "Gate")// && ((Gate)elem).GateType != "NOTBUBBLE")
                {
                    int gen = FindGateGeneration((Gate)elem, origGate);
                    if (gen == -1)
                    {
                        // Do nothing
                    }
                    else if (gen + 1 > maxGateGen)
                        maxGateGen = gen + 1;
                }
            }
            */
            return maxGateGen;
        }

        /// <summary>
        /// Finds all circuit elements, not currently used
        /// </summary>
        public void initializeGraph(List<CircuitElement> Inputs, List<String> elementsSeen)
        {
            List<CircuitElement> parents = new List<CircuitElement>();

            foreach (CircuitElement elem in Inputs)
            {
                elementsSeen.Add(elem.Name);
                if (!circuitElements.ContainsKey(elem.Name))
                    circuitElements[elem.Name] = elem;

                // Make a list of all parents
                foreach (List<CircuitElement> list in elem.Outputs.Values)
                {
                    foreach (CircuitElement parent in list)
                        if (!parents.Contains(parent) && !elementsSeen.Contains(parent.Name) && !Inputs.Contains(parent))
                            parents.Add(parent);
                    // Update our values
                    initializeGraph(parents, elementsSeen);
                }
            }
        }

        #endregion

    }
}