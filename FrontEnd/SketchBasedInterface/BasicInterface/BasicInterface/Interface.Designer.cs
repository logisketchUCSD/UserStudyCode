namespace BasicInterface
{
    partial class Interface
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
            this.InitBasic = new System.Windows.Forms.Button();
            this.selectionButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // InitBasic
            // 
            this.InitBasic.Location = new System.Drawing.Point(17, 15);
            this.InitBasic.Name = "InitBasic";
            this.InitBasic.Size = new System.Drawing.Size(158, 26);
            this.InitBasic.TabIndex = 0;
            this.InitBasic.Text = "Initialize Basic Interface";
            this.InitBasic.UseVisualStyleBackColor = true;
            this.InitBasic.Click += new System.EventHandler(this.InitBasic_Click);
            // 
            // selectionButton
            // 
            this.selectionButton.Location = new System.Drawing.Point(16, 45);
            this.selectionButton.Name = "selectionButton";
            this.selectionButton.Size = new System.Drawing.Size(158, 26);
            this.selectionButton.TabIndex = 1;
            this.selectionButton.Text = "Selection Interface";
            this.selectionButton.UseVisualStyleBackColor = true;
            this.selectionButton.Click += new System.EventHandler(this.selectionButton_Click);
            // 
            // Interface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(191, 189);
            this.Controls.Add(this.selectionButton);
            this.Controls.Add(this.InitBasic);
            this.Name = "Interface";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Interface";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button InitBasic;
        private System.Windows.Forms.Button selectionButton;


    }
}

