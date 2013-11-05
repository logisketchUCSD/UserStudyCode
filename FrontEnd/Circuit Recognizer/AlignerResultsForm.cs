using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ImageAligner;
using Microsoft.Ink;

namespace Circuit_Recognizer
{
    public partial class AlignerResultsForm : Form
    {
        public AlignerResultsForm()
        {
            InitializeComponent();
        }

        public AlignerResultsForm(ShapeResult result)
        {
            InitializeComponent();

            // Initialize ink panels
            InitializePanel(panelGroupedInk, result.GroupedShape);
            InitializePanel(panelBestShapeInk, result.BestMatchingShape);
            // Set labels Actual
            labelActualShapeName.Text = result.ExpectedShapeName;
            List<string> missing = new List<string>();
            List<string> extra = new List<string>();
            foreach (ImageMatchError error in result.ExpectedErrors)
            {
                if (error.Type == ErrorType.Missing)
                    missing.Add(error.Detail.ToString());
                else
                    extra.Add(error.Detail.ToString());
            }
            foreach (string name in missing)
                labelActualMissing.Text += "\n  -" + name;
            foreach (string name in extra)
                labelActualExtra.Text += "\n  -" + name;


            if (result.TopResult != null)
            {
                InitializePanel(panelResult1Ink, result.TopResult);
                // Set labels Result 1
                labelResult1Name.Text = result.TopResult.Name + ": " + result.TopResult.Score.ToString("#0.000") + " (" + result.TopResult.Confidence.ToString() + ")";
                missing = new List<string>();
                extra = new List<string>();
                foreach (ImageMatchError error in result.TopResult.Errors)
                {
                    if (error.Type == ErrorType.Missing)
                        missing.Add(error.Detail.ToString());
                    else
                        extra.Add(error.Detail.ToString());
                }
                foreach (string name in missing)
                    labelResult1Missing.Text += "\n  -" + name;
                foreach (string name in extra)
                    labelResult1Extra.Text += "\n  -" + name;
            }


            if (result.ResultNum2 != null)
            {
                InitializePanel(panelResult2Ink, result.ResultNum2);
                // Set labels Result 2
                labelResult2Name.Text = result.ResultNum2.Name + ": " + result.ResultNum2.Score.ToString("#0.000") + " (" + result.ResultNum2.Confidence.ToString() + ")";
                missing = new List<string>();
                extra = new List<string>();
                foreach (ImageMatchError error in result.ResultNum2.Errors)
                {
                    if (error.Type == ErrorType.Missing)
                        missing.Add(error.Detail.ToString());
                    else
                        extra.Add(error.Detail.ToString());
                }
                foreach (string name in missing)
                    labelResult2Missing.Text += "\n  -" + name;
                foreach (string name in extra)
                    labelResult2Extra.Text += "\n  -" + name;
            }


            if (result.ResultNum3 != null)
            {
                InitializePanel(panelResult3Ink, result.ResultNum3);
                // Set labels Result 2
                labelResult3Name.Text = result.ResultNum3.Name + ": " + result.ResultNum3.Score.ToString("#0.000") + " (" + result.ResultNum3.Confidence.ToString() + ")";
                missing = new List<string>();
                extra = new List<string>();
                foreach (ImageMatchError error in result.ResultNum3.Errors)
                {
                    if (error.Type == ErrorType.Missing)
                        missing.Add(error.Detail.ToString());
                    else
                        extra.Add(error.Detail.ToString());
                }
                foreach (string name in missing)
                    labelResult3Missing.Text += "\n  -" + name;
                foreach (string name in extra)
                    labelResult3Extra.Text += "\n  -" + name;
            }
        }

        private void InitializePanel(Panel panel, Sketch.Shape shape)
        {
            InkOverlay ink = new InkOverlay(panel);
            foreach (Sketch.Substroke s in shape.SubstrokesL)
            {
                ink.Ink.CreateStroke(s.PointsAsSysPoints);
                ink.Ink.Strokes[ink.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Black;
            }
            ScaleAndMoveInk(ref ink, panel);
        }

        private void InitializePanel(Panel panel, ImageTemplateResult result)
        {
            InkOverlay ink = new InkOverlay(panel);
            foreach (Sketch.Substroke s in result.SubstrokesUsedInMatch)
            {
                ink.Ink.CreateStroke(s.PointsAsSysPoints);
                ink.Ink.Strokes[ink.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Black;
            }
            foreach (Sketch.Substroke s in result.SubstrokesNOTUsedInMatch)
            {
                ink.Ink.CreateStroke(s.PointsAsSysPoints);
                ink.Ink.Strokes[ink.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Red;
            }
            ScaleAndMoveInk(ref ink, panel);
        }

        private void ScaleAndMoveInk(ref InkOverlay ink, Panel panel)
        {
            if (ink.Ink.Strokes.Count == 0)
                return;

            Rectangle box = ink.Ink.GetBoundingBox();
            Graphics g = panel.CreateGraphics();

            float scaleX = 1.0f;
            float scaleY = 1.0f;
            float Scale = 1.0f;

            Point pt = new Point(panel.Width, panel.Height);

            ink.Renderer.PixelToInkSpace(g, ref pt);

            scaleX = (float)pt.X / (float)box.Width * 0.9f;
            scaleY = (float)pt.Y / (float)box.Height * 0.9f;

            Scale = Math.Min(scaleX, scaleY);

            ink.Ink.Strokes.Scale(Scale, Scale);

            float offset = 300.0f;

            box = ink.Ink.GetBoundingBox();

            float InkMovedX = -(float)box.X + offset;
            float InkMovedY = -(float)(box.Y) + offset;

            box = ink.Ink.GetBoundingBox();
            ink.Ink.Strokes.Move(InkMovedX, InkMovedY);
        }
    }
}