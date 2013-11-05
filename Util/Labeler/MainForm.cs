using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using Sketch;
using ConverterXML;
using ConverterJnt;
using ConverterDRS;
using CommandManagement;
using Featurefy;

namespace Labeler
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private CommandManager CM;
		
		private Sketch.Sketch sketch;

		private DomainInfo domainInfo;

		private LabelerPanel labelerPanel;

		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem fileMenuItem;
		private System.Windows.Forms.MenuItem saveSketchMenuItem;
		private System.Windows.Forms.ToolBar mainToolBar;
		private System.Windows.Forms.ToolBarButton fragmentStrokeBtn;
		private System.Windows.Forms.MenuItem loadDomainMenuItem;
		private System.Windows.Forms.ToolBarButton openSketchBtn;
		private System.Windows.Forms.ToolBarButton saveSketchBtn;
		private System.Windows.Forms.ToolBarButton loadDomainBtn;
		private System.Windows.Forms.ToolBarButton separatorBtn1;
		private System.Windows.Forms.ToolBarButton autoFragmentBtn;
		private System.Windows.Forms.MenuItem openSketchMenuItem;
		private System.Windows.Forms.ToolBarButton undoBtn;
		private System.Windows.Forms.ToolBarButton redoBtn;
		private System.Windows.Forms.ToolBarButton separatorBtn2;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem undoMenuItem;
		private System.Windows.Forms.MenuItem redoMenuItem;
		private System.Windows.Forms.MenuItem quitMenuItem;
        private System.Windows.Forms.MenuItem aboutMenuItem;

        private ToolBarButton zoomBtn;
		private MenuItem menuLabeling;
		private MenuItem menuItem6;
		private MenuItem menuItem7;
		private MenuItem menuItem8;
		private MenuItem menuStandardLabel;
		private MenuItem menuMultipleLabeling;
		private MenuItem menuGateLabeling;
		private MenuItem menuNonGateLabeling;
		private ToolBarButton LabelModeButton;
		private ToolBarButton separatorBtn3;

        private IContainer components;
		private ImageList toolbarImages;

		private string prevousSaveDir = null;
		private string previousLoadDir = null;
		private string previousDomainDir = null;

		private int previousSaveType = -1;
		private int previousLoadType = -1;
		private int previousDomainType = -1;
		private MenuItem menuItem3;
		private MenuItem menuItem4;

		private int labelMode;
		private MenuItem menuViewStrokeInfo;
		private MenuItem menuSketchSummary;
		private MenuItem menuResample;
		private MenuItem menuZoom;
		private MenuItem subZoom500;
		private MenuItem subZoom250;
		private MenuItem subZoom200;
		private MenuItem subZoom150;
		private MenuItem subZoom100;
		private StatusBarPanel statusBarZoom;
        private StatusBarPanel statusBarText;
        private MenuItem menuItem9;

		private bool tooltips_shown = true;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Double-buffering code
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			UpdateStyles();

			// Initialize the CommandManager
			CM = new CommandManager();
			
			// Initialize the DomainInfo
			domainInfo = null;

			// Initialize the LabelerPanel
			InitLabelerPanel();
			
			// Re-add all of the window's components
			// This is needed for some reason, otherwise if we just add the LabelerPanel
			// it will try to fill the entire window.
			Controls.Clear();
			Controls.Add(labelerPanel);
			Controls.Add(mainToolBar);
			Controls.Add(statusBar);

			// Debug stuff
			//LoadSketch(@"C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\E85\0128\0128_Sketches\convertedJnt\0128_1.1.1.labeled.xml");
			//LoadDomain(@"C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\E85\Domain3.txt");
			LoadDomain(@"DefaultDomain.txt");
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusBarZoom = new System.Windows.Forms.StatusBarPanel();
            this.statusBarText = new System.Windows.Forms.StatusBarPanel();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenuItem = new System.Windows.Forms.MenuItem();
            this.openSketchMenuItem = new System.Windows.Forms.MenuItem();
            this.saveSketchMenuItem = new System.Windows.Forms.MenuItem();
            this.loadDomainMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.quitMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.undoMenuItem = new System.Windows.Forms.MenuItem();
            this.redoMenuItem = new System.Windows.Forms.MenuItem();
            this.menuResample = new System.Windows.Forms.MenuItem();
            this.menuItem9 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuViewStrokeInfo = new System.Windows.Forms.MenuItem();
            this.menuSketchSummary = new System.Windows.Forms.MenuItem();
            this.menuZoom = new System.Windows.Forms.MenuItem();
            this.subZoom500 = new System.Windows.Forms.MenuItem();
            this.subZoom250 = new System.Windows.Forms.MenuItem();
            this.subZoom200 = new System.Windows.Forms.MenuItem();
            this.subZoom150 = new System.Windows.Forms.MenuItem();
            this.subZoom100 = new System.Windows.Forms.MenuItem();
            this.menuLabeling = new System.Windows.Forms.MenuItem();
            this.menuStandardLabel = new System.Windows.Forms.MenuItem();
            this.menuMultipleLabeling = new System.Windows.Forms.MenuItem();
            this.menuGateLabeling = new System.Windows.Forms.MenuItem();
            this.menuNonGateLabeling = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.aboutMenuItem = new System.Windows.Forms.MenuItem();
            this.mainToolBar = new System.Windows.Forms.ToolBar();
            this.openSketchBtn = new System.Windows.Forms.ToolBarButton();
            this.saveSketchBtn = new System.Windows.Forms.ToolBarButton();
            this.loadDomainBtn = new System.Windows.Forms.ToolBarButton();
            this.separatorBtn1 = new System.Windows.Forms.ToolBarButton();
            this.undoBtn = new System.Windows.Forms.ToolBarButton();
            this.redoBtn = new System.Windows.Forms.ToolBarButton();
            this.separatorBtn2 = new System.Windows.Forms.ToolBarButton();
            this.LabelModeButton = new System.Windows.Forms.ToolBarButton();
            this.separatorBtn3 = new System.Windows.Forms.ToolBarButton();
            this.autoFragmentBtn = new System.Windows.Forms.ToolBarButton();
            this.fragmentStrokeBtn = new System.Windows.Forms.ToolBarButton();
            this.zoomBtn = new System.Windows.Forms.ToolBarButton();
            this.toolbarImages = new System.Windows.Forms.ImageList(this.components);
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarText)).BeginInit();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusBar.Location = new System.Drawing.Point(0, 553);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarZoom,
            this.statusBarText});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(908, 20);
            this.statusBar.TabIndex = 0;
            // 
            // statusBarZoom
            // 
            this.statusBarZoom.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusBarZoom.Name = "statusBarZoom";
            this.statusBarZoom.Text = "Zoom: 100%";
            this.statusBarZoom.Width = 91;
            // 
            // statusBarText
            // 
            this.statusBarText.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statusBarText.Name = "statusBarText";
            this.statusBarText.Width = 800;
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenuItem,
            this.menuItem1,
            this.menuItem3,
            this.menuLabeling,
            this.menuItem2});
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.Index = 0;
            this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.openSketchMenuItem,
            this.saveSketchMenuItem,
            this.loadDomainMenuItem,
            this.menuItem5,
            this.quitMenuItem});
            this.fileMenuItem.Text = "File";
            // 
            // openSketchMenuItem
            // 
            this.openSketchMenuItem.Index = 0;
            this.openSketchMenuItem.RadioCheck = true;
            this.openSketchMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.openSketchMenuItem.Text = "Open Sketch";
            this.openSketchMenuItem.Click += new System.EventHandler(this.openSketchMenuItem_Click);
            // 
            // saveSketchMenuItem
            // 
            this.saveSketchMenuItem.Index = 1;
            this.saveSketchMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.saveSketchMenuItem.Text = "Save Sketch";
            this.saveSketchMenuItem.Click += new System.EventHandler(this.saveSketchMenuItem_Click);
            // 
            // loadDomainMenuItem
            // 
            this.loadDomainMenuItem.Index = 2;
            this.loadDomainMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlL;
            this.loadDomainMenuItem.Text = "Load Domain";
            this.loadDomainMenuItem.Click += new System.EventHandler(this.loadDomainMenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 3;
            this.menuItem5.Text = "-";
            // 
            // quitMenuItem
            // 
            this.quitMenuItem.Index = 4;
            this.quitMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlW;
            this.quitMenuItem.Text = "Quit";
            this.quitMenuItem.Click += new System.EventHandler(this.quitMenuItem_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.undoMenuItem,
            this.redoMenuItem,
            this.menuResample,
            this.menuItem9});
            this.menuItem1.Text = "Edit";
            // 
            // undoMenuItem
            // 
            this.undoMenuItem.Index = 0;
            this.undoMenuItem.Text = "Undo";
            this.undoMenuItem.Click += new System.EventHandler(this.undoMenuItem_Click);
            // 
            // redoMenuItem
            // 
            this.redoMenuItem.Index = 1;
            this.redoMenuItem.Text = "Redo";
            this.redoMenuItem.Click += new System.EventHandler(this.redoMenuItem_Click);
            // 
            // menuResample
            // 
            this.menuResample.Index = 2;
            this.menuResample.Text = "Resample Stroke...";
            this.menuResample.Click += new System.EventHandler(this.menuResample_Click);
            // 
            // menuItem9
            // 
            this.menuItem9.Index = 3;
            this.menuItem9.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.menuItem9.Text = "Print Partial Shape";
            this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4,
            this.menuViewStrokeInfo,
            this.menuSketchSummary,
            this.menuZoom});
            this.menuItem3.Text = "View";
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.Text = "Show Tooltips";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuViewStrokeInfo
            // 
            this.menuViewStrokeInfo.Index = 1;
            this.menuViewStrokeInfo.Shortcut = System.Windows.Forms.Shortcut.CtrlI;
            this.menuViewStrokeInfo.Text = "Stroke Information...";
            this.menuViewStrokeInfo.Click += new System.EventHandler(this.menuViewStrokeInfo_Click);
            // 
            // menuSketchSummary
            // 
            this.menuSketchSummary.Index = 2;
            this.menuSketchSummary.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftI;
            this.menuSketchSummary.Text = "Sketch Information Summary...";
            this.menuSketchSummary.Click += new System.EventHandler(this.menuSketchSummary_Click);
            // 
            // menuZoom
            // 
            this.menuZoom.Index = 3;
            this.menuZoom.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.subZoom500,
            this.subZoom250,
            this.subZoom200,
            this.subZoom150,
            this.subZoom100});
            this.menuZoom.Text = "Zoom";
            // 
            // subZoom500
            // 
            this.subZoom500.Index = 0;
            this.subZoom500.Text = "500%";
            this.subZoom500.Click += new System.EventHandler(this.subZoom500_Click);
            // 
            // subZoom250
            // 
            this.subZoom250.Index = 1;
            this.subZoom250.Text = "250%";
            this.subZoom250.Click += new System.EventHandler(this.subZoom250_Click);
            // 
            // subZoom200
            // 
            this.subZoom200.Index = 2;
            this.subZoom200.Text = "200%";
            this.subZoom200.Click += new System.EventHandler(this.subZoom200_Click);
            // 
            // subZoom150
            // 
            this.subZoom150.Index = 3;
            this.subZoom150.Text = "150%";
            this.subZoom150.Click += new System.EventHandler(this.subZoom150_Click);
            // 
            // subZoom100
            // 
            this.subZoom100.Index = 4;
            this.subZoom100.Text = "100%";
            this.subZoom100.Click += new System.EventHandler(this.subZoom100_Click);
            // 
            // menuLabeling
            // 
            this.menuLabeling.Index = 3;
            this.menuLabeling.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuStandardLabel,
            this.menuMultipleLabeling,
            this.menuGateLabeling,
            this.menuNonGateLabeling});
            this.menuLabeling.Text = "Labeling Mode";
            // 
            // menuStandardLabel
            // 
            this.menuStandardLabel.Checked = true;
            this.menuStandardLabel.Index = 0;
            this.menuStandardLabel.RadioCheck = true;
            this.menuStandardLabel.Text = "Standard";
            this.menuStandardLabel.Click += new System.EventHandler(this.menu_StandardLabelingMode_Click);
            // 
            // menuMultipleLabeling
            // 
            this.menuMultipleLabeling.Index = 1;
            this.menuMultipleLabeling.RadioCheck = true;
            this.menuMultipleLabeling.Text = "Multiple";
            this.menuMultipleLabeling.Click += new System.EventHandler(this.menu_MultipleLabelingMode_Click);
            // 
            // menuGateLabeling
            // 
            this.menuGateLabeling.Index = 2;
            this.menuGateLabeling.RadioCheck = true;
            this.menuGateLabeling.Text = "Gate";
            this.menuGateLabeling.Click += new System.EventHandler(this.menu_GateLabelingMode_Click);
            // 
            // menuNonGateLabeling
            // 
            this.menuNonGateLabeling.Index = 3;
            this.menuNonGateLabeling.RadioCheck = true;
            this.menuNonGateLabeling.Text = "Non-Gate";
            this.menuNonGateLabeling.Click += new System.EventHandler(this.menu_NonGateLablelingMode_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 4;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.aboutMenuItem});
            this.menuItem2.Text = "Help";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Index = 0;
            this.aboutMenuItem.Text = "About";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // mainToolBar
            // 
            this.mainToolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.openSketchBtn,
            this.saveSketchBtn,
            this.loadDomainBtn,
            this.separatorBtn1,
            this.undoBtn,
            this.redoBtn,
            this.separatorBtn2,
            this.LabelModeButton,
            this.separatorBtn3,
            this.autoFragmentBtn,
            this.fragmentStrokeBtn,
            this.zoomBtn});
            this.mainToolBar.ButtonSize = new System.Drawing.Size(80, 43);
            this.mainToolBar.Divider = false;
            this.mainToolBar.DropDownArrows = true;
            this.mainToolBar.ImageList = this.toolbarImages;
            this.mainToolBar.Location = new System.Drawing.Point(0, 0);
            this.mainToolBar.Name = "mainToolBar";
            this.mainToolBar.ShowToolTips = true;
            this.mainToolBar.Size = new System.Drawing.Size(908, 40);
            this.mainToolBar.TabIndex = 1;
            this.mainToolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.mainToolBar_ButtonClick);
            // 
            // openSketchBtn
            // 
            this.openSketchBtn.ImageIndex = 0;
            this.openSketchBtn.Name = "openSketchBtn";
            this.openSketchBtn.Text = "Open Sketch";
            this.openSketchBtn.ToolTipText = "Open a sketch from a file";
            // 
            // saveSketchBtn
            // 
            this.saveSketchBtn.ImageIndex = 1;
            this.saveSketchBtn.Name = "saveSketchBtn";
            this.saveSketchBtn.Text = "Save Sketch";
            this.saveSketchBtn.ToolTipText = "Save the current sketch to a file";
            // 
            // loadDomainBtn
            // 
            this.loadDomainBtn.ImageIndex = 2;
            this.loadDomainBtn.Name = "loadDomainBtn";
            this.loadDomainBtn.Text = "Load Domain";
            this.loadDomainBtn.ToolTipText = "Load a valid domain file";
            // 
            // separatorBtn1
            // 
            this.separatorBtn1.Name = "separatorBtn1";
            this.separatorBtn1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // undoBtn
            // 
            this.undoBtn.Name = "undoBtn";
            this.undoBtn.Text = "Undo";
            this.undoBtn.ToolTipText = "Undo previous action";
            // 
            // redoBtn
            // 
            this.redoBtn.Name = "redoBtn";
            this.redoBtn.Text = "Redo";
            this.redoBtn.ToolTipText = "Redo previous undo";
            // 
            // separatorBtn2
            // 
            this.separatorBtn2.Name = "separatorBtn2";
            this.separatorBtn2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // LabelModeButton
            // 
            this.LabelModeButton.Name = "LabelModeButton";
            this.LabelModeButton.Text = "Labeling Mode";
            this.LabelModeButton.ToolTipText = "Switch the display label mode to Multiple";
            // 
            // separatorBtn3
            // 
            this.separatorBtn3.Name = "separatorBtn3";
            this.separatorBtn3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // autoFragmentBtn
            // 
            this.autoFragmentBtn.Name = "autoFragmentBtn";
            this.autoFragmentBtn.Text = "Auto Fragment";
            this.autoFragmentBtn.ToolTipText = "Automatically fragment the current sketch";
            // 
            // fragmentStrokeBtn
            // 
            this.fragmentStrokeBtn.Name = "fragmentStrokeBtn";
            this.fragmentStrokeBtn.Text = "Frag. Stroke";
            this.fragmentStrokeBtn.ToolTipText = "Hand fragment a selected stroke";
            // 
            // zoomBtn
            // 
            this.zoomBtn.ImageIndex = 5;
            this.zoomBtn.Name = "zoomBtn";
            this.zoomBtn.Text = "Zoom";
            this.zoomBtn.ToolTipText = "Click to zoom in to twice normal scale";
            // 
            // toolbarImages
            // 
            this.toolbarImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("toolbarImages.ImageStream")));
            this.toolbarImages.TransparentColor = System.Drawing.Color.Transparent;
            this.toolbarImages.Images.SetKeyName(0, "openHS.png");
            this.toolbarImages.Images.SetKeyName(1, "saveHS.png");
            this.toolbarImages.Images.SetKeyName(2, "INFO.ICO");
            this.toolbarImages.Images.SetKeyName(3, "otheroptions.ico");
            this.toolbarImages.Images.SetKeyName(4, "search4doc.ico");
            this.toolbarImages.Images.SetKeyName(5, "ZoomHS.png");
            // 
            // menuItem6
            // 
            this.menuItem6.Index = -1;
            this.menuItem6.Text = "";
            // 
            // menuItem7
            // 
            this.menuItem7.Index = -1;
            this.menuItem7.Text = "";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = -1;
            this.menuItem8.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(908, 573);
            this.Controls.Add(this.mainToolBar);
            this.Controls.Add(this.statusBar);
            this.Menu = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.Text = "Labeler";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.statusBarZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarText)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}


		/// <summary>
		/// Initialize a new Panel with certain, default attributes
		/// </summary>
		/// <param name="panel">Panel to set attributes for</param>
		private void SetPanelAttributes(Panel panel)
		{
			panel.AutoScroll = true;
			panel.BackColor = System.Drawing.Color.White;
			panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel.Dock = System.Windows.Forms.DockStyle.Fill;
		}


		/// <summary>
		/// Set the ToolBar attributes.
		/// This really isn't used, but is here incase we need multiple toolbars
		/// </summary>
		/// <param name="toolBar">ToolBar to set attributes for</param>
		private void SetToolBarAttributes(ToolBar toolBar)
		{
			toolBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			toolBar.ButtonSize = new System.Drawing.Size(80, 44);
			toolBar.DropDownArrows = true;
			toolBar.ShowToolTips = true;
		}
		
		
		/// <summary>
		/// Initialize the LabelerPanel
		/// </summary>
		private void InitLabelerPanel()
		{
			labelerPanel = new LabelerPanel(CM, domainInfo);
			SetPanelAttributes(labelerPanel);
		}
		
		#region MENU ITEMS

		/// <summary>
		/// Starts the "Open" dialog allowing the user to open an MIT XML file or a JNT file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void openSketchMenuItem_Click(object sender, System.EventArgs e)
		{
			OpenSketch();
		}


		/// <summary>
		/// Save a Sketch as a file.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void saveSketchMenuItem_Click(object sender, System.EventArgs e)
		{
			SaveSketch();
		}
		
		
		/// <summary>
		/// Load a valid Domain file into the application.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void loadDomainMenuItem_Click(object sender, System.EventArgs e)
		{
			LoadDomain();	
		}
		

		/// <summary>
		/// Quit the application
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void quitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}


		/// <summary>
		/// Undo the previous action
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void undoMenuItem_Click(object sender, System.EventArgs e)
		{
			CM.Undo();
		}


		/// <summary>
		/// Redo the previous undone action
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void redoMenuItem_Click(object sender, System.EventArgs e)
		{
			CM.Redo();
		}
		

		/// <summary>
		/// Open an About menu
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void aboutMenuItem_Click(object sender, System.EventArgs e)
		{
			OpenAboutMenu();
		}


		#endregion

		#region ACTIONS
		
		/// <summary>
		/// Starts the "Open" dialog allowing the user to open an MIT XML file or a JNT file.
		/// </summary>
		private void OpenSketch()
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();
			
			openFileDialog.Title  = "Load a Sketch";
			openFileDialog.Filter = Files.FUtil.OpenFilter;

			if (previousLoadDir != null)
				openFileDialog.InitialDirectory = previousLoadDir;
			if (previousLoadType > 0)
				openFileDialog.FilterIndex = previousLoadType;

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
                labelerPanel.changeStrokeLabeling(0);
				LoadSketch(openFileDialog.FileName, Files.FUtil.OpenFilterIndexToFileType(openFileDialog.FilterIndex));
				labelerPanel.changeZoom(false);
				zoomBtn.ToolTipText = "Click to zoom in to twice normal scale";
				previousLoadDir = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
				previousLoadType = openFileDialog.FilterIndex;
			}
		}

		
		/// <summary>
		/// Loads a valid MIT XML file, JNT file, or DRS file into the application as a Sketch.
		/// </summary>
		/// <param name="filename">The filename to open</param>
		/// <param name="filetype">The file type to open (1=XML, 2=JNT, 3=DRS)</param>
		private void LoadSketch(string filename, Files.Filetype filetype)
		{
			this.statusBarText.Text = "Loading Sketch...";
				
			string extension = System.IO.Path.GetExtension(filename.ToLower());
				
			// Load the Sketch
			switch(filetype)
			{
				case Files.Filetype.XML:
					sketch = new ReadXML(filename).Sketch;
					break;
				case Files.Filetype.JOURNAL:
					sketch = new ReadJnt(filename).Sketch;
					break;
				case Files.Filetype.DRS:
					sketch = new ReadDRS(filename).Sketch;
					break;
				default:
					throw new Exception("Undefined file type selected.");
			}

			this.Text = System.IO.Path.GetFileNameWithoutExtension(filename);
			
            // Initialize the new panels
			this.labelerPanel.Enabled = false;
			this.labelerPanel.Sketch = this.sketch; 
			this.labelerPanel.Enabled = true;

			updateLabelMode(0);
		
			// Clear the CommandManager
			CM.ClearStacks();

			statusBarZoom.Text = "Zoom: 100%";
			this.statusBarText.Text = "";
		}


		/// <summary>
		/// NOTE: Want to later add flags for saving seperately:
		/// - original
		/// - labeled
		/// - fragged
		/// - combination of the above
		/// </summary>
		private void SaveSketch()
		{
			System.Windows.Forms.SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = this.Text;

			saveFileDialog.Filter = Files.FUtil.SaveFilter;
			saveFileDialog.AddExtension = true;

			if (prevousSaveDir != null)
				saveFileDialog.InitialDirectory = prevousSaveDir;
			if (previousSaveType > 0)
				saveFileDialog.FilterIndex = previousSaveType;

			// Write the XML to a file
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				statusBarText.Text = "Saving sketch to " + saveFileDialog.FileName;
				saveFileDialog.FileName = Files.FUtil.EnsureExtension(saveFileDialog.FileName, Files.FUtil.SaveFilterIndexToFileType(saveFileDialog.FilterIndex));
				Files.Filetype ft = Files.FUtil.SaveFilterIndexToFileType(saveFileDialog.FilterIndex);
				Save(saveFileDialog.FileName, ft);
				prevousSaveDir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
				previousSaveType = saveFileDialog.FilterIndex;
			}
		}

		/// <summary>
		/// Actually performs the save operation
		/// </summary>
		private void Save(string filename, Files.Filetype fileType)
		{
			try
			{
				if (sketch == null)
				{
					MessageBox.Show("No data to save", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				switch(fileType)
				{
					case Files.Filetype.XML:
						ConverterXML.MakeXML xmlHolder = new MakeXML(sketch);
						xmlHolder.WriteXML(filename);
						break;
					case Files.Filetype.JOURNAL:
						ConverterJnt.MakeJnt jnt = new MakeJnt(sketch);
						jnt.WriteJnt(filename);
						break;
				}
				statusBarText.Text = "Sketch saved";
			}
			catch (Exception e)
			{
				MessageBox.Show(String.Format("Error writing to file {0}: {1}", filename, e.Message), "Error Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		/// <summary>
		/// Load a valid Domain file into the application.
		/// </summary>
		private void LoadDomain()
		{
			System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();
			
			openFileDialog.Title = "Load Domain File";
			openFileDialog.Filter = "Domain Files (*.txt)|*.txt";

			if (previousDomainDir != null)
				openFileDialog.InitialDirectory = previousDomainDir;
			if (previousDomainType > 0)
				openFileDialog.FilterIndex = previousDomainType;

			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				LoadDomain(openFileDialog.FileName);
				previousDomainDir = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
				previousDomainType = openFileDialog.FilterIndex;
			}
		}

		
		/// <summary>
		/// Load a valid Domain file into the application.
		/// </summary>
		/// <param name="filepath">Filepath of the domain file</param>
		private void LoadDomain(string filepath)
		{
			this.statusBarText.Text = "Loading domain file...";
			
			System.IO.StreamReader sr = new System.IO.StreamReader(filepath);

			this.domainInfo = new DomainInfo();
			string line = sr.ReadLine();
			string[] words = line.Split(null);
			
			// The first line is the study info
			domainInfo.AddInfo(words[0], words[1]);
			line = sr.ReadLine();
			
			// The next line is the domain
			words = line.Split(null);
			domainInfo.AddInfo(words[0], words[1]);
			line = sr.ReadLine();
			
			// Then the rest are labels
			while (line != null && line != "") 
			{
				words = line.Split(null);
				
				string label = words[0];
				int num = int.Parse(words[1]);
				string color = words[2];

				this.domainInfo.AddLabel(num, label, Color.FromName(color));
				line = sr.ReadLine();
			}

			List<string> labels = this.domainInfo.GetLabels();
			string[] labelsWithColors = new string[labels.Count];

			for (int i = 0; i < labelsWithColors.Length; i++)
			{
				labelsWithColors[i] = (string)labels[i] + "   (" + 
					this.domainInfo.GetColor((string)labels[i]).Name + ")";
			}

			sr.Close();

			this.labelerPanel.LTool.InitLabels(this.domainInfo);
			
			this.statusBarText.Text = "";
		}


		/// <summary>
		/// Opens an About menu
		/// </summary>
		private void OpenAboutMenu()
		{
			Labeler.About aboutDialog = new About();
			aboutDialog.ShowDialog();
		}

		#endregion

		#region TOOLBAR

		private void mainToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			// Load the Sketch file
			if (e.Button == this.openSketchBtn)
			{
				OpenSketch();
			}

			// Save the Sketch
			if (e.Button == this.saveSketchBtn)
			{
				SaveSketch();
			}

			// Load the Domain file
			if (e.Button == this.loadDomainBtn)
			{
				LoadDomain();
			}

			// Undo the previous Command
			if (e.Button == this.undoBtn)
			{
				CM.Undo();
				labelerPanel.UpdateColors();
				labelerPanel.Refresh();
			}

			// Redo the previously undone Command
			if (e.Button == this.redoBtn)
			{
				CM.Redo();
				labelerPanel.UpdateColors();
				labelerPanel.Refresh();
			}

			// Autofragment the Sketch
			if (e.Button == this.autoFragmentBtn)
			{
				CM.ExecuteCommand( new CommandList.AutoFragmentCmd(this.labelerPanel) );
			}
			
			// Fragment a stroke by hand
			if (e.Button == this.fragmentStrokeBtn)
			{
				if (this.labelerPanel.Selection.Count > 0)
				{
					Sketch.Substroke selected = 
						this.labelerPanel.LTool.getSubstrokeByMId(this.labelerPanel.Selection[0].Id) as Sketch.Substroke;

					if (selected != null)
					{
						Labeler.FragmentDialogBox fdb = new Labeler.FragmentDialogBox(
							new Sketch.Stroke[1] {selected.ParentStroke}, this.labelerPanel, this.CM);
						fdb.ShowDialog();
						fdb.Dispose();
					}
				}
			}
			// Toggle the label mode
			if (e.Button == LabelModeButton)
			{
				++labelMode;
				if (labelMode >= 4)
					labelMode = 0;
				updateLabelMode();
			}

			// Zoom in
			if (e.Button == this.zoomBtn)
			{
				bool zoomed_in = labelerPanel.changeZoom();
				if (zoomed_in)
				{
					zoomBtn.ToolTipText = "Click to return to normal scale";
					statusBarZoom.Text = "Zoom: 200%";
				}
				else
				{
					zoomBtn.ToolTipText = "Click to zoom in to twice normal scale";
					statusBarZoom.Text = "Zoom: 100%";
				}

			}

			this.labelerPanel.Refresh();
		}

		#endregion

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
		
		private void menu_StandardLabelingMode_Click(object sender, EventArgs e)
		{
			updateLabelMode(0);
		}

		private void menu_MultipleLabelingMode_Click(object sender, EventArgs e)
		{
			updateLabelMode(1);
		}

		private void menu_GateLabelingMode_Click(object sender, EventArgs e)
		{
			updateLabelMode(2);
		}

		private void menu_NonGateLablelingMode_Click(object sender, EventArgs e)
		{
			updateLabelMode(3);
		}

		private void updateLabelMode()
		{
			menuStandardLabel.Checked = (labelMode == 0);
			menuMultipleLabeling.Checked = (labelMode == 1);
			menuGateLabeling.Checked = (labelMode == 2);
			menuNonGateLabeling.Checked = (labelMode == 3);
			labelerPanel.changeStrokeLabeling(labelMode);
		}

		private void updateLabelMode(int mode)
		{
			if (mode > 3)
				mode = 0;
			labelMode = mode;
			updateLabelMode();
		}

		private void menuItem4_Click(object sender, EventArgs e)
		{
			if (tooltips_shown)
			{
				tooltips_shown = false;
				menuItem4.Checked = false;
				labelerPanel.show_tooltip = false;
			}
			else
			{
				tooltips_shown = true;
				menuItem4.Checked = true;
				labelerPanel.show_tooltip = true;
			}
		}

		private void menuViewStrokeInfo_Click(object sender, EventArgs e)
		{
			FeatureSketch sketchFeatures = new FeatureSketch(ref sketch);
			Microsoft.Ink.Strokes selected = labelerPanel.Selection;
			if (selected.Count != 1)
			{
				MessageBox.Show("You must select a single stroke to view information for", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			Microsoft.Ink.Stroke s = selected[0];
			Substroke ss = labelerPanel.LTool.getSubstrokeByMId(s.Id);
			StrokeInfoForm.strokeInfoForm sif = new StrokeInfoForm.strokeInfoForm(sketchFeatures.GetFeatureStrokeByStrokeGUID(ss.Id), sketchFeatures, s, ss);
			sif.Show();
		}

		private void menuSketchSummary_Click(object sender, EventArgs e)
		{
			FeatureSketch sketchFeatures = new FeatureSketch(ref sketch);
			SketchSummary ss = new SketchSummary(sketchFeatures);
			ss.Show();
		}

		private void menuResample_Click(object sender, EventArgs e)
		{
			Microsoft.Ink.Strokes selected = labelerPanel.Selection;
			if (selected.Count != 1)
			{
				MessageBox.Show("You must select a single stroke to view information for", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			Microsoft.Ink.Stroke s = selected[0];
			Substroke ss = labelerPanel.LTool.getSubstrokeByMId(s.Id);

			ResampleDialog r = new ResampleDialog(ss.PointsL.Count);
			if (r.ShowDialog() == DialogResult.OK)
			{
				CM.ExecuteCommand(new CommandList.ResampleCmd(ref sketch, ref labelerPanel, ref ss, r.NumPoints));
			}
			r.Dispose();
		}

		private void subZoom500_Click(object sender, EventArgs e)
		{
			labelerPanel.changeZoom(5.0);
			zoomBtn.ToolTipText = "Click to return to normal scale";
			statusBarZoom.Text = "Zoom: 500%";
		}

		private void subZoom250_Click(object sender, EventArgs e)
		{
			labelerPanel.changeZoom(2.5);
			zoomBtn.ToolTipText = "Click to return to normal scale";
			statusBarZoom.Text = "Zoom: 250%";
		}

		private void subZoom200_Click(object sender, EventArgs e)
		{
			labelerPanel.changeZoom(2.0);
			zoomBtn.ToolTipText = "Click to return to normal scale";
			statusBarZoom.Text = "Zoom: 200%";
		}

		private void subZoom150_Click(object sender, EventArgs e)
		{
			labelerPanel.changeZoom(1.5);
			zoomBtn.ToolTipText = "Click to return to normal scale";
			statusBarZoom.Text = "Zoom: 150%";
		}

		private void subZoom100_Click(object sender, EventArgs e)
		{
			labelerPanel.changeZoom(1.0);
			zoomBtn.ToolTipText = "Click to zoom in to twice normal scale";
			statusBarZoom.Text = "Zoom: 100%";
        }


        #region Printing of Partial Gates (for Eric P)

        private void menuItem9_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Title = "Qualifier for Partial Gate (e.g. noLeft, noTop, etc.)";
            saveDlg.Filter = "Image Training Files (*.txt)|*.txt";
            string dir = "C:\\Documents and Settings\\eric\\My Documents\\Research\\CURRENT\\imagebased\\image_training";
            if (System.IO.Directory.Exists(dir))
                saveDlg.InitialDirectory = dir;
            else
                saveDlg.RestoreDirectory = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                string filename = System.IO.Path.GetDirectoryName(saveDlg.FileName);
                string qualifier = System.IO.Path.GetFileNameWithoutExtension(saveDlg.FileName);

                int index = this.Text.IndexOf('_');
                string userNum = this.Text.Substring(0, index);
                if (this.Text.Contains("_HMC_"))
                {
                    int user = Convert.ToInt16(userNum);
                    user += 12;
                    userNum = user.ToString();
                }

                string task = "";
                if (this.Text.Contains("_AND_") || 
                    this.Text.Contains("_OR_") || 
                    this.Text.Contains("_NAND_") || 
                    this.Text.Contains("_NOR_") || 
                    this.Text.Contains("_NOT_") || 
                    this.Text.Contains("_XOR_"))
                    task = "RPT";
                else if (this.Text.Contains("_EQ1_") || 
                    this.Text.Contains("_EQ2_"))
                    task = "EQN";
                else if (this.Text.Contains("_COPY1_") || 
                    this.Text.Contains("_COPY2_"))
                    task = "CPY";

                string platform = "";
                if (this.Text.Contains("_P."))
                    platform = "P";
                else if (this.Text.Contains("_T."))
                    platform = "T";

                List<Substroke> strokes = new List<Substroke>();
                Microsoft.Ink.Strokes mStrokes = this.labelerPanel.Selection;

                string gate = "";
                bool gateFound = false;
                foreach (Microsoft.Ink.Stroke s in mStrokes)
                {
                    strokes.Add(this.labelerPanel.LTool.getSubstrokeByMId(s.Id));
                    if (!gateFound)
                    {
                        if (this.labelerPanel.LTool.getSubstrokeByMId(s.Id).FirstLabelL != "wire" && this.labelerPanel.LTool.getSubstrokeByMId(s.Id).FirstLabelL != "mesh")
                        {
                            gate = this.labelerPanel.LTool.getSubstrokeByMId(s.Id).FirstLabel;
                            gateFound = true;
                        }
                    }
                }

                int num = getHighestFileNum(filename, gate, userNum, platform, task, qualifier) + 1;
                string gateNum = num.ToString().PadLeft(3, "0".ToCharArray()[0]);
                
                filename += "\\" + gate + "_" + userNum + "_" + platform + "_" + task + "_" + qualifier + "_" + gateNum + ".txt";

                PrintPartialShape(strokes, filename);
            }
        }

        /// <summary>
        /// Takes a list of strokes and a filename and prints them to a text file
        /// </summary>
        /// <param name="strokes">List of strokes in the partial shape</param>
        /// <param name="filename">Filename to save data in</param>
        private void PrintPartialShape(List<Substroke> strokes, string filename)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename);

            int numPoints = 0;
            foreach (Substroke s in strokes)
                numPoints += s.Points.Length;

            writer.WriteLine(numPoints.ToString());

            foreach (Substroke s in strokes)
            {
                for (int i = 0; i < s.Points.Length; i++)
                {
                    writer.Write(s.Points[i].X.ToString("#0") + " " + s.Points[i].Y.ToString("#0"));
                    if (i != s.Points.Length - 1) // Not end of stroke
                        writer.WriteLine(" 0");
                    else // Is end of stroke
                        writer.WriteLine(" 1");
                }
            }

            writer.Close();
        }

        /// <summary>
        /// Look in a given directory for files that begin with 'shapeType'
        /// Get the highest number of that shapetype for a particular user,
        /// platform, and gate partial (e.g. left, right, top, etc.)
        /// </summary>
        /// <param name="dir">Directory to look in</param>
        /// <param name="shapeType">Shape Type to find highest number of</param>
        /// <param name="userNumber">User number to find highest number of</param>
        /// <param name="platform">Platform to find highest number of</param>
        /// <param name="partial">Part of gate to find highest number of</param>
        /// <returns>Highest number of specified shapetype</returns>
        private int getHighestFileNum(string dir, string shapeType, string userNumber, string platform, string task, string partial)
        {
            string[] filenames = System.IO.Directory.GetFiles(dir);
            List<string> validFileNames = new List<string>();

            for (int i = 0; i < filenames.Length; i++)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(filenames[i]);
                int length = shapeType.Length + userNumber.Length + platform.Length + task.Length + partial.Length + 5;
                string test = shapeType + "_" + userNumber + "_" + platform + "_" + task + "_" + partial + "_";
                if (name.Length >= length)
                {
                    if (name.Substring(0, length) == test)
                        validFileNames.Add(name);
                }
            }

            if (validFileNames.Count == 0)
                return 0;

            validFileNames.Sort();

            string num = validFileNames[validFileNames.Count - 1];
            string sub = userNumber + "_" + platform + "_" + task + "_" + partial + "_";
            int len = userNumber.Length + platform.Length + task.Length + partial.Length + 4;
            num = num.Substring(num.IndexOf(sub) + len);

            return Convert.ToInt16(num);
        }

        #endregion

        /* Eric's stuff which is no longer used in the labeler
        
        private List<StrokeFeatures> strokeFeatures;

        private Cluster.AllClusterSets allShapeClustersSpatial;
        private Cluster.AllClusterSets allShapeClustersTemporal;
        
        //private ToolBarButton switchLabelingButton;
        private ToolBarButton findWiresBtn;
        
        private void nextCluster_click(object sender, EventArgs e)
        {
            Cluster.AllClusterSets allShapeClusters = allShapeClustersSpatial;

            allShapeClusters.nextClusterSet();

            labelerPanel.clusterClassifications =
                allShapeClusters.getClusterClassifications(allShapeClusters.ClusterSets[allShapeClusters.CurrentClusterSet], sketch);

            labelerPanel.changeStrokeLabeling(5);
            string str = "Cluster #" + allShapeClusters.CurrentClusterSet.ToString() 
                //+ ":  " + allShapeClusters.currentDistances()
                + ":  " + allShapeClusters.currentTimes()
                + "     AvgArcLength= " + strokeFeatures[0].AvgArcLength.ToString("#0.0");
            statusBarText.Text = str;

            foreach (Cluster.Cluster c in allShapeClusters.ClusterSets[allShapeClusters.CurrentClusterSet].Clusters)
            {
                if (c.JustMerged)
                {
                    this.labelerPanel.thickenCluster(c.Strokes);
                }
            }
        }

        private void previousCluster_click(object sender, EventArgs e)
        {
            Cluster.AllClusterSets allShapeClusters = allShapeClustersSpatial;

            allShapeClusters.previousClusterSet();

            labelerPanel.clusterClassifications =
                allShapeClusters.getClusterClassifications(allShapeClusters.ClusterSets[allShapeClusters.CurrentClusterSet], sketch);

            labelerPanel.changeStrokeLabeling(5);
            string str = "Cluster #" + allShapeClusters.CurrentClusterSet.ToString()
                //+ ":  " + allShapeClusters.currentDistances()
                + ":  " + allShapeClusters.currentTimes()
                + "     AvgArcLength= " + strokeFeatures[0].AvgArcLength.ToString("#0.0");
            statusBarText.Text = str;

            foreach (Cluster.Cluster c in allShapeClusters.ClusterSets[allShapeClusters.CurrentClusterSet].Clusters)
            {
                if (c.JustMerged)
                {
                    this.labelerPanel.thickenCluster(c.Strokes);
                }
            }
        }

        private void showBestClusters(Cluster.AllClusterSets allClusters)
        {
            allClusters.nextClusterSet();

            labelerPanel.clusterClassifications =
                //allClusters.getClusterClassifications(allClusters.ClusterSets[allClusters.bestCluster], this.sketch);
                allClusters.getClusterClassifications(allClusters.BestClusterSet, this.sketch);

            labelerPanel.changeStrokeLabeling(5);
            this.statusBarText.Text = allClusters.CurrentClusterSet.ToString();
        }
        
        private void runSyncAndGetResults()
		{
			System.Diagnostics.Process prog = new System.Diagnostics.Process();
			prog.StartInfo.FileName = "C:\\nn.exe";
			prog.StartInfo.Arguments = "a C:\\weights.net C:\\temp.txt C:\\out.txt";
			prog.StartInfo.CreateNoWindow = true;

			bool started = prog.Start();
			prog.WaitForExit(1000);
		}

		private void runSyncAndGetResultsSecond()
		{
			System.Diagnostics.Process prog = new System.Diagnostics.Process();
			prog.StartInfo.FileName = "C:\\nn.exe";
			prog.StartInfo.Arguments = "a C:\\weights_wire.net C:\\temp_wire.txt C:\\out_wire.txt";
			prog.StartInfo.CreateNoWindow = true;

			bool started = prog.Start();
			prog.WaitForExit(1000);
		}

		private void classifySketch()
		{
			if (this.sketch != null)
			{
                bool threeWay = false;
                
				List<StrokeFeatures> features = StrokeFeatures.getMultiStrokeFeatures(this.sketch);
				this.strokeFeatures = features;
				StrokeFeatures.printFeaturesNoClass(features);

				runSyncAndGetResults();

				this.labelerPanel.wireClassifications = StrokeFeatures.readClassification2way();
                //addClassificationsToSubstrokes(StrokeFeatures.readClassification());
				this.strokeFeatures = StrokeFeatures.applyClassifications(this.strokeFeatures, this.labelerPanel.wireClassifications);

                // Create Initial seeding of clusters
                Dictionary<Guid, string> strokeClassifications = new Dictionary<Guid, string>(this.sketch.Substrokes.Length);
                foreach (StrokeFeatures stroke in this.strokeFeatures)
                    strokeClassifications.Add(stroke.Id, stroke.Classification);

                // Find all cluster sets
                allShapeClustersSpatial = new Cluster.AllClusterSets(this.sketch, strokeClassifications, "Shape", true);
                //allShapeClustersTemporal = new Cluster.AllClusterSets(this.sketch, strokeClassifications, "Text", false);

                //Cluster.AllClusterSets.printDistances(allShapeClustersSpatial.MergeDistances, allShapeClustersTemporal.TemporalMergeDistances);

                /*
                AllStrokeFeatures allFeatures = new AllStrokeFeatures(this.sketch);
                this.strokeFeatures = allFeatures.Features;
                allFeatures.printFeaturesNoClass();

                runSyncAndGetResults();

                this.labelerPanel.wireClassifications = allFeatures.readClassification();
                allFeatures.applyClassifications(this.labelerPanel.wireClassifications);

                Cluster.SketchClusters clusters = new Cluster.SketchClusters(this.sketch, this.strokeFeatures);
                 * 
			}
		}

        private void addClassificationsToSubstrokes(List<double[]> nnClass)
        {

        }

		private void featurizeSketch(string filename)
		{
			if (this.sketch != null)
			{
                
				List<StrokeFeatures> features = StrokeFeatures.getMultiStrokeFeatures(this.sketch);
				this.strokeFeatures = features;
				StrokeFeatures.print_featuresDT(features, filename, this.sketch);
                
                /*
                AllStrokeFeatures allFeatures = new AllStrokeFeatures(this.sketch);
                this.strokeFeatures = allFeatures.Features;
                allFeatures.printFeatures(filename, this.sketch);
                 * 
			}
		}

        // Eric's additions

		/// <summary>
		/// Featurizes the current sketch
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void FeaturizeThisSketchMenu_Click(object sender, System.EventArgs e)
		{
			statusBarText.Text = "Finding Stroke Features";
			SaveFileDialog saveDlg = new SaveFileDialog();
			saveDlg.Filter = "Text Files (*.txt)|*.txt";
			saveDlg.RestoreDirectory = true;
			string fName = "";
			if (saveDlg.ShowDialog() == DialogResult.OK)
				fName = saveDlg.FileName;

			featurizeSketch(fName);

			this.statusBarText.Text = "";
		}

		private int findStrokeNum(Guid ID)
		{
			int num = -1;
			for (int i = 0; i < this.sketch.Substrokes.Length; i++)
			{
				if (this.sketch.Substrokes[i].XmlAttrs.Id == ID)
					num = i;
			}

			return num;
		}

		/// <summary>
		/// Opens a dialog box to select multiple sketches to featurize.
		/// </summary>
		/// <param name="sender">Reference to the object that raised the event</param>
		/// <param name="e">Passes an object specific to the event that is being handled</param>
		private void FeaturizeMultSketchesMenu_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "XML (*.xml)|*.xml";
			dlg.RestoreDirectory = true;
			dlg.Multiselect = true;

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				SaveFileDialog saveDlg = new SaveFileDialog();
				saveDlg.Filter = "Text Files (*.txt)|*.txt";
				saveDlg.RestoreDirectory = true;
				string fName = "";
				if (saveDlg.ShowDialog() == DialogResult.OK)
					fName = saveDlg.FileName;

				for (int i = 0; i < dlg.FileNames.Length; i++)
				{
					LoadSketch(dlg.FileNames[i], Files.Filetype.XML);

					string txt = "Finding Stroke Features for Sketch # " + (i + 1).ToString() + " of " + dlg.FileNames.Length.ToString();
					this.statusBarText.Text = txt;
					this.Refresh();

					featurizeSketch(fName);
				}
				this.statusBarText.Text = "";
			}
		}
        
        
        
         * */

        /*
         * Came from within mainToolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
			// Find Wires
			if (e.Button == this.toolBarButton2)    //(e.Button == this.findWiresBtn)
			{
				this.classifySketch();

				this.labelerPanel.changeStrokeLabeling(4);

				this.strokeInfoBtn.Visible = true;
			}

            // Change cluster view
            if (e.Button == this.strokeInfoBtn)
            {
                showBestClusters(allShapeClustersSpatial);
            }
         * 
         * 
         * Came from within the OpenSketch()
        labelerPanel.wireClassifications = new List<double>();
        labelerPanel.clusterClassifications = new Dictionary<Guid, int>();
             * */

    }
}
