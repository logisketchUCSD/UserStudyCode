using System;
using System.Collections;
using System.Drawing;
using Microsoft.Ink;

namespace Converter
{
	/// <summary>
	/// Summary description for MakeInk.
	/// </summary>
	public class MakeInk
	{
		#region PRIVATE DATA

		private Ink ink;
		
		private Hashtable idToCStroke;
		private Hashtable idToMStroke;
		private Hashtable guidToMStroke;
		private Hashtable idToMSubstrokes;

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Converts the XML format into Ink strokes and creates a mapping between the XML stroke Ids and the
		/// Ink stroke Ids.
		/// </summary>
		public MakeInk(MakeXML xmlHolder)
		{			
			this.ink = new Ink();
			this.idToCStroke = this.getIdToCStroke(xmlHolder.getSketch(), this.ink);
			calculateIdToMStroke();
			this.idToMSubstrokes = new Hashtable();
		}


		/// <summary>
		/// Converts the XML format into Ink strokes and creates a mapping between the XML stroke Ids and the
		/// Ink stroke Ids.
		/// </summary>
		public MakeInk(MakeXML xmlHolder, Ink ink)
		{			
			this.ink = ink;
			this.idToCStroke = this.getIdToCStroke(xmlHolder.getSketch(), this.ink); 
			calculateIdToMStroke();
			this.idToMSubstrokes = new Hashtable();
		}

		/// <summary>
		/// Converts the XML format into Ink strokes and creates a mapping between the XML stroke Ids and the
		/// Ink stroke Ids.
		/// </summary>
		public MakeInk(MakeXML xmlHolder, Ink ink, Shape.ShapeType type)
		{			
			this.ink = ink;
			this.idToCStroke = this.getIdToCStroke(xmlHolder.getSketch(), this.ink); 
			calculateIdToMStroke();
			this.idToMSubstrokes = new Hashtable();
			addColor(xmlHolder, ink, type);
		}


		#endregion

		/// <summary>
		/// Creates the strokes in this xml document as C# strokes and adds them to the corresponding Ink object.
		/// We cannot create new strokes with the same Id's that we have in the MIT XML format, so instead we
		/// return a hash table mapping the created strokes' Ids (1, 2, ... , n) with the strokes.
		/// </summary>
		/// 
		/// <returns>A Hashtable that holds links back to the Converter strokes</returns>
		public Hashtable getIdToCStroke(Sketch sketch, Microsoft.Ink.Ink inkObject)
		{
			ArrayList strokes = sketch.Shapes;
			ArrayList points = sketch.Points;

			Hashtable retTable = new Hashtable();

			// Create a hashtable between points and IDs for easy access
			Hashtable pointTable = sketch.IdToPoint;	

			// Now create the C# strokes
			foreach (Shape s in strokes)
			{
				Stroke st = new Stroke(s);
				
				// first check to be sure it's a matching shape
				if(st.matches(Stroke.ShapeType.STROKE)) 
				{
					// Get all of the stroke's points
					ArrayList pts = s.getArgIds();
					
					System.Drawing.Point[] xypts = new System.Drawing.Point[pts.Count];
					//int[] pressureData = new int[pts.Count];
					//int[] timerData    = new int[pts.Count];

					int index = 0;
					//Console.WriteLine( "loading stroke...");
					foreach (string id in pts)
					{
						Point p = (Point)pointTable[id];
						st.Points.Add(p);
						st.Args.Add(new Stroke.Arg("Point", p.Id));

						// Just get the x and the y, ignore the rest
						int x = Convert.ToInt32(p.X);
						int y = Convert.ToInt32(p.Y);
						
						//Console.WriteLine( "[x: {0}, y: {1}]", x, y );
						xypts[index] = new System.Drawing.Point(x, y);
												
						//pressureData[index] = (int)p.Pressure;
						//timerData[index]    = (int)p.Time;

						++index;
					}
					
					Microsoft.Ink.Stroke newStroke = inkObject.CreateStroke(xypts);
				
					/*
					// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dntablet/html/TimeStampsBestPractices.asp
					// mike used: 8fe68ce5-c39c-4f52-afbb-65344e8e49ff
					newstroke.ExtendedProperties.Add(new Guid("8A54CF58-97E6-4fc5-8F06-F8BAD2E19B22"), (long)s.Time);
					
					// newstroke.SetPacketValuesByProperty(Microsoft.Ink.PacketProperty.NormalPressure, pressureData);
					newstroke.SetPacketValuesByProperty(Microsoft.Ink.PacketProperty.TimerTick, timerData);
					*/

					retTable.Add(newStroke.Id, st);
				}
			}

			return retTable;
		}
	

