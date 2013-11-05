/*
 * File: RecognitionManager.cs
 *
 * Author: Sketchers 2010
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2010.
 * 
 */

using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using RecognitionInterfaces;

namespace RecognitionManager
{
    /// <summary>
    /// The RecognitionManager listens for events on a sketch panel and responds
    /// accordingly. It is the middle ground between the UI and the recognition
    /// sides of the program.
    /// </summary>
    public class RecognitionManager
    {

        #region Members

        /// <summary>
        /// The filenames of the settings we'll use for recognition.
        /// </summary>
        private Dictionary<string, string> _filenames;

        /// <summary>
        /// The featuresketch we're working with.
        /// </summary>
        private Featurefy.FeatureSketch _featuresketch;

        /// <summary>
        /// The panel on the screen that the user interacts with.
        /// </summary>
        private SketchPanelLib.SketchPanel _panel;

        /// <summary>
        /// The domain this manager operates in. Specifically, the
        /// circuit domain.
        /// </summary>
        private ContextDomain.ContextDomain _domain;

        /// <summary>
        /// The classifier that handles single stroke classification.
        /// </summary>
        private Classifier _strokeClassifier;

        /// <summary>
        /// The grouper that handles grouping strokes into shapes.
        /// </summary>
        private Grouper _strokeGrouper;

        /// <summary>
        /// The recognizer used for recognizing shapes.
        /// </summary>
        private Recognizer _sketchRecognizer;

        /// <summary>
        /// The connector used for making connections between shapes.
        /// </summary>
        private Connector _connector;

        /// <summary>
        /// The refiner pipeline runs several refiners to improve recognition after 
        /// everything else has happened.
        /// </summary>
        private IRecognitionStep _refinement;

