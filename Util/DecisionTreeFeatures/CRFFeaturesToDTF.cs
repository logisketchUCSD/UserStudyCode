using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using CRF;
using Featurefy;

namespace DecisionTreeFeatures
{
	class CRFFeaturesToDTF
	{
		[STAThread]
		public static void Main(string[] args)
		{
			bool fragment = false;

			#region Handle Input

			List<string> arguments = new List<string>(args);

			// First, look for flags
			List<string> flags = arguments.FindAll(delegate(string s) { return s[0] == '-'; });
			foreach (string f in flags) arguments.Remove(f);
			foreach (string flag in flags)
			{
				switch (flag[1])
				{
					case 'F':
						fragment = true;
						break;
				}
			}

			if (arguments.Count != 2)
				printUsageAndExit();
			if (!Directory.Exists(arguments[1]))
			{
				Console.WriteLine("Directory {0} not found!", arguments[1]);
				Console.WriteLine();
				printUsageAndExit();
			}
			if (!File.Exists(arguments[0]))
			{
				Console.WriteLine("LabelMapper Description File {0} not found!{1}", arguments[0], Environment.NewLine);
				printUsageAndExit();
			}
			string lmFile = arguments[0];

			List<string> files = new List<string>(Directory.GetFiles(arguments[1], "*.xml", SearchOption.AllDirectories));
			if (files.Count == 0)
			{
				Console.WriteLine("No XML files found in {0}!{1}", arguments[1], Environment.NewLine);
				printUsageAndExit();
			}
			string outputDirectory;
			if (fragment)
			{
				outputDirectory = arguments[1] + @"\DTF_Fragmented";
				Console.WriteLine("Will auto-fragment all sketches");
			}
			else
			{
				outputDirectory = arguments[1] + @"\DTF_Unfragmented";
				Console.WriteLine("Will not auto-fragment any sketches");
			}
			if (!Directory.Exists(outputDirectory))
			{
				Console.WriteLine("Creating output directory {0}", outputDirectory);
				Directory.CreateDirectory(outputDirectory);
			}
			else
				Console.WriteLine("Using existing output directory {0}", outputDirectory);

			#endregion

			string names_file = outputDirectory + "\\features.names";

			LabelMapper.LabelMapper lm = new LabelMapper.LabelMapper(lmFile);
			DTF all = new DTF(lm.translatedClasses);
			addFeaturesToDTF(ref all);
			all.WriteNamesFile(names_file);

			Console.WriteLine("Beginning DTF calculations at {0}", DateTime.Now.ToShortTimeString());

			foreach (string filename in files)
			{
				Console.Write(".");
				if (!File.Exists(filename))
					throw new FileNotFoundException("File {0} not found", filename);
				Sketch.Sketch sk;
				try
				{
					ConverterXML.ReadXML reader = new ConverterXML.ReadXML(filename);
					sk = reader.Sketch;
				}
				catch (System.Xml.XmlException e)
				{
					Console.WriteLine("Error reading file {0} with ConverterXML. Exception Text: {1}", filename, e.Message);
					continue;
				}
				if (fragment)
					Fragmenter.Fragment.fragmentSketch(sk);
				FeatureSketch sketch = new FeatureSketch(ref sk);

				Node[] nodes = new Node[sk.Substrokes.Length];
				for (int i = 0; i < sk.Substrokes.Length; ++i)
					nodes[i] = new Node(sk.Substrokes[i], lm.translatedClasses.Count, i);

				#region Calculate Features

				double totalMinDistBetweenFrag = sketch.TotalMinDistBetweenSubstrokes;
				double totalAverageDistBetweenFrag = sketch.TotalAvgDistBetweenSubstrokes;
				double totalMaxDistBetweenFrag = sketch.TotalMaxDistBetweenSubstrokes;
				double totalTimeBetweenFrag = CreateGraph.totTimeBetweenFrag(nodes);
				double totalArcLength = sketch.TotalArcLength;
				double totalLengthOfFrag = sketch.TotalDistance;
				double averageSpeedOfFrag = sketch.AverageAverageSpeed;
				double totalMinDistBetweenEnds = sketch.TotalMinDistBetweenSubstrokes;
				double[] bbox = sketch.BBox.ToArray();


				SiteFeatures.setStageNumber(1);
				InteractionFeatures.setStageNumber(1);
				SiteFeatures site = new SiteFeatures(totalMinDistBetweenFrag,
													totalAverageDistBetweenFrag,
													totalMaxDistBetweenFrag,
													totalTimeBetweenFrag,
													totalArcLength,
													totalLengthOfFrag,
													averageSpeedOfFrag,
													totalMinDistBetweenEnds,
													bbox, ref sketch);
				InteractionFeatures inter = new InteractionFeatures(totalMinDistBetweenFrag,
													totalAverageDistBetweenFrag,
													totalMaxDistBetweenFrag,
													totalTimeBetweenFrag,
													totalArcLength,
													totalLengthOfFrag,
													averageSpeedOfFrag,
													totalMinDistBetweenEnds,
													ref sketch);

				#endregion

				#region Write out

				for (int i = 0; i < nodes.Length; ++i)
				{
					if (!lm.labelMap.ContainsKey(sk.Substrokes[i].FirstLabel))
					{
						Console.WriteLine("Label {0} not found in label map, continuing", sk.Substrokes[i].FirstLabel);
						continue;
					}
					double[] ret = site.evalSiteFeatures(nodes[i], sk.Substrokes);
					int allIdx = addSSStrokeToDTF(ref all, ref ret, ref sk.Substrokes[i], ref lm);
					ret = aggregateInteractionFeatures(inter, nodes[i], nodes, sk.Substrokes);
					addMSStrokeToDTF(ref all, ref ret, allIdx);
				}

				#endregion
			}
			Console.Write(Environment.NewLine);
			all.WriteDataFile(outputDirectory + "\\all.data");
			Console.WriteLine("DTF Calculations finished at {0}", DateTime.Now.ToShortTimeString());
		}

