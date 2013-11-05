using System;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Diagnostics;

using Microsoft.Ink;

using Converter;
using Sketch;

namespace UserStudyUI
{
	public delegate void FirePauseCountdown();
	public delegate void PauseCountdownFinished();


	/// <summary>
	/// User Study UI form.  This form defines the Windows WIMP/sketch UI for the user app front end.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Internals
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Internals

		/// <summary>
		/// Stores the sketch that the user is currently editing in this form.
		/// </summary>
		private Sketch.Sketch sketch;

		/// <summary>
		/// Stores the notes the user has currently written in the notes panel.
		/// </summary>
		private Sketch.Sketch notes;

		/// <summary>
		/// The JNT converter for converting Microsoft Strokes to Sketch strokes.
		/// </summary>
		private Converter.ReadJnt readJnt;

		/// <summary>
		/// The fraction of the form that the sketch panel fills.  Used during window resizing.
		/// </summary>
		private float sketchFraction = 0.6F;

		/// <summary>
		/// Stores the name of the current sketch file being used, if any.
		/// </summary>
		private string currentFileName = "";

		/// <summary>
		/// The current recognizer used on Sketch data.
		/// </summary>
		private Recognizer recognizer;

		/// <summary>
		/// True iff this front end is currently receiving 
		/// recognition results and updating the screen
		/// accordingly.
		/// </summary>
		private bool recognitionReadActive = false;

		/// <summary>
		/// Recognition trigger modes and current mode variable
		/// </summary>
		private enum Trigger {Button, Pause, LassoTap};
		private Trigger currentTrigger = Trigger.Button;

		/// <summary>
		/// The Pause trigger countdown timer for running the
		/// Pause trigger
		/// </summary>
		private PauseTriggerCountdownTimer pauseCountdownTimer;

		/// <summary>
		/// Pause trigger countdown even that this form triggers
		/// whenever the Pause trigger is fired (
		/// </summary>
		public event FirePauseCountdown PauseTriggerFiredEvent;

		/// <summary>
		/// Stores the last selction of strokes selected while using
		/// the Lasso-tap recognition trigger.  
		/// </summary>
		private Microsoft.Ink.Strokes lastSelection;

		// Begin Windows Forms private variables
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem11;
		private System.Windows.Forms.ToolBar toolBar1;

		private System.Windows.Forms.RadioButton sketchRadio;
		private System.Windows.Forms.RadioButton eraseRadio;
		private System.Windows.Forms.RadioButton lassoRadio;
		private System.Windows.Forms.Button recognizeButton;
		private System.Windows.Forms.MenuItem openItem;
		private System.Windows.Forms.MenuItem saveItem;
		private System.Windows.Forms.MenuItem saveAsItem;
		private System.Windows.Forms.MenuItem exitItem;
		private System.Windows.Forms.MenuItem undoItem;
		private System.Windows.Forms.MenuItem redoItem;
		private System.Windows.Forms.MenuItem copyItem;
		private System.Windows.Forms.MenuItem pasteItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.Splitter bottomSplitter;
		private System.Windows.Forms.Splitter sketchSplitter;
		private Microsoft.Ink.InkPicture sketchPicture;
		private Microsoft.Ink.InkPicture notesPicture;
		private System.Windows.Forms.Panel sketchPanel;
		private System.Windows.Forms.Panel notesPanel;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem zoomInItem;
		private System.Windows.Forms.MenuItem zoomOutItem;
		private System.Windows.Forms.MenuItem adminToggleMenuItem;
		private System.Windows.Forms.MenuItem adminMenuItem;
		private System.Windows.Forms.MenuItem buttonTriggerMenuItem;
		private System.Windows.Forms.MenuItem pauseTriggerMenuItem;
		private System.Windows.Forms.MenuItem lassoTriggerMenuItem;
		private System.Windows.Forms.MenuItem newSketchItem;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		/// <summary>
		/// Main form constructor. Creates all the components in the form and initializes InkOverlay components.
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

			// Create the Sketches
			sketch = new Sketch.Sketch();
			notes  = new Sketch.Sketch();
			sketch.XmlAttrs.Id = System.Guid.NewGuid();
			sketch.XmlAttrs.Units = "himetric";
			notes.XmlAttrs.Id = System.Guid.NewGuid();
			notes.XmlAttrs.Units = "himetric";

			// Create JNT converter for converting Ink strokes to sketch strokes
			readJnt = new ReadJnt("null filename");

			// Create resize hooks for Ink Pictures
			sketchPanel.Resize += new EventHandler(sketchPanel_Resize);
			notesPanel.Resize += new EventHandler(notesPanel_Resize);

			// TODO Configure Ink Picture properties (turn on time collection)
			

			// Hook into Ink Picture events
			sketchPicture.Ink.InkAdded += new StrokesEventHandler(inkAdded2Sketch);
			sketchPicture.Ink.InkDeleted += new StrokesEventHandler(inkDeleted2Sketch);
			sketchPicture.SelectionChanged += new InkOverlaySelectionChangedEventHandler(inkSelected2Sketch);
			notesPicture.Ink.InkAdded += new StrokesEventHandler(inkAdded2Notes);
			notesPicture.Ink.InkDeleted += new StrokesEventHandler(inkDeleted2Notes);

