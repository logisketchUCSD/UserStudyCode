using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Ink;

using CommandManagement;
using Domain;
using System.Windows;
using InkToSketchWPF;
using SketchPanelLib;

namespace EditMenu.CommandList
{
    public delegate void RegroupEventHandler(List<Sketch.Shape> shapes);
    public delegate void ErrorCorrectedEventHandler(Sketch.Shape shape);

	/// <summary>
	/// This class allows users to re-classify or re-label
    /// strokes as a different type. For example, a set of
    /// strokes might be accidentally recognized as part of a
    /// wire, in which case the user can select the portion
    /// he or she desires and re-label it as an AND gate or
    /// whatnot.
    /// 
    /// This class specifically deals with the relabeling 
    /// and regrouping portion for sketch substrokes and shapes.
	/// </summary>
	public class ApplyLabelCmd : Command
    {
        #region Internals
	
		/// <summary>
		/// Sketch to contain the labeled shape
		/// </summary>
        private SketchPanel sketchPanel;

		/// <summary>
		/// Necessary to apply and undo InkOverlay changes
		/// </summary>
		private StrokeCollection inkStrokes;
	
		/// <summary>
		/// Label to apply
		/// </summary>
		private string label;

		/// <summary>
		/// Label's color
		/// </summary>
		private System.Windows.Media.Color labelColor;

		/// <summary>
		/// Labeled shape resulting from applying a label
		/// </summary>
		private Sketch.Shape labeledShape;

		/// <summary>
		/// Necessary to undo label changes when applying a label
		/// </summary>
        private Dictionary<string, Data.Pair<ShapeType, StrokeCollection>> origLabels;

        /// <summary>
        /// Strokes that did not have a label at the beginning of the command
        /// </summary>
        private StrokeCollection unlabeledStrokes;

        /// <summary>
        /// Called on shapes to regroup
        /// </summary>
        public event RegroupEventHandler Regroup;

        public event ErrorCorrectedEventHandler ErrorCorrected;

        private bool userSpecifiedGroup;

        private bool userSpecifiedLabel;

        #endregion

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sketch">SketchPanel to add a label to</param>
		/// <param name="inkStrokes">InkOverlay strokes</param>
		/// <param name="inkStrokes">A StrokeCollection strokes</param>
		/// <param name="label">Label to apply</param>
		public ApplyLabelCmd(SketchPanel sketch, StrokeCollection inkStrokes, 
			string label, bool userSpecifiedGroup = true, bool userSpecifiedLabel = true)
		{
            isUndoable = false;

            this.sketchPanel = sketch;
			this.label = label;
			this.inkStrokes = inkStrokes;
            this.userSpecifiedGroup = userSpecifiedGroup;
            this.userSpecifiedLabel = userSpecifiedLabel;
            labelColor = LogicDomain.getType(label).Color;

			// Save the original labels of the substrokes
            origLabels = new Dictionary<string, Data.Pair<ShapeType, StrokeCollection>>();
            unlabeledStrokes = new StrokeCollection();

            foreach (Stroke stroke in inkStrokes)
            {
                Sketch.Substroke sub = sketchPanel.InkSketch.GetSketchSubstrokeByInk(stroke);

                if (sub.ParentShape != null)
                {
                    if (!origLabels.ContainsKey(sub.ParentShape.Name))
                        origLabels[sub.ParentShape.Name] = new Data.Pair<ShapeType, StrokeCollection>(sub.ParentShape.Type, new StrokeCollection());
                    origLabels[sub.ParentShape.Name].B.Add(stroke);
                }
                else
                    unlabeledStrokes.Add(stroke);
            }
		}

        /// <summary>
        /// Type of this command, to tell it appart
        /// </summary>
        /// <returns></returns>
        public override string Type()
        {
            return "ApplyLabel";
        }

