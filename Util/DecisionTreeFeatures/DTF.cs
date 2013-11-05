using System;
using System.Collections.Generic;
using System.Text;
using Set;

namespace DecisionTreeFeatures
{
	/// <summary>
	/// Abstraction of decision tree features file format
	/// </summary>
	public class DTF
	{
		#region Helper Structs

		private struct DTFStroke
		{
			/// <summary>
			/// The list of observations
			/// </summary>
			public List<DTFItem> observations;

			/// <summary>
			/// The correct classification for this stroke
			/// </summary>
			public string classification;

			public Guid Id;

			public DTFStroke(string classification, Guid id)
			{
				observations = new List<DTFItem>();
				this.classification = classification;
				Id = id;
			}

			public void Add(DTFItem observation)
			{
				observations.Add(observation);
			}
		}

		private struct DTFItem
		{
			/// <summary>
			/// The name for this class
			/// </summary>
			public string Name;
			/// <summary>
			/// Whether this variable is continuous or boolean
			/// </summary>
			public bool Continuous;

			/// <summary>
			/// The value of this thing
			/// </summary>
			private double val;

			/// <summary>
			/// The value of this DTFItem. Note: True = 1, False = 0
			/// </summary>
			public double Value
			{
				get { return val; }
				set
				{
					if (Continuous)
						val = value;
					else
					{
						if (value <= 0)
							val = 0.0;
						else
							val = 1.0;
					}
				}
			}

			/// <summary>
			/// Create a new DTFItem
			/// </summary>
			/// <param name="n">Name</param>
			/// <param name="c">Continuous</param>
			/// <param name="v">Value</param>
			public DTFItem(string n, bool c, double v)
			{
				Name = n;
				Continuous = c;
				val = 0.0;
				Value = v;
			}
		}

		#endregion

		#region Internals

		/// <summary>
		/// Feature type names
		/// </summary>
		private Dictionary<string, bool> NamesToContinuous;
		/// <summary>
		/// Observations
		/// </summary>
		private List<DTFStroke> strokes;
		/// <summary>
		/// The different classification classes
		/// </summary>
		private List<string> classes;

		#endregion

		#region Constructors

		/// <summary>
		/// Basic construcotr
		/// </summary>
		public DTF(List<string> classes)
		{
			if (classes.Count == 0)
				throw new Exception("You must provide classes!");
			NamesToContinuous = new Dictionary<string, bool>();
			strokes = new List<DTFStroke>();
			this.classes = classes;
		}	

		#endregion

		#region Useful Stuff

		/// <summary>
		/// Add a feature to this DTF set
		/// </summary>
		/// <param name="name">The name of the feature type</param>
		/// <param name="continuous">Is this feature type continuous or boolean?</param>
		public void AddFeature(string name, bool continuous)
		{
			NamesToContinuous[name] = continuous;
		}

		/// <summary>
		/// Add a stroke to the DTF
		/// </summary>
		/// <param name="classification">The correct classification for this stroke. Must be one of the classes
		/// specified in the constructor</param>
		/// <returns>The index of the stroke added</returns>
		public int AddStroke(string classification, Guid id)
		{
			strokes.Add(new DTFStroke(classification, id));
			return strokes.Count - 1;
		}

		/// <summary>
		/// Add an observation
		/// </summary>
		/// <param name="i">The index of the stroke to add to</param>
		/// <param name="name">The name of the feature type</param>
		/// <param name="value">The value</param>
		public void AddObservationToStroke(int i, string name, double value)
		{
			if (!NamesToContinuous.ContainsKey(name)) throw new Exception("Type must be added before observations!");
			strokes[i].Add(new DTFItem(name, NamesToContinuous[name], value));
		}

		/// <summary>
		/// Add an observation
		/// </summary>
		/// <param name="i">The index of the stroke to add to</param>
		/// <param name="name">The name of the feature type</param>
		/// <param name="value">The value</param>
		public void AddObservationToStroke(int i, string name, bool value)
		{
			if (!NamesToContinuous.ContainsKey(name)) throw new Exception("Type must be added before observations!");
			if (value)
				strokes[i].Add(new DTFItem(name, NamesToContinuous[name], 1.0));
			else
				strokes[i].Add(new DTFItem(name, NamesToContinuous[name], 0.0));
		}

		#endregion

		#region File Interaction

		/// <summary>
		/// Write out the names file
		/// </summary>
		/// <param name="filename">The filename to write to</param>
		public void WriteNamesFile(string filename)
		{
			System.IO.TextWriter tw = new System.IO.StreamWriter(filename, false);
			for (int cidx = 0; cidx < classes.Count; ++cidx)
			{
				tw.Write(classes[cidx]);
				if (cidx == classes.Count - 1)
					tw.Write(".");
				else
					tw.Write(", ");
			}
			tw.Write(Environment.NewLine + Environment.NewLine);

			List<string> names = new List<string>(NamesToContinuous.Count);
			foreach (KeyValuePair<string, bool> type in NamesToContinuous)
				names.Add(type.Key);
			names.Sort();

			foreach (string name in names)
			{
				tw.Write(name + ": ");
				if (NamesToContinuous[name])
					tw.WriteLine("continuous.");
				else
					tw.WriteLine("0, 1.");
			}
			tw.Close();
		}

		/// <summary>
		/// Write the data file to a filename
		/// </summary>
		/// <param name="filename">This one</param>
		public void WriteDataFile(string filename)
		{
			System.IO.TextWriter tw = new System.IO.StreamWriter(filename, false);
			foreach (DTFStroke s in strokes)
			{
				if (s.observations.Count != NamesToContinuous.Count)
					throw new Exception("The number of observations must match the number of features!");
				// Sort the observations by name. This should give us the same order as NamesToContinuous
				s.observations.Sort(delegate(DTFItem lhs, DTFItem rhs) { return lhs.Name.CompareTo(rhs.Name); });
				foreach (DTFItem i in s.observations)
					tw.Write(i.Value + ", ");
				tw.Write(s.classification + ", ");
				tw.WriteLine(s.Id.ToString());
			}
			tw.Close();
		}

		#endregion
	}
}
