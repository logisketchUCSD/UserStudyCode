using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RecognitionInterfaces;

namespace Recognizers
{

    /// <summary>
    /// A recognizer that represents a multithreaded version of another
    /// recognizer. Only the inner recognizer's "process" function will
    /// be multithreaded. The inner recognizer's "recognize" function
    /// must be thread-safe.
    /// </summary>
    public class ThreadedRecognizer : Recognizer
    {

        /// <summary>
        /// Instances of the worker class actually perform the work for each
        /// thread.
        /// </summary>
        private class Worker
        {
            private int _start;
            private int _end;
            private Sketch.Shape[] _shapes;
            private Featurefy.FeatureSketch _featureSketch;
            private Recognizer _innerRecognizer;

            /// <summary>
            /// Create a new worker.
            /// </summary>
            /// <param name="inner">the recognizer to use</param>
            /// <param name="start">the first shape to recognize (inclusive)</param>
            /// <param name="end">the last shape to recognize (exclusive)</param>
            /// <param name="featureSketch">the sketch whose shapes we'll work on</param>
            public Worker(Recognizer inner, int start, int end, Featurefy.FeatureSketch featureSketch)
            {
                _innerRecognizer = inner;
                _start = start;
                _end = end;
                _featureSketch = featureSketch;
                _shapes = _featureSketch.Sketch.Shapes;
            }

            /// <summary>
            /// Run the worker on the range given in the constructor.
            /// </summary>
            public void run()
            {
                for (int i = _start; i < _end; i++)
                {
                    Sketch.Shape shape = _shapes[i];
                    if (shape.AlreadyLabeled)
                        continue;
                    _innerRecognizer.recognize(shape, _featureSketch);
                }
            }
        }

        private Recognizer _innerRecognizer;
        private int _numThreads;

        /// <summary>
        /// Construct a new threaded recognizer with the given recognizer
        /// and number of threads. 
        /// 
        /// NOTE: The recognizer's "recognize" method MUST be thread-safe!
        /// </summary>
        /// <param name="worker">the recognizer that will actually perform recognition</param>
        /// <param name="numThreads">the number of threads to use during processing</param>
        public ThreadedRecognizer(Recognizer worker, int numThreads)
        {
            _innerRecognizer = worker;
            _numThreads = numThreads;
        }


        /// <summary>
        /// Run the recognizer on a sketch.
        /// 
        /// This method is multithreaded.
        /// 
        /// Precondition: the sketch has been grouped into shapes.
        /// 
        /// Postcondition: shape.Classification has been filled in with this recognizer's best guess 
        /// for every shape in the sketch.
        /// </summary>
        /// <param name="featureSketch">the sketch to work on</param>
        public override void process(Featurefy.FeatureSketch featureSketch)
        {
            List<Thread> threads = new List<Thread>(_numThreads);
            int shapesPerWorker = featureSketch.NumShapes / _numThreads;
            int finalEnd = 0;

            // fork off (_numThreads - 1) threads, each with shapesPerWorker tasks to perform
            for (int i = 0; i < _numThreads - 1; i++)
            {
                int start = i * shapesPerWorker;
                int end = start + shapesPerWorker;
                finalEnd = end;
                Worker worker = new Worker(_innerRecognizer, start, end, featureSketch);
                Thread t = new Thread(worker.run);
                t.Name = "recognition worker thread " + i;
                threads.Add(t);
                t.Start();
            }

            // do the rest of the work on this thread
            Worker myWorker = new Worker(_innerRecognizer, finalEnd, featureSketch.NumShapes, featureSketch);
            myWorker.run();

            // join the other threads to make sure they finished
            foreach (Thread t in threads)
            {
                t.Join();
            }
        }


        /// <summary>
        /// Recognizes and assigns the type of a shape.
        /// 
        /// Not multithreaded.
        /// 
        /// Precondition: shape is one of the shapes in featureSketch
        /// 
        /// Postcondition: shape.Classification has been filled in with this recognizer's best guess
        /// </summary>
        /// <param name="shape">The Shape to recognize</param>
        /// <param name="featureSketch">The FeatureSketch to use</param>
        public override void recognize(Sketch.Shape shape, Featurefy.FeatureSketch featureSketch)
        {
            _innerRecognizer.recognize(shape, featureSketch);
        }

        /// <summary>
        /// Teaches the worker recognizer.
        /// </summary>
        /// <param name="shape">the shape to learn</param>
        public override void learnFromExample(Sketch.Shape shape) 
        {
            _innerRecognizer.learnFromExample(shape);
        }

        /// <summary>
        /// Saves the worker recognizer.
        /// </summary>
        public override void save()
        {
            _innerRecognizer.save();
        }

        /// <summary>
        /// Retreive or change the inner recognizer.
        /// </summary>
        public Recognizer InnerRecognizer
        {
            get { return _innerRecognizer; }
            set { _innerRecognizer = value; }
        }

        /// <summary>
        /// Calls the inner recognizer's canRecognize function.
        /// </summary>
        /// <param name="classification">the classification to check</param>
        /// <returns>what the inner recognizer returns</returns>
        public override bool canRecognize(string classification)
        {
            return _innerRecognizer.canRecognize(classification);
        }

    }

}
