using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ConverterXML;
using Sketch;
using Featurefy;
using Cluster;
using NeuralNetAForge;
using ImageRecognizer;
using Microsoft.Ink;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Utilities.Neuro;

namespace Classifier
{
    public partial class Classifier : Form
    {
        #region Member Variables

        #region Sketch Info
        private Sketch.Sketch _Sketch;
        private Microsoft.Ink.InkOverlay _OverlayInk;
        private Graphics g;
        private List<StrokeFeatures> _StrokeFeatures;
        private Dictionary<string, double[][,]> _ClusterFeatures;
        private SketchClusterSet _BestClusters;
        private Dictionary<Guid, BitmapSymbol> _Cluster2Symbol;
        #endregion

        #region Names and Directories
        private string _SketchName;
        private string _SketchDirectory;
        private string _StrokeNetworkName;
        private string _ClusterNetworkName;
        private string _Cluster2NetworkName;
        private string _Cluster3NetworkName;
        private string _StrokeNetworkDirectory;
        private string _ClusterNetworkDirectory;
        private string _StrokeFeatureFilesDirectory;
        private string _ClusterFeatureFilesDirectory;
        private string _StrokeResultsDirectory;
        private string _ClusterResultsDirectory;
        private string _DomainsDirectory;
        private string _ImageDirectory;
        #endregion

        #region Neural Networks
        private NeuralNet _NetworkStrokes_2Class;
        private NeuralNet _NetworkStrokes_3Class;
        private NeuralNet _NetworkStrokes_2x2Class_2;
        private NeuralNet _NetworkCluster_2Class;
        private NeuralNet _NetworkCluster_3Class_1;
        private NeuralNet _NetworkCluster_3Class_2;
        private NeuralNet _NetworkCluster_3Class_3;

        private string _BitString2Way;
        private string _BitString3Way;
        private string _BitString2x2Way_2;

        private int _NumTrainingEpochs = 3000;
        #endregion

        #region Lookup Tables
        private Dictionary<string, DomainInfoClassifier> _DomainInfo;
        private Dictionary<Guid, Microsoft.Ink.Stroke> _Substroke2MStroke;
        private Dictionary<int, Sketch.Substroke> _MStroke2Substroke;
        private Dictionary<int, Color> _MStroke2Color;
        private Dictionary<Guid, string> _StrokeClassifications;
        private Dictionary<Guid, int> _ClusterClassifications;
        private Dictionary<string, Dictionary<int, Sketch.Substroke>> _Index2Substroke;
        private Dictionary<Guid, string> _ImageRecoResult;
        private Dictionary<Guid, Dictionary<string, List<SymbolRank>>> _FullImageRecoResults;
        private List<BitmapSymbol> _ImageDefinitions;
        private Dictionary<string, List<BitmapSymbol>> _PreviousImageDefinitions;
        #endregion

        #region Sizing and Moving Variables
        private float _InkMovedX;
        private float _InkMovedY;
        private float _Scale;
        #endregion

        #endregion


        #region Initialization

        /// <summary>
        /// Constructor - Initializes the form
        /// </summary>
        public Classifier()
        {
            InitializeComponent();
            myInit();
        }

        /// <summary>
        /// Function to initialize the variables
        /// </summary>
        private void myInit()
        {
            _Substroke2MStroke = new Dictionary<Guid, Microsoft.Ink.Stroke>();
            _MStroke2Substroke = new Dictionary<int, Substroke>();
            _MStroke2Color = new Dictionary<int, Color>();
            _StrokeClassifications = new Dictionary<Guid, string>();
            _ClusterClassifications = new Dictionary<Guid, int>();
            _ImageRecoResult = new Dictionary<Guid, string>();
            _FullImageRecoResults = new Dictionary<Guid, Dictionary<string, List<SymbolRank>>>();
            _PreviousImageDefinitions = new Dictionary<string, List<BitmapSymbol>>();
            _DomainInfo = new Dictionary<string, DomainInfoClassifier>();
            _ImageDefinitions = new List<BitmapSymbol>();
            _Index2Substroke = new Dictionary<string, Dictionary<int, Substroke>>();
            _ClusterFeatures = new Dictionary<string,double[][,]>();
            _StrokeFeatures = new List<StrokeFeatures>();
            _Cluster2Symbol = new Dictionary<Guid, BitmapSymbol>();

            _SketchName = "NoName";
            _StrokeNetworkName = "NoName";
            _ClusterNetworkName = "NoName";
            _Cluster2NetworkName = "NoName";
            _Cluster3NetworkName = "NoName";

            _Sketch = new Sketch.Sketch();

            _InkMovedX = new float();
            _InkMovedY = new float();
            _Scale = new float();

            this.inkPanel.Enabled = true;
            _OverlayInk = new Microsoft.Ink.InkOverlay();
            _OverlayInk.AttachedControl = this.inkPanel;
            _OverlayInk.Enabled = false;

            this.g = this.inkPanel.CreateGraphics();

            LoadSettings("..//..//..//Settings.txt");

            bool TwoWayLoaded;
            DomainInfoClassifier TwoWayDomain = LoadDomain(_DomainsDirectory + "\\DefaultDomain2Way.domain.txt", out TwoWayLoaded);
            if (TwoWayLoaded)
                _DomainInfo.Add("2", TwoWayDomain);
            else
                _DomainInfo.Add("2", new DomainInfoClassifier());

            bool ThreeWayLoaded;
            DomainInfoClassifier ThreeWayDomain = LoadDomain(_DomainsDirectory + "\\DefaultDomain3Way.domain.txt", out ThreeWayLoaded);
            if (ThreeWayLoaded)
                _DomainInfo.Add("3", ThreeWayDomain);
            else
                _DomainInfo.Add("3", new DomainInfoClassifier());

        }

        #endregion


        #region File I/O

        /// <summary>
        /// Prompts user to open a settings file containing directories and network structures
        /// </summary>
        private void OpenSettings()
        {
            bool openSuccessful;
            string settingsFile = SelectOpenSettings(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open Settings file, no action taken --- Ready";
                return;
            }

            bool loadSuccessful = LoadSettings(settingsFile);

            if (!loadSuccessful)
                StatusText.Text = "Error in file input, check settings file.";
            else
                StatusText.Text = "Ready";

            this.Refresh();
        }

