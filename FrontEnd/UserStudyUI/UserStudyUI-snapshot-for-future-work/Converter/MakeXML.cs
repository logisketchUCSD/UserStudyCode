/**
 * File: MakeXml.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;

namespace Converter
{
	/// <summary>
	/// MakeXml is the class that provides the interface for creating the XML document.
	/// </summary>
	public class MakeXML
	{
		#region INTERNALS

		/// <summary>
		/// The Sketch
		/// </summary>
		private Sketch.Sketch sketch;

		#endregion

		#region CONSTRUCTORS

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sketch">Create MakeXML from sketch</param>
		public MakeXML(Sketch.Sketch sketch)
		{
			this.sketch = sketch;
		}

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="makeXML">Create a MakeXML from a copy of an existing one</param>
		public MakeXML(MakeXML makeXML) : 
			this(makeXML.sketch.Clone())
		{
			//Calls the main constructor
		}

		#endregion

		#region GET XML AS STRING

		/// <summary>
		/// Returns a string containing the XML created by this MakeXML.
		/// </summary>
		/// <returns>the XML string</returns>
		public string getXMLstr()
		{
			// Create output string for xml
			StringWriter strWriter = new StringWriter();
			WriteXML(new XmlTextWriter(strWriter));

			return strWriter.ToString();
		}

		#endregion

		#region WRITE XML
		
		/// <summary>
		/// Write the XML document with the given filename.
		/// </summary>
		/// <param name="filename">Name of the document to create</param>
		public void WriteXML(string filename)
		{
			// Create filename for the XML document
			XmlTextWriter xmlDocument = new XmlTextWriter(filename, System.Text.Encoding.UTF8);

			WriteXML(xmlDocument);
		}

		/// <summary>
		/// Write the XML to the given XmlTextWriter
		/// </summary>
		/// <param name="textWriter">the target XmlTextWriter, which could be a file or string</param>
		private void WriteXML(XmlTextWriter textWriter)
		{
			// Use indentation
			textWriter.Formatting = System.Xml.Formatting.Indented;

			// Create the XML document
			textWriter.WriteStartDocument();

			MakeXML.WriteSketch(this.sketch, textWriter);
			
			textWriter.WriteEndDocument();
			
			// Close the XML document
			textWriter.Close();
		}


	
		private static void WriteSketch(Sketch.Sketch sketch, XmlTextWriter xmlDocument)
		{
			string[] sketchAttributeNames = sketch.XmlAttrs.getAttributeNames();
			object[] sketchAttributeValues = sketch.XmlAttrs.getAttributeValues();

			Sketch.Point[] points = sketch.Points;
			Sketch.Shape[] shapes = sketch.Shapes;
			Sketch.Stroke[] strokes = sketch.Strokes;
			Sketch.Substroke[] substrokes = sketch.Substrokes;
			
			int length;
			int i;

			xmlDocument.WriteStartElement("sketch");
			
			// Write all the attributes
			length = sketchAttributeNames.Length;
			for (i = 0; i < length; ++i)
				if (sketchAttributeValues[i] != null)
					xmlDocument.WriteAttributeString(sketchAttributeNames[i], sketchAttributeValues[i].ToString());

			// Write all the points
			length = points.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WritePoint(points[i], xmlDocument);

			// Write all the substrokes
			length = substrokes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteSubstroke(substrokes[i], xmlDocument);
			
			// Write all the strokes
			length = strokes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteStroke(strokes[i], xmlDocument);
			
			// Write all the shapes
			length = shapes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteShape(shapes[i], xmlDocument);			
		
			xmlDocument.WriteEndElement();
		}

		#region WRITE POINT

		private static void WritePointReference(Sketch.Point point, XmlTextWriter xmlDocument)
		{
			xmlDocument.WriteStartElement("arg");

			xmlDocument.WriteAttributeString("type", "point");
			xmlDocument.WriteString(point.XmlAttrs.Id.ToString());

			xmlDocument.WriteEndElement();
		}

		
		private static void WritePoint(Sketch.Point point, XmlTextWriter xmlDocument)
		{
			string[] pointAttributeNames = point.XmlAttrs.getAttributeNames();
			object[] pointAttributeValues = point.XmlAttrs.getAttributeValues();
			int length;
			int i;

			xmlDocument.WriteStartElement("point");

			// Write all the attributes
			length = pointAttributeNames.Length;
			for (i = 0; i < length; ++i)
				if (pointAttributeValues[i] != null)
					xmlDocument.WriteAttributeString(pointAttributeNames[i], pointAttributeValues[i].ToString());

			xmlDocument.WriteEndElement();
		}

		
		#endregion	
		
		#region WRITE SUBSTROKE

		private static void WriteSubstrokeReference(Sketch.Substroke substroke, XmlTextWriter xmlDocument)
		{
			xmlDocument.WriteStartElement("arg");

			xmlDocument.WriteAttributeString("type", "substroke");
			xmlDocument.WriteString(substroke.XmlAttrs.Id.ToString());

			xmlDocument.WriteEndElement();
		}

		
		private static void WriteSubstroke(Sketch.Substroke substroke, XmlTextWriter xmlDocument)
		{
			string[] substrokeAttributeNames = substroke.XmlAttrs.getAttributeNames();
			object[] substrokeAttributeValues = substroke.XmlAttrs.getAttributeValues();

			Sketch.Point[] points = substroke.Points;
			int length;
			int i;

			xmlDocument.WriteStartElement("shape");

			// Write all the attributes
			length = substrokeAttributeNames.Length;
			for (i = 0; i < length; ++i)
				if (substrokeAttributeValues[i] != null)
					xmlDocument.WriteAttributeString(substrokeAttributeNames[i], substrokeAttributeValues[i].ToString());

			// Write the point references
			length = points.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WritePointReference(points[i], xmlDocument);

			xmlDocument.WriteEndElement();
		}

		#endregion
		
		#region WRITE STROKE

		private static void WriteStrokeReference(Sketch.Stroke stroke, XmlTextWriter xmlDocument)
		{
			xmlDocument.WriteStartElement("arg");

			xmlDocument.WriteAttributeString("type", "stroke");//stroke.XmlAttrs.Type
			xmlDocument.WriteString(stroke.XmlAttrs.Id.ToString());

			xmlDocument.WriteEndElement();			
		}

		
		private static void WriteStroke(Sketch.Stroke stroke, XmlTextWriter xmlDocument)
		{
			string[] strokeAttributeNames = stroke.XmlAttrs.getAttributeNames();
			object[] strokeAttributeValues = stroke.XmlAttrs.getAttributeValues();

			Sketch.Substroke[] substrokes = stroke.Substrokes;
			int length;
			int i;

			xmlDocument.WriteStartElement("shape");

			// Write all the attributes
			length = strokeAttributeNames.Length;
			for (i = 0; i < length; ++i)
				if (strokeAttributeValues[i] != null)
					xmlDocument.WriteAttributeString(strokeAttributeNames[i], strokeAttributeValues[i].ToString());

			// Write the substroke references
			length = substrokes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteSubstrokeReference(substrokes[i], xmlDocument);
			
			xmlDocument.WriteEndElement();
		}
		

		#endregion

		#region WRITE SHAPE
		
		private static void WriteShapeReference(Sketch.Shape shape, XmlTextWriter xmlDocument)
		{
			xmlDocument.WriteStartElement("arg");

			xmlDocument.WriteAttributeString("type", "shape");
			xmlDocument.WriteString(shape.XmlAttrs.Id.ToString());

			xmlDocument.WriteEndElement();
		}
		

		private static void WriteShape(Sketch.Shape shape, XmlTextWriter xmlDocument)
		{
			string[] shapeAttributeNames = shape.XmlAttrs.getAttributeNames();
			object[] shapeAttributeValues = shape.XmlAttrs.getAttributeValues();

			Sketch.Shape[] shapes = shape.Shapes;
			Sketch.Substroke[] substrokes = shape.Substrokes;
			int length;
			int i;

			xmlDocument.WriteStartElement("shape");

			// Write all the attributes
			length = shapeAttributeNames.Length;
			for(i = 0; i < length; ++i)
				if (shapeAttributeValues[i] != null)
					xmlDocument.WriteAttributeString(shapeAttributeNames[i], shapeAttributeValues[i].ToString());

			// Write all the shapes args
			length = shapes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteShapeReference(shapes[i], xmlDocument);

			// Write all the substrokes args
			length = substrokes.Length;
			for (i = 0; i < length; ++i)
				MakeXML.WriteSubstrokeReference(substrokes[i], xmlDocument);

			xmlDocument.WriteEndElement();
		}
		
		#endregion
		
		#endregion

		#region OTHER

		/// <summary>
		/// Create a copy of this
		/// </summary>
		/// <returns>A new MakeXML</returns>
		public MakeXML Clone()
		{
			return new MakeXML(this);
		}

		#endregion
	}
}
