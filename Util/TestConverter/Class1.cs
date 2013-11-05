/**
 * File: Class1.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.IO;
using System.Collections;
using System.Drawing;
using Grouper;

namespace TestConverter
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		static Random random = new Random((int)DateTime.Now.Ticks);

		const string	DEFAULT_FILENAME = "test2";
		const int		DEFAULT_METHOD	 = Grouper.Grouper.DEBUG;
		const int		DEFAULT_COLOR    = Grouper.Grouper.COLORMODE_RANDOM;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ArrayList files = new ArrayList();
			int method	  = DEFAULT_METHOD;
			int colorMode = DEFAULT_COLOR;

			#region COMMAND-LINE PROCESSING
			for(int i=0; i<args.Length; i++)	
			{
				switch(args[i])	
				{
					case "-f":		// provide file
						i++;
						if(i == args.Length)	
						{
							Console.WriteLine("You must supply a filename after the -f flag");
							break;
						}
						files.AddRange(readFilenamesFromFile(args[i]));
						break;
					case "-m":
						i++;
						if(i == args.Length)	
						{
							Console.WriteLine("You must supply a method after the -m flag");
							break;
						}
						try	
						{
							method = System.Int32.Parse(args[i]);
						} 
						catch(FormatException)	
						{
							Console.WriteLine("Method argument was not a number.");
							method = DEFAULT_METHOD;
						}
						break;
					case "-d":
						i++;
						if(i == args.Length)	
						{
							Console.WriteLine("You must supply a directory after the -d flag");
							break;
						}
						try	
						{
							string[] directoryFiles = Directory.GetFiles(args[i]);
							foreach (string file in directoryFiles)	
							{
								if (file.IndexOf(".grp") < 0)	
								{
									Console.WriteLine(file);
									files.Add(removeFilenameExtension(file));
								}
							}
						} 
						catch (DirectoryNotFoundException e)	
						{
							Console.Error.WriteLine(e.ToString());
						}
						break;
					case "-c":
						i++;
						if(i == args.Length)	
						{
							Console.WriteLine("You must supply a color mode after the -c flag");
							break;
						}
						switch(args[i].ToLower())	
						{
							case "rand":
								colorMode = Grouper.Grouper.COLORMODE_RANDOM;
								break;
							case "list":
								colorMode = Grouper.Grouper.COLORMODE_LIST;
								break;
							case "none":
								colorMode = Grouper.Grouper.COLORMODE_NONE;
								break;
							default:
								Console.WriteLine("You must supply a color mode ('rand' or 'list' or 'none')");
								break;
						}
						break;
					default:
						// Anything that isn't a flag is a filename
						string line = args[i];
						if(line.Substring(line.Length-4, 4) == ".xml")	
						{
							line = line.Substring(0, line.Length-4);
						}
						files.Add(line);
						break;
				}
			}
			// Default target (if none is there)
			if (files.Count == 0)	
			{
				Console.WriteLine("Using default filename: " + DEFAULT_FILENAME);
				files.Add(DEFAULT_FILENAME);
			}
			#endregion

			Class1 test = new Class1();
			foreach (string filename in files)	
			{
				test.testSegment(filename, method, colorMode);
			}
			//test.testMakeHHRECO();
			//test.testWriteSVM();
			//test.testFeatures();
			//test.testReadInk();
			//test.testWriteXML();			
			//test.testReadXML();
			//test.testSplit();
		}

		private static ArrayList readFilenamesFromFile(string filename)	
		{
			ArrayList files = new ArrayList();
			System.IO.StreamReader reader = new StreamReader(filename);

			string line;
			while((line = reader.ReadLine()) != null)	
			{
				if(line.Substring(line.Length-4, 4) == ".xml")	
				{
					line = line.Substring(0, line.Length-4);
				}
				files.Add(line);
			}

			return files;
		}

		private static string removeFilenameExtension(string filename)	
		{
			int extlen = filename.Length - filename.LastIndexOf(".");

			if(extlen == filename.Length)
				return filename;

			return filename.Substring(0, filename.Length - extlen);
		}

		public void testSegment(string filename, int method, int colorMode)
		{		
			Console.WriteLine();
			Console.WriteLine("GROUPING: " + filename + ".xml");

			//Console.Write("Loading XML file...");
			ConverterXML.ReadXML reader;
			try
			{
				reader = new ConverterXML.ReadXML(filename + ".xml");
				//Console.WriteLine("Done");
			} 
			catch(FileNotFoundException)	
			{
				Console.WriteLine("ERROR: Could not find file: " + filename + ".xml");
				return;
			}

			//Console.WriteLine("Grouping...");
			Grouper.Grouper grouper = new Grouper.Grouper(reader.Sketch, method);
			grouper.groupAllSubstrokes();
			//Console.WriteLine("Done");

			//Console.Write("Writing output to " + filename + "...");
			grouper.writeToXML(filename, colorMode);
            //grouper.
		}

		#region TESTS
		public void testMakeHHRECO()
		{
			string filename = "c:\\trainingexamples.labeled.xml";

			ConverterXML.ReadXML testRead = new ConverterXML.ReadXML(filename);

			ConverterXML.MakeHHRECO hhreco = new ConverterXML.MakeHHRECO(testRead.Sketch);
			hhreco.writeXML("c:\\test.sml");
		}

		public string three(int i)
		{
			if(i < 10)
				return "00" + i;
			else
                return "0" + i;
		}

		public void testWriteSVM()
		{
			string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\JntToXml\\bin\\Release\\convertedJnt\\boxcir.1.fragged.labeled.xml";
			//string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\JntToXml\\bin\\Release\\convertedJnt\\circ_sq_test.1.fragged.labeled.xml";
			ConverterXML.ReadXML testRead = new ConverterXML.ReadXML(filename);

			Sketch.Substroke[] substrokes = testRead.Sketch.Substrokes;
			Featurefy.Normal normal = new Featurefy.Normal(substrokes);
			double[][] features = normal.Features;

			FileStream file = new FileStream(filename + ".train", FileMode.Create, FileAccess.Write);
            
			StreamWriter sw = new StreamWriter(file);


			int length = substrokes.Length;
			for(int i = 0; i < length; ++i)
			{
				/*
				if(i == length - 1)
					Console.WriteLine();
				*/
				string[] labels = substrokes[i].GetLabels();

				if(labels.Length > 0 && substrokes[i].GetLabels()[0].ToLower().Equals("circle"))
					sw.Write("1 ");
				else
					sw.Write("-1 ");

				int dlength = features[i].Length;

				for(int j = 0; j < dlength; ++j)
				{
					sw.Write((j + 1) + ":" + features[i][j] + " ");
				}
				sw.WriteLine();
			}

			sw.Close();
			file.Close();
		}

		public void testFeatures()
		{
			/*
			string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\TestConverter\\bin\\Debug\\yodude.xml";

			ConverterXML.ReadXML testRead = new ConverterXML.ReadXML(filename);


			Sketch.Stroke toSplit;
			int length = testRead.Sketch.Strokes.Length;
			for(int i = 0; i < length; ++i)
			{				
				toSplit = (Sketch.Stroke)testRead.Sketch.Strokes[i];
				int numPoints = toSplit.Points.Length;
				int[] indices = new int[] { numPoints / 4, numPoints / 2, numPoints * 3 / 4 };
				toSplit.SplitSubstrokeAt(0, indices);
			}

			toSplit = (Sketch.Stroke)testRead.Sketch.Strokes[0];
			toSplit.SplitSubstrokesAt(new int[] {0, 1, 2, 3}, new int[][]{ new int[]{5}, new int[]{5}, new int[]{5}, new int[]{5} });
			toSplit.SplitStrokeAt(new int[] { 20, 40, 50 });
		
			Featurefy.Normal normal = new Featurefy.Normal(testRead.Sketch.Substrokes);
			*/
			Console.WriteLine("Method temporarily disabled since it won't compile and I'm trying to get something else done. -Ned");
		}

		public void testSplit()
		{
			/*
			string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\TestConverter\\bin\\Debug\\yodude.xml";

			ConverterXML.ReadXML testRead = new ConverterXML.ReadXML(filename);


			Sketch.Stroke toSplit;
			int length = testRead.Sketch.Strokes.Length;
			for(int i = 0; i < length; ++i)
			{				
				toSplit = (Sketch.Stroke)testRead.Sketch.Strokes[i];
				int numPoints = toSplit.Points.Length;
				int[] indices = new int[] { numPoints / 4, numPoints / 2, numPoints * 3 / 4 };
				toSplit.SplitSubstrokeAt(0, indices);
			}

			toSplit = (Sketch.Stroke)testRead.Sketch.Strokes[0];
			toSplit.SplitSubstrokesAt(new int[] {0, 1, 2, 3}, new int[][]{ new int[]{5}, new int[]{5}, new int[]{5}, new int[]{5} });
			toSplit.SplitStrokeAt(new int[] { 20, 40, 50 });
			*/
			Console.WriteLine("Method temporarily disabled since it won't compile and I'm trying to get something else done. -Ned");
		}

		public void testReadInk()
		{
			/*
			ConverterJnt.ReadJnt ink = new ConverterJnt.ReadJnt("yodude.jnt");
			ConverterXML.MakeXML xml = new ConverterXML.MakeXML(ink.Sketch);
			xml.WriteXML("yodude.xml");
			*/
			Console.WriteLine("Method temporarily disabled since it won't compile and I'm trying to get something else done.");
		}

		public void testWriteXML()
		{
			Sketch.Sketch sketch = new Sketch.Sketch();
			sketch.XmlAttrs.Id = System.Guid.NewGuid();

			Sketch.Shape shape = new Sketch.Shape();
			shape.XmlAttrs.Id = System.Guid.NewGuid();
			shape.XmlAttrs.Name = "shapeName";
			shape.XmlAttrs.Type = "shape";
			shape.XmlAttrs.Time = "111";

			Sketch.Stroke stroke;
			Sketch.Substroke substroke;
			Sketch.Point point;

			ulong h;
			ulong i;
			ulong j;

			for(h = 0; h < 3; ++h)
			{
				stroke = new Sketch.Stroke();
				stroke.XmlAttrs.Id = System.Guid.NewGuid();
				stroke.XmlAttrs.Name = "strokeName";
				stroke.XmlAttrs.Type = "stroke";
				stroke.XmlAttrs.Time = h;

				for(i = 0; i < 5; ++i)
				{
					substroke = new Sketch.Substroke();
					substroke.XmlAttrs.Id = System.Guid.NewGuid();
					substroke.XmlAttrs.Name = "substrokeName";
					substroke.XmlAttrs.Type = "substroke";
					substroke.XmlAttrs.Time = i;

					for(j = 0; j < 10; ++j)
					{			
						point = new Sketch.Point();
						point.XmlAttrs.X = (float)random.NextDouble();
						point.XmlAttrs.Y = (float)random.NextDouble();
						point.XmlAttrs.Id = System.Guid.NewGuid();
						point.XmlAttrs.Pressure = (ushort)(random.NextDouble() * 256);
						point.XmlAttrs.Name = "point" + j.ToString();
						point.XmlAttrs.Time = j;

						substroke.AddPoint(point);
					}
					stroke.AddSubstroke(substroke);
					shape.AddSubstroke(substroke);
				}
				sketch.AddStroke(stroke);
			}
			sketch.AddShape(shape);

			sketch.Strokes[0].SplitStrokeAt(new int[] {7, 14, 16, 21} );

			ConverterXML.MakeXML xml = new ConverterXML.MakeXML(sketch);
			xml.WriteXML("test.xml"); 			
		}

		public void testReadXML()
		{
			/*
			//string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\TestConverter\\bin\\Debug\\test.xml";
			//string filename = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\TestConverter\\bin\\Debug\\test.xml";
			//string filename = "C:\\Documents and Settings\\Da Vinci\\Desktop\\testXML.xml";
			//string filename = "C:\\Documents and Settings\\Devin Smith\\My Documents\\Visual Studio Projects\\branches\\redesign\\TestConverter\\bin\\Debug\\yodude.xml";
			//string filename = "circuit.1.labeled.xml";
			string filename = "c:\\wire-gate1.labeled.xml";

			ConverterXML.ReadXML testRead = new ConverterXML.ReadXML(filename);
			

			ConverterXML.MakeXML xml = new ConverterXML.MakeXML(testRead.Sketch);
			xml.WriteXML("c:\\same.xml");

			
			//ConverterXML.ReadXML testRead2 = new ConverterXML.ReadXML("test1.xml");
			//xml.writeXML("test2.xml");
			

			*/
			Console.WriteLine("Method temporarily disabled since it won't compile and I'm trying to get something else done. -Ned");
		}
		#endregion
	}
}
