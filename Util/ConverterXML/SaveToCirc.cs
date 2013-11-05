using Domain;
using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows;

namespace ConverterXML
{
    /// <summary>
    /// Provides functionality for saving to LogiSim format. Reverse-engineered 
    /// from examining .circ files that Logisim 2.7.1 produced 
    /// (basically a special type of XML file). June 2011.
    /// </summary>
    public class SaveToCirc
    {
        #region INTERNALS

        /// <summary>
        /// The Sketch
        /// </summary>
        private CircuitSimLib.Circuit circuit;

        #endregion

        #region Setup

        /// <summary>
        /// Imports the circuit into our SaveToCirc object.
        /// </summary>
        /// <param name="circ">The circuit to be converted</param>
        public SaveToCirc(CircuitSimLib.Circuit circ)
        {
            this.circuit = circ;
        }

        /// <summary>
        /// Saves the converted circuit to the given filename
        /// </summary>
        /// <param name="filename"></param>
        public void WriteToFile(string filename)
        {
            XmlTextWriter xmlDocument = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
            xmlDocument.Formatting = System.Xml.Formatting.Indented;

            xmlDocument.WriteStartElement("project");
            SpecifyLibraries(xmlDocument);
            SpecifyTools(xmlDocument);

            CircuitToXML(xmlDocument);
            xmlDocument.WriteEndElement();

            xmlDocument.Close();
        }

        #endregion

