using System;
using System.Collections.Generic;

using Microsoft.Ink;

using Sketch;

using CommandManagement;


namespace Labeler
{
	/// <summary>
	/// Summary description for LabelMenu.
	/// </summary>
	public class LabelMenu : System.Windows.Forms.Panel
	{
		private const int PREVLABELHEIGHT = 15;

		private const int OUT_OF_BOUNDS = -1;
		
		private CommandManager CM;
		
		private DomainInfo domainInfo;
		
		private LabelerTool labelerPanel;
		
		private int previousLabelSelected = OUT_OF_BOUNDS;

        /// <summary>
        /// Boolean value that indicates whether to use checkboxes which allow strokes to be have
        /// multiple classifications or lists that allow only one
        /// </summary>
        private bool ALLOW_MULTI_GROUPS = false;

        //private System.Windows.Forms.ListBox previousLabel;

        //private System.Windows.Forms.ListBox labelList;
		
        private System.Windows.Forms.CheckedListBox previousLabel;

        private System.Windows.Forms.CheckedListBox labelList;



		private void InitializeComponent()
		{
            this.previousLabel = new System.Windows.Forms.CheckedListBox();
            this.labelList = new System.Windows.Forms.CheckedListBox();
            //this.labelList = new System.Windows.Forms.ListBox();
            //this.previousLabel = new System.Windows.Forms.ListBox();

			// 
			// previousLabel
			// 
            this.previousLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previousLabel.Name = "previousLabel";
            this.previousLabel.TabIndex = 0;

			// 
			// labelList
			// 
            this.labelList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelList.Name = "labelList";
            this.labelList.TabIndex = 0;

            if (ALLOW_MULTI_GROUPS)
                this.labelList.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            else
                this.labelList.SelectionMode = System.Windows.Forms.SelectionMode.One;
		}
	

		public LabelMenu(LabelerTool labelerPanel, CommandManager CM)
		{
			InitializeComponent();

			// Our Parent, the LabelerPanel containing this
			this.labelerPanel = labelerPanel;

			// CommandManager
			this.CM = CM;
			
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Height = 100;
			this.Width = 100;

			this.previousLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelList.Dock = System.Windows.Forms.DockStyle.Bottom;

			this.previousLabel.Size = new System.Drawing.Size(100, PREVLABELHEIGHT);
			this.labelList.Size = new System.Drawing.Size(100, this.Height - PREVLABELHEIGHT);

			this.Location = new System.Drawing.Point(20, 20);

			// Event handlers
			this.previousLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(previousLabel_MouseUp);
			this.previousLabel.MouseEnter += new EventHandler(previousLabel_MouseEnter);
			this.previousLabel.MouseLeave += new EventHandler(previousLabel_MouseLeave);
			this.previousLabel.DragLeave += new EventHandler(previousLabel_DragLeave);

			this.labelList.MouseUp += new System.Windows.Forms.MouseEventHandler(labelList_MouseUp);
			this.labelList.MouseEnter += new EventHandler(labelList_MouseEnter);

			// Add the controls
			this.Controls.Add(this.labelList);
			this.Controls.Add(this.previousLabel);

			// Initially hide the panel
			this.Hide();
		}


		public void InitLabels(DomainInfo domainInfo)
		{
			this.domainInfo = domainInfo;
			this.labelList.Items.Clear();
			
			int maxString = Int32.MinValue;
			
			if (this.domainInfo != null)
			{
				// Get all of the labels
				List<string> labels = this.domainInfo.GetLabels();

				// Add each label to our menu, keeping track of the longest string we insert
				foreach (string s in labels)
				{
					string text = s + "  (" + this.domainInfo.GetColor(s).Name + ")";			
					maxString = Math.Max(maxString, text.Length);

					this.labelList.Items.Add(text);
				}

				// Set the "previously clicked" item
				this.previousLabel.Items.Add("");
			}
			else
			{
				string defaultText = "No Labels loaded";
				maxString = Math.Max(maxString, defaultText.Length);

				this.labelList.Items.Add(defaultText);
			}

			// Set the new components' widths
			int newWidth = (maxString * 8) + 15;
			this.Size = new System.Drawing.Size(newWidth, 100);
			this.previousLabel.Width = newWidth;
			this.labelList.Width = newWidth;
		}


		public void UpdateSize(int width, int height)
		{
			this.Size = new System.Drawing.Size(width, height);

			this.previousLabel.Size = new System.Drawing.Size(width, 20);
		}


