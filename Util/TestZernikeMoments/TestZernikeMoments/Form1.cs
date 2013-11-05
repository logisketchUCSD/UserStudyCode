using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Ink;
using ZernikeMomentRecognizer;
using Utilities;

namespace TestZernikeMoments
{
    public partial class Form1 : Form
    {
        InkOverlay ink;
        List<ZernikeMoment> zMoments;

        public Form1()
        {
            InitializeComponent();
            ink = new InkOverlay(panel1);
            ink.Enabled = true;
            zMoments = new List<ZernikeMoment>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Point> points = new List<Point>();
            foreach (Stroke s in ink.Ink.Strokes)
                foreach (Point pt in s.GetPoints())
                    points.Add(pt);

            zMoments.Add(new ZernikeMoment(comboBox1.Text, points.ToArray()));

            ink.Ink.DeleteStrokes();
            this.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Print();
        }

        private void Print()
        {
            bool success;
            string filename = General.SelectSaveFile(out success, "File to save Zernike Features to", "Weka Text File (*.arff)|*.arff");

            StreamWriter writer = new StreamWriter(filename);

            PrintHeader(writer);

            foreach (ZernikeMoment zm in zMoments)
                zm.Print(writer);

            writer.Close();
        }

        private void PrintHeader(StreamWriter writer)
        {
            #region Write Header
            writer.WriteLine("@RELATION zernikeFeatures");
            writer.WriteLine();
            writer.WriteLine("@ATTRIBUTE A00 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A11 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A20 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A22 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A31 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A33 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A40 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A42 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A44 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A51 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A53 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A55 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A60 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A62 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A64 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A66 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A71 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A73 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A75 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A77 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A80 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A82 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A84 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A86 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A88 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A91 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A93 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A95 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A97 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A99 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A100 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A102 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A104 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A106 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A108 NUMERIC");
            writer.WriteLine("@ATTRIBUTE A1010 NUMERIC");
            writer.WriteLine("@ATTRIBUTE class {AND,OR,NAND,NOR,NOT,XOR,NOTBUBBLE}");
            writer.WriteLine();
            writer.WriteLine("@DATA");
            #endregion
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}