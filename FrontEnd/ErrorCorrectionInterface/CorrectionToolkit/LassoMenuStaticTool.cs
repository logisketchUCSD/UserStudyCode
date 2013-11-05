using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using Microsoft.Ink;

using SketchPanelLib;
using Sketch;
using Labeler;
using CommandManagement;

namespace CorrectionToolkit
{
    /// <summary>
    /// UI Tool for correcting stroke labels (e.g., for correcting recognition results).
    /// </summary>
    public class LassoMenuStaticTool : SketchPanelSubscriber
    {
        #region Internals

        /// <summary>
        /// The CommandManager managine Undo/Redo for this tool
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// The LabelMenu supplying the label correction panel for this tool
        /// </summary>
        private LabelMenu labelMenu;

        /// <summary>
        /// The Label button that triggers the LabelMenu
        /// </summary>
        private Button labelButton;

        #endregion

        #region Constructors and Lifecycle Methods

        /// <summary>
        /// Constructor.  Hooks into CommandManager cm.
        /// </summary>
        /// <param name="cm">the CommandManager managing Undo/Redo for this tool</param>
        public LassoMenuStaticTool(CommandManager cm)
            : base()
        {
            commandManager = cm;
        }

        /*/// <summary>
        /// Constructor.  Subscribes to parentPanel and hooks CommandManager cm
        /// to this tool.
        /// </summary>
        /// <param name="parentPanel">the panel to subscribe to</param>
        /// <param name="cm">the CommandManager managing Undo/Redo for this tool</param>
        public LassoMenuStaticTool(SketchPanel parentPanel, CommandManager cm)
            : base()
        {
            commandManager = cm;
            init();
            SubscribeToPanel(parentPanel);
        }*/

        /// <summary>
        /// Instatiates LabelMenu and supporting classes
        /// </summary>
        private void init()
        {
            labelMenu = new LabelMenu(new LabelerPanelAdatper(sketchPanel), commandManager);
            DomainInfo domain = FeedbackMechanism.LoadDomainInfo(FilenameConstants.DefaultCircuitDomainFilePath);
            labelMenu.InitLabels(domain);

            labelButton = new Button();
            labelButton.BackColor = Color.Coral;
            labelButton.FlatStyle = FlatStyle.Flat;
            labelButton.Size = new Size(100, 35);
            labelButton.TextAlign = ContentAlignment.MiddleCenter;
            labelButton.Text = "Change Label";
            labelButton.MouseDown += new MouseEventHandler(labelButton_MouseDown);
        }

        /// <summary>
        /// Subscribes to SketchPanel.  Subscibe only when tool is selected.
        /// <see cref="SketchPanelLib.SketchPanelListener.SubscribeToPanel()"/>
        /// </summary>
        public override void SubscribeToPanel(SketchPanel parentPanel)
        {
            base.SubscribeToPanel(parentPanel);

            // Hook into SketchPanel Events
            sketchPanel.InkPicture.SelectionChanged += new InkOverlaySelectionChangedEventHandler(InkPicture_SelectionChanged);

            // Create LabelMenu and Button
            init();

            // Put panel in Select Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Select;

            // Add button and menu
            sketchPanel.Controls.Add(this.labelButton);
            sketchPanel.Controls.Add(this.labelMenu);

            // Initially hide the controls
            this.labelButton.Hide();
            this.labelMenu.Hide();
        }

