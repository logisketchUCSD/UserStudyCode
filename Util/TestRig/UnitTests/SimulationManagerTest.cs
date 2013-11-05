using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    [DeploymentItem("settings.txt")]
    [DeploymentItem("Initialization Files\\FeatureListGroup.txt")]
    [DeploymentItem("Initialization Files\\FeatureListSingle.txt")]
    public class SimulationManagerTest
    {
        private SimulationManager.SimulationManager simulationManager;
        private InkToSketchWPF.InkCanvasSketch inkCanvasSketch;

        [TestInitialize]
        public void Initialize()
        {
            SketchPanelLib.SketchPanel panel = SketchPanelTest.newSketchPanel();
            inkCanvasSketch = panel.InkSketch;
            simulationManager = new SimulationManager.SimulationManager(ref panel);
        }

        [TestMethod]
        public void TestSetup()
        {
            Assert.IsNotNull(simulationManager);
        }

        [TestMethod]
        public void TestAddStroke()
        {
            Assert.IsTrue(simulationManager.recognizeCircuit());
        }
    }
}
