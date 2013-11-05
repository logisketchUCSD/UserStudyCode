using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Ink;

using CircuitSimLib;
using Set;

namespace InkCircuit
{
    /// <summary>
    /// Maps the ink a user sees to the circuit that calculates everything
    /// in the background for circuit simulation.
    /// </summary>
    public class InkCircuit
    {
        #region INTERNALS

        /// <summary>
        /// The circuit, which does the calculations for circuit simulation
        /// in the background.
        /// </summary>
        private Circuit circuit;

        /// <summary>
        /// The dictionary mapping all strokes in the sketch (by Guid) to
        /// strokes on the ink canvas (by String).
        /// </summary>
        private Dictionary<Guid?, String> sketchStrokesToInkStrokes;

        /// <summary>
        /// The collection of strokes the user sees.
        /// </summary>
        private System.Windows.Ink.StrokeCollection inkStrokes;

        /// <summary>
        /// A mapping of which Ink Stroke IDs in the sketch panel
        /// correspond to which input names in the Circuit.
        /// </summary>
        private Dictionary<ListSet<String>, string> inputMapping;

        /// <summary>
        /// A mapping of which Ink Stroke IDs in the sketch panel
        /// correspond to which output names in the Circuit.
        /// </summary>
        private Dictionary<ListSet<String>, string> outputMapping;

        /// <summary>
        /// A mapping of which Ink Stroke IDs of wires in the sketch panel
        /// correspond to their input circuit element names in the Circuit.
        /// (Each wire should have exactly one gate or label as input that 
        /// determines whether it has a value of 0 or 1)
        /// </summary>
        private Dictionary<ListSet<String>, string> wireToInputGateMapping;


        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Default constructor: really should not be used.
        /// </summary>
        public InkCircuit()
        {
            // nothing for now
        }

        /// <summary>
        /// Constructor that takes in the mapping of sketch stroke Guids to 
        /// ink stroke String Ids. This is necessary to create the dictionaries
        /// of inputMapping, outputMapping, and wireToInputGateMapping.
        /// </summary>
        /// <param name="sketchToInk"></param>
        public InkCircuit(Dictionary<Guid?, String> sketchToInk)
        {
            sketchStrokesToInkStrokes = sketchToInk;
            circuit = new Circuit();
            inputMapping = new Dictionary<ListSet<string>, string>();
            outputMapping = new Dictionary<ListSet<string>, string>();
            wireToInputGateMapping = new Dictionary<ListSet<string>, string>();
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
            ListSet<String> strokeIDs = new ListSet<String>();
            foreach (Sketch.Substroke substroke in inputShape.Substrokes)
                strokeIDs.Add(sketchStrokesToInkStrokes[substroke.Id]);

            inputMapping.Add(strokeIDs, inputShape.Name);
        }

        /// <summary>
        /// Update the output mapping dictionary to reflect an output
        /// recently added to the circuit
        /// </summary>
        /// <param name="ouput">The shape (label) corresponding to the output</param>
        public void addOutput(Sketch.Shape outputShape)
        {
            ListSet<String> strokeIDs = new ListSet<string>();
            foreach (Sketch.Substroke substroke in outputShape.Substrokes)
                strokeIDs.Add(sketchStrokesToInkStrokes[substroke.Id]);

            outputMapping.Add(strokeIDs, outputShape.Name);
        }

        /// <summary>
        /// Update the wire-to-input-gate mapping dictionary to reflect
        /// the addition of a gate to the circuit
        /// </summary>
        /// <param name="gate">The gate that has been added to the circuit.</param>
        /// <param name="wire">The wire mesh connected to the gate output.</param>
        public void addWire(Sketch.Shape gate, Sketch.Shape wire)
        {
            ListSet<String> strokeIDs = new ListSet<string>();
            foreach (Sketch.Substroke substroke in wire.Substrokes)
                strokeIDs.Add(sketchStrokesToInkStrokes[substroke.Id]);

            wireToInputGateMapping.Add(strokeIDs, gate.Name);
        }

        #endregion

        #region GETTERS & SETTERS

        /// <summary>
        /// Gets the input mapping dictionary.
        /// </summary>
        public Dictionary<ListSet<String>, string> InputMapping
        {
            get { return inputMapping; }
        }

        /// <summary>
        /// Gets the output mapping dictionary.
        /// </summary>
        public Dictionary<ListSet<String>, string> OutputMapping
        {
            get { return outputMapping; }
        }

        /// <summary>
        /// Gets the wire to input gate mapping dictionary.
        /// </summary>
        public Dictionary<ListSet<String>, string> WireToInputGateMapping
        {
            get { return wireToInputGateMapping; }
        }

        /// <summary>
        /// Gets or sets the circuit.
        /// </summary>
        public Circuit Circuit
        {
            get { return circuit; }
            set { circuit = value; }
        }

        #endregion
    }
}
