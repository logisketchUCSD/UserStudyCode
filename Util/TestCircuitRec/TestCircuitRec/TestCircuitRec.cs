using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConverterXML;
using Sketch;
using ZedGraph;
using System.Windows.Forms;
using CircuitRec;
using Microsoft.Ink;

namespace TestCircuitRec
{
    class TestCircuitRec
    {
        /// <summary>
        /// Tests the CircuitRec program.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Keep running the program until finished testing
            while (true)
            {
                #region Input Select
                
                ReadXML xml = null;
                String input = " ";
                bool fileloaderror = true;

                // Choose which file to load (has to be a valid file)
                while (fileloaderror)
                {
                    Console.Write("Enter file number or name: ");
                    input = Console.ReadLine();
                    Console.WriteLine();
                    try
                    {
                        // use Absolute path because otherwise current directory changes
                        xml = new ReadXML(@"C:\\Documents and Settings\\Guest\\My Documents\\Sketch\\Code\\Util\\TestCircuitRec\\TestCircuitRec\\TestingData\\" + input + ".xml");
                        fileloaderror = false;
                        
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        fileloaderror = true;
                    }
                }

                #endregion

                #region Call CircuitRec

                // Load domain file
                Domain domain = new Domain("digital_domain.txt");

                // Load WordList
                List<string> stringList = TextRecognition.TextRecognition.loadLabelStringList("../../../../TextRecognition/WordList.txt");
                WordList wordList = TextRecognition.TextRecognition.loadLabelWordList(stringList);

                // Run CircuitRec
                CircuitRec.CircuitRec CR = new CircuitRec.CircuitRec(domain, wordList);
                CR.Run(xml.Sketch);
                Dictionary<Guid?, object> map = CR.Substroke2CircuitMap;

                // Look at errors
                Console.WriteLine("Number of errors: " + CR.Errors.Count);
                foreach (ParseError e in CR.Errors)
                {
                    Console.WriteLine(e.Message);
                }

                #endregion

                #region Graphical Output

                // Gets the data ready for ZedGraph (do not worry about making this cleaner or faster since it is just to test CircuitRec)

                // Temporary data variables to output to Graph
                double tempx;
                double tempy;

                // Using ArrayLists to seperate invidual wires and symbols.
                ArrayList Wire_Graph = new ArrayList();
                ArrayList Wire_Bound = new ArrayList();
                ArrayList Wire_Bound2 = new ArrayList();
                ArrayList Symbol_Graph = new ArrayList();
                ArrayList Symbol_Bound = new ArrayList();
                ArrayList Symbol_Bound2 = new ArrayList();

                // Temporarily stores the values in a PointPairList
                PointPairList temp;

                foreach (Wire output in CR.Wires)
                {
                    temp = new PointPairList();

                    foreach (Point point in output.Points)
                    {
                        tempx = Convert.ToDouble(point.X);
                        tempy = Convert.ToDouble(point.Y);
                        String tag = output.ID + ": (" + point.X.ToString() + "," + point.Y.ToString() + ")";
                        temp.Add(tempx, -tempy, tag);
                    }

                    // Adds the wire data into the arraylist and then clears the wire PointPairList
                    Wire_Graph.Add(temp);
                }

                foreach (BaseSymbol output in CR.Symbols)
                {
                    // Used to keep track of the index for symbol PointPairList
                    temp = new PointPairList();

                    // Used to output to form
                    foreach (Point point in output.Points)
                    {
                        tempx = Convert.ToDouble(point.X);
                        tempy = Convert.ToDouble(point.Y);
                        temp.Add(tempx, -tempy);
                    }

                    Symbol_Graph.Add(temp);
                    temp = new PointPairList();
                }

                ArrayList EndPoints = new ArrayList();
                ArrayList EndPoints_1 = new ArrayList();
                ArrayList EndPoints_2 = new ArrayList();
                ArrayList EndPoints_3 = new ArrayList();
                temp = new PointPairList();

                foreach (Wire new_wire in CR.Wires)
                {
                    for (int index = 0; index < new_wire.EndPt.Length; index++)
                    {
                        temp.Add((double)new_wire.EndPt[index].X, -(double)new_wire.EndPt[index].Y);
                        EndPoints.Add(temp);
                        temp = new PointPairList();
                    }
                }
                // Get Label Points
                ArrayList labelpointsAL = new ArrayList();
                for (int count = 0; count < CR.LabelShapes.Count; count++)
                {
                    Shape templabels = (Shape)CR.LabelShapes[count];
                    PointPairList labelpoints = new PointPairList();
                    foreach (Sketch.Point point in templabels.Points)
                    {
                        labelpoints.Add((double)point.X, -(double)point.Y);
                    }
                    labelpointsAL.Add(labelpoints);
                }


                // Get endpoint lines (only adds endpoints)
                ArrayList eplinesAL = new ArrayList();

                foreach (Wire wire in CR.Wires)
                {
                    foreach (Point ep in wire.EndPt)
                    {
                        PointPairList eplines = new PointPairList();
                        //double x1 = (double)ep.X + 100;
                        //double x2 = (double)ep.X - 100;
                        //double y1 = (x1 + 100) * ep.getSlope() + ep.getOffset();
                        //double y2 = (x2 + 100) * ep.getSlope() + ep.getOffset();
                        eplines.Add((double)ep.X, -(double)ep.Y);
                        //eplines.Add(x1, -y1);
                        //eplines.Add(x2, -y2);
                        //eplines.Add((double)ep.X + 100, -(((double)ep.X + 100) * ep.getSlope() + ep.getOffset()));
                        //eplines.Add((double)ep.X - 100, -(((double)ep.X - 100) * ep.getSlope() + ep.getOffset()));
                        eplinesAL.Add(eplines);
                        //Console.WriteLine("m={0}, b={1}", ep.getSlope(), ep.getOffset());
                    }

                }
                Form1 instance5 = new Form1(Wire_Graph, Symbol_Graph, labelpointsAL, EndPoints, eplinesAL, EndPoints_2, EndPoints_3, CR.Wires, eplinesAL);
                Application.Run(instance5);

                #endregion

                Console.Write("Continue (y or n): ");
                if (Console.ReadLine().Equals("n"))
                    break;
                
            }

        }
    }
}
