/**
 * File:    Program.cs
 * 
 * Purpose: Changes the types "AND", "OR", "NOT", "NAND", "NOR", "XOR", "XNOR"
 *          to "Gate".
 * 
 * Authors: Skechers 2007
 *          Harvey Mudd College, Claremont, CA 91711.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using ConverterXML;
using Sketch;

namespace SwitchLabels
{
    class Program
    {

        static string OLD_EXTENSION = "*.xml";
        static string NEW_EXTENSION = ".wire-gate-label.xml";

        static void Main(string[] args)
        {
            const int FIRST_ARG = 0;
            const int SECOND_ARG = 1;

            if (args.Length != 2)
            {
                helpInfo();
            }
            else
            {

                string dirName = args[SECOND_ARG];
                bool existsDirectory = Directory.Exists(dirName);

                if (args[FIRST_ARG].ToLower() == "-d" && existsDirectory)
                {
                    processDir(dirName);
                }
                else
                {
                    Console.WriteLine("Direcory does not exist or wrong arguements");
                    helpInfo();
                }
            }
        }

        static void helpInfo()
        {
            Console.WriteLine("Usage: SwitchLabels.exe -d <Full Path of Directory>");
        }

        static void processDir(string dir)
        {
            string[] dataFiles = Directory.GetFiles(dir, OLD_EXTENSION);

            for (int index = 0; index < dataFiles.Length; ++index)
            {
                string filename = dataFiles[index];
                Console.WriteLine("Processing: {0}", filename);
                processFile(filename);
            }

        }

        /// <summary>
        /// processFile: performs the conversion of types.
        /// </summary>
        /// <param name="filename">Name of the file to be converted</param>
        static void processFile(string filename) 
        {
            string outputFilename = filename.Remove(filename.Length - Program.OLD_EXTENSION.Length + 1) + Program.NEW_EXTENSION;

            try
            {
                Sketch.Sketch sketch = (new ReadXML(filename)).Sketch;

                foreach (Sketch.Shape shape in sketch.Shapes)
                {
                    String type = shape.XmlAttrs.Type.ToString();

                    if (type.Equals("AND") || type.Equals("OR") || type.Equals("NOT") ||
                        type.Equals("NAND") || type.Equals("NOR") || type.Equals("XOR") ||
                        type.Equals("XNOR"))
                    {
                        shape.XmlAttrs.Type = "Gate";
                    }
                    else if (type.Equals("Other"))
                    {
                        sketch.RemoveShape(shape);
                    }
                    
                } 

                (new MakeXML(sketch)).WriteXML(outputFilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception was generated during conversion: {0}", e);
                return;
            }
        }
    }
    }

