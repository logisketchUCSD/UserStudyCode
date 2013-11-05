using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace GenerateSubsets
{
    public static class GenerateSubsets 
    {
        /// <summary>
        /// Function delegate that specifies the function to call after each subset has been generated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="len"></param>
        public delegate void SubsetDelegate<T>(T[] array, int len);

        /// <summary>
        /// Generates all subsets of T[] array.  With each 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="handler"></param>
        /// <returns>Number of subsets</returns>
        public static int AllSubsets<T>(T[] array, SubsetDelegate<T> handler)
        {
            int i, len = array.Length;
            bool[] included = new bool[len];
            int[] index = new int[len];
            int[] changeIndex = new int[len];

            /*
            included.Initialize(); //set all false
            index.Initialize(); //set all 0
            changeIndex.Initialize(); //set all 0
            */

            T[] subarray = new T[len]; //allocate the full amount
            int subsetLength = 0;
            int numSubsets = 1 << len; //2 ^ len
            int toChange;

            int ch;
            int ich;
            // TODO: Possible off by one error.  1 to numSubsets is only (2^n) - 1 subsets, not 2^n
            for (i = 1; i < numSubsets; ++i)  
            {
                toChange = change(i);
                if (included[toChange]) //remove it
                {
                    included[toChange] = false;

                    --subsetLength;

                    ch = changeIndex[subsetLength];
                    ich = index[toChange];

                    subarray[ich] = subarray[subsetLength];
                    changeIndex[ich] = ch;
                    index[ch] = ich;
                }
                else //add it
                {
                    //Index of added substroke is the last available spot
                    index[toChange] = subsetLength;

                    //Where it came from
                    changeIndex[subsetLength] = toChange;

                    //Set the last spot equal to the substroke
                    subarray[subsetLength] = array[toChange];

                    //Increase size
                    ++subsetLength;

                    included[toChange] = true;
                }

                //Call the designated function
                handler(subarray, subsetLength);                
            }

            return numSubsets;
        }

        /// <summary>
        /// Computes the number of ones in the bitrepresentation of x. http://aggregate.org/MAGIC/
        /// </summary>
        /// <param name="x">Value to find number of ones</param>
        /// <returns>Number of ones it bit representation of x</returns>
        private static int ones32(int x)
        {
            
            // 32-bit recursive reduction using SWAR...
	       //but first step is mapping 2-bit values
	       //into sum of 2 1-bit values in sneaky way
	        
            
            
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            x += (x >> 16);
            return (x & 0x0000003f);
            

            //http://infolab.stanford.edu/~manku/bitcount/bitcount.html

     
           //  n = COUNT(n, 0) ;
            // n = COUNT(n, 1) ;
            // n = COUNT(n, 2) ;
            // n = COUNT(n, 3) ;
            // n = COUNT(n, 4) ;
             // n = COUNT(n, 5) ;    for 64-bit integers 
        //     return n ;
           // http://www.scit.wlv.ac.uk/cbook/chap4.unsigned.integer.html
            
            //uint uCount = x
        //- ((x >> 1) & 0xDB6DB6DB)
        //- ((x >> 2) & 0x49249249); 
         //return (int)(
          // ((uCount + (uCount >> 3))
            //& 0xC71C71C7) % 63);
        }

        private static int BitCount(uint u)                         
        {
            /*
            uint uCount = u - ((u >> 1) & 0o33333333333) - ((u >> 2) & 0o11111111111);
            return ((uCount + (uCount >> 3)) & 0o30707070707) % 63;
            */

            uint uCount = u - ((u >> 1) & 0xDB6DB6DB) - ((u >> 2) & 0x49249249);
            return (int)(((uCount + (uCount >> 3)) & 0xC71C71C7) % 63);
        }

        #region FUNCTIONS

        /*
        private static int COUNT(int x, int c)
        {
            return ((x) & MASK(c)) + (((x) >> (TWO(c))) & MASK(c));
        }
        private static int MASK(int c)
        {
            return (unchecked((int)(unchecked((uint)(-1)))) / (TWO(TWO(c)) + 1));
        }
        private static int TWO(int c)
        {
            return (0x1 << (c));
        }

        /// <summary>
        /// Computes the log base 2 of x. http://aggregate.org/MAGIC/
        /// </summary>
        /// <param name="x">Value to take log base 2 of.</param>
        /// <returns>The log base 2 of x.</returns>
        private static int log2(int x)
        {
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return ones32(x >> 1);//(uint)(x >> 1));
        }

        /// <summary>
        /// Computes the log base 2 of x. x must be an exact power of 2.(Devin Smith)
        /// </summary>
        /// <param name="x">x = 2 ^ n</param>
        /// <returns>For x = 2 ^ n, returns n</returns>
        private static int log2_exact(int x)
        {
            //Since there is only 1 bit, it is an exact power of 2
            //We fill in the remaining 0's to the right with 1's
            //and then count the number of 1's

            return ones32(x - 1); 
            //return ones32(x - 1);
        }

        /// <summary>
        /// G = B XOR (SHR(B)) [ http://en.wikipedia.org/wiki/Gray_code ]
        /// 
        /// Gets the gray code corresponding to index x
        /// 
        /// Useful properties of gray codes is that every successive code is at most a distance 1 away from the previous code
        /// </summary>
        /// <param name="x">Index of gray code</param>
        /// <returns>Gray code at index x</returns>
        private static int gray(int x)
        {
            return x ^ (x >> 1);
        }*/

        #endregion

        /// <summary>
        /// Gets the index of the bit that changed at iter. This is due to the fact that successive gray codes only change by 1 bit.
        /// This is useful for subset generation
        /// </summary>
        /// <param name="iter">Current index of subset to generate</param>
        /// <returns>Index of item to add / remove from the subset</returns>
        private static int change(int iter)
        {
            //return log2_exact(gray(iter) ^ gray(iter - 1));

            /*
            int gray1 = iter ^ (iter >> 1); // = gray(iter);
            int gray2 = (iter - 1) ^ ((iter - 1) >> 1); // = gray(iter - 1);
            int diff = gray1 ^ gray2;
            int toOnes = diff - 1;
            return ones32(toOnes);
            */

            return ones32((iter ^ (iter >> 1) ^ (iter - 1) ^ ((iter - 1) >> 1)) - 1);
            //return BitCount((uint)(iter ^ (iter >> 1) ^ (iter - 1) ^ ((iter - 1) >> 1)) - 1);
        }
    }
}
