using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Microsoft.Ink;

using CommandManagement;
using CorrectionToolkit;

namespace SketchPanelLib
{
    /// <summary>
    /// Error correction test program.  Modified from SketchJournal:
    /// 
    /// SketchJournal is an attempt to recreate our own version of 
    /// Windows Journal (to the extent that SketchJournal will 
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
        /// Feedback mechanism for coloring strokes in circuit panel
        /// </summary>
        private ColorFeedback circuitColorFeedback;
        
        /// <summary>
        /// Label Correction tool for correcting labels
        /// of recognition results
        /// </summary>
        private LassoMenuStaticTool lassoMenuStaticTool;

        private LassoMenuStaticTool lassoSortedMenuTool;
        /// <summary>
        /// Regrouping tool to indicate stroke groups
        /// </summary>
        private LassoRegroupTool lassoRegroupTool;

        /// <summary>
        /// Command Manager for providing Undo/Redo
        /// TEMP NOT WIRED TO GUI
        /// </summary>
        private CommandManagement.CommandManager commandManager;
        
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
            //recognizer = new TimeoutRecognizer();
            recognizer = new GateRecognizer();
            //recognizer = new PartialGateRecognizer();
            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);

            // Add and configure feedback
            circuitColorFeedback = new ColorFeedback(sketchPanel, FilenameConstants.DefaultCircuitDomainFilePath);

            // Set up selection clearing callback for toolbar
            // FOR ALL NEW TOOLS, should add
            lassoMenuButton.CheckedChanged += new EventHandler(lassoMenuButton_CheckedChanged);
            lassoSortedMenuButton.CheckedChanged += new EventHandler(lassoSortedMenuButton_CheckedChanged);
            lassoRegroupButton.CheckedChanged += new EventHandler(lassoRegroupButton_CheckedChanged);
            retraceButton.CheckedChanged += new EventHandler(retraceButton_CheckedChanged);


            // Set up correction tool
            commandManager = new CommandManagement.CommandManager();
            lassoMenuStaticTool = new LassoMenuStaticTool(commandManager);
            lassoSortedMenuTool = new LassoMenuStaticTool(commandManager); // Same type as static for now
            lassoRegroupTool = new LassoRegroupTool(commandManager);
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

            saveFileDialog.Filter = "MIT XML Files (*.xml)|*.xml";
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
            uncheckOthers(sketchButton);

            // Hack to clear selection
            sketchPanel.InkPicture.Selection = sketchPanel.InkPicture.Ink.CreateStrokes();

            sketchPanel.Enabled = false;
            sketchPanel.InkPicture.EditingMode = InkOverlayEditingMode.Ink;

            sketchPanel.Enabled = true;
        }

        /// <summary>
        /// Toggles to Ink _Select_ mode.
        /// </summary>
        private void selectButton_Click(object sender, EventArgs e)
        {
            uncheckOthers(selectButton);

            sketchPanel.Enabled = false;
            sketchPanel.InkPicture.EditingMode = InkOverlayEditingMode.Select;

            sketchPanel.Enabled = true;
        }

        private void lassoMenuButton_Click(object sender, EventArgs e)
        {
            lassoMenuButton.Checked = true;
            uncheckOthers(lassoMenuButton);
        }

        private void lassoSortedMenuButton_Click(object sender, EventArgs e)
        {
            lassoSortedMenuButton.Checked = true;
            uncheckOthers(lassoSortedMenuButton);
        }

        private void lassoRegroupButton_Click(object sender, EventArgs e)
        {
            lassoRegroupButton.Checked = true;
            uncheckOthers(lassoRegroupButton);
        }

        private void retraceButton_Click(object sender, EventArgs e)
        {
            retraceButton.Checked = true;
            uncheckOthers(retraceButton);
        }

        /// <summary>
        /// Enables or disables the static menu correction tool
        /// </summary>
        private void lassoMenuButton_CheckedChanged(object sender, EventArgs e)
        {
            if (lassoMenuButton.Checked)
            {
                lassoMenuStaticTool.SubscribeToPanel(sketchPanel);
            }
            else
            {
                lassoMenuStaticTool.UnsubscribeFromPanel();
            }
        }

        /// <summary>
        /// Enables or disables the regroup correction tool
        /// </summary>
        private void lassoRegroupButton_CheckedChanged(object sender, EventArgs e)
        {
            if (lassoRegroupButton.Checked)
            {
                lassoRegroupTool.SubscribeToPanel(sketchPanel);
            }
            else
            {
                lassoRegroupTool.UnsubscribeFromPanel();
            }
        }
        
        private void lassoSortedMenuButton_CheckedChanged(object sender, EventArgs e)
        {
            if (lassoSortedMenuButton.Checked)
            {
                lassoSortedMenuTool.SubscribeToPanel(sketchPanel);
            }
            else
            {
                lassoSortedMenuTool.UnsubscribeFromPanel();
            }
        }

        private void retraceButton_CheckedChanged(object sender, EventArgs e)
        {
            //sketchPanel.Enabled = false;

            if (retraceButton.Checked)
            {
                sketchPanel.InkPicture.DefaultDrawingAttributes.Color = System.Drawing.Color.Red;
                //sketchPanel.InkPicture.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                sketchPanel.InkPicture.DefaultDrawingAttributes.Color = System.Drawing.Color.Black;
                //sketchPanel.InkPicture.ForeColor = System.Drawing.Color.Red;
            }

            //sketchPanel.Enabled = true;
        }


        /// <summary>
        /// Unchecks all toolstrip buttons except for <tt>current</tt>
        /// </summary>
        /// <param name="current">the tool button to leave checked</param>
        private void uncheckOthers(ToolStripButton current)
        {
            List<ToolStripButton> toolButtons = new List<ToolStripButton>();

            // MAINTAIN Add buttons for new tools the the List here
            toolButtons.Add(sketchButton);
            toolButtons.Add(selectButton);
            toolButtons.Add(lassoMenuButton);
            toolButtons.Add(lassoSortedMenuButton);
            toolButtons.Add(lassoRegroupButton);
            toolButtons.Add(retraceButton);

            foreach (ToolStripButton b in toolButtons)
            {
                if (b != current)
                {
                    b.Checked = false;
                }
            }
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

            recognizer = new GateRecognizer();
            gateToolStripMenuItem.Checked = true;
            partialGateToolStripMenuItem.Checked = false;

            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
            
        }

        private void partialGateToolStripMenuItem_Click(object sender, EventArgs e)
        {

            recognizer.UnsubscribeFromPanel();

            recognizer = new PartialGateRecognizer();
            gateToolStripMenuItem.Checked = false;
            partialGateToolStripMenuItem.Checked = true;

            recognizer.SubscribeToPanel(sketchPanel);
            sketchPanel.ResultReceived += new RecognitionResultReceivedHandler(sketchPanel_ResultReceived);
        }

        private void rerecognizeCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (rerecognizeCheckbox.Checked == true)
                Console.Beep();

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
