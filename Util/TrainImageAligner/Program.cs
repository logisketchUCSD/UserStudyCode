using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using ImageAligner;
using Sketch;
using ConverterXML;
using Utilities;

namespace TrainImageAligner
{
    class Program
    {
        static List<string> leafLabels = new List<string>(new string[] {
            "BackLine", "FrontArc", "BackArc", "TopArc", "BottomArc",
            "Bubble", "TopLine", "BottomLine", "Triangle", 
            "GreaterThan", "TouchUp", "Junk", 
            "LabelBoxOther", "Entire_OR", "Entire_AND" });

        static List<string> shapeLabels = new List<string>(new string[] {
            "AND", "OR", "NAND", "NOR", "NOT", "NOTBUBBLE", "XOR", "LabelBox" });

        static int NUMBEST = 5;

        static double m_TotalScore = 0.0;
        static ConfusionMatrix matrix;
        static Shape shape1;

        static void Main(string[] args)
        {
            //if (args.Length == 0)
                //return;

            if (NUMBEST != 0)
            {
                string filePath = Directory.GetCurrentDirectory();
                if (filePath.Contains("\\Code\\"))
                    filePath = filePath.Substring(0, filePath.IndexOf("\\Code\\") + 1);
                TrainByNBest(filePath + "Data\\Gate Study Data\\LabeledPartsSketches", "*.xml");
                return;
            }

            List<KeyValuePair<string, Dictionary<string, object>>> data = new List<KeyValuePair<string, Dictionary<string, object>>>();

            Dictionary<string, List<string>[]> user2sketches = GetSketchesPerUser(args[0], args[1]);

            // Foreach user: train each of the recognizers

            foreach (KeyValuePair<string, List<string>[]> pair in user2sketches)
            {
                string user = pair.Key;
                int userNum;
                bool good = int.TryParse(user.Substring(0, 2), out userNum);
                if (good && userNum <= 0)
                    continue;

                User u = new User(userNum.ToString());
                PlatformUsed platform = PlatformUsed.TabletPC;
                if (user.Contains("P"))
                    platform = PlatformUsed.Wacom;

                //Console.WriteLine("User: " + user);
                List<string> sketchFiles = pair.Value[0];
                List<string> testFiles = pair.Value[1];

                ImageAlignerRecognizer recognizer = new ImageAlignerRecognizer(u, platform);

                foreach (string sketchFile in sketchFiles)
                {
                    Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                    sketch = General.ReOrderParentShapes(sketch);

                    foreach (Shape shape in sketch.Shapes)
                        if (General.IsGate(shape) &&
                               shape.Substrokes[0].Labels[0] == shape.Label &&
                               shape.Substrokes.Length < NormativeSize(shape.Label) + 1)
                        {
                            recognizer.Add(shape);
                            if (shape.Label == "NOT")
                            {
                                List<Substroke> strokes = new List<Substroke>();
                                foreach (Substroke s in shape.Substrokes)
                                {
                                    if (s.Labels[s.Labels.Length - 1] == "Bubble")
                                    {
                                        strokes.Add(s);
                                        break;
                                    }
                                }
                                XmlStructs.XmlShapeAttrs attr = new XmlStructs.XmlShapeAttrs();
                                attr.Type = "NOTBUBBLE";
                                Shape nb = new Shape(strokes, attr);
                                recognizer.Add(nb);
                            }
                        }
                }

                recognizer.Save("ImageAlignerRecognizer" + user + ".iar");

                int numRight = 0;
                int numWrong = 0;
                Dictionary<Shape, ImageTemplateResult> results = new Dictionary<Shape, ImageTemplateResult>();

                foreach (string sketchFile in testFiles)
                {
                    Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                    sketch = General.ReOrderParentShapes(sketch);

                    foreach (Shape shape in sketch.Shapes)
                    {
                        if (General.IsGate(shape) && shape.Substrokes[0].Labels[0] == shape.Label && shape.Label != "LabelBox")
                        {
                            ImageTemplateResult result = recognizer.Recognize(shape);
                            if (result != null)
                            {
                                results.Add(shape, result);
                                if (result.Name == shape.Label)
                                    numRight++;
                                else
                                    numWrong++;
                            }
                        }
                    }
                }
                int total = numRight + numWrong;

                Console.WriteLine("User: " + user + " Correct: " + numRight.ToString() + "/" + total.ToString());
                if (args.Length > 2 && args[2] == "-v")
                {
                    foreach (KeyValuePair<Shape, ImageTemplateResult> kv in results)
                        WriteResult(kv.Key, kv.Value);

                }
            }
        }



