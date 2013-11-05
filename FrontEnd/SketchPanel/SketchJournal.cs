using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Featurefy;
using Microsoft.Ink;

namespace SketchPanelLib
{
    /// <summary>
    /// SketchJournal program.  SketchJournal is an attempt to recreate our
    /// own version of Windows Journal (to the extent that SketchJournal will 
    /// be useful to us) and to demonstrate how to use 
    /// the SketchPanel class.
    /// </summary>
    public partial class SketchJournalMainForm : Form
    {
        /// <summary>
        /// The SketchPanel instance to demo
        /// </summary>
        private SketchPanel sketchPanel;

        /// <summary>
        /// A demo Sketch Recognizer
        /// </summary>
        private SketchRecognizer recognizer;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SketchJournalMainForm()
        {
            InitializeComponent();

            // Initialize and add InkPicture to form
            toolStripContainer1.ContentPanel.SuspendLayout();
            sketchPanel = new SketchPanel();
            sketchPanel.Dock = DockStyle.Fill;
            toolStripContainer1.ContentPanel.Controls.Add(sketchPanel);
            toolStripContainer1.ContentPanel.ResumeLayout();

            // Attach a (demo) recognizer
            //recognizer = new TimeoutSketchRecognizer();
            recognizer = new GateSketchRecognizer();
            //recognizer = new PartialGateSketchRecognizer();
            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
        }

        /// <summary>
        /// Opens a Sketch XML or Journal file.
        /// </summary>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Load a Sketch";
            openFileDialog.Filter = "MIT XML sketches (*.xml)|*.xml|" +
                "Microsoft Windows Journal Files (*.jnt)|*.jnt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!System.IO.File.Exists(openFileDialog.FileName))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                sketchPanel.LoadSketch(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Saves a Sketch XML file.
        /// </summary>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "MIT XML Files (*.xml)|*.xml|Canonical Example (*.cxtd)|*.cxtd";
            saveFileDialog.AddExtension = true;

            // Write the XML to a file
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
				sketchPanel.SaveSketch(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Toggles to Ink _Sketch_ mode.
        /// </summary>
        private void sketchButton_Click(object sender, EventArgs e)
        {
            // Hack to clear selection
            sketchPanel.InkPicture.Selection = sketchPanel.InkPicture.Ink.CreateStrokes();

            sketchPanel.Enabled = false;
            sketchPanel.InkPicture.EditingMode = InkOverlayEditingMode.Ink;
            sketchButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            selectButton.BackColor = System.Drawing.SystemColors.Control;
            sketchPanel.Enabled = true;
        }

        /// <summary>
        /// Toggles to Ink _Select_ mode.
        /// </summary>
        private void selectButton_Click(object sender, EventArgs e)
        {
            sketchPanel.Enabled = false;
            sketchPanel.InkPicture.EditingMode = InkOverlayEditingMode.Select;
            sketchButton.BackColor = System.Drawing.SystemColors.Control;
            selectButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            sketchPanel.Enabled = true;
        }

        /// <summary>
        /// Copies selected strokes (or all ink if there is no selection)
        /// to clipboard
        /// </summary>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.CopyStrokes();
        }

        /// <summary>
        /// Cuts selected strokes (or all ink if there is no selection)
        /// to clipboard
        /// </summary>
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.CutStrokes();
        }

