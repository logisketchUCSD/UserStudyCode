using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CircuitParser
{

    public class CircuitInput : CircuitComponent
    {
        #region CONSTRUCTOR

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shape">The shape associated with
        /// the new CircuitInput</param>
        public CircuitInput(Sketch.Shape shape) : base(shape)
        {
            // Do nothing
        }

        #endregion

        #region CONNECT CIRCUIT PARTS

        /// <summary>
        /// Connects a wire-mesh to the invoking circuit input, making
        /// the input the input/source of the wire.
        /// </summary>
        /// <param name="wire">The wire to connect to</param>
        public void Connect(WireMesh wire)
        {
            // If we already connected this wire, we're done!
            if (_outputWires.Contains(wire))
                return;

            _outputWires.Add(wire);

            // Make sure the wire has the same connection
            wire.ConnectSource(this, 0);
        }

        #endregion
    }

    public class CircuitOutput : CircuitComponent
    {
        #region CONSTRUCTOR

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shape">The shape associated with
        /// the new CircuitOutput</param>
        public CircuitOutput(Sketch.Shape shape) : base(shape)
        {
            // Do Nothing
        }

        #endregion

        #region CONNECT CIRCUIT PARTS

        /// <summary>
        /// Connects a wire-mesh to the invoking circuit output, making
        /// the circuit output's value dependent on this wire.
        /// </summary>
        /// <param name="wire">The wire to connect</param>
        public void Connect(WireMesh wire)
        {
            // If we already connected to this wire, we're done!
            if (_inputWires.Contains(wire) || _outputWires.Contains(wire))
                return;

            // If this wire doesn't come from shomewhere, it comes from here.  Otherwise, it gives us value.
            if (!wire.HasSource)
            {
                _outputWires.Add(wire);
                wire.ConnectSource(this, 0);
            }
            else
            {
                _inputWires.Add(wire);
                wire.ConnectDependent(this);
            }
        }

        #endregion
    }
}