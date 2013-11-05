using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using ConverterJnt;


namespace NDollarTester
{
    public partial class N_Dollar_Form : Form
    {
        InkOverlay m_Ink;
        NDollar m_Recognizer;
        ReadJnt m_ReadJnt;

        public N_Dollar_Form()
        {
            InitializeComponent();

            m_Ink = new InkOverlay(inkPanel);
            m_Ink.Enabled = true;

            m_Recognizer = new NDollar();

            m_ReadJnt = new ReadJnt();
        }

        private void buttonClearInk_Click(object sender, EventArgs e)
        {
            ClearInk();
        }

        private void ClearInk()
        {
            m_Ink.Ink.DeleteStrokes();
            this.Refresh();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Sketch.Shape shape = MakeShape();
            m_Recognizer.addExample(textBox1.Text, shape);

            ClearInk();
        }

        private Sketch.Shape MakeShape()
        {
            List<Sketch.Substroke> strokes = new List<Sketch.Substroke>();

            foreach (Stroke stroke in m_Ink.Ink.Strokes)
            {
                Sketch.Stroke s = m_ReadJnt.InkStroke2SketchStroke(stroke);
                strokes.Add(s.SubstrokesL[0]);
            }
            
            Sketch.Shape shape = new Sketch.Shape(strokes, new Sketch.XmlStructs.XmlShapeAttrs());

            return shape;
        }

        private void buttonRecognize_Click(object sender, EventArgs e)
        {
            Sketch.Shape shape = MakeShape();
            double score;
            string result = m_Recognizer.classify(shape, out score);
            labelResult.Text = result +": " + score.ToString("#0.000");
        }
    }
}