using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;

namespace CircuitSimLib
{
    public class Circuit
    {
        #region Internals

        private int numInputs;

        private List<Gate> myGates;
    
        private List<INPUT> globalInputs;

        private Dictionary<string, CircuitElement> namesToElements;

        private List<OUTPUT> globalOutputs;

        private Dictionary<string, int> outputValues;

        private List<List<OUTPUT>> circuitList;

        private bool isOscillating;

        private bool debug = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a Circuit given a Dictionary of all the gates, 
        /// List of all outputs, and List of all inputs
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="outputs"></param>
        /// <param name="inputs"></param>
        public Circuit(Dictionary<string, Data.Pair<Sketch.Shape, Dictionary<string, int>>> dictionary,
                        Dictionary<string, Data.Pair<Sketch.Shape, Data.Pair<string, int>>> outputs, Dictionary<string, Sketch.Shape> inputs)
        {
            if (debug)
            {
                string gates = "Gates: ";
                foreach (string gate in dictionary.Keys)
                    gates += gate + " ";
                string instring = "Inputs: ";
                foreach (string inp in inputs.Keys)
                    instring += inp + " ";
                string outstring = "Outputs: ";
                foreach (string outp in outputs.Keys)
                    outstring += outp + " ";
                Console.WriteLine(gates);
                Console.WriteLine(instring);
                Console.WriteLine(outstring);
            }

            globalInputs = new List<INPUT>();
            myGates = new List<Gate>();
            globalOutputs = new List<OUTPUT>();
            namesToElements = new Dictionary<string, CircuitElement>();
            outputValues = new Dictionary<string, int>();
            isOscillating = false;

            // Builds global inputs
            foreach (string inputName in inputs.Keys)
            {
                INPUT input = new INPUT(inputName, inputs[inputName].Bounds, inputs[inputName].Orientation);
                namesToElements.Add(inputName, input);
                globalInputs.Add(input);
            }

            // Builds logic gates within the circuit
            foreach (string gateName in dictionary.Keys)
            {
                Gate tempGate = null;
                Rect bounds = dictionary[gateName].A.Bounds;
                double orientation = dictionary[gateName].A.Orientation;
                ShapeType type = dictionary[gateName].A.Type;

                if (type == LogicDomain.AND)
                    tempGate = new AND(gateName, bounds, orientation);
                else if (type == LogicDomain.NAND)
                    tempGate = new NAND(gateName, bounds, orientation);
                else if (type == LogicDomain.OR)
                    tempGate = new OR(gateName, bounds, orientation);
                else if (type == LogicDomain.NOR)
                    tempGate = new NOR(gateName, bounds, orientation);
                else if (type == LogicDomain.XOR)
                    tempGate = new XOR(gateName, bounds, orientation);
                else if (type == LogicDomain.XNOR)
                    tempGate = new XNOR(gateName, bounds, orientation);
                else if (type == LogicDomain.NOT)
                    tempGate = new NOT(gateName, bounds, orientation);
                else if (type == LogicDomain.NOTBUBBLE)
                    tempGate = new NOTBUBBLE(gateName, bounds, orientation);
                else if (type == LogicDomain.SUBCIRCUIT)
                    tempGate = new SubCircuit(gateName, bounds, dictionary[gateName].A, orientation);
                else if (type == LogicDomain.FULLADDER)
                    tempGate = new Adder(gateName, bounds, orientation);
                else if (type == LogicDomain.SUBTRACTOR)
                    tempGate = new Subtractor(gateName, bounds, orientation);

                if (tempGate != null)
                {
                    namesToElements.Add(gateName, tempGate);
                    myGates.Add(tempGate);
                }
            }

            // Connect all CircuitElements
            foreach (string gateName in dictionary.Keys)
            {
                foreach (string inputName in dictionary[gateName].B.Keys)
                {
                    namesToElements[gateName].addChild(namesToElements[inputName], dictionary[gateName].B[inputName]);
                }
            }

            //builds and connects all global Outputs
            foreach(string gate in outputs.Keys)
            {
                OUTPUT output = new OUTPUT(gate, outputs[gate].A.Bounds, namesToElements[outputs[gate].B.A]);
                output.addChild(namesToElements[outputs[gate].B.A], outputs[gate].B.B);
                namesToElements.Add(gate, output);
                globalOutputs.Add(output);
            }
            numInputs = globalInputs.Count;

            //This part of the code (until the end of the constructor) recognizes whether if the drawn diagram contains more than one circuit.
            List<KeyValuePair<OUTPUT, List<INPUT>>> tempList = new List<KeyValuePair<OUTPUT, List<INPUT>>>();
            circuitList = new List<List<OUTPUT>>();
            foreach (OUTPUT output in globalOutputs)
            {
                List<INPUT> lis = findGlobalInputs(output);
                KeyValuePair<OUTPUT, List<INPUT>> k = new KeyValuePair<OUTPUT, List<INPUT>>(output, lis);
	            tempList.Add(k);
            }
            for (int i = 0; i < tempList.Count; i++)
            {
                List<OUTPUT> outputList = new List<OUTPUT>();
                outputList.Add(tempList[i].Key);
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (i != j)
                    {
                        bool intersect = Check(tempList[i].Value, tempList[j].Value);
                        if (intersect)
                        {
                            outputList.Add(tempList[j].Key);
                        }
                    }
                }

                bool newCircuit = true;
                foreach (List<OUTPUT> subCircuit in circuitList)
                    if (Check(subCircuit, outputList))
                        newCircuit = false;
                if (newCircuit)
                    circuitList.Add(outputList);
            }
        }
        #endregion

