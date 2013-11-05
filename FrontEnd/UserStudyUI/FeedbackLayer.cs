using System;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Ink;
using System.Collections;

using Sketch;
using Labeler;

namespace UserStudyUI
{
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Feedback Layer Framework
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Feedback Layer Framework

	/// <summary>
	/// The context for a call to the feedback mechanism.  The feedback mechanism may
	/// behave differently depending on the context (e.g., for a stroke deletion, the
	/// mechanism may only need to erase graphics associated with the deleted stroke).
	/// </summary>
	public enum FeedbackContext {OnRecognitionResult, OnStrokeAdded, OnStrokeDeleted}
	
	/// <summary>
	/// A FeedbackMechanism applies a visual transformation to a sketch.  A 
	/// feedback mechanism might recolor Ink strokes, replace Ink strokes with symbols,
	/// or label Ink strokes with textual labels.  A FeedbackMechanism is similar to a
	/// Visitor (see Design Patterns).
	/// </summary>
	interface FeedbackMechanism
	{
		/// <summary>
		/// The UI calls this method whenever it needs to either overlay feedback or 
		/// repaint feedback (e.g., after a stroke deletion).
		/// </summary>
		/// <param name="sketch"></param>
		/// <param name="inkPicture"></param>
		/// <param name="ink2sketchIdTable">A hashtable mapping Ink Stroke IDs to Sketch Substroke IDs.  
		/// This hashtable allows the Feedback Mechanism to assocate Ink Strokes drawn to screen
		/// with their corresponding (labeled) Sketch substrokes.</param>
		/// <param name="context"></param>
		void FireFeedbackMechanism(Sketch.Sketch sketch, InkPicture inkPicture, 
			Hashtable ink2sketchIdTable, FeedbackContext context);
	}


	#endregion
	

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Blue-Red Feedback Mechanism
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Blue-Red Feedback Mechanism
	
	/// <summary>
	/// The standard blue-red feedback mechanism.  This mechanism works with the 
	/// wire recognizer to paint recognized strokes as blue and unrecognized strokes
	/// as red.  
	/// <see cref="WireLabelRecognizer"/>
	/// </summary>
	public class WireRecognizerFeedbackMechanism : FeedbackMechanism
	{
		public void FireFeedbackMechanism(Sketch.Sketch sketch, InkPicture inkPicture, Hashtable ink2sketchIdTable, FeedbackContext context)
		{
			// Disable the Ink Picture if it is already enabled
			bool toggleInkPic = inkPicture.Enabled;
			if (toggleInkPic)
				inkPicture.Enabled = false;

			foreach (Microsoft.Ink.Stroke istroke in inkPicture.Ink.Strokes)
			{
				// If this ink stroke does not have a 
				// corresponding sketch stroke, skip it
				if (!ink2sketchIdTable.Contains(istroke.Id))
					continue;

				// Get the label of the corresponding substroke
				Guid substrokeGuid = (Guid) ink2sketchIdTable[istroke.Id];
				Substroke substr = sketch.GetSubstroke(substrokeGuid);
				if (substr != null)
				{
                    string label = substr.FirstLabel;

					// Now color the stroke
					if (label == UIConstants.WireLabel)
					{
						istroke.DrawingAttributes.Color = UIConstants.WireColor;
					}
					else if (label == UIConstants.ErrorLabel)
					{
						istroke.DrawingAttributes.Color = UIConstants.ErrorColor;
					}
				}
				else
				{
					// We should report an error here because
					// there should be a substroke
					// in the sketch if it's referenced in the
					// hashtable
					continue;
				}
			}

			// Re-enable the ink picture if it was enabled.
			if (toggleInkPic)
				inkPicture.Enabled = true;
		}
	}

	#endregion


	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Domain-Aware Feedback Mechanism
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Domain-Aware Feedback Mechanism

	/// <summary>
	/// A domain aware feedback mechanism based upon the Labeler.  This mechanism loads a domain
	/// and transforms strokes based upon what type of symbol (e.g. AND gate, wire, etc.) 
	/// the stroke is part of.  This class serves as a base class for other feedback mechanisms that
	/// use a domain file to define the possible labels for symbols in a sketch.
	/// 
	/// This mechanism works with the wizard recognizer to paint recognized strokes using 
	/// colors defined by the domain file associated with the Labeler.
	/// <see cref="WizardRecognizer"/>
	/// </summary>
	public abstract class DomainAwareFeedbackMechanism : FeedbackMechanism
	{
		/// <summary>
		/// Coloring information for our symbol domain.  
		/// (Same as the [old] Labeler.) 
		/// </summary>
		protected DomainInfo domain;

