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
    /// <summary>
    /// Delegate for replacing names. Returns dict of old name keys to new name values
    /// </summary>
    /// <param name="?"></param>
    public delegate void ReplaceNamesEventHandler(Dictionary<string, string> newNameDict);

    /// <summary>
    /// Interaction logic for ReplaceNamesDialog.xaml
    /// </summary>
    public partial class ReplaceNamesDialog : Window
    {
        // List of our current text entries
        private List<TextBox> textBoxes;
        private bool debug = false;

        public event ReplaceNamesEventHandler ReplaceNames;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="names">List of input and output names</param>
        public ReplaceNamesDialog(List<string> names)
        {
            InitializeComponent();

            // Create all of our text inputs
            int row = 0;
            textBoxes = new List<TextBox>();
            foreach (string s in names)
            {
                // Create our label
                RowDefinition newRow = new RowDefinition();
                newRow.Height = new GridLength(40);
                Grid.RowDefinitions.Add(newRow);
                Label nameLabel = new Label();
                nameLabel.Content = s;
                Grid.SetRow(nameLabel, row);
                Grid.SetColumn(nameLabel, 0);
                Grid.Children.Add(nameLabel);

                // Create our TextBox
                TextBox textBox = new TextBox();
                textBox.Name = s;
                textBox.Text = s;
                textBox.Margin = new Thickness(5);
                textBox.IsReadOnly = false;
                textBoxes.Add(textBox);
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, 1);
                Grid.Children.Add(textBox);
                ++row;
            }
        }

        /// <summary>
        /// Determines which names have been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> newNameDict = new Dictionary<string,string>();
            List<string> allNames = new List<string>();

            // Add all changed values (not null) to dictionary
            foreach (TextBox textBox in textBoxes)
            {
                
                // If the name has been changed, add it to our dictionary and update
                // all names
                if (textBox.Text != textBox.Name && textBox.Text != "")
                {
                    if (debug) Console.WriteLine("TextBox " + textBox.Name + " Text: " + textBox.Text);
                    newNameDict.Add(textBox.Name, textBox.Text);
                    if (!allNames.Contains(textBox.Text))
                        allNames.Add(textBox.Text);
                    else
                    {
                        MessageBox.Show("All names must be unique!"); // Cannot set names
                        return;
                    }
                }
            }

            // If the dictionary is not null, call an event to replace the names
            if (ReplaceNames != null && newNameDict != null)
                ReplaceNames(newNameDict);
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
