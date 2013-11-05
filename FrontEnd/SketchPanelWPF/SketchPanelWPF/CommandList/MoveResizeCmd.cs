using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommandManagement;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows;

namespace SketchPanelLib.CommandList
{
    class MoveResizeCmd : Command
    {
        #region Internals

        /// <summary>
        /// SKetch that contains the strokes
        /// </summary>
        private InkToSketchWPF.InkCanvasSketch inkSketch;

        /// <summary>
        /// The strokes that have been resized
        /// </summary>
        private StrokeCollection StoredStrokes;

        /// <summary>
        /// A dictionary of the shapes in the selection to the shape they were broken off of, if any.
        /// </summary>
        private Dictionary<Sketch.Shape, Sketch.Shape> SplitShapes;

        /// <summary>
        /// The old location of the strokes
        /// </summary>
        private Rect OldBounds;

        /// <summary>
        /// The new location of the strokes
        /// </summary>
        private Rect NewBounds;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MoveResizeCmd(ref InkToSketchWPF.InkCanvasSketch sketch, Rect oldBounds, Rect newBounds)
        {
            isUndoable = true;
            inkSketch = sketch;
            OldBounds = oldBounds;
            NewBounds = newBounds;
            SplitShapes = new Dictionary<Sketch.Shape, Sketch.Shape>();

            StoredStrokes = inkSketch.InkCanvas.GetSelectedStrokes();
        }

        /// <summary>
        /// Type of this command, to tell it appart
        /// </summary>
        /// <returns></returns>
        public override string Type()
        {
            return "MoveResize";
        }


        /// <summary>
        /// Resizes a group of strokes
        /// Preconditions: The strokes are in the inkcanvas.
        /// </summary>
        public override bool Execute()
        {
            Resize(NewBounds);
            BreakShapes();

            return StoredStrokes.Count > 0;
        }

        /// <summary>
        /// Reselect the strokes after the inkchanged command has completed.
        /// </summary>
        public void selectStoredStrokes()
        {
            inkSketch.InkCanvas.Select(StoredStrokes);
        }


        /// <summary>
        /// Undos a resize of strokes
        /// Precondition: The strokes are in the Inkcanvas.
        /// </summary>
        public override bool UnExecute()
        {
            UnbreakShapes();
            Resize(OldBounds);
            inkSketch.InkCanvas.Select(StoredStrokes);

            return StoredStrokes.Count > 0;
        }

        /// <summary>
        /// Resizes the ink strokes and updates our data structures.
        /// Preconditions: The strokes are already in the inkcanvas.
        /// </summary>
        /// <param name="NewSize"></param>
        private void Resize(Rect NewSize)
        {
            Rect CurrSize = StoredStrokes.GetBounds();
            System.Windows.Media.Matrix resizeMatrix = new System.Windows.Media.Matrix();
            resizeMatrix.ScaleAt(NewSize.Width / CurrSize.Width, NewSize.Height / CurrSize.Height, CurrSize.X, CurrSize.Y);
            resizeMatrix.Translate(NewSize.X - CurrSize.X, NewSize.Y - CurrSize.Y);
            StoredStrokes.Transform(resizeMatrix, false);
            inkSketch.UpdateInkStrokes(StoredStrokes);
        }

        /// <summary>
        /// Break any shapes which are only partially in this selection
        /// </summary>
        private void BreakShapes()
        {
            foreach (Stroke stroke in StoredStrokes)
            {
                Sketch.Substroke substroke = inkSketch.GetSketchSubstrokeByInk(stroke);
                Sketch.Shape parent = substroke.ParentShape;

                if (parent != null && !SplitShapes.ContainsKey(parent))
                {
                    bool breakShape = false;
                    List<Sketch.Substroke> substrokes = new List<Sketch.Substroke>();

                    foreach (Sketch.Substroke sub in parent.Substrokes)
                    {
                        if (!StoredStrokes.Contains(inkSketch.GetInkStrokeBySubstroke(sub)))
                            breakShape = true;
                        else
                            substrokes.Add(sub);
                    }

                    if (breakShape)
                    {
                        Sketch.Shape newShape = inkSketch.Sketch.BreakOffShape(parent, substrokes);
                        SplitShapes[newShape] = parent;
                    }
                }
            }
        }

        /// <summary>
        /// Heals any shapes which were only partially in this selection
        /// </summary>
        private void UnbreakShapes()
        {
            foreach (Sketch.Shape newParent in SplitShapes.Keys)
                inkSketch.Sketch.mergeShapes(SplitShapes[newParent], newParent);
            SplitShapes.Clear();
        }
    }
}
