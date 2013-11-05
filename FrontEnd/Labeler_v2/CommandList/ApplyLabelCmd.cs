using System;
using System.Collections;
using System.Collections.Generic;

using Sketch;

using CommandManagement;

namespace Labeler.CommandList
{
	/// <summary>
	/// Summary description for ApplyLabelCmd.
	/// </summary>
	public class ApplyLabelCmd : Command
	{
		/// <summary>
		/// Is the command undoable?
		/// </summary>
		private bool isUndoable = true;
	
		/// <summary>
		/// Sketch to contain the labeled shape
		/// </summary>
		private Sketch.Sketch sketch;
	
		/// <summary>
		/// Substroke to apply the label to
		/// </summary>
		private List<Substroke> substrokes;

		/// <summary>
		/// Necessary to apply and undo InkOverlay changes
		/// </summary>
		private Microsoft.Ink.Strokes inkStrokes;
	
		/// <summary>
		/// Label to apply
		/// </summary>
		private string label;

		/// <summary>
		/// Label's color
		/// </summary>
		private System.Drawing.Color labelColor;

		/// <summary>
		/// The domainInfo we're working with
		/// </summary>
		private DomainInfo domainInfo;

		/// <summary>
		/// Labeled shape resulting from applying a label
		/// </summary>
		private Shape labeledShape;

		/// <summary>
		/// Necessary to undo color changes when applying a label
		/// </summary>
		private System.Drawing.Color[] origColors;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sketch">Sketch to add a label to</param>
		/// <param name="substrokes">Substrokes to label</param>
		/// <param name="inkStrokes">InkOverlay strokes</param>
		/// <param name="label">Label to apply</param>
		/// <param name="labelColor">Color of the applied label</param>
		/// <param name="domainInfo">DomainInfo for our Labeler</param>
		public ApplyLabelCmd(Sketch.Sketch sketch, List<Substroke> substrokes, Microsoft.Ink.Strokes inkStrokes, 
			string label, System.Drawing.Color labelColor, DomainInfo domainInfo)
		{
			this.sketch = sketch;
			this.substrokes = substrokes;
			this.label = label;
			this.labelColor = labelColor;
			this.inkStrokes = inkStrokes;
			this.domainInfo = domainInfo;

			// Initialize the original colors of the inkStrokes
			this.origColors = new System.Drawing.Color[inkStrokes.Count];
			int count = 0;

			foreach (Microsoft.Ink.Stroke stroke in inkStrokes)
			{
				origColors[count++] = stroke.DrawingAttributes.Color;
			}
		}


		/// <summary>
		/// Applies a label to a group of substrokes.
		/// </summary>
		public override void Execute()
		{
			foreach (Sketch.Substroke substroke in this.substrokes)
				substroke.RemoveLabel(this.label);
			
			for (int i = 0; i < this.inkStrokes.Count; i++)
			{
				Microsoft.Ink.Stroke stroke = this.inkStrokes[i];

				int currColorRank = 0;
				if (this.origColors[i] == System.Drawing.Color.Black)
					currColorRank = Int32.MinValue;
				else
					currColorRank = (int)this.domainInfo.ColorHierarchy[this.origColors[i]];
				
				int labelColorRank = (int)this.domainInfo.ColorHierarchy[this.labelColor];

				if (labelColorRank > currColorRank)
					stroke.DrawingAttributes.Color = this.labelColor;
				else
					stroke.DrawingAttributes.Color = this.origColors[i];
			}

			this.labeledShape = this.sketch.AddLabel(this.substrokes, this.label);
		}


		/// <summary>
		/// Removes a labeled shape from a sketch.
		/// </summary>
		public override void UnExecute()
		{
			this.sketch.RemoveLabel(this.labeledShape);

			int count = 0;
			foreach (Microsoft.Ink.Stroke stroke in this.inkStrokes)
			{
				stroke.DrawingAttributes.Color = this.origColors[count++];
			}
		}


		/// <summary>
		/// Returns if the command is undoable.
		/// </summary>
		/// <returns>True iff the command is undoable</returns>
		public override bool IsUndoable()
		{
			return this.isUndoable;
		}
	}
}
