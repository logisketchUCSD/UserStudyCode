/*
 * File: WorkPath.cs
 * 
 * Author: Andrew Danowitz
 * Harvey Mudd College, Claremont, CA 91711
 * Sketchers 2007.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ConverterXML;
using System.Diagnostics;
using VerilogWriter;
using InkToSketchWPF;
using System.Collections.Generic;
using CircuitRec;
using Sketch;
using Pins;

namespace uiWorkPath
{
    /// <summary>
    /// The workpath class contains the necessary methods for
    /// interfacing the UI to Xilinx and Modelsim.  It also contains
    /// a number of helper functions designed to aid in user feedback
    /// and rectifying the outputs between the circuitrec and truthtable
    /// recognizers
    /// </summary>
    public class WorkPath
    {
        private String projectPath;
        private String filePath;
        private Sketch.Sketch sketch;
        
        //Frequently used allowed filetypes
        private String xmlFilt = "XML files (*.xml)|*.xml";
        private String iseFilt = "ISE Project Files (*.ise)|*.ise";
        private String verilogFilt = "Verilog files (*.v)|*.v";

        #region Constructors

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="projectPath">String containing full filename of Xilinx .ise
        /// project file</param>
        /// <param name="filePath">String containing full filename of the sketch</param>
        /// <param name="sketch">Sketch.Sketch object</param>
        public WorkPath(String projectPath, String filePath, Sketch.Sketch sketch)
        {
            this.filePath = filePath;
            this.sketch = sketch;
            this.projectPath = projectPath;
        }

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="projectPath">String containing full filename of Xilinx .ise
        /// project file</param>
        /// <param name="filePath">String containing full filename of sketch xml</param>
        public WorkPath(String projectPath, String filePath)
        {
            this.projectPath = projectPath;
            this.filePath = filePath;
            this.sketch = null;
        }

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="filePath">String containing full filename of sketch xml</param>
        /// <param name="sketch">Sketch.Sketch object</param>
        public WorkPath(String filePath, Sketch.Sketch sketch)
        {
            this.filePath = filePath;
            this.sketch = sketch;
            this.projectPath = null;
        }
        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="sketch">Sketch.Sketch object</param>
        public WorkPath(Sketch.Sketch sketch)
        {
            this.sketch = sketch;
            this.filePath = null;
            this.projectPath = null;
        }

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="projectPath">String containing file path of Xilinx .ise
        /// project file</param>
        public WorkPath(String projectPath)
        {
            this.projectPath = projectPath;
            this.filePath = null;
            this.sketch = null;
        }

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        public WorkPath()
        {
            this.filePath = null;
            this.sketch = null;
            this.projectPath = null;
        }

        /// <summary>
        /// Constructor for the WorkPath class
        /// </summary>
        /// <param name="projectPath">String containing full filename of Xilinx .ise
        /// project file</param>
        /// <param name="filePath">String containing full filename of sketch xml</param>
        /// <param name="ink">Microsoft.Ink.Ink object</param>
        public WorkPath(String projectPath, String filePath, Microsoft.Ink.Ink ink)
        {
            this.filePath = filePath;
            
            //converts ink to sketch
            ReadInk newSketch = new ReadInk(ink);
            this.sketch = newSketch.Sketch;
            this.projectPath = projectPath;
        }
        #endregion

        #region Workpath primers

        /// <summary>
        /// Public class used to enter the workpath
        /// </summary>
        /// <param name="sktch">Sketch.Sketch object</param>
        /// <param name="tv">List of Pin objects containing the inputs and outputs
        /// recognized by the truth table recognizer</param>
        /// <param name="superWire">List of Superwire objects of the recognized
        /// circuit</param>
        public bool Go(Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circ)
        {
            String pn = this.projectPath;
            String fn = this.filePath;

            //If the project path is null, get the project
            if (pn == null)
            {
                pn = getProj(fn);
                if (pn == null)
                    return false;
           }
           
            //If the xml filename is null, get the desired filename
           if (fn == null)
           {
                fn = saveFile(xmlFilt, Path.GetDirectoryName(pn));
                if (fn == null)
                    return false;
            }
            
            //otherwise execute workpath
            return path(pn, fn, sktch, tv, circ);
        }

        /// <summary>
        /// Public class used to enter the workpath
        /// </summary>
        /// <param name="tv">List of Pin objects containing the inputs and outputs
        /// recognized by the truth table recognizer</param>
        /// <param name="superWire">List of Superwire objects of the recognized
        /// circuit</param>
        public bool Go(List<List<Pin>> tv, CircuitRec.CircuitRec circuit)
        {
            return Go(this.sketch, tv, circuit);
        }

        /// <summary>
        /// Public class used to enter the workpath
        /// </summary>
        /// <param name="projname">String containing full filename of Xilinx .ise
        /// project file</param>
        /// <param name="filename">String containing sketch xml filename</param>
        /// <param name="sktch">Sketch.Sketch object</param>
        /// <param name="superWire">List of Superwire objects of the recognized
        /// circuit</param>
        /// <param name="tv">List of Pin objects containing the inputs and outputs
        /// recognized by the truth table recognizer</param>
        public bool Go(String projname, String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circuit)
        {
            return path(projname, filename, sktch, tv, circuit);
        }
        #endregion

        public bool Go(String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circuit)
        {
            String projName;

            if (projectPath == null)
            {
                projectPath = getProj(filename);
                if (projectPath == null)
                    return false;
            }

            return Go(projectPath, filename, sktch, tv, circuit);
        }

        #region Workpath

        /// <summary>
        /// Workpath method
        /// </summary>
        /// <param name="projFile">String containing full filepath of Xilinx .ise
        /// project file</param>
        /// <param name="filename">String containing sketch xml filename</param>
        /// <param name="sktch">Sketch.Sketch object</param>
        /// <param name="superWire">List of Superwire objects of the recognized
        /// circuit</param>
        /// <param name="tv">List of Pin objects containing the inputs and outputs
        /// recognized by the truth table recognizer</param>
        private bool path(String projFile, String filename, Sketch.Sketch sktch, List<List<Pin>> tv, CircuitRec.CircuitRec circuit)
        { 
            //In the off chance that either the passed in filename or project file
            //is null, simulation can not proceed (possibly unnecessary)
            if (projFile == null || filename == null)
                return false;
            
            //save the sketch as an xml file
            //XMLWrite(filename, sktch);

            //if rectify returns null, the truth table and circuitrec results could
            //not be rectified, so simulation can't proceed
            List<Mesh> superWire = new List<Mesh>();
            superWire.AddRange(circuit.Meshes);

            if(inOutRectify(tv[0], superWire, filename)==null)
                return false;

            tv.RemoveAt(0);

            VerilogWrite vw = new VerilogWrite(circuit, filename);
            
            pins2testVectors(tv, filename);
            
            //Execute Xilinx and Modelsim components of workflow
            XilinxRun(filename, projFile);
            simulate(filename, projFile);

            //if it gets to the end of the method, then simulation made it completely
            //through the workpath code
            return true;
        }

        #region Workpath Helpers

        /// <summary>
        /// Method reads in discrepancies between expected and simulated circuit results
        /// from the error.log file generated by the testbench
        /// </summary>
        /// <param name="filename">Contains the name of any file in the directory
        /// containing the error.log file</param>
        /// <returns>List of Pins containing the inputs and outputs that failed on
        /// simulation</returns>
        public List<Pin> errors(String filename)
        {
            List<Pin> errorList = new List<Pin>();
            
            String dir = Path.GetDirectoryName(filename);
            TextReader tr = new StreamReader(dir + "\\error.log");
            String newLine = tr.ReadLine();
            Pin P;

            //so long as the end of the file hasn't been reached,
            //keep checking for errors
            while (newLine != null)
            {
                P = error(newLine);
                
                //If there is actually an error, add it to the list
                if (P != null)
                {
                    errorList.Add(P);
                }

                //go to the next line of the program
                newLine = tr.ReadLine();
                
            }
            
            //return the list of errors
            return errorList;
        }

        /// <summary>
        /// Takes in a String and returns a list of pins corresponding to simulation
        /// errors as enumerated by the string.
        /// </summary>
        /// <param name="str">String to check for errors</param>
        /// <returns>List of pins containing errors in the string</returns>
        private Pin error(String str)
        {
            Pin temp;
            char[] results;
            char[] expected;
            int endIndex = 0;
            String pinName;

            //if the file line is long enough to be an output
            if (str.Length > 10)
            {
                //if it has the output format
                if (str.Substring(0, 10).Equals("Expected: "))
                {
                    //get the output variable name by reading until the
                    //=
                    str = str.Substring(10);
                    endIndex = findSub(str, '=');

                    //Makes sure that there is a variable name
                    if (endIndex > 0)
                    {
                        pinName = str.Substring(0, endIndex);
                        str = str.Substring(endIndex + 1);
                        endIndex = findSub(str, ' ');
                        
                        //Finds the actual output value
                        if (endIndex > 0)
                        {
                            results = new char[endIndex];
                            results = str.Substring(0, endIndex).ToCharArray();

                            str = str.Substring(endIndex + 2);
                            endIndex = findSub(str, ' ');
                            
                            //Finds the expected output value
                            if (endIndex > 0)
                            {
                                expected = new char[endIndex];
                                expected = str.Substring(0, endIndex).ToCharArray();
                            }
                            else
                                expected = null;
                        }
                        else
                        {
                            results = null;
                            expected = null;
                        }

                        //creates a new pin, adds it to a list of pins and returns
                        temp = new Pin(PinPolarity.Ouput, pinName, results, expected);
                        return temp;
                    }
                    //if the string contained an output with no name, return nothing
                    else
                        return null;
                }
            }

            //if its not a circuit output, call the input handler
            return input(str);
        }

        /// <summary>
        /// Method detects input pin errors in a string
        /// </summary>
        /// <param name="str">String to be checked for input errors</param>
        /// <returns>List of pins containing input errors</returns>
        private Pin input(String str)
        {
            int endindex = 0;
            String pinName;
            char[] result;
            Pin input;

            //finds the end of the variable name
            endindex = findSub(str, '=');

            //if the variable name exists
            if (endindex > 0)
            {
                //Pull of the variable name and value, write to a pin
                //and return
                pinName = str.Substring(0, endindex);
                str = str.Substring(endindex + 1);
                result = new char[str.Length];
                result = str.ToCharArray();
                input = new Pin(PinPolarity.Input, pinName, result);
                return input;
            }

            //if it's not an input either, return nothing
            return null;
        }

        /// <summary>
        /// Method returns the index of a desired terminal character in a string
        /// </summary>
        /// <param name="str">String to search for terminal character</param>
        /// <param name="termChar">Terminal character to search for</param>
        /// <returns>Int containing index of terminal character</returns>
        private int findSub(String str, Char termChar)
        {
            int endindex = 0;

            //Loops through the chars in a string until it finds the
            //terminal char or runs out of string
            while (!str[endindex].Equals(termChar))
            {
                if (endindex < (str.Length - 1))
                    endindex++;
                else
                    break;
            }

            //returns the index of the character
            return endindex;
        }

        /// <summary>
        /// Method for saving sketch as xml file
        /// </summary>
        /// <param name="filename">String containing xml filename</param>
        /// <param name="sketch">Sketch.Sketch object</param>
        public void XMLWrite(String filename, Sketch.Sketch sketch)
        {
            //create a new xml object and write the xml file
            MakeXML xmlFile = new MakeXML(sketch);
            xmlFile.WriteXML(filename);
        }

        /// <summary>
        /// Method for simulating circuit in Modelsim
        /// </summary>
        /// <param name="filename">filename of sketch xml</param>
        /// <param name="projFile">filename of xilinx project file</param>
        public void simulate(String filename, String projFile)
        {
            //Gets the file directory and the name of the xml file
            String Dir = Path.GetDirectoryName(filename);
            String file = Path.GetFileNameWithoutExtension(filename);
            String projDir = Path.GetDirectoryName(projFile);

            //The contents of the .fdo script to be generated
            String script = "#Modelsim .fdo script\n" +
                "#This script is automatically generated\n" +
                "#by Workpath \n\n" +
                "vlib work\n" +
                "cd {" + Dir + "}\n" +
                "vlog +acc \"" + file + ".v\"\n" +
                "vlog +acc \"test.v\"\n" +
                "vlog +acc \"C:/Xilinx/verilog/src/glbl.v\"\n" +
                "vsim -t 1ps -L xilinxcorelib_v -L unisims_ver -lib work test glbl\n" +
                "view wave\n" +
                "add wave *\n" +
                "add wave /glbl/GSR\n" +
                "catch {do {test.udo}} msg\n" +
                "view structure\n" +
                "view signals\n" +
                "run 1000ns";

            //The contents of the .udo script to be generated
            String udoScript = "-- Custom simulation command file\n" +
                "-- Insert simulation controls below:";

            //Ensures that either a new .fdo file is created or the old .fdo file is completely overwritten
            FileStream fdo = new FileStream(projDir + "\\test.fdo", FileMode.Create, FileAccess.Write);
            TextWriter writeFdo = new StreamWriter(fdo);
            writeFdo.Write(script);
            writeFdo.Close();
            fdo.Close();

            //Ensures that a new .udo file is created or the old .udo file is completely overwritten
            FileStream udo = new FileStream(projDir + "\\test.udo", FileMode.Create, FileAccess.Write);
            TextWriter writeUdo = new StreamWriter(udo);
            writeUdo.Write(udoScript);
            writeUdo.Close();
            fdo.Close();

            //Calls modelsim on the .fdo script
            Process modelSim = Process.Start("vsim", "-do " + Dir + "\\test.fdo");
            
            //In order to pause the program until the user exits modelsim, uncomment the following:
            //modelSim.WaitForExit();
        }
        
        /// <summary>
        /// Method for adding generated verilog to a Xilinx project file
        /// </summary>
        /// <param name="filename">String containing sketch xml filename</param>
        /// <param name="projFile">String containing Xilinx project filename</param>
        public void XilinxRun(String filename, String projFile)
        {
            String projectName = Path.GetFileNameWithoutExtension(projFile);
            String projDir = Path.GetDirectoryName(projFile);
            String dir = Path.GetDirectoryName(filename);
            String file = Path.GetFileNameWithoutExtension(filename);
                       
            //Xilinx project setup script
            String script = "#Xilinx Xtclsh script\n" +
                "#This script is automatically generated\n" +
                "#by Workpath \n \n" +
                "cd {" + projDir + "}\n" +
                "project open " + projectName + "\n" +
                "cd {" + dir + "}\n" +
                "catch {xfile add " + file + ".v} msg\n" +
                "catch {xfile add test.v} msg\n" +
                "cd {" + projDir + "}\n" +
                //"catch {project save_as " + projectName + "} msg\n" +
                //"catch {process run \"Synthesize - XST\" -force rerun_all} msg\n" +
                "project close";

            //Writes setup script to Synth.tcl in the Xilinx project file directory
            TextWriter tw = new StreamWriter(projDir + "\\Synth.tcl", false, Encoding.ASCII);
            tw.Write(script);
            tw.Close();

            //Runs the setup script
            Process p = Process.Start("xtclsh", projDir + @"\Synth.tcl");
            p.WaitForExit();

            //Deletes the scrip when finished
            File.Delete(projDir + "\\Synth.tcl");

            //Opens the Xilinx project
            //Process x = Process.Start("ise", "-L " + projDir + "\\" + projectName + ".ise");

            //In order to pause the program until the user exits Xilinx, uncomment the following:
            //x.WaitForExit();
        }

        /// <summary>
        /// Method for obtaining the path of the desired Xilinx project file.  If a single
        /// project file exists in the same directory as the xml sketch file, that project
        /// is returned.  Otherwise, the user is asked to select the appropriate project 
        /// file
        /// </summary>
        /// <param name="filename">String containing sketch xml filename</param>
        /// <returns>String containing Xilinx project filename</returns>
        public String getProj(String filename)
        {
            //if the filename is null, then use an open file dialog
            if (filename == null)
                return getFile(iseFilt, null);

            String Dir = Path.GetDirectoryName(filename);
            
            //searches for all Xilinx project files in the current directory
            String[] projects = Directory.GetFiles(Dir, "*.ise");
            
            //if there are no projects or multiple projects, the user is allowed to select
            //the appropriate project file.
            if (projects.Length == 0 || projects.Length>2)
                return getFile(iseFilt, Dir);
            
            //if there is only one project, it is assumed to be the appropriate project file
            else
                return projects[0];
        }
        #endregion
        #endregion

        #region New Project

        /// <summary>
        /// Method for creating a new Xilinx project file
        /// </summary>
        public void createProj()
        {
            //Promps user for project name before continuing
            String projFile = saveFile(iseFilt, null);
            
            //if the user cancelled out of creating a project file, exit
            if (projFile == null)
                return;

            //otherwise, call createproj with overwrite check disabled
            createProj(projFile, false);
        }

        /// <summary>
        /// Method for creating a new Xilinx project file
        /// </summary>
        /// <param name="projFile">String containing desired Xilinx project filename</param>
        /// <param name="writeCheck">Boolean value determining whether to prompt user
        /// before overwriting a projectfile</param>
        public void createProj(String projFile, bool writeCheck)
        {
            //if a Xilinx project with the same name already exists
            if ((File.Exists(projFile)) && (writeCheck == true))
            {
                //and overwrite checking is enabled the user is warned about the 
                //conflict and given the choice of overwriting or selecting a new 
                //project name
                DialogResult result = MessageBox.Show("Warning:\nCurrent project file will be overwritten." +
                    "\nWould you like to continue?", "Project Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                
                if (result == DialogResult.No)
                {
                    projFile = saveFile(iseFilt, null);
                    if (projFile == null)
                        return;
                }
            }

            //If it's gotten to this point, either overwrite check is off, the user decided
            //to overwrite, or the user selected a new filename and was warned if that
            //file already exists, so, if it's already there, delete it
            if (File.Exists(projFile))
                File.Delete(projFile);

            //Sets the objects projectpath to the new project file
            this.projectPath = projFile;

            String projName = Path.GetFileNameWithoutExtension(projFile);
            String projDir = Path.GetDirectoryName(projFile);

            //tcl script to generate Xilinx project file
            String script = "#This scrip file was automatically\n" +
                "#Generated by uiWorkPath.dll\n" +
                "\nproject new " + projName + ".ise\n" +
                "\nproject set family spartan3\n" +
                "project set device xc3s400\n" +
                "project set package tq144\n" +
                "project set speed -4\n" +
                "project set top_level_module_type schematic\n" +
                "catch {project set generated_simulation_language " +
                "\"Modelsim-SE Verilog\"} msg\n" +
                "project close";

            //Writes tcl script
            FileStream fs = new FileStream(projDir + "\\projGen.tcl", FileMode.Create, FileAccess.Write);
            TextWriter tw = new StreamWriter(fs);
            tw.Write(script);
            tw.Close();
            fs.Close();

            //Runs tcl script
            Process p = Process.Start("xtclsh", "projGen.tcl");
            p.WaitForExit();
            File.Delete(projDir + "\\projGen.tcl");

            this.projectPath = projFile;
        }
        #endregion

        #region file getters/setters
        /// <summary>
        /// Method for opening a file
        /// </summary>
        /// <param name="filter">String containing file type filter</param>
        /// <returns>String containing selected filename</returns>
        public String getFile(String filter, String iniDir)
        {
            //Opens file open dialog
            OpenFileDialog fd = new OpenFileDialog();
            String filename;

            //If an initial directory is specified, start the file dialog with that
            //directory open
            if (iniDir != null)
                fd.InitialDirectory = iniDir;

            //Sets the filedialog allowable file filter
            fd.Filter = filter;

            //if user clicks ok return filename
            if (fd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = fd.FileName.ToLower();
                    return filename;

                }

                catch (IOException /*ioe*/)
                {
                    MessageBox.Show("File error");
                }

            }

            //if no filename is selected, return null
            return null;
        }

        /// <summary>
        /// Method for saving a file
        /// </summary>
        /// <param name="filter">String containing allowed file type filter</param>
        /// <returns>String containing selected filename</returns>
        private String saveFile(String filter, String dirPath)
        {
            SaveFileDialog fd = new SaveFileDialog();
            String filename;

            //sets allowable file type filter
            fd.Filter = filter;

            //Set the initial directory
            if (dirPath != null)
                fd.InitialDirectory = dirPath;

            //if user selects ok, return filename
            if (fd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = fd.FileName.ToLower();

                    return filename;
                }
                catch (IOException /*ioe*/)
                {
                    MessageBox.Show("File error");
                }

            }

            //if no filename is selected, return null
            return null;
        }
        #endregion

        #region Circuit Converters
        /// <summary>
        /// Method takes the inputs and outputs from a verilogwriter module and converts it
        /// to a list of pins
        /// </summary>
        /// <param name="mod">Module object to be converted to a list of pins</param>
        /// <returns>List containing all of the module's pins</returns>
        public List<Pin> mod2pin(Module mod)
        {
            List<Pin> pins = new List<Pin>();
            
            pins.AddRange(mod.inputPins);
            pins.AddRange(mod.outputPins);

            //returns the list of pins
            return pins;
        }

        /// <summary>
        /// Takes a list of pins and converts them into the inputs and outputs of
        /// a verilog writer module object
        /// </summary>
        /// <param name="pins">A list of Pin objects to be included in the module
        /// object</param>
        /// <param name="modName">A string containing the name of the module to be gnerated</param>
        /// <returns>Module object</returns>
        public Module pins2mod(List<Pin> pins, String modName)
        {
            Module mod = new Module(modName);
            List<Pin> inputs = new List<Pin>();
            List<Pin> outputs = new List<Pin>();
            
            if (pins.Count == 0)
                return null;

            //Goes through each Pin
            foreach (Pin instance in pins)
            {
                //checks to see if its an input or output, adds the input/output to
                //an list and places the Pin bus size in an int array
                if (instance.Polarity.Equals(PinPolarity.Input))
                {
                    inputs.Add(instance);
                }
                else if (instance.Polarity.Equals(PinPolarity.Ouput))
                {
                    outputs.Add(instance);
                }
            }

            mod.addInputs(inputs);
            
            mod.addOutputs(outputs);

            return mod;
        }
        #endregion

        #region Truth Tables

        /// <summary>
        /// Converts a list of pins into a .tv test vector file
        /// </summary>
        /// <param name="pins">List of List of pins to be converted to testvectors</param>
        public void pins2testVectors(List<List<Pin>> pins)
        {
            pins2testVectors(pins, this.filePath);
        }

        /// <summary>
        /// Converts list of pins into a .tv test vector file
        /// </summary>
        /// <param name="pins">List of pins List of pins to be converted to testvectors</param>
        /// <param name="filename">String containing a filename that exists in the desired
        /// testvector destination directory</param>
        public void pins2testVectors(List<List<Pin>> pinMatrix, String filename)
        {
            String dir = Path.GetDirectoryName(filename);
            TextWriter tw = new StreamWriter(dir + "\\testVect.tv");

            PinPolarity prevType = PinPolarity.Input;

            //nested foreach to pull out the appropriate testvector rows
            foreach (List<Pin> pins in pinMatrix)
            {
                foreach (Pin instance in pins)
                {
                    //if its changing from an input to an output, write an underscore
                    if (prevType.Equals(PinPolarity.Input) && instance.Polarity.Equals(PinPolarity.Ouput))
                        tw.Write("_");

                    //if its going from output to input, a newline is required
                    else if (prevType.Equals(PinPolarity.Ouput) && instance.Polarity.Equals(PinPolarity.Input))
                        tw.WriteLine();

                    //While the pin value is conventionally held in the
                    //pinVal variable, in the truthtable code, the pinName
                    //stores the value
                    tw.Write(instance.PinName);
                    
                    //update the previous type
                    prevType = instance.Polarity;
                }
            }

            tw.Close();
        }

        /// <summary>
        /// Method compares inputs and outputs generated by the circuit and truthtable 
        /// recognizers, attempts to reconcile the order and generated an appropriate
        /// Verilog testbench
        /// </summary>
        /// <param name="tv">List of Pins containing circuit input/outputs recognized
        /// by the truthtable recognizer</param>
        /// <param name="circ">List of superwires contained in the CircuitRec object</param>
        /// <returns>List of pins containing the correct pin order</returns>
        public List<Pin> inOutRectify(List<Pin> tv, List<Mesh> circ)
        {
            String filename = filePath;
           
            if (this.filePath == null)
                filename = saveFile(verilogFilt, null);

            return inOutRectify(tv, circ, filename);
        }

        /// <summary>
        /// Method compares inputs and outputs generated by the circuit and truthtable 
        /// recognizers, attempts to reconcile the order and generated an appropriate
        /// Verilog testbench
        /// </summary>
        /// <param name="tv">List of Pins containing circuit input/outputs recognized
        /// by the truthtable recognizer</param>
        /// <param name="circ">List of superwires contained in the CircuitRec object</param>
        /// <returns>List of pins containing the correct pin order</param>
        /// <param name="filename">Name of the xml/verilog file that contains the
        /// circuit whose inputs/outputs are to be rectified</param>
        /// <returns>List of pins containing the correct pin order</returns>
        public List<Pin> inOutRectify(List<Pin> tv, List<Mesh> circ, String filename)
        {
            //get the input/output pins from circuitrec
            PinList synthPins = new PinList(MeshToPins(circ));

            
            //initialize output list
            List<Pin> order = new List<Pin>();
            Pin clkPin;

            String dir = Path.GetDirectoryName(filename);
            String module = Path.GetFileNameWithoutExtension(filename);

            int i = 0;

            //Get the clock variable
            clkPin = synthPins.clk_var();

            //Remove the clock variable from synthPins
            synthPins.clk_rm(clkPin);

            //foreach (Pin instance in synthPins.Pins)
            //{
            //    Console.WriteLine(instance.PinName + " " + instance.bussize);
            //}

            order.Add(clkPin);

            //If there is a mismatch between number of inputs/outputs
            //return an error
            if (tv.Count != synthPins.Count)
            {
                MessageBox.Show("Mismatch between number of truth table and circuit inputs/outputs", "In/Out Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
            int flipped = 0;

            //Looks for each pin that exists in the testvector file in the circuit
            //if it exists in the circuit, it is added to this list of pin outputs
            foreach(Pin instance in tv)
            {
                i = indexFind(synthPins.Pins, instance);

                if (i < synthPins.Pins.Count)
                {
                    if (instance.Polarity.Equals(synthPins.Pins[i].Polarity))
                    {
                        //if the pins have the same input/output type, add it to the
                        //return list and remove it from synthpins
                        order.Add(instance);
                        synthPins.RemoveAt(i);
                    }
                    else
                        flipped++;
                }
            }

            //if all inputs/outputs match except every input/output has the wrong 
            //polarity, return an error
            if (flipped == synthPins.Count)
            {
                MessageBox.Show("Truthtable inputs/outputs appear to have been flipped", "Input/Output Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            //if some inputs/outputs could not be matched up, return an error
            if (synthPins.Pins.Count > 0)
            {
                //MessageBox.Show("here");
                //foreach (Pin instance in synthPins.Pins)
                //{
                //    Console.WriteLine(instance.PinName + " " + instance.bussize);
                //}
                //MessageBox.Show("there");
                MessageBox.Show("Input/Output lists could not be rectified", "Input/Output Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            Module mod = new Module("testBench");

            //write out a testbench with inputs/outputs in the appropriate order
            TestBenchWriter tbw = new TestBenchWriter(dir, pins2mod(order, module));
            tbw.writeBench();
            
            return order;
        }

        /// <summary>
        /// Method finds the index of a pin object in a list of pins
        /// </summary>
        /// <param name="tv">List of pins to be searched</param>
        /// <param name="instance">Pin instance to search for</param>
        /// <returns>Int corresponding to the Pin's index in the Pin List</returns>
        private int indexFind(List<Pin> tv, Pin instance)
        {
            int i;

            //Goes through each instance of the list and checks for name and bussize
            //It does not check for input/output in order to facilitate flipped truthtable
            //detection
            for (i = 0; i < tv.Count; i++)
            {
                if (tv[i].PinName.Equals(instance.PinName) && tv[i].bussize.Equals(instance.bussize))
                    return i;
            }

            i++;

            return i;
        }
     
        /// <summary>
        /// Pulls inputs and outputs from list of superwires
        /// </summary>
        /// <param name="wires">List of superwires</param>
        /// <returns>List of pins</returns>
        private List<Pin> MeshToPins(List<Mesh> wires)
        {
            List<Pin> pins = new List<Pin>();
            String pinName;
            Mesh test = new Mesh();
            int busSize;

            //Goes through each superwire
            foreach(Mesh wire in wires)
            {
                pinName = wire.Name;
                busSize = wire.Bussize;
                Pin temp;
               
                //checks its polarity, creates and appropriate pin and
                //adds it to the return list
                if (wire.IOType.Equals(WirePolarity.Input))
                {
                    temp = new Pin(PinPolarity.Input, pinName, busSize);
                    pins.Add(temp);
                }
                else if (wire.IOType.Equals(WirePolarity.Output))
                {
                    temp = new Pin(PinPolarity.Ouput, pinName, busSize);
                    pins.Add(temp);
                }
                
            }

            return pins;
        }

        /// <summary>
        /// Method returns number inputs/outputs found by the circuit recognizer
        /// </summary>
        /// <param name="circ">List of superwires from the circuitrec object</param>
        /// <returns>Int containing the total number of circuit inputs and outputs</returns>
        public int numCol(CircuitRec.CircuitRec circ)
        {
            //Convert the superwires to a list of input/output pins
            PinList pins = new PinList(MeshToPins(circ.Meshes));

            //removes the clock variable from the list of inputs/outputs
            pins.clk_rm();

            //returns the number of remaining inputs/outputs
            return pins.Count;
        }

        #endregion

        #region Sketcher Project File

        /// <summary>
        /// Creates a project file used to store internal filepaths from sketcher's program
        /// </summary>
        /// <param name="sketchProj">String containing desired project name and path</param>
        /// <param name="projF">String containing Xilinx project file path</param>
        /// <param name="circSketch">String containing the .xml circuit path</param>
        /// <param name="truthSketch">String containing .xml truthtable path</param>
        /// <param name="noteSketch">String containing .xml notes path</param>
        /// <param name="verilog">String containing the verilog file path</param>
        /// <returns>Boolean indicating whether write succeeded</returns>
        public Boolean createFile(String sketchProj, String projF, String circSketch, String truthSketch, String noteSketch, String verilog)
        {
            //if any of the paths are null, return false
            if (sketchProj == null || projF == null || circSketch == null || truthSketch == null || noteSketch == null || verilog == null)
                return false;

            //set up the file writer
            FileStream fs = new FileStream(sketchProj, FileMode.Create, FileAccess.Write);
            TextWriter tw = new StreamWriter(fs);

            //write the project paths
            tw.WriteLine(projF);
            tw.WriteLine(circSketch);
            tw.WriteLine(truthSketch);
            tw.WriteLine(noteSketch);
            tw.WriteLine(verilog);

            //close the files
            tw.Close();
            fs.Close();

            //write succeeded
            return true;
        }

        /// <summary>
        /// Reads in a sketcher project file and returns the filepaths.  Also sets the objects
        /// project and verilog file path based off of the info stored in the sketch file
        /// </summary>
        /// <param name="sketchProj">String containing desired project name and path</param>
        /// <param name="projF">String to contain Xilinx project file path</param>
        /// <param name="circkSketch">String to contain the .xml circuit path</param>
        /// <param name="truthSketch">String to contain .xml truthtable path</param>
        /// <param name="noteSketch">String to contain .xml notes path</param>
        /// <param name="verilog">String to contain the verilog file path</param>
        public void readFile(String sketchProj, out String projF, out String circkSketch, out String truthSketch, out String noteSketch, out String verilog)
        {
            FileStream fs = new FileStream(sketchProj, FileMode.Open, FileAccess.Read);
            TextReader tr = new StreamReader(fs);

            projF = tr.ReadLine();
            circkSketch = tr.ReadLine();
            truthSketch = tr.ReadLine();
            noteSketch = tr.ReadLine();
            verilog = tr.ReadLine();

            this.projectPath = projF;
            this.filePath = verilog;

            tr.Close();
            fs.Close();
        }
        
        #endregion
    }
}
