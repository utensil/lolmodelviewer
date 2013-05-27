/*
 * RAF Library - Plus (RAFlib-Plus)
 * Copyright 2012 Adrian Astley
 *
 *This file is part of RAFlib-Plus.
 *
 *RAFlib-Plus is free software: you can redistribute it and/or modify
 *it under the terms of the GNU General Public License as published by
 *the Free Software Foundation, either version 3 of the License, or
 *(at your option) any later version.

 *RAFlib-Plus is distributed in the hope that it will be useful,
 *but WITHOUT ANY WARRANTY; without even the implied warranty of
 *MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *GNU General Public License for more details.

 *You should have received a copy of the GNU General Public License
 *along with RAFlib-Plus.  If not, see <http://www.gnu.org/licenses/>
*/

/*
 * This class is designed to help extract and 
 * inject files from the League of Legends game files.
 * http://www.leagueoflegends.com 
 * 
 * This class is a modification of the original 
 * RAFlib generously created and provided by ItzWarty
 * and found here http://code.google.com/p/raf-manager/source/browse/#svn%2FProjects%2FRAFLib
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using ItzWarty;

namespace RAFlibPlus
{
    /// <summary>
    /// Manages the handling of hashes for RAF Strings, which is calculated in an unknown
    /// matter at the moment.
    /// </summary>
    public static class RAFHashManager
    {
        /// <summary>
        /// Get the hash of a entry file name
        /// </summary>
        /// <param name="s">Entry file name</param>
        /// <returns></returns>
        public static UInt32 GetHash(string s)
        {
            //if (hashes == null) Init();
            //Console.WriteLine("Calc hash of: " + s);
            /* Ported from documented code in RAF Documentation:
             * 
	         *      const char* pStr = 0;
	         *      unsigned long hash = 0;
	         *      unsigned long temp = 0;
             *
	         *      for(pStr = pName; *pStr; ++pStr)
	         *      {
		     *          hash = (hash << 4) + tolower(*pStr);
		     *          if (0 != (temp = hash & 0xf0000000)) 
		     *          {
			 *              hash = hash ^ (temp >> 24);
			 *              hash = hash ^ temp;
		     *          }
	         *      }
	         *      return hash;
             */
            UInt32 hash = 0;
            UInt32 temp = 0;
            for (int i = 0; i < s.Length; i++)
            {
                hash = (hash << 4) + s.ToLower()[i];
                if (0 != (temp = (hash & 0xF0000000)))
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }
            //Console.WriteLine("!");

            //Console.WriteLine("Hash expected: " + hashes[s]);
            //Console.WriteLine("Hash Calculated: " + hash);
            return hash;
        }
    }
}
