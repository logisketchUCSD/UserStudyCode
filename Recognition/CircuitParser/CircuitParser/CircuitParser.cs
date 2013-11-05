using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using SketchPanelLib;

namespace CircuitParser
{
    public class CircuitParser
    {
        #region INTERNALS

        /// <summary>
        /// Prints debug statements iff debug == true.
        /// </summary>
        private bool debug = false;

        /// <summary>
        /// Describes how components should be connected.
        /// </summary>
        ContextDomain.CircuitDomain _domain;

        /// <summary>
        /// A dictionary mapping substrokes to circuit elements
        /// </summary>
        /// <typeparam name="?"></typeparam>
        /// <typeparam name="LogicGate"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        Dictionary<Guid, CircuitPart> _substroke2CircuitMap;

        /// <summary>
        /// Maps components (gates and IOs) to names. Allows quick access to a particular component, using its name as a key.
        /// </summary>
        Dictionary<string, LogicGate> _logicGates;

        /// <summary>
        /// Maps wiremeshes to names. Allows quick access to a particular wire-mesh, using its name as a key.
        /// </summary>
        Dictionary<string, WireMesh> _wireMeshes;

        /// <summary>
        /// Maps circuit's inputs to names. Allows quick access to a particular circuit input by using its name as a key.
        /// </summary>
        Dictionary<string, CircuitInput> _circuitInputs;

        /// <summary>
        /// Maps circuit's outputs to names. Allows quick access to a particular circuit input by using its name as a key..
        /// </summary>
        Dictionary<string, CircuitOutput> _circuitOutputs;

        /// <summary>
        /// A mapping of all the shapes in the sketch to their names.
        /// This does not really need to be in this class, but is here
        /// because the SimulationManager uses it and I am running out
        /// of time to refactor the SimulationManager as well... so this
        /// is here to make things run for now.
        /// </summary>
        Dictionary<string, Sketch.Shape> _associatedSketchShapes;

        /// <summary>
        /// True if the sketch could be successfully parsed into a circuit.
        /// </summary>
        bool _successfulParse;

        /// <summary>
        /// A list of all the errors that arose when parsing.
        /// </summary>
        List<ParseError> _parseErrors;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Public constructor.
        /// </summary>
        public CircuitParser()
            : this(ContextDomain.CircuitDomain.GetInstance())
        {
        }

        /// <summary>
        /// Constructor for testing (pass in a fake domain).
        /// </summary>
        /// <param name="domain"></param>
        public CircuitParser(ContextDomain.CircuitDomain domain)
        {
            _domain = domain;
            resetCircuitModel();
        }

        #endregion

        #region MAIN METHOD

        /// <summary>
        /// Tries to parse a sketch into a circuit, and
        /// indicates whether or not it was successful.
        /// 
        /// The main functionality in circuit recognition
        /// here is that it both determines what are inputs
        /// and outputs on both the gate and circuit level,
        /// and organizes the results into something the 
        /// circuit can be built off of for simulation.
        /// </summary>
        /// <param name="sketch">The sketch to parse</param>
        /// <returns>True if parsing was a success, 
        /// false if there were errors</returns>
        public bool SuccessfullyParse(Sketch.Sketch sketch)
        {
            // We should have a cleared circuit model each time
            // we try to parse a sketch.
            resetCircuitModel();

            #region Check circuit domain
            
            string shouldNotWorkReason = "There are no shapes.";
            Sketch.Shape shouldNotWorkShape = null;
            if (sketch.Shapes.Length > 0)
            {
                foreach (Sketch.Shape shape in sketch.Shapes)
                {
                    if (!_domain.IsProperlyConnected(shape))
                    {
                        shouldNotWorkReason = "Shape " + shape.Name + " is not properly connected";
                        shouldNotWorkShape = shape;
                        break;
                    }
                }
            }
            #endregion

            // Check to make sure the sketch was valid in the first place.
            // If not, do not continue.
            _successfulParse = CheckSketch(sketch);
            if (!_successfulParse) return false;

            loadWiresAndGates(sketch);
            connectGatesToWires();
            loadCircuitIO(sketch);
            connectComponentsToComponents();

            // Make sure that the circuit we just made is valid
            _successfulParse = CheckCircuit();

            if (debug)
                printErrors();

            // This method call is here only to let the SimulationManager
            // have a mapping of shapes to their names. 
            mapShapesToNames(sketch);

            // Likewise, this method is here only for mesh highlighting 
            // to have a mapping of sketch strokes to circuit parts.
            mapStrokesToCircuitParts(sketch);

            return _successfulParse;
        }
        #endregion

