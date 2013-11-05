using System;
using System.Collections.Generic;
using System.Text;
using Recognizers;
using Congeal;

namespace BayesianRecognizer
{
	/// <summary>
	/// Class to recognize subshapes using a congealing method
	/// </summary>
	class SubshapeRecognizer
	{
		public enum Subshape { ARC, LINE, BUBBLE };

		#region INTERNALS

		private OldRecognizers.CongealRecognizer congealer;
		private List<Designation> designations;

		#endregion

		#region CONSTRUCTORS

		public SubshapeRecognizer()
		{
			designations = new List<Designation>();
			foreach (Subshape ss in new Subshape[] { Subshape.ARC, Subshape.BUBBLE, Subshape.LINE })
			{
			}
			congealer = new OldRecognizers.CongealRecognizer(designations);
		}

		#endregion

	}
}
