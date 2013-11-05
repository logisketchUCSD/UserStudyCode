using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace CircuitSimLib
{
    #region CircuitElement

    public abstract class CircuitElement
    {
        #region Internals

        /// <summary>
        /// the outputs represented in 1 or 0
        /// </summary>
        protected int[] myOutput;
        
        /// <summary>
        /// the previous output
        /// </summary>
        protected int[] myPrevOutput;

        /// <summary>
        /// The unique ID of this element
        /// </summary>
        protected Guid myId;

        /// <summary>
        /// name of this circuit
        /// </summary>
        protected String myName;

        /// <summary>
        /// type of CircuitElement (i.e. Gate, Input, or Output)
        /// </summary>
        protected String type;

        /// <summary>
        /// list of all inputs to the CircuitElement, along with the index of the input they are connected to
        /// </summary>
        protected Dictionary<CircuitElement, List<int>> myInputs;

        /// <summary>
        /// list of all parent CircuitElement of this CircuitElement.  Each sublist corresponds to an output of the gate
        /// </summary>
        protected Dictionary<int, List<CircuitElement>> myOutputs;

        /// <summary>
        /// The bounds of this element's corresponding shape
        /// </summary>
        protected Rect myBounds;

        /// <summary>
        /// The bounds of this element in the clean circuit
        /// </summary>
        private Rect myCleanBounds;

        /// <summary>
        /// The orientation of this element, in radians.
        /// </summary>
        protected double myOrientation;

        /// <summary>
        /// The orientation of this element in degrees, rounded to the nearest 90.
        /// DON"T use this variable, use Angle.
        /// </summary>
        private int? myAngle = null;

        #endregion

        #region Getters and Setters

        /// <summary>
        ///gives the name of the CircuitElement 
        /// </summary>
        /// <returns></returns>
        public String Name
        {
            get
            {
                return this.myName;
            }
        }

        /// <summary>
        /// gives the output of the CircuitElement
        /// </summary>
        /// <returns></returns>
        public int[] Output
        {
            get { return myOutput; }
        }

        /// <summary>
        /// gives a list of all inputs to this CircuitElement
        /// </summary>
        public Dictionary<CircuitElement, List<int>> Inputs
        {
            get { return myInputs; }
        }

        /// <summary>
        /// gives the list of parent nodes to this CircuitElement
        /// </summary>
        public Dictionary<int, List<CircuitElement>> Outputs
        {
            get { return myOutputs; }
        }

        /// <summary>
        /// gives the type of CircuitElement
        /// </summary>
        public String Type
        {
            get { return type; }
        }

        /// <summary>
        /// gets or sets the CircuitElement's previous output
        /// </summary>
        public int[] PrevOutput
        {
            get { return myPrevOutput; }
            set { myPrevOutput = value; }
        }

        /// <summary>
        /// The bounds of the shape corresponding to this circuit element
        /// </summary>
        public Rect Bounds
        {
            get { return myBounds; }
        }

        /// <summary>
        /// The bounds of the elemen in the clean circuit
        /// </summary>
        public Rect CleanBounds
        {
            get { return myCleanBounds; }
            set { myCleanBounds = value; }
        }

        /// <summary>
        /// The orientation of this shape, in radians
        /// </summary>
        public double Orientation
        {
            get { return myOrientation; }
        }

        /// <summary>
        /// The unique ID of this element
        /// </summary>
        public Guid ID
        {
            get
            {
                if (myId == null)
                    myId = new Guid();
                return myId;
            }
        }

        /// <summary>
        /// The angle of the shape, rounded to the nearest 90 degrees
        /// </summary>
        public int Angle
        {
            get
            {
                if (myAngle != null)
                    return (int)myAngle;

                // Convert to degrees
                double orientation = myOrientation * 180 / Math.PI;

                // Get an angle between 0 and 360
                orientation = (orientation + 360) % 360;

                // Round to nearest horizonatal or vertical.
                if (orientation < 45)
                    orientation = 0;
                else if (orientation < 135)
                    orientation = 90;
                else if (orientation < 225)
                    orientation = 180;
                else if (orientation < 315)
                    orientation = 270;
                else
                    orientation = 0;

                myAngle = (int)orientation;

                return (int)myAngle;
            }
        }
        #endregion
        
        #region Methods

        /// <summary>
        /// wires two CircuitElements (makes an added element its input)
        /// Only one of addChild and addParent needs to be called on the pair.
        /// </summary>
        /// <param name="inputToBeAdded"></param>
        public void addChild(CircuitElement inputToBeAdded, int index)
        {
            if (myInputs.ContainsKey(inputToBeAdded) && myInputs[inputToBeAdded].Contains(index))
                return;

            if (!myInputs.ContainsKey(inputToBeAdded))
                myInputs[inputToBeAdded] = new List<int>();
            myInputs[inputToBeAdded].Add(index);

            inputToBeAdded.addParent(this, index);
        }

        /// <summary>
        /// adds the parent circuitelement so that the child can refer to it.
        /// Only one of addChild and addParent needs to be called on the pair.
        /// </summary>
        /// <param name="outputToBeAdded"></param>
        public void addParent(CircuitElement outputToBeAdded, int index)
        {
            if (myOutputs.ContainsKey(index) && myOutputs[index].Contains(outputToBeAdded))
                return;

            if (!myOutputs.ContainsKey(index))
                myOutputs[index] = new List<CircuitElement>();
            myOutputs[index].Add(outputToBeAdded);

            outputToBeAdded.addChild(this, index);
        }

        /// <summary>
        /// Returns true if this element has the given parent
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool hasParent(CircuitElement parent)
        {
            foreach (List<CircuitElement> list in Outputs.Values)
                foreach (CircuitElement elem in list)
                    if (elem == parent) return true;
            return false;
        }

        /// <summary>
        /// given input values, CircuitElements gives list of ouput values
        /// </summary>
        /// <returns></returns>
        public abstract int[] calculate();

        /// <summary>
        /// Gets where a wire should connect to be an input to this element
        /// </summary>
        /// <param name="numInputsAdded"></param>
        /// <returns></returns>
        public Point InPoint(ref Dictionary<string, int> numInputsAdded, bool clean = false)
        {
            // Add this to the dictioanry
            if (numInputsAdded.ContainsKey(Name))
                numInputsAdded[Name] += 1;
            else
                numInputsAdded[Name] = 1;

            // Are we in the clean circuit?
            Rect theseBounds = Bounds;
            if (clean && CleanBounds != null) theseBounds = CleanBounds;

            // Get the point depending on how we're oriented
            Point end = new Point();
            if (Angle == 0)
            {
                end = theseBounds.TopLeft;
                end.Y += numInputsAdded[Name] * theseBounds.Height / (Inputs.Count + 1);
            }
            else if (Angle == 90)
            {
                end = theseBounds.TopRight;
                end.X -= numInputsAdded[Name] * theseBounds.Width / (Inputs.Count + 1);
            }
            else if (Angle == 180)
            {
                end = theseBounds.BottomRight;
                end.Y -= numInputsAdded[Name] * theseBounds.Height / (Inputs.Count + 1);
            }
            else if (Angle == 270)
            {
                end = theseBounds.BottomLeft;
                end.X += numInputsAdded[Name] * theseBounds.Width / (Inputs.Count + 1);
            }
            else  // If we're not snapping angles in Gate Drawing, we'll get here
            {
                end = theseBounds.TopLeft;
                end.Y += numInputsAdded[Name] * theseBounds.Height / (Inputs.Count + 1);
            }

            return end;
        }

        /// <summary>
        /// Returns the point where wires should connect to this element to recieve its output given the index of the output 
        /// Default index is 0.
        /// </summary>
        public Point OutPoint(int index = 0, bool clean = false)
        {
            // Are we in the clean circuit?
            Rect theseBounds = Bounds;
            if (clean && CleanBounds != null) theseBounds = CleanBounds;

            // Get the point based on the shape's orientation
            Point outPoint = new Point();
            if (Angle == 0)
                outPoint = new Point(theseBounds.Right, theseBounds.Top + (index + 1) * theseBounds.Height / (myOutput.Count() + 1));
            else if (Angle == 90)
                outPoint = new Point(theseBounds.Right - (index + 1) * theseBounds.Width / (myOutput.Count() + 1), theseBounds.Bottom);
            else if (Angle == 180)
                outPoint = new Point(theseBounds.Left, theseBounds.Bottom - (index + 1) * theseBounds.Height / (myOutput.Count() + 1));
            else if (Angle == 270)
                outPoint = new Point(theseBounds.Left + (index + 1) * theseBounds.Width / (myOutput.Count() + 1), theseBounds.Top);
            else  // If we're not snapping angles in Gate Drawing, we'll get here
                outPoint = new Point(theseBounds.Right, theseBounds.Top + (index + 1) * theseBounds.Height / (myOutput.Count() + 1));

            return outPoint;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// check whether this circuitelement is a gate
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsGate
        {
            get
            {
                if (this.Type == "Gate")
                {
                    return true;
                }
                return false;
            }
        }

        #endregion
    }

    #endregion

    #region Logic Gates

    #region Gate
    /// <summary>
    /// One concrete subclass of the CircuitElement, also a superclass
    /// </summary>
    public class Gate : CircuitElement
    {
        #region Internals

        /// <summary>
        /// type of gate (AND, OR, NOT, etc.)
        /// </summary>
        protected ShapeType logicgate;

        #endregion

        #region Getters

        /// <summary>
        /// returns the type of gate
        /// </summary>
        public ShapeType GateType
        {
            get { return this.logicgate; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of a Gate given a name
        /// </summary>
        /// <param name="gateName"></param>
        public Gate(String gateName, Rect bounds, double orientation = 0)
        {
            type = "Gate";
            myInputs = new Dictionary<CircuitElement, List<int>>();
            myOutputs = new Dictionary<int, List<CircuitElement>>();
            myName = gateName;
            myOutput = new int[1];
            myOutput[0] = 0;
            myPrevOutput = myOutput;
            myBounds = bounds;
            myOrientation = orientation;
        }

        #endregion

        #region Method
        
        /// <summary>
        /// arbitrarily returns a 1 (since the abstract method needs to be overridden)
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            return new int[1];
        }

        #endregion

    }

    #endregion 

    #region AND

    public class AND : Gate
    {
       #region Constructor
        /// <summary>
        /// Constructor of an AND gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
       public AND(String gateName, Rect bounds, double orientation = 0): 
           base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.AND;
        }

        #endregion

       #region Methods

        /// <summary>
        /// gives an output, using properties of AND gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            myOutput[0] = 1;
            int result = 1;
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 0)
                        {
                            myOutput[0] = 0;
                        }
                    }
                    else
                    {
                        result = child.calculate()[index];
                        if (result == 0)
                            myOutput[0] = 0;
                    }
                }
            }
            PrevOutput = myOutput;
            return myOutput;
        }

       #endregion

    }

    #endregion

    #region OR

    public class OR : Gate
    {
        #region Constructor

        /// <summary>
        /// constructor of an OR gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public OR(String gateName, Rect bounds, double orientation = 0): 
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.OR;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of an OR gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            myOutput[0] = 0;
            int result = myOutput[0];
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 1)
                        {
                            myOutput[0] = 1;
                        }
                    }
                    else
                    {
                        result = child.calculate()[index];
                        if (result == 1)
                            myOutput[0] = 1;
                    }
                }
            }
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion
    }

    #endregion

    #region NOT

    public class NOT : Gate
    {
        #region Constructor

        /// <summary>
        /// Constructor of a NOT gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public NOT(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.NOT;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of a NOT gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            CircuitElement child = Inputs.Keys.ElementAt(0);
            myOutput[0] = (int)Math.Abs(child.calculate()[Inputs[child][0]] - 1);
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion

    }

    #endregion

    #region XOR

    public class XOR : Gate
    {
        #region Constructor

        /// <summary>
        /// Constructor of a XOR, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public XOR(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.XOR;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of a XOR gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            int result = 0;
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 1)
                        {
                            result++;
                        }
                    }
                    else
                    {
                        if (child.calculate()[index] == 1)
                        {
                            result++;
                        }
                    }
                }
            }
            myOutput[0] = result % 2;
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion

    }

    #endregion

    #region XNOR

    public class XNOR : Gate
    {

        #region Constructor

        /// <summary>
        /// Constructor of a XNOR gate, give a name
        /// </summary>
        /// <param name="gateName"></param>
        public XNOR(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.XNOR;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of XNOR gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            int result = 0;
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 1)
                        {
                            result++;
                        }
                    }
                    else
                    {
                        if (child.calculate()[index] == 1)
                        {
                            result++;
                        }
                    }
                }
            }
            myOutput[0] = (result + 1) % 2;

            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion

    }

    #endregion

    #region NAND

    public class NAND : Gate
    {

        #region Constructor

        /// <summary>
        /// Constructor of a NAND gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public NAND(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.NAND;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of NAND gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            myOutput[0] = 0;
            int result = 0;
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 0)
                        {
                            myOutput[0] = 1;
                        }
                    }
                    else
                    {
                        result = child.calculate()[index];
                        if (result == 0)
                            myOutput[0] = 1;
                    }
                }
            }
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion
    }

    #endregion

    #region NOR

    public class NOR : Gate
    {
        #region Constructor

        /// <summary>
        /// Constructor of a NOR gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public NOR(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.NOR;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of NOR gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            myOutput[0] = 1;
            int result = 0;
            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    if (hasParent(child))
                    {
                        if (child.PrevOutput[index] == 0)
                        {
                            myOutput[0] = 1;
                        }
                    }
                    else
                    {
                        result = child.calculate()[index];
                        if (result == 1)
                            myOutput[0] = 0;
                    }
                }
            }
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion

    }
    #endregion

    #region NOTBUBBLE

    public class NOTBUBBLE : Gate
    {
        #region Constructor

        /// <summary>
        /// Constructor of a NOTBUBBLE gate, given a name
        /// </summary>
        /// <param name="gateName"></param>
        public NOTBUBBLE(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.NOTBUBBLE;
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives an output, using properties of a NOT bubble
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            PrevOutput = myOutput;
            CircuitElement child = Inputs.Keys.ElementAt(0);
            myOutput[0] = (int)Math.Abs(child.calculate()[Inputs[child][0]] - 1);
            PrevOutput = myOutput;
            return myOutput;
        }

        #endregion
    }


    #endregion

    #endregion
    #region Sub-Circuit

    public class SubCircuit : Gate
    {
        /// <summary>
        /// A dictionary to look up what this gate is supposed to do. To be loaded in
        /// </summary>
        private Dictionary<int[], int[]> behavior;

        /// <summary>
        /// The number of inputs that this circuit needs to function
        /// </summary>
        public int inputsRequired;

        /// <summary>
        /// The number of outputs that this circuit needs to function
        /// </summary>
        public int outputsRequired;


        public Sketch.Shape myShape;
        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="gateName"></param>
        /// <param name="bounds"></param>
        /// <param name="orientation"></param>
        public SubCircuit(String gateName, Rect bounds, Sketch.Shape associatedShape, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            inputsRequired = 3;
            outputsRequired = 2;
            myOutput = new int[2];
            logicgate = LogicDomain.SUBCIRCUIT;
            myShape = associatedShape;
            //TODO make this load in the behavior from a file and save it into behavior
            behavior = new Dictionary<int[],int[]>();
            //Just for now make the behavior be 2 outputs, both 0 for each input.
            for (int i = 0; i <= 1; i++)
                for (int j = 0; j <= 1; j++)
                    for (int k = 0; k <= 1; k++)
                    {
                        behavior.Add(new int[] { i, j, k }, new int[] { 0, 0 });
                    }
        }

        public override int[] calculate()
        {
            //Make the int array to look up what the behavior is for the loaded gate.
            int[] temp = behavior.Keys.ToArray()[0];
            if (myInputs.Keys.Count !=temp.Length)
                return null;
            int[] inputLookup = new int[myInputs.Keys.Count];
            foreach (CircuitElement input in myInputs.Keys)
            {
                foreach (int index in Inputs[input])
                {
                    inputLookup[index] = input.calculate()[index];
                }

            }
            myOutput = new int[] { 0, 0 };// behavior[inputLookup];
            return myOutput;
        }

    }
    #endregion
    #region Math Gates
    // Each MathGate has at least two outputs, 

        #region Adder
    public class Adder : Gate
    {
        public Adder(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.FULLADDER;
            myOutput = new int[2];
        }

        public override int[] calculate()
        {
            if (myInputs.Count > 3)
                return null;

            int sum = 0;
            foreach (CircuitElement input in myInputs.Keys)
            {
                foreach (int index in Inputs[input])
                    sum += input.calculate()[index];
            }

            myOutput[0] = sum % 2;
            if (sum > 1) myOutput[1] = 1;
            else myOutput[1] = 0;

            return myOutput;
        }
    }
        #endregion

        #region Subtractor
    public class Subtractor : Gate
    {
        public Subtractor(String gateName, Rect bounds, double orientation = 0) :
            base(gateName, bounds, orientation)
        {
            logicgate = LogicDomain.SUBTRACTOR;
            myOutput = new int[2];
        }

        public override int[] calculate()
        {
            if (myInputs.Count > 3)
                return null;

            int sum1=0, sum2=0, sum3=0;
            bool first = true;

            foreach (CircuitElement child in Inputs.Keys)
            {
                foreach (int index in Inputs[child])
                {
                    int val = child.calculate()[index];
                    if (first)
                    {
                        sum1 = val;
                        sum2 = val;
                        sum3 = 0;
                        first = false;
                    }
                    else
                    {
                        sum1 -= val;
                        sum1 = Math.Abs(sum1);
                        sum3 += val;
                    }
                }
            }

            myOutput[0] = sum1;
            if (sum2 < sum3) myOutput[1] = 1;
            else myOutput[1] = 0;

            return myOutput;
        }
    }
        #endregion

    #endregion

    #region OUTPUT

    public class OUTPUT : CircuitElement
    {
        #region Internal

        /// <summary>
        ///  The only gate connected to this output
        /// </summary>
        private CircuitElement myLinkedGate;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of an output, given a name and the gate in which it is connected
        /// </summary>
        /// <param name="name"></param>
        /// <param name="linkedGate"></param>
        public OUTPUT(String name, Rect bounds, CircuitElement linkedGate, double orientation = 0)
        {
            type = "Output";
            myInputs = new Dictionary<CircuitElement, List<int>>();
            myOutputs = new Dictionary<int, List<CircuitElement>>();
            myName = name;
            myLinkedGate = linkedGate;
            myBounds = bounds;
            myOutput = new int[1];
            myOrientation = orientation;
        }

        /// <summary>
        /// Constructor of an output, given only a name
        /// </summary>
        /// <param name="name"></param>
        public OUTPUT(String name, Rect bounds)
        {
            type = "Output";
            myInputs = new Dictionary<CircuitElement, List<int>>();
            myOutputs = new Dictionary<int, List<CircuitElement>>();
            myName = name;
            myBounds = bounds;
            myOutput = new int[1];
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives the output of the connected gate
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            myOutput[0] = myLinkedGate.calculate()[Inputs[myLinkedGate][0]];
            return myOutput;
        }

        #endregion

    }

    #endregion

    #region INPUT

    public class INPUT : CircuitElement
    {
        #region Constructor
        
        /// <summary>
        /// Constructor of an input, given a name
        /// </summary>
        /// <param name="name"></param>
        public INPUT(string name, Rect bounds, double orientation = 0)
        {
            type = "Input";
            myInputs = new Dictionary<CircuitElement, List<int>>();
            myOutputs = new Dictionary<int, List<CircuitElement>>();
            myName = name;
            myBounds = bounds;
            myOutput = new int[1];
            myOrientation = orientation;
        }

        /// <summary>
        /// Constructor of an input, given a name and a value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="name"></param>
        public INPUT(int i, string name, Rect bounds, double orientation = 0) : 
            this(name, bounds, orientation)
        {
            myOutput[0] = i;
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// get or set the value of the input
        /// </summary>
        public int Value
        {
            get { return myOutput[0]; }
            set { myOutput[0] = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// gives the input value
        /// </summary>
        /// <returns></returns>
        public override int[] calculate()
        {
            return myOutput;
        }

        #endregion

    }

    #endregion 
}