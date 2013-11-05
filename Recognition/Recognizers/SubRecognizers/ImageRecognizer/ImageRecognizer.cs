using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Sketch;
using CombinationRecognizer;
using SubRecognizer;
using Domain;
using Data;

namespace Recognizers
{
    /// <summary>
    /// This recognizer is basically a wrapper around the BitmapSymbols.
    /// It has a list of templates (BitmapSymbols) that it attempts to 
    /// find the best match with with you call the Recognize(strokes) 
    /// function.
    /// 
    ///  See http://www.andrew.cmu.edu/user/lkara/publications/kara_stahovich_CG2005.pdf
    /// </summary>
    [Serializable]
    public class ImageRecognizer : RecognitionInterfaces.Recognizer, ISerializable, RecognitionInterfaces.IOrienter
    {

        #region Internals

        /// <summary>
        /// The desired number of recognitions to return.
        /// </summary>
        protected const int numRecognitions = 5;

        /// <summary>
        /// List of the templates to compare to
        /// </summary>
        protected List<BitmapSymbol> _templates;

        /// <summary>
        ///  Turn on if you want to print debug info
        /// </summary>
        private bool debug = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new, empty AdaptiveImageRecognizer
        /// </summary>
        public ImageRecognizer()
        {
            _templates = new List<BitmapSymbol>();
        }

        /// <summary>
        /// Creates a new AdaptiveImageRecognizer with the given templates
        /// </summary>
        /// <param name="templates"></param>
        public ImageRecognizer(List<BitmapSymbol> templates)
        {
            _templates = templates;
        }

        /// <summary>
        /// Creates a new ImageRecognizer trained on a set of labeled data.
        /// </summary>
        /// <param name="data">a list of labeled shapes</param>
        public ImageRecognizer(List<Shape> data)
            : this()
        {
            foreach (Shape shape in data)
                Add(shape.Type, shape.SubstrokesL, shape.createBitmap(100, 100, true));
        }

        #endregion

        #region Interface Functions

        /// <summary>
        /// Adds a new example to the list of templates.
        /// </summary>
        /// <param name="label">Class name/label for the shape</param>
        /// <param name="strokes">List of strokes in the shape</param>
        public virtual BitmapSymbol Add(ShapeType label, List<Substroke> strokes, System.Drawing.Bitmap bitmap)
        {
            if (label.Classification != LogicDomain.GATE_CLASS)
                return new BitmapSymbol();

            BitmapSymbol bs = new BitmapSymbol(strokes, label, bitmap);

            // give the BitmapSymbol a unique name
            int templateCount = 0;
            List<string> alreadySeen = new List<string>();
            foreach (BitmapSymbol template in _templates)
                alreadySeen.Add(template.Name);
            while (alreadySeen.Contains(bs.SymbolType + "_" + templateCount))
                ++templateCount;
            bs.Name = bs.SymbolType + "_" + templateCount;

            if (debug) Console.WriteLine("Adding template " + bs.Name);

            _templates.Add(bs);
            return bs;
        }

        public override bool canRecognize(string classification)
        {
            return classification == LogicDomain.GATE_CLASS;
        }

        /// <summary>
        /// Finds a template based on the string name
        /// </summary>
        /// <param name="templateName">the name of the template</param>
        /// <returns>the BitmapSymbol template</returns>
        protected BitmapSymbol findTemplate(string templateName)
        {
            foreach (BitmapSymbol bs in _templates)
                if (templateName == bs.Name)
                    return bs;
            return null;
        }

        public virtual void orient(Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            ShapeType type = shape.Type;
            BitmapSymbol defn = new BitmapSymbol();
            foreach (BitmapSymbol template in _templates)
                if (template.SymbolType == type)
                {
                    defn = template;
                    break;
                }

            BitmapSymbol unknown = new BitmapSymbol(shape.SubstrokesL);
            shape.Orientation = unknown.bestOrientation(defn);
        }

        #endregion

        #region Recognition

