using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using ImageRecognizer;
using Utilities;
using ShapeTemplates;

namespace TestImagePlusDollar
{
    public partial class Form1 : Form
    {
        InkOverlay m_InkOverlay;
        msInkToHMCSketch.InkSketch m_Sketch;
        List<ImageTemplate> m_ImageTemplates;
        List<DollarTemplate> m_DollarTemplates;
        List<BitmapSymbol> m_CompleteSymbols;
        List<BitmapSymbol> m_PartialSymbols;
        double allowedRotation = 0.0;
        List<double> rotations = new List<double>(new double[1] { 0.0 });

        public Form1()
        {
            InitializeComponent();
            m_InkOverlay = new InkOverlay();
            m_InkOverlay.AttachedControl = panel1;
            m_InkOverlay.Enabled = true;
            m_Sketch = new msInkToHMCSketch.InkSketch(m_InkOverlay.Ink);

            m_ImageTemplates = new List<ImageTemplate>();

            string dir = System.IO.Directory.GetCurrentDirectory();
            System.IO.DirectoryInfo info = System.IO.Directory.GetParent(dir);
            info = System.IO.Directory.GetParent(info.FullName);
            dir = info.FullName + "\\Training Data";
            m_DollarTemplates = DollarTemplate.LoadTemplates(dir);
            
            m_CompleteSymbols = new List<BitmapSymbol>();
            m_PartialSymbols = new List<BitmapSymbol>();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void radioButtonTrain_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonTrain.Checked)
                comboBox1.Enabled = true;
            else
                comboBox1.Enabled = false;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            if (radioButtonTrain.Checked)
                CreateSymbol(comboBox1.Text);
            else
                RecognizeSymbol();
        }

        private void RecognizeSymbol()
        {
            List<Sketch.Substroke> subs = new List<Sketch.Substroke>();
            List<Point> points = new List<Point>();
            foreach (Stroke s in m_InkOverlay.Ink.Strokes)
            {
                subs.Add(m_Sketch.GetSketchSubstrokeByInkId(s.Id));
                foreach (Point pt in s.GetPoints())
                    points.Add(pt);
            }
            

            //Rectangle bbox = m_InkOverlay.Ink.GetBoundingBox();
            //ImageSymbol complete = new ImageSymbol(points.ToArray(), bbox);
            Dictionary<Sketch.Substroke, GatePart> strokeResults = RecognizeStrokesDollar(subs);
            ImageTemplate unknown = new ImageTemplate(subs);
            string best = unknown.Recognize(m_ImageTemplates, strokeResults);
            MessageBox.Show(best);

            foreach (Stroke s in m_InkOverlay.Ink.Strokes)
            { }

            

            /*
            List<Sketch.Substroke> subs = new List<Sketch.Substroke>();
            foreach (Stroke s in m_InkOverlay.Ink.Strokes)
                subs.Add(m_Sketch.GetSketchSubstrokeByInkId(s.Id));

            BitmapSymbol unknown = new BitmapSymbol(subs);
            List<ImageScore> results = unknown.FindBestMatches(m_CompleteSymbols, 
                10, allowedRotation, rotations);
            VisualizeSearch vsForm = new VisualizeSearch(unknown, results);
            vsForm.Show();
            vsForm.moveChildren();

            if (results.Count == 0)
                return;

            label1.Text = results[0].SymbolType;

            Rectangle box = m_InkOverlay.Ink.Strokes.GetBoundingBox();

            foreach (Sketch.Substroke s in subs)
            {
                BitmapSymbol us = new BitmapSymbol(s, box);
                List<ImageScore> rs = us.FindBestMatches(m_PartialSymbols, 10, allowedRotation, rotations);
                MessageBox.Show(rs[0].SymbolType);
                VisualizeSearch visualizeForm = new VisualizeSearch(us, rs);
                visualizeForm.Show();
                visualizeForm.moveChildren();
            }
            */

            m_InkOverlay.Ink.DeleteStrokes();
            this.Refresh();
        }