		/// <summary>
		/// Upon loading this Feedback Mechanism, read domain info from
		/// domain file.  <see cref="Labeler.Form1.loadDomain()"/>
		/// </summary>
		public DomainAwareFeedbackMechanism()
		{
			this.domain = loadDomain();
		}

		/// <summary>
		/// Reads domain info from file.
		/// </summary>
		/// <returns>A DomainInfo containing the domain</returns>
		public static DomainInfo loadDomain()
		{
			System.IO.StreamReader sr = new System.IO.StreamReader(UIConstants.ColorDomainFilepath);
			DomainInfo rdomain = new DomainInfo();

			string studyInfo = sr.ReadLine();
			string domainNote = sr.ReadLine();

			int labelNum;
			string line, label, color;
			string[] words;

			line = sr.ReadLine();
			while (line != null && line != "")
			{
				words = line.Split(null);
				
				label = words[0];
				labelNum = int.Parse(words[1]);
				color = words[2];

				rdomain.addLabel(labelNum, label, Color.FromName(color));
				line = sr.ReadLine();
			}

			return rdomain;
		}


		/// <summary>
		/// Inheritors classes supply the feedback mechanism's visual effect.
		/// </summary>
		public abstract void FireFeedbackMechanism(Sketch.Sketch sketch, 
			InkPicture inkPicture, Hashtable ink2sketchIdTable, FeedbackContext context);
	}

	#endregion


	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Stroke Coloring Feedback Mechanism
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Stroke Coloring Feedback Mechanism

	/// <summary>
	/// A stroke coloring feedback mechanism based upon the Labeler's coloring features.  This mechanism colors ink
	/// strokes based upon what type of symbol (e.g. AND gate, wire, etc.) the stroke belongs to.
	/// 
	/// This mechanism works with the wizard recognizer to paint recognized strokes using 
	/// colors defined by the domain file associated with the Labeler.
	/// <see cref="WizardRecognizer"/>
	/// </summary>
	public class StrokeColoringFeedbackMechanism : DomainAwareFeedbackMechanism
	{
		/// <summary>
		/// The InkPicture to which this feedback mechanism will send tooltips
		/// </summary>
		private InkPicture sketchPicture;

		/// <summary>
		/// Hashtable mapping Microsoft Ink Stroke Ids to stroke labels
		/// </summary>
		private Hashtable ink2sketchLabelTable = new Hashtable();

		/// <summary>
		/// ToolTip for displaying stroke labels
		/// </summary>
		private ToolTip toolTip;

		/// <summary>
		/// Installs ToolTip callback into the parent form's sketch picture
		/// </summary>
		/// <param name="sketchPicture">The InkPicture to which 
		/// this feedback mechanism will send tooltips</param>
		public StrokeColoringFeedbackMechanism(InkPicture inkpic)
		{
			this.sketchPicture = inkpic;

			// Add tooltip handler
			this.sketchPicture.MouseMove += new MouseEventHandler(sketchPicture_MouseMove);

			// Create ToolTip
			toolTip = new ToolTip();
			toolTip.InitialDelay = UIConstants.ColorFeedbackMechanismTooltipDelay;
			toolTip.ShowAlways = true;
		}

		/// <summary>
		/// Removes tooltip handler
		/// </summary>
		~StrokeColoringFeedbackMechanism()
		{
			sketchPicture.MouseMove -= new MouseEventHandler(sketchPicture_MouseMove);
		}

