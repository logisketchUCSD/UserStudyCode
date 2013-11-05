using System;
using System.Collections.Generic;
using System.Text;
using Sketch;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Set;

namespace TrainingDataPreprocessor
{
	public class IndividualPreprocessor
	{
		private List<string> _files;
		private Set<string> _gates;
		private Dictionary<string, List<Shape>> _sps;
		private string _dir;

		public IndividualPreprocessor(List<string> files, Set<string> ingates, string dir)
		{
			_files = files;
			_gates = ingates;
			_dir = dir;

			_sps = new Dictionary<string, List<Shape>>();
			foreach (string gate in _gates)
			{
				if (gate == "" || gate == " ")
					Console.WriteLine("Empty?");
				_sps.Add(gate, new List<Shape>());
			}

			foreach(string file in files)
			{
				Sketch.Sketch input = new ConverterXML.ReadXML(file).Sketch;
				foreach(Shape sp in input.ShapesL)
				{
					if (_sps.ContainsKey(sp.LowercasedType))
					{
						_sps[sp.LowercasedType].Add(sp);
					}
				}
			}

		}

		public void write()
		{
			foreach (KeyValuePair<string, List<Shape>> kvp in _sps)
			{
				int count = 0;
				foreach (Shape sp in kvp.Value)
				{
					Sketch.Sketch sketch = new Sketch.Sketch();
					sketch.AddShape(sp);
					foreach (Substroke s in sp.SubstrokesL)
					{
						sketch.AddStroke(s.ParentStroke);
					}
                    ConverterXML.SaveToXML maker = new ConverterXML.SaveToXML(sketch);
					maker.WriteXML(String.Format("{0}\\{1}_{2}.xml", _dir, kvp.Key, count));
					++count;
				}
			}
		}


	}

	/// <summary>
	/// Front-end for the preprocessor
	/// </summary>
	public class Preprocessor
	{
		#region Internals

		private TrainingData _td;
		Set<string> _gates;
		private static int WIDTH = 64;
		private static int HEIGHT = 64;

		private static string DEFAULT_EXTENSION = Files.FUtil.Extension(Files.Filetype.PREPROCESSED_DATA);

		private bool _verbose;

		#endregion

		#region Constructors

		public Preprocessor(List<string> files, Set<string> ingates, Dictionary<string, string> canonical_examples, bool verbosity)
		{
			_verbose = verbosity;
			_td = new TrainingData(WIDTH, HEIGHT);
			_gates = ingates;
			if (_verbose)
				Console.WriteLine("Beginning search for {0} gate types", _gates.Count);
			foreach (string file in files)
			{
				if (_verbose)
					Console.Write(".");
				Sketch.Sketch input = new ConverterXML.ReadXML(file).Sketch;
				addGatesFrom(input);
			}
			foreach (KeyValuePair<string, string> ce in canonical_examples)
			{
				Sketch.Sketch input = new ConverterXML.ReadXML(ce.Value).Sketch;
				addCanonicalExample(ce.Key, input);
			}
			if (_verbose)
			{
				Console.Write(Environment.NewLine);
				Console.WriteLine("SUMMARY:");
				foreach(string gate in _gates)
				{
					Console.WriteLine("Found {0} {1}s", _td.Images(gate).Count, gate);
				}
			}
		}

		#endregion

		/// <summary>
		/// Add gates to the preprocessor from a sketch
		/// </summary>
		/// <param name="input">The sketch to look in</param>
		public void addGatesFrom(Sketch.Sketch input)
		{
			foreach(Shape s in input.ShapesL)
			{
				string name = s.LowercasedType;
				if (name == "notbubble" || name=="not_bubble") name = "bubble";
				if (!_gates.Contains(name)) continue;
				_td.addGate(name, s);
			}
		}

		/// <summary>
		/// Adds a canonical example to the preprocessor. If the specified gate
		/// already contains a canonical example, it will be replaced
		/// </summary>
		/// <param name="gateName">The gate to add the example to</param>
		/// <param name="sketch">The example itself. Should contain
		/// a single image. If any parts are to be emphasized, they should be 
		/// drawn in red.</param>
		public void addCanonicalExample(string gateName, Sketch.Sketch sketch)
		{
			Shape s = new Shape();
			s.AddSubstrokes(new List<Substroke>(sketch.Substrokes));
			_td.addCanonicalGate(gateName, s);
		}

		/// <summary>
		/// Serializes the preprocessed data
		/// </summary>
		/// <param name="outputfile">The output file</param>
		public void writeToFile(string outputfile)
		{
			if (_verbose)
				Console.WriteLine("Dumping training data to file {0}", outputfile);
			_td.WriteToFile(outputfile);
		}

		#region Static functions

