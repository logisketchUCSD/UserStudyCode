using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public delegate bool FilterDelegate<T>(T value);

    /// <summary>
    /// Contains static methods for manipulating lists, dictionaries, etc.
    /// </summary>
    public abstract class Utils
    {

        /// <summary>
        /// Filter the entries of a dictionary by keeping only certain keys.
        /// </summary>
        /// <typeparam name="K">the type of dictionary keys</typeparam>
        /// <typeparam name="V">the type of dictionary values</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="filter">returns true for a given key if that entry is to be included in the result</param>
        /// <returns></returns>
        public static Dictionary<K,V> filterKeys<K,V>(Dictionary<K,V> dictionary, FilterDelegate<K> filter)
        {
            Dictionary<K,V> result = new Dictionary<K,V>();
            foreach (KeyValuePair<K, V> pair in dictionary)
                if (filter(pair.Key))
                    result.Add(pair.Key, pair.Value);
            return result;
        }

        /// <summary>
        /// Filter the entries of a dictionary by keeping only certain keys.
        /// </summary>
        /// <typeparam name="K">the type of dictionary keys</typeparam>
        /// <typeparam name="V">the type of dictionary values</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="filter">returns true for a given key if that entry is to be included in the result</param>
        /// <returns></returns>
        public static List<T> filter<T>(IEnumerable<T> list, FilterDelegate<T> filter)
        {
            List<T> result = new List<T>();
            foreach (T value in list)
                if (filter(value))
                    result.Add(value);
            return result;
        }

        /// <summary>
        /// Create a list contining one element.
        /// </summary>
        /// <typeparam name="T">the type of element</typeparam>
        /// <param name="value">the value to add to the list</param>
        /// <returns>a list containing only the given element</returns>
        public static List<T> singleEntryList<T>(T value)
        {
            List<T> result = new List<T>();
            result.Add(value);
            return result;
        }

        /// <summary>
        /// Determine whether a list contains duplicate entries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool containsDuplicates<T>(IEnumerable<T> list)
        {
            HashSet<T> entries = new HashSet<T>();
            foreach (T value in list)
            {
                if (entries.Contains(value))
                    return true;
                else
                    entries.Add(value);
            }
            return false;
        }

    }
}