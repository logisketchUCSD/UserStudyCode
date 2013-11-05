//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: MainForm.cs
// 
//-------------------------------------------------------------------------- 
using System;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Ink;

namespace InkExplorer.NET
{
    public class Form1 : System.Windows.Forms.Form
    {
        private Microsoft.Ink.InkPicture inkPicture1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Splitter splitter2;
        private System.Windows.Forms.ContextMenu cmStrokes;
        private System.Windows.Forms.ContextMenu cmExtendedProperties;
        private System.Windows.Forms.ContextMenu cmCustomStrokes;
        private System.Windows.Forms.ContextMenu cmDrawingAttributes;
        private System.Windows.Forms.ContextMenu cmStroke;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem miInkEM;
        private System.Windows.Forms.MenuItem miSelectEM;
        private System.Windows.Forms.MenuItem miStrokeEraseEM;
        private System.Windows.Forms.MenuItem miPointEraseEM;
        private System.Windows.Forms.MenuItem miRecognize;
        private System.Windows.Forms.MenuItem miStrokesRotate;
        private System.Windows.Forms.MenuItem miStrokeRotate;
        private System.Windows.Forms.MenuItem miAddProp;
        private System.Windows.Forms.MenuItem miClearProp;
        private System.Windows.Forms.MenuItem miModifyDA;
        private System.Windows.Forms.MenuItem miModifyStrokesDA;
        private System.Windows.Forms.MenuItem miClear;
        private System.Windows.Forms.MenuItem miDelete;
        private System.Windows.Forms.MenuItem miAddAll;
        private System.Windows.Forms.MenuItem miClearCS;
        private System.Windows.Forms.MenuItem miAddSelected;
        private System.Windows.Forms.MenuItem miSplit;
        private System.Windows.Forms.MenuItem miFile;
        private System.Windows.Forms.MenuItem miExit;
        private System.Windows.Forms.MenuItem miEdit;
        private System.Windows.Forms.MenuItem miView;
        private System.Windows.Forms.MenuItem miOpen;
        private System.Windows.Forms.MenuItem miSave;
        private System.Windows.Forms.MenuItem miClearInk;
        private System.Windows.Forms.MenuItem miCut;
        private System.Windows.Forms.MenuItem miCopy;
        private System.Windows.Forms.MenuItem mipaste;
        private System.Windows.Forms.MenuItem miSelectAll;
        private System.Windows.Forms.MenuItem miZoomIn;
        private System.Windows.Forms.MenuItem miZoomOut;
        private System.Windows.Forms.MenuItem miFlatten;
        private System.Windows.Forms.MenuItem miPrint;
        private System.Windows.Forms.MenuItem miRemove;
        private System.Windows.Forms.MenuItem miFuzz;
        private System.Windows.Forms.MenuItem miDivide;
        private System.Windows.Forms.MenuItem miRotateInk;
        private System.Windows.Forms.MenuItem miTools;
        private System.Windows.Forms.MenuItem miDrawStrokeBounds;
        private System.Windows.Forms.MenuItem miDrawStrokesBounds;
        private System.Windows.Forms.MenuItem miHitTest;
        private System.Windows.Forms.MenuItem miDrawCusps;
        private System.Windows.Forms.MenuItem miSelfIntersect;
        private System.Windows.Forms.MenuItem miStrokeIntersect;
        private System.Windows.Forms.MenuItem miRefresh;
        private System.Windows.Forms.MenuItem miPenSettings;
        private System.Windows.Forms.MenuItem miDeleteStrokes;
        private System.Windows.Forms.MenuItem miEditingMode;
        private System.Windows.Forms.Panel panel1;
        private TreeNode strokesNode;
        private TreeNode extpropNode;
        private TreeNode custStrokesNode;
        private bool isUpdating = false;
        private Point penPt = Point.Empty;
        private Rectangle invalidateRect = Rectangle.FromLTRB(0, 0, 0, 0);
        private Pen redPen = new Pen(Color.Red, 3f);
        private Pen blackPen = new Pen(Color.Black, 3f);
        private Pen greenPen = new Pen(Color.LightGreen, 3f);
        private Pen activePen;
        private System.ComponentModel.Container components = null;

        // Diameter of HitTest area in pixels.
        private const int HitSize = 30;
        
        // Margin between pages imported from JNT files in ink space coordinates.
        private const int pageMargin = 500;

