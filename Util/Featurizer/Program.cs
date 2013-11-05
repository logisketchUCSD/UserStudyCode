using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;
using Featurefy;
using Utilities;
using ConverterXML;
using StrokeClassifier;
using StrokeGrouper;

namespace Featurizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string parameters = "args:\n";
            parameters += "\t0 - Directory of sketches (input)\n";
            parameters += "\t1 - Search pattern for sketches (input)\n";
            parameters += "\t2 - Mapping text file - shape types --> group (input)\n";
            parameters += "\t3 - Directory for feature files (output)\n";
            parameters += "\t4 - Options (-s = single-stroke features only, -sg = single and grouping, -g = grouping only)\n";
            parameters += "\t5/6 - Files with features on for single-stroke and grouping (single stroke first if using both)\n";  

            bool SingleStrokeOn, GroupingOn;
            string options;

            #region Args checking, get options
            if (args.Length < 5)
            {
                Console.Write(parameters);
                Console.WriteLine();
                return;
            }

            options = args[4];

            if (options == "-s")
            {
                SingleStrokeOn = true;
                GroupingOn = false;
            }
            else if (options == "-g")
            {
                SingleStrokeOn = false;
                GroupingOn = true;
            }
            else if (options == "-sg")
            {
                SingleStrokeOn = true;
                GroupingOn = true;
            }
            else
            {
                Console.Write(parameters);
                Console.WriteLine();
                Console.WriteLine("Options invalid: must be -s -sg or -g");
                return;
            }
            #endregion

            #region Initializations

            if (!Directory.Exists(args[3]))
                Directory.CreateDirectory(args[3]);

            List<string> sketches = new List<string>(Directory.GetFiles(args[0], args[1]));

            Dictionary<string, string> mapping = GetMapping(args[2]);
            List<string> allClasses = new List<string>(mapping.Values);
            allClasses = General.RemoveDuplicates(allClasses);

            Dictionary<string, bool> ssFeaturesOn = new Dictionary<string, bool>();
            Dictionary<string, bool> pairFeaturesOn = new Dictionary<string, bool>();
            if (SingleStrokeOn && GroupingOn)
            {
                ssFeaturesOn = GetFeaturesOn(args[5]);
                pairFeaturesOn = GetFeaturesOn(args[6]);
            }
            else if (SingleStrokeOn)
            {
                ssFeaturesOn = GetFeaturesOn(args[5]);
            }
            else if (GroupingOn)
            {
                pairFeaturesOn = GetFeaturesOn(args[5]);
            }

            List<string> ssFeatureNames = new List<string>();
            foreach (KeyValuePair<string, bool> kvp in ssFeaturesOn)
                if (kvp.Value)
                    ssFeatureNames.Add(kvp.Key);
            List<string> ssClassificationNames = new List<string>(allClasses);
            //ssClassificationNames.Add("Gate");
            //ssClassificationNames.Add("Wire");
            //ssClassificationNames.Add("Label");
            
            Features ssFeatures = new Features(ssFeatureNames, ssClassificationNames, "Family_Trees_SingleStroke");

            List<string> pairFeatureNames = new List<string>();
            foreach (KeyValuePair<string, bool> kvp in pairFeaturesOn)
                if (kvp.Value)
                    pairFeatureNames.Add(kvp.Key);
            List<string> pairClassificationNames = new List<string>();
            pairClassificationNames.Add("Join");
            //pairClassificationNames.Add("JoinGate");
            //pairClassificationNames.Add("JoinLabel");
            //pairClassificationNames.Add("JoinWire");
            pairClassificationNames.Add("NoJoin");
            pairClassificationNames.Add("Ignore");
            Features pairFeatures = new Features(pairFeatureNames, pairClassificationNames, "Family_Trees_Grouping");

            List<string> users = new List<string>();

            string dir = args[3];

            /*Dictionary<Guid, string> allClassifications = new Dictionary<Guid, string>();
            StreamReader reader = new StreamReader("C:\\allClassifications.txt");
            string line;
            while ((line = reader.ReadLine()) != null && line != "")
            {
                string[] splits = line.Split(new char[] { ' ' });
                if (splits.Length == 2)
                    allClassifications.Add(new Guid(splits[0]), splits[1]);
            }
            reader.Close();*/

            #endregion

            SSandGroupFeatures(SingleStrokeOn, GroupingOn, sketches, mapping, allClasses,
                ssFeaturesOn, ssFeatures, ssFeatureNames, ssClassificationNames,
                pairFeaturesOn, pairFeatures, pairFeatureNames,
                users, dir, new Dictionary<Guid, string>()); //allClassifications);

            //PrintPatterns(sketches, dir);
        }

        private static void PrintPatterns(List<string> sketches, string dir)
        {
            foreach (string sketchFile in sketches)
            {
                Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                sketch = Utilities.General.ReOrderParentShapes(sketch);
                sketch = Utilities.General.LinkShapes(sketch);
                string userName = Path.GetFileNameWithoutExtension(sketchFile).Substring(0, 2);

            }
        }

        private static void SSandGroupFeatures(bool SingleStrokeOn, bool GroupingOn, 
            List<string> sketches, Dictionary<string, string> mapping, List<string> allClasses, 
            Dictionary<string, bool> ssFeaturesOn, Features ssFeatures, List<string> ssFeatureNames, List<string> ssClassificationNames,
            Dictionary<string, bool> pairFeaturesOn, Features pairFeatures, List<string> pairFeatureNames, List<string> users, string dir,
            Dictionary<Guid, string> allSSClassifications)
        {
            List<string> ignore = new List<string>();
            /*ignore.Add("DoubleShaftArrow");
            ignore.Add("EquationType");
            ignore.Add("ForceEquilEquation");
            ignore.Add("MomentEquilEquation");
            ignore.Add("GeomTrigEquation");
            ignore.Add("OtherEquation");
            ignore.Add("OtherText");
            ignore.Add("Ellipses");
            ignore.Add("Box");
            ignore.Add("Triangle");
            ignore.Add("AngleSquare");
            ignore.Add("Angle");
            ignore.Add("OtherGeometry");
            ignore.Add("Other");*/
            ignore.Add("unlabeled");

            #region Calculate Features
            foreach (string sketchFile in sketches)
            {
                Sketch.Sketch sketch = new ReadXML(sketchFile).Sketch;
                //sketch = Utilities.General.ReOrderParentShapes(sketch);
                //sketch = Utilities.General.LinkShapes(sketch);

                string nameShort = Path.GetFileNameWithoutExtension(sketchFile); 
                int index = nameShort.IndexOf('_');
                string userName = nameShort.Substring(0, index);
                //if (sketchFile.Contains("_T"))
                //userName += "_T";
                //else if (sketchFile.Contains("_P"))
                //userName += "_P";

                if (!users.Contains(userName))
                    users.Add(userName);

                FeatureSketch fSketch = new FeatureSketch(sketch, ssFeaturesOn, pairFeaturesOn, new Dictionary<string, double[]>());
                foreach (Substroke stroke in sketch.SubstrokesL)
                    stroke.Classification = mapping[stroke.FirstLabel];

                Dictionary<Substroke, double[]> valuesSingle;
                Dictionary<string, Dictionary<FeatureStrokePair, double[]>> class2pairValues;

                if (SingleStrokeOn)
                {
                    fSketch.GetValuesSingle(out valuesSingle, ValuePreparationStage.Normalized);

                    foreach (Substroke s in sketch.Substrokes)
                    {
                        string label = s.FirstLabel;
                        string cls = mapping[label];

                        if (!ignore.Contains(label))
                            ssFeatures.Add(userName, "SingleStroke", valuesSingle[s], cls);
                    }
                }

                if (GroupingOn)
                {
                    Dictionary<Substroke, string> classifications = new Dictionary<Substroke, string>();
                    // Use actual classifications from Weka classifier
                    //foreach (Substroke s in sketch.Substrokes)
                        //classifications.Add(s, allSSClassifications[s.Id]);
                    foreach (Substroke s in sketch.SubstrokesL)
                        classifications.Add(s, s.Classification);

                    fSketch.GetValuesPairwise(out class2pairValues, classifications, ValuePreparationStage.Normalized);

                    foreach (KeyValuePair<string, Dictionary<FeatureStrokePair, double[]>> kvp in class2pairValues)
                    {
                        string cls = kvp.Key;
                        Dictionary<FeatureStrokePair, double[]> features = kvp.Value;

                        foreach (FeatureStrokePair pair in features.Keys)
                        {
                            if (pair.StrokeA.ParentShapes[0] == pair.StrokeB.ParentShapes[0])
                            {
                                string parentName = mapping[pair.StrokeA.ParentShapes[0].Label];
                                string join = "Join";// +parentName;

                                //System.Drawing.Rectangle bbox = Utilities.Compute.BoundingBox(pair.StrokeA.ParentShapes[0].Substrokes);
                                //double diag = Utilities.Compute.DiagonalLength(bbox);
                                double[] minInShape = GetMinDistanceInShape(pair, pair.StrokeA.ParentShapes[0], features);
                                double factor = 1.15;
                                double minDistThresh = 200.0;
                                if (pair.SubstrokeDistance.Min <= minDistThresh ||
                                    pair.SubstrokeDistance.Min <= minInShape[0] * factor ||
                                    pair.SubstrokeDistance.Min <= minInShape[1] * factor)
                                    pairFeatures.Add(userName, cls, features[pair], join);
                                else
                                    pairFeatures.Add(userName, cls, features[pair], "Ignore");

                            }
                            else
                            {
                                //if (features[pair][2] < 0.15)
                                pairFeatures.Add(userName, cls, features[pair], "NoJoin");
                            }
                        }
                    }
                }

                /*foreach (Shape shape in sketch.ShapesL)
                {
                    if (shape.ParentShape.SubstrokesL.Count != 0)
                        continue;

                    Shape chained = ChainSubstrokesInShape(shape, fSketch);
                    if (!EquivalentShapes(shape, chained))
                    {
                        Console.WriteLine("ERROR - Shape wasn't chained!! {0}, {1}", shape.Label, Path.GetFileNameWithoutExtension(sketchFile));
                    }
                }*/
            }
            #endregion


            #region Printing

            if (SingleStrokeOn)
            {
                foreach (string user in users)
                {
                    string filename = dir + "\\SingleStrokeTraining_" + user + ".arff";
                    ssFeatures.PrintARFF(filename, user, "SingleStroke", true);
                    string filename2 = dir + "\\" + user + "_features.txt";
                    ssFeatures.Print(filename2, user, "SingleStroke");
                }
                ssFeatures.PrintARFF(dir + "\\SingleStrokeTraining_all.arff", "all", "SingleStroke", true);
            }

            if (GroupingOn)
            {
                foreach (string user in users)
                {
                    foreach (string cls in ssClassificationNames)
                    {
                        string filename = dir + "\\Grouping_" + user + "_" + cls + ".arff";
                        pairFeatures.PrintARFF(filename, user, cls, true);
                        string testFilename = dir + "\\testing\\Grouping_" + user + "_" + cls + ".arff";
                        pairFeatures.PrintTestingARFF(testFilename, user, cls, true);
                    }
                    /*string filename = dir + "Grouping_" + user + ".arff";
                    pairFeatures.PrintARFF(filename, user, "All", true);
                    string testFilename = dir + "\\testing\\Grouping_" + user + ".arff";
                    pairFeatures.PrintTestingARFF(testFilename, user, "All", true);*/
                }

                foreach (string cls in ssClassificationNames)
                {
                    string filename = dir + "\\Grouping_all_" + cls + ".arff";
                    pairFeatures.PrintARFF(filename, "all", cls, true);
                }
                /*string filename2 = dir + "Grouping_All.arff";
                pairFeatures.PrintARFF(filename2, "all", "All", true);*/
            }

            #endregion
        }

        private static double[] GetMinDistanceInShape(FeatureStrokePair pair, Shape shape, Dictionary<FeatureStrokePair, double[]> features)
        {
            List<FeatureStrokePair> pairsA = new List<FeatureStrokePair>();
            List<FeatureStrokePair> pairsB = new List<FeatureStrokePair>();

            foreach (FeatureStrokePair fPair in features.Keys)
            {
                if (fPair.StrokeA == pair.StrokeA)
                    pairsA.Add(fPair);
                else if (fPair.StrokeB == pair.StrokeA)
                    pairsA.Add(fPair);

                if (fPair.StrokeB == pair.StrokeB)
                    pairsB.Add(fPair);
                else if (fPair.StrokeA == pair.StrokeB)
                    pairsB.Add(fPair);
            }

            double[] values = new double[] { pairsA[0].SubstrokeDistance.Min, pairsB[0].SubstrokeDistance.Min };

            foreach (FeatureStrokePair fPair in pairsA)
                values[0] = Math.Min(values[0], fPair.SubstrokeDistance.Min);

            foreach (FeatureStrokePair fPair in pairsB)
                values[1] = Math.Min(values[1], fPair.SubstrokeDistance.Min);

            return values;
        }

        #region Helper Functions

        private static bool EquivalentShapes(Shape shape, Shape chained)
        {
            if (shape.SubstrokesL.Count != chained.SubstrokesL.Count)
                return false;

            foreach (Substroke stroke1 in shape.SubstrokesL)
                if (!chained.SubstrokesL.Contains(stroke1))
                    return false;

            return true;
        }

        private static Shape ChainSubstrokesInShape(Shape shape, FeatureSketch fSketch)
        {
            if (shape.SubstrokesL.Count < 2)
                return shape;

            double thresh = 0.2;
            System.Drawing.Rectangle bbox = Utilities.Compute.BoundingBox(shape.Substrokes);
            double diag = Utilities.Compute.DiagonalLength(bbox);
            double dist = thresh * diag;

            List<FeatureStrokePair> relevantPairs = new List<FeatureStrokePair>();
            foreach (Substroke stroke in shape.SubstrokesL)
            {
                foreach (FeatureStrokePair pair in fSketch.PairwiseFeatureSketch.AllFeaturePairs)
                {
                    if (relevantPairs.Contains(pair))
                        continue;
                    else if (pair.StrokeA == stroke && shape.SubstrokesL.Contains(pair.StrokeB))
                        relevantPairs.Add(pair);
                    else if (pair.StrokeB == stroke && shape.SubstrokesL.Contains(pair.StrokeA))
                        relevantPairs.Add(pair);
                }
            }

            List<StrokePair> joinedPairs = new List<StrokePair>();
            foreach (FeatureStrokePair pair in relevantPairs)
            {
                if (pair.SubstrokeDistance.Min < dist 
                    || pair.SubstrokeDistance.Min == fSketch.PairwiseFeatureSketch.MinDistanceFromStrokeSameClass(pair.StrokeA)
                    || pair.SubstrokeDistance.Min == fSketch.PairwiseFeatureSketch.MinDistanceFromStrokeSameClass(pair.StrokeB))
                    joinedPairs.Add(pair.StrokePair);
            }

            List<List<Substroke>> strokes = new List<List<Substroke>>();
            foreach (StrokePair pair in joinedPairs)
            {
                if (strokes.Count == 0)
                {
                    List<Substroke> list = new List<Substroke>();
                    list.Add(pair.Stroke1);
                    list.Add(pair.Stroke2);
                    strokes.Add(list);
                    continue;
                }

                bool found1 = false;
                bool found2 = false;
                List<Substroke> list1 = null;
                List<Substroke> list2 = null;
                foreach (List<Substroke> list in strokes)
                {
                    if (list.Contains(pair.Stroke1))
                    {
                        found1 = true;
                        list1 = list;
                    }

                    if (list.Contains(pair.Stroke2))
                    {
                        found2 = true;
                        list2 = list;
                    }
                }

                if (!found1 && !found2)
                {
                    List<Substroke> list = new List<Substroke>();
                    list.Add(pair.Stroke1);
                    list.Add(pair.Stroke2);
                    strokes.Add(list);
                    continue;
                }
                else if (found1 && found2)
                {
                    if (list1 != list2)
                    {
                        foreach (Substroke s2 in list2)
                        {
                            if (!list1.Contains(s2))
                                list1.Add(s2);
                        }
                        strokes.Remove(list2);
                    }
                    else
                        continue;
                }
                else if (found1)
                {
                    list1.Add(pair.Stroke2);
                }
                else if (found2)
                {
                    list2.Add(pair.Stroke1);
                }
            }

            bool change = true;
            while (strokes.Count > 1 && change)
            {
                int count1 = strokes.Count;
                List<Substroke> toRemove = null;
                foreach (List<Substroke> list1 in strokes)
                {
                    foreach (List<Substroke> list2 in strokes)
                    {
                        if (list1 != list2)
                        {
                            foreach (Substroke s in list1)
                            {
                                if (list2.Contains(s))
                                {
                                    foreach (Substroke s1 in list1)
                                        if (!list2.Contains(s1))
                                            list2.Add(s1);

                                    toRemove = list1;
                                    break;
                                }
                            }
                            if (toRemove != null)
                                break;
                        }
                    }
                    if (toRemove != null)
                        break;
                }
                if (toRemove != null)
                    strokes.Remove(toRemove);

                int count2 = strokes.Count;
                if (count1 == count2)
                    change = false;
            }

            List<Substroke> best = new List<Substroke>();
            foreach (List<Substroke> list in strokes)
                if (list.Count > best.Count)
                    best = list;

            return new Shape(best, shape.XmlAttrs);
        }

        private static Dictionary<string, double[]> GetMapping2(Dictionary<string, string> mapping)
        {
            List<string> classes = new List<string>();
            List<string> values = new List<string>(mapping.Values);
            foreach (string v in values)
                if (!classes.Contains(v))
                    classes.Add(v);

            Dictionary<string, double[]> output = new Dictionary<string, double[]>();
            foreach (string shape in mapping.Keys)
            {
                double[] nums = new double[classes.Count];
                for (int i = 0; i < nums.Length; i++)
                    nums[i] = 0.0;

                int index = classes.IndexOf(mapping[shape]);
                nums[index] = 1.0;

                output.Add(shape, nums);
            }

            return output;
        }

        private static Dictionary<string, bool> GetFeaturesOn(string p)
        {
            StreamReader reader = new StreamReader(p);

            Dictionary<string, bool> features = new Dictionary<string, bool>();

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                int index = line.IndexOf("=");
                string feature = line.Substring(0, index - 1);
                string on = line.Substring(index + 2);
                if (on == "true")
                    features.Add(feature, true);
                else if (on == "false")
                    features.Add(feature, false);
                else
                    throw new Exception("Value not true or false");
            }

            reader.Close();

            return features;
        }

        private static Dictionary<string, string> GetMapping(string filename)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            StreamReader reader = new StreamReader(filename);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                if (line.Substring(0, 2) == "//") continue;

                string shapeName = line.Substring(0, line.IndexOf(":"));
                string className = line.Substring(line.IndexOf(":") + 2);

                if (!map.ContainsKey(shapeName))
                    map.Add(shapeName, className);
            }

            reader.Close();

            return map;
        }

        #endregion
    }
}
