using System;
using System.Collections.Generic;
using System.Text;
using CommandManagement;
using Sketch;

namespace Labeler.CommandList
{
	class ResampleCmd : Command
	{

		#region Internals

		private Sketch.Sketch _sketch;
		private LabelerPanel _lp;
		private Substroke _ss;
		private Sketch.Sketch _origSketch;
		private int _pts;

		#endregion

		#region Constructors

		public ResampleCmd(ref Sketch.Sketch sketch, ref LabelerPanel lp, ref Substroke original, int numPts)
		{
			_sketch = sketch;
			_lp = lp;
			_ss = original;
			_pts = numPts;
		}

		#endregion

		#region Interface Functions

		public override bool IsUndoable()
		{
			return true;
		}

		public override void Execute()
		{
			_origSketch = _sketch.Clone();
			_ss.ResampleInPlace(_pts);
			_lp.Enabled = false;
			_lp.Sketch = _sketch;
			_lp.Enabled = true;
		}

		public override void UnExecute()
		{
			_lp.Enabled = false;
			_lp.Sketch = _origSketch;
			_sketch = _lp.Sketch;
			_lp.Enabled = true;
		}

		#endregion
	}
}
