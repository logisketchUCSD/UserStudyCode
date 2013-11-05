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
//using System.IO;
using System.Windows.Forms;

namespace uiWorkPath
{
    public class Pin
    {
        String inOut;
        String pinName;
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
        public Pin(String inOut, String pinName, char[] pinVal)
        {
            this.inOut = inOut;
            this.pinName = pinName;
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
        public Pin(String inOut, String pinName, char[] pinVal, char[] expected)
        {
            this.inOut = inOut;
            this.pinName = pinName;
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
        public Pin(String inOut, String pinName, int busSize)
        {
            this.inOut = inOut;
            this.pinName = pinName;
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
        public Pin(String inOut, String pinName, Sketch.Shape shape)
        {
            this.inOut = inOut;
            this.pinName = pinName;
            this.busSize = 1;
            this.expected = new char[busSize];
            this.pinVal = new char[busSize];
            this.shape = shape;
        }

        #endregion

        #region Getters

        /// <summary>
        /// Returns the name of the Pin
        /// </summary>
        public String PinName
        {
            get
            {
                return this.pinName;
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
        public String InOut
        {
            get
            {
                return this.inOut;
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
                
    }
}