        /// <summary>
        /// Load default Settings and directories from a text file
        /// </summary>
        /// <param name="filename">Text Settings file</param>
        private bool LoadSettings(string filename)
        {
            StreamReader reader = new StreamReader(filename);

            int NumInputsStroke2way = 0;
            int NumOutputsStroke2way = 0;
            int NumInputsStroke3way = 0;
            int NumOutputsStroke3way = 0;
            int NumInputsStroke2x2way_2 = 0;
            int NumOutputsStroke2x2way_2 = 0;
            int[] LayersStroke2way = new int[1] { 0 };
            int[] LayersStroke3way = new int[1] { 0 };
            int[] LayersStroke2x2way_2 = new int[1] { 0 };
            int NumInputsCluster2way = 0;
            int NumOutputsCluster2way = 0;
            int NumInputsCluster3way_1 = 0;
            int NumOutputsCluster3way_1 = 0;
            int NumInputsCluster3way_2 = 0;
            int NumOutputsCluster3way_2 = 0;
            int NumInputsCluster3way_3 = 0;
            int NumOutputsCluster3way_3 = 0;
            int[] LayersCluster2way = new int[1] { 0 };
            int[] LayersCluster3way_1 = new int[1] { 0 };
            int[] LayersCluster3way_2 = new int[1] { 0 };
            int[] LayersCluster3way_3 = new int[1] { 0 };
            string line;

            while ((line = reader.ReadLine()) != null && line != "")
            {
                #region get and assign line values
                string defn = line.Substring(0, line.IndexOf("=") - 1);
                string value = line.Substring(line.IndexOf("=") + 2);
                List<string> nums;
                switch (defn)
                {
                    case ("_SketchDirectory"):
                        _SketchDirectory = value;
                        break;
                    case ("_StrokeNetworkDirectory"):
                        _StrokeNetworkDirectory = value;
                        break;
                    case ("_ClusterNetworkDirectory"):
                        _ClusterNetworkDirectory = value;
                        break;
                    case ("_StrokeFeatureFilesDirectory"):
                        _StrokeFeatureFilesDirectory = value;
                        break;
                    case ("_ClusterFeatureFilesDirectory"):
                        _ClusterFeatureFilesDirectory = value;
                        break;
                    case ("_StrokeResultsDirectory"):
                        _StrokeResultsDirectory = value;
                        break;
                    case ("_ClusterResultsDirectory"):
                        _ClusterResultsDirectory = value;
                        break;
                    case ("_DomainsDirectory"):
                        _DomainsDirectory = value;
                        break;
                    case ("_ImageDirectory"):
                        _ImageDirectory = value;
                        break;
                    case ("NumInputsStroke2way"):
                        NumInputsStroke2way = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsStroke2way"):
                        NumOutputsStroke2way = Convert.ToInt16(value);
                        break;
                    case ("NumInputsStroke3way"):
                        NumInputsStroke3way = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsStroke3way"):
                        NumOutputsStroke3way = Convert.ToInt16(value);
                        break;
                    case ("NumInputsStroke2x2way_2"):
                        NumInputsStroke2x2way_2 = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsStroke2x2way_2"):
                        NumOutputsStroke2x2way_2 = Convert.ToInt16(value);
                        break;
                    case ("LayersStroke2way"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersStroke2way = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersStroke2way[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("LayersStroke3way"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersStroke3way = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersStroke3way[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("LayersStroke2x2way_2"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersStroke2x2way_2 = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersStroke2x2way_2[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("NumInputsCluster2way"):
                        NumInputsCluster2way = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsCluster2way"):
                        NumOutputsCluster2way = Convert.ToInt16(value);
                        break;
                    case ("NumInputsCluster3way_1"):
                        NumInputsCluster3way_1 = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsCluster3way_1"):
                        NumOutputsCluster3way_1 = Convert.ToInt16(value);
                        break;
                    case ("NumInputsCluster3way_2"):
                        NumInputsCluster3way_2 = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsCluster3way_2"):
                        NumOutputsCluster3way_2 = Convert.ToInt16(value);
                        break;
                    case ("LayersCluster2way"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersCluster2way = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersCluster2way[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("LayersCluster3way_1"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersCluster3way_1 = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersCluster3way_1[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("LayersCluster3way_2"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersCluster3way_2 = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersCluster3way_2[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("_BitString2Way"):
                        _BitString2Way = value;
                        break;
                    case ("_BitString3Way"):
                        _BitString3Way = value;
                        break;
                    case ("_BitString2x2Way_2"):
                        _BitString2x2Way_2 = value;
                        break;
                    case ("##"):
                        break;
                    case ("NumTrainingEpochs"):
                        _NumTrainingEpochs = Convert.ToInt32(value);
                        break;
                    case ("LayersCluster3way_3"):
                        nums = new List<string>(value.Split(" ".ToCharArray()));
                        nums.RemoveAll(IsEmpty);
                        LayersCluster3way_3 = new int[nums.Count];
                        for (int i = 0; i < nums.Count; i++)
                            LayersCluster3way_3[i] = Convert.ToInt16(nums[i]);
                        break;
                    case ("NumInputsCluster3way_3"):
                        NumInputsCluster3way_3 = Convert.ToInt16(value);
                        break;
                    case ("NumOutputsCluster3way_3"):
                        NumOutputsCluster3way_3 = Convert.ToInt16(value);
                        break;
                    default:
                        return false;
                }
                #endregion
            }

            _NetworkStrokes_2Class = new NeuralNet(LayersStroke2way, NumInputsStroke2way, NumOutputsStroke2way);
            _NetworkStrokes_3Class = new NeuralNet(LayersStroke3way, NumInputsStroke3way, NumOutputsStroke3way);
            _NetworkStrokes_2x2Class_2 = new NeuralNet(LayersStroke2x2way_2, NumInputsStroke2x2way_2, NumOutputsStroke2x2way_2);
            _NetworkCluster_2Class = new NeuralNet(LayersCluster2way, NumInputsCluster2way, NumOutputsCluster2way);
            _NetworkCluster_3Class_1 = new NeuralNet(LayersCluster3way_1, NumInputsCluster3way_1, NumOutputsCluster3way_1);
            _NetworkCluster_3Class_2 = new NeuralNet(LayersCluster3way_2, NumInputsCluster3way_2, NumOutputsCluster3way_2);
            _NetworkCluster_3Class_3 = new NeuralNet(LayersCluster3way_3, NumInputsCluster3way_3, NumOutputsCluster3way_3);

            reader.Close();

            return true;
        }

        /// <summary>
        /// Starts the "Open" dialog allowing the user to open an MIT XML file.
        /// </summary>
        private void OpenSketch()
        {
            bool openSuccessful;
            string sketchFile = SelectOpenSketch(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open Sketch, no action taken --- Ready";
                return;
            }

            OpenSketch(sketchFile);
        }

        private void OpenSketch(string sketchFile)
        {
            StatusText.Text = "Loading Sketch...";
            this.Refresh();

            string lastUser = "";
            if (_SketchName == "NoName")
                lastUser = "NoName";
            else
            {
                lastUser = _SketchName.Substring(0, 2);
                lastUser += _SketchName.Substring(_SketchName.LastIndexOf("_") + 1, 1);
            }

            bool loaded = LoadSketch(sketchFile);
            bool differentUserOrPlatform = false;

            if (loaded)
            {
                string currentUser = "";
                if (_SketchName == "NoName")
                    currentUser = "NoName";
                else
                {
                    currentUser = _SketchName.Substring(0, 2);
                    currentUser += _SketchName.Substring(_SketchName.LastIndexOf("_") + 1, 1);
                }

                if (currentUser != lastUser)
                    differentUserOrPlatform = true;
                
                /*string name = System.IO.Path.GetFileNameWithoutExtension(sketchFile);
                string userNum = name.Substring(0, 2);
                string platformShort = name.Substring(name.LastIndexOf("_") + 1, 1);
                if (_SketchName != "")
                {
                    string sUserNum = _SketchName.Substring(0, 2);
                    if (_SketchName == "NoName")
                        differentUserOrPlatform = true;
                    else if (userNum != sUserNum)
                        differentUserOrPlatform = true;
                    else
                    {
                        string sPlatform = _SketchName.Substring(_SketchName.LastIndexOf("_") + 1, 1);
                        if (platformShort != sPlatform)
                            differentUserOrPlatform = true;
                    }
                }*/

                if (differentUserOrPlatform)
                {
                    if (lastUser != "NoName")
                    {
                        if (!_PreviousImageDefinitions.ContainsKey(lastUser))
                            _PreviousImageDefinitions.Add(lastUser, new List<BitmapSymbol>(_ImageDefinitions));
                    }
                    this._ImageDefinitions.Clear();

                    if (_PreviousImageDefinitions.ContainsKey(currentUser))
                        _ImageDefinitions = _PreviousImageDefinitions[currentUser];
                    else
                        LoadImageDefinitions(GetCurrentUserDefinitionFile());
                }


                FillInkOverlay(_Sketch);
                StatusText.Text = "Ready";
                this.inkPanel.Refresh();
                this.Refresh();
            }
            else
            {
                StatusText.Text = "Error: File format must be *.xml, No action taken --- Ready";
                this.Refresh();
            }
        }

        /// <summary>
        /// Loads a valid MIT XML file into the application as a Sketch.
        /// </summary>
        /// <param name="filename">The filename to open</param>
        private bool LoadSketch(string filename)
        {
            string extension = System.IO.Path.GetExtension(filename.ToLower());

            if (extension == ".xml")
            {
                if (_Sketch != null)
                    DisposeDictionaries();

                _Sketch = new ReadXML(filename).Sketch;
                _SketchName = System.IO.Path.GetFileNameWithoutExtension(filename);
                if (_SketchName.Contains(".labeled"))
                    _SketchName = _SketchName.Substring(0, _SketchName.IndexOf(".labeled"));

                string userNumber = _SketchName.Substring(0, 2);
                _StrokeNetworkName = "User" + userNumber + "_holdout";
                if (TwoWay)
                    _ClusterNetworkName = "User" + userNumber + "_2Way_holdout";
                else if (ThreeWay)
                {
                    _ClusterNetworkName = "User" + userNumber + "_3Way_1_holdout";
                    _Cluster2NetworkName = "User" + userNumber + "_3Way_2_holdout";
                    _Cluster3NetworkName = "User" + userNumber + "_3Way_3_holdout";
                }

                this.Text = "Sketch Classifier and Clusterer - " + _SketchName;

                for (int i = 0; i < _Sketch.Substrokes.Length - 1; i++)
                {
                    if (_Sketch.Substrokes[i + 1].XmlAttrs.Time.Value < _Sketch.Substrokes[i].XmlAttrs.Time.Value)
                        System.Windows.Forms.MessageBox.Show("Strokes not in order");
                }

                

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Prompts the user to select the network to open.
        /// </summary>
        private void OpenNetwork()
        {
            bool openSuccessful;
            string networkFile = SelectOpenNetwork(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open network file, no action taken --- Ready";
                return;
            }

            bool loadedSuccessful = LoadNetwork(networkFile);

            if (!loadedSuccessful)
                StatusText.Text = "Unable to select the appropriate network, check FileName. No action taken --- Ready";
            else
                StatusText.Text = "Ready";

            this.Refresh();
        }

        /// <summary>
        /// Loads a Neural network for a given filename
        /// </summary>
        /// <param name="filename">Text file containing weights and structure for Neural net</param>
        private bool LoadNetwork(string filename)
        {
            if (filename.Contains("cluster"))
            {
                if (filename.Contains("2Way"))
                    _NetworkCluster_2Class.LoadNetwork(filename);
                else if (filename.Contains("3Way_1"))
                    _NetworkCluster_3Class_1.LoadNetwork(filename);
                else if (filename.Contains("3Way_2"))
                    _NetworkCluster_3Class_2.LoadNetwork(filename);
                else if (filename.Contains("3Way_3"))
                    _NetworkCluster_3Class_3.LoadNetwork(filename);
                else
                    return false;
            }
            else if (filename.Contains("features"))
            {
                if (filename.Contains("TwoXTwoWay"))
                    _NetworkStrokes_2x2Class_2.LoadNetwork(filename);
                else if (filename.Contains("TwoWay"))
                    _NetworkStrokes_2Class.LoadNetwork(filename);
                else if (filename.Contains("ThreeWay"))
                    _NetworkStrokes_3Class.LoadNetwork(filename);
                else
                    return false;
            }
            else
                return false;

            return true;
        }

        /// <summary>
        /// Open a valid Domain file into the application.
        /// </summary>
        private void OpenDomain()
        {
            bool openSuccessful;
            string domainFile = SelectOpenDomain(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open a domain file, no action taken --- Ready";
                return;
            }

            bool loaded;
            DomainInfoClassifier domainInfo = LoadDomain(domainFile, out loaded);

            if (!loaded)
            {
                StatusText.Text = "Unable to open specified domain file, check file --- Ready";
                return;
            }

            if (domainFile.Contains("_2way"))
                _DomainInfo.Add("2", domainInfo);
            else if (domainFile.Contains("_3way"))
                _DomainInfo.Add("3", domainInfo);
            else if (domainFile.Contains("_2x2way"))
                _DomainInfo.Add("2x2", domainInfo);
            else
            {
                StatusText.Text = "Selected domain could not be applied here, check filename --- Ready";
                return;
            }

            if (loaded && openSuccessful)
                StatusText.Text = "Ready";

            this.Refresh();
        }

        /// <summary>
        /// Load a valid Domain file into the application.
        /// </summary>
        /// <param name="filepath">Filepath of the domain file</param>
        private DomainInfoClassifier LoadDomain(string filename, out bool loaded)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            DomainInfoClassifier domainInfo = new DomainInfoClassifier();
            string line = reader.ReadLine();
            string[] words = line.Split(null);

            // The next line is the domain
            words = line.Split(null);
            domainInfo.AddInfo(words[0], words[1]);

            // The next line is a description (throw away)
            line = reader.ReadLine();

            // Now read the first Domain
            line = reader.ReadLine();

            // Then the rest are labels
            while (line != null && line != "")
            {
                words = line.Split(null);

                string label = words[0];
                int num = int.Parse(words[1]);
                string color = words[2];

                domainInfo.AddLabel(num, label, Color.FromName(color));
                line = reader.ReadLine();
            }

            List<string> labels = domainInfo.GetLabels();
            string[] labelsWithColors = new string[labels.Count];

            for (int i = 0; i < labelsWithColors.Length; i++)
            {
                labelsWithColors[i] = (string)labels[i] + "   (" +
                    domainInfo.GetColor((string)labels[i]).Name + ")";
            }

            reader.Close();

            loaded = true;

            return domainInfo;
        }

        /// <summary>
        /// Loads a list of xml sketches, featurizes them, then writes the features to file
        /// </summary>
        /// <param name="SketchFiles">List of sketch filenames to load</param>
        /// <param name="OutputFilename">Filename to save features to</param>
        private void WriteClusterNNTrainingFiles(List<string> SketchFiles, string OutputFilename)
        {
            StreamWriter writer_1;
            StreamWriter writer_2;
            StreamWriter writer_3;
            if (TwoWay)
            {
                writer_1 = new StreamWriter(OutputFilename, false);
                writer_2 = new StreamWriter(OutputFilename.Replace(".cluster", "_2.cluster"));
                writer_3 = new StreamWriter(OutputFilename.Replace(".cluster", "_3.cluster"));
            }
            else if (ThreeWay)
            {
                writer_1 = new StreamWriter(OutputFilename.Replace(".cluster", "_1.cluster"));
                writer_2 = new StreamWriter(OutputFilename.Replace(".cluster", "_2.cluster"));
                writer_3 = new StreamWriter(OutputFilename.Replace(".cluster", "_3.cluster"));
            }
            else
            {
                writer_1 = new StreamWriter(OutputFilename);
                writer_2 = new StreamWriter(OutputFilename.Replace(".cluster", "_2.cluster"));
                writer_3 = new StreamWriter(OutputFilename.Replace(".cluster", "_3.cluster"));
            }

            int numInputs;
            bool[] useFeatures = GetUseFeaturesCluster(out numInputs);
            int numOutputs = 1;

            writer_1.WriteLine(numInputs.ToString());
            writer_1.WriteLine(numOutputs.ToString());
            if (ThreeWay)
            {
                writer_2.WriteLine(numInputs.ToString());
                writer_2.WriteLine(numOutputs.ToString());

                writer_3.WriteLine(numInputs.ToString());
                writer_3.WriteLine(numOutputs.ToString());
            }

            for (int i = 0; i < SketchFiles.Count; i++)
            {
                bool loaded = LoadSketch(SketchFiles[i]);
                FillInkOverlay(_Sketch);
                ApplyPerfectClassifications();

                StatusText.Text = "Finding Clustering Features for Sketch # "
                    + (i + 1).ToString() + " of " + SketchFiles.Count.ToString();
                this.Refresh();

                List<SketchClusterSet> clusterSets = FeaturizeCurrentSketch_Cluster();
                _BestClusters = clusterSets[0];

                if (TwoWay)
                    WriteClusterFeatures(writer_1, _DomainInfo["2"].GetLabel(1));
                else if (ThreeWay)
                {
                    WriteClusterFeatures(writer_1, _DomainInfo["3"].GetLabel(1));
                    WriteClusterFeatures(writer_2, _DomainInfo["3"].GetLabel(2));
                    WriteClusterFeatures(writer_3, _DomainInfo["3"].GetLabel(0));
                }
                
                Application.DoEvents();

                this.Refresh();
            }

            writer_1.Close();
            writer_2.Close();
            writer_3.Close();
            if (TwoWay)
            {
                System.IO.File.Delete(OutputFilename.Replace(".cluster", "_2.cluster"));
                System.IO.File.Delete(OutputFilename.Replace(".cluster", "_3.cluster"));
            }

            StatusText.Text = "Ready";
        }

        /// <summary>
        /// Write the inter-stroke features from the SketchClusterSet out to file
        /// </summary>
        /// <param name="writer">Already opened StreamWriter</param>
        private void WriteClusterFeatures(System.IO.StreamWriter writer, string label)
        {
            // Features:
            //   0: minDistance
            //   1: timeGap
            //   2: horizontalOverlap
            //   3: verticalOverlap
            //   4: maxDistance
            //   5: centroidDistance
            //   6: Distance 1 ratio
            //   7: Distance 2 ratio
            int num = _ClusterFeatures[label][0].GetLength(0);

            for (int i = 0; i < num; i++)
            {
                for (int j = i + 1; j < num; j++)
                {
                    if (useMinimumDistanceToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][0][i, j].ToString("#0.000000") + " ");
                    if (useTimeGapToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][1][i, j].ToString("#0.000000") + " ");
                    if (useHorizontalOverlapToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][2][i, j].ToString("#0.000000") + " ");
                    if (useVerticalOverlapToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][3][i, j].ToString("#0.000000") + " ");
                    if (useMaximumDistanceToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][4][i, j].ToString("#0.000000") + " ");
                    if (useCentroidDistanceToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][5][i, j].ToString("#0.000000") + " ");
                    if (useDistanceRatio1ToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][6][i, j].ToString("#0.000000") + " ");
                    if (useDistanceRatio2ToolStripMenuItem.Checked)
                        writer.Write(_ClusterFeatures[label][7][i, j].ToString("#0.000000") + " ");


                    if (SameShape(i, j, label) && i != j)
                        writer.WriteLine("1");
                    else
                        writer.WriteLine("0");
                }
            }
        }

        /// <summary>
        /// Writes the Clustering accuracy results to file
        /// </summary>
        /// <param name="results">Clustering results</param>
        private void WriteClusterAccuracies(Dictionary<string, ClusterResults> results, List<string> names, string OutputFilename)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(OutputFilename);
            writer.Write("SketchName,#Expected Clusters,#Correct Clusters,");
            writer.Write("%Ink Correct,%Ink Missing,%Ink Extra,");
            writer.Write("a=100% e=0%,100%>a>=80% e=0%,80%>a>=60% e=0%,60%>a>=40% e=0%,");
            writer.Write("40%>a>=20% e=0%,a=0%,a=100% e>0%,100%>a>=80% e>0%,80%>a>=60% e>0%,");
            writer.Write("60%>a>=40% e>0%,40%>a>=20% e>0%,20%>a>0% e>0%,unaccounted for,");
            writer.Write("Complete,Complete extra,Incomplete best,Incomplete Extra,");
            writer.Write("Unmatching partial,Unmatching completely,Repairable Incompletes");
            writer.WriteLine();

            foreach (string name in names)
            {
                ClusterResults tempResult = results[name];
                int[] percentiles = tempResult.Percentiles;
                int[] categories = tempResult.MachineClusterCategories;
                writer.Write(name + ",");
                writer.Write(tempResult.NumExpectedClusters.ToString()
                    + "," + tempResult.NumPerfectMatches.ToString()
                    + "," + tempResult.CorrectInkAccuracy.ToString("#0.00")
                    + "," + tempResult.MissingInkAccuracy.ToString("#0.00")
                    + "," + tempResult.ExtraInkAccuracy.ToString("#0.00")  );
                writer.Write("," + percentiles[0].ToString()
                    + "," + percentiles[1].ToString()
                    + "," + percentiles[2].ToString()
                    + "," + percentiles[3].ToString()
                    + "," + percentiles[4].ToString()
                    + "," + percentiles[5].ToString()
                    + "," + percentiles[6].ToString()
                    + "," + percentiles[7].ToString()
                    + "," + percentiles[8].ToString()
                    + "," + percentiles[9].ToString()
                    + "," + percentiles[10].ToString()
                    + "," + percentiles[11].ToString()
                    + "," + percentiles[12].ToString()
                    + "," + percentiles[13].ToString()  );
                writer.Write("," + categories[0].ToString()
                    + "," + categories[1].ToString()
                    + "," + categories[2].ToString()
                    + "," + categories[3].ToString()
                    + "," + categories[4].ToString()
                    + "," + categories[5].ToString()
                    + "," + categories[6].ToString()  );
                writer.WriteLine();
            }

            writer.Close();
        }

        /// <summary>
        /// Writes the Clustering accuracy results to file
        /// </summary>
        /// <param name="results">Clustering results</param>
        private void WriteClusterAccuraciesNew(Dictionary<string, ClusteringResultsSketch> results, List<string> names, string OutputFilename)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(OutputFilename);
            writer.WriteLine("Sketch Name, # Shapes, # Perfect Clusters, # Conditional Perfect, # Conditional Perfect (no Matching)," +
                "# Shapes, # Texts, # Connectors, " +
                "# Perfect Shapes, # Perfect Texts, # Perfect Connectors, " +
                "# Cond Perfect Shapes, # Cond Perfect Texts, # Cond Perfect Connectors, " +
                "# Total Errors, # Split, # Merge, " + 
                "# Merged S2S, # Merged S2T, # Merged S2C, " +
                "# Merged T2S, # Merged T2T, # Merged T2C, " +
                "# Merged C2S, # Merged C2T, # Merged C2C, " +
                "# Merged NB2S, # Merged NB2S is only error, " + 
                "# Split Shape, # Split Text, # Split Connector, " +
                "# Extra Clusters, " +
                "% Ink Matching In Best Clusters, % Ink Extra Total, % Ink Extra From Best Cluster, " + 
                "% Ink Extra From Partial Matching Clusters, % Ink Extra From Completely Unmatching Clusters");

            foreach (string name in names)
            {
                ClusteringResultsSketch tempResult = results[name];
                writer.Write(name + ",");
                // Totals
                writer.Write(tempResult.ResultsShape.Count.ToString() + "," +

                    tempResult.NumPerfect.ToString() + "," +
                    tempResult.NumConditionalPerfect.ToString() + "," +
                    tempResult.NumConditionalPerfectNoMatchingStrokes.ToString() + "," +

                    tempResult.NumShapes.ToString() + "," +
                    tempResult.NumTexts.ToString() + "," +
                    tempResult.NumConnectors.ToString() + "," +

                    tempResult.NumPerfectShapes.ToString() + "," +
                    tempResult.NumPerfectTexts.ToString() + "," +
                    tempResult.NumPerfectConnectors.ToString() + "," +

                    tempResult.NumCondPerfectShapes.ToString() + "," +
                    tempResult.NumCondPerfectTexts.ToString() + "," +
                    tempResult.NumCondPerfectConnectors.ToString() + "," +

                    tempResult.NumTotalErrors.ToString() + "," +
                    tempResult.NumSplitErrors.ToString() + "," +
                    tempResult.NumMergeErrors.ToString() + ",");

                // Merge Errors
                writer.Write(tempResult.NumMergedShapeToShapeErrors.ToString() + "," +
                    tempResult.NumMergedShapeToTextErrors.ToString() + "," +
                    tempResult.NumMergedShapeToConnectorErrors.ToString() + "," +

                    tempResult.NumMergedTextToShapeErrors.ToString() + "," +
                    tempResult.NumMergedTextToTextErrors.ToString() + "," +
                    tempResult.NumMergedTextToConnectorErrors.ToString() + "," +

                    tempResult.NumMergedConnectorToShapeErrors.ToString() + "," +
                    tempResult.NumMergedConnectorToTextErrors.ToString() + "," + 
                    tempResult.NumMergedConnectorToConnectorErrors.ToString() + "," +

                    tempResult.NumMergedNOTBUBBLEToShapeErrors.ToString() + "," +
                    tempResult.NumMergedNOTBUBBLEisOnlyError.ToString() + ",");

                // Split Errors
                writer.Write(tempResult.NumSplitShapeErrors.ToString() + "," +
                    tempResult.NumSplitTextErrors.ToString() + "," +
                    tempResult.NumSplitConnectorErrors.ToString() + ",");

                // Unmatching Clusters
                writer.Write(tempResult.NumExtraClusters.ToString() + ",");

                // Ink
                writer.Write(tempResult.InkMatchingPercentage.ToString() + "," +
                    tempResult.InkExtraPercentageTotal.ToString() + "," +
                    tempResult.InkExtraPercentageBestMatches.ToString() + "," +
                    tempResult.InkExtraPercentagePartialMatchesNotBest.ToString() + "," +
                    tempResult.InkExtraPercentageCompletelyUnMatched.ToString() + ",");

                writer.WriteLine();
            }

            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        private void WriteImageAccuracies(string filename)
        {
        }

        #endregion


        #region User Dialog Boxes (Opening, Saving, Directory Selection)

        #region Opening

        /// <summary>
        /// Prompts the user to select sketches to load
        /// </summary>
        /// <param name="successful">Whether the files were successfully found</param>
        /// <returns>List of filenames</returns>
        private List<string> SelectOpenMultipleSketches(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Labeled XML Sketches (*.labeled.xml)|*labeled.xml";
            openDlg.Title = "Select Sketches to Load";
            openDlg.InitialDirectory = _SketchDirectory;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return new List<string>(openDlg.FileNames);
            }
            else
            {
                successful = false;
                return new List<string>();
            }
        }

        /// <summary>
        /// Prompts the user to select clustering feature file to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename of selected Clustering Feature File</returns>
        private string SelectOpenSingleFeatureFile(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Clustering Feature Files (*cluster.features.txt)|*.cluster.features.txt";
            openDlg.Title = "Select the Clustering Feature File to Open";
            openDlg.InitialDirectory = _StrokeFeatureFilesDirectory;
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select clustering feature files to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filenames of selected Clustering Feature Files</returns>
        private List<string> SelectOpenPerUserFeatureFiles(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Clustering Feature Files (*cluster.features.txt)|*.cluster.features.txt";
            openDlg.Title = "Select the Per-User Clustering Feature Files to Open";
            openDlg.InitialDirectory = _StrokeFeatureFilesDirectory;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return new List<string>(openDlg.FileNames);
            }
            else
            {
                successful = false;
                return new List<string>();
            }
        }

        /// <summary>
        /// Prompts the user to select a sketch to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename of selected sketch</returns>
        private string SelectOpenSketch(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "XML Sketch (*.xml)|*.xml";
            openDlg.Title = "Select the Sketch to Open";
            openDlg.InitialDirectory = _SketchDirectory;
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select a trained network to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename of selected network</returns>
        private string SelectOpenNetwork(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Trained Network File (*.weights.txt)|*.weights.txt";
            openDlg.Title = "Select the Network File to Open";
            openDlg.InitialDirectory = _StrokeNetworkDirectory;
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select a settings file to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename of selected settings</returns>
        private string SelectOpenSettings(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Settings File (*.txt)|*.txt";
            openDlg.Title = "Select the Settings File to Open";
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select a domain file to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename of selected domain</returns>
        private string SelectOpenDomain(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Domain File (*.domain.txt)|*.domain.txt";
            openDlg.Title = "Select the Domain File to Open";
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select a file to load
        /// </summary>
        /// <param name="successful">Whether the file was successfully found</param>
        /// <returns>Filename</returns>
        private string SelectOpenFile(out bool successful)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "All Files (*.*)|*.*";
            openDlg.Title = "Select File to Open";
            openDlg.Multiselect = false;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return openDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        #endregion

        #region Saving

        /// <summary>
        /// Prompts user to select a filename to save to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save to</returns>
        private string SelectSaveFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "All Files (*.*)|*.*";

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        private string SelectSaveImageAccuracyFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "All Files (*.*)|*.*";
            saveDlg.Title = "File to save image accuracy results to";

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the features to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save features to</returns>
        private string SelectSaveFeatureFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Clustering Feature Text Files (*.cluster.features.txt)|*.cluster.features.txt";
            saveDlg.Title = "File to save Clustering Features as";
            saveDlg.InitialDirectory = _StrokeFeatureFilesDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the features to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save features to</returns>
        private string SelectSaveMSclassifierFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Microsoft Classifier Results (*.ms.results.txt)|*.ms.results.txt";
            saveDlg.Title = "File to save Results from Microsoft Classifer to";
            saveDlg.InitialDirectory = _StrokeFeatureFilesDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the neural network to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save network to</returns>
        private string SelectSaveNetworkFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Neural Network Weights Files (*.weights.txt)|*.weights.txt";
            saveDlg.Title = "File to save Trained Neural Network as";
            saveDlg.InitialDirectory = _StrokeNetworkDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the neural network results to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save network results to</returns>
        private string SelectSaveNetworkResultsFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Neural Network Results Files (*.results.txt)|*.results.txt";
            saveDlg.Title = "File to save Neural Network test results as";
            saveDlg.InitialDirectory = _StrokeNetworkDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the clustering results to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save cluster results to</returns>
        private string SelectSaveClusterResultsFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Clustering Accuracy Results Files (*.cluster.accuracy.results.txt)|*.cluster.accuracy.results.txt";
            saveDlg.Title = "File to save Cluster Accuracy results as";
            saveDlg.InitialDirectory = _StrokeResultsDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the equation strings to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save equation strings to</returns>
        private string SelectSaveEquationsFile(out bool successful)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Equation String Results Files (*.equations.results.txt)|*.equations.results.txt";
            saveDlg.Title = "File to save Equation String results as";
            saveDlg.InitialDirectory = _StrokeResultsDirectory;
            saveDlg.AddExtension = true;

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts user to select a filename to save the equation strings to
        /// </summary>
        /// <param name="successful">Whether the file was successfully loaded</param>
        /// <returns>Filename to save equation strings to</returns>
        private string SelectSaveImageFile(out bool successful, out System.Drawing.Imaging.ImageFormat format)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.FilterIndex = 1;
            saveDlg.Filter = "Ink Image Files (*.jpg)|*.jpg|" +
                "Ink Image Files (*.tiff)|*.tiff|" +
                "Ink Image Files (*.gif)|*.gif|" + 
                "Ink Image Files (*.png)|*.png|" + 
                "Ink Image Files (*.bmp)|*.bmp";
            saveDlg.Title = "File to save image of ink to...";
            saveDlg.RestoreDirectory = true;
            saveDlg.AddExtension = true; 

            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                int formatIndex = saveDlg.FilterIndex;
                if (formatIndex == 1)
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                else if (formatIndex == 2)
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                else if (formatIndex == 3)
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                else if (formatIndex == 4)
                    format = System.Drawing.Imaging.ImageFormat.Png;
                else if (formatIndex == 5)
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                else
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                successful = true;
                return saveDlg.FileName;
            }
            else
            {
                int formatIndex = saveDlg.FilterIndex;
                if (formatIndex == 1)
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                else if (formatIndex == 2)
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                else if (formatIndex == 3)
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                else if (formatIndex == 4)
                    format = System.Drawing.Imaging.ImageFormat.Png;
                else if (formatIndex == 5)
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                else
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                successful = false;
                return "";
            }
        }

        #endregion

        #region Directory Selection

        /// <summary>
        /// Prompts the user to select a directory to save the feature files to
        /// </summary>
        /// <param name="successful">Whether directory was successfully selected</param>
        /// <returns>Directory path</returns>
        private string SelectDirectorySaveFeatureFile(out bool successful)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "Select the Directory to Save Clustering Feature Files To";
            folderDlg.SelectedPath = _StrokeFeatureFilesDirectory;

            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return folderDlg.SelectedPath;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        /// <summary>
        /// Prompts the user to select a directory to save the feature files to
        /// </summary>
        /// <param name="successful">Whether directory was successfully selected</param>
        /// <returns>Directory path</returns>
        private string SelectDirectorySaveImageFile(out bool successful)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "Select the Directory to Save Clustered Sketch Images To";
            folderDlg.SelectedPath = _StrokeFeatureFilesDirectory;

            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                successful = true;
                return folderDlg.SelectedPath;
            }
            else
            {
                successful = false;
                return "";
            }
        }

        #endregion

        #endregion


        #region Ink Handling

        /// <summary>
        /// Change the colors to display the best clustering
        /// </summary>
        private void DisplayBestCluster()
        {
            foreach (Cluster.Cluster c in _BestClusters.Clusters)
            {
                Color color = int2Color(_ClusterClassifications[c.Id]);

                foreach (Substroke s in c.Strokes)
                {
                    _Substroke2MStroke[s.Id].DrawingAttributes.Color = color;
                    _MStroke2Color[_Substroke2MStroke[s.Id].Id] = color;
                }
            }
            this.Refresh();
        }

        /// <summary>
        /// Returns a color for a stroke based on the index of the cluster it belongs to
        /// </summary>
        /// <param name="index">Index of cluster</param>
        /// <returns>Color for stroke</returns>
        private Color int2Color(int index)
        {
            while (index > 20)
                index -= 20;
            switch (index)
            {
                case (0):
                    return Color.SpringGreen;
                case (1):
                    return Color.Red;
                case (2):
                    return Color.Green;
                case (3):
                    return Color.RosyBrown;
                case (4):
                    return Color.Salmon;
                case (5):
                    return Color.Brown;
                case (6):
                    return Color.Magenta;
                case (7):
                    return Color.Maroon;
                case (8):
                    return Color.Olive;
                case (9):
                    return Color.Orange;
                case (10):
                    return Color.Plum;
                case (11):
                    return Color.SeaGreen;
                case (12):
                    return Color.YellowGreen;
                case (13):
                    return Color.AliceBlue;
                case (14):
                    return Color.Coral;
                case (15):
                    return Color.Cyan;
                case (16):
                    return Color.DarkOrange;
                case (17):
                    return Color.DarkGreen;
                case (18):
                    return Color.DarkCyan;
                case (19):
                    return Color.DeepPink;
                case (20):
                    return Color.Fuchsia;
                case (21):
                    return Color.Lavender;
                default:
                    return Color.Lime;
            }
        }

        /// <summary>
        /// Populates the stroke objects in an Ink Overlay object using the
        /// substrokes in a sketch object
        /// </summary>
        /// <param name="sketch">Sketch containing substrokes to convert</param>
        private void FillInkOverlay(Sketch.Sketch sketch)
        {
            _OverlayInk.Ink.DeleteStrokes();
            _Substroke2MStroke.Clear();
            _MStroke2Substroke.Clear();
            foreach (Substroke s in sketch.Substrokes)
            {
                _OverlayInk.Ink.CreateStroke(s.PointsAsSysPoints);
                _Substroke2MStroke.Add(s.Id, _OverlayInk.Ink.Strokes[_OverlayInk.Ink.Strokes.Count - 1]);
                _MStroke2Substroke.Add(_OverlayInk.Ink.Strokes[_OverlayInk.Ink.Strokes.Count - 1].Id, s);
                _MStroke2Color.Add(_OverlayInk.Ink.Strokes[_OverlayInk.Ink.Strokes.Count - 1].Id, Color.Black);
            }

            // Move center the ink's origin to the top-left corner
            Rectangle bb = _OverlayInk.Ink.GetBoundingBox();
            _InkMovedX = 0.0f;
            _InkMovedY = 0.0f;
            _Scale = 1.0f;

            ScaleAndMoveInk();
            UpdateColors();
        }

        /// <summary>
        /// Updates the colors of a sketch based on the lookup table
        /// </summary>
        private void UpdateColors()
        {
            ChangeStrokesDisplayed();

            foreach (Microsoft.Ink.Stroke s in _OverlayInk.Ink.Strokes)
                s.DrawingAttributes.Color = _MStroke2Color[s.Id];

            //_OverlayInk.Ink.Strokes[_OverlayInk.Ink.Strokes.Count - 3].DrawingAttributes.Color = Color.SeaGreen;
            
            this.inkPanel.Refresh();
        }

        /// <summary>
        /// Find the best scale for the ink based on bounding box size compared to the panel size.
        /// Then actually scale the ink in both x and y directions after finding the best scale.
        /// Then move the ink to the top left corner of the panel with some padding on left and top.
        /// </summary>
        private void ScaleAndMoveInk()
        {
            if (_OverlayInk.Ink.Strokes.Count == 0)
                return;

            Rectangle box = _OverlayInk.Ink.GetBoundingBox();

            float scaleX = 1.0f;
            float scaleY = 1.0f;

            System.Drawing.Point pt = new System.Drawing.Point(this.inkPanel.Width, this.inkPanel.Height);

            _OverlayInk.Renderer.PixelToInkSpace(this.g, ref pt);

            scaleX = (float)pt.X / (float)box.Width * 0.9f;
            scaleY = (float)pt.Y / (float)box.Height * 0.9f;

            this._Scale = Math.Min(scaleX, scaleY);

            _OverlayInk.Ink.Strokes.Scale(_Scale, _Scale);

            float offset = 300.0f;
            float menuStripOffset = 600.0f;

            box = _OverlayInk.Ink.GetBoundingBox();

            this._InkMovedX = -(float)box.X + offset;
            this._InkMovedY = -(float)(box.Y) + offset + menuStripOffset;

            box = _OverlayInk.Ink.GetBoundingBox();
            _OverlayInk.Ink.Strokes.Move(_InkMovedX, _InkMovedY);
        }

        private void ChangeStrokesDisplayed()
        {
            foreach (Substroke s in _Sketch.Substrokes)
            {
                if (IsGate(s) && !displayGatesToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 255;
                else if (IsGate(s) && displayGatesToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 0;

                else if (IsLabel(s) && !displayTextToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 255;
                else if (IsLabel(s) && displayTextToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 0;

                else if (IsConnector(s) && !displayWiresToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 255;
                else if (IsConnector(s) && displayWiresToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 0;

                else if (!displayOtherToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 255;
                else if (displayOtherToolStripMenuItem.Checked)
                    _Substroke2MStroke[s.Id].DrawingAttributes.Transparency = 0;
            }
        }

        private void displayGatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateColors();
        }

        private void displayWiresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateColors();
        }

        private void displayTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateColors();
        }

        private void displayOtherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateColors();
        }

        #endregion


        #region Miscellaneous Functions

        /// <summary>
        /// Clear all Dictionaries so that we start fresh and don't try to
        /// duplicate an key in the dictionary
        /// </summary>
        private void DisposeDictionaries()
        {
            _Index2Substroke.Clear();
            _ClusterClassifications.Clear();
            _MStroke2Color.Clear();
            _MStroke2Substroke.Clear();
            _Substroke2MStroke.Clear();
            _StrokeFeatures.Clear();
            _StrokeClassifications.Clear();
            _ImageRecoResult.Clear();
            _Cluster2Symbol.Clear();
        }

        /// <summary>
        /// Look at the Option Menu Items that have been checked, get the bool array
        /// that specifies which options are "checked"
        /// </summary>
        /// <param name="numFeatures">Number of features turned on</param>
        /// <returns>Bool array of features</returns>
        private bool[] GetUseFeaturesCluster(out int numFeatures)
        {
            int numAllowedFeatures = 8;
            bool[] useFeatures = new bool[numAllowedFeatures];
            numFeatures = 0;

            useFeatures[0] = useMinimumDistanceToolStripMenuItem.Checked;
            useFeatures[1] = useTimeGapToolStripMenuItem.Checked;
            useFeatures[2] = useHorizontalOverlapToolStripMenuItem.Checked;
            useFeatures[3] = useVerticalOverlapToolStripMenuItem.Checked;
            useFeatures[4] = useMaximumDistanceToolStripMenuItem.Checked;
            useFeatures[5] = useCentroidDistanceToolStripMenuItem.Checked;
            useFeatures[6] = useDistanceRatio1ToolStripMenuItem.Checked;
            useFeatures[7] = useDistanceRatio2ToolStripMenuItem.Checked;

            foreach (bool feature in useFeatures)
            {
                if (feature)
                    numFeatures++;
            }

            return useFeatures;
        }

        /// <summary>
        /// Get the features to use for stroke classification from the bitStrings
        /// </summary>
        /// <param name="numFeatures"></param>
        /// <returns></returns>
        private bool[] GetUseFeaturesStroke(out int numFeatures)
        {
            bool[] useFeatures;
            if (ThreeWay)
                useFeatures = new bool[_BitString3Way.Length];
            else if (TwoWay)
                useFeatures = new bool[_BitString2Way.Length];
            else if (TwoXTwoWay)
                useFeatures = new bool[_BitString2x2Way_2.Length];
            else
                useFeatures = new bool[0];
            char one = '1';
            numFeatures = 0;

            for (int i = 0; i < useFeatures.Length; i++)
            {
                if (TwoWay)
                {
                    if (_BitString2Way[i] == one)
                    {
                        useFeatures[i] = true;
                        numFeatures++;
                    }
                    else
                        useFeatures[i] = false;
                }
                else if (ThreeWay)
                {
                    if (_BitString3Way[i] == one)
                    {
                        useFeatures[i] = true;
                        numFeatures++;
                    }
                    else
                        useFeatures[i] = false;
                }
                else if (TwoXTwoWay)
                {
                    if (_BitString2x2Way_2[i] == one)
                    {
                        useFeatures[i] = true;
                        numFeatures++;
                    }
                    else
                        useFeatures[i] = false;
                }
            }

            return useFeatures;
        }

        /// <summary>
        /// Determine whether 2 strokes belong to the same parent shape based on indices
        /// </summary>
        /// <param name="i">Index of first stroke</param>
        /// <param name="j">Index of second stroke</param>
        /// <returns>Belong to same shape?</returns>
        private bool SameShape(int i, int j, string label)
        {
            if (_Index2Substroke[label][i].ParentShapes.Count > 0 && _Index2Substroke[label][j].ParentShapes.Count > 0)
            {
                if (_Index2Substroke[label][i].ParentShapes[0].Id == _Index2Substroke[label][j].ParentShapes[0].Id)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Get the selected number of training epochs for the Neural Networks
        /// </summary>
        /// <returns>Number of training epochs to run</returns>
        private int NumTrainingEpochs
        {
            get
            {
                if (trainingEpochs500.Checked)
                    return 500;
                else if (trainingEpochs1000.Checked)
                    return 1000;
                else if (trainingEpochs2000.Checked)
                    return 2000;
                else if (trainingEpochs5000.Checked)
                    return 5000;
                else
                    return 1000;
            }
        }

        /// <summary>
        /// Gets the option for classification, if Two Class
        /// </summary>
        private bool TwoWay
        {
            get { return twoWayToolStripMenuItem.Checked; }
        }

        /// <summary>
        /// Gets the option for classification, if Three Class
        /// </summary>
        private bool ThreeWay
        {
            get { return threeWayToolStripMenuItem.Checked; }
        }

        /// <summary>
        /// Gets the option for classification, if Three Class by way of double two-way networks
        /// </summary>
        private bool TwoXTwoWay
        {
            get { return twoXTwoWayToolStripMenuItem.Checked; }
        }

        /// <summary>
        /// Whether to use actual stroke classification for clustering
        /// </summary>
        private bool ActualStrokeClassification
        {
            get { return useActualStrokeClassificationsToolStripMenuItem.Checked; }
        }

        /// <summary>
        /// Whether to use perfect stroke classification for clustering
        /// </summary>
        private bool PerfectStrokeClassification
        {
            get { return usePerfectStrokeClassificationsToolStripMenuItem.Checked; }
        }

        /// <summary>
        /// Predicate for determining whether a list has an empty string in it, i.e. "" or " "
        /// </summary>
        /// <param name="value">string</param>
        /// <returns>whether string is 'empty'</returns>
        private bool IsEmpty(string value)
        {
            if (value == " " || value == "")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Determine whether a shape is part of a 'gate' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns></returns>
        private bool IsGate(Shape s)
        {
            List<string> gateShapes = new List<string>();
            gateShapes.Add("AND");
            gateShapes.Add("OR");
            gateShapes.Add("NAND");
            gateShapes.Add("NOR");
            gateShapes.Add("NOT");
            gateShapes.Add("NOTBUBBLE");
            gateShapes.Add("BUBBLE");
            gateShapes.Add("XOR");
            gateShapes.Add("XNOR");
            gateShapes.Add("LabelBox");
            gateShapes.Add("Male");
            gateShapes.Add("Female");

            return gateShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'gate' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a gate</returns>
        private bool IsGate(Substroke s)
        {
            List<string> gateShapes = new List<string>();
            gateShapes.Add("AND");
            gateShapes.Add("OR");
            gateShapes.Add("NAND");
            gateShapes.Add("NOR");
            gateShapes.Add("NOT");
            gateShapes.Add("NOTBUBBLE");
            gateShapes.Add("BUBBLE");
            gateShapes.Add("XOR");
            gateShapes.Add("XNOR");
            gateShapes.Add("LabelBox");
            gateShapes.Add("Male");
            gateShapes.Add("Female");

            return gateShapes.Contains(s.FirstLabel);
        }

        /// <summary>
        /// Determine whether a shape is part of a 'label' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns>whether it is a gate</returns>
        private bool IsLabel(Shape s)
        {
            List<string> labelShapes = new List<string>();
            labelShapes.Add("Label");
            labelShapes.Add("Text");

            return labelShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'label' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a gate</returns>
        private bool IsLabel(Substroke s)
        {
            List<string> labelShapes = new List<string>();
            labelShapes.Add("Label");
            labelShapes.Add("Text");

            return labelShapes.Contains(s.FirstLabel);
        }

        /// <summary>
        /// Determine whether a shape is part of a 'connector' by way of its xml.type
        /// </summary>
        /// <param name="s">Shape</param>
        /// <returns>whether it is a connector</returns>
        private bool IsConnector(Shape s)
        {
            List<string> connectorShapes = new List<string>();
            connectorShapes.Add("Wire");
            connectorShapes.Add("ChildLink");
            connectorShapes.Add("Marriage");
            connectorShapes.Add("Divorce");

            return connectorShapes.Contains(s.XmlAttrs.Type);
        }

        /// <summary>
        /// Determine whether a substroke is part of a 'connector' by way of its xml.type
        /// </summary>
        /// <param name="s">Substroke</param>
        /// <returns>whether it is a connector</returns>
        private bool IsConnector(Substroke s)
        {
            List<string> connectorShapes = new List<string>();
            connectorShapes.Add("Wire");
            connectorShapes.Add("ChildLink");
            connectorShapes.Add("Marriage");
            connectorShapes.Add("Divorce");

            return connectorShapes.Contains(s.FirstLabel);
        }

        private bool IsOther(Shape s)
        {
            List<string> otherShapes = new List<string>();
            otherShapes.Add("other");

            return otherShapes.Contains(s.XmlAttrs.Type);
        }

        private bool IsOther(Substroke s)
        {
            List<string> otherShapes = new List<string>();
            otherShapes.Add("other");

            return otherShapes.Contains(s.FirstLabel);
        }

        /// <summary>
        /// Update the directory paths based on whether it is 2/3/2x2 way classification & clustering
        /// </summary>
        private void UpdateDirectories()
        {
            if (TwoWay)
            {
                if (_ClusterFeatureFilesDirectory.Contains("ThreeWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("ThreeWay", "TwoWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("ThreeWay", "TwoWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("ThreeWay", "TwoWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("ThreeWay", "TwoWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("ThreeWay", "TwoWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("ThreeWay", "TwoWay");
                }
                else if (_ClusterFeatureFilesDirectory.Contains("TwoXTwoWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("TwoXTwoWay", "TwoWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("TwoXTwoWay", "TwoWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("TwoXTwoWay", "TwoWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("TwoXTwoWay", "TwoWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("TwoXTwoWay", "TwoWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("TwoXTwoWay", "TwoWay");
                }

                string userNumber = _SketchName.Substring(0, 2);
                _ClusterNetworkName = "User" + userNumber + "_2Way_holdout";

            }
            else if (ThreeWay)
            {
                if (_ClusterFeatureFilesDirectory.Contains("TwoXTwoWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("TwoXTwoWay", "ThreeWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("TwoXTwoWay", "ThreeWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("TwoXTwoWay", "ThreeWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("TwoXTwoWay", "ThreeWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("TwoXTwoWay", "ThreeWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("TwoXTwoWay", "ThreeWay");
                }
                else if (_ClusterFeatureFilesDirectory.Contains("TwoWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("TwoWay", "ThreeWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("TwoWay", "ThreeWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("TwoWay", "ThreeWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("TwoWay", "ThreeWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("TwoWay", "ThreeWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("TwoWay", "ThreeWay");
                }

                string userNumber = _SketchName.Substring(0, 2);
                _ClusterNetworkName = "User" + userNumber + "_3Way_1_holdout";
                _Cluster2NetworkName = "User" + userNumber + "_3Way_2_holdout";
                _Cluster3NetworkName = "User" + userNumber + "_3Way_3_holdout";
            }
            else if (TwoXTwoWay)
            {
                if (_ClusterFeatureFilesDirectory.Contains("TwoWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("TwoWay", "TwoXTwoWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("TwoWay", "TwoXTwoWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("TwoWay", "TwoXTwoWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("TwoWay", "TwoXTwoWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("TwoWay", "TwoXTwoWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("TwoWay", "TwoXTwoWay");
                }
                else if (_ClusterFeatureFilesDirectory.Contains("ThreeWay"))
                {
                    _ClusterFeatureFilesDirectory = _ClusterFeatureFilesDirectory.Replace("ThreeWay", "TwoXTwoWay");
                    _ClusterNetworkDirectory = _ClusterNetworkDirectory.Replace("ThreeWay", "TwoXTwoWay");
                    _ClusterResultsDirectory = _ClusterResultsDirectory.Replace("ThreeWay", "TwoXTwoWay");
                    _StrokeFeatureFilesDirectory = _StrokeFeatureFilesDirectory.Replace("ThreeWay", "TwoXTwoWay");
                    _StrokeNetworkDirectory = _StrokeNetworkDirectory.Replace("ThreeWay", "TwoXTwoWay");
                    _StrokeResultsDirectory = _StrokeResultsDirectory.Replace("ThreeWay", "TwoXTwoWay");
                }
            }
        }

        #endregion


        #region Single Stroke Classification

        /// <summary>
        /// Classify the sketch using the stroke features of the sketch
        /// </summary>
        private bool ClassifySketch()
        {
            this.StatusText.Text = "Classifying Sketch...";
            this.Refresh();

            if (_StrokeFeatures.Count == 0)
                _StrokeFeatures = StrokeFeatures.getMultiStrokeFeatures(_Sketch);

            //CalculateFeatureSpace();

            string network = _StrokeNetworkDirectory + "\\" + _StrokeNetworkName + ".features.weights.txt";
            bool loaded = LoadNetwork(network);
            if (!loaded)
            {
                StatusText.Text = "Unable to load appropriate Network --- Ready";
                return false;
            }

            if (ActualStrokeClassification && TwoWay)
            {
                double[] classifications = runNN2wayAndGetResults();
                ApplyClassifications(classifications);
            }
            else if (ActualStrokeClassification && ThreeWay)
            {
                double[][] classifications = runNN3wayAndGetResults();
                ApplyClassifications(classifications);
            }
            else
                ApplyPerfectClassifications();

            this.StatusText.Text = "Ready";

            UpdateColors();

            this.Refresh();

            return true;
        }

        private void CalculateFeatureSpace()
        {
            Dictionary<string, bool> useList = new Dictionary<string, bool>();
            useList.Add("Arc Length", true);
            useList.Add("Bounding Box Width", true);
            useList.Add("Bounding Box Height", true);
            useList.Add("Bounding Box Area", true);
            useList.Add("Path Density", true);
            useList.Add("End Point to Arc Length Ratio", true);
            useList.Add("Sum of Thetas", true);
            useList.Add("Sum of Abs Value of Thetas", true);
            useList.Add("Sum of Squared Thetas", true);
            useList.Add("Sum of Sqrt of Thetas", true);
            useList.Add("Average Pen Speed", true);
            useList.Add("Maximum Pen Speed", true);
            useList.Add("Minimum Pen Speed", true);
            useList.Add("Difference Between Maximum and Minimum Pen Speed", true);
            useList.Add("Time to Previous Stroke", true);
            useList.Add("Time to Next Stroke", true);
            useList.Add("Time to Draw Stroke", true);



        }

        /// <summary>
        /// Runs the features through a trained Neural net to get results (2-way)
        /// </summary>
        /// <returns></returns>
        private double[] runNN2wayAndGetResults()
        {
            double[][] testSet = new double[_StrokeFeatures.Count][];

            for (int i = 0; i < _StrokeFeatures.Count; i++)
            {
                testSet[i] = AssignFeatureValues(_StrokeFeatures[i]);
            }

            return _NetworkStrokes_2Class.TestNN(testSet);
        }

        /// <summary>
        /// Runs the features through a trained Neural net to get results (3-way)
        /// </summary>
        /// <returns></returns>
        private double[][] runNN3wayAndGetResults()
        {
            double[][] testSet = new double[_StrokeFeatures.Count][];

            // Actual Method
            
            for (int i = 0; i < _StrokeFeatures.Count; i++)
            {
                testSet[i] = AssignFeatureValues(_StrokeFeatures[i]);
            }
            //WriteTestSetData("c:\\old.txt", testSet);

            return _NetworkStrokes_3Class.TestNN(testSet, true);


            // Hack to display strokes with a certain feature
            /*for (int i = 0; i < _StrokeFeatures.Count; i++)
            {
                testSet[i] = new double[3];
                testSet[i][0] = 0.0;
                testSet[i][1] = 0.0;
                //testSet[i][2] = _StrokeFeatures[i].ClosedShapeConnections;
                if (_StrokeFeatures[i].ClosedShapeConnections)
                    testSet[i][2] = 1.0;
                else
                    testSet[i][2] = 0.0;
            }
            
            return testSet;*/

            
        }

        private void WriteTestSetData(string filename, double[][] testSet)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename);

            foreach (double[] arr in testSet)
            {
                foreach (double value in arr)
                    writer.Write(value.ToString() + " ");

                writer.WriteLine();
            }

            writer.Close();
        }

        /// <summary>
        /// Creates an array of feature values to be put in the NN
        /// </summary>
        /// <param name="feature">Stroke features to put in array</param>
        /// <returns>Array of features for a stroke</returns>
        private double[] AssignFeatureValues(StrokeFeatures feature)
        {
            int k = 0;
            int f = 0;
            int numFeatures;

            bool[] useFeatures = GetUseFeaturesStroke(out numFeatures);
            double[] values = new double[numFeatures];

            if (useFeatures[f])
            {
                values[k] = feature.ArcLength / feature.AvgArcLength;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.AverageSpeed / feature.AvgSpeedSketch;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.MaximumSpeed / feature.AvgSpeedSketch;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.SumTheta / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.SumAbsTheta / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.SumSquaredTheta / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.SumSqrtTheta / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.PathDensity / 100.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.BoundingBox.Width / feature.AvgWidth;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.BoundingBox.Height / feature.AvgHeight;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.BoundingBox.Width * feature.BoundingBox.Height / feature.AvgArea;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                if (feature.SelfEnclosingShape)
                {
                    values[k] = 1.0;
                    k++;
                }
                else
                {
                    values[k] = 0.0; k++;
                }
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.NumSelfIntersections / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.NumLIntersections / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.NumTIntersections_teeing / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.NumTIntersections_td_into / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.NumXIntersections / 10.0;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                if (feature.ClosedShapeConnections)
                {
                    values[k] = 1.0;
                    k++;
                }
                else
                {
                    values[k] = 0.0;
                    k++;
                }
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.EndPointToArcLengthRatio;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.Distance2LeftEdge;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.Distance2TopEdge;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.EnclosedInAnotherShape;
                k++;
            }
            f++;
            if (useFeatures[f])
            {
                values[k] = feature.MinimumSpeed / feature.AvgSpeedSketch;
                k++;
            }
            f++;

            return values;
        }

        /// <summary>
        /// Apply _StrokeClassifications as if the classifier were perfect
        /// </summary>
        private void ApplyPerfectClassifications()
        {
            _StrokeClassifications.Clear();

            string clsStr = "3";
            int cls = 2;
            if (TwoWay)
            {
                clsStr = "2";
                cls = 1;
            }

            foreach (Substroke s in _Sketch.SubstrokesL)
            {
                if (IsGate(s))
                {
                    _MStroke2Color[_Substroke2MStroke[s.Id].Id] = _DomainInfo[clsStr].GetColor(cls);
                    _StrokeClassifications.Add(s.Id, _DomainInfo[clsStr].GetLabel(cls));
                }
                else if (IsLabel(s))
                {
                    _MStroke2Color[_Substroke2MStroke[s.Id].Id] = _DomainInfo[clsStr].GetColor(1);
                    _StrokeClassifications.Add(s.Id, _DomainInfo[clsStr].GetLabel(1));
                }
                else
                {
                    _MStroke2Color[_Substroke2MStroke[s.Id].Id] = _DomainInfo[clsStr].GetColor(0);
                    _StrokeClassifications.Add(s.Id, _DomainInfo[clsStr].GetLabel(0));
                }
            }
        }

        /// <summary>
        /// Using the classifications, assign colors and probabilities to strokes
        /// For 2-Way Classification
        /// </summary>
        /// <param name="NNclassifications">Classifications read in from the NN</param>
        private void ApplyClassifications(double[] NNclassifications)
        {
            _StrokeClassifications.Clear();
            for (int i = 0; i < _StrokeFeatures.Count; i++)
            {
                if (NNclassifications[i] > 0.5)
                {
                    double confidence = 1.0 - (1.0 - NNclassifications[i]) * 2.0;
                    _MStroke2Color[_Substroke2MStroke[_StrokeFeatures[i].Id].Id] = _DomainInfo["2"].GetColor(0);
                    _StrokeClassifications.Add(_StrokeFeatures[i].Id, _DomainInfo["2"].GetLabel(0));
                }
                else
                {
                    double confidence = 1.0 - NNclassifications[i] * 2.0;
                    _MStroke2Color[_Substroke2MStroke[_StrokeFeatures[i].Id].Id] = _DomainInfo["2"].GetColor(1);
                    _StrokeClassifications.Add(_StrokeFeatures[i].Id, _DomainInfo["2"].GetLabel(1));
                }
            }
        }

        /// <summary>
        /// Using the classifications, assign colors and probabilities to strokes
        /// For 3-Way Classification
        /// </summary>
        /// <param name="NNclassifications">Classifications read in from the NN</param>
        private void ApplyClassifications(double[][] NNclassifications)
        {
            _StrokeClassifications.Clear();

            for (int i = 0; i < _StrokeFeatures.Count; i++)
            {
                int maxClass = 3;

                if (NNclassifications[i][0] >= NNclassifications[i][1])
                {
                    if (NNclassifications[i][0] >= NNclassifications[i][2])
                        maxClass = 0;
                    else
                        maxClass = 2;
                }
                else
                {
                    if (NNclassifications[i][1] >= NNclassifications[i][2])
                        maxClass = 1;
                    else
                        maxClass = 2;
                }

                // Class 1
                if (maxClass == 0)
                {
                    _MStroke2Color[_Substroke2MStroke[_StrokeFeatures[i].Id].Id] = _DomainInfo["3"].GetColor(0);
                    _StrokeClassifications.Add(_StrokeFeatures[i].Id, _DomainInfo["3"].GetLabel(0));
                }
                // Class 2
                else if (maxClass == 1)
                {
                    _MStroke2Color[_Substroke2MStroke[_StrokeFeatures[i].Id].Id] = _DomainInfo["3"].GetColor(1);
                    _StrokeClassifications.Add(_StrokeFeatures[i].Id, _DomainInfo["3"].GetLabel(1));
                }
                // Class 3
                else if (maxClass == 2)
                {
                    _MStroke2Color[_Substroke2MStroke[_StrokeFeatures[i].Id].Id] = _DomainInfo["3"].GetColor(2);
                    _StrokeClassifications.Add(_StrokeFeatures[i].Id, _DomainInfo["3"].GetLabel(2));
                }
            }
        }

        private void FeatureUserHoldoutTesting()
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Feature Files (*.features.txt)|*.features.txt";
            openDlg.Title = "Select Per-User Feature Files to Include in Test";
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            List<string> usersToExclude = new List<string>();
            //usersToExclude.Add("01");
            //usersToExclude.Add("02");
            //usersToExclude.Add("03");
            //usersToExclude.Add("04");
            //usersToExclude.Add("05");
            //usersToExclude.Add("06");
            //usersToExclude.Add("07");
            //usersToExclude.Add("08");
            //usersToExclude.Add("09");
            //usersToExclude.Add("10");
            //usersToExclude.Add("11");
            //usersToExclude.Add("12");
            //usersToExclude.Add("13");
            //usersToExclude.Add("14");
            //usersToExclude.Add("15");
            //usersToExclude.Add("16");
            //usersToExclude.Add("17");
            //usersToExclude.Add("18");
            //usersToExclude.Add("19");
            //usersToExclude.Add("20");
            //usersToExclude.Add("21");
            //usersToExclude.Add("22");
            //usersToExclude.Add("23");
            //usersToExclude.Add("24");
            // Clear this list to start over
            usersToExclude.Clear();

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                int numIterations = 10;
                for (int n = 1; n <= numIterations; n++)
                {
                    int numFiles = openDlg.FileNames.Length;

                    int numFeatures;
                    bool[] useFeatures = GetUseFeaturesStroke(out numFeatures);

                    // i = file number to hold out for training
                    for (int i = 0; i < numFiles; i++)//3; i++)
                    {
                        CreateTestPerUserFeature(openDlg.FileNames[i], useFeatures);
                        bool first = true;

                        for (int j = 0; j < numFiles; j++)
                        {
                            if (i != j)
                            {
                                if (first)
                                {
                                    first = false;
                                    CreateTrainingPerUserFeature(openDlg.FileNames[j], useFeatures);
                                }
                                else
                                    AppendTrainingPerUserFeature(openDlg.FileNames[j], useFeatures);
                            }
                        }

                        // Train NN
                        this.StatusText.Text = "Training Features Network...";
                        this.Refresh();
                        string outputFilename = openDlg.FileNames[i];
                        int sIndex = outputFilename.IndexOf("\\User") + 5;
                        string userNum = outputFilename.Substring(sIndex, 2);
                        outputFilename = outputFilename.Replace("\\User", "\\Weights and Results " + n.ToString() + " \\User");
                        if (!usersToExclude.Contains(userNum))
                        {
                            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(outputFilename)))
                                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFilename));
                            outputFilename = outputFilename.Replace(".features", "_holdout.features.weights");
                            if (ThreeWay)
                                _NetworkStrokes_3Class.TrainNN("../../train_peruser.features.txt", outputFilename, _NumTrainingEpochs);
                            else if (TwoWay)
                                _NetworkStrokes_2Class.TrainNN("../../train_peruser.features.txt", outputFilename, _NumTrainingEpochs);
                            else
                                break;

                            // Test NN
                            this.StatusText.Text = "Testing Features Network...";
                            this.Refresh();
                            outputFilename = outputFilename.Replace("weights", "results");
                            if (ThreeWay)
                                _NetworkStrokes_3Class.TestNN("../../test_peruser.features.txt", outputFilename);
                            else if (TwoWay)
                                _NetworkStrokes_2Class.TestNN("../../test_peruser.features.txt", outputFilename);
                            else
                                break;
                        }

                        // Done with this holdout
                        this.StatusText.Text = "Ready";
                        this.Refresh();
                    }

                    this.StatusText.Text = "Ready";
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Create a temporary file that will be used for testing the NN
        /// while doing a user holdout study.
        /// </summary>
        /// <param name="filename">Filename of an .interstroke.txt file for the user who is being held out</param>
        private void CreateTestPerUserFeature(string filename, bool[] useFeatures)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            System.IO.StreamWriter writer = new System.IO.StreamWriter("../../test_peruser.features.txt", false);

            string line = reader.ReadLine(); //numInputs
            line = reader.ReadLine(); //numOutputs

            int numFeatures = 0;
            for (int i = 0; i < useFeatures.Length; i++)
            {
                if (useFeatures[i])
                    numFeatures++;
            }

            writer.WriteLine(numFeatures.ToString());
            if (ThreeWay)
                writer.WriteLine("3");
            else
                writer.WriteLine("1");

            while ((line = reader.ReadLine()) != null)
            {
                string[] orig = line.Split("  ".ToCharArray());
                string[] vals = RemoveEmptyStrings(orig);
                int n = vals.Length;
                // Write feature values
                for (int i = 0; i < useFeatures.Length; i++)
                {
                    if (useFeatures[i])
                        writer.Write(vals[i] + "  ");
                }

                // Write output values
                if (ThreeWay)
                    writer.WriteLine(vals[n - 3] + "  " + vals[n - 2] + "  " + vals[n - 1]);
                else
                    writer.WriteLine(vals[n - 1]);
            }

            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// Removes any strings that are "" from an array
        /// </summary>
        /// <param name="orig"></param>
        /// <returns></returns>
        private string[] RemoveEmptyStrings(string[] orig)
        {
            List<string> vals = new List<string>(orig.Length);

            foreach (string s in orig)
            {
                if (!IsEmpty(s))
                    vals.Add(s);
            }

            string[] output = new string[vals.Count];
            for (int i = 0; i < vals.Count; i++)
                output[i] = vals[i];

            return output;
        }

        /// <summary>
        /// Create a temporary file that will be used for training the NN
        /// while doing a user holdout study.
        /// </summary>
        /// <param name="filename">Filename of the first user's .interstroke.txt file who is NOT being held out</param>
        private void CreateTrainingPerUserFeature(string filename, bool[] useFeatures)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            System.IO.StreamWriter writer = new System.IO.StreamWriter("../../train_peruser.features.txt", false);

            string line = reader.ReadLine(); //numInputs
            line = reader.ReadLine(); //numOutputs

            int numFeatures = 0;
            for (int i = 0; i < useFeatures.Length; i++)
            {
                if (useFeatures[i])
                    numFeatures++;
            }

            writer.WriteLine(numFeatures.ToString());
            if (ThreeWay)
                writer.WriteLine("3");
            else
                writer.WriteLine("1");

            while ((line = reader.ReadLine()) != null)
            {
                string[] orig = line.Split("  ".ToCharArray());
                string[] vals = RemoveEmptyStrings(orig);
                int n = vals.Length;
                for (int i = 0; i < useFeatures.Length; i++)
                {
                    if (useFeatures[i])
                        writer.Write(vals[i] + "  ");
                }

                // Write output values
                if (ThreeWay)
                    writer.WriteLine(vals[n - 3] + "  " + vals[n - 2] + "  " + vals[n - 1]);
                else
                    writer.WriteLine(vals[n - 1]);
            }

            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// Append to a temporary file that will be used for training the NN
        /// while doing a user holdout study.
        /// </summary>
        /// <param name="filename">Filename of a user's .interstroke.txt file who is NOT being held out</param>
        private void AppendTrainingPerUserFeature(string filename, bool[] useFeatures)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
            System.IO.StreamWriter writer = new System.IO.StreamWriter("../../train_peruser.features.txt", true);

            string line = reader.ReadLine(); //numInputs
            line = reader.ReadLine(); //numOutputs

            while ((line = reader.ReadLine()) != null)
            {
                string[] orig = line.Split("  ".ToCharArray());
                string[] vals = RemoveEmptyStrings(orig);
                int n = vals.Length;
                for (int i = 0; i < useFeatures.Length; i++)
                {
                    if (useFeatures[i])
                        writer.Write(vals[i] + "  ");
                }

                // Write output values
                if (ThreeWay)
                    writer.WriteLine(vals[n - 3] + "  " + vals[n - 2] + "  " + vals[n - 1]);
                else
                    writer.WriteLine(vals[n - 1]);
            }

            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private List<int[]> ReadResultsFeature(string filename)
        {
            StreamReader reader = new StreamReader(filename);

            string line = reader.ReadLine(); // Get rid of Layers line
            line = reader.ReadLine(); // Get rid of Overall accuracy line
            List<int[]> allValues = new List<int[]>();

            while ((line = reader.ReadLine()) != null && line != "")
            {
                if (line.Contains("Accuracy"))
                {
                    int startIndex = line.IndexOf("(") + 1;
                    int endIndex = line.IndexOf(" ", startIndex);
                    int correct = Convert.ToInt32(line.Substring(startIndex, endIndex - startIndex));
                    startIndex = line.IndexOf("/") + 2;
                    endIndex = line.IndexOf(")", startIndex);
                    int total = Convert.ToInt32(line.Substring(startIndex, endIndex - startIndex));
                    int[] values = new int[2];

                    if (allValues.Count == 1)
                    {
                        values[1] = correct;
                        values[0] = total - correct;
                    }
                    else
                    {
                        values[0] = correct;
                        values[1] = total - correct;
                    }

                    allValues.Add(values);
                }
            }

            if (allValues.Count > 2)
            {
                allValues.Clear();

                while ((line = reader.ReadLine()) == "")
                {
                    // Do nothing
                }

                while (line != null && line != "")
                {
                    string[] numsString = line.Split(" ".ToCharArray());
                    List<string> numsStringL = new List<string>(numsString);
                    numsStringL.RemoveAll(IsEmpty);
                    int[] values = new int[numsStringL.Count];
                    for (int i = 0; i < numsStringL.Count; i++)
                        values[i] = Convert.ToInt32(numsStringL[i]);

                    allValues.Add(values);

                    line = reader.ReadLine();
                }
            }

            reader.Close();

            return allValues;
        }

        #endregion


        #region Clustering

        /// <summary>
        /// Featurize the current sketch, Load the appropriate NN, and then Cluster the sketch
        /// </summary>
        private bool ClusterSketch()
        {
            if (_StrokeClassifications.Count == 0)
            {
                bool classified = ClassifySketch();
                if (!classified)
                {
                    StatusText.Text = "Unable to classify sketch --- Ready";
                    return false;
                }
            }

            this.StatusText.Text = "Featurizing Sketch";
            this.Refresh();

            List<SketchClusterSet> clustersets = FeaturizeCurrentSketch_Cluster();

            string network = _ClusterNetworkDirectory + "\\" + _ClusterNetworkName + ".cluster.weights.txt";
            string network2 = _ClusterNetworkDirectory + "\\" + _Cluster2NetworkName + ".cluster.weights.txt";
            string network3 = _ClusterNetworkDirectory + "\\" + _Cluster3NetworkName + ".cluster.weights.txt";
            bool loaded1 = LoadNetwork(network);
            if (!loaded1)
            {
                StatusText.Text = "Unable to load appropriate cluster Network --- Ready";
                return false;
            }
            if (ThreeWay)
            {
                bool loaded2 = LoadNetwork(network2);
                if (!loaded2)
                {
                    StatusText.Text = "Unable to load appropriate cluster Network for 3Class_2 --- Ready";
                    return false;
                }
                bool loaded3 = LoadNetwork(network3);
                if (!loaded3)
                {
                    StatusText.Text = "Unable to load appropriate cluster Network for 3Class_3 --- Ready";
                    return false;
                }
            }

            int numFeatures;
            if (TwoWay)
            {
                _BestClusters = clustersets[0];
                _ClusterClassifications = _BestClusters.getBestClustering(_NetworkCluster_2Class, GetUseFeaturesCluster(out numFeatures));
                foreach (Cluster.Cluster c in _BestClusters.Clusters)
                    c.Type = _DomainInfo["2"].GetLabel(1);
            }
            else if (ThreeWay)
            {
                Dictionary<Guid, int> FirstClassifications = clustersets[0].getBestClustering(_NetworkCluster_3Class_1, GetUseFeaturesCluster(out numFeatures));
                Dictionary<Guid, int> SecondClassifications = clustersets[1].getBestClustering(_NetworkCluster_3Class_2, GetUseFeaturesCluster(out numFeatures));
                // Third class
                //Dictionary<Guid, int> ThirdClassifications = clustersets[2].getBestClustering(_NetworkCluster_3Class_3, GetUseFeaturesCluster(out numFeatures));

                // Set best clusters = 1st class (Text)
                foreach (Cluster.Cluster c in clustersets[0].Clusters)
                    c.Type = _DomainInfo["3"].GetLabel(1);

                _BestClusters = clustersets[0];
                _ClusterClassifications = FirstClassifications;

                // Add 2nd class (gates)
                foreach (Cluster.Cluster c in clustersets[1].Clusters)
                {
                    c.Type = _DomainInfo["3"].GetLabel(2);
                    c.ClassificationNum += _BestClusters.Clusters.Count;
                }

                
                foreach (Cluster.Cluster c in clustersets[1].Clusters)
                {
                    _BestClusters.Clusters.Add(c);
                    _ClusterClassifications.Add(c.Id, c.ClassificationNum);
                }

                /*// Add 3rd class (Connectors)
                foreach (Cluster.Cluster c in clustersets[2].Clusters)
                {
                    c.Type = _DomainInfo["3"].GetLabel(0);
                    c.ClassificationNum += _BestClusters.Clusters.Count;
                }

                foreach (Cluster.Cluster c in clustersets[2].Clusters)
                {
                    _BestClusters.Clusters.Add(c);
                    _ClusterClassifications.Add(c.Id, c.ClassificationNum);
                }*/
            }

            DisplayBestCluster();

            foreach (Substroke s in _Sketch.SubstrokesL)
            {
                int count = 0;
                foreach (Cluster.Cluster c in _BestClusters.Clusters)
                {
                    if (c.contains(s))
                        count++;
                }

                if (count > 1)
                    System.Console.WriteLine(_SketchName);
            }

            UpdateColors();

            StatusText.Text = "Ready";
            this.Refresh();

            return true;
        }

        /// <summary>
        /// Select sketches to open and creates a training file for a "clustering" NN
        /// </summary>
        private void FeaturizeClusteringMultipleSketches()
        {
            bool openSuccessful;
            List<string> FilesToOpen = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to Open Files, no action taken.";
                return;
            }

            bool saveSuccessful;
            string outputFilename = SelectSaveFeatureFile(out saveSuccessful);
            if (!saveSuccessful)
            {
                StatusText.Text = "No File selected to save features to, no action taken.";
                return;
            }

            if (TwoWay)
            {
                if (!outputFilename.Contains("2Way"))
                    outputFilename = outputFilename.Insert(outputFilename.IndexOf(".cluster"), "2Way");
            }
            else if (ThreeWay)
            {
                if (!outputFilename.Contains("3Way"))
                    outputFilename = outputFilename.Insert(outputFilename.IndexOf(".cluster"), "3Way");
            }

            WriteClusterNNTrainingFiles(FilesToOpen, outputFilename);
        }

        /// <summary>
        /// Selects a large number of sketches and creates training files
        /// that are per-user so that a user-holdout can be done.
        /// </summary>
        private void FeaturizeClusteringSketchesUserHoldout()
        {
            bool openSuccessful;
            List<string> FilesToOpen = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to Open Files, no action taken.";
                return;
            }

            bool dirSuccessful;
            string dir = SelectDirectorySaveFeatureFile(out dirSuccessful);
            if (!dirSuccessful)
            {
                StatusText.Text = "No Directory selected, no action taken.";
                return;
            }

            Dictionary<string, List<string>> PerUserSketches = new Dictionary<string, List<string>>();
            List<string> userNumbers = new List<string>();

            foreach (string name in FilesToOpen)
            {
                string temp = System.IO.Path.GetFileName(name);
                string userNumber = temp.Substring(0, 2);
                if (PerUserSketches.ContainsKey(userNumber))
                    PerUserSketches[userNumber].Add(name);
                else
                {
                    List<string> filenames = new List<string>();
                    filenames.Add(name);
                    PerUserSketches.Add(userNumber, filenames);
                    userNumbers.Add(userNumber);
                }
            }

            foreach (string userNumber in userNumbers)
            {
                Application.DoEvents();
                List<string> userFileNames = PerUserSketches[userNumber];
                string outputFilename = "";

                if (TwoWay)
                    outputFilename = dir + "\\User" + userNumber + "_2Way.cluster.features.txt";
                else if (ThreeWay)
                    outputFilename = dir + "\\User" + userNumber + "_3Way.cluster.features.txt";

                WriteClusterNNTrainingFiles(userFileNames, outputFilename);
            }
        }

        /// <summary>
        /// Fill the Member SketchClusterSet with all strokes in the sketch
        /// </summary>
        /// <returns>The features from the SketchClusterSet</returns>
        private List<SketchClusterSet> FeaturizeCurrentSketch_Cluster()
        {
            _ClusterFeatures.Clear();
            _Index2Substroke.Clear();
            List<SketchClusterSet> clusterSets = new List<SketchClusterSet>();
            // Features to use:
            //   0: minDistance
            //   1: timeGap
            //   2: horizontalOverlap
            //   3: verticalOverlap
            //   4: maxDistance
            //   5: centroidDistance
            //   6: Distance 1 ratio
            //   7: Distance 2 ratio
            if (TwoWay)
            {
                SketchClusterSet temp = new SketchClusterSet(_Sketch, _StrokeClassifications, _DomainInfo["2"].GetLabel(1));
                clusterSets.Add(temp);
                _ClusterFeatures.Add(_DomainInfo["2"].GetLabel(1), temp.InterStrokeFeatures);
                _Index2Substroke.Add(_DomainInfo["2"].GetLabel(1), temp.Index2Substroke);
            }
            else if (ThreeWay)
            {
                SketchClusterSet temp_1 = new SketchClusterSet(_Sketch, _StrokeClassifications, _DomainInfo["3"].GetLabel(1));
                clusterSets.Add(temp_1);

                _ClusterFeatures.Add(_DomainInfo["3"].GetLabel(1), temp_1.InterStrokeFeatures);
                _Index2Substroke.Add(_DomainInfo["3"].GetLabel(1), temp_1.Index2Substroke);

                SketchClusterSet temp_2 = new SketchClusterSet(_Sketch, _StrokeClassifications, _DomainInfo["3"].GetLabel(2));
                clusterSets.Add(temp_2);

                _ClusterFeatures.Add(_DomainInfo["3"].GetLabel(2), temp_2.InterStrokeFeatures);
                _Index2Substroke.Add(_DomainInfo["3"].GetLabel(2), temp_2.Index2Substroke);

                SketchClusterSet temp_3 = new SketchClusterSet(_Sketch, _StrokeClassifications, _DomainInfo["3"].GetLabel(0));
                clusterSets.Add(temp_3);

                _ClusterFeatures.Add(_DomainInfo["3"].GetLabel(0), temp_3.InterStrokeFeatures);
                _Index2Substroke.Add(_DomainInfo["3"].GetLabel(0), temp_3.Index2Substroke);
            }
            else if (TwoXTwoWay)
            {
            }

            return clusterSets;
        }

        /// <summary>
        /// Perform a User Holdout Network Training for clustering
        /// </summary>
        private void TrainClusterNeuralNetworkHoldout()
        {
            bool openSuccessful;
            List<string> FeatureFiles = SelectOpenPerUserFeatureFiles(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Could not open the feature files, no action taken.";
                return;
            }

            string dir = System.IO.Path.GetDirectoryName(FeatureFiles[0]);
            string testFilename = dir + "\\tempTest.cluster.features.txt";
            string trainFilename = dir + "\\tempTrain.cluster.features.txt";
            int numFiles = FeatureFiles.Count;

            // i = file number to hold out for training
            if (TwoWay)
            {
                for (int i = 0; i < numFiles; i++)
                {
                    System.IO.File.Copy(FeatureFiles[i], testFilename, true);
                    bool first = true;

                    for (int j = 0; j < numFiles; j++)
                    {
                        if (i != j)
                        {
                            if (first)
                            {
                                first = false;
                                System.IO.File.Copy(FeatureFiles[i], trainFilename, true);
                            }
                            else
                                AppendTrainingPerUserInterstroke(FeatureFiles[j], trainFilename);
                        }
                    }

                    // Train NN
                    this.StatusText.Text = "Training Clustering Features Network...";
                    this.Refresh();
                    string outputFilename = FeatureFiles[i];
                    outputFilename = outputFilename.Replace("FeatureFiles", "Networks");
                    outputFilename = outputFilename.Replace(".cluster.features.", "_holdout.cluster.weights.");
                    _NetworkCluster_2Class.TrainNN(trainFilename, outputFilename, NumTrainingEpochs);

                    // Test NN
                    this.StatusText.Text = "Testing Clustering Features Network...";
                    this.Refresh();
                    outputFilename = outputFilename.Replace("Networks", "Results");
                    outputFilename = outputFilename.Replace("weights", "results");
                    _NetworkCluster_2Class.TestNN(testFilename, outputFilename);

                    // Done with this holdout
                }
            }
            else if (ThreeWay)
            {
                List<string> FeatureFiles_1 = new List<string>();
                List<string> FeatureFiles_2 = new List<string>();
                List<string> FeatureFiles_3 = new List<string>();

                foreach (string filename in FeatureFiles)
                {
                    if (filename.Contains("3Way_1"))
                        FeatureFiles_1.Add(filename);
                    else if (filename.Contains("3Way_2"))
                        FeatureFiles_2.Add(filename);
                    else if (filename.Contains("3Way_3"))
                        FeatureFiles_3.Add(filename);
                }

                // Train Gates classifier
                for (int i = 0; i < FeatureFiles_1.Count; i++)
                {
                    if (System.IO.File.Exists(testFilename))
                        System.IO.File.Delete(testFilename);
                    if (System.IO.File.Exists(trainFilename))
                        System.IO.File.Delete(trainFilename);

                    System.IO.File.Copy(FeatureFiles_1[i], testFilename, true);
                    bool first = true;

                    for (int j = 0; j < FeatureFiles_1.Count; j++)
                    {
                        if (i != j)
                        {
                            if (first)
                            {
                                first = false;
                                System.IO.File.Copy(FeatureFiles_1[j], trainFilename, true);
                            }
                            else
                                AppendTrainingPerUserInterstroke(FeatureFiles_1[j], trainFilename);
                        }
                    }

                    // Train NN
                    this.StatusText.Text = "Training Clustering Features Network...";
                    this.Refresh();
                    string outputFilename = FeatureFiles_1[i];
                    outputFilename = outputFilename.Replace("FeatureFiles", "Networks");
                    outputFilename = outputFilename.Replace(".cluster.features.", "_holdout.cluster.weights.");
                    _NetworkCluster_3Class_1.TrainNN(trainFilename, outputFilename, NumTrainingEpochs);

                    // Test NN
                    this.StatusText.Text = "Testing Clustering Features Network...";
                    this.Refresh();
                    outputFilename = outputFilename.Replace("Networks", "Results");
                    outputFilename = outputFilename.Replace("weights", "results");
                    _NetworkCluster_3Class_1.TestNN(testFilename, outputFilename);

                    // Done with this holdout
                }

                // Train Text classifier
                for (int i = 0; i < FeatureFiles_2.Count; i++)
                {
                    if (System.IO.File.Exists(testFilename))
                        System.IO.File.Delete(testFilename);
                    if (System.IO.File.Exists(trainFilename))
                        System.IO.File.Delete(trainFilename);

                    System.IO.File.Copy(FeatureFiles_2[i], testFilename, true);
                    bool first = true;

                    for (int j = 0; j < FeatureFiles_2.Count; j++)
                    {
                        if (i != j)
                        {
                            if (first)
                            {
                                first = false;
                                System.IO.File.Copy(FeatureFiles_2[j], trainFilename, true);
                            }
                            else
                                AppendTrainingPerUserInterstroke(FeatureFiles_2[j], trainFilename);
                        }
                    }

                    // Train NN
                    this.StatusText.Text = "Training Clustering Features Network...";
                    this.Refresh();
                    string outputFilename = FeatureFiles_2[i];
                    outputFilename = outputFilename.Replace("FeatureFiles", "Networks");
                    outputFilename = outputFilename.Replace(".cluster.features.", "_holdout.cluster.weights.");
                    _NetworkCluster_3Class_2.TrainNN(trainFilename, outputFilename, NumTrainingEpochs);

                    // Test NN
                    this.StatusText.Text = "Testing Clustering Features Network...";
                    this.Refresh();
                    outputFilename = outputFilename.Replace("Networks", "Results");
                    outputFilename = outputFilename.Replace("weights", "results");
                    _NetworkCluster_3Class_2.TestNN(testFilename, outputFilename);

                    // Done with this holdout
                }

                // Train Connectors classifier
                for (int i = 0; i < FeatureFiles_3.Count; i++)
                {
                    if (System.IO.File.Exists(testFilename))
                        System.IO.File.Delete(testFilename);
                    if (System.IO.File.Exists(trainFilename))
                        System.IO.File.Delete(trainFilename);

                    System.IO.File.Copy(FeatureFiles_3[i], testFilename, true);
                    bool first = true;

                    for (int j = 0; j < FeatureFiles_3.Count; j++)
                    {
                        if (i != j)
                        {
                            if (first)
                            {
                                first = false;
                                System.IO.File.Copy(FeatureFiles_3[j], trainFilename, true);
                            }
                            else
                                AppendTrainingPerUserInterstroke(FeatureFiles_3[j], trainFilename);
                        }
                    }

                    // Train NN
                    this.StatusText.Text = "Training Clustering Features Network...";
                    this.Refresh();
                    string outputFilename = FeatureFiles_3[i];
                    outputFilename = outputFilename.Replace("FeatureFiles", "Networks");
                    outputFilename = outputFilename.Replace(".cluster.features.", "_holdout.cluster.weights.");
                    _NetworkCluster_3Class_3.TrainNN(trainFilename, outputFilename, NumTrainingEpochs);

                    // Test NN
                    this.StatusText.Text = "Testing Clustering Features Network...";
                    this.Refresh();
                    outputFilename = outputFilename.Replace("Networks", "Results");
                    outputFilename = outputFilename.Replace("weights", "results");
                    _NetworkCluster_3Class_3.TestNN(testFilename, outputFilename);

                    // Done with this holdout
                }
            }
            else
                return;
            this.StatusText.Text = "Ready";
            this.Refresh();
        }

        /// <summary>
        /// Append to a temporary file that will be used for training the NN
        /// while doing a user holdout study.
        /// </summary>
        /// <param name="FeatureFilename">Filename of a user's .cluster.features.txt file who is NOT being held out</param>
        /// <param name="TrainFilename">Filename of temp training file</param>
        private void AppendTrainingPerUserInterstroke(string FeatureFilename, string TrainFilename)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(FeatureFilename);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(TrainFilename, true);

            string line = reader.ReadLine(); // get rid of numInputs
            line = reader.ReadLine(); // get rid of numOutputs
            line = reader.ReadLine();

            while (line != null)
            {
                writer.WriteLine(line);
                line = reader.ReadLine();
            }

            reader.Close();
            writer.Close();
        }

        /// <summary>
        /// Run a cluster accuracy results test, using networks trained from user holdout
        /// </summary>
        private void RunClusterAccuracy()
        {
            bool openSuccessful;
            List<string> sketchFiles = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open sketches, no action taken --- Ready";
                this.Refresh();
                return;
            }

            bool saveSuccessful;
            string saveFile = SelectSaveClusterResultsFile(out saveSuccessful);
            if (!saveSuccessful)
            {
                StatusText.Text = "Unable to get file to save results to, no action taken --- Ready";
                this.Refresh();
                return;
            }

            Dictionary<string, ClusterResults> Results = new Dictionary<string, ClusterResults>(sketchFiles.Count);
            List<string> names = new List<string>(sketchFiles.Count);

            for (int i = 0; i < sketchFiles.Count; i++)
            {
                StatusText.Text = "Clustering Sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                this.Refresh();

                bool loadedSketch = LoadSketch(sketchFiles[i]);
                if (!loadedSketch)
                {
                    StatusText.Text = "Unable to load sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                FillInkOverlay(_Sketch);
                bool classified = ClassifySketch();
                if (!classified)
                {
                    StatusText.Text = "Unable to classify sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                bool clustered = ClusterSketch();
                if (!clustered)
                {
                    StatusText.Text = "Unable to cluster sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                names.Add(_SketchName);
                
                ClusterResults currentResults = ComputeClusterAccuracy();
                Results.Add(_SketchName, currentResults);
            }

            WriteClusterAccuracies(Results, names, saveFile);

            StatusText.Text = "Ready";
            this.Refresh();
        }

        private void RunClusterAccuracyNew()
        {
            bool openSuccessful;
            List<string> sketchFiles = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open sketches, no action taken --- Ready";
                this.Refresh();
                return;
            }

            bool saveSuccessful;
            string saveFile = SelectSaveClusterResultsFile(out saveSuccessful);
            if (!saveSuccessful)
            {
                StatusText.Text = "Unable to get file to save results to, no action taken --- Ready";
                this.Refresh();
                return;
            }

            Dictionary<string, ClusteringResultsSketch> Results = new Dictionary<string, ClusteringResultsSketch>(sketchFiles.Count);
            List<string> names = new List<string>(sketchFiles.Count);

            for (int i = 0; i < sketchFiles.Count; i++)
            {
                StatusText.Text = "Clustering Sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                this.Refresh();

                bool loadedSketch = LoadSketch(sketchFiles[i]);
                if (!loadedSketch)
                {
                    StatusText.Text = "Unable to load sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                FillInkOverlay(_Sketch);
                bool classified = ClassifySketch();
                if (!classified)
                {
                    StatusText.Text = "Unable to classify sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                bool clustered = ClusterSketch();
                if (!clustered)
                {
                    StatusText.Text = "Unable to cluster sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                names.Add(_SketchName);
                Cluster.ClusteringResultsSketch resultsFromClustering = new ClusteringResultsSketch(_Sketch, GetHandClusters(), _BestClusters.Clusters, _StrokeClassifications);
                Results.Add(_SketchName, resultsFromClustering);
            }

            //WriteClusterAccuracies(Results, names, saveFile);
            WriteClusterAccuraciesNew(Results, names, saveFile);

            StatusText.Text = "Ready";
            this.Refresh();
        }

        /// <summary>
        /// Find the hand and machine clusters, then get the ClusterResults from those
        /// </summary>
        /// <returns>Cluster Results for the current sketch</returns>
        private ClusterResults ComputeClusterAccuracy()
        {
            List<Shape> HandClusters = GetHandClusters();
            List<Cluster.Cluster> MachineClusters = new List<Cluster.Cluster>(_BestClusters.Clusters);

            ClusterResults results = new ClusterResults(HandClusters, MachineClusters);

            return results;
        }

        /// <summary>
        /// Get the hand labeled clusters in the sketch ('real' clusters)
        /// </summary>
        /// <returns>List of shapes which are the 'real' clusters</returns>
        private List<Shape> GetHandClusters()
        {
            List<Shape> handClusters = new List<Shape>();
            if (ThreeWay || TwoXTwoWay)
            {
                foreach (Shape s in _Sketch.ShapesL)
                {
                    if (IsGate(s))
                        handClusters.Add(s);
                    else if (IsLabel(s))
                        handClusters.Add(s);
                    else if (IsConnector(s))
                        handClusters.Add(s);
                }
            }
            else if (TwoWay)
            {
                List<Shape> labelShapes = new List<Shape>();
                List<Shape> labelBoxShapes = new List<Shape>();
                foreach (Shape s in _Sketch.ShapesL)
                {
                    if (IsGate(s))
                        handClusters.Add(s);
                    else if (IsLabel(s))
                    {
                        if (s.XmlAttrs.Type == "Label")
                            labelShapes.Add(s);
                        else if (s.XmlAttrs.Type == "LabelBox")
                            labelBoxShapes.Add(s);
                    }
                }

                if (labelBoxShapes.Count == 0)
                {
                    foreach (Shape s in labelShapes)
                        handClusters.Add(s);
                }
                else
                {
                    List<Guid> labelIDs = new List<Guid>(labelShapes.Count);
                    foreach (Shape label in labelShapes)
                        labelIDs.Add(label.Id);
                    List<Guid> labelBoxIDs = new List<Guid>(labelBoxShapes.Count);
                    foreach (Shape labelBox in labelBoxShapes)
                        labelBoxIDs.Add(labelBox.Id);

                    List<Shape> combinedShapes = new List<Shape>();
                    foreach (Shape label in labelShapes)
                    {
                        foreach (Shape box in labelBoxShapes)
                        {
                            if (ShapesOverlap(label, box))
                            {
                                XmlStructs.XmlShapeAttrs xmlAttrs = label.XmlAttrs;
                                List<Substroke> strokes = new List<Substroke>();
                                foreach (Substroke s in label.Substrokes)
                                    strokes.Add(s);
                                foreach (Substroke s in box.Substrokes)
                                    strokes.Add(s);
                                combinedShapes.Add(new Shape(strokes, xmlAttrs));
                                labelIDs.Remove(label.Id);
                                labelBoxIDs.Remove(box.Id);
                                break;
                            }
                        }
                    }

                    foreach (Shape s in combinedShapes)
                        handClusters.Add(s);
                    foreach (Shape s in labelShapes)
                    {
                        if (labelIDs.Contains(s.Id))
                            handClusters.Add(s);
                    }
                    foreach (Shape s in labelBoxShapes)
                    {
                        if (labelBoxIDs.Contains(s.Id))
                            handClusters.Add(s);
                    }
                }
            }

            return handClusters;
        }

        /// <summary>
        /// Determine whether two shapes overlap
        /// </summary>
        /// <param name="a">First Shape</param>
        /// <param name="b">Second Shape</param>
        /// <returns>Whether they overlap</returns>
        private bool ShapesOverlap(Shape a, Shape b)
        {
            Rectangle boxA = GetBoundingBox(a);
            Rectangle boxB = GetBoundingBox(b);

            return BoundingBoxOverlap(boxA, boxB, 100);
        }

        /// <summary>
        /// Determine whether two bounding boxes overlap with a fudge-factor
        /// </summary>
        /// <param name="a">Bounding Box of shape A</param>
        /// <param name="b">Bounding Box of shape B</param>
        /// <param name="fudgeFactor">Distance between boxes to consider as actual overlap</param>
        /// <returns>Whether they overlap</returns>
        private bool BoundingBoxOverlap(Rectangle a, Rectangle b, int fudgeFactor)
        {
            bool overlap = false;

            if ((a.X >= (b.X - fudgeFactor) && a.X <= (b.X + b.Width + fudgeFactor))
                || ((a.X + a.Width) >= (b.X - fudgeFactor) && (a.X + a.Width) <= (b.X + b.Width + fudgeFactor))
                || ((b.X >= (a.X - fudgeFactor)) && (b.X <= (a.X + a.Width + fudgeFactor))))  // overlap in x
            {
                if ((a.Y >= (b.Y - fudgeFactor) && a.Y <= (b.Y + b.Height + fudgeFactor))
                    || ((a.Y + a.Height) >= (b.Y - fudgeFactor) && (a.Y + a.Height) <= (b.Y + b.Height + fudgeFactor))
                    || (b.Y >= (a.Y - fudgeFactor) && b.Y <= (a.Y + a.Height + fudgeFactor)))  // overlap in y
                    overlap = true;
            }

            return overlap;
        }

        /// <summary>
        /// Gets the bounding box of the entire sketch.
        /// </summary>
        /// <returns>Bounding Box</returns>
        private System.Drawing.Rectangle GetBoundingBox(Shape s)
        {
            int x = (int)s.SubstrokesL[0].Points[0].X;
            int y = (int)s.SubstrokesL[0].Points[0].Y;
            int maxX = x;
            int maxY = y;

            foreach (Substroke stroke in s.SubstrokesL)
            {
                foreach (Sketch.Point pt in stroke.Points)
                {
                    x = Math.Min(x, (int)pt.X);
                    y = Math.Min(y, (int)pt.Y);
                    maxX = Math.Max(maxX, (int)pt.X);
                    maxY = Math.Max(maxY, (int)pt.Y);
                }
            }
            int width = maxX - x;
            int height = maxY - y;

            return new Rectangle(x, y, width, height);
        }

        #endregion


        #region Image-Based Recognition

        private void RecognizeClusters()
        {
            if (_StrokeFeatures.Count == 0)
            {
                ClassifySketch();
                ClusterSketch();
            }
            else if (_BestClusters == null)
                ClusterSketch();

            if (_ImageDefinitions.Count == 0)
                LoadImageDefinitions(GetCurrentUserDefinitionFile());

            this.StatusText.Text = "Recognizing Clusters...";
            this.Refresh();

            if (TwoWay)
            {
            }

            foreach (Cluster.Cluster c in this._BestClusters.Clusters)
            {
                BitmapSymbol UnknownSymbol = new BitmapSymbol(c.Strokes);
                UnknownSymbol.Process();
                Dictionary<string, List<SymbolRank>> SRs;
                if (c.Type == "Other")
                {
                    List<string> results = UnknownSymbol.FindSimilarity_and_Rank(_ImageDefinitions, out SRs);
                    if (results.Count > 0 && !_ImageRecoResult.ContainsKey(c.Id) && !_Cluster2Symbol.ContainsKey(c.Id))
                    {
                        this._ImageRecoResult.Add(c.Id, results[0]);
                        _FullImageRecoResults.Add(c.Id, SRs);
                        _Cluster2Symbol.Add(c.Id, UnknownSymbol);
                    }
                    else if (!_ImageRecoResult.ContainsKey(c.Id) && !_Cluster2Symbol.ContainsKey(c.Id))
                    {
                        this._ImageRecoResult.Add(c.Id, "-----");
                        this._FullImageRecoResults.Add(c.Id, SRs);
                        _Cluster2Symbol.Add(c.Id, UnknownSymbol);
                    }
                }
                else if (c.Type == "Label")
                {
                    if (!_ImageRecoResult.ContainsKey(c.Id) && !_Cluster2Symbol.ContainsKey(c.Id))
                    {
                        this._ImageRecoResult.Add(c.Id, "Label");
                        this._FullImageRecoResults.Add(c.Id, new Dictionary<string, List<SymbolRank>>());
                        _Cluster2Symbol.Add(c.Id, UnknownSymbol);
                    }
                }
            }

            this.StatusText.Text = "Ready";
            this.Refresh();
        }

        private void LoadImageDefinitions(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            this.StatusText.Text = "Loading Image Definitions...";
            this.statusStrip1.Refresh();
            this.Refresh();

            string line;
            while ((line = reader.ReadLine()) != null && line != "")
            {
                BitmapSymbol DefnSymbol = new BitmapSymbol();
                DefnSymbol.Input(line, _ImageDirectory + "\\");
                DefnSymbol.Process();
                _ImageDefinitions.Add(DefnSymbol);
            }

            reader.Close();
        }

        private string GetCurrentUserDefinitionFile()
        {
            string dir = _ImageDirectory + "\\";
            string userNumber = this._SketchName.Substring(0, 2);
            string platformShort = this._SketchName.Substring(this._SketchName.LastIndexOf("_") + 1, 1);
            string platform = "";
            if (platformShort == "T")
                platform = "Tablet";
            else if (platformShort == "P")
                platform = "Wacom";

            string filename = dir + "defns_User" + userNumber + "_" + platform + ".gate.txt";

            return filename;
        }

        /// <summary>
        /// Run the image-based recognizer on the actual gates, bypassing
        /// classification and clustering
        /// </summary>
        private Dictionary<Guid, double[][]> RecognizeGates()
        {
            if (_ImageDefinitions.Count == 0)
                LoadImageDefinitions(GetCurrentUserDefinitionFile());

            this.StatusText.Text = "Recognizing Shapes...";
            this.Refresh();
            Dictionary<Guid, double[][]> scores = new Dictionary<Guid, double[][]>();

            foreach (Shape shape in _Sketch.Shapes)
            {
                BitmapSymbol UnknownSymbol = new BitmapSymbol(shape.SubstrokesL);
                UnknownSymbol.Process();
                Dictionary<string, List<SymbolRank>> SRs;
                if (IsGate(shape))
                {
                    List<string> results = UnknownSymbol.FindSimilarity_and_Rank(_ImageDefinitions, out SRs);
                    if (results.Count > 0 && !_ImageRecoResult.ContainsKey(shape.Id) && !_Cluster2Symbol.ContainsKey(shape.Id))
                    {
                        this._ImageRecoResult.Add(shape.Id, results[0]);
                        this._FullImageRecoResults.Add(shape.Id, SRs);
                        _Cluster2Symbol.Add(shape.Id, UnknownSymbol);

                        double[][] score = new double[SRs.Count][];
                        for (int i = 0; i < SRs["Fusio"].Count; i++)
                            score[i] = BitmapSymbol.Compare(SRs["Fusio"][i].Symbol, UnknownSymbol);

                        scores.Add(shape.Id, score);
                    }
                    else if (!_ImageRecoResult.ContainsKey(shape.Id) && !_Cluster2Symbol.ContainsKey(shape.Id))
                    {
                        this._ImageRecoResult.Add(shape.Id, "-----");
                        this._FullImageRecoResults.Add(shape.Id, SRs);
                        _Cluster2Symbol.Add(shape.Id, UnknownSymbol);
                    }
                }
            }

            this.StatusText.Text = "Ready";
            this.Refresh();

            return scores;
        }

        #endregion


        #region Menu Items

        #region File

        private void openSketchToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenSketch();
        }

        private void loadDomainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDomain();
        }

        private void loadNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenNetwork();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion

        #region Single Stroke Classification

        private void classifySketchToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ClassifySketch();
        }

        private void featurizeOnUserBasisToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Labeled XML Sketches (*labeled.xml)|*labeled.xml";
            openDlg.Title = "Select Sketches to Featurize";
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                string dir = "";
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();
                folderDlg.Description = "Select Directory in which to place PerUser feature files";
                if (folderDlg.ShowDialog() == DialogResult.OK)
                    dir = folderDlg.SelectedPath;
                else
                    return;

                int numFeatures;
                bool[] useFeatures = GetUseFeaturesStroke(out numFeatures);
                for (int i = 0; i < useFeatures.Length; i++)
                {
                    if (!useFeatures[i])
                    {
                        numFeatures++;
                        useFeatures[i] = true;
                    }
                }

                string[] AllSketches = openDlg.FileNames;
                Dictionary<string, List<string>> PerUserSketches = new Dictionary<string, List<string>>(AllSketches.Length);
                List<string> userNumbers = new List<string>(AllSketches.Length);

                foreach (string name in AllSketches)
                {
                    string temp = System.IO.Path.GetFileName(name);
                    string userNumber = temp.Substring(0, 2);
                    if (PerUserSketches.ContainsKey(userNumber))
                    {
                        PerUserSketches[userNumber].Add(name);
                    }
                    else
                    {
                        List<string> filenames = new List<string>();
                        filenames.Add(name);
                        PerUserSketches.Add(userNumber, filenames);
                        userNumbers.Add(userNumber);
                    }
                }

                foreach (string userNumber in userNumbers)
                {
                    Application.DoEvents();
                    List<string> userFileNames = PerUserSketches[userNumber];

                    string filename = dir + "\\User" + userNumber + ".features.txt";

                    StreamWriter writer = new StreamWriter(filename);
                    int numInputs = 0;
                    for (int i = 0; i < useFeatures.Length; i++)
                    {
                        if (useFeatures[i])
                            numInputs++;
                    }
                    int numOutputs;
                    if (ThreeWay)
                        numOutputs = 3;
                    else
                        numOutputs = 1;

                    writer.WriteLine(numInputs.ToString());
                    writer.WriteLine(numOutputs.ToString());

                    writer.Close();


                    for (int i = 0; i < userFileNames.Count; i++)
                    {
                        bool loaded = LoadSketch(userFileNames[i]);

                        string txt = "Finding Stroke Features for Sketch # " + (i + 1).ToString() + " of " + userFileNames.Count.ToString();
                        this.StatusText.Text = txt;
                        this.Refresh();

                        _StrokeFeatures = StrokeFeatures.getMultiStrokeFeatures(_Sketch);

                        /*if (!allSubstrokes)
                        {
                            // Modify to take out certain strokeFeatures
                            List<StrokeFeatures> toBeRemoved = new List<StrokeFeatures>();
                            foreach (StrokeFeatures feature in this.strokeFeatures)
                            {
                                if (sketch.GetSubstroke(feature.Id).FirstLabelL == "wire")
                                    toBeRemoved.Add(feature);
                            }
                            foreach (StrokeFeatures feature in toBeRemoved)
                                this.strokeFeatures.Remove(feature);
                        }*/
                        if (ThreeWay)
                            StrokeFeatures.Print(_StrokeFeatures, filename,
                                _Sketch, _DomainInfo["3"].GetLabels(), false,
                                true, useFeatures, true);
                        else if (TwoWay)
                            StrokeFeatures.Print(_StrokeFeatures, filename,
                                _Sketch, _DomainInfo["2"].GetLabels(), false,
                                true, useFeatures, true);

                        this.inkPanel.Refresh();
                        this.Refresh();
                    }
                    this.StatusText.Text = "Ready";
                }
            
            }
        }       

        private void trainAndTestNNOnUserBasisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FeatureUserHoldoutTesting();
        }

        private void compileUserHoldoutResultsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<string> filenames = new List<string>();

            /*FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "Select the base path which contains "
                + "the directories with results files to compile";

            if (folderDlg.ShowDialog() == DialogResult.OK)
                filenames = new List<string>(System.IO.Directory.GetFiles(folderDlg.SelectedPath,
                    "*.results.txt", SearchOption.AllDirectories));
            else
                return;*/

            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Multiselect = true;
            openDlg.RestoreDirectory = true;
            openDlg.Filter = "Results text files (*.features.results.txt)|*.features.results.txt";

            if (openDlg.ShowDialog() == DialogResult.OK)
                filenames = new List<string>(openDlg.FileNames);
            else
                return;

            Dictionary<string, List<int[]>> Results = new Dictionary<string, List<int[]>>(filenames.Count);
            List<string> ResultKeys = new List<string>(filenames.Count);

            foreach (string name in filenames)
            {
                List<int[]> res = ReadResultsFeature(name);
                Results.Add(name, res);
                ResultKeys.Add(name);
            }

            string outputFilename;
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Title = "Select the filename to save the results to";
            saveDlg.Filter = "Text Files (*.txt)|*.txt";

            if (saveDlg.ShowDialog() == DialogResult.OK)
                outputFilename = saveDlg.FileName;
            else
                return;

            StreamWriter writer = new StreamWriter(outputFilename);
            foreach (string name in ResultKeys)
            {
                //int index = name.IndexOf("DISTANCE") - 4;
                //int index = name.IndexOf("Features");
                //string dThresh = name.Substring(index, 3);
                string dThresh = name;

                writer.Write(dThresh + "," + System.IO.Path.GetFileNameWithoutExtension(name).Substring(0, 6) + ",");
                foreach (int[] values in Results[name])
                {
                    foreach (int currentValue in values)
                    {
                        writer.Write(currentValue.ToString() + ",");
                    }
                    writer.Write(",");
                }
                writer.WriteLine();
            }
            writer.Close();
        }

        private void featurizeSketchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Labeled XML Sketches (*labeled.xml)|*labeled.xml";
            openDlg.Title = "Select Sketches to Featurize";
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                int numInputs;
                bool[] useFeatures = GetUseFeaturesStroke(out numInputs);

                int numOutputs;
                if (ThreeWay)
                    numOutputs = 3;
                else
                    numOutputs = 1;


                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.Filter = "Stroke Feature Text Files (*.features.txt)|*.features.txt";
                saveDlg.Title = "File to save TRAINING stroke features as";
                saveDlg.RestoreDirectory = true;
                saveDlg.AddExtension = true;
                string trainFilename = "";
                if (saveDlg.ShowDialog() == DialogResult.OK)
                    trainFilename = saveDlg.FileName;

                StreamWriter trainWriter = new StreamWriter(trainFilename);
                trainWriter.WriteLine(numInputs.ToString());
                trainWriter.WriteLine(numOutputs.ToString());
                trainWriter.Close();



                SaveFileDialog saveDlg2 = new SaveFileDialog();
                saveDlg2.Filter = "Stroke Feature Text Files (*.features.txt)|*.features.txt";
                saveDlg2.Title = "File to save TESTING stroke features as";
                saveDlg2.RestoreDirectory = true;
                saveDlg2.AddExtension = true;
                string testFilename = "";
                if (saveDlg2.ShowDialog() == DialogResult.OK)
                    testFilename = saveDlg2.FileName;

                StreamWriter testWriter = new StreamWriter(testFilename);
                testWriter.WriteLine(numInputs.ToString());
                testWriter.WriteLine(numOutputs.ToString());
                testWriter.Close();

                List<int> testFileNums = new List<int>((int)(openDlg.FileNames.Length * 0.2));
                while (testFileNums.Count < testFileNums.Capacity)
                {
                    Random random = new Random();
                    int r = random.Next(0, openDlg.FileNames.Length - 1);
                    if (!testFileNums.Contains(r))
                        testFileNums.Add(r);
                }


                for (int i = 0; i < openDlg.FileNames.Length; i++)
                {
                    bool loaded = LoadSketch(openDlg.FileNames[i]);

                    string txt = "Finding Stroke Features for Sketch # " + (i + 1).ToString() + " of " + openDlg.FileNames.Length.ToString();
                    this.StatusText.Text = txt;
                    this.Refresh();

                    _StrokeFeatures = StrokeFeatures.getMultiStrokeFeatures(_Sketch);
                    bool allSubstrokes = true;
                    if (testFileNums.Contains(i))
                    {
                        StrokeFeatures.Print(_StrokeFeatures, testFilename,
                            _Sketch, _DomainInfo["3"].GetLabels(), false,
                            true, useFeatures, allSubstrokes);
                    }
                    else
                    {
                        StrokeFeatures.Print(_StrokeFeatures, trainFilename,
                            _Sketch, _DomainInfo["3"].GetLabels(), false,
                            true, useFeatures, allSubstrokes);
                    }

                    this.inkPanel.Refresh();
                    this.Refresh();
                }
                this.StatusText.Text = "Ready";
            }
        }

        private void featurizeSelectedSketchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Labeled XML Sketches (*labeled.xml)|*labeled.xml";
            openDlg.Title = "Select Sketches to Featurize";
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                int numInputs;
                bool[] useFeatures = GetUseFeaturesStroke(out numInputs);

                int numOutputs;
                if (ThreeWay)
                    numOutputs = 3;
                else
                    numOutputs = 1;


                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.Filter = "Stroke Feature Text Files (*.features.txt)|*.features.txt";
                saveDlg.Title = "File to save TRAINING stroke features as";
                saveDlg.RestoreDirectory = true;
                saveDlg.AddExtension = true;
                string trainFilename = "";
                if (saveDlg.ShowDialog() == DialogResult.OK)
                    trainFilename = saveDlg.FileName;

                StreamWriter trainWriter = new StreamWriter(trainFilename);
                trainWriter.WriteLine(numInputs.ToString());
                trainWriter.WriteLine(numOutputs.ToString());
                trainWriter.Close();


                for (int i = 0; i < openDlg.FileNames.Length; i++)
                {
                    bool loaded = LoadSketch(openDlg.FileNames[i]);

                    string txt = "Finding Stroke Features for Sketch # " + (i + 1).ToString() + " of " + openDlg.FileNames.Length.ToString();
                    this.StatusText.Text = txt;
                    this.Refresh();

                    _StrokeFeatures = StrokeFeatures.getMultiStrokeFeatures(_Sketch);
                    bool allSubstrokes = true;
                    if (ThreeWay)
                    {
                        StrokeFeatures.Print(_StrokeFeatures, trainFilename,
                            _Sketch, _DomainInfo["3"].GetLabels(), false,
                            true, useFeatures, allSubstrokes);
                    }
                    else if (TwoWay)
                    {
                        StrokeFeatures.Print(_StrokeFeatures, trainFilename,
                            _Sketch, _DomainInfo["2"].GetLabels(), false,
                            true, useFeatures, allSubstrokes);
                    }

                    this.inkPanel.Refresh();
                    this.Refresh();
                }
                this.StatusText.Text = "Ready";
            }
        }

        private void trainNeuralNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Feature Files (*.features.txt)|*.features.txt";
            openDlg.Title = "Training File";
            openDlg.RestoreDirectory = true;

            string trainFile = "";
            if (openDlg.ShowDialog() == DialogResult.OK)
                trainFile = openDlg.FileName;
            else
                return;

            OpenFileDialog openDlg2 = new OpenFileDialog();
            openDlg2.Filter = "Feature Files (*.features.txt)|*.features.txt";
            openDlg2.Title = "Testing File";
            openDlg2.RestoreDirectory = true;

            string testFile = "";
            if (openDlg2.ShowDialog() == DialogResult.OK)
                testFile = openDlg2.FileName;
            else
                return;

            string outputFilename = trainFile.Replace(".features.txt", ".features.weights.txt");

            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(outputFilename)))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFilename));

            this.StatusText.Text = "Training Neural Network...";
            this.Refresh();

            if (ThreeWay)
                _NetworkStrokes_3Class.TrainNN(trainFile, outputFilename, _NumTrainingEpochs);
            else if (TwoWay)
                _NetworkStrokes_2Class.TrainNN(trainFile, outputFilename, _NumTrainingEpochs);

            // Test NN
            outputFilename = outputFilename.Replace("weights", "results");
            if (ThreeWay)
                _NetworkStrokes_3Class.TestNN(testFile, outputFilename);
            else if (TwoWay)
                _NetworkStrokes_2Class.TestNN(testFile, outputFilename);

            this.StatusText.Text = "Ready";
            this.Refresh();
        }

        private void testNeuralNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region Clustering

        private void clusterSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool clustered = ClusterSketch();
        }

        private void featurizeOnUserBasisToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FeaturizeClusteringSketchesUserHoldout();
        }

        private void trainAndTestNNOnUserBasisToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TrainClusterNeuralNetworkHoldout();
        }

        private void runClusteringAccuracyMenuStrip_Click(object sender, EventArgs e)
        {
            //RunClusterAccuracy();
            RunClusterAccuracyNew();
        }

        private void featurizeSketchesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FeaturizeClusteringMultipleSketches();
        }

        private void trainNeuralNetworkToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void testNeuralNetworkToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region Image-Based 
        
        private void recognizeClustersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecognizeClusters();
        }


        private void completeGatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool openSuccessful;
            List<string> sketches = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
                return;

            List<Guid> Ids = new List<Guid>();
            Dictionary<Guid, double[][]> AllScores = new Dictionary<Guid, double[][]>();
            Dictionary<Guid, Dictionary<string, List<SymbolRank>>> AllSRs = new Dictionary<Guid, Dictionary<string, List<SymbolRank>>>();

            foreach (string filename in sketches)
            {
                OpenSketch(filename);
                Dictionary<Guid, double[][]> scores = RecognizeGates();

                foreach (Shape s in _Sketch.Shapes)
                {
                    if (scores.ContainsKey(s.Id))
                    {
                        Ids.Add(s.Id);
                        AllScores.Add(s.Id, scores[s.Id]);
                        AllSRs.Add(s.Id, _FullImageRecoResults[s.Id]);
                    }
                }
            }
        }

        #endregion

        #region Options

        private void twoWayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (twoWayToolStripMenuItem.Checked)
            {
                threeWayToolStripMenuItem.Checked = false;
                twoXTwoWayToolStripMenuItem.Checked = false;
            }
            else
                threeWayToolStripMenuItem.Checked = true; // Default

            UpdateDirectories();
        }

