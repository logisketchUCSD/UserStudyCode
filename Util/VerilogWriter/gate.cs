using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Pins;
using CircuitRec;

namespace VerilogWriter
{
    /// <summary>
    /// gate objects contain all the information for each gate, its name, type, 
    /// and its inputs, output, and bus size.  Gates have one output, but any 
    /// number of inputs.  valid gate types are defined by the enum Gates.  Each
    /// current valid gate can only have one output.
    /// </summary>
    public class Gate
	{
        private SymbolTypes gateType;
		private List<Pin> outputs;
		private List<Pin> inputs;
		private string gatename;
		private int bussize;
       
        /// <summary>
        /// Constructor for gate class
        /// </summary>
        /// <param name="gatename">String containing the gate name</param>
        /// <param name="gatetype">Gates Enum containing gatetype</param>
        /// <param name="outputs">List of pins containing gate outputs</param>
        /// <param name="inputs">List of pins containing gate inputs</param>
        /// <param name="bussize">Int containing gate bussize</param>
		public Gate(String gatename,SymbolTypes gateType, List<Pin> outputs, List<Pin> inputs,int bussize)
		{
			this.gateType = gateType;
			this.outputs = outputs;
			this.inputs = inputs;
			this.gatename = gatename;
			this.bussize = bussize;
		}

		/// <summary>
		/// Prints the verilog code for each gate
		/// </summary>
		/// <param name="filepath">Desired file directory</param>
		/// <param name="filename">Desired file name</param>
		/// <returns>Boolean indicating whether print succeeded</returns>
		public Boolean printGate(String filepath, String filename)
		{
            //if there are no inputs or outputs to the gate, return false
            if (inputs.Count == 0 || outputs.Count == 0)
                return false;

            //set up file write
            FileStream inputfile = new FileStream(filename + ".v", FileMode.Append, FileAccess.Write);
            StreamWriter sWriter = new StreamWriter(inputfile);

            //Switch to handle printing different gate types
			switch(gateType)
			{
                //for simple gates and their inverses, write gate, if write fails, return false
				case SymbolTypes.AND:
                    if(!basicGateWriter("&", sWriter))
                        return false;
                    break;
				
                case SymbolTypes.OR:
                    if(!basicGateWriter("|", sWriter))
                        return false;
                    break;

				case SymbolTypes.XOR:
                    if(!basicGateWriter("^", sWriter))
                        return false;
                    break;

				case SymbolTypes.XNOR:
                    if(!basicInvGateWriter("^", sWriter))
                        return false;
                    break;

				case SymbolTypes.NAND:
                    if(!basicInvGateWriter("&", sWriter))
                        return false;
                    break;

				case SymbolTypes.NOR:
                    if(!basicInvGateWriter("|", sWriter))
                        return false;
                    break;

                //write not gate
				case SymbolTypes.NOT:
					//assume only one input for NOT gate
                    if ((inputs.Count == 1) && (outputs.Count == 1))
                        sWriter.WriteLine("assign {0} = ~{1};", outputs[0].PinName, inputs[0].PinName);
                    //if more than one input, write error and return false
                    else
                    {
                        //This error shows only if there are more than one inputs to the NOT gate
                        printError("Error: NOT gate may only recieve single input.");
                        sWriter.Close();
                        return false;
                    }
                    break;

                //writes the mux
				case SymbolTypes.MUX:
					if(inputs.Count == 3)//if 2 input mux
					{
						sWriter.WriteLine("assign {0} = {1} ? {2} : {3};"
							,outputs[0].PinName,inputs[0].PinName,inputs[2].PinName,inputs[1].PinName);
					}
					else if(inputs.Count == 4)//if 3 input mux
					{
						sWriter.WriteLine("assign #1 {0} = {1}[1] ? {2} : ({1}[0] ? {3} : {4};"
									,outputs[0].PinName,inputs[0].PinName,inputs[3].PinName,inputs[2].PinName,inputs[1].PinName);
					}
					else if(inputs.Count == 5)//4 input mux
					{
						sWriter.WriteLine("always @( * )");
						sWriter.WriteLine("case({0})",inputs[0]);
						sWriter.WriteLine("	2'b00: {0} <= {1};",outputs[0].PinName,inputs[1].PinName);
						sWriter.WriteLine("	2'b01: {0} <= {1};",outputs[0].PinName,inputs[2].PinName);
						sWriter.WriteLine("	2'b10: {0} <= {1};",outputs[0].PinName,inputs[3].PinName);
						sWriter.WriteLine("	2'b11: {0} <= {1};",outputs[0].PinName,inputs[4].PinName);
						sWriter.WriteLine("endcase");
					}
					else //if invalid # of inputs print error, return false
					{
						printError("Error: Only 2,3, and 4 input muxes are currently supported");
                        sWriter.Close();
                        return false;
					}
                    break;
					
                //writes flipflop
				case SymbolTypes.FLIPFLOP:
                    //if the flipflop has 1 input
                    if (inputs.Count == 1)
                    {
                        sWriter.WriteLine("always @(posedge clk, posedge reset)");
                        sWriter.WriteLine("if(reset)		{0} <= 0;", inputs[0].PinName);
                        sWriter.WriteLine("else			{0} <= {1};"
                            , inputs[0].PinName, outputs[0].PinName);
                    }
                    else
                    {
                        //This error shows only if there are more than one inputs 
                        //(not counting clock and reset) to the flop gate.
                        printError("Error: FlipFlop only takes one input");
                        sWriter.Close();
                        return false;
                    }
                    break;

                //writes flop enable
				case SymbolTypes.FLIPFLOPEN:
                    if (inputs.Count == 2)
                    {
                        sWriter.WriteLine("always @(posedge clk, posedge reset)");
                        sWriter.WriteLine("if(reset)		{0} <= 0;", inputs[1]);
                        sWriter.WriteLine("elseif({0})		{1} <= {2};"
                            , inputs[0], inputs[1], outputs[0].PinName);
                    }
                    else
                    {
                        //This error shows only if there are more than one inputs 
                        //(not counting clock and reset) to the flop gate.
                        printError("Error: Enabled flipflop only takes one input and one enable");
                    }
                    break;

                //Return false
				default:
                    sWriter.Close();
                    return false;
					
			}//end switch

            //close the streamWriter
			sWriter.Close();

            //return true
            return true;
		}//end printgate()

