Andrew Danowitz

ReadMe Includes:
gate.cs
module.cs
modulestruct.cs
TestBenchWriter.cs
VerilogWrite.cs

Gate:
---------
This is a class library, so it needs to be called from another program.  To use this, create a new Gate object.  The gate constructor requires gatename, a SymbolTypes gateType, outputs, inputs, and bussize

Gate Methods:
-----------------

printGate (String filepath, String filename):  Prints verilog describing gate at the end of the file at filepath\filename

basicGateWriter(String logicToken, StreamWriter sWriter):  Helper function for printGate class.  Takes in the verilog logic token used for basic Verilog gates (i.e. & for SymbolTypes.AND) and prints the gate Verilog to the sWriter file

basicInvGateWriter(String logicToken, StreamWriter sWriter):  Same as basicGateWriter except for inverting gates, such as nand, nor and xnor

printError(String msg):  Prints any given error message to the console

Externally Available Values:
---------------------------------
None

Known Bugs/Issues:
-------------------------
PrintGate can only print up to 4-wide multiplexers.  Also, there is currently no convention for telling which multiplexer wire(s) is the select.

Likely Modifications:
---------------------------

SymbolTypes:  To increase the range of symbols able to be recognized, new symbols must be added to the SymbolTypes enum in CircuitRec.csproj

printGate(String filepath, String filename):  To add support for printing new gates add a new "case" to the SymbolTypes switch method.  Each new case should check to ensure that the gate object is a valid instance of that gate type (i.e. correct number of inputs/outputs) and write the appropriate Verilog.  For simple gate symbols where Verilog is of the form assign y = A (string) B, the basicGateWriter can be used

-----------------------------------------------------------------------------------------------------------------------

Module:
-----------
This is a class library, so it needs to be called from another program.  To use this, create a new Module object.  The gate constructor requires a modulename.

Module Methods:
-----------------------
addGate(String gateName, SymbolTypes gateType, List<Pin> outs, List<Pin> ins):  Checks to see if the gate parameters are valid.  If so, a new gate is created and added to the internal gate list

removeGate(string gatename):  Attempts to remove the gate from the module gate list.  Returns true if remove succeeded

addOutputs(List<Pin> output):  Adds a list of output pins to the outputs list

addInputs(List<Pin> input):  Adds a list of input pins to the inputs list

addWires(List<Pin> wire):  Adds a list of wire pins to the wire list

removeInput(Pin pin):  Removes pin from the inputs list

removeOutput(Pin pin):  Removes pin from the outputs list

removeWire(Pin pin):  Removes wire from the wires list

printModule(String filepath, String filename):  Prints circuit module to the specified Verilog filename

moduleCheck():  Checks to make sure that the module has at least one input and output

printerror(String msg):  Writes specified error message to the cosole

Externally Available Values:
---------------------------------
moduleName (gettable, settable):  Returns the module name

outputPins (gettable):  Returns list of output pins

inputPins (gettable):  Returns list of input pins

wirePins (gettable):  Returns list of wire pins

Known Bugs/Issues:
-------------------------
None

Likely Modifications:
--------------------------------
None

-------------------------------------------------------------------------------------------------------------------

ModuleStruct:
------------------
This is a class library, so it needs to be called from another program.  To use this, create a new ModuleStruct object.  The ModuleStruct constructor takes a modulename, a list of all output pins, a list of all input pins and a list of all internal wires

ModuleStruct Methods:
----------------------------
printNewModule(String filepath, String filename):  Prints the header of a new Verilog circuit module

printWires(String filepath, String filename):  Prints out the list of module wires at the end of the specified file

Externally Available Values:
---------------------------------
None

Known Bugs/Issues:
------------------------
None

Likely Modifications:
---------------------------
None

------------------------------------------------------------------------------------------------------

TestBenchWriter:
---------------------
This is a class library, so it needs to be called from another program.  To use this, create a new TestBenchWriter object.  The constructor takes in a file path and a module containing the circuit inputs and outputs.  

TestBenchWriter Methods:
-------------------------------
writeBench():  Writes a verilog testbench for the object module

headerWrite(String dir):  Writes the default Verilog testbench header to dir\test.v

instantiate(String dir):  Writes the inputs, outputs, and expected outputs and instantiates the unit under test (assumed to be the object module) in verilog to the end of the file dir\test.v

initTest(String dir):  Writes the Verilog to enable testbench I/O.  Also writes the Verilog to read in the test vectors and assign them to the module variables

test(String dir, int tv_count):  Writes the Verilog to compare simulation results to expected results and to write any discrepancies to an external file

Externally Available Values:
---------------------------------
None

Known Bugs/Issues:
------------------------
If the users specifically uses the "auto_clock" variable name, "auto_clock" would not be removed from the list of inputs meaning testvector values would be assigned to it

Likely Modifications:
---------------------------
None

------------------------------------------------------------------------------------------------------------------------

VerilogWrite:
----------------
This is a class library, so it needs to be called from another program.  To use this, create a new VerilogWrite object.  The constructor takes in either a CircuitRec circuit and a filename, a filename, or no arguments.

VerilogWrite Methods:
---------------------------
write():  Calls write(CircuitRec circuit, String filename) using object internal variables

write(CircuitRec.CircuitRec circuit):  Calls write(CircuitRec circuit, String filename) using object internal filename

write(CircuitRec.CircuitRec circuit, String filename):  Converts circuit into a module and prints the corresponding Verilog file to the filename

Externally Available Values:
---------------------------------
None

Known Bugs/Issues:
------------------------
None

Likely Modifications:
---------------------------
None