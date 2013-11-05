using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecognitionInterfaces;
using Featurefy;

namespace RecognitionManager
{
    /// <summary>
    /// The RecognitionPipeline is responsible for handling the steps in the recognition process. It
    /// aims to be an easy-to-use class that can run recognition steps for you.
    /// </summary>
    public class RecognitionPipeline : IRecognitionStep
    {

        #region Default Steps

        private static readonly Lazy<Recognizers.ImageRecognizer> imageRecognizer = new Lazy<Recognizers.ImageRecognizer>(delegate() { return Recognizers.ImageRecognizer.Load(AppDomain.CurrentDomain.BaseDirectory + @"\\SubRecognizers\ImageRecognizer\Image.ir"); });

        private static readonly Lazy<ContextDomain.ContextDomain> defaultDomain = new Lazy<ContextDomain.ContextDomain>(delegate() { return ContextDomain.CircuitDomain.GetInstance(); });
        private static readonly Lazy<Classifier> defaultClassifier = new Lazy<Classifier>(delegate() { return new StrokeClassifier.StrokeClassifier(); });
        private static readonly Lazy<Grouper> defaultGrouper = new Lazy<Grouper>(delegate() { return new StrokeGrouper.StrokeGrouper(); });
        private static readonly Lazy<Recognizer> defaultRecognizer = new Lazy<Recognizer>(delegate() { return new Recognizers.UniversalRecognizer(null, null, imageRecognizer.Value); });
        private static readonly Lazy<Orienter> defaultOrienter = new Lazy<Orienter>(delegate() { return new Orienter(imageRecognizer.Value); });
        private static readonly Lazy<Connector> defaultConnector = new Lazy<Connector>(delegate() { return new Connector(defaultDomain.Value); });
        private static readonly Lazy<IRecognitionStep> defaultRefiner = new Lazy<IRecognitionStep>(delegate() {
            RecognitionPipeline refinement = new RecognitionPipeline();
#if USE_SEARCH_REFINEMENT
            refinement.addStep(new Refiner.SearchRefiner(defaultDomain.Value));
#else
            refinement.addStep(new Refiner.CarefulContextRefiner(defaultDomain.Value, defaultRecognizer.Value));
#endif
            //refinement.addStep(new Refiner.StrokeStealRefiner(connector, recognizer));
            //refinement.addStep(new Refiner.StrokeShedRefiner(connector, recognizer));
            refinement.addStep(new Refiner.UniqueNamer());
            refinement.addStep(new Refiner.GroupNotBubble());
            refinement.addStep(new Refiner.RecognizedMarker());
            return refinement;
        });

        /// <summary>
        /// Create a default recognition pipeline with the following steps:
        ///   1: Classify Single Strokes
        ///   2: Group Strokes into Shapes
        ///   3: Recognize Shapes
        ///   4: Connect Shapes
        ///   5: Refine Recognition
        /// </summary>
        public static RecognitionPipeline createDefaultPipeline(Dictionary<string, string> settings)
        {
            Recognizer recognizer = createDefaultRecognizer();
            Connector connector   = createDefaultConnector();

            RecognitionPipeline result = new RecognitionPipeline();
            result.addStep(createDefaultClassifier());
            result.addStep(createDefaultGrouper());
            result.addStep(recognizer);
            result.addStep(connector);
            result.addStep(createDefaultRefiner(connector, recognizer));
            return result;
        }

        /// <summary>
        /// The pipeline for recognizing on the fly
        /// </summary>
        /// <returns></returns>
        public static RecognitionPipeline createOnFlyPipeline()
        {
            Recognizer recognizer = createDefaultRecognizer();

            RecognitionPipeline result = new RecognitionPipeline();
            result.addStep(createDefaultClassifier());
            result.addStep(createDefaultGrouper());
            result.addStep(recognizer);
            return result;
        }

        /// <summary>
        /// Return the classifier that should be used by the main UI program.
        /// </summary>
        /// <returns>a new classifier</returns>
        public static Classifier createDefaultClassifier()
        {
            return defaultClassifier.Value;
        }

        /// <summary>
        /// Return the grouper that should be used by the main UI program.
        /// </summary>
        /// <returns>a new grouper</returns>
        public static Grouper createDefaultGrouper()
        {
            return defaultGrouper.Value;
        }

        /// <summary>
        /// Return the recognizer that should be used by the main UI program.
        /// </summary>
        /// <returns>a new recognizer</returns>
        public static Recognizer createDefaultRecognizer()
        {
            return defaultRecognizer.Value;
        }

        /// <summary>
        /// Return the orienter that should be used by the main UI program.
        /// </summary>
        /// <returns></returns>
        public static Orienter createDefaultOrienter()
        {
            return defaultOrienter.Value;
        }

        /// <summary>
        /// Return the connector that should be used by the main UI program.
        /// </summary>
        /// <returns>a new connector</returns>
        public static Connector createDefaultConnector()
        {
            return defaultConnector.Value;
        }

        /// <summary>
        /// Return the domain that should be used by the main UI program
        /// </summary>
        /// <returns></returns>
        public static ContextDomain.ContextDomain createDefaultDomain()
        {
            return defaultDomain.Value;
        }

        /// <summary>
        /// Return the refiner that should be used by the main UI program.
        /// </summary>
        /// <returns>a new refiner</returns>
        public static RecognitionInterfaces.IRecognitionStep createDefaultRefiner(Connector connector, Recognizer recognizer)
        {
            return defaultRefiner.Value;
        }

        #endregion

        #region Internals

        /// <summary>
        /// The list of steps performed by this pipeline.
        /// </summary>
        private List<IRecognitionStep> _steps;

        #endregion

        #region Constructor

        /// <summary>
        /// Create an empty RecognitionPipeline.
        /// </summary>
        public RecognitionPipeline()
        {
            _steps = new List<IRecognitionStep>();
        }

        #endregion

        #region Managing the Pipeline

        /// <summary>
        /// Add a step to the pipeline.
        /// </summary>
        /// <param name="step">the step to add</param>
        public virtual void addStep(IRecognitionStep step)
        {
            _steps.Add(step);
        }

        #endregion

        #region Processing a Sketch

        /// <summary>
        /// Process a sketch. This runs every recognition step on the sketch in
        /// the order they were added to this pipeline. This method does nothing
        /// if no steps have been added.
        /// </summary>
        /// <param name="featureSketch">the sketch to process</param>
        public virtual void process(FeatureSketch featureSketch)
        {
            foreach (IRecognitionStep step in _steps)
            {
                step.process(featureSketch);
            }
        }

        /// <summary>
        /// Process an ordinary sketch by first creating a feature sketch for it.
        /// This method calls process using the created feature sketch, then
        /// returns the feature sketch, in case you want to use it. The given 
        /// ordinary sketch is modified in the process.
        /// </summary>
        /// <param name="sketch">the ordinary sketch to use</param>
        /// <returns>the featureSketch used during processing</returns>
        public FeatureSketch process(Sketch.Sketch sketch)
        {
            FeatureSketch featureSketch = FeatureSketch.MakeFeatureSketch(sketch);
            process(featureSketch);
            return featureSketch;
        }

        #endregion

    }
}
