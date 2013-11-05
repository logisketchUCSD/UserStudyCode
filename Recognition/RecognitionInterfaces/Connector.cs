using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Featurefy;
using Domain;

namespace RecognitionInterfaces
{
    public class Connector : IRecognitionStep
    {

        /// <summary>
        /// The domain this connector will use
        /// </summary>
        private ContextDomain.ContextDomain _domain;

        /// <summary>
        /// Create a new connector in the specified domain
        /// </summary>
        /// <param name="domain">the domain to use</param>
        public Connector(ContextDomain.ContextDomain domain)
        {
            _domain = domain;
        }

        /// <summary>
        /// Connects all shapes in a sketch as well as possible.
        /// 
        /// Precondition: the shapes of the sketch have identified types.
        /// 
        /// Postconditions: 
        ///    - the shapes are connected and oriented according to the context domain.
        ///    - no wires are connected to each other; connected wires are a single shape
        /// </summary>
        /// <param name="featureSketch">the sketch to connect</param>
        public virtual void process(Featurefy.FeatureSketch featureSketch)
        {
            recomputeConnectedShapes(featureSketch.Sketch.Shapes, featureSketch.Sketch);

            //foreach (Sketch.Shape shape in featureSketch.Sketch.ShapesL)
                //_domain.OrientShape(shape, featureSketch.Sketch);
        }

        /// <summary>
        /// Recalculate the connectedShapes of every shape in a given list,
        /// and correctly updates all related shapes.
        /// </summary>
        /// <param name="shapeList">the list of shapes to reconnect</param>
        /// <param name="featureSketch">the sketch the shapes belong to</param>
        public void recomputeConnectedShapes(IEnumerable<Sketch.Shape> shapeList, Sketch.Sketch sketch)
        {
            // clear ALL connections first
            foreach (Sketch.Shape shape in sketch.Shapes)
                shape.ClearConnections();

            sketch.CheckConsistency();

            // connect every shape
            foreach (Sketch.Shape shape in shapeList)
                connect(shape, sketch);

            // Make sure connected wires are the same shape
            Data.Pair<Sketch.Shape, Sketch.Shape> pair = null;
            while ((pair = wiresToMerge(sketch)) != null)
            {
                sketch.mergeShapes(pair.A, pair.B);
                sketch.connectShapes(pair.A, pair.A);
            }

            sketch.CheckConsistency();
        }

        /// <summary>
        /// If there are two wires connected to each other in the sketch, this function
        /// returns them. Otherwise it returns null.
        /// </summary>
        /// <param name="sketch">the sketch to check</param>
        /// <returns></returns>
        private Data.Pair<Sketch.Shape, Sketch.Shape> wiresToMerge(Sketch.Sketch sketch)
        {
            foreach (Sketch.Shape shape in sketch.Shapes)
            {
                if (shape.Type != LogicDomain.WIRE)
                    continue;

                foreach (Sketch.Shape connectedShape in shape.ConnectedShapes)
                {
                    if (connectedShape.Type == LogicDomain.WIRE && shape != connectedShape)
                        return new Data.Pair<Sketch.Shape, Sketch.Shape>(shape, connectedShape);
                }
            }
            return null;
        }

        /// <summary>
        /// Find and make all necessary connections from one shape in a sketch.
        /// 
        /// Precondition: 
        ///    - no shapes in the sketch have connections to shapes that are missing
        /// </summary>
        /// <param name="shape">the shape whose conenctions will be updated</param>
        /// <param name="featureSketch">the sketch that the shape belongs to</param>
        public virtual void connect(Sketch.Shape shape, Sketch.Sketch sketch)
        {
            _domain.ConnectShape(shape, sketch);
            sketch.CheckConsistency();
        }

        /// <summary>
        /// Get the context domain for this connector. Does not support set.
        /// </summary>
        public ContextDomain.ContextDomain Domain
        {
            get { return _domain; }
        }

    }
}
