using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Utilities;
using Utilities.Neuro;

namespace Neural_Network_Trainer
{
    public partial class MainForm : Form
    {
        NeuralNetwork m_Network;
        NeuralNetworkInfo m_Info;

        string m_Output;
        List<string> m_Source;
        string m_DomainFile;

        BackgroundWorker m_Worker;

        Queue<string> m_QueueForLOOCV;
        string m_CurrentUserForLOOCV;
        Dictionary<string, FeatureSet[]> m_AllFeaturesForLOOCV;

        public MainForm()
        {
            InitializeComponent();
            this.comboBoxActivation.SelectedIndex = 0;
            this.comboBoxLearningMethod.SelectedIndex = 0;

            m_QueueForLOOCV = new Queue<string>();
            m_CurrentUserForLOOCV = "";
            m_AllFeaturesForLOOCV = new Dictionary<string, FeatureSet[]>();

            m_Info = new NeuralNetworkInfo();
            UpdateNetworkInfoFromForm();
            m_Network = new NeuralNetwork(m_Info);

            m_Output = textBoxOutput.Text;
            m_Source = new List<string>();
            m_DomainFile = textBoxDomainFile.Text;

            m_Worker = new BackgroundWorker();
            m_Worker.WorkerSupportsCancellation = true;
            m_Worker.WorkerReportsProgress = true;
            m_Worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            m_Worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            m_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            toolStripStatusLabel1.Text = "Ready";
        }

