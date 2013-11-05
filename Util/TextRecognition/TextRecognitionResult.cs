using System;
using System.Collections.Generic;
using System.Text;

namespace TextRecognition
{
    public class TextRecognitionResult
    {
        private String match;

        private double confidence;

        /// <summary>
        /// Creates a TextRecognitionResult
        /// </summary>
        /// <param name="m">String match</param>
        /// <param name="c">Double Confidence</param>
        public TextRecognitionResult(string m, double c)
        {
            match = m;
            confidence = c;
        }

        public String Match
        {
            get { return this.match; }
        }
        public double Confidence
        {
            get { return this.confidence; }
        }
    }
}
