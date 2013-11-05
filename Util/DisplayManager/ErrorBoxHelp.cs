using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using CircuitParser;
namespace DisplayManager
{
    public class ErrorBoxHelp
    {
        public ParseError thisError;
        public System.Windows.Rect errShape;
        private System.Windows.Shapes.Rectangle helpBox;
        private SketchPanelLib.SketchPanel panel;

        /// <summary>
        /// Has information for highlighting a parse error on the sketchPanel
        /// </summary>
        /// <param name="error"></param>
        /// <param name="sketchPanel"></param>
        public ErrorBoxHelp (ParseError error, SketchPanelLib.SketchPanel sketchPanel)
        {
            thisError = error;
            panel = sketchPanel;

            errShape = error.Where.Bounds;
            errShape.X -= 10;
            errShape.Y -= 10;
            helpBox = new System.Windows.Shapes.Rectangle();
            
            helpBox.Height = errShape.Height + 20;
            helpBox.Width = errShape.Width + 20;
            helpBox.Fill = Brushes.Yellow;
            helpBox.Opacity = .4;
        }

        /// <summary>
        /// The actual drawing of the highlighted box onto the screen
        /// </summary>
        public void drawBox()
        {
            InkCanvas.SetTop(helpBox, errShape.Y);
            InkCanvas.SetLeft(helpBox, errShape.X);
            // Add rect to Picture
            if (!panel.InkCanvas.Children.Contains(helpBox))
            {
                panel.InkCanvas.Children.Add(helpBox);
            }
            helpBox.Visibility = System.Windows.Visibility.Visible;
        }
        /// <summary>
        /// removes the box
        /// </summary>
        public void undrawBox()
        {
            helpBox.Visibility = System.Windows.Visibility.Hidden;
            panel.InkCanvas.Children.Remove(helpBox);
        }
    }
}
