using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using ImageRecognizer;

namespace Classifier
{
    public partial class ClusterForm : Form
    {
        private InkOverlay _OverlayInk;
        private float _InkMovedX;
        private float _InkMovedY;
        private float _Scale;

        public ClusterForm(Strokes strokes, List<Sketch.Substroke> substrokes, Dictionary<string, List<SymbolRank>> SRs, BitmapSymbol Unknown)
        {
            InitializeComponent();

            InkPanel.Enabled = true;
            _OverlayInk = new Microsoft.Ink.InkOverlay();
            _OverlayInk.AttachedControl = InkPanel;
            _OverlayInk.Enabled = false;

            FillInkOverlay(strokes);
            FillLabels(substrokes, SRs);
        }

        /// <summary>
        /// Populates the stroke objects in an Ink Overlay object using the
        /// substrokes in a sketch object
        /// </summary>
        /// <param name="sketch">Sketch containing substrokes to convert</param>
        private void FillInkOverlay(Strokes strokes)
        {
            _OverlayInk.Ink.DeleteStrokes();
            foreach (Stroke s in strokes)
                _OverlayInk.Ink.CreateStroke(s.GetPoints());

            // Move center the ink's origin to the top-left corner
            Rectangle bb = _OverlayInk.Ink.GetBoundingBox();
            _InkMovedX = 0.0f;
            _InkMovedY = 0.0f;
            _Scale = 1.0f;

            ScaleAndMoveInk();
            this.InkPanel.Refresh();
        }

        /// <summary>
        /// Find the best scale for the ink based on bounding box size compared to the panel size.
        /// Then actually scale the ink in both x and y directions after finding the best scale.
        /// Then move the ink to the top left corner of the panel with some padding on left and top.
        /// </summary>
        private void ScaleAndMoveInk()
        {
            if (_OverlayInk.Ink.Strokes.Count == 0)
                return;

            Rectangle box = _OverlayInk.Ink.GetBoundingBox();

            float offset = 300.0f;

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            System.Drawing.Point pt = new System.Drawing.Point(this.InkPanel.Width, this.InkPanel.Height);

            _OverlayInk.Renderer.PixelToInkSpace(this.CreateGraphics(), ref pt);

            scaleX = ((float)pt.X - offset) / (float)box.Width * 0.9f;
            scaleY = ((float)pt.Y - offset) / (float)box.Height * 0.9f;

            this._Scale = Math.Min(scaleX, scaleY);

            _OverlayInk.Ink.Strokes.Scale(_Scale, _Scale);


            box = _OverlayInk.Ink.GetBoundingBox();

            this._InkMovedX = -(float)box.X + offset;
            this._InkMovedY = -(float)(box.Y) + offset;

            box = _OverlayInk.Ink.GetBoundingBox();
            _OverlayInk.Ink.Strokes.Move(_InkMovedX, _InkMovedY);
        }

        private void FillLabels(List<Sketch.Substroke> strokes, Dictionary<string, List<SymbolRank>> SRs)
        {
            string name = SRs["Fusio"][0].SymbolName;
            double distance = SRs["Fusio"][0].Distance;
            
            label1.Text = name + "   Score: " + distance.ToString("#0.00");

            BitmapSymbol bestSymbol = SRs["Fusio"][0].Symbol;

            BitmapSymbol UnknownSymbol = new BitmapSymbol(strokes);
            UnknownSymbol.Process();

            double[] scores = BitmapSymbol.Compare(bestSymbol, UnknownSymbol);

            label6.Text += scores[1].ToString("#0.00");
            label7.Text += scores[2].ToString("#0.00");
            label8.Text += scores[3].ToString("#0.00");
            label9.Text += scores[4].ToString("#0.00");

            if (SRs["Fusio"].Count > 1)
            {
                name = SRs["Fusio"][1].SymbolName;
                distance = SRs["Fusio"][1].Distance;
                label2.Text = name + "   Score: " + distance.ToString("#0.00");
            }

            if (SRs["Fusio"].Count > 2)
            {
                name = SRs["Fusio"][2].SymbolName;
                distance = SRs["Fusio"][2].Distance;
                label3.Text = name + "   Score: " + distance.ToString("#0.00");
            }

            if (SRs["Fusio"].Count > 3)
            {
                name = SRs["Fusio"][3].SymbolName;
                distance = SRs["Fusio"][3].Distance;
                label4.Text = name + "   Score: " + distance.ToString("#0.00");
            }

            if (SRs["Fusio"].Count > 4)
            {
                name = SRs["Fusio"][4].SymbolName;
                distance = SRs["Fusio"][4].Distance;
                label5.Text = name + "   Score: " + distance.ToString("#0.00");
            }
        }
    }
}