		/// <summary>
		/// Set Ink stroke colors based upon domain
		/// </summary>
		public override void FireFeedbackMechanism(Sketch.Sketch sketch, 
			InkPicture inkPicture, Hashtable ink2sketchIdTable, FeedbackContext context)
		{
			// Only fire feedback on recogition events
			if (context != FeedbackContext.OnRecognitionResult)
				return;
			

			// Disable the Ink Picture if it is already enabled
			bool toggleInkPic = inkPicture.Enabled;
			if (toggleInkPic)
				inkPicture.Enabled = false;

			// Set Ink stroke colors
			foreach (Microsoft.Ink.Stroke istroke in inkPicture.Ink.Strokes)
			{
				// If this ink stroke does not have a 
				// corresponding sketch stroke, skip it
				if (!ink2sketchIdTable.Contains(istroke.Id))
					continue;

				// Get the label of the corresponding substroke
				Guid substrokeGuid = (Guid) ink2sketchIdTable[istroke.Id];
				Substroke substr = sketch.GetSubstroke(substrokeGuid);
				if (substr != null)
				{
                    string label = substr.FirstLabel;

					// Now color the stroke
					istroke.DrawingAttributes.Color = domain.getColor(label);

					// Store the label
					if (this.ink2sketchLabelTable.Contains(istroke.Id))
					{
						this.ink2sketchLabelTable[istroke.Id] = label;
					}
					else
					{
						this.ink2sketchLabelTable.Add(istroke.Id, label);
					}
				}
				else
				{
					// We should report an error here because
					// there should be a substroke
					// in the sketch if it's referenced in the
					// hashtable
					continue;
				}
			}

			// Re-enable the ink picture if it was enabled.
			if (toggleInkPic)
				inkPicture.Enabled = true;
		}

		/// <summary>
		/// Displays a tooltip based on the label of the nearest stroke.
		/// </summary>
		/// <param name="sender">event sender</param>
		/// <param name="e">event information, includign mouse location</param>
		private void sketchPicture_MouseMove(object sender, MouseEventArgs e)
		{
			// Get the current mouse position and convert it into InkSpace
			System.Drawing.Point mousePt = new System.Drawing.Point(e.X, e.Y);
			Graphics graphics = sketchPicture.CreateGraphics();
			sketchPicture.Renderer.PixelToInkSpace(graphics, ref mousePt);

			// Get the Microsoft Stroke closest to the mouse pointer
			float strokePt, distance;
			Microsoft.Ink.Stroke closestMSubstroke = sketchPicture.Ink.NearestPoint(mousePt, out strokePt, out distance);

			if (closestMSubstroke == null)
				return;

			// Fire tooltip if the mouse pointer is close enough
			if (distance < UIConstants.ColorFeedbackMechanismTooltipDistanceThreshold)
			{
				// Get the stroke's label, if it has one
				if (!ink2sketchLabelTable.Contains(closestMSubstroke.Id))
					return;

				string label = (string) ink2sketchLabelTable[closestMSubstroke.Id];
			
				// Show the ToolTip
				this.toolTip.SetToolTip(sketchPicture, label);
				this.toolTip.Active = true;
			}
			else
			{
				// Don't show the ToolTip if the mouse pointer is not close
				this.toolTip.Active = false;
			}	
		}
	}

	#endregion


	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Text Labeling Feedback Mechanism
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Text Labeling Feedback Mechanism