        private static void TrainByNBest(string dir, string search)
        {
            string cMatrixFile = "Code\\Recognition\\ImageAligner\\allConfusion.txt";
            matrix = GetConfusionMatrix(cMatrixFile);
            ImageAligner.ImageAlignerRecognizer recognizer = new ImageAlignerRecognizer();
            string[] files = System.IO.Directory.GetFiles(dir, search);
            Dictionary<string, Dictionary<int, List<Sketch.Shape>>> shapes = new Dictionary<string, Dictionary<int, List<Shape>>>();
            
            Console.WriteLine("Reading Files: starting @ " + DateTime.Now.ToString());
            foreach (string file in files)
            {
                bool copyEqn = false;
                if (file.Contains("COPY") || file.Contains("EQ"))
                    copyEqn = true;

                Sketch.Sketch sketch = new ReadXML(file).Sketch;
                sketch = General.ReOrderParentShapes(sketch);
                sketch = General.LinkShapes(sketch);

                foreach (Shape shape in sketch.Shapes)
                {
                    if (!General.IsParent(shape))
                        continue;
                    if (!General.IsGate(shape))
                        continue;
                    if (copyEqn && shape.Label != "LabelBox")
                        continue;

                    bool junkFound = false;
                    foreach (Substroke stroke in shape.SubstrokesL)
                        if (stroke.Labels.Length > 0 
                            && (stroke.Labels[stroke.Labels.Length - 1] == "Junk" || stroke.Labels[stroke.Labels.Length - 1] == "TouchUp"))
                            junkFound = true;

                    if (!junkFound)
                    {
                        if (!shapes.ContainsKey(shape.Label))
                            shapes.Add(shape.Label, new Dictionary<int, List<Shape>>());
                        if (!shapes[shape.Label].ContainsKey(shape.SubstrokesL.Count))
                            shapes[shape.Label].Add(shape.SubstrokesL.Count, new List<Shape>());
                        if (shapes[shape.Label][shape.SubstrokesL.Count].Count < 10000)
                            shapes[shape.Label][shape.SubstrokesL.Count].Add(shape);
                    }
                }
            }

            foreach (string label in shapes.Keys)
            {
                foreach (KeyValuePair<int, List<Shape>> kvp in shapes[label])
                {
                    List<Shape> shapeClass = kvp.Value;
                    Console.WriteLine(label + " " + kvp.Key + "-Stroke: starting @ " + DateTime.Now.ToString());
                    int num = 0;
                    if (shapeClass.Count < 5)
                        continue;
                    else if (shapeClass.Count < 20)
                        num = 1;
                    else if (shapeClass.Count < 50)
                        num = 2;
                    else
                        num = 3;

                    Dictionary<Shape, double> scores = new Dictionary<Shape, double>();
                    int n = 0;
                    foreach (Shape currentShape in shapeClass)
                    {
                        shape1 = currentShape;
                        if (n % 50 == 1)
                            Console.WriteLine("\t" + n + " @ " + DateTime.Now.ToString());
                        n++;

                        m_TotalScore = 0.0;
                        GetTotalScore(shape1, shapeClass);
                        scores.Add(shape1, m_TotalScore);
                    }

                    for (int i = 0; i < num; i++)
                    {
                        double bestscore = double.NegativeInfinity;
                        Shape bestShape = null;
                        foreach (KeyValuePair<Shape, double> score in scores)
                            if (score.Value > bestscore)
                            {
                                bestscore = score.Value;
                                bestShape = score.Key;
                            }

                        if (bestShape != null)
                        {
                            recognizer.Add(bestShape);
                            scores.Remove(bestShape);
                        }
                    }

                }
            }
            recognizer.Save("ImageAlignerRecognizer" + "NBEST_eric.iar");
        }

        private static void GetTotalScore(Shape shape1, List<Shape> shapes)
        {
            int numThreads = System.Environment.ProcessorCount;
            List<Thread> threads = new List<Thread>();
            List<List<Shape>> TestingShapes = new List<List<Shape>>(numThreads);

            for (int i = 0; i < numThreads; i++)
            {
                int start = shapes.Count / numThreads * i;
                int count = shapes.Count / numThreads;
                Shape[] shapesArray = new Shape[count];
                shapes.CopyTo(start, shapesArray, 0, count);
                TestingShapes.Add(new List<Shape>(shapesArray));
            }

            if (shapes.Count % numThreads != 0)
            {
                List<Shape> leftOvers = new List<Shape>();
                foreach (Shape s in shapes)
                {
                    bool found = false;
                    foreach (List<Shape> list in TestingShapes)
                    {
                        if (list.Contains(s))
                            found = true;
                    }

                    if (!found)
                        leftOvers.Add(s);
                }

                for (int i = 0; i < leftOvers.Count; i++)
                {
                    int n = i;
                    while (n >= numThreads)
                        n -= numThreads;

                    TestingShapes[n].Add(leftOvers[i]);
                }
            }

            for (int i = 0; i < numThreads; i++)
            {
                Thread newThread = new Thread(DoWork);
                // Want to change the process priority, not the thread
                //newThread.Priority = ThreadPriority.BelowNormal;
                threads.Add(newThread);
                threads[i].Start(TestingShapes[i]);
            }

            for (int i = 0; i < numThreads; i++)
                threads[i].Join();
        }

