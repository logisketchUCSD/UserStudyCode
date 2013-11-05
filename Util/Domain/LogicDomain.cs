using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Domain
{
    public class LogicDomain : Domain
    {

        #region Internals

            #region Classifications
        public static string GATE_CLASS = "Gate";
        public static string TEXT_CLASS = "Text";
        public static string WIRE_CLASS = "Wire";
            #endregion

            #region Types
        public static ShapeType AND = new ShapeType("AND", GATE_CLASS, Colors.Brown);
        public static ShapeType OR = new ShapeType("OR", GATE_CLASS, Colors.Green);
        public static ShapeType NOT = new ShapeType("NOT", GATE_CLASS, Colors.Purple);
        public static ShapeType NOR = new ShapeType("NOR", GATE_CLASS, Colors.HotPink);
        public static ShapeType XOR = new ShapeType("XOR", GATE_CLASS, Colors.DarkCyan);
        public static ShapeType XNOR = new ShapeType("XNOR", GATE_CLASS, Colors.MediumOrchid);
        public static ShapeType NAND = new ShapeType("NAND", GATE_CLASS, Colors.DarkOrange);
        public static ShapeType NOTBUBBLE = new ShapeType("NotBubble", GATE_CLASS, Colors.Gold);

        public static ShapeType SUBCIRCUIT = new ShapeType("SubCircuit", GATE_CLASS, Colors.BlanchedAlmond);

        public static ShapeType FULLADDER = new ShapeType("FullAdder", GATE_CLASS, Colors.Aquamarine);
        public static ShapeType SUBTRACTOR = new ShapeType("Subtractor", GATE_CLASS, Colors.LimeGreen);

        public static ShapeType TEXT = new ShapeType("Text", TEXT_CLASS, Colors.Salmon);
        public static ShapeType WIRE = new ShapeType("Wire", WIRE_CLASS, Colors.Blue);
            #endregion

            #region Class Lists
        public static List<ShapeType> Gates = new List<ShapeType> { AND, NAND, OR, NOR, XOR, XNOR, NOT, NOTBUBBLE, SUBCIRCUIT, FULLADDER, SUBTRACTOR };
        public static List<ShapeType> Texts = new List<ShapeType> { TEXT };
        public static List<ShapeType> Wires = new List<ShapeType> { WIRE };
        public static List<ShapeType> Types = new List<ShapeType> { TEXT, WIRE, AND, OR, NOT, NOR, XOR, XNOR, NAND, SUBCIRCUIT, NOTBUBBLE, FULLADDER, SUBTRACTOR };
            #endregion

        #endregion

        #region Public Methods

        public static ShapeType getType(string type)
        {
            string typeLower = type.ToLower();
            switch (typeLower)
            {
                case "and": return AND;
                case "or": return OR;
                case "not": return NOT;
                case "nor": return NOR;
                case "xor": return XOR;
                case "xnor": return XNOR;
                case "nand": return NAND;
                case "notbubble": return NOTBUBBLE;

                case "subcircuit": return SUBCIRCUIT;
                case "fulladder": return FULLADDER;
                case "adder": return FULLADDER;
                case "subtractor": return SUBTRACTOR;

                case "text": return TEXT;
                case "label": return TEXT; // these two are to support old code where
                case "io": return TEXT;    // naming conventions were super inconsistant

                case "wire": return WIRE;

                default: return new ShapeType();
            }
        }

        /// <summary>
        /// Returns a Tuple representing the min number of outputs the gate can have, and the
        /// max number of outputs the gate can have.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Tuple<int, int> validOutputs(ShapeType type)
        {
            if (type == SUBCIRCUIT)
                return new Tuple<int, int>(0, 100);
            if (type == FULLADDER || type == SUBTRACTOR)
                return new Tuple<int, int>(2, 2);
            else
                return new Tuple<int, int>(1, 1);
        }

        /// <summary>
        /// The maximum number of outputs that the gate can have.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int MaxOutputs(ShapeType type)
        {
            return validOutputs(type).Item2;
        }

        /// <summary>
        /// Returns a Tuple representing the min number of inputs the gate can have, and the
        /// max number of inputs the gate can have.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Tuple<int, int> validInputs(ShapeType type)
        {
            if (type == SUBCIRCUIT)
                return new Tuple<int, int>(0, 100);
            if (type == FULLADDER || type == SUBTRACTOR)
                return new Tuple<int, int>(1, 3);
            else
                return new Tuple<int, int>(1, 5);
        }


        #endregion

        #region IsClass getters

        public static bool IsGate(ShapeType type)
        {
            return (type.Classification == GATE_CLASS);
        }

        public static bool IsWire(ShapeType type)
        {
            return (type.Classification == WIRE_CLASS);
        }

        public static bool IsText(ShapeType type)
        {
            return (type.Classification == TEXT_CLASS);
        }

        #endregion

    }
}