		private static void printUsageAndExit()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("\tCRFFeaturesToDTF.exe LabelMapperFile Directory");
			Console.WriteLine("\t\tThis program will write DecisionTree features to Directory/DTF for all .xml files in Directory");
			Environment.Exit(2);
		}

		private static void addFeaturesToDTF(ref DTF dtf)
		{
			// Single stroke features
			dtf.AddFeature("biasFunction", false);
			dtf.AddFeature("distBetweenEndsLarge", false);
			dtf.AddFeature("distBetweenEndsSmall", false);
			dtf.AddFeature("arcLengthShort", false);
			dtf.AddFeature("arcLengthLong", false);
			dtf.AddFeature("turningZero", false);
			dtf.AddFeature("turningSmall", false);
			dtf.AddFeature("turning360", false);
			dtf.AddFeature("turningLarge", false);
			dtf.AddFeature("distFromLR", true);
			dtf.AddFeature("distFromTB", true);
			dtf.AddFeature("twoCorners", false);
			// Multi-stroke features
			dtf.AddFeature("numTouching", true);
			dtf.AddFeature("numMinDistSmall", true);
			dtf.AddFeature("numMinDistLarge", true);
			dtf.AddFeature("numMaxDistSmall", true);
			dtf.AddFeature("numMaxDistLarge", true);
			dtf.AddFeature("numCorners", true);
			dtf.AddFeature("numMinDistEndsLarge", true);
			dtf.AddFeature("numDistAvgPtsSmall", true);
			dtf.AddFeature("numDistAvgPtsLarge", true);
			dtf.AddFeature("numArePerpendicular", true);
			dtf.AddFeature("numAreParallel", true);
			dtf.AddFeature("MSbiasFunction", false);
			dtf.AddFeature("numEndsClose", true);
			dtf.AddFeature("numPenLifted", true);
			dtf.AddFeature("numStraightCurved", true);
			dtf.AddFeature("numStraightStraight", true);
			dtf.AddFeature("numCurvedStraight", true);
			dtf.AddFeature("numCurvedCurved", true);
			dtf.AddFeature("numSimilarlyBounded", true);
			dtf.AddFeature("numAngleSmall", true);
			dtf.AddFeature("numAngleLarge", true);
		}

