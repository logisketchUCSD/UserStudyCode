using System;
using System.Collections.Generic;
using VerilogWriter;
using System.IO;
using Pins;
using CircuitRec;

namespace VerilogWriter
{
    
    /// <summary>
    /// Prepares circuitrec object for the verilogwriter
    /// </summary>
    public class VerilogWrite
    {
        CircuitRec.CircuitRec circuit;
        String filename;

        /// <summary>
        /// Constructor for VerilogWrite class
        /// </summary>
        /// <param name="circuit">CircuitRec object to be written to
        /// Verilog</param>
        /// <param name="filename">String containing the desired verilog filename</param>
        public VerilogWrite(CircuitRec.CircuitRec circuit, String filename)
    	{
            this.circuit = circuit;
            this.filename = filename;
        }

        /// <summary>
        /// Constructor for VerilogWrite class
        /// </summary>
        /// <param name="filename">String containing the desired verilog filename</param>
        public VerilogWrite(String filename)
        {
            this.circuit = null;
            this.filename = filename;
        }

        /// <summary>
        /// Constructor for VerilogWrite class
        /// </summary>
        public VerilogWrite()
        {
            this.circuit = null;
            this.filename = null;
        }
		
        /// <summary>
        /// Writes VerilogWrite object to a verilog file
        /// </summary>
        public Boolean write()
        {
            return write(this.circuit, this.filename);
        }

        /// <summary>
        /// Writes VerilogWrite object to a verilog file
        /// </summary>
        /// <param name="circuit">CircuitRec object to be written to
        /// Verilog</param>
        public Boolean write(CircuitRec.CircuitRec circuit)
        {
            return write(circuit, this.filename);
        }

        /// <summary>
        /// Writes VerilogWrite object to a verilog file
        /// </summary>
        /// <param name="circuit">CircuitRec object to be written to
        /// Verilog</param>
        /// <param name="filename">String containing the desired verilog filename</param>
        public Boolean write(CircuitRec.CircuitRec circuit, String filename)
        {
            String dir = Path.GetDirectoryName(filename);

            String modName = Path.GetFileNameWithoutExtension(filename);

            // Calls VerilogWriter to write the .v file

            // Module Name
            Module M = new Module(modName);

            // Inputs to Module
            List<Pin> inputs = new List<Pin>();
            List<Pin> outputs = new List<Pin>();
            List<Pin> wires = new List<Pin>();
            Pin temp;
            //Loops through each superwire and pulls out the inputs
            //and outputs
            foreach (Mesh new_wire in circuit.Meshes)
            {
                if (new_wire.IOType.Equals(WirePolarity.Input))
                {
                    temp = new Pin(PinPolarity.Input, new_wire.Name, new_wire.Bussize);
                    inputs.Add(temp);
                }
                else if (new_wire.IOType.Equals(WirePolarity.Output))
                {
                    temp = new Pin(PinPolarity.Ouput, new_wire.Name, new_wire.Bussize);
                    outputs.Add(temp);
                }
                else
                {
                    temp = new Pin(PinPolarity.Wire, new_wire.Name, new_wire.Bussize);
                    wires.Add(temp);
                }
            }
            
            M.addInputs(inputs);
            M.addOutputs(outputs);
            M.addWires(wires);
           
            // Outputs of Symbols
            String output_name = "";
            int symbolSize = 1;

            //Loops through each symbol, determines the superwire name
            //of the inputs and outputs and add them to the module
            foreach (BaseSymbol new_symb in circuit.Symbols)
            {
                List<Pin> symbInputs = new List<Pin>();
                List<Pin> symbOutputs = new List<Pin>();
                Pin temporary;

                //Look at the wires connected to the symbol
                foreach (Wire symb_wire in new_symb.InputWires)
                {
                    //Loop through each superwire attached to the gate to determine
                    //which the inputwire belongs to.  Add it to the gate's input
                    foreach (Mesh sup in new_symb.ConMeshes)
                    {
                        if (sup.AllConnectedWires.Contains(symb_wire))
                        {
                            temporary = new Pin(PinPolarity.Input, sup.Name, sup.Bussize);
                            symbInputs.Add(temporary);
                            break;
                        }
                    }
                }

                //Look at the output wires of the sympol
                foreach (Wire symb_wire in new_symb.OutputWires)
                {
                    //Figure out which superwire each output is attacked to and add it
                    //to the list of outputs
                    foreach (Mesh sup in new_symb.ConMeshes)
                    {
                        
                        if (sup.AllConnectedWires.Contains(symb_wire))
                        {
                            temporary = new Pin(PinPolarity.Ouput, sup.Name, sup.Bussize);
                            symbOutputs.Add(temporary);
                            break;
                        }
                    }
                }

                M.addGate(new_symb.Name, new_symb.SymbType, symbOutputs, symbInputs);
            }
            
            //Writes the verilog module
            return M.printModule(dir, modName);
        }

    }
     
}