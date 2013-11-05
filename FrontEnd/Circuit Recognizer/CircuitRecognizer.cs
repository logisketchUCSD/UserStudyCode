using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SketchPanelLib;
using Clusterer;
using DisplayManager;

namespace Circuit_Recognizer
{
    public partial class CircuitRecognizer : Form
    {
        #region Members

        SketchPanel m_Panel;

        DisplayManager.DisplayManager m_DisplayManager;

        Clusterer.Clusterer m_Clusterer;

        DateTime m_StartTime;
        DateTime m_EndTime;

        #endregion

        public CircuitRecognizer()
        {
            InitializeComponent();

            // Initialize the Panel for inking
            m_Panel = new SketchPanel();
            m_Panel.Parent = this;
            m_Panel.Top = this.Top + 25;
            m_Panel.Left = this.Left + 2;
            m_Panel.Height = this.Height - 82;
            m_Panel.Width = this.Width - 11;
            //m_Panel.Dock = DockStyle.Fill;
            m_Panel.BackColor = Color.White;
            m_Panel.BorderStyle = BorderStyle.Fixed3D;
            this.Resize += new EventHandler(CircuitRecognizer_Resize);

            //string netFile = "C:\\Documents and Settings\\eric\\Desktop\\3classFBD_Tom2.txt";
            //Utilities.Neuro.NeuralNetwork network = Utilities.Neuro.NeuralNetwork.CreateNetworkFromWekaOutput(netFile);
            
            string directory = System.IO.Directory.GetCurrentDirectory();
            string dir = directory.Substring(0, directory.IndexOf("\\Code\\") + 6);
            dir += "Recognition\\Settings";
            string SettingsFilename = dir + "\\" + "settings_current.txt"; //"settingsFBD.txt";
            Dictionary<string, string> filenames = ReadSettings(SettingsFilename);

            //network.Save(filenames["StrokeClassificationNetwork"]);
            StrokeClassifier.StrokeClassifierSettings sSettings = StrokeClassifier.StrokeClassifierSettings.MakeStrokeClassifierSettings(filenames);
            StrokeGrouper.StrokeGrouperSettings gSettings = StrokeGrouper.StrokeGrouperSettings.MakeStrokeGrouperSettings(filenames);
            Clusterer.ClustererSettings settings = new Clusterer.ClustererSettings(sSettings, gSettings);
            settings.AvgsAndStdDevs = Utilities.General.GetAvgsForSoftMax(filenames["FeatureAvgsAndStdDevs"]);
            

            // Get the saved Clusterer settings for m_Clusterer initialization
            string filename = "Settings_Statics.scs"; //"CircuitRecSettings.scs"; // "FBDRecSettings.scs";
            string path = System.IO.Directory.GetCurrentDirectory();
            path = path.Substring(0, path.IndexOf("\\Code\\") + 6);
            path += "Recognition\\Clusterer\\Clusterer\\";
            path += filename;

            settings.Save(path);

            ClustererSettings clustSettings = ClustererSettings.Load(path);
            path = path.Substring(0, path.IndexOf("\\Code\\") + 6);
            clustSettings.ImageAlignerFilename = path + "Recognition\\ImageAligner\\TrainedRecognizers\\ImageAlignerRecognizerNBEST_eric.iar";
            settings.ImageAlignerFilename = path + "Recognition\\ImageAligner\\TrainedRecognizers\\ImageAlignerRecognizerNBEST_eric.iar";

            // Initiliaze Clusterer
            m_Clusterer = new Clusterer.Clusterer(settings);
            m_Clusterer.FeaturizingDone += new FeaturizationCompleted(m_Clusterer_FeaturizingDone);
            m_Clusterer.StrokeClassificationDone += new StrokeClassificationCompleted(m_Clusterer_StrokeClassificationDone);
            m_Clusterer.InitialClustersDone += new InitialClustersCompleted(m_Clusterer_InitialClustersDone);
            m_Clusterer.FinalClustersDone += new FinalClustersCompleted(m_Clusterer_FinalClustersDone);
            
            // Plug clusterer into InkSketch events
            m_Panel.InkSketch.StrokeAdded += new msInkToHMCSketch.StrokeAddedEventHandler(InkSketch_StrokeAdded);
            m_Panel.InkSketch.StrokeRemoved += new msInkToHMCSketch.StrokeRemovedEventHandler(InkSketch_StrokeRemoved);
            m_Panel.InkSketch.SketchLoaded += new msInkToHMCSketch.SketchLoadedEventHandler(InkSketch_SketchLoaded);

            m_Panel.MouseDoubleClick += new MouseEventHandler(m_Panel_MouseDoubleClick);

            // Initialize the Display Manager
            Labeler.DomainInfo domain = Labeler.DomainInfo.LoadDomainInfo();
            m_DisplayManager = new DisplayManager.DisplayManager(ref m_Panel, domain);
        }

        void m_Panel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            m_Panel.InkSketch.DeleteInkStroke(m_Panel.InkSketch.Ink.Strokes[m_Panel.InkSketch.Ink.Strokes.Count - 1]);
            m_Panel.InkSketch.DeleteInkStroke(m_Panel.InkSketch.Ink.Strokes[m_Panel.InkSketch.Ink.Strokes.Count - 1]);

            System.Drawing.Point pt = e.Location;
            Graphics g = m_Panel.CreateGraphics();
            m_Panel.InkPicture.Renderer.PixelToInkSpace(g, ref pt);

            Sketch.Shape shape = null;
            if (shape == null)
                return;

            List<ImageAligner.ImageTemplateResult> results = m_Clusterer.Recognizer.Recognize(shape, 3);

            ImageAligner.ShapeResult result = new ImageAligner.ShapeResult(shape, null, null, results);

            AlignerResultsForm form = new AlignerResultsForm(result);
            form.Show();   
        }

        void CircuitRecognizer_Resize(object sender, EventArgs e)
        {
            m_Panel.Height = this.Height - 82;
            m_Panel.Width = this.Width - 11;
        }

        #region Events

        #region Clusterer

        void m_Clusterer_FeaturizingDone(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void m_Clusterer_StrokeClassificationDone(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        void m_Clusterer_InitialClustersDone(object sender, EventArgs e)
        {
            m_EndTime = DateTime.Now;
            long msLong = m_EndTime.Ticks - m_StartTime.Ticks;
            int ms = (int)(msLong / 10000);
            //Console.WriteLine("  Computed in {0} ms", ms);
            //throw new Exception("The method or operation is not implemented.");
        }

        void m_Clusterer_FinalClustersDone(object sender, EventArgs e)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region InkSketch

        void InkSketch_SketchLoaded()
        {
            ///throw new Exception("The method or operation is not implemented.");
        }

        void InkSketch_StrokeRemoved(Sketch.Substroke sub)
        {
            m_Clusterer.RemoveStroke(sub);
        }

        void InkSketch_StrokeAdded(Sketch.Stroke str)
        {
            //Console.Write("{0}", m_Panel.InkSketch.Ink.Strokes.Count);
            m_StartTime = DateTime.Now;
            m_Clusterer.AddStroke(str);
        }

        #endregion

        #region Clicks

        private void exitProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void classifySketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Status.Text = "Classifying Sketch...";
            this.Refresh();

            if (m_Clusterer.Classifier.NeedsUpdating)
                m_Clusterer.Classifier.AssignClassificationsToFeatureSketch();

            m_DisplayManager.updateSketch(m_Clusterer.FeatureSketch.Sketch);
            m_DisplayManager.displayClassification(m_Clusterer.Settings.ClassifierSettings.Domain.Class2Color);

            Status.Text = "";
            this.Refresh();
        }

        private void groupSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Status.Text = "Grouping Sketch...";
            this.Refresh();
            m_Clusterer.GroupSketch(m_Clusterer.Grouper.Results);

            m_DisplayManager.updateSketch(m_Clusterer.FeatureSketch.Sketch);
            m_DisplayManager.displayGroups();
            Status.Text = "";
            this.Refresh();
        }

        private void takeSnapshotOfSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Status.Text = "Saving Image...";
            byte[] image = m_Panel.InkPicture.Ink.Save(Microsoft.Ink.PersistenceFormat.Gif);

            System.IO.MemoryStream stream = new System.IO.MemoryStream(image);
            stream.Position = 0;
            Bitmap InkImage = (Bitmap)Bitmap.FromStream(stream);

            InkImage.Save("C:\\sketchImage.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            Status.Text = "";
        }