        /// <summary>
        /// Verilog printing method for simple logic gates
        /// </summary>
        /// <param name="logicToken">String containing Verilog logic symbol</param>
        /// <param name="sWriter">StreamWriter for the file where this should be written</param>
        /// <returns>Boolean indicating whether gate write was successful</returns>
        private Boolean basicGateWriter(String logicToken, StreamWriter sWriter)
        {
            int i = 0;

            //if there are other than 1 outputs, print an error, close the stream and
            //return false
            if (outputs.Count != 1)
            {
                printError("Error:  Gate may only have single output");
                sWriter.Close();
                return false;
            }

            //Write the left half of the assignment
            sWriter.Write("assign {0} = ", outputs[0].PinName);

            //Go through each input pin
            foreach (Pin datain in inputs)
            {
                //if its the first pin, just write out its name
                if (i==0)
                    sWriter.Write(datain.PinName);
                //otherwise, write the logicToken, then the pin name
                else
                    sWriter.Write(" " + logicToken + " " + datain.PinName);
                i++;
            }
            sWriter.WriteLine(";");

            //Write succeeded
            return true;
        }

        /// <summary>
        /// Printer method for the inverse of simple logic gates
        /// </summary>
        /// <param name="logicToken">String containing Verilog logic symbol</param>
        /// <param name="sWriter">StreamWriter for the file where this should be written</param>
        /// <returns>Boolean indicating whether gate write was successful</returns>
        private Boolean basicInvGateWriter(String logicToken, StreamWriter sWriter)
        {
            int i = 0;

            //if there are other than 1 outputs, print an error, close the stream and
            //return false
            if (outputs.Count != 1)
            {
                printError("Error:  Gate may only have single output");
                sWriter.Close();
                return false;
            }

            //Write the left half of the assignment
            sWriter.Write("assign {0} = ", outputs[0].PinName);
            sWriter.Write("~(");

            //Go through each input pin
            foreach (Pin datain in inputs)
            {
                //if its the first pin, just write out its name
                if (i==0)
                    sWriter.Write(datain.PinName);
                //otherwise, write the logicToken, then the pin name
                else
                    sWriter.Write(" " + logicToken + " " + datain.PinName);
                i++;
            }
            sWriter.WriteLine(");");
            
            //write succeeded
            return true;
        }

        /// <summary>
        /// Prints an error message to the console
        /// </summary>
        /// <param name="msg">String containing error message</param>
        private void printError(String msg)
        {
            Console.WriteLine(msg);
        }
	}//end class gate
}

