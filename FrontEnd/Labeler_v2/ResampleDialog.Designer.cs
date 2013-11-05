namespace Labeler
{
	partial class ResampleDialog
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
			this.labelAmt = new System.Windows.Forms.Label();
			this.numericPoints = new System.Windows.Forms.NumericUpDown();
			this.buttonResample = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.numericPoints)).BeginInit();
			this.SuspendLayout();
			// 
			// labelAmt
			// 
			this.labelAmt.AutoSize = true;
			this.labelAmt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAmt.Location = new System.Drawing.Point(12, 30);
			this.labelAmt.Name = "labelAmt";
			this.labelAmt.Size = new System.Drawing.Size(113, 16);
			this.labelAmt.TabIndex = 0;
			this.labelAmt.Text = "Number of Points:";
			// 
			// numericPoints
			// 
			this.numericPoints.Location = new System.Drawing.Point(131, 26);
			this.numericPoints.Maximum = new decimal(new int[] {
            800,
            0,
            0,
            0});
			this.numericPoints.Name = "numericPoints";
			this.numericPoints.Size = new System.Drawing.Size(120, 20);
			this.numericPoints.TabIndex = 1;
			// 
			// buttonResample
			// 
			this.buttonResample.Location = new System.Drawing.Point(34, 64);
			this.buttonResample.Name = "buttonResample";
			this.buttonResample.Size = new System.Drawing.Size(75, 23);
			this.buttonResample.TabIndex = 2;
			this.buttonResample.Text = "Resample";
			this.buttonResample.UseVisualStyleBackColor = true;
			this.buttonResample.Click += new System.EventHandler(this.buttonResample_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(176, 64);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// ResampleDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(285, 99);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.buttonResample);
			this.Controls.Add(this.numericPoints);
			this.Controls.Add(this.labelAmt);
			this.Name = "ResampleDialog";
			this.Text = "Resample Stroke";
			((System.ComponentModel.ISupportInitialize)(this.numericPoints)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelAmt;
		private System.Windows.Forms.NumericUpDown numericPoints;
		private System.Windows.Forms.Button buttonResample;
		private System.Windows.Forms.Button btnCancel;
	}
}