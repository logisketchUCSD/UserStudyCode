//-------------------------------------------------------------------------- 
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: AddPropertyForm.cs
// 
//-------------------------------------------------------------------------- 
using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace InkExplorer.NET
{
    /// <summary>
    /// Summary description for AddProperty.
    /// </summary>
    public class AddPropertyForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tbGuid;
        private System.Windows.Forms.Label lblGuid;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.TextBox tbValue;
        private Guid m_guid;

        public AddPropertyForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbGuid = new System.Windows.Forms.TextBox();
            this.lblGuid = new System.Windows.Forms.Label();
            this.lblValue = new System.Windows.Forms.Label();
            this.tbValue = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbGuid
            // 
            this.tbGuid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.tbGuid.Location = new System.Drawing.Point(80, 16);
            this.tbGuid.Name = "tbGuid";
            this.tbGuid.Size = new System.Drawing.Size(264, 22);
            this.tbGuid.TabIndex = 0;
            this.tbGuid.Text = "";
            // 
            // lblGuid
            // 
            this.lblGuid.Location = new System.Drawing.Point(16, 16);
            this.lblGuid.Name = "lblGuid";
            this.lblGuid.Size = new System.Drawing.Size(56, 24);
            this.lblGuid.TabIndex = 1;
            this.lblGuid.Text = "Guid";
            // 
            // lblValue
            // 
            this.lblValue.Location = new System.Drawing.Point(16, 56);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(56, 24);
            this.lblValue.TabIndex = 2;
            this.lblValue.Text = "Value";
            // 
            // tbValue
            // 
            this.tbValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.tbValue.Location = new System.Drawing.Point(80, 56);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(264, 22);
            this.tbValue.TabIndex = 3;
            this.tbValue.Text = "";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(168, 88);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 32);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(256, 88);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 32);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            // 
            // AddPropertyForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(352, 128);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.lblGuid);
            this.Controls.Add(this.tbGuid);
            this.Name = "AddPropertyForm";
            this.Text = "AddProperty";
            this.Load += new System.EventHandler(this.AddProperty_Load);
            this.ResumeLayout(false);

        }
        #endregion

        public Guid Guid
        {
            get {return m_guid;}
        }

        public string Value
        {
            get{return tbValue.Text;}
        }
            
        private void AddProperty_Load(object sender, System.EventArgs e)
        {
            m_guid = Guid.NewGuid();
            tbGuid.Text = m_guid.ToString();
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
