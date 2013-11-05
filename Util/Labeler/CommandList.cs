using System;
using System.Collections;

using Microsoft.Ink;

using Sketch;
using CommandManagement;

namespace Labeler
{
	/// <summary>
	/// Summary description for Commands.
	/// </summary>
	public class CommandList
	{
		#region Open Fragment Editing Window

		public class OpenFragWindow : Command
		{
			private bool isUndoable = false;

			private Microsoft.Ink.Strokes selection;

			private Hashtable mIdToSubstroke;

			private CommandManager CM;

			public OpenFragWindow(Microsoft.Ink.Strokes selection, Hashtable mIdToSubstroke)
			{
				this.selection = selection;
				this.mIdToSubstroke = mIdToSubstroke;
			}

			public override void Execute()
			{
				if (this.selection.Count > 0)
				{
					Sketch.Substroke selected = this.mIdToSubstroke[ this.selection[0].Id ] as Sketch.Substroke;

					if (selected != null)
					{
						FragmentDialogBox fdb = new FragmentDialogBox(new Sketch.Stroke[1] {selected.ParentStroke});
						fdb.Show();
					}
				}
			}


			/// <summary>
			/// Unexecutes the command.
			/// Nothing to do here.
			/// </summary>
			public override void UnExecute()
			{
				// Not undoable
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

		#endregion

		#region APPLY LABEL

		/// <summary>
		/// Apply's a label to substrokes
		/// </summary>
		public class ApplyLabel : Command
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
			private ArrayList substrokes;

			/// <summary>
			/// Label to apply
			/// </summary>
			private string label;

			/// <summary>
			/// Label's color
			/// </summary>
			private System.Drawing.Color labelColor;

			/// <summary>
			/// Labeled shape resulting from applying a label
			/// </summary>
			private Shape labeledShape;

			/// <summary>
			/// Necessary to apply and undo InkOverlay changes
			/// </summary>
			private Microsoft.Ink.Strokes inkStrokes;
			
			/// <summary>
			/// Necessary to undo color changes when applying a label
			/// </summary>
			private System.Drawing.Color[] origColors;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="sketch">Sketch to add a label to</param>
			/// <param name="substrokes">Substrokes to label</param>
			/// <param name="label">Label to apply</param>
			/// <param name="labelColor">Color of the applied label</param>
			/// <param name="inkStrokes">InkOverlay strokes</param>
			public ApplyLabel(Sketch.Sketch sketch, ArrayList substrokes, string label, 
				System.Drawing.Color labelColor, Microsoft.Ink.Strokes inkStrokes)
			{
				this.sketch = sketch;
				this.substrokes = substrokes;
				this.label = label;
				this.labelColor = labelColor;
				this.inkStrokes = inkStrokes;

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
				foreach (Microsoft.Ink.Stroke stroke in this.inkStrokes)
				{
					stroke.DrawingAttributes.Color = this.labelColor;
				}

				this.labeledShape = this.sketch.AddLabel(this.substrokes,
					this.label, 0);
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

		#endregion

		#region SPLIT STROKE AT

		public class SplitStrokeAt : Command
		{
			/// <summary>
			/// This Command is undoable.
			/// </summary>
			private bool isUndoable = true;
			
			/// <summary>
			/// InkOverlay for the Labeler.
			/// </summary>
			private Microsoft.Ink.InkOverlay oInk;

			/// <summary>
			/// Editing stroke to split.
			/// </summary>
			private Microsoft.Ink.Stroke splitStroke;

			/// <summary>
			/// Intersection points between the splitter stroke and the InkOverlay strokes.
			/// </summary>
			private System.Drawing.Point[] pointInter;

			/// <summary>
			/// The Labeler's mIdToIndices that holds Microsoft.Stroke Ids and various split points in
			/// ArrayLists
			/// </summary>
			private Hashtable mIdToIndices;

			/// <summary>
			/// A temporary version of the mIdToIndices variable, used for undoing this Command.
			/// </summary>
			private Hashtable newMIdToIndices;

			private System.Windows.Forms.ProgressBar progressBar;
			
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="oInk"></param>
			/// <param name="splitStroke"></param>
			/// <param name="pointInter"></param>
			/// <param name="mIdToIndices"></param>
			/// <param name="progressBar"></param>
			public SplitStrokeAt(Microsoft.Ink.InkOverlay oInk, Microsoft.Ink.Stroke splitStroke,
				System.Drawing.Point[] pointInter, Hashtable mIdToIndices, 
				System.Windows.Forms.ProgressBar progressBar)
			{
				this.oInk = oInk;
				this.splitStroke = splitStroke;
				this.pointInter = pointInter;
				this.mIdToIndices = mIdToIndices;
				this.progressBar = progressBar;

				this.newMIdToIndices = new Hashtable();
			}


			/// <summary>
			/// Marks points to be split in the InkOverlay.
			/// </summary>
			public override void Execute()
			{
				// Go through every stroke to see if the pointInter intersect it
				foreach (Microsoft.Ink.Stroke s in this.oInk.Ink.Strokes)
				{
					// Go through every possible point Intersection
					for (int i = 0; i < this.pointInter.Length; ++i)
					{
						// Do not consider the red stroke we drew
						if (s.Id != this.splitStroke.Id)
						{
							float theDistance;
							float theFIndex = s.NearestPoint(this.pointInter[i], out theDistance);
						
							// This is the closeness factor... we would like it as small as possible without missing any intersections
							// While most of the time theDistance is 0.0, sometimes they have been on the order of 70.0
							if (theDistance <= 0.0f)
							{
								if (this.newMIdToIndices[s.Id] == null)
									this.newMIdToIndices[s.Id] = new ArrayList();

								if ( !((ArrayList)this.newMIdToIndices[s.Id]).Contains(theFIndex) )
									((ArrayList)this.newMIdToIndices[s.Id]).Add(theFIndex);
							}
						}
					}

					this.progressBar.Value += 1;
				}

				// Add the new split points to mIdToIndices
				foreach (DictionaryEntry mId in this.newMIdToIndices)
				{
					if (!this.mIdToIndices.ContainsKey(mId.Key))
						this.mIdToIndices.Add(mId.Key, (ArrayList)mId.Value);
					else
                        ((ArrayList)this.mIdToIndices[mId]).AddRange((ArrayList)this.newMIdToIndices[mId]);
				}

				// Delete our red erase line
				oInk.Ink.DeleteStroke(this.splitStroke);
			}


			/// <summary>
			/// Undoes the addition of split points in the InkOverlay.
			/// </summary>
			public override void UnExecute()
			{
				// Add the new split points to mIdToIndices
				foreach (DictionaryEntry mId in this.newMIdToIndices)
				{
					if (this.mIdToIndices[mId.Key] == this.newMIdToIndices[mId.Key])
						this.mIdToIndices.Remove(mId.Key);

					else
					{
						foreach (float splitIndex in (ArrayList)this.newMIdToIndices[mId.Key])
						{
							((ArrayList)this.mIdToIndices[mId.Key]).Remove(splitIndex);
						}
					}
				}
			}


			/// <summary>
			/// Returns whether this Command is undoable or not.
			/// </summary>
			/// <returns>Returns true iff this Command is undoable.</returns>
			public override bool IsUndoable()
			{
				return this.isUndoable;
			}

		}

		#endregion
	}
}
