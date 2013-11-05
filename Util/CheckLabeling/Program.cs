using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using msInkToHMCSketch;
using Sketch;
using ConverterXML;

namespace CheckLabeling
{
    class Program
    {
        #region Define Leaves

        static List<string> leafLabels = new List<string>(new string[] {
            "BackLine", "FrontArc", "BackArc", "TopArc", "BottomArc",
            "Bubble", "TopLine", "BottomLine", "Triangle", 
            "GreaterThan", "TouchUp", "Junk", "A", "B", "C", "D", 
            "Y", "LabelBoxOther", "Not_V", "Not_Hat", "Entire_OR", "Entire_AND" });

        static List<string> NOTBUBBLEleaves = new List<string>(new string[] {
            "Bubble", "TouchUp", "Junk" });

        static List<string> ORleaves = new List<string>(new string[] {
            "BackArc", "FrontArc", "TopArc", "BottomArc", "TouchUp", 
            "Junk", "Entire_OR" });

        static List<string> ANDleaves = new List<string>(new string[] {
            "BackLine", "FrontArc", "TouchUp", "Junk", "Entire_AND"  });

        static List<string> NANDleaves = new List<string>(new string[] {
            "BackLine", "FrontArc", "Bubble", "TouchUp", "Junk", "Entire_AND"  });

        static List<string> NORleaves = new List<string>(new string[] {
            "BackArc", "FrontArc", "TopArc", "BottomArc", "Bubble", 
            "TouchUp", "Junk", "Entire_OR" });

        static List<string> NOTleaves = new List<string>(new string[] {
            "BackLine", "TopLine", "BottomLine", "Triangle", "Not_V", 
            "Not_Hat", "GreaterThan", "Bubble", "TouchUp", "Junk" });

        static List<string> XORleaves = new List<string>(new string[] {
            "BackArc", "FrontArc", "TopArc", "BottomArc", "TouchUp", 
            "Junk", "Entire_OR" });

        static List<string> XNORleaves = new List<string>(new string[] {
            "BackArc", "FrontArc", "TopArc", "BottomArc", "Bubble",
            "TouchUp", "Junk", "Entire_OR" });

        static List<string> LabelBoxleaves = new List<string>(new string[] {
            "BackLine", "TopLine", "BottomLine", "LabelBoxOther", 
            "TouchUp", "Junk" });

        static List<string> Labelleaves = new List<string>(new string[] {
            "A", "B", "C", "D", "Y" });

        static List<string> XNORSubShapes = new List<string>(new string[] {
            "XOR", "NOR", "OR", "NOTBUBBLE" });

        static List<string> NOTSubShapes = new List<string>(new string[] {
            "NOTBUBBLE" });

        static List<string> NORSubShapes = new List<string>(new string[] {
            "OR", "NOTBUBBLE" });

        static List<string> NANDSubShapes = new List<string>(new string[] {
            "AND", "NOTBUBBLE" });

        static List<string> XORSubShapes = new List<string>(new string[] {
            "OR" });

        static List<string> TopLevelShapes = new List<string>(new string[] {
            "AND", "OR", "NAND", "NOR", "NOT", "NOTBUBBLE", "XOR", "XNOR", 
            "LabelBox", "Label", "Wire", "Other" });
        
        #endregion

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;

            string[] filenames = Directory.GetFiles(args[0], args[1]);

