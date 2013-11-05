using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Ink;
using System.Drawing;
using ConverterXML;

namespace TextRecognition
{
    /// <summary>
    /// author: Sara
    /// uses Microsoft Ink Recognition to recognize text
    /// </summary>
    public class TextRecognition
    {

        # region INTERNALS

        /// <summary>
        /// Factiod for recognizing truth tables.
        /// </summary>
        private static string dataFactoid = "(1|0|X)";

        /// <summary>
        /// Factoid for recognizing text that are letters or numbers and starts with a letter (for labels)
        /// </summary>
        private static string labelFactoid = "((IS_UPPERCHAR|IS_LOWERCHAR)?(IS_ALPHANUMERIC_FULLWIDTH))";

        #endregion

        #region GETTERS
        /// <summary>
        /// The truth table factoid.
        /// </summary>
        public static string data
        {
            get
            {
                return dataFactoid;
            }
        }

        /// <summary>
        /// The label factoid.
        /// </summary>
        public static string label
        {
            get
            {
                return labelFactoid;
            }
        }

        #endregion

        #region WORDLIST METHODS

        /// <summary>
        /// Loads a list of strings from a file that will be used for the text recognition WordList.
        /// </summary>
        /// <param name="filepath">The file path where the list of words is loaded from.</param>
        /// <returns>The loaded list of strings that will be added to the WordList</returns>
        public static List<string> loadLabelStringList(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            List<string> loadedWords = new List<string>();
            
            // Parses the text file
            string line = sr.ReadLine();
            while (line != null && line != "")
            {
                loadedWords.Add(line);

                line = sr.ReadLine();
            }
            sr.Close();

            return loadedWords;
        }

        /// <summary>
        /// Loads a list of strings into a WordList.
        /// </summary>
        /// <param name="wordsToLoad">List of strings that will be added to the WordList.</param>
        /// <returns>The WordList that will be used in the text recognition.</returns>
        public static WordList loadLabelWordList(List<string> wordsToLoad)
        {
            WordList wordList = new WordList();

            // Loads the string array into a WordList
            foreach (string word in wordsToLoad)
            {
                wordList.Add(word);
            }

            return wordList;
        }

        /// <summary>
        /// Saves a WordList in the forma of a List of strings to a file.  To use this, need to add new words from the user
        /// to the WordList and to the List of strings that was loaded and then save the List of strings using this method.
        /// </summary>
        /// <param name="filepath">The file path where the WordList is saved to.</param>
        /// <param name="wordsToSave">List of strings that will be written to a file that can later be loaded to
        /// create a new user-specific (broader) WordList.</param>
        public static void saveLabelWordList(string filepath, List<string> wordsToSave)
        {
            StreamWriter sw = new StreamWriter(filepath, false);
            foreach(string word in wordsToSave)
            {
                sw.WriteLine(word);
            }

            sw.Close();
        }

        /// <summary>
        /// creates the label word list
        /// </summary>
        /// <returns>the label word list</returns>
        public static WordList createLabelWordList()
        {
            WordList wordList = new WordList();

            // Array of words for the default WordList.
            string[] words = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K",
                "L", "M", "N", "0", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y",
                "Z", "Cout", "Cin", "A'", "B'", "C'", "D'", "E'", "F'", "G'", "H'", 
                "I'", "J'", "K'", "L'", "M'", "N'", "0'", "P'", "Q'", "R'", "S'", "T'",
                "U'", "V'", "W'", "X'", "Y'", "Z'", "Reset", "Clock", "En", "Clk", "CLK",
                "Sa", "Sb", "Sc", "Sd", "Se", "Sf", "Sg", "Sout", "Sin", "RESET", "EN", 
                "S1", "S0", "S1'", "S0'", "Happy", "AB", "A+B", "SW", 
                "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", 
                "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

            // Create the WordList
            foreach (string word in words)
            {
                wordList.Add(word);
            }

            return wordList;
        }

        # endregion

        # region STROKES TO INK

        /// <summary>
        /// converts a Sketch substroke into a microsoft.ink stroke
        /// </summary>
        /// <param name="substroke">xml substroke</param>
        /// <param name="ink">microsoft ink object</param>
        /// <returns>microsoft ink stroke</returns>
        public static Microsoft.Ink.Stroke strokeToInk(Sketch.Substroke substroke, Microsoft.Ink.Ink ink)
        {
            System.Drawing.Point[] pts = new System.Drawing.Point[substroke.Points.Length];

            // Convert our point format to Microsoft point format
            int len = pts.Length;
            for (int k = 0; k < len; k++)
            {
                pts[k] = new System.Drawing.Point((int)(substroke.Points[k].X),
                    (int)(substroke.Points[k].Y));
            }

            return ink.CreateStroke(pts);
        }

        /// <summary>
        /// converts a Sketch shape into a Microsoft.Ink.Strokes object
        /// </summary>
        /// <param name="sh">xml shape</param>
        /// <returns>group of microsoft ink strokes</returns>
        public static Microsoft.Ink.Strokes shapeToInk(Sketch.Shape sh)
        {
            // Create a strokes collection
            Microsoft.Ink.Ink ink = new Microsoft.Ink.Ink();
            Microsoft.Ink.Strokes strokes = ink.CreateStrokes();

            Sketch.Substroke[] subs = sh.Substrokes;
            int len = subs.Length;

            // Add the substrokes of our format to the strokes collection.
            for (int i = 0; i < len; i++)
            {
                strokes.Add(strokeToInk(subs[i], ink));
            }
            return strokes;
        }