        public Dictionary<Sketch.Substroke, GatePart> RecognizeStrokesDollar(List<Sketch.Substroke> subs)
        {
            Dictionary<Sketch.Substroke, GatePart> strokeResults = new Dictionary<Sketch.Substroke, GatePart>(subs.Count);
            foreach (Sketch.Substroke s in subs)
            {
                DollarTemplate dollar = new DollarTemplate(s.PointsL);
                Dictionary<DollarTemplate, double> dResults = dollar.RecognizeSymbol(m_DollarTemplates);
                if (dResults == null)
                    return strokeResults;

                KeyValuePair<DollarTemplate, double> top = new KeyValuePair<DollarTemplate, double>();
                foreach (KeyValuePair<DollarTemplate, double> pair in dResults)
                {
                    top = pair;
                    break;
                }
                DollarTemplate Tbest = top.Key;
                switch (Tbest.Name)
                {
                    case "BackLine":
                        strokeResults.Add(s, GatePart.BackLine);
                        break;
                    case "Or_Left":
                        strokeResults.Add(s, GatePart.BackArc);
                        break;
                    case "Or_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "Or_RightTop":
                        strokeResults.Add(s, GatePart.TopArc);
                        break;
                    case "Or_RightBottom":
                        strokeResults.Add(s, GatePart.BottomArc);
                        break;
                    case "And_Left":
                        strokeResults.Add(s, GatePart.BackLine);
                        break;
                    case "And_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "And_Or_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "Not_Triangle":
                        strokeResults.Add(s, GatePart.Triangle);
                        break;
                    case "Not_Bubble":
                        strokeResults.Add(s, GatePart.Bubble);
                        break;
                    case "Bubble":
                        strokeResults.Add(s, GatePart.Bubble);
                        break;
                }

            }

            return strokeResults;
        }

        public Dictionary<Sketch.Substroke, GatePart> RecognizeStrokesRubine(List<Sketch.Substroke> subs)
        {
            Dictionary<Sketch.Substroke, GatePart> strokeResults = new Dictionary<Sketch.Substroke, GatePart>(subs.Count);
            foreach (Sketch.Substroke s in subs)
            {
                DollarTemplate dollar = new DollarTemplate(s.PointsL);
                Dictionary<DollarTemplate, double> dResults = dollar.RecognizeSymbol(m_DollarTemplates);
                if (dResults == null)
                    return strokeResults;

                KeyValuePair<DollarTemplate, double> top = new KeyValuePair<DollarTemplate, double>();
                foreach (KeyValuePair<DollarTemplate, double> pair in dResults)
                {
                    top = pair;
                    break;
                }
                DollarTemplate Tbest = top.Key;
                switch (Tbest.Name)
                {
                    case "BackLine":
                        strokeResults.Add(s, GatePart.BackLine);
                        break;
                    case "Or_Left":
                        strokeResults.Add(s, GatePart.BackArc);
                        break;
                    case "Or_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "Or_RightTop":
                        strokeResults.Add(s, GatePart.TopArc);
                        break;
                    case "Or_RightBottom":
                        strokeResults.Add(s, GatePart.BottomArc);
                        break;
                    case "And_Left":
                        strokeResults.Add(s, GatePart.BackLine);
                        break;
                    case "And_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "And_Or_Right":
                        strokeResults.Add(s, GatePart.FrontArc);
                        break;
                    case "Not_Triangle":
                        strokeResults.Add(s, GatePart.Triangle);
                        break;
                    case "Not_Bubble":
                        strokeResults.Add(s, GatePart.Bubble);
                        break;
                    case "Bubble":
                        strokeResults.Add(s, GatePart.Bubble);
                        break;
                }

            }

            return strokeResults;
        }

