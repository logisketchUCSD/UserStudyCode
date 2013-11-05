namespace Circuit_Recognizer
{
    partial class AlignerResultsForm
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
            this.groupBoxActual = new System.Windows.Forms.GroupBox();
            this.groupBoxActualText = new System.Windows.Forms.GroupBox();
            this.labelActualExtra = new System.Windows.Forms.Label();
            this.labelActualMissing = new System.Windows.Forms.Label();
            this.labelActualShapeName = new System.Windows.Forms.Label();
            this.panelBestShapeInk = new System.Windows.Forms.Panel();
            this.panelGroupedInk = new System.Windows.Forms.Panel();
            this.labelBestResultExtra = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelBestResultMissing = new System.Windows.Forms.Label();
            this.labelBestResultName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxResult1 = new System.Windows.Forms.GroupBox();
            this.groupBoxResult2 = new System.Windows.Forms.GroupBox();
            this.groupBoxResult3 = new System.Windows.Forms.GroupBox();
            this.panelResult1Ink = new System.Windows.Forms.Panel();
            this.panelResult2Ink = new System.Windows.Forms.Panel();
            this.panelResult3Ink = new System.Windows.Forms.Panel();
            this.pictureBoxResult1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxResult2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxResult3 = new System.Windows.Forms.PictureBox();
            this.labelResult1Extra = new System.Windows.Forms.Label();
            this.labelResult1Missing = new System.Windows.Forms.Label();
            this.labelResult1Name = new System.Windows.Forms.Label();
            this.labelResult2Extra = new System.Windows.Forms.Label();
            this.labelResult2Missing = new System.Windows.Forms.Label();
            this.labelResult2Name = new System.Windows.Forms.Label();
            this.labelResult3Extra = new System.Windows.Forms.Label();
            this.labelResult3Missing = new System.Windows.Forms.Label();
            this.labelResult3Name = new System.Windows.Forms.Label();
            this.groupBoxActual.SuspendLayout();
            this.groupBoxActualText.SuspendLayout();
            this.panelBestShapeInk.SuspendLayout();
            this.panelGroupedInk.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxResult1.SuspendLayout();
            this.groupBoxResult2.SuspendLayout();
            this.groupBoxResult3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult3)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxActual
            // 
            this.groupBoxActual.Controls.Add(this.groupBox1);
            this.groupBoxActual.Controls.Add(this.groupBoxActualText);
            this.groupBoxActual.Controls.Add(this.panelBestShapeInk);
            this.groupBoxActual.Controls.Add(this.panelGroupedInk);
            this.groupBoxActual.Location = new System.Drawing.Point(12, 12);
            this.groupBoxActual.Name = "groupBoxActual";
            this.groupBoxActual.Size = new System.Drawing.Size(523, 338);
            this.groupBoxActual.TabIndex = 0;
            this.groupBoxActual.TabStop = false;
            this.groupBoxActual.Text = "Actual";
            // 
            // groupBoxActualText
            // 
            this.groupBoxActualText.Controls.Add(this.labelActualExtra);
            this.groupBoxActualText.Controls.Add(this.labelActualMissing);
            this.groupBoxActualText.Controls.Add(this.labelActualShapeName);
            this.groupBoxActualText.Location = new System.Drawing.Point(262, 19);
            this.groupBoxActualText.Name = "groupBoxActualText";
            this.groupBoxActualText.Size = new System.Drawing.Size(255, 150);
            this.groupBoxActualText.TabIndex = 3;
            this.groupBoxActualText.TabStop = false;
            this.groupBoxActualText.Text = "Should Be";
            // 
            // labelActualExtra
            // 
            this.labelActualExtra.AutoSize = true;
            this.labelActualExtra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActualExtra.Location = new System.Drawing.Point(130, 48);
            this.labelActualExtra.Name = "labelActualExtra";
            this.labelActualExtra.Size = new System.Drawing.Size(90, 16);
            this.labelActualExtra.TabIndex = 4;
            this.labelActualExtra.Text = "Extra Strokes:";
            // 
            // labelActualMissing
            // 
            this.labelActualMissing.AutoSize = true;
            this.labelActualMissing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActualMissing.Location = new System.Drawing.Point(7, 48);
            this.labelActualMissing.Name = "labelActualMissing";
            this.labelActualMissing.Size = new System.Drawing.Size(106, 16);
            this.labelActualMissing.TabIndex = 3;
            this.labelActualMissing.Text = "Missing Strokes:";
            // 
            // labelActualShapeName
            // 
            this.labelActualShapeName.AutoSize = true;
            this.labelActualShapeName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelActualShapeName.Location = new System.Drawing.Point(6, 16);
            this.labelActualShapeName.Name = "labelActualShapeName";
            this.labelActualShapeName.Size = new System.Drawing.Size(65, 24);
            this.labelActualShapeName.TabIndex = 2;
            this.labelActualShapeName.Text = "Shape";
            // 
            // panelBestShapeInk
            // 
            this.panelBestShapeInk.BackColor = System.Drawing.Color.White;
            this.panelBestShapeInk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelBestShapeInk.Controls.Add(this.label2);
            this.panelBestShapeInk.Location = new System.Drawing.Point(6, 175);
            this.panelBestShapeInk.Name = "panelBestShapeInk";
            this.panelBestShapeInk.Size = new System.Drawing.Size(250, 150);
            this.panelBestShapeInk.TabIndex = 1;
            // 
            // panelGroupedInk
            // 
            this.panelGroupedInk.BackColor = System.Drawing.Color.White;
            this.panelGroupedInk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelGroupedInk.Controls.Add(this.label1);
            this.panelGroupedInk.Location = new System.Drawing.Point(6, 19);
            this.panelGroupedInk.Name = "panelGroupedInk";
            this.panelGroupedInk.Size = new System.Drawing.Size(250, 150);
            this.panelGroupedInk.TabIndex = 0;
            // 
            // labelBestResultExtra
            // 
            this.labelBestResultExtra.AutoSize = true;
            this.labelBestResultExtra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBestResultExtra.Location = new System.Drawing.Point(130, 48);
            this.labelBestResultExtra.Name = "labelBestResultExtra";
            this.labelBestResultExtra.Size = new System.Drawing.Size(90, 16);
            this.labelBestResultExtra.TabIndex = 4;
            this.labelBestResultExtra.Text = "Extra Strokes:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.labelBestResultExtra);
            this.groupBox1.Controls.Add(this.labelBestResultMissing);
            this.groupBox1.Controls.Add(this.labelBestResultName);
            this.groupBox1.Location = new System.Drawing.Point(262, 175);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(255, 150);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Best Result";
            // 
            // labelBestResultMissing
            // 
            this.labelBestResultMissing.AutoSize = true;
            this.labelBestResultMissing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBestResultMissing.Location = new System.Drawing.Point(7, 48);
            this.labelBestResultMissing.Name = "labelBestResultMissing";
            this.labelBestResultMissing.Size = new System.Drawing.Size(106, 16);
            this.labelBestResultMissing.TabIndex = 3;
            this.labelBestResultMissing.Text = "Missing Strokes:";
            // 
            // labelBestResultName
            // 
            this.labelBestResultName.AutoSize = true;
            this.labelBestResultName.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBestResultName.Location = new System.Drawing.Point(6, 16);
            this.labelBestResultName.Name = "labelBestResultName";
            this.labelBestResultName.Size = new System.Drawing.Size(65, 24);
            this.labelBestResultName.TabIndex = 2;
            this.labelBestResultName.Text = "Shape";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(133, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Actual Grouped Shape";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(157, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Expected Shape";
            // 
            // groupBoxResult1
            // 
            this.groupBoxResult1.Controls.Add(this.labelResult1Extra);
            this.groupBoxResult1.Controls.Add(this.pictureBoxResult1);
            this.groupBoxResult1.Controls.Add(this.labelResult1Missing);
            this.groupBoxResult1.Controls.Add(this.panelResult1Ink);
            this.groupBoxResult1.Controls.Add(this.labelResult1Name);
            this.groupBoxResult1.Location = new System.Drawing.Point(541, 57);
            this.groupBoxResult1.Name = "groupBoxResult1";
            this.groupBoxResult1.Size = new System.Drawing.Size(365, 266);
            this.groupBoxResult1.TabIndex = 1;
            this.groupBoxResult1.TabStop = false;
            this.groupBoxResult1.Text = "Result #1";
            // 
            // groupBoxResult2
            // 
            this.groupBoxResult2.Controls.Add(this.labelResult2Extra);
            this.groupBoxResult2.Controls.Add(this.pictureBoxResult2);
            this.groupBoxResult2.Controls.Add(this.labelResult2Missing);
            this.groupBoxResult2.Controls.Add(this.panelResult2Ink);
            this.groupBoxResult2.Controls.Add(this.labelResult2Name);
            this.groupBoxResult2.Location = new System.Drawing.Point(12, 356);
            this.groupBoxResult2.Name = "groupBoxResult2";
            this.groupBoxResult2.Size = new System.Drawing.Size(364, 266);
            this.groupBoxResult2.TabIndex = 2;
            this.groupBoxResult2.TabStop = false;
            this.groupBoxResult2.Text = "Result #2";
            // 
            // groupBoxResult3
            // 
            this.groupBoxResult3.Controls.Add(this.labelResult3Extra);
            this.groupBoxResult3.Controls.Add(this.pictureBoxResult3);
            this.groupBoxResult3.Controls.Add(this.labelResult3Missing);
            this.groupBoxResult3.Controls.Add(this.panelResult3Ink);
            this.groupBoxResult3.Controls.Add(this.labelResult3Name);
            this.groupBoxResult3.Location = new System.Drawing.Point(407, 356);
            this.groupBoxResult3.Name = "groupBoxResult3";
            this.groupBoxResult3.Size = new System.Drawing.Size(366, 266);
            this.groupBoxResult3.TabIndex = 2;
            this.groupBoxResult3.TabStop = false;
            this.groupBoxResult3.Text = "Result #3";
            // 
            // panelResult1Ink
            // 
            this.panelResult1Ink.BackColor = System.Drawing.Color.White;
            this.panelResult1Ink.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult1Ink.Location = new System.Drawing.Point(6, 19);
            this.panelResult1Ink.Name = "panelResult1Ink";
            this.panelResult1Ink.Size = new System.Drawing.Size(200, 144);
            this.panelResult1Ink.TabIndex = 0;
            // 
            // panelResult2Ink
            // 
            this.panelResult2Ink.BackColor = System.Drawing.Color.White;
            this.panelResult2Ink.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult2Ink.Location = new System.Drawing.Point(6, 19);
            this.panelResult2Ink.Name = "panelResult2Ink";
            this.panelResult2Ink.Size = new System.Drawing.Size(200, 144);
            this.panelResult2Ink.TabIndex = 1;
            // 
            // panelResult3Ink
            // 
            this.panelResult3Ink.BackColor = System.Drawing.Color.White;
            this.panelResult3Ink.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelResult3Ink.Location = new System.Drawing.Point(6, 19);
            this.panelResult3Ink.Name = "panelResult3Ink";
            this.panelResult3Ink.Size = new System.Drawing.Size(200, 144);
            this.panelResult3Ink.TabIndex = 1;
            // 
            // pictureBoxResult1
            // 
            this.pictureBoxResult1.Location = new System.Drawing.Point(212, 19);
            this.pictureBoxResult1.Name = "pictureBoxResult1";
            this.pictureBoxResult1.Size = new System.Drawing.Size(144, 144);
            this.pictureBoxResult1.TabIndex = 1;
            this.pictureBoxResult1.TabStop = false;
            // 
            // pictureBoxResult2
            // 
            this.pictureBoxResult2.Location = new System.Drawing.Point(212, 19);
            this.pictureBoxResult2.Name = "pictureBoxResult2";
            this.pictureBoxResult2.Size = new System.Drawing.Size(144, 144);
            this.pictureBoxResult2.TabIndex = 2;
            this.pictureBoxResult2.TabStop = false;
            // 
            // pictureBoxResult3
            // 
            this.pictureBoxResult3.Location = new System.Drawing.Point(212, 19);
            this.pictureBoxResult3.Name = "pictureBoxResult3";
            this.pictureBoxResult3.Size = new System.Drawing.Size(144, 144);
            this.pictureBoxResult3.TabIndex = 3;
            this.pictureBoxResult3.TabStop = false;
            // 
            // labelResult1Extra
            // 
            this.labelResult1Extra.AutoSize = true;
            this.labelResult1Extra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult1Extra.Location = new System.Drawing.Point(130, 198);
            this.labelResult1Extra.Name = "labelResult1Extra";
            this.labelResult1Extra.Size = new System.Drawing.Size(90, 16);
            this.labelResult1Extra.TabIndex = 7;
            this.labelResult1Extra.Text = "Extra Strokes:";
            // 
            // labelResult1Missing
            // 
            this.labelResult1Missing.AutoSize = true;
            this.labelResult1Missing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult1Missing.Location = new System.Drawing.Point(7, 198);
            this.labelResult1Missing.Name = "labelResult1Missing";
            this.labelResult1Missing.Size = new System.Drawing.Size(106, 16);
            this.labelResult1Missing.TabIndex = 6;
            this.labelResult1Missing.Text = "Missing Strokes:";
            // 
            // labelResult1Name
            // 
            this.labelResult1Name.AutoSize = true;
            this.labelResult1Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult1Name.Location = new System.Drawing.Point(6, 166);
            this.labelResult1Name.Name = "labelResult1Name";
            this.labelResult1Name.Size = new System.Drawing.Size(65, 24);
            this.labelResult1Name.TabIndex = 5;
            this.labelResult1Name.Text = "Shape";
            // 
            // labelResult2Extra
            // 
            this.labelResult2Extra.AutoSize = true;
            this.labelResult2Extra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult2Extra.Location = new System.Drawing.Point(130, 198);
            this.labelResult2Extra.Name = "labelResult2Extra";
            this.labelResult2Extra.Size = new System.Drawing.Size(90, 16);
            this.labelResult2Extra.TabIndex = 10;
            this.labelResult2Extra.Text = "Extra Strokes:";
            // 
            // labelResult2Missing
            // 
            this.labelResult2Missing.AutoSize = true;
            this.labelResult2Missing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult2Missing.Location = new System.Drawing.Point(7, 198);
            this.labelResult2Missing.Name = "labelResult2Missing";
            this.labelResult2Missing.Size = new System.Drawing.Size(106, 16);
            this.labelResult2Missing.TabIndex = 9;
            this.labelResult2Missing.Text = "Missing Strokes:";
            // 
            // labelResult2Name
            // 
            this.labelResult2Name.AutoSize = true;
            this.labelResult2Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult2Name.Location = new System.Drawing.Point(6, 166);
            this.labelResult2Name.Name = "labelResult2Name";
            this.labelResult2Name.Size = new System.Drawing.Size(65, 24);
            this.labelResult2Name.TabIndex = 8;
            this.labelResult2Name.Text = "Shape";
            // 
            // labelResult3Extra
            // 
            this.labelResult3Extra.AutoSize = true;
            this.labelResult3Extra.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult3Extra.Location = new System.Drawing.Point(130, 198);
            this.labelResult3Extra.Name = "labelResult3Extra";
            this.labelResult3Extra.Size = new System.Drawing.Size(90, 16);
            this.labelResult3Extra.TabIndex = 13;
            this.labelResult3Extra.Text = "Extra Strokes:";
            // 
            // labelResult3Missing
            // 
            this.labelResult3Missing.AutoSize = true;
            this.labelResult3Missing.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult3Missing.Location = new System.Drawing.Point(7, 198);
            this.labelResult3Missing.Name = "labelResult3Missing";
            this.labelResult3Missing.Size = new System.Drawing.Size(106, 16);
            this.labelResult3Missing.TabIndex = 12;
            this.labelResult3Missing.Text = "Missing Strokes:";
            // 
            // labelResult3Name
            // 
            this.labelResult3Name.AutoSize = true;
            this.labelResult3Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelResult3Name.Location = new System.Drawing.Point(6, 166);
            this.labelResult3Name.Name = "labelResult3Name";
            this.labelResult3Name.Size = new System.Drawing.Size(65, 24);
            this.labelResult3Name.TabIndex = 11;
            this.labelResult3Name.Text = "Shape";
            // 
            // AlignerResultsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1023, 634);
            this.Controls.Add(this.groupBoxResult3);
            this.Controls.Add(this.groupBoxResult2);
            this.Controls.Add(this.groupBoxResult1);
            this.Controls.Add(this.groupBoxActual);
            this.Name = "AlignerResultsForm";
            this.Text = "AlignerResultsForm";
            this.groupBoxActual.ResumeLayout(false);
            this.groupBoxActualText.ResumeLayout(false);
            this.groupBoxActualText.PerformLayout();
            this.panelBestShapeInk.ResumeLayout(false);
            this.panelBestShapeInk.PerformLayout();
            this.panelGroupedInk.ResumeLayout(false);
            this.panelGroupedInk.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxResult1.ResumeLayout(false);
            this.groupBoxResult1.PerformLayout();
            this.groupBoxResult2.ResumeLayout(false);
            this.groupBoxResult2.PerformLayout();
            this.groupBoxResult3.ResumeLayout(false);
            this.groupBoxResult3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxResult3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxActual;
        private System.Windows.Forms.Panel panelBestShapeInk;
        private System.Windows.Forms.Panel panelGroupedInk;
        private System.Windows.Forms.GroupBox groupBoxActualText;
        private System.Windows.Forms.Label labelActualMissing;
        private System.Windows.Forms.Label labelActualShapeName;
        private System.Windows.Forms.Label labelActualExtra;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelBestResultExtra;
        private System.Windows.Forms.Label labelBestResultMissing;
        private System.Windows.Forms.Label labelBestResultName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxResult1;
        private System.Windows.Forms.GroupBox groupBoxResult2;
        private System.Windows.Forms.GroupBox groupBoxResult3;
        private System.Windows.Forms.Panel panelResult1Ink;
        private System.Windows.Forms.Panel panelResult2Ink;
        private System.Windows.Forms.Panel panelResult3Ink;
        private System.Windows.Forms.Label labelResult1Extra;
        private System.Windows.Forms.PictureBox pictureBoxResult1;
        private System.Windows.Forms.Label labelResult1Missing;
        private System.Windows.Forms.Label labelResult1Name;
        private System.Windows.Forms.Label labelResult2Extra;
        private System.Windows.Forms.PictureBox pictureBoxResult2;
        private System.Windows.Forms.Label labelResult2Missing;
        private System.Windows.Forms.Label labelResult2Name;
        private System.Windows.Forms.Label labelResult3Extra;
        private System.Windows.Forms.PictureBox pictureBoxResult3;
        private System.Windows.Forms.Label labelResult3Missing;
        private System.Windows.Forms.Label labelResult3Name;
    }
}