        #region LogiSim config
        /// <summary>
        /// Sets configuration details for LogiSim menus.
        /// </summary>
        /// <param name="textWriter"></param>
        private void SpecifyLibraries(XmlTextWriter textWriter)
        {
            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Wiring");
            textWriter.WriteAttributeString("name", "0");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Gates");
            textWriter.WriteAttributeString("name", "1");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Plexers");
            textWriter.WriteAttributeString("name", "2");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Arithmetic");
            textWriter.WriteAttributeString("name", "3");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Memory");
            textWriter.WriteAttributeString("name", "4");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#I/O");
            textWriter.WriteAttributeString("name", "5");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("lib");
            textWriter.WriteAttributeString("desc", "#Base");
            textWriter.WriteAttributeString("name", "6");
            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("name", "Text Tool");

            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "text");
            textWriter.WriteAttributeString("val", "");
            textWriter.WriteEndElement();
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "font");
            textWriter.WriteAttributeString("val", "SansSerif plain 12");
            textWriter.WriteEndElement();
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "halign");
            textWriter.WriteAttributeString("val", "center");
            textWriter.WriteEndElement();
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "valign");
            textWriter.WriteAttributeString("val", "base");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();
            textWriter.WriteEndElement();
        }

        private void SpecifyTools(XmlTextWriter textWriter)
        {
            textWriter.WriteStartElement("main");
            textWriter.WriteAttributeString("name", "main");
            textWriter.WriteEndElement();

            // Tool mappings
            textWriter.WriteStartElement("mappings");
            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("map", "Button2");
            textWriter.WriteAttributeString("name", "Menu Tool");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("map", "Ctrl Button1");
            textWriter.WriteAttributeString("name", "Menu Tool");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("map", "Button3");
            textWriter.WriteAttributeString("name", "Menu Tool");
            textWriter.WriteEndElement();
            textWriter.WriteEndElement();

            // Toolbars
            textWriter.WriteStartElement("toolbar");

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("name", "Poke Tool");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("name", "Edit Tool");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "6");
            textWriter.WriteAttributeString("name", "Text Tool");
            textWriter.WriteEndElement();

            // Input pin
            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "0");
            textWriter.WriteAttributeString("name", "Pin");
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "tristate");
            textWriter.WriteAttributeString("val", "false");
            textWriter.WriteEndElement();
            textWriter.WriteEndElement();

            // Input pin
            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "0");
            textWriter.WriteAttributeString("name", "Pin");
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "output");
            textWriter.WriteAttributeString("val", "true");
            textWriter.WriteEndElement();
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "1");
            textWriter.WriteAttributeString("name", "NOT Gate");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "1");
            textWriter.WriteAttributeString("name", "AND Gate");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("tool");
            textWriter.WriteAttributeString("lib", "1");
            textWriter.WriteAttributeString("name", "OR Gate");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();
        }

        #endregion

        #region Write circuit elements
        /// <summary>
        /// Writes the appropriate LogiSim XML to the given file
        /// </summary>
        /// <param name="textWriter"></param>
        public void CircuitToXML(XmlTextWriter textWriter)
        {
            textWriter.WriteStartElement("circuit");
            textWriter.WriteAttributeString("name", "main");

            foreach (var nameAndElement in circuit.CircuitElementGraph)
            {
                CircuitSimLib.CircuitElement element = nameAndElement.Value;
                if (element is CircuitSimLib.INPUT)
                    InputToXML(textWriter, (CircuitSimLib.INPUT)element);
                else if (element is CircuitSimLib.OUTPUT)
                    OutputToXML(textWriter, (CircuitSimLib.OUTPUT)element);
                else if (element is CircuitSimLib.Gate)
                    GateToXML(textWriter, (CircuitSimLib.Gate)element);
                else
                    Console.Write("invalid type");
                WiresOutOfElementToXML(textWriter, element);
            }
        }

        /// <summary>
        /// Writes all the outgoing wires of the given element
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="element"></param>
        private void WiresOutOfElementToXML(XmlTextWriter textWriter, CircuitSimLib.CircuitElement element)
        {
            foreach(var outputIndexAndNeighbors in element.Outputs)
            {
                int outputNo = outputIndexAndNeighbors.Key;
                Point start = getOutpoint(element, outputNo);

                System.Collections.Generic.List<CircuitSimLib.CircuitElement> rightNeighbors = outputIndexAndNeighbors.Value;
                foreach (CircuitSimLib.CircuitElement rightNeighbor in rightNeighbors)
                {
                    // Generate the inpoint based on which input we're connected to
                    System.Collections.Generic.List<int> inputNo;
                    rightNeighbor.Inputs.TryGetValue(element, out inputNo);
                    foreach (int index in inputNo)
                    {
                        Point end = getInpoint(rightNeighbor, index);

                        // Create the wires
                        ConnectWires(textWriter, start, end);
                    }
                }
            }
        }

        /// <summary>
        /// Get the location of the input from the element and the index of the input
        /// </summary>
        /// <param name="element">The element with inputs</param>
        /// <param name="inputNo">Index of the input (0-indexed)</param>
        /// <returns></returns>
        private Point getInpoint(CircuitSimLib.CircuitElement element, int inputNo)
        {
            Point location = ConvertPointToLogisimPoint(element.OutPoint());
            if (element is CircuitSimLib.INPUT || element is CircuitSimLib.OUTPUT)
                return location;
            else if (element is CircuitSimLib.NOT)
            {
                return new Point(location.X - 30, location.Y);
            }
            else
            {
                int locX = (int)location.X - 50;
                int locY = (int)location.Y + (inputNo - 2)*10;
                return new Point(locX, locY);
            }
        }

        /// <summary>
        /// Get the location of the output from the element and the index of the output
        /// </summary>
        /// <param name="element">The element which has outputs</param>
        /// <param name="outputNo">The index of the output (0-indexed) </param>
        /// <returns></returns>
        private Point getOutpoint(CircuitSimLib.CircuitElement element, int outputNo)
        {
            // Currently just returns the location of the output
            return element.OutPoint(outputNo);
        }

        /// <summary>
        /// Create wires according to LogiSim specs (no diagonals)
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void ConnectWires(XmlTextWriter textWriter, Point start, Point end)
        {
            // Convert to LogiSim points first
            start = ConvertPointToLogisimPoint(start);
            end = ConvertPointToLogisimPoint(end);

            if (start == end)
                return;
            // already a straight line
            else if (start.X == end.X || start.Y == end.Y)
            {
                textWriter.WriteStartElement("wire");
                textWriter.WriteAttributeString("from", GetLocationAsString(start));
                textWriter.WriteAttributeString("to", GetLocationAsString(end));
                textWriter.WriteEndElement();
            }
            // split the wire into three linear segments
            else
            {
                Point mid1 = new Point((start.X + end.X) / 2, start.Y);
                Point mid2 = new Point((start.X + end.X) / 2, end.Y);
                ConnectWires(textWriter, start, mid1);
                ConnectWires(textWriter, mid1, mid2);
                ConnectWires(textWriter, mid2, end); 
            }
        }

        /// <summary>
        /// Writes the gate in Logisim format
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="gate"></param>
        private void GateToXML(XmlTextWriter textWriter, CircuitSimLib.Gate gate)
        {
            // Declare that it's a particular type of gate
            textWriter.WriteStartElement("comp");
            textWriter.WriteAttributeString("lib", "1");
            textWriter.WriteAttributeString("loc", GetLocationAsString(gate.OutPoint()));
            textWriter.WriteAttributeString("name", GateTypeToLogiSimType(gate.GateType));

            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "label");
            textWriter.WriteAttributeString("val", gate.Name);
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes the output in Logisim format
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="output"></param>
        private void OutputToXML(XmlTextWriter textWriter, CircuitSimLib.OUTPUT output)
        {
            textWriter.WriteStartElement("comp");

            // Declare that it's a Pin
            textWriter.WriteAttributeString("lib", "0");
            textWriter.WriteAttributeString("loc", GetLocationAsString(output.OutPoint()));
            textWriter.WriteAttributeString("name", "Pin");

            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "label");
            textWriter.WriteAttributeString("val", output.Name);
            textWriter.WriteEndElement();

            // Declare that it's an output
            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "output");
            textWriter.WriteAttributeString("val", "true");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();
        }

        /// <summary>
        /// Writes an input in Logisim format
        /// </summary>
        /// <param name="textWriter"></param>
        /// <param name="input"></param>
        public void InputToXML(XmlTextWriter textWriter, CircuitSimLib.INPUT input)
        {
            textWriter.WriteStartElement("comp");

            // Declare that it's an IO
            textWriter.WriteAttributeString("lib", "0");
            textWriter.WriteAttributeString("loc", GetLocationAsString(input.OutPoint()));
            textWriter.WriteAttributeString("name", "Pin");

            textWriter.WriteStartElement("a");
            textWriter.WriteAttributeString("name", "label");
            textWriter.WriteAttributeString("val", input.Name);
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();
        }

        /// <summary>
        /// Convert a Point into a XML-acceptable coordinate pair (x, y)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private string GetLocationAsString(Point point)
        {
            Point logisimPoint = ConvertPointToLogisimPoint(point);
            Tuple<int, int> intCoords = Tuple.Create((int)logisimPoint.X, (int)logisimPoint.Y);
            return intCoords.ToString();
        }
        /// <summary>
        /// Rounds a Point off to the closest point on a 10x10 grid
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Point ConvertPointToLogisimPoint(Point point)
        {
            // LogiSim wants the coords to be multiples of 10
            int x = (int)(point.X / 10) * 10;
            int y = (int)(point.Y / 10) * 10;
            return new Point(x, y);
        }

        /// <summary>
        /// Get the LogiSim name for the gate from the ShapeType
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        private string GateTypeToLogiSimType(ShapeType shapeType)
        {
            if (shapeType == LogicDomain.AND)
                return "AND Gate";
            else if (shapeType == LogicDomain.OR)
                return "OR Gate";
            else if (shapeType == LogicDomain.NOT)
                return "NOT Gate";
            else if (shapeType == LogicDomain.NOR)
                return "NOR Gate";
            else if (shapeType == LogicDomain.XOR)
                return "XOR Gate";
            else if (shapeType == LogicDomain.XNOR)
                return "XNOR Gate";
            else if (shapeType == LogicDomain.NAND)
                return "NAND Gate";
            else
                return "blargety blarg";
        }
        #endregion

    }
}