/**
 * File:    Accuracy.cs
 * 
 * Author:  Sketchers 2007
 *
 * Purpose: See printHelp() in Program.cs
 */
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using Fragmenter;
using ConverterXML;
using statistic;

namespace ThresholdEval
{
    //Already defined in Stats.cs
    //delegate void printData(string st, params object[] ob);

    class Accuracy
    {
        #region Internals
        List<string> dataCheck;
        List<string> dataLabeled;
        #endregion

        public enum Files { filesCheck, filesLabeled };

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataFilesCheck"></param>
        /// <param name="dataFilesLabeled"></param>
        public Accuracy(List<string> dataFilesCheck, List<string> dataFilesLabeled)
        {
            this.dataCheck = new List<string>();
            this.dataLabeled = new List<string>();

            this.AddFiles(dataFilesCheck, Files.filesCheck);
            this.AddFiles(dataFilesLabeled, Files.filesLabeled);
        }

        /// <summary>
        /// Adds files to dataCheck and dataLabeled.
        /// </summary>
        /// <param name="dataFiles">Files to be added.</param>
        /// <param name="typeFiles">Which List to add files to.</param>
        public void AddFiles(List<string> dataFiles, Files typeFiles)
        {
            foreach (string file in dataFiles) 
            {
                if (typeFiles == Files.filesCheck)
                    dataCheck.Add(file);

                if (typeFiles == Files.filesLabeled)
                    dataLabeled.Add(file);
            }
        }
        /// <summary>
        /// This function calculates the accuracy of the labeling.
        /// Note: For the sake of a good running time we assume that the dataCheck and 
        /// dataFiles contain corresponding files i.e. dataCheck[i] corresponds to 
        /// dataFiles[i]. 
        /// </summary>
        public void calcAccuracy(string logFile)
        {
            //string dummyFile = "foo.txt";
            StreamWriter swr = null;//new StreamWriter(dummyFile);
            printData printAcc;

            if (logFile.Length != 0)
            {
                swr = new StreamWriter(logFile);
                printAcc = new printData(swr.WriteLine);
            }
            else
            {
                printAcc = new printData(Console.WriteLine);
            }


            /* at index 0 -> data for wires
             * at index 1 -> data for gates
             * at index 2 -> data for labels
             */ 
            double[] numCorrectOverall = new double[3];
            double[] numTotalOverall = new double[3];
            double totalLabels = 0.0;
            double mislabeledAsOther = 0.0;
            double mislabeledAsWire = 0.0;
            double mislabeledAsGate = 0.0;
            double correctLabels = 0.0;
            double totalSubstrokes = 0.0;
            double missedSubstrokes = 0.0;
            double unknownLabels = 0.0;

            List<float?> probCorWires = new List<float?>();
            List<float?> probWrongWires = new List<float?>();

            List<float?> probCorGates = new List<float?>();
            List<float?> probWrongGates = new List<float?>();

            List<float?> probCorLabels = new List<float?>();
            List<float?> probWrongLabels = new List<float?>(); 

            //Check whether we are comparing the same substrokes.
            //Check for the sizes.
            for (int index = 0; index < dataLabeled.Count; ++index)
            {
                string fileLabeled = (string)dataLabeled[index];
                string fileCheck = (string)dataCheck[index];

                //printAcc("fileLabeled: {0}", fileLabeled);
                //printAcc("fileCheck: {0}", fileCheck);

                Sketch.Sketch sketchLabeled = (new ReadXML(fileLabeled)).Sketch;
                Sketch.Sketch sketchCheck = (new ReadXML(fileCheck)).Sketch;

                Fragment.fragmentSketch(sketchLabeled);
                Fragment.fragmentSketch(sketchCheck);

                /* at index 0 -> data for wires
                 * at index 1 -> data for gates
                 * at index 2 -> data for labels
                 */ 
                double[] numCorrect = new double[3];
                double[] numTotal = new double[3];

                totalSubstrokes += sketchLabeled.Substrokes.Length;
                //Check whether we are comparing the same substrokes.
                //Check for the sizes.
                for (int i = 0; i < sketchLabeled.Substrokes.Length; ++i)
                {
                    Sketch.Substroke subStr = sketchLabeled.Substrokes[i];
                    string labelCorrect = subStr.GetFirstLabel();
                    //
                    //if (labelCorrect.Equals("Wire") || labelCorrect.Equals("Label"))
                    //    labelCorrect = "Nongate";
                    string labelCorrectId = subStr.XmlAttrs.Id.ToString();

                    float xCorrect = subStr.XmlAttrs.X.Value;
                    float yCorrect = subStr.XmlAttrs.Y.Value;

                    //string labelCorrect = subStr.ParentStroke.XmlAttrs.Type.ToString();
                    //string labelCorrectId = subStr.ParentStroke.XmlAttrs.Id.ToString();

                    if (labelCorrect.Length == 0 || labelCorrect.Equals("BUBBLE"))
                    {
                        printAcc("*** Skipping the following id: {0}***", labelCorrectId);
                        continue;
                    }

                    
                    string labelCheck = "";
                    string labelCheckId = "";

                    float xCheck = -1;
                    float yCheck = -1;

                    float? prob = 0;

                    foreach (Sketch.Substroke sub in sketchCheck.Substrokes)
                    {
                        
                        string tmp = sub.XmlAttrs.Id.ToString();
                        float xTmp = sub.XmlAttrs.X.Value;
                        float yTmp = sub.XmlAttrs.Y.Value;

                        //if (tmp.Equals(labelCorrectId))
                        if (xTmp == xCorrect && yTmp == yCorrect)
                        {
                            labelCheck = sub.GetFirstLabel();
                            labelCheckId = sub.XmlAttrs.Id.ToString();

                            xCheck = sub.XmlAttrs.X.Value;
                            yCheck = sub.XmlAttrs.Y.Value;

                            //prob = sub.ParentShapes[0].XmlAttrs.Probability.Value;
                            break;
                        }
                        
                    }

                    if (xCheck == -1 || yCheck == -1)
                    {
                        printAcc("*** A stroke is not found in sketchCheck ***");
                        continue;
                    }

                    /*
                    if (labelCheck.Length == 0)
                    {

                        printAcc("*** LabelCheck has length 0 => could not find a sustroke in sketchCheck ***");
                        printAcc("Correct label: {0} labelCorrectId: {1}", labelCorrect, labelCorrectId);
                        ++missedSubstrokes;
                        continue;
                    }
                    */
                    //printAcc("");
                    //printAcc("LabelCorrectId is: {0}", labelCorrectId);
                    //printAcc("LabelCheckId is  : {0}", labelCheckId);
                    //printAcc("Type of label is : {0}", labelCheck);
                    //printAcc("Probability for this label is: {0}", prob);
                    
                    switch (labelCorrect)
                    { 
                        case "Wire":
                            ++totalLabels;
                            ++(numTotal[0]);
                            ++(numTotalOverall[0]);
                            if (labelCheck.Equals("Wire"))
                            {
                                ++correctLabels;
                                ++(numCorrect[0]);
                                ++(numCorrectOverall[0]);

                                probCorWires.Add(prob);
                            }
                            else
                            {
                                probWrongWires.Add(prob);
                            }
                       
                            break;
                        
                        case "Gate":
                            ++totalLabels;
                            ++(numTotal[1]);
                            ++(numTotalOverall[1]);
                            if (labelCheck.Equals("Gate"))
                            {
                                ++correctLabels;
                                ++(numCorrect[1]);
                                ++(numCorrectOverall[1]);

                                probCorGates.Add(prob);
                            }
                            else
                            {
                                probWrongGates.Add(prob);
                            }
                            break;

                        case "Label":
                            ++totalLabels;
                            ++(numTotal[2]);
                            ++(numTotalOverall[2]);

                            if (labelCheck.Equals("Wire"))
                            {
                                ++mislabeledAsWire;
                                ++mislabeledAsOther;
                            }
                            if (labelCheck.Equals("Gate"))
                            {
                                ++mislabeledAsGate;
                                ++mislabeledAsOther;
                            }

                            if (labelCheck.Equals("Label"))
                            {
                                ++correctLabels;
                                ++(numCorrect[2]);
                                ++(numCorrectOverall[2]);

                                probCorLabels.Add(prob);
                            }
                            else
                            {
                                probWrongLabels.Add(prob);
                            }
                            break;
                        
                        /*//---Start new part.
                        case "Gate":
                            ++(numTotal[0]);
                            ++(numTotalOverall[0]);
                            if (labelCheck.Equals("Gate"))
                            {
                                ++(numCorrect[0]);
                                ++(numCorrectOverall[0]);
                            }
                            break;

                        case "Nongate":
                            ++(numTotal[1]);
                            ++(numTotalOverall[1]);
                            if (labelCheck.Equals("Nongate"))
                            {
                                ++(numCorrect[1]);
                                ++(numCorrectOverall[1]);
                            }
                            break;
                       */ //---End

                        default:
                            printAcc("*** UNKNOWN LABEL: Correct label: {0}, Examined label: {1}", labelCorrect, labelCheck);
                            printAcc("***                Correct id: {0}", labelCorrectId);
                            ++unknownLabels;
                            break;
                    }
                }
                
                printAcc("");
                printAcc("Comparing files {0} and {1}.", fileCheck, fileLabeled);
                printAcc("Percentage of correctly labeled Wires:  {0:##.000%}", 
                    numCorrect[0] / numTotal[0]);
                printAcc("Percentage of correctly labeled Gates:  {0:##.000%}",
                    numCorrect[1] / numTotal[1]);
                printAcc("Percentage of correctly labeled Labels: {0:##.000%}",
                    numCorrect[2] / numTotal[2]);
                
                /*//---Start new part.
                printAcc("");
                printAcc("Comparing files {0} and {1}.", fileCheck, fileLabeled);
                printAcc("Percentage of correctly labeled Gate:  {0:##.000%}",
                    numCorrect[0] / numTotal[0]);
                printAcc("Percentage of correctly labeled Nongate:  {0:##.000%}",
                    numCorrect[1] / numTotal[1]);
                //---End of new part.*/
            }
            
            printAcc("");
            printAcc("Results based on whole set of data:");
            printAcc("Labels missed: {0} out of {1} => {2:##.000%}",
                missedSubstrokes, totalSubstrokes, missedSubstrokes / totalSubstrokes);
            printAcc("Unlabeled strokes: {0} out of {1} => {2:##.000%}",
                unknownLabels, totalLabels, unknownLabels/totalLabels);
            printAcc("Overall accuracy: {0:##.000%}", 
                correctLabels / totalLabels);
            printAcc("Percentage of correctly labeled Wires: {0:##.000%}",
                numCorrectOverall[0] / numTotalOverall[0]);
            printAcc("Percentage of correctly labeled Gates: {0:##.000%}",
                numCorrectOverall[1] / numTotalOverall[1]);
            printAcc("Percentage of correctly labeled Labels: {0:##.000%}",
                numCorrectOverall[2] / numTotalOverall[2]);
            printAcc("Percentage of labels mislabeled as Wires: {0:##.000%}",
                mislabeledAsWire/mislabeledAsOther);
            printAcc("Percentage of labels mislabeled as Gates: {0:##.000%}",
                mislabeledAsGate/mislabeledAsOther);
            
            /*//---Start new part.
            printAcc("");
            printAcc("Results based on whole set of data:");
            printAcc("Percentage of correctly labeled Gates: {0:##.000%}",
                numCorrectOverall[0] / numTotalOverall[0]);
            printAcc("Percentage of correctly labeled Nongates: {0:##.000%}",
                numCorrectOverall[1] / numTotalOverall[1]);
            //---End new part.*/

            if (logFile.Length != 0)
            {
                swr.Close();
            }
            /*
            swr1.Close(); swr2.Close(); swr3.Close();
            swr4.Close(); swr5.Close(); swr6.Close();
            */
        } 

        static double[] fixArray(float?[] oldArray)
        { 
            double[] tmp = new double[oldArray.Length];
            for (int i = 0; i < oldArray.Length; ++i)
            {
                tmp[i] = System.Convert.ToDouble(oldArray[i]);
            }

            return tmp;
        }

    }
}
