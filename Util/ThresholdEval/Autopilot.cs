/**
 * File:    Autopilot.cs
 * 
 * Author:  Sketchers 2007
 *
 * Purpose: See printHelp() in Program.cs.
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

// Note that currently we are passing only 4 files for labeling (see wiki) but this number
// could easily be changed to the value we would want.
namespace ThresholdEval
{
    class Autopilot
    {

        //string infoFile = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\TESTRESULTS\Info.txt";
        string infoFile;
        int numIters;

        static bool nextTcrfCreated = false;

        //static bool firstThreadStarted = false;

        public Autopilot(string infoFile, int numIters)
        {
            this.numIters = numIters;
            this.infoFile = infoFile;
        }

        public void start()
        {
            #region Setting up directories, files and executables.
            StreamReader sr = new StreamReader(infoFile);

            string runCRFExe;
            getLineFromInfoFile(sr, out runCRFExe);
            //string runCRFExe = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\bin\Debug\RunCRF.exe";
            //Console.WriteLine("runCRFExe is " + runCRFExe);

            string labelFile;
            getLineFromInfoFile(sr, out labelFile);
            //string labelFile = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\bin\Debug\lnlDomainCRF.txt";
            //Console.WriteLine("labelfile is " + labelFile);

            string labelFile2;
            getLineFromInfoFile(sr, out labelFile2);
            //string labelFile = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\bin\Debug\wgDomainCRF.txt";
            //Console.WriteLine("labelfile2 is " + labelFile2);

            string inputFilesDir;
            getLineFromInfoFile(sr, out inputFilesDir);
            //string inputFilesDir = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\TrainingSwitchedUnFrag";
            //Console.WriteLine("inputFilesdir is " + inputFilesDir);

            string tcrfOutDir;
            getLineFromInfoFile(sr, out tcrfOutDir);
            //string tcrfOutDir = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\TESTRESULTS\";
            //Console.WriteLine("tcrfoutDir is " + tcrfOutDir);

            string foldName = "";
            string tcrfOut = "";
            string logFile = "";

            string xmlOutDir = tcrfOutDir;
            string xmlOut = "";

            string xmlInDir;
            getLineFromInfoFile(sr, out xmlInDir);
            //string xmlInDir = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Recognition\RunCRF\INPUT";
            //Console.WriteLine("xmlInDir is " + xmlInDir);

            string xmlIn = "";

            string thresholdEvalExe = "";
            getLineFromInfoFile(sr, out thresholdEvalExe);
            //string thresholdEvalExe = @"C:\Documents and Settings\Student\My Documents\Trunk2\Code\Util\ThresholdEval\bin\Debug\ThresholdEval.exe";
            //Console.WriteLine("thresholdEvalExe is " + thresholdEvalExe);

            // Hack .... change this to double later on.
            List<string> testParams = new List<string>();
            getDataFromInfoFile(sr, ref testParams, numIters);
            //double[] testParams   = new double[] { 1.3, 1.4, 1.5, 1.6, 1.7 };

            List<string> foldNames = new List<string>();
            getDataFromInfoFile(sr, ref foldNames, numIters);
            //string[] foldNames    = new string[] { "CircularInkLow1.3", "CircularInkLow1.4", "CircularInkLow1.5",
            //                                       "CircularInkLow1.6", "CircularInkLow1.7" };

            List<string> tcrfOuts = new List<string>();
            getDataFromInfoFile(sr, ref tcrfOuts, numIters);
            //string[] tcrfOuts = new string[] { "CircularInkLow1.3.tcrf", "CircularInkLow1.4.tcrf", "CircularInkLow1.5.tcrf",
            //                                   "CircularInkLow1.6.tcrf", "CircularInkLow1.7.tcrf" };

            List<string> logFiles = new List<string>();
            getDataFromInfoFile(sr, ref logFiles, numIters);
            //string[] logFiles     = new string[] { "CircularInkLow1.3Log.txt", "CircularInkLow1.4Log.txt", "CircularInkLow1.5Log.txt", 
            //                                       "CircularInkLow1.6Log.txt", "CircularInkLow1.7Log.txt" };

            List<string> xmlOuts = new List<string>();
            // We are always using the same 4 files.
            getDataFromInfoFile(sr, ref xmlOuts, 4);
            //string[] xmlOuts  = new string[] { "OutputFile1.xml", "OutputFile2.xml",
            //                                   "OutputFile3.xml", "OutputFile4.xml" };

            List<string> xmlIns = new List<string>();
            // Since we are always using 4 input files, we get 4 output files.
            getDataFromInfoFile(sr, ref xmlIns, 4);
            //string[] xmlIns   = new string[] { "INPUT1.xml", "INPUT2.xml", "INPUT3.xml", "INPUT4.xml" };


            // Debugging.
            /*ArrayList[] ar = new ArrayList[6] {testParams, foldNames, tcrfOuts, logFiles,
                            xmlOuts, xmlIns};
            for (int i = 0; i < ar.Length; ++i)
            {
               foreach (string str in ar[5]) 
                {
                    Console.WriteLine(str);
                }
                Console.WriteLine(ar[5].Count);
            }*/
            #endregion

            #region Let the fun begin.
            if (testParams.Count != foldNames.Count ||
                xmlIns.Count != xmlOuts.Count)
            {
                Console.WriteLine("ERROR: testParams and foldNames need to have the same length.");
                Console.WriteLine("       nameXMLIns and nameXMLOuts need to have the same lenths.");
                return;
            }

            Console.WriteLine("STARTING PROGRAM: {0}", DateTime.Now);

            for (int index = 0; index < testParams.Count; ++index)
            {
                foldName = (string)foldNames[index];
                tcrfOut = (string)tcrfOuts[index];
                logFile = (string)logFiles[index];

                // This is for multipassCRF
                //string RunCRFtrainCommandArgs = " -p2 -t -ft -n 2 -np2 2 -l " + labelFile + " -lp2 " +
                //   labelFile2 + " -o " + tcrfOutDir + foldName + "\\" + tcrfOut + "\" -d " + inputFilesDir;

                // This is for normalCRF
                string RunCRFtrainCommandArgs = " -t -ft -n 2 -l " + labelFile + " -o " + tcrfOutDir + 
                    foldName + "\\" + tcrfOut + "\" -d " + inputFilesDir;

                if (!Directory.Exists(tcrfOutDir.Remove(0, 1) + foldName))
                    Directory.CreateDirectory(tcrfOutDir.Remove(0, 1) + foldName); // UNCOMMENT

                //CRF.SiteFeatures.parameter = System.Double.Parse((string)testParams[index]);   // UNCOMMENT               

                // Console.WriteLine(RunCRFtrainCommand);
                // Here we start this process.
                /*Thread runRunCRFTrain = 
                    new Thread(new ThreadStart(Autopilot.startRunCRFTrain));

                if (!firstThreadStarted)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Starting a RunCRF thread in training mode. Threshold value: {0}", testParams[index]);
                    Console.WriteLine("    This thread is currently running.");

                    firstThreadStarted = true;
                    runRunCRFTrain.Start();
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("Ready to start a RunCRF thread in training mode. Threshold value: {0}.", testParams[index]);
                    Console.WriteLine("    This thread starts in 30 secs. Current time: {0}.", DateTime.Now);
                    Thread.Sleep(30000);
                    //Thread.Sleep(2400000);  // 40 mins
                    Console.WriteLine("    This thread is now running. Started at: {0}.", DateTime.Now);
                    runRunCRFTrain.Start();

                }
                */
                // runRunCRFTrain.Join();


                FileSystemWatcher watcher = new FileSystemWatcher();
                string path = tcrfOutDir + foldName;
                watcher.Path = path.Remove(0, 1);
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.Filter = "*.tcrf";
                watcher.Created += new FileSystemEventHandler(whenCreated);
                watcher.Changed += new FileSystemEventHandler(whenCreated);
                watcher.EnableRaisingEvents = true;
                
                Process RunCRFTrain = new Process();
                RunCRFTrain.StartInfo.FileName = runCRFExe;
                RunCRFTrain.StartInfo.Arguments = RunCRFtrainCommandArgs;
                RunCRFTrain.Start();
                Console.WriteLine("");
                Console.WriteLine("Started RunCRF.exe in training mode. Threshold value beeing tested: {0}.", testParams[index]);
                RunCRFTrain.WaitForExit();

                while (true)
                {
                    //Console.WriteLine("Weee, I am in the infinite loop.");
                    if (nextTcrfCreated)
                        break;
                }

                nextTcrfCreated = false;

                for (int i = 0; i < xmlIns.Count; ++i)
                {
                    xmlIn = (string)xmlIns[i];
                    xmlOut = (string)xmlOuts[i];

                    string pathToTcrf = tcrfOutDir + foldName + "\\" + tcrfOut + "\"";
                    string pathToTcrfP2 = tcrfOutDir + foldName + "\\" + tcrfOut + ".p2" + "\"";

                    //This is for multipass CRF.
                    //string RunCRFLabelCommandArgs = " -p2 -fl -c " + pathToTcrf + " " + pathToTcrfP2 + " -np2 2" + " -l " + labelFile + 
                    //    " -lp2 " + labelFile2 + " -o " + xmlOutDir + foldName + "\\" + xmlOut + "\" " + xmlInDir + "\\" + xmlIn + "\"";

                    //This is for normal CRF.
                    string RunCRFLabelCommandArgs = " -fl -c " + pathToTcrf + " -l " + labelFile + 
                        " -o " + xmlOutDir + foldName + "\\" + xmlOut + "\" " + xmlInDir + "\\" + xmlIn + "\"";

                    // Console.WriteLine(RunCRFLabelCommand);
                    // Here we start this process.
                    // Thread runRunCRFLabel = //UNCOMMENT
                    //    new Thread(new ThreadStart(Autopilot.startRunCRFLabel));
                    // Console.WriteLine("");
                    // Console.WriteLine("Starting a RunCRF thread in inference mode to label {0}.", xmlIn);
                    // runRunCRFLabel.Start();
                    // runRunCRFLabel.Join();
                    Process RunCRFLabel = new Process();
                    RunCRFLabel.StartInfo.FileName = runCRFExe;
                    RunCRFLabel.StartInfo.Arguments = RunCRFLabelCommandArgs;
                    RunCRFLabel.Start();
                    Console.WriteLine("    Started RunCRF.exe in labeling mode mode. File being labeled: {0}.", xmlIn);
                    RunCRFLabel.WaitForExit();
                }

                //string thresholdEvalCommandArgs = "-c " + xmlOutDir + foldName + "\" " +
                //        inputFilesDir + " -l " + xmlOutDir + foldName + "\\" + logFile + "\"";

                string thresholdEvalCommandArgs = "-c " + xmlOutDir + foldName + "\" " +
                        xmlInDir + "\" -l " + xmlOutDir + foldName + "\\" + logFile + "\"";

                //Console.WriteLine(thresholdEvalCommandArgs);
                //Console.ReadLine();

                // Console.WriteLine(thresholdEvalCommand);  
                // Here we start this process.
                // Thread runThreasholdEval =
                //     new Thread(new ThreadStart(Autopilot.startThresholdEval));
                // Console.WriteLine("Starting a ThresholdEval thread to find accuracy of the value: {0}.",
                //              testParams[index]);
                // runThreasholdEval.Start();
                // runThreasholdEval.Join();
                Process ThresholdEval = new Process();
                ThresholdEval.StartInfo.FileName = thresholdEvalExe;
                ThresholdEval.StartInfo.Arguments = thresholdEvalCommandArgs;
                ThresholdEval.Start();
                Console.WriteLine("");
                Console.WriteLine("    Started ThresholdEval.exe to find the accuracy of the value: {0}.", 
                    testParams[index]);
                ThresholdEval.WaitForExit();
            }

            Console.WriteLine("ENDING PROGRAM: {0}", DateTime.Now);
            #endregion
        }

        #region Helper functions
        private static void whenCreated(Object source, FileSystemEventArgs e)
        {
            //Console.WriteLine("The delegate is being called.");
            nextTcrfCreated = true;
        }
        
        /*static void startRunCRFLabel()
        {
            Process RunCRFLabel = new Process();
            RunCRFLabel.StartInfo.FileName = runCRFExe;
            RunCRFLabel.StartInfo.Arguments = RunCRFLabelCommandArgs;
            RunCRFLabel.Start();
        }

        static void startRunCRFTrain()
        {
            Process RunCRFTrain = new Process();
            RunCRFTrain.StartInfo.FileName = runCRFExe;
            RunCRFTrain.StartInfo.Arguments = RunCRFtrainCommandArgs;
            RunCRFTrain.Start();
        }

        static void startThresholdEval()
        {
            Process ThresholdEval = new Process();
            ThresholdEval.StartInfo.FileName = thresholdEvalExe;
            ThresholdEval.StartInfo.Arguments = thresholdEvalCommandArgs;
            ThresholdEval.Start();
        }
        */
        static void getLineFromInfoFile(StreamReader sr, out string line)
        {
            while ((line = sr.ReadLine()) != null)
                if (!line.StartsWith("#"))
                    break;
        }

        static void getDataFromInfoFile(StreamReader sr, ref List<string> data, int numIters)
        {
            string line;

            while ((line = sr.ReadLine()) != null)
                if (!line.StartsWith("#"))
                    break;

            data.Add(line);

            for (int count = 0; count < numIters - 1; ++count)
                data.Add(sr.ReadLine());
        }
        #endregion
    }
}
