namespace Basic
{
    partial class ButtonForm
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
            this.OverlayOn = new System.Windows.Forms.Button();
            this.GesturesOn = new System.Windows.Forms.Button();
            this.FeedbackBox = new System.Windows.Forms.TextBox();
            this.GetHelpButton = new System.Windows.Forms.Button();
            this.UndoButton = new System.Windows.Forms.Button();
            this.RedoButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OverlayOn
            // 
            this.OverlayOn.BackColor = System.Drawing.SystemColors.Control;
            this.OverlayOn.Location = new System.Drawing.Point(3, 3);
            this.OverlayOn.Name = "OverlayOn";
            this.OverlayOn.Size = new System.Drawing.Size(108, 23);
            this.OverlayOn.TabIndex = 9;
            this.OverlayOn.Text = "Turn Overlay Off";
            this.OverlayOn.UseVisualStyleBackColor = false;
            this.OverlayOn.Click += new System.EventHandler(this.OverlayOn_Click);
            // 
            // GesturesOn
            // 
            this.GesturesOn.Location = new System.Drawing.Point(117, 3);
            this.GesturesOn.Name = "GesturesOn";
            this.GesturesOn.Size = new System.Drawing.Size(108, 23);
            this.GesturesOn.TabIndex = 12;
            this.GesturesOn.Text = "Turn Gestures Off";
            this.GesturesOn.UseVisualStyleBackColor = true;
            this.GesturesOn.Click += new System.EventHandler(this.GesturesOn_Click);
            // 
            // FeedbackBox
            // 
            this.FeedbackBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.FeedbackBox.Location = new System.Drawing.Point(342, 5);
            this.FeedbackBox.Name = "FeedbackBox";
            this.FeedbackBox.Size = new System.Drawing.Size(247, 20);
            this.FeedbackBox.TabIndex = 14;
            // 
            // GetHelpButton
            // 
            this.GetHelpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GetHelpButton.Location = new System.Drawing.Point(602, 4);
            this.GetHelpButton.Name = "GetHelpButton";
            this.GetHelpButton.Size = new System.Drawing.Size(49, 22);
            this.GetHelpButton.TabIndex = 15;
            this.GetHelpButton.Text = "Help";
            this.GetHelpButton.UseVisualStyleBackColor = true;
            this.GetHelpButton.Click += new System.EventHandler(this.GetHelpButton_Click);
            // 
            // UndoButton
            // 
            this.UndoButton.Enabled = false;
            this.UndoButton.Location = new System.Drawing.Point(231, 3);
            this.UndoButton.Name = "UndoButton";
            this.UndoButton.Size = new System.Drawing.Size(48, 23);
            this.UndoButton.TabIndex = 16;
            this.UndoButton.Text = "Undo";
            this.UndoButton.UseVisualStyleBackColor = true;
            this.UndoButton.Click += new System.EventHandler(this.UndoButton_Click);
            // 
            // RedoButton
            // 
            this.RedoButton.Enabled = false;
            this.RedoButton.Location = new System.Drawing.Point(285, 3);
            this.RedoButton.Name = "RedoButton";
            this.RedoButton.Size = new System.Drawing.Size(51, 23);
            this.RedoButton.TabIndex = 17;
            this.RedoButton.Text = "Redo";
            this.RedoButton.UseVisualStyleBackColor = true;
            this.RedoButton.Click += new System.EventHandler(this.RedoButton_Click);
            // 
            // ButtonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 29);
            this.Controls.Add(this.RedoButton);
            this.Controls.Add(this.UndoButton);
            this.Controls.Add(this.GetHelpButton);
            this.Controls.Add(this.FeedbackBox);
            this.Controls.Add(this.GesturesOn);
            this.Controls.Add(this.OverlayOn);
            this.Location = new System.Drawing.Point(100, 100);
            this.Name = "ButtonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ButtonForm";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button OverlayOn;
        private System.Windows.Forms.Button GesturesOn;
        internal System.Windows.Forms.TextBox FeedbackBox;
        private System.Windows.Forms.Button GetHelpButton;
        internal System.Windows.Forms.Button UndoButton;
        internal System.Windows.Forms.Button RedoButton;

    }
}