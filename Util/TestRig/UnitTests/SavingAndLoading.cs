using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SavingAndLoading
    {

        [TestMethod]
        public void TestXMLConversion()
        {

            const string filename = "tmp.xml";
            Domain.ShapeType testType = Domain.LogicDomain.AND;

            Sketch.Sketch sketch1 = Sketches.newValidSketch();

            foreach (Sketch.Shape shape in sketch1.Shapes)
                shape.Type = testType;

            ConverterXML.SaveToXML writer = new ConverterXML.SaveToXML(sketch1);
            writer.WriteXML(filename);

            ConverterXML.ReadXML reader = new ConverterXML.ReadXML(filename);
            Sketch.Sketch sketch2 = reader.Sketch;

            sketch2.CheckConsistency();

            Assert.IsTrue(sketch1.Equals(sketch2), "Original sketch is not equal to the loaded sketch");
            Assert.IsTrue(sketch2.Equals(sketch1), "Loaded sketch is not equal to the original");

            foreach (Sketch.Shape shape in sketch2.Shapes)
                Assert.AreEqual(testType, shape.Type, "Shape types are not preserved across save/load");

        }

    }
}