        private void CreateSymbol(string name)
        {
            if (name == "")
            {
                MessageBox.Show("You must specify a name for the symbol");
                return;
            }

            int numStrokesExpected = -1;
            if (name.Contains("2-Stroke"))
                numStrokesExpected = 2;
            else if (name.Contains("3-Stroke"))
                numStrokesExpected = 3;
            else if (name.Contains("4-Stroke"))
                numStrokesExpected = 4;

            if (m_InkOverlay.Ink.Strokes.Count != numStrokesExpected)
            {
                MessageBox.Show("The number of strokes expected is not the same as the number of strokes present.");
                return;
            }

            if (name == "AND (2-Stroke)")
            {
                //CreateAND2();
                CreateAND2_new();
            }
            else if (name == "OR (2-Stroke)")
            {
                //CreateOR2();
                CreateOR2_new();
            }
            else if (name == "NOT (2-Stroke)")
            {
                //CreateNOT2();
                CreateNOT2_new();
            }
            else if (name == "OR (3-Stroke)")
                CreateOR3();
            else
            {
                MessageBox.Show("Unrecognized Symbol Name");
                return;
            }

            m_InkOverlay.Ink.DeleteStrokes();
            
            this.Refresh();
        }

        private void CreateNOT2_new()
        {
            Create2StrokeLR_new(Gate.NOT, GatePart.Triangle, GatePart.Bubble);
        }

        private void CreateOR2_new()
        {
            Create2StrokeLR_new(Gate.OR, GatePart.BackArc, GatePart.FrontArc);
        }

        private void CreateAND2_new()
        {
            Create2StrokeLR_new(Gate.AND, GatePart.BackLine, GatePart.FrontArc);
        }

        private void Create2StrokeLR_new(Gate name, GatePart leftPart, GatePart rightPart)
        {
            if (m_InkOverlay.Ink.Strokes.Count != 2)
            {
                MessageBox.Show("Incorrect # of strokes present");
                return;
            }

            Rectangle b0 = m_InkOverlay.Ink.Strokes[0].GetBoundingBox();
            Rectangle b1 = m_InkOverlay.Ink.Strokes[1].GetBoundingBox();
            Rectangle bLeft, bRight;
            Stroke msLeft, msRight;
            if (b0.Right < b1.Right)
            {
                msLeft = m_InkOverlay.Ink.Strokes[0];
                msRight = m_InkOverlay.Ink.Strokes[1];
                bLeft = b0;
                bRight = b1;
            }
            else
            {
                msLeft = m_InkOverlay.Ink.Strokes[1];
                msRight = m_InkOverlay.Ink.Strokes[0];
                bLeft = b1;
                bRight = b0;
            }


            Sketch.Substroke Left = m_Sketch.GetSketchSubstrokeByInkId(msLeft.Id);
            Sketch.Substroke Right = m_Sketch.GetSketchSubstrokeByInkId(msRight.Id);
            List<Sketch.Substroke> strokes = new List<Sketch.Substroke>();
            Dictionary<Sketch.Substroke, GatePart> strokeInfo = new Dictionary<Sketch.Substroke, GatePart>();
            strokes.Add(Left);
            strokeInfo.Add(Left, leftPart);
            strokes.Add(Right);
            strokeInfo.Add(Right, rightPart);

            Rectangle shapeBbox = m_InkOverlay.Ink.Strokes.GetBoundingBox();
            SymbolInfo info = new SymbolInfo(new User("Tester"), name.ToString(), "Gate");
            ImageTemplate complete = new ImageTemplate(strokes, strokeInfo, info);

            m_ImageTemplates.Add(complete);
        }