		private static int addSSStrokeToDTF(ref DTF dtf, ref double[] ret, ref Substroke ss, ref LabelMapper.LabelMapper lm)
		{
			int idx = dtf.AddStroke(lm.labelMap[ss.FirstLabel], ss.Id);
			dtf.AddObservationToStroke(idx, "biasFunction", ret[0]);
			dtf.AddObservationToStroke(idx, "distBetweenEndsLarge", ret[1]);
			dtf.AddObservationToStroke(idx, "distBetweenEndsSmall", ret[2]);
			dtf.AddObservationToStroke(idx, "arcLengthShort", ret[3]);
			dtf.AddObservationToStroke(idx, "arcLengthLong", ret[4]);
			dtf.AddObservationToStroke(idx, "turningZero", ret[5]);
			dtf.AddObservationToStroke(idx, "turningSmall", ret[6]);
			dtf.AddObservationToStroke(idx, "turning360", ret[7]);
			dtf.AddObservationToStroke(idx, "turningLarge", ret[8]);
			dtf.AddObservationToStroke(idx, "distFromLR", ret[9]);
			dtf.AddObservationToStroke(idx, "distFromTB", ret[10]);
			dtf.AddObservationToStroke(idx, "twoCorners", ret[11]);
			return idx;
		}

		private static void addMSStrokeToDTF(ref DTF dtf, ref double[] ret, int idx)
		{
			dtf.AddObservationToStroke(idx, "numTouching", ret[0]);
			dtf.AddObservationToStroke(idx, "numMinDistSmall", ret[1]);
			dtf.AddObservationToStroke(idx, "numMinDistLarge", ret[2]);
			dtf.AddObservationToStroke(idx, "numMaxDistSmall", ret[3]);
			dtf.AddObservationToStroke(idx, "numMaxDistLarge", ret[4]);
			dtf.AddObservationToStroke(idx, "numCorners", ret[5]);
			dtf.AddObservationToStroke(idx, "numMinDistEndsLarge", ret[6]);
			dtf.AddObservationToStroke(idx, "numDistAvgPtsSmall", ret[7]);
			dtf.AddObservationToStroke(idx, "numDistAvgPtsLarge", ret[8]);
			dtf.AddObservationToStroke(idx, "numArePerpendicular", ret[9]);
			dtf.AddObservationToStroke(idx, "numAreParallel", ret[10]);
			dtf.AddObservationToStroke(idx, "MSbiasFunction", ret[11]);
			dtf.AddObservationToStroke(idx, "numEndsClose", ret[12]);
			dtf.AddObservationToStroke(idx, "numPenLifted", ret[13]);
			dtf.AddObservationToStroke(idx, "numStraightCurved", ret[14]);
			dtf.AddObservationToStroke(idx, "numStraightStraight", ret[15]);
			dtf.AddObservationToStroke(idx, "numCurvedStraight", ret[16]);
			dtf.AddObservationToStroke(idx, "numCurvedCurved", ret[17]);
			dtf.AddObservationToStroke(idx, "numSimilarlyBounded", ret[18]);
			dtf.AddObservationToStroke(idx, "numAngleSmall", ret[19]);
			dtf.AddObservationToStroke(idx, "numAngleLarge", ret[20]);
		}

		private static double[] aggregateInteractionFeatures(InteractionFeatures inter, Node nui, Node[] nodes, Substroke[] substrokes)
		{
			double[] ret = new double[21];

			int indexToSkip = Array.IndexOf(nodes, nui);

			for (int i = 0; i < nodes.Length; ++i)
			{
				if (i == indexToSkip) continue;
				double[] intermediate = inter.evalInteractionFeatures(nui, nodes[i], substrokes);
				for (int j = 0; j < intermediate.Length; ++j)
				{
					// Move the values to be between 0 and 1 instead of between -1 and 1
					intermediate[j] += 1;
					intermediate[j] /= 2;
				}
				for (int j = 0; j < ret.Length; ++j)
				{
					ret[j] += intermediate[j];
				}
			}

			return ret;
		}
	}
}
