Andrew Danowitz

WorkPath:
---------------
This is a class library, so it needs to be called from another program.  To use this, create a new WorkPath object.  There are a number of WorkPath constructors that require various combinations of:  verilog filename, Xilinx project path, sketch, truthtable pins (tv), and CircuitRec circuit.  

WorkPath Methods:
-----------------------
Go(Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circ):  Checks to see if the object 	has filename and projectname then calls Go(projectPath, filename, sktch, tv, circuit)

 Go(List<List<Pin>> tv, CircuitRec.CircuitRec circuit):  Calls Go(this.sketch, tv, circuit)

Go(String projname, String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec 	circuit):  Calls path(projname, filename, sktch, tv, circuit)

Go(String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circuit):  Checks to 	see if the object contains a valid project path and calls Go(projectPath, filename, sktch, tv, 	circuit)

path(String projFile, String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec 	circuit):  Checks that the projFile and filename are valid, rectifies the truthtable and circuit 	pins, generates circuit Verilog, test bench, and test vector file.  Then calls XilinxRun and 	simulate

errors(String filename):  Reads in errors from the testbench error.log file and returns them as a list of 	pins

error(String str):  Takes in a String and a pin containing the the name, value and expected value (if 	applicable) of the simulation error listed in the string

input(String str):  Same as error(String str) except specifically used for input errors

findSub(String str, Char termChar):  Finds the zero-based index of a desired terminal character in 	string str

XMLWrite(String filename, Sketch.Sketch sketch):  Writes the sketch to a specified filename

simulate(String filename, String projFile):  Generates custom Modelsim .fdo script and .udo script.  	Runs modelsim on .fdo script

getProj(String filename):  Checks filename directory for Xilinx project files.  If only one exists, this 	project file is used for project path, if zero or more than one exist, the user is prompted to 	select a file

createProj():  Prompts user for project file name and calls createProj(projFile, false)

createProj(String projFile, bool writeCheck):  Performs write-checks if enabled, deletes project to be 	overwritten, generates and executes a Xilinx tcl script to create the new project file

getFile(String filter, String iniDir):  Prompts the user for a file via an open file dialog.  Allowable 	filetypes specified by filter and the default file dialog directory is included in iniDir

saveFile(String filter, String dirPath):  Same as getFile except uses a saveFile dialog

mod2pin(Module mod):  Returns a list of pins containing module inputs and outputs

pins2mod(List<Pin> pins, String modName):  Converts a list of input and output pins to a module

pins2testVectors(List<List<Pin>> pins):  Calls pins2testVectors(pins, this.filePath)

pins2testVectors(List<List<Pin>> pins, String this.filePath):  Takes in a 2-dimensional pin list and 	prints out a file of test vectors to be used by a self-checking test bench

inOutRectify(List<Pin> tv, List<Mesh> circ):  Gets the filePath and calls inOutRectify(tv, circ, filename)

inOutRectify(List<Pin> tv, List<Mesh> circ, String filename):  Attempts to rectify inputs/outputs 	between truthtable and circuit.  Gets list of input/output pins from circ Mesh, removes the 	clock pin, and checks to ensure that both have the same number of labels.  Then goes 	through and attempts to matchup the inputs/outputs from the lists via pin name, bus size 	and polarity.  Also has a "flipped" feature designed to detect whether all of the input/output 	polarities have been flipped.  Uses the rectified pin list to write the testbench and returns the 	rectified pin order

indexFind(List<Pin> tv, Pin instance):  Finds the zero-based index of a Pin in a list.  Pin equality is 	determined by name and bus size only.  If pin is not present in list, the returned index will 	be greater than (List.count-1)

MeshToPins(List<Mesh> wires):  Converts a list of meshes into a list of input/output pins

numCol(CircuitRec.CircuitRec circ):  Given a circuit recognition result, returns the expected number of 	truthtable columns

createFile(String sketchProj, String projF, String circSketch, String truthSketch, String noteSketch, String verilog):  Writes all project file names to a "sketchProj" file to enable the eventual implementation of a Sketch GUI project file

readFile(String sketchProj, out String projF, out String circkSketch, out String truthSketch, out String noteSketch, out String verilog):  Reads in all project file names from a "sketchProj" file

Externally Available Values:
---------------------------------
None

Known Bugs/Issues:
-------------------------
None

Likely Modifications:
--------------------------------
In order to automatically read in simulation errors after simulation, change simulate(String filename, String projFile) to List<Pin> type and end the method with "return errors(filename)"