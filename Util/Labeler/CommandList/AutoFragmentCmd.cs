using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;
using Featurefy;
using Fragmenter;

using CommandManagement;

namespace EditMenu.CommandList
{
	/// <summary>
	/// Summary description for AutoFragmentCmd.
	/// </summary>
	public class AutoFragmentCmd : Command
	{
		/// <summary>
		/// Is the Command undoable?
		/// </summary>
		private bool isUndoable = true;

		/// <summary>
		/// New Hashtable mapping FeatureStrokes to auto-fragged points
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> strokeToCorners;

		/// <summary>
		/// Old Hashtable mapping FeatureStrokes to fragmentation points
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> oldStrokeToCorners;

		/// <summary>
		/// LabelerPanel
		/// </summary>
		private LabelerPanel labelerPanel;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="labelerPanel">LabelerPanel holding the Strokes to fragment</param>
		public AutoFragmentCmd(LabelerPanel labelerPanel)
		{
			this.labelerPanel = labelerPanel;
            Sketch.Stroke[] strokes = this.labelerPanel.Sketch.Strokes;

			this.oldStrokeToCorners = new Dictionary<Sketch.Stroke, List<int>>(strokes.Length);
			foreach (Sketch.Stroke stroke in strokes)
			{
                List<int> corners;
                if (this.labelerPanel.LTool.StrokeToCorners.TryGetValue(stroke, out corners))
                {
                    this.oldStrokeToCorners.Add(stroke, corners);
                }
			}

			this.strokeToCorners = new Dictionary<Sketch.Stroke, List<int>>();
			
			// Initialize the FeatureStrokes
			FeatureStroke[] featureStrokes = new FeatureStroke[strokes.Length];
			for (int i = 0; i < featureStrokes.Length; i++)
			{
				featureStrokes[i] = new FeatureStroke(strokes[i]);
			}

			for (int i = 0; i < featureStrokes.Length; i++)
			{
				List<int> currCorners = new List<int>();
			
				int[] corners = new Corners(featureStrokes[i]).FindCorners();
				
				if (corners.Length > 0)
				{
					this.strokeToCorners.Add(strokes[i], new List<int>(corners));
				}
				else
				{
					this.strokeToCorners.Add(strokes[i], new List<int>());
				}
			}
		}

		
		/// <summary>
		/// Auto-fragments the strokes in the LabelerPanel
		/// </summary>
		public override void Execute()
		{
			this.labelerPanel.UpdateFragmentCorners(this.strokeToCorners);
		}

		
		/// <summary>
		/// Undoes the Auto-fragmentation
		/// </summary>
		public override void UnExecute()
		{
			this.labelerPanel.UpdateFragmentCorners(this.oldStrokeToCorners);
		}

		
		/// <summary>
		/// Returns true if the Command is undoable
		/// </summary>
		/// <returns>True if the Command is undoable, false otherwise</returns>
		public override bool IsUndoable()
		{
			return isUndoable;
		}
	}
}
