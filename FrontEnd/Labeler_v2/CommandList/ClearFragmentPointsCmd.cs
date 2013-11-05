using System;
using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;

using CommandManagement;

namespace Labeler.CommandList
{
	/// <summary>
	/// Summary description for ClearFragmentPointsCmd.
	/// </summary>
	public class ClearFragmentPointsCmd : Command
	{
		/// <summary>
		/// Is the Command undoable?
		/// </summary>
		private bool isUndoable = true;

		private FragmentPanel fragPanel;

		private Sketch.Stroke[] strokes;
		
		/// <summary>
		/// Hashtable mapping FeatureStrokes to fragment points
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> clearedStrokeToCorners;

		private Dictionary<Sketch.Stroke, List<int>> oldStrokeToCorners;
		
		/// <summary>
		/// InkOverlay to draw the fragment points
		/// </summary>
		private InkOverlay overlayInk;
		
		/// <summary>
		/// How to display the fragment points
		/// </summary>
		private Microsoft.Ink.DrawingAttributes fragmentPtAttributes;

		private Microsoft.Ink.Ink inkForDeletedStrokes;

		private Microsoft.Ink.Stroke[] oldStrokes;
		
		
		public ClearFragmentPointsCmd(FragmentPanel fragPanel)
		{
			this.fragPanel = fragPanel;
			
			this.strokes = this.fragPanel.Strokes;
			
			this.clearedStrokeToCorners = new Dictionary<Sketch.Stroke,List<int>>();
			foreach (Sketch.Stroke stroke in this.strokes)
			{
				this.clearedStrokeToCorners[stroke] = new List<int>();
			}

			this.oldStrokeToCorners = new Dictionary<Sketch.Stroke,List<int>>(this.fragPanel.StrokeToCorners);
			
			this.overlayInk = this.fragPanel.OverlayInk;
			this.fragmentPtAttributes = this.fragPanel.FragmentPtAttributes;
		}


		public override void Execute()
		{
			this.fragPanel.StrokeToCorners = this.clearedStrokeToCorners;
			
			this.inkForDeletedStrokes = new Ink();
			this.inkForDeletedStrokes.AddStrokesAtRectangle(this.overlayInk.Ink.Strokes,
				this.overlayInk.Ink.Strokes.GetBoundingBox());

			this.overlayInk.Ink.DeleteStrokes();

			this.fragPanel.SketchInk.Refresh();
		}

	
		public override void UnExecute()
		{
			this.fragPanel.StrokeToCorners = this.oldStrokeToCorners;

			this.overlayInk.Ink.AddStrokesAtRectangle(this.inkForDeletedStrokes.Strokes,
				this.inkForDeletedStrokes.Strokes.GetBoundingBox());

			this.fragPanel.SketchInk.Refresh();
		}


		/// <summary>
		/// Returns true if the Command is undoable
		/// </summary>
		/// <returns>True if the Command is undoable, false otherwise</returns>
		public override bool IsUndoable()
		{
			return isUndoable;
		}


		private void DisplayFragmentPoints(Sketch.Stroke stroke, List<int> indices)
		{
			Sketch.Point[] points = stroke.Points;
			
			foreach (int index in indices)
			{
				System.Drawing.Point pt = new System.Drawing.Point((int)points[index].X,
					(int)points[index].Y);

				// Set the fragment point with drawing attributes
				Microsoft.Ink.Stroke fragPt = this.overlayInk.Ink.CreateStroke(new System.Drawing.Point[1] {pt});
				fragPt.DrawingAttributes = this.fragmentPtAttributes;
			}
		}
	}
}
