using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Ink;
using Featurefy;
using Sketch;

namespace StrokeInfoForm
{
    public partial class strokeInfoForm : Form
    {
		Featurefy.FeatureSketch _sketchFeatures;
		Featurefy.FeatureStroke _strokeFeatures;
        Microsoft.Ink.Stroke _inkStroke;
		Substroke _substroke;
        InkPicture pictureInk;
        float inkMovedX;
        float inkMovedY;
        float scale;
        Graphics g;
        Renderer r;

        const float CUSHION = 500.0f;

        public strokeInfoForm()
        {
            InitializeComponent();
        }

		public strokeInfoForm(Featurefy.FeatureStroke strokeFeatures, Featurefy.FeatureSketch sketchFeatures, Microsoft.Ink.Stroke inkStroke, Substroke ss)
        {
            InitializeComponent();
            this.inkMovedX = 0.0f;
            this.inkMovedY = 0.0f;
            this.scale = 2.0f;
            
            pictureInk = new InkPicture();
            pictureInk.Bounds = new Rectangle(0, 0, this.Width - dataView.Width, this.Height);
            Controls.Add(pictureInk);
            r = new Renderer();
            g = this.pictureInk.CreateGraphics();
            pictureInk.Painted += new InkOverlayPaintedEventHandler(pictureInk_Painted);

			_sketchFeatures = sketchFeatures;
			_strokeFeatures = strokeFeatures;
			_inkStroke = inkStroke;
			_substroke = ss;
            
            this.pictureInk.Ink.CreateStroke(_inkStroke.GetPoints());
            this.pictureInk.Ink.Strokes[0].DrawingAttributes.Color = Color.Red;
			populateListView();
            //drawPrimitives(this.features);
        }

        void pictureInk_Painted(object sender, PaintEventArgs e)
        {
            //drawPrimitives(this.features);
        }

		private void populateListView()
		{
			Application.EnableVisualStyles();
			ListViewGroup info = new ListViewGroup("Stroke Information");
			dataView.Groups.Add(info);
			addNewDatum("Stroke ID", _strokeFeatures.Id, info);
			addNewDatum("Stroke Label", _substroke.FirstLabel, info);
			addNewDatum("Number of Points", _strokeFeatures.Points.Length, info);
			ListViewGroup arcLength = new ListViewGroup("Arc Length Features");
			dataView.Groups.Add(arcLength);
			addNewDatum("Arc Length", _strokeFeatures.ArcLength.TotalLength, arcLength);
			addNewDatum("Ink Density (rectangular)", _strokeFeatures.ArcLength.InkDensity, arcLength);
			addNewDatum("Ink Density (circular)", _strokeFeatures.ArcLength.CircularInkDensity, arcLength);
			ListViewGroup spatial = new ListViewGroup("Spatial Features");
			dataView.Groups.Add(spatial);
			addNewDatum("Bounding Box Width", _strokeFeatures.Spatial.Width, spatial);
			addNewDatum("Bounding Box Height", _strokeFeatures.Spatial.Height, spatial);
			addNewDatum("Bounding Box Area", _strokeFeatures.Spatial.Area, spatial);
			ListViewGroup curvature = new ListViewGroup("Curvature Features");
			dataView.Groups.Add(curvature);
			addNewDatum("Total Angle", _strokeFeatures.Curvature.TotalAngle, curvature);
			addNewDatum("Average Curvature", _strokeFeatures.Curvature.AverageCurvature, curvature);
			addNewDatum("Total Abs Angle", _strokeFeatures.Curvature.TotalAbsAngle, curvature);
			addNewDatum("Total Squared Angle", _strokeFeatures.Curvature.TotalSquaredAngle, curvature);
			addNewDatum("Sum Sqrt Curvature", _strokeFeatures.Curvature.SumSqrtCurvature, curvature);
			addNewDatum("Minimum Curvature", _strokeFeatures.Curvature.MinimumCurvature, curvature);
			addNewDatum("Maximum Curvature", _strokeFeatures.Curvature.MaximumCurvature, curvature);
			ListViewGroup speed = new ListViewGroup("Speed Features");
			dataView.Groups.Add(speed);
			addNewDatum("Average Speed", _strokeFeatures.Speed.AverageSpeed, speed);
			addNewDatum("Maximum Speed", _strokeFeatures.Speed.MaximumSpeed, speed);
			addNewDatum("Minimum Speed", _strokeFeatures.Speed.MinimumSpeed, speed);
			ListViewGroup slope = new ListViewGroup("Slope Features");
			dataView.Groups.Add(slope);
			addNewDatum("Average Slope", _strokeFeatures.Slope.AverageSlope, slope);
			addNewDatum("Slope Direction", _strokeFeatures.Slope.Direction, slope);
			ListViewGroup intersections = new ListViewGroup("Intersection Features");
			dataView.Groups.Add(intersections);
			addNewDatum("Self Intersections", _strokeFeatures.Spatial.NumSelfIntersections, intersections);
			addNewDatum("L Intersections", _sketchFeatures.NumLIntersections(_substroke), intersections);
			addNewDatum("T Intersections", _sketchFeatures.NumTIntersections(_substroke), intersections);
			addNewDatum("X Intersections", _sketchFeatures.NumXIntersections(_substroke), intersections);
			addNewDatum("Single-Stroke Closed Shape", _strokeFeatures.ClosedShape, intersections);
			addNewDatum("Multi-Stroke Closed Shape", _sketchFeatures.InClosedShape(_substroke), intersections);			
		}

