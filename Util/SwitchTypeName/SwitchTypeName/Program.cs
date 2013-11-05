using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using Converter;
using Sketch;

namespace SwitchTypeName
{
    class Program
    {
        static string endExtension = "*.labeled.xml";
        static string newExtension = ".switched.xml";

        static void Main(string[] args)
        {
            ArrayList argArray = new ArrayList(args);
            int numArgs = argArray.Count;
            

            string[] files;

            if (numArgs == 0)
            {
                Console.WriteLine("***************************************************************************");
                Console.WriteLine("*** SwitchTypeName.exe                                                    *");
                Console.WriteLine("*** by Team 2007.                                                         *");
                Console.WriteLine("*** Harvey Mudd College, Claremont, CA 91711.                             *");
                Console.WriteLine("*** Sketchers 2007.                                                       *");
                Console.WriteLine("***                                                                       *");
                Console.WriteLine("*** Usage: SwitchTypeName.exe (-c | -d directory | -r) (-i in) (-o out)   *");
                Console.WriteLine("***                                                                       *");
                Console.WriteLine("*** Default Extensions: -i labeled.xml -o switched.xml                    *");
                Console.WriteLine("***                                                                       *");
                Console.WriteLine("*** -c: switch all files in current directory                             *");
                Console.WriteLine("*** -d directory: switch all files in the specified directory             *");
                Console.WriteLine("*** -r: recursively switch files from the current directory               *");
                return;
            }
            else
                files = getFiles(argArray);
                if (files == null)
                    return;

                foreach (string filename in files)
                {
                    Console.WriteLine("Trying " + filename + "...");
                    switchFile(filename);
                }
        }

        static string[] getFiles(ArrayList argArray)
        {
            string[] files;

            if (argArray.Contains("-i"))
                Program.endExtension = "*." + (string)argArray[argArray.IndexOf("-i") + 1];
          
            if (argArray.Contains("-o"))
                Program.newExtension = "." + (string)argArray[argArray.IndexOf("-o") + 1];

            if (argArray.Contains("-c")) // Convert everything in this directory
            {
                files = Directory.GetFiles(Directory.GetCurrentDirectory(), Program.endExtension);
            }
            else if (argArray.Contains("-d")) // Convert everything in specified directory
            {
                if (argArray.IndexOf("-d") + 1 >= argArray.Count)	// Are we in range?
                {
                    Console.Error.WriteLine("No directory specified.");
                    return null;
                }
                else if (!Directory.Exists((string)argArray[argArray.IndexOf("-d") + 1])) // Does dir exist?
                {
                    Console.Error.WriteLine("Directory doesn't exist.");
                    return null;
                }
                else
                    files = Directory.GetFiles((string)argArray[argArray.IndexOf("-d") + 1], Program.endExtension);
            }
            else if (argArray.Contains("-r")) //Recursive from current dir
            {
                //Get recursive files
                ArrayList rFiles = new ArrayList();
                DirSearch(Directory.GetCurrentDirectory(), ref rFiles);

                //Get current dir files
                string[] currDir = Directory.GetFiles(Directory.GetCurrentDirectory(), Program.endExtension);

                files = new string[rFiles.Count + currDir.Length];

                //populate both recursive and current into files
                int current;
                for (current = 0; current < currDir.Length; ++current)
                    files[current] = currDir[current];

                foreach (string s in rFiles)
                {
                    files[current++] = s;
                }
            }
            else //Convert only the specified files
            {
                files = null;
            }

            return files;
        }

        /// <summary>
        /// Change an labeled XML file into a new one.
        /// </summary>
        /// <param name="filename"></param>
        static void switchFile(string filename)
        {
            string outputFilename = filename.Remove(filename.Length - Program.endExtension.Length + 1) + Program.newExtension;
            if (Directory.Exists(outputFilename))
                return;

            try
            {
                //Create a sketch
                Sketch.Sketch sketch;
                sketch = (new ReadXML(filename)).Sketch;
                            
                //Go through all of the shapes (labeled items)
                foreach(Sketch.Shape shape in sketch.Shapes)
                {
                    String type = shape.XmlAttrs.Type.ToString();
                    String name = shape.XmlAttrs.Name.ToString();

                    //Make sure it is the old format
                    if (type.Equals("shape"))
                    {
                        shape.XmlAttrs.Type = name;
                        shape.XmlAttrs.Name = type;
                    }
                }

                //Remove old .labeled.xml and append .switched.xml and write it out
                (new MakeXML(sketch)).WriteXML(outputFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);
                Console.WriteLine(e.StackTrace);
                Console.ReadLine();
                return;
            }

        }

        /// <summary>
		/// Perform a recursive directory search. http://support.microsoft.com/default.aspx?scid=kb;en-us;303974
		/// </summary>
		/// <param name="sDir">Directory to search recursively</param>
		/// <param name="rFiles">Array to add the files to</param>
        static void DirSearch(string sDir, ref ArrayList rFiles)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, Program.endExtension))
                    {
                        rFiles.Add(f);
                    }
                    DirSearch(d, ref rFiles);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
