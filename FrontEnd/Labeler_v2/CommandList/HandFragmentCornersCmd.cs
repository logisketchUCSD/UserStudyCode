using System;
using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;
using Featurefy;

using CommandManagement;


namespace Labeler.CommandList
{
	/// <summary>
	/// Summary description for GetFragmentCornersCmd.
	/// </summary>
	public class HandFragmentCornersCmd : Command
	{
		/// <summary>
		/// Is the Command undoable?
		/// </summary>
		private bool isUndoable = true;

		/// <summary>
		/// Unique points to add to the Hashtable
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> ptsToAdd;

		/// <summary>
		/// Hashtable mapping FeatureStrokes to fragment points
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> strokeToCorners;

		/// <summary>
		/// InkOverlay to draw the fragment points
		/// </summary>
		private InkOverlay overlayInk;

		/// <summary>
		/// How to display the fragment points
		/// </summary>
		private Microsoft.Ink.DrawingAttributes fragmentPtAttributes;
		
		/// <summary>
		/// What points have been drawn within this Command
		/// </summary>
		private List<System.Drawing.Point> fragmentPtsDrawn;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ptsToAdd">Unique points to add to the current Hashtable</param>
		/// <param name="strokeToCorners">Hashtable mapping FeatureStrokes to fragment points</param>
		/// <param name="overlayInk">InkOverlay to draw the points on</param>
		/// <param name="fragmentPtAttributes">Attributes for the drawn fragment points</param>
		public HandFragmentCornersCmd(Dictionary<Sketch.Stroke, List<int>> ptsToAdd, 
            Dictionary<Sketch.Stroke, List<int>> strokeToCorners, 
			InkOverlay overlayInk, Microsoft.Ink.DrawingAttributes fragmentPtAttributes)
		{
			this.ptsToAdd = new Dictionary<Sketch.Stroke, List<int>>(ptsToAdd);
			this.strokeToCorners = strokeToCorners;
			this.overlayInk = overlayInk;
			this.fragmentPtAttributes = fragmentPtAttributes;

			this.fragmentPtsDrawn = new List<System.Drawing.Point>();
		}

		
		/// <summary>
		/// Create the fragmentation points in the FragmentPanel
		/// </summary>
		public override void Execute()
		{
			foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in ptsToAdd)
			{
				Sketch.Stroke stroke = entry.Key;
				List<int> newIndices = entry.Value;

				// Associate the split point we've found with our FeatureStroke
				if (!this.strokeToCorners.ContainsKey(stroke))
				{
					this.strokeToCorners.Add(stroke, newIndices.GetRange(0, newIndices.Count));

					DisplayFragmentPoints(stroke, newIndices);
				}
				else
				{
					List<int> oldIndices = this.strokeToCorners[stroke];
					
					foreach (int index in newIndices)
					{
						if (!oldIndices.Contains(index))
						{
							oldIndices.Add(index);
							DisplayFragmentPoints(stroke, index);
						}

						// This Command currently only works with a UNIQUE set of pts in ptsToAdd
						else
							break;
					}
				}
			}
		}

		
		/// <summary>
		/// Undoes the current fragmentation within the FragmentPanel
		/// </summary>
		public override void UnExecute()
		{
			// Remove all of the fragment point references from the Hashtable
			foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in this.ptsToAdd)
			{
				Sketch.Stroke stroke = entry.Key;
				List<int> addedIndices = entry.Value;
				List<int> indices = this.strokeToCorners[stroke];

				foreach (int index in addedIndices)
                    indices.Remove(index);
			}
			
			// Remove all the drawn strokes from the InkOverlay
			foreach (System.Drawing.Point coordinates in this.fragmentPtsDrawn)
			{
				foreach (Microsoft.Ink.Stroke fragPt in this.overlayInk.Ink.Strokes)
				{
					if (fragPt.BezierPoints[0] == coordinates)
					{
						this.overlayInk.Ink.DeleteStroke(fragPt);
						break;
					}
				}
			}
		}

		
		/// <summary>
		/// Returns true if the Command is undoable
		/// </summary>
		/// <returns>True if the Command is undoable, false otherwise</returns>
		public override bool IsUndoable()
		{
			return isUndoable;
		}


		private void DisplayFragmentPoints(Sketch.Stroke stroke, int index)
		{
			Sketch.Point[] points = stroke.Points;
			
			System.Drawing.Point coordinates = new System.Drawing.Point((int)points[index].X,
				(int)points[index].Y);

			// Set the fragment point with drawing attributes
			Microsoft.Ink.Stroke fragPt = overlayInk.Ink.CreateStroke(new System.Drawing.Point[1] {coordinates});
			fragPt.DrawingAttributes = this.fragmentPtAttributes;

			this.fragmentPtsDrawn.Add(coordinates);
		}


		private void DisplayFragmentPoints(Sketch.Stroke stroke, List<int> indices)
		{
			Sketch.Point[] points = stroke.Points;
			
			foreach (int index in indices)
			{
				System.Drawing.Point coordinates = new System.Drawing.Point((int)points[index].X,
					(int)points[index].Y);

				// Set the fragment point with drawing attributes
				Microsoft.Ink.Stroke fragPt = overlayInk.Ink.CreateStroke(new System.Drawing.Point[1] {coordinates});
				fragPt.DrawingAttributes = this.fragmentPtAttributes;

				this.fragmentPtsDrawn.Add(coordinates);
			}
		}
	}
}
