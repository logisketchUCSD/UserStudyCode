using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Data;

namespace UnitTests
{
    [TestClass]
    public class PairTest
    {
        [TestMethod]
        public void TestPairOrderIndependence()
        {

            // ================ HOMOGENOUS PAIRS =================

            Pair<string, string> p1 = new Pair<string, string>("hello", "goodbye");
            Pair<string, string> p2 = new Pair<string, string>("goodbye", "hello");
            Pair<string, string> p3 = new Pair<string, string>("goodbye", "goodbye");

            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode(), "Equivalent pairs have different hash codes");
            Assert.IsTrue(p1.Equals(p2), "Equivalent pairs are not equal");
            Assert.IsTrue(p2.Equals(p1), "Equivalent pairs are not equal");

            Assert.AreNotEqual(p1.GetHashCode(), p3.GetHashCode(), "Unequal pairs have the same hash code");
            Assert.IsFalse(p1.Equals(p3), "Unequal pairs are equal");
            Assert.IsFalse(p3.Equals(p1), "Unequal pairs are equal");


            // ================ HETEROGENEOUS PAIRS =================

            Pair<string, int> p4 = new Pair<string, int>("woah", 5);
            Pair<int, string> p5 = new Pair<int, string>(5, "woah");
            Pair<int, string> p6 = new Pair<int, string>(5, "hola");

            Assert.AreEqual(p4.GetHashCode(), p5.GetHashCode(), "Equivalent pairs have different hash codes");
            Assert.IsTrue(p4.Equals(p5), "Equivalent pairs are not equal");
            Assert.IsTrue(p5.Equals(p4), "Equivalent pairs are not equal");

            Assert.AreNotEqual(p4.GetHashCode(), p6.GetHashCode(), "Unequal pairs have the same hash code");
            Assert.IsFalse(p4.Equals(p6), "Unequal pairs are equal");
            Assert.IsFalse(p6.Equals(p4), "Unequal pairs are equal");

            Assert.AreNotEqual(p5.GetHashCode(), p6.GetHashCode(), "Unequal pairs have the same hash code");
            Assert.IsFalse(p5.Equals(p6), "Unequal pairs are equal");
            Assert.IsFalse(p6.Equals(p5), "Unequal pairs are equal");

        }
    }
}
