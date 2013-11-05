using System;
using System.IO;
using System.Collections;

namespace DataAnalysis
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			DataAnalyzer analyzer = new DataAnalyzer( );
			// Read in the files in the input file
			if (args.Length < 1)
			{
				Console.WriteLine( "Please enter a file specifying which files to load" );
				Console.WriteLine( "Usage: DataAnalysis filename.txt" );
				return;
			}

			// Here I will process more command line arguments for statistics that we care about

			// Open the file for reading 
				try 
				{
					// Create an instance of StreamReader to read from a file.
					// The using statement also closes the StreamReader.
					using (StreamReader sr = new StreamReader(args[0])) 
					{
						String line;
						// Read and display lines from the file until the end of 
						// the file is reached.
						while ((line = sr.ReadLine()) != null) 
						{
							String[] sketchInfo = line.Split();
							if (File.Exists(sketchInfo[1])) 
							{
								Console.WriteLine( "Loading file {0} for user {1}", sketchInfo[1], sketchInfo[0] );
								analyzer.loadSketch(sketchInfo[1], Convert.ToInt32(sketchInfo[0]));
							}
						}
					}
				}
				catch (Exception e) 
				{
					// Let the user know what went wrong.
					Console.WriteLine("The file could not be read:");
					Console.WriteLine(e.Message);
				}
			//analyzer.testStatistics();
			//analyzer.objectStats();
			//analyzer.consecutiveStats();
			//analyzer.timingStats("C:\\cygwin\\home\\alvarado\\research\\sketch\\dataCollection\\analysis\\timingStats2.txt");
			//analyzer.strokesPerShape("C:\\cygwin\\home\\alvarado\\research\\sketch\\dataCollection\\analysis\\strokesStats2.txt");
			//analyzer.consecutiveStats( "C:\\cygwin\\home\\alvarado\\research\\sketch\\dataCollection\\analysis\\cStats2.txt" );
			analyzer.globalStats("C:\\cygwin\\home\\alvarado\\research\\sketch\\dataCollection\\analysis\\genStats2.csv" );
		}
	}
}