        private void TrainSingleNetwork()
        {
            List<FeatureSet> featureSets = new List<FeatureSet>(10000);
            Cursor.Current = Cursors.WaitCursor;

            // Read in each source file and create FeatureSets
            for (int i = 0; i < m_Source.Count; i++)
            {
                toolStripStatusLabel1.Text = "Reading Feature File " + i.ToString() + " of " + m_Source.Count.ToString();
                string message = FeatureSet.AddFeatureSetsFromFile(m_Source[i], ref featureSets);
                if (message != "")
                {
                    MessageBox.Show(message);
                    return;
                }
            }

            try
            {
                FeatureSet[] sets = featureSets.ToArray();

                m_Worker.RunWorkerAsync(sets);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void TrainLOOCVNetworks()
        {
            m_AllFeaturesForLOOCV = new Dictionary<string, FeatureSet[]>();
            try
            {
                // Read in each source file and create FeatureSets
                for (int i = 0; i < m_Source.Count; i++)
                {
                    toolStripStatusLabel1.Text = "Reading Feature File " + i.ToString() + " of " + m_Source.Count.ToString();
                    FeatureSet[] sets = FeatureSet.GetFeatureSetsFromFile(m_Source[i]);
                    string str = System.IO.Path.GetFileNameWithoutExtension(m_Source[i]);

                    if (!m_AllFeaturesForLOOCV.ContainsKey(str))
                        m_AllFeaturesForLOOCV.Add(str, sets);

                    if (!m_QueueForLOOCV.Contains(str))
                        m_QueueForLOOCV.Enqueue(str);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            try
            {
                m_Worker.RunWorkerAsync(m_QueueForLOOCV.Dequeue());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (radioButtonSingleNetwork.Checked)
            {
                m_Network.Save(m_Output);

                toolStripStatusLabel1.Text = "Ready";
                Cursor.Current = Cursors.Default;
            }
            else
            {
                string outputFilename = m_Output + "\\Network" + m_CurrentUserForLOOCV + ".nn";
                m_Network.Save(outputFilename);

                if (m_QueueForLOOCV.Count == 0)
                {
                    toolStripStatusLabel1.Text = "Ready";
                    Cursor.Current = Cursors.Default;
                }
                else
                    m_Worker.RunWorkerAsync(m_QueueForLOOCV.Dequeue());
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (radioButtonSingleNetwork.Checked)
                toolStripStatusLabel1.Text = e.ProgressPercentage.ToString() + "% Completed";
            else
            {
                int num = m_Source.Count - m_QueueForLOOCV.Count;
                toolStripStatusLabel1.Text = e.ProgressPercentage.ToString() 
                    + "% Completed of Network " + num.ToString() 
                    + " of " + m_Source.Count.ToString();
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (m_Worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            if (radioButtonSingleNetwork.Checked)
            {
                FeatureSet[] sets = (FeatureSet[])e.Argument;

                if (sets.Length == 0)
                    return;

                m_Info.NumInputs = sets[0].Features.Length;
                m_Info.Type = "All";

                m_Network = new NeuralNetwork(m_Info);

                m_Network.TrainBackground(sets, m_Worker);
            }
            else
            {
                string userKey = (string)e.Argument;

                List<FeatureSet> sets = new List<FeatureSet>(10000);
                foreach (KeyValuePair<string, FeatureSet[]> set in m_AllFeaturesForLOOCV)
                {
                    if (set.Key != userKey)
                    {
                        foreach (FeatureSet values in set.Value)
                            sets.Add(values);
                    }
                }

                if (sets.Count == 0)
                    return;

                m_Info.NumInputs = sets[0].Features.Length;
                m_Info.Type = "UserHoldout";
                int n = 0;
                string userNum = userKey.Substring(n, 2);

                int num;
                while (!int.TryParse(userNum, out num) && n < userKey.Length - 2)
                {
                    n++;
                    userNum = userKey.Substring(n, 2);
                }
                string platform = "";
                string x = userKey.Substring(n + 2, 2);
                if (x == "_P" || x == "_T")
                    platform = x;
                m_CurrentUserForLOOCV = userNum + platform;
                m_Info.UserName = userNum;
                m_Info.UserID = Guid.NewGuid();
                NeuralNetworkInfo info = new NeuralNetworkInfo(m_Info);

                m_Network = new NeuralNetwork(m_Info);

                m_Network.TrainBackground(sets.ToArray(), m_Worker);
            }
        }

        private void buttonTrain_Click(object sender, EventArgs e)
        {
            if (m_Worker.IsBusy)
            {
                MessageBox.Show("Nope. Can't do that!");
                return;
            }

            UpdateNetworkInfoFromForm();

            if (radioButtonSingleNetwork.Checked)
                TrainSingleNetwork();
            else
                TrainLOOCVNetworks();
        }

        private void UpdateNetworkInfoFromForm()
        {
            string[] layersStr = textBoxLayers.Text.Split(",".ToCharArray());
            int[] layers = new int[layersStr.Length];
            try
            {
                for (int i = 0; i < layersStr.Length; i++)
                    layers[i] = Convert.ToInt32(layersStr[i]);

                m_Info.Layers = layers;
            }
            catch
            {
                MessageBox.Show("Unable to convert Layer Structure to integer array.");
                textBoxLayers.Focus();
            }

            try
            {
                int epochs = Convert.ToInt32(textBoxEpochs.Text);

                m_Info.NumTrainingEpochs = epochs;
            }
            catch
            {
                MessageBox.Show("Unable to convert Training Epochs to integer.");
                textBoxEpochs.Focus();
            }

            try
            {
                double LR = Convert.ToDouble(textBoxLearningRate.Text);

                m_Info.LearningRate = LR;
            }
            catch
            {
                MessageBox.Show("Unable to convert Learning Rate to number.");
                textBoxLearningRate.Focus();
            }

            try
            {
                double momentum = Convert.ToDouble(textBoxMomentum.Text);

                m_Info.Momentum = momentum;
            }
            catch
            {
                MessageBox.Show("Unable to convert Momentum to number.");
                textBoxMomentum.Focus();
            }

            try
            {
                if (textBoxDomainFile.Text != "")
                    m_Info.Domain = new Domain(textBoxDomainFile.Text);
            }
            catch
            {
                MessageBox.Show("Unable to load the selected Domain File");
                textBoxDomainFile.Focus();
            }

            if (comboBoxActivation.Text == "Sigmoid")
                m_Info.ActivationFunction = new SigmoidFunction();
            else if (comboBoxActivation.Text == "Threshold")
                m_Info.ActivationFunction = new ThresholdFunction();
            else if (comboBoxActivation.Text == "Bipolar Sigmoid")
                m_Info.ActivationFunction = new BipolarSigmoidFunction();
            else
                MessageBox.Show("No Valid Activation Function is Selected");

            if (comboBoxLearningMethod.Text == "Back-Propagation")
                m_Info.Teacher = "BackPropagation";
            else if (comboBoxLearningMethod.Text == "Perceptron")
                m_Info.Teacher = "Perceptron";
            else if (comboBoxLearningMethod.Text == "Delta Rule")
                m_Info.Teacher = "DeltaRule";
            else
                MessageBox.Show("No Valid Learning Method is Selected");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            bool successful;
            if (radioButtonLOOCV.Checked)
            {
                m_Output = General.SelectDirectory(out successful, "Select Directory for trained networks.");
                if (!successful)
                    return;

                textBoxOutput.Text = m_Output;
            }
            else
            {
                m_Output = General.SelectSaveFile(out successful, "File to save network as", "Neural Network (*.nn)|*.nn");
                if (!successful)
                    return;

                textBoxOutput.Text = m_Output;
            }
        }

        private void buttonBrowseSourceFiles_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> sourceFiles = General.SelectOpenFiles(out successful, "Select Source Files", "Text (*.txt)|*.txt");
            if (!successful)
                return;
            
            textBoxSourceFiles.Text = "";
            for (int i = 0; i < sourceFiles.Count; i++)
            {
                textBoxSourceFiles.Text += sourceFiles[i];
                if (i < sourceFiles.Count - 1)
                     textBoxSourceFiles.Text += ",";
            }
        }

        private void buttonDomainFile_Click(object sender, EventArgs e)
        {
            bool successful;
            m_DomainFile = General.SelectOpenFile(out successful, "Domain file to tie to network", "Domain Files (*.dom)|*.dom");
            if (!successful)
                return;

            textBoxDomainFile.Text = m_DomainFile;
        }

        private void textBoxLayers_Leave(object sender, EventArgs e)
        {
            UpdateNetworkInfoFromForm();
        }

        private void textBoxEpochs_Leave(object sender, EventArgs e)
        {
            UpdateNetworkInfoFromForm();
        }

        private void textBoxLearningRate_Leave(object sender, EventArgs e)
        {
            UpdateNetworkInfoFromForm();
        }

        private void textBoxMomentum_Leave(object sender, EventArgs e)
        {
            UpdateNetworkInfoFromForm();
        }

        private void textBoxOutput_TextChanged(object sender, EventArgs e)
        {
            m_Output = textBoxOutput.Text;
        }

        private void textBoxSourceFiles_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBoxSourceFiles.Text == "")
                    m_Source.Clear();
                else
                    m_Source = new List<string>(textBoxSourceFiles.Text.Split(",".ToCharArray()));
            }
            catch
            {
                MessageBox.Show("Unable to convert entry to a list of filenames.\n Make sure they are comma separated.");
                textBoxSourceFiles.Focus();
                return;
            }
        }

        private void radioButtonLOOCV_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonLOOCV.Checked)
            {
                labelOutput.Text = "Output Directory: ";
                labelSource.Text = "Source Files: ";
            }
        }

        private void radioButtonSingleNetwork_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSingleNetwork.Checked)
            {
                labelOutput.Text = "Output File: ";
                labelSource.Text = "Source File: ";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_Worker.IsBusy)
            {
                m_Worker.CancelAsync();
                m_QueueForLOOCV.Clear();
                MessageBox.Show("Operation will terminate once the current network has been trained.");
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            // Get the trained networks
            bool successful;
            List<string> networkFiles = General.SelectOpenFiles(out successful, "Select the trained networks to use.",
                "Trained Networks (*.nn)|*.nn");
            if (!successful)
            {
                MessageBox.Show("Unable to get network filenames");
                return;
            }

            // Load all the networks from file
            Dictionary<string, NeuralNetwork> networks = new Dictionary<string, NeuralNetwork>(networkFiles.Count);
            foreach (string file in networkFiles)
            {
                string nameShort = Path.GetFileNameWithoutExtension(file);
                int start = nameShort.IndexOf("Network") + "Network".Length;
                string username = nameShort.Substring(start);
                NeuralNetwork net = NeuralNetwork.LoadNetworkFromFile(file);
                if (!networks.ContainsKey(username))
                    networks.Add(username, net);
                else
                    MessageBox.Show("Unable to load network " + nameShort);
            }

            // Get the filename to save results to
            bool saveFileSuccess;
            string saveFilename = General.SelectSaveFile(out saveFileSuccess, "File to save results to", "Text (*.txt)|*.txt");
            if (!saveFileSuccess)
            {
                MessageBox.Show("Unable to select file to save the results to");
                return;
            }

            // Run each file through the networks
            Dictionary<string, NeuralNetworkResults> allResults = new Dictionary<string, NeuralNetworkResults>(networks.Count);
            foreach (string source in m_Source)
            {
                string nameShort = Path.GetFileNameWithoutExtension(source);
                int start = nameShort.IndexOf("values") + "values".Length + 1;
                string user = nameShort.Substring(start);
                if (networks.ContainsKey(user))
                {
                    NeuralNetwork net = networks[user];
                    FeatureSet[] set = FeatureSet.GetFeatureSetsFromFile(source);
                    bool success;
                    NeuralNetworkResults results = net.Evaluate(set, "", out success);
                    if (success)
                    {
                        if (!allResults.ContainsKey(user))
                            allResults.Add(user, results);
                        else
                            MessageBox.Show("Already added results for user " + user);
                    }
                    else
                        MessageBox.Show("Unable to evaluate with user " + user);
                }
                else
                    MessageBox.Show("User: " + user + " does not have a definition in 'networks'");
            }

            StreamWriter writer = new StreamWriter(saveFilename);
            foreach (KeyValuePair<string, NeuralNetworkResults> entry in allResults)
            {
                int[][] matrix = entry.Value.ConfusionMatrix;
                writer.WriteLine(entry.Key + "," + entry.Value.Accuracy.ToString("#0.000") + ","
                    + matrix[0][0] + "," + matrix[0][1] + "," + matrix[1][0] + "," + matrix[1][1]);
                /*writer.WriteLine(entry.Key + "," + entry.Value.Accuracy.ToString("#0.000") + ",Confusion Matrix,,Classified As");
                writer.WriteLine(",,,,Different Shape, Same Shape");

                writer.WriteLine(",,Should Be,Different Shape,{0},{1}", matrix[0][0], matrix[0][1]);
                writer.WriteLine(",,,Same Shape,{0},{1}", matrix[1][0], matrix[1][1]);

                writer.WriteLine();*/
            }
            writer.Close();
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            HelpForm form = new HelpForm();
            form.Show();
        }
    }
}