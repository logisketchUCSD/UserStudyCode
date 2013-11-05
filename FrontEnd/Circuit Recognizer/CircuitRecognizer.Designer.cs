namespace Circuit_Recognizer
{
    partial class CircuitRecognizer
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
            this.openSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearInkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classifySketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runClassifiersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recognizeSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refineSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.takeSnapshotOfSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchProcessGroupingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recoAccuracyAfterGroupingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.trainWithWekaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchTrainWithWekaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findGroupingAccuracyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findClassifierAccuracyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trainSSWithWekaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchTrainSSWithWekaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.testGroupingClassifierWekaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyzeOrdersOfLabelsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupWithSimpleThresholdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupWithSimpleUserHoldoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateGroupingAccuracyFromTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeAllSSClassifcationsToFIleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.actionToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(892, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSketchToolStripMenuItem,
            this.clearInkToolStripMenuItem,
            this.playSketchToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitProgramToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openSketchToolStripMenuItem
            // 
            this.openSketchToolStripMenuItem.Name = "openSketchToolStripMenuItem";
            this.openSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openSketchToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openSketchToolStripMenuItem.Text = "Open Sketch";
            this.openSketchToolStripMenuItem.Click += new System.EventHandler(this.openSketchToolStripMenuItem_Click);
            // 
            // clearInkToolStripMenuItem
            // 
            this.clearInkToolStripMenuItem.Name = "clearInkToolStripMenuItem";
            this.clearInkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.clearInkToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.clearInkToolStripMenuItem.Text = "Clear Ink";
            this.clearInkToolStripMenuItem.Click += new System.EventHandler(this.clearInkToolStripMenuItem_Click);
            // 
            // playSketchToolStripMenuItem
            // 
            this.playSketchToolStripMenuItem.Name = "playSketchToolStripMenuItem";
            this.playSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.playSketchToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.playSketchToolStripMenuItem.Text = "Play Sketch";
            this.playSketchToolStripMenuItem.Click += new System.EventHandler(this.playSketchToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(174, 6);
            // 
            // exitProgramToolStripMenuItem
            // 
            this.exitProgramToolStripMenuItem.Name = "exitProgramToolStripMenuItem";
            this.exitProgramToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.exitProgramToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.exitProgramToolStripMenuItem.Text = "Exit Program";
            this.exitProgramToolStripMenuItem.Click += new System.EventHandler(this.exitProgramToolStripMenuItem_Click);
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.classifySketchToolStripMenuItem,
            this.groupSketchToolStripMenuItem,
            this.runClassifiersToolStripMenuItem,
            this.recognizeSketchToolStripMenuItem,
            this.refineSketchToolStripMenuItem,
            this.toolStripSeparator2,
            this.takeSnapshotOfSketchToolStripMenuItem,
            this.batchProcessGroupingToolStripMenuItem,
            this.recoAccuracyAfterGroupingToolStripMenuItem,
            this.loadResultsToolStripMenuItem,
            this.toolStripSeparator3,
            this.trainWithWekaToolStripMenuItem,
            this.batchTrainWithWekaToolStripMenuItem,
            this.findGroupingAccuracyToolStripMenuItem,
            this.findClassifierAccuracyToolStripMenuItem,
            this.trainSSWithWekaToolStripMenuItem,
            this.batchTrainSSWithWekaToolStripMenuItem,
            this.toolStripSeparator4,
            this.testGroupingClassifierWekaToolStripMenuItem,
            this.analyzeOrdersOfLabelsToolStripMenuItem,
            this.groupWithSimpleThresholdsToolStripMenuItem,
            this.groupWithSimpleUserHoldoutToolStripMenuItem,
            this.calculateGroupingAccuracyFromTextToolStripMenuItem,
            this.writeAllSSClassifcationsToFIleToolStripMenuItem,
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem,
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem});
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // classifySketchToolStripMenuItem
            // 
            this.classifySketchToolStripMenuItem.Name = "classifySketchToolStripMenuItem";
            this.classifySketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.classifySketchToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.classifySketchToolStripMenuItem.Text = "Classify Sketch";
            this.classifySketchToolStripMenuItem.Click += new System.EventHandler(this.classifySketchToolStripMenuItem_Click);
            // 
            // groupSketchToolStripMenuItem
            // 
            this.groupSketchToolStripMenuItem.Name = "groupSketchToolStripMenuItem";
            this.groupSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.groupSketchToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.groupSketchToolStripMenuItem.Text = "Group Sketch";
            this.groupSketchToolStripMenuItem.Click += new System.EventHandler(this.groupSketchToolStripMenuItem_Click);
            // 
            // runClassifiersToolStripMenuItem
            // 
            this.runClassifiersToolStripMenuItem.AutoToolTip = true;
            this.runClassifiersToolStripMenuItem.Name = "runClassifiersToolStripMenuItem";
            this.runClassifiersToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.runClassifiersToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.runClassifiersToolStripMenuItem.Text = "Run Classifiers";
            this.runClassifiersToolStripMenuItem.Click += new System.EventHandler(this.runClassifiersToolStripMenuItem_Click);
            // 
            // recognizeSketchToolStripMenuItem
            // 
            this.recognizeSketchToolStripMenuItem.Name = "recognizeSketchToolStripMenuItem";
            this.recognizeSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.recognizeSketchToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.recognizeSketchToolStripMenuItem.Text = "Recognize Components";
            this.recognizeSketchToolStripMenuItem.Click += new System.EventHandler(this.recognizeSketchToolStripMenuItem_Click);
            // 
            // refineSketchToolStripMenuItem
            // 
            this.refineSketchToolStripMenuItem.Name = "refineSketchToolStripMenuItem";
            this.refineSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.refineSketchToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.refineSketchToolStripMenuItem.Text = "Refine Sketch";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(279, 6);
            // 
            // takeSnapshotOfSketchToolStripMenuItem
            // 
            this.takeSnapshotOfSketchToolStripMenuItem.Name = "takeSnapshotOfSketchToolStripMenuItem";
            this.takeSnapshotOfSketchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.takeSnapshotOfSketchToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.takeSnapshotOfSketchToolStripMenuItem.Text = "Take Snapshot of Sketch";
            this.takeSnapshotOfSketchToolStripMenuItem.Click += new System.EventHandler(this.takeSnapshotOfSketchToolStripMenuItem_Click);
            // 
            // batchProcessGroupingToolStripMenuItem
            // 
            this.batchProcessGroupingToolStripMenuItem.Name = "batchProcessGroupingToolStripMenuItem";
            this.batchProcessGroupingToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.batchProcessGroupingToolStripMenuItem.Text = "Batch Process Grouping";
            this.batchProcessGroupingToolStripMenuItem.Click += new System.EventHandler(this.batchProcessGroupingToolStripMenuItem_Click);
            // 
            // recoAccuracyAfterGroupingToolStripMenuItem
            // 
            this.recoAccuracyAfterGroupingToolStripMenuItem.Name = "recoAccuracyAfterGroupingToolStripMenuItem";
            this.recoAccuracyAfterGroupingToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.recoAccuracyAfterGroupingToolStripMenuItem.Text = "Reco Accuracy after Grouping";
            this.recoAccuracyAfterGroupingToolStripMenuItem.Click += new System.EventHandler(this.recoAccuracyAfterGroupingToolStripMenuItem_Click);
            // 
            // loadResultsToolStripMenuItem
            // 
            this.loadResultsToolStripMenuItem.Name = "loadResultsToolStripMenuItem";
            this.loadResultsToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.loadResultsToolStripMenuItem.Text = "Load Results";
            this.loadResultsToolStripMenuItem.Click += new System.EventHandler(this.loadResultsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(279, 6);
            // 
            // trainWithWekaToolStripMenuItem
            // 
            this.trainWithWekaToolStripMenuItem.Name = "trainWithWekaToolStripMenuItem";
            this.trainWithWekaToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.trainWithWekaToolStripMenuItem.Text = "Train with Weka";
            this.trainWithWekaToolStripMenuItem.Click += new System.EventHandler(this.trainWithWekaToolStripMenuItem_Click);
            // 
            // batchTrainWithWekaToolStripMenuItem
            // 
            this.batchTrainWithWekaToolStripMenuItem.Name = "batchTrainWithWekaToolStripMenuItem";
            this.batchTrainWithWekaToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.batchTrainWithWekaToolStripMenuItem.Text = "Batch Train with Weka";
            this.batchTrainWithWekaToolStripMenuItem.Click += new System.EventHandler(this.batchTrainWithWekaToolStripMenuItem_Click);
            // 
            // findGroupingAccuracyToolStripMenuItem
            // 
            this.findGroupingAccuracyToolStripMenuItem.Name = "findGroupingAccuracyToolStripMenuItem";
            this.findGroupingAccuracyToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.findGroupingAccuracyToolStripMenuItem.Text = "Find Grouping Accuracy";
            this.findGroupingAccuracyToolStripMenuItem.Click += new System.EventHandler(this.findGroupingAccuracyToolStripMenuItem_Click);
            // 
            // findClassifierAccuracyToolStripMenuItem
            // 
            this.findClassifierAccuracyToolStripMenuItem.Name = "findClassifierAccuracyToolStripMenuItem";
            this.findClassifierAccuracyToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.findClassifierAccuracyToolStripMenuItem.Text = "Find Classifier Accuracy";
            this.findClassifierAccuracyToolStripMenuItem.Click += new System.EventHandler(this.findClassifierAccuracyToolStripMenuItem_Click);
            // 
            // trainSSWithWekaToolStripMenuItem
            // 
            this.trainSSWithWekaToolStripMenuItem.Name = "trainSSWithWekaToolStripMenuItem";
            this.trainSSWithWekaToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.trainSSWithWekaToolStripMenuItem.Text = "Train SS with Weka";
            this.trainSSWithWekaToolStripMenuItem.Click += new System.EventHandler(this.trainSSWithWekaToolStripMenuItem_Click);
            // 
            // batchTrainSSWithWekaToolStripMenuItem
            // 
            this.batchTrainSSWithWekaToolStripMenuItem.Name = "batchTrainSSWithWekaToolStripMenuItem";
            this.batchTrainSSWithWekaToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.batchTrainSSWithWekaToolStripMenuItem.Text = "Batch Train SS with Weka";
            this.batchTrainSSWithWekaToolStripMenuItem.Click += new System.EventHandler(this.batchTrainSSWithWekaToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(279, 6);
            // 
            // testGroupingClassifierWekaToolStripMenuItem
            // 
            this.testGroupingClassifierWekaToolStripMenuItem.Name = "testGroupingClassifierWekaToolStripMenuItem";
            this.testGroupingClassifierWekaToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.testGroupingClassifierWekaToolStripMenuItem.Text = "Test Grouping Classifier Weka";
            this.testGroupingClassifierWekaToolStripMenuItem.Click += new System.EventHandler(this.testGroupingClassifierWekaToolStripMenuItem_Click);
            // 
            // analyzeOrdersOfLabelsToolStripMenuItem
            // 
            this.analyzeOrdersOfLabelsToolStripMenuItem.Name = "analyzeOrdersOfLabelsToolStripMenuItem";
            this.analyzeOrdersOfLabelsToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.analyzeOrdersOfLabelsToolStripMenuItem.Text = "Analyze Orders of Labels";
            this.analyzeOrdersOfLabelsToolStripMenuItem.Click += new System.EventHandler(this.analyzeOrdersOfLabelsToolStripMenuItem_Click);
            // 
            // groupWithSimpleThresholdsToolStripMenuItem
            // 
            this.groupWithSimpleThresholdsToolStripMenuItem.Name = "groupWithSimpleThresholdsToolStripMenuItem";
            this.groupWithSimpleThresholdsToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.groupWithSimpleThresholdsToolStripMenuItem.Text = "Group with Simple Thresholds";
            this.groupWithSimpleThresholdsToolStripMenuItem.Click += new System.EventHandler(this.groupWithSimpleThresholdsToolStripMenuItem_Click);
            // 
            // groupWithSimpleUserHoldoutToolStripMenuItem
            // 
            this.groupWithSimpleUserHoldoutToolStripMenuItem.Name = "groupWithSimpleUserHoldoutToolStripMenuItem";
            this.groupWithSimpleUserHoldoutToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.groupWithSimpleUserHoldoutToolStripMenuItem.Text = "Group with Simple User-Holdout";
            this.groupWithSimpleUserHoldoutToolStripMenuItem.Click += new System.EventHandler(this.groupWithSimpleUserHoldoutToolStripMenuItem_Click);
            // 
            // calculateGroupingAccuracyFromTextToolStripMenuItem
            // 
            this.calculateGroupingAccuracyFromTextToolStripMenuItem.Name = "calculateGroupingAccuracyFromTextToolStripMenuItem";
            this.calculateGroupingAccuracyFromTextToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.calculateGroupingAccuracyFromTextToolStripMenuItem.Text = "Calculate Grouping Accuracy from Text";
            this.calculateGroupingAccuracyFromTextToolStripMenuItem.Click += new System.EventHandler(this.calculateGroupingAccuracyFromTextToolStripMenuItem_Click);
            // 
            // writeAllSSClassifcationsToFIleToolStripMenuItem
            // 
            this.writeAllSSClassifcationsToFIleToolStripMenuItem.Name = "writeAllSSClassifcationsToFIleToolStripMenuItem";
            this.writeAllSSClassifcationsToFIleToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.writeAllSSClassifcationsToFIleToolStripMenuItem.Text = "Write all SS Classifcations To FIle";
            this.writeAllSSClassifcationsToFIleToolStripMenuItem.Click += new System.EventHandler(this.writeAllSSClassifcationsToFIleToolStripMenuItem_Click);
            // 
            // calculateGroupingAccuracyFromTextUHToolStripMenuItem
            // 
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem.Name = "calculateGroupingAccuracyFromTextUHToolStripMenuItem";
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem.Text = "Calculate Grouping Accuracy from Text U-H";
            this.calculateGroupingAccuracyFromTextUHToolStripMenuItem.Click += new System.EventHandler(this.calculateGroupingAccuracyFromTextUHToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Status});
            this.statusStrip1.Location = new System.Drawing.Point(0, 594);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(892, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Status
            // 
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(0, 17);
            // 
            // checkSketchesForUnlabeledStrokesToolStripMenuItem
            // 
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem.Name = "checkSketchesForUnlabeledStrokesToolStripMenuItem";
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem.Text = "Check Sketches for Unlabeled Strokes";
            this.checkSketchesForUnlabeledStrokesToolStripMenuItem.Click += new System.EventHandler(this.checkSketchesForUnlabeledStrokesToolStripMenuItem_Click);
            // 
            // CircuitRecognizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 616);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "CircuitRecognizer";
            this.Text = "Circuit Recognizer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitProgramToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classifySketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recognizeSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refineSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem takeSnapshotOfSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel Status;
        private System.Windows.Forms.ToolStripMenuItem clearInkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchProcessGroupingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recoAccuracyAfterGroupingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trainWithWekaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchTrainWithWekaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem findGroupingAccuracyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findClassifierAccuracyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trainSSWithWekaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchTrainSSWithWekaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem testGroupingClassifierWekaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analyzeOrdersOfLabelsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playSketchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupWithSimpleThresholdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateGroupingAccuracyFromTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeAllSSClassifcationsToFIleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupWithSimpleUserHoldoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateGroupingAccuracyFromTextUHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runClassifiersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkSketchesForUnlabeledStrokesToolStripMenuItem;
    }
}

