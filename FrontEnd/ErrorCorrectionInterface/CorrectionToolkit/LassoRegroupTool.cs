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
    public class LassoRegroupTool : SketchPanelSubscriber
    {
        #region Internals

        /// <summary>
        /// The CommandManager managine Undo/Redo for this tool
        /// </summary>
        private CommandManager commandManager;

        #endregion

        #region Constructors and Lifecycle Methods

        /// <summary>
        /// Constructor.  Hooks into CommandManager cm.
        /// </summary>
        /// <param name="cm">the CommandManager managing Undo/Redo for this tool</param>
        public LassoRegroupTool(CommandManager cm)
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
        public LassoRegroupTool(SketchPanel parentPanel, CommandManager cm)
            : base()
        {
            commandManager = cm;
            init();
            SubscribeToPanel(parentPanel);
        }*/

        /// <summary>
        /// 
        /// </summary>
        private void init()
        {
            //DomainInfo domain = FeedbackMechanism.LoadDomainInfo(FilenameConstants.DefaultCircuitDomainFilePath);
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

            //init();

            // Put panel in Select Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Select;
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

            // HACK Clear selection
            sketchPanel.InkPicture.Selection = new Ink().CreateStrokes();

            // Put panel in Ink Mode
            sketchPanel.EditingMode = InkOverlayEditingMode.Ink;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Should perform the regrouping and then deselect
        /// <see cref="Labeler.LabelerPanel.sketchInk_SelectionChanged()"/>
        /// </summary>
        private void InkPicture_SelectionChanged(object sender, EventArgs e)
        {
            if (sketchPanel.InkPicture.Selection.Count > 0)
            {
                // Attempt to color all Ink strokes
                foreach (Microsoft.Ink.Stroke iStroke in sketchPanel.InkPicture.Selection)
                {
                    // Get corresponding label
                    Substroke sStroke = sketchPanel.InkSketch.GetSketchSubstrokeByInkId(iStroke.Id);
                    iStroke.DrawingAttributes.Color = System.Drawing.Color.Green;
                }

                // Request repaint
                sketchPanel.InkPicture.Invalidate();
                // HACK Clear selection
                sketchPanel.InkPicture.Selection = new Ink().CreateStrokes();
            }
        }

        #endregion
    }
}