        private void openSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSketch();
        }

        private void OpenSketch()
        {
            bool success;
            string filename = Utilities.General.SelectOpenSketch(out success);
            if (!success)
                return;

            Status.Text = "Loading Sketch...";
            this.Refresh();

            string sketchFile = System.IO.Path.GetFileNameWithoutExtension(filename);

            //LoadAppropriateNetworks(sketchFile);

            m_Panel.loadSketch(filename, false, true);

            Status.Text = "Featurizing Sketch...";
            this.Refresh();

            m_Panel.InkSketch.Sketch.resetShapes();

            int index = sketchFile.IndexOf('_');
            int num;
            bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
            if (goodUser)
            {
                string user = num.ToString().PadLeft(index, '0');
                m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
            }

            Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                m_Clusterer.FeatureSketch.FeatureListSingle,
                m_Clusterer.FeatureSketch.FeatureListPair,
                m_Clusterer.Settings.AvgsAndStdDevs);
            m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);

            // HACK to use perfect classifications
            /*Dictionary<Sketch.Substroke, string> realClasses = new Dictionary<Sketch.Substroke, string>();
            Sketch.Sketch origSketch = new ConverterXML.ReadXML(filename).Sketch;
            origSketch = Utilities.General.ReOrderParentShapes(origSketch);
            origSketch = Utilities.General.LinkShapes(origSketch);
            foreach (Sketch.Substroke realStroke in origSketch.SubstrokesL)
            {
                Sketch.Substroke stroke = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(realStroke.Id);
                string cls = Utilities.General.GetClass(realStroke.FirstLabel);
                stroke.Classification = cls;
                realClasses.Add(stroke, cls);
            }
            m_Clusterer.Grouper.UpdateStrokeClassifications(realClasses);
            m_Clusterer.Grouper.Group();*/

            Status.Text = "";
            this.Refresh();
        }

        private void clearInkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Panel.InkSketch.Clear();
            m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings);

            m_Clusterer.FeaturizingDone += new FeaturizationCompleted(m_Clusterer_FeaturizingDone);
            m_Clusterer.StrokeClassificationDone += new StrokeClassificationCompleted(m_Clusterer_StrokeClassificationDone);
            m_Clusterer.InitialClustersDone += new InitialClustersCompleted(m_Clusterer_InitialClustersDone);
            m_Clusterer.FinalClustersDone += new FinalClustersCompleted(m_Clusterer_FinalClustersDone);
            // Plug clusterer into InkSketch events
            //m_Panel.InkSketch.StrokeAdded += new msInkToHMCSketch.StrokeAddedEventHandler(InkSketch_StrokeAdded);
            //m_Panel.InkSketch.StrokeRemoved += new msInkToHMCSketch.StrokeRemovedEventHandler(InkSketch_StrokeRemoved);
            //m_Panel.InkSketch.SketchLoaded += new msInkToHMCSketch.SketchLoadedEventHandler(InkSketch_SketchLoaded);

            // Initialize the Display Manager
            Labeler.DomainInfo domain = Labeler.DomainInfo.LoadDomainInfo();
            m_DisplayManager = new DisplayManager.DisplayManager(ref m_Panel, domain);
            
            this.Refresh();
        }

        #endregion

        #endregion

        #region Misc Functions

        public Dictionary<string, string> ReadSettings(string settingsFilename)
        {
            Dictionary<string, string> entries = new Dictionary<string, string>();
            Dictionary<string, string> filenames = new Dictionary<string, string>();
            System.IO.StreamReader reader = new System.IO.StreamReader(settingsFilename);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line != "" && line[0] != '%' && line.Contains("="))
                {
                    int index = line.IndexOf("=");
                    string key = line.Substring(0, index - 1);
                    string value = line.Substring(index + 2, line.Length - (index + 2));

                    if (!entries.ContainsKey(key))
                        entries.Add(key, value);
                }
            }

            reader.Close();

            if (entries.ContainsKey("Directory"))
            {
                string dir = System.IO.Path.GetDirectoryName(settingsFilename);
                dir += "\\" + entries["Directory"];
                entries.Remove("Directory");

                foreach (KeyValuePair<string, string> pair in entries)
                {
                    if (!pair.Key.Contains("Directory"))
                        filenames.Add(pair.Key, dir + pair.Value);
                    else
                        filenames.Add(pair.Key, "\\" + pair.Value);
                }
            }
            else
                MessageBox.Show("No Directory specified! (Key = Directory)");

            return filenames;
        }

        #endregion

        private void recognizeSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Status.Text = "Recognizing Components...";
            this.Refresh();

            m_Clusterer.RecognizeAndLabelSketch();

            m_DisplayManager.updateSketch(m_Clusterer.FeatureSketch.Sketch);
            m_DisplayManager.displayGroups();

            Status.Text = "";
            this.Refresh();
        }

        private void batchProcessGroupingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            List<string> sketchFiles = Utilities.General.SelectOpenFiles(out success,
                "Select sketches to group and then compare to actual labeled sketch",
                "Labeled XML Sketches (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            bool success2;
            string saveFile = Utilities.General.SelectSaveFile(out success2,
                "Select file to save classifications/changes to",
                "Plain Text File (*.txt)|*.txt");
            if (!success2)
                return;

            bool successMatrix;
            string matrixFile = Utilities.General.SelectSaveFile(out successMatrix,
                "File to save Confusion Matrix to",
                "Text Files (*.txt)|*.txt");
            if (!successMatrix)
                return;

            List<string> classes = new List<string>();
            classes.Add("Gate");
            classes.Add("Label");
            classes.Add("Wire");
            Utilities.ConfusionMatrix matrix = new Utilities.ConfusionMatrix(classes);

            Dictionary<string, Utilities.ConfusionMatrix> userMatrices = new Dictionary<string, Utilities.ConfusionMatrix>();
            Dictionary<string, Utilities.ConfusionMatrix> sketchMatrices = new Dictionary<string, Utilities.ConfusionMatrix>();
            Dictionary<Guid, string[]> allClassifications = new Dictionary<Guid, string[]>();

            Utilities.Domain domain = m_Clusterer.Settings.ClassifierSettings.Domain;


            Utilities.TestSketches outSketches = new Utilities.TestSketches();
            int n = 1;

            foreach (string file in sketchFiles)
            {
                Status.Text = "Loading Sketch " + n + " of " + sketchFiles.Count;
                this.Refresh();

                Sketch.Sketch sketch = new ConverterXML.ReadXML(file).Sketch;
                sketch = Utilities.General.ReOrderParentShapes(sketch);
                sketch = Utilities.General.LinkShapes(sketch);
                Sketch.Sketch origSketch = new Sketch.Sketch(sketch);
                //origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                //origSketch = Utilities.General.LinkShapes(origSketch);
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                origSketch.XmlAttrs.Name = name;
                sketch.resetShapes();
                LoadAppropriateNetworks(name);
                m_DisplayManager.updateSketch(sketch);

                foreach (Sketch.Substroke stroke in origSketch.SubstrokesL)
                    allClassifications.Add(stroke.Id, new string[] { domain.Shape2Class[stroke.FirstLabel], "", "" });

                Status.Text = "Featurizing Sketch " + n + " of " + sketchFiles.Count;
                this.Refresh();

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);
                int num;
                string user = "None";
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    user = num.ToString().PadLeft(index, '0');
                    m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
                }

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(sketch,
                    m_Clusterer.FeatureSketch.FeatureListSingle,
                    m_Clusterer.FeatureSketch.FeatureListPair,
                    m_Clusterer.Settings.AvgsAndStdDevs);

                m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);

                Status.Text = "Grouping Sketch " + n + " of " + sketchFiles.Count;
                this.Refresh();

                if (m_Clusterer.Classifier.NeedsUpdating)
                    m_Clusterer.Classifier.AssignClassificationsToFeatureSketch();

                foreach (Sketch.Substroke stroke in m_Clusterer.FeatureSketch.Sketch.SubstrokesL)
                    allClassifications[stroke.Id][1] = new string(stroke.Classification.ToCharArray());

                m_Clusterer.GroupSketch(m_Clusterer.Grouper.Results);

                // Hack to use perfect grouping
                /*List<Utilities.StrokePair> allPairs = new List<Utilities.StrokePair>();
                foreach (Utilities.StrokePair pair in m_Clusterer.Grouper.Results.Pairs("Gate"))
                    allPairs.Add(pair);
                foreach (Utilities.StrokePair pair in m_Clusterer.Grouper.Results.Pairs("Wire"))
                    allPairs.Add(pair);
                foreach (Utilities.StrokePair pair in m_Clusterer.Grouper.Results.Pairs("Label"))
                    allPairs.Add(pair);
                Dictionary<Utilities.StrokePair, double> pair2result = new Dictionary<Utilities.StrokePair,double>();
                foreach (Utilities.StrokePair pair in allPairs)
                {
                    if (origSketch.GetSubstroke(pair.Stroke1.Id).ParentShapes[0] == origSketch.GetSubstroke(pair.Stroke2.Id).ParentShapes[0])
                        pair2result.Add(pair, 1.0);
                    else
                        pair2result.Add(pair, 0.0);
                }
                StrokeGrouper.StrokeGrouperResult resultGroupPerfect = new StrokeGrouper.StrokeGrouperResult(
                    allPairs, pair2result, m_Clusterer.Classifier.Result.AllClassifications, m_Clusterer.Settings.ClassifierSettings.Domain); 
                m_Clusterer.GroupSketch(resultGroupPerfect);
                */


                // HACK to use perfect classifications
                //Dictionary<Sketch.Substroke, string> realClasses = new Dictionary<Sketch.Substroke, string>();
                //origSketch = new ConverterXML.ReadXML(file).Sketch;
                //origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                //origSketch = Utilities.General.LinkShapes(origSketch);
                /*foreach (Sketch.Substroke realStroke in origSketch.SubstrokesL)
                {
                    Sketch.Substroke stroke = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(realStroke.Id);
                    string clsA = Utilities.General.GetClass(realStroke.FirstLabel);
                    if (clsA == "Other")
                        clsA = "Label";
                    stroke.Classification = clsA;
                    realClasses.Add(stroke, clsA);
                }
                m_Clusterer.Grouper.UpdateStrokeClassifications(realClasses);
                m_Clusterer.Grouper.Group();*/
                // END HACK

                foreach (Sketch.Substroke stroke in m_Clusterer.FeatureSketch.Sketch.SubstrokesL)
                    allClassifications[stroke.Id][2] = new string(stroke.Classification.ToCharArray());

                m_DisplayManager.updateSketch(m_Clusterer.FeatureSketch.Sketch);
                m_DisplayManager.displayGroups();
                m_DisplayManager.displayClassification(m_Clusterer.Settings.ClassifierSettings.Domain.Class2Color);


                StrokeClassifier.StrokeClassifierResult result = m_Clusterer.Classifier.Result;
                Dictionary<Sketch.Substroke, string> cls = result.AllClassifications;

                if (!userMatrices.ContainsKey(user))
                    userMatrices.Add(user, new Utilities.ConfusionMatrix(classes));
                sketchMatrices.Add(name, new Utilities.ConfusionMatrix(classes));

                foreach (Sketch.Substroke stroke in origSketch.SubstrokesL)
                {
                    Sketch.Substroke s = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(stroke.Id);
                    string actual = domain.Shape2Class[stroke.FirstLabel];
                    matrix.Add(actual, cls[s]);
                    userMatrices[user].Add(actual, cls[s]);
                    sketchMatrices[name].Add(actual, cls[s]);
                }

                this.Refresh();

                List<Sketch.Shape> shapes = new List<Sketch.Shape>();
                foreach (Sketch.Shape shape in sketch.Shapes)
                {
                    //if (shape.Label != "Gate") continue;

                    shapes.Add(shape);
                }
                outSketches.Add(origSketch, shapes);
                
                n++;
            }

            Status.Text = "Saving Data";
            this.Refresh();

            // Write out SS classifications to see if any changed during grouping
            System.IO.StreamWriter writerChange = new System.IO.StreamWriter(saveFile);
            foreach (string[] stroke in allClassifications.Values)
                writerChange.WriteLine(stroke[0] + "\t" + stroke[1] + "\t" + stroke[2]);
            writerChange.Close();

            // Write out SS accuracies for aggregate
            System.IO.StreamWriter writer = new System.IO.StreamWriter(matrixFile);
            matrix.Print(writer);
            writer.Close();

            // Write out SS accuracies for each user
            string userFile = matrixFile.Replace(".txt", "_User.txt");
            System.IO.StreamWriter writerUser = new System.IO.StreamWriter(userFile);
            foreach (KeyValuePair<string, Utilities.ConfusionMatrix> kvp in userMatrices)
            {
                writerUser.WriteLine(kvp.Key);
                kvp.Value.Print(writerUser);
                writerUser.WriteLine();
            }
            writerUser.Close();

            // Write out SS accuracies for each sketch
            string sketchMatrixFile = matrixFile.Replace(".txt", "_Sketch.txt");
            System.IO.StreamWriter writerSketch = new System.IO.StreamWriter(sketchMatrixFile);
            foreach (KeyValuePair<string, Utilities.ConfusionMatrix> kvp in sketchMatrices)
            {
                writerSketch.WriteLine(kvp.Key);
                kvp.Value.Print(writerSketch);
                writerSketch.WriteLine();
            }
            writerSketch.Close();

            // NOT SAVING THE SKETCHES WITH GROUPED SHAPES
            //outSketches.Save(saveFile);

            TestGroupingAccuracy(outSketches);

            Status.Text = "";
            this.Refresh();
        }

        private void LoadAppropriateNetworks(string file)
        {
            int index = file.IndexOf('_');
            string userNum = file.Substring(0, index);

            string strokeNetworkDir = "NetworksStroke16New\\";
            string name = "Network" + userNum + ".nn";
            string baseDir = System.IO.Directory.GetCurrentDirectory();
            baseDir = baseDir.Substring(0, baseDir.IndexOf("Trunk\\"));
            string dir = baseDir + "Trunk\\Code\\Recognition\\Settings\\Initialization Files\\";
            string nameStroke = dir + strokeNetworkDir + name;
            string nameWire = dir + "NetworksWire\\" + name;
            string nameLabel = dir + "NetworksLabel\\" + name;
            string nameGate = dir + "NetworksGate\\" + name;

            string[] domainFiles = System.IO.Directory.GetFiles(dir + strokeNetworkDir, "*.dom");
            if (domainFiles.Length > 0)
            {
                Utilities.Domain domain = Utilities.Domain.LoadDomainFromFile(domainFiles[0]);
                m_Clusterer.Settings.ClassifierSettings.Domain = domain;
            }

            //m_Clusterer.Classifier.Settings.Network = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(nameStroke);
            //m_Clusterer.Grouper.Settings.Networks["Gate"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(nameGate);
            //m_Clusterer.Grouper.Settings.Networks["Label"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(nameLabel);
            //m_Clusterer.Grouper.Settings.Networks["Wire"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(nameWire);
        }

        private void recoAccuracyAfterGroupingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            string filename = Utilities.General.SelectOpenFile(out success,
                "Test Sketches with associated grouped shapes",
                "TestSketches (*.ts)|*.ts");
            if (!success)
                return;

            //string recoFilename = "C:\\Documents and Settings\\eric\\My Documents\\Trunk\\Code\\Recognition\\ImageAligner\\TrainedRecognizers\\ImageAlignerRecognizerNBEST.iar";
            //m_Clusterer.Recognizer = ImageAligner.ImageAlignerRecognizer.Load(recoFilename);
            Utilities.TestSketches sketches = Utilities.TestSketches.Load(filename);
            List<ImageAligner.TestSketchAccuracy> accuracies = new List<ImageAligner.TestSketchAccuracy>();
            List<ImageAligner.ShapeResult> allResults = new List<ImageAligner.ShapeResult>();
            foreach (KeyValuePair<Sketch.Sketch, List<Sketch.Shape>> kvp in sketches.Sketches)
            {
                
                Dictionary<Sketch.Shape, List<ImageAligner.ImageTemplateResult>> shapeResults = 
                    new Dictionary<Sketch.Shape, List<ImageAligner.ImageTemplateResult>>(kvp.Value.Count);

                foreach (Sketch.Shape shape in kvp.Value)
                {
                    List<ImageAligner.ImageTemplateResult> results = m_Clusterer.Recognizer.Recognize(shape, 3);
                    shapeResults.Add(shape, results);
                }

                ImageAligner.TestSketchAccuracy accuracy = new ImageAligner.TestSketchAccuracy(kvp.Key, shapeResults);
                accuracies.Add(accuracy);
                foreach (ImageAligner.ShapeResult result in accuracy.Results)
                    allResults.Add(result);
            }

            foreach (ImageAligner.ShapeResult result in allResults)
            {
                AlignerResultsForm form = new AlignerResultsForm(result);
                form.Show();
            }

            int total = allResults.Count;
            int perfect = 0;
            int humanCorrect = 0;
            int helpful = 0;
            int completenessCorrect = 0;
            int totallyOff = 0;
            Dictionary<string, int> labels = new Dictionary<string, int>();
            List<string> gateShapes = new List<string>(new string[] {
            "AND", "OR", "NAND", "NOR", "NOT", "NOTBUBBLE", "XOR", "LabelBox" });
            int[][] confusion = new int[gateShapes.Count][];

            for (int i = 0; i < gateShapes.Count; i++)
            {
                confusion[i] = new int[gateShapes.Count];
                labels.Add(gateShapes[i], i);
            }
            
            

            foreach (ImageAligner.ShapeResult result in allResults)
            {
                if (result.IsPerfect)
                    perfect++;

                if (result.IsHumanCorrect)
                    humanCorrect++;

                if (!result.IsPerfect && result.IsHumanCorrect)
                {
                    Console.Write("");
                }

                if (result.DoesTopResultGivePerfectGuidance)
                    helpful++;

                if (!Utilities.General.IsGate(result.ExpectedShapeName))
                    totallyOff++;

                if (result.IsCorrectCompleteness)
                    completenessCorrect++;

                if (result.BothShapesComplete && gateShapes.Contains(result.ExpectedShapeName))
                {
                    confusion[labels[result.ExpectedShapeName]][labels[result.TopResult.Name]]++;
                }
            }

            string fName = "C:\\results.txt";
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fName);
            writer.WriteLine("Total " + allResults.Count);
            writer.WriteLine("Perfect " + perfect);
            writer.WriteLine("HumanCorrect " + humanCorrect);
            writer.WriteLine("Helpful " + helpful);
            writer.WriteLine("TotallyOff " + totallyOff);
            writer.WriteLine("CompletenessCorrect " + completenessCorrect);
            writer.Write("Name");
            foreach (string lab in labels.Keys)
                writer.Write(" " + lab);
            writer.WriteLine();
            foreach (string lab in labels.Keys)
            {
                writer.Write(lab);
                for (int i = 0; i < gateShapes.Count; i++)
                    writer.Write(" " + confusion[labels[lab]][i]);
                writer.WriteLine();
            }
            writer.Close();

            PrintResults(allResults);

            List<ImageAligner.ShapeResult[]> toSerialize = new List<ImageAligner.ShapeResult[]>();
            ImageAligner.ShapeResult[] arr = new ImageAligner.ShapeResult[100];
            for (int i = 0; i < allResults.Count; i++)
            {
                int n = i % 100;
                if (n == 0)
                {
                    toSerialize.Add(arr);
                    arr = new ImageAligner.ShapeResult[100];
                }

                arr[n] = allResults[i];
            }

            int m = 0;
            foreach (ImageAligner.ShapeResult[] a in toSerialize)
            {
                m++;
                string saveName = "C:\\savedResults" + m.ToString() + ".shr";
                System.IO.Stream stream = System.IO.File.Open(saveName, System.IO.FileMode.Create);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, a);
                stream.Close();
            }
            
        }

        private void PrintResults(List<ImageAligner.ShapeResult> allResults)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\resultsText.txt");

            writer.WriteLine("Expected Shape; Expected Missing; Expected Extra; Recognized Shape; Recognized Missing; Recognized Extra; Recognized Score; Recognized Confidence;");

            foreach (ImageAligner.ShapeResult result in allResults)
            {
                writer.Write(result.ExpectedShapeName);
                writer.Write(";");
                foreach (ImageAligner.ImageMatchError error in result.ExpectedErrors)
                    if (error.Type == ImageAligner.ErrorType.Missing)
                        writer.Write(" " + error.Detail.ToString());
                writer.Write(";");
                foreach (ImageAligner.ImageMatchError error in result.ExpectedErrors)
                    if (error.Type == ImageAligner.ErrorType.Extra)
                        writer.Write(" " + error.Detail.ToString());
                writer.Write(";");

                if (result.TopResult == null)
                    continue;
                writer.Write(result.TopResult.Name);
                writer.Write(";");
                foreach (ImageAligner.ImageMatchError error in result.TopResult.Errors)
                    if (error.Type == ImageAligner.ErrorType.Missing)
                        writer.Write(" " + error.Detail.ToString());
                writer.Write(";");
                foreach (ImageAligner.ImageMatchError error in result.TopResult.Errors)
                    if (error.Type == ImageAligner.ErrorType.Extra)
                        writer.Write(" " + error.Detail.ToString());
                writer.Write(";");
                writer.Write(result.TopResult.Score);
                writer.Write(";");
                writer.Write(result.TopResult.Confidence);
                writer.WriteLine();
            }

            writer.Close();
        }

        private void loadResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string saveName = "C:\\savedResults.shr";
            ImageAligner.ShapeResult[] loaded;
            System.IO.Stream s = System.IO.File.Open(saveName, System.IO.FileMode.Open);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter b =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            loaded = (ImageAligner.ShapeResult[])b.Deserialize(s);
            s.Close();
        }

        private void trainWithWekaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successARFF;
            string arffFile = Utilities.General.SelectOpenFile(out successARFF,
                "Select the feature file to train from",
                "Weka input file (*.arff)|*.arff");
            if (!successARFF)
                return;

            bool successModel;
            string modelFile = Utilities.General.SelectSaveFile(out successModel,
                "Select the model file to save to",
                "Weka model file (*.model)|*.model");
            if (!successModel)
                return;

            Utilities.WekaWrap.Train(arffFile, modelFile, "adaboost_j48", 28);
        }

        private void batchTrainWithWekaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successARFF;
            List<string> files = Utilities.General.SelectOpenFiles(out successARFF,
                "Select the feature files to train from",
                "Weka input file (*.arff)|*.arff");
            if (!successARFF)
                return;

            bool successDir;
            string outputDir = Utilities.General.SelectDirectory(out successDir,
                "Select the directory to save the model to");
            if (!successDir)
                return;

            foreach (string arffFile in files)
            {
                int numFeatures = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(arffFile);
                string line;
                while ((line = reader.ReadLine()) != null && !line.Contains("@DATA"))
                {
                    if (line.Contains("@ATTRIBUTE"))
                        numFeatures++;
                }
                reader.Close();

                string modelFile = outputDir + "\\" + System.IO.Path.GetFileNameWithoutExtension(arffFile) + ".model";
                Utilities.WekaWrap.Train(arffFile, modelFile, "adaboost_j48", numFeatures);
            }
        }

        private void findGroupingAccuracyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            string filename = Utilities.General.SelectOpenFile(out success,
                "Test Sketches with associated grouped shapes",
                "TestSketches (*.ts)|*.ts");
            if (!success)
                return;

            Utilities.TestSketches sketches = Utilities.TestSketches.Load(filename);

            TestGroupingAccuracy(sketches);
        }

        private void TestGroupingAccuracy(Utilities.TestSketches sketches)
        {
            List<StrokeGrouper.GrouperAccuracy> accuracies = new List<StrokeGrouper.GrouperAccuracy>();
            List<StrokeGrouper.ShapeResult> allResults = new List<StrokeGrouper.ShapeResult>();

            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:\\actualPOV.txt");
            System.IO.StreamWriter writer2 = new System.IO.StreamWriter("C:\\groupsPOV.txt");

            foreach (KeyValuePair<Sketch.Sketch, List<Sketch.Shape>> kvp in sketches.Sketches)
            {
                StrokeGrouper.GrouperAccuracy accuracy = new StrokeGrouper.GrouperAccuracy(kvp.Key, kvp.Value);
                accuracies.Add(accuracy);

                foreach (StrokeGrouper.ShapeResult result in accuracy.Results)
                {
                    allResults.Add(result);
                    result.Print(writer2);
                }

                foreach (StrokeGrouper.ShapeResultFromActual result in accuracy.ResultsFromActual)
                {
                    result.Print(writer);
                }
            }
            writer.Close();
            writer2.Close();
        }

        private void findClassifierAccuracyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            List<string> sketchFiles = Utilities.General.SelectOpenFiles(out success,
                "Sketch files to open and process for classifier accuracy",
                "Labeled XML Files (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            
            bool successMatrix;
            string matrixFile = Utilities.General.SelectSaveFile(out successMatrix, 
                "File to save Confusion Matrix to",
                "Text Files (*.txt)|*.txt");
            if (!successMatrix)
                return;

            List<string> classes = m_Clusterer.Settings.ClassifierSettings.Domain.Classes;
            Utilities.ConfusionMatrix matrix = new Utilities.ConfusionMatrix(classes);

            foreach (string file in sketchFiles)
            {
                Sketch.Sketch origSketch = new ConverterXML.ReadXML(file).Sketch;
                origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                origSketch = Utilities.General.LinkShapes(origSketch);

                LoadAppropriateNetworks(System.IO.Path.GetFileNameWithoutExtension(file));

                m_Panel.loadSketch(file, false, true);

                Status.Text = "Featurizing Sketch...";
                this.Refresh();

                m_Panel.InkSketch.Sketch.resetShapes();

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);
                int num;
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    string user = num.ToString().PadLeft(index, '0');
                    m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
                }

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                    m_Clusterer.FeatureSketch.FeatureListSingle,
                    m_Clusterer.FeatureSketch.FeatureListPair,
                    m_Clusterer.Settings.AvgsAndStdDevs);
                m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);

                Status.Text = "";
                this.Refresh();

                StrokeClassifier.StrokeClassifierResult result = m_Clusterer.Classifier.Result;
                Dictionary<Sketch.Substroke, string> cls = result.AllClassifications;

                foreach (Sketch.Substroke stroke in origSketch.SubstrokesL)
                {
                    Sketch.Substroke s = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(stroke.Id);
                    string strokeClass = m_Clusterer.Settings.ClassifierSettings.Domain.Shape2Class[stroke.FirstLabel];
                    bool added = matrix.Add(strokeClass, cls[s]);
                    if (!added)
                        throw new Exception("Invalid class to add to confusion matrix");
                }

            }


            System.IO.StreamWriter writer = new System.IO.StreamWriter(matrixFile);
            matrix.Print(writer);
            writer.Close();
        }

        private void trainSSWithWekaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successARFF;
            string arffFile = Utilities.General.SelectOpenFile(out successARFF,
                "Select the feature file to train from",
                "Weka input file (*.arff)|*.arff");
            if (!successARFF)
                return;

            bool successModel;
            string modelFile = Utilities.General.SelectSaveFile(out successModel,
                "Select the model file to save to",
                "Weka model file (*.model)|*.model");
            if (!successModel)
                return;

            Utilities.WekaWrap.Train(arffFile, modelFile, "adaboost_j48", 17);
        }

        private void batchTrainSSWithWekaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successARFF;
            List<string> files = Utilities.General.SelectOpenFiles(out successARFF,
                "Select the feature files to train from",
                "Weka input file (*.arff)|*.arff");
            if (!successARFF)
                return;

            bool successDir;
            string outputDir = Utilities.General.SelectDirectory(out successDir,
                "Select the directory to save the model to");
            if (!successDir)
                return;

            foreach (string arffFile in files)
            {
                string modelFile = outputDir + "\\" + System.IO.Path.GetFileNameWithoutExtension(arffFile) + ".model";
                Utilities.WekaWrap.Train(arffFile, modelFile, "adaboost_j48", 17);
            }
        }

        private void testGroupingClassifierWekaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successARFF;
            List<string> files = Utilities.General.SelectOpenFiles(out successARFF,
                "Select the feature files to test from",
                "Weka input file (*.arff)|*.arff");
            if (!successARFF)
                return;

            bool successAll;
            string allFile = Utilities.General.SelectOpenFile(out successAll,
                "Select the feature file with all examples",
                "Weka input file (*.arff)|*.arff");
            if (!successAll)
                return;

            bool successModels;
            List<string> models = Utilities.General.SelectOpenFiles(out successModels,
                "Select the trained models to use",
                "Weka Model File (*.model)|*.model");
            if (!successModels)
                return;

            bool successSave;
            string saveFile = Utilities.General.SelectSaveFile(out successSave,
                "Select file to save the confusion matrix to",
                "Text File (*.txt)|*.txt");
            if (!successSave)
                return;

            string className = "NA";
            if (System.IO.Path.GetFileName(allFile).Contains("Wire"))
                className = "Wire";
            else if (System.IO.Path.GetFileName(allFile).Contains("Label"))
                className = "Label";
            else if (System.IO.Path.GetFileName(allFile).Contains("Gate"))
                className = "Gate";

            Dictionary<string, double> maxCentDist = new Dictionary<string, double>();
            maxCentDist.Add("Gate", 0.15);
            maxCentDist.Add("Label", 0.07);
            maxCentDist.Add("Wire", 1.00);

            List<string> classes = new List<string>();
            classes.Add("JoinGate");
            classes.Add("JoinLabel");
            classes.Add("JoinWire");
            classes.Add("NoJoin");
            classes.Add("Ignore");
            Utilities.ConfusionMatrix matrix = new Utilities.ConfusionMatrix(classes);

            System.IO.StreamReader readerAll = new System.IO.StreamReader(allFile);
            List<string> allFeatures = new List<string>();
            List<string> featureNames = new List<string>();
            string lineAll;
            while ((lineAll = readerAll.ReadLine()) != null)
            {
                if (lineAll.Contains("@ATTRIBUTE") && !lineAll.Contains("class"))
                {
                    string feat = lineAll.Substring(lineAll.IndexOf("'") + 1, lineAll.LastIndexOf("'") - lineAll.IndexOf("'") - 1);
                    featureNames.Add(feat);
                    continue;
                }
                else if (lineAll.Contains("@RELATION") || lineAll.Contains("@DATA") || lineAll.Trim() == "" || lineAll.Contains("@ATTRIBUTE"))
                    continue;

                allFeatures.Add(lineAll);
            }
            readerAll.Close();
            int index = 0;
            string last = "@DATA";
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                string modelfile = "";
                foreach (string model in models)
                {
                    string modelName = System.IO.Path.GetFileNameWithoutExtension(model);
                    if (name == modelName)
                    {
                        modelfile = model;
                        break;
                    }
                }

                List<double[]> testSet = new List<double[]>();

                //int numFeatures = 0;
                List<string> actuals = new List<string>();

                System.IO.StreamReader reader = new System.IO.StreamReader(file);
                string line;
                while ((line = reader.ReadLine()) != last && line != null)
                { }
                line = reader.ReadLine();

                bool started = false;
                for (int n = index; n < allFeatures.Count; n++)
                {
                    string featureLine = allFeatures[n];
                    if (line == featureLine)
                    {
                        if (started)
                        {
                            last = allFeatures[n - 1];
                            index = n;
                            break;
                        }
                        else
                            reader.ReadLine();
                    }
                    else
                    {
                        started = true;
                        string[] data = featureLine.Split(",".ToCharArray());
                        string currentClass = data[data.Length - 1];
                        

                        double[] nums = new double[data.Length - 1];
                        for (int i = 0; i < nums.Length; i++)
                        {
                            double cur;
                            bool good = double.TryParse(data[i], out cur);
                            if (!good)
                                throw new Exception("Unable to parse '" + data[i] + "' to double");

                            nums[i] = cur;
                        }

                        double centroidDist = nums[2];
                        if (true)//centroidDist < maxCentDist[className])
                        {
                            testSet.Add(nums);
                            actuals.Add(currentClass);
                        }
                    }
                }

                /*for (int n = index; n < allFeatures.Count; n++)
                {
                    string featureLine = allFeatures[n];
                    bool found = false;


                    reader = new System.IO.StreamReader(file);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line == featureLine)
                        {
                            found = true;
                            break;
                        }
                    }
                    reader.Close();

                    if (!found)
                    {
                        started = true;
                        index = n;
                        string[] data = featureLine.Split(",".ToCharArray());
                        string currentClass = data[data.Length - 1];
                        actuals.Add(currentClass);

                        double[] nums = new double[data.Length - 1];
                        for (int i = 0; i < nums.Length; i++)
                        {
                            double cur;
                            bool good = double.TryParse(data[i], out cur);
                            if (!good)
                                throw new Exception("Unable to parse '" + data[i] + "' to double");

                            nums[i] = cur;
                        }
                        testSet.Add(nums);
                    }
                    else if (started)
                        break;
                }*/

                string filename = "C:\\temp.arff";

                Utilities.WekaWrap.createWekaARFFfile(filename, featureNames, testSet, classes);

                List<string> classifications = Utilities.WekaWrap.Classify(filename, modelfile, "adaboost_j48", featureNames.Count);

                for (int i = 0; i < actuals.Count; i++)
                    matrix.Add(actuals[i], classifications[i]);
            }

            System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFile);
            matrix.Print(writer);
            writer.Close();
        }

        private void analyzeOrdersOfLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int timeThreshold = 5000;
            double distanceThreshold = 2000.0;

            bool successSketches;
            List<string> sketchFiles = Utilities.General.SelectOpenMultipleSketches(out successSketches);
            if (!successSketches)
                return;

            bool successSave;
            string saveFile = Utilities.General.SelectSaveFile(out successSave,
                "Select file to save the order strings to",
                "Text File (*.txt)|*.txt");
            if (!successSave)
                return;

            bool successSave2;
            string saveFile2 = Utilities.General.SelectSaveFile(out successSave2,
                "Select file to save the matrices to",
                "Text File (*.txt)|*.txt");
            if (!successSave2)
                return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFile);

            #region Initialize Counters

            int timeGap = 0;
            double distanceGap = 0.0;

            List<string> labels = new List<string>();
            labels.Add("Gate");
            labels.Add("Wire");
            labels.Add("Label");
            List<string> labelsGeneric = new List<string>();
            labelsGeneric.Add("Same");
            labelsGeneric.Add("Different");
            List<string> groupsGeneric = new List<string>();
            groupsGeneric.Add("Same");
            groupsGeneric.Add("Different");
            List<string> bitsGeneric = new List<string>();
            bitsGeneric.Add("sCsG");
            bitsGeneric.Add("sCdG");
            bitsGeneric.Add("dCdG");
            List<string> bitsSpecific = new List<string>();
            bitsSpecific.Add("sGsG");
            bitsSpecific.Add("sGdG");
            bitsSpecific.Add("dCdG");
            bitsSpecific.Add("sWsG");
            bitsSpecific.Add("sWdG");
            bitsSpecific.Add("sLsG");
            bitsSpecific.Add("sLdG");

            List<string> states = new List<string>();
            states.Add("bGsS");
            states.Add("bGdS");
            states.Add("bWsS");
            states.Add("bWdS");
            states.Add("bLsS");
            states.Add("bLdS");
            states.Add("dCdS");
            states.Add("lP");
            states.Add("lD");
            states.Add("lPlD");

            Utilities.ConfusionMatrix states_A = new Utilities.ConfusionMatrix(states);
            Utilities.ConfusionMatrix states_B = new Utilities.ConfusionMatrix(states);
            int[] states_PI = new int[states.Count];
            Dictionary<string, int> states_Indices = new Dictionary<string, int>();
            for (int i = 0; i < states.Count; i++)
            {
                states_PI[i] = 0;
                states_Indices.Add(states[i], i);
            }
            List<string> statesSequence = new List<string>();
            List<string> statesSequencePerfect = new List<string>();

            Utilities.ConfusionMatrix labels_A = new Utilities.ConfusionMatrix(labels);
            Utilities.ConfusionMatrix labels_B = new Utilities.ConfusionMatrix(labels);
            int[] labels_PI = new int[labels.Count];
            Dictionary<string, int> labels_Indices = new Dictionary<string, int>(labels.Count);
            Utilities.ConfusionMatrix labelsPerfect_A = new Utilities.ConfusionMatrix(labels);
            int[] labelsPerfect_PI = new int[labels.Count];
            for (int i = 0; i < labels.Count; i++)
            {
                labels_PI[i] = 0;
                labelsPerfect_PI[i] = 0;
                labels_Indices.Add(labels[i], i);
            }

            Utilities.ConfusionMatrix labelsGeneric_A = new Utilities.ConfusionMatrix(labelsGeneric);
            Utilities.ConfusionMatrix labelsGeneric_B = new Utilities.ConfusionMatrix(labelsGeneric);
            int[] labelsGeneric_PI = new int[labelsGeneric.Count];
            Utilities.ConfusionMatrix labelsGenericPerfect_A = new Utilities.ConfusionMatrix(labelsGeneric);
            int[] labelsGenericPerfect_PI = new int[labelsGeneric.Count];
            Dictionary<string, int> labelsGeneric_Indices = new Dictionary<string, int>(labelsGeneric.Count);
            for (int i = 0; i < labelsGeneric.Count; i++)
            {
                labelsGeneric_PI[i] = 0;
                labelsGenericPerfect_PI[i] = 0;
                labelsGeneric_Indices.Add(labelsGeneric[i], i);
            }

            Utilities.ConfusionMatrix groupsGeneric_A = new Utilities.ConfusionMatrix(groupsGeneric);
            Utilities.ConfusionMatrix groupsGeneric_B = new Utilities.ConfusionMatrix(groupsGeneric);
            int[] groupsGeneric_PI = new int[groupsGeneric.Count];
            Utilities.ConfusionMatrix groupsGenericPerfect_A = new Utilities.ConfusionMatrix(groupsGeneric);
            int[] groupsGenericPerfect_PI = new int[groupsGeneric.Count];
            Dictionary<string, int> groupsGeneric_Indices = new Dictionary<string, int>(groupsGeneric.Count);
            for (int i = 0; i < groupsGeneric.Count; i++)
            {
                groupsGeneric_PI[i] = 0;
                groupsGenericPerfect_PI[i] = 0;
                groupsGeneric_Indices.Add(groupsGeneric[i], i);
            }

            Utilities.ConfusionMatrix bitsGeneric_A = new Utilities.ConfusionMatrix(bitsGeneric);
            Utilities.ConfusionMatrix bitsGeneric_B = new Utilities.ConfusionMatrix(bitsGeneric);
            int[] bitsGeneric_PI = new int[bitsGeneric.Count];
            Utilities.ConfusionMatrix bitsGenericPerfect_A = new Utilities.ConfusionMatrix(bitsGeneric);
            int[] bitsGenericPerfect_PI = new int[bitsGeneric.Count];
            Dictionary<string, int> bitsGeneric_Indices = new Dictionary<string, int>(bitsGeneric.Count);
            for (int i = 0; i < bitsGeneric.Count; i++)
            {
                bitsGeneric_PI[i] = 0;
                bitsGenericPerfect_PI[i] = 0;
                bitsGeneric_Indices.Add(bitsGeneric[i], i);
            }

            Utilities.ConfusionMatrix bitsSpecific_A = new Utilities.ConfusionMatrix(bitsSpecific);
            Utilities.ConfusionMatrix bitsSpecific_B = new Utilities.ConfusionMatrix(bitsSpecific);
            int[] bitsSpecific_PI = new int[bitsSpecific.Count];
            Utilities.ConfusionMatrix bitsSpecificPerfect_A = new Utilities.ConfusionMatrix(bitsSpecific);
            int[] bitsSpecificPerfect_PI = new int[bitsSpecific.Count];
            Dictionary<string, int> bitsSpecific_Indices = new Dictionary<string, int>(bitsSpecific.Count);
            for (int i = 0; i < bitsSpecific.Count; i++)
            {
                bitsSpecific_PI[i] = 0;
                bitsSpecificPerfect_PI[i] = 0;
                bitsSpecific_Indices.Add(bitsSpecific[i], i);
            }

            string clsGeneric0 = "";
            string groupGeneric0 = "";
            string bitGeneric0 = "";
            string bitSpecific0 = "";
            string clsGenericPerfect0 = "";
            string groupGenericPerfect0 = "";
            string bitGenericPerfect0 = "";
            string bitSpecificPerfect0 = "";
            string state0 = "";
            string statePerfect0 = "";

            #endregion

            #region loop through each sketch file

            foreach (string file in sketchFiles)
            {
                Status.Text = "Loading Sketch...";
                this.Refresh();

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);

                LoadAppropriateNetworks(sketchFile);

                m_Panel.loadSketch(file, false, true);

                Status.Text = "Featurizing Sketch...";
                this.Refresh();

                m_Panel.InkSketch.Sketch.resetShapes();

                int num;
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    string user = num.ToString().PadLeft(index, '0');
                    m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
                }

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                    m_Clusterer.FeatureSketch.FeatureListSingle,
                    m_Clusterer.FeatureSketch.FeatureListPair,
                    m_Clusterer.Settings.AvgsAndStdDevs);
                m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);
                SortedList<ulong, Sketch.Substroke> order = m_Clusterer.FeatureSketch.OrderOfStrokes;

                // HACK to use perfect classifications
                Dictionary<Guid, string[]> realClasses = new Dictionary<Guid, string[]>();
                Sketch.Sketch origSketch = new ConverterXML.ReadXML(file).Sketch;
                origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                origSketch = Utilities.General.LinkShapes(origSketch);

                StrokeClassifier.StrokeClassifierResult ssResult = m_Clusterer.Classifier.Result;
                StrokeGrouper.StrokeGrouperResult groupingResult = m_Clusterer.Grouper.Results;

                foreach (Sketch.Substroke realStroke in origSketch.SubstrokesL)
                {
                    Sketch.Substroke stroke = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(realStroke.Id);
                    string cls = Utilities.General.GetClass(realStroke.FirstLabel);
                    string[] both = new string[] { cls, stroke.Classification };
                    realClasses.Add(realStroke.Id, both);
                }

                writer.WriteLine(sketchFile);

                List<Sketch.Substroke> strokes = new List<Sketch.Substroke>(order.Values);

                for (int i = 0; i < strokes.Count; i++)
                {
                    Sketch.Substroke stroke = strokes[i];
                    Sketch.Substroke realStroke = origSketch.GetSubstroke(stroke.Id);
                    if (i == 0)
                    {
                        string cls = realClasses[stroke.Id][1];
                        string clsPerfect = realClasses[stroke.Id][0];
                        string clsGeneric = "Same";
                        string groupGeneric = "Same";
                        string bitGeneric = "sCsG";
                        string bitSpecific = "";
                        if (cls == "Gate")
                            bitSpecific = "sGsG";
                        else if (cls == "Wire")
                            bitSpecific = "sWsG";
                        else if (cls == "Label")
                            bitSpecific = "sLsG";
                        string bitSpecificPerfect = "";
                        if (clsPerfect == "Gate")
                            bitSpecificPerfect = "sGsG";
                        else if (clsPerfect == "Wire")
                            bitSpecificPerfect = "sWsG";
                        else if (clsPerfect == "Label")
                            bitSpecificPerfect = "sLsG";
                        string statePerfect = "error";
                        if (clsPerfect == "Gate")
                            statePerfect = "bGsS";
                        else if (clsPerfect == "Wire")
                            statePerfect = "bWsS";
                        else if (clsPerfect == "Label")
                            statePerfect = "bLsS";
                        string state = "error";
                        if (cls == "Gate")
                            state = "bGsS";
                        else if (cls == "Wire")
                            state = "bWsS";
                        else if (cls == "Label")
                            state = "bLsS";

                        int labels_index = labels_Indices[cls];
                        labels_PI[labels_index]++;
                        labelsPerfect_PI[labels_index]++;
                        labels_B.Add(clsPerfect, cls);

                        int labelsGeneric_index = labelsGeneric_Indices[clsGeneric];
                        labelsGeneric_PI[labelsGeneric_index]++;
                        labelsGenericPerfect_PI[labelsGeneric_index]++;
                        labelsGeneric_B.Add(clsGeneric, clsGeneric);

                        int groupGeneric_index = groupsGeneric_Indices[groupGeneric];
                        groupsGeneric_PI[groupGeneric_index]++;
                        groupsGenericPerfect_PI[groupGeneric_index]++;
                        groupsGeneric_B.Add(groupGeneric, groupGeneric);

                        int bitGeneric_index = bitsGeneric_Indices[bitGeneric];
                        bitsGeneric_PI[bitGeneric_index]++;
                        bitsGenericPerfect_PI[bitGeneric_index]++;
                        bitsGeneric_B.Add(bitGeneric, bitGeneric);

                        int bitSpecific_index = bitsSpecific_Indices[bitSpecific];
                        bitsSpecific_PI[bitSpecific_index]++;
                        bitsSpecificPerfect_PI[bitSpecific_index]++;
                        bitsSpecific_B.Add(bitSpecificPerfect, bitSpecific);

                        states_PI[states_Indices[statePerfect]]++;
                        states_B.Add(statePerfect, state);
                        statesSequencePerfect.Add(statePerfect);
                        statesSequence.Add(state);

                        clsGeneric0 = clsGeneric;
                        groupGeneric0 = groupGeneric;
                        bitGeneric0 = bitGeneric;
                        bitSpecific0 = bitSpecific;
                        clsGenericPerfect0 = clsGeneric;
                        groupGenericPerfect0 = groupGeneric;
                        bitGenericPerfect0 = bitGeneric;
                        bitSpecificPerfect0 = bitSpecificPerfect;
                        state0 = state;
                        statePerfect0 = statePerfect;

                        continue;
                    }

                    Sketch.Substroke previousStroke = strokes[i - 1];
                    Sketch.Substroke previousRealStroke = origSketch.GetSubstroke(previousStroke.Id);

                    timeGap = (int)(stroke.Points[0].Time - previousStroke.Points[previousStroke.Points.Length - 1].Time);
                    Utilities.SubstrokeDistance dist = new Utilities.SubstrokeDistance(stroke, previousStroke);
                    distanceGap = dist.Min;

                    string cls1 = realClasses[stroke.Id][1];
                    string cls0 = realClasses[previousStroke.Id][1];
                    string clsPerfect1 = realClasses[stroke.Id][0];
                    string clsPerfect0 = realClasses[previousStroke.Id][0];

                    string clsGeneric1 = "error";
                    if (cls1 == cls0)
                        clsGeneric1 = "Same";
                    else if (cls1 != cls0)
                        clsGeneric1 = "Different";
                    string clsGenericPerfect1 = "error";
                    if (clsPerfect1 == clsPerfect0)
                        clsGenericPerfect1 = "Same"; 
                    else if (clsPerfect1 != clsPerfect0)
                        clsGenericPerfect1 = "Different";

                    string groupGeneric1 = "error";
                    if (stroke.ParentShapes[0] == previousStroke.ParentShapes[0])
                        groupGeneric1 = "Same";
                    else if (stroke.ParentShapes[0] != previousStroke.ParentShapes[0])
                        groupGeneric1 = "Different";
                    string groupGenericPerfect1 = "error";
                    if (realStroke.ParentShapes[0] == previousRealStroke.ParentShapes[0])
                        groupGenericPerfect1 = "Same";
                    else if (realStroke.ParentShapes[0] != previousRealStroke.ParentShapes[0])
                        groupGenericPerfect1 = "Different";

                    string bitGeneric1 = "error";
                    if (clsGeneric1 == "Same" && groupGeneric1 == "Same")
                        bitGeneric1 = "sCsG";
                    else if (clsGeneric1 == "Same" && groupGeneric1 == "Different")
                        bitGeneric1 = "sCdG";
                    else if (clsGeneric1 == "Different" && groupGeneric1 == "Different")
                        bitGeneric1 = "dCdG";

                    string bitGenericPerfect1 = "error";
                    if (clsGenericPerfect1 == "Same" && groupGenericPerfect1 == "Same")
                        bitGenericPerfect1 = "sCsG";
                    else if (clsGenericPerfect1 == "Same" && groupGenericPerfect1 == "Different")
                        bitGenericPerfect1 = "sCdG";
                    else if (clsGenericPerfect1 == "Different" && groupGenericPerfect1 == "Different")
                        bitGenericPerfect1 = "dCdG";


                    string bitSpecific1 = "error";
                    if (clsGeneric1 == "Different")
                        bitSpecific1 = "dCdG";
                    else if (clsGeneric1 == "Same" && cls1 == "Gate" && groupGeneric1 == "Same")
                        bitSpecific1 = "sGsG";
                    else if (clsGeneric1 == "Same" && cls1 == "Wire" && groupGeneric1 == "Same")
                        bitSpecific1 = "sWsG";
                    else if (clsGeneric1 == "Same" && cls1 == "Label" && groupGeneric1 == "Same")
                        bitSpecific1 = "sLsG";
                    else if (clsGeneric1 == "Same" && cls1 == "Gate" && groupGeneric1 == "Different")
                        bitSpecific1 = "sGdG";
                    else if (clsGeneric1 == "Same" && cls1 == "Wire" && groupGeneric1 == "Different")
                        bitSpecific1 = "sWdG";
                    else if (clsGeneric1 == "Same" && cls1 == "Label" && groupGeneric1 == "Different")
                        bitSpecific1 = "sLdG";

                    string bitSpecificPerfect1 = "error";
                    if (clsGenericPerfect1 == "Different")
                        bitSpecificPerfect1 = "dCdG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Gate" && groupGenericPerfect1 == "Same")
                        bitSpecificPerfect1 = "sGsG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Wire" && groupGenericPerfect1 == "Same")
                        bitSpecificPerfect1 = "sWsG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Label" && groupGenericPerfect1 == "Same")
                        bitSpecificPerfect1 = "sLsG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Gate" && groupGenericPerfect1 == "Different")
                        bitSpecificPerfect1 = "sGdG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Wire" && groupGenericPerfect1 == "Different")
                        bitSpecificPerfect1 = "sWdG";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Label" && groupGenericPerfect1 == "Different")
                        bitSpecificPerfect1 = "sLdG";

                    string statePerfect1 = "error";
                    if (clsGenericPerfect1 == "Different")
                        statePerfect1 = "dCdS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Gate" && groupGenericPerfect1 == "Same")
                        statePerfect1 = "bGsS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Wire" && groupGenericPerfect1 == "Same")
                        statePerfect1 = "bWsS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Label" && groupGenericPerfect1 == "Same")
                        statePerfect1 = "bLsS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Gate" && groupGenericPerfect1 == "Different")
                        statePerfect1 = "bGdS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Wire" && groupGenericPerfect1 == "Different")
                        statePerfect1 = "bWdS";
                    else if (clsGenericPerfect1 == "Same" && clsPerfect1 == "Label" && groupGenericPerfect1 == "Different")
                        statePerfect1 = "bLdS";

                    string state1 = "error";
                    if (clsGeneric1 == "Different")
                        state1 = "dCdS";
                    else if (clsGeneric1 == "Same" && cls1 == "Gate" && groupGeneric1 == "Same")
                        state1 = "bGsS";
                    else if (clsGeneric1 == "Same" && cls1 == "Wire" && groupGeneric1 == "Same")
                        state1 = "bWsS";
                    else if (clsGeneric1 == "Same" && cls1 == "Label" && groupGeneric1 == "Same")
                        state1 = "bLsS";
                    else if (clsGeneric1 == "Same" && cls1 == "Gate" && groupGeneric1 == "Different")
                        state1 = "bGdS";
                    else if (clsGeneric1 == "Same" && cls1 == "Wire" && groupGeneric1 == "Different")
                        state1 = "bWdS";
                    else if (clsGeneric1 == "Same" && cls1 == "Label" && groupGeneric1 == "Different")
                        state1 = "bLdS";


                    if (timeGap >= timeThreshold && distanceGap >= distanceThreshold)
                    {
                        statesSequence.Add("lPlD");
                        statesSequencePerfect.Add("lPlD");

                        statesSequencePerfect.Add(statePerfect1);
                        statesSequence.Add(state1);

                        states_A.Add("lPlD", statePerfect0);
                        states_A.Add(statePerfect1, "lPlD");
                        states_B.Add(statePerfect1, state1);
                    }
                    else if (timeGap >= timeThreshold)
                    {
                        statesSequence.Add("lP");
                        statesSequencePerfect.Add("lP");

                        statesSequencePerfect.Add(statePerfect1);
                        statesSequence.Add(state1);

                        states_A.Add("lP", statePerfect0);
                        states_A.Add(statePerfect1, "lP");
                        states_B.Add(statePerfect1, state1);
                    }
                    else if (distanceGap >= distanceThreshold)
                    {
                        statesSequence.Add("lD");
                        statesSequencePerfect.Add("lD");

                        statesSequencePerfect.Add(statePerfect1);
                        statesSequence.Add(state1);

                        states_A.Add("lD", statePerfect0);
                        states_A.Add(statePerfect1, "lD");
                        states_B.Add(statePerfect1, state1);
                    }
                    else
                    {
                        statesSequencePerfect.Add(statePerfect1);
                        statesSequence.Add(state1);

                        states_A.Add(statePerfect1, statePerfect0);
                        states_B.Add(statePerfect1, state1);
                    }

                    labels_A.Add(cls1, cls0);
                    labelsPerfect_A.Add(clsPerfect1, clsPerfect0);
                    labels_B.Add(clsPerfect1, cls1);

                    labelsGeneric_A.Add(clsGeneric1, clsGeneric0);
                    labelsGenericPerfect_A.Add(clsGenericPerfect1, clsGenericPerfect0);
                    labelsGeneric_B.Add(clsGenericPerfect1, clsGeneric1);

                    groupsGeneric_A.Add(groupGeneric1, groupGeneric0);
                    groupsGenericPerfect_A.Add(groupGenericPerfect1, groupGenericPerfect0);
                    groupsGeneric_B.Add(groupGenericPerfect1, groupGeneric1);

                    bitsGeneric_A.Add(bitGeneric1, bitGeneric0);
                    bitsGenericPerfect_A.Add(bitGenericPerfect1, bitGenericPerfect0);
                    bitsGeneric_B.Add(bitGenericPerfect1, bitGeneric1);

                    bitsSpecific_A.Add(bitSpecific1, bitSpecific0);
                    bitsSpecificPerfect_A.Add(bitSpecificPerfect1, bitSpecificPerfect0);
                    bitsSpecific_B.Add(bitSpecificPerfect1, bitSpecific1);


                    clsGeneric0 = clsGeneric1;
                    groupGeneric0 = groupGeneric1;
                    bitGeneric0 = bitGeneric1;
                    bitSpecific0 = bitSpecific1;
                    clsGenericPerfect0 = clsGenericPerfect1;
                    groupGenericPerfect0 = groupGenericPerfect1;
                    bitGenericPerfect0 = bitGenericPerfect1;
                    bitSpecificPerfect0 = bitSpecificPerfect1;
                    statePerfect0 = statePerfect1;
                }

                foreach (string state in statesSequence)
                    writer.Write(state + "\t");
                writer.WriteLine();

                foreach (string state in statesSequencePerfect)
                    writer.Write(state + "\t");
                writer.WriteLine();
                writer.WriteLine();
                

                foreach (Sketch.Substroke stroke in strokes)
                    writer.Write(realClasses[stroke.Id][1] + "\t");
                writer.WriteLine();

                foreach (Sketch.Substroke stroke in strokes)
                    writer.Write(realClasses[stroke.Id][0] + "\t");
                writer.WriteLine();
                writer.WriteLine();

                writer.Write("same\t");
                for (int i = 1; i < strokes.Count; i++)
                {
                    if (strokes[i].ParentShapes[0] == strokes[i - 1].ParentShapes[0])
                        writer.Write("same\t");
                    else
                        writer.Write("diff\t");
                }
                writer.WriteLine();

                writer.Write("same\t");
                for (int i = 1; i < strokes.Count; i++)
                {
                    Sketch.Substroke s0 = origSketch.GetSubstroke(strokes[i - 1].Id);
                    Sketch.Substroke s1 = origSketch.GetSubstroke(strokes[i].Id);
                    if (s0.ParentShapes[0] == s1.ParentShapes[0])
                        writer.Write("same\t");
                    else
                        writer.Write("diff\t");
                }
                writer.WriteLine();

                writer.WriteLine();

                writer.Write("sCsG\t");
                for (int i = 1; i < strokes.Count; i++)
                {
                    Sketch.Substroke s0 = strokes[i - 1];
                    Sketch.Substroke s1 = strokes[i];
                    string cls0 = realClasses[strokes[i - 1].Id][1];
                    string cls1 = realClasses[strokes[i].Id][1];
                    if (s0.ParentShapes[0] == s1.ParentShapes[0] && cls0 == cls1)
                        writer.Write("sCsG\t");
                    else if (cls0 == cls1)
                        writer.Write("sCdG\t");
                    else if (s0.ParentShapes[0] == s1.ParentShapes[0])
                        writer.Write("dCsG\t");
                    else
                        writer.Write("dCdG\t");
                }
                writer.WriteLine();

                writer.Write("sCsG\t");
                for (int i = 1; i < strokes.Count; i++)
                {
                    Sketch.Substroke s0 = origSketch.GetSubstroke(strokes[i - 1].Id);
                    Sketch.Substroke s1 = origSketch.GetSubstroke(strokes[i].Id);
                    string cls0 = realClasses[strokes[i - 1].Id][0];
                    string cls1 = realClasses[strokes[i].Id][0];
                    if (s0.ParentShapes[0] == s1.ParentShapes[0] && cls0 == cls1)
                        writer.Write("sCsG\t");
                    else if (cls0 == cls1)
                        writer.Write("sCdG\t");
                    else if (s0.ParentShapes[0] == s1.ParentShapes[0])
                        writer.Write("dCsG\t");
                    else
                        writer.Write("dCdG\t");
                }
                writer.WriteLine();
                writer.WriteLine();



                string clsA = realClasses[strokes[0].Id][1];
                if (clsA == "Gate")
                    writer.Write("sGsG\t");
                else if (clsA == "Wire")
                    writer.Write("sWsG\t");
                else if (clsA == "Label")
                    writer.Write("sLsG\t");

                for (int i = 1; i < strokes.Count; i++)
                {
                    Sketch.Substroke s0 = strokes[i - 1];
                    Sketch.Substroke s1 = strokes[i];
                    string cls0 = realClasses[strokes[i - 1].Id][1];
                    string cls1 = realClasses[strokes[i].Id][1];

                    if (cls0 == cls1)
                    {
                        if (cls0 == "Gate")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sGsG\t");
                            else
                                writer.Write("sGdG\t");
                        }
                        else if (cls0 == "Wire")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sWsG\t");
                            else
                                writer.Write("sWdG\t");
                        }
                        else if (cls0 == "Label")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sLsG\t");
                            else
                                writer.Write("sLdG\t");
                        }
                    }
                    else
                    {
                        if (cls0 == "Gate")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                        else if (cls0 == "Wire")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                        else if (cls0 == "Label")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                    }
                }
                writer.WriteLine();

                clsA = realClasses[strokes[0].Id][0];
                if (clsA == "Gate")
                    writer.Write("sGsG\t");
                else if (clsA == "Wire")
                    writer.Write("sWsG\t");
                else if (clsA == "Label")
                    writer.Write("sLsG\t");

                for (int i = 1; i < strokes.Count; i++)
                {
                    Sketch.Substroke s0 = origSketch.GetSubstroke(strokes[i - 1].Id);
                    Sketch.Substroke s1 = origSketch.GetSubstroke(strokes[i].Id);
                    string cls0 = realClasses[strokes[i - 1].Id][0];
                    string cls1 = realClasses[strokes[i].Id][0];

                    if (cls0 == cls1)
                    {
                        if (cls0 == "Gate")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sGsG\t");
                            else
                                writer.Write("sGdG\t");
                        }
                        else if (cls0 == "Wire")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sWsG\t");
                            else
                                writer.Write("sWdG\t");
                        }
                        else if (cls0 == "Label")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("sLsG\t");
                            else
                                writer.Write("sLdG\t");
                        }
                    }
                    else
                    {
                        if (cls0 == "Gate")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                        else if (cls0 == "Wire")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                        else if (cls0 == "Label")
                        {
                            if (s0.ParentShapes[0] == s1.ParentShapes[0])
                                writer.Write("dCsG\t");
                            else
                                writer.Write("dCdG\t");
                        }
                    }
                }
                writer.WriteLine();
                writer.WriteLine();

                Status.Text = "";
                this.Refresh();

            }
            #endregion

            writer.Close();

            /*labels_A.IncreaseAllByOne();
            double[][] labels_A1 = new double[labels.Count][];
            for (int i = 0; i < labels.Count; i++)
            {
                labels_A1[i] = new double[labels.Count];
                for (int j = 0; j < labels.Count; j++)
                {
                    labels_A1[i][j] = labels_A.GetProbability(labels[i], labels[j]);
                }
            }*/

            // Add 1 to each entry
            labels_A.IncreaseAllByOne();
            labelsGeneric_A.IncreaseAllByOne();
            groupsGeneric_A.IncreaseAllByOne();
            bitsGeneric_A.IncreaseAllByOne();
            bitsSpecific_A.IncreaseAllByOne();
            labelsPerfect_A.IncreaseAllByOne();
            labelsGenericPerfect_A.IncreaseAllByOne();
            groupsGenericPerfect_A.IncreaseAllByOne();
            bitsGenericPerfect_A.IncreaseAllByOne();
            bitsSpecificPerfect_A.IncreaseAllByOne();
            labels_B.IncreaseAllByOne();
            labelsGeneric_B.IncreaseAllByOne();
            groupsGeneric_B.IncreaseAllByOne();
            bitsGeneric_B.IncreaseAllByOne();
            bitsSpecific_B.IncreaseAllByOne();
            
            states_A.IncreaseAllByOne();
            states_B.IncreaseAllByOne();
            for (int i = 0; i < states_PI.Length; i++)
                states_PI[i]++;

            for (int i = 0; i < labels_PI.Length; i++)
                labels_PI[i]++;
            for (int i = 0; i < labelsPerfect_PI.Length; i++)
                labelsPerfect_PI[i]++;
            for (int i = 0; i < labelsGeneric_PI.Length; i++)
                labelsGeneric_PI[i]++;
            for (int i = 0; i < labelsGenericPerfect_PI.Length; i++)
                labelsGenericPerfect_PI[i]++;
            for (int i = 0; i < groupsGeneric_PI.Length; i++)
                groupsGeneric_PI[i]++;
            for (int i = 0; i < groupsGenericPerfect_PI.Length; i++)
                groupsGenericPerfect_PI[i]++;
            for (int i = 0; i < bitsGeneric_PI.Length; i++)
                bitsGeneric_PI[i]++;
            for (int i = 0; i < bitsGenericPerfect_PI.Length; i++)
                bitsGenericPerfect_PI[i]++;
            for (int i = 0; i < bitsSpecific_PI.Length; i++)
                bitsSpecific_PI[i]++;
            for (int i = 0; i < bitsSpecificPerfect_PI.Length; i++)
                bitsSpecificPerfect_PI[i]++;

            System.IO.StreamWriter writer2 = new System.IO.StreamWriter(saveFile2);



            // States
            writer2.WriteLine("States Perfect PI");
            for (int i = 0; i < states_PI.Length; i++)
                writer2.WriteLine(states_PI[i]);
            writer2.WriteLine("States Perfect A");
            states_A.Print(writer2);
            writer2.WriteLine("States B");
            states_B.Print(writer2);
            writer2.WriteLine();
            writer2.WriteLine();

            // labels
            // write out labels PI
            // write out labels A
            // write out labels B
            writer2.WriteLine("Labels PI");
            for (int i = 0; i < labels_PI.Length; i++)
                writer2.WriteLine(labels_PI[i]);
            writer2.WriteLine("Labels A");
            labels_A.Print(writer2);
            writer2.WriteLine("Labels B");
            labels_B.Print(writer2);
            writer2.WriteLine();

            writer2.WriteLine("Labels Perfect PI");
            for (int i = 0; i < labelsPerfect_PI.Length; i++)
                writer2.WriteLine(labelsPerfect_PI[i]);
            writer2.WriteLine("Labels A Perfect");
            labelsPerfect_A.Print(writer2);
            writer2.WriteLine();
            writer2.WriteLine();


            // labels
            // write out labels PI
            // write out labels A
            // write out labels B
            writer2.WriteLine("Labels Generic PI");
            for (int i = 0; i < labelsGeneric_PI.Length; i++)
                writer2.WriteLine(labelsGeneric_PI[i]);
            writer2.WriteLine("Labels Generic A");
            labelsGeneric_A.Print(writer2);
            writer2.WriteLine("Labels Generic B");
            labelsGeneric_B.Print(writer2);
            writer2.WriteLine();

            writer2.WriteLine("Labels Generic Perfect PI");
            for (int i = 0; i < labelsGenericPerfect_PI.Length; i++)
                writer2.WriteLine(labelsGenericPerfect_PI[i]);
            writer2.WriteLine("Labels Generic A Perfect");
            labelsGenericPerfect_A.Print(writer2);
            writer2.WriteLine();


            // labels
            // write out labels PI
            // write out labels A
            // write out labels B
            writer2.WriteLine("Groups Generic PI");
            for (int i = 0; i < groupsGeneric_PI.Length; i++)
                writer2.WriteLine(groupsGeneric_PI[i]);
            writer2.WriteLine("Groups Generic A");
            groupsGeneric_A.Print(writer2);
            writer2.WriteLine("Groups Generic B");
            groupsGeneric_B.Print(writer2);
            writer2.WriteLine();

            writer2.WriteLine("Groups Generic Perfect PI");
            for (int i = 0; i < groupsGenericPerfect_PI.Length; i++)
                writer2.WriteLine(groupsGenericPerfect_PI[i]);
            writer2.WriteLine("Groups Generic A Perfect");
            groupsGenericPerfect_A.Print(writer2);
            writer2.WriteLine();


            // labels
            // write out labels PI
            // write out labels A
            // write out labels B
            writer2.WriteLine("Bits Generic PI");
            for (int i = 0; i < bitsGeneric_PI.Length; i++)
                writer2.WriteLine(bitsGeneric_PI[i]);
            writer2.WriteLine("Bits Generic A");
            bitsGeneric_A.Print(writer2);
            writer2.WriteLine("Bits Generic B");
            bitsGeneric_B.Print(writer2);
            writer2.WriteLine();

            writer2.WriteLine("Bits Generic Perfect PI");
            for (int i = 0; i < bitsGenericPerfect_PI.Length; i++)
                writer2.WriteLine(bitsGenericPerfect_PI[i]);
            writer2.WriteLine("Bits Generic A Perfect");
            bitsGenericPerfect_A.Print(writer2);
            writer2.WriteLine();



            // labels
            // write out labels PI
            // write out labels A
            // write out labels B
            writer2.WriteLine("Bits Specific PI");
            for (int i = 0; i < bitsSpecific_PI.Length; i++)
                writer2.WriteLine(bitsSpecific_PI[i]);
            writer2.WriteLine("Bits Specific A");
            bitsSpecific_A.Print(writer2);
            writer2.WriteLine("Bits Specific B");
            bitsSpecific_B.Print(writer2);
            writer2.WriteLine();

            writer2.WriteLine("Bits Specific Perfect PI");
            for (int i = 0; i < bitsSpecificPerfect_PI.Length; i++)
                writer2.WriteLine(bitsSpecificPerfect_PI[i]);
            writer2.WriteLine("Bits Specific A Perfect");
            bitsSpecificPerfect_A.Print(writer2);
            writer2.WriteLine();




            writer2.Close();
        }

        private void playSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int factor = 3;
            bool success;
            string filename = Utilities.General.SelectOpenSketch(out success);
            if (!success)
                return;

            string sketchFile = System.IO.Path.GetFileNameWithoutExtension(filename);

            LoadAppropriateNetworks(sketchFile);

            Sketch.Sketch sketch = new ConverterXML.ReadXML(filename).Sketch;
            sketch = Utilities.General.ReOrderParentShapes(sketch);
            sketch = Utilities.General.LinkShapes(sketch);

            int num;
            int index = sketchFile.IndexOf('_');
            bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
            if (goodUser)
            {
                string user = num.ToString().PadLeft(index, '0');
                m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
            }

            m_Panel.loadSketch(filename, false, true);

            m_Panel.InkSketch.Sketch.resetShapes();

            Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                m_Clusterer.FeatureSketch.FeatureListSingle,
                m_Clusterer.FeatureSketch.FeatureListPair,
                m_Clusterer.Settings.AvgsAndStdDevs);
            m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);

            // HACK to use perfect classifications
            /*Dictionary<Sketch.Substroke, string> realClasses = new Dictionary<Sketch.Substroke, string>();
            Sketch.Sketch origSketch = new ConverterXML.ReadXML(filename).Sketch;
            origSketch = Utilities.General.ReOrderParentShapes(origSketch);
            origSketch = Utilities.General.LinkShapes(origSketch);
            foreach (Sketch.Substroke realStroke in origSketch.SubstrokesL)
            {
                Sketch.Substroke stroke = m_Clusterer.FeatureSketch.Sketch.GetSubstroke(realStroke.Id);
                string cls = Utilities.General.GetClass(realStroke.FirstLabel);
                stroke.Classification = cls;
                realClasses.Add(stroke, cls);
            }*/
            
            m_Clusterer.Grouper.UpdateStrokeClassifications(m_Clusterer.Classifier.Result.AllClassifications);
            m_Clusterer.Grouper.Group();

            m_DisplayManager.displayGroups();

            foreach (Sketch.Substroke stroke in m_Panel.InkSketch.Sketch.SubstrokesL)
                stroke.Classification = "Invisible";

            m_DisplayManager.displayClassification(m_Clusterer.Settings.ClassifierSettings.Domain.Class2Color);

            SortedList<ulong, Sketch.Substroke> order = new SortedList<ulong, Sketch.Substroke>();
            foreach (Sketch.Substroke stroke in sketch.SubstrokesL)
                order.Add(stroke.XmlAttrs.Time.Value, stroke);

            List<ulong> times = new List<ulong>(order.Keys);
            List<Sketch.Substroke> strokes = new List<Sketch.Substroke>(order.Values);
            int[] pauses = new int[Math.Max(1, order.Count - 1)];

            for (int i = 1; i < strokes.Count; i++)
                pauses[i - 1] = (int)(strokes[i].XmlAttrs.Time - strokes[i - 1].Points[strokes[i - 1].Points.Length - 1].Time);

            for (int i = 0; i < strokes.Count; i++)
            {
                m_Panel.InkSketch.Sketch.SubstrokesL[i].Classification = "Visible";
                m_DisplayManager.displayClassification(m_Clusterer.Settings.ClassifierSettings.Domain.Class2Color);

                if (i < pauses.Length)
                    System.Threading.Thread.Sleep(pauses[i] / factor);
            }
        }

        private void groupWithSimpleThresholdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.Year + "_" + time.Month.ToString().PadLeft(2, '0') + "_" + time.Day.ToString().PadLeft(2, '0') + "_"
                + time.Hour.ToString().PadLeft(2, '0') + "_" + time.Minute.ToString().PadLeft(2, '0') + "_" + time.Second.ToString().PadLeft(2, '0');

            bool success;
            List<string> filenames = Utilities.General.SelectOpenFiles(out success,
                "Sketches to test",
                "Labeled Sketches (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            string distMeasure = "Min";

            #region Parameters
            List<int[]> parameters = new List<int[]>();
            parameters.Add(new int[] { 100, 10000000 });
            parameters.Add(new int[] { 200, 10000000 });
            parameters.Add(new int[] { 300, 10000000 });
            parameters.Add(new int[] { 400, 10000000 });
            parameters.Add(new int[] { 100, 1500 });
            parameters.Add(new int[] { 200, 1500 });
            parameters.Add(new int[] { 300, 1500 });
            parameters.Add(new int[] { 400, 1500 });
            parameters.Add(new int[] { 100, 2000 });
            parameters.Add(new int[] { 200, 2000 });
            parameters.Add(new int[] { 300, 2000 });
            parameters.Add(new int[] { 400, 2000 });
            parameters.Add(new int[] { 100, 2500 });
            parameters.Add(new int[] { 200, 2500 });
            parameters.Add(new int[] { 300, 2500 });
            parameters.Add(new int[] { 400, 2500 });
            parameters.Add(new int[] { 100, 3000 });
            parameters.Add(new int[] { 200, 3000 });
            parameters.Add(new int[] { 300, 3000 });
            parameters.Add(new int[] { 400, 3000 });
            parameters.Add(new int[] { 100, 4000 });
            parameters.Add(new int[] { 200, 4000 });
            parameters.Add(new int[] { 300, 4000 });
            parameters.Add(new int[] { 400, 4000 });
            parameters.Add(new int[] { 100, 5000 });
            parameters.Add(new int[] { 200, 5000 });
            parameters.Add(new int[] { 300, 5000 });
            parameters.Add(new int[] { 400, 5000 });
            parameters.Add(new int[] { 100, 6000 });
            parameters.Add(new int[] { 200, 6000 });
            parameters.Add(new int[] { 300, 6000 });
            parameters.Add(new int[] { 400, 6000 });
            parameters.Add(new int[] { 100, 7000 });
            parameters.Add(new int[] { 200, 7000 });
            parameters.Add(new int[] { 300, 7000 });
            parameters.Add(new int[] { 400, 7000 });
            parameters.Add(new int[] { 100, 8000 });
            parameters.Add(new int[] { 200, 8000 });
            parameters.Add(new int[] { 300, 8000 });
            parameters.Add(new int[] { 400, 8000 });
            parameters.Add(new int[] { 100, 9000 });
            parameters.Add(new int[] { 200, 9000 });
            parameters.Add(new int[] { 300, 9000 });
            parameters.Add(new int[] { 400, 9000 });
            parameters.Add(new int[] { 100, 10000 });
            parameters.Add(new int[] { 200, 10000 });
            parameters.Add(new int[] { 300, 10000 });
            parameters.Add(new int[] { 400, 10000 });
            #endregion

            List<KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>> sketches = new List<KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>>();
            // format for parameters: (0) = max distance gap in ink pixels, (1) = max time gap in milliseconds
            List<string> labels = new List<string>();
            labels.Add("join");
            labels.Add("nojoin");

            Dictionary<int[], List<Utilities.ConfusionMatrix>> confusions = new Dictionary<int[], List<Utilities.ConfusionMatrix>>();
            foreach (int[] param in parameters)
            {
                List<Utilities.ConfusionMatrix> matrices = new List<Utilities.ConfusionMatrix>();
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                confusions.Add(param, matrices);
                //Utilities.ConfusionMatrix joinConfusion = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionGate = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionLabel = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionWire = new Utilities.ConfusionMatrix(labels);
            }

            foreach (string file in filenames)
            {
                Sketch.Sketch origSketch = new ConverterXML.ReadXML(file).Sketch;
                origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                origSketch = Utilities.General.LinkShapes(origSketch);

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);

                LoadAppropriateNetworks(sketchFile);

                int num;
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    string user = num.ToString().PadLeft(index, '0');
                    m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
                }

                m_Panel.loadSketch(file, false, true);

                m_Panel.InkSketch.Sketch.resetShapes();

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                    m_Clusterer.FeatureSketch.FeatureListSingle,
                    m_Clusterer.FeatureSketch.FeatureListPair,
                    m_Clusterer.Settings.AvgsAndStdDevs);
                m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);
                m_Clusterer.Classifier.AssignClassificationsToFeatureSketch();

                foreach (int[] param in parameters)
                {
                    sketches.Add(new KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>(origSketch,
                        new KeyValuePair<int[], List<Sketch.Shape>>(param,
                            GetSimpleGroups(m_Clusterer.FeatureSketch,
                                m_Clusterer.Classifier.Result.AllClassifications,
                                m_Clusterer.Settings.ClassifierSettings.Domain,
                                param, distMeasure, origSketch, confusions[param][0],
                                confusions[param][1], confusions[param][2], confusions[param][3]))));
                }
            }


            foreach (int[] param in parameters)
            {
                string name1 = "C:\\GroupingAccuracy\\actualPOV_" + timeStr + "_d" + distMeasure + param[0] + "_t" + param[1] + ".txt";
                //string name2 = "C:\\GroupingAccuracy\\groupPOV_" + timeStr + "_d" + distMeasure + param[0] + "_t" + param[1] + ".txt";

                System.IO.StreamWriter writer = new System.IO.StreamWriter(name1);
                //System.IO.StreamWriter writer2 = new System.IO.StreamWriter(name2);

                foreach (KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>> kvp in sketches)
                {
                    int[] current = kvp.Value.Key;
                    if (param[0] != current[0] || param[1] != current[1])
                        continue;

                    
                    StrokeGrouper.GrouperAccuracy accuracy = new StrokeGrouper.GrouperAccuracy(kvp.Key, kvp.Value.Value);
                    
                    
                    //foreach (StrokeGrouper.ShapeResult result in accuracy.Results)
                        //result.Print(writer2);

                    foreach (StrokeGrouper.ShapeResultFromActual result in accuracy.ResultsFromActual)
                        result.Print(writer);

                    
                }

                List<Utilities.ConfusionMatrix> matrices = confusions[param];
                writer.WriteLine();
                foreach (Utilities.ConfusionMatrix m in matrices)
                    m.Print(writer);

                writer.Close();
                //writer2.Close();
            }
        }

        private List<Sketch.Shape> GetSimpleGroups(Featurefy.FeatureSketch featureSketch, 
            Dictionary<Sketch.Substroke, string> classifications, Utilities.Domain domain, 
            int[] param, string distMeasure, Sketch.Sketch origSketch, Utilities.ConfusionMatrix joinConfusion,
            Utilities.ConfusionMatrix joinConfusionGate, Utilities.ConfusionMatrix joinConfusionLabel, 
            Utilities.ConfusionMatrix joinConfusionWire)
        {
            Sketch.Sketch sketch = featureSketch.Sketch;

            Dictionary<Utilities.StrokePair, string> joinResults = new Dictionary<Utilities.StrokePair, string>();
            List<Utilities.StrokePair> pairs = featureSketch.PairwiseFeatureSketch.AllStrokePairs;
            foreach (Utilities.StrokePair pair in pairs)
            {
                Sketch.Substroke strokeA = pair.Stroke1;
                Sketch.Substroke strokeB = pair.Stroke2;

                if (strokeA.Classification != strokeB.Classification)
                {
                    joinResults.Add(pair, "NoJoin");
                    if (origSketch.GetSubstroke(strokeA.Id).ParentShapes[0] == origSketch.GetSubstroke(strokeB.Id).ParentShapes[0])
                        joinConfusion.Add("join", "nojoin");
                    else
                        joinConfusion.Add("nojoin", "nojoin");
                    continue;
                }

                Utilities.SubstrokeDistance distance = featureSketch.PairwiseFeatureSketch.FeatureStrokePair(pair).SubstrokeDistance;
                if (distMeasure == "Min")
                {
                    if (distance.Min <= param[0] && (double)distance.Time <= param[1])
                    {
                        joinResults.Add(pair, "Join");

                        if (origSketch.GetSubstroke(strokeA.Id).ParentShapes[0] == origSketch.GetSubstroke(strokeB.Id).ParentShapes[0])
                        {
                            joinConfusion.Add("join", "join");
                            if (strokeA.Classification == "Gate")
                                joinConfusionGate.Add("join", "join");
                            else if (strokeA.Classification == "Label")
                                joinConfusionLabel.Add("join", "join");
                            else if (strokeA.Classification == "Wire")
                                joinConfusionWire.Add("join", "join");
                        }
                        else
                        {
                            joinConfusion.Add("nojoin", "join");
                            if (strokeA.Classification == "Gate")
                                joinConfusionGate.Add("nojoin", "join");
                            else if (strokeA.Classification == "Label")
                                joinConfusionLabel.Add("nojoin", "join");
                            else if (strokeA.Classification == "Wire")
                                joinConfusionWire.Add("nojoin", "join");
                        }
                    }
                    else
                    {
                        joinResults.Add(pair, "NoJoin");

                        if (origSketch.GetSubstroke(strokeA.Id).ParentShapes[0] == origSketch.GetSubstroke(strokeB.Id).ParentShapes[0])
                        {
                            joinConfusion.Add("join", "nojoin");
                            if (strokeA.Classification == "Gate")
                                joinConfusionGate.Add("join", "nojoin");
                            else if (strokeA.Classification == "Label")
                                joinConfusionLabel.Add("join", "nojoin");
                            else if (strokeA.Classification == "Wire")
                                joinConfusionWire.Add("join", "nojoin");
                        }
                        else
                        {
                            joinConfusion.Add("nojoin", "nojoin");
                            if (strokeA.Classification == "Gate")
                                joinConfusionGate.Add("nojoin", "nojoin");
                            else if (strokeA.Classification == "Label")
                                joinConfusionLabel.Add("nojoin", "nojoin");
                            else if (strokeA.Classification == "Wire")
                                joinConfusionWire.Add("nojoin", "nojoin");
                        }
                            
                    }
                }
                else if (distMeasure == "Max")
                {
                    if (distance.Max <= param[0] && (double)distance.Time <= param[1])
                        joinResults.Add(pair, "Join");
                    else
                        joinResults.Add(pair, "NoJoin");
                }
                else if (distMeasure == "Centroid")
                {
                    if (distance.Avg <= param[0] && (double)distance.Time <= param[1])
                        joinResults.Add(pair, "Join");
                    else
                        joinResults.Add(pair, "NoJoin");
                }
            }

            StrokeGrouper.StrokeGrouperResult result = new StrokeGrouper.StrokeGrouperResult(
                pairs, joinResults, classifications, domain);

            sketch.RemoveGroups();

            foreach (Sketch.Substroke s in sketch.SubstrokesL)
                if (s.ParentShapes.Count > 0 && s.ParentShapes[0].Label == "Unknown")
                    s.ParentShapes[0].Label = s.Classification;


            foreach (Utilities.StrokePair pair in result.JoinedPairs)
            {
                Sketch.Shape shape1 = sketch.GetSubstroke(pair.Stroke1.Id).ParentShapes[0];
                Sketch.Shape shape2 = sketch.GetSubstroke(pair.Stroke2.Id).ParentShapes[0];
                if (shape1 != shape2 && shape1.SubstrokesL.Count != 0 && shape2.SubstrokesL.Count != 0)
                {
                    shape1.Label = pair.Stroke1.Classification;
                    shape2.Label = pair.Stroke2.Classification;
                    shape1 = sketch.mergeShapes(shape1, shape2);
                    shape1.Probability = 0f;
                }
            }

            List<Sketch.Shape> shapes = new List<Sketch.Shape>(sketch.ShapesL);

            return shapes;
        }

        private void calculateGroupingAccuracyFromTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            List<string> filenames = Utilities.General.SelectOpenFiles(out success,
                "ActualPOV files to compile results for",
                "ActualPOV text file (*.txt)|*.txt");
            if (!success)
                return;

            bool successOut;
            string outFile = Utilities.General.SelectSaveFile(out successOut,
                "File to save results to",
                "Text file (*.txt)|*.txt");
            if (!successOut)
                return;

            List<GroupAccuracy> results = new List<GroupAccuracy>();

            foreach (string file in filenames)
            {
                GroupAccuracy res = GetGroupAccuracyForActualPOVfile(file);
                results.Add(res);
            }

            results.Sort(delegate(GroupAccuracy R1, GroupAccuracy R2) { return R1.avgInkFound.CompareTo(R2.avgInkFound); });
            results.Reverse();

            System.IO.StreamWriter writer = new System.IO.StreamWriter(outFile);

            foreach (GroupAccuracy acc in results)
                acc.Write(writer);

            writer.Close();
        }

        private GroupAccuracy GetGroupAccuracyForActualPOVfile(string file)
        {
            GroupAccuracy accuracy = new GroupAccuracy();

            string fileShort = System.IO.Path.GetFileNameWithoutExtension(file);
            if (fileShort.Contains("_d"))
            {
                int index1 = fileShort.IndexOf("_d") + 2;
                accuracy.groupingMethod = "SimpleThreshold" + fileShort.Substring(index1, 3);
                index1 = index1 + 3;
                int index2 = fileShort.IndexOf("_", index1);
                int length = index2 - index1;
                accuracy.distThresh = double.Parse(fileShort.Substring(index1, length));
            }
            if (fileShort.Contains("_t"))
            {
                int index1 = fileShort.IndexOf("_t") + 2;
                int index2 = fileShort.IndexOf("_", index1);
                int length = 1;
                if (index2 == -1)
                    length = fileShort.Length - index1;
                else
                    length = index2 - index1;
                accuracy.timeThresh = double.Parse(fileShort.Substring(index1, length));
            }

            List<double> inkFoundAll = new List<double>();
            List<double> inkExtraAll = new List<double>();
            List<double> inkFoundGate = new List<double>();
            List<double> inkExtraGate = new List<double>();
            List<double> inkFoundWire = new List<double>();
            List<double> inkExtraWire = new List<double>();
            List<double> inkFoundLabel = new List<double>();
            List<double> inkExtraLabel = new List<double>();

            System.IO.StreamReader reader = new System.IO.StreamReader(file);

            string line;
            while ((line = reader.ReadLine()) != null && line != "")
            {
                string[] blocks = line.Split(new char[] { ';' });
                if (blocks.Length != 6)
                    continue;

                string shape = blocks[1];
                string[] missing = blocks[2].Split(new char[] { ' ' });
                string[] extra = blocks[3].Split(new char[] { ' ' });
                double inkFound = double.Parse(blocks[4]);
                double inkExtra = double.Parse(blocks[5]);

                accuracy.count++;
                int totalNumErrors = missing.Length + extra.Length - 2;
                if (totalNumErrors == 0)
                    accuracy.numPerfect++;
                else if (totalNumErrors == 1)
                    accuracy.numWith1Error++;
                else if (totalNumErrors == 2)
                    accuracy.numWith2Error++;

                inkFoundAll.Add(inkFound);
                inkExtraAll.Add(inkExtra);
                if (shape == "Label" || shape == "Text" || shape.ToLower() == "other")
                {
                    accuracy.Labelcount++;
                    if (totalNumErrors == 0)
                        accuracy.LabelnumPerfect++;
                    else if (totalNumErrors == 1)
                        accuracy.LabelnumWith1Error++;
                    else if (totalNumErrors == 2)
                        accuracy.LabelnumWith2Error++;

                    inkFoundLabel.Add(inkFound);
                    inkExtraLabel.Add(inkExtra);
                }
                else if (shape == "Wire" || shape == "Marriage" || shape == "Divorce" || shape == "ChildLink")
                {
                    accuracy.Wirecount++;
                    if (totalNumErrors == 0)
                        accuracy.WirenumPerfect++;
                    else if (totalNumErrors == 1)
                        accuracy.WirenumWith1Error++;
                    else if (totalNumErrors == 2)
                        accuracy.WirenumWith2Error++;

                    inkFoundWire.Add(inkFound);
                    inkExtraWire.Add(inkExtra);
                }
                else
                {
                    accuracy.Gatecount++;
                    if (totalNumErrors == 0)
                        accuracy.GatenumPerfect++;
                    else if (totalNumErrors == 1)
                        accuracy.GatenumWith1Error++;
                    else if (totalNumErrors == 2)
                        accuracy.GatenumWith2Error++;

                    inkFoundGate.Add(inkFound);
                    inkExtraGate.Add(inkExtra);
                }
            }

            double[] inkFoundAllArray = inkFoundAll.ToArray();
            double[] inkExtraAllArray = inkExtraAll.ToArray();
            double[] inkFoundGateArray = inkFoundGate.ToArray();
            double[] inkExtraGateArray = inkExtraGate.ToArray();
            double[] inkFoundWireArray = inkFoundWire.ToArray();
            double[] inkExtraWireArray = inkExtraWire.ToArray();
            double[] inkFoundLabelArray = inkFoundLabel.ToArray();
            double[] inkExtraLabelArray = inkExtraLabel.ToArray();

            if (inkFoundAllArray.Length > 0)
            {
                accuracy.minInkFound = Utilities.Compute.Min(inkFoundAllArray);
                accuracy.maxInkFound = Utilities.Compute.Max(inkFoundAllArray);
                accuracy.avgInkFound = Utilities.Compute.Mean(inkFoundAllArray);
                accuracy.medianInkFound = Utilities.Compute.Median(inkFoundAllArray);
                accuracy.stdDevInkFound = Utilities.Compute.StandardDeviation(inkFoundAllArray);
            }

            if (inkFoundGateArray.Length > 0)
            {
                accuracy.GateminInkFound = Utilities.Compute.Min(inkFoundGateArray);
                accuracy.GatemaxInkFound = Utilities.Compute.Max(inkFoundGateArray);
                accuracy.GateavgInkFound = Utilities.Compute.Mean(inkFoundGateArray);
                accuracy.GatemedianInkFound = Utilities.Compute.Median(inkFoundGateArray);
                accuracy.GatestdDevInkFound = Utilities.Compute.StandardDeviation(inkFoundGateArray);
            }

            if (inkFoundLabelArray.Length > 0)
            {
                accuracy.LabelminInkFound = Utilities.Compute.Min(inkFoundLabelArray);
                accuracy.LabelmaxInkFound = Utilities.Compute.Max(inkFoundLabelArray);
                accuracy.LabelavgInkFound = Utilities.Compute.Mean(inkFoundLabelArray);
                accuracy.LabelmedianInkFound = Utilities.Compute.Median(inkFoundLabelArray);
                accuracy.LabelstdDevInkFound = Utilities.Compute.StandardDeviation(inkFoundLabelArray);
            }

            if (inkFoundWireArray.Length > 0)
            {
                accuracy.WireminInkFound = Utilities.Compute.Min(inkFoundWireArray);
                accuracy.WiremaxInkFound = Utilities.Compute.Max(inkFoundWireArray);
                accuracy.WireavgInkFound = Utilities.Compute.Mean(inkFoundWireArray);
                accuracy.WiremedianInkFound = Utilities.Compute.Median(inkFoundWireArray);
                accuracy.WirestdDevInkFound = Utilities.Compute.StandardDeviation(inkFoundWireArray);
            }

            if (inkExtraAllArray.Length > 0)
            {
                accuracy.minInkExtra = Utilities.Compute.Min(inkExtraAllArray);
                accuracy.maxInkExtra = Utilities.Compute.Max(inkExtraAllArray);
                accuracy.avgInkExtra = Utilities.Compute.Mean(inkExtraAllArray);
                accuracy.medianInkExtra = Utilities.Compute.Median(inkExtraAllArray);
                accuracy.stdDevInkExtra = Utilities.Compute.StandardDeviation(inkExtraAllArray);
            }

            if (inkExtraGateArray.Length > 0)
            {
                accuracy.GateminInkExtra = Utilities.Compute.Min(inkExtraGateArray);
                accuracy.GatemaxInkExtra = Utilities.Compute.Max(inkExtraGateArray);
                accuracy.GateavgInkExtra = Utilities.Compute.Mean(inkExtraGateArray);
                accuracy.GatemedianInkExtra = Utilities.Compute.Median(inkExtraGateArray);
                accuracy.GatestdDevInkExtra = Utilities.Compute.StandardDeviation(inkExtraGateArray);
            }

            if (inkExtraLabelArray.Length > 0)
            {
                accuracy.LabelminInkExtra = Utilities.Compute.Min(inkExtraLabelArray);
                accuracy.LabelmaxInkExtra = Utilities.Compute.Max(inkExtraLabelArray);
                accuracy.LabelavgInkExtra = Utilities.Compute.Mean(inkExtraLabelArray);
                accuracy.LabelmedianInkExtra = Utilities.Compute.Median(inkExtraLabelArray);
                accuracy.LabelstdDevInkExtra = Utilities.Compute.StandardDeviation(inkExtraLabelArray);
            }

            if (inkExtraWireArray.Length > 0)
            {
                accuracy.WireminInkExtra = Utilities.Compute.Min(inkExtraWireArray);
                accuracy.WiremaxInkExtra = Utilities.Compute.Max(inkExtraWireArray);
                accuracy.WireavgInkExtra = Utilities.Compute.Mean(inkExtraWireArray);
                accuracy.WiremedianInkExtra = Utilities.Compute.Median(inkExtraWireArray);
                accuracy.WirestdDevInkExtra = Utilities.Compute.StandardDeviation(inkExtraWireArray);
            }

            if (accuracy.count > 0)
            {
                accuracy.percentPerfect = (double)accuracy.numPerfect / (double)accuracy.count;
                accuracy.percent1orLess = (double)(accuracy.numPerfect + accuracy.numWith1Error) / (double)accuracy.count;
                accuracy.percent2orLess = (double)(accuracy.numPerfect + accuracy.numWith1Error + accuracy.numWith2Error) / (double)accuracy.count;
            }

            if (accuracy.Gatecount > 0)
            {
                accuracy.GatepercentPerfect = (double)accuracy.GatenumPerfect / (double)accuracy.Gatecount;
                accuracy.Gatepercent1orLess = (double)(accuracy.GatenumPerfect + accuracy.GatenumWith1Error) / (double)accuracy.Gatecount;
                accuracy.Gatepercent2orLess = (double)(accuracy.GatenumPerfect + accuracy.GatenumWith1Error + accuracy.GatenumWith2Error) / (double)accuracy.Gatecount;
            }

            if (accuracy.Wirecount > 0)
            {
                accuracy.WirepercentPerfect = (double)accuracy.WirenumPerfect / (double)accuracy.Wirecount;
                accuracy.Wirepercent1orLess = (double)(accuracy.WirenumPerfect + accuracy.WirenumWith1Error) / (double)accuracy.Wirecount;
                accuracy.Wirepercent2orLess = (double)(accuracy.WirenumPerfect + accuracy.WirenumWith1Error + accuracy.WirenumWith2Error) / (double)accuracy.Wirecount;
            }

            if (accuracy.Labelcount > 0)
            {
                accuracy.LabelpercentPerfect = (double)accuracy.LabelnumPerfect / (double)accuracy.Labelcount;
                accuracy.Labelpercent1orLess = (double)(accuracy.LabelnumPerfect + accuracy.LabelnumWith1Error) / (double)accuracy.Labelcount;
                accuracy.Labelpercent2orLess = (double)(accuracy.LabelnumPerfect + accuracy.LabelnumWith1Error + accuracy.LabelnumWith2Error) / (double)accuracy.Labelcount;
            }

            reader.Close();

            return accuracy;
        }

        private void writeAllSSClassifcationsToFIleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            List<string> filenames = Utilities.General.SelectOpenFiles(out success,
                "Sketches",
                "Labeled Sketch XML File (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            bool successOut;
            string outFile = Utilities.General.SelectSaveFile(out successOut,
                "File to save results to",
                "Text file (*.txt)|*.txt");
            if (!successOut)
                return;

            System.IO.StreamWriter writer = new System.IO.StreamWriter(outFile);
            foreach (string file in filenames)
            {
                Sketch.Sketch sketch = new ConverterXML.ReadXML(file).Sketch;
                sketch = Utilities.General.ReOrderParentShapes(sketch);
                sketch = Utilities.General.LinkShapes(sketch);

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);
                int num;
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    string user = num.ToString().PadLeft(index, '0');
                    m_Clusterer.Settings.ClassifierSettings.CurrentUser = new Utilities.User(user);
                }

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(sketch, m_Clusterer.Settings.ClassifierSettings.FeaturesOn, new Dictionary<string, bool>(), new Dictionary<string, double[]>());
                StrokeClassifier.StrokeClassifier classifier = new StrokeClassifier.StrokeClassifier(fSketch, m_Clusterer.Settings.ClassifierSettings);
                StrokeClassifier.StrokeClassifierResult result = classifier.ClassifyWithWeka();
                foreach (KeyValuePair<Sketch.Substroke, string> r in result.AllClassifications)
                {
                    writer.WriteLine(r.Key.Id.ToString() + " " + r.Value);
                }
            }
            writer.Close();
        }

        private void groupWithSimpleUserHoldoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.Year + "_" + time.Month.ToString().PadLeft(2, '0') + "_" + time.Day.ToString().PadLeft(2, '0') + "_"
                + time.Hour.ToString().PadLeft(2, '0') + "_" + time.Minute.ToString().PadLeft(2, '0') + "_" + time.Second.ToString().PadLeft(2, '0');

            bool success;
            List<string> filenames = Utilities.General.SelectOpenFiles(out success,
                "Sketches to test",
                "Labeled Sketches (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            string distMeasure = "Min";

            #region Parameters
            List<int[]> parameters = new List<int[]>();
            parameters.Add(new int[] { 100, 10000000 });
            parameters.Add(new int[] { 200, 10000000 });
            parameters.Add(new int[] { 300, 10000000 });
            parameters.Add(new int[] { 400, 10000000 });
            parameters.Add(new int[] { 100, 1500 });
            parameters.Add(new int[] { 200, 1500 });
            parameters.Add(new int[] { 300, 1500 });
            parameters.Add(new int[] { 400, 1500 });
            parameters.Add(new int[] { 100, 2000 });
            parameters.Add(new int[] { 200, 2000 });
            parameters.Add(new int[] { 300, 2000 });
            parameters.Add(new int[] { 400, 2000 });
            parameters.Add(new int[] { 100, 2500 });
            parameters.Add(new int[] { 200, 2500 });
            parameters.Add(new int[] { 300, 2500 });
            parameters.Add(new int[] { 400, 2500 });
            parameters.Add(new int[] { 100, 3000 });
            parameters.Add(new int[] { 200, 3000 });
            parameters.Add(new int[] { 300, 3000 });
            parameters.Add(new int[] { 400, 3000 });
            parameters.Add(new int[] { 100, 4000 });
            parameters.Add(new int[] { 200, 4000 });
            parameters.Add(new int[] { 300, 4000 });
            parameters.Add(new int[] { 400, 4000 });
            parameters.Add(new int[] { 100, 5000 });
            parameters.Add(new int[] { 200, 5000 });
            parameters.Add(new int[] { 300, 5000 });
            parameters.Add(new int[] { 400, 5000 });
            parameters.Add(new int[] { 100, 6000 });
            parameters.Add(new int[] { 200, 6000 });
            parameters.Add(new int[] { 300, 6000 });
            parameters.Add(new int[] { 400, 6000 });
            parameters.Add(new int[] { 100, 7000 });
            parameters.Add(new int[] { 200, 7000 });
            parameters.Add(new int[] { 300, 7000 });
            parameters.Add(new int[] { 400, 7000 });
            parameters.Add(new int[] { 100, 8000 });
            parameters.Add(new int[] { 200, 8000 });
            parameters.Add(new int[] { 300, 8000 });
            parameters.Add(new int[] { 400, 8000 });
            parameters.Add(new int[] { 100, 9000 });
            parameters.Add(new int[] { 200, 9000 });
            parameters.Add(new int[] { 300, 9000 });
            parameters.Add(new int[] { 400, 9000 });
            parameters.Add(new int[] { 100, 10000 });
            parameters.Add(new int[] { 200, 10000 });
            parameters.Add(new int[] { 300, 10000 });
            parameters.Add(new int[] { 400, 10000 });
            #endregion

            List<KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>> sketches = new List<KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>>();
            Dictionary<string, List<Sketch.Sketch>> user2sketches = new Dictionary<string, List<Sketch.Sketch>>();
            // format for parameters: (0) = max distance gap in ink pixels, (1) = max time gap in milliseconds
            List<string> labels = new List<string>();
            labels.Add("join");
            labels.Add("nojoin");

            Dictionary<int[], List<Utilities.ConfusionMatrix>> confusions = new Dictionary<int[], List<Utilities.ConfusionMatrix>>();
            foreach (int[] param in parameters)
            {
                List<Utilities.ConfusionMatrix> matrices = new List<Utilities.ConfusionMatrix>();
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                matrices.Add(new Utilities.ConfusionMatrix(labels));
                confusions.Add(param, matrices);
                //Utilities.ConfusionMatrix joinConfusion = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionGate = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionLabel = new Utilities.ConfusionMatrix(labels);
                //Utilities.ConfusionMatrix joinConfusionWire = new Utilities.ConfusionMatrix(labels);
            }

            foreach (string file in filenames)
            {
                Sketch.Sketch origSketch = new ConverterXML.ReadXML(file).Sketch;
                origSketch = Utilities.General.ReOrderParentShapes(origSketch);
                origSketch = Utilities.General.LinkShapes(origSketch);

                string sketchFile = System.IO.Path.GetFileNameWithoutExtension(file);

                LoadAppropriateNetworks(sketchFile);

                int num;
                int index = sketchFile.IndexOf('_');
                bool goodUser = int.TryParse(sketchFile.Substring(0, index), out num);
                if (goodUser)
                {
                    string user = num.ToString().PadLeft(index, '0');
                    if (!user2sketches.ContainsKey(user))
                        user2sketches.Add(user, new List<Sketch.Sketch>());
                    user2sketches[user].Add(origSketch);
                    m_Clusterer.Settings.CurrentUser = new Utilities.User(user);
                }

                m_Panel.loadSketch(file, false, true);

                m_Panel.InkSketch.Sketch.resetShapes();

                Featurefy.FeatureSketch fSketch = new Featurefy.FeatureSketch(m_Panel.InkSketch.Sketch,
                    m_Clusterer.FeatureSketch.FeatureListSingle,
                    m_Clusterer.FeatureSketch.FeatureListPair,
                    m_Clusterer.Settings.AvgsAndStdDevs);
                m_Clusterer = new Clusterer.Clusterer(m_Clusterer.Settings, fSketch);
                m_Clusterer.Classifier.AssignClassificationsToFeatureSketch();

                foreach (int[] param in parameters)
                {
                    sketches.Add(new KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>>(origSketch,
                        new KeyValuePair<int[], List<Sketch.Shape>>(param,
                            GetSimpleGroups(m_Clusterer.FeatureSketch,
                                m_Clusterer.Classifier.Result.AllClassifications,
                                m_Clusterer.Settings.ClassifierSettings.Domain,
                                param, distMeasure, origSketch, confusions[param][0],
                                confusions[param][1], confusions[param][2], confusions[param][3]))));
                }
            }


            foreach (int[] param in parameters)
            {
                foreach (string user in user2sketches.Keys)
                {
                    string name1 = "C:\\GroupingAccuracy\\actualPOV_" + timeStr + "_d" + distMeasure + param[0] + "_t" + param[1] + "_u" + user + ".txt";
                    //string name2 = "C:\\GroupingAccuracy\\groupPOV_" + timeStr + "_d" + distMeasure + param[0] + "_t" + param[1] + ".txt";

                    System.IO.StreamWriter writer = new System.IO.StreamWriter(name1);
                    //System.IO.StreamWriter writer2 = new System.IO.StreamWriter(name2);

                    foreach (KeyValuePair<Sketch.Sketch, KeyValuePair<int[], List<Sketch.Shape>>> kvp in sketches)
                    {
                        int[] current = kvp.Value.Key;
                        if (param[0] != current[0] || param[1] != current[1] || !user2sketches[user].Contains(kvp.Key))
                            continue;


                        StrokeGrouper.GrouperAccuracy accuracy = new StrokeGrouper.GrouperAccuracy(kvp.Key, kvp.Value.Value);


                        //foreach (StrokeGrouper.ShapeResult result in accuracy.Results)
                        //result.Print(writer2);

                        foreach (StrokeGrouper.ShapeResultFromActual result in accuracy.ResultsFromActual)
                            result.Print(writer);


                    }

                    //List<Utilities.ConfusionMatrix> matrices = confusions[param];
                    //writer.WriteLine();
                    //foreach (Utilities.ConfusionMatrix m in matrices)
                        //m.Print(writer);

                    writer.Close();
                    //writer2.Close();
                }
            }
        }

        private void calculateGroupingAccuracyFromTextUHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            string dir = Utilities.General.SelectDirectory(out success, "Directory containing ActualPOV files to compile");

            //List<string> filenames = Utilities.General.SelectOpenFiles(out success,
              //  "ActualPOV files to compile results for",
                //"ActualPOV text file (*.txt)|*.txt");
            if (!success)
                return;

            bool successOut;
            string outFile = Utilities.General.SelectSaveFile(out successOut,
                "File to save results to",
                "Text file (*.txt)|*.txt");
            if (!successOut)
                return;

            string[] filenames = System.IO.Directory.GetFiles(dir, "ActualPOV*.txt");

            Dictionary<string, Dictionary<double, Dictionary<double, string>>> user2param2file = new Dictionary<string, Dictionary<double, Dictionary<double, string>>>();
            foreach (string file in filenames)
            {
                double[] param = new double[2];
                string user = "";
                string fileShort = System.IO.Path.GetFileNameWithoutExtension(file);
                if (fileShort.Contains("_d"))
                {
                    int index1 = fileShort.IndexOf("_d") + 2;
                    index1 = index1 + 3;
                    int index2 = fileShort.IndexOf("_", index1);
                    int length = index2 - index1;
                    param[0] = double.Parse(fileShort.Substring(index1, length));
                }
                if (fileShort.Contains("_t"))
                {
                    int index1 = fileShort.IndexOf("_t") + 2;
                    int index2 = fileShort.IndexOf("_", index1);
                    int length = 1;
                    if (index2 == -1)
                        length = fileShort.Length - index1;
                    else
                        length = index2 - index1;
                    param[1] = double.Parse(fileShort.Substring(index1, length));
                }
                if (fileShort.Contains("_u"))
                {
                    int index1 = fileShort.IndexOf("_u") + 2;
                    int index2 = fileShort.IndexOf("_", index1);
                    int length = 1;
                    if (index2 == -1)
                        length = fileShort.Length - index1;
                    else
                        length = index2 - index1;
                    user = fileShort.Substring(index1, length);
                }


                if (!user2param2file.ContainsKey(user))
                    user2param2file.Add(user, new Dictionary<double, Dictionary<double, string>>());
                if (!user2param2file[user].ContainsKey(param[0]))
                    user2param2file[user].Add(param[0], new Dictionary<double, string>());
                if (!user2param2file[user][param[0]].ContainsKey(param[1]))
                    user2param2file[user][param[0]].Add(param[1], file);

            }

            Dictionary<string, double[]> user2bestParams = new Dictionary<string, double[]>();

            foreach (string user in user2param2file.Keys)
            {
                List<GroupAccuracy> results = new List<GroupAccuracy>();
                foreach (double param in user2param2file[user].Keys)
                {
                    foreach (double param2 in user2param2file[user][param].Keys)
                    {
                        string file = "C:\\tempCompile_dMin" + param.ToString("#0") + "_t" + param2.ToString("#0") + ".txt";
                        System.IO.StreamWriter writerCompiler = new System.IO.StreamWriter(file);

                        foreach (KeyValuePair<string, Dictionary<double, Dictionary<double, string>>> kvp in user2param2file)
                        {
                            if (kvp.Key == user)
                                continue;

                            System.IO.StreamReader readerCompiler = new System.IO.StreamReader(kvp.Value[param][param2]);
                            string line;
                            while ((line = readerCompiler.ReadLine()) != null && line != "")
                                writerCompiler.WriteLine(line);
                            readerCompiler.Close();
                        }

                        writerCompiler.Close();

                        GroupAccuracy res = GetGroupAccuracyForActualPOVfile(file);
                        results.Add(res);
                    }
                }

                results.Sort(delegate(GroupAccuracy R1, GroupAccuracy R2) { return R1.avgInkFound.CompareTo(R2.avgInkFound); });
                results.Reverse();

                double[] bestParams = new double[2];
                bestParams[0] = results[0].distThresh;
                bestParams[1] = results[0].timeThresh;
                user2bestParams.Add(user, bestParams);
            }

            System.IO.StreamWriter writer = new System.IO.StreamWriter(outFile);

            foreach (KeyValuePair<string, double[]> kvp in user2bestParams)
                writer.WriteLine(kvp.Key + "\t" + kvp.Value[0] + "\t" + kvp.Value[1]);

            writer.Close();



            string fileFinal = "C:\\tempCompile.txt";
            System.IO.StreamWriter writerCompilerFinal = new System.IO.StreamWriter(fileFinal);

            foreach (KeyValuePair<string, double[]> pair in user2bestParams)
            {
                string user = pair.Key;
                double param = pair.Value[0];
                double param2 = pair.Value[1];

                foreach (KeyValuePair<string, Dictionary<double, Dictionary<double, string>>> kvp in user2param2file)
                {
                    if (kvp.Key != user)
                        continue;

                    System.IO.StreamReader readerCompiler = new System.IO.StreamReader(kvp.Value[param][param2]);
                    string line;
                    while ((line = readerCompiler.ReadLine()) != null && line != "")
                        writerCompilerFinal.WriteLine(line);
                    readerCompiler.Close();
                }
            }

            writerCompilerFinal.Close();
            GroupAccuracy resFinal = GetGroupAccuracyForActualPOVfile(fileFinal);

            string outFileResults = outFile.Replace(".txt", "_GroupingResults.txt");
            System.IO.StreamWriter writerCompilerResults = new System.IO.StreamWriter(outFileResults);
            resFinal.Write(writerCompilerResults);
            writerCompilerResults.Close();

        }

        private void runClassifiersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_Clusterer.Classifier.AssignClassificationsToFeatureSketch();
        }

        private void checkSketchesForUnlabeledStrokesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool success;
            List<string> sketchFiles = Utilities.General.SelectOpenFiles(out success,
                "Sketch files to check for unlabeled strokes",
                "Labeled XML Files (*.labeled.xml)|*.labeled.xml");
            if (!success)
                return;

            foreach (string sketchFile in sketchFiles)
            {
                Sketch.Sketch sketch = new ConverterXML.ReadXML(sketchFile).Sketch;
                string name = System.IO.Path.GetFileNameWithoutExtension(sketchFile);

                foreach (Sketch.Substroke stroke in sketch.SubstrokesL)
                {
                    if (stroke.FirstLabel == "unlabeled")
                    {

                        Console.WriteLine("Unlabeled Stroke Found: " + name);
                    }
                    
                }
            }
        }
    }

    class GroupAccuracy
    {
        public string groupingMethod;
        public double distThresh;
        public double timeThresh;

        public int count;
        public int numPerfect;
        public int numWith1Error;
        public int numWith2Error;
        public double percentPerfect;
        public double percent1orLess;
        public double percent2orLess;
        public Utilities.ConfusionMatrix joinConfusion;

        public int Gatecount;
        public int GatenumPerfect;
        public int GatenumWith1Error;
        public int GatenumWith2Error;
        public double GatepercentPerfect;
        public double Gatepercent1orLess;
        public double Gatepercent2orLess;
        public Utilities.ConfusionMatrix joinConfusionGate;

        public int Wirecount;
        public int WirenumPerfect;
        public int WirenumWith1Error;
        public int WirenumWith2Error;
        public double WirepercentPerfect;
        public double Wirepercent1orLess;
        public double Wirepercent2orLess;
        public Utilities.ConfusionMatrix joinConfusionWire;

        public int Labelcount;
        public int LabelnumPerfect;
        public int LabelnumWith1Error;
        public int LabelnumWith2Error;
        public double LabelpercentPerfect;
        public double Labelpercent1orLess;
        public double Labelpercent2orLess;
        public Utilities.ConfusionMatrix joinConfusionLabel;

        public double avgInkFound;
        public double minInkFound;
        public double maxInkFound;
        public double stdDevInkFound;
        public double medianInkFound;

        public double avgInkExtra;
        public double minInkExtra;
        public double maxInkExtra;
        public double stdDevInkExtra;
        public double medianInkExtra;

        public double GateavgInkFound;
        public double GateminInkFound;
        public double GatemaxInkFound;
        public double GatestdDevInkFound;
        public double GatemedianInkFound;

        public double GateavgInkExtra;
        public double GateminInkExtra;
        public double GatemaxInkExtra;
        public double GatestdDevInkExtra;
        public double GatemedianInkExtra;

        public double WireavgInkFound;
        public double WireminInkFound;
        public double WiremaxInkFound;
        public double WirestdDevInkFound;
        public double WiremedianInkFound;

        public double WireavgInkExtra;
        public double WireminInkExtra;
        public double WiremaxInkExtra;
        public double WirestdDevInkExtra;
        public double WiremedianInkExtra;

        public double LabelavgInkFound;
        public double LabelminInkFound;
        public double LabelmaxInkFound;
        public double LabelstdDevInkFound;
        public double LabelmedianInkFound;

        public double LabelavgInkExtra;
        public double LabelminInkExtra;
        public double LabelmaxInkExtra;
        public double LabelstdDevInkExtra;
        public double LabelmedianInkExtra;

        public GroupAccuracy()
        {
            List<string> labels = new List<string>();
            labels.Add("join");
            labels.Add("nojoin");

            joinConfusion = new Utilities.ConfusionMatrix(labels);
            joinConfusionGate = new Utilities.ConfusionMatrix(labels);
            joinConfusionLabel = new Utilities.ConfusionMatrix(labels);
            joinConfusionWire = new Utilities.ConfusionMatrix(labels);
        }

        internal void Write(System.IO.StreamWriter writer)
        {
            writer.Write(groupingMethod + "\t");
            writer.Write(distThresh.ToString("#0.0") + "\t");
            writer.Write(timeThresh.ToString("#0.0") + "\t");
            writer.Write(count.ToString() + "\t");
            writer.Write(numPerfect.ToString() + "\t");
            writer.Write(numWith1Error.ToString() + "\t");
            writer.Write(numWith2Error.ToString() + "\t");
            writer.Write(percentPerfect.ToString("#0.00000") + "\t");
            writer.Write(percent1orLess.ToString("#0.00000") + "\t");
            writer.Write(percent2orLess.ToString("#0.00000") + "\t");
            writer.Write(avgInkFound.ToString("#0.00000") + "\t");
            writer.Write(stdDevInkFound.ToString("#0.00000") + "\t");
            writer.Write(minInkFound.ToString("#0.00000") + "\t");
            writer.Write(maxInkFound.ToString("#0.00000") + "\t");
            writer.Write(medianInkFound.ToString("#0.00000") + "\t");
            writer.Write(avgInkExtra.ToString("#0.00000") + "\t");
            writer.Write(stdDevInkExtra.ToString("#0.00000") + "\t");
            writer.Write(minInkExtra.ToString("#0.00000") + "\t");
            writer.Write(maxInkExtra.ToString("#0.00000") + "\t");
            writer.Write(medianInkExtra.ToString("#0.00000") + "\t");
            writer.Write(Gatecount.ToString() + "\t");
            writer.Write(GatenumPerfect.ToString() + "\t");
            writer.Write(GatenumWith1Error.ToString() + "\t");
            writer.Write(GatenumWith2Error.ToString() + "\t");
            writer.Write(GatepercentPerfect.ToString("#0.00000") + "\t");
            writer.Write(Gatepercent1orLess.ToString("#0.00000") + "\t");
            writer.Write(Gatepercent2orLess.ToString("#0.00000") + "\t");
            writer.Write(GateavgInkFound.ToString("#0.00000") + "\t");
            writer.Write(GatestdDevInkFound.ToString("#0.00000") + "\t");
            writer.Write(GateminInkFound.ToString("#0.00000") + "\t");
            writer.Write(GatemaxInkFound.ToString("#0.00000") + "\t");
            writer.Write(GatemedianInkFound.ToString("#0.00000") + "\t");
            writer.Write(GateavgInkExtra.ToString("#0.00000") + "\t");
            writer.Write(GatestdDevInkExtra.ToString("#0.00000") + "\t");
            writer.Write(GateminInkExtra.ToString("#0.00000") + "\t");
            writer.Write(GatemaxInkExtra.ToString("#0.00000") + "\t");
            writer.Write(GatemedianInkExtra.ToString("#0.00000") + "\t");
            writer.Write(Wirecount.ToString() + "\t");
            writer.Write(WirenumPerfect.ToString() + "\t");
            writer.Write(WirenumWith1Error.ToString() + "\t");
            writer.Write(WirenumWith2Error.ToString() + "\t");
            writer.Write(WirepercentPerfect.ToString("#0.00000") + "\t");
            writer.Write(Wirepercent1orLess.ToString("#0.00000") + "\t");
            writer.Write(Wirepercent2orLess.ToString("#0.00000") + "\t");
            writer.Write(WireavgInkFound.ToString("#0.00000") + "\t");
            writer.Write(WirestdDevInkFound.ToString("#0.00000") + "\t");
            writer.Write(WireminInkFound.ToString("#0.00000") + "\t");
            writer.Write(WiremaxInkFound.ToString("#0.00000") + "\t");
            writer.Write(WiremedianInkFound.ToString("#0.00000") + "\t");
            writer.Write(WireavgInkExtra.ToString("#0.00000") + "\t");
            writer.Write(WirestdDevInkExtra.ToString("#0.00000") + "\t");
            writer.Write(WireminInkExtra.ToString("#0.00000") + "\t");
            writer.Write(WiremaxInkExtra.ToString("#0.00000") + "\t");
            writer.Write(WiremedianInkExtra.ToString("#0.00000") + "\t");
            writer.Write(Labelcount.ToString() + "\t");
            writer.Write(LabelnumPerfect.ToString() + "\t");
            writer.Write(LabelnumWith1Error.ToString() + "\t");
            writer.Write(LabelnumWith2Error.ToString() + "\t");
            writer.Write(LabelpercentPerfect.ToString("#0.00000") + "\t");
            writer.Write(Labelpercent1orLess.ToString("#0.00000") + "\t");
            writer.Write(Labelpercent2orLess.ToString("#0.00000") + "\t");
            writer.Write(LabelavgInkFound.ToString("#0.00000") + "\t");
            writer.Write(LabelstdDevInkFound.ToString("#0.00000") + "\t");
            writer.Write(LabelminInkFound.ToString("#0.00000") + "\t");
            writer.Write(LabelmaxInkFound.ToString("#0.00000") + "\t");
            writer.Write(LabelmedianInkFound.ToString("#0.00000") + "\t");
            writer.Write(LabelavgInkExtra.ToString("#0.00000") + "\t");
            writer.Write(LabelstdDevInkExtra.ToString("#0.00000") + "\t");
            writer.Write(LabelminInkExtra.ToString("#0.00000") + "\t");
            writer.Write(LabelmaxInkExtra.ToString("#0.00000") + "\t");
            writer.Write(LabelmedianInkExtra.ToString("#0.00000") + "\t");

            writer.WriteLine();
        }
    }

}