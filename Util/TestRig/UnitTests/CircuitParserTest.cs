using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Ink;
using System.Windows.Input;

namespace UnitTests
{
    /// <summary>
    /// Run the CircuitParser on Sketches that we construct and check that parsing succeeded/failed as expected.
    /// </summary>
    [TestClass]
    [DeploymentItem("CircuitDomain.txt")]
    public class CircuitParserTest
    {
        private Sketch.Sketch sketch;
        private CircuitParser.CircuitParser parser;

        [TestInitialize()]
        public void Initialize()
        {
            sketch = new Sketch.Sketch();
            parser = new CircuitParser.CircuitParser();
        }

        [TestCleanup]
        public void Cleanup()
        {
            sketch = null;
            parser = null;
        }

        [TestMethod]
        public void TestParseEmpty()
        {
            Assert.IsTrue(parser.SuccessfullyParse(sketch), "parser should be able to parse empty sketch");
        }

        [TestMethod]
        public void TestParseSingleWire()
        {
            List<Sketch.Shape> modifiedShapes;
            // Make a single substroke into a wire
            Sketch.Substroke substroke = AddNewStroke();
            Sketch.Shape wire = sketch.MakeNewShapeFromSubstroke(out modifiedShapes, substroke, Domain.LogicDomain.WIRE, 1);
            wire.Name = "wire_0";
           
            Assert.IsFalse(parser.SuccessfullyParse(sketch), "parser should not be able to parse sketch with single wire");
        }

        [TestMethod]
        public void TestParseTwoInputsAndWire()
        {
            List<Sketch.Shape> modifiedShapes;
            // Add a wire
            Sketch.Substroke wireSub = AddNewStroke(new Tuple<double, double>(0.2, 0.2), new Tuple<double, double>(0.8, 0.8));
            Sketch.Shape wire = sketch.MakeNewShapeFromSubstroke(out modifiedShapes, wireSub, Domain.LogicDomain.WIRE, 1);
            wire.Name = "wire_0";

            // Add the left input and connect it to the wire
            Sketch.Substroke firstSub = AddNewStroke(new Tuple<double, double>(0.0, 0.0), new Tuple<double, double>(0.1, 0.1));
            Sketch.Shape firstLabel = sketch.MakeNewShapeFromSubstroke(out modifiedShapes, firstSub, Domain.LogicDomain.TEXT, 1);
            firstLabel.Name = "A";
            wire.Endpoints[0].ConnectedShape = firstLabel;
            sketch.connectShapes(firstLabel, wire);

            // Add the right input and connect it to the wire
            Sketch.Substroke secondSub = AddNewStroke(new Tuple<double, double>(0.9, 0.9), new Tuple<double, double>(1.0, 1.0));
            Sketch.Shape secondLabel = sketch.MakeNewShapeFromSubstroke(out modifiedShapes, secondSub, Domain.LogicDomain.TEXT, 1);
            secondLabel.Name = "B";
            wire.Endpoints[1].ConnectedShape = secondLabel;
            sketch.connectShapes(secondLabel, wire);

            Assert.IsTrue(parser.SuccessfullyParse(sketch), "parser should be able to parse sketch with a wire connecting two labels");
        }

        /// <summary>
        /// Adds a new, straight stroke to the top-left corner of the sketch. Returns the stroke's substroke.
        /// </summary>
        /// <returns>substroke of the new stroke</returns>
        private Sketch.Substroke AddNewStroke()
        {
            Stroke inkStroke = InkCanvasSketchTest.newStroke();
            return AddSketchStrokeToSketch(inkStroke);
        }

        /// <summary>
        /// Adds a new, straight stroke to the top-left corner of the sketch. Returns the stroke's substroke.
        /// </summary>
        /// <param name="start">Start point of the stroke</param>
        /// <param name="end">End point of the stroke</param>
        /// <returns>substroke of the new stroke</returns>
        private Sketch.Substroke AddNewStroke(Tuple<double, double> start, Tuple<double, double> end)
        {
            Stroke inkStroke = InkCanvasSketchTest.newStroke(start, end);
            return AddSketchStrokeToSketch(inkStroke);
        }

        /// <summary>
        /// Adds the given inkStroke to the Sketch by constructing a Sketch stroke and adding that. Returns the stroke's substroke.
        /// </summary>
        /// <param name="inkStroke"></param>
        /// <returns></returns>
        private Sketch.Substroke AddSketchStrokeToSketch(Stroke inkStroke)
        {
            Sketch.Stroke sketchStroke = new Sketch.Stroke(inkStroke, new Guid(), (float)1.0);
            sketch.AddStroke(sketchStroke);
            return sketchStroke.FirstSubstroke;
        }

    }
}
