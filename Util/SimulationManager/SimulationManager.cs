using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Ink;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Domain;
using CircuitSimLib;
using SketchPanelLib;
using Sketch;
using System.Windows.Media;
using Data;
using InkToSketchWPF;
using CircuitParser;

namespace SimulationManager
{
    /// <summary>
    /// Event handler for telling when the truth table is closed.
    /// </summary>
    public delegate void TruthTableClosedHandler();

    /// <summary>
    /// Maps the ink a user sees to the circuit that calculates everything
    /// in the background for circuit simulation.
    /// </summary>
    public class SimulationManager
    {
        #region CONSTANTS

        private System.Windows.Media.Color ON_COLOR = System.Windows.Media.Colors.LightSkyBlue;
        private System.Windows.Media.Color OFF_COLOR = System.Windows.Media.Colors.MidnightBlue;
        private System.Windows.Media.Color ERROR_COLOR = System.Windows.Media.Colors.Red;

        #endregion

        #region INTERNALS

        /// <summary>
        /// The file path of the current directory.
        /// This is needed because circuitParser looks for 
        /// digital_domain.txt inside the specified directory.
        /// </summary>
        private string directory;

        /// <summary>
        /// The panel that the simulation manager is working on.
        /// </summary>
        private SketchPanel sketchPanel;

        /// <summary>
        /// Prints debugging statements if debug is set to true.
        /// </summary>
        private bool debug = false;

        /// <summary>
        /// A boolean indicating if the circuit is a valid circuit.
        /// A valid circuit has no wires connected to more than one output,
        /// and every gate has a valid input.
        /// </summary>
        private bool valid;

        /// <summary>
        /// The clean representation of the circuit.
        /// </summary>
        private CleanCircuit cleanCircuit;

        /// <summary>
        /// Makes sure we do not subscribe or unsubscribe when we shouldn't.
        /// </summary>
        private bool subscribed;

        /// <summary>
        /// Whether we are displaying a clean circuit or the sketch circuit
        /// </summary>
        private bool displayingClean;

        /// <summary>
        /// The window which displays the truth table
        /// </summary>
        private TruthTableWindow truthTableWindow;

        /// <summary>
        /// Lets us know when to uncheck the truth table checkbox
        /// </summary>
        public TruthTableClosedHandler TruthTableClosed;

        #region Circuit Related Internals

        /// <summary>
        /// A mapping of which Ink Stroke IDs in the sketch panel
        /// correspond to which input names in the Circuit.
        /// </summary>
        private Dictionary<string, ListSet<String>> inputMapping;

        /// <summary>
        /// A mapping of which Ink Stroke IDs in the sketch panel
        /// correspond to which output names in the Circuit.
        /// </summary>
        private Dictionary<string, ListSet<String>> outputMapping;

        /// <summary>
        /// A mapping of gates to the StrokeIDs of the wires connected to them, 
        /// organized by the index of the output they are connected to.
        /// </summary>
        private Dictionary<string, Dictionary<int, ListSet<String>>> wireToInputGateMapping;

        /// <summary>
        /// The circuit recognizer that allows us to get the inputs, 
        /// outputs, and structure of the circuit
        /// </summary>
        private CircuitParser.CircuitParser circuitParser;

        #endregion

        #region Input/Output toggle related

        private CircuitValuePopups circuitValuePopups;

        private Dictionary<string, System.Windows.Point> oldPopupLocations;

        #endregion

        #endregion

        #region CONSTRUCTOR & INITIALIZERS

        /// <summary>
        /// Constructor for the simulation manager.
        /// </summary>
        /// <param name="panel">The panel that this manager controls</param>
        public SimulationManager(ref SketchPanel panel)
        {
            sketchPanel = panel;
            directory = AppDomain.CurrentDomain.BaseDirectory;

            // Circuit related initializations
            inputMapping = new Dictionary<string,ListSet<String>>();
            outputMapping = new Dictionary<string, ListSet<String>>();
            wireToInputGateMapping = new Dictionary<string, Dictionary<int, ListSet<string>>>();
        }

        #endregion

        #region Subscription

        /// <summary>
        /// Subscribes to panel- brings up popups and truth table for simulation
        /// </summary>
        public void SubscribeToPanel()
        {
            if (subscribed)
                return;
            subscribed = true;

            // Prepare to receive events from toggles
            MakeNewToggles();

            simulateCircuit();
        }

