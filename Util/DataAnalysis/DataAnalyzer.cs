using System;
using System.Collections;
using ConverterXML;
using Sketch;
using System.IO;

namespace DataAnalysis
{
	/// <summary>
	/// This class extracts statistics from labeled sketches.  
	/// Here are the statistics that I would like to support:
	/// - Number of gates/wires drawn with non-consecutive strokes (and how many strokes are non-consecutive)
	/// - Order of text relative to diagram pieces
	///		- Order of labels vs rest of diagram
	///	- Order of gates vs. wires that connect to them (input and output)
	///	- Timing between strokes in the same object vs different objects
	///	- Differences between participants in any of the above
	///	- Number of strokes per each gate and per wire and per text item
	///	- Length of strokes in each wire/text/gate
	/// </summary>
	public class DataAnalyzer
	{
		private ArrayList sketches;   // Holds the sketches that have been loaded
		private Hashtable userIds;	  // Holds the user Ids of the people who created the sketches
										// Indexed by sketch id.


		/// <summary>
		/// Create a new data analyzer
		/// </summary>
		public DataAnalyzer( )
		{
			sketches = new ArrayList();
			userIds = new Hashtable();
		}

		/// <summary>
		/// Load a new sketch to be analyzed
		/// </summary>
		/// <param name="filename">The name of the file containing the sketch</param>
		/// <param name="userID">The id of the user.  
		/// TODO: This should come from the sketch itself,
		/// but right now I think that information is empty.</param>
		public void loadSketch( string filename, int userId )
		{
			
			Sketch.Sketch sk = (new ReadXML(filename)).Sketch;
			if (userIds.ContainsKey(sk.XmlAttrs.Id))
			{
				Console.WriteLine( "Skipping sketch " + sk.XmlAttrs.Id + ". Already loaded" );
				return;
			}
			sketches.Add(sk);
			userIds.Add(sk.XmlAttrs.Id, userId);
		}

		public void testStatistics( string filename )
		{
			StreamWriter sw = File.CreateText(filename);
			foreach ( Sketch.Sketch s in sketches )
			{
				sw.WriteLine( "Sketch " + s+ " has " + s.Strokes.Length + " strokes ");

			}
			sw.Close();
		}

		/// <summary>
		/// Print the number of objects to a file
		/// </summary>
		public void objectStats( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			foreach (Sketch.Sketch s in sketches)
			{
				
				foreach (Shape sh in s.Shapes)
				{
					sw.WriteLine(sh.XmlAttrs.Name + " containing " + sh.Substrokes.Length + " substrokes");
				}
			}

			sw.Close();
		}

		// This function should return something or write to a file
		public void consecutiveStats( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			// Put the key at the top of the file
			sw.WriteLine( "SketchID UserID ShapeName IsConsecutive" );
			foreach (Sketch.Sketch s in sketches)
			{
				// For each shape calculate if it is consecutive (i.e., are there any strokes interrupting it)
				foreach (Shape sh in s.Shapes)
				{
					// get the strokes in the shape
					// In theory these substrokes are time ordered...
					Substroke[] strokes = sh.Substrokes;
					if (strokes.Length == 0)
					{
						Console.WriteLine( "Something's wrong.  Shape " + sh + " has no substrokes" );
					}
					
					Substroke s1 = strokes[0];
					// find the strokes in the sketch and make sure they are not interrupted by strokes not in the shape
					bool in_shape = false;
					int sh_index = 0;
					bool okSoFar = true;
					for (int i = 0; i < s.Substrokes.Length; i++)
					{
					
						Substroke str = s.Substrokes[i];
						if (!in_shape && str.Equals( s1 )) 
						{
							in_shape = true;
						}
						if (sh_index == strokes.Length) 
						{
							break;
						}
						if (in_shape) 
						{
							s1 = strokes[sh_index];
							if (!s1.Equals(str))
							{
								okSoFar = false;
								sw.WriteLine( "{0} {1} {2} False", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], sh.XmlAttrs.Name );
								break;
							}
							
							sh_index++;
						}
					}
					if (okSoFar) 
					{
						sw.WriteLine( "{0} {1} {2} True", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], sh.XmlAttrs.Name);
					}
				}
			}