        /// <summary>
        /// Uses the RecognitionInterfaces.Recognizer recognize method 
        /// which recognizes and assigns the type of a shape.  This 
        /// implementation allows the use of the learnFromExample
        /// method in Interface Functions.
        /// </summary>
        /// <param name="shape">The shape to recognize</param>
        /// <param name="featureSketch">The featureSketch to use</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            BitmapSymbol unknown = new BitmapSymbol(shape.SubstrokesL);
            List<SymbolRank> results = unknown.Recognize(_templates);
                
            if (results.Count > 0)
            {
                // Populate the dictionary of alterateTypes with all of the ShapeTypes in results
                Dictionary<ShapeType, double> alternateTypes = new Dictionary<ShapeType, double>();

                if (debug)
                    Console.WriteLine("\nRecognition results: ");

                foreach (SymbolRank result in results)
                {
                    if (!alternateTypes.ContainsKey(result.SymbolType))
                        alternateTypes.Add(result.SymbolType, getProbability(result.SymbolType, results));

                    if (debug)
                        Console.WriteLine(result.SymbolType + " with template " + result.SymbolName);
                }

                ShapeType type = results[0].SymbolType; // the most likely type

                float probability = (float)alternateTypes[type]; // grab the probability of our most likely type

                alternateTypes.Remove(type); // the most likely type is NOT an alternate

                shape.setRecognitionResults(
                    type, 
                    probability, 
                    alternateTypes, 
                    results[0].BestOrientation, 
                    results[0].SymbolName);
            }
        }

            #region ComboRecognizer Compatability

        /// <summary>
        /// Recognizes the given strokes. Used by ComboRecognizer.
        /// </summary>
        /// <param name="strokes">The list of strokes to recognize</param>
        /// <returns>A ranked list of possible ShapeType matches</returns>
        public List<ShapeType> Recognize(List<Substroke> strokes)
        {
            BitmapSymbol unknown = new BitmapSymbol(strokes);
            List<SymbolRank> results = unknown.Recognize(_templates);
            List<ShapeType> output = new List<ShapeType>();
            foreach (SymbolRank sr in results)
                output.Add(sr.SymbolType);
            return output;
        }

            #endregion

        /// <summary>
        /// Gets the probability that the specified classification is correct.
        /// </summary>
        /// <param name="type">The ShapeType you want to check the probability of</param>
        /// <param name="helpfulTemplates">The list of templates we are checking against</param>
        /// <returns>A double probability that is the number of templates of this type divided by
        ///     the number of templates used in recognition</returns>
        public double getProbability(ShapeType type, List<SymbolRank> templates)
        {
            int count = 0;
            foreach (SymbolRank result in templates)
                if (result.SymbolType == type)
                    ++count;
            double probability = (1.0 * count / numRecognitions);

           if (debug) Console.WriteLine("Probability of " + type + " is " + probability);

           return probability;
        }

        #endregion

        #region Serialization, Saving, and Loading

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public ImageRecognizer(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _templates = (List<BitmapSymbol>)info.GetValue("templates", typeof(List<BitmapSymbol>));
        }

        /// <summary>
        /// Serialization Function
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("templates", _templates);
        }

        /// <summary>
        /// Serializes the object and saves it to the specified filename
        /// </summary>
        /// <param name="filename">Filename to save the object as</param>
        public void Save(string filename)
        {
            System.IO.Stream stream = System.IO.File.Open(filename, System.IO.FileMode.Create);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            bformatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Loads a previously saved ImageRecognizer from the given filename, 
        /// using the deserialization constructor
        /// </summary>
        /// <param name="filename">Filename which is the saved ImageRecognizer</param>
        /// <returns>Re-instantiated ImageRecognzier</returns>
        public static ImageRecognizer Load(string filename)
        {
            System.IO.Stream stream = System.IO.File.Open(filename, System.IO.FileMode.Open);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            ImageRecognizer image = (ImageRecognizer)bformatter.Deserialize(stream);
            stream.Close();

             #if DEBUG
            Console.WriteLine("Image recognizer loaded.");
             #endif

            return image;
        }

        #endregion
    }
}