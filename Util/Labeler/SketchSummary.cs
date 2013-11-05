using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Featurefy;

namespace Labeler
{
	public partial class SketchSummary : Form
	{
		#region Internals

		FeatureSketch _fs;

		#endregion

		public SketchSummary(FeatureSketch fs)
		{
			InitializeComponent();
			_fs = fs;
			PopulateBox();
		}

		#region Private

		private void PopulateBox()
		{
			Application.EnableVisualStyles();
			tIntersectionsListBox.Items.Clear();
			int tcount = 0;
			foreach (FeatureSketch.TStrokes t in _fs.TIntersections)
			{
				tIntersectionsListBox.Items.Add(String.Format("{0} - {1}", t.S1, t.S2));
				++tcount;
			}
			tIntersectionsCount.Text = "Count: " + tcount.ToString();

			lIntersectionsListBox.Items.Clear();
			int lcount = 0;
			foreach (FeatureSketch.LStrokes l in _fs.LIntersections)
			{
				lIntersectionsListBox.Items.Add(String.Format("{0} - {1}", l.S1, l.S2));
				++lcount;
			}
			lIntersectionsCount.Text = "Count: " + lcount.ToString();

			xIntersectionsListBox.Items.Clear();
			int xcount = 0;
			foreach (FeatureSketch.XStrokes x in _fs.XIntersections)
			{
				xIntersectionsListBox.Items.Add(String.Format("{0} - {1}", x.S1, x.S2));
				++xcount;
			}
			xIntersectionsCount.Text = "Count: " + xcount.ToString();

			addNewStat("Average Arc Length", _fs.AverageArcLength);
			addNewStat("Number of Strokes", _fs.NumStrokes);
			addNewStat("Number of Points", _fs.NumPoints);
			addNewStat("Average Points/Stroke", _fs.AverageNumPointsPerStroke);
			addNewStat("Sketch Fragmented", _fs.Sketch.isFragmented);
			addNewStat("Sketch Labeled", _fs.Sketch.isLabeled);
			addNewStat("Number of Shape Types", _fs.Sketch.LabelTypes.Count);
			addNewStat("Sketch Units", _fs.Sketch.XmlAttrs.Units);
			addNewStat("Average Average Speed", _fs.AverageAverageSpeed);
			addNewStat("Average Min Dist Between Substrokes", _fs.AvgMinDistBetweenSubstrokes);
			addNewStat("Average Max Dist Between Substrokes", _fs.AvgMaxDistBetweenSubstrokes);
			addNewStat("Average Avg Dist Between Substrokes", _fs.AvgAvgDistBetweenSubstrokes);
			addNewStat("Average Min Distance Between Endpoints", _fs.AverageMinDistanceBetweenEndpoints);
		}

		private void addNewStat(string title, object data)
		{
			ListViewItem item = new ListViewItem(title);
			item.SubItems.Add(data.ToString());
			statsListView.Items.Add(item);
		}

		#endregion
	}
}