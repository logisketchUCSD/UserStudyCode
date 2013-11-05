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
    public class CorrectionToolkit
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
        /// The CommandManager managine Undo/Redo for this tool
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// Constructor connects to given command manager
        /// </summary>
        /// <param name="cm"></param>
        public CorrectionToolkit(CommandManager cm)
        {
            commandManager = cm;
        }

        /*
        /// <summary>
        /// Subscribes to SketchPanel.  Subscibe only when tool is selected.
        /// <see cref="SketchPanelLib.SketchPanelListener.SubscribeToPanel()"/>
        /// </summary>
        public void SubscribeToPanel(SketchPanel parentPanel)
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
         */
    }
}