        public void UnsubscribeFromPanel()
        {
            if (!subscribed)
                return;
            subscribed = false;

            if (truthTableWindow != null && truthTableWindow.IsActive)
                truthTableWindow.Close();

            // Unsubsribe Popups
            circuitValuePopups.SetInputEvent -= new SetInputEventHandler(setInputValue);
            circuitValuePopups.ClearAllToggles();
        }

        #endregion

        #region Getters & Setters

        public bool Valid
        {
            get { return valid; }
        }

        public bool Subscribed
        {
            get { return subscribed; }
        }

        public CircuitParser.CircuitParser CircuitParser
        {
            get { return circuitParser; }
        }

        public bool DisplayingClean
        {
            get { return displayingClean; }
            set { displayingClean = value; }
        }

        private Sketch.Sketch sketch
        {
            get { return sketchPanel.InkSketch.Sketch; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates SimulationManager, sets the sketchPanel's circuit, and creates the clean circuit.
        /// </summary>
        private CircuitSimLib.Circuit createCircuitAndCleanCircuit(CircuitParser.CircuitParser parser)
        {
            CircuitSimLib.Circuit circuit = createCircuit(parser);
            sketchPanel.Circuit = circuit;
            cleanCircuit = new CleanCircuit(ref sketchPanel, false);
            return circuit;
        }

        /// <summary>
        /// Updates SimulationManager's dictionaries and returns the circuit produced from the parser.
        /// </summary>
        private Circuit createCircuit(CircuitParser.CircuitParser parser)
        {
            // Reset Dictionaries
            inputMapping.Clear();
            outputMapping.Clear();
            wireToInputGateMapping.Clear();

            // Update the mappings with the circuit inputs.
            foreach (KeyValuePair<string, Shape> input in parser.CircuitInputs)
                addInput(parser.ShapeNames[input.Key]);

            // Update the mappings with the circuit outputs.
            foreach (string output in circuitParser.CircuitOutputs.Keys)
                addOutput(parser.ShapeNames[output]);

            // Update the mappings with meshes and their input components
            foreach (CircuitParser.WireMesh wire in parser.WireMeshes)
            {
                if (wire.HasSource)
                    addWire(wire);
            }

            return new Circuit(parser.CircuitStructure, parser.CircuitOutputs, parser.CircuitInputs);
        }

        /// <summary>
        /// Sets all the inputs to the circuit to 0 and computes
        /// the values for all the other parts of the circuit.
        /// </summary>
        public void simulateCircuit()
        {
            // Set all the inputs to have the value 0
            foreach (string inputName in inputMapping.Keys)
                sketchPanel.Circuit.setInputValue(inputName, 0);

            calculateOutputs();
            colorWires();
        }

        /// <summary>
        /// Color wires according to whether they have a value of 0 or 1.
        /// </summary>
        public void colorWires()
        {
            if (cleanCircuit != null)
                cleanCircuit.UpdateWires();

            foreach (string gateName in wireToInputGateMapping.Keys)
            {
                foreach (int index in wireToInputGateMapping[gateName].Keys)
                {
                    int value = sketchPanel.Circuit.gateValue(gateName, index);
                    System.Windows.Media.Color wireColor = ERROR_COLOR;
                    if (value == 1)
                        wireColor = ON_COLOR;
                    else if (value == 0)
                        wireColor = OFF_COLOR;

                    ListSet<string> wire = wireToInputGateMapping[gateName][index];
                    foreach (String wireStroke in wire)
                        sketchPanel.InkSketch.GetInkStrokeById(wireStroke).DrawingAttributes.Color = wireColor;
                }
            }
        }

        /// <summary>
        /// Recognize a circuit after the sketch has been labeled.
        /// </summary>
        public bool recognizeCircuit()
        {
            circuitParser = new CircuitParser.CircuitParser();
            return recognizeCircuit(circuitParser);
        }

        public bool recognizeCircuit(CircuitParser.CircuitParser parser)
        {
            if (debug) Console.WriteLine("========== Circuit Parsing Has Begun! ==========");
            valid = false;

            // Determine the circuit structure and trigger event for feedback mechanisms
            valid = (parser.SuccessfullyParse(sketch));

            if (!valid)
            {
                if (debug) 
                    Console.WriteLine("The circuit parser could not parse the circuit.");
                return false;
            }

            CircuitSimLib.Circuit circuit = createCircuitAndCleanCircuit(parser);
            valid = parser.checkSubCircuits(circuit);

            if (!valid)
            {
                if (debug)
                    Console.WriteLine("SimulationManager.makeCircuit() failed.");
                return false;
            }

            return valid;
        }


        /// <summary>
        /// Displays the clean version of the circuit on the given Canvas.
        /// </summary>
        /// <param name="cleanCanvas"></param>
        public void DisplayCleanCircuit(DockPanel dock)
        {
            dock.Children.Remove(this.cleanCircuit);
            
            // Otherwise create the circuit and set the layout
            cleanCircuit = new CleanCircuit(ref sketchPanel, true);
            dock.Children.Add(this.cleanCircuit);

            cleanCircuit.Width = dock.ActualWidth;
            cleanCircuit.Height = dock.ActualHeight;

            displayingClean = true;
            cleanCircuit.EditingMode = InkCanvasEditingMode.None;
            cleanCircuit.EditingModeInverted = InkCanvasEditingMode.None;

            MakeNewToggles();
        }

        /// <summary>
        /// Removes the clean circuit
        /// </summary>
        public void RemoveCleanCircuit(DockPanel dock)
        {
            dock.Children.Remove(this.cleanCircuit);
            displayingClean = false;

            MakeNewToggles();
        }

        /// <summary>
        /// Remakes all the input/ouput popups
        /// </summary>
        public void MakeNewToggles()
        {
            if (circuitValuePopups == null || !circuitValuePopups.HasPopups)
            {
                circuitValuePopups = new CircuitValuePopups(inputMapping, sketchPanel, outputMapping);
                oldPopupLocations = circuitValuePopups.Locations;
                circuitValuePopups.SetInputEvent += new SetInputEventHandler(setInputValue);
            }

            if (displayingClean)
            {
                circuitValuePopups.Locations = cleanCircuit.LabelLocations;
                circuitValuePopups.MakeNewToggles(cleanCircuit);
            }
            else
            {
                circuitValuePopups.Locations = oldPopupLocations;
                circuitValuePopups.MakeNewToggles(sketchPanel.InkCanvas);
            }

        }

        /// <summary>
        /// Closes all toggles
        /// </summary>
        public void HideToggles()
        {
            if (circuitValuePopups != null)
                circuitValuePopups.HideToggles();
        }

        /// <summary>
        /// Opens all toggles
        /// </summary>
        public void ShowToggles()
        {
            if (circuitValuePopups == null)
                MakeNewToggles();

            if (debug) Console.WriteLine("Displaying toggles");
            circuitValuePopups.ShowToggles();
        }

        /// <summary>
        /// The cleaned circuit
        /// </summary>
        public CleanCircuit CleanCircuit
        {
            get { return cleanCircuit; }
        }

        #endregion

        #region UPDATE DICTIONARIES

        /// <summary>
        /// Update the input mapping dictionary to reflect an input
        /// recently added to the circuit
        /// </summary>
        /// <param name="input">The shape (label) corresponding to the input</param>
        public void addInput(Sketch.Shape inputShape)
        {
            if (debug)
                Console.WriteLine("Trying to add " + inputShape.Type.Name + " called " + inputShape.Name + " to inputs");
               
            ListSet<String> strokeIDs = new ListSet<String>();
            foreach (Sketch.Substroke substroke in inputShape.Substrokes)
                strokeIDs.Add(sketchPanel.InkSketch.GetInkStrokeIDBySubstroke(substroke));

            if (!inputMapping.ContainsKey(inputShape.Name))
                inputMapping.Add(inputShape.Name,strokeIDs);
        }

        /// <summary>
        /// Update the output mapping dictionary to reflect an output
        /// recently added to the circuit
        /// </summary>
        /// <param name="ouput">The shape (label) corresponding to the output</param>
        public void addOutput(Sketch.Shape outputShape)
        {
            if (debug)
                Console.WriteLine("Trying to add " + outputShape.Type + " called " + outputShape.Name + " to outputs");
             
            ListSet<String> strokeIDs = new ListSet<string>();
            foreach (Sketch.Substroke substroke in outputShape.Substrokes)
                strokeIDs.Add(sketchPanel.InkSketch.GetInkStrokeIDBySubstroke(substroke));

            if(!outputMapping.ContainsKey(outputShape.Name))
                outputMapping.Add(outputShape.Name, strokeIDs);
        }

        /// <summary>
        /// Update the wire-to-input-gate mapping dictionary to reflect
        /// the addition of a gate to the circuit
        /// </summary>
        /// <param name="gate">The gate that has been added to the circuit.</param>
        /// <param name="wire">The wire mesh connected to the gate output.</param>
        public void addWire(WireMesh wireMesh)
        {
            if (debug)
                Console.WriteLine("Trying to add " + wireMesh.Shape.Type + " called " + wireMesh.Name + " to wires of " + wireMesh.Source.Shape.Type);

            if(!wireToInputGateMapping.ContainsKey(wireMesh.Source.Name))
                wireToInputGateMapping.Add(wireMesh.Source.Name, new Dictionary<int, ListSet<String>>());
            if (!wireToInputGateMapping[wireMesh.Source.Name].ContainsKey(wireMesh.SourceIndex))
                wireToInputGateMapping[wireMesh.Source.Name][wireMesh.SourceIndex] = new ListSet<string>();
            foreach (Sketch.Substroke substroke in wireMesh.Shape.Substrokes)
                wireToInputGateMapping[wireMesh.Source.Name][wireMesh.SourceIndex].Add(sketchPanel.InkSketch.GetInkStrokeIDBySubstroke(substroke));
        }

        #endregion

        #region Input/Output Functions

        /// <summary>
        /// Change the value of a specified input in the circuit
        /// and update the ink colors accordingly
        /// </summary>
        /// <param name="name">the name of the input used in the circuit</param>
        /// <param name="value">the value (0 or 1) to set it to</param>
        private void setInputValue(string inputName, int value)
        {
            sketchPanel.Circuit.setInputValue(inputName, value);
            calculateOutputs();
            colorWires();
            if (displayingClean)
                ShowToggles();
        }

        /// <summary>
        /// Calculates the values (0's or 1's) across the entire circuit
        /// and updates the display accordingly
        /// </summary>
        private void calculateOutputs()
        {
            sketchPanel.Circuit.calculateOutputs();
            foreach (string outputName in outputMapping.Keys)
            {
                int value = sketchPanel.Circuit.gateValue(outputName, 0);
                circuitValuePopups.SetPopup(outputName, value, false);
            }
        }

        #endregion

        #region Truth Table

        /// <summary>
        /// Brings up the truth table window and hooks into truth table events
        /// </summary>
        public void DisplayTruthTable()
        {
            TruthTable truthTable = new TruthTable(sketchPanel.Circuit);
            truthTableWindow = new TruthTableWindow(truthTable.TruthTableHeader, truthTable.TruthTableOutputs, sketchPanel.Circuit.NumInputs);
            truthTableWindow.Closed +=new EventHandler(truthTableWindow_Closed);
            
            truthTableWindow.Show();
            truthTableWindow.SimulateRow += new RowHighlightEventHandler(truthTableWindow_SimulateRow);
            truthTableWindow.Highlight += new HighlightEventHandler(truthTableWindow_HighlightLabel);
            truthTableWindow.UnHighlight += new UnhighlightEventHandler(truthTableWindow_UnHighlightLabel);
            truthTableWindow.RelabelStrokes += new RelabelStrokesEventHandler(truthTableWindow_RelabelStrokes);
        }

        public void CloseTruthTable()
        {
            if (truthTableWindow != null && truthTableWindow.IsEnabled)
            {
                truthTableWindow.SimulateRow -= new RowHighlightEventHandler(truthTableWindow_SimulateRow);
                truthTableWindow.Close();
            }
        }


        /// <summary>
        /// Sets input values to those of highlighted row in truth table
        /// </summary>
        /// <param name="inputs">List of input values</param>
        private void truthTableWindow_SimulateRow(Dictionary<string, int> inputs)
        {
            foreach (string i in inputs.Keys)
            {
                sketchPanel.Circuit.setInputValue(i, inputs[i]);

                circuitValuePopups.SetPopup(i, inputs[i], true);
            }
            calculateOutputs();
            colorWires();
        }


        /// <summary>
        /// Highlights the corresponding strokes to the input
        /// </summary>
        /// <param name="name"></param>
        private void truthTableWindow_HighlightLabel(string name)
        {
            // If it is not in the dictionary - return

            ListSet<string> strokeSet = new ListSet<string>();
            if (inputMapping.ContainsKey(name))
                strokeSet = inputMapping[name];
            else if (outputMapping.ContainsKey(name))
                strokeSet = outputMapping[name];
            else
                return;

            foreach (string s in strokeSet)
            {
                System.Windows.Ink.Stroke stroke = sketchPanel.InkSketch.GetInkStrokeById(s);
                stroke.DrawingAttributes.Color = Colors.DarkRed;
            }
        }

        /// <summary>
        /// Unhighlights the corresponding strokes to the input
        /// </summary>
        /// <param name="name"></param>
        private void truthTableWindow_UnHighlightLabel(string name)
        {
            // If it is not in the dictionary - return
            ListSet<string> strokeSet = new ListSet<string>();
            if (inputMapping.ContainsKey(name))
                strokeSet = inputMapping[name];
            else if (outputMapping.ContainsKey(name))
                strokeSet = outputMapping[name];
            else
                return;

            foreach (string s in strokeSet)
            {
                System.Windows.Ink.Stroke stroke = sketchPanel.InkSketch.GetInkStrokeById(s);
                Substroke sub = sketchPanel.InkSketch.GetSketchSubstrokeByInk(stroke);
                stroke.DrawingAttributes.Color = sub.Type.Color;
            }
        }

        /// <summary>
        /// Updates the dictionaries based on the new names of the inputs/outputs
        /// </summary>
        /// <param name="newNameDict">Dictionary of old name keys to new name values</param>
        private void truthTableWindow_RelabelStrokes(Dictionary<string, string> newNameDict)
        {
            foreach (string s in newNameDict.Keys)
            {
                // Change the shape names that were affected
                foreach (Shape shape in sketch.Shapes)
                {
                    if (shape.Name == s)
                        shape.Name = newNameDict[s];
                }

                // Update all the dictionaries
                if (inputMapping.ContainsKey(s))
                {
                    ListSet<string> values = inputMapping[s];
                    LabelShape(values, newNameDict[s]);
                    inputMapping.Remove(s);
                    inputMapping.Add(newNameDict[s], values);
                }
                else if (outputMapping.ContainsKey(s))
                {
                    ListSet<string> values = outputMapping[s];
                    LabelShape(values, newNameDict[s]);
                    outputMapping.Remove(s);
                    outputMapping.Add(newNameDict[s], values);
                }
                if (wireToInputGateMapping.ContainsKey(s))
                {
                    Dictionary<int, ListSet<string>> values = wireToInputGateMapping[s];
                    wireToInputGateMapping.Remove(s);
                    wireToInputGateMapping.Add(newNameDict[s], values);
                }
            }

            // Create our new Circuit
            if (!circuitParser.SuccessfullyParse(sketchPanel.Sketch)) return;
            sketchPanel.Circuit = new Circuit(circuitParser.CircuitStructure,
                                             circuitParser.CircuitOutputs,
                                             circuitParser.CircuitInputs);

            // Update popup names
            circuitValuePopups.UpdatePopupNames(newNameDict);
            cleanCircuit.UpdateLabel(newNameDict);
            ShowToggles();
        }

        /// <summary>
        /// Event triggered when the truth table window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void truthTableWindow_Closed(object sender, EventArgs e)
        {
            if(TruthTableClosed != null)
                TruthTableClosed();
        }
        #endregion
        
        #region Helpers

        /// <summary>
        /// Finds the strokes of a list of stroke ids
        /// </summary>
        /// <param name="strokeIDs"></param>
        /// <returns></returns>
        private StrokeCollection GetStrokes(ListSet<string> strokeIDs)
        {
            StrokeCollection strokes = new StrokeCollection();
            foreach (string s in strokeIDs)
                strokes.Add(sketchPanel.InkSketch.GetInkStrokeById(s));

            return strokes;
        }

        /// <summary>
        /// Find the parent shape of a list of strokes
        /// </summary>
        /// <param name="strokeIds"></param>
        /// <returns></returns>
        private void LabelShape(ListSet<string> strokeIds, string name)
        {
            // Create list of all shapes
            List<Shape> shapes = new List<Shape>();
            foreach (string s in strokeIds)
            {
                Sketch.Substroke sub = sketchPanel.InkSketch.GetSketchSubstrokeByInkId(s);
                if (!shapes.Contains(sub.ParentShape))
                    shapes.Add(sub.ParentShape);
            }

            // Set the name to what the user specified
            foreach (Shape shape in shapes)
                shape.Name = name;
        }

        #endregion
    }
}