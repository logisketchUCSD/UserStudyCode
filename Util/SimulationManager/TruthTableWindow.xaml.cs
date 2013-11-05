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

namespace SimulationManager
{
    #region Events

    // An event to enable simulation of highlighted entries
    public delegate void RowHighlightEventHandler(Dictionary<string, int> inputs);

    // An event to enable highlighting of corresponding input/output
    public delegate void HighlightEventHandler(string name);

    // An event to enable unhighlighting of corresponding input/output
    public delegate void UnhighlightEventHandler(string name);

    // An event to enable relabeling of input and outputs
    public delegate void RelabelStrokesEventHandler(Dictionary<string,string> newNameDict);

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TruthTableWindow : Window
    {

        #region Internals

        ///<summary>
        /// List on input and output names
        ///</summary>
        private List<string> Headers;

        /// <summary>
        /// Dictionary of input and output strings
        /// </summary>
        private List<List<int>> Rows;

        /// <summary>
        /// Number of input entries
        /// </summary>
        private int numInputs;

        /// <summary>
        /// Number of output entries
        /// </summary>
        private int numOutputs;

        /// <summary>
        /// Timer for hovering and to trigger simulation and timer interval
        /// </summary>
        private System.Windows.Forms.Timer rowHoverTimer;
        private const int HOVER_INTERVAL = 500;

        /// <summary>
        /// Whether or not to simulate the row that is highlighted
        /// </summary>
        private bool simulate;

        /// <summary>
        /// Current inputs to simulate for when timer goes off
        /// </summary>
        private Dictionary<string, int> currInputs;

        /// <summary>
        /// Our event for highlight simulation
        /// </summary>
        public event RowHighlightEventHandler SimulateRow;

        /// <summary>
        /// Our event for highlighting input/output strokes
        /// </summary>
        public event HighlightEventHandler Highlight;

        /// <summary>
        /// Our event for unhighlighting input/output strokes
        /// </summary>
        public event UnhighlightEventHandler UnHighlight;

        /// <summary>
        /// Our event for renaming
        /// </summary>
        public event RelabelStrokesEventHandler RelabelStrokes;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="rows"></param>
        public TruthTableWindow(List<string> headers, List<List<int>> rows, int inputs)
        {
            InitializeComponent();

            // Set table properties
            this.Headers = headers;
            this.Rows = rows;
            this.numInputs = inputs;
            this.numOutputs = headers.Count - inputs;
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

            // Create truth table and hover timer
            CreateTruthTable();
            simulate = true;
            this.rowHoverTimer = new System.Windows.Forms.Timer();
            this.rowHoverTimer.Interval = HOVER_INTERVAL;
            this.rowHoverTimer.Tick += new EventHandler(rowHoverTimer_Tick);
        }


        /// <summary>
        /// Enters the entry into the window menu
        /// </summary>
        private void CreateTruthTable()
        {
            TruthTable.FontFamily = new FontFamily("Verdana");
            TruthTable.CellSpacing = 0;
            TableRowGroup tableRowGroup = new TableRowGroup();
            TruthTable.RowGroups.Add(tableRowGroup);

            int index = 0;

            // Create the Header Row
            TableRow HeaderRow = new TableRow();
            foreach (string label in Headers)
            {
                index++;
                // Add a column with the name of the input
                TableColumn Column = new TableColumn();
                Column.Width = new GridLength((FlowDocReader.ActualWidth)/Headers.Count);
                if (FlowDocReader.ActualWidth > 50)
                    Column.Width = new GridLength((FlowDocReader.ActualWidth-50) / Headers.Count);
                Column.Name = label;
                TruthTable.Columns.Add(Column);
                TableCell headerCell = new TableCell(new Paragraph(new Run(label)));
                headerCell.TextAlignment = TextAlignment.Center;

                // Color input and output objects differently to disinguish
                if (index > numInputs)
                    headerCell.Foreground = System.Windows.Media.Brushes.IndianRed;
                else
                    headerCell.Foreground = System.Windows.Media.Brushes.Navy;
                headerCell.BorderThickness = new Thickness(0, 0, 0, 1);
                headerCell.BorderBrush = System.Windows.Media.Brushes.Black;
                HeaderRow.Cells.Add(headerCell);
                
                // Hook into events
                headerCell.StylusEnter += new StylusEventHandler(header_StylusEnter);
                headerCell.StylusLeave += new StylusEventHandler(header_StylusLeave);
                headerCell.StylusDown += new StylusDownEventHandler(header_StylusDown);
            }

            TruthTable.RowGroups[0].Rows.Add(HeaderRow);
        
            // Create the rest of the table
            foreach (List<int> row in Rows)
            {
                TableRow tableRow = new TableRow();
                int count = 1;
                foreach (int i in row)
                {
                    TableCell intCell = new TableCell(new Paragraph(new Run(i.ToString())));
                    intCell.TextAlignment = TextAlignment.Center;

                    // Create a border between inputs and outputs
                    intCell.BorderThickness = new Thickness(0, 0, 0, 0);
                    if ( count == numInputs)
                        intCell.BorderThickness = new Thickness(0, 0, 1, 0);
                    intCell.BorderBrush = System.Windows.Media.Brushes.Black;

                    tableRow.Cells.Add(intCell);
                    count++;
                }
                TruthTable.RowGroups[0].Rows.Add(tableRow);

                // Hook into row events for highlight simulation
                tableRow.StylusEnter += new StylusEventHandler(Row_StylusEnter);
                tableRow.StylusLeave += new StylusEventHandler(Row_StylusLeave);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Highlights the row the pen is above and sets current inputs for simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_StylusEnter(object sender, StylusEventArgs e)
        {
            rowHoverTimer.Start();

            // Highlight rows
            TableRow row = (TableRow)e.Source;
            row.Background = System.Windows.Media.Brushes.SlateGray;
            row.Foreground = System.Windows.Media.Brushes.White;
            
            // Check if we are simulating highlighted inputs
            if (!simulate)
                return;

            // Create the list of inputs
            Dictionary<string, int> inputs = new Dictionary<string, int>();
            for (int i = 0; i < numInputs; i++)
            {
                Paragraph para = (Paragraph)row.Cells[i].Blocks.LastBlock;
                Run run = (Run)para.Inlines.FirstInline;
                inputs.Add(TruthTable.Columns[i].Name, System.Convert.ToInt32(run.Text));
            }
            currInputs = inputs;
        }

        /// <summary>
        /// Unhighlights a row when the pen leaves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_StylusLeave(object sender, StylusEventArgs e)
        {
            rowHoverTimer.Stop();

            // Unhighlight the row
            TableRow row = (TableRow)e.Source;
            row.Background = System.Windows.Media.Brushes.Transparent;
            row.Foreground = System.Windows.Media.Brushes.Black;
        }

        /// <summary>
        /// Highlights the corresponding strokes of the input/output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void header_StylusEnter(object sender, StylusEventArgs e)
        {
            TableCell cell = (TableCell)e.Source;
            cell.Background = cell.Foreground;
            cell.Foreground = Brushes.White;

            if (Highlight != null)
            {
                Paragraph para = (Paragraph)cell.Blocks.LastBlock;
                Run run = (Run)para.Inlines.FirstInline;
                Highlight(run.Text);
            }
        }

        /// <summary>
        /// Unhighlights the corresponding strokes of the input/output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void header_StylusLeave(object sender, StylusEventArgs e)
        {
            TableCell cell = (TableCell)e.Source;
            cell.Foreground = cell.Background;
            cell.Background = Brushes.Transparent;

            if (UnHighlight != null)
            {
                Paragraph para = (Paragraph)cell.Blocks.LastBlock;
                Run run = (Run)para.Inlines.FirstInline;
                UnHighlight(run.Text);
            }
        }


        /// <summary>
        /// Brings up an editing box for recognized input/output names
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void header_StylusDown(object sender, StylusDownEventArgs e)
        {
            ReplaceNamesDialog replaceNamesDialog = new ReplaceNamesDialog(this.Headers);
            replaceNamesDialog.Show();
            replaceNamesDialog.ReplaceNames += new ReplaceNamesEventHandler(replaceNamesDialog_ReplaceNames);

        }

        /// <summary>
        /// Calls the event to simulate the inputs on the sketch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rowHoverTimer_Tick(object sender, EventArgs e)
        {
            rowHoverTimer.Stop();

            if (simulate && SimulateRow!=null)
                SimulateRow(currInputs);
        }

        /// <summary>
        /// Updates whether or not we are simulating highlighted rows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimulateCheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox simulateBox = (CheckBox)e.Source;
            simulate = (bool)simulateBox.IsChecked;
            System.Console.WriteLine(simulate);
        }


        /// <summary>
        /// Allows users to enter a string of inputs to simulate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Clicked(object sender, RoutedEventArgs e)
        {
            string values = InputString.Text;
            if (values.Count() != numInputs)
            {
                MessageBox.Show("Number of inputs does not match string length.");
                return;
            }

            // Go through our string and enter values into our input dictionary
            Dictionary<string, int> newInputs = new Dictionary<string,int>();
            for (int index = 0; index < numInputs; index++)
            {
                // Convert our char into the int represented
                int value = System.Convert.ToInt32(values[index].ToString());

                if (value != 0 && value != 1)
                {
                    MessageBox.Show("Inputs not valid.");
                    return;
                }
                string rowName = TruthTable.Columns[index].Name;
                newInputs.Add(rowName, value);
            }

            // Call the event
            if (SimulateRow != null)
            {
                SimulateRow(newInputs);
            }
        }

        private void EnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OKButton_Clicked(sender, e);
        }

        /// <summary>
        /// Updates the Truth Table headers and also alerts the dictionaries in
        /// SimulationManager
        /// </summary>
        /// <param name="newNameDict">Dictionary of old name keys to new name values</param>
        private void replaceNamesDialog_ReplaceNames(Dictionary<string, string> newNameDict)
        {
            int index = 0;
            foreach (TableColumn col in TruthTable.Columns)
            {
                // If the name has been changed, update the paragraph and column name
                if (newNameDict.ContainsKey(col.Name))
                {
                    Block newBlock = new Paragraph(new Run(newNameDict[col.Name]));
                    TruthTable.RowGroups[0].Rows[0].Cells[index].Blocks.Clear();
                    TruthTable.RowGroups[0].Rows[0].Cells[index].Blocks.Add(newBlock);
                    col.Name = newNameDict[col.Name];
                    this.Headers[index] = col.Name;
                }
                index++;


            }

            RelabelStrokes(newNameDict);
        }

        private void ResizeWindow(object sender, RoutedEventArgs e)
        {
            FlowDocReader.Width = Grid.ActualWidth;
            FlowDocReader.Height = Math.Abs(Grid.ActualHeight - 55);
            
            foreach (TableColumn Column in TruthTable.Columns)
            {
                Column.Width = new GridLength((FlowDocReader.ActualWidth) / Headers.Count);
                if (FlowDocReader.ActualWidth > 50)
                    Column.Width = new GridLength((FlowDocReader.ActualWidth - 50) / Headers.Count);
            }
        }

        #endregion

    }
}
