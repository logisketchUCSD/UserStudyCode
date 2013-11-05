using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace UserStudyUI
{
	/// <summary>
	/// Text input box form for changing the title of the main User Study Form UI.
	/// <see cref="UserStudyUIForm.setFormTitleMenuItem_Click()"/>
	/// </summary>
	public class FormTitleInputForm : System.Windows.Forms.Form
	{
		private string titleString;

		private System.Windows.Forms.Button  okButton;
		private System.Windows.Forms.TextBox textBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FormTitleInputForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			okButton.DialogResult = DialogResult.OK;

			textBox1.Text = "";

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
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(16, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(224, 20);
			this.textBox1.TabIndex = 2;
			this.textBox1.Text = "";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(88, 48);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// FormTitleInputForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(264, 94);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.textBox1);
			this.Name = "FormTitleInputForm";
			this.Text = "Change Form Title Text...";
			this.Load += new System.EventHandler(this.Form2_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Focus the text box on load
		/// </summary>
		private void Form2_Load(object sender, System.EventArgs e)
		{
			this.textBox1.SelectAll();
		}

		/// <summary>
		/// Closes the dialog and sets the return string for the new title.
		/// </summary>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.titleString = this.textBox1.Text;
			this.Close();
		}

		/// <summary>
		/// Getter and setter for the form title string.
		/// </summary>
		public string ReturnString
		{
			get
			{
				return titleString;
			}

			set
			{
				this.textBox1.Text = value;
			}
		}


	}
}
