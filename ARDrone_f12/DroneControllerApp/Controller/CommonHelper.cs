#region Copyright Notice

//Copyright © 2007-2011, PARROT SA, all rights reserved. 

//DISCLAIMER 
//The APIs is provided by PARROT and contributors "AS IS" and any express or implied warranties, including, but not limited to, the implied warranties of merchantability 
//and fitness for a particular purpose are disclaimed. In no event shall PARROT and contributors be liable for any direct, indirect, incidental, special, exemplary, or 
//consequential damages (including, but not limited to, procurement of substitute goods or services; loss of use, data, or profits; or business interruption) however 
//caused and on any theory of liability, whether in contract, strict liability, or tort (including negligence or otherwise) arising in any way out of the use of this 
//software, even if advised of the possibility of such damage. 

//Author            : Wilke Jansoone
//Email             : wilke.jansoone@digitude.net
//Publishing date   : 28/11/2010 

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions
//are met:
//    - Redistributions of source code must retain the above copyright notice, this list of conditions, the disclaimer and the original author of the source code.
//    - Neither the name of the PixVillage Team, nor the names of its contributors may be used to endorse or promote products derived from this software without 
//      specific prior written permission.

#endregion

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DroneController
{
    /// <summary>
    /// This class contains some miscellaneous helper methods.
    /// </summary>
    internal static class CommonHelper
    {
        /// <summary>
        /// Displays the bit sequence of an integer value.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>A string with the bit pattern representing the integer.</returns>
        /// <example>The integer value '751' would result in '00000000000000000000001011101111', can be verified with calculator.</example>
        /// <remarks>This is a method used frequently during testing.</remarks>
        internal static string DisplayBitSequence(int value)
        {
            StringBuilder buffer = new StringBuilder();

            byte[] byteBuffer = BitConverter.GetBytes(value);

            Array.Reverse(byteBuffer);

            foreach (byte byteValue in byteBuffer)
            {
                for (int count = 128; count > 0; count /= 2)
                {
                    if ((byteValue & count) != 0) buffer.Append("1");
                    if ((byteValue & count) == 0) buffer.Append("0");
                }

                buffer.Append(" ");
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Sets the bit in an integer value at the requested position.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <param name="position">The position at which to set the bit.</param>
        /// <returns>The integer value with the bit set.</returns>
        internal static int SetBitValue(int value, int position)
        {
            return value |= (1 << position);
        }

        /// <summary>
        /// Clears the bit in an integer value at the requested position.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <param name="position">The position at which to clear the bit.</param>
        /// <returns>The integer value with the bit cleared.</returns>
        internal static int ClearBitValue(int value, int position)
        {
            return value &= ~(1 << position);
        }

        /// <summary>
        /// Flips the bit in an integer value at the requested position.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <param name="position">The position at which to flip the bit.</param>
        /// <returns>The integer value with the bit flipped.</returns>
        internal static int FlipBitValue(int value, int position)
        {
            return value ^= (1 << position);
        }

        /// <summary>
        /// Converts the string to a byte array containing the ASCII values of each char.
        /// </summary>
        /// <param name="ssid">The ssid.</param>
        /// <returns></returns>
        internal static byte[] ConvertStringToByteArray(string ssid)
        {
            int numChars = ssid.Length;
            byte[] bytes = new byte[32];

            for (int index = 0; index < numChars; index++)
            {
                bytes[index] = (byte)ssid[index];
            }

            return bytes;

            //return Encoding.ASCII.GetBytes(ssid);
        }

        /// <summary>
        /// Converts the byte array to a string.
        /// </summary>
        /// <param name="buffer">The byte sequence.</param>
        /// <returns>A string representing the byte array.</returns>
        internal static string ConvertByteArrayToString(byte[] buffer)
        {
            return ConvertByteArrayToString(buffer, buffer.Length);
        }

        /// <summary>
        /// Converts the byte array to a string.
        /// </summary>
        /// <param name="buffer">The byte sequence.</param>
        /// <param name="length">The byte count to use for conversion.</param>
        /// <returns>A string representing the byte array.</returns>
        internal static string ConvertByteArrayToString(byte[] buffer, int length)
        {
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

/*        /// <summary>
        /// Generates a new numbered filename based on a specific pattern.
        /// </summary>
        /// <param name="path">The path where to look for identically named files.</param>
        /// <param name="fileNamePattern">The file name pattern.</param>
        /// <param name="extension">The extension.</param>
        /// <returns>A new unique filename.</returns>
        internal static string GenerateNewNumberedFileName(string path, string fileNamePattern, string extension)
        {
            List<int> usedIndexes = new List<int>();

            Regex regex = new Regex(fileNamePattern + "[0-9*]*." + extension, RegexOptions.Compiled);

            List<string> fileNames = Directory.GetFiles(path, fileNamePattern + "*." + extension).ToList<String>();

            foreach (var fileName in fileNames)
            {
                Match match = regex.Match(fileName);

                if (match.Captures.Count > 0)
                {
                    string indexValue = match.Captures[0].Value.Replace(fileNamePattern, "").Replace("." + extension, "");

                    usedIndexes.Add(Int32.Parse(indexValue));
                }
            }

            int currentIndex = 0;

            while (true)
            {
                if (!usedIndexes.Contains(++currentIndex))
                {
                    break;
                }
            }

            if (!path.EndsWith(@"\"))
            {
                path += @"\";
            }

            return String.Format("{0}{1}{2}.{3}", path, fileNamePattern, currentIndex, extension);
        }
 * */
    }
}