            foreach (string file in filenames)
            {
                Console.WriteLine();
                Console.WriteLine("Sketch: " + Path.GetFileName(file));
                Console.WriteLine();
                try
                {
                    Sketch.Sketch sketch = new ReadXML(file).Sketch;
                    sketch = ReOrderParentShapes(sketch);
                    List<int> two = new List<int>(new int[] { 2 });
                    List<int> three = new List<int>(new int[] { 3 });
                    List<int> twoThree = new List<int>(new int[] { 2, 3 });
                    List<int> threeFour = new List<int>(new int[] { 3, 4 });
                    List<string> empty = new List<string>();

                    foreach (Substroke stroke in sketch.Substrokes)
                    {
                        foreach (Shape s in stroke.ParentShapes)
                        {
                            if (s.XmlAttrs.Type == "OR" && s.SubstrokesL.Count == 1 && stroke.Labels[stroke.Labels.Length - 1] != "Entire_OR")
                                Console.WriteLine("  Single Stroke OR found");
                            if (s.XmlAttrs.Type == "AND" && s.SubstrokesL.Count == 1 && stroke.Labels[stroke.Labels.Length - 1] != "Entire_AND")
                                Console.WriteLine("  Single Stroke AND found");
                        }
                        #region Switch of stroke's first label
                        switch (stroke.FirstLabel)
                        {
                            case ("Wire"):
                                break;
                            case ("Label"):
                                CheckStroke(stroke, two, empty, Labelleaves);
                                break;
                            case ("LabelBox"):
                                CheckStroke(stroke, two, empty, LabelBoxleaves);
                                break;
                            case ("XNOR"):
                                CheckStroke(stroke, threeFour, XNORSubShapes, XNORleaves);
                                break;
                            case ("NOT"):
                                CheckStroke(stroke, twoThree, NOTSubShapes, NOTleaves);
                                break;
                            case ("XOR"):
                                CheckStroke(stroke, twoThree, XORSubShapes, XORleaves);
                                break;
                            case ("NOR"):
                                CheckStroke(stroke, three, NORSubShapes, NORleaves);
                                break;
                            case ("NAND"):
                                CheckStroke(stroke, three, NANDSubShapes, NANDleaves);
                                break;
                            case ("AND"):
                                CheckStroke(stroke, two, empty, ANDleaves);
                                break;
                            case ("OR"):
                                CheckStroke(stroke, two, empty, ORleaves);
                                break;
                            case ("NOTBUBBLE"):
                                CheckStroke(stroke, two, empty, NOTBUBBLEleaves);
                                break;
                            case ("Other"):
                                break;
                            default:
                                Console.WriteLine("    Warning: Unknown Label: " 
                                    + stroke.FirstLabel + ": " + stroke.Id.ToString());
                                break;
                        }
                        #endregion
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("!!!! Error with Sketch " + Path.GetFileName(file));
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void CheckStroke(Substroke stroke, 
            List<int> allowableNumLabels, 
            List<string> subShapes, 
            List<string> shapeLeafLabels)
        {
            if (!allowableNumLabels.Contains(stroke.Labels.Length))
            {
                Console.WriteLine("  Error: Stroke " + stroke.Id.ToString()
                    + " is a " + stroke.FirstLabel + " and has these labels:");
                foreach (string label in stroke.Labels)
                    Console.WriteLine("    " + label);
            }
            List<string> labels = new List<string>(stroke.Labels);
            bool foundTopLevel = false;
            foreach (string name in TopLevelShapes)
                if (labels.Contains(name))
                    foundTopLevel = true;

            if (!foundTopLevel)
            {
                Console.WriteLine("  Error: Did not find a Top Level Shape for stroke " 
                    + stroke.Id.ToString() + ", Labels:");
                foreach (string label in labels)
                    Console.WriteLine("    " + label);

            }
            if (stroke.FirstLabel != "Wire" && stroke.FirstLabel != "Other")
            {
                bool foundLeaf = false;
                foreach (string name in leafLabels)
                    if (labels.Contains(name))
                        foundLeaf = true;

                if (!foundLeaf)
                {
                    Console.WriteLine("  Error: Did not find a Leaf Shape for stroke "
                        + stroke.Id.ToString() + ", Labels:");
                    foreach (string label in labels)
                        Console.WriteLine("    " + label);
                }
            }

            foreach (string label in stroke.Labels)
            {
                if (label != stroke.FirstLabel && !subShapes.Contains(label))
                {
                    if (!leafLabels.Contains(label))
                        Console.WriteLine("  Error: Stroke " + stroke.Id.ToString()
                            + " is a " + stroke.FirstLabel
                            + " and has this incorrect label: " + label);
                    else if (!shapeLeafLabels.Contains(label))
                        Console.WriteLine("    Warning: Stroke " + stroke.Id.ToString()
                            + " is a " + stroke.FirstLabel
                            + " and has this possibly incorrect label: " + label);
                }
            }
        }


        private static Sketch.Sketch ReOrderParentShapes(Sketch.Sketch sketch)
        {
            List<List<string>> hierarchy = GetHierarchy();
            
            foreach (Substroke stroke in sketch.Substrokes)
            {
                List<string> labels = new List<string>(stroke.Labels);
                List<Shape> oldParents = new List<Shape>(stroke.ParentShapes);
                List<Shape> parents = new List<Shape>();

                foreach (List<string> order in hierarchy)
                {
                    foreach (string shape in order)
                    {
                        if (labels.Contains(shape))
                        {
                            foreach (Shape s in oldParents)
                            {
                                if (s.Label == shape)
                                {
                                    parents.Add(s);
                                    //oldParents.Remove(s);
                                    //labels.Remove(shape);
                                }
                            }
                        }
                    }
                }

                stroke.ParentShapes = parents;
            }

            return sketch;
        }

        private static List<List<string>> GetHierarchy()
        {
            List<List<string>> hierarchy = new List<List<string>>();
            List<string> first = new List<string>();
            first.Add("Wire");
            first.Add("Label");
            first.Add("XNOR");
            first.Add("NOT");
            first.Add("LabelBox");

            List<string> second = new List<string>();
            second.Add("NOR");
            second.Add("NAND");
            second.Add("XOR");

            List<string> third = new List<string>();
            third.Add("AND");
            third.Add("OR");
            third.Add("NOTBUBBLE");

            List<string> fourth = new List<string>(leafLabels);

            hierarchy.Add(first);
            hierarchy.Add(second);
            hierarchy.Add(third);
            hierarchy.Add(fourth);

            return hierarchy;
        }
    }
}
