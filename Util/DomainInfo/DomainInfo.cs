/**
 * File: DomainInfo.cs
 * 
 * 
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, Max Pfleuger, Christine Alvarado
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 */

using Domain;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DomainInfo
{
    /// <summary>
    /// Create a new container to hold domain information for the labeler
    /// </summary>
    public class DomainInfo
    {
        #region INTERNALS

        private Dictionary<ShapeType, Color> typesToColors;

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor. Initializes the Hashtables.
        /// </summary>
        public DomainInfo()
        {
            typesToColors = new Dictionary<ShapeType, Color>();
        }

        #endregion

        #region GETTERS & ADDERS

        /// <summary>
        /// Adds a label with the corresponding type and color.
        /// </summary>
        /// <param name="name">Name of the label</param>
        /// <param name="color">Color of the label</param>
        public void AddLabel(ShapeType type, Color color)
        {
            typesToColors.Add(type, color);
        }


        /// <summary>
        /// Gets an ArrayList of all the ShapeTypes in the Domain.
        /// </summary>
        /// <returns>All of the labels in the Domain</returns>
        public List<ShapeType> GetTypes()
        {
            List<ShapeType> types = new List<ShapeType>();

            // Add the labels according to preference
            foreach (ShapeType type in typesToColors.Keys)
            {
                types.Add(type);
            }

            return types;
        }


        /// <summary>
        /// Gets the System.Windows.Media.Color of a given label.
        /// </summary>
        /// <param name="label">The name of the label</param>
        /// <returns>The color of the label</returns>
        public Color GetColor(ShapeType label)
        {
            if (typesToColors.ContainsKey(label))
                return typesToColors[label];
            else
                return Colors.Black;
        }


        public Color GetColor(string label)
        {
            return GetColor(LogicDomain.getType(label));
        }

        #endregion

        /// <summary>
        /// Loads domain info from the file in the domain info folder.
        /// </summary>
        /// <returns></returns>
        public static DomainInfo LoadDomainInfo()
        {
            string filepath = AppDomain.CurrentDomain.BaseDirectory + @"CircuitColorDomain.txt";
            return LoadDomainInfo(filepath);
        }

        /// <summary>
        /// Loads the given domain file.  
        /// <seealso cref="Labeler.MainForm.LoadDomain"/>
        /// </summary>
        /// <param name="domainFilePath">the file path to load</param>
        /// <returns>the DomainInfo loaded</returns>
        public static DomainInfo LoadDomainInfo(string domainFilePath)
        {
            // Check to see if there is a domain file to load for this feedback mechanism
            if (domainFilePath == null)
                return null;

            // Make sure file exists
            if (!System.IO.File.Exists(domainFilePath))
                return null;

            // Load domain file
            System.IO.StreamReader sr = new System.IO.StreamReader(domainFilePath);

            DomainInfo domain = new DomainInfo();
            string line = sr.ReadLine();
            string[] words = line.Split(null);

            // The first two lines are useless
            line = sr.ReadLine();
            line = sr.ReadLine();

            // Then the rest are labels
            while (line != null && line != "")
            {
                words = line.Split(null);

                string label = words[0];
                string color = words[1];

                domain.AddLabel(LogicDomain.getType(label), (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
                line = sr.ReadLine();
            }

            sr.Close();

            return domain;
        }
    }
}
