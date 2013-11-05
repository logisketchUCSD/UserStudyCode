namespace TestImageAligner
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTemplatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadRecognizerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTestSymbolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.accuracyOnShapesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addRemoveStrokesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.runToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(683, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadTemplatesToolStripMenuItem,
            this.loadRecognizerToolStripMenuItem,
            this.loadTestSymbolsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadTemplatesToolStripMenuItem
            // 
            this.loadTemplatesToolStripMenuItem.Name = "loadTemplatesToolStripMenuItem";
            this.loadTemplatesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.loadTemplatesToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.loadTemplatesToolStripMenuItem.Text = "Load Templates";
            this.loadTemplatesToolStripMenuItem.Click += new System.EventHandler(this.loadTemplatesToolStripMenuItem_Click);
            // 
            // loadRecognizerToolStripMenuItem
            // 
            this.loadRecognizerToolStripMenuItem.Name = "loadRecognizerToolStripMenuItem";
            this.loadRecognizerToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.loadRecognizerToolStripMenuItem.Text = "Load Recognizer";
            this.loadRecognizerToolStripMenuItem.Click += new System.EventHandler(this.loadRecognizerToolStripMenuItem_Click);
            // 
            // loadTestSymbolsToolStripMenuItem
            // 
            this.loadTestSymbolsToolStripMenuItem.Name = "loadTestSymbolsToolStripMenuItem";
            this.loadTestSymbolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.loadTestSymbolsToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.loadTestSymbolsToolStripMenuItem.Text = "Load Test Symbols";
            this.loadTestSymbolsToolStripMenuItem.Click += new System.EventHandler(this.loadTestSymbolsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(198, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // runToolStripMenuItem
            // 
            this.runToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.accuracyOnShapesToolStripMenuItem,
            this.addRemoveStrokesToolStripMenuItem});
            this.runToolStripMenuItem.Name = "runToolStripMenuItem";
            this.runToolStripMenuItem.Size = new System.Drawing.Size(38, 20);
            this.runToolStripMenuItem.Text = "Run";
            // 
            // accuracyOnShapesToolStripMenuItem
            // 
            this.accuracyOnShapesToolStripMenuItem.Name = "accuracyOnShapesToolStripMenuItem";
            this.accuracyOnShapesToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.accuracyOnShapesToolStripMenuItem.Text = "Accuracy on Shapes";
            this.accuracyOnShapesToolStripMenuItem.Click += new System.EventHandler(this.accuracyOnShapesToolStripMenuItem_Click);
            // 
            // addRemoveStrokesToolStripMenuItem
            // 
            this.addRemoveStrokesToolStripMenuItem.Name = "addRemoveStrokesToolStripMenuItem";
            this.addRemoveStrokesToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.addRemoveStrokesToolStripMenuItem.Text = "Add/Remove Strokes";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 363);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Image Aligner Tester";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTemplatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadTestSymbolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem loadRecognizerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem accuracyOnShapesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addRemoveStrokesToolStripMenuItem;
    }
}

