namespace SketchPanelLib
{
    partial class SketchJournalMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SketchJournalMainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editingModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.backgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.blankToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.linedPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.graphPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.inkColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.recognizersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.partialGateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.sketchButton = new System.Windows.Forms.ToolStripButton();
			this.selectButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.zoomInButton = new System.Windows.Forms.ToolStripButton();
			this.zoomOutButton = new System.Windows.Forms.ToolStripButton();
			this.zoomToFit = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.recognizeButton = new System.Windows.Forms.ToolStripButton();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrokeInformation = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Font = new System.Drawing.Font("Tahoma", 12F);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editingModeToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.propertiesToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(542, 27);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSketchToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(45, 23);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// newSketchToolStripMenuItem
			// 
			this.newSketchToolStripMenuItem.Image = global::SketchPanelLib.Properties.Resources.NewDocumentHS;
			this.newSketchToolStripMenuItem.Name = "newSketchToolStripMenuItem";
			this.newSketchToolStripMenuItem.Size = new System.Drawing.Size(176, 24);
			this.newSketchToolStripMenuItem.Text = "New Sketch";
			this.newSketchToolStripMenuItem.Click += new System.EventHandler(this.newSketchToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = global::SketchPanelLib.Properties.Resources.openHS;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(176, 24);
			this.openToolStripMenuItem.Text = "Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Image = global::SketchPanelLib.Properties.Resources.saveHS;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(176, 24);
			this.saveToolStripMenuItem.Text = "Save...";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Image = global::SketchPanelLib.Properties.Resources.none;
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(176, 24);
			this.quitToolStripMenuItem.Text = "Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// editingModeToolStripMenuItem
			// 
			this.editingModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.deleteAllToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem});
			this.editingModeToolStripMenuItem.Name = "editingModeToolStripMenuItem";
			this.editingModeToolStripMenuItem.Size = new System.Drawing.Size(48, 23);
			this.editingModeToolStripMenuItem.Text = "Edit";
			// 
			// deleteAllToolStripMenuItem
			// 
			this.deleteAllToolStripMenuItem.Name = "deleteAllToolStripMenuItem";
			this.deleteAllToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
			this.deleteAllToolStripMenuItem.Text = "Delete All";
			this.deleteAllToolStripMenuItem.Click += new System.EventHandler(this.deleteAllToolStripMenuItem_Click);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Visible = false;
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
			this.cutToolStripMenuItem.Text = "Cut";
			this.cutToolStripMenuItem.Visible = false;
			this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
			this.pasteToolStripMenuItem.Text = "Paste";
			this.pasteToolStripMenuItem.Visible = false;
			this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Visible = false;
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Visible = false;
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Visible = false;
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundToolStripMenuItem,
            this.inkColorToolStripMenuItem,
            this.recognizersToolStripMenuItem});
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(92, 23);
			this.propertiesToolStripMenuItem.Text = "Properties";
			// 
			// backgroundToolStripMenuItem
			// 
			this.backgroundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blankToolStripMenuItem,
            this.linedPaperToolStripMenuItem,
            this.graphPaperToolStripMenuItem,
            this.imageToolStripMenuItem});
			this.backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
			this.backgroundToolStripMenuItem.Size = new System.Drawing.Size(178, 24);
			this.backgroundToolStripMenuItem.Text = "Background";
			// 
			// blankToolStripMenuItem
			// 
			this.blankToolStripMenuItem.Name = "blankToolStripMenuItem";
			this.blankToolStripMenuItem.Size = new System.Drawing.Size(182, 24);
			this.blankToolStripMenuItem.Text = "Blank";
			this.blankToolStripMenuItem.Click += new System.EventHandler(this.blankToolStripMenuItem_Click);
			// 
			// linedPaperToolStripMenuItem
			// 
			this.linedPaperToolStripMenuItem.Name = "linedPaperToolStripMenuItem";
			this.linedPaperToolStripMenuItem.Size = new System.Drawing.Size(182, 24);
			this.linedPaperToolStripMenuItem.Text = "Lined Paper";
			this.linedPaperToolStripMenuItem.Click += new System.EventHandler(this.linedPaperToolStripMenuItem_Click);
			// 
			// graphPaperToolStripMenuItem
			// 
			this.graphPaperToolStripMenuItem.Name = "graphPaperToolStripMenuItem";
			this.graphPaperToolStripMenuItem.Size = new System.Drawing.Size(182, 24);
			this.graphPaperToolStripMenuItem.Text = "Graph Paper";
			this.graphPaperToolStripMenuItem.Click += new System.EventHandler(this.graphPaperToolStripMenuItem_Click);
			// 
			// imageToolStripMenuItem
			// 
			this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
			this.imageToolStripMenuItem.Size = new System.Drawing.Size(182, 24);
			this.imageToolStripMenuItem.Text = "Image";
			this.imageToolStripMenuItem.Click += new System.EventHandler(this.imageToolStripMenuItem_Click);
			// 
			// inkColorToolStripMenuItem
			// 
			this.inkColorToolStripMenuItem.Name = "inkColorToolStripMenuItem";
			this.inkColorToolStripMenuItem.Size = new System.Drawing.Size(178, 24);
			this.inkColorToolStripMenuItem.Text = "Ink Color...";
			this.inkColorToolStripMenuItem.Click += new System.EventHandler(this.inkColorToolStripMenuItem_Click);
			// 
			// recognizersToolStripMenuItem
			// 
			this.recognizersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gateToolStripMenuItem,
            this.partialGateToolStripMenuItem});
			this.recognizersToolStripMenuItem.Name = "recognizersToolStripMenuItem";
			this.recognizersToolStripMenuItem.Size = new System.Drawing.Size(178, 24);
			this.recognizersToolStripMenuItem.Text = "Recognizers";
			// 
			// gateToolStripMenuItem
			// 
			this.gateToolStripMenuItem.Name = "gateToolStripMenuItem";
			this.gateToolStripMenuItem.Size = new System.Drawing.Size(175, 24);
			this.gateToolStripMenuItem.Text = "Gate";
			this.gateToolStripMenuItem.Click += new System.EventHandler(this.gateToolStripMenuItem_Click);
			// 
			// partialGateToolStripMenuItem
			// 
			this.partialGateToolStripMenuItem.Name = "partialGateToolStripMenuItem";
			this.partialGateToolStripMenuItem.Size = new System.Drawing.Size(175, 24);
			this.partialGateToolStripMenuItem.Text = "Partial Gate";
			this.partialGateToolStripMenuItem.Click += new System.EventHandler(this.partialGateToolStripMenuItem_Click);
			// 
			// toolStrip1
			// 
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sketchButton,
            this.selectButton,
            this.toolStripSeparator1,
            this.zoomInButton,
            this.zoomOutButton,
            this.zoomToFit,
            this.toolStripSeparator2,
            this.recognizeButton});
			this.toolStrip1.Location = new System.Drawing.Point(3, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(502, 26);
			this.toolStrip1.TabIndex = 2;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// sketchButton
			// 
			this.sketchButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.sketchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.sketchButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.sketchButton.Image = ((System.Drawing.Image)(resources.GetObject("sketchButton.Image")));
			this.sketchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.sketchButton.Name = "sketchButton";
			this.sketchButton.Size = new System.Drawing.Size(68, 23);
			this.sketchButton.Text = "Sketch";
			this.sketchButton.ToolTipText = "Allows you to sketch ink.";
			this.sketchButton.Click += new System.EventHandler(this.sketchButton_Click);
			// 
			// selectButton
			// 
			this.selectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.selectButton.Image = ((System.Drawing.Image)(resources.GetObject("selectButton.Image")));
			this.selectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.selectButton.Name = "selectButton";
			this.selectButton.Size = new System.Drawing.Size(63, 23);
			this.selectButton.Text = "Select";
			this.selectButton.ToolTipText = "Allows you to select, move, and resize Ink strokes.";
			this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
			// 
			// zoomInButton
			// 
			this.zoomInButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.zoomInButton.Image = ((System.Drawing.Image)(resources.GetObject("zoomInButton.Image")));
			this.zoomInButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.zoomInButton.Name = "zoomInButton";
			this.zoomInButton.Size = new System.Drawing.Size(76, 23);
			this.zoomInButton.Text = "Zoom +";
			this.zoomInButton.ToolTipText = "Zoom in.";
			this.zoomInButton.Click += new System.EventHandler(this.zoomInButton_Click);
			// 
			// zoomOutButton
			// 
			this.zoomOutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.zoomOutButton.Image = ((System.Drawing.Image)(resources.GetObject("zoomOutButton.Image")));
			this.zoomOutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.zoomOutButton.Name = "zoomOutButton";
			this.zoomOutButton.Size = new System.Drawing.Size(70, 23);
			this.zoomOutButton.Text = "Zoom -";
			this.zoomOutButton.ToolTipText = "Zoom out.";
			this.zoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomToFit
			// 
			this.zoomToFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.zoomToFit.Image = ((System.Drawing.Image)(resources.GetObject("zoomToFit.Image")));
			this.zoomToFit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.zoomToFit.Name = "zoomToFit";
			this.zoomToFit.Size = new System.Drawing.Size(106, 23);
			this.zoomToFit.Text = "Zoom to Fit";
			this.zoomToFit.Click += new System.EventHandler(this.zoomToFit_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 26);
			// 
			// recognizeButton
			// 
			this.recognizeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.recognizeButton.Image = ((System.Drawing.Image)(resources.GetObject("recognizeButton.Image")));
			this.recognizeButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.recognizeButton.Name = "recognizeButton";
			this.recognizeButton.Size = new System.Drawing.Size(97, 23);
			this.recognizeButton.Text = "Recognize";
			this.recognizeButton.ToolTipText = "Runs a recognition algorithm on the current sketch.";
			this.recognizeButton.Click += new System.EventHandler(this.recognizeButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 644);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusStrip1.Size = new System.Drawing.Size(542, 22);
			this.statusStrip1.TabIndex = 3;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Font = new System.Drawing.Font("Tahoma", 9F);
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(31, 17);
			this.toolStripStatusLabel1.Text = "Idle.";
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(542, 591);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 27);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(542, 617);
			this.toolStripContainer1.TabIndex = 4;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuStrokeInformation});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 23);
			this.viewToolStripMenuItem.Text = "View";
			// 
			// menuStrokeInformation
			// 
			this.menuStrokeInformation.Name = "menuStrokeInformation";
			this.menuStrokeInformation.Size = new System.Drawing.Size(243, 24);
			this.menuStrokeInformation.Text = "Stroke Information...";
			this.menuStrokeInformation.Click += new System.EventHandler(this.menuStrokeInformation_Click);
			// 
			// SketchJournalMainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(542, 666);
			this.Controls.Add(this.toolStripContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "SketchJournalMainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Sketch Journal";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editingModeToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton sketchButton;
        private System.Windows.Forms.ToolStripButton selectButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton zoomInButton;
        private System.Windows.Forms.ToolStripButton zoomOutButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton recognizeButton;
        private System.Windows.Forms.ToolStripMenuItem newSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blankToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linedPaperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem graphPaperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton zoomToFit;
        private System.Windows.Forms.ToolStripMenuItem inkColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recognizersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem partialGateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuStrokeInformation;
    }
}
