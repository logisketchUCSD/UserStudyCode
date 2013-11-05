namespace NDollarTester
{
    partial class N_Dollar_Form
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
            this.inkPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonRecognize = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonClearInk = new System.Windows.Forms.Button();
            this.labelResult = new System.Windows.Forms.Label();
            this.inkPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // inkPanel
            // 
            this.inkPanel.BackColor = System.Drawing.Color.White;
            this.inkPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.inkPanel.Controls.Add(this.labelResult);
            this.inkPanel.Controls.Add(this.label2);
            this.inkPanel.Controls.Add(this.buttonRecognize);
            this.inkPanel.Controls.Add(this.textBox1);
            this.inkPanel.Controls.Add(this.label1);
            this.inkPanel.Controls.Add(this.buttonSave);
            this.inkPanel.Controls.Add(this.buttonClearInk);
            this.inkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inkPanel.Location = new System.Drawing.Point(0, 0);
            this.inkPanel.Name = "inkPanel";
            this.inkPanel.Size = new System.Drawing.Size(492, 367);
            this.inkPanel.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(107, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Result: ";
            // 
            // buttonRecognize
            // 
            this.buttonRecognize.Location = new System.Drawing.Point(3, 39);
            this.buttonRecognize.Name = "buttonRecognize";
            this.buttonRecognize.Size = new System.Drawing.Size(98, 23);
            this.buttonRecognize.TabIndex = 4;
            this.buttonRecognize.Text = "Recognize Ink";
            this.buttonRecognize.UseVisualStyleBackColor = true;
            this.buttonRecognize.Click += new System.EventHandler(this.buttonRecognize_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(201, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(107, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Template Name: ";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(3, 10);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(98, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save Template";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonClearInk
            // 
            this.buttonClearInk.Location = new System.Drawing.Point(382, 10);
            this.buttonClearInk.Name = "buttonClearInk";
            this.buttonClearInk.Size = new System.Drawing.Size(75, 23);
            this.buttonClearInk.TabIndex = 0;
            this.buttonClearInk.Text = "Clear Ink";
            this.buttonClearInk.UseVisualStyleBackColor = true;
            this.buttonClearInk.Click += new System.EventHandler(this.buttonClearInk_Click);
            // 
            // labelResult
            // 
            this.labelResult.AutoSize = true;
            this.labelResult.Location = new System.Drawing.Point(156, 44);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(0, 13);
            this.labelResult.TabIndex = 6;
            // 
            // N_Dollar_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 367);
            this.Controls.Add(this.inkPanel);
            this.Name = "N_Dollar_Form";
            this.Text = "$N Dollar Recognizer";
            this.inkPanel.ResumeLayout(false);
            this.inkPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel inkPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonRecognize;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonClearInk;
        private System.Windows.Forms.Label labelResult;
    }
}

