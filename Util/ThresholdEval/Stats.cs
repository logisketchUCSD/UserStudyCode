/**
 * File:    Stats.cs
 * 
 * Author:  Sketchers 2007
 *
 * Purpose: See printHelp() in Program.cs
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ConverterXML;
using Sketch;
using Featurefy;
using statistic;

namespace ThresholdEval
{
    delegate void printData(string st, params object[] ob);

    class Stats
    {
        List<string> dataFiles;

        public Stats(List<string> data) 
        {
            dataFiles = new List<string>();

            foreach (string file in data)
                this.dataFiles.Add(file);
        }

        public void calcStats(string criterion, string logFile)
        {
            //string dummyFile = "foo.txt";
            StreamWriter sw = null;//new StreamWriter(dummyFile);
            printData print;

            if (logFile.Length != 0)
            {
                sw = new StreamWriter(logFile);
                print = new printData(sw.WriteLine);
            }
            else 
            {
                print = new printData(Console.WriteLine);
            }

            List<double> overallDataGate = new List<double>();
            List<double> overallDataWire = new List<double>();
            List<double> overallDataLabel = new List<double>();
            List<double> overallDataAll = new List<double>();
            
            foreach (string filename in dataFiles)
            {
                //Console.WriteLine("Filename: {0}", filename);
                //Console.WriteLine("Length: {0}", dataFiles.Count);

                FeatureStroke featStr;
                string strokeLabel;
                double data = 0;

                Sketch.Sketch sketchHolder = (new ReadXML(filename)).Sketch;
                switchToWireGateLabel(ref sketchHolder);
                Stroke[] sketchStrokes = sketchHolder.Strokes;

                List<double> dataGate = new List<double>();
                List<double> dataWire = new List<double>();
                List<double> dataLabel = new List<double>();
                List<double> dataAll = new List<double>();

                foreach (Sketch.Substroke subStroke in sketchHolder.Substrokes)
                {
                    featStr = new FeatureStroke(subStroke);
                    strokeLabel = subStroke.GetFirstLabel();
                    calcData(ref data, featStr, criterion);

                    //Console.WriteLine("SubStroke Label: {0}", strokeLabel);
                    //Console.WriteLine("Circular Ink Density: {0}", data);
                    //Console.WriteLine("");

                    if (strokeLabel.Equals("Wire"))
                    {
                        dataWire.Add(data);
                        overallDataWire.Add(data);
                        dataAll.Add(data);
                        overallDataAll.Add(data);
                    }
                    else if (strokeLabel.Equals("Gate"))
                    {
                        dataGate.Add(data);
                        overallDataGate.Add(data);
                        dataAll.Add(data);
                        overallDataAll.Add(data);
                    }
                    else if (strokeLabel.Equals("Label"))
                    {
                        dataLabel.Add(data);
                        overallDataLabel.Add(data);
                        dataAll.Add(data);
                        overallDataAll.Add(data);
                    }
                    else
                    {
                        print("ERROR: Unknown label of a stroke.");
                    }
                }

                print("");
                print("Filename: {0}", filename);
                double[] gateArray = dataGate.ToArray();
                statistics gateStats = new statistics(gateArray);
                print("Gates -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                    gateStats.mean(), gateStats.s(), gateStats.max(), gateStats.min());

                double[] wireArray = dataWire.ToArray();
                statistics wireStats = new statistics(wireArray);
                print("Wire  -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                    wireStats.mean(), wireStats.s(), wireStats.max(), wireStats.min());

                double[] labelArray = dataLabel.ToArray();
                statistics labelStats = new statistics(labelArray);
                print("Label -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                    labelStats.mean(), labelStats.s(), labelStats.max(), labelStats.min());

                double[] allArray = dataAll.ToArray();
                statistics allStats = new statistics(allArray);
                print("Total -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                    allStats.mean(), allStats.s(), allStats.max(), allStats.min());
                
            }

            print("");
            print("Calculations based on data from all the files");
            double[] overallGateArray = overallDataGate.ToArray();
            statistics overallGateStats = new statistics(overallGateArray);
            print("Gates -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                overallGateStats.mean(), overallGateStats.s(), overallGateStats.max(), overallGateStats.min());

            double[] overallWireArray = overallDataWire.ToArray();
            statistics overallWireStats = new statistics(overallWireArray);
            print("Wire  -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                overallWireStats.mean(), overallWireStats.s(), overallWireStats.max(), overallWireStats.min());

            double[] overallLabelArray = overallDataLabel.ToArray();
            statistics overallLabelStats = new statistics(overallLabelArray);
            print("Label -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                overallLabelStats.mean(), overallLabelStats.s(), overallLabelStats.max(), overallLabelStats.min());

            double[] overallArray = overallDataAll.ToArray();
            statistics overallStats = new statistics(overallArray);
            print("Total -> Mean: {0:###.000}, StDev: {1:###.000}, Max: {2:###.000}, Min: {3:###.000}", 
                overallStats.mean(), overallStats.s(), overallStats.max(), overallStats.min());

            
            if (logFile.Length != 0)
            {
                sw.Close();
            }
        }


        void calcData(ref double data, FeatureStroke featStr, string criterion)
        { 
            switch (criterion)
            {
                case "CircularInkDensity":
                    data = featStr.ArcLength.CircularInkDensity;
                    break;
                case "SquareInkDensity":
                    data = featStr.ArcLength.InkDensity;
                    break;
                case "MaximumCurvature":
                    data = featStr.Curvature.MaximumCurvature;
                    break;
                default:
                    Console.WriteLine("Unknown statistic.");
                    Console.WriteLine("Please add it to ThresholdEval->Stats->calcData.");
                    break;
            }
        }

        /// <summary>
        /// Makes sure that all the types in a XML file are wire, gate or label
        /// </summary>
        /// <param name="sketch">Sketch which types we change</param>
        void switchToWireGateLabel(ref Sketch.Sketch sketch)
        {
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
        }
    }
}
