using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecognitionInterfaces
{

    /// <summary>
    /// The Classifier is responsible for assigning classes to substrokes.
    /// </summary>
    public abstract class Classifier : IRecognitionStep
    {

        /// <summary>
        /// Classifies all the substrokes in a sketch.
        /// 
        /// Postcondition: substroke.Classification is "Wire," "Gate," "Label," or "Unknown" for
        /// every substroke in the sketch. Strokes are not reclassified if they have a parent
        /// shape with AlreadyLabeled set to true.
        /// </summary>
        /// <param name="sketch">the sketch to use</param>
        public virtual void process(Featurefy.FeatureSketch sketch)
        {
            foreach (Sketch.Substroke substroke in sketch.Sketch.Substrokes)
            {
                Sketch.Shape parent = substroke.ParentShape;
                if (parent != null && parent.AlreadyLabeled)
                    continue;

                classify(substroke, sketch);
            }
        }

        /// <summary>
        /// Classify a substroke (e.g., as "Wire," "Gate," or "Label")
        /// 
        /// Postcondition: substroke.Classification is "Wire," "Gate," "Label," or "Unknown."
        /// </summary>
        /// <param name="featureSketch">the FeatureSketch containing information about this substroke</param>
        /// <param name="substroke">the substroke to classify</param>
        public abstract void classify(Sketch.Substroke substroke, Featurefy.FeatureSketch featureSketch);

    }

}