		/// <summary>
		/// Gets label data for a given group of Microsoft strokes
		/// </summary>
		public void UpdateLabelMenu(Microsoft.Ink.Strokes selection)
		{
			if (this.domainInfo != null && selection.Count > 0)
			{
				for (int i = 0; i < this.labelList.Items.Count; i++)
				{
					string currLabel = this.domainInfo.GetLabel(i);

					bool labelPresentInAll = true;
					Sketch.Shape foundShape = null;

					foreach (Microsoft.Ink.Stroke stroke in selection)
					{
                        int temp = stroke.Id; 
                        Sketch.Substroke currSelection = 
							(this.labelerPanel.getSubstrokeByMId(temp) as Sketch.Substroke);
						
						List<Sketch.Shape> labelShapes = currSelection.ParentShapes;
						
						bool labelPresent = false;
						foreach (Sketch.Shape labelShape in labelShapes)
						{
							if (labelShape.XmlAttrs.Type as String == currLabel)
							{
								if (foundShape == null)
								{
									foundShape = labelShape;
									labelPresent = true;
									break;	
								}
								else if (foundShape == labelShape)
								{
									labelPresent = true;
									break;	
								}
							}
						}

						if (!labelPresent)
						{
							labelPresentInAll = false;
							break;
						}
					}

					// Check or not?
					if (labelPresentInAll)
						this.labelList.SetItemChecked(i, true);
					else
						this.labelList.SetItemChecked(i, false);
				}
			
				// Set our previously selected label value to the corresponding value
				if (this.previousLabelSelected != OUT_OF_BOUNDS)
					this.previousLabel.SetItemChecked(0, this.labelList.GetItemChecked(this.previousLabelSelected));
			}
		}


		private void previousLabel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.previousLabelSelected != OUT_OF_BOUNDS	&& this.previousLabel.SelectedIndex != OUT_OF_BOUNDS &&
				this.Bounds.Contains(e.X + this.Location.X, e.Y + this.Location.Y))
			{
				ApplyLabelToSelection(this.previousLabelSelected);
			}
		}

		
		private void labelList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
            int howmanyselected = this.labelList.SelectedItems.Count;
			if (this.labelList.SelectedItem != null 
				&& this.Bounds.Contains(e.X + this.Location.X, e.Y + this.Location.Y))
			{
				// Get the index for the label
				int selectedLabelIndex = this.labelList.SelectedIndex;

				ApplyLabelToSelection(selectedLabelIndex);
			}
		}


		private void ApplyLabelToSelection(int selectionIndex)
		{
			if (this.domainInfo != null && selectionIndex != OUT_OF_BOUNDS && 
				this.labelerPanel.SketchInk.Selection.Count > 0)
			{
				// Get the string of the label
				string label = this.domainInfo.GetLabel(selectionIndex);

				// Get the label's color
				System.Drawing.Color labelColor = this.domainInfo.GetColor(selectionIndex);

				// For each Ink stroke selected, add it to a temporary Sketch.Substroke ArrayList
				List<Substroke> selected = new List<Substroke>();
				foreach (Microsoft.Ink.Stroke stroke in this.labelerPanel.SketchInk.Selection)
				{
                    int temp = stroke.Id;
					selected.Add(this.labelerPanel.getSubstrokeByMId(temp));
				}

                bool shouldLabel = this.labelList.GetItemChecked(selectionIndex);
				if (!shouldLabel)
				{
					// Apply the label
					CM.ExecuteCommand(new CommandList.ApplyLabelCmd(this.labelerPanel.Sketch, 
						selected, this.labelerPanel.SketchInk.Selection, label, labelColor, 
						this.domainInfo));

					// Update the previous selection
					this.previousLabelSelected = selectionIndex;
					this.previousLabel.Items.Clear();
					this.previousLabel.Items.Add(this.labelList.Items[selectionIndex]);
				}
				else
				{
					// Remove the label
					CM.ExecuteCommand(new CommandList.RemoveLabelCmd(this.labelerPanel.Sketch, 
						selected, this.labelerPanel.SketchInk.Selection, 
						this.labelerPanel.MIdtoSubstroke, label, this.domainInfo));
				
					this.labelerPanel.UnThickenLabel(this.labelerPanel.SketchInk.Selection);
				}

				UpdateLabelMenu(this.labelerPanel.SketchInk.Selection);

				// Clear the current selection
				//this.labelerPanel.SketchInk.Selection = new Ink().CreateStrokes();
				
				this.Hide();
				
				this.previousLabel.SelectedIndex = OUT_OF_BOUNDS;
				this.labelList.SelectedIndex = OUT_OF_BOUNDS;

				this.labelerPanel.SketchInk.Invalidate();
			}
		}

		private void previousLabel_MouseEnter(object sender, EventArgs e)
		{
			this.previousLabel.Focus();
		//	this.previousLabel.SelectedIndex = 0;
		}

		private void labelList_MouseEnter(object sender, EventArgs e)
		{
			this.labelList.Focus();
		//	this.labelList.SelectedIndex = 0;
		}

		private void previousLabel_MouseLeave(object sender, EventArgs e)
		{
			return;
		}

		private void previousLabel_DragLeave(object sender, EventArgs e)
		{
			return;
		}
	}
}
