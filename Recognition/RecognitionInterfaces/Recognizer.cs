using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecognitionInterfaces
{

    /// <summary>
    /// The Recognizer is responsible for identifying shapes after the grouper has run.
    /// Some recognizers can learn from user interaction as well, as the user identifies
    /// errors.
    /// </summary>
    public abstract class Recognizer : IRecognitionStep
    {

        /// <summary>
        /// Run the recognizer on a sketch.
        /// 
        /// Precondition: the sketch has been grouped into shapes.
        /// 
        /// Postcondition: shape.Type and shape.Probability have been filled in with 
        /// this recognizer's best guess for every shape in the sketch.
        /// </summary>
        /// <param name="featureSketch">the sketch to work on</param>
        public virtual void process(Featurefy.FeatureSketch featureSketch)
        {
            foreach (Sketch.Shape shape in featureSketch.Sketch.Shapes)
            {
                if (shape.AlreadyLabeled || !canRecognize(shape.Classification))
                    continue;
                recognize(shape, featureSketch);
            }
        }

        /// <summary>
        /// Recognizes and assigns the type of a shape.
        /// 
        /// Note: although not all recognizers use a featuresketch
        /// to recognize a given shape, we pass it in anyway to leave
        /// the possibility open for other recognizers to do so.
        /// 
        /// Precondition: shape is one of the shapes in featureSketch
        /// 
        /// Postcondition: shape.Type has been filled in with this recognizer's best guess, and
        /// shape.Probability has been filled in with an estimation of the recognizer's confidence.
        /// </summary>
        /// <param name="shape">The Shape to recognize</param>
        /// <param name="featureSketch">The FeatureSketch to use</param>
        public abstract void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch);

        /// <summary>
        /// Update the recognizer to learn from an example shape.
        /// 
        /// Precondition: shape has a valid type
        /// </summary>
        /// <param name="shape">the shape to learn</param>
        public virtual void learnFromExample(Sketch.Shape shape) {  }

        /// <summary>
        /// If the recognizer learns from user input, it can override this method
        /// to save itself when the program quits so the training is remembered from
        /// one session to the next. Otherwise, this function does nothing.
        /// </summary>
        public virtual void save() { }

        /// <summary>
        /// Determine whether this recognizer can recognize the given class of shapes.
        /// 
        /// In the default implementation this returns true for logic gates, wires, and text.
        /// </summary>
        /// <param name="classification">the shape class to check</param>
        /// <returns>true if this recognizer can recognize this class of shapes</returns>
        public virtual bool canRecognize(string classification) 
        {
            return
                classification == Domain.LogicDomain.GATE_CLASS ||
                classification == Domain.LogicDomain.WIRE_CLASS ||
                classification == Domain.LogicDomain.TEXT_CLASS;
        }

    }

}