		/// <summary>
		/// Applies a label to a group of substrokes.
		/// </summary>
        public override bool Execute()
        {
            // Get the sketch we are working with
            Sketch.Sketch sketch = sketchPanel.InkSketch.Sketch;

            // Accumulate the list of substrokes that are about to be relabeled
            List<Sketch.Substroke> substrokes = new List<Sketch.Substroke>();
            foreach (Stroke stroke in inkStrokes)
			{
                // Find the corresponding substroke in the sketch
                Sketch.Substroke sub = sketchPanel.InkSketch.GetSketchSubstrokeByInk(stroke);

                // Add it to the growing list
                substrokes.Add(sub);
			}

            // Make a new shape out of these substrokes
            Domain.ShapeType theType = Domain.LogicDomain.getType(label);
            List<Sketch.Shape> shapesToRegroup;
            labeledShape = sketch.MakeNewShapeFromSubstrokes(out shapesToRegroup, substrokes, theType, 1.0);

            if (userSpecifiedLabel)
            {
                // Updates the orientation for the sake of the ghost gate
                if (labeledShape.Type.Classification == LogicDomain.GATE_CLASS)
                {
                    RecognitionInterfaces.Orienter orienter = RecognitionManager.RecognitionPipeline.createDefaultOrienter();
                    orienter.orient(labeledShape, sketchPanel.InkSketch.FeatureSketch);
                }

                // Update the shape name for text
                else if (labeledShape.Type.Classification == LogicDomain.TEXT_CLASS)
                {
                    RecognitionInterfaces.Recognizer recognizer = new Recognizers.TextRecognizer();
                    recognizer.recognize(labeledShape, sketchPanel.InkSketch.FeatureSketch);
                }
            }

            // Record the fact that user specified this grouping or label, 
            // so we don't accidentally change it in the future.
            labeledShape.AlreadyGrouped = userSpecifiedGroup;
            if (label != new ShapeType().Name)
            {
                labeledShape.UserLabeled = userSpecifiedLabel;


                // Also, update the recognition to take this into account
                if (userSpecifiedLabel && ErrorCorrected != null) 
                    ErrorCorrected(labeledShape);
            }

            // Fun Fix
            // Problem description:
            //   Suppose you draw wire -> notgate -> wire
            //                      --------|>o--------
            //   Then you label the notgate as a wire, so the
            //   whole ensamble becomes a single wire. Then you
            //   relabel it as a notgate again. The two wires
            //   which *were* distinct are still part of the same
            //   wire mesh.
            // Problem solution:
            //   Here, when you apply a label to a group of 
            //   substrokes, we will "explode" all the wire shapes
            //   that were changed (break them into single-substroke
            //   shapes). We expect that they will be reconnected 
            //   later.

            List<Sketch.Shape> newShapesToRegroup = new List<Sketch.Shape>(shapesToRegroup);
            foreach (Sketch.Shape modifiedShape in shapesToRegroup)
            {
                if (Domain.LogicDomain.IsWire(modifiedShape.Type))
                {
                    List<Sketch.Shape> newShapes = sketch.ExplodeShape(modifiedShape);
                    newShapesToRegroup.Remove(modifiedShape);
                    newShapesToRegroup.AddRange(newShapes);
                }
            }
            shapesToRegroup = newShapesToRegroup;

            // Make sure the old connected shapes are updated with their
            // relationships to the newly labeled shape
            if (Regroup != null)
            {
                // Regroup everything so highlighting/labels are correctly updated
                // Ensures that the newly labeled shape's relationship to its
                // connected shapes are updated as well
                shapesToRegroup.Add(labeledShape);
                Regroup(new List<Sketch.Shape>(shapesToRegroup));
            }

            sketchPanel.EnableDrawing();
            return true;
		}


		/// <summary>
		/// Removes a labeled shape from a sketch.
		/// </summary>
        public override bool UnExecute()
        {
            // Go through original labels, apply them
            foreach (string shape in origLabels.Keys)
            {
                ApplyLabelCmd unLabel = new ApplyLabelCmd(sketchPanel, origLabels[shape].B, origLabels[shape].A.Name, false, false);
                unLabel.Regroup = Regroup;
                unLabel.Execute();
            }

            // Unlabel everything that was not labeled before (note: will make these Unknown, but all the same shape)
            if (unlabeledStrokes.Count > 0)
            {
                ApplyLabelCmd unLabel2 = new ApplyLabelCmd(sketchPanel, unlabeledStrokes, (new ShapeType()).Name, false, false);
                unLabel2.Regroup = Regroup;
                unLabel2.Execute();
            }

            return true;
        }
	}
}