	/// <summary>
	/// A text labeling feedback mechanism.  This mechanism text-labels ink
	/// shapes based upon what type of symbol (e.g. AND gate, wire, etc.) the stroke belongs to.
	/// 
	/// This mechanism works with the wizard recognizer to paint recognized shapes using 
	/// text labels defined by the domain file associated with the Labeler.
	/// <see cref="WizardRecognizer"/>
	/// </summary>
	public class TextLabelFeedbackMechanism : DomainAwareFeedbackMechanism
	{
		/// <summary>
		/// ArrayList of all TextBox controls that this mechanism creates.  Allows
		/// us to support label deletions.
		/// </summary>
		private ArrayList textboxList = new ArrayList();
		
		
		/// <summary>
		/// Set Ink stroke text labels based upon domain
		/// </summary>
		public override void FireFeedbackMechanism(Sketch.Sketch sketch, 
			InkPicture inkPicture, Hashtable ink2sketchIdTable, FeedbackContext context)
		{
			//
			// Preprocessing
			//

			// Remove all current textboxes from the inkPicture
			// and clear the table of textboxes; start out fresh
			foreach (Label currentLabel in textboxList)
			{
				inkPicture.Controls.Remove(currentLabel);
			}
			textboxList.Clear();

			
			// Build hashtable of sketch substroke Ids to ink strokes
			Hashtable substroke2InkTable = new Hashtable();
			System.Guid currentGuid;
			foreach (Microsoft.Ink.Stroke iStroke in inkPicture.Ink.Strokes)
			{
				// If this ink stroke does not have a 
				// corresponding sketch stroke, skip it
				if (!ink2sketchIdTable.Contains(iStroke.Id))
					continue;

				currentGuid = (System.Guid) ink2sketchIdTable[iStroke.Id];
				substroke2InkTable.Add(currentGuid, iStroke);
			}

			
			//
			// Iterate through shapes
			//

			Graphics g = inkPicture.CreateGraphics();
			foreach (Sketch.Shape shape in sketch.Shapes)
			{
				// Get label of this shape
                string label = shape.Substrokes[0].FirstLabel;

				//
				// Color wires and don't label them
				//
				if (label == "Wire")
				{
					foreach (Sketch.Substroke substroke in shape.Substrokes)
					{
						if (!substroke2InkTable.Contains(substroke.XmlAttrs.Id))
						{
							// If, for some reason, there is no ink stroke
							// that corresponds to this substroke, skip it
							continue;
						}

						Microsoft.Ink.Stroke iStroke = (Microsoft.Ink.Stroke) substroke2InkTable[substroke.XmlAttrs.Id];
						iStroke.DrawingAttributes.Color = UIConstants.TextFeedbackMechanismWireColor;
					}
				}

				//
				// Draw boxes over input/output labels
				//
				else if (label == "Label")
				{
					// For efficiency's sake, we color the strokes while calculating the bounding box.
					Rectangle boundingBox = calculateBoundingBox(shape, substroke2InkTable, true);
					
					// Devise the (x,y) location of the text label on the screen
					System.Drawing.Point upperLeft = new System.Drawing.Point(boundingBox.X, boundingBox.Y);
					System.Drawing.Point lowerRight = new System.Drawing.Point(boundingBox.Right, boundingBox.Bottom);

					inkPicture.Renderer.InkSpaceToPixel(g, ref upperLeft);
					inkPicture.Renderer.InkSpaceToPixel(g, ref lowerRight);

					upperLeft.X -= 10;
					upperLeft.Y -= 10;
					lowerRight.X += 10;
					lowerRight.Y += 10;

					int bwidth = lowerRight.X - upperLeft.X;
					int bheight = lowerRight.Y - upperLeft.Y;

					// Create bounding box for label
					// HACK!!  TODO: draw a transparent label
					// instead of four labels to make 
					// a box around the symbol
					Label topLabel = new Label();
					Label bottomLabel = new Label();
					Label leftLabel = new Label();
					Label rightLabel = new Label();

					topLabel.UseMnemonic = false;
					bottomLabel.UseMnemonic = false;
					leftLabel.UseMnemonic = false;
					rightLabel.UseMnemonic = false;

					topLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
					bottomLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
					leftLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
					rightLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

					// Set label positions
					topLabel.Location = upperLeft;
					topLabel.Height = 1;
					topLabel.Width = bwidth;

					bottomLabel.Location = new System.Drawing.Point(upperLeft.X, upperLeft.Y + bheight);
					bottomLabel.Height = 1;
					bottomLabel.Width = bwidth;

					leftLabel.Location = new System.Drawing.Point(upperLeft.X, upperLeft.Y);
					leftLabel.Height = bheight;
					leftLabel.Width = 1;

					rightLabel.Location = new System.Drawing.Point(upperLeft.X + bwidth, upperLeft.Y);
					rightLabel.Height = bheight;
					rightLabel.Width = 1;

					topLabel.Enabled = true;
					bottomLabel.Enabled = true;
					leftLabel.Enabled = true;
					rightLabel.Enabled = true;

					inkPicture.Controls.Add(topLabel);
					inkPicture.Controls.Add(bottomLabel);
					inkPicture.Controls.Add(leftLabel);
					inkPicture.Controls.Add(rightLabel);

					textboxList.Add(topLabel);
					textboxList.Add(bottomLabel);
					textboxList.Add(leftLabel);
					textboxList.Add(rightLabel);
				}

				//
				// Draw labels for other symbols
			    //
				else
				{
					// For efficiency's sake, we color the strokes while calculating the bounding box.
					Rectangle boundingBox = calculateBoundingBox(shape, substroke2InkTable, true);
					
					// Devise the (x,y) location of the text label on the screen
					System.Drawing.Point center = new System.Drawing.Point(boundingBox.Width/2 + boundingBox.Left, 
						boundingBox.Height/2 + boundingBox.Top);
					System.Drawing.Point upperLeft = new System.Drawing.Point(boundingBox.X, boundingBox.Y);
					System.Drawing.Point lowerRight = new System.Drawing.Point(boundingBox.Right, boundingBox.Bottom);

					inkPicture.Renderer.InkSpaceToPixel(g, ref center);
					inkPicture.Renderer.InkSpaceToPixel(g, ref upperLeft);
					inkPicture.Renderer.InkSpaceToPixel(g, ref lowerRight);
					center.X += inkPicture.Bounds.X;
					center.Y += inkPicture.Bounds.Y;

					// Create textbox for label
					Label textLabel = new Label();
					textLabel.UseMnemonic = false;
					textLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
					textLabel.Text = label;
					textLabel.TextAlign = ContentAlignment.MiddleCenter;
					textLabel.AutoSize = true;
					

					// Position label so that it is either on top of the symbol,
					// or, if the symbol is very small, along side the symbol
					int bwidth = lowerRight.X - upperLeft.X;
					int bheight = lowerRight.Y - upperLeft.Y;
					if (textLabel.Width < bwidth && textLabel.Height < bheight)
					{
						center.X -= textLabel.Width / 2;
						center.Y -= textLabel.Height / 2;
					}
					else
					{
						// If taller than is wide, place label to the right;
						// else place label just above symbol
						if (boundingBox.Height > boundingBox.Width)
						{
							center.Y -= textLabel.Height / 2;
							center.X += bwidth / 2;
						}
						else
						{
							center.Y += bheight / 2;
							center.X -= textLabel.Width / 2;
						}
					}

					textLabel.Location = center;
					textLabel.Enabled = true;

					inkPicture.Controls.Add(textLabel);
					textboxList.Add(textLabel);
				}
			}

		}

