using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecognitionInterfaces;

namespace Refiner
{

    /// <summary>
    /// This recognition step ensures that every shape
    /// has a unique name.
    /// 
    /// Sample usage:
    /// new Refiner.UniqueNamer().process(_featuresketch);
    /// </summary>
    public class UniqueNamer : IRecognitionStep
    {
        private bool DEBUG = false;

        /// <summary>
        /// Makes sure that every shape in the sketch has a unique name.
        /// </summary>
        public virtual void process(Featurefy.FeatureSketch featureSketch)
        {
            process(featureSketch.Sketch);
        }


        /// <summary>
        /// Makes sure that every shape in the sketch has a unique name.
        /// </summary>
        public virtual void process(Sketch.Sketch sketch)
        {

            Recognizers.TextRecognizer textRecognizer = new Recognizers.TextRecognizer();
            HashSet<string> alreadySeen = new HashSet<string>();
            int count = 0;

            foreach (Sketch.Shape shape in sketch.Shapes)
            {
                if (shape.Type == LogicDomain.TEXT && shape.Name == null)
                    textRecognizer.recognize(shape);

                if (alreadySeen.Contains(shape.Name) || shape.Name == null)
                {
                    while (alreadySeen.Contains(shape.Type + "_" + count))
                        count++;
                    shape.Name = (shape.Type + "_" + count);
                }

                alreadySeen.Add(shape.Name);
            }

            if (DEBUG)
            {
                // For debugging purposes:
                Console.WriteLine("Sketch contains");
                foreach (Sketch.Shape shape in sketch.Shapes)
                    Console.WriteLine(" - " + shape.Name + ", a " + shape.Type);
            }
        }

    }

}
