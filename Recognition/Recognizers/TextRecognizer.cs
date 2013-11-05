using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Ink;

namespace Recognizers
{

    /// <summary>
    /// Recognizes text only.
    /// </summary>
    public class TextRecognizer : RecognitionInterfaces.Recognizer
    {

        #region Internals

        /// <summary>
        /// The text recognizer from Microsoft.Ink that we harness
        /// </summary>
        RecognizerContext _microsoftTextRecognizer;
        private bool debug = false;
        
        #endregion

        #region Constructor, Destructor, and Initializers

        public TextRecognizer()
        {
            _microsoftTextRecognizer = new RecognizerContext();    
            
            // Specify what words should be recognizable, to enhance accuracy
            _microsoftTextRecognizer.WordList = createWordList();

            // Indicate that we want to only use words from this wordlist.
            _microsoftTextRecognizer.Factoid = Factoid.WordList;
            _microsoftTextRecognizer.RecognitionFlags = RecognitionModes.WordMode | RecognitionModes.Coerce;
        }

        ~ TextRecognizer()
        {
            /*
             * From http://msdn.microsoft.com/en-us/library/ms828542.aspx :
             * "To avoid a memory leak you must explicitly call the Dispose 
             * method on any RecognizerContext collection to which an event 
             * handler has been attached before the collection goes out of 
             * scope."
             * 
             * We don't attach an event handler, but a little care never 
             * hurt anyone.
             */
            _microsoftTextRecognizer.Dispose();
        }

        private static WordList createWordList()
        {
            // Create an array of words for the default WordList.
            // Note that when words are added to a WordList, it
            // capitalized versions are also implicitly added.
            string[] words = { 
                "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", 
                "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", 
                "w", "x", "y", "z",

                "a'", "b'", "c'", "d'", "e'", "f'", "g'", "h'", "i'",
                "j'", "k'", "l'", "m'", "n'", "o'", "p'", "q'", "r'",
                "s'", "t'","u'", "v'", "w'", "x'", "y'", "z'",

                "out", "in", "cout", "cin", "reset", "clock", "en", 
                "Sa", "Sb", "Sc", "Sd", "Se", "Sf", "Sg", "Sout", "Sin", 
                "S1", "S0", "S1'", "S0'", "AB", "A+B", "SW", 
                 };

            // Create the WordList
            WordList wordList = new WordList();
            foreach (string word in words)
                wordList.Add(word);

            return wordList;
        }

        #endregion

        #region Recognition

        /// <summary>
        /// Recognize a shape as text. The same as calling
        /// TextRecognizer.recognize(shape, null).
        /// </summary>
        /// <param name="shape">the shape to recognize</param>
        public void recognize(Sketch.Shape shape)
        {
            recognize(shape, null);
        }

        /// <summary>
        /// Recognizes the text a shape forms, and updates the
        /// shape with the recognition results (the text and the
        /// likelihood that the recognition was correct).
        /// 
        /// This method does not use the featureSketch argument.
        /// 
        /// Postcondition: the shape is of type IO and its Name
        /// property tells what text it was recognized as.
        /// </summary>
        /// <param name="shape">The shape to recognize</param>
        /// <param name="featureSketch">Not used.</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            // Prepare the shape for recognition
            SketchOrInkConverter converter = new SketchOrInkConverter();
            _microsoftTextRecognizer.Strokes = converter.convertToInk(shape);

            // Try to recognize the shape
            RecognitionStatus status;
            RecognitionResult result;
            result = _microsoftTextRecognizer.Recognize(out status);

            // Origanize the results
            string shapeName = "";
            float probability = 0F;
            if ((result != null) && (status == RecognitionStatus.NoError))
            {
                shapeName = result.TopString;
                switch (result.TopConfidence)
                {
                    case RecognitionConfidence.Poor:
                        probability = .1F;
                        break;
                    case RecognitionConfidence.Intermediate:
                        probability = .5F;
                        break;
                    case RecognitionConfidence.Strong:
                        probability = .9F;
                        break;
                }
            }

            // Update the shape to reflect these results
            if(debug) Console.WriteLine("Found input/output label: " + shapeName + " (confidence = " + probability + ")");
            shape.setRecognitionResults(LogicDomain.TEXT, probability, shapeName);
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// This recognizer only recognizes text.
        /// </summary>
        /// <param name="classification">a shape classification</param>
        /// <returns>true if classification is "Label"</returns>
        public override bool canRecognize(string classification)
        {
            return classification == LogicDomain.TEXT_CLASS;
        }

        #endregion

    }

}