		#region STROKE SPLITTING

		/// <summary>
		/// Split both a Microsoft and Converter stroke at indices and add them to the docmunte
		/// </summary>
		/// <param name="id">The id of the Microsoft and Converter stroke</param>
		/// <param name="indices">The indices to split at</param>
		/// <param name="document">The document to add to</param>
		public void splitStrokes(int id, float[] indices, MakeXML document)
		{
			Microsoft.Ink.Stroke mStroke = this.getMStrokeById(id);
			Stroke cStroke = (Stroke)this.idToCStroke[id];
			this.splitStrokes(mStroke, cStroke, indices, document);			
		}


		/// <summary>
		/// Break up the Microsoft stroke, and associated Converter stroke, at the given indices and add it to the document
		/// </summary>
		/// <param name="mStroke">The Microsoft Stroke</param>
		/// <param name="indices">The indices to split at</param>
		/// <param name="document">The document to add to</param>
		public void splitStrokes(Microsoft.Ink.Stroke mStroke, float[] indices, MakeXML document)
		{
			this.splitStrokes(mStroke.Id, indices, document);
		}


		/// <summary>
		/// Split both a Microsoft and Converter stroke
		/// </summary>
		/// <param name="mStroke">The Microsoft stroke</param>
		/// <param name="cStroke">The Converter stroke</param>
		/// <param name="indices">The indices to split at</param>
		/// <param name="document">The document</param>
		private void splitStrokes(Microsoft.Ink.Stroke mStroke, Stroke cStroke, float[] indices, MakeXML document)
		{
			int id = mStroke.Id;


			//We can only split two of the same strokes... they must be in the hashtable as such
			if(this.idToCStroke.ContainsKey(id) && this.getMStrokeById(id) != null) 
			{
				//Remove the stroke from the hashtable (this may work below the splitting up code)
				//this.mStrokeToCStroke.Remove(mStroke);
				
				this.idToCStroke.Remove(id);

				//Get the corresponding splitted strokes
				//The should line up
				Microsoft.Ink.Stroke[] mStrokes = splitMStroke(mStroke, indices);
				Stroke[] cStrokes = splitCStroke(cStroke, indices);

				//Create the updated mapping
				for(int i = 0; i < mStrokes.Length; ++i)
				{
					int sId = mStrokes[i].Id;
					this.idToCStroke.Add(sId, cStrokes[i]);
					document.addShape(cStrokes[i]);
				}
			}
			else
			{
				object i = "test";
				int b = (int)i;
				//Error halt here
			}
		}


		/// <summary>
		/// Given a Microsoft Stroke id, this will split the stroke into indices.Length + 1 strokes.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="indices"></param>
		private Microsoft.Ink.Stroke[] splitMStroke(int id, float[] indices)
		{		
			return splitMStroke(getMStrokeById(id), indices);	
		}


		/// <summary>
		/// Given a Microsoft Stroke, this will split the stroke into indices.Length + 1 strokes.
		/// </summary>
		/// <param name="toSplit">Stroke to split</param>
		/// <param name="indices">Indices to split at</param>
		private Microsoft.Ink.Stroke[] splitMStroke(Microsoft.Ink.Stroke toSplit, float[] indices)
		{
			//Sort the indices to split
			Array.Sort(indices);
			
			//Reverse them
			Array.Reverse(indices);


			//We will walk along, and break up one stroke each time
			Microsoft.Ink.Stroke secondStroke;

			//Here is our array that we will add to the hashtable later
			Microsoft.Ink.Stroke[] strokes = new Microsoft.Ink.Stroke[indices.Length + 1];

			//Here is what will happen... imagine this in the stroke:
			//
			//                  5            4              3           2               1
			// ==========================================================================================
			// 0              25.2          50.7           82.3        90.1          111.2     stroke.Count
			//
			//It will make the cuts in reverse order according to the indices, highest indices first
			//Then it adds each stroke to the array, following into the hashtable

			int current = 0;
			foreach(float index in indices)
			{				
				//Split up the stroke
				secondStroke = toSplit.Split(index);
				
				//Add the latter one
				strokes[current++] = secondStroke;
			}

			//Dont forget the very first stroke
			strokes[current] = toSplit;
		

			int id = toSplit.Id; 

			//Now add it to idToSubstrokes
			if (this.idToMSubstrokes.ContainsKey(id))
			{
				this.idToMSubstrokes[id] = strokes;	
			}
			else
			{
				this.idToMSubstrokes.Add(id, strokes);
				//idToMicroSubstrokes.Add(id, strokes);
			}

			return strokes;
		}

	
		/*private ArrayList combineArrays(ArrayList one, ArrayList two)
		{
			if(one == null)
				return two;
			else if(two == null)
				return one;
			else
			{				
				ArrayList oneCopy = (ArrayList)one.Clone();
				ArrayList twoCopy = (ArrayList)two.Clone();
			
				foreach(object o in twoCopy)
					oneCopy.Add(o);

				return oneCopy;
			}
		}*/


