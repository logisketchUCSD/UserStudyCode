using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace TestBenchTester
{
    public partial class Form1 : Form
    {
        private Button test = new Button();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.test.Text = "Generate TestBench";
            this.test.Location = new System.Drawing.Point(90, 10);
            this.test.Size = new Size(200, 25);
            this.test.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(test);
            this.test.Click += new EventHandler(test_Click);
        }

        void test_Click(object sender, EventArgs e)
        {
            VerilogWriter.module mod = new VerilogWriter.module("led");

            ArrayList input = new ArrayList();
            ArrayList output = new ArrayList();
            //int[] insize = new int[2];
            //int[] outsize = new int[1];

            
            input.Add("s");
            input.Add("clk");
            output.Add("led");
            //input.Add("a");
            //input.Add("b");
            //input.Add("c");
            //output.Add("y");
            int[] insize = new int[input.Count];
            int[] outsize = new int[output.Count];
            /*insize[0] = 1;
            insize[1] = 1;
            insize[2] = 1;
            outsize[0] = 1;
            */
            
            insize[0] = 4;
            insize[1] = 1;
            outsize[0] = 8;
            mod.addinputs(input, insize);
            mod.addoutputs(output, outsize);

            VerilogWriter.TestBenchWriter tbw = new VerilogWriter.TestBenchWriter("C:\\documents and settings\\student\\desktop", mod);
            tbw.writeBench();
            //tbw.writeBench();
        }
    }
}