using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ImageAligner;
using ConverterXML;
using Sketch;
using Utilities;

namespace TestImageAligner
{
    public partial class Form1 : Form
    {
        ImageAlignerRecognizer m_Recognizer;

        public Form1()
        {
            InitializeComponent();

            m_Recognizer = new ImageAlignerRecognizer();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadTestSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool openSuccess;
            List<string> sketches = General.SelectOpenFiles(out openSuccess, "Sketches containing test shapes",
                "Labeled XML Sketches (*.labeled.xml)|*.labeled.xml");

            if (!openSuccess)
            {
                MessageBox.Show("Unable to load sketches");
                return;
            }

            //m_Recognizer = ImageAlignerRecognizer.Load("C:\\Reco.iar");
            //m_Recognizer = ImageAlignerRecognizer.Load("C:\\Documents and Settings\\eric\\My Documents\\Trunk\\Code\\Recognition\\ImageAligner\\TrainedRecognizers\\ImageAlignerRecognizerNBEST.iar");

            int numRight = 0;
            int numWrong = 0;
            Dictionary<Shape, ImageTemplateResult> results = new Dictionary<Shape, ImageTemplateResult>();

            foreach (string sketchFile in sketches)
            {
                Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                sketch = General.ReOrderParentShapes(sketch);

                foreach (Shape shape in sketch.Shapes)
                    if (General.IsGate(shape))
                        if (shape.Substrokes[0].Labels[0] == shape.Label)
                        {
                            ImageTemplateResult result = m_Recognizer.Recognize(shape);
                            results.Add(shape, result);
                            if (result != null && result.Name == shape.Label)
                                numRight++;
                            else
                                numWrong++;
                        }
            }
        }

        private void loadTemplatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool openSuccess;
            List<string> sketches = General.SelectOpenFiles(out openSuccess, "Sketches containing template shapes",
                "Labeled (including gate parts) XML Sketches (*.labeled.xml)|*.labeled.xml");

            if (!openSuccess)
            {
                MessageBox.Show("Unable to load sketches");
                return;
            }

            foreach (string sketchFile in sketches)
            {
                Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                sketch = General.ReOrderParentShapes(sketch);

                foreach (Shape shape in sketch.Shapes)
                    if (General.IsGate(shape))
                        if (shape.Substrokes[0].Labels[0] == shape.Label)
                            m_Recognizer.Add(shape);
            }

            m_Recognizer.Save("C:\\Reco.iar");
        }

        private void loadRecognizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            string filename = General.SelectOpenFile(out success,
                "Select the trained Image Aligner Recognizer to load",
                "ImageAligner Recognizer (*.iar)|*.iar");

            if (success)
                m_Recognizer = ImageAlignerRecognizer.Load(filename);
            else
                MessageBox.Show("No valid file selected");
        }

        private void accuracyOnShapesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            string filename = General.SelectOpenFile(out success,
                "Test Sketches with associated grouped shapes",
                "TestSketches (*.ts)|*.ts");
            if (!success)
                return;


        }
    }
}