        /// <summary>
        /// Unsubscribes from SketchPanel
        /// <see cref="SketchPanelLib.SketchPanelListener.UnSubscribeToPanel()"/>
        /// </summary>
        public override void UnsubscribeFromPanel()
        {
            if (sketchPanel == null)
                return;

            sketchPanel.InkPicture.SelectionChanged -= new InkOverlaySelectionChangedEventHandler(InkPicture_SelectionChanged);

            sketchPanel.Controls.Remove(this.labelButton);
            sketchPanel.Controls.Remove(this.labelMenu);

            // HACK Clear selection
            sketchPanel.InkPicture.Selection = new Ink().CreateStrokes();

            // Put panel in Ink Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Ink;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Opens LabelMenu when labelButton is pressed
        /// <see cref="Labeler.LabelerPanel.labelButton_MouseDown()"/>
        /// </summary>
        private void labelButton_MouseDown(object sender, MouseEventArgs e)
        {
            // Calculate LabelMenu position
            System.Drawing.Point topLeft = this.labelButton.Location;

            if (topLeft.X + labelMenu.Width > sketchPanel.InkPicture.Width)
                topLeft.X -= (labelMenu.Width - labelButton.Width);
            if (topLeft.Y + labelMenu.Height > sketchPanel.InkPicture.Height)
                topLeft.Y -= (labelMenu.Height - labelButton.Height);

            // Update the LableMenu labels
            labelMenu.UpdateLabelMenu(sketchPanel.InkPicture.Selection);

            // Move and show LabelMenu
            labelMenu.Location = topLeft;
            labelMenu.BringToFront();
            labelMenu.Show();
            labelMenu.Focus();
        }

        /// <summary>
        /// Moves labelButton to appropriate location and shows it when necessary.
        /// <see cref="Labeler.LabelerPanel.sketchInk_SelectionChanged()"/>
        /// </summary>
        private void InkPicture_SelectionChanged(object sender, EventArgs e)
        {
            if (sketchPanel.InkPicture.Selection.Count > 0)
            {
                // Calculate labelButton position
                int x, y;
                x = sketchPanel.InkPicture.Selection.GetBoundingBox().X +
                    sketchPanel.InkPicture.Selection.GetBoundingBox().Width;
                y = sketchPanel.InkPicture.Selection.GetBoundingBox().Y +
                    sketchPanel.InkPicture.Selection.GetBoundingBox().Height;

                System.Drawing.Point bottomRight = new System.Drawing.Point(x, y);
                using (Graphics g = sketchPanel.InkPicture.CreateGraphics())
                {
                    sketchPanel.InkPicture.Renderer.InkSpaceToPixel(sketchPanel.InkPicture.CreateGraphics(),
                        ref bottomRight);
                }

                // Position and show labelButton
                bottomRight.X -= 15 - sketchPanel.AutoScrollPosition.X; // Empirical padding for position
                bottomRight.Y -= 2 - sketchPanel.AutoScrollPosition.Y;  // Empirical padding for position
                labelButton.Location = bottomRight;

                labelButton.Visible = true;
                labelButton.BringToFront();
            }
            else
            {
                labelButton.Visible = false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Adapter/Wrapper class.  Partially maps fields and methods on a SketchPanel
    /// to supply a LabelerPanel-like Interface.
    /// </summary>
    public class LabelerPanelAdatper : LabelerPanel
    {
        /// <summary>
        /// The SketchPanel that this class wraps
        /// </summary>
        protected SketchPanel sketchPanel;

        /// <summary>
        /// HACK need this constructor to get this class to compile. 
        /// This Constructor simply creates a new LabelerPanel. Do not use it.
        /// <see cref="http://blogs.msdn.com/jmanning/archive/2005/09/21/472456.aspx"/>
        /// </summary>
        /*public LabelerPanelAdatper(CommandManager CM, DomainInfo domainInfo)
            : base(CM, domainInfo)
        {
        }*/

        /// <summary>
        /// Constructor.  Wraps the given SketchPanel
        /// </summary>
        /// <param name="panel">The panel to wrap</param>
        public LabelerPanelAdatper(SketchPanel panel)
        {
            sketchPanel = panel;

            // Build Ink 2 Sketch map
            base.mIdToSubstroke = new Dictionary<int, Substroke>();
            foreach (Substroke sub in sketchPanel.InkSketch.Sketch.Substrokes)
            {
                Microsoft.Ink.Stroke iStroke = sketchPanel.InkSketch.GetInkStrokeBySubstrokeId(sub.XmlAttrs.Id);
                base.mIdToSubstroke.Add(iStroke.Id, sub);
            }

            base.sketch = sketchPanel.InkSketch.Sketch;
            base.sketchInk = sketchPanel.InkPicture as Labeler.mInkPicture;
        }

        /// <summary>
        /// Method stub
        /// </summary>
        override public void ThickenLabel(Microsoft.Ink.Strokes newSelection)
        {
            // Method stub
        }

        /// <summary>
        /// Method stub
        /// </summary>
        override public void UnThickenLabel(Microsoft.Ink.Strokes previousSelection)
        {
            // Method stub
        }
    }
}