        #region HELPERS

        #region Reset / initialize circuit model

        /// <summary>
        /// Resets all the data members associated with a model of the
        /// circuit, so we can be sure to start afresh each time we
        /// parse a sketch.
        /// </summary>
        private void resetCircuitModel()
        {
            _logicGates = new Dictionary<string, LogicGate>();
            _wireMeshes = new Dictionary<string, WireMesh>();
            _circuitInputs = new Dictionary<string, CircuitInput>();
            _circuitOutputs = new Dictionary<string, CircuitOutput>();
            _associatedSketchShapes = null;
            _substroke2CircuitMap = new Dictionary<Guid, CircuitPart>();

            _successfulParse = true;    // Innocent until proven guilty
            _parseErrors = new List<ParseError>();
        }

        #endregion

        #region Loading wires and gates

        /// <summary>
        /// Creates a circuit component for every Wire or Gate shape in 
        /// a given sketch to store in the invoking circuit parser. 
        /// Note that it does not add labels (circuit inputs/outputs)
        /// or anything else that's not a wire or gate.
        /// 
        /// Assumption: every shape in the sketch has a unique name.
        /// 
        /// Note: we can remove this assumption by using the shape ID
        /// (GUIDs) as keys in CircuitParser's dictionaries. We use 
        /// the shape names primarily because it helps with debugging,
        /// and that circuit elements are differentiated by their names
        /// later along the line so names should be unique anyway.
        /// </summary>
        /// <param name="sketch"></param>
        private void loadWiresAndGates(Sketch.Sketch sketch)
        {
            foreach (Sketch.Shape shape in sketch.Shapes)
            {
                if (Domain.LogicDomain.IsWire(shape.Type))
                {
                    WireMesh newWire = new WireMesh(shape);
                    _wireMeshes.Add(newWire.Name, newWire);
                    if (debug) Console.WriteLine("Found wire: " + newWire.Name);
                }
                else if (Domain.LogicDomain.IsGate(shape.Type))
                {
                    LogicGate newGate = new LogicGate(shape);
                    _logicGates.Add(newGate.Name, newGate);
                    if (debug) Console.WriteLine("Found gate: " + newGate.Name);
                }
            }
        }

        #endregion

        #region Connecting gates to wires

        /// <summary>
        /// Connects every logic gate in the circuit to its appropriate
        /// input and output wires.
        /// </summary>
        private void connectGatesToWires()
        {
            foreach (LogicGate gate in _logicGates.Values)
                connectWiresTo(gate);
        }


