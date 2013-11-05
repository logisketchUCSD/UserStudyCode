/**
 * File:    LabelMapper.cs
 * 
 * Purpose: Takes a text file to create a map from one label domain to another.
 *          Then, given a sketch, it can convert its labels to the new domain.
 * 
 * Authors: Skechers 2007
 *          Harvey Mudd College, Claremont, CA 91711.
 */

using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sketch;

namespace LabelMapper
{
    public class LabelMapper
	{
		#region Internals

		public Dictionary<string, string> labelMap;
        private bool verbose;

		#endregion

		/// <summary>
		/// Construct a new LabelMap
		/// </summary>
		/// <param name="labelMapFile">The filename to read from</param>
        public LabelMapper(string labelMapFile)
        {
            this.labelMap = new Dictionary<string,string>();
            try
            {
                // Read the label map file						
                StreamReader reader = new StreamReader(labelMapFile);
                this.verbose = true;
                string line;
                string[] labelPair;
                char[] splitChar = "=".ToCharArray(); // The map file is in the form labelA=labelB, so we need to split.

                // Read lines until the end of the file is reached
                while ((line = reader.ReadLine()) != null)
                {
                    labelPair = line.Split(splitChar);
                    /*
                    // add the labels to the domain lists if they are new
                    if (!trueLabelDomain.Contains(labelPair[0]))
                    {
                        trueLabelDomain.Add(labelPair[0]);
                        if (verbose) System.Console.Out.WriteLine("Added _{0}_ to true   label domain", labelPair[0]);
                    }
                    if (!resultLabelDomain.Contains(labelPair[1]))
                    {
                        resultLabelDomain.Add(labelPair[1]);
                        if (verbose) System.Console.Out.WriteLine("Added _{0}_ to result label domain", labelPair[1]);
                    }
                    */
                    // add the pair to the hash mapping
                    this.labelMap[labelPair[0]] = labelPair[1];
                }
                /*
                // Transfer to the translated label domain, which should not be changed after this point
                // (if new labels are found they will be added to the other domains)
                foreach (string s in resultLabelDomain)
                    transLabelDomain.Add(s);
                */
            }
            catch
            {
                // Don't break the program if we don't have a label map, it's not essential.
                System.Console.Out.WriteLine("LabelMapper could not process label map file.");
            }
        }

		/// <summary>
		/// Convert a sketch from 
		/// </summary>
		/// <param name="originalSketch"></param>
        public void translateSketch(ref Sketch.Sketch originalSketch)
        {
            foreach (Shape shape in originalSketch.Shapes)
            {
				ShapeType type = shape.Type;

                if (labelMap.ContainsKey(type.Name)) // Convert it if it's in our map
                    shape.Type = LogicDomain.getType(labelMap[type.Name]);
                else if (this.verbose)
                    System.Console.Error.WriteLine("Type {0} not specified in label map", type);
            }
        }

		/// <summary>
		/// Get the classes that this label mapper knows about (the target classes, that is)
		/// </summary>
		public List<string> translatedClasses
		{
			get
			{
				Set.ListSet<string> classes = new Set.ListSet<string>();
				foreach (KeyValuePair<string, string> items in labelMap)
				{
					classes.Add(items.Value);
				}
				return classes.AsList();
			}
		}
		
		/// <summary>
		/// Get the source classes that this label mapper knows about
		/// </summary>
		public List<string> sourceClasses
		{
			get
			{
				Set.Set<string> classes = new Set.ListSet<string>();
				foreach (KeyValuePair<string, string> items in labelMap)
				{
					classes.Add(items.Key);
				}
				return classes.AsList();
			}
		}
    }
}
