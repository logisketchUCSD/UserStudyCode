/**
 * File: DomainInfo.cs
 * 
 * Notes: Domain information for the labeler
 * 
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, Max Pfleuger, Christine Alvarado
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Labeler
{	
	/// <summary>
	/// Create a new container to hold domain information for the labeler
	/// </summary>
	public class DomainInfo
	{
		#region INTERNALS

		/// <summary>
		/// Hashtable containing string labels to integer values
		/// </summary>
		private Dictionary<string,int> labelsToNums;
		
		/// <summary>
		/// Hashtable containing integer values to string labels
		/// </summary>
		private Dictionary<int,string> numsToLabels;
		
		/// <summary>
		/// Hashtable containing string labels to colors
		/// </summary>
		private Dictionary<string,Color> labelsToColors;
		
		/// <summary>
		/// Hashtable containing colors to string labels
		/// </summary>
		private Dictionary<int,Color> numsToColors;
		
		/// <summary>
		/// Information about the Domain
		/// </summary>
		private Dictionary<string, string> info;

		#endregion

		#region CONSTRUCTOR

		/// <summary>
		/// Constructor. Initializes the Hashtables.
		/// </summary>
		public DomainInfo()
		{
			labelsToNums = new Dictionary<string, int>();
			numsToLabels = new Dictionary<int, string>();

			labelsToColors = new Dictionary<string, Color>();
			numsToColors   = new Dictionary<int, Color>();

			info = new Dictionary<string, string>();
		}

		#endregion

		#region GETTERS & ADDERS

		/// <summary>
		/// Adds a label with the corresponding name, int value, and color.
		/// </summary>
		/// <param name="num">Integer value corresponding to the Label</param>
		/// <param name="name">Name of the label</param>
		/// <param name="color">Color of the label</param>
		public void addLabel(int num, string name, Color color)
		{
			labelsToNums.Add(name, num);
			numsToLabels.Add(num, name);

			labelsToColors.Add(name, color);
			numsToColors.Add(num, color);
		}

		/// <summary>
		/// Gets a label by its corresponding integer representation.
		/// </summary>
		/// <param name="num">Integer value of the label</param>
		/// <returns>The name of the label</returns>
		public string getLabel(int num)
		{
			return numsToLabels[num];
		}


		/// <summary>
		/// Gets an ArrayList of all the labels in the Domain.
		/// </summary>
		/// <returns>All of the labels in the Domain</returns>
		public List<string> getLabels()
		{
			return new List<string>(labelsToNums.Keys);
		}


		/// <summary>
		/// Gets a label's integer representation.
		/// </summary>
		/// <param name="label">The name of the label</param>
		/// <returns>The label's integer value</returns>
		public int getLabelNumber(string label)
		{
			return labelsToNums[label];
		}


		/// <summary>
		/// Gets the System.Drawing.Color of a given label.
		/// </summary>
		/// <param name="label">The name of the label</param>
		/// <returns>The color of the label</returns>
		public Color getColor(string label)
		{
			if(labelsToColors.ContainsKey(label))
				return labelsToColors[label];
			else
				return Color.Black;
		}


		/// <summary>
		/// Adds information about the Domain to the corresponding hashtable.
		/// </summary>
		/// <param name="key">Information's key</param>
		/// <param name="val">Information's value</param>
		public void addInfo(string key, string val)
		{
			info[key] = val;
		}


		/// <summary>
		/// Gets information about the Domain, given a valid key.
		/// </summary>
		/// <param name="key">Information's key</param>
		/// <returns>Information's value</returns>
		public string getInfo(string key)
		{
			return info[key];
		}

		#endregion
	}
}