        /// <summary>
        /// The main Form's constructor.
        /// </summary>
        public Form1()
        {
            // Initilize Windows Forms UI.
            InitializeComponent();
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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.miFile = new System.Windows.Forms.MenuItem();
            this.miOpen = new System.Windows.Forms.MenuItem();
            this.miSave = new System.Windows.Forms.MenuItem();
            this.miPrint = new System.Windows.Forms.MenuItem();
            this.miClearInk = new System.Windows.Forms.MenuItem();
            this.miExit = new System.Windows.Forms.MenuItem();
            this.miEditingMode = new System.Windows.Forms.MenuItem();
            this.miInkEM = new System.Windows.Forms.MenuItem();
            this.miSelectEM = new System.Windows.Forms.MenuItem();
            this.miStrokeEraseEM = new System.Windows.Forms.MenuItem();
            this.miPointEraseEM = new System.Windows.Forms.MenuItem();
            this.miHitTest = new System.Windows.Forms.MenuItem();
            this.miEdit = new System.Windows.Forms.MenuItem();
            this.miCut = new System.Windows.Forms.MenuItem();
            this.miCopy = new System.Windows.Forms.MenuItem();
            this.mipaste = new System.Windows.Forms.MenuItem();
            this.miSelectAll = new System.Windows.Forms.MenuItem();
            this.miView = new System.Windows.Forms.MenuItem();
            this.miZoomIn = new System.Windows.Forms.MenuItem();
            this.miZoomOut = new System.Windows.Forms.MenuItem();
            this.miRotateInk = new System.Windows.Forms.MenuItem();
            this.miRefresh = new System.Windows.Forms.MenuItem();
            this.miTools = new System.Windows.Forms.MenuItem();
            this.miDivide = new System.Windows.Forms.MenuItem();
            this.miPenSettings = new System.Windows.Forms.MenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.listView1 = new System.Windows.Forms.ListView();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.inkPicture1 = new Microsoft.Ink.InkPicture();
            this.cmStrokes = new System.Windows.Forms.ContextMenu();
            this.miDrawStrokesBounds = new System.Windows.Forms.MenuItem();
            this.miRecognize = new System.Windows.Forms.MenuItem();
            this.miModifyStrokesDA = new System.Windows.Forms.MenuItem();
            this.miStrokesRotate = new System.Windows.Forms.MenuItem();
            this.miDeleteStrokes = new System.Windows.Forms.MenuItem();
            this.miClear = new System.Windows.Forms.MenuItem();
            this.cmExtendedProperties = new System.Windows.Forms.ContextMenu();
            this.miAddProp = new System.Windows.Forms.MenuItem();
            this.miClearProp = new System.Windows.Forms.MenuItem();
            this.cmCustomStrokes = new System.Windows.Forms.ContextMenu();
            this.miAddSelected = new System.Windows.Forms.MenuItem();
            this.miAddAll = new System.Windows.Forms.MenuItem();
            this.miClearCS = new System.Windows.Forms.MenuItem();
            this.cmDrawingAttributes = new System.Windows.Forms.ContextMenu();
            this.miModifyDA = new System.Windows.Forms.MenuItem();
            this.cmStroke = new System.Windows.Forms.ContextMenu();
            this.miDrawStrokeBounds = new System.Windows.Forms.MenuItem();
            this.miDrawCusps = new System.Windows.Forms.MenuItem();
            this.miSelfIntersect = new System.Windows.Forms.MenuItem();
            this.miStrokeIntersect = new System.Windows.Forms.MenuItem();
            this.miDelete = new System.Windows.Forms.MenuItem();
            this.miRemove = new System.Windows.Forms.MenuItem();
            this.miSplit = new System.Windows.Forms.MenuItem();
            this.miStrokeRotate = new System.Windows.Forms.MenuItem();
            this.miFlatten = new System.Windows.Forms.MenuItem();
            this.miFuzz = new System.Windows.Forms.MenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.miFile,
                                                                                      this.miEditingMode,
                                                                                      this.miEdit,
                                                                                      this.miView,
                                                                                      this.miTools});
            // 
            // miFile
            // 
            this.miFile.Index = 0;
            this.miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                   this.miOpen,
                                                                                   this.miSave,
                                                                                   this.miPrint,
                                                                                   this.miClearInk,
                                                                                   this.miExit});
            this.miFile.Text = "File";
            // 
            // miOpen
            // 
            this.miOpen.Index = 0;
            this.miOpen.Text = "Open ...";
            this.miOpen.Click += new System.EventHandler(this.miOpen_Click);
            // 
            // miSave
            // 
            this.miSave.Index = 1;
            this.miSave.Text = "Save As ...";
            this.miSave.Click += new System.EventHandler(this.miSave_Click);
            // 
            // miPrint
            // 
            this.miPrint.Index = 2;
            this.miPrint.Text = "Print ...";
            this.miPrint.Click += new System.EventHandler(this.miPrint_Click);
            // 
            // miClearInk
            // 
            this.miClearInk.Index = 3;
            this.miClearInk.Text = "Clear";
            this.miClearInk.Click += new System.EventHandler(this.miClearInk_Click);
            // 
            // miExit
            // 
            this.miExit.Index = 4;
            this.miExit.Text = "Exit";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // miEditingMode
            // 
            this.miEditingMode.Index = 1;
            this.miEditingMode.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                          this.miInkEM,
                                                                                          this.miSelectEM,
                                                                                          this.miStrokeEraseEM,
                                                                                          this.miPointEraseEM,
                                                                                          this.miHitTest});
            this.miEditingMode.Text = "Mode";
            // 
            // miInkEM
            // 
            this.miInkEM.Index = 0;
            this.miInkEM.Text = "Ink";
            this.miInkEM.Click += new System.EventHandler(this.miInkEM_Click);
            // 
            // miSelectEM
            // 
            this.miSelectEM.Index = 1;
            this.miSelectEM.Text = "Select";
            this.miSelectEM.Click += new System.EventHandler(this.miSelectEM_Click);
            // 
            // miStrokeEraseEM
            // 
            this.miStrokeEraseEM.Index = 2;
            this.miStrokeEraseEM.Text = "Stroke Erase";
            this.miStrokeEraseEM.Click += new System.EventHandler(this.miStrokeEraseEM_Click);
            // 
            // miPointEraseEM
            // 
            this.miPointEraseEM.Index = 3;
            this.miPointEraseEM.Text = "Point Erase";
            this.miPointEraseEM.Click += new System.EventHandler(this.miPointEraseEM_Click);
            // 
            // miHitTest
            // 
            this.miHitTest.Index = 4;
            this.miHitTest.Text = "Hit Test";
            this.miHitTest.Click += new System.EventHandler(this.miHitTest_Click);
            // 
            // miEdit
            // 
            this.miEdit.Index = 2;
            this.miEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                   this.miCut,
                                                                                   this.miCopy,
                                                                                   this.mipaste,
                                                                                   this.miSelectAll});
            this.miEdit.Text = "Edit";
            // 
            // miCut
            // 
            this.miCut.Index = 0;
            this.miCut.Text = "Cut";
            this.miCut.Click += new System.EventHandler(this.miCut_Click);
            // 
            // miCopy
            // 
            this.miCopy.Index = 1;
            this.miCopy.Text = "Copy";
            this.miCopy.Click += new System.EventHandler(this.miCopy_Click);
            // 
            // mipaste
            // 
            this.mipaste.Index = 2;
            this.mipaste.Text = "Paste";
            this.mipaste.Click += new System.EventHandler(this.mipaste_Click);
            // 
            // miSelectAll
            // 
            this.miSelectAll.Index = 3;
            this.miSelectAll.Text = "Select All";
            this.miSelectAll.Click += new System.EventHandler(this.miSelectAll_Click);
            // 
            // miView
            // 
            this.miView.Index = 3;
            this.miView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                   this.miZoomIn,
                                                                                   this.miZoomOut,
                                                                                   this.miRotateInk,
                                                                                   this.miRefresh});
            this.miView.Text = "View";
            // 
            // miZoomIn
            // 
            this.miZoomIn.Index = 0;
            this.miZoomIn.Text = "Zoom In";
            this.miZoomIn.Click += new System.EventHandler(this.miZoomIn_Click);
            // 
            // miZoomOut
            // 
            this.miZoomOut.Index = 1;
            this.miZoomOut.Text = "Zoom Out";
            this.miZoomOut.Click += new System.EventHandler(this.miZoomOut_Click);
            // 
            // miRotateInk
            // 
            this.miRotateInk.Index = 2;
            this.miRotateInk.Text = "Rotate";
            this.miRotateInk.Click += new System.EventHandler(this.miRotateInk_Click);
            // 
            // miRefresh
            // 
            this.miRefresh.Index = 3;
            this.miRefresh.Text = "Refresh";
            this.miRefresh.Click += new System.EventHandler(this.miRefresh_Click);
            // 
            // miTools
            // 
            this.miTools.Index = 4;
            this.miTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                    this.miDivide,
                                                                                    this.miPenSettings});
            this.miTools.Text = "Tools";
            // 
            // miDivide
            // 
            this.miDivide.Index = 0;
            this.miDivide.Text = "Divide Ink";
            this.miDivide.Click += new System.EventHandler(this.miDivide_Click);
            // 
            // miPenSettings
            // 
            this.miPenSettings.Index = 1;
            this.miPenSettings.Text = "Pen Settings ...";
            this.miPenSettings.Click += new System.EventHandler(this.miPenSettings_Click);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView1.HideSelection = false;
            this.treeView1.ImageIndex = -1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
                                                                                  new System.Windows.Forms.TreeNode("Ink", new System.Windows.Forms.TreeNode[] {
                                                                                                                                                                   new System.Windows.Forms.TreeNode("Strokes"),
                                                                                                                                                                   new System.Windows.Forms.TreeNode("ExtendedProperties"),
                                                                                                                                                                   new System.Windows.Forms.TreeNode("CustomStrokes")})});
            this.treeView1.SelectedImageIndex = -1;
            this.treeView1.Size = new System.Drawing.Size(213, 564);
            this.treeView1.TabIndex = 0;
            this.treeView1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeSelect);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(213, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 564);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // listView1
            // 
            this.listView1.Dock = System.Windows.Forms.DockStyle.Top;
            this.listView1.Location = new System.Drawing.Point(216, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(616, 208);
            this.listView1.TabIndex = 2;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter2.Location = new System.Drawing.Point(216, 208);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(616, 3);
            this.splitter2.TabIndex = 3;
            this.splitter2.TabStop = false;
            // 
            // inkPicture1
            // 
            this.inkPicture1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(255)), ((System.Byte)(255)), ((System.Byte)(192)));
            this.inkPicture1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.inkPicture1.Location = new System.Drawing.Point(0, 0);
            this.inkPicture1.MarginX = -2147483648;
            this.inkPicture1.MarginY = -2147483648;
            this.inkPicture1.Name = "inkPicture1";
            this.inkPicture1.Size = new System.Drawing.Size(617, 354);
            this.inkPicture1.TabIndex = 4;
            this.inkPicture1.Stroke += new Microsoft.Ink.InkCollectorStrokeEventHandler(this.inkPicture1_Stroke);
            this.inkPicture1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.inkPicture1_MouseMove);
            this.inkPicture1.Paint += new System.Windows.Forms.PaintEventHandler(this.inkPicture1_Paint);
            // 
            // cmStrokes
            // 
            this.cmStrokes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.miDrawStrokesBounds,
                                                                                      this.miRecognize,
                                                                                      this.miModifyStrokesDA,
                                                                                      this.miStrokesRotate,
                                                                                      this.miDeleteStrokes,
                                                                                      this.miClear});
            // 
            // miDrawStrokesBounds
            // 
            this.miDrawStrokesBounds.Index = 0;
            this.miDrawStrokesBounds.Text = "Draw Bounds";
            this.miDrawStrokesBounds.Click += new System.EventHandler(this.miDrawStrokesBounds_Click);
            // 
            // miRecognize
            // 
            this.miRecognize.Index = 1;
            this.miRecognize.Text = "Get RecognitionResult";
            // 
            // miModifyStrokesDA
            // 
            this.miModifyStrokesDA.Index = 2;
            this.miModifyStrokesDA.Text = "Modify DrawingAttributes";
            this.miModifyStrokesDA.Click += new System.EventHandler(this.miModifyStrokesDA_Click);
            // 
            // miStrokesRotate
            // 
            this.miStrokesRotate.Index = 3;
            this.miStrokesRotate.Text = "Rotate";
            this.miStrokesRotate.Click += new System.EventHandler(this.miRotate_Click);
            // 
            // miDeleteStrokes
            // 
            this.miDeleteStrokes.Index = 4;
            this.miDeleteStrokes.Text = "Delete";
            this.miDeleteStrokes.Click += new System.EventHandler(this.miDeleteStrokes_Click);
            // 
            // miClear
            // 
            this.miClear.Index = 5;
            this.miClear.Text = "Clear";
            this.miClear.Click += new System.EventHandler(this.miClear_Click);
            // 
            // cmExtendedProperties
            // 
            this.cmExtendedProperties.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                 this.miAddProp,
                                                                                                 this.miClearProp});
            // 
            // miAddProp
            // 
            this.miAddProp.Index = 0;
            this.miAddProp.Text = "Add Property";
            this.miAddProp.Click += new System.EventHandler(this.miAddProp_Click);
            // 
            // miClearProp
            // 
            this.miClearProp.Index = 1;
            this.miClearProp.Text = "Clear";
            this.miClearProp.Click += new System.EventHandler(this.miClearProp_Click);
            // 
            // cmCustomStrokes
            // 
            this.cmCustomStrokes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                            this.miAddSelected,
                                                                                            this.miAddAll,
                                                                                            this.miClearCS});
            // 
            // miAddSelected
            // 
            this.miAddSelected.Index = 0;
            this.miAddSelected.Text = "Add Selected Strokes";
            this.miAddSelected.Click += new System.EventHandler(this.miAddSelected_Click);
            // 
            // miAddAll
            // 
            this.miAddAll.Index = 1;
            this.miAddAll.Text = "Add All Strokes";
            this.miAddAll.Click += new System.EventHandler(this.miAddAll_Click);
            // 
            // miClearCS
            // 
            this.miClearCS.Index = 2;
            this.miClearCS.Text = "Clear";
            this.miClearCS.Click += new System.EventHandler(this.miClearCS_Click);
            // 
            // cmDrawingAttributes
            // 
            this.cmDrawingAttributes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                this.miModifyDA});
            // 
            // miModifyDA
            // 
            this.miModifyDA.Index = 0;
            this.miModifyDA.Text = "Modify";
            this.miModifyDA.Click += new System.EventHandler(this.miModifyDA_Click);
            // 
            // cmStroke
            // 
            this.cmStroke.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                     this.miDrawStrokeBounds,
                                                                                     this.miDrawCusps,
                                                                                     this.miSelfIntersect,
                                                                                     this.miStrokeIntersect,
                                                                                     this.miDelete,
                                                                                     this.miRemove,
                                                                                     this.miSplit,
                                                                                     this.miStrokeRotate,
                                                                                     this.miFlatten,
                                                                                     this.miFuzz});
            // 
            // miDrawStrokeBounds
            // 
            this.miDrawStrokeBounds.Index = 0;
            this.miDrawStrokeBounds.Text = "Draw Bounds";
            this.miDrawStrokeBounds.Click += new System.EventHandler(this.miDrawStrokeBounds_Click);
            // 
            // miDrawCusps
            // 
            this.miDrawCusps.Index = 1;
            this.miDrawCusps.Text = "Draw Cusps";
            this.miDrawCusps.Click += new System.EventHandler(this.miDrawCusps_Click);
            // 
            // miSelfIntersect
            // 
            this.miSelfIntersect.Index = 2;
            this.miSelfIntersect.Text = "Draw Seflintersections";
            this.miSelfIntersect.Click += new System.EventHandler(this.miSelfIntersect_Click);
            // 
            // miStrokeIntersect
            // 
            this.miStrokeIntersect.Index = 3;
            this.miStrokeIntersect.Text = "Draw Strokeintersections";
            this.miStrokeIntersect.Click += new System.EventHandler(this.miStrokeIntersect_Click);
            // 
            // miDelete
            // 
            this.miDelete.Index = 4;
            this.miDelete.Text = "Delete";
            this.miDelete.Click += new System.EventHandler(this.miDelete_Click);
            // 
            // miRemove
            // 
            this.miRemove.Index = 5;
            this.miRemove.Text = "Remove";
            this.miRemove.Click += new System.EventHandler(this.miRemove_Click);
            // 
            // miSplit
            // 
            this.miSplit.Index = 6;
            this.miSplit.Text = "Split";
            this.miSplit.Click += new System.EventHandler(this.miSplit_Click);
            // 
            // miStrokeRotate
            // 
            this.miStrokeRotate.Index = 7;
            this.miStrokeRotate.Text = "Rotate";
            this.miStrokeRotate.Click += new System.EventHandler(this.miStrokeRotate_Click);
            // 
            // miFlatten
            // 
            this.miFlatten.Index = 8;
            this.miFlatten.Text = "Flatten";
            this.miFlatten.Click += new System.EventHandler(this.miFlatten_Click);
            // 
            // miFuzz
            // 
            this.miFuzz.Index = 9;
            this.miFuzz.Text = "Fuzz";
            this.miFuzz.Click += new System.EventHandler(this.miFuzz_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "ISF Files|*.isf|Gif Files|*.gif|Journal Files|*.jnt|All Files|*.*";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "ISF Files|*.isf|Gif Files|*.gif|All Files|*.*";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.inkPicture1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(216, 211);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(616, 353);
            this.panel1.TabIndex = 5;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(832, 564);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.treeView1);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Ink Explorer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() 
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(1033);
            Application.Run(new Form1());
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Initilize InkPicture.
            inkPicture1.DesiredPacketDescription =
                new Guid[]{PacketProperty.X,
                              PacketProperty.Y,
                              PacketProperty.NormalPressure,
                              PacketProperty.TimerTick};

            inkPicture1.Ink.InkAdded += new StrokesEventHandler(Ink_InkChanged);
            inkPicture1.Ink.InkDeleted += new StrokesEventHandler(Ink_InkChanged);

            // Create the Recognizer language menu.
            Recognizers recos = new Recognizers();
            if (recos.Count == 0)
            {
                // If no recognizers are installed, disable the
                // "Get RecognitionResult" menu item.
                miRecognize.Enabled = false;
                return;
            }
            foreach (Recognizer reco in recos)
            {
                // Add sub menu item for each language recognizer.
                if (reco.Languages.Length > 0)
                {
                    MenuItem miRecoLanguage = new MenuItem(reco.Name,
                        new EventHandler(miRecoLanguage_Click));
                    miRecognize.MenuItems.Add(miRecoLanguage);
                }
            }

            // Initialize TreeView.
            strokesNode = treeView1.Nodes[0].Nodes[0];
            extpropNode = treeView1.Nodes[0].Nodes[1];
            custStrokesNode = treeView1.Nodes[0].Nodes[2];
            treeView1.ExpandAll();
            treeView1.SelectedNode = strokesNode;
            UpdateTreeView();

            // Update InkPicture when the Panel resizes.
            panel1.Resize += new EventHandler(panel1_Resize);
        }
        
        private void treeView1_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
        {
            // Update the list view when the user selects a node in the tree view.
            UpdateListView(e.Node);
        }

        private void UpdateListView(TreeNode selectedNode)
        {
            // Update the list view according to the currently selected tree node.
            string strNewNode;
            int index = selectedNode.Text.IndexOf('_');
            if ( index > -1)
                strNewNode= selectedNode.Text.Substring(0, index);
            else
                strNewNode = selectedNode.Text;

            switch (strNewNode)
            {
                case "Ink":
                    treeView1.ContextMenu = null;
                    listView1.View = View.List;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();
                    break;

                case "Strokes":
                {
                    treeView1.ContextMenu = cmStrokes;
                    listView1.View = View.Details;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();

                    int itemWidth = (listView1.Width - SystemInformation.VerticalScrollBarWidth)/7;
                    listView1.Columns.Add("Id", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("Deleted", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("PacketCount", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("BezierPoints", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("PolylineCusps", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("BezierCusps", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("SelfIntersections", itemWidth, HorizontalAlignment.Center);

                    Strokes strokes = (Strokes)selectedNode.Tag;
                    if (strokes != null)
                    {
                        foreach (Stroke stroke in strokes)
                        {
                            if (!stroke.Deleted)
                            {
                                listView1.Items.Add(new ListViewItem(new string[]{stroke.Id.ToString(),
                                                                                     stroke.Deleted.ToString(),
                                                                                     stroke.PacketCount.ToString(),
                                                                                     stroke.BezierPoints.Length.ToString(),
                                                                                     stroke.PolylineCusps.Length.ToString(),
                                                                                     stroke.BezierCusps.Length.ToString(),
                                                                                     stroke.SelfIntersections.Length.ToString()}));
                            }
                            else
                            {
                                listView1.Items.Add(new ListViewItem(new string[]{stroke.Id.ToString(),
                                                                                     stroke.Deleted.ToString(),
                                                                                     "n/a", "n/a", "n/a", "n/a", "n/a"}));
                            }
                        }
                    }
                    break;
                }

                case "RecognitionResult":
                    treeView1.ContextMenu = null;
                    listView1.View = View.Details;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();
                    listView1.Columns.Add("RecognizedString", (listView1.Width - SystemInformation.VerticalScrollBarWidth)/2, HorizontalAlignment.Center);
                    listView1.Columns.Add("Confidence", (listView1.Width - SystemInformation.VerticalScrollBarWidth)/2, HorizontalAlignment.Center);

                    RecognitionResult res = (RecognitionResult)selectedNode.Tag;
                    if (res != null)
                    {
                        // Find out if confidence is supported.
                        bool isConfidenceSupported = false;
                        try
                        {
                            RecognitionConfidence confidence = res.TopAlternate.Confidence;
                            isConfidenceSupported = true;
                        }
                        catch{}

                        RecognitionAlternates alts = res.GetAlternatesFromSelection();
                        foreach (RecognitionAlternate alt in alts)
                        {
                            if (isConfidenceSupported)
                            {
                                listView1.Items.Add(new ListViewItem(new string[]{alt.ToString(),
                                                                                     alt.Confidence.ToString()}));
                            }
                            else
                            {
                                listView1.Items.Add(new ListViewItem(new string[]{alt.ToString(), "n/a"}));
                            }
                        }
                    }
                    break;

                case "Stroke":
                {
                    treeView1.ContextMenu = cmStroke;
                    listView1.View = View.Details;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();

                    int itemWidth = (listView1.Width - SystemInformation.VerticalScrollBarWidth)/4;
                    listView1.Columns.Add("X", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("Y", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("NormalPressure", itemWidth, HorizontalAlignment.Center);
                    listView1.Columns.Add("TimerTick", itemWidth, HorizontalAlignment.Center);

                    Stroke stroke = (Stroke)selectedNode.Tag;
                    if (!stroke.Deleted)
                    {
                        int []pdX = stroke.GetPacketValuesByProperty(PacketProperty.X);
                        int []pdY = stroke.GetPacketValuesByProperty(PacketProperty.Y);
                        int []pdP;
                        if (IsPropertyIncluded(stroke, PacketProperty.NormalPressure))
                            pdP = stroke.GetPacketValuesByProperty(PacketProperty.NormalPressure);
                        else
                            pdP = new int[pdX.Length];

                        int []pdT;
                        if (IsPropertyIncluded(stroke, PacketProperty.TimerTick))
                            pdT = stroke.GetPacketValuesByProperty(PacketProperty.TimerTick);
                        else
                            pdT = new int[pdX.Length];

                        listView1.Items.Clear();
                        for (int i=0; i<pdX.Length; i++)
                        {
                            listView1.Items.Add(new ListViewItem(new string[]{pdX[i].ToString(), pdY[i].ToString(), pdP[i].ToString(), pdT[i].ToString()}));
                        }
                    }
                    break;
                }

                case "ExtendedProperties":
                    treeView1.ContextMenu = cmExtendedProperties;
                    listView1.View = View.Details;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();
                    listView1.Columns.Add("Guid", (listView1.Width - SystemInformation.VerticalScrollBarWidth)/2, HorizontalAlignment.Center);
                    listView1.Columns.Add("Data", (listView1.Width - SystemInformation.VerticalScrollBarWidth)/2, HorizontalAlignment.Center);

                    ExtendedProperties eps = (ExtendedProperties)selectedNode.Tag;
                    foreach (ExtendedProperty ep in eps)
                    {
                        listView1.Items.Add(new ListViewItem(new string[]{ep.Id.ToString(), ep.Data.ToString()}));
                    }
                    break;

                case "CustomStrokes":
                    treeView1.ContextMenu = cmCustomStrokes;
                    listView1.View = View.List;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();
                    foreach (Strokes strokes in inkPicture1.Ink.CustomStrokes)
                    {
                        listView1.Items.Add("Strokes_" + strokes.ToString());
                    }
                    break;

                case "DrawingAttributes":
                    treeView1.ContextMenu = cmDrawingAttributes;
                    listView1.Columns.Clear();
                    listView1.Items.Clear();
                    DrawingAttributes da = (DrawingAttributes)selectedNode.Tag;
                    listView1.View = View.List;
                    listView1.Items.Add("Color = " + da.Color.Name);
                    listView1.Items.Add("AntiAliased = " + da.AntiAliased.ToString());
                    listView1.Items.Add("FitToCurve = " + da.FitToCurve.ToString());
                    listView1.Items.Add("Width = " + da.Width.ToString());
                    listView1.Items.Add("Height = " + da.Height.ToString());
                    listView1.Items.Add("IgnorePressure = " + da.IgnorePressure.ToString());
                    listView1.Items.Add("PenTip = " + da.PenTip.ToString());
                    listView1.Items.Add("RasterOperation = " + da.RasterOperation.ToString());
                    listView1.Items.Add("Transparency = " + da.Transparency.ToString());
                    break;
            }
        }

        private void UpdateTreeView()
        {
            // Update the tree view to reflect the current state of the ink.
            if (isUpdating) return;
            isUpdating = true;
            strokesNode.Tag = inkPicture1.Ink.Strokes;
            extpropNode.Tag = inkPicture1.Ink.ExtendedProperties;
            UpdateStrokes(inkPicture1.Ink.Strokes, strokesNode);
            custStrokesNode.Nodes.Clear();
            foreach (Strokes strokes in inkPicture1.Ink.CustomStrokes)
            {
                TreeNode node = new TreeNode("Strokes_" + strokes.ToString());
                node.Tag = strokes;
                custStrokesNode.Nodes.Add(node);
                UpdateStrokes(strokes, node);
            }
            isUpdating = false;
        }

        private void UpdateStrokes(Strokes strokes, TreeNode node)
        {
            // Update a Strokes node in the tree view.
            node.Nodes.Clear();
            TreeNode recoNode = new TreeNode("RecognitionResult");
            recoNode.Tag = strokes.RecognitionResult;
            node.Nodes.Add(recoNode);

            foreach (Stroke s in strokes)
            {
                TreeNode strokeNode = CreateNewStrokeNode(s);
                node.Nodes.Add(strokeNode);
            }
        }

        private TreeNode CreateNewStrokeNode(Stroke stroke)
        {
            TreeNode strokeNode= new TreeNode("Stroke_" + stroke.Id.ToString());
            strokeNode.Tag = stroke;
            if (!stroke.Deleted)
            {
                TreeNode daNode = new TreeNode("DrawingAttributes");
                daNode.Tag = stroke.DrawingAttributes;
                strokeNode.Nodes.Add(daNode);
                TreeNode extDaNode = new TreeNode("ExtendedProperties");
                extDaNode.Tag = stroke.DrawingAttributes.ExtendedProperties;
                daNode.Nodes.Add(extDaNode);
                TreeNode epNode = new TreeNode("ExtendedProperties");
                epNode.Tag = stroke.ExtendedProperties;
                strokeNode.Nodes.Add(epNode);
            }
            return strokeNode;
        }

        private void ResizeInkPicture()
        {
            // Make sure the InkPicture is big enough to display all ink
            // and at least as big as its parent scroll panel.
            Rectangle inkRect = inkPicture1.Ink.GetBoundingBox();
            Point pt = new Point(inkRect.Right, inkRect.Bottom);
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                inkPicture1.Renderer.InkSpaceToPixel(g, ref pt);
            }

            if (pt.X > inkPicture1.Width || panel1.Size.Width > inkPicture1.Width)
                inkPicture1.Width = Math.Max(pt.X, panel1.Size.Width);
            if (pt.Y > inkPicture1.Height || panel1.Size.Height > inkPicture1.Height)
                inkPicture1.Height = Math.Max(pt.Y, panel1.Size.Height);
        }

        private void panel1_Resize(object sender, System.EventArgs e)
        {
            ResizeInkPicture();
        }

        private void inkPicture1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (inkPicture1.InkEnabled == true) return;

            // Update the location of the hittest area.
            penPt.X = e.X;
            penPt.Y = e.Y;

            // Convert location and size to inkspace coordinates.
            Point pt1 = penPt;
            Point pt2 = new Point(e.X + HitSize/2, e.Y);
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                inkPicture1.Renderer.PixelToInkSpace(g, ref pt1);
                inkPicture1.Renderer.PixelToInkSpace(g, ref pt2);
            }

            // Do the hittest.
            Strokes hitStrokes = inkPicture1.Ink.HitTest(pt1, (float)(pt2.X - pt1.X));

            // Set the color of the hittest area depending on hittest result.
            if( hitStrokes.Count > 0 )
            {
                activePen = redPen;
                
                // Mark hit strokes as selected.
                if (inkPicture1.Selection.Count != hitStrokes.Count ||
                    inkPicture1.Selection[0].Id != hitStrokes[0].Id)
                    inkPicture1.Selection = hitStrokes;
            }
            else
            {
                activePen = blackPen; 
            }           
            
            // Erase previous hittest area.
            inkPicture1.Invalidate(invalidateRect);

            // Draw new hittest area.
            invalidateRect.X = e.X - HitSize/2;
            invalidateRect.Y = e.Y - HitSize/2;
            invalidateRect.Width = invalidateRect.Height = HitSize; 
            invalidateRect.Inflate((int)activePen.Width, (int)activePen.Width);            
            inkPicture1.Invalidate(invalidateRect);
        }

        private void inkPicture1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Draw guide-lines.
            e.Graphics.DrawLine(Pens.Blue, 50, 0, 50, inkPicture1.Height);
            for (int i=50; i<inkPicture1.Height; i+=50)
            {
                e.Graphics.DrawLine(Pens.Blue, 0, i, inkPicture1.Width, i);
            }

            // Draw hittest area.
            if (inkPicture1.InkEnabled == false)
            {           
                e.Graphics.DrawEllipse(activePen, penPt.X - HitSize/2, penPt.Y - HitSize/2, HitSize, HitSize);
            }
        }

        private void inkPicture1_Stroke(object sender, Microsoft.Ink.InkCollectorStrokeEventArgs e)
        {
            if (inkPicture1.EditingMode == InkOverlayEditingMode.Select) return;
            if (inkPicture1.EditingMode != InkOverlayEditingMode.Ink)
            {
                UpdateTreeView();
            }
            else
            {
                strokesNode.Nodes.Add(CreateNewStrokeNode(e.Stroke));
                ((Strokes)strokesNode.Tag).Add(e.Stroke);
                UpdateListView(strokesNode);
            }
        }

        private void Ink_InkChanged(object sender, StrokesEventArgs e)
        {
            if (inkPicture1.EditingMode == InkOverlayEditingMode.Select) return;
            if (treeView1.InvokeRequired)
            {
                // Marshal call to UI thread if not currently erasing.
                if (!inkPicture1.CollectingInk || inkPicture1.EditingMode != InkOverlayEditingMode.Delete)
                    Invoke(new StrokesEventHandler(Ink_InkChanged), new object[]{sender, e});
                return;
            }
            ResizeInkPicture();
            if (inkPicture1.CollectingInk && inkPicture1.EditingMode == InkOverlayEditingMode.Ink)
            {
                // Don't update TreeView when we collected a regular stroke.
				// This avoids flickering on the screen.
                return;
            }
            else
            {
                UpdateTreeView();
            }
            if (strokesNode.IsSelected) UpdateListView(strokesNode);
        }

        private bool IsPropertyIncluded(Stroke stroke, Guid pp)
        {
            foreach (Guid g in stroke.PacketDescription)
            {
                if (g == pp)
                {
                    return true;
                }
            }
            return false;
        }

        #region Menu event handlers
        private void miInkEM_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = true;
            inkPicture1.EditingMode = InkOverlayEditingMode.Ink;
            inkPicture1.Refresh();
        }

        private void miSelectEM_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = true;
            inkPicture1.EditingMode = InkOverlayEditingMode.Select;
            inkPicture1.Refresh();
        }

        private void miStrokeEraseEM_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = true;
            inkPicture1.EditingMode = InkOverlayEditingMode.Delete;
            inkPicture1.EraserMode = InkOverlayEraserMode.StrokeErase;
            inkPicture1.Refresh();
        }

        private void miPointEraseEM_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = true;
            inkPicture1.EditingMode = InkOverlayEditingMode.Delete;
            inkPicture1.EraserMode = InkOverlayEraserMode.PointErase;
            inkPicture1.Refresh();
        }

        private void miHitTest_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = false;
        }

        private void miRecoLanguage_Click(object sender, System.EventArgs e)
        {
            RecognitionStatus status;
            RecognitionResult res;
            RecognizerContext rc;

            // Find the matching Recognizer object.
            string recoName = ((MenuItem)sender).Text;
            Recognizers recos = new Recognizers();
            Recognizer selectedReco = null;
            foreach (Recognizer reco in recos)
            {
                if (reco.Name == recoName)
                {
                    selectedReco = reco;
                    break;
                }
            }

            // Create a RecognizerContext for the selected recognizer.
            if (selectedReco != null)
            {
                rc = selectedReco.CreateRecognizerContext();;
            }
            else
            {
                rc = new RecognizerContext();
            }

            rc.Strokes = (Strokes)treeView1.SelectedNode.Tag;
            foreach (Stroke stroke in rc.Strokes)
            {
                if (stroke.Deleted)
                {
                    MessageBox.Show("Can't recognize deleted Stroke object");
                    return;
                }
            }

            if (rc.Strokes.Count > 0)
            {
                res = rc.Recognize(out status);
                res.SetResultOnStrokes();
                treeView1.SelectedNode.Nodes[0].Tag = res;
                treeView1.SelectedNode = treeView1.SelectedNode.Nodes[0];
            }
        }

        private void miRotate_Click(object sender, System.EventArgs e)
        {
            Strokes strokes = (Strokes)treeView1.SelectedNode.Tag;
            Rectangle rect = strokes.GetBoundingBox();
            Point pt = new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
            strokes.Rotate(-45f, pt);
            inkPicture1.Refresh();
        }

        private void miStrokeRotate_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            Rectangle rect = stroke.GetBoundingBox();
            Point pt = new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
            stroke.Rotate(45f, pt);
            inkPicture1.Refresh();
            UpdateListView(treeView1.SelectedNode); 
        }

        private void miClearProp_Click(object sender, System.EventArgs e)
        {
            if (treeView1.SelectedNode.Parent.Text.StartsWith("DrawingAttributes"))
            {
                DrawingAttributes da = (DrawingAttributes)treeView1.SelectedNode.Parent.Tag;
                da.ExtendedProperties.Clear();
                Stroke stroke = (Stroke)treeView1.SelectedNode.Parent.Parent.Tag;
                stroke.DrawingAttributes = da;
                treeView1.SelectedNode.Parent.Tag = da;
            }
            else
            {
                ExtendedProperties ep = (ExtendedProperties)treeView1.SelectedNode.Tag;
                ep.Clear();
            }
            UpdateTreeView();
            UpdateListView(treeView1.SelectedNode);
        }

        private void miAddProp_Click(object sender, System.EventArgs e)
        {
            AddPropertyForm frm = new AddPropertyForm();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (treeView1.SelectedNode.Parent.Text.StartsWith("DrawingAttributes"))
                {
                    DrawingAttributes da = (DrawingAttributes)treeView1.SelectedNode.Parent.Tag;
                    da.ExtendedProperties.Add(frm.Guid, (object)frm.Value);
                    Stroke stroke = (Stroke)treeView1.SelectedNode.Parent.Parent.Tag;
                    stroke.DrawingAttributes = da;
                    treeView1.SelectedNode.Parent.Tag = da;
                }
                else
                {
                    ExtendedProperties ep = (ExtendedProperties)treeView1.SelectedNode.Tag;
                    ep.Add(frm.Guid, (object)frm.Value);
                    UpdateListView(treeView1.SelectedNode);
                }
                UpdateTreeView();
                UpdateListView(treeView1.SelectedNode);
            }
        }

        private void miModifyDA_Click(object sender, System.EventArgs e)
        {
            DrawingAttributes da = (DrawingAttributes)treeView1.SelectedNode.Tag;
            DrawingAttributesForm frm = new DrawingAttributesForm();
            frm.DrawingAttributes = da;
            frm.DrawingAttributesChanged += new EventHandler(frm_StrokeDrawingAttributesChanged);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                treeView1.SelectedNode.Tag = frm.DrawingAttributes;
                inkPicture1.Refresh();
                UpdateListView(treeView1.SelectedNode);
            }
        }

        private void frm_StrokeDrawingAttributesChanged(object sender, EventArgs e)
        {
            inkPicture1.Refresh();
        }

        private void miModifyStrokesDA_Click(object sender, System.EventArgs e)
        {
            DrawingAttributes da = new DrawingAttributes();
            DrawingAttributesForm frm = new DrawingAttributesForm();
            frm.DrawingAttributes = da;
            frm.DrawingAttributesChanged += new EventHandler(frm_StrokesDrawingAttributesChanged);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                Strokes strokes = (Strokes)treeView1.SelectedNode.Tag;
                strokes.ModifyDrawingAttributes(da);
                inkPicture1.Refresh();
                UpdateListView(treeView1.SelectedNode);
            }        
        }

        private void frm_StrokesDrawingAttributesChanged(object sender, EventArgs e)
        {
            Strokes strokes = (Strokes)treeView1.SelectedNode.Tag;
            strokes.ModifyDrawingAttributes(((DrawingAttributesForm)sender).DrawingAttributes);
            inkPicture1.Refresh();
            UpdateListView(treeView1.SelectedNode);
        }

        private void miClear_Click(object sender, System.EventArgs e)
        {
            Strokes strokes = (Strokes)treeView1.SelectedNode.Tag;
            strokes.Clear();
            UpdateTreeView();
            UpdateListView(treeView1.SelectedNode);
        }

        private void miDelete_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            inkPicture1.Ink.DeleteStroke(stroke);
            inkPicture1.Refresh();
        }

        private void miAddAll_Click(object sender, System.EventArgs e)
        {
            string str = "All_" + DateTime.Now.ToLongTimeString();
            inkPicture1.Ink.CustomStrokes.Add(str, inkPicture1.Ink.Strokes);
            UpdateTreeView();
            UpdateListView(custStrokesNode);
        }

        private void miAddSelected_Click(object sender, System.EventArgs e)
        {
            string str = "Selected_" + DateTime.Now.ToLongTimeString();
            inkPicture1.Ink.CustomStrokes.Add(str, inkPicture1.Selection);
            UpdateTreeView();
            UpdateListView(custStrokesNode);        
        }

        private void miClearCS_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Ink.CustomStrokes.Clear();
            UpdateTreeView();
            UpdateListView(custStrokesNode);
        }

        private void miSplit_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            float fi = (float)(stroke.PacketCount -1) / 2f;
            stroke.Split(fi);
            if (stroke.PacketCount > 3)
            {
                Stroke strokeToDelete = stroke.Split(stroke.PacketCount - 3);
                inkPicture1.Ink.DeleteStroke(strokeToDelete);
            }
            inkPicture1.Refresh();
        }

        private void miExit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void miClearInk_Click(object sender, System.EventArgs e)
        {
            inkPicture1.InkEnabled = false;
            inkPicture1.Ink = new Ink();
            inkPicture1.Ink.InkAdded += new StrokesEventHandler(Ink_InkChanged);
            inkPicture1.Ink.InkDeleted += new StrokesEventHandler(Ink_InkChanged);
            inkPicture1.Renderer = new Renderer();
            inkPicture1.InkEnabled = true;
            UpdateTreeView();
            treeView1.SelectedNode = strokesNode;
            UpdateListView(treeView1.SelectedNode);
        }

        private void miOpen_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = File.Open(openFileDialog1.FileName, FileMode.Open);
                if (openFileDialog1.FileName.EndsWith(".isf") ||
                    openFileDialog1.FileName.EndsWith(".gif"))
                {
                    Byte []inkBytes = new byte[fs.Length];
                    fs.Read(inkBytes, 0, (int)fs.Length);
                    inkPicture1.InkEnabled = false;
                    inkPicture1.Ink = new Ink();
                    inkPicture1.Ink.InkAdded += new StrokesEventHandler(Ink_InkChanged);
                    inkPicture1.Ink.InkDeleted += new StrokesEventHandler(Ink_InkChanged);
                    inkPicture1.Ink.Load(inkBytes);
                    inkPicture1.InkEnabled = true;
                }
                else if (openFileDialog1.FileName.EndsWith(".jnt"))
                {
                    Stream stream = JournalReader.ReadFromStream(fs); 
                    XmlDocument doc = new XmlDocument(); 
                    doc.Load(stream); 
                    inkPicture1.InkEnabled = false;
                    inkPicture1.Ink = new Ink();
                    inkPicture1.Ink.InkAdded += new StrokesEventHandler(Ink_InkChanged);
                    inkPicture1.Ink.InkDeleted += new StrokesEventHandler(Ink_InkChanged);
                    XmlNodeList pageNodes = doc.GetElementsByTagName("JournalPage");
                    int pageOffset = 0;
                    foreach (XmlElement pageNode in pageNodes)
                    {
                        Ink tempInk = new Ink();
                        XmlNodeList inkNodes = pageNode.GetElementsByTagName("InkObject");
                        foreach (XmlElement inkNode in inkNodes)
                        {
                            Byte []bytes = Convert.FromBase64String(inkNode.InnerText);
                            Ink ink = new Ink();
                            ink.Load(bytes);
                            ink.Strokes.Scale(2.54f, 2.54f);
                            Rectangle rect = ink.Strokes.GetBoundingBox();
                            rect.Offset(0, pageOffset);
                            tempInk.AddStrokesAtRectangle(ink.Strokes, rect);
                        }
                        inkPicture1.Ink.AddStrokesAtRectangle(tempInk.Strokes, tempInk.GetBoundingBox());
                        
						pageOffset += inkPicture1.Ink.GetBoundingBox().Height + pageMargin;
                    }
                    inkPicture1.InkEnabled = true;
                }
                fs.Close();
                UpdateTreeView();
            }
        }

        private void miSave_Click(object sender, System.EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = File.Open(saveFileDialog1.FileName, FileMode.OpenOrCreate);
                Byte []inkBytes;
                if (saveFileDialog1.FileName.EndsWith(".gif"))
                    inkBytes = inkPicture1.Ink.Save(PersistenceFormat.Gif);
                else
                    inkBytes = inkPicture1.Ink.Save(PersistenceFormat.InkSerializedFormat);
                fs.Write(inkBytes, 0, inkBytes.Length);
                fs.Close();
            }
        }

        private void miCut_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Ink.ClipboardCopy(InkClipboardFormats.Default, InkClipboardModes.Cut);
            inkPicture1.Refresh();
        }

        private void miCopy_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Ink.ClipboardCopy(InkClipboardFormats.Default, InkClipboardModes.Copy);        
        }

        private void mipaste_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Ink.ClipboardPaste();
            inkPicture1.Refresh();
        }

        private void miSelectAll_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Selection = inkPicture1.Ink.Strokes;
            inkPicture1.Refresh();
        }

        private void miZoomIn_Click(object sender, System.EventArgs e)
        {
            Matrix matrix = new Matrix();
            inkPicture1.Renderer.GetViewTransform(ref matrix);
            matrix.Scale(1.1f, 1.1f);
            inkPicture1.Renderer.SetViewTransform(matrix);
            ResizeInkPicture();
            inkPicture1.Refresh();
        }

        private void miZoomOut_Click(object sender, System.EventArgs e)
        {
            Matrix matrix = new Matrix();
            inkPicture1.Renderer.GetViewTransform(ref matrix);
            matrix.Scale(0.9f, 0.9f);
            inkPicture1.Renderer.SetViewTransform(matrix);
            ResizeInkPicture();
            inkPicture1.Refresh();
        }

        private void miFlatten_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            Point ptStart = stroke.GetPoint(0);
            Point ptEnd = stroke.GetPoint(stroke.PacketCount-1);
            double mx = (double)(ptEnd.X - ptStart.X)/(double)(stroke.PacketCount-1);
            double my = (double)(ptEnd.Y - ptStart.Y)/(double)(stroke.PacketCount-1);

            for (int i=1; i<stroke.PacketCount-1; i++)
            {
                Point pt = new Point(ptStart.X + (int)(mx*i), ptStart.Y + (int)(my*i));
                stroke.SetPoint(i, pt);
            }
            inkPicture1.Refresh();
            UpdateListView(treeView1.SelectedNode); 
        }

        private void miPrint_Click(object sender, System.EventArgs e)
        {
            printDialog1.Document = new PrintDocument();
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDialog1.Document.PrintPage += new PrintPageEventHandler(Document_PrintPage);
                printDialog1.Document.Print();
            }
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            inkPicture1.Renderer.Draw(e.Graphics, inkPicture1.Ink.Strokes);
        }

        private void miRemove_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            Strokes strokes = (Strokes)treeView1.SelectedNode.Parent.Tag;
            strokes.Remove(stroke);
            UpdateTreeView();        
        }

        private void miDivide_Click(object sender, System.EventArgs e)
        {
            inkPicture1.Ink.CustomStrokes.Clear();
            Divider divider = new Divider();
            divider.RecognizerContext = new RecognizerContext();
            divider.Strokes = inkPicture1.Ink.Strokes;
            DivisionUnits units = divider.Divide().ResultByType(InkDivisionType.Segment);
            foreach (DivisionUnit unit in units)
            {
                inkPicture1.Ink.CustomStrokes.Add(unit.RecognitionString, unit.Strokes);
            }
            UpdateTreeView();
            custStrokesNode.Expand();
            treeView1.SelectedNode = custStrokesNode;
        }

        private void miFuzz_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i=1; i<stroke.PacketCount-1; i++)
            {
                Point pt = stroke.GetPoint(i);
                pt = new Point(pt.X + rnd.Next(100), pt.Y + rnd.Next(100));
                stroke.SetPoint(i, pt);
            }
            inkPicture1.Refresh();
            UpdateListView(treeView1.SelectedNode);
        }

        private void miRotateInk_Click(object sender, System.EventArgs e)
        {
            Rectangle rect = inkPicture1.Ink.GetBoundingBox();
            Point pt = new Point(rect.X + rect.Width/2 , rect.Y + rect.Height/2);
            inkPicture1.Renderer.Rotate(45f, pt);
            inkPicture1.Refresh();
        }

        private void miDrawStrokeBounds_Click(object sender, System.EventArgs e)
        {
            Rectangle rect = ((Stroke)treeView1.SelectedNode.Tag).GetBoundingBox();
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                Point []pts = new Point[2];
                pts[0] = rect.Location;
                pts[1] = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                inkPicture1.Renderer.InkSpaceToPixel(g, ref pts);
                rect = Rectangle.FromLTRB(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y);
                g.DrawRectangle(greenPen, rect);
            }
        }

        private void miDrawStrokesBounds_Click(object sender, System.EventArgs e)
        {
            Rectangle rect = ((Strokes)treeView1.SelectedNode.Tag).GetBoundingBox();
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                Point []pts = new Point[2];
                pts[0] = rect.Location;
                pts[1] = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                inkPicture1.Renderer.InkSpaceToPixel(g, ref pts);
                rect = Rectangle.FromLTRB(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y);
                g.DrawRectangle(greenPen, rect);
            }        
        }

        private void miDrawCusps_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                int []indices;
                if (stroke.DrawingAttributes.FitToCurve == false)
                {
                    indices = stroke.PolylineCusps;
                }
                else
                {
                    indices = stroke.BezierCusps;
                }

                foreach (int index in indices)
                {
                    Point pt = stroke.GetPoint(index);
                    inkPicture1.Renderer.InkSpaceToPixel(g, ref pt);
                    g.FillEllipse(Brushes.Red, pt.X-5, pt.Y-5, 10, 10);
                }
            }
        }

        private void miSelfIntersect_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                foreach (float fIndex in stroke.SelfIntersections)
                {
                    Point pt1 = stroke.GetPoint((int)fIndex);
                    Point pt2 = stroke.GetPoint((int)fIndex + 1);
                    float fraction = fIndex - (int)fIndex;
                    Point pt = new Point(pt1.X + (int)(fraction * (pt2.X - pt1.X)),
                        pt1.Y + (int)(fraction * (pt2.Y - pt1.Y)));
                    inkPicture1.Renderer.InkSpaceToPixel(g, ref pt);
                    g.FillEllipse(Brushes.Red, pt.X-5, pt.Y-5, 10, 10);
                }
            }        
        }

        private void miStrokeIntersect_Click(object sender, System.EventArgs e)
        {
            Stroke stroke = (Stroke)treeView1.SelectedNode.Tag;
            Strokes strokes = (Strokes)treeView1.SelectedNode.Parent.Tag;
            using (Graphics g = inkPicture1.CreateGraphics())
            {
                float []intersections = stroke.FindIntersections(strokes);
                foreach (float fIndex in intersections)
                {
                    Point pt1 = stroke.GetPoint((int)fIndex);
                    Point pt2 = stroke.GetPoint((int)fIndex + 1);
                    float fraction = fIndex - (int)fIndex;
                    Point pt = new Point(pt1.X + (int)(fraction * (pt2.X - pt1.X)),
                        pt1.Y + (int)(fraction * (pt2.Y - pt1.Y)));
                    inkPicture1.Renderer.InkSpaceToPixel(g, ref pt);
                    g.FillEllipse(Brushes.Red, pt.X-5, pt.Y-5, 10, 10);
                }
            }                
        }

        private void miRefresh_Click(object sender, System.EventArgs e)
        {
            ResizeInkPicture();
            inkPicture1.Refresh();
            UpdateTreeView();
            UpdateListView(treeView1.SelectedNode);
        }

        private void miPenSettings_Click(object sender, System.EventArgs e)
        {
            DrawingAttributesForm frm = new DrawingAttributesForm();
            frm.DrawingAttributes = inkPicture1.DefaultDrawingAttributes;
            frm.ShowDialog();
        }

        private void miDeleteStrokes_Click(object sender, System.EventArgs e)
        {
            Strokes strokes = (Strokes)treeView1.SelectedNode.Tag;
            inkPicture1.Ink.DeleteStrokes(strokes);
            inkPicture1.Refresh();
        }
        #endregion
    }
}