        /// <summary>
        /// Set to true to pring sebugging info
        /// </summary>
        private bool debug = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a Recognition Manager for the given sketch panel with default settings.
        /// Settings are loaded from file settings.txt
        /// </summary>
        /// <param name="p">a sketch panel to manage</param>
        public RecognitionManager(SketchPanelLib.SketchPanel p)
        {
            // Load settings from text file
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string SettingsFilename = directory + "//settings.txt";
            _filenames = Files.SettingsReader.readSettings(SettingsFilename);

            // Initialize the recognition machines
            _domain = ContextDomain.CircuitDomain.GetInstance();
            _strokeClassifier = RecognitionPipeline.createDefaultClassifier();
            _strokeGrouper = RecognitionPipeline.createDefaultGrouper();
            _sketchRecognizer = RecognitionPipeline.createDefaultRecognizer();
            _connector = RecognitionPipeline.createDefaultConnector();
            _refinement = RecognitionPipeline.createDefaultRefiner(_connector, _sketchRecognizer);

            // Add events
            _panel = p;
            _featuresketch = p.InkSketch.FeatureSketch;
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// Returns true iff the substrokes in the sketch, the feature sketch, the pairwise feature sketch,
        /// and the intersection sketch are the same.
        /// </summary>
        public bool sketchSubstrokesAreConsistent()
        {
            return _featuresketch.hasConsistentSubstrokes();
        }
        #endregion

        #region Recognition Steps

        ////////////////////////////////////////////////
        // The order of recognition steps is as follows:
        //   1: Classify Single Strokes
        //   2: Group Strokes into Shapes
        //   3: Recognize Shapes
        //   4: Connect Shapes
        //   5: Refine Recognition
        /////////////////////////////////////////////////

        /// <summary>
        /// Applies the single stroke recognizer to the internal sketch.
        /// </summary>
        public void ClassifySingleStrokes()
        {
            _strokeClassifier.process(_featuresketch);
        }

        /// <summary>
        /// Applies the stroke grouper to the sketch, and forms the resulting shapes in the sketch.
        /// 
        /// Precondition: ClassifySingleStrokes() has been called.
        /// </summary>
        public void GroupStrokes()
        {
            _strokeGrouper.process(_featuresketch);
        }

        /// <summary>
        /// Recognizes each shape in the sketch with the shape recognizer.
        /// 
        /// Precondition: GroupStrokes() has been called.
        /// </summary>
        public void Recognize()
        {
            _sketchRecognizer.process(_featuresketch);
        }

        /// <summary>
        /// Connects the sketch based on class or label.
        /// 
        /// Precondition: Recognize() has been called.
        /// </summary>
        public void ConnectSketch()
        {
            _connector.process(_featuresketch);
        }

        /// <summary>
        /// Refines the recognition after the shapes have all been recognized.
        /// 
        /// Precondition: ConnectSketch() has been called.
        /// </summary>
        public void RefineSketch()
        {
            _refinement.process(_featuresketch);

            if (debug)
            {
                Console.WriteLine("Connections:");
                foreach (Sketch.Shape shape in _featuresketch.Sketch.Shapes)
                {
                    Console.Write("  --> " + shape.Name + ": ");
                    bool first = true;
                    foreach (Sketch.Shape connectedShape in shape.ConnectedShapes)
                    {
                        if (!first)
                            Console.Write(", ");
                        first = false;

                        Console.Write(connectedShape.Name);
                    }
                    Console.WriteLine();
                }
            }
        }

        #endregion

        #region Other

        public void MakeShapeNames()
        {
            new Refiner.UniqueNamer().process(_featuresketch);
        }


        /// <summary>
	    /// Test the validity of each labeled shape.
	    /// </summary>
	    /// <returns>A dictionary mapping shapes to validity.</returns>
	    public Dictionary<Sketch.Shape, bool> TestValidity()
	    {
            Sketch.Sketch sketch = _featuresketch.Sketch;
	        Dictionary<Sketch.Shape, bool> dict = new Dictionary<Sketch.Shape, bool>();
	        foreach (Sketch.Shape shape in sketch.Shapes)
	        {
	            dict.Add(shape, _domain.IsProperlyConnected(shape));
	        }
	
	        return dict;
	    }

        /// <summary>
        /// Rerecognizes the strokes given as a single group
        /// </summary>
        public void RerecognizeGroup(Sketch.Shape shape)
        {
            regroupShape(shape);
            shape.ClearConnections();
            _connector.connect(shape, _featuresketch.Sketch);
        }

        /// <summary>
        /// Regroups and recognizes the shape
        /// </summary>
        /// <param name="shape"></param>
        private void regroupShape(Sketch.Shape shape)
        {
            if (shape.Type == new ShapeType())
            {
                //shape.Classification = (new ShapeType()).Classification;
                // If any strokes do not have classifications, we need to reclassify
                foreach (Sketch.Substroke substroke in shape.Substrokes)
                {
                    _strokeClassifier.classify(substroke, _featuresketch);
                }
            }

            if (shape.Substrokes.Length > 0)
            {
                // Give all the substrokes the same classification since they are in the same group
                string Classification = shape.Substrokes[0].Classification;
                shape.Classification = Classification;

                _sketchRecognizer.recognize(shape, _featuresketch);
            }

            else
            {
                Console.WriteLine("WARNING No substrokes available");
            }

            MakeShapeNames();
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Gets and sets the featuresketch.
        /// </summary>
        public Featurefy.FeatureSketch Featuresketch
        {
            get { return _featuresketch; }
            set { _featuresketch = value; }
        }

        /// <summary>
        /// Gets and sets the classifier used to classify strokes
        /// </summary>
        public RecognitionInterfaces.Classifier StrokeClassifier
        {
            get { return _strokeClassifier; }
            set { _strokeClassifier = value; }
        }

        /// <summary>
        /// Gets and sets the grouper used to group strokes into shapes
        /// </summary>
        public RecognitionInterfaces.Grouper StrokeGrouper
        {
            get { return _strokeGrouper; }
            set { _strokeGrouper = value; }
        }

        /// <summary>
        /// Gets and sets the recognizer used to identify shapes
        /// </summary>
        public RecognitionInterfaces.Recognizer Recognizer
        {
            get { return _sketchRecognizer; }
            set { _sketchRecognizer = value; }
        }

        #endregion

    }
}
