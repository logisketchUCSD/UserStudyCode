using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Cluster;
using Sketch;
using Microsoft.Ink;

namespace Classifier
{
    public partial class ClusterAccuracyForm : Form
    {
        private ClusteringResultsSketch results;
        private int currentResultsNumber = -1;
        private const float paddingOffset = 200.0f;
        private InkOverlay clusterInkOverlay;
        private Dictionary<Substroke, Color> strokeColor;

        public ClusterAccuracyForm(ClusteringResultsSketch results)
        {
            InitializeComponent();

            this.results = results;
            strokeColor = new Dictionary<Substroke, Color>(results.Sketch.Substrokes.Length);
            FindColors(results);

            UpdateCurrentResultsNumber(1);

            this.clusterInkPanel.Enabled = true;
            clusterInkOverlay = new InkOverlay();
            clusterInkOverlay.AttachedControl = this.clusterInkPanel;
            clusterInkOverlay.Enabled = false;
            if (currentResultsNumber >= 0)
                FillInkOverlay(results.ResultsShape[currentResultsNumber], clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);
            else
                FillInkOverlay(new Sketch.Sketch(), clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);

            this.sketchInkPanel.Enabled = true;
            InkOverlay sketchInkOverlay = new InkOverlay();
            sketchInkOverlay.AttachedControl = this.sketchInkPanel;
            sketchInkOverlay.Enabled = false;
            FillInkOverlay(results.Sketch, sketchInkOverlay, sketchInkPanel, paddingOffset, results.StrokeClassifications);

            labelTotalNumErrors.Text += results.NumTotalErrors.ToString();
            labelTotalPerfectClusters.Text += results.NumPerfect.ToString();
            labelConditionalPerfectClusters.Text += results.NumConditionalPerfect.ToString();
            labelSplitErrors.Text += results.NumSplitErrors.ToString();
            labelMergeErrors.Text += results.NumMergeErrors.ToString();
            
            labelMergeShape2ShapeErrors.Text += results.NumMergedShapeToShapeErrors.ToString();
            labelMergedWire2ShapeErrors.Text += results.NumMergedConnectorToShapeErrors.ToString();
            labelMergedText2ShapeErrors.Text += results.NumMergedTextToShapeErrors.ToString();
            labelMergeShape2Text.Text += results.NumMergedShapeToTextErrors.ToString();
            labelMergeWire2Text.Text += results.NumMergedConnectorToTextErrors.ToString();
            labelMergeText2Text.Text += results.NumMergedTextToTextErrors.ToString();
            labelMergeNOTBUBBLE.Text += results.NumMergedNOTBUBBLEToShapeErrors.ToString();

            double percentage = results.InkMatchingPercentage * 100.0;
            labelInkMatchingPercentage.Text += percentage.ToString("#0.0") + "%";
            percentage = results.InkExtraPercentageTotal * 100.0;
            labelInkExtraTotalPercentage.Text += percentage.ToString("#0.0") + "%";
            percentage = results.InkExtraPercentageBestMatches * 100.0;
            labelInkExtraFromBestClusters.Text += percentage.ToString("#0.0") + "%";
            percentage = results.InkExtraPercentagePartialMatchesNotBest * 100.0;
            labelInkExtraPartialMatches.Text += percentage.ToString("#0.0") + "%";
            percentage = results.InkExtraPercentageCompletelyUnMatched * 100.0;
            labelInkExtraCompletelyUnmatched.Text += percentage.ToString("#0.0") + "%";
        }