        /// <summary>
        /// Figures out everything a given logic gate should be 
        /// connected to, and connects them appropriately.
        /// 
        /// Assumption: Every wire-mesh has a unique name.
        /// 
        /// Note: this currently only deals with connections to wires.
        /// Hence, it does not make any connections for notbubbles yet.
        /// </summary>
        /// <param name="gate">The gate to connect</param>
        private void connectWiresTo(LogicGate gate)
        {
            // We keep track of what wire-meshes are inputs and outputs 
            // so we can do sanity checks before actually connecting 
            // them to the logic gate.
            List<string> inputWires = new List<string>();
            List<string> outputWires = new List<string>();

            int maxOutputs = LogicDomain.MaxOutputs(gate.Type);

            // Cycle through everything connected to the gate's associated
            // shape and categorize their connection type accordingly
            foreach (Sketch.Shape connectedShape in gate.Shape.ConnectedShapes)
            {
                if (!Domain.LogicDomain.IsWire(connectedShape.Type))
                {
                    throw new Exception("Gate " + gate + " was connected to non-wire shape " + connectedShape);
                }

                Sketch.EndPoint connectingEndpoint =
                    gate.Shape.ClosestEndpointFrom(connectedShape);

                if (gate.ShouldBeInput(connectingEndpoint))
                    inputWires.Add(connectedShape.Name);
                else
                    outputWires.Add(connectedShape.Name);
            }

            // If it looks like we mixed up the output and input wires,
            // swap them so they're more correct (this can happen if the
            // gate's orientation was recognized in the wrong direction)
            if ((outputWires.Count > maxOutputs) && (inputWires.Count <= maxOutputs))
                swap(ref outputWires, ref inputWires);

            // Make the connections.
            foreach (string wireName in inputWires)
            {
                WireMesh inputWire = _wireMeshes[wireName];
                gate.ConnectInput(inputWire);
            }
            foreach (string wireName in outputWires)
            {
                WireMesh outputWire = _wireMeshes[wireName];
                gate.ConnectOutput(outputWire);
            }
            gate.ConnectAllInputs();
            gate.ConnectAllOutputs();
        }


        /// <summary>
        /// Small helper method to swap two values.
        /// 
        /// If there's a standard swap method in C#, get rid of this 
        /// method and just use the standard one.
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        private void swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        #endregion

        #region Loading circuit inputs and outputs

        private void loadCircuitIO(Sketch.Sketch sketch)
        {
            foreach (Sketch.Shape shape in sketch.Shapes)
                if (shape.Type == LogicDomain.TEXT)
                    loadInputOrOutput(shape);
        }

        /// <summary>
        /// Determines whether or not a given shape is a circuit
        /// input, and creates and connects it accordingly.  
        /// Will ignore any text that is not connected to anything.
        /// 
        /// Assumptions: 
        ///   * The names of wire-meshes correspond directly to
        ///     their associated shapes.
        ///   * Wire-meshes all have unique names.
        ///   * The given shape is not a label connected to two wires.
        /// </summary>
        /// <param name="shape">The shape to analyze</param>
        /// <returns>True if loading was successful, false
        /// if the given shape cannot be recognized as an input 
        /// or output</returns>
        private void loadInputOrOutput(Sketch.Shape shape)
        {
            // Make sure we're actually dealing with something sensical.
            // Note: If there are no connected shapes, we don't make an input or output, it is just ignored in the circuit.
            if (shape.Type != LogicDomain.TEXT || shape.ConnectedShapes.Count == 0)
                return;

            // Retreive the wire connected to this label shape, while
            // also checking that the shape has the correct number and 
            // kinds of connections.
            List<WireMesh> connectedWires = new List<WireMesh>();
            
            // Assume that we have an input until we are told otherwise.
            bool input = true;

            foreach (Sketch.Shape connectedShape in shape.ConnectedShapes)
            {
                // Is this a wire?
                if (!LogicDomain.IsWire(connectedShape.Type)) continue;

                // Get the connected wire
                WireMesh connected = _wireMeshes[connectedShape.Name];

                // Have we already seen this?
                if (connectedWires.Contains(connected)) continue;
                connectedWires.Add(connected);

                // If we're dealing with any wire that already has a source, this will not be an input
                if (connected.HasSource)
                    input = false;
            }

            if (input)
            {
                CircuitInput newInput = new CircuitInput(shape);
                foreach (WireMesh wire in connectedWires)
                    newInput.Connect(wire);
                _circuitInputs.Add(newInput.Name, newInput);
            }
            else
            {
                CircuitOutput newOutput = new CircuitOutput(shape);
                foreach (WireMesh wire in connectedWires)
                    newOutput.Connect(wire);
                _circuitOutputs.Add(newOutput.Name, newOutput);
            }
        }

