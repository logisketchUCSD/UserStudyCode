/**
 * File:    Program.cs
 * 
 * Author:  Sketchers 2007
 *
 * Purpose: See printHelp()
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using Sketch;
using Featurefy;
using ConverterXML;
using statistic;

namespace ThresholdEval
{
    class Program
    {
        static string FILE_EXTENSION = "*.xml";

        static void Main(string[] args)
        {
            # region Variables
            bool statistics   = false;
            bool recursion    = false;
            bool comparison   = false;
            bool autopilot    = false;
            int numIters      = 0;
            string dirLabeled = "";
            string dirLabeled2 = "";
            string dirCheck   = "";
            string criterion  = "";
            string logFile    = "";
            List<string> dataFilesCheck = new List<string>();
            List<string> dataFilesLabeled = new List<string>();
            List<string> dataFilesLabeled2 = new List<string>();
            # endregion

            #region Command line parsing
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-h":
                        printHelp();
                        break;
                    case "-r":
                        recursion = true;
                        break;
                    case "-s":
                        statistics = true;
                        ++i;
                        #region Arg check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify a criterion and a directory after the option -s.");
                            return;
                        }
                        #endregion
                        criterion = args[i];
                        ++i;
                        #region Dir check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify a directory after the criterion.");
                            return;
                        }
                        #endregion
                        dirCheck = args[i];
                        break;
                    case "-l":
                        ++i;
                        #region Arg check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify a file after the option -l.");
                            return;
                        }
                        #endregion
                        logFile = args[i];
                        break;
                    case "-n":
                        ++i;
                        numIters = Convert.ToInt32(args[i]);
                        #region Arg check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify the number of tests after the flag -n.");
                            return;
                        }
                        #endregion
                        break;
                    case "-c":
                        comparison = true;
                        ++i;
                        #region Dir check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify 2 directories after the option -c.");
                            return;
                        }
                        #endregion
                        dirCheck = args[i];
                        ++i;
                        #region Dir check
                        if (i >= args.Length)
                        {
                            Console.WriteLine("You need to specify 2 directories after the option -c.");
                            return;
                        }
                        #endregion
                        dirLabeled = args[i];
                        //++i;
                        //dirLabeled2 = args[i];
                        break;
                    case "-a":
                        autopilot = true;
                        break;
                    default:  
                        Console.WriteLine("Invalid arguements.");
                        return;
                }
            }
            #endregion

            #region Fun
            if (statistics)
            {
                parseDir(dirCheck, ref dataFilesCheck, recursion);
                Stats st = new Stats(dataFilesCheck);

                st.calcStats(criterion, logFile);
            }

            if (comparison)
            {
                //parseDir(dirCheck, ref dataFilesCheck, dirLabeled, ref dataFilesLabeled, 
                //    dirLabeled2, ref dataFilesLabeled2, recursion);
                parseDir(dirCheck, ref dataFilesCheck, dirLabeled, ref dataFilesLabeled, recursion);
                if (dataFilesCheck.Count == dataFilesLabeled.Count)
                {
                    //Accuracy acc = new Accuracy(dataFilesCheck, dataFilesLabeled, dataFilesLabeled2);
                    Accuracy acc = new Accuracy(dataFilesCheck, dataFilesLabeled);
                    acc.calcAccuracy(logFile);
                }
                else
                {
                    Console.WriteLine("Error: The two directories contain different number of files.");
                    return;
                }
            }

            if (autopilot)
            {
                // Note that we can have only one Autopilot per instance of the program,
                // because otherwise the computer is a having hard time keeping cool.
                Autopilot a = new Autopilot(logFile, numIters);
                a.start();
            }
            #endregion
        } 

        # region Helper functions

        /// <summary>
        /// Prints out help information.
        /// </summary>
        public static void printHelp()
        {
            Console.WriteLine("There is detailed info on the wiki.");
        }

        /// <summary>
        /// Searches the specified directory and saves filenames.
        /// </summary>
        /// <param name="dirCheck">Directory to be searched.</param>
        /// <param name="dataFiles">Container for the filenames.</param>
        /// <param name="recursion">If true, recursive search is performed.</param>
        static void parseDir(string dirCheck, ref List<string> dataFilesCheck, bool recursion)
        {
            if (Directory.Exists(dirCheck))
            {
                string[] files = Directory.GetFiles(dirCheck, FILE_EXTENSION);
                dataFilesCheck = new List<string>(files);

                if (recursion)
                    recParseDir(dirCheck, ref dataFilesCheck);
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
                printHelp();
                return;
            }
        }

        /// <summary>
        /// Searches the specified directories and saves filenames.
        /// </summary>
        /// <param name="dirCheck">Directory to be searched.</param>
        /// <param name="dataFiles1">Container for the filenames in dirCheck.</param>
        /// <param name="dirLabeled">Directory to be searched.</param>
        /// <param name="dataFiles2">Container for the filenames in dirLabeled.</param>
        /// <param name="recursion">If true, recursive search is performed.</param>
        //static void parseDir(string dirCheck, ref List<string> dataFilesCheck, string dirLabeled, ref List<string> dataFilesLabeled,
        //    string dirLabeled2, ref List<string> dataFilesLabeled2, bool recursion)
        static void parseDir(string dirCheck, ref List<string> dataFilesCheck, string dirLabeled,
            ref List<string> dataFilesLabeled, bool recursion)
        {
            parseDir(dirCheck, ref dataFilesCheck, recursion);
            parseDir(dirLabeled, ref dataFilesLabeled, recursion);
            //parseDir(dirLabeled2, ref dataFilesLabeled2, recursion);
        }

        /// <summary>
        /// Searches the directory recursively and saves files.
        /// </summary>
        /// <param name="dirCheck">Parent directory.</param>
        /// <param name="dataFiles">Container storing the filenames.</param>
        static void recParseDir(string dirCheck, ref List<string> dataFiles)
        {
            foreach (string dir in Directory.GetDirectories(dirCheck))
            {
                foreach (string file in Directory.GetFiles(dir, FILE_EXTENSION))
                {
                    dataFiles.Add(file);
                }
               
                recParseDir(dir, ref dataFiles);
            }
        }

        #endregion
    }
}