        private void UpdateCurrentResultsNumber(int increment)
        {
            if (radioButtonErrors.Checked)
            {
                if (!results.ContainsCorrectnessLevel(-1))
                    currentResultsNumber = -1;
                else if (increment == 1)
                {
                    currentResultsNumber++;

                    if (currentResultsNumber >= this.results.ResultsShape.Count)
                        currentResultsNumber -= this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness < 0)
                        return;
                    else
                        UpdateCurrentResultsNumber(1);
                }
                else if (increment == -1)
                {
                    currentResultsNumber--;

                    if (currentResultsNumber < 0)
                        currentResultsNumber += this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness < 0)
                        return;
                    else
                        UpdateCurrentResultsNumber(-1);
                }
            }
            else if (radioButtonConditionalPerfect.Checked)
            {
                if (!results.ContainsCorrectnessLevel(0))
                    currentResultsNumber = -1;
                else if (increment == 1)
                {
                    currentResultsNumber++;

                    if (currentResultsNumber >= this.results.ResultsShape.Count)
                        currentResultsNumber -= this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness == 0)
                        return;
                    else
                        UpdateCurrentResultsNumber(1);
                }
                else if (increment == -1)
                {
                    currentResultsNumber--;

                    if (currentResultsNumber < 0)
                        currentResultsNumber += this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness == 0)
                        return;
                    else
                        UpdateCurrentResultsNumber(-1);
                }
            }
            else if (radioButtonPerfect.Checked)
            {
                if (!results.ContainsCorrectnessLevel(1))
                    currentResultsNumber = -1;
                else if (increment == 1)
                {
                    currentResultsNumber++;

                    if (currentResultsNumber >= this.results.ResultsShape.Count)
                        currentResultsNumber -= this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness == 1)
                        return;
                    else
                        UpdateCurrentResultsNumber(1);
                }
                else if (increment == -1)
                {
                    currentResultsNumber--;

                    if (currentResultsNumber < 0)
                        currentResultsNumber += this.results.ResultsShape.Count;

                    if (this.results.ResultsShape[currentResultsNumber].Correctness == 1)
                        return;
                    else
                        UpdateCurrentResultsNumber(-1);
                }
            }
        }

        #region Ink Handling

        private void FindColors(ClusteringResultsSketch results)
        {
            Dictionary<Guid, string> strokeClassifications = results.StrokeClassifications;
            foreach (ClusteringResultsShape shapeResults in results.ResultsShape)
            {
                List<Substroke> strokesAdded = new List<Substroke>();
                foreach (Substroke s in shapeResults.Shape.SubstrokesL)
                {
                    string type = GetShapeType(s.ParentShapes[0]);
                    if (strokeClassifications[s.Id] == type && shapeResults.BestCluster.contains(s.Id))
                    {
                        strokesAdded.Add(s);
                        if (!this.strokeColor.ContainsKey(s))
                            this.strokeColor.Add(s, Color.Green);
                        else
                        {
                        }
                    }
                    else if (strokeClassifications[s.Id] == type && !shapeResults.BestCluster.contains(s.Id))
                    {
                        strokesAdded.Add(s);
                        if (!this.strokeColor.ContainsKey(s))
                            this.strokeColor.Add(s, Color.Red);
                        else
                        {
                        }
                    }
                    else if (strokeClassifications[s.Id] != type && shapeResults.BestCluster.contains(s.Id))
                    {
                        strokesAdded.Add(s);
                        if (!this.strokeColor.ContainsKey(s))
                            this.strokeColor.Add(s, Color.Fuchsia);
                        else
                        {
                        }
                    }
                    else if (strokeClassifications[s.Id] != type && !shapeResults.BestCluster.contains(s.Id))
                    {
                        strokesAdded.Add(s);
                        if (!this.strokeColor.ContainsKey(s))
                            this.strokeColor.Add(s, Color.Yellow);
                        else
                        {
                        }
                    }
                }

                foreach (Substroke s in shapeResults.BestCluster.Strokes)
                {
                    if (!strokesAdded.Contains(s))
                    {
                        string type = GetShapeType(s.ParentShapes[0]);
                        if (strokeClassifications[s.Id] == type)
                        {
                            if (!this.strokeColor.ContainsKey(s))
                                this.strokeColor.Add(s, Color.Red);
                            else
                            {
                                this.strokeColor.Remove(s);
                                this.strokeColor.Add(s, Color.Red);
                            }
                        }
                        else if (strokeClassifications[s.Id] != type)
                        {
                            if (!this.strokeColor.ContainsKey(s))
                                this.strokeColor.Add(s, Color.Orange);
                            else
                            {
                                this.strokeColor.Remove(s);
                                this.strokeColor.Add(s, Color.Orange);
                            }
                        }
                    }
                }
            }

            foreach (Substroke s in results.Sketch.SubstrokesL)
            {
                if (!this.strokeColor.ContainsKey(s))
                    this.strokeColor.Add(s, Color.Black);
            }
        }

        /// <summary>
        /// Populates the stroke objects in an Ink Overlay object using the
        /// substrokes in a sketch object
        /// </summary>
        /// <param name="sketch">Sketch containing substrokes to convert</param>
        private void FillInkOverlay(Sketch.Sketch sketch, InkOverlay overlayInk, Panel inkPanel, float paddingOffset, Dictionary<Guid, string> strokeClassifications)
        {
            overlayInk.Ink.DeleteStrokes();
            foreach (Substroke s in sketch.Substrokes)
            {
                overlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                Color c = Color.Black;
                if (this.strokeColor.ContainsKey(s))
                    c = this.strokeColor[s];

                overlayInk.Ink.Strokes[overlayInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = c;
            }

            // Move center the ink's origin to the top-left corner
            Rectangle bb = overlayInk.Ink.GetBoundingBox();

            ScaleAndMoveInk(overlayInk, inkPanel, paddingOffset);
            UpdateColors(overlayInk, inkPanel);
        }

        /// <summary>
        /// Populates the stroke objects in an Ink Overlay object using the
        /// substrokes in a sketch object
        /// </summary>
        /// <param name="sketch">Sketch containing substrokes to convert</param>
        private void FillInkOverlay(ClusteringResultsShape shapeResults, InkOverlay overlayInk, Panel inkPanel, float paddingOffset, Dictionary<Guid, string> strokeClassifications)
        {
            overlayInk.Ink.DeleteStrokes();
            List<Substroke> strokesAdded = new List<Substroke>();
            foreach (Substroke s in shapeResults.Shape.SubstrokesL)
            {
                string type = GetShapeType(s.ParentShapes[0]);
                if (strokeClassifications[s.Id] == type && shapeResults.BestCluster.contains(s.Id))
                {
                    overlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                    if (this.strokeColor.ContainsKey(s))
                        overlayInk.Ink.Strokes[overlayInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = this.strokeColor[s];
                    strokesAdded.Add(s);
                }
                else if (strokeClassifications[s.Id] == type && !shapeResults.BestCluster.contains(s.Id))
                {
                    overlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                    if (this.strokeColor.ContainsKey(s))
                        overlayInk.Ink.Strokes[overlayInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = this.strokeColor[s];
                    strokesAdded.Add(s);
                }
            }

            foreach (Substroke s in shapeResults.BestCluster.Strokes)
            {
                if (!strokesAdded.Contains(s))
                {
                    string type = GetShapeType(s.ParentShapes[0]);
                    if (strokeClassifications[s.Id] == type)
                    {
                        overlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                        if (this.strokeColor.ContainsKey(s))
                            overlayInk.Ink.Strokes[overlayInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = this.strokeColor[s];
                    }
                    else if (strokeClassifications[s.Id] != type)
                    {
                        overlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                        if (this.strokeColor.ContainsKey(s))
                            overlayInk.Ink.Strokes[overlayInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = this.strokeColor[s];
                    }
                }
            }

            // Move center the ink's origin to the top-left corner
            Rectangle bb = overlayInk.Ink.GetBoundingBox();

            ScaleAndMoveInk(overlayInk, inkPanel, paddingOffset);
            UpdateColors(overlayInk, inkPanel);
        }

        /// <summary>
        /// Updates the colors of a sketch based on the lookup table
        /// </summary>
        private void UpdateColors(InkOverlay overlayInk, Panel inkPanel)
        {
            foreach (Microsoft.Ink.Stroke s in overlayInk.Ink.Strokes)
            {
                //s.DrawingAttributes.Color = _MStroke2Color[s.Id];
            }

            inkPanel.Refresh();
        }

        /// <summary>
        /// Find the best scale for the ink based on bounding box size compared to the panel size.
        /// Then actually scale the ink in both x and y directions after finding the best scale.
        /// Then move the ink to the top left corner of the panel with some padding on left and top.
        /// </summary>
        private void ScaleAndMoveInk(InkOverlay overlayInk, Panel inkPanel, float offset)
        {
            if (overlayInk.Ink.Strokes.Count == 0)
                return;

            Rectangle box = overlayInk.Ink.GetBoundingBox();

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            System.Drawing.Point pt = new System.Drawing.Point(inkPanel.Width, inkPanel.Height);

            overlayInk.Renderer.PixelToInkSpace(inkPanel.CreateGraphics(), ref pt);

            scaleX = (float)pt.X / (float)box.Width * 0.9f;
            scaleY = (float)pt.Y / (float)box.Height * 0.9f;

            float scale = Math.Min(scaleX, scaleY);

            overlayInk.Ink.Strokes.Scale(scale, scale);

            box = overlayInk.Ink.GetBoundingBox();

            float inkMovedX = -(float)(box.X) + offset;
            float inkMovedY = -(float)(box.Y) + offset;

            box = overlayInk.Ink.GetBoundingBox();
            overlayInk.Ink.Strokes.Move(inkMovedX, inkMovedY);
        }

        #endregion

        #region Check type of shape or stroke


        /// <summary>
        /// Gets a subset of the list of strokes in a shape which have been correctly classified.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="strokeClassifications"></param>
        /// <returns></returns>
        private List<Substroke> GetCorrectlyLabeledStrokes(Shape s, Dictionary<Guid, string> strokeClassifications)
        {
            string type = GetShapeType(s);

            // Create List of Correctly labeled strokes
            List<Substroke> strokes = new List<Substroke>();
            foreach (Substroke sub in s.SubstrokesL)
            {
                if (strokeClassifications.ContainsKey(sub.Id) && strokeClassifications[sub.Id] == type)
                    strokes.Add(sub);
            }

            return strokes;
        }

        /// <summary>
        /// Get the type of the shape (as a string)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string GetShapeType(Shape s)
        {
            if (IsGate(s))
                return "Other";
            else if (IsLabel(s))
                return "Label";
            else if (IsConnector(s))
                return "Connector";
            else
                return "none";
        }

        /// <summary>
        /// Determine whether a shape is part of a 'gate' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns></returns>
        private bool IsGate(Shape s)
        {
            List<string> gateShapes = new List<string>();
            gateShapes.Add("AND");
            gateShapes.Add("OR");
            gateShapes.Add("NAND");
            gateShapes.Add("NOR");
            gateShapes.Add("NOT");
            gateShapes.Add("NOTBUBBLE");
            gateShapes.Add("BUBBLE");
            gateShapes.Add("XOR");
            gateShapes.Add("XNOR");
            gateShapes.Add("LabelBox");
            gateShapes.Add("Male");
            gateShapes.Add("Female");

            return gateShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'gate' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a gate</returns>
        private bool IsGate(Substroke s)
        {
            List<string> gateShapes = new List<string>();
            gateShapes.Add("AND");
            gateShapes.Add("OR");
            gateShapes.Add("NAND");
            gateShapes.Add("NOR");
            gateShapes.Add("NOT");
            gateShapes.Add("NOTBUBBLE");
            gateShapes.Add("BUBBLE");
            gateShapes.Add("XOR");
            gateShapes.Add("XNOR");
            gateShapes.Add("LabelBox");
            gateShapes.Add("Male");
            gateShapes.Add("Female");

            return gateShapes.Contains(s.FirstLabel);
        }

        /// <summary>
        /// Determine whether a shape is part of a 'label' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns>whether it is a gate</returns>
        private bool IsLabel(Shape s)
        {
            List<string> labelShapes = new List<string>();
            labelShapes.Add("Label");
            labelShapes.Add("Text");

            return labelShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'label' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a gate</returns>
        private bool IsLabel(Substroke s)
        {
            List<string> labelShapes = new List<string>();
            labelShapes.Add("Label");
            labelShapes.Add("Text");

            return labelShapes.Contains(s.FirstLabel);
        }

        /// <summary>
        /// Determine whether a shape is part of a 'connector' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns>whether it is a connector</returns>
        private bool IsConnector(Shape s)
        {
            List<string> connectorShapes = new List<string>();
            connectorShapes.Add("Wire");
            connectorShapes.Add("ChildLink");
            connectorShapes.Add("Marriage");
            connectorShapes.Add("Divorce");

            return connectorShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'connector' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a connector</returns>
        private bool IsConnector(Substroke s)
        {
            List<string> connectorShapes = new List<string>();
            connectorShapes.Add("Wire");
            connectorShapes.Add("ChildLink");
            connectorShapes.Add("Marriage");
            connectorShapes.Add("Divorce");

            return connectorShapes.Contains(s.FirstLabel);
        }

        #endregion

        private void buttonPreviousCluster_Click(object sender, EventArgs e)
        {
            UpdateCurrentResultsNumber(-1);
            if (currentResultsNumber >= 0)
                FillInkOverlay(results.ResultsShape[currentResultsNumber], clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);
            else
                FillInkOverlay(new Sketch.Sketch(), clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);
        }

        private void buttonNextCluster_Click(object sender, EventArgs e)
        {
            UpdateCurrentResultsNumber(1);
            if (currentResultsNumber >= 0)
                FillInkOverlay(results.ResultsShape[currentResultsNumber], clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);
            else
                FillInkOverlay(new Sketch.Sketch(), clusterInkOverlay, clusterInkPanel, paddingOffset, results.StrokeClassifications);
        }
    }
}