        private void CreateOR3()
        {
            if (m_InkOverlay.Ink.Strokes.Count != 3)
            {
                MessageBox.Show("Incorrect # of strokes present");
                return;
            }

            Rectangle b0 = m_InkOverlay.Ink.Strokes[0].GetBoundingBox();
            Rectangle b1 = m_InkOverlay.Ink.Strokes[1].GetBoundingBox();
            Rectangle b2 = m_InkOverlay.Ink.Strokes[2].GetBoundingBox();
            Stroke msLeft, msTop, msBottom;
            Rectangle bLeft, bTop, bBottom;

            if (b0.Right < b1.Right && b0.Right < b2.Right)
            {
                msLeft = m_InkOverlay.Ink.Strokes[0];
                bLeft = b0;
                if (b1.Top > b2.Top)
                {
                    msTop = m_InkOverlay.Ink.Strokes[1];
                    msBottom = m_InkOverlay.Ink.Strokes[2];
                    bTop = b1;
                    bBottom = b2;
                }
                else
                {
                    msTop = m_InkOverlay.Ink.Strokes[2];
                    msBottom = m_InkOverlay.Ink.Strokes[1];
                    bTop = b2;
                    bBottom = b1;
                }
            }
            else if (b1.Right < b2.Right && b1.Right < b0.Right)
            {
                msLeft = m_InkOverlay.Ink.Strokes[1];
                bLeft = b1;
                if (b0.Top > b2.Top)
                {
                    msTop = m_InkOverlay.Ink.Strokes[0];
                    msBottom = m_InkOverlay.Ink.Strokes[2];
                    bTop = b0;
                    bBottom = b2;
                }
                else
                {
                    msTop = m_InkOverlay.Ink.Strokes[2];
                    msBottom = m_InkOverlay.Ink.Strokes[0];
                    bTop = b2;
                    bBottom = b0;
                }             
            }
            else
            {
                msLeft = m_InkOverlay.Ink.Strokes[2];
                bLeft = b2;
                if (b0.Top > b1.Top)
                {
                    msTop = m_InkOverlay.Ink.Strokes[0];
                    msBottom = m_InkOverlay.Ink.Strokes[1];
                    bTop = b0;
                    bBottom = b1;
                }
                else
                {
                    msTop = m_InkOverlay.Ink.Strokes[1];
                    msBottom = m_InkOverlay.Ink.Strokes[0];
                    bTop = b1;
                    bBottom = b0;
                }
            }

            Sketch.Substroke Left = m_Sketch.GetSketchSubstrokeByInkId(msLeft.Id);
            Sketch.Substroke Top = m_Sketch.GetSketchSubstrokeByInkId(msTop.Id);
            Sketch.Substroke Bottom = m_Sketch.GetSketchSubstrokeByInkId(msBottom.Id);
            List<Sketch.Substroke> strokes = new List<Sketch.Substroke>();
            strokes.Add(Left);
            strokes.Add(Top);
            strokes.Add(Bottom);

            Rectangle shapeBbox = m_InkOverlay.Ink.Strokes.GetBoundingBox();
            BitmapSymbol complete = new BitmapSymbol(strokes);
            BitmapSymbol leftPartial = new BitmapSymbol(Left, shapeBbox);// bLeft);
            BitmapSymbol topPartial = new BitmapSymbol(Top, shapeBbox);// bTop);
            BitmapSymbol bottomPartial = new BitmapSymbol(Bottom, shapeBbox);// bBottom);

            complete.Name = "OR_3";
            complete.SymbolType = "OR_3";
            complete.SymbolClass = "Gate";
            complete.Platform = PlatformUsed.TabletPC;
            complete.DrawTask = DrawingTask.Synthesize;
            complete.Completeness = SymbolCompleteness.Complete;


            leftPartial.Name = "OR_3-Left";
            leftPartial.SymbolType = "OR_3-Left";
            leftPartial.SymbolClass = "Gate";
            leftPartial.Platform = PlatformUsed.TabletPC;
            leftPartial.DrawTask = DrawingTask.Synthesize;
            leftPartial.Completeness = SymbolCompleteness.Partial;

            topPartial.Name = "OR_3-topRight";
            topPartial.SymbolType = "OR_3-topRight";
            topPartial.SymbolClass = "Gate";
            topPartial.Platform = PlatformUsed.TabletPC;
            topPartial.DrawTask = DrawingTask.Synthesize;
            topPartial.Completeness = SymbolCompleteness.Partial;

            bottomPartial.Name = "OR_3-bottomRight";
            bottomPartial.SymbolType = "OR_3-bottomRight";
            bottomPartial.SymbolClass = "Gate";
            bottomPartial.Platform = PlatformUsed.TabletPC;
            bottomPartial.DrawTask = DrawingTask.Synthesize;
            bottomPartial.Completeness = SymbolCompleteness.Partial;

            m_CompleteSymbols.Add(complete);
            m_PartialSymbols.Add(leftPartial);
            m_PartialSymbols.Add(topPartial);
            m_PartialSymbols.Add(bottomPartial);
        }