			sw.Close();
		}

		/// <summary>
		/// Writes the timing statistics for the strokes in the sketch to a file
		/// The format of the lines in the file are: 
		/// SketchID SketcherID ShapeID Name TimeGap Continues Returned
		/// Where Name is the label, TimeGap is the time from the end of the previous stroke to
		/// the start of this stroke, Continues is a boolean (0 or 1) that says
		/// whether this stroke is part of the object that's currently being drawn
		/// and Returned is a boolean (0 or 1) that indicates whether this stroke
		/// has returned to an object that the user started drawing previously.
		/// If both Continues and Returns are 0, then this stroke starts a new object completely
		/// 
		/// Warning: This function will probably break if shapes are hierarchical
		/// so I assume they are not for now.
		/// </summary>
		public void timingStats( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			sw.WriteLine( "SketchID UserID ShapeID StrokeID ShapeName Time ContinuesPrevShape ContinuesSameType ReturnsToPriorShape" );
			bool continues;
			bool continuesType;
			bool returns;
			foreach (Sketch.Sketch s in sketches)
			{
				ArrayList seenShapes = new ArrayList();
				Shape currShape = null;
				Substroke last = null;
				foreach ( Substroke stroke in s.Substrokes )
				{
					// Find the shape that this stroke is part of
					// This method should probably be part of sketch
					Shape strShape = findShape( s, stroke );
					if (strShape == null)
					{
						continues = false;
						continuesType = false;
					}
					else 
					{
						continues = strShape == currShape;
						if ( currShape == null ) 
						{
							continuesType = false;
						}
						else 
						{
							continuesType = strShape.XmlAttrs.Name.Equals( currShape.XmlAttrs.Name );
						}
					}
					// if the current shape is not the same as the shape the string is from,
					// see if we've seen this shape before
					returns = !continues && seenShapes.Contains(strShape);
					currShape = strShape;
					if (currShape != null && !seenShapes.Contains(currShape))
					{
						seenShapes.Add(currShape);
					}
					ulong time = 0;
					if (last != null) 
					{
						if (stroke.Points[0].Time > last.Points[last.Length-1].Time) 
						{
							// This means that these two strokes have the SAME timestamp for some reason.  There might be one or two in a file
							// but if there are too many, be suspicious
							time = stroke.Points[0].Time - last.Points[last.Length-1].Time;								
						}
					}
					if (last != null) 
					{
						if (currShape != null) 
						{
							sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], currShape.XmlAttrs.Id, stroke.XmlAttrs.Id, currShape.XmlAttrs.Name, time, continues, continuesType, returns);
						}
						else 
						{
							sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], 0, stroke.XmlAttrs.Id, "None", time, continues, continuesType, returns);
						}
					}
					else 
					{
						if (currShape != null) 
						{
							sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], currShape.XmlAttrs.Id, stroke.XmlAttrs.Id, currShape.XmlAttrs.Name, -1, continues, continuesType, returns);
						}
						else 
						{
							sw.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}", s.XmlAttrs.Id, userIds[s.XmlAttrs.Id], 0, stroke.XmlAttrs.Id, "None", -1, continues, continuesType, returns);
						}
					}
					last = stroke;
				}
			}

			sw.Close();
		}

		/// <summary>
		/// Calculate some global statistics for a set of sketches
		/// </summary>
		/// <param name="statistics">The file to write the stats to</param>
		public void globalStats( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			sw.WriteLine("SketchID,UserID,NumStrokes,NumShapes");
			foreach (Sketch.Sketch sk in sketches)
			{	
				sw.WriteLine("{0},{1},{2},{3}", sk.XmlAttrs.Id, userIds[sk.XmlAttrs.Id], sk.Substrokes.Length, sk.Shapes.Length);
			}
			sw.Close();
		}

		/// <summary>
		/// Writes the number of strokes per shape in a file
		/// SketchID SketcherID ShapeID ShapeType NumStrokes
		/// Note: For now we assume strokes are substrokes
		/// </summary>
		public void strokesPerShape( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			sw.WriteLine( "SketchID UserID ShapeID ShapeName NumStrokesInShape" );
			foreach (Sketch.Sketch sk in sketches)
			{
				foreach (Shape sh in sk.Shapes)
				{
					sw.WriteLine("{0} {1} {2} {3} {4}", sk.XmlAttrs.Id, userIds[sk.XmlAttrs.Id], sh.XmlAttrs.Id, sh.XmlAttrs.Name, sh.Substrokes.Length);
				}
			}
			sw.Close();
		}


		/// <summary>
		/// Count the number of non-label shapes before and after the labels in the sketch
		/// (i.e. where are labels drawn in the sketching process.)  But I don't know that this is really what 
		/// we want to do...
		/// </summary>
		/// <param name="statistics"></param>
		public void labelOrder( string statistics )
		{
			StreamWriter sw = File.CreateText(statistics);
			foreach (Sketch.Sketch sk in sketches)
			{
				int index = 0;
				foreach (Shape sh in sk.Shapes)
				{
					// Not yet finished.
				}
			}
		}

		private Shape findShape( Sketch.Sketch sk, Substroke sub )
		{
			Shape ret = null;
			foreach (Shape sh in sk.Shapes)
			{
				foreach (Substroke ss in sh.Substrokes)
				{
					if (ss.Equals(sub))
					{
						return sh;
					}
				}
			}
			
			return ret;
		}
	}
}
