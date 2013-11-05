/*
 * File: Pin.cs
 * 
 * Author: Andrew Danowitz
 * Harvey Mudd College, Claremont, CA 91711
 * Sketchers 2007.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Pins
{
    /// <summary>
    /// Enum describing the polarity of a Pin
    /// </summary>
    public enum PinPolarity
    {
        Input,
        Ouput,
        Wire
    }

    /// <summary>
    /// Pin object stores a name, value, expected value, bussize, polarity and sketch
    /// shape.  Primarily for use with the truthtable and workpath classes
    /// </summary>
    public class Pin
    {
        PinPolarity polarity;
        String[] pinNames;
        char[] pinVal;
        char[] expected;
        int busSize;
        Sketch.Shape shape;

        #region Constructors

        /// <summary>
        /// Constructor for Pin class
        /// </summary>
        /// <param name="inOut">String containing whether Pin is an input ("in"
        /// or an output "out"</param>
        /// <param name="pinName">String containing Pin name</param>
        /// <param name="pinVal">Integer array containing values held by this instance
        /// of the Pin</param>
        public Pin(PinPolarity polarity, String pinName, char[] pinVal)
        {
            this.polarity = polarity;
            this.pinNames = new string[] { pinName };
            this.pinVal = new char[pinVal.Length];
            this.pinVal = pinVal;
            this.expected = null;
            this.busSize = pinVal.Length;
            this.shape = null;
        }

        /// <summary>
        /// Constructor for Pin class
        /// </summary>
        /// <param name="inOut">String containing whether Pin is an input ("in"
        /// or an output "out"</param>
        /// <param name="pinName">String containing Pin name</param>
        /// <param name="pinVal">Integer array containing values held by this instance
        /// of the Pin</param>
        /// <param name="expected">Integer array containing values that the Pin should
        /// be holding if the circuit is correct (primarily for output pins)</param>
        public Pin(PinPolarity polarity, String pinName, char[] pinVal, char[] expected)
        {
            this.polarity = polarity;
            this.pinNames = new string[] { pinName };
            this.pinVal = new char[pinVal.Length];
            this.pinVal = pinVal;
            this.expected = new char[expected.Length];
            this.expected = expected;
            this.busSize = pinVal.Length;
            this.shape = null;
        }

        /// <summary>
        /// Constructor for Pin class
        /// </summary>
        /// <param name="inOut">String containing whether Pin is an input ("in"
        /// or an output "out"</param>
        /// <param name="pinName">String containing Pin name</param>
        /// <param name="busSize">Integer containing the number of values contained by
        /// the Pin</param>
        public Pin(PinPolarity polarity, String pinName, int busSize)
        {
            this.polarity = polarity;
            this.pinNames = new string[] { pinName };
            this.busSize = busSize;
            this.expected = new char[busSize];
            this.pinVal = new char[busSize];
            this.shape = null;
        }

        /// <summary>
        /// Constructor for Pin class
        /// </summary>
        /// <param name="inOut">"in" put or "out" put</param>
        /// <param name="pinName">pin's label</param>
        /// <param name="shape">the shape representing the strokes of the pin</param>
        public Pin(PinPolarity polarity, String pinName, Sketch.Shape shape)
        {
            this.polarity = polarity;
            this.pinNames = new string[] { pinName };
            this.busSize = 1;
            this.expected = new char[busSize];
            this.pinVal = new char[busSize];
            this.shape = shape;
        }

        /// <summary>
        /// constructor that takes a list of alternate pin names
        /// </summary>
        /// <param name="polarity">input/output/wire</param>
        /// <param name="pinNames">string of possible pin names</param>
        /// <param name="shape">the shape representing the strokes of the pin</param>
        public Pin(PinPolarity polarity, String[] pinNames, Sketch.Shape shape)
        {
            this.polarity = polarity;
            this.pinNames = pinNames;
            this.busSize = 1;
            this.expected = new char[busSize];
            this.pinVal = new char[busSize];
            this.shape = shape;
        }

        #endregion

        #region Getters

        /// <summary>
        /// Returns the name of the Pin and 
        /// allows you to change the pinNames array
        /// </summary>
        public String PinName
        {
            get
            {
                return this.pinNames[0];
            }
            set
            {
                List<string> pins = new List<string>(this.pinNames);
                if (pins.Contains(value))
                {
                    string prevFirst = this.pinNames[0];
                    int index = pins.IndexOf(value);
                    this.pinNames[0] = value;
                    this.pinNames[index] = prevFirst;
                }
                else
                {
                    pins.Insert(0, value);
                    this.pinNames = pins.ToArray();
                }
            }
        }

        /// <summary>
        /// returns the array of pin names
        /// </summary>
        public String[] PinNames
        {
            get
            {
                return this.pinNames;
            }
        }

        /// <summary>
        /// Returns or sets the binary value actually held by the Pin
        /// </summary>
        public char[] PinVal
        {
            get
            {
                return this.pinVal;
            }
            set
            {
                this.pinVal = value;
            }
        }

        /// <summary>
        /// Returns whether the Pin is an input "in" or and output "out"
        /// </summary>
        public PinPolarity Polarity
        {
            get
            {
                return this.polarity;
            }
        }

        /// <summary>
        /// Returns or sets the binary value that the Pin should have (primarily for outputs)
        /// </summary>
        public char[] Expected
        {
            get
            {
                return this.expected;
            }
            set
            {
                this.expected = value;
            }
        }

        /// <summary>
        /// Returns the number of bits that the Pin holds
        /// </summary>
        public int bussize
        {
            get
            {
                return this.busSize;
            }
        }

        public Sketch.Shape Shape
        {
            get
            {
                return this.shape;
            }
        }

        #endregion

        /// <summary>
        /// Returns the value held by the pins as a string
        /// </summary>
        /// <returns>A string containing the pin's stored value</returns>
        public String val2str()
        {
            String ret = new String(pinVal);
            return ret;
        }

        /// <summary>
        /// Returns the string's expected value as a string
        /// </summary>
        /// <returns>A string containing the pin's stored value</returns>
        public String expected2str()
        {
            String ret = new String(expected);
            return ret;
        }

        /// <summary>
        /// Checks to see if a pin is a member of a list of pins
        /// </summary>
        /// <param name="pins">List of pins to check for membership in</param>
        /// <returns>Boolean indicating whether a match was found</returns>
        public Boolean isMember(List<Pin> pins)
        {
            //Goes through each pin in the list
            foreach (Pin instance in pins)
            {
                if (instance.Equals(this))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the pin's index within a list of pins
        /// </summary>
        /// <param name="pins">List of pins to search for current pin</param>
        /// <returns>Int containing pin index</returns>
        public int findIndex(List<Pin> pins)
        {
            int index = 0;

            foreach (Pin instance in pins)
            {
                if (instance.PinName.Equals(this.pinNames[0]) && instance.Polarity.Equals(this.polarity) && (instance.bussize == this.bussize))
                    return index;
                index++;
            }

            return index;
        }

        /// <summary>
        /// Checks pin equality where equality is defined as having the same name, bus size and polarity
        /// </summary>
        /// <param name="pin">Pin to be compared</param>
        /// <returns>Boolean indicating whether the pins are equal</returns>
        public Boolean Equals(Pin pin)
        {
            if (this.pinNames[0] == pin.pinNames[0] && this.bussize == pin.bussize && this.polarity.Equals(pin.polarity))
                return true;
            else
                return false;
        }
    }
}
