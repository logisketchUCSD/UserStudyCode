using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using ConverterXML;
using Sketch;
using Set;

namespace TrainingDataPreprocessor
{
	/// <summary>
	/// This class stores training data for symbol recognizers
	/// </summary>
	[Serializable]
	public class TrainingData
	{
		[Serializable]
		public struct GateImage
		{
			public Bitmap bitmap;
			public Shape shape;
		}

		private Dictionary<string, List<GateImage>> _gateImages;
		private Dictionary<string, Shape> _canonicalExamples;
		private Set<string> _gates;
		private int width;
		private int height;

		#region Constructors

		/// <summary>
		/// Default constructor for this TrainingData object
		/// </summary>
		public TrainingData(int w, int h)
		{
			_gateImages = new Dictionary<string, List<GateImage>>();
			_gates = new Set.HashSet<string>();
			_canonicalExamples = new Dictionary<string, Shape>();
			width = w;
			height = h;
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Serialize this object to the given file
		/// </summary>
		/// <param name="outputfile">The file to save to</param>
		public void WriteToFile(string outputfile)
		{
			Stream stream = File.Open(outputfile, FileMode.Create);
			BinaryFormatter fmt = new BinaryFormatter();
			fmt.Serialize(stream, this);
			stream.Close();
		}

		/// <summary>
		/// Loads a new TrainingData object from a file
		/// </summary>
		/// <param name="filename">The file to read from</param>
		/// <returns>The TrainingData object</returns>
		public static TrainingData ReadFromFile(string filename)
		{
			Stream stream = File.Open(filename, FileMode.Open);
			BinaryFormatter fmt = new BinaryFormatter();
			TrainingData td = (TrainingData)fmt.Deserialize(stream);
			stream.Close();
			return td;
		}

		#endregion

		#region Accessors

		/// <summary>
		/// The list of the gate types that this data contains
		/// </summary>
		public Set<string> Gates
		{
			get
			{
				return _gates;
			}
		}

		public List<GateImage> Images(string gate)
		{
			if (!_gateImages.ContainsKey(gate))
				return new List<GateImage>();
			return _gateImages[gate];
		}

		/// <summary>
		/// Get the canonical example for the gate
		/// </summary>
		/// <param name="gate">Which gate?</param>
		/// <returns>The canonical example, if one exists. Otherwise, `null`</returns>
		public Sketch.Shape CanonicalExample(string gate)
		{
			if (_canonicalExamples.ContainsKey(gate))
				return _canonicalExamples[gate];
			else
				return null;
		}

		#endregion

		/// <summary>
		/// Add a gate to the preprocessor's data store
		/// </summary>
		/// <param name="name">The gate name</param>
		/// <param name="image">A rasterized version of the gate</param>
		public void addGate(string name, Shape gate)
		{
			_gates.Add(name);
			if (!_gateImages.ContainsKey(name))
				_gateImages.Add(name, new List<GateImage>());
			GateImage image = new GateImage();
			image.bitmap = substrokesToBitmap(width, height, gate.SubstrokesL);
			image.shape = gate;
			_gateImages[name].Add(image);
		}

		/// <summary>
		/// Adds a canonical example to the preprocessor's data store
		/// </summary>
		/// <param name="name">The gate name</param>
		/// <param name="gate">The example</param>
		public void addCanonicalGate(string name, Shape gate)
		{
			if (!_gates.Contains(name.ToLower()))
				_gates.Add(name);
			_canonicalExamples[name] = gate;
		}


		private static Bitmap substrokesToBitmap(int width, int height, List<Sketch.Substroke> ss)
		{
			return new Adrian.PhotoX.Lib.GaussianBlur(1).ProcessImage(
				new SymbolRec.Image.Image(width, height, new SymbolRec.Substrokes(ss)).getThisAsBitmap());
		}

	}
}
