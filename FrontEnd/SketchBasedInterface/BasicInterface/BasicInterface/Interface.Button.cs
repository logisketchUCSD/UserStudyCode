using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Documents;

namespace BasicInterface
{
     public partial class Interface
     {

         bool buttonSelect = false;
         bool buttonTouchSelect = false;

         #region global constants

         #endregion

         #region Button Select

         void inkCanvas_KeyDown2(object sender, System.Windows.Input.KeyEventArgs e)
         {
             //System.Windows.MessageBox.Show(e.Key.ToString());

             if (e.Key.ToString().Equals("RightCtrl"))
             {
                 if (!buttonTouchSelect) // Only show this once - or we'll get some error
                 {
                     removeSelectionHandle();
                     showSelectHandle();
                 }
                 buttonTouchSelect = true;  // indicate button touch select is on
                 hoverTime.Enabled = false; // don't allow hover selection
             }

             else if (e.Key.ToString().Equals("F1"))
             {
                 loadCanvas("C:\\Sketch\\SketchBasedInterface\\BasicInterface\\Task1.txt");
             }

             else if (e.Key.ToString().Equals("F2"))
             {
                 loadCanvas("C:\\Sketch\\SketchBasedInterface\\BasicInterface\\Task2.txt");
             }

         }

         private void loadCanvas(string path)
         {
             // task holds the xml string defining the inkcanvas saved
             string task = "";

             // read from the appropriate file, as specified by the path variable
             System.IO.StreamReader sr = new System.IO.StreamReader(path);
             string line;
             line = sr.ReadLine();
             while (line != null)
             {
                 task += line;
                 line = sr.ReadLine();
             }
             sr.Close();

             // Now, interpret the xml...
             System.IO.StringReader stringReader = new System.IO.StringReader(task);
             System.Xml.XmlReader xmlReader = System.Xml.XmlTextReader.Create(
                 stringReader, new System.Xml.XmlReaderSettings());

             // Save all the children of the saved inkCanvas
             InkCanvas temp = new InkCanvas();
             temp = (InkCanvas)(System.Windows.Markup.XamlReader.Load(xmlReader));
             List<UIElement> tempChildren = new List<UIElement>();

             inkCanvas.Children.Clear();
             foreach (UIElement obj in temp.Children)
             {
                 tempChildren.Add(obj);
             }

             // move each saved UIElement from temp to inkCanvas.
             foreach (UIElement obj in tempChildren)
             {
                 temp.Children.Remove(obj);
                 inkCanvas.Children.Add(obj);
             }
             //inkCanvas.Children.Add((InkCanvas)(System.Windows.Markup.XamlReader.Load(xmlReader)));
             //inkCanvas.UpdateLayout();
         }

         private void saveCanvas()
         {
             // Activated using F1
             System.Text.StringBuilder sb = new System.Text.StringBuilder();
             System.IO.TextWriter tw = new System.IO.StringWriter(sb);
             System.Xml.XmlTextWriter xw = new System.Xml.XmlTextWriter(tw);
             xw.Formatting = System.Xml.Formatting.Indented;
             System.Windows.Markup.XamlWriter.Save(inkCanvas, xw);
             xw.Close();

             System.IO.StreamWriter sw = new System.IO.StreamWriter(
                 "C:\\Sketch\\SketchBasedInterface\\BasicInterface\\Task.txt");
             sw.WriteLine(sb.ToString());
             sw.Close();
         }

         void inkCanvas_KeyUp2(object sender, System.Windows.Input.KeyEventArgs e)
         {
             if (e.Key.ToString().Equals("RightCtrl"))
             {
                 buttonTouchSelect = false;
                 removeSelectionHandle();
                 hoverTime.Enabled = true;
                 hoverTime.Stop();
             }
         }

         private void transferSelection()
         {
             ReadOnlyCollection<UIElement> selected = inkCanvas.GetSelectedElements();
             foreach (UIElement obj in selected)
             {
                 currentSelection.Add(obj);
             }
             updateSelection();
         }


         private void buttonTouchSelection(System.Windows.Input.StylusEventArgs e)
         {
             Point pos = e.GetPosition(inkCanvas);
             moveX = pos.X;
             moveY = pos.Y;

             UIElement currentStylusOver = findShape(moveX, moveY);

             // If we are still in the same element, then don't need to do anything
             if (currentStylusOver == null)
             {
                 lastStylusOver = null;
                 firstHover = true;
                 return;
             }

             bool ignore = false;
             if (currentStylusOver.GetType().ToString().Equals("System.Windows.Shapes.Rectangle"))
             {
                 if (((Rectangle)currentStylusOver).DesiredSize.Width == 4 || ((Rectangle)currentStylusOver).Fill == Brushes.Transparent)
                     ignore = true;
             }

             if (firstHover == true)
             {
                 if (ignore)
                     return;
                 double left = InkCanvas.GetLeft(currentStylusOver);
                 double width = InkCanvas.GetRight(currentStylusOver) - left;
                 double middle = left + (width / 2);
                 if (left < moveX && moveX < middle)
                     left_side = true;
                 else
                     left_side = false;
                 firstHover = false;
                 leftCurrent = currentStylusOver;
             }

             if (ignore)
                 return;

             // if the stylus has moved to a different shape, then no way we are going to select.
             // just reset everything and return
             if (leftCurrent.Equals(currentStylusOver) == false)
             {
                 firstHover = true;
                 return;
             }

             // If we are crossing over a button or an item control, then don't do anything
             if (currentStylusOver.GetType().ToString().Equals("System.Windows.Controls.ItemsControl"))
                 return;
             if (currentStylusOver.GetType().ToString().Equals("System.Windows.Controls.Button"))
                 return;

             if (buttonTouchSelect && allowSelection) // shouldSelect is for timing issues
             // allowSelection is for disabling selection while menu is on
             {
                 if (left_side)
                 {
                     double left = InkCanvas.GetLeft(currentStylusOver);
                     double width = InkCanvas.GetRight(currentStylusOver) - left;
                     double middle = left + (width / 2);
                     if (left < moveX && moveX < middle)
                         return;
                     else
                         left_side = false;
                 }
                 else
                 {
                     double left = InkCanvas.GetLeft(currentStylusOver);
                     double width = InkCanvas.GetRight(currentStylusOver) - left;
                     double middle = left + (width / 2);
                     if (left < moveX && moveX < middle)
                         left_side = true;
                     else
                         return;
                 }
                 if (currentSelection.Contains(currentStylusOver))
                     currentSelection.Remove(currentStylusOver);
                 else
                     currentSelection.Add(currentStylusOver);

                 lastStylusOver = currentStylusOver;
                 updateSelection();
             }
         }


         #endregion
    }
}
