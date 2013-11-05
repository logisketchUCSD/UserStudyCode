using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CircuitSimLib
{
    public class TruthTable
    {
        #region Internals

        /// <summary>
        /// List of strings that will appear as the header of the truth table
        /// </summary>
        private List<string> truthTableHeader;

        /// <summary>
        /// List of the inputs as strings
        /// </summary>
        private List<string> inputs;

        /// <summary>
        /// List of the outputs as strings
        /// </summary>
        private List<string> outputs;

        /// <summary>
        /// Dictionary, the key is a list of input values, the value is a list of output values 
        /// </summary>
        private Dictionary<List<int>, List<int>> truthTableDictionary;

        /// <summary>
        /// Similar to the dictionary, but requires no key (must know num inputs to decode)
        /// </summary>
        private List<List<int>> truthTableOutputs;

        #endregion

        #region Constructor

        /// <summary>
        /// Default construct for an empty truth table
        /// </summary>
        public TruthTable()
        {
            truthTableHeader = new List<string>();
            inputs = new List<string>();
            outputs = new List<string>();
            truthTableDictionary = new Dictionary<List<int>,List<int>>();
            truthTableOutputs = new List<List<int>>();
        }

        /// <summary>
        /// Constructor for a truth table, given a circuit, List of inputs, and List of outputs
        /// </summary>
        /// <param name="circuit"></param>
        /// <param name="inputs"></param>
        /// <param name="heads"></param>
        public TruthTable(Circuit circuit)
        {
            List<INPUT> inputs = circuit.GlobalInputs;
            List<OUTPUT> heads = circuit.GlobalOutputs;
            truthTableOutputs = new List<List<int>>();
            this.inputs = new List<string>();
            this.outputs = new List<string>();

            //create the Lists
            foreach (INPUT i in inputs)
            {
                this.inputs.Add(i.Name);
            }
            foreach (OUTPUT o in heads)
            {
                this.outputs.Add(o.Name);
            }

            //create the dictionary
            int numInputs = inputs.Count;
            Dictionary<List<int>, List<int>> tempDict = new Dictionary<List<int>, List<int>>();

            for (int i = 0; i < Math.Pow(2,numInputs); i++)
            {
                List<int> key = toBinary(i, numInputs);
                circuit.setInputValues(key);
                List<int> value = new List<int>();
                for (int j = 0; j < heads.Count(); j++)
                {
                    value.Add(heads[j].calculate()[0]);
                }


                tempDict.Add(key, value);
                truthTableOutputs.Add(new List<int>(key.Concat(value)));
            }
            truthTableDictionary = tempDict;
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// returns a List of strings that represent the header of a truth table
        /// </summary>
        public List<string> TruthTableHeader
        {
            get
            {
                return new List<string>(this.inputs.Concat(this.outputs));// this.truthTableHeader;
            }
        }

        /// <summary>
        /// returns a Dictionary with keys as input combinations and values as output value(s)
        /// </summary>
        public Dictionary<List<int>, List<int>> TruthTableDictionary
        {
            get
            {
                return this.truthTableDictionary;
            }
        }

        /// <summary>
        /// returns a list with lists of input and output values
        /// </summary>
        public List<List<int>> TruthTableOutputs
        {
            get
            {
                return truthTableOutputs;
            }
        }

        /// <summary>
        /// returns a list with lists of inputs
        /// </summary>
        public List<string> Inputs
        {
            get
            {
                return this.inputs;
            }
        }

        /// <summary>
        /// returns a list with lists of outputs
        /// </summary>
        public List<string> Outputs
        {
            get
            {
                return this.outputs;
            }
        }

        #endregion

        #region Helper Method

        /// <summary>
        /// helper method that converts decimal to binary with the correct amount of placeholders
        /// </summary>
        /// <param name="num"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<int> toBinary(int num, int size)
        {
            List<int> ans = new List<int>();
            int[] lol = new int[size];
            for (int i = 0; i < lol.Length; i++)
            {
                if (num > 0)
                {
                    lol[i] = num % 2;
                    num = num / 2;
                }
                else
                {
                    lol[i] = 0;
                }
            }
            for (int i = lol.Length - 1; i > -1; i--)
            {
                ans.Add(lol[i]);
            }
            return ans;
        }
        #endregion
    }
}
