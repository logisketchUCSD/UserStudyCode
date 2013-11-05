namespace PowerpointMinipad
{
    partial class MiniPadForm
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
            this.xInput = new System.Windows.Forms.TextBox();
            this.coordButton = new System.Windows.Forms.Button();
            this.yInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // xInput
            // 
            this.xInput.Location = new System.Drawing.Point(21, 245);
            this.xInput.Name = "xInput";
            this.xInput.Size = new System.Drawing.Size(49, 20);
            this.xInput.TabIndex = 0;
            // 
            // coordButton
            // 
            this.coordButton.Cursor = System.Windows.Forms.Cursors.NoMove2D;
            this.coordButton.Location = new System.Drawing.Point(166, 243);
            this.coordButton.Name = "coordButton";
            this.coordButton.Size = new System.Drawing.Size(75, 23);
            this.coordButton.TabIndex = 1;
            this.coordButton.Text = "Place it!";
            this.coordButton.UseVisualStyleBackColor = true;
            this.coordButton.Click += new System.EventHandler(this.coordButton_Click);
            // 
            // yInput
            // 
            this.yInput.Location = new System.Drawing.Point(95, 244);
            this.yInput.Name = "yInput";
            this.yInput.Size = new System.Drawing.Size(50, 20);
            this.yInput.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 248);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "x";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(76, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "y";
            // 
            // MiniPadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.yInput);
            this.Controls.Add(this.coordButton);
            this.Controls.Add(this.xInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MiniPadForm";
            this.Text = "MiniPad!";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox xInput;
        private System.Windows.Forms.Button coordButton;
        private System.Windows.Forms.TextBox yInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}