        #region Getters and Setters

        /// <summary>
        /// Returns the list of global inputs in the circuit.
        /// </summary>
        public List<INPUT> GlobalInputs
        {
            get { return this.globalInputs; }
        }

        /// <summary>
        /// returns a list of all global outputs in the circuit
        /// </summary>
        public List<OUTPUT> GlobalOutputs
        {
            get
            {
                return this.globalOutputs;
            }
        }

        public List<Gate> AllGates
        {
            get
            {
                return this.myGates;
            }
        }

        /// <summary>
        /// dictionary all global output values of the circuit in a List of ints
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> AllOutputValues
        {
            get
            {
                return this.outputValues;
            }
        }

        /// <summary>
        /// returns the number of global inputs in the circuit
        /// </summary>
        /// <returns></returns>
        public int NumInputs
        {
            get
            {
                return this.numInputs;
            }
        }

        /// <summary>
        /// Returns the dictionary containing the circuit elements in the circuit.
        /// </summary>
        public Dictionary<string, CircuitElement> CircuitElementGraph
        {
            get { return namesToElements; }
        }

        #endregion
        
        #region Public Methods
       
        /// <summary>
        /// calculate all the values of the outputs of the circuit, given input values. also takes care of oscillations
        /// </summary>
        public void calculateOutputs()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            Boolean loopBreaker = true;
            List<string> knownConfigurations = new List<string>();
            while (loopBreaker)
            {
                foreach (OUTPUT output in globalOutputs)
                {
                    int outputValue = output.calculate()[0];
                    dict[output.Name] = outputValue;
                }
                string config = "";
                foreach (INPUT input in globalInputs)
                {
                    config += input.Output;
                }
                foreach (Gate gate in myGates)
                {
                    config += gate.Output;
                }
                if (knownConfigurations.Contains(config))
                {
                    loopBreaker = false;
                    if(config.Equals(knownConfigurations[knownConfigurations.Count-1]))
                    {
                        isOscillating = false;
                    }
                    else
                    {
                        isOscillating = true;
                    }
                }
                else
                {
                    knownConfigurations.Add(config);
                }
            }
            outputValues = dict;
            if (isOscillating)
            {
                MessageBox.Show("Oscillation Apparent");
            }
        }


        /// <summary>
        /// Change a value of an individual input
        /// </summary>
        /// <param name="inputName"></param>
        /// <param name="inputValue"></param>
        public void setInputValue(string inputName, int inputValue)
        {
            ((INPUT)namesToElements[inputName]).Value = inputValue;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// returns a list of all Global Inputs to a given CircuitElement
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<INPUT> findGlobalInputs(CircuitElement e)
        {
            List<CircuitElement> visited = new List<CircuitElement>();
            List<CircuitElement> toBeVisited = new List<CircuitElement>();
            List<INPUT> inputList = new List<INPUT>();

            visited.Add(e);

            while (toBeVisited.Count > 0)
            {
                CircuitElement cur = toBeVisited[0];
                visited.Add(cur);
                toBeVisited.Remove(cur);

                if ((cur.Inputs == null || cur.Inputs.Count == 0) && cur is INPUT)
                {
                    inputList.Add((INPUT)e);
                }
                else if (cur.Inputs != null)
                {
                    foreach (CircuitElement input in cur.Inputs.Keys)
                    {
                        if (!visited.Contains(input) && !toBeVisited.Contains(input))
                        {
                            toBeVisited.Add(input);
                        }
                    }
                }
            }

            return inputList;
        }

        /// <summary>
        /// checks if any of the inputs in the list overlap
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool Check(List<INPUT> first, List<INPUT> second)
        {
            foreach (INPUT secondItem in second)
                if (first.Contains(secondItem))
                    return true;
            return false;
        }

        /// <summary>
        /// checks if any of the output in the list overlap
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool Check(List<OUTPUT> first, List<OUTPUT> second)
        {
            foreach (OUTPUT secondItem in second)
                if (first.Contains(secondItem))
                    return true;
            return false;
        }

        /// <summary>
        /// allows user to change the values of the inputs, specifically used for TruthTable
        /// </summary>
        /// <param name="j"></param>
        public void setInputValues(List<int> j)
        {
            for (int i = 0; i < j.Count; i++)
            {
                globalInputs[i].Value = j[i];
            }
        }

        /// <summary>
        /// gives an output of a gate, given its name and index
        /// </summary>
        /// <param name="gateName"></param>
        /// <returns></returns>
        public int gateValue(string gateName, int index)
        {
            return namesToElements[gateName].Output[index];
        }

        #endregion

    }

}
