using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CircuitSimLib;

namespace CleanCircuitWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CleanWindow : Window
    {

        #region Internals

        private SketchPanelLib.SketchPanel sketchpanel;

        private Canvas cleanPanel;

        private int maxGeneration;

        private int maxChildren;

        #endregion

        #region Constructors

        public CleanWindow(ref Canvas cleanCanvas, ref SketchPanelLib.SketchPanel sketchpanel)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                // Log error
                System.Console.WriteLine(ex.InnerException.Message);
                //System.Console.WriteLine(ex.ListTrace);
            }

            // Set up clean panel
            this.sketchpanel = sketchpanel;
            this.cleanPanel = cleanCanvas;
            this.cleanPanel.Height = cleanDock.ActualHeight;
            this.cleanPanel.Width = cleanDock.ActualWidth;
            if (!cleanDock.Children.Contains(this.cleanPanel))
                cleanDock.Children.Add(this.cleanPanel);
            initializeGraph();
        }

        public CleanWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                // Log error
                System.Console.WriteLine(ex.InnerException.Message);
                //System.Console.WriteLine(ex.ListTrace);
            }

            // Set up clean panel
            this.sketchpanel = new SketchPanelLib.SketchPanel(new CommandManagement.CommandManager());
            this.cleanPanel = new Canvas();
            this.cleanPanel.Height = cleanDock.ActualHeight;
            this.cleanPanel.Width = cleanDock.ActualWidth;
            if (!cleanDock.Children.Contains(this.cleanPanel))
                cleanDock.Children.Add(this.cleanPanel);
        }

        public void initializeGraph()
        {
            int maxGen = Int32.MinValue;
            int maxChild = Int32.MinValue;
            foreach (CircuitElement e in sketchpanel.Circuit.GlobalOutputs)
            {
                KeyValuePair<int, int> pair = calculateGraphValues(e);
                if (pair.Key > maxGen)
                {
                    maxGen = pair.Key;
                }
                maxChild += pair.Value;
            }
            maxChild = Math.Max(maxChild, sketchpanel.Circuit.GlobalOutputs.Count);
            maxChildren = maxChild;
            maxGeneration = maxGen;
        }
        public KeyValuePair<int,int> calculateGraphValues(CircuitSimLib.CircuitElement e)
        {
            if(e.Inputs==null||e.Inputs.Count==0)
            {
                return new KeyValuePair<int,int>(0,1);
            }

            int maxChild = e.Inputs.Count;
            int nextGenChild = Int32.MinValue;
            int generations = Int32.MinValue;

            foreach(CircuitSimLib.CircuitElement child in e.Inputs.Keys)
            {
                KeyValuePair<int,int> pair = calculateGraphValues(child);
                if(pair.Value > generations)
                    generations = pair.Value+1;
                nextGenChild += child.Inputs.Count;
            }
            if (nextGenChild > maxChild)
                maxChild = nextGenChild;
            return new KeyValuePair<int,int>(maxChild, generations);
        }

        #endregion

        #region Events
        public void Window_SizeChanged(object sender, RoutedEventArgs e)
        {
            cleanPanel.Height = cleanDock.ActualHeight;
            cleanPanel.Width = cleanDock.ActualWidth;
        }


        public void Window_Closed(object sender, EventArgs e)
        {
            cleanDock.Children.Remove(cleanPanel);
        }

        #endregion 

        #region Getters and Setters

        public DockPanel CleanWindowDock
        {
            get { return cleanDock; }
            set { cleanDock = value; }
        }

        #endregion

    }
}
