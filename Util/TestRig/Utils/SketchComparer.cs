using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sketch;
using Domain;

namespace TestRig.Utils
{

    /// <summary>
    /// Can be used to compare two sketches.
    /// </summary>
    class SketchComparer
    {

        #region Internals

        private Sketch.Sketch _original;
        private Sketch.Sketch _toCompare;
        private Lazy<double> _classificationQuality;
        private Lazy<double> _groupingQuality;
        private Lazy<double> _recognitionQuality;
        private Lazy<ConfusionMatrix<string>> _classificationConfusionMatrix;
        private Lazy<ConfusionMatrix<ShapeType>> _recognitionConfusionMatrix;

        #endregion

        #region Constructor

        public SketchComparer(Sketch.Sketch original, Sketch.Sketch toCompare)
        {
            _original = original;
            _toCompare = toCompare;
            _recognitionConfusionMatrix = new Lazy<ConfusionMatrix<ShapeType>>(computeRecognitionConfusionMatrix);
            _classificationConfusionMatrix = new Lazy<ConfusionMatrix<string>>(computeClassificationConfusionMatrix);
            _classificationQuality = new Lazy<double>(computeClassificationQuality);
            _groupingQuality = new Lazy<double>(computeGroupingQuality);
            _recognitionQuality = new Lazy<double>(computeRecognitionQuality);
        }

        #endregion

        #region Computation

        private double computeClassificationQuality()
        {
            int totalStrokes = 0;
            int totalCorrect = 0;

            foreach (Substroke correctStroke in _original.Substrokes)
            {
                
                string originalClass = correctStroke.Classification;
                totalStrokes++;

                Substroke resultStroke = _toCompare.SubstrokesL.Find(delegate(Substroke s) { return s.Equals(correctStroke); });
                string resultClass = resultStroke.Classification;

                if (resultClass == originalClass)
                    totalCorrect++;
            }

            return (double)totalCorrect / totalStrokes;
        }

        private double computeGroupingQuality()
        {
            int totalGroups = 0;
            int correctGroups = 0;

            foreach (Sketch.Shape correctShape in _original.Shapes)
            {

                ShapeType originalType = correctShape.Type;
                totalGroups++;

                if (_toCompare.ShapesL.Exists(delegate(Sketch.Shape s) { return s.Equals(correctShape); }))
                {
                    correctGroups++;
                }
            }

            return (double)correctGroups / totalGroups;
        }

        private double computeRecognitionQuality()
        {
            int totalShapes = 0;
            int correctShapes = 0;

            foreach (Sketch.Shape correctShape in _original.Shapes)
            {

                ShapeType originalType = correctShape.Type;

                if (_toCompare.ShapesL.Exists(delegate(Sketch.Shape s) { return s.Equals(correctShape); }))
                {
                    Sketch.Shape resultShape = _toCompare.ShapesL.Find(delegate(Sketch.Shape s) { return s.Equals(correctShape); });
                    ShapeType resultType = resultShape.Type;

                    totalShapes++;

                    if (resultType == originalType)
                        correctShapes++;

                }
            }

            return (double)correctShapes / totalShapes;
        }

        private ConfusionMatrix<ShapeType> computeRecognitionConfusionMatrix()
        {
            ConfusionMatrix<ShapeType> result = new ConfusionMatrix<ShapeType>();

            foreach (Sketch.Shape correctShape in _original.Shapes)
            {

                ShapeType originalType = correctShape.Type;

                if (_toCompare.ShapesL.Exists(delegate(Sketch.Shape s) { return s.Equals(correctShape); }))
                {
                    Sketch.Shape resultShape = _toCompare.ShapesL.Find(delegate(Sketch.Shape s) { return s.Equals(correctShape); });
                    ShapeType resultType = resultShape.Type;

                    // Record stats
                    result.increment(originalType, resultType);

                }
            }

            return result;
        }

        private ConfusionMatrix<string> computeClassificationConfusionMatrix()
        {
            ConfusionMatrix<string> result = new ConfusionMatrix<string>();

            foreach (Substroke correctStroke in _original.Substrokes)
            {

                string originalClass = correctStroke.Classification;

                Substroke resultStroke = _toCompare.SubstrokesL.Find(delegate(Substroke s) { return s.Equals(correctStroke); });
                string resultClass = resultStroke.Classification;

                // Record stats
                result.increment(originalClass, resultClass);
            }

            return result;
        }

        #endregion

        #region Accessors

        public ConfusionMatrix<string> ClassificationConfusion
        {
            get 
            { 
                return _classificationConfusionMatrix.Value; 
            }
        }

        public double ClassificationQuality
        {
            get
            {
                return _classificationQuality.Value;
            }
        }

        public double GroupingQuality
        {
            get
            {
                return _groupingQuality.Value;
            }
        }

        public ConfusionMatrix<ShapeType> RecognitionConfusion
        {
            get
            {
                return _recognitionConfusionMatrix.Value;
            }
        }

        public double RecognitionQuality
        {
            get
            {
                return _recognitionQuality.Value;
            }
        }

        #endregion

    }
}
