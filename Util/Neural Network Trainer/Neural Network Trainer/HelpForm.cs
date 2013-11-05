using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Neural_Network_Trainer
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();

            labelFileFormat.Text = "Required Training File Format (Input files):\n"
                + "  -Each input file should represent the training data from one user,\n"
                + "       the first 2 characters in the file will be the userNumber, and _T will\n"
                + "       indicate it was drawn on a tablet, whereas _P is the paper-based Wacom platform\n"
                + "       e.g. featureFile = 03data_T.txt --> userName = 03_T --> Network = Network03_T.nn\n"
                + "  -You can select multiple files when training\n" 
                + "  -The program concatenates all input files necessary for training\n"
                + "  -Plain Text File\n"
                + "  -First Line contains the number of inputs (features), e.g. 23\n"
                + "  -Second Line contains the number of outputs (classes), e.g. 3\n"
                + "  -Lines 3 --> End contain feature values first (comma separated), \n"
                + "       followed by class values (0 or 1, also comma separated)\n"
                + "  -Example with 4 features and 3 classes:\n"
                + "     4\n"
                + "     3\n"
                + "     1.2, 2.8, 9.7, -0.3, 0, 0, 1\n"
                + "     0.1, 4.5, 8.0, -0.8, 1, 0, 0\n"
                + "     -0.4, 1.1, 6.3, 0.1, 0, 1, 0\n\n\n";

            labelFileFormat.Text += "Output File/Directory:"
                + "  -If doing a single network, you must specify the full filename\n"
                + "  -If doing LOOCV, select the directory and the program will create \n"
                + "       a network for each 'user', depicted by the different input files.\n\n\n";

            labelFileFormat.Text += "Network Settings:\n"
                + "  -Layer Structure: Specifies how many nodes you want in each layer (excludes input layer)\n"
                + "       You must have the same number of nodes in the last layer as you have outputs (classes > 2)\n"
                + "       Example: You have 4 features, 3 classes, and want 2 hidden nodes, Layer Structure = 2,3\n"
                + "  -Training Epochs: how many iterations of training to run, 1000 is usually enough\n"
                + "  -Learning Rate: Range 0.0-1.0, recommend leaving close to 0.3\n"
                + "  -Momentum: Range 0.0-1.0, recommend leaving close to 0.3\n"
                + "  -Learning Method: Recommend Back Propagation for most tasks, Perceptron MUST have only 1 layer\n"
                + "  -Activation Function: Recommend Sigmoid\n"
                + "  -Domain File: this is a .dom file which has already been created. Technically this is not needed,\n"
                + "       but is nice information to have associated with the network. You should be able to find them\n"
                + "       in the Trunk/Data/Domains directory, or you can create your own using Trunk/Util/CreateDomain";
        }
    }
}