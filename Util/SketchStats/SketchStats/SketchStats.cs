using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ConverterXML;


namespace SketchStats
{
    /// <summary>
    /// author: Sara
    /// Loads a sketch and print out statistics about it including:
    /// - number of strokes
    /// - average length of stroke
    /// - number of shapes
    /// - number of strokes per shape
    /// - list of the types of shapes in sketch and how many of each
    /// </summary>
    
    class StetchStats
    {
        /// <summary>
        /// returns number of strokes in a sketch
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static int numStrokes(Sketch.Sketch sk)
        {
            int numStrokes = sk.Substrokes.Length;
            return numStrokes;
        }

        /// <summary>
        /// returns average length of the strokes in the sketch
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static int avgStrokeLength(Sketch.Sketch sk)
        {
            double sumLengths = 0;

            for ( int i = 0; i < numStrokes(sk); i++ )
            {
                Featurefy.ArcLength al = new Featurefy.ArcLength(sk.Strokes[i].Points);
                sumLengths += al.TotalLength;
            }

            double avgLength = sumLengths / numStrokes(sk);
            return (int)avgLength;
        }

        /// <summary>
        /// returns number of shapes in a sketch
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static int numShapes(Sketch.Sketch sk)
        {
            int numShapes = sk.Shapes.Length;
            return numShapes;
        }

        /// <summary>
        /// returns the number of strokes per shape
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static double numStrokesPerShape(Sketch.Sketch sk)
        {
            double strokesPerShape = ((double)numStrokes(sk)) / numShapes(sk);
            return strokesPerShape;
        }

        /// <summary>
        /// returns an ArrayList of the types of shapes in the sketch (no repeats)
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static ArrayList shapeTypes(Sketch.Sketch sk)
        {
            ArrayList shapeTypes = new ArrayList();

            for (int i = 0; i < numShapes(sk); i++)
            {
                if (shapeTypes.Contains(sk.Shapes[i].XmlAttrs.Type) == false)
                {
                    shapeTypes.Add(sk.Shapes[i].XmlAttrs.Type);
                }
            }

            return shapeTypes;
        }

        /// <summary>
        /// returns an int array of the number of times each shape occurs
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static int[] countTypes(Sketch.Sketch sk)
        {
            int[] countTypes = new int[shapeTypes(sk).Count];

            for (int i = 0; i < countTypes.Length; i++)
            {
                countTypes[i] = 0;
            }

            for (int i = 0; i < numShapes(sk); i++)
            {
                int index = shapeTypes(sk).IndexOf(sk.Shapes[i].XmlAttrs.Type);
                countTypes[index]++;
            }

            return countTypes;
        }

        /// <summary>
        /// inputs: ArrayList and int array of the same size
        /// output: prints the elements of the arrays as a list of pairs
        /// meant for taking in a list of types and their respective counts, and printing them
        /// </summary>
        /// <param name="al"></param>
        /// <returns></returns>
        static string myPrintArrayList(ArrayList L, int[] M)
        {
            string text = "\n";
            for (int i = 0; i < L.Count; i++)
            {
                text += "     " + L[i] + ": " + M[i] + "\n";
            }
            
            return text;
        }

        static string printList(int[] L)
        {
            string text = "";

            for (int i = 0; i < L.Length; i++)
            {
                text += L[i] + " ";
            }
            return text;
        }

        /// <summary>
        /// prints information about all the shapes in a sketch including:
        /// 1) labeled type (if any)
        /// 2) midpoint of shape
        /// 3) assigned column (to be done)
        /// 4) assigned type (to be done)
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static string printShapeData(Sketch.Sketch sk)
        {
            string text = "";

            for (int i = 0; i < numShapes(sk); i++)
            {
                text += "Shape[" + i + "] labeled: " + sk.Shapes[i].XmlAttrs.Type;
                text += ", x,y: " + sk.Shapes[i].XmlAttrs.X + "," + sk.Shapes[i].XmlAttrs.Y;
            }
            
            return text;
        }
        
        /// <summary>
        /// returns the percentage of shapes correctly identified
        /// </summary>
        /// <param name="sk"></param>
        /// <returns></returns>
        static double percentAccuracy(Sketch.Sketch sk)
        {
            TruthTables.TruthTable tt = new TruthTables.TruthTable(sk);
            double correct = 0;

            for (int i = 0; i < numShapes(sk); i++)
            {
                if ((string)sk.Shapes[i].XmlAttrs.Type == tt.assignType(sk.Shapes[i]))
                {
                    correct++;
                }
            }

            return 100 * correct / numShapes(sk);
        }
        
        /// <summary>
        /// prints out all the stats for the sketch
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Sketch.Sketch sketch = (new ReadXML(args[0])).Sketch;

                Console.WriteLine("\n --------------- DATA FOR SKETCH: " + args[0] + " ---------------");
                Console.WriteLine("\n Number of Strokes is: " + numStrokes(sketch));
                Console.WriteLine("\n Average Length of Stroke is: " + avgStrokeLength(sketch) + " pixels");
                Console.WriteLine("\n Number of Shapes is: " + numShapes(sketch));
                Console.WriteLine("\n Number of Strokes per Shape is: " + numStrokesPerShape(sketch));
                Console.WriteLine("\n List of Shape types: \n" + myPrintArrayList(shapeTypes(sketch), countTypes(sketch)));
                Console.WriteLine("\n Shape Data \n" + printShapeData(sketch));
                //Console.WriteLine("\n Shape Data \n" + printShapeData(Featurefy.TruthTable.sortByX(sketch)));
                Console.WriteLine("\n Percent of types correctly identified: " + percentAccuracy(sketch));
                //int index = TruthTables.Column.findMinX(sketch);
                //Console.WriteLine("\n Shape with min X coordinate: Shape[" + index + "]");
                //Console.WriteLine("\n symbols per col: " + printList(col.symbolsPerCol()));
                Console.WriteLine("\n------------------------------- END DATA -------------------------------");
            }
        }
    }
}