		[STAThread]
		public static void Main(string[] args)
		{
			Set<string> gates = new Set.HashSet<string>();
			Dictionary<string, string> canonical = new Dictionary<string, string>();
			List<string> files = new List<string>();
			string IndividualOutputDir = "";
			string outfile = "";
			string canonical_dir = "";
			bool verbose = false;
			// I like to use modes for handling command-line arguments
			// Mode specification:
			//   0 : normal
			//   1 : gate select
			//   2 : directory list
			//   3 : file select
			//   4 : canonical example directory select
			//   5 : individual output dir selection
			int mode = 0;

			//Start out by parsing the arguments
			foreach (string arg in args)
			{
				if (arg[0] == '-') // handle flags
				{
					switch (arg[1])
					{
						case 'g':
							mode = 1;
							break;
						case 'd':
							mode = 2;
							break;
						case 'f':
							mode = 3;
							break;
						case 'v':
							verbose = true;
							mode = 0;
							break;
						case 'c':
							mode = 4;
							break;
						case 'D':
							mode = 5;
							break;
						case 'h':
						default:
							mode = 0;
							printUsage();
							break;
					}
				}
				else // hope we're in a mode
				{
					switch (mode)
					{
						case 1:
							// Looking for gates
							if (arg.Contains("a"))
								gates.Add("and");
							if (arg.Contains("o"))
								gates.Add("or");
							if (arg.Contains("n"))
								gates.Add("not");
							if (arg.Contains("x"))
								gates.Add("xor");
							if (arg.Contains("d"))
								gates.Add("nand");
							if (arg.Contains("r"))
								gates.Add("nor");
							if (arg.Contains("s"))
								gates.Add("xnor");
							if (arg.Contains("b"))
								gates.Add("bubble");
							break;
						case 2:
							if (System.IO.Directory.Exists(arg))
							{
								files.AddRange(System.IO.Directory.GetFiles(arg, "*" + Files.FUtil.Extension(Files.Filetype.XML)));
							}
							else
							{
								Console.WriteLine("Directory {0} not found", arg);
							}
							break;
						case 3:
							if (!System.IO.Directory.Exists(arg.Substring(0, arg.LastIndexOf(@"\"))))
								confirmCreation(arg.Substring(0, arg.LastIndexOf(@"\")));
							if (!arg.Substring(arg.LastIndexOf(@"\")).Contains("."))
								outfile = arg + DEFAULT_EXTENSION;
							else
								outfile = arg;
							break;
						case 4:
							if (System.IO.Directory.Exists(arg))
							{
								canonical_dir = arg;
							}
							else
							{
								Console.WriteLine("Directory {0} not found", arg);
							}
							break;
						case 5:
							if (System.IO.Directory.Exists(arg))
							{
								IndividualOutputDir = arg;
							}
							else
							{
								Console.WriteLine("Directory {0} not found", arg);
							}
							break;
					}
				}
			}
			if (outfile == "")
			{
				Console.WriteLine("No output file specified, aborting");
				Environment.Exit(2);
			}
			if (files.Count == 0)
			{
				Console.WriteLine("No input files found, aborting");
				Environment.Exit(2);
			}
			if (canonical_dir == "")
			{
				Console.WriteLine("No canonical examples found, so none will be available for training");
			}
			else
			{
				foreach(string g in gates)
				{
					if (File.Exists(canonical_dir + '\\' + g + Files.FUtil.Extension(Files.Filetype.CANONICAL_EXAMPLE)))
					{
						canonical[g] = canonical_dir + '\\' + g + Files.FUtil.Extension(Files.Filetype.CANONICAL_EXAMPLE);
					}
					else
					{
						Console.WriteLine("No canonical example provided for {0}, so none will be available", g);
					}
				}
			}
			if (IndividualOutputDir != "")
			{
				IndividualPreprocessor ip = new IndividualPreprocessor(files, gates, IndividualOutputDir);
				ip.write();
			}
			else
			{
				Preprocessor p = new Preprocessor(files, gates, canonical, verbose);
				p.writeToFile(outfile);
			}
		}

		/// <summary>
		/// Prints out a usage statement
		/// </summary>
		private static void printUsage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("TrainingDataPreprocessor -g [aonxdrs] -f filename [-d directory ...] [-v]");
			Console.WriteLine("Flags:");
			Console.WriteLine("\t-g Train for the following gates. Acceptable gates:");
			Console.WriteLine("\t\t a = and");
			Console.WriteLine("\t\t o = or");
			Console.WriteLine("\t\t n = not");
			Console.WriteLine("\t\t x = xor");
			Console.WriteLine("\t\t d = nand");
			Console.WriteLine("\t\t r = nor");
			Console.WriteLine("\t\t s = xnor");
			Console.WriteLine("\t\t b = bubble/notbubble");
			Console.WriteLine("\t-f Save the output training file to the given output");
			Console.WriteLine("\t-d Scan the specified directories for data");
			Console.WriteLine("\t-c Scan the specified directory for canonical gate examples");
			Console.WriteLine("\t-D Write to individual files in the specified output directory");
			Console.WriteLine("\t-v Enable verbose mode");
		}

		private static void confirmCreation(string directory)
		{
			Console.Write("Directory {0} was not found, create it?[Y/n] ", directory);
			char response = (char)Console.Read();
			Console.Write(Environment.NewLine);
			if (response == 'n')
			{
				Console.WriteLine("Invalid output file, aborting operation");
				Environment.Exit(2);
			}
			else
			{
				try
				{
					System.IO.Directory.CreateDirectory(directory);
				}
				catch (System.IO.IOException)
				{
					Console.WriteLine("Error creating directory. Aborting");
					Environment.Exit(2);
				}
			}
		}

		#endregion
	}
}
