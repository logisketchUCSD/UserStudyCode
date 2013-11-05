using System;
using System.Collections.Generic;
using System.Text;


namespace WPFCircuitSimulatorUI
{
    /// <summary>
    /// Constants associated with Circuit Simulator Project Files
    /// 
    /// Convetion: Project file names conform to the following
    /// convention, where PROJNAME is the name of a project
    ///    * PROJNAME.circuit.xml - The Circuit Sketch XML file
    ///    * PROJNAME.notes.xml - The Notes Sketch XML file
    ///    * PROJNAME.input.xml - The Input Sketch XML file (e.g., a Truth Table)
    ///    * PROJNAME.ise - The Xilinx Project file
    ///    * PROJNAME.v - The Verilog Module file
    /// </summary>
    public static class FilenameConstants
    {
        /// <summary>
        /// Default location of the circuit label domain file.  This domain file
        /// maps circuit labels to colors.
        /// </summary>
        public static string DefaultCircuitDomainFilePath =
            AppDomain.CurrentDomain.BaseDirectory + @"CircuitColorDomain.txt";

        /// <summary>
        /// Default location of the circuit structure domain file.  This domain file
        /// specifies the structure of circuit gates for the use of CircuitParser recognition.
        /// </summary>
        public static string DefaultCircuitParserDomainFilepath =
            AppDomain.CurrentDomain.BaseDirectory + @"digital_domain.txt";

        /// <summary>
        /// TEMP
        /// </summary>
        public static string DefaultTruthTableDomainFilepath =
            AppDomain.CurrentDomain.BaseDirectory + @"TruthTableDomain.txt";

        /// <summary>
        /// Default name for new project files
        /// </summary>
        public static string DefaultProjectName = "New_Project";

        /// <summary>
        /// Default file system root for new projects.
        /// </summary>
        public static string DefaultProjectRoot = @"C:\";

        /// <summary>
        /// The filepath for our Data folder.
        /// </summary>
        public static string DataRoot = AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\..\..\..\Data\";

        /// <summary>
        /// Default path for new projects.
        /// </summary>
        public static string DefaultProjectPath = DefaultProjectRoot + DefaultProjectName + @"\";

        /// <summary>
        /// Default extension for XML files.
        /// </summary>
        public static string DefaultXMLExtension = ".xml";

        /// <summary>
        /// Default extension for logisim files
        /// </summary>
        public static string DefaultLogisimExtension = ".circ";

        /// <summary>
        /// Default extension for Circuit Sketch XML files.
        /// </summary>
        public static string DefaultCircuitExtention = ".circuit.xml";

        /// <summary>
        /// Default extension for Input Sketch XML files.
        /// </summary>
        public static string DefaultInputExtension = ".input.xml";

        /// <summary>
        /// Default extension for Notes Sketch XML files.
        /// </summary>
        public static string DefaultNotesExtension = ".notes.xml";

        /// <summary>
        /// Default extension for Xilinx project files.
        /// </summary>
        public static string DefaultXilinxProjectExtension = ".ise";

        /// <summary>
        /// Default extension for Verilog module files.
        /// </summary>
        public static string DefaultVerilogModuleExtension = ".v";

        /// <summary>
        /// The extension for Windows JournalFiles
        /// </summary>
        public static string JournalFileExtension = ".jnt";

        /// <summary>
        /// The extension for Sketch XML files
        /// </summary>
        public static string SketchXMLExtension = ".xml";

        /// <summary>
        /// Our program's name, for window title display
        /// </summary>
        public static string ProgramName = "LogiSketch";
    }
}
