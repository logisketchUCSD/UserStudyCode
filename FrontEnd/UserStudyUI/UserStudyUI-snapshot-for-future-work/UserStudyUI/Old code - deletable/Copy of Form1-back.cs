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
		/// The current feedback mechanism used on Sketch/Ink data.
		/// </summary>
		private FeedbackMechanism feedbackMechanism;

		/// <summary>
		/// True iff this front end is currently receiving 
		/// recognition results and updating the screen
		/// accordingly.
		/// </summary>
		private bool recognitionReadActive = false;

		/// <summary>
		/// Recognition trigger modes and current mode variable
		/// </summary>
		private enum Trigger {Button, Pause, LassoTap, CheckTap, CheckTapAnnotate};
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
		/// Maps Microsoft Ink Stroke IDs to Sketch substroke
		/// IDs.
		/// </summary>
		private Hashtable inkToSketchIds = new Hashtable();

		/// <summary>
		/// Stores the time of the last Check gesture.  Used for
		/// recognizing the check-tap gesture for check-tap triggered
		/// recognition.  This member represents a time stored in 
		/// DateTime.Tick units.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/system.datetime.aspx"/>
		/// </summary>
		private long lastCheckGesture = 0L;

		/// <summary>
		/// The Ink Stroke ID of the check gesture stored in lastCheckGesture
		/// </summary>
		private Microsoft.Ink.Stroke lastCheckGestureStroke;

		/// <summary>
		/// Collection of lasso Ink strokes made while the lasso-tap gesture was
		/// active.
		/// </summary>
		private ArrayList lassoStrokes = new ArrayList();

		/// <summary>
		/// Flag for determining if the user can currently enabled
		/// lasso-selection using a gesture.
		/// </summary>
		private bool lassoStrokeCollecting = false;

		/// <summary>
		/// Stores the IDs of annotation strokes.
		/// </summary>
		private ArrayList annotationStrokeIds = new ArrayList();

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
		private System.Windows.Forms.MenuItem checkTapMenuItem;
		private System.Windows.Forms.MenuItem coloredAnnotationMenuItem;
		private System.Windows.Forms.CheckBox annotationCheckBox;

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
			this.SetStyle( ControlStyles.UserPaint, true ); 
			this.SetStyle( ControlStyles.AllPaintingInWmPaint, true ); 
			this.SetStyle( ControlStyles.DoubleBuffer, true ); 
			this.UpdateStyles();


			// Create the Sketches
			sketch = new Sketch.Sketch();
			notes  = new Sketch.Sketch();
			sketch.XmlAttrs.Id = System.Guid.NewGuid();
			sketch.XmlAttrs.Units = "himetric";
			notes.XmlAttrs.Id = System.Guid.NewGuid();
			notes.XmlAttrs.Units = "himetric";

			// Create JNT converter for converting Ink strokes to sketch strokes
			readJnt = new ReadJnt("null file");

			// Create resize hooks for Ink Pictures
			sketchPanel.Resize += new EventHandler(sketchPanel_Resize);
			notesPanel.Resize += new EventHandler(notesPanel_Resize);

			// TODO Configure Ink Picture properties (turn on time collection)
			

			// Hook into Ink Picture events
			sketchPicture.Ink.InkAdded += new StrokesEventHandler(inkAdded2Sketch);
			sketchPicture.Ink.InkDeleted += new StrokesEventHandler(inkDeleted2Sketch);
			sketchPicture.CursorInRange += new InkCollectorCursorInRangeEventHandler(sketchPicture_CursorInRange);
			sketchPicture.StrokesDeleting += new InkOverlayStrokesDeletingEventHandler(sketchPicture_StrokesDeleting);
			notesPicture.Ink.InkAdded += new StrokesEventHandler(inkAdded2Notes);
			notesPicture.Ink.InkDeleted += new StrokesEventHandler(inkDeleted2Notes);
			notesPicture.CursorInRange += new InkCollectorCursorInRangeEventHandler(notesPicture_CursorInRange);

			// Create resize handler for sketch/notes splitter
			this.Resize += new EventHandler(Form1_Resize);

			// Create the recognizer and subscribe this class to it
			recognizer = new WireLabelRecognizer();
			recognizer.RecognitionEvent += new RecognitionResultsEventHandler(recognizer_RecognitionEvent);

			// Create the feedback mechanism
			feedbackMechanism = new TextLabelFeedbackMechanism();

			// Create the Pause countdown mechanism and 
			// subscribe to its countdown-finished event
			pauseCountdownTimer = new PauseTriggerCountdownTimer(this);
			pauseCountdownTimer.CountdownFinishedEvent += new PauseCountdownFinished(this.triggerRecognition);

			// Hide annotation button
			annotationCheckBox.Visible = false;
			annotationCheckBox.Enabled = false;

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
			this.checkTapMenuItem = new System.Windows.Forms.MenuItem();
			this.lassoTriggerMenuItem = new System.Windows.Forms.MenuItem();
			this.coloredAnnotationMenuItem = new System.Windows.Forms.MenuItem();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.sketchRadio = new System.Windows.Forms.RadioButton();
			this.eraseRadio = new System.Windows.Forms.RadioButton();
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
			this.annotationCheckBox = new System.Windows.Forms.CheckBox();
			this.sketchPanel.SuspendLayout();
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
																						  this.checkTapMenuItem,
																						  this.lassoTriggerMenuItem,
																						  this.coloredAnnotationMenuItem});
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
			// checkTapMenuItem
			// 
			this.checkTapMenuItem.Index = 2;
			this.checkTapMenuItem.Text = "Check-Tap";
			this.checkTapMenuItem.Click += new System.EventHandler(this.checkTapMenuItem_Click);
			// 
			// lassoTriggerMenuItem
			// 
			this.lassoTriggerMenuItem.Index = 3;
			this.lassoTriggerMenuItem.Text = "Lasso-Tap";
			this.lassoTriggerMenuItem.Click += new System.EventHandler(this.lassoTriggerMenuItem_Click);
			// 
			// coloredAnnotationMenuItem
			// 
			this.coloredAnnotationMenuItem.Index = 4;
			this.coloredAnnotationMenuItem.Text = "Colored Annotation";
			this.coloredAnnotationMenuItem.Click += new System.EventHandler(this.coloredAnnotationMenuItem_Click);
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
			this.sketchRadio.Enabled = false;
			this.sketchRadio.Location = new System.Drawing.Point(8, 8);
			this.sketchRadio.Name = "sketchRadio";
			this.sketchRadio.Size = new System.Drawing.Size(104, 32);
			this.sketchRadio.TabIndex = 8;
			this.sketchRadio.TabStop = true;
			this.sketchRadio.Text = "Sketch Tool";
			this.sketchRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.sketchRadio.CheckedChanged += new System.EventHandler(this.sketchRadio_CheckedChanged);
			// 
			// eraseRadio
			// 
			this.eraseRadio.Appearance = System.Windows.Forms.Appearance.Button;
			this.eraseRadio.Enabled = false;
			this.eraseRadio.Location = new System.Drawing.Point(120, 8);
			this.eraseRadio.Name = "eraseRadio";
			this.eraseRadio.Size = new System.Drawing.Size(104, 32);
			this.eraseRadio.TabIndex = 9;
			this.eraseRadio.Text = "Erase Tool";
			this.eraseRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.eraseRadio.CheckedChanged += new System.EventHandler(this.eraseRadio_CheckedChanged);
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
			// annotationCheckBox
			// 
			this.annotationCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
			this.annotationCheckBox.BackColor = System.Drawing.Color.Silver;
			this.annotationCheckBox.Location = new System.Drawing.Point(232, 8);
			this.annotationCheckBox.Name = "annotationCheckBox";
			this.annotationCheckBox.Size = new System.Drawing.Size(152, 32);
			this.annotationCheckBox.TabIndex = 19;
			this.annotationCheckBox.Text = "Annotation Tool";
			this.annotationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.annotationCheckBox.CheckedChanged += new System.EventHandler(this.annotationCheckBox_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
			this.ClientSize = new System.Drawing.Size(536, 452);
			this.Controls.Add(this.annotationCheckBox);
			this.Controls.Add(this.notesPanel);
			this.Controls.Add(this.sketchSplitter);
			this.Controls.Add(this.sketchPanel);
			this.Controls.Add(this.bottomSplitter);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.recognizeButton);
			this.Controls.Add(this.eraseRadio);
			this.Controls.Add(this.sketchRadio);
			this.Controls.Add(this.toolBar1);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "User Study UI";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.sketchPanel.ResumeLayout(false);
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
		// Menu Item and Button Callbacks
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Menu Item and Button Callbacks

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
			inkToSketchIds.Clear();
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
		/// Enables stroke erasing mode.
		/// </summary>
		private void eraseRadio_CheckedChanged(object sender, System.EventArgs e)
		{
			if (eraseRadio.Checked)
			{
				//sketchPicture.Enabled = false;
				sketchPicture.EraserMode = InkOverlayEraserMode.StrokeErase;
				sketchPicture.EditingMode = InkOverlayEditingMode.Delete;
				//sketchPicture.Enabled = true;
			}
		}

		/// <summary>
		/// Enables standard sketching mode.
		/// </summary>
		private void sketchRadio_CheckedChanged(object sender, System.EventArgs e)
		{
			if (sketchRadio.Checked) 
			{
				//sketchPicture.Enabled = false;
				sketchPicture.EditingMode = InkOverlayEditingMode.Ink;
				//sketchPicture.Enabled = true;
			}
		}

		/// <summary>
		/// Enables the annotation stylus mode
		/// </summary>
		private void annotationCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (annotationCheckBox.Checked)
			{
				sketchPicture.DefaultDrawingAttributes.Color = UIConstants.AnnotationColor;
			}
			else 
			{
				sketchPicture.DefaultDrawingAttributes.Color = UIConstants.DefaultInkColor;
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
				recognizer.recognize(sketch, sketch.Strokes, true);
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
		// Admin Menu Item Callbacks
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Admin Menu Item callbacks

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
				checkTapMenuItem.Checked = false;
				recognizeButton.Visible = true;
				recognizeButton.Enabled = true;
				annotationCheckBox.Visible = false;
				annotationCheckBox.Enabled = false;
				notesPanel.Enabled = true;
				notesPanel.Visible = true;
				sketchSplitter.Enabled = true;
				sketchSplitter.Visible = true;
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);

				// Update Ink Collection
				this.disableCheckTapGesture();
				this.disableLassoTapGesture();
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
				checkTapMenuItem.Checked = false;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				annotationCheckBox.Visible = false;
				annotationCheckBox.Enabled = false;
				notesPanel.Enabled = true;
				notesPanel.Visible = true;
				sketchSplitter.Enabled = true;
				sketchSplitter.Visible = true;
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);

				// Update Ink Collection
				this.disableCheckTapGesture();
				this.disableLassoTapGesture();
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
				checkTapMenuItem.Checked = false;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				annotationCheckBox.Visible = false;
				annotationCheckBox.Enabled = false;
				notesPanel.Enabled = false;
				notesPanel.Visible = false;
				sketchSplitter.Enabled = false;
				sketchSplitter.Visible = false;
				sketchPanel.Height = bottomSplitter.Bounds.Top - sketchPanel.Bounds.Top;


				// Update Ink Collection
				this.disableCheckTapGesture();
				this.enableLassoTapGesture();
			}
		}

		/// <summary>
		/// Turns on check-tap triggered recognition
		/// </summary>
		private void checkTapMenuItem_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger != Trigger.CheckTap) 
			{
				currentTrigger = Trigger.CheckTap;

				// Modify UI appearance
				buttonTriggerMenuItem.Checked = false;
				pauseTriggerMenuItem.Checked = false;
				lassoTriggerMenuItem.Checked = false;
				checkTapMenuItem.Checked = true;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				annotationCheckBox.Visible = false;
				annotationCheckBox.Enabled = false;
				notesPanel.Enabled = true;
				notesPanel.Visible = true;
				sketchSplitter.Enabled = true;
				sketchSplitter.Visible = true;
				sketchPanel.Height = (int)(this.Height * this.sketchFraction);

				// Update Ink Collection
				this.disableLassoTapGesture();
				this.enableCheckTapGesture();

			}
		}

		/// <summary>
		/// Turns on color annotation feature with check-tap trigger
		/// </summary>
		private void coloredAnnotationMenuItem_Click(object sender, System.EventArgs e)
		{
			if (currentTrigger != Trigger.CheckTapAnnotate) 
			{
				currentTrigger = Trigger.CheckTapAnnotate;

				// Modify UI appearance
				buttonTriggerMenuItem.Checked = false;
				pauseTriggerMenuItem.Checked = false;
				lassoTriggerMenuItem.Checked = false;
				checkTapMenuItem.Checked = true;
				recognizeButton.Visible = false;
				recognizeButton.Enabled = false;
				annotationCheckBox.Visible = true;
				annotationCheckBox.Enabled = true;
				notesPanel.Enabled = false;
				notesPanel.Visible = false;
				sketchSplitter.Enabled = false;
				sketchSplitter.Visible = false;
				sketchPanel.Height = bottomSplitter.Bounds.Top - sketchPanel.Bounds.Top;

				// Update Ink Collection
				this.disableLassoTapGesture();
				this.enableCheckTapGesture();

			}
		}

		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Ink to Sketch Delegates and Event Handling
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Ink to Sketch Delegates and Event Handling

		/// <summary>
		/// Handles stroke events sent from the sketch panel InkImage
		/// </summary>
		/// <param name="sender">Source InkCollector that raised this event</param>
		/// <param name="e">Stroke event object containing event data</param>
		private void sketchPicture_Stroke(object sender, Microsoft.Ink.InkCollectorStrokeEventArgs e)
		{
			// Only collect strokes if we are in ink mode and are not annotating
			if (sketchPicture.EditingMode == InkOverlayEditingMode.Ink && !annotationCheckBox.Checked && !this.lassoStrokeCollecting)
			{
				Sketch.Stroke stroke = readJnt.InkStroke2SketchStroke(e.Stroke, null, false);
				inkToSketchIds.Add(e.Stroke.Id, stroke.Substrokes[0].XmlAttrs.Id);
				sketch.AddStroke(stroke);
				//Console.WriteLine(stroke.XmlAttrs.Id);
			} 
			else if (sketchPicture.EditingMode == InkOverlayEditingMode.Ink && annotationCheckBox.Checked)
			{
				// Keep track of annotation strokes
				annotationStrokeIds.Add(e.Stroke.Id);
			}
			else if (this.lassoStrokeCollecting)
			{
				this.lassoStrokes.Add(e.Stroke);
			}
		}

		/// <summary>
		/// Handles stroke delete events sent from the sketch InkPicture
		/// </summary>
		/// <param name="sender">Source InkCollector that raised the event</param>
		/// <param name="e">Event object containing event data</param>
		private void sketchPicture_StrokesDeleting(object sender, InkOverlayStrokesDeletingEventArgs e)
		{
			if (sketchPicture.EditingMode == InkOverlayEditingMode.Delete) 
			{
				Microsoft.Ink.Strokes strokesToDelete = e.StrokesToDelete;

				foreach (Microsoft.Ink.Stroke inkStroke in strokesToDelete)
				{
					int inkStrokeID = inkStroke.Id;
					if (inkToSketchIds.Contains(inkStrokeID))
					{
						System.Guid sketchStrokeID = (System.Guid) inkToSketchIds[inkStrokeID];

						// Remove the stroke from the skech and hash table
						inkToSketchIds.Remove(inkStrokeID);
						sketch.RemoveSubstroke(sketch.GetSubstroke(sketchStrokeID));
						annotationStrokeIds.Remove(inkStrokeID);
					}
					else 
					{
						// We are deleting a stroke that is not in the sketch...
						Console.WriteLine("Error in stroke deletion: can't find a substroke to delete that corresponds to given Ink stroke");
					}
				}
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
			inkToSketchIds.Clear();
			this.loadSketch2Image(sketch, this.sketchPicture, this.sketchPanel, this.inkToSketchIds);
			this.resizeInkPicture(this.sketchPicture, this.sketchPanel);
			feedbackMechanism.FireFeedbackMechanism(this.sketch, this.sketchPicture, this.inkToSketchIds, FeedbackContext.OnRecognitionResult);
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
			this.loadSketch2Image(notes, this.notesPicture, this.notesPanel, null);
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
		/// <param name="inkId2SketchIdTable">the hash table relating ink stroke IDs to sketch stroke IDs to update (can be null)</param>
		private void loadSketch2Image(Sketch.Sketch newSketch, Microsoft.Ink.InkPicture inkPic, System.Windows.Forms.Panel targetPanel, Hashtable inkId2SketchIdTable)
		{
			// Load the Sketch into the InkPicture
			// by loading the Sketch substrokes
			inkPic.Enabled = false;

			// Delete ink strokes
			if (currentTrigger == Trigger.CheckTapAnnotate)
			{
				foreach (Microsoft.Ink.Stroke stroke in inkPic.Ink.Strokes)
				{
					if (!annotationStrokeIds.Contains(stroke.Id))
					{
						inkPic.Ink.DeleteStroke(stroke);
					}
				}
			}
			else 
			{
				inkPic.Ink.DeleteStrokes();
			}

			// Insert new strokes
			foreach (Sketch.Stroke stroke in newSketch.Strokes)
			{
				Sketch.Substroke[] substrokes = stroke.Substrokes;
				for (int i = 0; i < substrokes.Length; i++)
				{
					Sketch.Point[] sketchPts = substrokes[i].Points;
					System.Drawing.Point[] simplePts = new System.Drawing.Point[sketchPts.Length];
				
					for (int k = 0; k < simplePts.Length; k++)
					{
						simplePts[k] = new System.Drawing.Point((int)sketchPts[k].X, (int)sketchPts[k].Y);
					}
					
					// Create the InkOverlay stroke and upate the hash table if necessary
					Microsoft.Ink.Stroke newInkStroke = inkPic.Ink.CreateStroke(simplePts);
					if (inkId2SketchIdTable != null)
					{
						inkId2SketchIdTable.Add(newInkStroke.Id, substrokes[i].XmlAttrs.Id);
					}

					// Assign default color
					inkPic.Ink.Strokes[inkPic.Ink.Strokes.Count - 1].DrawingAttributes.Color = UIConstants.DefaultInkColor;
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
			//this.Refresh();
			
		}


		/// <summary>
		/// Enables and disables stroke deletion mode for the sketch panel when the cursor orientation changes 
		/// (e.g., when the cursor is inverted).  
		/// <see cref="http://msdn2.microsoft.com/en-us/library/ms812492.aspx#peninktopic_006"/>
		/// </summary>
		private void sketchPicture_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
		{
			if (sketchRadio.Checked || eraseRadio.Checked)
			{
				// Using explicite comparisons to minimize the number of refreshes 
				// that the cursor event causes; only switch modes once and don't 
				// keep pushing into sketch or erase if we're already in that mode.
				if(e.Cursor.Inverted && sketchRadio.Checked == true)
				{
					eraseRadio.Checked = true;
				}
				else if (!e.Cursor.Inverted && eraseRadio.Checked == true)
				{
					sketchRadio.Checked = true;
				}
			}
		}

		/// <summary>
		/// Enables and disables stroke deletion mode for the notes panel when the cursor orientation changes 
		/// (e.g., when the cursor is inverted).  
		/// <see cref="http://msdn2.microsoft.com/en-us/library/ms812492.aspx#peninktopic_006"/>
		/// </summary>
		private void notesPicture_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
		{
			// Using explicite comparisons to minimize the number of refreshes 
			// that the cursor event causes; only switch modes once and don't 
			// keep pushing into sketch or erase if we're already in that mode.
			if(e.Cursor.Inverted && notesPicture.EditingMode == InkOverlayEditingMode.Ink)
			{
				notesPicture.Enabled = false;
				notesPicture.EditingMode = InkOverlayEditingMode.Delete;

				// specify pixel deleting
				notesPicture.EraserMode = InkOverlayEraserMode.PointErase;

				notesPicture.Enabled = true;
			} 
			else if (!e.Cursor.Inverted && notesPicture.EditingMode != InkOverlayEditingMode.Ink)
			{
				// The stylus is in the correct orientation for sketching
				notesPicture.Enabled = false;
				notesPicture.EditingMode = InkOverlayEditingMode.Ink;
				notesPicture.Enabled = true;
			}
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
				// Load the recognized sketch
				Sketch.Sketch rSketch = re.recognitionResults[0];
				this.loadSketch2SketchPanel(rSketch);
				this.Refresh();
			}

			this.recognitionReadActive = false;

		}

		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Lasso-tap-triggered Recognition
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Lasso-tap-triggered Recognition


		/// <summary>
		/// Enables gesture watching for lasso-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void enableLassoTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkAndGesture;
			
			sketchPicture.SetGestureStatus(ApplicationGesture.Check, true);
			sketchPicture.SetGestureStatus(ApplicationGesture.Tap, true);
			sketchPicture.Gesture += new InkCollectorGestureEventHandler(sketchPicture_LassoTapGesture);
			Console.WriteLine("Lasso-tap enabled");
		}

		/// <summary>
		/// Disables gesture watching for lasso-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void disableLassoTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkOnly;

			ApplicationGesture noGest = ApplicationGesture.NoGesture;
			System.Array theGestureIds = System.Enum.GetValues(noGest.GetType());
			foreach (ApplicationGesture theGestureId in theGestureIds)
			{
				sketchPicture.SetGestureStatus(theGestureId, false);
			}

			sketchPicture.Gesture -= new InkCollectorGestureEventHandler(sketchPicture_LassoTapGesture);
		}

		/// <summary>
		/// Processes the lasso-tap gesture and fires recognition when necessary.
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Gesture event object.  
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkcollectorgestureeventargs.aspx"/>
		/// </param>
		private void sketchPicture_LassoTapGesture(object sender, InkCollectorGestureEventArgs e)
		{
			Console.WriteLine("Identified gesture" + e.Gestures[0].Id + " with confidence " + e.Gestures[0].Confidence);
			
			
			ApplicationGesture theGestureId = e.Gestures[0].Id;
			
			// Process check first, and then look for tap
			if (theGestureId == ApplicationGesture.Check)
			{
				// Ensure gesture has minimum confidence
				if (e.Gestures[0].Confidence.CompareTo(UIConstants.GestureConfidenceThreshhold) >= 0)
				{
					// Do not erase the check gesture yet
					e.Cancel = true;
					lastCheckGesture = DateTime.Now.Ticks;
					lastCheckGestureStroke = e.Strokes[0];
				}
			} 
			else if (theGestureId == ApplicationGesture.Tap) 
			{
				// Ensure gesture has minimum confidence
				if (e.Gestures[0].Confidence.CompareTo(UIConstants.GestureConfidenceThreshhold) >= 0) 
				{
					// Make sure check gesture happened within time threshold
					if (DateTime.Now.Ticks - lastCheckGesture <= UIConstants.CheckTapGestureThreshold)
					{
						// Make sure the tap stroke gets erased
						e.Cancel = false;
						
						
						// If we are in lasso stroke collecting (select) mode, then recognize the selection; 
						// else move into the lasso stroke collection mode
						if (this.lassoStrokeCollecting)
						{
							Console.WriteLine("Exiting lasso stroke collection");

							this.lassoStrokeCollecting = false;
							sketchPicture.DefaultDrawingAttributes.Color = UIConstants.DefaultInkColor;

							// Build the collection of lasso points
							ArrayList lassoPointsAr = new ArrayList();
							foreach (Microsoft.Ink.Stroke lassoStroke in this.lassoStrokes)
							{
								Sketch.Stroke sStroke = readJnt.InkStroke2SketchStroke(lassoStroke, null, false);
								foreach (Sketch.Substroke subStroke in sStroke.Substrokes)
								{
									for (int i = 0; i < subStroke.Points.Length; ++i)
									{
										System.Drawing.Point point = 
											new System.Drawing.Point((int) subStroke.Points[i].X, 
																	 (int) subStroke.Points[i].Y);
										lassoPointsAr.Add(point);
									}
								}
							}

							// Get the selected strokes
							System.Drawing.Point[] lassoPoints = (System.Drawing.Point[]) lassoPointsAr.ToArray(typeof (System.Drawing.Point));
							Microsoft.Ink.Strokes lassoedInkStrokes = sketchPicture.Ink.HitTest(lassoPoints, UIConstants.LassoTapSelectionThreshold);

							Sketch.Stroke[] lassoedStrokes  = new Sketch.Stroke[lassoedInkStrokes.Count];
							for (int j = 0; j < lassoedInkStrokes.Count; ++j)
							{
								int inkStrokeId = lassoedInkStrokes[j].Id;
								//lassoedInkStrokes[j].DrawingAttributes.Color = Color.Green;
								if (this.inkToSketchIds.Contains(inkStrokeId))
								{
									System.Guid sketchSubstrokeId = (System.Guid) this.inkToSketchIds[inkStrokeId];
									Substroke substrokeHit = this.sketch.GetSubstroke(sketchSubstrokeId);
									lassoedStrokes[j] = substrokeHit.ParentStroke;
									//Console.WriteLine(substrokeHit.ParentStroke.XmlAttrs.Id);
								}
							}

							//Console.WriteLine("sketch below");
							//foreach (Sketch.Stroke stroke in this.sketch.Strokes)
							//{
							//	Console.WriteLine(stroke.XmlAttrs.Id);
							//}

							// Clear the lasso strokes
							foreach (Microsoft.Ink.Stroke ilstroke in this.lassoStrokes)
							{
								this.sketchPicture.Ink.Strokes.Remove(ilstroke);
							}
							this.lassoStrokes.Clear();

							// Recognize the strokes
							recognizer.recognize(this.sketch, lassoedStrokes, true);
						}
						else 
						{
							Console.WriteLine("Entering lasso stroke collection");

							// Erase the check stroke
							lastCheckGestureStroke.DrawingAttributes.Color = Color.Green;
							this.Refresh();
							//sketchPicture.Ink.DeleteStroke(lastCheckGestureStroke);
							System.Guid sketchStrokeID = (System.Guid) inkToSketchIds[lastCheckGestureStroke.Id];
							inkToSketchIds.Remove(lastCheckGestureStroke.Id);
							sketch.RemoveSubstroke(sketch.GetSubstroke(sketchStrokeID));

							this.lassoStrokeCollecting = true;
							sketchPicture.DefaultDrawingAttributes.Color = UIConstants.AnnotationColor;
						}
					}
					else 
					{
						// Do not erase the tap ink; there was no valid check gesture first
						e.Cancel = true;
					}
				}
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
			recognizer.recognize(sketch, sketch.Strokes, true);
		}


		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Check-Tap Triggered Recognition
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Check-Tap Triggered Recognition

		/// <summary>
		/// Enables gesture watching for the check-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void enableCheckTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkAndGesture;
			
			sketchPicture.SetGestureStatus(ApplicationGesture.Check, true);
			sketchPicture.SetGestureStatus(ApplicationGesture.Tap, true);
			sketchPicture.Gesture += new InkCollectorGestureEventHandler(sketchPicture_CheckTapGesture);
		}

		/// <summary>
		/// Disables gesture watching for check-tap recognition trigger.
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkpicture.gesture.aspx"/>
		/// </summary>
		private void disableCheckTapGesture()
		{
			sketchPicture.CollectionMode = CollectionMode.InkOnly;

			ApplicationGesture noGest = ApplicationGesture.NoGesture;
			System.Array theGestureIds = System.Enum.GetValues(noGest.GetType());
			foreach (ApplicationGesture theGestureId in theGestureIds)
			{
				sketchPicture.SetGestureStatus(theGestureId, false);
			}

			sketchPicture.Gesture -= new InkCollectorGestureEventHandler(sketchPicture_CheckTapGesture);
		}

		/// <summary>
		/// Processes the lasso-tap gesture and fires recognition when necessary.  According to 
		/// Microsoft's documentation for the check gesture, "The upward stroke must be twice as long as the smaller downward stroke."
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Gesture event object.  
		/// <see cref="http://msdn2.microsoft.com/en-us/library/microsoft.ink.inkcollectorgestureeventargs.aspx"/>
		/// <see cref="http://msdn2.microsoft.com/en-gb/library/microsoft.ink.applicationgesture.aspx" />
		/// </param>
		private void sketchPicture_CheckTapGesture(object sender, InkCollectorGestureEventArgs e)
		{
			Console.WriteLine("Identified gesture" + e.Gestures[0].Id + " with confidence " + e.Gestures[0].Confidence);
			
			// If there are no gestures for some reason, cancel
			if (e.Gestures.Length == 0)
			{
				e.Cancel = true; // Do not auto-erase gesture ink
				return;
			}

			// If we are annotating, then cancel
			if (annotationCheckBox.Checked)
			{
				e.Cancel = true;
				return;
			}

			ApplicationGesture theGestureId = e.Gestures[0].Id;
			
			// Process check first, and then look for tap
			if (theGestureId == ApplicationGesture.Check)
			{
				// Ensure gesture has minimum confidence
				if (e.Gestures[0].Confidence.CompareTo(UIConstants.GestureConfidenceThreshhold) >= 0)
				{
					// Do not erase the check gesture yet
					e.Cancel = true;
					lastCheckGesture = DateTime.Now.Ticks;
					lastCheckGestureStroke = e.Strokes[0];
				}
			} 
			else if (theGestureId == ApplicationGesture.Tap)
			{
				// Ensure gesture has minimum confidence
				if (e.Gestures[0].Confidence.CompareTo(UIConstants.GestureConfidenceThreshhold) >= 0) 
				{
					// Make sure check gesture happened within time threshold
					if (DateTime.Now.Ticks - lastCheckGesture <= UIConstants.CheckTapGestureThreshold)
					{
						// Make sure the tap stroke gets erased
						e.Cancel = false;

						// Erase the check stroke
						sketchPicture.Ink.Strokes.Remove(lastCheckGestureStroke);
						System.Guid sketchStrokeID = (System.Guid) inkToSketchIds[lastCheckGestureStroke.Id];
						inkToSketchIds.Remove(lastCheckGestureStroke.Id);
						sketch.RemoveSubstroke(sketch.GetSubstroke(sketchStrokeID));
						

						Console.WriteLine("CheckTap firing recognition");
						
						// Fire recognition
						recognizer.recognize(this.sketch, sketch.Strokes, true);
					}
					else 
					{
						// Do not erase the tap ink; there was no valid check gesture first
						e.Cancel = true;
					}
				}
			}
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
				Thread.Sleep(UIConstants.PauseTriggerWait);
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