        #endregion

        #region Connect circuit components to circuit components

        /// <summary>
        /// Connects all the circuit components (logic gates, 
        /// circuit inputs, and circuit outputs) that share
        /// a wire-mesh to each other  
        /// </summary>
        private void connectComponentsToComponents()
        {
            foreach (WireMesh wireMesh in _wireMeshes.Values)
            {
                if (!wireMesh.HasSource)
                    continue;

                CircuitComponent wireInput = wireMesh.Source;
                foreach (CircuitComponent wireOutput in wireMesh.Dependents)
                    wireInput.ConnectOutput(wireOutput, wireMesh.SourceIndex);
            }
        }

        #endregion

        #region Organize / get results


        /// <summary>
        /// Gets a dictionary describing the circuit.
        /// Specifically, it generates a dictionary containing information
        /// about each logic gate in the circuit: its name, shape, inputs, and the output port it is connected to on each input.
        /// </summary>
        public Dictionary<string, Data.Pair<Sketch.Shape, Dictionary<string, int>>> CircuitStructure
        {
            get
            {
                Dictionary<string, Data.Pair<Sketch.Shape, Dictionary<string, int>>> circuitStructure =
                    new Dictionary<string, Data.Pair<Sketch.Shape, Dictionary<string, int>>>();

                foreach (LogicGate gate in _logicGates.Values)
                {
                    Dictionary<string, int> inputComponents = new Dictionary<string, int>();
                    foreach (KeyValuePair<CircuitComponent, int> component in gate.InputComponents)
                        inputComponents.Add(component.Key.Name, component.Value);

                    Data.Pair<Sketch.Shape, Dictionary<string, int>> thisGate = new Data.Pair<Sketch.Shape, Dictionary<string, int>>(gate.Shape, inputComponents);

                    circuitStructure.Add(gate.Name, thisGate);
                }

                return circuitStructure;
            }
        }

        /// <summary>
        /// Gets a list of all the logic gates in the circuit model.
        /// </summary>
        public List<LogicGate> LogicGates
        {
            get
            {
                List<LogicGate> logicGates = new List<LogicGate>();
                foreach (LogicGate gate in _logicGates.Values)
                    logicGates.Add(gate);
                return logicGates;
            }
        }

        /// <summary>
        /// Gets a list of all the wire-meshes in the circuit model.
        /// </summary>
        public List<WireMesh> WireMeshes
        {
            get
            {
                List<WireMesh> wireMeshes = new List<WireMesh>();
                foreach (WireMesh wire in _wireMeshes.Values)
                    wireMeshes.Add(wire);
                return wireMeshes;
            }
        }

        public Dictionary<string, CircuitInput> CircuitInputsDict
        {
            get { return _circuitInputs; }
        }

        public Dictionary<string, CircuitOutput> CircuitOutputsDict
        {
            get { return _circuitOutputs; }
        }

        /// <summary>
        /// Gets a list of all the circuit input names and their associated shapes
        /// </summary>
        public Dictionary<string, Sketch.Shape> CircuitInputs
        {
            get
            {
                Dictionary<string, Sketch.Shape> circuitInputs = new Dictionary<string, Sketch.Shape>();

                foreach (string inputName in _circuitInputs.Keys)
                    circuitInputs[inputName] = _circuitInputs[inputName].Shape;

                return circuitInputs;
            }
        }