		/// <summary>
		/// Get the microsoft stroke by the specific id.
		/// </summary>
		/// <param name="id">The id of the stroke</param>
		/// <returns>The stroke with the desired id</returns>
		public Microsoft.Ink.Stroke getMStrokeById(int id)
		{
			return (Microsoft.Ink.Stroke)this.idToMStroke[id];
		}

		
		/// <summary>
		/// Creates the IdToMStroke hashtable
		/// </summary>
		public void calculateIdToMStroke()
		{
			this.idToMStroke = new Hashtable();
			foreach(Microsoft.Ink.Stroke stroke in this.ink.Strokes)
				this.idToMStroke.Add(stroke.Id, stroke);
			calculateGuidToMStroke();
		}
		
		private void calculateGuidToMStroke()
		{
			this.guidToMStroke = new Hashtable();
			IDictionaryEnumerator enumerator = this.idToCStroke.GetEnumerator();
			while(enumerator.MoveNext())
			{
				int id = (int)enumerator.Key;
				Microsoft.Ink.Stroke mStroke = this.getMStrokeById(id);
				string guid = ((Stroke)enumerator.Value).Id;
				this.guidToMStroke.Add(guid, mStroke);
			}
		}
		
		/// <summary>
		/// Split up the stroke
		/// </summary>
		/// <param name="id">The id of the stroke to be split up</param>
		/// <param name="indices">The indices of the split points</param>
		/// <returns>The substroke array from splitting</returns>
		private Stroke[] splitCStroke(int id, float[] indices)
		{
			return splitCStroke((Stroke)idToCStroke[id], indices);
		}
	

		/// <summary>
		/// Take a stroke and split it at the indicated indices
		/// </summary>
		/// <param name="toSplit">The stroke to split</param>
		/// <param name="indices">The indices to split at</param>
		/// <returns>Array of substrokes</returns>
		private Stroke[] splitCStroke(Stroke toSplit, float[] indices)
		{
			//Sort the indices to split
			Array.Sort(indices);
			
			//Reverse them
			Array.Reverse(indices);


			//We will walk along, and break up one stroke each time
			Stroke secondStroke;

			//Here is our array that we will return the Stroke[]
			Stroke[] strokes = new Stroke[indices.Length + 1];

			//Here is what will happen... imagine this in the stroke:
			//
			//                  5            4              3           2               1
			// ==========================================================================================
			// 0              25.2          50.7           82.3        90.1          111.2     stroke.Count
			//
			//It will make the cuts in reverse order according to the indices, highest indices first
			//Then it adds each stroke to the array, following into the hashtable

			int current = 0;
			foreach(float index in indices)
			{				
				//Split up the stroke
				secondStroke = splitCStroke(toSplit, index);
				
				//Add the latter one
				strokes[current++] = secondStroke;
			}

			//Dont forget the very first stroke
			strokes[current] = toSplit;

			return strokes;
		}


		/// <summary>
		/// This works like the Microsoft Stroke.Split.
		/// </summary>
		/// <param name="toSplit">Stroke to split</param>
		/// <param name="index">Index to split at</param>
		/// <returns>The latter part of the stroke</returns>
		private Stroke splitCStroke(Stroke toSplit, float index)
		{
			//Our stroke to return... it is the part after the index
			Stroke toReturn = new Stroke(System.Guid.NewGuid().ToString(), "Substroke", toSplit.Time, "Substroke");
			toReturn.addFeatures(toSplit);
			toReturn.SubstrokeOf = toSplit.Id;
			//toSplit.addFeatures(toSplit);
			//toSplit.SubstrokeOf = toSplit.Id;

			//We can't have a float index
			int realIndex = (int)(index + 0.5f);
			
			//Make the value smaller if it is too big
			if(realIndex + 1 > toSplit.Points.Count)
				realIndex = toSplit.Points.Count - 1;

			int count = toSplit.Points.Count;

			//Add Points, Args to toReturn
			for(int i = realIndex; i < count; ++i)
			{
				toReturn.Points.Add(toSplit.Points[i]);
				toReturn.Args.Add(toSplit.Args[i]);
			}

            //Remove the Points and Args from toSplit
			int sizeToRemove = count - realIndex - 1;
			
			toSplit.Points.RemoveRange(realIndex + 1, sizeToRemove);
			toSplit.Args.RemoveRange(realIndex + 1, sizeToRemove);
			
			toSplit.Name = "Substroke";
			toSplit.Type = "Substroke";
			toSplit.SubstrokeOf = toSplit.Id;
			toSplit.Id = System.Guid.NewGuid().ToString();
		
			
			
            return toReturn;
		}

		
		#endregion
		