		private void addNewDatum(string key, object value)
		{
			ListViewItem item = new ListViewItem(key);
			item.SubItems.Add(value.ToString());
			dataView.Items.Add(item);
		}

		private void addNewDatum(string key, object value, ListViewGroup group)
		{
			ListViewItem item = new ListViewItem(key, group);
			item.SubItems.Add(value.ToString());
			dataView.Items.Add(item);
		}

        public void addStroke(System.Drawing.Point[] points, Color color)
        {
            this.pictureInk.Ink.CreateStroke(points);
            this.pictureInk.Ink.Strokes[this.pictureInk.Ink.Strokes.Count - 1].DrawingAttributes.Color = color;
        }

        public void addStroke(Sketch.Point[] points, Color color)
        {
            System.Drawing.Point[] sysPoints = new System.Drawing.Point[points.Length];
            for (int i = 0; i < points.Length; i++)
                sysPoints[i] = points[i].SysDrawPoint;

            addStroke(sysPoints, color);
        }

        public void moveStrokes()
        {
            this.inkMovedX = -pictureInk.Ink.GetBoundingBox().X + CUSHION;
            this.inkMovedY = -pictureInk.Ink.GetBoundingBox().Y + CUSHION;
            pictureInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
            pictureInk.Renderer.Scale(this.scale, this.scale);
        }

        public void drawPrimitives(Featurefy.StrokeFeatures features)
        {
            foreach (Primitive primitive in features.Primitives)
            {
                primitive.draw(this.g, this.r, this.inkMovedX, this.inkMovedY, this.scale);
            }
        }

		private void dataView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListView.SelectedListViewItemCollection selected = dataView.SelectedItems;
			foreach (ListViewItem i in selected)
			{
				if (i.Text == "L Intersections" && _sketchFeatures.NumLIntersections(_substroke) != 0)
				{
					string text = "";
					foreach (FeatureSketch.LStrokes l in _sketchFeatures.GetLIntersections(_substroke))
					{
						if (l.S1 == _substroke) text += l.S2.Id.ToString();
						else text += l.S1.Id.ToString();
						text += Environment.NewLine;
					}
					MessageBox.Show(text, "L Intersections", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else if (i.Text == "X Intersections" && _sketchFeatures.NumXIntersections(_substroke) != 0)
				{
					string text = "";
					foreach (FeatureSketch.XStrokes x in _sketchFeatures.GetXIntersections(_substroke))
					{
						if (x.S1 == _substroke) text += x.S2.Id.ToString();
						else text += x.S1.Id.ToString();
						text += Environment.NewLine;
					}
					MessageBox.Show(text, "X Intersections", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else if (i.Text == "T Intersections" && _sketchFeatures.NumTIntersections(_substroke) != 0)
				{
					string text = "";
					foreach (FeatureSketch.TStrokes t in _sketchFeatures.GetTIntersections(_substroke))
					{
						if (t.S1 == _substroke) text += "Stem: " + t.S2.Id.ToString();
						else text += "Top: " + t.S1.Id.ToString();
						text += Environment.NewLine;
					}
					MessageBox.Show(text, "T Intersections", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}
    }
}