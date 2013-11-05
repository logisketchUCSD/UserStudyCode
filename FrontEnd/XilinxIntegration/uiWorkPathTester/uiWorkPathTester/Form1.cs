using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using System.IO;
using uiWorkPath;
using VerilogWriter;
using System.Collections.Generic;
using CircuitRec;
using Pins;


namespace uiWorkPathTester
{
    public partial class Form1 : Form
    {
        private InkCollector inkCol;
        private Button sim = new Button();
        private Button newProj = new Button();
        private Button erRead = new Button();
        private Button mod2pin2ink = new Button();

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

            this.newProj.Text = "New Project";
            this.newProj.Location = new System.Drawing.Point(90, 45);
            this.newProj.Size = new Size(100, 25);
            this.newProj.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(newProj);

            this.erRead.Text = "Read Errors";
            this.erRead.Location = new Point(90, 80);
            this.erRead.Size = new Size(100, 25);
            this.erRead.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(erRead);

            this.mod2pin2ink.Text = "Test Conv";
            this.mod2pin2ink.Location = new Point(90, 105);
            this.mod2pin2ink.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(mod2pin2ink);

            this.inkCol.AutoRedraw = true;

            this.sim.Click += new EventHandler(sim_Click);
            this.newProj.Click += new EventHandler(newProj_Click);
            this.inkCol.Stroke += new InkCollectorStrokeEventHandler(inkCol_Stroke);
            this.erRead.Click += new EventHandler(erRead_Click);
            this.mod2pin2ink.Click += new EventHandler(mod2pin2ink_Click);
        }

        void mod2pin2ink_Click(object sender, EventArgs e)
        {
            WorkPath path = new WorkPath();
            Module mod = new Module("mistaKitties");
            List<Pin> inputs = new List<Pin>();
            List<Pin> outputs = new List<Pin>();
            inputs.Add(new Pin(PinPolarity.Input, "clk", 1));
            inputs.Add(new Pin(PinPolarity.Input, "s", 4));
            inputs.Add(new Pin(PinPolarity.Input, "b", 1));
            
            /*
            int[] insizes = new int[3];
            inputs.Add("clk");
            inputs.Add("s");
            inputs.Add("b");
            insizes[0] = 1;
            insizes[1] = 4;
            insizes[2] = 1;*/

            //List<Pin> outputs = new List<Pin>();
            //int[] outsizes = new int[2];

            outputs.Add(new Pin(PinPolarity.Ouput, "x", 5));
            outputs.Add(new Pin(PinPolarity.Ouput, "y", 1));
            //outsizes[0] = 5;
            //outsizes[1] = 1;

            mod.addInputs(inputs);
            mod.addOutputs(outputs);

            List<Pin> pins = new List<Pin>();

            pins.AddRange(path.mod2pin(mod));

            

            Module mod2 = path.pins2mod(pins, "MistaB");

            int i = 0;
            foreach (Pin str in mod2.inputPins)
            {
                Console.WriteLine(str.PinName + " " + str.bussize);
                i++;
            }
            i = 0;
            foreach (Pin str in mod2.outputPins)
            {
                Console.WriteLine(str.PinName + " " + str.bussize);
                i++;
            }

            List<Pin> pin2 = new List<Pin>();
            foreach (Pin instance in pins)
                if(instance.PinName != "clk")
                    pin2.Add(instance);

            foreach (Pin datapin in pin2)
            {
                Console.WriteLine(datapin.PinName + " " + datapin.bussize);
            }
    //        Console.WriteLine(pin2[0].PinName + " " + pins[0]);
      //      Console.WriteLine(pin2[0].Equals(pins[0]));
            //Pin dk = new Pin("in", "clk", 1);
            //pin2.Remove(pin2[0]);
            //pin2.Add(dk);
            pin2.Reverse();
            Mesh sup = new Mesh();
            Mesh sup1 = new Mesh();
            Mesh sup2 = new Mesh();
            Mesh sup3 = new Mesh();
            Mesh sup4 = new Mesh();
            List<Mesh> list = new List<Mesh>();
            
            //sup.Name = "clk";
            //sup.IOType = WirePolarity.Input;
            //list.Add(sup);
            sup1.Name = "s";
            sup1.IOType = WirePolarity.Input;
            sup1.Bussize = 4;
            list.Add(sup1);
            sup2.Name = "b";
            sup2.Bussize = 1;
            sup2.IOType = WirePolarity.Input;
            list.Add(sup2);
            sup3.Name = "x";
            sup3.Bussize = 5;
            sup3.IOType = WirePolarity.Output;
            list.Add(sup3);
            sup4.Name = "y";
            sup4.Bussize = 1;
            sup4.IOType = WirePolarity.Output;
            list.Add(sup4);
            List<Pin> pins4 = new List<Pin>();

            pins4.AddRange(path.inOutRectify(pin2, list, "c:\\cats\\test.v"));
            
            List<List<Pin>> sups = new List<List<Pin>>();
            List<List<Pin>> sups3 = new List<List<Pin>>();
            sups3.Add(pin2);
            sups.Add(pins);
            sups.AddRange(sups3);
            
            MessageBox.Show("shows");
            foreach (Pin instance in sups[0])
            {
                Console.WriteLine(instance.PinName);
            }
            
        }

        void erRead_Click(object sender, EventArgs e)
        {
            WorkPath path = new WorkPath();
            List<Pin> errors = new List<Pin>();
            errors.AddRange(path.errors("c:\\cats\\error.log"));

            foreach (Pin datapin in errors)
            {                
                Console.Write(datapin.PinName + " ");
                Console.Write(datapin.val2str());
                
                if(datapin.Polarity.Equals(PinPolarity.Ouput))
                {
                    Console.Write(" ");
                    Console.Write(datapin.expected2str());
                    
                }
                Console.WriteLine();
            }

            //path.pins2testVectors(errors, "c:\\cats\\error.log");
        }

        void inkCol_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            //if the stroke doesn't already have a timestamp, add one
            if (!e.Stroke.ExtendedProperties.DoesPropertyExist(Microsoft.Ink.StrokeProperty.TimeID))
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

        void newProj_Click(object sender, EventArgs e)
        {
            WorkPath path = new WorkPath(null, null, inkCol.Ink);
            path.createProj();
            //path.Go();
        }

        void sim_Click(object sender, EventArgs e)
        {
            WorkPath path = new WorkPath("c:\\cats\\lab1_xx.ise", null, inkCol.Ink);
            //path.Go();
        }
    }
}