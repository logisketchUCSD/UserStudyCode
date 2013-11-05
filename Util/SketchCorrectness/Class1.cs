using System;
using System.Collections;
using ConverterXML;

namespace SketchCorrectness
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
			ArrayList aArgs = new ArrayList(args);
			bool withId = aArgs.Contains("-id");
			if(withId)
				aArgs.Remove("-id");
		
			if(withId && args.Length < 3 || !withId && args.Length < 2)
			{
				Console.WriteLine("SketchCorrectness.exe [-id] correctSketch.xml compareSketch.xml");
				return;
			}


			ConverterXML.ReadXML read1 = new ReadXML((string)aArgs[0]);
			ConverterXML.ReadXML read2 = new ReadXML((string)aArgs[1]);

			Console.WriteLine(read1.Sketch.CompareToForCorrectness(read2.Sketch, withId));
		}
	}
}
