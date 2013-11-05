using System;
using System.Data;
using System.Collections;
using System.ComponentModel;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using Microsoft.Ink;

using Converter;
using Sketch;
using CommandManagement;

namespace Labeler
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		#region INTERNALS

		/// <summary>
		/// CommandManager for the Labeler instance.
		/// </summary>
		private CommandManager CM;

		/// <summary>
		/// List of Commands for the Labeler instance.
		/// </summary>
		private CommandList commands;
		
		/// <summary>
		/// The Sketch we will be using in the Labeler.
		/// </summary>
		private Sketch.Sketch sketch;
		
		/// <summary>
		/// The InkOverlay we place ontop of our drawing panel. Holds all of the strokes to draw 
		/// and provides scaling and rendering functions.
		/// </summary>
		private Microsoft.Ink.InkOverlay oInk;
		
		/// <summary>
		/// Hashtable from Microsoft Stroke Ids to Sketch.Substroke Ids.
		/// </summary>
		private Hashtable mIdToSubstroke;

		/// <summary>
		/// Hashtable from Microsoft Stroke Ids to their corresponding indices in the InkOverlay.
		/// </summary>
		private Hashtable mIdToInkIndex;

		/// <summary>
		/// Hashtable mapping a Microsoft.Stroke's Id to the indices where the stroke should be split
		/// </summary>
		private Hashtable mIdToIndices;

		/// <summary>
		/// Total scaling factor from the original size.
		/// </summary>
		private float totalScale = 1.0f;
		
		/// <summary>
		/// Previous scaling factor. Used when typing in a scale from the zoom text box.
		/// </summary>
		private float prevScale  = 1.0f;
		
		/// <summary>
		/// New scaling factor. Used when typing in a scale from the zoom text box.
		/// </summary>
		private float newScale   = 1.0f;

		/// <summary>
		/// The scrollbar positions, corresponding to how much the InkOverlay should be scrolled by.
		/// </summary>
		private System.Drawing.Point ScrollPos = new System.Drawing.Point(0,0);
		
		private System.Windows.Forms.ToolTip toolTip;


		private System.Windows.Forms.MainMenu FileMenu;
		private System.Windows.Forms.MenuItem FileOpen;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;	// Open MIT XML
		private System.Windows.Forms.OpenFileDialog openFileDialog2;	// Open Domain file
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;	// Save as MIT XML
		
		private DomainInfo domainInfo;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Button labelButton;
		private System.Windows.Forms.ComboBox shapeLabels;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.RadioButton editModeBtn;
		private System.Windows.Forms.RadioButton selectModeBtn;
		private System.Windows.Forms.TextBox zoomTextBox;
		private System.Windows.Forms.CheckBox panelWidthBox;
		private System.Windows.Forms.Label zoomLabel;

		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.Button loadDomainBtn;
		private System.Windows.Forms.Button saveXmlBtn;
		private System.Windows.Forms.Button openXmlBtn;
		private System.Windows.Forms.MenuItem openXmlMI;
		private System.Windows.Forms.MenuItem loadDomainMI;
		private System.Windows.Forms.MenuItem saveXmlMI;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.ToolBar toolBar;
		private System.Windows.Forms.Button zoomInBtn;
		private System.Windows.Forms.Button zoomOutBtn;
		private System.Windows.Forms.Button commitBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.HScrollBar hScrollBar;
		private System.Windows.Forms.VScrollBar vScrollBar;
		private System.Windows.Forms.CheckBox highlightEnd;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem undoMI;
		private System.Windows.Forms.MenuItem redoMI;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
				
		#endregion

		/// <summary>
		/// Main form initialization. Creates all the components specified in the Form design
		/// from Visual Studio. Also creates the InkOverlay for the form.
		/// </summary>
		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			// Double-buffering code
			this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();

			// Initialize the CommandManager & CommandList
			this.CM = new CommandManager();
			this.commands = new CommandList();
			
			// Create the ToolTip to be used in displaying Substroke information
			toolTip = new ToolTip();
			toolTip.InitialDelay = 100;
			toolTip.ShowAlways = true;
		
			// Initialize the Hashtables
			this.mIdToIndices	  = new Hashtable();
			this.mIdToSubstroke	  = new Hashtable();
			this.mIdToInkIndex	  = new Hashtable();

			// Create the InkOverlay
			oInk = new InkOverlay(panel);
			oInk.Stroke += new InkCollectorStrokeEventHandler(strokeSplitHandler);

			// Set the selection and mouse movement handlers
			oInk.SelectionMoved += new InkOverlaySelectionMovedEventHandler(oInk_SelectionMoved);
			oInk.SelectionResized += new InkOverlaySelectionResizedEventHandler(oInk_SelectionResized);
			oInk.MouseMove += new InkCollectorMouseMoveEventHandler(oInk_MouseMove);

			// Can be used later to incorporate gestures, such as SystemGesture.RightTap
			//oInk.SystemGesture += new InkCollectorSystemGestureEventHandler(oInk_SystemGesture);

			// Set a handler to perform certain functions after the InkOverlay has painted
			oInk.Painted += new InkOverlayPaintedEventHandler(oInk_Painted);

			oInk.Enabled = true;

			// Keyboard shortcuts associated with the form
			//this.KeyDown += new KeyEventHandler(Form1_KeyDown);
			
			// Initial GUI modes is Selection
			changeModeSelect();
		}

		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.FileMenu = new System.Windows.Forms.MainMenu();
			this.FileOpen = new System.Windows.Forms.MenuItem();
			this.openXmlMI = new System.Windows.Forms.MenuItem();
			this.loadDomainMI = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.saveXmlMI = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.undoMI = new System.Windows.Forms.MenuItem();
			this.redoMI = new System.Windows.Forms.MenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
			this.panel = new System.Windows.Forms.Panel();
			this.labelButton = new System.Windows.Forms.Button();
			this.shapeLabels = new System.Windows.Forms.ComboBox();
			this.selectModeBtn = new System.Windows.Forms.RadioButton();
			this.editModeBtn = new System.Windows.Forms.RadioButton();
			this.commitBtn = new System.Windows.Forms.Button();
			this.hScrollBar = new System.Windows.Forms.HScrollBar();
			this.vScrollBar = new System.Windows.Forms.VScrollBar();
			this.toolBar = new System.Windows.Forms.ToolBar();
			this.zoomTextBox = new System.Windows.Forms.TextBox();
			this.zoomLabel = new System.Windows.Forms.Label();
			this.panelWidthBox = new System.Windows.Forms.CheckBox();
			this.openXmlBtn = new System.Windows.Forms.Button();
			this.loadDomainBtn = new System.Windows.Forms.Button();
			this.saveXmlBtn = new System.Windows.Forms.Button();
			this.zoomInBtn = new System.Windows.Forms.Button();
			this.zoomOutBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.highlightEnd = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// FileMenu
			// 
			this.FileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.FileOpen,
																					 this.menuItem3});
			// 
			// FileOpen
			// 
			this.FileOpen.Index = 0;
			this.FileOpen.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.openXmlMI,
																					 this.loadDomainMI,
																					 this.menuItem2,
																					 this.saveXmlMI,
																					 this.menuItem1,
																					 this.exitMenuItem});
			this.FileOpen.Text = "File";
			// 
			// openXmlMI
			// 
			this.openXmlMI.Index = 0;
			this.openXmlMI.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.openXmlMI.Text = "Open XML...";
			this.openXmlMI.Click += new System.EventHandler(this.openXmlMI_Click);
			// 
			// loadDomainMI
			// 
			this.loadDomainMI.Index = 1;
			this.loadDomainMI.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
			this.loadDomainMI.Text = "Load Domain File...";
			this.loadDomainMI.Click += new System.EventHandler(this.loadDomainMI_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 2;
			this.menuItem2.Text = "-";
			// 
			// saveXmlMI
			// 
			this.saveXmlMI.Index = 3;
			this.saveXmlMI.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.saveXmlMI.Text = "Save...";
			this.saveXmlMI.Click += new System.EventHandler(this.saveXmlMI_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 4;
			this.menuItem1.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 5;
			this.exitMenuItem.Text = "Exit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.undoMI,
																					  this.redoMI});
			this.menuItem3.Text = "Edit";
			// 
			// undoMI
			// 
			this.undoMI.Index = 0;
			this.undoMI.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
			this.undoMI.Text = "Undo";
			this.undoMI.Click += new System.EventHandler(this.undoMI_Click);
			// 
			// redoMI
			// 
			this.redoMI.Index = 1;
			this.redoMI.Shortcut = System.Windows.Forms.Shortcut.CtrlY;
			this.redoMI.Text = "Redo";
			this.redoMI.Click += new System.EventHandler(this.redoMI_Click);
			// 
			// statusBar
			// 
			this.statusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.statusBar.Location = new System.Drawing.Point(0, 529);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(496, 16);
			this.statusBar.SizingGrip = false;
			this.statusBar.TabIndex = 0;
			// 
			// panel
			// 
			this.panel.AutoScroll = true;
			this.panel.BackColor = System.Drawing.SystemColors.Window;
			this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel.Location = new System.Drawing.Point(0, 37);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(776, 427);
			this.panel.TabIndex = 1;
			// 
			// labelButton
			// 
			this.labelButton.Enabled = false;
			this.labelButton.Location = new System.Drawing.Point(688, 500);
			this.labelButton.Name = "labelButton";
			this.labelButton.Size = new System.Drawing.Size(88, 24);
			this.labelButton.TabIndex = 2;
			this.labelButton.Text = "Apply Label";
			this.labelButton.Click += new System.EventHandler(this.labelButton_Click);
			// 
			// shapeLabels
			// 
			this.shapeLabels.Location = new System.Drawing.Point(512, 500);
			this.shapeLabels.Name = "shapeLabels";
			this.shapeLabels.Size = new System.Drawing.Size(168, 21);
			this.shapeLabels.TabIndex = 3;
			this.shapeLabels.Text = "No Dataset Loaded";
			// 
			// selectModeBtn
			// 
			this.selectModeBtn.Checked = true;
			this.selectModeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.selectModeBtn.ForeColor = System.Drawing.SystemColors.ControlText;
			this.selectModeBtn.Location = new System.Drawing.Point(16, 500);
			this.selectModeBtn.Name = "selectModeBtn";
			this.selectModeBtn.Size = new System.Drawing.Size(120, 24);
			this.selectModeBtn.TabIndex = 4;
			this.selectModeBtn.TabStop = true;
			this.selectModeBtn.Text = "Selection Mode";
			this.selectModeBtn.CheckedChanged += new System.EventHandler(this.selectModeBtn_CheckedChanged);
			// 
			// editModeBtn
			// 
			this.editModeBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.editModeBtn.Location = new System.Drawing.Point(152, 500);
			this.editModeBtn.Name = "editModeBtn";
			this.editModeBtn.TabIndex = 5;
			this.editModeBtn.Text = "Editing Mode";
			this.editModeBtn.CheckedChanged += new System.EventHandler(this.editModeBtn_CheckedChanged);
			// 
			// commitBtn
			// 
			this.commitBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.commitBtn.Location = new System.Drawing.Point(304, 500);
			this.commitBtn.Name = "commitBtn";
			this.commitBtn.Size = new System.Drawing.Size(80, 24);
			this.commitBtn.TabIndex = 6;
			this.commitBtn.Text = "Commit";
			this.commitBtn.Click += new System.EventHandler(this.commitBtn_Click);
			// 
			// hScrollBar
			// 
			this.hScrollBar.LargeChange = 50;
			this.hScrollBar.Location = new System.Drawing.Point(0, 464);
			this.hScrollBar.Name = "hScrollBar";
			this.hScrollBar.Size = new System.Drawing.Size(776, 17);
			this.hScrollBar.SmallChange = 8;
			this.hScrollBar.TabIndex = 22;
			this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar_Scroll);
			// 
			// vScrollBar
			// 
			this.vScrollBar.LargeChange = 50;
			this.vScrollBar.Location = new System.Drawing.Point(776, 36);
			this.vScrollBar.Name = "vScrollBar";
			this.vScrollBar.Size = new System.Drawing.Size(17, 428);
			this.vScrollBar.SmallChange = 8;
			this.vScrollBar.TabIndex = 21;
			this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar_Scroll);
			// 
			// toolBar
			// 
			this.toolBar.ButtonSize = new System.Drawing.Size(39, 30);
			this.toolBar.DropDownArrows = true;
			this.toolBar.Location = new System.Drawing.Point(0, 0);
			this.toolBar.Name = "toolBar";
			this.toolBar.ShowToolTips = true;
			this.toolBar.Size = new System.Drawing.Size(792, 36);
			this.toolBar.TabIndex = 9;
			// 
			// zoomTextBox
			// 
			this.zoomTextBox.Enabled = false;
			this.zoomTextBox.Location = new System.Drawing.Point(657, 9);
			this.zoomTextBox.Name = "zoomTextBox";
			this.zoomTextBox.Size = new System.Drawing.Size(55, 20);
			this.zoomTextBox.TabIndex = 10;
			this.zoomTextBox.Text = "1.0";
			this.zoomTextBox.TextChanged += new System.EventHandler(this.zoomTextBox_TextChanged);
			// 
			// zoomLabel
			// 
			this.zoomLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomLabel.Location = new System.Drawing.Point(600, 7);
			this.zoomLabel.Name = "zoomLabel";
			this.zoomLabel.Size = new System.Drawing.Size(48, 24);
			this.zoomLabel.TabIndex = 11;
			this.zoomLabel.Text = "Zoom:";
			this.zoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panelWidthBox
			// 
			this.panelWidthBox.Checked = true;
			this.panelWidthBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.panelWidthBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.panelWidthBox.Location = new System.Drawing.Point(480, 8);
			this.panelWidthBox.Name = "panelWidthBox";
			this.panelWidthBox.TabIndex = 12;
			this.panelWidthBox.Text = "Page Width";
			this.panelWidthBox.CheckedChanged += new System.EventHandler(this.panelWidthBox_CheckedChanged);
			// 
			// openXmlBtn
			// 
			this.openXmlBtn.Location = new System.Drawing.Point(8, 8);
			this.openXmlBtn.Name = "openXmlBtn";
			this.openXmlBtn.Size = new System.Drawing.Size(86, 23);
			this.openXmlBtn.TabIndex = 13;
			this.openXmlBtn.Text = "Open XML";
			this.openXmlBtn.Click += new System.EventHandler(this.openXmlBtn_Click);
			// 
			// loadDomainBtn
			// 
			this.loadDomainBtn.Location = new System.Drawing.Point(103, 8);
			this.loadDomainBtn.Name = "loadDomainBtn";
			this.loadDomainBtn.Size = new System.Drawing.Size(86, 23);
			this.loadDomainBtn.TabIndex = 14;
			this.loadDomainBtn.Text = "Load Domain";
			this.loadDomainBtn.Click += new System.EventHandler(this.loadDomainBtn_Click);
			// 
			// saveXmlBtn
			// 
			this.saveXmlBtn.Location = new System.Drawing.Point(197, 8);
			this.saveXmlBtn.Name = "saveXmlBtn";
			this.saveXmlBtn.TabIndex = 15;
			this.saveXmlBtn.Text = "Save XML";
			this.saveXmlBtn.Click += new System.EventHandler(this.saveXmlBtn_Click);
			// 
			// zoomInBtn
			// 
			this.zoomInBtn.Enabled = false;
			this.zoomInBtn.Font = new System.Drawing.Font("Arial Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomInBtn.ForeColor = System.Drawing.Color.Green;
			this.zoomInBtn.Location = new System.Drawing.Point(722, 8);
			this.zoomInBtn.Name = "zoomInBtn";
			this.zoomInBtn.Size = new System.Drawing.Size(24, 23);
			this.zoomInBtn.TabIndex = 16;
			this.zoomInBtn.Text = "+";
			this.zoomInBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.zoomInBtn.Click += new System.EventHandler(this.zoomInBtn_Click);
			// 
			// zoomOutBtn
			// 
			this.zoomOutBtn.Enabled = false;
			this.zoomOutBtn.Font = new System.Drawing.Font("Arial Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.zoomOutBtn.ForeColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(0)), ((System.Byte)(0)));
			this.zoomOutBtn.Location = new System.Drawing.Point(756, 8);
			this.zoomOutBtn.Name = "zoomOutBtn";
			this.zoomOutBtn.Size = new System.Drawing.Size(24, 23);
			this.zoomOutBtn.TabIndex = 17;
			this.zoomOutBtn.Text = "-";
			this.zoomOutBtn.Click += new System.EventHandler(this.zoomOutBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cancelBtn.Location = new System.Drawing.Point(400, 500);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 24);
			this.cancelBtn.TabIndex = 18;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(496, 529);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(296, 16);
			this.progressBar.TabIndex = 19;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(0, 36);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 509);
			this.splitter1.TabIndex = 20;
			this.splitter1.TabStop = false;
			// 
			// highlightEnd
			// 
			this.highlightEnd.Checked = true;
			this.highlightEnd.CheckState = System.Windows.Forms.CheckState.Checked;
			this.highlightEnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.highlightEnd.Location = new System.Drawing.Point(312, 8);
			this.highlightEnd.Name = "highlightEnd";
			this.highlightEnd.Size = new System.Drawing.Size(144, 24);
			this.highlightEnd.TabIndex = 23;
			this.highlightEnd.Text = "Highlight Endpoints";
			this.highlightEnd.CheckedChanged += new System.EventHandler(this.highlightEnd_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(792, 545);
			this.Controls.Add(this.highlightEnd);
			this.Controls.Add(this.panelWidthBox);
			this.Controls.Add(this.zoomTextBox);
			this.Controls.Add(this.selectModeBtn);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.zoomOutBtn);
			this.Controls.Add(this.zoomInBtn);
			this.Controls.Add(this.saveXmlBtn);
			this.Controls.Add(this.loadDomainBtn);
			this.Controls.Add(this.openXmlBtn);
			this.Controls.Add(this.zoomLabel);
			this.Controls.Add(this.toolBar);
			this.Controls.Add(this.vScrollBar);
			this.Controls.Add(this.hScrollBar);
			this.Controls.Add(this.commitBtn);
			this.Controls.Add(this.editModeBtn);
			this.Controls.Add(this.shapeLabels);
			this.Controls.Add(this.labelButton);
			this.Controls.Add(this.panel);
			this.Controls.Add(this.statusBar);
			this.Menu = this.FileMenu;
			this.MinimumSize = new System.Drawing.Size(800, 400);
			this.Name = "Form1";
			this.Text = "Labeler";
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.ResumeLayout(false);

		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			try
			{
				Application.Run(new Form1());
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + " " + e.StackTrace);
			}
		}
		
		
		#region FORM EVENTS (MenuItems, Buttons, etc.)
		
		/// <summary>
		/// Starts the "Open" dialog allowing the user to open an MIT XML file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void openXmlMI_Click(object sender, System.EventArgs e)
		{
			this.statusBar.Text    = "Choose a valid MIT XML sketch file...";
			openFileDialog1.Title  = "Open MIT XML";
			openFileDialog1.Filter = "MIT XML Files (*.xml)|*.xml";
			
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				// Clear the previous hashtables
				this.mIdToInkIndex.Clear();
				this.mIdToIndices.Clear();
				this.mIdToSubstroke.Clear();

				this.statusBar.Text = "Loading file...";
				
				// Load the Sketch
				this.sketch = (new ReadXML(openFileDialog1.FileName)).Sketch;
				
				// Get the strokes and points out of the newly loaded file
				// This program only works for UNLABELED files because it only grabs the strokes and points
				// not other labeled strokes or substrokes
				
				updateInkOverlay(this.sketch, this.oInk);
				
				this.Text = "LablerSession ~ " + System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
				
				this.statusBar.Text = "File loaded.";
			
				FormResize();
				selectModeBtn.Checked = true;
				commitBtn.Enabled = true;
			}
			else
			{
				this.statusBar.Text = "";
			}
		}


		/// <summary>
		/// Starts the "Open" dialog allowing the user to open an MIT XML file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void openXmlBtn_Click(object sender, System.EventArgs e)
		{
			openXmlMI_Click(sender, e);
		}

		
		/// <summary>
		/// Starts the "Load Domain File" dialog allowing the user to open a domain file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void loadDomainMI_Click(object sender, System.EventArgs e)
		{
			this.statusBar.Text = "Load a valid domain file...";
			
			openFileDialog2.Title = "Load Domain File";
			openFileDialog1.Filter = "Domain Files (*.txt)|*.txt";
			
			if (openFileDialog2.ShowDialog() == DialogResult.OK)
			{
				System.IO.StreamReader sr = new 
					System.IO.StreamReader(openFileDialog2.FileName);
				loadDomain( sr );
				sr.Close();

				this.statusBar.Text = "Domain file loaded";

				// Reset the colors based on the labels
				updateColorsFromLabels();
			
				panel.Refresh();
			}
			else
			{
				this.statusBar.Text = "";
			}

		}
	

		/// <summary>
		/// Starts the "Open Domain File" dialog allowing the user to open a domain file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void loadDomainBtn_Click(object sender, System.EventArgs e)
		{
			loadDomainMI_Click(sender, e);
		}

		
		/// <summary>
		/// Loads the domain file
		/// </summary>
		/// <param name="sr">IO stream</param>
		private void loadDomain(System.IO.StreamReader sr)
		{
			domainInfo = new DomainInfo();
			string line = sr.ReadLine();
			string [] words = line.Split(null);
			
			// The first line is the study info
			domainInfo.addInfo(words[0], words[1]);
			line = sr.ReadLine();
			Console.WriteLine("Study is " + words[1]);

			// The next line is the domain
			words = line.Split(null);
			domainInfo.addInfo(words[0], words[1]);
			line = sr.ReadLine();
			Console.WriteLine("Domain is " + words[1]);

			// Then the rest are labels
			while ( line != null && line != "" ) 
			{
				words = line.Split(null);
				
				string label = words[0];
				int num = int.Parse(words[1]);
				string color = words[2];

				domainInfo.addLabel(num, label, Color.FromName(color));
				line = sr.ReadLine();
			}

			ArrayList labels = domainInfo.getLabels();
			string[] labelsWithColors = new string[labels.Count];

			for (int i = 0; i < labelsWithColors.Length; i++)
			{
				labelsWithColors[i] = (string)labels[i] + "   (" + domainInfo.getColor((string)labels[i]).Name + ")";
			}

			shapeLabels.DataSource = labelsWithColors;
			this.labelButton.Enabled = true;
		}
		

		/// <summary>
		/// Starts the "Save As" dialog allowing the user to save a labeled MIT XML file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void saveXmlMI_Click(object sender, System.EventArgs e)
		{
			this.statusBar.Text = "Choose where to save the file...";
			saveFileDialog1.Filter = "MIT XML Files (*.xml)|*.xml";
			saveFileDialog1.AddExtension = true;

			// Write the XML to a file
			if ((saveFileDialog1.ShowDialog() == DialogResult.OK))
			{
				this.statusBar.Text = "Saving file...";
				
				if (this.sketch != null)
				{
					Converter.MakeXML xmlHolder = new MakeXML(this.sketch);
					xmlHolder.WriteXML(saveFileDialog1.FileName);
					
					this.statusBar.Text = "File saved.";
				}
				else 
					MessageBox.Show("No file has been loaded...", "Error");
			}
			else
			{
				this.statusBar.Text = "";
			}
		}

		
		/// <summary>
		/// Starts the "Save" dialog allowing the user to save a labeled MIT XML file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void saveXmlBtn_Click(object sender, System.EventArgs e)
		{
			saveXmlMI_Click(sender, e);
		}

		
		/// <summary>
		/// Exits the program
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.Application.Exit();
		}


		/// <summary>
		/// Undo the previous undoable Command.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void undoMI_Click(object sender, System.EventArgs e)
		{
			CM.Undo();
			panel.Refresh();
			
		}


		/// <summary>
		/// Redo the previous Undo.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void redoMI_Click(object sender, System.EventArgs e)
		{
			CM.Redo();
			panel.Refresh();
		}


		/// <summary>
		/// Changes the GUI mode to select
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void selectModeBtn_CheckedChanged(object sender, System.EventArgs e)
		{			
			changeModeSelect();
		}

		
		/// <summary>
		/// Changes the GUI mode to edit
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void editModeBtn_CheckedChanged(object sender, System.EventArgs e)
		{
			changeModeEdit();
		}

		
		/// <summary>
		/// Resizes the InkOverlay's strokes when the application's window is resized
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void Form1_Resize(object sender, System.EventArgs e)
		{
			FormResize();
		}


		/// <summary>
		/// Labels a current selection with the current shapeLabel's properties.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void labelButton_Click(object sender, System.EventArgs e)
		{
			// If we are in select mode and have selected something
			if (oInk.EditingMode == InkOverlayEditingMode.Select && oInk.Selection.Count > 0)
			{
				this.statusBar.Text = "Creating Label...";
				
				// Here is the strokes that are selected
				Microsoft.Ink.Strokes inkStrokes = oInk.Selection;
				ArrayList selected = new ArrayList();

				// For each Ink stroke selected, add it to a temporary Sketch.Substroke ArrayList
				foreach (Microsoft.Ink.Stroke stroke in inkStrokes)
				{
					selected.Add(this.mIdToSubstroke[stroke.Id] as Sketch.Substroke);
				}

				// Get the new label and color to apply
				string label = (string)domainInfo.getLabels()[shapeLabels.SelectedIndex];
				System.Drawing.Color labelColor = domainInfo.getColor(label);
				
				// Create a labeled shape within the Sketch
				CM.ExecuteCommand(new CommandList.ApplyLabel(this.sketch, selected, label, labelColor,
					inkStrokes));
				
				this.statusBar.Text = "";
				
				panel.Refresh();
			}
		}

		
		/// <summary>
		/// Commits the strokes split during the editing mode.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void commitBtn_Click(object sender, System.EventArgs e)
		{
			// Split the Sketch.Strokes
			splitAll();

			// Reset the InkOverlay
			updateInkOverlay(this.sketch, this.oInk);

			panel.Refresh();
		}		
		

		/// <summary>
		/// Cancels any stroke splitting we have done in the editing mode
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void cancelBtn_Click(object sender, System.EventArgs e)
		{
			this.mIdToIndices.Clear();
			
			panel.Refresh();
		}


		/// <summary>
		/// Handles the InkOverlay to ensure that displayed strokes have not been moved.
		/// Code pulled from: http://windowssdk.msdn.microsoft.com/en-us/library/microsoft.ink.inkoverlay.selectionmoved.aspx
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Selection moving event</param>
		private void oInk_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
		{
			// Get the selection's bounding box
			Rectangle newBounds = oInk.Selection.GetBoundingBox();

			// Move to back to original spot
			oInk.Selection.Move(e.OldSelectionBoundingRect.Left - newBounds.Left,
				e.OldSelectionBoundingRect.Top - newBounds.Top);

			// Trick to insure that selection handles are updated
			oInk.Selection = oInk.Selection;
		}

		
		/// <summary>
		/// Handles the InkOverlay to ensure that displayed strokes have not been resized.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Selection resizing event</param>
		private void oInk_SelectionResized(object sender, InkOverlaySelectionResizedEventArgs e)
		{
			// Move to back to original spot
			oInk.Selection.ScaleToRectangle(e.OldSelectionBoundingRect);

			// Trick to insure that selection handles are updated
			oInk.Selection = oInk.Selection;
		}


		/// <summary>
		/// Handler for various SystemGesture in the InkOverlay.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the SystemGesture event that is being handled</param>
		private void oInk_SystemGesture(object sender, InkCollectorSystemGestureEventArgs e)
		{
			// Currently unused, but could act as a handler for gestures at a later date
			// if (e.Id == SystemGesture.RightTap) . . .
		}


		/*private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			// Apply a label
			if (e.Control && e.KeyCode == Keys.L)
			{
				Console.WriteLine("in 1");
				if (this.sketch != null && this.domainInfo != null &&
					this.oInk != null && this.oInk.Selection.Count > 0)
				{
					Console.WriteLine("in 2");
					this.labelButton_Click(sender, (System.EventArgs)e);
				}
			}
		}*/

		#endregion

		#region DRAW & DISPLAY CODE
		
		/// <summary>
		/// Paints all of the end and split points on the InkOverlay strokes whenever the InkOverlay
		/// is repainted.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes a paint events object specific to the event that is being handled</param>
		private void oInk_Painted(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics g = panel.CreateGraphics();
			
			if (this.highlightEnd.Checked == true)
			{
				// Highlights the endpoints
				drawEndPts(g, Color.Blue);
			
				// Highlights the stroke splitting points
				drawSplitPts(g, Color.Red);
			}
		}
								  
		
		/// <summary>
		/// Resizes the rendered InkOverlay so that the ink will be as large as possible while still
		/// displaying the file in its entirety.
		/// </summary>
		private void FormResize() 
		{
			this.panel.Width  = this.Width - this.vScrollBar.Width - 8;
			this.panel.Height = this.Height - this.toolBar.Height - 139;

			// Bottom Buttons
			this.labelButton.Location	= new System.Drawing.Point(this.Width - this.labelButton.Width - 20, this.Height - 110);	
			this.shapeLabels.Location	= new System.Drawing.Point(this.labelButton.Location.X - 180, this.Height - 110);
			this.selectModeBtn.Location = new System.Drawing.Point(20, this.Height - 110);
			this.editModeBtn.Location	= new System.Drawing.Point(40 + selectModeBtn.Width, this.Height - 110);
			this.commitBtn.Location		= new System.Drawing.Point(this.Width / 2 - this.commitBtn.Width - 10, this.Height - 110);
			this.cancelBtn.Location		= new System.Drawing.Point(this.Width / 2 + 10, this.Height - 110);

			this.statusBar.Location		= new System.Drawing.Point(0, this.Height - 71);
			this.progressBar.Location	= new System.Drawing.Point(this.Width - 304, this.Height - 71);
			this.statusBar.Size			= new Size(this.progressBar.Location.X, this.statusBar.Size.Height);
			
			// Zoom Buttons
			this.zoomTextBox.Location	= new System.Drawing.Point(this.Width - 143, 8);
			this.zoomLabel.Location		= new System.Drawing.Point(this.Width - 200, 7);
			this.zoomInBtn.Location		= new System.Drawing.Point(this.Width - 78, 8);
			this.zoomOutBtn.Location	= new System.Drawing.Point(this.Width - 44, 8);
			this.panelWidthBox.Location = new System.Drawing.Point(this.Width - 320, 8);
			
			// Endpoint Button
			this.highlightEnd.Location = new System.Drawing.Point(this.Width - 475, 8);

			// Position and stretch the scroll bars
			this.hScrollBar.Location	= new System.Drawing.Point(0, this.Height - 136);
			this.vScrollBar.Location	= new System.Drawing.Point(this.Width - this.vScrollBar.Width - 8, this.toolBar.Height + 2);
			this.hScrollBar.Size		= new Size(this.panel.Width, this.hScrollBar.Height);
			this.vScrollBar.Size		= new Size(this.vScrollBar.Width, this.panel.Height);
			
			// Actual stroke bounding box (in Ink Space)
			int strokeWidth  = oInk.Ink.Strokes.GetBoundingBox().Width;
			int strokeHeight = oInk.Ink.Strokes.GetBoundingBox().Height;
			
			int inkWidth  = panel.Width - 60;
			int inkHeight = panel.Height - 60;
		
			Graphics g = panel.CreateGraphics();

			if (strokeWidth != 0 && strokeHeight != 0)
			{
				// If we want to scale by the panel's width
				if (this.panelWidthBox.Checked == true)
				{
					// Convert the rendering space from Ink Space to Pixels
					System.Drawing.Point botRight = new System.Drawing.Point(strokeWidth, strokeHeight);
					oInk.Renderer.InkSpaceToPixel(g, ref botRight); 				
				
					System.Drawing.Point topLeft = new System.Drawing.Point(0, 0);
					oInk.Renderer.InkSpaceToPixel(g, ref topLeft); 				
				
					System.Drawing.Point scalePt = new System.Drawing.Point(botRight.X - topLeft.X, botRight.Y - topLeft.Y);
				
					// Scale the rendered strokes by the smallest (x or y) scaling factor
					float xScale = (float)inkWidth / (float)scalePt.X;
					float yScale = (float)inkHeight / (float)scalePt.Y;
		
					//float scale = xScale < yScale ? xScale : yScale;
					float scale = xScale;

					oInk.Renderer.Scale(scale, scale, false);
				
					// Update the scaling factors
					totalScale *= scale;
					prevScale = totalScale;

					// Update the user-displayed zoom
					zoomTextBox.Text = totalScale.ToString();
				}
				else
				{
					if (prevScale != 0.0f)
						oInk.Renderer.Scale(newScale / prevScale, newScale / prevScale, false);
					
					totalScale = prevScale = newScale;	
				}
			}

			// Re-map the scroll bars		
			System.Drawing.Point temp   = new System.Drawing.Point(strokeWidth, strokeHeight);
			System.Drawing.Point origin = new System.Drawing.Point(0, 0);
			oInk.Renderer.InkSpaceToPixel(g, ref temp);
			oInk.Renderer.InkSpaceToPixel(g, ref origin);
		
			this.hScrollBar.Maximum = Math.Max(0, (temp.X - origin.X) - inkWidth);
			this.vScrollBar.Maximum = Math.Max(0, (temp.Y - origin.Y) - inkHeight);

			this.ScrollPos.X = Math.Min(this.hScrollBar.Value, this.hScrollBar.Maximum);
			this.ScrollPos.Y = Math.Min(this.vScrollBar.Value, this.vScrollBar.Maximum);
			HandleScroll();
		}
		 

		/// <summary>
		/// Creates a new ViewTransform matrix that shifts the image based on the scrollbar positions.
		/// Converts the scrollbar pixels into Ink Space coordinates and then shifts the rendered ink
		/// by that amount.
		/// </summary>
		private void HandleScroll() 
		{
			// Convert to ink space.  Since we are updating the view 
			// transform of the renderer, the origin of the ink coordinates 
			// may have moved relative to the screen.   
			System.Drawing.Point scrollAmount = new System.Drawing.Point(-this.ScrollPos.X, -this.ScrollPos.Y);
			System.Drawing.Point origin = new System.Drawing.Point(0, 0);

			Graphics g = panel.CreateGraphics();
			oInk.Renderer.PixelToInkSpace(g, ref scrollAmount);
			oInk.Renderer.PixelToInkSpace(g, ref origin);
			
			// Create a translation transform based on the current x and y 
			// scroll positions.
			System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
			m.Translate((scrollAmount.X - origin.X) * this.totalScale, (scrollAmount.Y - origin.Y) * this.totalScale);
			
			oInk.Renderer.SetViewTransform(m);

			panel.Refresh();
		}


		/// <summary>
		/// Activates whenever the horizontal scroll bar is used.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes a scroll object specific to the event that is being handled</param>
		private void hScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			this.ScrollPos.X = e.NewValue;
			HandleScroll();

			panel.Refresh();
		}


		/// <summary>
		/// Activates whenever the vertical scroll bar is used.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes a scroll object specific to the event that is being handled</param>
		private void vScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
		{
			this.ScrollPos.Y = e.NewValue;
			HandleScroll();

			panel.Refresh();
		}


		/// <summary>
		/// Highlights the end points of strokes in the specified color.
		/// </summary>
		/// <param name="g">Drawing panel graphics</param>
		/// <param name="color">Color to draw the points in</param>
		private void drawEndPts(Graphics g, Color color)
		{
			ArrayList pts = new ArrayList();
			
			foreach (Microsoft.Ink.Stroke s in oInk.Ink.Strokes)
			{
				System.Drawing.Point ep1 = s.GetPoint(0);
				System.Drawing.Point ep2 = s.GetPoint(s.PacketCount - 1);
				
				pts.Add(ep1);
				pts.Add(ep2);
			}

			highlightPoints(g, (System.Drawing.Point[])pts.ToArray(typeof(System.Drawing.Point)), color);
		}


		/// <summary>
		/// Highlights the points where a stroke will be split in the specified color.
		/// </summary>
		/// <param name="g">Drawing panel graphics</param>
		/// <param name="color">Color to draw the points in</param>
		private void drawSplitPts(Graphics g, Color color)
		{
			foreach (DictionaryEntry entry in this.mIdToIndices)
			{
				int mId = (int)entry.Key;
				
				float[] indices = (float[])((ArrayList)entry.Value).ToArray(typeof(float));
				
				Microsoft.Ink.Stroke s = oInk.Ink.Strokes[ (int)this.mIdToInkIndex[mId] ];

				if (s != null)
					highlightPoints(g, floatsToPoints(s, indices), color);
			}
		}


		/// <summary>
		/// Highlights a given array of points a certain color.
		/// </summary>
		/// <param name="g">The panel's graphics</param>
		/// <param name="points">Points to highlight</param>
		/// <param name="color">Color of the highlighted points</param>
		/// <remarks>Creates a new stroke from each point and renders the stroke/point using certain attributes
		/// such as color, width, and height</remarks>
		private void highlightPoints(Graphics g, System.Drawing.Point[] points, Color color) 
		{
			Microsoft.Ink.Ink highPts = new Ink();
				
			// Create strokes consisting of one point each
			// This is done so that we can draw the points in our InkOverlay and correctly scale the points
			// accordingly.
			for (int i = 0; i < points.Length; i++)
			{
				System.Drawing.Point[] p = new System.Drawing.Point[1];
				p[0] = points[i];
				highPts.CreateStroke(p);
			}

			// Display features of the point (Color, Width, and Height)
			Microsoft.Ink.DrawingAttributes da = new Microsoft.Ink.DrawingAttributes(color);
			da.Height = 115;
			da.Width  = 115;

			// Render each point
			for (int i = 0; i < highPts.Strokes.Count; i++)
			{
				oInk.Renderer.Draw(g, highPts.Strokes[i], da);
			}
		}
		

		/// <summary>
		/// Changes the current GUI mode to selection, allowing the user to select strokes.
		/// </summary>
		private void changeModeSelect() 
		{			
			oInk.EditingMode = InkOverlayEditingMode.Select;

			panel.BackColor = Color.White;
			selectModeBtn.ForeColor = Color.Fuchsia;
			editModeBtn.ForeColor   = Color.Black;
		
			panel.Refresh();
		}


		/// <summary>
		/// Changes the current GUI mode to editing, allowing the user to split strokes.
		/// </summary>
		private void changeModeEdit()
		{
			// WHY DON'T YOU WORK?!?!
			oInk.Enabled = false;
			oInk.Selection.Clear();
			
			oInk.EditingMode = InkOverlayEditingMode.Ink;
			oInk.DefaultDrawingAttributes.Color = Color.Red;
			
			panel.BackColor			= Color.Beige;
			selectModeBtn.ForeColor = Color.Black;
			editModeBtn.ForeColor   = Color.Fuchsia;
			
			oInk.Enabled = true;
			
			panel.Refresh();
		}
		
		
		/// <summary>
		/// If the checkbox is checked, then update the ink's zoom based on the panel width. Otherwise
		/// we will update the zoom based on the value in the zoom text box.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void panelWidthBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (panelWidthBox.Checked == false) 
			{
				this.zoomTextBox.Enabled = true;
				this.zoomInBtn.Enabled   = true;
				this.zoomOutBtn.Enabled  = true;
				getScaleFromBox();
			}
			else
			{
				this.zoomTextBox.Enabled = false;
				this.zoomInBtn.Enabled   = false;
				this.zoomOutBtn.Enabled  = false;
				FormResize();
			}
		}

		
		/// <summary>
		/// Do nothing... 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void highlightEnd_CheckedChanged(object sender, System.EventArgs e)
		{
			// Don't delete
			panel.Refresh();
		}



		/// <summary>
		/// Updates the zoom factor of the image from the text entered into the zoom text box
		/// every time the text is changed.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void zoomTextBox_TextChanged(object sender, System.EventArgs e)
		{
			getScaleFromBox();	
		}


		/// <summary>
		/// Zooms in by a factor of 1 each time you click the button.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void zoomInBtn_Click(object sender, System.EventArgs e)
		{
			if (newScale >= 1.0)
				newScale = (float)Math.Floor(newScale + 1.4);	
			else
				newScale = (float)(newScale * 2.0);
		
			this.zoomTextBox.Text = newScale.ToString();
			FormResize();
		}
		
		
		/// <summary>
		/// Zooms out by a factor of 1 each time you click the button.
		/// When the zoom scale is less than 1, it divides the zoom by half each time.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void zoomOutBtn_Click(object sender, System.EventArgs e)
		{
			if (newScale >= 2.0) 
				newScale = (float)Math.Floor(newScale - 1.0);
			else
				newScale = (float)(newScale / 2.0);
		
			this.zoomTextBox.Text = newScale.ToString();
			FormResize();
		}

		
		/// <summary>
		/// Updates the zoom factor of the image from the text entered into the zoom text box.
		/// </summary>
		private void getScaleFromBox()
		{
			try
			{
				newScale = (float)Convert.ToDouble(zoomTextBox.Text);
				FormResize();
				this.statusBar.Text = "";
			}
			catch
			{
				this.statusBar.Text = "Zoom cannot be updated: Invalid float";
			}
		}

		
		/// <summary>
		/// Updates the colors of the InkOverlay strokes based on the labels we have in the domain.
		/// </summary>
		private void updateColorsFromLabels()
		{
			if (this.domainInfo != null && this.sketch != null)
			{
				Sketch.Substroke[] substrokes = this.sketch.Substrokes;

				for (int i = 0; i < substrokes.Length; i++)
				{
					string[] labels = substrokes[i].GetLabels();
					
					// Make sure we have a valid domain loaded
					if (labels.Length > 0)
						oInk.Ink.Strokes[i].DrawingAttributes.Color = (Color)(domainInfo.getColor(labels[0]));
				}
			}
		}


		/// <summary>
		/// Displays a ToolTip for a Substroke based on where the mouse is located.
		/// 
		/// NOTE: It's a little jittery when you move, but I'm not sure if that is due to the closest Substroke
		/// calculations or redisplaying it.		
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void oInk_MouseMove(object sender, CancelMouseEventArgs e)
		{
			// Get the current mouse position and convert it into InkSpace
			System.Drawing.Point mousePt = new System.Drawing.Point(e.X, e.Y);
			oInk.Renderer.PixelToInkSpace(panel.CreateGraphics(), ref mousePt);

			// Get the closest Microsoft Stroke (in the InkOverlay)
			float strokePt, distance;
			Microsoft.Ink.Stroke closestMSubstroke = oInk.Ink.NearestPoint(mousePt, out strokePt, out distance);

			// If the distance to a Substroke is less than some threshold...
			if (distance < 10)
			{
				// Get the Sketch's corresponding Substroke
				Substroke closestSubstroke = (Substroke)this.mIdToSubstroke[closestMSubstroke.Id];
			
				// Create the ToolTip string with and Id and Labels
				string toolTipLabel = "Id: " + closestSubstroke.XmlAttrs.Id.ToString() + "\nLabels: ";
				string[] labels = closestSubstroke.GetLabels();
				for (int i = 0; i < labels.Length; i++)
				{
					toolTipLabel += labels[i];
				
					if (i < labels.Length - 1)
						toolTipLabel += ", ";        
				}

				toolTipLabel += "\nProbabilities: " + closestSubstroke.GetFirstBelief();
				
//				double[] probs = closestSubstroke.GetBeliefs();
//				for ( int i = 0; i < probs.Length; i++ ) 
//				{
//					toolTipLabel += probs[i].ToString();
					
//					if ( i < probs.Length - 1 )
//						toolTipLabel += ", ";
//				}

				// Show the ToolTip
				this.toolTip.SetToolTip(panel, toolTipLabel);
				this.toolTip.Active = true;
			}
			else
			{
				// Don't show the ToolTip if we aren't close enough to any Substroke
				this.toolTip.Active = false;
			}	
		}

		#endregion		
		
		#region STROKE SPLITTING
	
		/// <summary>
		/// Updates the InkOverlay by deleting all of the currently stored strokes and creating
		/// new strokes from the given Sketch's Substrokes.
		/// </summary>
		/// <param name="sketch">Sketch to extract substrokes from</param>
		/// <param name="oInk">InkOverlay to update</param>
		private void updateInkOverlay(Sketch.Sketch sketch, Microsoft.Ink.InkOverlay oInk)
		{
			// Make sure we have a valid Sketch
			if (sketch != null)
			{
				oInk.Enabled = false;
				oInk.Ink.DeleteStrokes();
				this.mIdToIndices.Clear();
				this.mIdToSubstroke.Clear();
				this.mIdToInkIndex.Clear();
				
				Sketch.Substroke[] substrokes = sketch.Substrokes;
				
				// Go through all of the Sketch's Substrokes
				for (int i = 0; i < substrokes.Length; i++)
				{
					Sketch.Point[] sketchPts = substrokes[i].Points;
					System.Drawing.Point[] simplePts = new System.Drawing.Point[sketchPts.Length];

					// Create a System.Drawing.Point array from the Sketch.Point values
					for (int k = 0; k < simplePts.Length; k++)
					{
						simplePts[k] = new System.Drawing.Point((int)sketchPts[k].X, (int)sketchPts[k].Y);
					}
					
					// Create the InkOverlay stroke
					oInk.Ink.CreateStroke(simplePts);

					// Get the color
					int argb = 0;
					if (substrokes[i].XmlAttrs.Color != null)
						argb = (int)substrokes[i].XmlAttrs.Color;
						
					// Make it the color if it exists
					if (argb != 0)
						oInk.Ink.Strokes[oInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.FromArgb(argb);
					
					// Update the hashtables with proper references
					int mId = oInk.Ink.Strokes[oInk.Ink.Strokes.Count - 1].Id;
					this.mIdToInkIndex.Add( mId, oInk.Ink.Strokes.Count - 1 );
					this.mIdToSubstroke.Add( mId, substrokes[i] );
				}

				// Move the Ink's origin to the upper left-hand corner.
				oInk.Ink.Strokes.Move(-1 * oInk.Ink.GetBoundingBox().X, -1 * oInk.Ink.GetBoundingBox().Y);
				oInk.Enabled = true;

				if (domainInfo != null)
					updateColorsFromLabels();
			}
		}


		/// <summary>
		/// Checks what mode we are in to see if we should split strokes.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void strokeSplitHandler(object sender, InkCollectorStrokeEventArgs e)
		{
			// If we are in editing (stroke splitting) mode
			if (oInk.EditingMode == InkOverlayEditingMode.Ink)
				inkModeStroke(sender, e);
			else
				selectModeStroke(sender, e);
		}


		/// <summary>
		/// Does nothing.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void selectModeStroke(object sender, InkCollectorStrokeEventArgs e)
		{
			// Currently does nothing
		}


		/// <summary>
		/// Finds the intersections of the editing stroke with the actual stroke collection.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void inkModeStroke(object sender, InkCollectorStrokeEventArgs e)
		{			
			// Find the float intersection points
			float[] inter = e.Stroke.FindIntersections(e.Stroke.Ink.Strokes);
			System.Drawing.Point[] pointInter = new System.Drawing.Point[inter.Length];
		
			// Find the actual points of intersection
			for (int i = 0; i < pointInter.Length; ++i)
				pointInter[i] = floatToPoint(e.Stroke, inter[i]);

			// Initialize the progress bar
			this.statusBar.Text = "Calculating intersection points...";
			this.progressBar.Value = 0;
			this.progressBar.Maximum = oInk.Ink.Strokes.Count;

			/* Go through every stroke to see if the pointInter intersect it
			foreach (Microsoft.Ink.Stroke s in oInk.Ink.Strokes)
			{
				// Go through every possible point Intersection
				for (int i = 0; i < pointInter.Length; ++i)
				{
					// Do not consider the red stroke we drew
					if (s.Id != e.Stroke.Id)
					{
						float theDistance;
						float theFIndex = s.NearestPoint(pointInter[i], out theDistance);
						
						// This is the closeness factor... we would like it as small as possible without missing any intersections
						// While most of the time theDistance is 0.0, sometimes they have been on the order of 70.0
						if (theDistance <= 0.0f)
						{
							if (mIdToIndices[s.Id] == null)
								mIdToIndices[s.Id] = new ArrayList();

							if ( !((ArrayList)mIdToIndices[s.Id]).Contains(theFIndex) )
								((ArrayList)mIdToIndices[s.Id]).Add(theFIndex);
							
							//Console.WriteLine("Intersected " + s.Id);
							
							// We cannot break here since a single point may intersect more than one stroke
							//break;
						}
					}
				}

				this.progressBar.Value += 1;
			}

			// Delete our red erase line
			oInk.Ink.DeleteStroke(e.Stroke);*/

			CM.ExecuteCommand(new CommandList.SplitStrokeAt(this.oInk, e.Stroke, pointInter,
				this.mIdToIndices, this.progressBar));

			this.statusBar.Text = "";
			this.progressBar.Value = 0;

			panel.Refresh();
		}


		/// <summary>
		/// Splits the actual ink object into multiple strokes, indicated by
		/// the changes made in ink mode
		/// </summary>
		private void splitAll()
		{
			// Initialize the progress bar
			this.statusBar.Text		 = "Splitting strokes...";
			this.progressBar.Value	 = 0;
			this.progressBar.Maximum = mIdToIndices.Count;

			// Go through all Microsoft.Ink.Stroke Id's within our hashtable
			foreach (DictionaryEntry mId in this.mIdToIndices)
			{
				// Get the float array of indices where we should split a stroke
				float[] fIndices = (float[])((ArrayList)mId.Value).ToArray(typeof(float));
				
				// Convert the array to ints
				int[] indices = new int[fIndices.Length];
				for (int i = 0; i < indices.Length; i++)
					indices[i] = Convert.ToInt32(fIndices[i]);

				// Find the corresponding Substroke
				Substroke currSubstroke = (Sketch.Substroke)this.mIdToSubstroke[ mId.Key ];
				
				// Split it
				if (currSubstroke != null)
					currSubstroke.SplitAt(indices);

				this.progressBar.Value += 1;
			}

			this.statusBar.Text = "";
			this.progressBar.Value = 0;
		}

	
		/// <summary>
		/// Get the Point at a floating point index. 
		/// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/tpcsdk10/lonestar/Microsoft.Ink/Classes/stroke/Methods/getpoint.asp.
		/// </summary>
		/// <param name="theStroke">Stroke</param>
		/// <param name="theFIndex">Floating point index</param>
		/// <returns>A point on the stroke</returns>
		private System.Drawing.Point floatToPoint(Microsoft.Ink.Stroke theStroke, float theFIndex)
		{
			System.Drawing.Point ptResult = theStroke.GetPoint((int)theFIndex);
			float theFraction = theFIndex - (int)theFIndex;
			if (theFraction > 0.0f)
			{
				System.Drawing.Point ptDelta = theStroke.GetPoint((int)theFIndex + 1);
				ptResult.X += (int)((ptDelta.X - ptResult.X) * theFraction);
				ptResult.Y += (int)((ptDelta.Y - ptResult.Y) * theFraction);
			}
			return ptResult;
		}


		/// <summary>
		/// Get the Point array from an array of float indices
		/// </summary>
		/// <param name="stroke">Stroke</param>
		/// <param name="indices">Floating point indices</param>
		/// <returns>An array of points on the stroke</returns>
		private System.Drawing.Point[] floatsToPoints(Microsoft.Ink.Stroke stroke, float[] indices)
		{
			System.Drawing.Point[] points = new System.Drawing.Point[indices.Length];
			
			int current = 0;
			foreach (float index in indices)
			{
				points[current] = floatToPoint(stroke, index);
				++current;
			}
			
			return points;
		}
		
		#endregion	
	}
}
