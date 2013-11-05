using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitParser
{
    /// <summary>
    /// An abstract class for all circuit parts used in the
    /// circuit parser. Allows for specialization of different
    /// kinds of circuit parts, especially when it comes to the 
    /// nature of their input/output connections.
    /// </summary>
    public abstract class CircuitPart
    {
        /// <summary>
        /// All circuit parts are associated with a 
        /// shape in the sketch.
        /// </summary>
        protected Sketch.Shape _associatedShape;

        /// <summary>
        /// Get the associated shape.
        /// </summary>
        public Sketch.Shape Shape
        {
            get { return _associatedShape; }
        }

        /// <summary>
        /// Get the name of the Circuit Part, 
        /// which should be unique.
        /// </summary>
        public string Name
        {
            get { return _associatedShape.Name; }
        }

        /// <summary>
        /// Gets the bounds of the circuit part.
        /// </summary>
        public System.Windows.Rect Bounds
        {
            get { return _associatedShape.Bounds; }
        }

        /// <summary>
        /// Compares two wires on the basis of the Y value of the endpoint that connects to this gate
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int WireSort(WireMesh a, WireMesh b)
        {
            Sketch.EndPoint aEnd = Shape.ClosestEndpointFrom(a.Shape);
            Sketch.EndPoint bEnd = Shape.ClosestEndpointFrom(b.Shape);

            if (aEnd.Y < bEnd.Y) return -1;
            else if (aEnd.Y > bEnd.Y) return 1;
            return 0;
        }

    }

    /// <summary>
    /// An class to further differentiate circuit parts.
    /// Circuit components refer to gates, circuit inputs, and 
    /// circuit outputs. 
    /// 
    /// This class makes and holds connections for gates, inputs, and outputs.
    /// </summary>
    public abstract class CircuitComponent : CircuitPart
    {

        #region INTERNALS

        /// <summary>
        /// All the wires connected as inputs to the logic gate.
        /// </summary>
        protected List<WireMesh> _inputWires;

        /// <summary>
        /// The wires connected as an outputs.
        /// </summary>
        protected List<WireMesh> _outputWires;

        /// <summary>
        /// Logic gates or circuit components that are indirectly 
        /// connected as inputs through a single wire-mesh.
        /// 
        /// Assumption: no one will put CircuitOutputs in the
        /// list of input components.
        /// </summary>
        protected Dictionary<CircuitComponent, int> _inputComponents;

        /// <summary>
        /// Logic gates or circuit components that are indirectly 
        /// connected as outputs through a single wire-mesh.
        /// 
        /// Assumption: no one will put a CircuitInput in the list of
        /// outputs components.
        /// </summary>
        protected List<List<CircuitComponent>> _outputComponents;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructs a new, unconnected logic gate 
        /// associated with a given shape.
        /// </summary>
        /// <param name="shape">The shape associated
        /// with the new logic gate</param>
        public CircuitComponent(Sketch.Shape shape)
        {
            _associatedShape = shape;
            _inputWires = new List<WireMesh>();
            _outputWires = new List<WireMesh>();
            _inputComponents = new Dictionary<CircuitComponent, int>();
            _outputComponents = new List<List<CircuitComponent>>();
        }

        #endregion

        #region CONNECT CIRCUIT PARTS

        /// <summary>
        /// Connects a wire-mesh as an input to the invoking logic gate.
        /// </summary>
        /// <param name="wire">The wire-mesh to connect</param>
        public void ConnectInput(WireMesh wire)
        {
            // If we already connected to this wire, we're done!
            if (_inputWires.Contains(wire))
                return;

            _inputWires.Add(wire);
            return;
        }

        /// <summary>
        /// Sorts and connects all inputs
        /// </summary>
        /// <returns></returns>
        public void ConnectAllInputs()
        {
            _inputWires.Sort(WireSort);

            foreach (WireMesh wire in _inputWires)
            {
                wire.ConnectDependent(this);
            }
        }

        /// <summary>
        /// Connects a wire-mesh as the output to the invoking logic gate.
        /// </summary>
        /// <param name="wire">The wire-mesh to connect</param>
        public void ConnectOutput(WireMesh wire)
        {
            // If we're already connected to this wire, we're done!
            if (_outputWires.Contains(wire))
                return;

            _outputWires.Add(wire);
            return;
        }

        /// <summary>
        /// Sorts and connects all outputs
        /// </summary>
        /// <returns></returns>
        public void ConnectAllOutputs()
        {
            _outputWires.Sort(WireSort);

            foreach (WireMesh wire in _outputWires)
                wire.ConnectSource(this, _outputWires.IndexOf(wire));
        }

        /// <summary>
        /// Connects a given circuit component as an input to the 
        /// invoking logic gate.
        /// </summary>
        /// <param name="wire">The component to connect</param>
        public void ConnectInput(CircuitComponent component, int index)
        {
            // If we already made this connection, we're done!
            if (_inputComponents.ContainsKey(component))
                return;

            // Make the connection.
            _inputComponents.Add(component, index);

            // Ensure that the given component also has this connection.
            component.ConnectOutput(this, index);
        }

        /// <summary>
        /// Connects a circuit component to the output of the 
        /// invoking logic gate. In other words, the output of
        /// the invoking logic gate becomes directly tied with
        /// an input or value of a given circuit component.
        /// </summary>
        /// <param name="wire">The component to connect</param>
        public void ConnectOutput(CircuitComponent component, int index)
        {
            while (_outputComponents.Count < index + 1)
            {
                _outputComponents.Add(new List<CircuitComponent>());
            }

            // If we already made this connection, we're done!
            if (_outputComponents[index].Contains(component))
                return;

            // Make the connection.
            _outputComponents[index].Add(component);

            // Ensure that the given component also has this connection.
            component.ConnectInput(this, index);
        }

        #endregion

        #region GETTERS

        /// <summary>
        /// A way to retreive the type of gate.
        /// </summary>
        /// <example>"AND"</example>
        /// <example>"OR"</example>
        public Domain.ShapeType Type
        {
            get { return _associatedShape.Type; }
        }

        /// <summary>
        /// Get all the wires connected as inputs to the logic gate.
        /// </summary>
        public List<WireMesh> InputWires
        {
            get { return _inputWires; }
        }

        /// <summary>
        /// Get thewires connected to the gate's outputs.
        /// </summary>
        public List<WireMesh> OutputWires
        {
            get { return _outputWires; }
        }

        /// <summary>
        /// Get logic gates or circuit components that are indirectly 
        /// connected as inputs through a single wire-mesh.
        /// </summary>
        public Dictionary<CircuitComponent, int> InputComponents
        {
            get { return _inputComponents; }
        }

        /// <summary>
        /// Get logic gates or circuit components that are indirectly 
        /// connected as outputs through a single wire-mesh.
        /// </summary>
        public List<List<CircuitComponent>> OutputComponents
        {
            get { return _outputComponents; }
        }

        #endregion
    }
}