        /// <summary>
        /// Gets a list of all the circuit output names, each
        /// paired with its shape and the output it is connected to.
        /// </summary>
        public Dictionary<string, Data.Pair<Sketch.Shape, Data.Pair<string, int>>> CircuitOutputs
        {
            get
            {
                // Initialize the list we're going to return.
                Dictionary<string, Data.Pair<Sketch.Shape, Data.Pair<string, int>>> circuitOutputs =
                    new Dictionary<string, Data.Pair<Sketch.Shape, Data.Pair<string, int>>>();

                // Fill the list up with all the desired information
                foreach (CircuitOutput output in _circuitOutputs.Values)
                {
                    Data.Pair<Sketch.Shape, Data.Pair<string, int>> outputInfo = 
                        new Data.Pair<Sketch.Shape, Data.Pair<string, int>>(output.Shape, 
                            new Data.Pair<string, int>(output.InputWires[0].Source.Name, output.InputWires[0].SourceIndex));

                    circuitOutputs.Add(output.Name, outputInfo);
                }

                return circuitOutputs;
            }
        }

        /// <summary>
        /// Returns all components in LogicGates, CircuitOutputs, and CircuitInputs
        /// </summary>
        private Dictionary<string, CircuitComponent> CircuitComponents
        {
            get
            {
                Dictionary<string, CircuitComponent> circuitComponents = new Dictionary<string, CircuitComponent>();
                foreach (string gate in _logicGates.Keys)
                    circuitComponents.Add(gate, _logicGates[gate]);
                foreach (string gate in _circuitInputs.Keys)
                    circuitComponents.Add(gate, _circuitInputs[gate]);
                foreach (string gate in _circuitOutputs.Keys)
                    circuitComponents.Add(gate, _circuitOutputs[gate]);
                return circuitComponents;
            }
        }

        /// <summary>
        /// Gets a mapping of all the shapes in the sketch to their names.
        /// </summary>
        public Dictionary<string, Sketch.Shape> ShapeNames
        {
            get { return _associatedSketchShapes; }
        }

        /// <summary>
        /// Gets a mapping of all substrokes in the sketch to their circuit elements
        /// </summary>
        public Dictionary<Guid, CircuitPart> Substroke2CircuitMap
        {
            get
            {
                return _substroke2CircuitMap;
            }
        }

        /// <summary>
        /// Loads all the shapes from a sketch into the associated sketch 
        /// shapes data member. 
        /// 
        /// This method is currently only used by the getter "ShapeNames," 
        /// which gets only used in SimulationManager (as of July 2010). 
        /// Once/if SimulationManager is refactored to not need "ShapeNames"
        /// anymore, just get rid of this and the associated methods/data
        /// members because it's unecessary clutter.
        /// </summary>
        /// <param name="sketch"></param>
        private void mapShapesToNames(Sketch.Sketch sketch)
        {
            _associatedSketchShapes = new Dictionary<string, Sketch.Shape>();
            foreach (Sketch.Shape shape in sketch.Shapes)
                _associatedSketchShapes.Add(shape.Name, shape);
        }

        /// <summary>
        /// Creates the dictionary of substrokes to circuit parts needed for mesh highlighting
        /// </summary>
        /// <param name="sketch"></param>
        private void mapStrokesToCircuitParts(Sketch.Sketch sketch)
        {
            _substroke2CircuitMap = new Dictionary<Guid, CircuitPart>();

            foreach (CircuitInput input in _circuitInputs.Values)
                foreach (Sketch.Substroke sub in input.Shape.Substrokes)
                    _substroke2CircuitMap.Add(sub.Id, input);

            foreach (CircuitOutput output in _circuitOutputs.Values)
                foreach (Sketch.Substroke sub in output.Shape.Substrokes)
                    _substroke2CircuitMap.Add(sub.Id, output);

            foreach (LogicGate gate in _logicGates.Values)
                foreach (Sketch.Substroke sub in gate.Shape.Substrokes)
                    if (!_substroke2CircuitMap.ContainsKey(sub.Id))
                        _substroke2CircuitMap.Add(sub.Id, gate);

            foreach (WireMesh wire in _wireMeshes.Values)
                foreach (Sketch.Substroke sub in wire.Shape.Substrokes)
                    _substroke2CircuitMap.Add(sub.Id, wire);
        }

        #endregion

