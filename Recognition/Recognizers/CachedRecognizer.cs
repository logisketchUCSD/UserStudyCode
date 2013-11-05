using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecognitionInterfaces;

namespace Recognizers
{

    /// <summary>
    /// This class uses hash codes to cache the recognition results of an inner
    /// recognizer. If a recognizer may be asked repeatedly to recognize the
    /// same shapes, it can help to wrap it in a CachedRecognizer.
    /// </summary>
    class CachedRecognizer : Recognizer
    {

        private Recognizer _innerRecognizer;
        private Dictionary<int, string> _typeResults;
        private Dictionary<int, float> _probabilityResults;

        /// <summary>
        /// Construct a new cached recognizer using the given recognizer as a
        /// backend.
        /// </summary>
        /// <param name="worker">the recognizer to use as a backend</param>
        public CachedRecognizer(Recognizer worker)
        {
            _innerRecognizer = worker;
            _typeResults = new Dictionary<int, string>();
            _probabilityResults = new Dictionary<int, float>();
        }

        /// <summary>
        /// Uses the cached result if it is available, or makes a call to the inner
        /// recognizer otherwise. This method is thread-safe provided that
        ///    1. the underlying recognizer's "recognize" method is thread-safe
        ///    2. this method will not be called concurrently on the same shape
        /// </summary>
        /// <param name="shape">the shape to recognize</param>
        /// <param name="featureSketch">the featureSketch to work on</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            int hash = shape.GetHashCode();

            // Safely determine if the value is already cached. We never remove cached
            // results, so it is safe to release the lock afterward.
            bool isCached;
            lock (_typeResults)
                isCached = _typeResults.ContainsKey(hash);

            if (isCached)
            {
                // Write the cached results to the shape. Since we can assume that
                // we don't need to lock the shape, all we need to synchronize on is
                // the dictionary when we retreive results.
                ShapeType type;
                float prob;
                lock (_typeResults)
                {
                    type = LogicDomain.getType(_typeResults[hash]);
                    prob = _probabilityResults[hash];
                }
                shape.setRecognitionResults(type, prob);
            }
            else
            {
                // We only need a lock here when we write results to the dictionary.
                _innerRecognizer.recognize(shape, featureSketch);
                lock (_typeResults)
                {
                    _typeResults.Add(hash, shape.Type.Name);
                    _probabilityResults.Add(hash, shape.Probability);
                }
            }

        }


        /// <summary>
        /// Teaches the worker recognizer.
        /// </summary>
        /// <param name="shape">the shape to learn</param>
        public override void learnFromExample(Sketch.Shape shape)
        {
            _innerRecognizer.learnFromExample(shape);
        }

        /// <summary>
        /// Saves the worker recognizer.
        /// </summary>
        public override void save()
        {
            _innerRecognizer.save();
        }

        /// <summary>
        /// Retreive or change the inner recognizer.
        /// </summary>
        public Recognizer InnerRecognizer
        {
            get { return _innerRecognizer; }
            set { _innerRecognizer = value; }
        }

        /// <summary>
        /// Calls the inner recognizer's canRecognize function.
        /// </summary>
        /// <param name="classification">the classification to check</param>
        /// <returns>what the inner recognizer returns</returns>
        public override bool canRecognize(string classification)
        {
            return _innerRecognizer.canRecognize(classification);
        }

    }

}