        /// <summary>
        /// Pastes from clipboard
        /// </summary>
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchButton_Click(sender, e);
            sketchPanel.PasteStrokes();
            sketchPanel.InkPicture.Refresh();
        }

        /// <summary>
        /// Undoes the last command
        /// to clipboard
        /// </summary>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.Undo();
        }

        /// <summary>
        /// Redoes the last command
        /// to clipboard
        /// </summary>
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.Redo();
        }

        /// <summary>
        /// Deletes selected strokes from panel
        /// </summary>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.DeleteStrokes();
        }

        /// <summary>
        /// Zooms in
        /// </summary>
        private void zoomInButton_Click(object sender, EventArgs e)
        {
            sketchPanel.ZoomIn();
        }

        /// <summary>
        /// Zooms out
        /// </summary>
        private void zoomOutButton_Click(object sender, EventArgs e)
        {
            sketchPanel.ZoomOut();
        }

        /// <summary>
        /// Zooms to fit the screen
        /// </summary>
        private void zoomToFit_Click(object sender, EventArgs e)
        {
            sketchPanel.ZoomToFit();
        }

        /// <summary>
        /// Displays a color dialog to change the default ink color
        /// </summary>
        private void inkColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDlg = new System.Windows.Forms.ColorDialog();
            colorDlg.AnyColor = true;
            colorDlg.ShowHelp = true;
            colorDlg.Color = sketchPanel.InkPicture.DefaultDrawingAttributes.Color;

            if (colorDlg.ShowDialog() != DialogResult.Cancel)
            {
                sketchPanel.InkPicture.DefaultDrawingAttributes.Color = colorDlg.Color;
            }
        }

        /// <summary>
        /// Invokes recognition on the sketch.
        /// </summary>
        private void recognizeButton_Click(object sender, EventArgs e)
        {
            sketchPanel.TriggerRecognition();
        }

        /// <summary>
        /// Creates a new Sketch
        /// </summary>
        private void newSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchButton_Click(sender, e);
            sketchPanel.InitPanel();
        }

        /// <summary>
        /// Sets the background image of the SketchPanel to a user specified file.
        /// </summary>
        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Load a Background Image...";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!System.IO.File.Exists(openFileDialog.FileName))
                {
                    MessageBox.Show("Error: target file does not exist");
                }

                this.sketchPanel.ChangeBackground(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Sets the background image of the SketchPanel to a grid.
        /// </summary>
        private void graphPaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sketchPanel.ChangeBackground(
                System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "/images/graphpaper.gif");
        }

        /// <summary>
        /// Sets the background image of the SketchPanel to a lined paper.
        /// </summary>
        private void linedPaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sketchPanel.ChangeBackground(
                System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "/images/notebook_paper.jpg");
        }

        /// <summary>
        /// Unsets the background image of the SketchPanel.
        /// </summary>
        private void blankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.sketchPanel.ChangeBackground("");
        }

        /// <summary>
        /// Sets the status line of the GUI with the status of the recognition 
        /// result (e.g., the probabilities assigned to the recognized symbols).
        /// </summary>
        /// <param name="result"></param>
        private void sketchPanel_ResultReceived(RecognitionResult result)
        {
            if (result != null)
                this.toolStripStatusLabel1.Text = result.status;
        }

        private void gateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            recognizer.UnsubscribeFromPanel();

            recognizer = new GateSketchRecognizer();
            gateToolStripMenuItem.Checked = true;
            partialGateToolStripMenuItem.Checked = false;

            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
            
        }

        private void partialGateToolStripMenuItem_Click(object sender, EventArgs e)
        {

            recognizer.UnsubscribeFromPanel();

            recognizer = new PartialGateSketchRecognizer();
            gateToolStripMenuItem.Checked = false;
            partialGateToolStripMenuItem.Checked = true;

            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
        }

		private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			sketchPanel.DeleteAllStrokes();
		}

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sketchPanel.SelectAllStrokes();
        }

		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void menuStrokeInformation_Click(object sender, EventArgs e)
		{
			Sketch.Sketch sketch = sketchPanel.Sketch;
			FeatureSketch sketchFeatures = new FeatureSketch(ref sketch);
			Microsoft.Ink.Strokes selected = sketchPanel.InkSketch.Ink.Strokes;
			Microsoft.Ink.Stroke s = selected[selected.Count - 1];
			Sketch.Substroke ss = sketchPanel.InkSketch.GetSketchSubstrokeByInkId(s.Id);
			StrokeInfoForm.strokeInfoForm sif = new StrokeInfoForm.strokeInfoForm(sketchFeatures.GetFeatureStrokeByStrokeGUID(ss.Id), sketchFeatures, s, ss);
			sif.Show();
		}
    }

    /// <summary>
    /// Main executable for SketchJournal
    /// </summary>
    static class SketchJournalProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SketchJournalMainForm());
        }
    }
}