        # endregion

        # region RECOGNITION

        ///// <summary>
        ///// default recognizer
        ///// </summary>
        ///// <param name="sh">shape we want to analyze</param>
        ///// <returns>recognized as a string</returns>
        //public static string recognize(Sketch.Shape sh)
        //{
        //    Microsoft.Ink.Strokes st = shapeToInk(sh);
        //    return st.ToString();
        //}

        /// <summary>
        /// returns a string that is the Microsoft.Ink.Strokes
        /// recognition is biased by the specified factoid
        /// </summary>
        /// <param name="st">group of microsoft ink strokes</param>
        /// <param name="factoid">regular expression biasing recognition</param>
        /// <returns>string representation biased toward factoid</returns>
        public static string recognize(Sketch.Shape sh, string factoid)
        {
            Microsoft.Ink.Strokes st = shapeToInk(sh);

            // Set factoid
            RecognizerContext myRecoContext = new RecognizerContext();
            myRecoContext.Factoid = factoid;

            RecognitionStatus status;
            RecognitionResult recoResult;

            // Recognize the text
            myRecoContext.Strokes = st;
            recoResult = myRecoContext.Recognize(out status);
            return recoResult.TopString;
        }

        /// <summary>
        /// same as recognize, but can support the mode Coerce
        /// which makes recognition more strict
        /// </summary>
        /// <param name="sh">shape we want to analyze</param>
        /// <param name="factoid">factoid to bias recognizer</param>
        /// <param name="mode">can be coerce</param>
        /// <returns>recognized text</returns>
        public static string recognize(Sketch.Shape sh, string factoid, RecognitionModes mode)
        {
            Microsoft.Ink.Strokes st = shapeToInk(sh);

            // Set factoid
            RecognizerContext myRecoContext = new RecognizerContext();
            myRecoContext.Factoid = factoid;

            // Set recognition mode
            myRecoContext.RecognitionFlags = mode;

            RecognitionStatus status;
            RecognitionResult recoResult;

            // Recognize the text
            myRecoContext.Strokes = st;
            recoResult = myRecoContext.Recognize(out status);
            return recoResult.TopString;
        }


                /// <summary>
        /// same as recognize, but uses a word list instead
        /// </summary>
        /// <param name="sh">the shape we want to analzye</param>
        /// <returns>recognized text</returns>
        public static TextRecognitionResult recognize(Sketch.Shape sh)
        {
            return recognize(sh, createLabelWordList(), RecognitionModes.Coerce);
        }

        /// <summary>
        /// same as recognize, but uses a word list instead
        /// </summary>
        /// <param name="sh">the shape we want to analzye</param>
        /// <param name="wordList">the word list</param>
        /// <param name="mode">can be set to "Coerce"</param>
        /// <returns>recognized text</returns>
        public static TextRecognitionResult recognize(Sketch.Shape sh, WordList wordList, RecognitionModes mode)
        {

            Microsoft.Ink.Strokes st = shapeToInk(sh);

            // Use the WordList as a factoid
            RecognizerContext myRecoContext = new RecognizerContext();
            string words = Factoid.WordList;
            myRecoContext.Factoid = words;
            myRecoContext.WordList = wordList;

            // Set the recognition mode
            myRecoContext.RecognitionFlags = mode;

            RecognitionStatus status;
            RecognitionResult recoResult;

            // Recognize the text (if it is null return ? since the coerce mode can return a null result)
            myRecoContext.Strokes = st;
            recoResult = myRecoContext.Recognize(out status);
            if (recoResult == null)
                return new TextRecognitionResult("?", 0.0);
            else
            {
                double prob = 0.0;
                switch (recoResult.TopConfidence)
                {
                    case RecognitionConfidence.Poor: prob = .1; break;
                    case RecognitionConfidence.Intermediate: prob = .5; break;
                    case RecognitionConfidence.Strong: prob = .9; break;
                }
                return new TextRecognitionResult(recoResult.TopString, prob);
            }
        }



        /// <summary>
        /// returns a list of recognition alternates for a shape
        /// </summary>
        /// <param name="sh">the shape (text) we want to analyze</param>
        /// <param name="wordList">the word list we want to recognize from</param>
        /// <param name="mode">the recognition mode</param>
        /// <param name="numResults">the number of results we want</param>
        /// <returns>the string array of alternates, including the top alternate</returns>
        public static string[] recognizeAlternates(Sketch.Shape sh, WordList wordList, RecognitionModes mode, int numResults)
        {
            string[] alternates = new string[numResults];

            // Loops for the number of desired results and gets next best guess by
            // removing the word that was just recognized from the WordList
            for (int i = 0; i < numResults; i++)
            {
                string nextWord = recognize(sh, wordList, mode).Match;
                alternates[i] = nextWord;
                wordList.Remove(nextWord);
            }

            return alternates;
        }

        /// <summary>
        /// main for testing
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Sketch.Sketch sketch = (new ReadXML(args[i])).Sketch;
                Sketch.Shape shape = new Sketch.Shape(new Sketch.Shape[0], sketch.Substrokes, new Sketch.XmlStructs.XmlShapeAttrs(true));
                sketch.AddShape(shape);

                // Create default WordList
                WordList wordlist = createLabelWordList();

                // Get first guess plus four alternatate recognition results
                string[] alternates = recognizeAlternates(shape, wordlist, RecognitionModes.Coerce, 5);

                // Print results
                for (int j = 0; j < 5; j++)
                {
                    Console.WriteLine(alternates[j]);
                }
            }  
        }

        # endregion
    }
}
