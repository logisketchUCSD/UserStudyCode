using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using RecognitionInterfaces;

namespace Recognizers
{
    /// <summary>
    /// Recognizes all kinds of shapes: wires, text, and gates. Uses several different sub-recognizers.
    /// </summary>
    public class UniversalRecognizer : Recognizer
    {

        #region Constants

        /// <summary>
        /// Whether or not to enable debug output in the console.
        /// </summary>
        private const bool DEBUG = false;

        #endregion

        #region Internals

        /// <summary>
        /// The recognizer that will be used to identify wires.
        /// </summary>
        private Recognizer _wireRecognizer;

        /// <summary>
        /// The recognizer that will be used to identify text.
        /// </summary>
        private Recognizer _textRecognizer;

        /// <summary>
        /// The recognizer that will be used to identify gates.
        /// </summary>
        private Recognizer _gateRecognizer;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a default UniversalRecognizer with caching.
        /// </summary>
        public UniversalRecognizer()
            :this(true)
        {
        }

        /// <summary>
        /// Default constructor. Uses Recognizers.WireRecognizer as the wire
        /// recognizer, Recognizers.TextRecognizer as the text recognizer, and
        /// CombinationRecognizer.ComboRecognizer as the gate recognizer.
        /// </summary>
        /// <param name="cacheInnerRecognizers">True if the individual 
        /// sub-recognizers should cache their results. (NOTE: you typically 
        /// would not want to wrap a UniversalRecognizer in a CachedRecognizer
        /// since the CachedRecognizer ignores the shape classification when
        /// retrieving results. So if you did that and changed the shape 
        /// classification, the resulting recognizer may often ignore it.</param>
        public UniversalRecognizer(bool cacheInnerRecognizers)
            :this(null, null, null, true)
        {
        }

        /// <summary>
        /// Constructs a universal recognizer that using the specified 
        /// recognizers and caching.
        /// </summary>
        /// <param name="wireRecognizer">the recognizer to use on wires</param>
        /// <param name="textRecognizer">the recognizer to use on text</param>
        /// <param name="gateRecognizer">the recognizer to use on gates</param>
        public UniversalRecognizer(
            Recognizer wireRecognizer,
            Recognizer textRecognizer,
            Recognizer gateRecognizer)
            :this (wireRecognizer, textRecognizer, gateRecognizer, true)
        {
        }


        /// <summary>
        /// Constructs a universal recognizer that using the specified 
        /// recognizers and caching enabled or disabled.
        /// </summary>
        /// <param name="wireRecognizer">the recognizer to use on wires</param>
        /// <param name="textRecognizer">the recognizer to use on text</param>
        /// <param name="gateRecognizer">the recognizer to use on gates</param>
        /// <param name="cacheInnerRecognizers">True if the individual 
        /// sub-recognizers should cache their results. (NOTE: you typically 
        /// would not want to wrap a UniversalRecognizer in a CachedRecognizer
        /// since the CachedRecognizer ignores the shape classification when
        /// retrieving results. So if you did that and changed the shape 
        /// classification, the resulting recognizer may often ignore it.</param>
        /// <exception cref="ArgumentException.ArgumentException">Thrown if 
        /// the provided recognizers cannot recognize what they are intended
        /// to (e.g., if the wireRecognizer reports that it cannot recognize
        /// wires).</exception>
        public UniversalRecognizer(
            Recognizer wireRecognizer,
            Recognizer textRecognizer,
            Recognizer gateRecognizer,
            bool cacheInnerRecognizers)
        {
            if (wireRecognizer == null)
                wireRecognizer = new WireRecognizer();
            if (textRecognizer == null)
                textRecognizer = new TextRecognizer();
            if (gateRecognizer == null)
                gateRecognizer = ImageRecognizer.Load(AppDomain.CurrentDomain.BaseDirectory + @"SubRecognizers\ImageRecognizer\Image.ir");

            if (!wireRecognizer.canRecognize("Wire"))
                throw new ArgumentException("The provided wire recognizer '" + wireRecognizer + "' cannot recognize wires!");
            if (!textRecognizer.canRecognize("Text"))
                throw new ArgumentException("The provided text recognizer '" + textRecognizer + "' cannot recognize text!");
            if (!gateRecognizer.canRecognize("Gate"))
                throw new ArgumentException("The provided gate recognizer '" + gateRecognizer + "' cannot recognize gates!");

            if (cacheInnerRecognizers)
            {
                wireRecognizer = new CachedRecognizer(wireRecognizer);
                textRecognizer = new CachedRecognizer(textRecognizer);
                gateRecognizer = new CachedRecognizer(gateRecognizer);
            }

            _wireRecognizer = wireRecognizer;
            _textRecognizer = textRecognizer;
            _gateRecognizer = gateRecognizer;
        }

        #endregion

        #region Recognition Methods

        /// <summary>
        /// Recognizes a given shape and updates the shape accordingly.
        /// </summary>
        /// <param name="shape">The shape to recogize</param>
        /// <param name="featureSketch">A featuresketch</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            // Make sure we do nothing if the user specified the label
            if (shape.AlreadyLabeled)
                return;
            
            // Use a particular recognizer based on how the shape was
            // classified.

            if (shape.Classification == LogicDomain.WIRE_CLASS)
            {
                _wireRecognizer.recognize(shape, featureSketch);
            }
            else if (shape.Classification == LogicDomain.TEXT_CLASS)
            {
                _textRecognizer.recognize(shape, featureSketch);
            }
            else if (shape.Classification == LogicDomain.GATE_CLASS)
            {
                _gateRecognizer.recognize(shape, featureSketch);
            }
            else
            {
                Console.WriteLine(
                    "Error in shape recognition: cannot recognize" +
                    " a shape with the classification \"" +
                    shape.Classification + "\"");
            }
        }


        /// <summary>
        /// Use the given shape as a learning example to update the recognizer. This
        /// trains only the relevant sub-recognizer depending on what type the
        /// shape is (gate, wire, or label).
        /// </summary>
        /// <param name="shape">the shape to learn from</param>
        public override void learnFromExample(Sketch.Shape shape)
        {
            if (Domain.LogicDomain.IsGate(shape.Type))
                _gateRecognizer.learnFromExample(shape);
            else if (Domain.LogicDomain.IsWire(shape.Type))
                _wireRecognizer.learnFromExample(shape);
            else if (Domain.LogicDomain.IsText(shape.Type))
                _textRecognizer.learnFromExample(shape);
        }

        /// <summary>
        /// Save settings for all sub-recognizers.
        /// </summary>
        public override void save()
        {
            _gateRecognizer.save();
            _wireRecognizer.save();
            _textRecognizer.save();
        }

        #endregion

    }

}
