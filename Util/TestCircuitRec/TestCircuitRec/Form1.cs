using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using ZedGraph;
using Sketch;
using CircuitRec;

namespace TestCircuitRec
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private ZedGraph.ZedGraphControl zg1;
        private GraphPane myPane;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        ArrayList wire_list = new ArrayList();
        ArrayList symb_list = new ArrayList();
        ArrayList bounds = new ArrayList();
        ArrayList endpts = new ArrayList();
        ArrayList endpts_1 = new ArrayList();
        ArrayList endpts_2 = new ArrayList();
        ArrayList endpts_3 = new ArrayList();
        public List<Wire> wires;
        ArrayList endpoints = new ArrayList();
        ArrayList eps = new ArrayList();

        public Form1(ArrayList edgepts)
        {
            InitializeComponent();
            wire_list = edgepts;
            myPane = zg1.GraphPane;
        }

        public Form1(ArrayList epts, ArrayList eptsbb, ArrayList wirepts, ArrayList eps)
        {
            InitializeComponent();
            endpts = epts;
            bounds = eptsbb;
            wire_list = wirepts;
            this.eps = eps;

            myPane = zg1.GraphPane;
        }

        public Form1(ArrayList wires, ArrayList symbs, ArrayList boundingbox, ArrayList endpts,
                    ArrayList endpts_1, ArrayList endpts_2, ArrayList endpts_3, List<Wire> wiresL, ArrayList endpointsAL)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            wire_list = wires;
            symb_list = symbs;
            bounds = boundingbox;
            this.endpts = endpts;
            this.endpts_1 = endpts_1;
            this.endpts_2 = endpts_2;
            this.endpts_3 = endpts_3;
            this.wires = wiresL;
            this.endpoints = endpointsAL;

            myPane = zg1.GraphPane;


        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // zg1
            // 
            this.zg1.Location = new System.Drawing.Point(8, 8);
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0;
            this.zg1.ScrollMaxX = 0;
            this.zg1.ScrollMaxY = 0;
            this.zg1.ScrollMaxY2 = 0;
            this.zg1.ScrollMinX = 0;
            this.zg1.ScrollMinY = 0;
            this.zg1.ScrollMinY2 = 0;
            this.zg1.Size = new System.Drawing.Size(352, 248);
            this.zg1.TabIndex = 0;
            this.zg1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.plot_enter);
            this.zg1.Load += new System.EventHandler(this.zg1_Load);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(368, 262);
            this.Controls.Add(this.zg1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }
        #endregion

        private void Form1_Load(object sender, System.EventArgs e)
        {

            CreateChart(zg1);
            SetSize();
        }

        // Call this method from the Form_Load method, passing your ZedGraphControl
        public void CreateChart(ZedGraphControl zgc)
        {
            // Sets up the GraphPane properties
            // GraphPane myPane = zgc.GraphPane;
            myPane.Legend.IsVisible = false;

            // Set the title and axis labels
            myPane.Title.Text = "Sample data output";
            myPane.XAxis.Title.Text = "X Axis";
            myPane.YAxis.Title.Text = "Y Axis";


            foreach (PointPairList list in wire_list)
                myPane.AddCurve("Wire", list, Color.Red, SymbolType.Circle);

            // Add wire labels
            for (int i = 0; i < wires.Count; i++)
            {
                Wire tempwire = (Wire)wires[i];
                // Add a text item to label the highlighted range
                TextObj text = new TextObj((string)tempwire.Name, (double)tempwire.EndPt[0].X, -(double)tempwire.EndPt[0].Y, CoordType.AxisXYScale,
                    AlignH.Right, AlignV.Center);
                text.FontSpec.FontColor = Color.Black;
                text.FontSpec.Fill.IsVisible = false;
                text.FontSpec.Border.IsVisible = false;
                text.FontSpec.IsBold = true;
                text.FontSpec.IsItalic = true;
                myPane.GraphObjList.Add(text);
            }

            foreach (PointPairList list in symb_list)
                myPane.AddCurve("Symbol", list, Color.Green, SymbolType.Diamond);

            foreach (PointPairList list in bounds)
                myPane.AddCurve("Label", list, Color.Orange, SymbolType.Circle);

            foreach (PointPairList list in this.endpts)
                myPane.AddCurve("Endpoints", list, Color.Black, SymbolType.Square);

            for (int i = 0; i < eps.Count; i++)
            {
                EndPoint tempep = (EndPoint)this.eps[i];
                // Add a text item to label the highlighted range
                TextObj text = new TextObj(tempep.Type + ",m=" + String.Format("{0:#.###}", tempep.Slope), (double)tempep.X, -(double)tempep.Y, CoordType.AxisXYScale,
                    AlignH.Right, AlignV.Center);
                text.FontSpec.FontColor = Color.Black;
                text.FontSpec.Fill.IsVisible = false;
                text.FontSpec.Border.IsVisible = false;
                text.FontSpec.IsBold = true;
                text.FontSpec.IsItalic = true;
                myPane.GraphObjList.Add(text);
            }


            /*
            foreach ( PointPairList list in this.endpts_1 )
                myPane.AddCurve( "Endpoint Lines", list, Color.Black, SymbolType.Square );

            
            foreach ( PointPairList list in this.endpts_2 )
                myPane.AddCurve( "endpts", list, Color.Yellow, SymbolType.Square );

            foreach ( PointPairList list in this.endpts_3 )
                myPane.AddCurve( "endpts", list, Color.Green, SymbolType.Star );
        */

            // Calculate the Axis Scale Ranges
            zgc.AxisChange();

        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            SetSize();
        }

        private void SetSize()
        {
            zg1.Location = new System.Drawing.Point(10, 10);
            // Leave a small margin around the outside of the control
            zg1.Size = new Size(this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20);
        }

        private void zg1_Load(object sender, System.EventArgs e)
        {

        }

        private void plot_enter(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {

        }


    }
}