		/// <summary>
		/// Returns the bounding box for the given shape in ink space coordinates.  Optionall colors the strokes
		/// the default color for non-wire symbols. 
		/// <see cref="UIConstants.TextFeedbackMechanismSymbolColor"/>
		/// </summary>
		/// <param name="shape">the shape for which the bounding box will be calculated</param>
		/// <param name="substroke2InkTable">A hashtable mapping Ink Stroke IDs to Sketch Substroke IDs.  
		/// This hashtable allows the Feedback Mechanism to assocate Ink Strokes drawn to screen
		/// with their corresponding (labeled) Sketch substrokes.</param>
		/// <param name="colorStrokes">True iff the microsoft ink strokes should be colored while calculating
		/// the bounding box (for efficiency's sake).</param>
		/// <returns>A System.Drawing.Rectangle representing the bounding box
		/// for the given shape (in ink space coordinates)</returns>
		private Rectangle calculateBoundingBox(Sketch.Shape shape, Hashtable substroke2InkTable, bool colorStrokes)
		{
			// Compute the bounding box of all the ink strokes
			// associated with this shape
			Rectangle boundingBox = Rectangle.Empty;

			foreach (Sketch.Substroke substroke in shape.Substrokes)
			{
				if (!substroke2InkTable.Contains(substroke.XmlAttrs.Id))
				{
					// If, for some reason, there is no ink stroke
					// that corresponds to this substroke, skip it
					continue;
				}

				Microsoft.Ink.Stroke iStroke = (Microsoft.Ink.Stroke) substroke2InkTable[substroke.XmlAttrs.Id];
				
				if (colorStrokes)
				{
					iStroke.DrawingAttributes.Color = UIConstants.TextFeedbackMechanismSymbolColor;
				}
				
				Rectangle strokeBox = iStroke.GetBoundingBox();

				if (boundingBox.IsEmpty)
				{
					boundingBox = strokeBox;
				}
				else
				{
					if (strokeBox.X < boundingBox.X)
						boundingBox.X = strokeBox.X;
					if (strokeBox.Y < boundingBox.Y)
						boundingBox.Y = strokeBox.Y;
					if (strokeBox.Bottom > boundingBox.Bottom)
						boundingBox.Height = strokeBox.Bottom - boundingBox.Y;
					if (strokeBox.Right > boundingBox.Right)
						boundingBox.Width = strokeBox.Right - boundingBox.X;
				}
			}

			return boundingBox;
		}
	}

	#endregion


}
