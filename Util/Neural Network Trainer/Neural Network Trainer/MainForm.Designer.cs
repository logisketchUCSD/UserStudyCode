namespace Neural_Network_Trainer
{
    partial class MainForm
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
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonTest = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxActivation = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLearningMethod = new System.Windows.Forms.ComboBox();
            this.buttonDomainFile = new System.Windows.Forms.Button();
            this.textBoxDomainFile = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxMomentum = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxLearningRate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxEpochs = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxLayers = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.buttonSourceFiles = new System.Windows.Forms.Button();
            this.textBoxSourceFiles = new System.Windows.Forms.TextBox();
            this.labelSource = new System.Windows.Forms.Label();
            this.buttonBrowseOutput = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.labelOutput = new System.Windows.Forms.Label();
            this.radioButtonLOOCV = new System.Windows.Forms.RadioButton();
            this.radioButtonSingleNetwork = new System.Windows.Forms.RadioButton();
            this.buttonTrain = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonHelp);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonTest);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.button1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.groupBox1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.groupBoxOutput);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.buttonTrain);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(442, 319);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(442, 365);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(442, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(45, 17);
            this.toolStripStatusLabel1.Text = "Status: ";
            // 
            // buttonTest
            // 
            this.buttonTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTest.Location = new System.Drawing.Point(340, 37);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(74, 28);
            this.buttonTest.TabIndex = 4;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(340, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(74, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxActivation);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBoxLearningMethod);
            this.groupBox1.Controls.Add(this.buttonDomainFile);
            this.groupBox1.Controls.Add(this.textBoxDomainFile);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBoxMomentum);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxLearningRate);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxEpochs);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxLayers);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 127);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(322, 183);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Network Settings";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Activation Function: ";
            // 
            // comboBoxActivation
            // 
            this.comboBoxActivation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxActivation.FormattingEnabled = true;
            this.comboBoxActivation.Items.AddRange(new object[] {
            "Sigmoid",
            "Threshold",
            "Bipolar Sigmoid"});
            this.comboBoxActivation.Location = new System.Drawing.Point(116, 118);
            this.comboBoxActivation.Name = "comboBoxActivation";
            this.comboBoxActivation.Size = new System.Drawing.Size(121, 21);
            this.comboBoxActivation.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Learning Method: ";
            // 
            // comboBoxLearningMethod
            // 
            this.comboBoxLearningMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLearningMethod.FormattingEnabled = true;
            this.comboBoxLearningMethod.Items.AddRange(new object[] {
            "Back-Propagation",
            "Perceptron",
            "Delta Rule"});
            this.comboBoxLearningMethod.Location = new System.Drawing.Point(105, 90);
            this.comboBoxLearningMethod.Name = "comboBoxLearningMethod";
            this.comboBoxLearningMethod.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLearningMethod.TabIndex = 10;
            // 
            // buttonDomainFile
            // 
            this.buttonDomainFile.Location = new System.Drawing.Point(241, 145);
            this.buttonDomainFile.Name = "buttonDomainFile";
            this.buttonDomainFile.Size = new System.Drawing.Size(75, 23);
            this.buttonDomainFile.TabIndex = 8;
            this.buttonDomainFile.Text = "... (Browse)";
            this.buttonDomainFile.UseVisualStyleBackColor = true;
            this.buttonDomainFile.Click += new System.EventHandler(this.buttonDomainFile_Click);
            // 
            // textBoxDomainFile
            // 
            this.textBoxDomainFile.Location = new System.Drawing.Point(80, 147);
            this.textBoxDomainFile.Name = "textBoxDomainFile";
            this.textBoxDomainFile.Size = new System.Drawing.Size(155, 20);
            this.textBoxDomainFile.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 150);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(68, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Domain File: ";
            // 
            // textBoxMomentum
            // 
            this.textBoxMomentum.Location = new System.Drawing.Point(212, 64);
            this.textBoxMomentum.Name = "textBoxMomentum";
            this.textBoxMomentum.Size = new System.Drawing.Size(40, 20);
            this.textBoxMomentum.TabIndex = 7;
            this.textBoxMomentum.Text = "0.3";
            this.textBoxMomentum.Leave += new System.EventHandler(this.textBoxMomentum_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(141, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Momentum: ";
            // 
            // textBoxLearningRate
            // 
            this.textBoxLearningRate.Location = new System.Drawing.Point(92, 64);
            this.textBoxLearningRate.Name = "textBoxLearningRate";
            this.textBoxLearningRate.Size = new System.Drawing.Size(43, 20);
            this.textBoxLearningRate.TabIndex = 5;
            this.textBoxLearningRate.Text = "0.3";
            this.textBoxLearningRate.Leave += new System.EventHandler(this.textBoxLearningRate_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Learning Rate: ";
            // 
            // textBoxEpochs
            // 
            this.textBoxEpochs.Location = new System.Drawing.Point(102, 38);
            this.textBoxEpochs.Name = "textBoxEpochs";
            this.textBoxEpochs.Size = new System.Drawing.Size(62, 20);
            this.textBoxEpochs.TabIndex = 3;
            this.textBoxEpochs.Text = "1000";
            this.textBoxEpochs.Leave += new System.EventHandler(this.textBoxEpochs_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Training Epochs: ";
            // 
            // textBoxLayers
            // 
            this.textBoxLayers.Location = new System.Drawing.Point(190, 13);
            this.textBoxLayers.Name = "textBoxLayers";
            this.textBoxLayers.Size = new System.Drawing.Size(62, 20);
            this.textBoxLayers.TabIndex = 1;
            this.textBoxLayers.Text = "10,3";
            this.textBoxLayers.Leave += new System.EventHandler(this.textBoxLayers_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(178, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Layer Structure (Comma Separated):";
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Controls.Add(this.buttonSourceFiles);
            this.groupBoxOutput.Controls.Add(this.textBoxSourceFiles);
            this.groupBoxOutput.Controls.Add(this.labelSource);
            this.groupBoxOutput.Controls.Add(this.buttonBrowseOutput);
            this.groupBoxOutput.Controls.Add(this.textBoxOutput);
            this.groupBoxOutput.Controls.Add(this.labelOutput);
            this.groupBoxOutput.Controls.Add(this.radioButtonLOOCV);
            this.groupBoxOutput.Controls.Add(this.radioButtonSingleNetwork);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 3);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(322, 118);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Input/Output";
            // 
            // buttonSourceFiles
            // 
            this.buttonSourceFiles.Location = new System.Drawing.Point(241, 89);
            this.buttonSourceFiles.Name = "buttonSourceFiles";
            this.buttonSourceFiles.Size = new System.Drawing.Size(75, 23);
            this.buttonSourceFiles.TabIndex = 7;
            this.buttonSourceFiles.Text = "... (Browse)";
            this.buttonSourceFiles.UseVisualStyleBackColor = true;
            this.buttonSourceFiles.Click += new System.EventHandler(this.buttonBrowseSourceFiles_Click);
            // 
            // textBoxSourceFiles
            // 
            this.textBoxSourceFiles.Location = new System.Drawing.Point(101, 92);
            this.textBoxSourceFiles.Name = "textBoxSourceFiles";
            this.textBoxSourceFiles.Size = new System.Drawing.Size(134, 20);
            this.textBoxSourceFiles.TabIndex = 6;
            this.textBoxSourceFiles.TextChanged += new System.EventHandler(this.textBoxSourceFiles_TextChanged);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(6, 95);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(71, 13);
            this.labelSource.TabIndex = 5;
            this.labelSource.Text = "Source Files: ";
            // 
            // buttonBrowseOutput
            // 
            this.buttonBrowseOutput.Location = new System.Drawing.Point(241, 62);
            this.buttonBrowseOutput.Name = "buttonBrowseOutput";
            this.buttonBrowseOutput.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseOutput.TabIndex = 4;
            this.buttonBrowseOutput.Text = "... (Browse)";
            this.buttonBrowseOutput.UseVisualStyleBackColor = true;
            this.buttonBrowseOutput.Click += new System.EventHandler(this.buttonBrowseOutput_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(101, 65);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(134, 20);
            this.textBoxOutput.TabIndex = 3;
            this.textBoxOutput.Text = "C:\\";
            this.textBoxOutput.TextChanged += new System.EventHandler(this.textBoxOutput_TextChanged);
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(6, 68);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(90, 13);
            this.labelOutput.TabIndex = 2;
            this.labelOutput.Text = "Output Directory: ";
            // 
            // radioButtonLOOCV
            // 
            this.radioButtonLOOCV.AutoSize = true;
            this.radioButtonLOOCV.Checked = true;
            this.radioButtonLOOCV.Location = new System.Drawing.Point(6, 19);
            this.radioButtonLOOCV.Name = "radioButtonLOOCV";
            this.radioButtonLOOCV.Size = new System.Drawing.Size(314, 17);
            this.radioButtonLOOCV.TabIndex = 1;
            this.radioButtonLOOCV.TabStop = true;
            this.radioButtonLOOCV.Text = "LOOCV (Leave-One-Out Cross-Validation) (Multiple Networks)";
            this.radioButtonLOOCV.UseVisualStyleBackColor = true;
            this.radioButtonLOOCV.CheckedChanged += new System.EventHandler(this.radioButtonLOOCV_CheckedChanged);
            // 
            // radioButtonSingleNetwork
            // 
            this.radioButtonSingleNetwork.AutoSize = true;
            this.radioButtonSingleNetwork.Location = new System.Drawing.Point(6, 42);
            this.radioButtonSingleNetwork.Name = "radioButtonSingleNetwork";
            this.radioButtonSingleNetwork.Size = new System.Drawing.Size(97, 17);
            this.radioButtonSingleNetwork.TabIndex = 0;
            this.radioButtonSingleNetwork.Text = "Single Network";
            this.radioButtonSingleNetwork.UseVisualStyleBackColor = true;
            this.radioButtonSingleNetwork.CheckedChanged += new System.EventHandler(this.radioButtonSingleNetwork_CheckedChanged);
            // 
            // buttonTrain
            // 
            this.buttonTrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTrain.Location = new System.Drawing.Point(340, 3);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(74, 28);
            this.buttonTrain.TabIndex = 0;
            this.buttonTrain.Text = "Train";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(442, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // buttonHelp
            // 
            this.buttonHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonHelp.Location = new System.Drawing.Point(340, 95);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(75, 27);
            this.buttonHelp.TabIndex = 8;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 365);
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Neural Networks";
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.RadioButton radioButtonLOOCV;
        private System.Windows.Forms.RadioButton radioButtonSingleNetwork;
        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.Button buttonBrowseOutput;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.TextBox textBoxSourceFiles;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Button buttonSourceFiles;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxEpochs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxLayers;
        private System.Windows.Forms.TextBox textBoxMomentum;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxLearningRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonDomainFile;
        private System.Windows.Forms.TextBox textBoxDomainFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxActivation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLearningMethod;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Button buttonHelp;
    }
}

