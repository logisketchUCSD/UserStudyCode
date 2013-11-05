using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Sketch;
using ConverterXML;

namespace ConverterDRS
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("BatchDRSConverter: Convert DRS formatted files to MIT XML formatted files");
                Console.WriteLine("usage: BatchDRSConverter.exe [-d input_directory] [-o output_directory] [files]");
                Console.WriteLine("if an input directory is given, extra files on the command line are ignored");

                Environment.Exit(1);
            }

            string indir="", outdir="";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-d") indir = args[++i];
                else if (args[i] == "-o") outdir = args[++i];
            }

            List<string> files = new List<string>();
            if (indir == "")
            {
                for (int i = 0; i < args.Length; i++)
                    if (args[i] == "-o") i++;
                    else files.Add(args[i]);
            }
            else
            {
                if (!Directory.Exists(indir))
                {
                    Console.WriteLine("Input directory '{0}' does not exist. Exiting.", indir);
                    Environment.Exit(1);
                }
                files.AddRange(Directory.GetFiles(indir, ".drs"));
                files.AddRange(Directory.GetFiles(indir, ".DRS"));
            }

            foreach (string file in files)
            {
                if (!File.Exists(file))
                    Console.WriteLine("File '{0}' does not exist. Continuing.", file);

                Sketch.Sketch sketch = ReadDRS.load(file);
                MakeXML mxml = new MakeXML(sketch);

                string outfile = "";
                if (outdir == "")
                    outfile = file.Substring(0, file.LastIndexOf('.')) + ".xml";
                else
                    outfile = outdir + file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar),
                        file.LastIndexOf('.') - file.LastIndexOf(Path.DirectorySeparatorChar)) + ".xml";

                mxml.WriteXML(outfile);
            }

        }
    }
}
