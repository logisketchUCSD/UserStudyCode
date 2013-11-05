using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using Featurefy;

namespace StrokeInfoFormWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StrokeInfoWindow : Window
    {
        #region Internals
        FeatureSketch mSketchFeatures;
        FeatureStroke mStrokeFeatures;
        Stroke mInkStroke;
        Sketch.Substroke mSubstroke;
        double inkMovedX;
        double inkMovedY;
        float scale;
        //Graphics graphics;

        Color Red;
        Color Blue;
        Color Green;
        Color Black;
        
        const double CUSION = 500;
        #endregion

        #region Constructors
        /// <summary>
        /// Zero-Argument Constructor for StrokeInfoWindow
        /// </summary>
        public StrokeInfoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for StrokeInfoForm
        /// </summary>
        /// <param name="strokefeatures"></param> a FeatureStroke
        /// <param name="sketchFeatures"></param> a FeatureSketch
        /// <param name="inkStroke"></param> a System.Windows.Controls Stroke
        /// <param name="substroke"></param> a Sketch Substroke
        public StrokeInfoWindow(FeatureStroke strokefeatures, FeatureSketch sketchFeatures, Stroke inkStroke, Sketch.Substroke substroke)
        {
            InitializeComponent();

            inkMovedX = 0;
            inkMovedY = 0;
            scale = 2.0f;

            mInkCanvas.Width = Width/2 - 20;
            mInkCanvas.Height = Height-20;
            
            mInkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(mInkCanvas_StrokeCollected);

            mSketchFeatures = sketchFeatures;
            mStrokeFeatures = strokefeatures;
            mInkStroke = inkStroke;
            mSubstroke = substroke;

            mInkCanvas.Strokes.Add(mInkStroke);

            Red = Color.FromArgb(255,255,0,0);
            Green = Color.FromArgb(255, 0, 255, 0);
            Blue = Color.FromArgb(255, 0, 0, 255);
            Black = Color.FromArgb(255,0,0,0);

            mInkCanvas.Strokes[0].DrawingAttributes.Color = Red;
            populateListView();

        }
        #endregion

        #region ListView stuff
        // Still do not have this figured out, so commented out for now

        /// <summary>
        /// Helps display all the features of the stroke
        /// </summary>
        private void populateListView()
        {
            /*System.Windows.Application app = new System.Windows.Application();
            ListBox info = new ListView();
            this.
            addNewDatum("Stroke ID", mStrokeFeatures.Id, info);
            addNewDatum("Stroke Type", mSubstroke.FirstLabel, info);
            addNewDatum("Number of Points", mStrokeFeatures.Points.Length, info);
            ListViewGroup arcLength = new ListViewGroup("Arc Length Features");
            dataView.Groups.Add(arcLength);
            addNewDatum("Arc Length", mStrokeFeatures.ArcLength.TotalLength, arcLength);
            addNewDatum("Ink Density (rectangular)", mStrokeFeatures.ArcLength.InkDensity, arcLength);
            addNewDatum("Ink Density (circular)", mStrokeFeatures.ArcLength.CircularInkDensity, arcLength);
            ListViewGroup spatial = new ListViewGroup("Spatial Features");
            dataView.Groups.Add(spatial);
            addNewDatum("Bounding Box Width", mStrokeFeatures.Spatial.Width, spatial);
            addNewDatum("Bounding Box Height", mStrokeFeatures.Spatial.Height, spatial);
            addNewDatum("Bounding Box Area", mStrokeFeatures.Spatial.Area, spatial);
            ListViewGroup curvature = new ListViewGroup("Curvature Features");
            dataView.Groups.Add(curvature);
            addNewDatum("Total Angle", mStrokeFeatures.Curvature.TotalAngle, curvature);
            addNewDatum("Average Curvature", mStrokeFeatures.Curvature.AverageCurvature, curvature);
            addNewDatum("Total Abs Angle", mStrokeFeatures.Curvature.TotalAbsAngle, curvature);
            addNewDatum("Total Squared Angle", mStrokeFeatures.Curvature.TotalSquaredAngle, curvature);
            addNewDatum("Sum Sqrt Curvature", mStrokeFeatures.Curvature.SumSqrtCurvature, curvature);
            addNewDatum("Minimum Curvature", mStrokeFeatures.Curvature.MinimumCurvature, curvature);
            addNewDatum("Maximum Curvature", mStrokeFeatures.Curvature.MaximumCurvature, curvature);
            ListViewGroup speed = new ListViewGroup("Speed Features");
            dataView.Groups.Add(speed);
            addNewDatum("Average Speed", mStrokeFeatures.Speed.AverageSpeed, speed);
            addNewDatum("Maximum Speed", mStrokeFeatures.Speed.MaximumSpeed, speed);
            addNewDatum("Minimum Speed", mStrokeFeatures.Speed.MinimumSpeed, speed);
            ListViewGroup slope = new ListViewGroup("Slope Features");
            dataView.Groups.Add(slope);
            addNewDatum("Average Slope", mStrokeFeatures.Slope.AverageSlope, slope);
            addNewDatum("Slope Direction", mStrokeFeatures.Slope.Direction, slope);
            ListViewGroup intersections = new ListViewGroup("Intersection Features");
            dataView.Groups.Add(intersections);
            addNewDatum("Self Intersections", mStrokeFeatures.Spatial.NumSelfIntersections, intersections);
            addNewDatum("L Intersections", mSketchFeatures.NumLIntersections(mSubstroke), intersections);
            addNewDatum("T Intersections", mSketchFeatures.NumTIntersections(mSubstroke), intersections);
            addNewDatum("X Intersections", mSketchFeatures.NumXIntersections(mSubstroke), intersections);
            addNewDatum("Single-Stroke Closed Shape", mStrokeFeatures.ClosedShape, intersections);
            addNewDatum("Multi-Stroke Closed Shape", mSketchFeatures.InClosedShape(mSubstroke), intersections);
            this.Show();
            app.Run(this);*/
        }

        private void addNewDatum(string key, object value)
        {
            //ListViewItem item = new ListViewItem();
            //item.SubItems.Add(value.ToString());
            //dataView.Items.Add(item);
        }

        private void addNewDatum(string key, object value, int TEMP)//, ListViewGroup group)
        {
            //ListViewItem item = new ListViewItem(key, group);
            //item.SubItems.Add(value.ToString());
            //dataView.Items.Add(item);
        }
        #endregion

        #region Modifying Strokes

        public void addStroke(StylusPointCollection points, Color color)
        {
            Stroke newStroke = new Stroke(points);
            newStroke.DrawingAttributes.Color = color;
            this.mInkCanvas.Strokes.Add(newStroke);
        }

        public void addStroke(Sketch.Point[] points, Color color)
        {
            StylusPointCollection sysPoints = new StylusPointCollection(points.Count());
            for (int i = 0; i < points.Length; i++)
            {
                sysPoints[i] = new StylusPoint((double)points[i].X, (double)points[i].Y, (float)points[i].Pressure);
            }
            addStroke(sysPoints, color);
        }

        public void moveStrokes()
        {
            inkMovedX = mInkCanvas.Strokes.GetBounds().X + CUSION;
            inkMovedY = mInkCanvas.Strokes.GetBounds().Y + CUSION;
           
            // Do not know how to do this with InkCanvas
            //pictureInk.Renderer.Move(this.inkMovedX, this.inkMovedY);
            //pictureInk.Renderer.Scale(this.scale, this.scale);
        }

        public void drawPrimitives(Featurefy.StrokeFeatures features)
        {
            foreach (Sketch.Primitive primitive in features.Primitives)
            {
                // Do not know how to do this without renderer
                //primitive.draw(this.g, this.r, this.inkMovedX, this.inkMovedY, this.scale);
            }
        }

        #endregion

        #region Events

        private void mInkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Commented out in original...
            //drawPrimitives(this.features);
        }

        // Still working out ListView problems
        private void dataView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            /*
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
            }*/
        }

        #endregion
    }
}
