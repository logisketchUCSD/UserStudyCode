namespace Labeler
{
	partial class SketchSummary
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
			this.components = new System.ComponentModel.Container();
			this.tIntersectionPanel = new System.Windows.Forms.Panel();
			this.tIntersectionsListBox = new System.Windows.Forms.ListBox();
			this.tIntersectionsCount = new System.Windows.Forms.Label();
			this.tIntersectionsLabel = new System.Windows.Forms.Label();
			this.lIntersectionsPanel = new System.Windows.Forms.Panel();
			this.lIntersectionsListBox = new System.Windows.Forms.ListBox();
			this.lIntersectionsCount = new System.Windows.Forms.Label();
			this.lIntersectionsLabel = new System.Windows.Forms.Label();
			this.xIntersectionsPanel = new System.Windows.Forms.Panel();
			this.xIntersectionsListBox = new System.Windows.Forms.ListBox();
			this.xIntersectionsCount = new System.Windows.Forms.Label();
			this.xIntersectionsLabel = new System.Windows.Forms.Label();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.statsLabel = new System.Windows.Forms.Label();
			this.statsListView = new System.Windows.Forms.ListView();
			this.columnKey = new System.Windows.Forms.ColumnHeader();
			this.columnValue = new System.Windows.Forms.ColumnHeader();
			this.tIntersectionPanel.SuspendLayout();
			this.lIntersectionsPanel.SuspendLayout();
			this.xIntersectionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// tIntersectionPanel
			// 
			this.tIntersectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tIntersectionPanel.Controls.Add(this.tIntersectionsListBox);
			this.tIntersectionPanel.Controls.Add(this.tIntersectionsCount);
			this.tIntersectionPanel.Controls.Add(this.tIntersectionsLabel);
			this.tIntersectionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.tIntersectionPanel.Location = new System.Drawing.Point(0, 0);
			this.tIntersectionPanel.Name = "tIntersectionPanel";
			this.tIntersectionPanel.Size = new System.Drawing.Size(513, 100);
			this.tIntersectionPanel.TabIndex = 0;
			// 
			// tIntersectionsListBox
			// 
			this.tIntersectionsListBox.FormattingEnabled = true;
			this.tIntersectionsListBox.Location = new System.Drawing.Point(11, 26);
			this.tIntersectionsListBox.Name = "tIntersectionsListBox";
			this.tIntersectionsListBox.Size = new System.Drawing.Size(489, 69);
			this.tIntersectionsListBox.TabIndex = 2;
			// 
			// tIntersectionsCount
			// 
			this.tIntersectionsCount.AutoSize = true;
			this.tIntersectionsCount.Location = new System.Drawing.Point(461, 2);
			this.tIntersectionsCount.Name = "tIntersectionsCount";
			this.tIntersectionsCount.Size = new System.Drawing.Size(47, 13);
			this.tIntersectionsCount.TabIndex = 1;
			this.tIntersectionsCount.Text = "Count: 0";
			// 
			// tIntersectionsLabel
			// 
			this.tIntersectionsLabel.AutoSize = true;
			this.tIntersectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tIntersectionsLabel.Location = new System.Drawing.Point(-1, -1);
			this.tIntersectionsLabel.Name = "tIntersectionsLabel";
			this.tIntersectionsLabel.Size = new System.Drawing.Size(110, 16);
			this.tIntersectionsLabel.TabIndex = 0;
			this.tIntersectionsLabel.Text = "T Intersections";
			// 
			// lIntersectionsPanel
			// 
			this.lIntersectionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lIntersectionsPanel.Controls.Add(this.lIntersectionsListBox);
			this.lIntersectionsPanel.Controls.Add(this.lIntersectionsCount);
			this.lIntersectionsPanel.Controls.Add(this.lIntersectionsLabel);
			this.lIntersectionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.lIntersectionsPanel.Location = new System.Drawing.Point(0, 100);
			this.lIntersectionsPanel.Name = "lIntersectionsPanel";
			this.lIntersectionsPanel.Size = new System.Drawing.Size(513, 100);
			this.lIntersectionsPanel.TabIndex = 1;
			// 
			// lIntersectionsListBox
			// 
			this.lIntersectionsListBox.FormattingEnabled = true;
			this.lIntersectionsListBox.Location = new System.Drawing.Point(11, 26);
			this.lIntersectionsListBox.Name = "lIntersectionsListBox";
			this.lIntersectionsListBox.Size = new System.Drawing.Size(489, 69);
			this.lIntersectionsListBox.TabIndex = 3;
			// 
			// lIntersectionsCount
			// 
			this.lIntersectionsCount.AutoSize = true;
			this.lIntersectionsCount.Dock = System.Windows.Forms.DockStyle.Right;
			this.lIntersectionsCount.Location = new System.Drawing.Point(462, 0);
			this.lIntersectionsCount.Name = "lIntersectionsCount";
			this.lIntersectionsCount.Size = new System.Drawing.Size(47, 13);
			this.lIntersectionsCount.TabIndex = 2;
			this.lIntersectionsCount.Text = "Count: 0";
			// 
			// lIntersectionsLabel
			// 
			this.lIntersectionsLabel.AutoSize = true;
			this.lIntersectionsLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.lIntersectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lIntersectionsLabel.Location = new System.Drawing.Point(0, 0);
			this.lIntersectionsLabel.Name = "lIntersectionsLabel";
			this.lIntersectionsLabel.Size = new System.Drawing.Size(108, 16);
			this.lIntersectionsLabel.TabIndex = 1;
			this.lIntersectionsLabel.Text = "L Intersections";
			// 
			// xIntersectionsPanel
			// 
			this.xIntersectionsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.xIntersectionsPanel.Controls.Add(this.xIntersectionsListBox);
			this.xIntersectionsPanel.Controls.Add(this.xIntersectionsCount);
			this.xIntersectionsPanel.Controls.Add(this.xIntersectionsLabel);
			this.xIntersectionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.xIntersectionsPanel.Location = new System.Drawing.Point(0, 200);
			this.xIntersectionsPanel.Name = "xIntersectionsPanel";
			this.xIntersectionsPanel.Size = new System.Drawing.Size(513, 100);
			this.xIntersectionsPanel.TabIndex = 2;
			// 
			// xIntersectionsListBox
			// 
			this.xIntersectionsListBox.FormattingEnabled = true;
			this.xIntersectionsListBox.Location = new System.Drawing.Point(11, 26);
			this.xIntersectionsListBox.Name = "xIntersectionsListBox";
			this.xIntersectionsListBox.Size = new System.Drawing.Size(489, 69);
			this.xIntersectionsListBox.TabIndex = 4;
			// 
			// xIntersectionsCount
			// 
			this.xIntersectionsCount.AutoSize = true;
			this.xIntersectionsCount.Dock = System.Windows.Forms.DockStyle.Right;
			this.xIntersectionsCount.Location = new System.Drawing.Point(462, 0);
			this.xIntersectionsCount.Name = "xIntersectionsCount";
			this.xIntersectionsCount.Size = new System.Drawing.Size(47, 13);
			this.xIntersectionsCount.TabIndex = 3;
			this.xIntersectionsCount.Text = "Count: 0";
			// 
			// xIntersectionsLabel
			// 
			this.xIntersectionsLabel.AutoSize = true;
			this.xIntersectionsLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.xIntersectionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.xIntersectionsLabel.Location = new System.Drawing.Point(0, 0);
			this.xIntersectionsLabel.Name = "xIntersectionsLabel";
			this.xIntersectionsLabel.Size = new System.Drawing.Size(109, 16);
			this.xIntersectionsLabel.TabIndex = 2;
			this.xIntersectionsLabel.Text = "X Intersections";
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// statsLabel
			// 
			this.statsLabel.AutoSize = true;
			this.statsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.statsLabel.Location = new System.Drawing.Point(12, 303);
			this.statsLabel.Name = "statsLabel";
			this.statsLabel.Size = new System.Drawing.Size(126, 16);
			this.statsLabel.TabIndex = 4;
			this.statsLabel.Text = "Sketch Statistics:";
			// 
			// statsListView
			// 
			this.statsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnKey,
            this.columnValue});
			this.statsListView.FullRowSelect = true;
			this.statsListView.GridLines = true;
			this.statsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.statsListView.Location = new System.Drawing.Point(12, 334);
			this.statsListView.Name = "statsListView";
			this.statsListView.Size = new System.Drawing.Size(489, 165);
			this.statsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.statsListView.TabIndex = 5;
			this.statsListView.UseCompatibleStateImageBehavior = false;
			this.statsListView.View = System.Windows.Forms.View.Details;
			// 
			// columnKey
			// 
			this.columnKey.Text = "Key";
			this.columnKey.Width = 180;
			// 
			// columnValue
			// 
			this.columnValue.Text = "Value";
			this.columnValue.Width = 300;
			// 
			// SketchSummary
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(513, 511);
			this.Controls.Add(this.statsListView);
			this.Controls.Add(this.statsLabel);
			this.Controls.Add(this.xIntersectionsPanel);
			this.Controls.Add(this.lIntersectionsPanel);
			this.Controls.Add(this.tIntersectionPanel);
			this.Name = "SketchSummary";
			this.Opacity = 0.96;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Sketch Summary";
			this.tIntersectionPanel.ResumeLayout(false);
			this.tIntersectionPanel.PerformLayout();
			this.lIntersectionsPanel.ResumeLayout(false);
			this.lIntersectionsPanel.PerformLayout();
			this.xIntersectionsPanel.ResumeLayout(false);
			this.xIntersectionsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel tIntersectionPanel;
		private System.Windows.Forms.Label tIntersectionsCount;
		private System.Windows.Forms.Label tIntersectionsLabel;
		private System.Windows.Forms.ListBox tIntersectionsListBox;
		private System.Windows.Forms.Panel lIntersectionsPanel;
		private System.Windows.Forms.ListBox lIntersectionsListBox;
		private System.Windows.Forms.Label lIntersectionsCount;
		private System.Windows.Forms.Label lIntersectionsLabel;
		private System.Windows.Forms.Panel xIntersectionsPanel;
		private System.Windows.Forms.ListBox xIntersectionsListBox;
		private System.Windows.Forms.Label xIntersectionsCount;
		private System.Windows.Forms.Label xIntersectionsLabel;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Label statsLabel;
		private System.Windows.Forms.ListView statsListView;
		private System.Windows.Forms.ColumnHeader columnKey;
		private System.Windows.Forms.ColumnHeader columnValue;


	}
}