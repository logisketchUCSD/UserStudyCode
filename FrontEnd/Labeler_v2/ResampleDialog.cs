using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Labeler
{
	public partial class ResampleDialog : Form
	{
		public ResampleDialog(int numPoints)
		{
			InitializeComponent();
			numericPoints.Value = numPoints;
		}

		private void buttonResample_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		public int NumPoints
		{
			get
			{
				return (int)numericPoints.Value;
			}
		}
	}
}