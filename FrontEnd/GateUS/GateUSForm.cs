using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

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

        private int usernumber = -1;
        private string[] gates = new string[6];
        private string[] copies = new string[2];
        private string[] eqs = new string[2];
        private string[] commands = new string[10];
        private string currCommand = null;
        private int counter = -1;
        //private int mode;
        private Random rand = new Random();
        private readonly string AND = "Please draw 10 AND gates (one at a time) in the space below, including 2 input and 1 output wires.  When you're finished, please press done.";
        private readonly string OR = "Please draw 10 OR gates (one at a time) in the space below, including 2 input and 1 output wires.  When you're finished, please press done.";
        private readonly string XOR = "Please draw 10 XOR gates (one at a time) in the space below, including 2 input and 1 output wires.  When you're finished, please press done.";
        private readonly string NAND = "Please draw 10 NAND gates (one at a time) in the space below, including 2 input and 1 output wires.  When you're finished, please press done.";
        private readonly string NOR = "Please draw 10 NOR gates (one at a time) in the space below, including 2 input and 1 output wires.  When you're finished, please press done.";
        private readonly string NOT = "Please draw 10 NOT gates (one at a time) in the space below, including 1 input and 1 output wires.  When you're finished, please press done.";
        private readonly string COPY1 = "Please receive Circuit 1 from Sam or Raquel and copy it in the space below.  When you're finished, please press done.";
        private readonly string COPY2 = "Please receive Circuit 2 from Sam or Raquel and copy it in the space below.  When you're finished, please press done.";
        private readonly string EQ1 = "Please receive Equation 1 from Sam or Raquel and draw the circuit out in the space below.  When you're finished, please press done.";
        private readonly string EQ2 = "Please receive Equation 2 from Sam or Raquel and draw the circuit out in the space below.  When you're finished, please press done.";
        
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SketchJournalMainForm()
        {
            InitializeComponent();
            doneButton.Text = "Begin";
            gates[0] = AND;
            gates[1] = OR;
            gates[2] = XOR;
            gates[3] = NAND;
            gates[4] = NOR;
            gates[5] = NOT;
            copies[0] = COPY1;
            copies[1] = COPY2;
            eqs[0] = EQ1;
            eqs[1] = EQ2;
            gates = shuffle(gates);
            copies = shuffle(copies);
            eqs = shuffle(eqs);

            // Initialize and add InkPicture to form
            toolStripContainer1.ContentPanel.SuspendLayout();
            sketchPanel = new SketchPanel();
            sketchPanel.Dock = DockStyle.Fill;
            toolStripContainer1.ContentPanel.Controls.Add(sketchPanel);
            toolStripContainer1.ContentPanel.ResumeLayout();

            // Attach a (demo) recognizer
            recognizer = new TimeoutRecognizer();
            recognizer.SubscribeToPanel(sketchPanel);            
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
        /// Displays a color dialog to change the default ink color
        /// </summary>
        private void inkColorButton_Click(object sender, EventArgs e)
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

        private void SketchJournalMainForm_Load(object sender, EventArgs e)
        {

        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            //sketchPanel.SaveSketch("happy.xml");
            //sketchPanel.InitPanel();
            textBox1.Dispose();
            label1.Dispose();
            save();
            counter++;
            if (counter < commands.Length)
            {
                doneButton.Text = "Done";
                command.Text = commands[counter];
                currCommand = commands[counter];
                sketchPanel.InitPanel();

            }
            else
            {
                command.Text = "Thank you very much for participating in our test! Please go collect your treat.";
                doneButton.Text = "Yay";
                currCommand = null;
                sketchPanel.InitPanel();
            }
        }

        private void save()
        {
            if (doneButton.Text.Equals("Begin"))
                sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_STOPSIGN.xml");
            if(currCommand!=null)
            {
                if(currCommand.Equals(AND))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_AND.xml");
                else if(currCommand.Equals(OR))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_OR.xml");
                else if(currCommand.Equals(XOR))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_XOR.xml");
                else if(currCommand.Equals(NAND))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_NAND.xml");
                else if(currCommand.Equals(NOR))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_NOR.xml");
                else if(currCommand.Equals(NOT))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_NOT.xml");
                else if(currCommand.Equals(COPY1))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_COPY1.xml");
                else if(currCommand.Equals(COPY2))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_COPY2.xml");
                else if(currCommand.Equals(EQ1))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_EQ1.xml");
                else if(currCommand.Equals(EQ2))
                    sketchPanel.SaveSketch("C:\\GateStudy\\" + usernumber + "_EQ2.xml");
            }
            
        }

        private string[] shuffle(string[] orig)
        {
            string[] shuffled = new string[orig.Length];
            bool[] used = new bool[shuffled.Length];
            for (int i = 0; i < used.Length; i++)
                used[i] = false;
            int iCount = 0;
            while (iCount < shuffled.Length)
            {
                int myRand = rand.Next(0, orig.Length);
                if(!used[myRand])
                {
                    shuffled[iCount] = orig[myRand];
                    used[myRand] = true;
                    iCount++;
                }
            }
            return shuffled;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(textBox1.Text.Length>0)
                usernumber = Int32.Parse(textBox1.Text);
            shuffleOnUserNum();
        }
        private void shuffleOnUserNum()
        {
            //int totalLength = gates.Length + copies.Length + eqs.Length;
            int gatesCopiesLength = gates.Length + copies.Length;
            int gatesEqsLength = gates.Length + eqs.Length;
            int copiesEqsLength = copies.Length + eqs.Length;
            int cmd = usernumber % 6;
            switch (cmd)
            {
                case 0:
                    for (int i = 0; i < gates.Length; i++)
                        commands[i] = gates[i];
                    for (int i = 0; i < copies.Length; i++)
                        commands[i+gates.Length] = copies[i];
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i+gatesCopiesLength] = eqs[i];
                    break;
                case 1:
                    for (int i = 0; i < gates.Length; i++)
                        commands[i] = gates[i];
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i+gates.Length] = eqs[i];
                    for (int i = 0; i < copies.Length; i++)
                        commands[i+gatesEqsLength] = copies[i];
                    break;
                case 2:
                    for (int i = 0; i < copies.Length; i++)
                        commands[i] = copies[i];
                    for (int i = 0; i < gates.Length; i++)
                        commands[i+copies.Length] = gates[i];
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i+gatesCopiesLength] = eqs[i];
                    break;
                case 3:
                    for (int i = 0; i < copies.Length; i++)
                        commands[i] = copies[i];
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i+copies.Length] = eqs[i];
                    for (int i = 0; i < gates.Length; i++)
                        commands[i+copiesEqsLength] = gates[i];
                    break;
                case 4:
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i] = eqs[i];
                    for (int i = 0; i < gates.Length; i++)
                        commands[i+eqs.Length] = gates[i];
                    for (int i = 0; i < copies.Length; i++)
                        commands[i+gatesEqsLength] = copies[i];
                    break;
                case 5:
                    for (int i = 0; i < eqs.Length; i++)
                        commands[i] = eqs[i];
                    for (int i = 0; i < copies.Length; i++)
                        commands[i+eqs.Length] = copies[i];
                    for (int i = 0; i < gates.Length; i++)
                        commands[i+copiesEqsLength] = gates[i];
                    break;
                default:
                    break;
            }
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
            Application.SetCompatibleTextRenderingDefault(true);
            Application.Run(new SketchJournalMainForm());
        }
    }
}