        private void threeWayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (threeWayToolStripMenuItem.Checked)
            {
                twoWayToolStripMenuItem.Checked = false;
                twoXTwoWayToolStripMenuItem.Checked = false;
            }
            else
                threeWayToolStripMenuItem.Checked = true; // Default

            UpdateDirectories();
        }

        private void twoXTwoWayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (twoXTwoWayToolStripMenuItem.Checked)
            {
                twoWayToolStripMenuItem.Checked = false;
                threeWayToolStripMenuItem.Checked = false;
            }
            else
                threeWayToolStripMenuItem.Checked = true; // Default

            UpdateDirectories();
        }

        private void epochs500toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trainingEpochs500.Checked)
            {
                trainingEpochs1000.Checked = false;
                trainingEpochs2000.Checked = false;
                trainingEpochs5000.Checked = false;
            }
            else
                trainingEpochs1000.Checked = true; // Default
        }

        private void epochs1000toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trainingEpochs1000.Checked)
            {
                trainingEpochs500.Checked = false;
                trainingEpochs2000.Checked = false;
                trainingEpochs5000.Checked = false;
            }
            else
                trainingEpochs1000.Checked = true; // Default
        }

        private void epochs2000toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trainingEpochs2000.Checked)
            {
                trainingEpochs500.Checked = false;
                trainingEpochs1000.Checked = false;
                trainingEpochs5000.Checked = false;
            }
            else
                trainingEpochs1000.Checked = true; // Default
        }

        private void epochs5000toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trainingEpochs5000.Checked)
            {
                trainingEpochs500.Checked = false;
                trainingEpochs1000.Checked = false;
                trainingEpochs2000.Checked = false;
            }
            else
                trainingEpochs1000.Checked = true; // Default
        }

        private void useActualStrokeClassificationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (useActualStrokeClassificationsToolStripMenuItem.Checked)
                usePerfectStrokeClassificationsToolStripMenuItem.Checked = false;
            else
                useActualStrokeClassificationsToolStripMenuItem.Checked = true;
        }

        private void usePerfectStrokeClassificationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (usePerfectStrokeClassificationsToolStripMenuItem.Checked)
                useActualStrokeClassificationsToolStripMenuItem.Checked = false;
            else
                useActualStrokeClassificationsToolStripMenuItem.Checked = true;
        }

        #endregion

        


        #endregion

        private void inkPanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (_BestClusters != null && _ImageRecoResult.Count > 0)
                {
                    Font drawFont = new Font("Arial", 16);
                    foreach (Cluster.Cluster c in _BestClusters.Clusters)
                    {
                        string gateType = _ImageRecoResult[c.Id];
                        string modifier = "";

                        if (gateType.Contains("_no"))
                        {
                            int index = gateType.IndexOf("_no");
                            int index2 = gateType.IndexOf("_", index + 1);
                            modifier = gateType.Substring(index, index2 - index);
                        }

                        int len = gateType.IndexOf('_');
                        if (len >= 0)
                            gateType = gateType.Substring(0, len);

                        if (modifier != "")
                            gateType += modifier;

                        Color color = _Substroke2MStroke[c.Strokes[0].Id].DrawingAttributes.Color;
                        SolidBrush drawBrush = new SolidBrush(color);

                        int[] ids = new int[c.Strokes.Count];

                        for (int i = 0; i < c.Strokes.Count; i++)
                            ids[i] = _Substroke2MStroke[c.Strokes[i].Id].Id;

                        Microsoft.Ink.Strokes s = _OverlayInk.Ink.CreateStrokes(ids);

                        System.Drawing.Rectangle box = s.GetBoundingBox();

                        System.Drawing.Point location = box.Location;

                        System.Drawing.Point pt2 = new System.Drawing.Point(box.X + box.Width, box.Y + box.Height);

                        _OverlayInk.Renderer.InkSpaceToPixel(this.g, ref location);
                        _OverlayInk.Renderer.InkSpaceToPixel(this.g, ref pt2);

                        System.Drawing.Size size = new Size(pt2.X - location.X, pt2.Y - location.Y);

                        System.Drawing.Point writePt = new System.Drawing.Point(location.X, location.Y - 20);

                        e.Graphics.DrawString(gateType, drawFont, drawBrush, writePt);

                        e.Graphics.DrawRectangle(new Pen(drawBrush), new Rectangle(location, size));
                    }
                }
            }
            catch (Exception exc) { System.Console.WriteLine(exc.ToString()); }
        }

        private void inkPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_ImageRecoResult.Count == 0)
                return;

            System.Drawing.Point clickPt = new System.Drawing.Point(e.X, e.Y);
            _OverlayInk.Renderer.PixelToInkSpace(this.g, ref clickPt);
            clickPt.X = (int)((clickPt.X - (int)_InkMovedX) / _Scale);
            clickPt.Y = (int)((clickPt.Y - (int)_InkMovedY) / _Scale);

            foreach (Cluster.Cluster c in _BestClusters.Clusters)
            {
                if (c.BoundingBox.Contains(clickPt))
                {
                    int[] ids = new int[c.Strokes.Count];
                    for (int i = 0; i < c.Strokes.Count; i++)
                        ids[i] = _Substroke2MStroke[c.Strokes[i].Id].Id;

                    Microsoft.Ink.Strokes s = _OverlayInk.Ink.CreateStrokes(ids);

                    ClusterForm form = new ClusterForm(s, c.Strokes, _FullImageRecoResults[c.Id], _Cluster2Symbol[c.Id]);
                    form.Show();
                }
            }
        }

        private void showAllRecognizedGatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_ImageRecoResult.Count == 0)
                return;

            foreach (Cluster.Cluster c in _BestClusters.Clusters)
            {
                if (c.Type == "Other")
                {
                    int[] ids = new int[c.Strokes.Count];
                    for (int i = 0; i < c.Strokes.Count; i++)
                        ids[i] = _Substroke2MStroke[c.Strokes[i].Id].Id;

                    Microsoft.Ink.Strokes s = _OverlayInk.Ink.CreateStrokes(ids);

                    ClusterForm form = new ClusterForm(s, c.Strokes, _FullImageRecoResults[c.Id], _Cluster2Symbol[c.Id]);
                    form.Show();
                }
            }
        }

        private string GetGateNameAndType(string name)
        {
            int index_1 = name.IndexOf('_', 0);
            string gateType = name.Substring(0, index_1);

            int index_last = name.LastIndexOf('_');

            int index_2 = name.IndexOf('_', index_1 + 1);
            string userNum = name.Substring(index_1 + 1, index_2 - index_1);

            int index_3 = name.IndexOf('_', index_2 + 1);
            string platform = name.Substring(index_2 + 1, index_3 - index_2);

            int index_4 = name.IndexOf('_', index_3 + 1);
            string task = name.Substring(index_3 + 1, index_4 - index_3);

            if (index_last == index_4)
                return gateType;
            else
            {
                int index_5 = name.IndexOf('_', index_4 + 1);
                string modifier = name.Substring(index_4, index_5 - index_4);

                return gateType + modifier;
            }
        }

        private void WriteImageRecoAccuracyForShape(StreamWriter writer, Shape shape, ClusteringResultsShape result)
        {
            // 2: Perfectly matching cluster
            // 1: All strokes that were correctly classified were found, but some were incorrectly classified
            // 0: No strokes in the shape were correctly classified
            // -1: Error - Split or Merge
            if (result.Correctness > 0)
            {
                Cluster.Cluster c = result.BestCluster;
                if (_FullImageRecoResults.ContainsKey(c.Id))
                {
                    Dictionary<string, List<SymbolRank>> SRs = _FullImageRecoResults[c.Id];
                    if (SRs.ContainsKey("Fusio"))
                    {
                        string name0 = "";
                        string name1 = "";
                        string name2 = "";

                        if (SRs["Fusio"].Count > 0)
                            name0 = (GetGateNameAndType(SRs["Fusio"][0].SymbolName));

                        if (SRs["Fusio"].Count > 1)
                            name1 = (GetGateNameAndType(SRs["Fusio"][1].SymbolName));

                        if (SRs["Fusio"].Count > 2)
                            name2 = (GetGateNameAndType(SRs["Fusio"][2].SymbolName));

                        writer.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", _SketchName, shape.Label, result.Correctness, name0, name1, name2);
                    }
                }
            }
            else if (result.Correctness == 0)
            {
                writer.WriteLine("{0}, {1}, 0", _SketchName, shape.Label);
            }
            else if (result.Correctness == -2)
            {
                writer.WriteLine("{0}, {1}, -2", _SketchName, shape.Label);
            }
            else
            {
                Cluster.Cluster c = result.BestCluster;
                if (_FullImageRecoResults.ContainsKey(c.Id))
                {
                    Dictionary<string, List<SymbolRank>> SRs = _FullImageRecoResults[c.Id];
                    if (SRs.ContainsKey("Fusio"))
                    {
                        string[] names = new string[3];

                        if (SRs["Fusio"].Count > 0)
                            names[0] = (GetGateNameAndType(SRs["Fusio"][0].SymbolName));

                        if (SRs["Fusio"].Count > 1)
                            names[1] = (GetGateNameAndType(SRs["Fusio"][1].SymbolName));

                        if (SRs["Fusio"].Count > 2)
                            names[2] = (GetGateNameAndType(SRs["Fusio"][2].SymbolName));

                        List<Guid> clustersInvolved = new List<Guid>();
                        if (result.Errors.Count > 0)
                        {
                            foreach (ClusteringError error in result.Errors)
                            {
                                foreach (Cluster.Cluster clus in error.InvolvedClusters)
                                {
                                    if (!clustersInvolved.Contains(clus.Id) && clus.Id != c.Id)
                                        clustersInvolved.Add(clus.Id);
                                }
                            }
                        }

                        List<string[]> involvedClustersNames = new List<string[]>();
                        foreach (Guid clusterInvolved in clustersInvolved)
                        {
                            if (_FullImageRecoResults.ContainsKey(clusterInvolved))
                            {
                                Dictionary<string, List<SymbolRank>> otherSRs = _FullImageRecoResults[clusterInvolved];
                                if (otherSRs.ContainsKey("Fusio"))
                                {
                                    string[] otherNames = new string[3];

                                    if (otherSRs["Fusio"].Count > 0)
                                        otherNames[0] = (GetGateNameAndType(otherSRs["Fusio"][0].SymbolName));

                                    if (otherSRs["Fusio"].Count > 1)
                                        otherNames[1] = (GetGateNameAndType(otherSRs["Fusio"][1].SymbolName));

                                    if (otherSRs["Fusio"].Count > 2)
                                        otherNames[2] = (GetGateNameAndType(otherSRs["Fusio"][2].SymbolName));

                                    involvedClustersNames.Add(otherNames);
                                }
                            }
                        }

                        writer.Write("{0}, {1}, {2}, {3}, {4}, {5}", _SketchName, shape.Label, result.Correctness, names[0], names[1], names[2]);

                        foreach (string[] otherNames in involvedClustersNames)
                            writer.Write(", {0}, {1}, {2}", otherNames[0], otherNames[1], otherNames[2]);

                        writer.WriteLine();
                    }
                }
            }
        }

        private void ImageRecoAccuracy(StreamWriter writer)
        {
            RecognizeClusters();
            Cluster.ClusteringResultsSketch resultsCluster = new ClusteringResultsSketch(_Sketch, GetHandClusters(), _BestClusters.Clusters, _StrokeClassifications);

            List<ClusteringResultsShape> resultsShape = resultsCluster.ResultsShape;

            foreach (Shape shape in _Sketch.Shapes)
            {
                if (IsGate(shape))
                {
                    foreach (ClusteringResultsShape result in resultsShape)
                    {
                        if (shape == result.Shape)
                        {
                            WriteImageRecoAccuracyForShape(writer, shape, result);
                            break;
                        }
                    }
                }
            }

            foreach (Cluster.Cluster c in _BestClusters.Clusters)
            {
                if (c.Type == "Other")
                {
                    bool foundGate = false;
                    Dictionary<string, int> otherLabels = new Dictionary<string, int>();
                    foreach (Substroke s in c.Strokes)
                    {
                        if (otherLabels.ContainsKey(s.FirstLabel))
                            otherLabels[s.FirstLabel]++;
                        else
                            otherLabels.Add(s.FirstLabel, 1);

                        if (IsGate(s))
                        {
                            foundGate = true;
                            break;
                        }
                    }

                    string otherLabel = "";
                    SortedList<double, string> otherLabelOrder = new SortedList<double, string>();
                    foreach (KeyValuePair<string, int> pair in otherLabels)
                    {
                        double key = (double)pair.Value;
                        while (otherLabelOrder.ContainsKey(key))
                        {
                            key += 0.0001;
                        }
                        otherLabelOrder.Add(key, pair.Key);

                    }

                    int count = 0;
                    foreach (KeyValuePair<double, string> pair in otherLabelOrder)
                    {
                        count++;
                        if (count == otherLabelOrder.Count)
                            otherLabel += pair.Value;
                        else
                            otherLabel += pair.Value + "/";
                    }

                    if (!foundGate)
                    {
                        if (_FullImageRecoResults.ContainsKey(c.Id))
                        {
                            Dictionary<string, List<SymbolRank>> SRs = _FullImageRecoResults[c.Id];
                            if (SRs.ContainsKey("Fusio"))
                            {
                                string name0 = "";
                                string name1 = "";
                                string name2 = "";

                                if (SRs["Fusio"].Count > 0)
                                    name0 = (GetGateNameAndType(SRs["Fusio"][0].SymbolName));

                                if (SRs["Fusio"].Count > 1)
                                    name1 = (GetGateNameAndType(SRs["Fusio"][1].SymbolName));

                                if (SRs["Fusio"].Count > 2)
                                    name2 = (GetGateNameAndType(SRs["Fusio"][2].SymbolName));

                                writer.WriteLine("{0}, {1}, -3, {2}, {3}, {4}", _SketchName, otherLabel, name0, name1, name2);
                            }
                        }
                    }
                }
            }
        }

        private void clusteringAccuracyThisSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool classified = ClassifySketch();
            if (!classified)
            {
                StatusText.Text = "Unable to classify sketch";
                this.Refresh();
                return;
            }
            bool clustered = ClusterSketch();
            if (!clustered)
            {
                StatusText.Text = "Unable to cluster sketch";
                this.Refresh();
                return;
            }
            Cluster.ClusteringResultsSketch resultsFromClustering = new ClusteringResultsSketch(_Sketch, GetHandClusters(), _BestClusters.Clusters, _StrokeClassifications);

            ClusterAccuracyForm form = new ClusterAccuracyForm(resultsFromClustering);
            form.Show();
        }

        private void changeFilenamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.RestoreDirectory = true;
            openDlg.Multiselect = true;

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                List<string> filenames = new List<string>(openDlg.FileNames);

                foreach (string name in filenames)
                {
                    if (name.Contains("User") && name.Contains("-labeled"))
                    {
                        string temp = name.Replace("User", "");
                        temp = temp.Replace("-labeled", ".labeled");

                        System.IO.File.Copy(name, temp);
                    }
                }
            }
        }

        private void testMSTextVNonTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> names = SelectOpenMultipleSketches(out successful);
            if (!successful)
            {
                this.StatusText.Text = "Unable to open sketches...Ready";
                return;
            }

            bool successfulSave;
            string outputFilename = SelectSaveMSclassifierFile(out successfulSave);
            if (!successfulSave)
            {
                this.StatusText.Text = "Unable to create file to save results to...Ready";
                return;
            }

            Dictionary<string, int[,]> allResults = new Dictionary<string, int[,]>(names.Count);

            int i = 0;
            foreach (string name in names)
            {
                // Display status
                i++;
                string txt = "Finding MS Classifications for Sketch # " + i.ToString() + " of " + names.Count.ToString();
                this.StatusText.Text = txt;
                this.Refresh();

                // Load sketch and populate Microsoft Ink Overlay
                bool loaded = LoadSketch(name);
                if (!loaded)
                {
                    this.StatusText.Text = "Unable to load sketch " + name;
                    return;
                }
                FillInkOverlay(_Sketch);

                int[,] results = AnalyzeCurrentSketch(_OverlayInk.Ink);
                allResults.Add(name, results);
                
                this.Refresh();
                
            }

            StreamWriter writer = new StreamWriter(outputFilename);
            foreach (KeyValuePair<string, int[,]> result in allResults)
            {
                int[,] nums = result.Value;
                writer.Write(result.Key + ",");
                writer.Write(nums[0, 0].ToString() + ",");
                writer.Write(nums[0, 1].ToString() + ",,");
                writer.Write(nums[1, 0].ToString() + ",");
                writer.Write(nums[1, 1].ToString() + ",");
                writer.WriteLine(",," + nums[2, 0].ToString());
            }
            writer.Close();

            this.StatusText.Text = "Ready";
            this.Refresh();
        }

        private int[,] AnalyzeCurrentSketch(Microsoft.Ink.Ink theInk)
        {
            int[,] results = new int[3, 2];
            results[2, 0] = theInk.Strokes.Count;
            using (InkAnalyzer analyzer = new InkAnalyzer(theInk, this))
            {
                analyzer.AddStrokes(_OverlayInk.Ink.Strokes);
                analyzer.Analyze();
                int j = 0;

                ContextNodeCollection drawings = analyzer.FindNodesOfType(ContextNodeType.InkDrawing);
                foreach (ContextNode node in drawings)
                {
                    j++;
                    if (int2Color(j) == Color.Red)
                        j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        if (IsGate(_MStroke2Substroke[stroke.Id]) || 
                            IsConnector(_MStroke2Substroke[stroke.Id]) || 
                            IsOther(_MStroke2Substroke[stroke.Id]))
                            results[0, 0]++;
                        else
                            results[1, 0]++;

                        stroke.DrawingAttributes.Color = int2Color(j);
                    }
                }

                ContextNodeCollection inkwords = analyzer.FindNodesOfType(ContextNodeType.InkWord);
                foreach (ContextNode node in inkwords)
                {
                    j++;
                    if (int2Color(j) == Color.Red)
                        j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        if (IsLabel(_MStroke2Substroke[stroke.Id]))
                            results[1, 1]++;
                        else
                            results[0, 1]++;

                        stroke.DrawingAttributes.Color = int2Color(j);
                        stroke.DrawingAttributes.Height *= 3.0f;
                        stroke.DrawingAttributes.Width *= 3.0f;
                    }
                }

                ContextNodeCollection textwords = analyzer.FindNodesOfType(ContextNodeType.TextWord);
                foreach (ContextNode node in textwords)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        if (IsLabel(_MStroke2Substroke[stroke.Id]))
                            results[1, 1]++;
                        else
                            results[0, 1]++;

                        stroke.DrawingAttributes.Color = Color.Red;
                    }
                }

                

                ContextNodeCollection unclassifieds = analyzer.FindNodesOfType(ContextNodeType.UnclassifiedInk);
                foreach (ContextNode node in unclassifieds)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        stroke.DrawingAttributes.Color = Color.Red;
                        stroke.DrawingAttributes.Height *= 3.0f;
                        stroke.DrawingAttributes.Width *= 3.0f;
                    }
                }

                ContextNodeCollection images = analyzer.FindNodesOfType(ContextNodeType.Image);
                foreach (ContextNode node in images)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        stroke.DrawingAttributes.Color = Color.Red;
                        stroke.DrawingAttributes.Height *= 3.0f;
                        stroke.DrawingAttributes.Width *= 3.0f;
                    }
                }

                ContextNodeCollection inkbullets = analyzer.FindNodesOfType(ContextNodeType.InkBullet);
                foreach (ContextNode node in inkbullets)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                    {
                        if (IsLabel(_MStroke2Substroke[stroke.Id]))
                            results[1, 1]++;
                        else
                            results[0, 1]++;

                        stroke.DrawingAttributes.Color = Color.Red;
                        stroke.DrawingAttributes.Height *= 3.0f;
                        stroke.DrawingAttributes.Width *= 3.0f;
                    }
                }

                System.Console.WriteLine("Drawings.Count = " + drawings.Count.ToString());
                System.Console.WriteLine("InkWords.Count = " + inkwords.Count.ToString());
                System.Console.WriteLine("TextWords.Count = " + textwords.Count.ToString());
                System.Console.WriteLine("InkBullets.Count = " + inkbullets.Count.ToString());
                System.Console.WriteLine("Images.Count = " + images.Count.ToString());
                System.Console.WriteLine("UnClassified.Count = " + unclassifieds.Count.ToString());
                System.Console.WriteLine(); System.Console.WriteLine();

                if (textwords.Count > 0|| images.Count > 0 || unclassifieds.Count > 0)
                    MessageBox.Show("TextWords.Count = " + textwords.Count.ToString() + "   " +
                        "Images.Count = " + images.Count.ToString() + "   " + 
                        "UnClassifieds.Count = " + unclassifieds.Count.ToString());

                #region unused node collection types
                /*
                ContextNodeCollection lines = analyzer.FindNodesOfType(ContextNodeType.Line);
                foreach (ContextNode node in lines)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                        stroke.DrawingAttributes.Color = int2Color(j);
                }

                ContextNodeCollection objects = analyzer.FindNodesOfType(ContextNodeType.Object);
                foreach (ContextNode node in objects)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                        stroke.DrawingAttributes.Color = int2Color(j);
                }

                ContextNodeCollection paragraphs = analyzer.FindNodesOfType(ContextNodeType.Paragraph);
                foreach (ContextNode node in paragraphs)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                        stroke.DrawingAttributes.Color = int2Color(j);
                }

                ContextNodeCollection writingRegions = analyzer.FindNodesOfType(ContextNodeType.WritingRegion);
                foreach (ContextNode node in writingRegions)
                {
                    j++;
                    foreach (Microsoft.Ink.Stroke stroke in node.Strokes)
                        stroke.DrawingAttributes.Color = int2Color(j);
                }*/
                #endregion
            }

            return results;
        }

        private void createImagesOfClusteringForSketchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = SelectOpenMultipleSketches(out successful);
            if (!successful)
            {
                this.StatusText.Text = "Unable to open files...Ready";
                return;
            }

            bool successfulDir;
            string dir = SelectDirectorySaveImageFile(out successfulDir);
            if (!successfulDir)
            {
                this.StatusText.Text = "Unable to select appropriate directory for saving...Ready";
                return;
            }

            int i = 0;
            foreach (string file in filenames)
            {
                string text = "Creating image for file #";
                this.StatusText.Text = text + i.ToString() + "of " + filenames.Count.ToString();
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.Contains(".labeled"))
                    name = name.Substring(0, name.IndexOf(".labeled"));

                bool loaded = LoadSketch(file);
                if (!loaded)
                {
                    this.StatusText.Text = "Unable to load sketch " + name;
                    return;
                }

                FillInkOverlay(_Sketch);

                bool classified = ClassifySketch();
                if (!classified)
                {
                    this.StatusText.Text = "Unable to classify sketch " + name;
                    return;
                }

                bool clustered = ClusterSketch();
                if (!clustered)
                {
                    this.StatusText.Text = "Unable to cluster sketch " + name;
                    return;
                }

                string filename = dir + "\\" + name + ".jpg";
                CreateImage(filename, _OverlayInk);
                
            }

            this.StatusText.Text = "Ready";
        }

        private void CreateImage(string filename, InkOverlay ink)
        {
            byte[] image = ink.Ink.Save(PersistenceFormat.Gif);

            MemoryStream stream = new MemoryStream(image);
            stream.Position = 0;
            Bitmap InkImage = (Bitmap)Bitmap.FromStream(stream);

            InkImage.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void CreateImage(string filename, InkOverlay ink, System.Drawing.Imaging.ImageFormat format)
        {
            byte[] image = ink.Ink.Save(PersistenceFormat.Gif);

            MemoryStream stream = new MemoryStream(image);
            stream.Position = 0;
            Bitmap InkImage = (Bitmap)Bitmap.FromStream(stream);

            InkImage.Save(filename, format);
        }

        private void printInkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Drawing.Imaging.ImageFormat format;
            bool successful;
            string filename = SelectSaveImageFile(out successful, out format);
            if (!successful)
            {
                this.StatusText.Text = "File invalid...Ready";
                return;
            }

            CreateImage(filename, _OverlayInk, format);
        }

        #region Timing

        private void timeClassificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeSpan t = TimeToClassifySketch();

            MessageBox.Show("Time to classify = " + t.TotalMilliseconds.ToString());
        }

        private void timeClusteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_StrokeClassifications.Count == 0)
            {
                bool classified = ClassifySketch();
                if (!classified)
                {
                    StatusText.Text = "Unable to classify sketch --- Ready";
                    return;
                }
            }

            TimeSpan t = TimeToClusterSketch();

            MessageBox.Show("Time to classify = " + t.TotalMilliseconds.ToString());
        }

        private void timeClassificationClusteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this._StrokeClassifications.Clear();

            TimeSpan t = TimeToClusterSketch();

            MessageBox.Show("Time to classify = " + t.TotalMilliseconds.ToString());
        }

        private TimeSpan TimeToClassifySketch()
        {
            DateTime startTime = DateTime.Now;

            ClassifySketch();

            DateTime endTime = DateTime.Now;

            return endTime - startTime;
        }

        private TimeSpan TimeToClusterSketch()
        {
            DateTime startTime = DateTime.Now;

            ClusterSketch();

            DateTime endTime = DateTime.Now;

            return endTime - startTime;
        }

        private TimeSpan[] TimeToDoBoth()
        {
            DateTime start = DateTime.Now;

            TimeSpan t1 = TimeToClassifySketch();
            TimeSpan t2 = TimeToClusterSketch();

            DateTime end = DateTime.Now;
            TimeSpan t3 = end - start;

            return new TimeSpan[3] { t1, t2, t3 };
        }

        private void findAllTimesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool openSuccessful;
            List<string> sketchFiles = SelectOpenMultipleSketches(out openSuccessful);
            if (!openSuccessful)
            {
                StatusText.Text = "Unable to open sketches, no action taken --- Ready";
                this.Refresh();
                return;
            }

            bool saveSuccessful;
            string saveFile = SelectSaveFile(out saveSuccessful);
            if (!saveSuccessful)
            {
                StatusText.Text = "Unable to get file to save results to, no action taken --- Ready";
                this.Refresh();
                return;
            }

            Dictionary<string, TimeSpan> ClassifyTimes = new Dictionary<string, TimeSpan>(sketchFiles.Count);
            Dictionary<string, TimeSpan> ClusterTimes = new Dictionary<string, TimeSpan>(sketchFiles.Count);
            Dictionary<string, TimeSpan> BothTimes = new Dictionary<string, TimeSpan>(sketchFiles.Count);
            Dictionary<string, ulong> SketchTimes = new Dictionary<string, ulong>(sketchFiles.Count);
            List<string> names = new List<string>(sketchFiles.Count);

            for (int i = 0; i < sketchFiles.Count; i++)
            {
                StatusText.Text = "Testing Sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                this.Refresh();

                bool loadedSketch = LoadSketch(sketchFiles[i]);
                if (!loadedSketch)
                {
                    StatusText.Text = "Unable to load sketch # " + (i + 1).ToString() + " of " + sketchFiles.Count.ToString();
                    this.Refresh();
                    return;
                }
                FillInkOverlay(_Sketch);

                TimeSpan[] times = TimeToDoBoth();
                Sketch.Substroke endStroke = _Sketch.Substrokes[_Sketch.Substrokes.Length - 1];
                Sketch.Point endPt = endStroke.Points[endStroke.Points.Length - 1];

                ulong sketchTime = endPt.Time - _Sketch.Substrokes[0].Points[0].Time;

                names.Add(_SketchName);
                ClassifyTimes.Add(_SketchName, times[0]);
                ClusterTimes.Add(_SketchName, times[1]);
                BothTimes.Add(_SketchName, times[2]);
                SketchTimes.Add(_SketchName, sketchTime);
            }

            StreamWriter writer = new StreamWriter(saveFile);

            foreach (string name in names)
            {
                double classify = ClassifyTimes[name].Milliseconds + ClassifyTimes[name].Seconds * 1000.0 + ClassifyTimes[name].Minutes * 60.0 * 1000.0;
                double cluster = ClusterTimes[name].Milliseconds + ClusterTimes[name].Seconds * 1000.0 + ClusterTimes[name].Minutes * 60.0 * 1000.0;
                double both = BothTimes[name].Milliseconds + BothTimes[name].Seconds * 1000.0 + BothTimes[name].Minutes * 60.0 * 1000.0;
                
                writer.Write(System.IO.Path.GetFileNameWithoutExtension(name));
                writer.Write("," + classify.ToString("#0"));
                writer.Write("," + cluster.ToString("#0"));
                writer.Write("," + both.ToString("#0"));
                writer.Write("," + SketchTimes[name].ToString("0.0"));
                writer.WriteLine();
            }

            writer.Close();

            StatusText.Text = "Ready";
            this.Refresh();
        }

        #endregion

        private void loadXMLNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        { }

        private void reportAccuracyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> sketchFiles = SelectOpenMultipleSketches(out successful);
            if (!successful)
            {
                this.StatusText.Text = "Unable to select sketches";
                return;
            }

            bool saveFileSuccessful;
            string saveFile = SelectSaveImageAccuracyFile(out saveFileSuccessful);
            if (!saveFileSuccessful)
            {
                this.StatusText.Text = "Unable to select file to save results to";
                return;
            }

            StreamWriter writer = new StreamWriter(saveFile);
            writer.WriteLine("SketchFile, Correct Class - Shape, Cluster Correctness, Top Image Match, Image Match #2, Image Match #3");

            foreach (string file in sketchFiles)
            {
                OpenSketch(file);

                ImageRecoAccuracy(writer);
            }

            writer.Close();
        }

        /*private void loadXMLNetworkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successXML;
            string xmlFilename = SelectOpenFile(out successXML);
            if (!successXML)
                return;
            bool successXSD;
            string schemaFilename = SelectOpenFile(out successXSD);
            if (!successXSD)
                return;

            XMLReadWrite.NetworkXML nXML = new XMLReadWrite.NetworkXML(xmlFilename, schemaFilename);
            nXML.Read();
            Utilities.Neuro.ActivationNetwork network = nXML.Network;

            NeuralNetworkInfo info = new NeuralNetworkInfo(nXML.Info.Domain.ToString(), "SketchHoldout");
            info.UserID = Guid.NewGuid();
            info.UserName = "01";
            info.SketchID = Guid.NewGuid();
            info.SketchName = "01_COPY1_T";
            XMLReadWrite.NetworkXML writeXML = new XMLReadWrite.NetworkXML("C:\\text.xml", network, info);
            writeXML.Write();

            NeuralNetwork NN = new NeuralNetwork(network, info);

            // Serialize the NeuralNetwork
            Stream stream = File.Open("C:\\net.nn", FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();

            Console.WriteLine("Writing Network Information");
            bformatter.Serialize(stream, NN);
            stream.Close();

            // DeSerialize the NeuralNetwork
            NeuralNetwork a = null;
            stream = File.Open("C:\\net.nn", FileMode.Open);
            bformatter = new BinaryFormatter();

            Console.WriteLine("Reading Network Information");
            a = (NeuralNetwork)bformatter.Deserialize(stream);
            stream.Close();
        }*/

    }
}