        private static void DoWork(object data)
        {
            List<Shape> shapes = (List<Shape>)data;

            double totalscore = 0.0;
            ImageTemplate template1 = new ImageTemplate(shape1, GetShapeParts(shape1), new SymbolInfo(), matrix);
            foreach (Shape shape2 in shapes)
                if (shape1 != shape2)
                {
                    ImageTemplate template2 = new ImageTemplate(shape2, GetShapeParts(shape2), new SymbolInfo(), matrix);
                    totalscore += template1.Recognize(template2).Score;
                }


            m_TotalScore += totalscore;
        }

        private static Dictionary<Substroke, string> GetShapeParts(Shape shape1)
        {
            Dictionary<Substroke, string> parts = new Dictionary<Substroke, string>();
            foreach (Substroke s in shape1.Substrokes)
                parts.Add(s, s.Labels[s.Labels.Length - 1]);

            return parts;
        }

        private static ConfusionMatrix GetConfusionMatrix(string filename)
        {
            string filePath = System.IO.Directory.GetCurrentDirectory();
            if (filePath.Contains("\\Code\\"))
                filePath = filePath.Substring(0, filePath.IndexOf("\\Code\\") + 1);
            else if (filePath.Contains("\\Sketch\\"))
                filePath = filePath.Substring(0, filePath.IndexOf("\\Sketch\\") + 8);
            else if (filePath.Contains("\\Trunk\\"))
                filePath = filePath.Substring(0, filePath.IndexOf("\\Trunk\\") + 7);
            filePath += filename;
            ConfusionMatrix matrix = new ConfusionMatrix(General.leafLabels);
            matrix.LoadFromFile(filePath);

            return matrix;
        }

        private static void WriteResult(Shape shape, ImageTemplateResult result)
        {
            Console.WriteLine("\tShape:   \t" + shape.Label);
            Console.WriteLine("\tTemplate:\t" + result.Name + "\t" + result.Score.ToString() + "\t" + result.Confidence.ToString());
            foreach (ImageMatchError error in result.Errors)
                Console.WriteLine("\t\tErrors:\t" + error.Type.ToString() + "\t" + error.Detail.ToString() + "\t" + error.Severity.ToString());
            Console.WriteLine("\t\tMap (Unknown part (believed from SSR) --> Template part):");
            foreach (KeyValuePair<string, string> pair in result.Map.MapWithPartNames)
                Console.WriteLine("\t\t\t" + pair.Key + "\t" + pair.Value);
            Console.WriteLine();
        }

        private static int NormativeSize(string gate)
        {
            switch (gate)
            {
                case "AND":
                    return 2;
                case "OR":
                    return 3;
                case "NAND":
                    return 3;
                case "NOR":
                    return 4;
                case "NOT":
                    return 4;
                case "NOTBUBBLE":
                    return 1;
                case "XOR":
                    return 4;
                case "LabelBox":
                    return 5;
                case "BUBBLE":
                    return 1;
                case "XNOR":
                    return 5;
                default:
                    return 0;
            }
        }

        private static Dictionary<string, List<string>[]> GetSketchesPerUser(string dir, string search)
        {
            Dictionary<string, List<string>[]> sketches = new Dictionary<string, List<string>[]>();

            List<string> files = new List<string>(System.IO.Directory.GetFiles(dir, search));

            foreach (string f in files)
            {
                string fShort = Path.GetFileName(f);
                string user = fShort.Substring(0, 2);
                if (fShort.Contains("_T"))
                    user += "T";
                else if (fShort.Contains("_P"))
                    user += "P";

                if (!sketches.ContainsKey(user))
                {
                    sketches.Add(user, new List<string>[2]);
                    sketches[user][1] = new List<string>();
                    sketches[user][0] = new List<string>();
                }

                if (fShort.Contains("EQ") || fShort.Contains("COPY"))
                    sketches[user][1].Add(f);
                else
                    sketches[user][0].Add(f);
            }

            return sketches;
        }

    }
}
