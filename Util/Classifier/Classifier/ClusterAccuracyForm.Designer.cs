namespace Classifier
{
    partial class ClusterAccuracyForm
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
            this.clusterInkPanel = new System.Windows.Forms.Panel();
            this.radioButtonErrors = new System.Windows.Forms.RadioButton();
            this.radioButtonConditionalPerfect = new System.Windows.Forms.RadioButton();
            this.radioButtonPerfect = new System.Windows.Forms.RadioButton();
            this.buttonPreviousCluster = new System.Windows.Forms.Button();
            this.buttonNextCluster = new System.Windows.Forms.Button();
            this.sketchInkPanel = new System.Windows.Forms.Panel();
            this.labelTotalNumErrors = new System.Windows.Forms.Label();
            this.labelSplitErrors = new System.Windows.Forms.Label();
            this.labelMergeShape2ShapeErrors = new System.Windows.Forms.Label();
            this.labelMergedWire2ShapeErrors = new System.Windows.Forms.Label();
            this.labelMergedText2ShapeErrors = new System.Windows.Forms.Label();
            this.labelTotalPerfectClusters = new System.Windows.Forms.Label();
            this.labelConditionalPerfectClusters = new System.Windows.Forms.Label();
            this.labelMergeErrors = new System.Windows.Forms.Label();
            this.labelMergeText2Text = new System.Windows.Forms.Label();
            this.labelMergeWire2Text = new System.Windows.Forms.Label();
            this.labelMergeShape2Text = new System.Windows.Forms.Label();
            this.labelInkExtraPartialMatches = new System.Windows.Forms.Label();
            this.labelInkExtraFromBestClusters = new System.Windows.Forms.Label();
            this.labelInkExtraTotalPercentage = new System.Windows.Forms.Label();
            this.labelInkMatchingPercentage = new System.Windows.Forms.Label();
            this.labelInkExtraCompletelyUnmatched = new System.Windows.Forms.Label();
            this.labelMergeNOTBUBBLE = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // clusterInkPanel
            // 
            this.clusterInkPanel.BackColor = System.Drawing.Color.White;
            this.clusterInkPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.clusterInkPanel.Location = new System.Drawing.Point(12, 35);
            this.clusterInkPanel.Name = "clusterInkPanel";
            this.clusterInkPanel.Size = new System.Drawing.Size(366, 366);
            this.clusterInkPanel.TabIndex = 0;
            // 
            // radioButtonErrors
            // 
            this.radioButtonErrors.AutoSize = true;
            this.radioButtonErrors.Checked = true;
            this.radioButtonErrors.Location = new System.Drawing.Point(12, 12);
            this.radioButtonErrors.Name = "radioButtonErrors";
            this.radioButtonErrors.Size = new System.Drawing.Size(101, 17);
            this.radioButtonErrors.TabIndex = 1;
            this.radioButtonErrors.TabStop = true;
            this.radioButtonErrors.Text = "Clustering Errors";
            this.radioButtonErrors.UseVisualStyleBackColor = true;
            // 
            // radioButtonConditionalPerfect
            // 
            this.radioButtonConditionalPerfect.AutoSize = true;
            this.radioButtonConditionalPerfect.Location = new System.Drawing.Point(119, 12);
            this.radioButtonConditionalPerfect.Name = "radioButtonConditionalPerfect";
            this.radioButtonConditionalPerfect.Size = new System.Drawing.Size(154, 17);
            this.radioButtonConditionalPerfect.TabIndex = 2;
            this.radioButtonConditionalPerfect.Text = "Conditional Perfect Clusters";
            this.radioButtonConditionalPerfect.UseVisualStyleBackColor = true;
            // 
            // radioButtonPerfect
            // 
            this.radioButtonPerfect.AutoSize = true;
            this.radioButtonPerfect.Location = new System.Drawing.Point(279, 12);
            this.radioButtonPerfect.Name = "radioButtonPerfect";
            this.radioButtonPerfect.Size = new System.Drawing.Size(99, 17);
            this.radioButtonPerfect.TabIndex = 3;
            this.radioButtonPerfect.Text = "Perfect Clusters";
            this.radioButtonPerfect.UseVisualStyleBackColor = true;
            // 
            // buttonPreviousCluster
            // 
            this.buttonPreviousCluster.Location = new System.Drawing.Point(92, 407);
            this.buttonPreviousCluster.Name = "buttonPreviousCluster";
            this.buttonPreviousCluster.Size = new System.Drawing.Size(83, 23);
            this.buttonPreviousCluster.TabIndex = 4;
            this.buttonPreviousCluster.Text = "<-- Previous";
            this.buttonPreviousCluster.UseVisualStyleBackColor = true;
            this.buttonPreviousCluster.Click += new System.EventHandler(this.buttonPreviousCluster_Click);
            // 
            // buttonNextCluster
            // 
            this.buttonNextCluster.Location = new System.Drawing.Point(181, 407);
            this.buttonNextCluster.Name = "buttonNextCluster";
            this.buttonNextCluster.Size = new System.Drawing.Size(83, 23);
            this.buttonNextCluster.TabIndex = 5;
            this.buttonNextCluster.Text = "Next -->";
            this.buttonNextCluster.UseVisualStyleBackColor = true;
            this.buttonNextCluster.Click += new System.EventHandler(this.buttonNextCluster_Click);
            // 
            // sketchInkPanel
            // 
            this.sketchInkPanel.BackColor = System.Drawing.Color.White;
            this.sketchInkPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.sketchInkPanel.Location = new System.Drawing.Point(403, 35);
            this.sketchInkPanel.Name = "sketchInkPanel";
            this.sketchInkPanel.Size = new System.Drawing.Size(604, 365);
            this.sketchInkPanel.TabIndex = 6;
            // 
            // labelTotalNumErrors
            // 
            this.labelTotalNumErrors.AutoSize = true;
            this.labelTotalNumErrors.Location = new System.Drawing.Point(12, 478);
            this.labelTotalNumErrors.Name = "labelTotalNumErrors";
            this.labelTotalNumErrors.Size = new System.Drawing.Size(89, 13);
            this.labelTotalNumErrors.TabIndex = 7;
            this.labelTotalNumErrors.Text = "Total # of Errors: ";
            // 
            // labelSplitErrors
            // 
            this.labelSplitErrors.AutoSize = true;
            this.labelSplitErrors.Location = new System.Drawing.Point(12, 501);
            this.labelSplitErrors.Name = "labelSplitErrors";
            this.labelSplitErrors.Size = new System.Drawing.Size(85, 13);
            this.labelSplitErrors.TabIndex = 8;
            this.labelSplitErrors.Text = "# of Split Errors: ";
            // 
            // labelMergeShape2ShapeErrors
            // 
            this.labelMergeShape2ShapeErrors.AutoSize = true;
            this.labelMergeShape2ShapeErrors.Location = new System.Drawing.Point(276, 433);
            this.labelMergeShape2ShapeErrors.Name = "labelMergeShape2ShapeErrors";
            this.labelMergeShape2ShapeErrors.Size = new System.Drawing.Size(181, 13);
            this.labelMergeShape2ShapeErrors.TabIndex = 9;
            this.labelMergeShape2ShapeErrors.Text = "# of Merged Shape -> Shape Errors: ";
            // 
            // labelMergedWire2ShapeErrors
            // 
            this.labelMergedWire2ShapeErrors.AutoSize = true;
            this.labelMergedWire2ShapeErrors.Location = new System.Drawing.Point(276, 455);
            this.labelMergedWire2ShapeErrors.Name = "labelMergedWire2ShapeErrors";
            this.labelMergedWire2ShapeErrors.Size = new System.Drawing.Size(172, 13);
            this.labelMergedWire2ShapeErrors.TabIndex = 10;
            this.labelMergedWire2ShapeErrors.Text = "# of Merged Wire -> Shape Errors: ";
            // 
            // labelMergedText2ShapeErrors
            // 
            this.labelMergedText2ShapeErrors.AutoSize = true;
            this.labelMergedText2ShapeErrors.Location = new System.Drawing.Point(276, 477);
            this.labelMergedText2ShapeErrors.Name = "labelMergedText2ShapeErrors";
            this.labelMergedText2ShapeErrors.Size = new System.Drawing.Size(171, 13);
            this.labelMergedText2ShapeErrors.TabIndex = 11;
            this.labelMergedText2ShapeErrors.Text = "# of Merged Text -> Shape Errors: ";
            // 
            // labelTotalPerfectClusters
            // 
            this.labelTotalPerfectClusters.AutoSize = true;
            this.labelTotalPerfectClusters.Location = new System.Drawing.Point(12, 433);
            this.labelTotalPerfectClusters.Name = "labelTotalPerfectClusters";
            this.labelTotalPerfectClusters.Size = new System.Drawing.Size(136, 13);
            this.labelTotalPerfectClusters.TabIndex = 12;
            this.labelTotalPerfectClusters.Text = "Total # of Perfect Clusters: ";
            // 
            // labelConditionalPerfectClusters
            // 
            this.labelConditionalPerfectClusters.AutoSize = true;
            this.labelConditionalPerfectClusters.Location = new System.Drawing.Point(12, 455);
            this.labelConditionalPerfectClusters.Name = "labelConditionalPerfectClusters";
            this.labelConditionalPerfectClusters.Size = new System.Drawing.Size(164, 13);
            this.labelConditionalPerfectClusters.TabIndex = 13;
            this.labelConditionalPerfectClusters.Text = "# of Conditional Perfect Clusters: ";
            // 
            // labelMergeErrors
            // 
            this.labelMergeErrors.AutoSize = true;
            this.labelMergeErrors.Location = new System.Drawing.Point(12, 522);
            this.labelMergeErrors.Name = "labelMergeErrors";
            this.labelMergeErrors.Size = new System.Drawing.Size(95, 13);
            this.labelMergeErrors.TabIndex = 14;
            this.labelMergeErrors.Text = "# of Merge Errors: ";
            // 
            // labelMergeText2Text
            // 
            this.labelMergeText2Text.AutoSize = true;
            this.labelMergeText2Text.Location = new System.Drawing.Point(276, 545);
            this.labelMergeText2Text.Name = "labelMergeText2Text";
            this.labelMergeText2Text.Size = new System.Drawing.Size(161, 13);
            this.labelMergeText2Text.TabIndex = 17;
            this.labelMergeText2Text.Text = "# of Merged Text -> Text Errors: ";
            // 
            // labelMergeWire2Text
            // 
            this.labelMergeWire2Text.AutoSize = true;
            this.labelMergeWire2Text.Location = new System.Drawing.Point(276, 523);
            this.labelMergeWire2Text.Name = "labelMergeWire2Text";
            this.labelMergeWire2Text.Size = new System.Drawing.Size(162, 13);
            this.labelMergeWire2Text.TabIndex = 16;
            this.labelMergeWire2Text.Text = "# of Merged Wire -> Text Errors: ";
            // 
            // labelMergeShape2Text
            // 
            this.labelMergeShape2Text.AutoSize = true;
            this.labelMergeShape2Text.Location = new System.Drawing.Point(276, 501);
            this.labelMergeShape2Text.Name = "labelMergeShape2Text";
            this.labelMergeShape2Text.Size = new System.Drawing.Size(171, 13);
            this.labelMergeShape2Text.TabIndex = 15;
            this.labelMergeShape2Text.Text = "# of Merged Shape -> Text Errors: ";
            // 
            // labelInkExtraPartialMatches
            // 
            this.labelInkExtraPartialMatches.AutoSize = true;
            this.labelInkExtraPartialMatches.Location = new System.Drawing.Point(505, 501);
            this.labelInkExtraPartialMatches.Name = "labelInkExtraPartialMatches";
            this.labelInkExtraPartialMatches.Size = new System.Drawing.Size(279, 13);
            this.labelInkExtraPartialMatches.TabIndex = 21;
            this.labelInkExtraPartialMatches.Text = "% Extra Ink from Partially Matching But not Best Clusters:  ";
            // 
            // labelInkExtraFromBestClusters
            // 
            this.labelInkExtraFromBestClusters.AutoSize = true;
            this.labelInkExtraFromBestClusters.Location = new System.Drawing.Point(505, 477);
            this.labelInkExtraFromBestClusters.Name = "labelInkExtraFromBestClusters";
            this.labelInkExtraFromBestClusters.Size = new System.Drawing.Size(156, 13);
            this.labelInkExtraFromBestClusters.TabIndex = 20;
            this.labelInkExtraFromBestClusters.Text = "% Extra Ink from Best Clusters:  ";
            // 
            // labelInkExtraTotalPercentage
            // 
            this.labelInkExtraTotalPercentage.AutoSize = true;
            this.labelInkExtraTotalPercentage.Location = new System.Drawing.Point(505, 455);
            this.labelInkExtraTotalPercentage.Name = "labelInkExtraTotalPercentage";
            this.labelInkExtraTotalPercentage.Size = new System.Drawing.Size(96, 13);
            this.labelInkExtraTotalPercentage.TabIndex = 19;
            this.labelInkExtraTotalPercentage.Text = "% Total Extra Ink:  ";
            // 
            // labelInkMatchingPercentage
            // 
            this.labelInkMatchingPercentage.AutoSize = true;
            this.labelInkMatchingPercentage.Location = new System.Drawing.Point(505, 433);
            this.labelInkMatchingPercentage.Name = "labelInkMatchingPercentage";
            this.labelInkMatchingPercentage.Size = new System.Drawing.Size(86, 13);
            this.labelInkMatchingPercentage.TabIndex = 18;
            this.labelInkMatchingPercentage.Text = "% Ink Matching: ";
            // 
            // labelInkExtraCompletelyUnmatched
            // 
            this.labelInkExtraCompletelyUnmatched.AutoSize = true;
            this.labelInkExtraCompletelyUnmatched.Location = new System.Drawing.Point(505, 523);
            this.labelInkExtraCompletelyUnmatched.Name = "labelInkExtraCompletelyUnmatched";
            this.labelInkExtraCompletelyUnmatched.Size = new System.Drawing.Size(243, 13);
            this.labelInkExtraCompletelyUnmatched.TabIndex = 22;
            this.labelInkExtraCompletelyUnmatched.Text = "% Extra Ink from Completely Unmatching Clusters: ";
            // 
            // labelMergeNOTBUBBLE
            // 
            this.labelMergeNOTBUBBLE.AutoSize = true;
            this.labelMergeNOTBUBBLE.Location = new System.Drawing.Point(276, 569);
            this.labelMergeNOTBUBBLE.Name = "labelMergeNOTBUBBLE";
            this.labelMergeNOTBUBBLE.Size = new System.Drawing.Size(215, 13);
            this.labelMergeNOTBUBBLE.TabIndex = 23;
            this.labelMergeNOTBUBBLE.Text = "# of Merged NOTBUBBLE -> Shape Errors: ";
            // 
            // ClusterAccuracyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 600);
            this.Controls.Add(this.labelMergeNOTBUBBLE);
            this.Controls.Add(this.labelInkExtraCompletelyUnmatched);
            this.Controls.Add(this.labelInkExtraPartialMatches);
            this.Controls.Add(this.labelInkExtraFromBestClusters);
            this.Controls.Add(this.labelInkExtraTotalPercentage);
            this.Controls.Add(this.labelInkMatchingPercentage);
            this.Controls.Add(this.labelMergeText2Text);
            this.Controls.Add(this.labelMergeWire2Text);
            this.Controls.Add(this.labelMergeShape2Text);
            this.Controls.Add(this.labelMergeErrors);
            this.Controls.Add(this.labelConditionalPerfectClusters);
            this.Controls.Add(this.labelTotalPerfectClusters);
            this.Controls.Add(this.labelMergedText2ShapeErrors);
            this.Controls.Add(this.labelMergedWire2ShapeErrors);
            this.Controls.Add(this.labelMergeShape2ShapeErrors);
            this.Controls.Add(this.labelSplitErrors);
            this.Controls.Add(this.labelTotalNumErrors);
            this.Controls.Add(this.sketchInkPanel);
            this.Controls.Add(this.buttonNextCluster);
            this.Controls.Add(this.buttonPreviousCluster);
            this.Controls.Add(this.radioButtonPerfect);
            this.Controls.Add(this.radioButtonConditionalPerfect);
            this.Controls.Add(this.radioButtonErrors);
            this.Controls.Add(this.clusterInkPanel);
            this.Name = "ClusterAccuracyForm";
            this.Text = "ClusterAccuracyForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel clusterInkPanel;
        private System.Windows.Forms.RadioButton radioButtonErrors;
        private System.Windows.Forms.RadioButton radioButtonConditionalPerfect;
        private System.Windows.Forms.RadioButton radioButtonPerfect;
        private System.Windows.Forms.Button buttonPreviousCluster;
        private System.Windows.Forms.Button buttonNextCluster;
        private System.Windows.Forms.Panel sketchInkPanel;
        private System.Windows.Forms.Label labelTotalNumErrors;
        private System.Windows.Forms.Label labelSplitErrors;
        private System.Windows.Forms.Label labelMergeShape2ShapeErrors;
        private System.Windows.Forms.Label labelMergedWire2ShapeErrors;
        private System.Windows.Forms.Label labelMergedText2ShapeErrors;
        private System.Windows.Forms.Label labelTotalPerfectClusters;
        private System.Windows.Forms.Label labelConditionalPerfectClusters;
        private System.Windows.Forms.Label labelMergeErrors;
        private System.Windows.Forms.Label labelMergeText2Text;
        private System.Windows.Forms.Label labelMergeWire2Text;
        private System.Windows.Forms.Label labelMergeShape2Text;
        private System.Windows.Forms.Label labelInkExtraPartialMatches;
        private System.Windows.Forms.Label labelInkExtraFromBestClusters;
        private System.Windows.Forms.Label labelInkExtraTotalPercentage;
        private System.Windows.Forms.Label labelInkMatchingPercentage;
        private System.Windows.Forms.Label labelInkExtraCompletelyUnmatched;
        private System.Windows.Forms.Label labelMergeNOTBUBBLE;
    }
}