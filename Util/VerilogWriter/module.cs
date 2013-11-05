using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using Pins;
using CircuitRec;

namespace VerilogWriter
{
	/// <summary>
	/// This class is designed to store the inputs, outputs, gates, and internal wires that
    /// constitute a recognized circuit.  This representation is solely used for the
    /// purposes of generating verilog
	/// </summary>
	public class Module
	{
		private String modulename;
		private Dictionary<String, Gate> gateData;//Hashtable gatedata contains all gate objects with corresponding names.
        private PinList inputs;
        private PinList outputs;
        private PinList wires;
        private List<String> gateOrder;//gateorder keeps track of the order in which gates were added.
		
		/// <summary>
		/// Constructor for the module class
		/// </summary>
		/// <param name="modulename">A string holding the name of a circuit</param>
		public Module(string modulename)
		{
			this.modulename = modulename;
            this.inputs = new PinList();
            this.outputs = new PinList();
            this.wires = new PinList();
            this.gateData = new Dictionary<string, Gate>();
            this.gateOrder = new List<string>();
		}

		//Use addgate() with gatename, gatetype, a single output, and a List of inputs.
		//to add a gate to the module.  Valid gatetypes are "and","or","xor","nand","nor","xnor","not", and is not case sensitive.
		//The "not" gate may only recieve one input.
		/// <summary>
		/// This method adds an individual gate to the circuit
		/// </summary>
		/// <param name="gateName">String containing the unique gate name</param>
		/// <param name="gateType">Gate.Gates enum containing gate type</param>
        /// <param name="outs">List of Pin.Pins containing the outputs of the gate</param>
        /// <param name="ins">List of Pin.Pins containing the inputs to the gate</param>
		/// <returns>Boolean indicating whether the gate was successfully added</returns>
        public Boolean addGate(string gateName, SymbolTypes gateType, List<Pin> outs, List<Pin> ins)
		{
            //if the gate is invalidate, return false
            if (gateName == null || outs == null || ins == null)
                return false;

            //log the gatename
            gateOrder.Add(gateName);

            //Construct the gate and add it to the dictionary
			Gate newGate = new Gate(gateName, gateType, outs, ins, outs[0].bussize);
			gateData.Add(gateName,newGate);

            return true;
		}

		/// <summary>
		/// Method removes a gate from the module
		/// </summary>
		/// <param name="gatename">String containing the unique gate name</param>
		/// <returns>Boolean indicating whether remove succeeded</returns>
		public Boolean removeGate(string gatename)
		{
            //if the gate isn't in the dictionary, return false
            if (!gateData.ContainsKey(gatename))
                return false;
            
            //otherwise, remove the gate from the dictionary and list of gates
            gateData.Remove(gatename);
            gateOrder.Remove(gatename);
            return true;
		}

		/// <summary>
		/// Method adds a range of output pins to the module outputs.
		/// </summary>
		/// <param name="output">List of output pins to be added</param>
		public void addOutputs(List<Pin> output)
		{
			this.outputs.AddRange(output);
		}
        /// <summary>
        /// Method adds a range of input type pins to the module outputs.
        /// </summary>
        /// <param name="input">List of input pins to be added</param>
		public void addInputs(List<Pin> input)
		{
            this.inputs.AddRange(input);
		}
        /// <summary>
        /// Method adds a range of wire type pins to the module .
        /// </summary>
        /// <param name="wire">List of wire pins to be added</param>
        public void addWires(List<Pin> wire)
        {
            this.wires.AddRange(wire);
        }

		//removeoutput() and removeinput() remove single inputs and outputs by name
		/// <summary>
		/// Removes specified input pin from input list
		/// </summary>
		/// <param name="pin">Pin.Pin to be removed from inputs</param>
		/// <returns>A boolean indicating whether remove was successful</returns>
        public Boolean removeInput(Pin pin)
		{
            return inputs.removePin(pin);
        }
        
        /// <summary>
        /// Removes a pin from the output list
        /// </summary>
        /// <param name="pin">Name of pin to be removed</param>
        /// <returns>Boolean indicating whether remove was successful</returns>
        public Boolean removeOutput(Pin pin)
        {
            return outputs.removePin(pin);
		}

        /// <summary>
        /// Removes a Pin.Pin from the wires list
        /// </summary>
        /// <param name="pin">Pin.Pin to be removed</param>
        /// <returns>Boolean indicating whether remove was successful</returns>
        public Boolean removeWire(Pin pin)
        {
            return wires.removePin(pin);
        }

		//printmodule() prints the Verilog based on existing gate objects
		//and the input and output lists.
        /// <summary>
        /// Prints the module to a verilog file
        /// </summary>
        /// <param name="filepath">String containing the full desired directory path</param>
        /// <param name="filename">String containing the desired filename</param>
        /// <returns></returns>
		public Boolean printModule(String filepath,String filename)
		{
            //If the module has both inputs and outputs
            if (moduleCheck())
            {
                ModuleStruct newMod = new ModuleStruct(modulename, outputs.Pins, inputs.Pins, wires.Pins);
                
                //Print the new module, if printNewModule fails, return false
                if (!newMod.printNewModule(filepath, filename))
                    return false;

                //Print the wires
                newMod.printwires(filepath, filename);//insert wire declaration printing

                //go through each gate in the dictionary
                foreach (string s in gateOrder)
                {
                    Gate g = gateData[s];

                    //Print the gate.  If pring fails, return false
                    if (!g.printGate(filepath, filename))
                        return false;
                }

                //print the end of the module
                newMod.printend(filepath, filename);
            }

            //if the module has no inputs or outputs, return false
            else
            {
                printerror("No inputs or outputs");
                return false;
                
            }

            //if the method gets to here, it succeeded
            return true;
		}

		//modulecheck() checks if the output or input list are empty,
		//returns false if either is empty, true otherwise.
		public bool moduleCheck()
		{
            //if there are no outputs or inputs, return false
			if(outputs.Count == 0 || inputs.Count == 0)
				return false;
			return true;
		}
		
        /// <summary>
        /// Writes an error method to the console
        /// </summary>
        /// <param name="msg">String containing error message</param>
		public void printerror(String msg)
		{
			Console.WriteLine(msg);
        }

        #region GET

        /// <summary>
        /// Returns the name of the module
        /// </summary>
        public String moduleName
        {
            get
            {
                return this.modulename;
            }
            set
            {
                this.modulename = value;
            }
        }

        /// <summary>
        /// Returns a List of the output pins
        /// </summary>
        public List<Pin> outputPins
        {
            get
            {
                return this.outputs.Pins;
            }
        }

        /// <summary>
        /// Returns a list of the input pins
        /// </summary>
        public List<Pin> inputPins
        {
            get
            {
                return this.inputs.Pins;
            }
        }

        /// <summary>
        /// Returns a list of wire pins
        /// </summary>
        public List<Pin> wirePins
        {
            get
            {
                return this.wires.Pins;
            }
        }

        #endregion
    }//end class module
}

