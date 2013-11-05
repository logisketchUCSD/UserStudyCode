using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using Sketch;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text;
using msInkToHMCSketch;
using ConverterXML;
//using MathNet.Numerics;
using System.Drawing.Drawing2D;


namespace sketch3
{
    public partial class Form1 : Form
    {
        private const float SAMPLE_RATE = 133.0f;
        private InkCollector inkCol;
        private Button sim = new Button();
        
        
        public Form1()
        {
            
            InitializeComponent();

            inkCol = new InkCollector(this.Handle);
            inkCol.Enabled = true;
            inkCol.AutoRedraw = true;
                      
            
        } 

        private void Form1_Load(object sender, EventArgs e)
        {
            this.sim.Text = "Simulate";
            this.sim.Location = new System.Drawing.Point(90, 10);
            this.sim.Size = new Size(100, 25);
            this.sim.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(sim);
            this.inkCol.AutoRedraw = true;
            this.sim.Click += new EventHandler(sim_Click);
            this.inkCol.Stroke += new  InkCollectorStrokeEventHandler(inkCol_Stroke);
            
        }

        
        /// <summary>
        /// Adds a timestamp to every stroke.  Based off of the documentation found at
        /// http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkcollector.stroke.aspx
        /// and http://msdn2.microsoft.com/en-us/library/ms812508.aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void inkCol_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
                      
            //if the stroke doesn't already have a timestamp, add one
            if(!e.Stroke.ExtendedProperties.DoesPropertyExist(Microsoft.Ink.StrokeProperty.TimeID))
            {
                try
                {
                    //Retrieve the current time. UTC format.
                    DateTime dt = DateTime.Now;
                    long filetime = dt.ToFileTime();
                    MemoryStream memstr = new MemoryStream();
                    BinaryWriter binwrite = new BinaryWriter(memstr);
                    binwrite.Write(filetime);
                    Byte[] bt = memstr.ToArray();

                    memstr.Close();
                    binwrite.Close();

                    e.Stroke.ExtendedProperties.Add(Microsoft.Ink.StrokeProperty.TimeID, bt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }     
        }

        /// <summary>
        /// Handler for the "simulate" button.  When clicked, this method saves the ink
        /// in the MIT XML format used by Harvey Mudd.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sim_Click(object sender, EventArgs e)
        {
            inkCol.AutoRedraw = true;
            
            ReadInk newSketch = new ReadInk(inkCol.Ink);

            Sketch.Sketch sketch = newSketch.Sketch;

            String filename = XMLWrite(sketch);

            String extensionlessFilename = Path.GetFileNameWithoutExtension(filename);

            XilinxRun(filename);
            simulate(filename);
            return;
        }

        /// <summary>
        /// This method saves the user-generated sketch in the MIT xml format
        /// </summary>
        /// <param name="sketch">Sketch.Sketch object to be written to xml</param>
        /// <returns>String containing the generated xml's filepath</returns>
        String XMLWrite(Sketch.Sketch sketch)
        {
            inkCol.AutoRedraw = true;
            SaveFileDialog saveDialog = new SaveFileDialog();
            String filename;
            saveDialog.Filter = "XML files (*.xml)|*.xml";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    filename = saveDialog.FileName.ToLower();
                    
                    MakeXML xmlFile = new MakeXML(sketch);
                    
                    xmlFile.WriteXML(filename);

                    

                    return filename;

                }
                catch (IOException /*ioe*/)
                {
                    MessageBox.Show("File error");
                }

            }

            return null;

        }

        /// <summary>
        /// This method adds the generated Verilog files to a Xilinx project file,
        /// synthesizes using the XST tool and runs Xilinx
        /// </summary>
        /// <param name="filename">The full filename where the .xml file was saved</param>
        void XilinxRun(String filename) 
        {
            String Dir = Path.GetDirectoryName(filename);
            String projectName;
            String[] projects = Directory.GetFiles(Dir, "*.ise");
            if (projects.Length == 0)
            {
                filename = getProj();
                Dir = Path.GetDirectoryName(filename);
                projectName = Path.GetFileNameWithoutExtension(filename);
            }
            else
            {
                projectName = Path.GetFileNameWithoutExtension(projects[0]);
            }

            String file = Path.GetFileNameWithoutExtension(filename);
            
            String script = "#Xilinx Xtclsh script\n" +
                "#This script is automatically generated\n" +
                "#by sketch3.exe \n \n" +
                "project open " + projectName + "\n" +
                "xfile add " + file + ".v\n" +
                "xfile add test.v\n" +
                "project save_as " + projectName + "\n" +
                "process run \"Synthesize - XST\" -force rerun_all\n" +
                "project close";
            
            TextWriter tw = new StreamWriter(Dir + "\\Synth.tcl", false, Encoding.ASCII);
            tw.Write(script);
            tw.Close();
            Process p = Process.Start("xtclsh", "Synth.tcl");
            p.WaitForExit();
            Process x = Process.Start("ise.exe", "-L " + Dir + "\\" + projectName);
            //In order to pause the program until the user exits Xilinx, uncomment the following:
            //x.WaitForExit();

            
        }
        /// <summary>
        /// This method simulates user's circuit
        /// </summary>
        /// <param name="filename">The full filename where the .xml file was saved</param>
        void simulate(String filename)
        {
            //Gets the file directory and the name of the xml file
            String Dir = Path.GetDirectoryName(filename);
            String file = Path.GetFileNameWithoutExtension(filename);

            //The contents of the .fdo script to be generated
            String script = "#Modelsim .fdo script\n" +
                "#This script is automatically generated\n" +
                "#by sketch3.exe \n\n" +
                "vlib work\n" +
                "vlog +acc \"" + file + ".v\"\n" +
                "vlog +acc \"test.v\"\n" +
                "vlog +acc \"C:/Xilinx/verilog/src/glbl.v\"\n" +
                "vsim -t 1ps -L xilinxcorelib_v -L unisims_ver -lib work test glbl\n" +
                "view wave\n" +
                "add wave *\n" +
                "add wave /glbl/GSR\n" +
                "do {test.udo}\n" +
                "view structure\n" +
                "view signals\n" +
                "run 1000ns";

            //The contents of the .udo script to be generated
            String udoScript = "-- Custom simulation command file\n" +
                "-- Insert simulation controls below:";

            //Ensures that either a new .fdo file is created or the old .fdo file is completely overwritten
            FileStream fdo = new FileStream(Dir + "\\test.fdo", FileMode.Create, FileAccess.Write);
            TextWriter writeFdo = new StreamWriter(fdo);
            writeFdo.Write(script);
            writeFdo.Close();
            fdo.Close();

            //Ensures that a new .udo file is created or the old .udo file is completely overwritten
            FileStream udo = new FileStream(Dir + "\\test.udo", FileMode.Create, FileAccess.Write);
            TextWriter writeUdo = new StreamWriter(udo);
            writeUdo.Write(udoScript);
            writeUdo.Close();
            fdo.Close();

            //Calls modelsim on the .fdo script
            Process modelSim = Process.Start("vsim", "-do test.fdo");
            //In order to pause the program until the user exits modelsim, uncomment the following:
            //modelSim.WaitForExit();
        }

        /// <summary>
        /// This module allows the user to select which directory their Xilinx project file
        /// is in
        /// </summary>
        /// <returns>String containing the filepath of the Xilinx project file</returns>
        String getProj()
        {
            OpenFileDialog fd = new OpenFileDialog();
            String filename;
            fd.Filter = "Xilinx Project Files (*.ise)|*.ise";

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

            return null;
        }
    }
}