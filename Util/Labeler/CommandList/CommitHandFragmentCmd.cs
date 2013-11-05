using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;
using Featurefy;

using CommandManagement;

namespace EditMenu.CommandList
{
	/// <summary>
	/// Summary description for CommitHandFragmentCmd.
	/// </summary>
	public class CommitHandFragmentCmd : Command
	{
		/// <summary>
		/// Is the Command undoable?
		/// </summary>
		private bool isUndoable = true;

		/// <summary>
		/// Hashtable containing the new, fragmented FeatureStroke to fragment point Hashtable
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> fragStrokeToCorners;

		/// <summary>
		/// Hashtable containing the old, LabelerPanel FeatureStroke to fragment point Hashtable
		/// </summary>
		private Dictionary<Sketch.Stroke, List<int>> oldStrokeToCorners;

		/// <summary>
		/// LabelerPanel
		/// </summary>
		private LabelerPanel labelerPanel;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fStrokeToCorners">FeatureStrokes to fragment points</param>
		/// <param name="labelerPanel">LabelerPanel</param>
		public CommitHandFragmentCmd(Dictionary<Sketch.Stroke, List<int>> fragStrokeToCorners, LabelerPanel labelerPanel)
		{
			this.labelerPanel = labelerPanel;
			this.fragStrokeToCorners = new Dictionary<Sketch.Stroke,List<int>>(fragStrokeToCorners);
			this.oldStrokeToCorners = new Dictionary<Sketch.Stroke,List<int>>();

            foreach (KeyValuePair<Sketch.Stroke, List<int>> entry in this.fragStrokeToCorners)
            {
                try
                {
                    this.oldStrokeToCorners[entry.Key] =
                        this.labelerPanel.LTool.StrokeToCorners[entry.Key];
                }

                catch (KeyNotFoundException e)
                {
                    continue;
                }
            }
		}

		
		/// <summary>
		/// Commit the hand-fragment changes
		/// </summary>
		public override void Execute()
		{
			this.labelerPanel.UpdateFragmentCorners(this.fragStrokeToCorners);
			
			this.labelerPanel.Selection = new Ink().CreateStrokes();
		}

		
		/// <summary>
		/// Undo the hand-fragment changes
		/// </summary>
		public override void UnExecute()
		{
			this.labelerPanel.UpdateFragmentCorners(this.oldStrokeToCorners);
			
			/*foreach (DictionaryEntry entry in this.fragStrokeToCorners)
			{
				Sketch.Stroke key = entry.Key as Sketch.Stroke;
				ArrayList val = this.oldStrokeToCorners[key] as ArrayList;
				
				if (val == null)
					this.labelerPanel.StrokeToCorners.Remove(key);
				else
					this.labelerPanel.StrokeToCorners[key] = new ArrayList(val);
			}
			
			this.labelerPanel.UpdateFragmentCorners();*/
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
