using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Ink;

namespace Labeler
{
	/// <summary>
	/// Summary description for StrokeForm.
	/// </summary>
	public class StrokeForm : System.Windows.Forms.Form
	{
		private Microsoft.Ink.InkOverlay oInk;


		private System.Windows.Forms.Panel panel1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public StrokeForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			oInk = new Microsoft.Ink.InkOverlay(panel1);
			oInk.EditingMode = Microsoft.Ink.InkOverlayEditingMode.Ink;
			oInk.Enabled = true;
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Location = new System.Drawing.Point(8, 40);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(656, 592);
			this.panel1.TabIndex = 0;
			// 
			// StrokeForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(680, 653);
			this.Controls.Add(this.panel1);
			this.Name = "StrokeForm";
			this.Text = "StrokeForm";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
