using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;

using Sketch;
using Labeler; // For DomainInfo
using SketchPanelLib;


namespace CorrectionToolkit
{
    #region Feedback Framework

    /// <summary>
    /// A FeedbackMechanism applies a visual transformation to a sketch.  A 
    /// feedback mechanism might recolor Ink strokes, replace Ink strokes with symbols,
    /// label Ink strokes with textual labels, etc.  A FeedbackMechanism is similar to a
    /// Visitor (see Design Patterns).
    /// 
    /// This generic (abstract) Feedback Mechanism that subscribes to a SketchPanel and 
    /// (possibly) loads a domain mapping Sketch symbols to colors.  Feedback can be triggered
    /// abitrarily, but usually FeedbackMechanisms are sensative to the several events that 
    /// SketchPanels publish to (such as the Zoom Event or the File Loaded Event).  Subclassers
    /// should implement the SubscribeToPanel() and UnsubscribeFromPanel() methods in order
    /// to hook into these events.
    /// 
    /// Feedback should run in the same thread as the front end.  
    /// </summary>
    public abstract class FeedbackMechanism : SketchPanelSubscriber
    {
        /// <summary>
        /// The domain mapping sketch labels to colors.
        /// </summary>
        protected DomainInfo mDomain;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FeedbackMechanism()
            : base()
        {
        }

        /// <summary>
        /// Constructor.  Subscribes to parent panel.
        /// </summary>
        /// <param name="parentPanel">the parent SketchPanel</param>
        public FeedbackMechanism(SketchPanel parentPanel)
            : this(parentPanel, null)
        {
        }

        /// <summary>
        /// Constructor.  Subscribes to parent panel events and 
        /// loads target domain file.
        /// </summary>
        /// <param name="parentPanel"></param>
        public FeedbackMechanism(SketchPanel parentPanel, string domainFilePath)
            : base(parentPanel)
        {
            this.mDomain = LoadDomainInfo(domainFilePath);
        }

        /// <summary>
        /// Loads the given domain file.  
        /// <seealso cref="Labeler.MainForm.LoadDomain"/>
        /// </summary>
        /// <param name="domainFilePath">the file path to load</param>
        /// <returns>the DomainInfo loaded</returns>
        public static DomainInfo LoadDomainInfo(string domainFilePath)
        {
            // Check to see if there is a domain file to load for this feedback mechanism
            if (domainFilePath == null)
                return null;

            // Make sure file exists
            if (!System.IO.File.Exists(domainFilePath))
                return null;

            // Load domain file
            System.IO.StreamReader sr = new System.IO.StreamReader(domainFilePath);

            DomainInfo domain = new DomainInfo();
            string line = sr.ReadLine();
            string[] words = line.Split(null);

            // The first line is the study info
            domain.AddInfo(words[0], words[1]);
            line = sr.ReadLine();

            // The next line is the domain
            words = line.Split(null);
            domain.AddInfo(words[0], words[1]);
            line = sr.ReadLine();

            // Then the rest are labels
            while (line != null && line != "")
            {
                words = line.Split(null);

                string label = words[0];
                int num = int.Parse(words[1]);
                string color = words[2];

                domain.AddLabel(num, label, Color.FromName(color));
                line = sr.ReadLine();
            }

            List<string> labels = domain.GetLabels();
            string[] labelsWithColors = new string[labels.Count];

            for (int i = 0; i < labelsWithColors.Length; i++)
            {
                labelsWithColors[i] = labels[i] + "   (" +
                    domain.GetColor(labels[i]).Name + ")";
            }

            sr.Close();

            return domain;
        }

        /// <summary>
        /// Gets the domain for this Feedback Mechanism
        /// </summary>
        public DomainInfo DomainInfo
        {
            get
            {
                return mDomain;
            }
        }
    }

    #endregion

    #region Sample (Stroke Coloring) Feedback Mechanism

    /// <summary>
    /// Colors Ink strokes that correspond to Sketch
    /// symbols using a domain file that maps between symbol
    /// names (labels) and colors.
    /// </summary>
    public class ColorFeedback : FeedbackMechanism
    {
        /// <summary>
        /// Constructors
        /// </summary>
        public ColorFeedback()
            : base() { }

        public ColorFeedback(SketchPanel parentPanel)
            : base(parentPanel) { }

        public ColorFeedback(SketchPanel parentPanel, string domainFilePath)
            : base(parentPanel, domainFilePath) { }

        /// <summary>
        /// <see cref="SketchPanelSubscriber.SubscribeToPanel"/>
        /// </summary>
        public override void SubscribeToPanel(SketchPanel parentPanel)
        {
            base.SubscribeToPanel(parentPanel);

            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
            sketchPanel.SketchFileLoaded += new SketchFileLoadedHandler(sketchPanel_SketchFileLoaded);
            //sketchPanel.InkPicture.Resize += new EventHandler(InkPicture_Resize);
        }

        /// <summary>
        /// <see cref="SketchPanelSubscriber.UnsubscribeFromPanel"/>
        /// </summary>
        public override void UnsubscribeFromPanel()
        {
            sketchPanel.ResultReceived -= new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
            sketchPanel.SketchFileLoaded -= new SketchFileLoadedHandler(sketchPanel_SketchFileLoaded);
        }

        /// <summary>
        /// DEBUG TEMP HACK
        /// </summary>
        //void InkPicture_Resize(object sender, EventArgs e)
        //{
        //    colorStrokes();
        //}

        /// <summary>
        /// Attempts to color the Ink strokes using the domain when a file is loaded
        /// </summary>
        private void sketchPanel_SketchFileLoaded()
        {
            colorStrokes();
        }

        /// <summary>
        /// Colors the Ink strokes when a recognition result is received.
        /// </summary>
        private void sketchPanel_ResultReceived(RecognitionResult result)
        {
            colorStrokes();
        }

        /// <summary>
        /// Colors Ink strokes in the parent SketchPanel using the domain.
        /// </summary>
        protected virtual void colorStrokes()
        {
            if (mDomain == null)
                return;

            // Attempt to color all Ink strokes
            foreach (Microsoft.Ink.Stroke iStroke in sketchPanel.InkSketch.Ink.Strokes)
            {
                // Get corresponding label
                Substroke sStroke = sketchPanel.InkSketch.GetSketchSubstrokeByInkId(iStroke.Id);
                string label = sStroke.FirstLabel;

                // DEBUG Console.WriteLine(label == null ? "null" : label);

                // Skip unlabeled strokes
                /*if (label == null || label.Equals("unlabeled"))
                {
                    Console.WriteLine("here111");
                    continue;
                }*/

                // HACK for truth tables
                /*if (label.Equals("Label"))
                {
                    foreach (Shape sh in sStroke.ParentShapes)
                    {
                        if (sh.XmlAttrs.Type.Equals("I"))
                            label = "Divider";
                    }
                }*/

                // Color the stroke; if the stroke is not in the domain, then we
                // color the stroke black.
                iStroke.DrawingAttributes.Color = mDomain.GetColor(label);
            }

            // Request repaint
            sketchPanel.InkPicture.Invalidate();
        }
    }

    #endregion
}
