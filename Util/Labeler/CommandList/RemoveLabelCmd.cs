using System;
using System.Collections.Generic;

using Sketch;
using Labeler;
using CommandManagement;

namespace EditMenu.CommandList
{
	/// <summary>
	/// Summary description for RemoveLabelCmd.
	/// </summary>
	public class RemoveLabelCmd : Command
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
		/// Hashtable from Microsoft Strokes to Sketch.Substrokes.
		/// </summary>
		private Dictionary<int, Guid?> mIdToSubstroke;
		
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
		/// <param name="sketch">Sketch to remove a label from</param>
		/// <param name="substrokes">Substrokes that contain the label</param>
		/// <param name="inkStrokes">InkOverlay strokes</param>
		/// <param name="mIdToSubstroke">Hashtable mapping Microsoft.Ink.Stroke Ids to Sketch.Substrokes</param>
		/// <param name="label">Label to remove</param>
		/// <param name="domainInfo">DomainInfo for our Labeler</param>
		public RemoveLabelCmd(Sketch.Sketch sketch, List<Substroke> substrokes, Microsoft.Ink.Strokes inkStrokes, 
			Dictionary<int, Guid?> mIdToSubstroke, string label, DomainInfo domainInfo)
		{
			this.sketch = sketch;
			this.substrokes = substrokes;
			this.inkStrokes = inkStrokes;
			this.mIdToSubstroke = mIdToSubstroke;
			this.label = label;
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
		/// Removes a labeled shape from a Sketch.
		/// </summary>
		public override void Execute()
		{
			this.labeledShape = null;
			foreach (Substroke s in this.substrokes)
			{
				if (this.labeledShape == null)
					this.labeledShape = s.RemoveLabel(this.label);
				else if (this.labeledShape != s.RemoveLabel(this.label))
					throw new Exception("Error removing " + this.label + ": Not all substrokes contain the same label");

				// Remove the shape if it's empty
				if (this.labeledShape != null && this.labeledShape.Substrokes.Length == 0 && this.labeledShape.Shapes.Length == 0)
					this.sketch.RemoveShape(this.labeledShape);
			}

			foreach (Microsoft.Ink.Stroke stroke in this.inkStrokes)
			{
				int bestColorRank = Int32.MinValue;
                int temp = stroke.Id;
				string[] remainingLabels = this.sketch.GetSubstroke((Guid) mIdToSubstroke[temp]).Labels;

				if (remainingLabels.Length > 0)
				{
					foreach (string currLabel in remainingLabels)
					{
						System.Drawing.Color color = this.domainInfo.GetColor(currLabel);
						int currRank = -1;
                        if (this.domainInfo.ColorHierarchy.ContainsKey(color))
                            currRank = (int)this.domainInfo.ColorHierarchy[color];
						
						if (currRank > bestColorRank)
						{
							bestColorRank = currRank;
							stroke.DrawingAttributes.Color = color;
						}
					}
				}
				else
				{
					stroke.DrawingAttributes.Color = System.Drawing.Color.Black;
				}
			}
		}


		/// <summary>
		/// Applies a label to a group of substrokes.
		/// </summary>
		public override void UnExecute()
		{
			this.sketch.AddLabel(this.substrokes, this.label);
			
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
