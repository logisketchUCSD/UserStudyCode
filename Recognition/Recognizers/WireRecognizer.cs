using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Recognizers
{
    class WireRecognizer : RecognitionInterfaces.Recognizer
    {
        /// <summary>
        /// Recognizes a shape as a wire and updates the shape
        /// accordingly.
        /// </summary>
        /// <param name="shape">The shape to recognize</param>
        /// <param name="featureSketch">The sketch describing features of the shape</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            // Let's assume we're 100% sure it's a wire, for now...
            shape.setRecognitionResults(LogicDomain.WIRE, 1F);
        }

        /// <summary>
        /// This recognizer only recognizes wires.
        /// </summary>
        /// <param name="classification">a shape classification</param>
        /// <returns>true if classification is "Wire"</returns>
        public override bool canRecognize(string classification)
        {
            return classification == LogicDomain.WIRE_CLASS;
        }
    }
}
