/**
 * File: XmlStructs.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;

namespace Sketch
{
	/// <summary>
	/// XmlStructs. Contains structs holding variables for relevant XML attributes.
	/// </summary>
	public class XmlStructs
	{
		/// <summary>
		/// XML attribute struct for the Sketch data
		/// </summary>
		public struct XmlSketchAttrs
		{
			#region INTERNALS

			/// <summary>
			/// GUID of the Sketch.
			/// </summary>
			public object Id;

			/// <summary>
			/// Units of the Sketch.
			/// </summary>
			/// <example>Units = "Himetric"</example>
			public object Units;

			#endregion

			#region METHODS

			/// <summary>
			/// Get the attribute names.
			/// </summary>
			/// <returns>A string array of attribute names</returns>
			public string[] getAttributeNames()
			{
				return new string[] { "id", "units" };
			}


			/// <summary>
			/// Get the attribute values.
			/// </summary>
			/// <returns>An object array of attribute values</returns>
			public object[] getAttributeValues()
			{
				return new object[] { Id, Units };
			}

			
			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlSketchAttrs struct copying the old one's values</returns>
			public XmlSketchAttrs Clone()
			{
				XmlSketchAttrs temp = new XmlSketchAttrs();
				temp.Id = this.Id;
				temp.Units = this.Units;
			
				return temp;
			}
			
			#endregion
		}

		
		/// <summary>
		/// XML attribute struct for the Sketcher data
		/// </summary>
		public struct XmlSketcherAttrs
		{
			#region INTERNALS

			/// <summary>
			/// GUID of the Sketcher.
			/// </summary>
			public object Id;

			/// <summary>
			/// The DPI for the x-axis.
			/// </summary>
			public object XDpi;
			
			/// <summary>
			/// The DPI for the y-axis.
			/// </summary>
			public object YDpi;

			/// <summary>
			/// The nickname of the Sketcher.
			/// </summary>
			public object Nickname;

			#endregion

			#region METHODS
			
			/// <summary>
			/// Get the attribute names.
			/// </summary>
			/// <returns>A string array of attribute names</returns>
			public string[] getAttributeNames()
			{
				return new string[] { "id", "nickname" };
			}


			/// <summary>
			/// Get the attribute values.
			/// </summary>
			/// <returns>An object array of attribute values</returns>
			public object[] getAttributeValues()
			{
				return new object[] { Id, Nickname };
			}


			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlSketcherAttrs struct copying the old one's values</returns>
			public XmlSketcherAttrs Clone()
			{
				XmlSketcherAttrs temp = new XmlSketcherAttrs();
				temp.Id = this.Id;
				temp.XDpi = this.XDpi;
				temp.YDpi = this.YDpi;
				temp.Nickname = this.Nickname;

				return temp;
			}
			
			#endregion
		}

		
		/// <summary>
		/// XML attribute struct for the Study data
		/// </summary>
		public struct XmlStudyAttrs
		{
			#region INTERNALS

			/// <summary>
			/// Name of the study.
			/// </summary>
			public object Name;

			#endregion

			#region METHODS

			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlStudyAttrs struct copying the old one's values</returns>
			public XmlStudyAttrs Clone()
			{
				XmlStudyAttrs temp = new XmlStudyAttrs();
				temp.Name = this.Name;

				return temp;
			}
			
			#endregion
		}

		
		/// <summary>
		/// XML attribute struct for the Domain data
		/// </summary>
		public struct XmlDomainAttrs
		{
			#region INTERNALS

			/// <summary>
			/// Name of the domain.
			/// </summary>
			public object Name;

			#endregion

			#region METHODS

			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlDomainAttrs struct copying the old one's values</returns>
			public XmlDomainAttrs Clone()
			{
				XmlDomainAttrs temp = new XmlDomainAttrs();
				temp.Name = this.Name;

				return temp;
			}
			
			#endregion
		}

		
		/// <summary>
		/// XML attribute struct for the Shape data
		/// </summary>
		public class XmlShapeAttrs
		{
			#region INTERNALS

			/// <summary>
			/// GUID of the Shape.
			/// </summary>
			public object Id;

			/// <summary>
			/// Name of the Shape.
			/// </summary>
			public object Name;

			/// <summary>
			/// Ending time of the drawn Shape.
			/// </summary>
			public object Time;

			/// <summary>
			/// Type of the Shape.
			/// </summary>
			public object Type;

			/// <summary>
			/// Shape's author. GUID representation.
			/// </summary>
			public object Author;
			
			/// <summary>
			/// Color of the Shape. RGBA represenation.
			/// </summary>
			public object Color;

			/// <summary>
			/// Height of the Shape.
			/// </summary>
			public object Height;

			/// <summary>
			/// Width of the Shape.
			/// </summary>
			public object Width;

			/// <summary>
			/// Area of the Shape.
			/// </summary>
			public object Area;

			/// <summary>
			/// Boolean for if we should draw the Shape's ink.
			/// </summary>
			public object LaysInk;

			/// <summary>
			/// Pen orientation when the Shape was drawn.
			/// </summary>
			public object Orientation;

			/// <summary>
			/// Pen tip used in drawing the Shape. Either "Rectangle" or "Ball".
			/// </summary>
			public object PenTip;

			/// <summary>
			/// How to Render the Ink. Either "MaskPen" or "CopyPen".
			/// </summary>
			public object Raster;

			/// <summary>
			/// Stores the GUID of the Substroke's parent, if the current Shape
			/// is a Substroke.
			/// </summary>
			public object SubstrokeOf;
			
			/// <summary>
			/// First Point??
			/// </summary>
			public object P1;
			
			/// <summary>
			/// End Point??
			/// </summary>
			public object P2;
			
			/// <summary>
			/// The x-coordinate of the upper left corner of the Shape??
			/// </summary>
			public object X;
			
			/// <summary>
			/// The y-coordinate of the upper left corner of the Shape??
			/// </summary>
			public object Y;
			
			/// <summary>
			/// If the Shape is text, what is the actual text value.
			/// </summary>
			public object Text;
			
			/// <summary>
			/// The x-coordinate of the upper left corner of the Shape.
			/// </summary>
			public object LeftX;
			
			/// <summary>
			/// The y-coordinate of the upper left corner of the Shape.
			/// </summary>
			public object TopY; 
			
			/// <summary>
			/// ??
			/// </summary>
			public object Control1;
			
			/// <summary>
			/// ??
			/// </summary>
			public object Control2;
			
			/// <summary>
			/// The starting argument of the Shape.
			/// </summary>
			public object Start;
			
			/// <summary>
			/// The endign argument of the Shape.
			/// </summary>
			public object End;
			
			/// <summary>
			/// The source that generated this Shape.
			/// </summary>
			public object Source;

			#endregion

			#region METHODS

			/// <summary>
			/// Get the attribute names.
			/// </summary>
			/// <returns>A string array of attribute names</returns>
			public string[] getAttributeNames()
			{
				return new string[] { "type", "name", "id", "time", "x", "y", "author", "color", 
										"height", "width", "area", "laysInk", "orientation", "penTip", "raster", 
										"substrokeOf", "p1", "p2", "text", "leftx", "topy", "control1",
										"control2", "start", "end", "source" };
			}

			
			/// <summary>
			/// Get the attribute values.
			/// </summary>
			/// <returns>An object array of attribute values</returns>
			public object[] getAttributeValues()
			{
				return new object[] {	Type, Name, Id, Time, X, Y, Author, Color,
										Height, Width, Area, LaysInk, Orientation, PenTip, Raster, 
										SubstrokeOf, P1, P2, Text, LeftX, TopY, Control1,
										Control2, Start, End, Source };
			}

			
			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlShapeAttrs struct copying the old one's values</returns>
			public XmlShapeAttrs Clone()
			{
				XmlShapeAttrs temp = new XmlShapeAttrs();
				temp.Id = this.Id;
				temp.Name = this.Name;
				temp.Time = this.Time;
				temp.Type = this.Type;
				temp.Author = this.Author;
				temp.Color = this.Color;
				temp.Height = this.Height;
				temp.Width = this.Width;
				temp.Area = this.Area;
				temp.LaysInk = this.LaysInk;
				temp.Orientation = this.Orientation;
				temp.PenTip = this.PenTip;
				temp.Raster = this.Raster;
				temp.SubstrokeOf = this.SubstrokeOf;
				temp.P1 = this.P1;
				temp.P2 = this.P2;
				temp.X = this.X;
				temp.Y = this.Y;
				temp.Text = this.Text;
				temp.LeftX = this.LeftX;
				temp.TopY = this.TopY;
				temp.Control1 = this.Control1;
				temp.Control2 = this.Control2;
				temp.Start = this.Start;
				temp.End = this.End;
				temp.Source = this.Source;

				return temp;
			}

			
			#endregion
		}

		
		/// <summary>
		/// XML attribute struct for the Point data
		/// </summary>
		public struct XmlPointAttrs
		{
			#region	INTERNALS

			/// <summary>
			/// The x-coordinate of the Point.
			/// </summary>
			public object X;

			/// <summary>
			/// The y-coordinate of the Point.
			/// </summary>
			public object Y;

			/// <summary>
			/// The pen pressure of the Point.
			/// </summary>
			public object Pressure;

			/// <summary>
			/// Time the Point was drawn.
			/// </summary>
			public object Time;

			/// <summary>
			/// Name of the Point.
			/// </summary>
			public object Name;

			/// <summary>
			/// The Point's GUID.
			/// </summary>
			public object Id;

			#endregion

			#region METHODS

			/// <summary>
			/// Get the attribute names.
			/// </summary>
			/// <returns>A string array of attribute names</returns>
			public string[] getAttributeNames()
			{
				return new string[] { "x", "y", "pressure", "time", "name", "id" };
			}

						
			/// <summary>
			/// Get the attribute values.
			/// </summary>
			/// <returns>An object array of attribute values</returns>
			public object[] getAttributeValues()
			{
				return new object[] { X, Y, Pressure, Time, Name, Id };
			}

			
			/// <summary>
			/// Clone method.
			/// </summary>
			/// <returns>A new XmlPointAttrs struct copying the old one's values</returns>
			public XmlPointAttrs Clone()
			{
				XmlPointAttrs temp = new XmlPointAttrs();
				temp.X = this.X;
				temp.Y = this.Y;
				temp.Pressure = this.Pressure;
				temp.Time = this.Time;
				temp.Id = this.Id;
				temp.Name = this.Name;
				
				return temp;
			}
			
			#endregion		
		}
	}
}
