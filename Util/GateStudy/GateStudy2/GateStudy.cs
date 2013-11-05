using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConverterXML;
using Sketch;

using UCRDataAnalysis;

namespace GateStudy2
{
    static class GateStudy
    {
        /// <summary>
        /// The main entry point for the application.
        /// Writes information about the sketches from the user study to GateStats
        /// or writes information about point timing to GateTimes.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] files = { "AND", "OR", "XOR", "NAND", "NOR", "NOT", "EQ1", "EQ2", "COPY1", "COPY2" };

            string sHeader = "SketchID,UserID,ShapeID,StrokeID,ShapeName,ShapeTime,"
                +"StrokeTime,IsConsecutive,AveragePressure,Width,Height,AvgCurvature,"
                +"MaxCurvature,MinCurvature,NumPoints,MinDist,StrokesPerShape,StartTime,EndTime";
            string tHeader = "SketchID, StrokeID, X, Y, Time";
            GateStats stats = new GateStats(files, sHeader);
            StreamWriter sSw;
            StreamWriter tSw;

            string[] hmc = Directory.GetFiles("c:\\sketch\\Data\\Gate Study Data\\LabeledSketches\\TabletPC\\HMC", "*.xml", SearchOption.AllDirectories);
            string[] ucr = Directory.GetFiles("c:\\sketch\\Data\\Gate Study Data\\LabeledSketches\\TabletPC\\UCR", "*.xml", SearchOption.AllDirectories);

            List<MySketch> sketches = new List<MySketch>();

            // read HMC
            for (int i = 1; i <= 12; i++)
            {
                foreach (string file in hmc)
                {
                    // only care about and/or/not
                    //if (file.Contains("NAND") || file.Contains("NOR") || file.Contains("XOR")) continue;
                    if (file.Contains(string.Format("\\{0}_", i))) // iso,copy
                    {
                        string act = file.Contains("COPY") ? "copy" : "iso";
                        sketches.Add(new MySketch(new ReadXML(file).Sketch, i, act));
                    }
                    else if (file.Contains(string.Format("_{0}_", i))) // synth
                    {
                        sketches.Add(new MySketch(new ReadXML(file).Sketch, i, "synth"));
                    }
                }
            }
            // read UCR
            for (int i = 1; i <= 12; i++)
            {
                foreach (string file in ucr)
                {
                    // only care about and/or/not
                    //if (file.Contains("NAND") || file.Contains("NOR") || file.Contains("XOR")) continue;
                    if (file.Contains(string.Format("\\{0}_", i))) // iso,copy
                    {
                        string act = file.Contains("COPY") ? "copy" : "iso";
                        sketches.Add(new MySketch(new ReadXML(file).Sketch, i + 12, act));
                    }
                    else if (file.Contains(string.Format("_{0}_", i))) // synth
                    {
                        sketches.Add(new MySketch(new ReadXML(file).Sketch, i + 12, "synth"));
                    }
                }
            }

            sketches.Sort( delegate (MySketch a, MySketch b) { return a.Author.CompareTo(b.Author); } );


            int curruser = 0;
            string curuserstr = (curruser > 12) ? "HMC" + curruser : "UCR" + (curruser - 12);

            sSw = new StreamWriter("GateStats" + curuserstr + ".csv");
            tSw = new StreamWriter("GateTimes" + curuserstr + ".csv");
            Console.WriteLine("Writing user " + curuserstr + "...");
            sSw.WriteLine(sHeader);
            tSw.WriteLine(tHeader);
            foreach (MySketch mysketch in sketches)
            {
                if (mysketch.Author != curruser)
                {
                    sSw.Close();
                    tSw.Close();
                    curruser = mysketch.Author;
                    curuserstr = (curruser > 12) ? "HMC" + curruser : "UCR" + (curruser - 12);

                    sSw = new StreamWriter("GateStats" + curuserstr + ".csv");
                    tSw = new StreamWriter("GateTimes" + curuserstr + ".csv");
                    Console.WriteLine("Writing user " + curuserstr + "...");
                }
                Sketch.Sketch sketch = mysketch.Sketch;
                for(int j = 0; j < sketch.Substrokes.Length; j++)
                {
                    Substroke sub = sketch.Substrokes[j];

                    for (int i = 0; i < sub.Points.Length; i++)
                    {
                        string time = sketch.XmlAttrs.Id + "," + sub.XmlAttrs.Id + ","
                            + sub.Points[i].X + ","
                            + sub.Points[i].Y + "," + sub.Points[i].Time;
                        tSw.WriteLine(time);
                    }
                    
                    for (int i = 0; i < sub.ParentShapes.Count; i++)
                    {
                        string line = sketch.XmlAttrs.Id + "," + curuserstr + ","
                            + sub.ParentShapes[i].XmlAttrs.Id + ","
                            + sub.XmlAttrs.Id + "," + sub.ParentShapes[i].XmlAttrs.Type + ","
                            + stats.findShapeTime(sub.ParentShapes[i]) + "," + stats.findSubStrokeTime(sub) + ","
                            + stats.isConsecutive(sketch, sub.ParentShapes[i]) + ","
                            + stats.findAvgPressure(sub) + "," + stats.getWidth(sub.ParentShapes[i]) + ","
                            + stats.getHeight(sub.ParentShapes[i]) + "," + stats.getAvgCurvature(sub) + ","
                            + stats.getMaxCurvature(sub) + "," + stats.getMinCurvature(sub) + "," 
                            + stats.getNumPoints(sub) + ",";
                        if (sub.ParentShapes[i].XmlAttrs.Type == "Wire")
                            line =line+ stats.minDistWireToGates(stats.findGatesInSketch(sketch), sub.ParentShapes[i]);
                        else if (sub.ParentShapes[i].XmlAttrs.Type == "Label")
                            line += stats.minDistLabelToWires(stats.findWiresInSketch(sketch), sub.ParentShapes[i]);
                        else
                            line += stats.minDistGateToWires(stats.findWiresInSketch(sketch), sub.ParentShapes[i]);
                        line += "," + stats.numStrokesPerShape(sub.ParentShapes[i]) + ","
                            + stats.getStartTime(sub) + "," + stats.getEndTime(sub);
                        sSw.WriteLine(line);
                    }
                }
            }

            sSw.Close();
            tSw.Close();


        }
    }
}