			// Create resize handler for sketch/notes splitter
			this.Resize += new EventHandler(Form1_Resize);

			// Create the dummy recognizer and subscribe this class to it
			recognizer = new WireLabelRecognizer();
			recognizer.RecognitionEvent += new RecognitionResultsEventHandler(recognizer_RecognitionEvent);

			// Create the Pause countdown mechanism and 
			// subscribe to its countdown-finished event
			pauseCountdownTimer = new PauseTriggerCountdownTimer(this);
			pauseCountdownTimer.CountdownFinishedEvent += new PauseCountdownFinished(this.triggerRecognition);

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



		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Windows Form Designer generated code
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Windows Form Designer generated code




		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.newSketchItem = new System.Windows.Forms.MenuItem();
			this.openItem = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.saveItem = new System.Windows.Forms.MenuItem();
			this.saveAsItem = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.exitItem = new System.Windows.Forms.MenuItem();
			this.adminToggleMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.undoItem = new System.Windows.Forms.MenuItem();
			this.redoItem = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.copyItem = new System.Windows.Forms.MenuItem();
			this.pasteItem = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.zoomInItem = new System.Windows.Forms.MenuItem();
			this.zoomOutItem = new System.Windows.Forms.MenuItem();
			this.adminMenuItem = new System.Windows.Forms.MenuItem();
			this.buttonTriggerMenuItem = new System.Windows.Forms.MenuItem();
			this.pauseTriggerMenuItem = new System.Windows.Forms.MenuItem();
			this.lassoTriggerMenuItem = new System.Windows.Forms.MenuItem();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.sketchRadio = new System.Windows.Forms.RadioButton();
			this.eraseRadio = new System.Windows.Forms.RadioButton();
			this.lassoRadio = new System.Windows.Forms.RadioButton();
			this.recognizeButton = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.bottomSplitter = new System.Windows.Forms.Splitter();
			this.sketchPanel = new System.Windows.Forms.Panel();
			this.sketchPicture = new Microsoft.Ink.InkPicture();
			this.sketchSplitter = new System.Windows.Forms.Splitter();
			this.notesPanel = new System.Windows.Forms.Panel();
			this.notesPicture = new Microsoft.Ink.InkPicture();
			this.sketchPanel.SuspendLayout();
			this.notesPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem2,
																					  this.adminMenuItem});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.newSketchItem,
																					  this.openItem,
																					  this.menuItem4,
																					  this.saveItem,
																					  this.saveAsItem,
																					  this.menuItem7,
																					  this.exitItem,
																					  this.adminToggleMenuItem});
			this.menuItem1.Text = "File";
			// 
			// newSketchItem
			// 
			this.newSketchItem.Index = 0;
			this.newSketchItem.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
			this.newSketchItem.Text = "New Sketch";
			this.newSketchItem.Click += new System.EventHandler(this.newSketchItem_Click);
			// 
			// openItem
			// 
			this.openItem.Index = 1;
			this.openItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.openItem.Text = "Open Sketch...";
			this.openItem.Click += new System.EventHandler(this.openItem_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "-";
			// 
			// saveItem
			// 
			this.saveItem.Index = 3;
			this.saveItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.saveItem.Text = "Save";
			this.saveItem.Click += new System.EventHandler(this.saveItem_Click);
			// 
			// saveAsItem
			// 
			this.saveAsItem.Index = 4;
			this.saveAsItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftS;
			this.saveAsItem.Text = "Save Sketch as...";
			this.saveAsItem.Click += new System.EventHandler(this.saveAsItem_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 5;
			this.menuItem7.Text = "-";
			// 
			// exitItem
			// 
			this.exitItem.Index = 6;
			this.exitItem.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
			this.exitItem.Text = "Exit";
			this.exitItem.Click += new System.EventHandler(this.exitItem_Click);
			// 
			// adminToggleMenuItem
			// 
			this.adminToggleMenuItem.Checked = true;
			this.adminToggleMenuItem.Index = 7;
			this.adminToggleMenuItem.Text = "Toggle Admin Mode";
			this.adminToggleMenuItem.Click += new System.EventHandler(this.adminToggleMenuItem_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.undoItem,
																					  this.redoItem,
																					  this.menuItem11,
																					  this.copyItem,
																					  this.pasteItem,
																					  this.menuItem3,
																					  this.zoomInItem,
																					  this.zoomOutItem});
			this.menuItem2.Text = "Edit";
			// 
			// undoItem
			// 
			this.undoItem.Index = 0;
			this.undoItem.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
			this.undoItem.Text = "Undo";
			this.undoItem.Click += new System.EventHandler(this.undoItem_Click);
			// 
			// redoItem
			// 
			this.redoItem.Index = 1;
			this.redoItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftZ;
			this.redoItem.Text = "Redo";
			this.redoItem.Click += new System.EventHandler(this.redoItem_Click);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 2;
			this.menuItem11.Text = "-";
			// 
			// copyItem
			// 
			this.copyItem.Index = 3;
			this.copyItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.copyItem.Text = "Copy";
			this.copyItem.Click += new System.EventHandler(this.copyItem_Click);
			// 
			// pasteItem
			// 
			this.pasteItem.Index = 4;
			this.pasteItem.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
			this.pasteItem.Text = "Paste";
			this.pasteItem.Click += new System.EventHandler(this.pasteItem_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 5;
			this.menuItem3.Text = "-";
			// 
			// zoomInItem
			// 
			this.zoomInItem.Index = 6;
			this.zoomInItem.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.zoomInItem.Text = "Zoom in";
			this.zoomInItem.Click += new System.EventHandler(this.zoomInItem_Click);
			// 
			// zoomOutItem
			// 
			this.zoomOutItem.Index = 7;
			this.zoomOutItem.Shortcut = System.Windows.Forms.Shortcut.ShiftF1;
			this.zoomOutItem.Text = "Zoom out";
			this.zoomOutItem.Click += new System.EventHandler(this.zoomOutItem_Click);
			// 
			// adminMenuItem
			// 
			this.adminMenuItem.Index = 2;
			this.adminMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.buttonTriggerMenuItem,
																						  this.pauseTriggerMenuItem,
																						  this.lassoTriggerMenuItem});
			this.adminMenuItem.Text = "Admin";
			// 
			// buttonTriggerMenuItem
			// 
			this.buttonTriggerMenuItem.Checked = true;
			this.buttonTriggerMenuItem.Index = 0;
			this.buttonTriggerMenuItem.Text = "Button";
			this.buttonTriggerMenuItem.Click += new System.EventHandler(this.buttonTriggerMenuItem_Click);
			// 
			// pauseTriggerMenuItem
			// 
			this.pauseTriggerMenuItem.Index = 1;
			this.pauseTriggerMenuItem.Text = "Pause";
			this.pauseTriggerMenuItem.Click += new System.EventHandler(this.pauseTriggerMenuItem_Click);
			// 
			// lassoTriggerMenuItem
			// 
			this.lassoTriggerMenuItem.Index = 2;
			this.lassoTriggerMenuItem.Text = "Lasso-Tap using Ink Selection";
			this.lassoTriggerMenuItem.Click += new System.EventHandler(this.lassoTriggerMenuItem_Click);
			// 
			// toolBar1
			// 
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(536, 42);
			this.toolBar1.TabIndex = 6;
			// 
			// sketchRadio
			// 
			this.sketchRadio.Appearance = System.Windows.Forms.Appearance.Button;
			this.sketchRadio.Checked = true;
			this.sketchRadio.Location = new System.Drawing.Point(8, 8);
			this.sketchRadio.Name = "sketchRadio";
			this.sketchRadio.Size = new System.Drawing.Size(104, 32);
			this.sketchRadio.TabIndex = 8;
			this.sketchRadio.TabStop = true;
			this.sketchRadio.Text = "Sketch Tool";
			this.sketchRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// eraseRadio
			// 
			this.eraseRadio.Appearance = System.Windows.Forms.Appearance.Button;
			this.eraseRadio.Location = new System.Drawing.Point(120, 8);
			this.eraseRadio.Name = "eraseRadio";
			this.eraseRadio.Size = new System.Drawing.Size(104, 32);
			this.eraseRadio.TabIndex = 9;
			this.eraseRadio.Text = "Erase Tool";
			this.eraseRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lassoRadio
			// 
			this.lassoRadio.Appearance = System.Windows.Forms.Appearance.Button;
			this.lassoRadio.Enabled = false;
			this.lassoRadio.Location = new System.Drawing.Point(232, 8);
			this.lassoRadio.Name = "lassoRadio";
			this.lassoRadio.Size = new System.Drawing.Size(104, 32);
			this.lassoRadio.TabIndex = 10;
			this.lassoRadio.Text = "Lasso Tool";
			this.lassoRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lassoRadio.Visible = false;
			this.lassoRadio.CheckedChanged += new System.EventHandler(this.lassoRadio_CheckedChanged);
			// 
			// recognizeButton
			// 
			this.recognizeButton.Location = new System.Drawing.Point(408, 8);
			this.recognizeButton.Name = "recognizeButton";
			this.recognizeButton.Size = new System.Drawing.Size(112, 32);
			this.recognizeButton.TabIndex = 11;
			this.recognizeButton.Text = "Recognize";
			this.recognizeButton.Click += new System.EventHandler(this.recognizeButton_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 428);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(536, 24);
			this.statusBar1.TabIndex = 14;
			this.statusBar1.Text = "statusBar1";
			// 
			// bottomSplitter
			// 
			this.bottomSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomSplitter.Location = new System.Drawing.Point(0, 425);
			this.bottomSplitter.Name = "bottomSplitter";
			this.bottomSplitter.Size = new System.Drawing.Size(536, 3);
			this.bottomSplitter.TabIndex = 15;
			this.bottomSplitter.TabStop = false;
			// 
			// sketchPanel
			// 
			this.sketchPanel.AutoScroll = true;
			this.sketchPanel.Controls.Add(this.sketchPicture);
			this.sketchPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sketchPanel.Location = new System.Drawing.Point(0, 42);
			this.sketchPanel.Name = "sketchPanel";
			this.sketchPanel.Size = new System.Drawing.Size(536, 262);
			this.sketchPanel.TabIndex = 16;
			// 
			// sketchPicture
			// 
			this.sketchPicture.BackColor = System.Drawing.Color.White;
			this.sketchPicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.sketchPicture.Location = new System.Drawing.Point(0, 0);
			this.sketchPicture.MarginX = -2147483648;
			this.sketchPicture.MarginY = -2147483648;
			this.sketchPicture.Name = "sketchPicture";
			this.sketchPicture.Size = new System.Drawing.Size(536, 262);
			this.sketchPicture.TabIndex = 0;
			this.sketchPicture.Stroke += new Microsoft.Ink.InkCollectorStrokeEventHandler(this.sketchPicture_Stroke);
			// 
			// sketchSplitter
			// 
			this.sketchSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.sketchSplitter.Location = new System.Drawing.Point(0, 304);
			this.sketchSplitter.Name = "sketchSplitter";
			this.sketchSplitter.Size = new System.Drawing.Size(536, 16);
			this.sketchSplitter.TabIndex = 17;
			this.sketchSplitter.TabStop = false;
			this.sketchSplitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.sketchSplitter_SplitterMoved);
			// 
			// notesPanel
			// 
			this.notesPanel.AutoScroll = true;
			this.notesPanel.Controls.Add(this.notesPicture);
			this.notesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notesPanel.Location = new System.Drawing.Point(0, 320);
			this.notesPanel.Name = "notesPanel";
			this.notesPanel.Size = new System.Drawing.Size(536, 105);
			this.notesPanel.TabIndex = 18;
			// 
			// notesPicture
			// 
			this.notesPicture.BackColor = System.Drawing.Color.White;
			this.notesPicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.notesPicture.Location = new System.Drawing.Point(0, 0);
			this.notesPicture.MarginX = -2147483648;
			this.notesPicture.MarginY = -2147483648;
			this.notesPicture.Name = "notesPicture";
			this.notesPicture.Size = new System.Drawing.Size(536, 104);
			this.notesPicture.TabIndex = 0;
			this.notesPicture.Stroke += new Microsoft.Ink.InkCollectorStrokeEventHandler(this.notesPicture_Stroke);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.ClientSize = new System.Drawing.Size(536, 452);
			this.Controls.Add(this.notesPanel);
			this.Controls.Add(this.sketchSplitter);
			this.Controls.Add(this.sketchPanel);
			this.Controls.Add(this.bottomSplitter);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.recognizeButton);
			this.Controls.Add(this.lassoRadio);
			this.Controls.Add(this.eraseRadio);
			this.Controls.Add(this.sketchRadio);
			this.Controls.Add(this.toolBar1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "User Study UI";
			this.sketchPanel.ResumeLayout(false);
			this.notesPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Windows Form UI Hooks and Callbacks
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Windows Form UI Hooks and Callbacks

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}


		/// <summary>
		/// Resizes the sketch Ink Image when the sketch panel changes shape.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void sketchPanel_Resize(object sender, EventArgs e)
		{
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
		}

		/// <summary>
		/// Resizes the notes Ink Image when the notes panel changes shape.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void notesPanel_Resize(object sender, EventArgs e)
		{
			this.resizeInkPicture(this.notesPicture, this.notesPanel);
		}

		/// <summary>
		/// Makes sure the InkPicture inkPic is big enough to display all ink 
		/// and at least as big as its parent panel.
		/// <seealso cref="InkExplorer.NET.Form1.ResizeInkPicture"/>
		/// </summary>
		/// <param name="inkPic">The InkPicture instance to resize</param>
		/// <param name="panel">The parent Windows Forms panel containing the InkPicture</param>
		private void resizeInkPicture(Microsoft.Ink.InkPicture inkPic, System.Windows.Forms.Panel panel) 
		{
			Rectangle inkRect = inkPic.Ink.GetBoundingBox();
			System.Drawing.Point pt = new System.Drawing.Point(inkRect.Right, inkRect.Bottom);
			using (Graphics g = inkPic.CreateGraphics())
			{
				inkPic.Renderer.InkSpaceToPixel(g, ref pt);
			}

			if (pt.X > inkPic.Width || panel.Size.Width > inkPic.Width)
				inkPic.Width = Math.Max(pt.X, panel.Size.Width) - 10;
			if (pt.Y > inkPic.Height || panel.Size.Height > inkPic.Height)
				inkPic.Height = Math.Max(pt.Y, panel.Size.Height) - 10;
		}

		/// <summary>
		/// Resizes the skech panel when the window is resized.  Ensures that the 
		/// status bar will always be shown by maintaining the relative position of
		/// the sketch/notes panel splitter.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void Form1_Resize(object sender, EventArgs e)
		{
			// Set the new sketch panel height
			if (currentTrigger != Trigger.LassoTap)
			{
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);
			} 
			else
			{
				sketchPanel.Height = bottomSplitter.Bounds.Top - sketchPanel.Bounds.Top;
			}
		}

		/// <summary>
		/// Sets the relative splitter position.  The splitter's relative position is
		/// maintained while the main window is resized
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void sketchSplitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			this.sketchFraction = ((float) sketchPanel.Height) / ((float) this.Height);

			// Do some range checking to insure splitter is set to sane
			// position.
			if (this.sketchFraction < 0.1f) 
			{
				this.sketchFraction = 0.1f;
			}
			else if (this.sketchFraction > 0.9f)
			{
				this.sketchFraction = 0.9f;
			}
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Menu Item Callbacks
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Menu Item callbacks

		/// <summary>
		/// Menu item event handler for the File>Open... menuItem.  Supports opening an MIT XML sketch file.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void openItem_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.Title  = "Choose a valid MIT XML sketch file...";
			openFileDialog1.Filter = "MIT XML Files (*.xml)|*.xml";
			
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				// Load the Sketch from the XML file
				this.loadSketch2SketchPanel((new ReadXML(openFileDialog1.FileName)).Sketch);
				this.currentFileName = openFileDialog1.FileName;
			}
			else
			{
				// The user cancelled or the open action failed.
			}
		}

		/// <summary>
		/// Opens the "Save" dialog
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void saveItem_Click(object sender, System.EventArgs e)
		{
			if (this.currentFileName != null) 
			{
				if (this.sketch != null)
				{
					Converter.MakeXML xmlHolder = new MakeXML(this.sketch);
					xmlHolder.WriteXML(this.currentFileName);
				}
				else 
				{
					MessageBox.Show("The sketch is empty.  No data was saved.", "Error");
				}
			}
		}

		/// <summary>
		/// Opens the "Save as" dialog
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void saveAsItem_Click(object sender, System.EventArgs e)
		{
			saveFileDialog1.Filter = "MIT XML Files (*.xml)|*.xml";
			saveFileDialog1.AddExtension = true;

			// Write the XML to a file
			if ((saveFileDialog1.ShowDialog() == DialogResult.OK))
			{
				if (this.sketch != null)
				{
					this.currentFileName = saveFileDialog1.FileName;
					Converter.MakeXML xmlHolder = new MakeXML(this.sketch);
					xmlHolder.WriteXML(saveFileDialog1.FileName);
				}
				else 
				{
					MessageBox.Show("The sketch is empty.  No data was saved.", "Error");
				}
			}
			else
			{
				// Error saving file
			}
		}

		/// <summary>
		/// Deletes all the strokes collected and presents a new, blank
		/// inteface to the user.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void newSketchItem_Click(object sender, System.EventArgs e)
		{
			// Blank out the current file name so that we don't save strokes
			// from the new skech to an old file
			this.currentFileName = null;

			// Empty out sketches
			sketch = new Sketch.Sketch();
			notes  = new Sketch.Sketch();
			sketch.XmlAttrs.Id = System.Guid.NewGuid();
			sketch.XmlAttrs.Units = "himetric";
			notes.XmlAttrs.Id = System.Guid.NewGuid();
			notes.XmlAttrs.Units = "himetric";

			// Empty out the Ink Pictures
			sketchPicture.Enabled = false;
			sketchPicture.Ink.DeleteStrokes();
			sketchPicture.Enabled = true;

			notesPicture.Enabled = false;
			notesPicture.Ink.DeleteStrokes();
			notesPicture.Enabled = true;

			this.Refresh();
		}

		
		/// <summary>
		/// Exits the application
		/// </summary>
		/// <param name="sender">Reference to the object that passed the event</param>
		/// <param name="e">The event object</param>
		private void exitItem_Click(object sender, System.EventArgs e)
		{
			System.Windows.Forms.Application.Exit();
		}


		/// <summary>
		/// Toggles the admin menu on and off
		/// </summary>
		/// <param name="sender">Event Sender</param>
		/// <param name="e">Event Object</param>
		private void adminToggleMenuItem_Click(object sender, System.EventArgs e)
		{
			if (this.adminMenuItem.Visible) 
			{
				this.adminMenuItem.Visible = false;
				this.adminMenuItem.Enabled = false;
				this.adminToggleMenuItem.Checked = false;
			} 
			else 
			{
				this.adminMenuItem.Visible = true;
				this.adminMenuItem.Enabled = true;
				this.adminToggleMenuItem.Checked = true;
			}
		}

		/// <summary>
		/// Turns on button-triggered recognition and makes the recognition button visible.
		/// </summary>
		private void buttonTriggerMenuItem_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger != Trigger.Button) 
			{
				currentTrigger = Trigger.Button;

				// Modify UI appearance
				buttonTriggerMenuItem.Checked = true;
				pauseTriggerMenuItem.Checked = false;
				lassoTriggerMenuItem.Checked = false;
				recognizeButton.Visible = true;
				recognizeButton.Enabled = true;
				lassoRadio.Visible = false;
				lassoRadio.Enabled = false;
				notesPanel.Enabled = true;
				notesPanel.Visible = true;
				sketchSplitter.Enabled = true;
				sketchSplitter.Visible = true;
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);

				// Update Ink Collection
				//this.disableGestures();

			}
		}

		/// <summary>
		/// Turns on pause-triggered recognition.
		/// </summary>
		private void pauseTriggerMenuItem_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger != Trigger.Pause) 
			{
				currentTrigger = Trigger.Pause;

				// Modify UI appearance
				buttonTriggerMenuItem.Checked = false;
				pauseTriggerMenuItem.Checked = true;
				lassoTriggerMenuItem.Checked = false;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				lassoRadio.Visible = false;
				lassoRadio.Enabled = false;
				notesPanel.Enabled = true;
				notesPanel.Visible = true;
				sketchSplitter.Enabled = true;
				sketchSplitter.Visible = true;
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);

				// Update Ink Collection
				//this.disableGestures();
			}
		}

		/// <summary>
		/// Turns on lasso-tap triggered recognition.
		/// </summary>
		private void lassoTriggerMenuItem_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger != Trigger.LassoTap) 
			{
				currentTrigger = Trigger.LassoTap;

				// Modify UI appearance
				buttonTriggerMenuItem.Checked = false;
				pauseTriggerMenuItem.Checked = false;
				lassoTriggerMenuItem.Checked = true;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				lassoRadio.Visible = true;
				lassoRadio.Enabled = true;
				notesPanel.Enabled = false;
				notesPanel.Visible = false;
				sketchSplitter.Enabled = false;
				sketchSplitter.Visible = false;
				sketchPanel.Height = bottomSplitter.Bounds.Top - sketchPanel.Bounds.Top;


				// Update Ink Collection
				//this.enableGestures();
			}
		}

		private void undoItem_Click(object sender, System.EventArgs e)
		{
		
		}

		private void redoItem_Click(object sender, System.EventArgs e)
		{
		
		}

		private void copyItem_Click(object sender, System.EventArgs e)
		{
		
		}

		private void pasteItem_Click(object sender, System.EventArgs e)
		{
		
		}

		/// <summary>
		/// Enables the first step of the lasso-tap gesture: selection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lassoRadio_CheckedChanged(object sender, System.EventArgs e)
		{
			if (lassoRadio.Checked) 
			{
				sketchPicture.EditingMode = InkOverlayEditingMode.Select;
			} 
			else // unchecked
			{
				sketchPicture.EditingMode = InkOverlayEditingMode.Ink;
				this.disableTapGesture();
			}
		}

		/// <summary>
		/// Handles click event for Recognize Button and triggers recognizer.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void recognizeButton_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger == Trigger.Button) 
			{
				recognizer.recognize(sketch, true);
			}
		}

		private void zoomInItem_Click(object sender, System.EventArgs e)
		{
			sketchPicture.Renderer.Scale(UIConstants.zoomInFactor, UIConstants.zoomInFactor, false);
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
			this.Refresh();
		}

		private void zoomOutItem_Click(object sender, System.EventArgs e)
		{
			sketchPicture.Renderer.Scale(UIConstants.zoomOutFactor, UIConstants.zoomOutFactor, false);
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
			this.Refresh();
		}

		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Ink to Sketch delegates
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Ink to Sketch delegates

		/// <summary>
		/// Handles stroke events sent from the sketch panel InkImage
		/// </summary>
		/// <param name="sender">Source InkCollector that raised this event</param>
		/// <param name="e">Stroke event object containing event data</param>
		private void sketchPicture_Stroke(object sender, Microsoft.Ink.InkCollectorStrokeEventArgs e)
		{
			// Only collect strokes if we are not in lasso-tap mode
			if (!lassoRadio.Checked)
			{
				Sketch.Stroke stroke = readJnt.InkStroke2SketchStroke(e.Stroke, null, false);
				sketch.AddStroke(stroke);
			}
		}

		/// <summary>
		/// Handles stroke events sent from the notes panel InkImage
		/// </summary>
		/// <param name="sender">Source InkCollector that raised this event</param>
		/// <param name="e">Stroke event object containing event data</param>
		private void notesPicture_Stroke(object sender, Microsoft.Ink.InkCollectorStrokeEventArgs e)
		{
			Sketch.Stroke stroke = readJnt.InkStroke2SketchStroke(e.Stroke, null, false);
			notes.AddStroke(stroke);

			this.resizeInkPicture(this.notesPicture, this.notesPanel);
		}

		private void inkAdded2Sketch(object sender, StrokesEventArgs e) 
		{
			if (this.recognitionReadActive)
				return;
			
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
			
			if (currentTrigger == Trigger.Pause) 
			{
				PauseTriggerFiredEvent();
			}
		}

		private void inkDeleted2Sketch(object sender, StrokesEventArgs e)
		{
			if (this.recognitionReadActive)
				return;
			
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);

			if (currentTrigger == Trigger.Pause) 
			{
				PauseTriggerFiredEvent();
			}
		}

		private void inkAdded2Notes(object sender, StrokesEventArgs e)
		{
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
		}

		private void inkDeleted2Notes(object sender, StrokesEventArgs e)
		{
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
		}

		/// <summary>
		/// Overwrites the contents of the sketch panel with the given Sketch instance.  Deletes 
		/// all the strokes in the Ink Picture and refreshes the display.
		/// </summary>
		/// <param name="newSketch">The Sketch to load</param>
		public void loadSketch2SketchPanel(Sketch.Sketch newSketch)
		{
			// Load the Sketch
			sketch = newSketch;
			this.loadSketch2Image(sketch, this.sketchPicture, this.sketchPanel);
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
			this.Refresh();
		}

		/// <summary>
		/// Overwrites the contents of the notes panel with the given Sketch instance.  Deletes 
		/// all the strokes in the Ink Overlay and refreshes the display.
		/// </summary>
		/// <param name="newSketch">The Sketch to load</param>
		public void loadSketch2NotesPanel(Sketch.Sketch newSketch)
		{
			// Load the sketch
			notes = newSketch;
			this.loadSketch2Image(notes, this.notesPicture, this.notesPanel);
			this.resizeInkPicture(this.notesPicture, this.notesPanel);
			this.Refresh();
		}

		/// <summary>
		/// Helper for loadSketch2SketchPanel() and loadSketch2NotesPanel().  Loads a sketch into the panel and 
		/// assigns the defaut stroke coloring.
		/// </summary>
		/// <seealso cref="Labeler.updateInkOverlay">Labeler.updateInkOverlay</seealso>
		/// <param name="newSketch">the Sketch to load</param>
		/// <param name="inkPic">the InkOverlay associated with the target panel into which we want to load the sketch</param>
		/// <param name="targetPanel">the target panel</param>
		private void loadSketch2Image(Sketch.Sketch newSketch, Microsoft.Ink.InkPicture inkPic, System.Windows.Forms.Panel targetPanel)
		{
			// Load the Sketch into the InkPicture
			// by loading the Sketch substrokes
			inkPic.Enabled = false;
			inkPic.Ink.DeleteStrokes();

			Sketch.Substroke[] substrokes = newSketch.Substrokes;
			for (int i = 0; i < substrokes.Length; i++)
			{
				Sketch.Point[] sketchPts = substrokes[i].Points;
				System.Drawing.Point[] simplePts = new System.Drawing.Point[sketchPts.Length];
				
				for (int k = 0; k < simplePts.Length; k++)
				{
					simplePts[k] = new System.Drawing.Point((int)sketchPts[k].X, (int)sketchPts[k].Y);
				}
					
				// Create the InkOverlay stroke
				inkPic.Ink.CreateStroke(simplePts);

				// Assign default color
				inkPic.Ink.Strokes[inkPic.Ink.Strokes.Count - 1].DrawingAttributes.Color = UIConstants.DefaultInkColor;

				// Quick hack: feeback mechanism that colors wires blue
				if (substrokes[i].GetFirstLabel() == UIConstants.WireLabel)
				{
					inkPic.Ink.Strokes[inkPic.Ink.Strokes.Count - 1].DrawingAttributes.Color = Color.Blue;
				}
			}

			// If we are opening a new file, then move and scale the ink as needed.
			// Otherwise, leave the ink as-is
			if (!this.recognitionReadActive)
			{
				// Move the Ink's origin to the upper left-hand corner.
				inkPic.Ink.Strokes.Move(-1 * inkPic.Ink.GetBoundingBox().X, -1 * inkPic.Ink.GetBoundingBox().Y);

			
				// Scale the sketch to fill the InkImage
				Rectangle inkRect = inkPic.Ink.GetBoundingBox();
				System.Drawing.Point rightBottom = new System.Drawing.Point(inkRect.Right, inkRect.Bottom);
				using (Graphics g = inkPic.CreateGraphics())
				{
					inkPic.Renderer.InkSpaceToPixel(g, ref rightBottom);
				}

				System.Drawing.Point scalePt = new System.Drawing.Point(rightBottom.X - inkRect.Left, 
					rightBottom.Y - inkRect.Top);

				// Scale the rendered strokes by the smallest (x or y) scaling factor
				float xScale = (float)(targetPanel.Width - UIConstants.scaleOffset) / (float)scalePt.X;
				float yScale = (float)(targetPanel.Height - UIConstants.scaleOffset) / (float)scalePt.Y;
		
				float scale = xScale < yScale ? xScale : yScale;

				inkPic.Renderer.Scale(scale, scale, false);
			}

			inkPic.Enabled = true;
			this.Refresh();
		}


		#endregion



		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Recognition Event Handling
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Recognition Event Handling

		/// <summary>
		/// Handles recognition results retrieved from the recognizer.
		/// </summary>
		/// <param name="source">The recognizer sending the recognition results.</param>
		/// <param name="re">The recognition results. <see cref="RecognizerEventArgs"/></param>
		private void recognizer_RecognitionEvent(object source, RecognizerEventArgs re)
		{
			this.recognitionReadActive = true;

			if (re.recognitionResults.Length > 0) 
			{
				Sketch.Sketch sketch = re.recognitionResults[0];
				this.loadSketch2SketchPanel(sketch);
				this.Refresh();
			}

			this.recognitionReadActive = false;

			// Restore sketching mode, if applicable
			if (lassoRadio.Checked)
			{
				sketchRadio.Checked = true;
			}


		}

		#endregion



		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Lasso-tap-triggered Recognition using Selection Mode
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Lasso-tap-triggered Recognition using Selection Mode


		/// <summary>
		/// Enables gesture watching for lasso-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void enableTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkAndGesture;
			
			sketchPicture.SetGestureStatus(ApplicationGesture.Tap, true);
			sketchPicture.Gesture += new InkCollectorGestureEventHandler(sketchPicture_LassoTapWithSelectGesture);
		}

		/// <summary>
		/// Disables gesture watching for lasso-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void disableTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkOnly;

			ApplicationGesture noGest = ApplicationGesture.NoGesture;
			System.Array theGestureIds = System.Enum.GetValues(noGest.GetType());
			foreach (ApplicationGesture theGestureId in theGestureIds)
			{
				sketchPicture.SetGestureStatus(theGestureId, false);
			}
		}

		/// <summary>
		/// Handles Ink selection events for the lasso-tap gesture.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event object</param>
		private void inkSelected2Sketch(object sender, EventArgs e)
		{
			this.lastSelection = sketchPicture.Selection;
			sketchPicture.Selection.Clear();
			sketchPicture.EditingMode = InkOverlayEditingMode.Ink;
			this.enableTapGesture();
		}

		/// <summary>
		/// Processes the lasso-tap gesture and fires recognition when necessary.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Gesture event object.  
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkcollectorgestureeventargs.aspx"/>
		/// </param>
		private void sketchPicture_LassoTapWithSelectGesture(object sender, InkCollectorGestureEventArgs e)
		{
			Console.WriteLine("Identified gesture" + e.Gestures[0].Id + " with confidence " + e.Gestures[0].Confidence);
			
			// If we are not in lasso-mode, ignore all gestures
			if (!lassoRadio.Checked)
			{
				e.Cancel = true;
				this.disableTapGesture();
				return;
			}

			// If there are no gestures for some reason, cancel
			if (e.Gestures.Length == 0)
			{
				this.disableTapGesture();
				return;
			}

			ApplicationGesture theGestureId = e.Gestures[0].Id;
			// Act only on tap gestures
			if (theGestureId == ApplicationGesture.Tap) 
			{
				// Ensure gesture has minimum confidence
				if (e.Gestures[0].Confidence.CompareTo(UIConstants.LassoTapConfidenceThreshhold) >= 0) 
				{
					// Hit test for tap
					if (this.lastSelection.GetBoundingBox().Contains(e.Gestures[0].HotPoint))
					{
						// Make sure the tap stroke gets erased
						e.Cancel = false;

						// Collect strokes and recognize them
						Sketch.Sketch lassoedSketch = new Sketch.Sketch();
						foreach (Microsoft.Ink.Stroke iStroke in this.lastSelection) 
						{
							Sketch.Stroke stroke = readJnt.InkStroke2SketchStroke(iStroke, null, false);  // FIXME
							lassoedSketch.AddStroke(stroke);
						}

						// FIXME can't do these while doing recognition apparently; get a JIT exception if we do.
						// Return to sketching mode
						//sketchRadio.Select();
						//sketchPicture.Selection.Clear();

						recognizer.recognize(lassoedSketch, true);

					}
				}
			} 
			else // Cancel the lasso-tap gesture 
			{
				// Return to sketching mode
				sketchRadio.Select();
				sketchPicture.Selection.Clear();
			}
		}

		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Pause-triggered Recognition - Form Hooks
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Pause-triggered Recognition - Form Hooks

		public void triggerRecognition()
		{
			Console.WriteLine("Pause trigger firing recognizer");
			recognizer.recognize(sketch, true);
		}


		#endregion


	}


	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Pause-triggered Recognition - Countdown Timer
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Pause-triggered Recognition - Countdown Timer

	public class PauseTriggerCountdownTimer 
	{
		/// <summary>
		/// The parentForm running this timer.
		/// </summary>
		private Form1 parentForm;

		/// <summary>
		/// Event that is published to when the countdown finishes
		/// </summary>
		public event PauseCountdownFinished CountdownFinishedEvent;

		/// <summary>
		/// Thread for running the countdown
		/// </summary>
		private Thread countdownThread;

		/// <summary>
		/// Hooks this countdown timer to the parent form that runs
		/// this timer.  The parent is pinged when the countdown is done
		/// through the CountdownFinishedEvent.
		/// </summary>
		/// <param name="parent"></param>
		public PauseTriggerCountdownTimer(Form1 parent) 
		{
			this.parentForm = parent;
			parent.PauseTriggerFiredEvent += new FirePauseCountdown(parent_PauseTriggerFiredEvent);
			countdownThread = new Thread(new ThreadStart(this.RunTimer));
		}

		/// <summary>
		/// Starts or resets the countdown whenever the pause trigger is
		/// fired on the parent.
		/// </summary>
		private void parent_PauseTriggerFiredEvent()
		{
			Console.WriteLine("Timer got trigger event");
			if (!countdownThread.ThreadState.Equals(System.Threading.ThreadState.Unstarted) &&
				!countdownThread.ThreadState.Equals(System.Threading.ThreadState.Stopped))
			{
				Console.WriteLine("Resetting countdown");
				// Restart the countdown
				countdownThread.Abort();
				countdownThread = new Thread(new ThreadStart(this.RunTimer));
				countdownThread.Start();
			} 
			else 
			{
				Console.WriteLine("Starting countdown");
				// Start the countdown
				countdownThread = new Thread(new ThreadStart(this.RunTimer));
				countdownThread.Start();
			}
		}

		/// <summary>
		/// The actual countdown method.  Sleeps for the duration of the 
		/// pause and then fires a countdown-finished event.
		/// </summary>
		private void RunTimer() 
		{
			try 
			{
				Console.WriteLine("Countdown thread running");
				Thread.Sleep(UIConstants.pauseTriggerWait);
				Console.WriteLine("Countdown thread done sleeping");
				CountdownFinishedEvent();
			}
			catch(ThreadAbortException e)
			{
				Console.WriteLine("Countdown thread aborting");
				// Kill the countdown
			}	
		}
	}

	#endregion


}