        #region Handling errors

        /// <summary>
        /// Gets a list of the errors that arose in 
        /// parsing a circuit.
        /// </summary>
        public List<ParseError> ParseErrors
        {
            get { return _parseErrors; }
        }

        /// <summary>
        /// Prints errors from parsing the circuit.
        /// </summary>
        private void printErrors()
        {
            if (_parseErrors.Count == 0)
                return;

            Console.WriteLine("The following errors arose in parsing the circuit:");
            foreach (ParseError error in _parseErrors)
            {
                Console.WriteLine(error.Explanation);
                Sketch.Shape errShape = error.Where;
            }

            Console.WriteLine();
        }


        #endregion

        #region Checking Circuit & Sketch Validity

        /// <summary>
        /// Checks whether the circuit is valid, returns a bool indicating this.  
        /// Adds any errors it comes across to the _parseErrors list.
        /// 
        /// Looks for:
        ///    * Each wire has one source and at least one output
        ///    * No wire outputs to an input and no output is a wire's input
        ///    * Each gate has an allowable number of inputs and outputs
        ///    * Each circuit element and wire have reciprocal connections
        ///    * Each input and output are connected to something
        /// </summary>
        /// <returns>A bool, which is true if the circuit is valid</returns>
        private bool CheckCircuit()
        {
            // Assume the circuit is fine, decide otherwise later.
            bool valid = true;

            #region Check Wire Connections
            // Each wire should have one input and at least one output
            foreach (WireMesh mesh in WireMeshes)
            {
                // Do the connections exist?
                if (!mesh.HasSource)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The wire " + mesh.Name + " is missing a source.", 
                        "This wire does not have a source.  Try dragging its red endpoints to connect it to something.", mesh.Shape));
                }
                /*
                // This is actually ok right now, outputs can be drawn in the middle of wires to give intermediary values.
                else if (mesh.Source is CircuitOutput)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The wire " + mesh.Name + "'s source is an output, " + mesh.Source.Name + ".",
                        "This wire's source is an output, " + mesh.Source.Name + ".", mesh.Shape));
                }
                */
                if (mesh.Dependents.Count < 1)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The wire " + mesh.Name + " has no dependents.", 
                        "Nothing uses this wire's value! Please connect it to something by dragging its endpoints.", mesh.Shape));
                }
                foreach (CircuitComponent connected in mesh.Dependents)
                    if (connected is CircuitInput)
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The wire " + mesh.Name + " is giving a value to the input" + connected.Name + ".",
                            "This wire is giving a value to the input" + connected.Name + ".", mesh.Shape));
                    }
            }
            #endregion

            #region Check Gate Connections
            // Each gate should have the correct number of inputs and outputs
            foreach (LogicGate gate in LogicGates)
            {                
                Utilities.IntRange inputs = _domain.NumberInputs(gate.Type);
                Utilities.IntRange outputs = _domain.NumberOutputs(gate.Type);
                // Get the minimum and maximum input and output numbers

                // Check input numbers
                if (gate.InputWires.Count < inputs.Min)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The gate " + gate.Name + " is missing some inputs.  It needs at least " + inputs.Min + " but has " + gate.InputWires.Count + ".", 
                        "This gate is missing some inputs.  It needs at least " + inputs.Min + " but has " + gate.InputWires.Count + ".", gate.Shape));
                }
                else if (gate.InputWires.Count > inputs.Max)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The gate " + gate.Name + " has too many inputs.  It needs at most " + inputs.Max + " but has " + gate.InputWires.Count + ".",
                        "This gate has too many inputs.  It needs at most " + inputs.Max + " but has " + gate.InputWires.Count + ".", gate.Shape));
                }

                // Check input connections
                foreach (WireMesh wire in gate.InputWires)
                {
                    if (!wire.Dependents.Contains(gate))
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The gate " + gate.Name + " is connected to the wire " + wire.Name + ", but that wire is not connected to the gate!",
                           "The gate " + gate.Name + " is connected to the wire " + wire.Name + ", but that wire is not connected to the gate!", wire.Shape));
                    }
                }

                // Check output numbers
                if (gate.OutputWires.Count < outputs.Min)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The gate " + gate.Name + " is missing some outputs.  It needs at least " + outputs.Min + " but has " + gate.OutputWires.Count + ".",
                        "This gate is missing some outputs.  It needs at least " + outputs.Min + " but has " + gate.OutputWires.Count + ".", gate.Shape));
                }
                else if (gate.OutputWires.Count > outputs.Max)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The gate " + gate.Name + " has too many outputs.  It needs at most " + outputs.Max + " but has " + gate.OutputWires.Count + ".",
                        "This gate has too many outputs.  It needs at most " + outputs.Max + " but has " + gate.OutputWires.Count + ".", gate.Shape));
                }

                // Check output connections
                foreach (WireMesh wire in gate.OutputWires)
                {
                    if (wire.Source != gate)
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The gate " + gate.Name + " is connected to the wire " + wire.Name + ", but that wire is not connected to the gate!",
                           "The gate " + gate.Name + " is connected to the wire " + wire.Name + ", but that wire is not connected to the gate!", wire.Shape));
                    }
                }
            }
            #endregion

            #region Check Input/Output connections
            // Each input should be connected correctly
            foreach (CircuitInput input in CircuitInputsDict.Values)
            {
                if (input.OutputWires.Count == 0)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The input " + input.Name + " is not connected to anything.",
                        "This input (" + input.Name + ") is not connected to anything.", input.Shape));
                }
                else
                {
                    foreach (WireMesh wire in input.OutputWires)
                        if (wire.Source != input)
                        {
                            valid = false;
                            _parseErrors.Add(new ParseError("The input " + input.Name + " is connected to the wire " + wire.Name + ", but that wire does not get it's value from the input!",
                                "This input (" + input.Name + ") is connected to the wire " + wire.Name + ", but that wire does not get it's value from thr input!", input.Shape));
                        }
                }
                if (input.InputWires.Count > 0)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The input " + input.Name + " connected as an output.",
                        "This input (" + input.Name + ") is connected as an output.", input.Shape));
                }
            }

            // Each output should be connected correctly
            foreach (CircuitOutput output in CircuitOutputsDict.Values)
            {
                if (output.InputWires.Count == 0)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The output " + output.Name + " is not connected to anything.",
                        "This output (" + output.Name + ") is not connected to anything.", output.Shape));
                }
                else if (output.InputWires.Count > 1)
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The output " + output.Name + " gets a value from more than one wire.",
                        "This output (" + output.Name + ") gets a value from more than one wire.", output.Shape));
                }
                else if (!output.InputWires[0].Dependents.Contains(output))
                {
                    valid = false;
                    _parseErrors.Add(new ParseError("The output " + output.Name + " is connected to the wire " + output.InputWires[0].Name + ", but that wire does not give it's value to the output!",
                        "This output (" + output.Name + ") is connected to the wire " + output.InputWires[0].Name + ", but that wire does not give it's value to the output!", output.Shape));
                }
                foreach (WireMesh wire in output.OutputWires)
                {
                    if (wire.Source != output)
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The output " + output.Name + " is connected to the wire " + wire.Name + ", but that wire does not get it's value from the output!",
                                "This output (" + output.Name + ") is connected to the wire " + wire.Name + ", but that wire does not get it's value from the output!", output.Shape));
                    }
                }
            }
            #endregion

            return valid;
        }

        /// <summary>
        /// check to see if if the inputs and outputs for any subcircuits are consistent, and raise
        /// ParseErrors if they are not
        /// </summary>
        /// <param name="sketchpanel"></param>
        /// <returns></returns>
        public bool checkSubCircuits(CircuitSimLib.Circuit Circuit)
        {
            if (Circuit == null)
                return false;
            bool valid = true;
            foreach (CircuitSimLib.Gate gate in Circuit.AllGates)
            {
                if (gate.GateType == LogicDomain.SUBCIRCUIT)
                {
                    CircuitSimLib.SubCircuit subCircuit = ((CircuitSimLib.SubCircuit)gate);
                    if (gate.Inputs.Count != subCircuit.inputsRequired)
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The Subcircuit " + gate.Name + " needs " + subCircuit.inputsRequired + " inputs, but has " + gate.Inputs.Count,
                             "The Subcircuit " + gate.Name + " needs " + subCircuit.inputsRequired + " inputs, but has " + gate.Inputs.Count, subCircuit.myShape));
                    }
                    if (gate.Outputs.Count != subCircuit.outputsRequired)
                    {
                        valid = false;
                        _parseErrors.Add(new ParseError("The Subcircuit " + gate.Name + " needs " + subCircuit.outputsRequired + " outputs, but has " + gate.Outputs.Count,
                             "The Subcircuit " + gate.Name + " needs " + subCircuit.outputsRequired + " outputs, but has " + gate.Outputs.Count, subCircuit.myShape));
                    }

                }
            }
            return valid;
        }

        /// <summary>
        /// Checks whether the sketch is valid, and returns a bool indicating this.
        /// Adds any errors it comes across to the _parseErrors list.
        /// 
        /// Looks for:
        ///    * Basic consistency via sketch.CheckConsistency
        ///    * Gates connected to only wires
        ///    * Wires not connected to wires
        ///    * Text connected to at most one wire
        /// </summary>
        /// <param name="sketch">The sketch to check the validity of</param>
        /// <returns>A bool, which is true if the sketch is valid</returns>
        private bool CheckSketch(Sketch.Sketch sketch)
        {
            bool valid = true;

            // Check basic sketch consistency, will (rightly) throw an exception if it finds something wrong.
            // Eventually we should not need this, for the sketch should always pass this, but it's a useful check for now.
            sketch.CheckConsistency();

            foreach (Sketch.Shape shape in sketch.Shapes)
            {
                #region Gate Checks
                if (LogicDomain.IsGate(shape.Type))
                {
                    // Each gate should be connected to only wires
                    foreach (Sketch.Shape connected in shape.ConnectedShapes)
                    {
                        if (!LogicDomain.IsWire(connected.Type))
                        {
                            valid = false;
                            _parseErrors.Add(new ParseError("Gate " + shape.Name + " is connected to something that is not a wire, " + connected.Name + "of type " + connected.Type.Name,
                                "This gate is connected to something that is not a wire.  Draw wires between them or group them together.", shape));
                        }
                    }
                }
                #endregion

                #region Wire Checks
                else if (LogicDomain.IsWire(shape.Type))
                {
                    // Wires shouldn't be connected to other wires
                    foreach (Sketch.Shape connected in shape.ConnectedShapes)
                    {
                        if (shape != connected && LogicDomain.IsWire(connected.Type))
                        {
                            valid = false;
                            _parseErrors.Add(new ParseError("Wire " + shape.Name + " is connected to another wire, " + connected.Name, 
                                "This wire is connected to another wire.  Try grouping these wires together.", shape));
                        }
                    }
                }
                #endregion

                #region Input/Output Checks
                else if (LogicDomain.IsText(shape.Type))
                {
                    // Text should only be connected to wires, if anything.
                    foreach (Sketch.Shape wire in shape.ConnectedShapes)
                        if (!LogicDomain.IsWire(wire.Type))
                        {
                            valid = false;
                            _parseErrors.Add(new ParseError("Text " + shape.Name + " should only be connected to a wire, but is connected to a " + wire.Type.Name + ".",
                                shape.Name + " should only be connected to a wire, but is connected to a " + wire.Type.Name + ".", shape));
                        }
                }
                #endregion
            }

            return valid;
        }

        #endregion

        #endregion
    }
}