        private void Create2StrokeLR(string name, string leftName, string rightName)
        {
            if (m_InkOverlay.Ink.Strokes.Count != 2)
            {
                MessageBox.Show("Incorrect # of strokes present");
                return;
            }

            Rectangle b0 = m_InkOverlay.Ink.Strokes[0].GetBoundingBox();
            Rectangle b1 = m_InkOverlay.Ink.Strokes[1].GetBoundingBox();
            Rectangle bLeft, bRight;
            Stroke msLeft, msRight;
            if (b0.Right < b1.Right)
            {
                msLeft = m_InkOverlay.Ink.Strokes[0];
                msRight = m_InkOverlay.Ink.Strokes[1];
                bLeft = b0;
                bRight = b1;
            }
            else
            {
                msLeft = m_InkOverlay.Ink.Strokes[1];
                msRight = m_InkOverlay.Ink.Strokes[0];
                bLeft = b1;
                bRight = b0;
            }

            Sketch.Substroke Left = m_Sketch.GetSketchSubstrokeByInkId(msLeft.Id);
            Sketch.Substroke Right = m_Sketch.GetSketchSubstrokeByInkId(msRight.Id);
            List<Sketch.Substroke> strokes = new List<Sketch.Substroke>();
            strokes.Add(Left);
            strokes.Add(Right);

            Rectangle shapeBbox = m_InkOverlay.Ink.Strokes.GetBoundingBox();
            BitmapSymbol complete = new BitmapSymbol(strokes);
            BitmapSymbol leftPartial = new BitmapSymbol(Left, shapeBbox);// bLeft);
            BitmapSymbol rightPartial = new BitmapSymbol(Right, shapeBbox);// bRight);

            complete.Name = name;
            complete.SymbolType = name;
            complete.SymbolClass = "Gate";
            complete.Platform = PlatformUsed.TabletPC;
            complete.DrawTask = DrawingTask.Synthesize;
            complete.Completeness = SymbolCompleteness.Complete;


            leftPartial.Name = leftName;
            leftPartial.SymbolType = leftName;
            leftPartial.SymbolClass = "Gate";
            leftPartial.Platform = PlatformUsed.TabletPC;
            leftPartial.DrawTask = DrawingTask.Synthesize;
            leftPartial.Completeness = SymbolCompleteness.Partial;

            rightPartial.Name = rightName;
            rightPartial.SymbolType = rightName;
            rightPartial.SymbolClass = "Gate";
            rightPartial.Platform = PlatformUsed.TabletPC;
            rightPartial.DrawTask = DrawingTask.Synthesize;
            rightPartial.Completeness = SymbolCompleteness.Partial;

            m_CompleteSymbols.Add(complete);
            m_PartialSymbols.Add(leftPartial);
            m_PartialSymbols.Add(rightPartial);
        }

        private void CreateNOT2()
        {
            Create2StrokeLR("NOT_2", "NOT_2-Triangle", "NOT_2-Bubble");
        }

        private void CreateAND2()
        {
            Create2StrokeLR("AND_2", "AND_2-Left", "AND_2-Right");
        }

        private void CreateOR2()
        {
            Create2StrokeLR("OR_2", "OR_2-Left", "OR_2-Right");
        }

        
    }
}