		/// <summary>
		/// Create the xml document
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="document"></param>
		public void writeXML(string filename, MakeXML document)
		{
			document.writeXML(filename);			
		}


		/// <summary>
		/// Adds the necessary substrokes to the documnent and writes it out.
		/// </summary>
		/// <param name="document"></param>
		/// <param name="idToIndices"></param>
		public void createSubstrokes(MakeXML document, Hashtable idToIndices)
		{
			IDictionaryEnumerator Enumerator = idToMSubstrokes.GetEnumerator();
			
			//Iterate through each Stroke that will be subdivided
			while (Enumerator.MoveNext())
			{
				//Get the id of the Stroke... we use it for hashtable lookups
				int id = (int)Enumerator.Key;
				Microsoft.Ink.Stroke[] substrokes = (Microsoft.Ink.Stroke[])Enumerator.Value;

				//Here is the stroke we want to break up
				Stroke xmlStroke = (Stroke)idToCStroke[id];

				//Here are the indices we will use to break it up
				float[] indices = (float[])((ArrayList)idToIndices[id]).ToArray(typeof(float));
						
				//Break up the stroke into multiple substrokes
				Stroke[] xmlSubstrokes = breakXmlStroke(xmlStroke, indices);

				//Add the substrokes into the xml document
				foreach(Stroke s in xmlSubstrokes)
					document.addShape(s);	
			
				
				//TEST... MIGHT NEED TO REMOVE THIS...
				//idToIndices.Remove(id);

				//ADD REST
				this.idToCStroke.Remove(id);
				for(int i = 0; i < substrokes.Length; ++i)
				{
					this.idToCStroke.Add(substrokes[i].Id, xmlSubstrokes[i]);
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="document"></param>
		/// <param name="strokes"></param>
		/// <param name="label"></param>
		public void createShapeWithLabel(MakeXML document, Microsoft.Ink.Strokes strokes, string label)
		{
			//Create the new shape we will add
			Shape toAdd = new Shape(System.Guid.NewGuid().ToString(), label, "", label);
			
			//Extra information for the shape
			toAdd.Source = "LabelerSession" + this.GetHashCode().ToString();
			toAdd.Width = strokes.GetBoundingBox().Width.ToString();
			toAdd.Height = strokes.GetBoundingBox().Height.ToString();
			toAdd.X = strokes.GetBoundingBox().X.ToString();
			toAdd.Y = strokes.GetBoundingBox().Y.ToString();
			

			ulong currentTime;
			ulong bestTime = 0;

			foreach(Microsoft.Ink.Stroke stroke in strokes)
			{
				int id = stroke.Id;

				Stroke s = (Stroke)idToCStroke[id];

				Stroke.Arg arg = new Stroke.Arg(s.Type, s.Id);

				toAdd.Args.Add(arg);

				currentTime = Convert.ToUInt64(s.Time);
				if(currentTime > bestTime)
					bestTime = currentTime;				
			}

			toAdd.Time = bestTime.ToString();

			document.addShape(toAdd);
		}

		
		/// <summary>
		/// Break the stroke into substrokes at the indicated indices
		/// </summary>
		/// <param name="stroke"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public Stroke[] breakXmlStroke(Stroke stroke, float[] indices)
		{
			//We need one more stroke than there are cut indices
			Stroke[] strokes = new Stroke[indices.Length + 1];

			int[] intIndices = new int[indices.Length];
			for(int i = 0; i < indices.Length; ++i)
				intIndices[i] = (int)(indices[i] + 0.5f);

			Array.Sort(intIndices);
			
			if(intIndices[intIndices.Length - 1] > stroke.Points.Count - 1)
			{
				Console.Error.WriteLine("bad indices...");
			}


			int previousIndex = 0;
			for(int i = 0; i < intIndices.Length + 1; ++i)
			{				
				int index = i < intIndices.Length ? intIndices[i] : stroke.Points.Count - 1;

				strokes[i] = new Stroke(
					System.Guid.NewGuid().ToString(), 
					"Substroke", 
					((Point)stroke.Points[index]).Time.ToString(), //Time of the last point in it
					"Substroke");

				strokes[i].addFeatures(stroke);
				strokes[i].SubstrokeOf = stroke.Id;

				//Should we have <= index, or < index?  Should we include pts more than once?
				for(int j = previousIndex; j <= index; ++j)
				{
					strokes[i].Points.Add(stroke.Points[j]);
					strokes[i].Args.Add(new Stroke.Arg("Point", ((Point)stroke.Points[j]).Id));
				}

				previousIndex = index;				
			}

			return strokes;

		}
		

		/// <summary>
		/// Returns the Ink associated with the MakeInk object
		/// </summary>
		/// <returns>Ink holding the strokes representation of the XML shapes</returns>
		public Ink GetInk() 
		{
			return ink;
		}


		private void addColor(MakeXML xmlHolder, Ink ink, Shape.ShapeType type)
		{
			Shape[] shapes = new Shape[0];
			switch(type)
			{
				case(Shape.ShapeType.LABELED):
					shapes = xmlHolder.getSketch().Labeled;
					break;
				case(Shape.ShapeType.CLUSTERED):
					shapes = xmlHolder.getSketch().Clustered;
					break;
			}

			foreach(Shape shape in shapes)
			{
				string name = shape.Name;
				Color c = getRandomColor();
				foreach(Shape.Arg arg in shape.Args)
				{
					string guid = arg.Id;
					Shape toColor = (Shape)xmlHolder.getSketch().IdToShape[guid];
					if(toColor != null)
					{
						string guid2 = toColor.Id;

						if(guidToMStroke.ContainsKey(guid2))
							((Microsoft.Ink.Stroke)guidToMStroke[guid2]).DrawingAttributes.Color = c;
					}
				}
			}
		}

		static int currentColor = 10;
		
		private Color getRandomColor()
		{
			++currentColor;
			Color[] colors = new Color[] 
				{ Color.Aqua, Color.Aquamarine, Color.Azure, Color.Beige, Color.Bisque, Color.Black, Color.BlanchedAlmond,
					Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood, Color.CadetBlue, Color.Chartreuse, Color.Chocolate, Color.Coral, Color.CornflowerBlue,
					Color.Cornsilk, Color.Crimson, Color.Cyan, Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki,
					Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue, 
					Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue, Color.DimGray, Color.DodgerBlue, Color.Firebrick,
					Color.FloralWhite, Color.ForestGreen, Color.Fuchsia, Color.Gainsboro, Color.Gold, Color.Goldenrod, Color.Gray, Color.Green, Color.GreenYellow, 
					Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, Color.Khaki, Color.Lavender, Color.LavenderBlush, Color.LawnGreen, Color.LemonChiffon,
					Color.Lime, Color.LimeGreen, Color.Linen, 
					Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, Color.MediumBlue, Color.MediumOrchid, Color.MediumPurple, Color.MediumSeaGreen, 
					Color.MediumSlateBlue, Color.MediumSpringGreen, Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, Color.MintCream, Color.MistyRose, 
			        Color.Moccasin, Color.Navy, Color.OldLace, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed, Color.Orchid, Color.PaleGoldenrod, Color.PaleGreen,
					Color.PaleTurquoise, Color.PaleVioletRed, Color.PapayaWhip, Color.PeachPuff, Color.Peru, Color.Pink, Color.Plum, Color.PowderBlue, Color.Purple, Color.Red,
					Color.RosyBrown, Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen, Color.SeaShell, Color.Sienna, Color.Silver, Color.SkyBlue,
					Color.SlateBlue, Color.SlateGray, Color.SpringGreen, Color.SteelBlue, Color.Tan, Color.Teal, Color.Thistle, Color.Tomato, Color.Turquoise, Color.Violet,
					Color.Wheat, Color.WhiteSmoke, Color.Yellow, Color.YellowGreen};
			
			return colors[currentColor % colors.Length];
		}
		
		/// <summary>
		/// Returns the Ink associated with the MakeInk object
		/// </summary>
		public Ink INK
		{
			get
			{
				return ink;
			}
		}


		/// <summary>
		/// Mapping from Micro Stroke id's to corresponding converter stroke
		/// </summary>
		public Hashtable IdToCStroke
		{
			get
			{
				return this.idToCStroke;
			}
		}

		
		/// <summary>
		/// Write this...
		/// </summary>
		public Hashtable IdToMStroke
		{
			get
			{
				return this.idToMStroke;
			}
		}

		
		/// <summary>
		/// Get idToMicroSubstrokes hashtable;
		/// </summary>
		public Hashtable IdToMSubstrokes
		{
			get
			{
				return idToMSubstrokes;
			}
		}
	}
}
