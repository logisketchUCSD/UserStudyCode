using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

using CommandManagement;

namespace Labeler
{
	/// <summary>
	/// Summary description for FragmentDialogBox.
	/// </summary>
	public class FragmentDialogBox : System.Windows.Forms.Form
	{
		private CommandManager CM;

		private CommandManager DialogCM;

		private LabelerPanel labelerPanel;

		private Sketch.Stroke[] strokes;

		private Dictionary<Sketch.Stroke, List<int>> fragStrokeToCorners;
		
		private FragmentPanel fragPanel;
		private System.Windows.Forms.ToolBar fragPanelToolbar;
		private System.Windows.Forms.ToolBarButton undoBtn;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.ToolBarButton doneBtn;
		private System.Windows.Forms.ToolBarButton cancelBtn;
		private System.Windows.Forms.ToolBarButton redoBtn;
		private System.Windows.Forms.ToolBarButton separatorBtn1;
		private System.Windows.Forms.ToolBarButton clearBtn;
		private System.Windows.Forms.ToolBarButton separatorBtn2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FragmentDialogBox(Sketch.Stroke[] strokes, LabelerPanel labelerPanel, CommandManager CM)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Store the old CommandManager
			this.CM = CM;

			// Create a new CommandManager for this window
			this.DialogCM = new CommandManager();

			// Strokes to hand fragment
			this.strokes = strokes;

			// Labeler panel
			this.labelerPanel = labelerPanel;

			this.fragStrokeToCorners = new Dictionary<Sketch.Stroke,List<int>>();
			foreach (Sketch.Stroke stroke in this.strokes)
			{
                List<int> val = new List<int>();
                try
                {
                    val = this.labelerPanel.LTool.StrokeToCorners[stroke];
                }
                catch (Exception e)
                {
                    val = null;
                }

				if (val != null)
					fragStrokeToCorners.Add(stroke, new List<int>(val));
			}

			// Create the FragmentPanel for this Window
			this.fragPanel = new FragmentPanel(this.strokes, this.fragStrokeToCorners, this.DialogCM);
			
			this.Controls.Clear();
			this.Controls.Add(this.fragPanel);
			this.Controls.Add(this.fragPanelToolbar);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.fragPanelToolbar = new System.Windows.Forms.ToolBar();
			this.undoBtn = new System.Windows.Forms.ToolBarButton();
			this.redoBtn = new System.Windows.Forms.ToolBarButton();
			this.separatorBtn1 = new System.Windows.Forms.ToolBarButton();
			this.clearBtn = new System.Windows.Forms.ToolBarButton();
			this.separatorBtn2 = new System.Windows.Forms.ToolBarButton();
			this.doneBtn = new System.Windows.Forms.ToolBarButton();
			this.cancelBtn = new System.Windows.Forms.ToolBarButton();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.SuspendLayout();
			// 
			// fragPanelToolbar
			// 
			this.fragPanelToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.undoBtn,
            this.redoBtn,
            this.separatorBtn1,
            this.clearBtn,
            this.separatorBtn2,
            this.doneBtn,
            this.cancelBtn});
			this.fragPanelToolbar.ButtonSize = new System.Drawing.Size(47, 43);
			this.fragPanelToolbar.DropDownArrows = true;
			this.fragPanelToolbar.Location = new System.Drawing.Point(0, 0);
			this.fragPanelToolbar.Name = "fragPanelToolbar";
			this.fragPanelToolbar.ShowToolTips = true;
			this.fragPanelToolbar.Size = new System.Drawing.Size(448, 42);
			this.fragPanelToolbar.TabIndex = 0;
			this.fragPanelToolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.fragPanelToolbar_ButtonClick);
			// 
			// undoBtn
			// 
			this.undoBtn.Name = "undoBtn";
			this.undoBtn.Text = "Undo";
			// 
			// redoBtn
			// 
			this.redoBtn.Name = "redoBtn";
			this.redoBtn.Text = "Redo";
			// 
			// separatorBtn1
			// 
			this.separatorBtn1.Name = "separatorBtn1";
			this.separatorBtn1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// clearBtn
			// 
			this.clearBtn.Name = "clearBtn";
			this.clearBtn.Text = "Clear";
			// 
			// separatorBtn2
			// 
			this.separatorBtn2.Name = "separatorBtn2";
			this.separatorBtn2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// doneBtn
			// 
			this.doneBtn.Name = "doneBtn";
			this.doneBtn.Text = "Done";
			// 
			// cancelBtn
			// 
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Text = "Cancel";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 280);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(448, 22);
			this.statusBar.TabIndex = 1;
			// 
			// FragmentDialogBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(448, 302);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.fragPanelToolbar);
			this.Name = "FragmentDialogBox";
			this.Text = "Fragment Stroke";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		
		/// <summary>
		/// Handle the toolbar button being pressed.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void fragPanelToolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			// Undo the previous action
			if (e.Button == this.undoBtn)
			{
				DialogCM.Undo();
				this.fragPanel.SketchInk.Refresh();
			}

			// Redo the previously undone action
			else if (e.Button == this.redoBtn)
			{
				DialogCM.Redo();
				this.fragPanel.SketchInk.Refresh();
			}

			else if (e.Button == this.clearBtn)
			{
				DialogCM.ExecuteCommand( new CommandList.ClearFragmentPointsCmd(this.fragPanel) );
			}
			
			// Commit the changes
			else if (e.Button == this.doneBtn)
			{
				CM.ExecuteCommand( new CommandList.CommitHandFragmentCmd(this.fragPanel.StrokeToCorners,
					this.labelerPanel) );
				this.Close();
			}

			// Cancel the changes
			else if (e.Button == this.cancelBtn)
			{
				this.Close();
			}
		}
	}
}
